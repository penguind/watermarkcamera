//  Copyright 2011年 Renren Inc. All rights reserved.
//  - Powered by Team Pegasus. -

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using Microsoft.Phone.Net.NetworkInformation;

namespace RenrenSDKLibrary
{
    /// <summary>
    /// When request is end，proceed to this delegate method。
    /// </summary>
    /// <param name="responseData"></param>
    public delegate void DownloadStringCompletedMethod(string responseData);

    /// <summary>
    /// Provides common methods for sending data to and receiving data from an HTTP POST web request.
    /// </summary>
    public class RenrenWebRequest
    {
        #region Members
        /// <summary>
        /// Content-header boundary
        /// </summary>
        string boundary = String.Empty;

        #endregion

        #region Events

        /// <summary>
        /// Event handler for DownloadStringCompleted event.
        /// </summary>
        /// <param name="sender">Object firing the event.</param>
        /// <param name="e">Argument holding the data downloaded.</param>
        public delegate void DownloadStringCompletedHandler(object sender, DownloadStringCompletedEventArgs e);
        /// <summary>
        /// Occurs when an asynchronous resource-download operation is completed.
        /// </summary>
        public event DownloadStringCompletedHandler DownloadStringCompleted;

        #endregion

        #region public methods without files
        /// <summary>
        /// Downloads the resource at the specified Uri as a string.
        /// </summary>
        /// <param name="address">The location of the resource to be downloaded.</param>
        public void DownloadStringAsync(string address, List<APIParameter> paras, HttpMethod method=HttpMethod.POST)
        {
            if (paras == null || paras.Count == 0) throw new ArgumentNullException("The paras required!");

            HttpWebRequest request;
            try
            {
                // POST
                if (method == HttpMethod.POST)
                {
                    RequestState state = new RequestState();
                    request = (HttpWebRequest)WebRequest.Create(address);                  
                    request.Method = HttpMethod.POST.ToString();
                    request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    //平台统计用
                    request.UserAgent = "Renren Windows Phone SDK v2.0 (windows phone; windows phone 7.1)";
                    state.request = request;
                    state.paras = paras;

                    request.BeginGetRequestStream(new AsyncCallback(RequestReady), state);
                }
                else if (method == HttpMethod.GET)
                {
                    address = ApiHelper.AddParametersToURL(address, paras);
                    request = (HttpWebRequest)WebRequest.Create(address);
                    request.Method = HttpMethod.GET.ToString();
                    request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                    request.BeginGetResponse(new AsyncCallback(ResponseReady), request);
                }
            }
            catch
            {
                if (DownloadStringCompleted != null)
                {

                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        DownloadStringCompleted(this, new DownloadStringCompletedEventArgs(new Exception("Error creating HTTP web request.")));
                    });

                }
            }
        }
        // request ready 
        void RequestReady(IAsyncResult asyncResult)
        {
            RequestState state = asyncResult.AsyncState as RequestState;
            HttpWebRequest request = state.request;
            List<APIParameter> paras = state.paras;
            string querystring = ApiHelper.GetQueryFromParas(paras);

            using (Stream stream = request.EndGetRequestStream(asyncResult))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(querystring);
                    writer.Flush();
                }
            }
            ApiHelper.WriteLog(paras);
            request.BeginGetResponse(new AsyncCallback(ResponseReady), request);
        }
        #endregion

        #region Public methods with files

        /// <summary>
        /// Downloads the resource at the specified Uri as a string.
        /// </summary>
        /// <param name="address">The location of the resource to be downloaded.</param>
        public void DownloadStringAsyncWithFile(string address,List<APIParameter> paras,List<APIParameter> files)
        {
            if (paras == null || paras.Count == 0) throw new ArgumentNullException("The paras required!");
            if (files == null || files.Count == 0) throw new ArgumentNullException("The files required!");

            HttpWebRequest request;
            RequestState state = new RequestState();      
            try
            {
                request = (HttpWebRequest)WebRequest.Create(address);

                boundary = DateTime.Now.Ticks.ToString("X");
                request.Method = HttpMethod.POST.ToString();
                request.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;

                state.request = request;
                state.paras = paras;
                state.files = files;

                request.BeginGetRequestStream(new AsyncCallback(RequestReadyWithFile), state);
            }
            catch
            {
                if (DownloadStringCompleted != null)
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                     {
                          DownloadStringCompleted(this, new DownloadStringCompletedEventArgs(new Exception("Error creating HTTP web request.")));
                     });
                }
            }
        }
        void RequestReadyWithFile(IAsyncResult asyncResult)
        {
            RequestState state = asyncResult.AsyncState as RequestState;
            HttpWebRequest request = state.request;
            
            byte[] beginBoundary = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundary = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            List<APIParameter> paras = state.paras;
            List<APIParameter> files = state.files;
            string paraTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            string fileTemplate = "Content-Disposition: form-data; name=\"{0}\";filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            
            using (Stream stream = request.EndGetRequestStream(asyncResult))
            {
                foreach (APIParameter para in paras)
                {
                    string value = para.Value;
                    if(para.Value == null)
                        value = "";
                    byte[] bpara = Encoding.UTF8.GetBytes(String.Format(paraTemplate, para.Name, value));
                    stream.Write(beginBoundary, 0, beginBoundary.Length);
                    stream.Write(bpara, 0, bpara.Length);
                }
                foreach (APIParameter file in files)
                {
                    byte[] bfile = Encoding.UTF8.GetBytes(String.Format(fileTemplate, file.Name, file.Value, ApiHelper.GetContentType(file.Value)));
                    stream.Write(beginBoundary, 0, beginBoundary.Length);
                    stream.Write(bfile, 0, bfile.Length);
                    // 写入文件内容
                    byte[] content = ApiHelper.GetFileContent(file.Value);
                    stream.Write(content,0,content.Length);
                    stream.Write(endBoundary, 0, endBoundary.Length);
                }
            }
            ApiHelper.WriteLog(paras);
            ApiHelper.WriteLog(files);
            request.BeginGetResponse(new AsyncCallback(ResponseReady), request);
        }
        #endregion

        #region Response Ready
        void ResponseReady(IAsyncResult asyncResult)
        {
            HttpWebRequest request = asyncResult.AsyncState as HttpWebRequest;
            HttpWebResponse response = null;

            try
            {
                response = (HttpWebResponse)request.EndGetResponse(asyncResult);

                string result = string.Empty;
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(responseStream))
                    {
                        result = reader.ReadToEnd();
                    }
                }

                ApiHelper.WriteLog(result);
                if (DownloadStringCompleted != null)
                {
                     System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                     {
                          DownloadStringCompleted(this, new DownloadStringCompletedEventArgs(result));
                     });
                }
            }
            catch
            {
                if (DownloadStringCompleted != null)
                {
                    string errorMsg;
                    if (IsNetWorkConnected())
                    {
                        errorMsg = "No response.";
                    }
                    else
                    {
                        errorMsg = "No network.";
                    }
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        DownloadStringCompleted(this, new DownloadStringCompletedEventArgs(new Exception(errorMsg)));
                    });
                }
            }
        }
        #endregion

        private bool IsNetWorkConnected()
        {
            bool networkIsAvailable = NetworkInterface.GetIsNetworkAvailable();
            if (!networkIsAvailable)
                return false;

            NetworkInterfaceType currentNetworkType = NetworkInterface.NetworkInterfaceType;
            if (currentNetworkType == NetworkInterfaceType.None)
            {
                return false;
            }
            
            return true;
        }
    }

    /// <summary>
    /// Request state 
    /// </summary>
    class RequestState
    {
        public HttpWebRequest request { get; set; }
        public List<APIParameter> paras { get; set; }
        public List<APIParameter> files { get; set; }
    }

}

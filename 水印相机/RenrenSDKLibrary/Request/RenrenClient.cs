//  Copyright 2011年 Renren Inc. All rights reserved.
//  - Powered by Team Pegasus. -

using System;
using System.Net;
using System.Collections.Generic;

namespace RenrenSDKLibrary
{
    public class RenrenClient
    {
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
        

        #region 异步方法可以上传文件
        /// <summary>
        /// 执行接口方法
        /// 传入参数列表，比如接口名字，format形式等
        /// </summary>
        /// <param name="paras">参数列表</param>
        /// <returns>服务器响应数据</returns>
        public void CallMethod(List<APIParameter> paras, List<APIParameter> files)
        {
            string accessToken = RenrenSDK.RenrenInfo.tokenInfo.access_token;
            string callID = DateTime.Now.Millisecond.ToString();

            if (paras == null || paras.Count == 0) throw new ArgumentNullException("paras required");
            if (files == null || files.Count == 0) throw new ArgumentNullException("files required");

            paras.Add(new APIParameter("call_id", DateTime.Now.Millisecond.ToString()));
            paras.Add(new APIParameter("access_token", accessToken));
            paras.Add(new APIParameter("v", "1.0"));
            paras.Add(new APIParameter("format", "JSON"));
            string strSig = ApiHelper.CalSig(paras);
            if (strSig == "")
                return;
            paras.Add(new APIParameter("sig", strSig));
            RenrenWebRequest client = new RenrenWebRequest();
            client.DownloadStringCompleted += new RenrenWebRequest.DownloadStringCompletedHandler(client_DownloadStringCompleted);
            client.DownloadStringAsyncWithFile(ConstantValue.RequestUri.ToString(), paras, files);
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (DownloadStringCompleted != null)
                {
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        DownloadStringCompleted(this, new DownloadStringCompletedEventArgs(e.Result));
                    });
                }
            }
            else
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    DownloadStringCompleted(this, new DownloadStringCompletedEventArgs(new Exception("error")));
                });
            }
        }
        /// <summary>
        /// 执行接口的方法，之所以在这里加上这个format，人人网返回的数据形式默认的不是json而是xml。
        /// </summary>
        /// <param name="methodName">传入要执行的接口的名称</param>
        /// <returns>服务器响应数据</returns>
        public void CallMethod(string methodName, string filename)
        {
            if (methodName == "")
                return;
            List<APIParameter> paras = new List<APIParameter>() { 
                new APIParameter("method",methodName)
            };
            List<APIParameter> files = new List<APIParameter>() { 
                new APIParameter("upload",filename)
            };
            CallMethod(paras, files);
        }
        #endregion
    }
}

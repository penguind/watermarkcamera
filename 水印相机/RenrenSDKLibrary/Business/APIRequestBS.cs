using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace RenrenSDKLibrary
{
    //API请求的handler
    public delegate void APIRequestCompletedHandler(object sender, APIRequestCompletedEventArgs e);

    internal class APIRequestBS : BusinessBase
    {
        #region Members
        APIRequest apiRequest;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when an asynchronous resource-download operation is completed.
        /// </summary>
        public event APIRequestCompletedHandler APIRequestCompleted;
        #endregion

        /// <summary>
        /// 构造
        /// </summary>
        public APIRequestBS()
        {
            apiRequest = new APIRequest();
        }

        #region Override
        /// <summary>
        /// success
        /// </summary>
        /// <param name="resultString">msg</param>
        public override void RequestComplete(string resultString)
        {
            if (APIRequestCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    APIRequestCompleted(this, new APIRequestCompletedEventArgs(resultString));
                });
            }
        }

        /// <summary>
        /// error
        /// </summary>
        /// <param name="errorMsg">msg</param>
        public override void RequestError(string errorMsg)
        {
            if (APIRequestCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    APIRequestCompleted(this, new APIRequestCompletedEventArgs(new Exception(errorMsg)));
                });
            }
        }
        #endregion

        public void ClearAPIRequestEvent()
        {
            APIRequestCompleted = null;
        }

        /// <summary>
        /// 获取API请求结果
        /// </summary>
        public void GetAPIRequestData(List<APIParameter> param)
        {
            apiRequest.RequestAPIInterface(param, DownloadStringCompleted);
        }
    }
}
//  Copyright 2011年 Renren Inc. All rights reserved.
//  - Powered by Team Pegasus. -

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
using System.IO;
using System.Text;

namespace RenrenSDKLibrary
{
    abstract class BusinessBase
    {
        /// <summary>
        /// 处理请求成功事件
        /// </summary>
        public abstract void RequestComplete(string resultString);

        /// <summary>
        /// 处理请求失败事件
        /// </summary>
        public abstract void RequestError(string errorMsg);

        /// <summary>
        /// 网络事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                RequestError(e.Error.Message);
                return;
            }
            if (ApiHelper.ContainsError(e.Result))
            {
                ErrorMessage error = new ErrorMessage();
                try
                {
                    error = (ErrorMessage)JsonUtility.DeserializeObj(
                            new MemoryStream(Encoding.UTF8.GetBytes(e.Result)), 
                            typeof(ErrorMessage));
                    RequestError(error.error_msg);
                }
                catch
                {
                    RequestError("encoding error");
                }
            }
            else
            {
                RequestComplete(e.Result);
            }
        }
    }
}

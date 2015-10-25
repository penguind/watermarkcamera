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
    internal class CreateAlbumBS : BusinessBase
    {
        #region Members
        CreateAlbumRequest createAlbumRequest;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when an asynchronous resource-download operation is completed.
        /// </summary>
        public event RenrenSDKLibrary.CreateAlbumRequest.CreateAlbumCompletedHandler CreateAlbumCompleted;
        #endregion

        /// <summary>
        /// 构造
        /// </summary>
        public CreateAlbumBS()
        {
            createAlbumRequest = new CreateAlbumRequest();
        }

        /// <summary>
        /// 创建相册
        /// </summary>
        public void CreateAlbum(string name, string caption, string location)
        {
            if (ConstantValue.ApiKey == null)
            {
                NotifyError("we need apiKey ");
                return;
            }
            createAlbumRequest.CreateAlbum(name, caption, location, DownloadStringCompleted);
        }

        public void CleanCreatAlbumEvent()
        {
            CreateAlbumCompleted = null;
        }

        #region Override
        /// <summary>
        /// success
        /// </summary>
        /// <param name="resultString"></param>
        public override void RequestComplete(string resultString)
        {
            try
            {
                //获取新建相册对象（aid）
                Album newAlbum = (Album)JsonUtility.DeserializeObj(
                    new MemoryStream(Encoding.UTF8.GetBytes(resultString)), typeof(Album));
                NotifyMessage(newAlbum);
            }
            catch
            {
                NotifyError("encoding error");
                return;
            }
        }

        /// <summary>
        /// error
        /// </summary>
        /// <param name="errorMsg"></param>
        public override void RequestError(string errorMsg)
        {
            NotifyError(errorMsg);
        }
        #endregion

        /// <summary>
        /// 通知成功信息
        /// </summary>
        /// <param name="msg">内容</param>
        private void NotifyMessage(Album msg)
        {
            if (CreateAlbumCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    CreateAlbumCompleted(this, new CreateAlbumCompletedEventArgs(msg));
                });
            }
        }

        /// <summary>
        /// 通知失败信息
        /// </summary>
        /// <param name="msg"></param>
        private void NotifyError(string msg)
        {
            if (CreateAlbumCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    CreateAlbumCompleted(this,
                        new CreateAlbumCompletedEventArgs(new Exception(msg)));
                });
            }
        }
    }
}

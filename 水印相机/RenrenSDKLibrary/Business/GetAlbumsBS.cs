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
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace RenrenSDKLibrary
{
    internal class GetAlbumsBS: BusinessBase
    {
        #region Members
        GetAlbumsRequest getAlbumsRequest;
        ObservableCollection<Album> albumsList = new ObservableCollection<Album>();
        #endregion

        #region Events
        /// <summary>
        /// Occurs when an asynchronous resource-download operation is completed.
        /// </summary>
        public event GetAlbumsRequest.GetAlbumsCompletedHandler GetAlbumsCompleted;
        #endregion

        /// <summary>
        /// 构造
        /// </summary>
        public GetAlbumsBS()
        {
            getAlbumsRequest = new GetAlbumsRequest();
        }

        public void CleanGetAlbumsEvent()
        {
            GetAlbumsCompleted = null;
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
                var result = resultString;
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
                var datalist = (List<Album>)JsonUtility.DeserializeObj(ms, typeof(List<Album>));
                NotifyMessage(datalist);
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
        /// 获取当前用户相册列表
        /// </summary>
        public void GetAlbums(int uid, int page, int count, string aids)
        {

            if (ConstantValue.ApiKey == null)
            {
                NotifyError("we need apiKey ");
                return;
            }

            getAlbumsRequest.GetAlbumList(uid, DownloadStringCompleted, page, count, aids);
        }

        /// <summary>
        /// 通知成功信息
        /// </summary>
        /// <param name="msg">内容</param>
        private void NotifyMessage(List<Album> msg)
        {
            if (GetAlbumsCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    GetAlbumsCompleted(this, new GetAlbumsCompletedEventArgs(msg));
                });
            }
        }

        /// <summary>
        /// 通知失败信息
        /// </summary>
        /// <param name="msg"></param>
        private void NotifyError(string msg)
        {
            if (GetAlbumsCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    GetAlbumsCompleted(this,
                        new GetAlbumsCompletedEventArgs(new Exception(msg)));
                });
            }
        }
    }
}

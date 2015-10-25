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
using System.Collections.Generic;
using RenrenSDKLibrary.Constants;
using System.Text;
using System.IO;

namespace RenrenSDKLibrary
{
    public class GetAlbumsRequest : RenrenClient
    {
        public delegate void GetAlbumsCompletedHandler(object sender, GetAlbumsCompletedEventArgs e);

        /// <summary>
        /// 获取指定用户相册列表
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="userSecretKey"></param>
        /// <param name="userId"></param>
        public void GetAlbumList(int userId, DownloadStringCompletedHandler callback)
        {
            GetAlbumList(userId,callback, -1, -1, null );
        }
        /// <summary>
        /// 获取指定用户相册列表 可扩展
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="userSecretKey"></param>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        public void GetAlbumList(int userId, DownloadStringCompletedHandler callback, int page, int count, string aids)
        {
            string accessToken = RenrenSDK.RenrenInfo.tokenInfo.access_token;
            string callID = String.Format("{0}", DateTime.Now.Second);

            List<APIParameter> parameters = new List<APIParameter>() { new APIParameter("method", Method.GetAlbums) };
            parameters.Add(new APIParameter("access_token", accessToken));
            parameters.Add(new APIParameter("call_id", callID));            
            parameters.Add(new APIParameter("v", "1.0"));
            parameters.Add(new APIParameter("uid", userId.ToString()));
            parameters.Add(new APIParameter("format", "JSON"));

            if (page != -1)
                parameters.Add(new APIParameter("page", page.ToString()));
            if (count != -1)
                parameters.Add(new APIParameter("count", count.ToString()));
            if(!string.IsNullOrEmpty(aids))
                parameters.Add(new APIParameter("aids", aids));
            string sig = ApiHelper.CalSig(parameters);           
            if (string.IsNullOrEmpty(sig))
                return;
            parameters.Add(new APIParameter("sig", sig));

            RenrenWebRequest getAlbumsAgent = new RenrenWebRequest();           
            getAlbumsAgent.DownloadStringCompleted += new RenrenWebRequest.DownloadStringCompletedHandler(callback);
            getAlbumsAgent.DownloadStringAsync(ConstantValue.RequestUri.ToString(), parameters);
        }
    }
}

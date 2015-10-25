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

namespace RenrenSDKLibrary
{
    public class CreateAlbumRequest:RenrenClient
    {
        public delegate void CreateAlbumCompletedHandler(object sender, CreateAlbumCompletedEventArgs e);
        /// <summary>
        /// 创建相册
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="userSecretKey"></param>
        /// <param name="userId"></param>
        public void CreateAlbum(string sessionKey, string name, DownloadStringCompletedHandler callback)
        {
            CreateAlbum(name,string.Empty,string.Empty, callback);
        }

        /// <summary>
        /// 创建相册
        /// </summary>
        /// <param name="sessionKey"></param>
        /// <param name="userSecretKey"></param>
        /// <param name="userId"></param>
        public void CreateAlbum(string name, string description, string location, DownloadStringCompletedHandler callback)
        {
            string accessToken = RenrenSDK.RenrenInfo.tokenInfo.access_token;
            string callID = String.Format("{0}", DateTime.Now.Second);

            List<APIParameter> parameters = new List<APIParameter>();
            parameters.Add(new APIParameter("access_token", accessToken));
            parameters.Add(new APIParameter("call_id", callID));
            parameters.Add(new APIParameter("method", Method.CreateAlbum));
            parameters.Add(new APIParameter("v", "1.0"));
            parameters.Add(new APIParameter("name", name));
            parameters.Add(new APIParameter("format", "JSON"));
            if (!string.IsNullOrEmpty(description))
                parameters.Add(new APIParameter("description",description));
            if(!string.IsNullOrEmpty(location))
                parameters.Add(new APIParameter("location",location));

            string sig = ApiHelper.CalSig(parameters);
            if (string.IsNullOrEmpty(sig))
                return;
            parameters.Add(new APIParameter("sig", sig));

            RenrenWebRequest createAlbumsAgent = new RenrenWebRequest();
            
            createAlbumsAgent.DownloadStringCompleted += new RenrenWebRequest.DownloadStringCompletedHandler(callback);
            createAlbumsAgent.DownloadStringAsync(ConstantValue.RequestUri.ToString(), parameters);
        }
    }
}

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
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.Phone.Info;
using RenrenSDKLibrary.Constants;

namespace RenrenSDKLibrary
{
    public delegate void GetAppFriendsCompletedHandler(object sender, GetAppFriendsCompletedEventArgs e);
    public delegate void GetAppFriendsIDCompletedHandler(object sender, GetAppFriendsIDCompletedEventArgs e);

    public class GetAppFriendsRequest : RenrenClient
    {


        /// <summary>
        /// 得到当前登录用户的App好友列表
        /// </summary>
        /// <param name="sig">签名认证。是用当次请求的所有参数计算出来的值</param>
        /// <param name="method">friends.get</param>
        /// <param name="v">API的版本号，固定值为1.0</param>
        public void GetAppFriends(List<string> scope, DownloadStringCompletedHandler callback)
        {
            string accessToken = RenrenSDK.RenrenInfo.tokenInfo.access_token;
            string callID = String.Format("{0}", DateTime.Now.Second);

            List<APIParameter> parameters = new List<APIParameter>() { 
                new APIParameter("method",Method.GetAppFriends)
            };
            parameters.Add(new APIParameter("access_token", accessToken));
            parameters.Add(new APIParameter("call_id", callID));
            parameters.Add(new APIParameter("v", "1.0"));
            parameters.Add(new APIParameter("userId", RenrenSDK.RenrenInfo.detailInfo.uid.ToString()));
            parameters.Add(new APIParameter("format", "JSON"));
            if (scope != null && scope.Count > 0)
            {
                string[] arrScope = scope.ToArray<string>();
             parameters.Add(new APIParameter("fields", String.Join(" ", arrScope)));
            }
            string strSig = ApiHelper.CalSig(parameters);
            if (strSig == "")
                return;
            parameters.Add(new APIParameter("sig", strSig));

            RenrenWebRequest getAppFriendsAgent = new RenrenWebRequest();
            getAppFriendsAgent.DownloadStringCompleted +=
                new RenrenWebRequest.DownloadStringCompletedHandler(callback);


            getAppFriendsAgent.DownloadStringAsync(ConstantValue.RequestUri.ToString(), parameters);

            
        }

    }
}

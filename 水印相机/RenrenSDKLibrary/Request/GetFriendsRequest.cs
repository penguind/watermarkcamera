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
    public delegate void GetFriendsCompletedHandler(object sender, GetFriendsCompletedEventArgs e);

    public class GetFriendsRequest : RenrenClient
    {


        /// <summary>
        /// 得到当前登录用户的好友列表，带scope参数
        /// </summary>
        /// <param name="sessionkey">当前用户的session_key</param>
        /// <param name="userid">当前用户的id</param>
        /// <param name="callback">回调函数</param>
        /// <param name="scope">额外需要返回的字段，多个字段用逗号(,)隔开。目前支持如下字段: headurl_with_logo, tinyurl_with_logo</param>
        /// <param name="count">返回每页个数，默认为500</param>
        /// <param name="page">分页，默认为1</param>
        public void GetFriends(DownloadStringCompletedHandler callback,List<string> scope, int count ,int page)
        {
            string accessToken = RenrenSDK.RenrenInfo.tokenInfo.access_token;
            string callID = String.Format("{0}", DateTime.Now.Second);

            List<APIParameter> parameters = new List<APIParameter>() { 
                new APIParameter("method",Method.GetFriends)
            };
            parameters.Add(new APIParameter("access_token", accessToken));
            parameters.Add(new APIParameter("call_id", callID));
            parameters.Add(new APIParameter("v", "1.0"));
            parameters.Add(new APIParameter("userId", RenrenSDK.RenrenInfo.detailInfo.uid.ToString()));
            parameters.Add(new APIParameter("format", "JSON"));
            parameters.Add(new APIParameter("page", page.ToString()));
            if (count != 500) parameters.Add(new APIParameter("count", count.ToString()));
            if (scope != null && scope.Count > 0)
            {
                string[] arrScope = scope.ToArray<string>();
                parameters.Add(new APIParameter("fields", String.Join(" ", arrScope)));
            }
            string strSig = ApiHelper.CalSig(parameters);
            if (strSig == "")
                return;
            parameters.Add(new APIParameter("sig", strSig));

            RenrenWebRequest getFriendsAgent = new RenrenWebRequest();
            getFriendsAgent.DownloadStringCompleted +=
                new RenrenWebRequest.DownloadStringCompletedHandler(callback);
            getFriendsAgent.DownloadStringAsync(ConstantValue.RequestUri.ToString(), parameters);


        }

    }
}

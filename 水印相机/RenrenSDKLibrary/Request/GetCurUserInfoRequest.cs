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
    public class GetCurUserInfoRequest : RenrenClient
    {
        /// <summary>
        /// 得到当前登录用户的用户信息
        /// </summary>
        /// <param name="sig">签名认证。是用当次请求的所有参数计算出来的值</param>
        /// <param name="method">users.getLoggedInUser</param>
        /// <param name="v">API的版本号，固定值为1.0</param>


        public void GetCurUserInfo(DownloadStringCompletedHandler callback)
        {
            List<APIParameter> parameters = new List<APIParameter>() 
            { 
                new APIParameter("method","users.getLoggedInUser")//获取当前用户ID
            };
            string accessToken = RenrenSDK.RenrenInfo.tokenInfo.access_token;
            string callID = String.Format("{0}",DateTime.Now.Second);

            //添加其他参数
            parameters.Add(new APIParameter("v", "1.0"));
            parameters.Add(new APIParameter("access_token", accessToken));
            parameters.Add(new APIParameter("call_id", callID));
            parameters.Add(new APIParameter("format", "JSON"));

            //生成并添加sig参数
            string strSig = ApiHelper.CalSig(parameters);
            if (strSig == "")
                return;
            parameters.Add(new APIParameter("sig", strSig));


            RenrenWebRequest getCurUserInfoAgent = new RenrenWebRequest();
            //是否将参数转码utf-8
            getCurUserInfoAgent.DownloadStringCompleted += new RenrenWebRequest.DownloadStringCompletedHandler(callback);
            getCurUserInfoAgent.DownloadStringAsync(ConstantValue.RequestUri.ToString(), parameters);
        }
    }
}

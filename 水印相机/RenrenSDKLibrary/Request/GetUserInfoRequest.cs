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
using System.Collections.ObjectModel;

namespace RenrenSDKLibrary
{
    //获取指定用户
    public delegate void GetUserInfoCompletedHandler(object sender, GetUsersCompletedEventArgs e);

    public class GetUserInfoRequest : RenrenClient
    {
        //以下为详细版
        public void GetUserInfo(List<string> scope, DownloadStringCompletedHandler callback, List<int> uids = null)
        {
            List<APIParameter> parameters = new List<APIParameter>() 
            { 
                new APIParameter("method",Method.GetInfo)//获取当前用户ID
            };

            string accessToken = RenrenSDK.RenrenInfo.tokenInfo.access_token;
            string callID = String.Format("{0}", DateTime.Now.Second);

            //添加其他参数
            parameters.Add(new APIParameter("v", "1.0"));
            parameters.Add(new APIParameter("access_token", accessToken));
            parameters.Add(new APIParameter("call_id", callID));
            parameters.Add(new APIParameter("format", "JSON"));
            if (scope != null && scope.Count > 0)
            {
                string[] arrScope = scope.ToArray<string>();
                parameters.Add(new APIParameter("fields", String.Join(" ", arrScope)));
            }
            if (uids != null && uids.Count > 0)
            {
                int[] uid = uids.ToArray<int>();
                parameters.Add(new APIParameter("uids", String.Join(",", uid)));
            }
        
            //生成并添加sig参数
            string strSig = ApiHelper.CalSig(parameters);
            if (strSig == "")
                return;
            parameters.Add(new APIParameter("sig", strSig));

            RenrenWebRequest getUsersInfoAgent = new RenrenWebRequest();//获取所有用户的
            //是否将参数转码utf-8
            getUsersInfoAgent.DownloadStringCompleted += new RenrenWebRequest.DownloadStringCompletedHandler(callback);
            getUsersInfoAgent.DownloadStringAsync(ConstantValue.RequestUri.ToString(), parameters);//参数准确，从RequestUri下载数据


        }
    }
}

//  Copyright 2012年 Renren Inc. All rights reserved.
//  - Powered by Open Platform. -

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

namespace RenrenSDKLibrary
{
    public class APIRequest : RenrenClient
    {
        //以下为详细版
        public void RequestAPIInterface(List<APIParameter> param, DownloadStringCompletedHandler callback)
        {
            List<APIParameter> parameters = new List<APIParameter>();

            string accessToken = RenrenSDK.RenrenInfo.tokenInfo.access_token;
            string callID = String.Format("{0}", DateTime.Now.Second);

            //添加参数
            parameters.Add(new APIParameter("v", "1.0"));
            parameters.Add(new APIParameter("access_token", accessToken));
            parameters.Add(new APIParameter("call_id", callID));
            parameters.Add(new APIParameter("format", "JSON"));

            foreach (APIParameter customParam in param)
            {
                parameters.Add(customParam);
            }

            //生成并添加sig参数
            string strSig = ApiHelper.CalSig(parameters);
            if (strSig == "")
                return;
            parameters.Add(new APIParameter("sig", strSig));

            RenrenWebRequest webRequest = new RenrenWebRequest();//获取所有用户的
            //是否将参数转码utf-8
            webRequest.DownloadStringCompleted += new RenrenWebRequest.DownloadStringCompletedHandler(callback);
            webRequest.DownloadStringAsync(ConstantValue.RequestUri.ToString(), parameters);//参数准确，从RequestUri下载数据


        }
    }
}

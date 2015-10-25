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
    public delegate void LoginCompletedHandler(object sender, LoginCompletedEventArgs e);

    public class LoginRequest: RenrenClient
    {
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public void LogIn(string username, string password, List<string> scope, 
            DownloadStringCompletedHandler callback )
        {
            List<APIParameter> parameters = new List<APIParameter>() { 
                new APIParameter("method","admin.getAllocation")
            };
            parameters.Add(new APIParameter("grant_type", "password"));
            parameters.Add(new APIParameter("username", username));
            parameters.Add(new APIParameter("password", password));
            parameters.Add(new APIParameter("client_id", ConstantValue.ApiKey));
            parameters.Add(new APIParameter("client_secret", ConstantValue.SecretKey));

            if (scope != null && scope.Count > 0)
            {
                string[] arrScope = scope.ToArray<string>();
                parameters.Add(new APIParameter("scope", String.Join(" ", arrScope)));
            }

            RenrenWebRequest logInAgent = new RenrenWebRequest();
            logInAgent.DownloadStringCompleted +=
                new RenrenWebRequest.DownloadStringCompletedHandler(callback);

            logInAgent.DownloadStringAsync(ConstantValue.OAuthUri.ToString(), parameters);           
        }
    }
}

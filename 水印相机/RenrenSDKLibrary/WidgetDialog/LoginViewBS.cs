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
using RenrenSDKLibrary.Controls;
using Microsoft.Phone.Controls;
using RenrenSDKLibrary.Constants;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace RenrenSDKLibrary.WidgetDialog
{
    public class LoginViewBS : WidgetDialog
    {
        #region Members
        LoginBS loginBS;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when an asynchronous resource-download operation is completed.
        /// </summary>
        public event LoginCompletedHandler LoginCompleted;
        #endregion


        /// <summary>
        /// 构造
        /// </summary>
        public LoginViewBS()
        {
            loginBS = new LoginBS();
            loginBS.LoginCompleted += new LoginCompletedHandler(loginBS_LoginCompleted);
        }

        public void CleanLoginEvent()
        {
            LoginCompleted = null;
        }

        #region Public
        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="redirect_uri"></param>
        /// <param name="scope"></param>
        public void Login(string redirect_uri, List<string> scope)
        {
            if (redirect_uri == null)
            {
                return;
            }

            string uri = ConstantValue.LoginAuth;
            uri += "client_id=" + ConstantValue.ApiKey + "&" + "response_type=token";

            if (scope != null && scope.Count > 0)
            {
                string[] arrScope = scope.ToArray<string>();
                string scopeString = String.Join(" ", arrScope);
                uri += "&" + "scope=" + scopeString;
            }

            uri += "&" + "redirect_uri=" + redirect_uri + "&display=touch";

            if (browserControl != null)
            {
                browserControl.SetUri(uri);
                browserControl.LoadCompleted -=
                     new BrowserControl.LoadCompletedEventHandler(RenrenBrowser_LoadCompleted);
                browserControl.LoadCompleted +=
                     new BrowserControl.LoadCompletedEventHandler(RenrenBrowser_LoadCompleted);
            }
            else
            {
                return;
            }
        }
        #endregion

        /// <summary>
        /// 处理回退事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void page_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            RemoveBrowser();
        }

        /// <summary>
        /// 网页加载成功的回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenrenBrowser_LoadCompleted(object sender,
            NavigatingEventArgs e)
        {
            string redirect_uri = "http://" + e.Uri.Host + e.Uri.AbsolutePath;
            if (redirect_uri == ConstantValue.Redirect_Uri)
            {
                string accessToken = ApiHelper.GetQueryString(new Uri(e.Uri.ToString()), "access_token");
                if (accessToken != "")
                {
                    string expiresIn = ApiHelper.GetQueryString(new Uri(e.Uri.ToString()), "expires_in");
                    loginBS.SaveAccessToken(accessToken, expiresIn);
                }
                string error = ApiHelper.GetQueryString(new Uri(e.Uri.ToString()), "error");
                if (error == "login_denied")
                {
                    RemoveBrowser();
                }
                else if (error != "")
                {
                    NotifyLoginError(error);
                }
            }
        }

        /// <summary>
        /// 事件回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void loginBS_LoginCompleted(object sender, LoginCompletedEventArgs e)
        {
            if (LoginCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    LoginCompleted(this, e);
                });
            }
            RemoveBrowser();
        }

        /// <summary>
        /// 通知失败信息
        /// </summary>
        /// <param name="msg"></param>
        private void NotifyLoginError(string msg)
        {
            if (LoginCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    LoginCompleted(this,new LoginCompletedEventArgs(new Exception(msg)));
                });
            }
            RemoveBrowser();
        }
    }
}

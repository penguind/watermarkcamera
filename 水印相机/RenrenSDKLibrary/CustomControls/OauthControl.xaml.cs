//  Copyright 2012年 Renren Inc. All rights reserved.
//  - Powered by Open Platform. -

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.ComponentModel;
using Microsoft.Phone.Controls;

namespace RenrenSDKLibrary
{
    public partial class OauthControl : UserControl
    {

        public string Scope{get;set;}
        LoginBS loginBS;

        public event LoginCompletedHandler LoginCompleted;

        public OauthControl()
        {
            InitializeComponent();

            loginBS = new LoginBS();
            loginBS.LoginCompleted += new LoginCompletedHandler(loginBS_LoginCompleted);
        }

        private void OauthControl_Loaded(object sende, RoutedEventArgs e)
        {
            string accredit = string.Format("{0}client_id={1}&response_type=token&redirect_uri={2}&scope={3}&display=touch"
                , ConstantValue.LoginAuth, ConstantValue.ApiKey, ConstantValue.Redirect_Uri, this.Scope);

            OAuthBrowser.Navigate(new Uri(accredit));
        }

        private EventHandler _OAuthBrowserCancelled;
        public EventHandler OAuthBrowserCancelled
        {
            get
            {
                return _OAuthBrowserCancelled;
            }
            set
            {
                _OAuthBrowserCancelled = value;
            }
        }

        private EventHandler _OAuthBrowserNavigated;
        public EventHandler OAuthBrowserNavigated
        {
            get
            {
                return _OAuthBrowserNavigated;
            }
            set
            {
                _OAuthBrowserNavigated = value;
            }
        }

        private EventHandler _OAuthBrowserNavigating;
        public EventHandler OAuthBrowserNavigating
        {
            get
            {
                return _OAuthBrowserNavigating;
            }
            set
            {
                _OAuthBrowserNavigating = value;
            }
        }

        private void BrowserNavigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            
        }

        private void BrowserNavigating(object sender, NavigatingEventArgs e)
        {
            string redirect_uri = "http://" + e.Uri.Host + e.Uri.AbsolutePath;
            if (redirect_uri == ConstantValue.Redirect_Uri)
            {
                string accessToken = ApiHelper.GetQueryString(new Uri(e.Uri.ToString()), "access_token");
                if (accessToken != "")
                {
                    string expiresIn = ApiHelper.GetQueryString(new Uri(e.Uri.ToString()), "expires_in");
                    loginBS.SaveAccessToken(accessToken, expiresIn);
                    return;
                }

                string error = ApiHelper.GetQueryString(new Uri(e.Uri.ToString()), "error");
                if (error == "login_denied")
                {
                    if (null != OAuthBrowserCancelled)
                        OAuthBrowserCancelled.Invoke(sender, e);
                    return;
                }
                else if (error != "")
                {
                    if (LoginCompleted != null)
                    {
                        System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                        {
                            LoginCompleted(this, new LoginCompletedEventArgs(new Exception(error)));
                        });
                    }
                    return;
                }
            }
            if (null != OAuthBrowserNavigating)
                OAuthBrowserNavigating.Invoke(sender, e);
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
        }
    }
}

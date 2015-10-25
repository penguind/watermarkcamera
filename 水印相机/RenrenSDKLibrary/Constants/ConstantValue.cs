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

namespace RenrenSDKLibrary
{
    public class ConstantValue
    {
        //ApiKey
        public static string ApiKey = "";
        //AppID
        public static string AppID = "";
        //Secret
        public static string SecretKey = "";

        //授权验证Uri
        public static Uri OAuthUri = new Uri("https://graph.renren.com/oauth/token", UriKind.Absolute);
        //开放平台Uri
        public static Uri RequestUri = new Uri("http://api.renren.com/restserver.do",UriKind.Absolute);
        //重定向Uri
        public static string Redirect_Uri = "http://graph.renren.com/oauth/login_success.html";

        public static string LoginAuth = "https://graph.renren.com/oauth/authorize?";

        public static string WidgetDialog = "http://widget.renren.com/dialog/";

        public static string WidgetRedirect_Uri = "http://widget.renren.com/callback.html";

        public static string PostMethod = "POST";

        public static string GetMethod = "GET";
    }

    // http method
    public enum HttpMethod
    {
        POST,
        GET
    }
}

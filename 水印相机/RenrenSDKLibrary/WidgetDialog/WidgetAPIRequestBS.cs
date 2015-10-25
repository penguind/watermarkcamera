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
using RenrenSDKLibrary.Controls;
using Microsoft.Phone.Controls;
using RenrenSDKLibrary.Constants;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace RenrenSDKLibrary.WidgetDialog
{
    public class WidgetAPIRequestBS : WidgetDialog
    {

        #region public function
        /// <summary>
        /// 显示dialog
        /// </summary>
        /// <param name="requiredParam"></param>
        /// <param name="optionalParam"></param>
        public void RunDialog(String dialogType,
            List<APIParameter> param)
        {
            if (param == null)
            {
                return;
            }
            browserControl.LoadCompleted -=
                 new BrowserControl.LoadCompletedEventHandler(RenrenBrowser_LoadCompleted);
            browserControl.LoadCompleted +=
                 new BrowserControl.LoadCompletedEventHandler(RenrenBrowser_LoadCompleted);

            string uri = ConstantValue.WidgetDialog;
            uri += dialogType + "?";
            uri += "ua=b3d8ed7827b219321125b55d789b7f22";
            uri += "&display=touch";
            uri += "&app_id=" + ConstantValue.AppID;
            uri += "&redirect_uri=" + ConstantValue.WidgetRedirect_Uri;

            foreach(APIParameter customParam in param )
            {
                uri += "&" + customParam.Name + "=" + customParam.Value;
            }

            if (RenrenSDK.RenrenInfo.tokenInfo.access_token != null)
            {
                uri += "&access_token=" + RenrenSDK.RenrenInfo.tokenInfo.access_token;
            }

            if (browserControl != null)
            {
                browserControl.SetUri(uri);
            }
            else
            {
                return;
            }
        }
        #endregion

        #region private function
        /// <summary>
        /// 网页加载成功的回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenrenBrowser_LoadCompleted(object sender,
            NavigatingEventArgs e)
        {
            string error = ApiHelper.GetQueryString(new Uri(e.Uri.ToString()), "error");
            if (error == "access_denied")
            {
                RemoveBrowser();
            }
            else if (error == "login_denied")
            {
                RemoveBrowser();
            }
            else if (error != "")
            {
                NotifyError(error);
            }
            string flag = ApiHelper.GetQueryString(new Uri(e.Uri.ToString()), "flag");
            if (flag == "success")
            {
                NotifyMessage(flag);
            }
        }
        #endregion
    }
}

/*
 * 分享页
 * 暂时只能分享到腾讯微博、新浪微博、人人
 * 
 */ 

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
using Microsoft.Phone.Controls;
using RenrenSDKLibrary;
using SinaBase;
using WeiboSdk;
using System.IO.IsolatedStorage;

namespace WaterMark
{
    public partial class SharePage : PhoneApplicationPage
    {
        RenrenAPI api = 水印相机.App.api;
        private IsolatedStorageSettings _appSetting;

        public SharePage()
        {
            InitializeComponent();
            _appSetting = IsolatedStorageSettings.ApplicationSettings;
            //MessageBox.Show("如果您需要测试账号2017507471@qq.com密码renrentest","仅测试通知",MessageBoxButton.OK);
        }

        // 腾讯微博
        private void tencentWeiboButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (WaterMark.WTool.WNetWork.isNetAvailible() == false) { MessageBox.Show("Network not available"); return; }
            try
            {
                NavigationService.Navigate(new Uri("/share/TencentWeiboEditPage.xaml", UriKind.Relative));
            }
            catch (Exception) { MessageBox.Show("Tencent weibo not available now") ;}
        }

        // 人人
        private void renrenShareButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (WaterMark.WTool.WNetWork.isNetAvailible() == false) { MessageBox.Show("Network not available"); return; }
            List<string> scope = new List<string> { "publish_feed", "publish_blog", "read_user_album", "create_album", "photo_upload" };
            if (api.IsAccessTokenValid())
            {
                NavigationService.Navigate(new Uri("/share/RenrenEditPage.xaml", UriKind.Relative));
            }
            else
            {
                api.Login(this, scope, renren_LoginCompletedHandler);
            }
        }
        public void renren_LoginCompletedHandler(object sender, LoginCompletedEventArgs e)
        {
            if (e.Error == null)
                NavigationService.Navigate(new Uri("/share/RenrenEditPage.xaml", UriKind.Relative));
            else
                MessageBox.Show(e.Error.Message);
        }

        // 新浪微博
        private void sinaWeiboButton_Click(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (WaterMark.WTool.WNetWork.isNetAvailible() == false) { MessageBox.Show("Network not available"); return; }
            if (水印相机.App.api.IsAccessTokenValid())
            {
                NavigationService.Navigate(new Uri("/share/RenrenEditPage.xaml", UriKind.Relative));
            }
            else
            {
                WeiboSdk.PageViews.AuthenticationView.OAuth2VerifyCompleted = (e1, e2, e3) => VerifyBack(e1, e2, e3);

                WeiboSdk.PageViews.AuthenticationView.OBrowserCancelled = new EventHandler(cancleEvent);

                ////其它通知事件...
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NavigationService.Navigate(new Uri("/WeiboSdk;component/PageViews/AuthenticationView.xaml", UriKind.Relative));
                });
            }
        }
        public void cancleEvent(Object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/share/SharePage.xaml", UriKind.Relative));
        }
        private void VerifyBack(bool isSucess, SdkAuthError errCode, SdkAuth2Res response)
        {

            if (errCode.errCode == SdkErrCode.SUCCESS)
            {
                if (null != response)
                {
                    水印相机.App.SinaAccessToken = response.accesssToken;
                    水印相机.App.SinaRefleshToken = response.refleshToken;
                }
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NavigationService.Navigate(new Uri("/share/SinaWeiboEditPage.xaml", UriKind.Relative));
                });
            }
            else if (errCode.errCode == SdkErrCode.NET_UNUSUAL)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show("Network not available");
                });
            }
            else if (errCode.errCode == SdkErrCode.SERVER_ERR)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    MessageBox.Show(errCode.specificCode,"Sina weibo server error" ,MessageBoxButton.OK);
                });
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (TencentWeiboSDK.OAuthConfigruation.IfSaveAccessToken == true)
            {
                this.twName.Text = TencentWeiboSDK.OAuthConfigruation.AccessToken.Name;
                this.twName.Visibility = System.Windows.Visibility.Visible;
                this.logoutTencentWeiboButton.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.twName.Text = "";
                this.twName.Visibility = System.Windows.Visibility.Collapsed;
                this.logoutTencentWeiboButton.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        //注销腾讯微博
        private void logoutTencentWeiboButton_Click(object sender, RoutedEventArgs e)
        {
            TencentWeiboSDK.OAuthConfigruation.ClearAccessToken();
            TencentWeiboSDK.OAuthConfigruation.IfSaveAccessToken = false;
            this.twName.Text = "";
            this.twName.Visibility = System.Windows.Visibility.Collapsed;
            this.logoutTencentWeiboButton.Visibility = System.Windows.Visibility.Collapsed;

        }

        private void PhoneApplicationPage_BackKeyPress_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();

        }

        private void BackButtonPress(object sender, EventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        } 
    }

}
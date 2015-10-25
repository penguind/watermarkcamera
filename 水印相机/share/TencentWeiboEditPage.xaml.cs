/*
 *               分享到腾讯微博                    *  
 *    app_key和app_sercet在 水印相机.App 中定义    *
 *            
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using Microsoft.Phone.Info;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.Threading;
using TencentWeiboSDK;
using TencentWeiboSDK.Services;
using TencentWeiboSDK.Services.Util;
using TencentWeiboSample.ViewModel;

namespace WaterMark.share
{
    public partial class TencentWeiboEditPage : PhoneApplicationPage
    {
        //实例化ViewModel
        private PostNewViewModel vm = new PostNewViewModel();
        BitmapImage imageSource;
        bool isPublish;
        public TencentWeiboEditPage()
        {
            InitializeComponent();
            isPublish = false;
            imageSource = new BitmapImage();
            try
            {
                if (OAuthConfigruation.IfSaveAccessToken == true) this.loginGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
            catch (Exception) { MessageBox.Show("Tencent weibo is not available"); }
            loadPic();
        }

        // 确认分享
        private void confirmButton(object sender, EventArgs e)
        {
            if (this.loginGrid.Visibility == System.Windows.Visibility.Visible) return;
            focusButton.Focus();
            //if (this.waitGrid.Visibility == System.Windows.Visibility.Visible) return;//防止触发多次
            if (this.inputText.Text.Trim().Length == 0 || this.inputText.Text.Length > 140) { inputText.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red) ; return; }
            if (isPublish == true && MessageBox.Show("You may have send it.Send it again?", "Prompt", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            else
            {
                vm.ImageSource = imageSource;
                vm.Text = this.inputText.Text;
                isPublish = true;
                vm.Post(() =>
                {
                    //回调回来后可以通知用户已经发送成功或失败
                });
                sharingGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }
        private void Login()
        {
            // 开始进行 OAuth 授权.
            oAuthLoginBrowser1.OAuthLogin((callback) =>
            {
                // 若已获得 AccessToken 则跳转到 TimelineView 页面
                // 注意： 若 OAuthConfigruation.IfSaveAccessToken 属性为 False，则需要在此处保存用户的 AccessToken(callback.Data) 以便下次使用.
                if (callback.Succeed)
                {
                    OAuthConfigruation.AccessToken = callback.Data;
                    OAuthConfigruation.IfSaveAccessToken = true;
                    this.loginGrid.Visibility = System.Windows.Visibility.Collapsed;
                    loadPic();
                    this.twName.Text = OAuthConfigruation.AccessToken.Name;
                    this.ApplicationBar.IsVisible = true;
                }
            });
        }
        public void loadPic()
        {
            try
            {
                String shareimg = "shareimg.jpg";
                using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (iso.FileExists(shareimg))
                    {
                        using (IsolatedStorageFileStream isostream = iso.OpenFile("shareimg.jpg", FileMode.Open, FileAccess.Read))
                        {
                            imageSource.SetSource(isostream);
                            this.shareImage.Source = imageSource;
                        }
                    }
                    else
                    {
                        MessageBox.Show("Picture loaded failed,text is available", "Error", MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception) { MessageBox.Show("Picture loaded failed,text is available", "Error", MessageBoxButton.OK); imageSource = null; }
        }
        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            Login();
        }

        private void inputText_TextInput(object sender, TextChangedEventArgs e)
        {
            if (this.inputText.Text.Length > 140)
            {
                this.numText.Text = "140 words most, " + (this.inputText.Text.Length - 140).ToString() + " over";
                inputText.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
            }
            else
            {
                this.numText.Text = "" + (140 - this.inputText.Text.Length).ToString() + " available";
                inputText.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            }
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (isPublish == false && MessageBox.Show("BackKey again to giving up saving?", "Prompt", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) { e.Cancel = true; return; }
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }
    }
}
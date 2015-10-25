/*
 *               分享到新浪微博                    *  
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
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Info;
using System.IO;
using System.IO.IsolatedStorage;
using SinaBase;
using WeiboSdk;

namespace WaterMark.share
{
    public partial class SinaWeiboEditPage : PhoneApplicationPage
    {
        double toastTimeout;
        BitmapImage imageSource;
        string fileName;
        bool isPublish;

        public SinaWeiboEditPage()
        {
            InitializeComponent();
            fileName = "";
            isPublish = false;
            imageSource = new BitmapImage();
            this.textBox2.Text = 水印相机.App.SinaReloginText;
        }
        
        private void inputText_TextInput(object sender, TextChangedEventArgs e)
        {
            if (this.inputText.Text.Length > 140)
            {
                this.numText.Text = "140 is most," + (this.inputText.Text.Length - 140).ToString() + " is over";
                inputText.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
            }
            else
            {
                this.numText.Text = "" + (140 - this.inputText.Text.Length).ToString() + " available";
                inputText.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            }
        }
        void toastGridTimeTick(object sender, EventArgs e)
        {
            toastTimeout -= 0.1;
            if (toastTimeout < 0.0)
            {
                this.waitGrid.Visibility = System.Windows.Visibility.Collapsed;
                (sender as System.Windows.Threading.DispatcherTimer).Stop();
            }
        }
        public void closeToast()
        {
            this.waitGrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
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
                            fileName = DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + "_" + DateTime.Now.Hour + ":" + DateTime.Now.Minute+".jpg";
                        }
                    }
                    else
                    {
                        MessageBox.Show("Picture loaded failed", "Error", MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception) { MessageBox.Show("Picture loaded failed", "Error", MessageBoxButton.OK); imageSource = null; }
        }

        private void confirmButton_Click(object sender, EventArgs e)
        {
            if (this.loginGrid.Visibility == System.Windows.Visibility.Visible) return;
            focusButton.Focus();
            if (waitGrid.Visibility == System.Windows.Visibility.Visible) return;
            if (this.inputText.Text.Trim().Length == 0 || this.inputText.Text.Length > 140) { inputText.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red); return; }
            if (isPublish == true && MessageBox.Show("You may have send it.Send it again?", "Prompt", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            SdkShare sdkShare = new SdkShare();
            sdkShare.AccessToken = 水印相机.App.SinaAccessToken;
            sdkShare.PicturePath = "shareimg.jpg";
            sdkShare.Message = this.inputText.Text;
            sdkShare.TitleText = "";
            sdkShare.Completed = new EventHandler<SendCompletedEventArgs>(ShareCompleted);
            waitGrid.Visibility = System.Windows.Visibility.Visible;
            this.textBox2.Text = "Updating";
            isPublish = true;
            sdkShare.Show();
        }
        void ShareCompleted(object sender, SendCompletedEventArgs e)
        {
            if (e.IsSendSuccess)
            {
                this.textBox2.Text = "Successfully!";
                水印相机.App.SinaReloginText = "";
            }
            else
            {
                if (e.Response.Contains("过期"))
                {
                    水印相机.App.SinaAccessToken = "";
                    水印相机.App.SinaReloginText = this.textBox2.Text;
                    MessageBox.Show("Relog in please.");
                    if (NavigationService.CanGoBack) NavigationService.GoBack();
                }
            }
            toastTimeout = 3.0;
            System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer();
            tmr.Interval = TimeSpan.FromSeconds(0.1);
            tmr.Tick += toastGridTimeTick;
            tmr.Start();
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isPublish == false && MessageBox.Show("BackKey again to giving up saving.", "Prompt", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) { e.Cancel = true; return; }
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }

    }

}
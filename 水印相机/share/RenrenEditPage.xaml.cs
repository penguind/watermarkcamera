/*
 *               分享到人人                    *  
 *  app_key和app_sercet在 水印相机.App 中定义  *
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
using RenrenSDKLibrary;

namespace WaterMark.share
{
    public partial class RenrenEditPage : PhoneApplicationPage
    {
        RenrenAPI api = 水印相机.App.api;
        private BitmapImage imageSource;
        private string fileName;
        double toastTimeout;
        bool isPublish;

        public RenrenEditPage()
        {
            InitializeComponent();
            imageSource = new BitmapImage();
            isPublish = false;
        }

        private void inputText_TextInput(object sender, TextChangedEventArgs e)
        {
            if (this.inputText.Text.Length > 240)
            {
                this.numText.Text = "240 is most, " + (this.inputText.Text.Length - 240).ToString() + " is over";
                inputText.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Orange);
            }
            else
            {
                this.numText.Text = "" + (240 - this.inputText.Text.Length).ToString() + " available";
                inputText.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
            }
        }

        private void confirmButton(object sender, EventArgs e)
        {
            focusButton.Focus();
            if (waitGrid.Visibility == System.Windows.Visibility.Visible) return;
            if (this.inputText.Text.Trim().Length == 0 || this.inputText.Text.Length > 240) { inputText.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Red); return; }
            if (isPublish == true && MessageBox.Show("You may have send it.Send it again?", "Prompt", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            this.textBox2.Text = "Updating...";
            api.PublishPhoto(imageSource, fileName, UphotPhoto_DownloadStringCompleted,inputText.Text);
            waitGrid.Visibility = System.Windows.Visibility.Visible;
            isPublish = true;
        }

        private void UphotPhoto_DownloadStringCompleted(object sender,
             RenrenSDKLibrary.UploadPhotoCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                this.textBox2.Text = e.Error.ToString();
            }
            else
            {
                this.textBox2.Text = "Successfully!";
            }
            toastTimeout = 3.0;
            System.Windows.Threading.DispatcherTimer tmr = new System.Windows.Threading.DispatcherTimer();
            tmr.Interval = TimeSpan.FromSeconds(0.1);
            tmr.Tick += toastGridTimeTick;
            tmr.Start();
        }
        void toastGridTimeTick(object sender, EventArgs e)
        {
            toastTimeout -= 0.1;
            if (toastTimeout < 0.0)
            {
                this.waitGrid.Visibility = System.Windows.Visibility.Collapsed;
                (sender as System.Windows.Threading.DispatcherTimer).Stop();
                if (NavigationService.CanGoBack) NavigationService.GoBack();
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

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {

            if (isPublish == false && MessageBox.Show("BackKey again to giving up saving?", "Prompt", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) { e.Cancel = true; return; }
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }
    }
}
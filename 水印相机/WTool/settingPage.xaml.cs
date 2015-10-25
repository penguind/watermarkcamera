using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using WaterMark;
using Microsoft.Devices;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;

namespace 水印相机
{
    public partial class settingPage : PhoneApplicationPage
    {
        //获取设置
        private IsolatedStorageSettings _appSetting;
        //访问图片库
        MediaLibrary library;
        ////设置分辨率的选择按钮
        //ContextMenu resolutionMenu;
        public settingPage()
        {
            InitializeComponent();
            _appSetting = IsolatedStorageSettings.ApplicationSettings;
            //自动联网获取信息--默认开启
            if (_appSetting.Contains("autonet"))
            {
                if (_appSetting["autonet"].ToString().Trim().CompareTo("ON") == 0)
                {
                    this.netSwitch.IsChecked = true;
                }
                else
                    this.netSwitch.IsChecked = false;
            }
            else
            {
                _appSetting.Add("autonet", "OFF");
                this.netSwitch.IsChecked = false;
                _appSetting.Save();
            }
            // 显示机型--默认关闭
            if (_appSetting.Contains("showWPLogo"))
            {
                if (_appSetting["showWPLogo"].ToString().Trim().CompareTo("ON") == 0)
                {
                    this.showWPSwith.IsChecked = true;
                }
                else
                    this.showWPSwith.IsChecked = false;
            }
            else
            {
                _appSetting.Add("showWPLogo", "OFF");
                this.showWPSwith.IsChecked = false;
                _appSetting.Save();
            }
            // 设置中保存的签名
            if (_appSetting.Contains("signature"))
            {
                this.usersign.Text = _appSetting["signature"].ToString().Trim();
            }
            else
            {
                _appSetting.Add("signature", "");
                _appSetting.Save();
            }
            // 取景相机中保存原图--默认保存
            if (_appSetting.Contains("saveRealBackup"))
            {
                if (_appSetting["saveRealBackup"].ToString().Trim().CompareTo("ON") == 0)
                {
                    this.saveRealBackup.IsChecked = true;
                }
                else
                    this.saveRealBackup.IsChecked = false;
            }
            else
            {
                _appSetting.Add("saveRealBackup", "ON");
                this.saveRealBackup.IsChecked = true;
                _appSetting.Save();
            } 
            // 取景相机使用前置摄像头
            if (_appSetting.Contains("useFrontCam"))
            {
                if (_appSetting["useFrontCam"].ToString().Trim().CompareTo("ON") == 0)
                {
                    this.useFrontCamButton.IsChecked = true;
                }
                else
                    this.useFrontCamButton.IsChecked = false;
            }
            else
            {
                _appSetting.Add("useFrontCam", "OFF");
                this.useFrontCamButton.IsChecked = false;
                _appSetting.Save();
            }
            // 取景相机使用的分辨率等级
            if (_appSetting.Contains("camResolutionLevel"))
            {
                if (_appSetting["camResolutionLevel"].ToString().ToLower().Trim().CompareTo("b") == 0)
                {
                    resolutionList.SelectedIndex = 1;
                }
                else if (_appSetting["camResolutionLevel"].ToString().ToLower().Trim().CompareTo("c") == 0)
                {
                    resolutionList.SelectedIndex = 2;
                }
                else if (_appSetting["camResolutionLevel"].ToString().ToLower().Trim().CompareTo("d") == 0)
                {
                    resolutionList.SelectedIndex = 3;
                }
                else
                    resolutionList.SelectedIndex = 0; // a
            }
            else
            {
                _appSetting.Add("camResolutionLevel", "a");
                resolutionList.SelectedIndex = 0; // a
                _appSetting.Save();
            }

            titleText.Text = "WaterMark Camera version " + App.waterMarkVersion;

            if ((new Microsoft.Phone.Marketplace.LicenseInformation()).IsTrial() == true) //试用版
            {
                donateButoon.Visibility = System.Windows.Visibility.Visible;
                thanksText.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                donateButoon.Visibility = System.Windows.Visibility.Collapsed;
                thanksText.Visibility = System.Windows.Visibility.Visible;
            }
        }
           
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e); 
            if (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == false)
            {
                this.useFrontCamButton.IsChecked = false;
                this.useFrontCamButton.IsEnabled = false;
            } 
        } 

        //返回键
        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            try
            { 
                //选择了不同的分辨率选项
                if (resolutionList.SelectedIndex == 3)
                {
                    _appSetting["camResolutionLevel"] = "d";
                }
                else if (resolutionList.SelectedIndex == 2)
                {
                    _appSetting["camResolutionLevel"] = "c";
                }
                else if (resolutionList.SelectedIndex == 1)
                {
                    _appSetting["camResolutionLevel"] = "b";
                }
                else  // 0 and error
                {
                    _appSetting["camResolutionLevel"] = "a";
                }
                _appSetting.Save();
            }
            catch (Exception) { }
            NavigationService.GoBack();
        }

        //添加签名
        private void addSignature(object sender, RoutedEventArgs e)
        {
            _appSetting["signature"] = this.usersign.Text.Trim();
            _appSetting.Save();
        }
        private void usersign_GotFocus(object sender, RoutedEventArgs e)
        {
            this.animation_textinput_click.Begin(); //晃动的动画
        }

        //开启或关闭网络开关
        private void netSwitch_Checked(object sender, RoutedEventArgs e)
        {
            _appSetting["autonet"] = "ON";
            _appSetting.Save();
        }
        private void netSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            _appSetting["autonet"] = "OFF";
            _appSetting.Save();
        }

        // 联系我
        private void connectMeLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Phone.Tasks.EmailComposeTask emailComposeTask = new Microsoft.Phone.Tasks.EmailComposeTask();
                emailComposeTask.Subject = "A letter from watermark camera";
                emailComposeTask.Body = "";
                emailComposeTask.To = "562862158@qq.com";
                emailComposeTask.Show();
            }
            catch (Exception) { }
        }

        private void voteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var details = new Microsoft.Phone.Tasks.MarketplaceDetailTask();
                details.ContentIdentifier = "727564a8-b795-4da0-a63d-635cf5d50ec4"; //link to English version app in the App Store
                details.Show();
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to connect to the market.Please commit in market-store app");
            }
        }

        // 显示机型
        private void showWPSwith_Checked(object sender, RoutedEventArgs e)
        {
            _appSetting["showWPLogo"] = "ON";
            _appSetting.Save();
        }

        private void showWPSwith_Unchecked(object sender, RoutedEventArgs e)
        {
            _appSetting["showWPLogo"] = "OFF";
            _appSetting.Save();
        }

        //实时取景相机保存原图
        private void saveRealBackup_Checked(object sender, RoutedEventArgs e)
        {
            _appSetting["saveRealBackup"] = "ON";
            _appSetting.Save();
        }

        private void saveRealBackup_Unchecked(object sender, RoutedEventArgs e)
        {
            _appSetting["saveRealBackup"] = "OFF";
            _appSetting.Save();
        }  

        // 重置所有设置
        private void resetSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.animation_resetButtonClick.Begin();
            if (MessageBox.Show("Sure to clear settings?\nYour photo will be safe", "Warning", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                _appSetting.Clear();
                if (_appSetting.Contains("version") == true)
                {
                    _appSetting["version"] = App.waterMarkVersion;
                }
                else
                {
                    _appSetting.Add("version", App.waterMarkVersion);
                }
                _appSetting.Save();
                this.usersign.Text = "";
                netSwitch.IsChecked = false;
                showWPSwith.IsChecked = false;
                MessageBox.Show("If unexcepted quit often occur, please email me.", "Settings reset successfully", MessageBoxButton.OK);
            }
        }

        private void useFrontCamButton_Checked(object sender, RoutedEventArgs e)
        {
            _appSetting["useFrontCam"] = "ON";
            _appSetting.Save();
        }

        private void useFrontCamButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _appSetting["useFrontCam"] = "OFF";
            _appSetting.Save();
        }

        //发送邮件
        private void mailButton_Click(object sender, RoutedEventArgs e)
        {
            EmailComposeTask etask = new EmailComposeTask();
            etask.Subject = "About watermark camera";
            etask.To = "562862158@qq.com";
            etask.Show();
        }

        //捐助
        private void donateButton_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
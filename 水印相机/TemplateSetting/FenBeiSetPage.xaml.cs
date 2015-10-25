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

namespace WaterMark.TemplateSetting
{
    public partial class FenBeiSetPage : PhoneApplicationPage
    {
        private IsolatedStorageSettings _appSetting;
        public FenBeiSetPage()
        {
            InitializeComponent();
            _appSetting = IsolatedStorageSettings.ApplicationSettings;
            if (_appSetting.Contains("useFenbeiByDefault"))
            {
                if (_appSetting["useFenbeiByDefault"].ToString().Trim().CompareTo("ON") == 0)
                {
                    this.useDefaultFB.IsChecked = true;
                    this.userWord.IsEnabled = false;
                }
                else
                {
                    this.useDefaultFB.IsChecked = false;
                    this.userWord.IsEnabled = true;
                }
            }
            else
            {
                _appSetting.Add("useFenbeiByDefault", "ON");
                _appSetting.Save();
            }
            if (_appSetting.Contains("fenbeiWords"))
            {
                this.userWord.Text = _appSetting["fenbeiWords"].ToString();
            }
            else
            {
                _appSetting.Add("fenbeiWords", "");
                _appSetting.Save();
                this.userWord.Text = "";
            }
        }

        //使用默认文本
        private void useDefault_Checked(object sender, RoutedEventArgs e)
        {
            _appSetting["useFenbeiByDefault"] = "ON";
            _appSetting.Save();
            this.userWord.IsEnabled = false;
        }
        private void useDefault_Unchecked(object sender, RoutedEventArgs e)
        {
            _appSetting["useFenbeiByDefault"] = "OFF";
            _appSetting.Save();
            this.userWord.IsEnabled = true;
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _appSetting["fenbeiWords"] = this.userWord.Text;
        }

    }
}
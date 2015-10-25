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
using System.IO.IsolatedStorage;

namespace WaterMark
{
    public partial class GuidePage : PhoneApplicationPage
    {
        public GuidePage()
        {
            InitializeComponent();
        }

        private void beginButton_Click(object sender, RoutedEventArgs e)
        {
           var _appSetting = IsolatedStorageSettings.ApplicationSettings;
           if (_appSetting.Contains("version"))
           {
               if (_appSetting["version"].ToString().Trim().CompareTo(水印相机.App.waterMarkVersion) != 0)
               {
                   _appSetting["version"] = 水印相机.App.waterMarkVersion;
               }
           }
           else
           {
               _appSetting.Add("version", 水印相机.App.waterMarkVersion);
               _appSetting.Save();
           }
           //NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
           if (NavigationService.CanGoBack) NavigationService.GoBack();
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {

            var _appSetting = IsolatedStorageSettings.ApplicationSettings;
            if (_appSetting.Contains("version"))
            {
                if (_appSetting["version"].ToString().Trim().CompareTo(水印相机.App.waterMarkVersion) != 0)
                {
                    _appSetting["version"] = 水印相机.App.waterMarkVersion;
                }
            }
            else
            {
                _appSetting.Add("version", 水印相机.App.waterMarkVersion);
                _appSetting.Save();
            }
            //NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }
    }
}
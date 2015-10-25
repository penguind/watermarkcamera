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
using System.Windows.Media;

namespace WaterMark.WTool
{
    public partial class Colorw2 : PhoneApplicationPage
    {
        private IsolatedStorageSettings _appSetting;
        private byte aValue=255;
        public Colorw2()
        {
            InitializeComponent();
            _appSetting = IsolatedStorageSettings.ApplicationSettings;
            
        }
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                Color colors = new Color() { A = 255, G = 255, B = 255, R = 255 };
                if (_appSetting.Contains("ColorA"))
                {
                    aValue = colors.A = Byte.Parse(_appSetting["ColorA"].ToString().Trim());
                    aSlider.Value = aValue / 25.5;
                    aPer.Text = ((int)(aValue / 2.55)).ToString() + "%";
                }
                else
                    _appSetting.Add("ColorA", "255");  
                if (_appSetting.Contains("ColorR")) 
                    colors.R = (Byte)(Int32.Parse(_appSetting["ColorR"].ToString().Trim()));  
                else 
                    _appSetting.Add("ColorR", "255"); 
                if (_appSetting.Contains("ColorG")) 
                    colors.G = (Byte)(Int32.Parse(_appSetting["ColorG"].ToString().Trim()));  
                else 
                    _appSetting.Add("ColorG", "255"); 
                if (_appSetting.Contains("ColorB")) 
                    colors.B = (Byte)(Int32.Parse(_appSetting["ColorB"].ToString().Trim()));  
                else 
                    _appSetting.Add("ColorB", "255"); 
                // 简单水印设置
                string useAlignment = NavigationContext.QueryString["useAlignment"];
                if (useAlignment != null && useAlignment.Length > 0 && useAlignment.Contains("true"))
                {
                    // 自定义的字体大小
                    if (_appSetting.Contains("FontSize"))
                    {
                        this.sizeTextBox.Text = Double.Parse(_appSetting["FontSize"].ToString().Trim()).ToString().Trim();
                    }
                    else
                    {
                        _appSetting.Add("FontSize", "5");
                    }
                    //自定义位置
                    if (_appSetting.Contains("SimpleTextAlignment"))
                    {
                        string alignmentsetting = _appSetting["SimpleTextAlignment"].ToString().Trim().ToLower();
                        if (alignmentsetting.CompareTo("right") == 0) { this.simpleAlignmentRight.IsChecked = true; }
                        else
                        {
                            if (alignmentsetting.CompareTo("center") == 0) { this.simpleAlignmentCenter.IsChecked = true; }
                            else { this.simpleAlignmentLeft.IsChecked = true; }
                        }
                    }
                    else
                    {
                        _appSetting.Add("SimpleTextAlignment", "left");
                        this.simpleAlignmentLeft.IsChecked = true;
                    }
                    // 设置使用黑色背景条
                    if (_appSetting.Contains("UseBackGround"))
                    {
                        if (_appSetting["UseBackGround"].ToString().ToLower().Contains("yes") == true)
                            this.useBackGround.IsChecked = true;
                    }
                    else
                    {
                        _appSetting.Add("UseBackGround", "yes");
                    }
                    simpleSettingGrid.Visibility = System.Windows.Visibility.Visible;
                }
                else simpleSettingGrid.Visibility = System.Windows.Visibility.Collapsed;
                title.Foreground = new SolidColorBrush(colors);
                this.m_colorpicker.Color = colors;
                _appSetting.Save();
            }
            catch (Exception) { NavigationService.Navigate(new Uri("MainPage.xaml", UriKind.Relative)); }
        }

        private void m_colorpicker_ColorChanged(object sender, Color color)
        {
            this.title.Foreground = new SolidColorBrush( this.m_colorpicker.Color );
        }

        private void aSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
                aValue = (byte)(aSlider.Value * 25.5);
                aPer.Text = ((int)(((double)aValue)/2.55)).ToString()+"%";
                this.title.Foreground = new SolidColorBrush(new Color() { R = m_colorpicker.Color.R, G = m_colorpicker.Color.G, B = m_colorpicker.Color.B, A = aValue });
        }
        private bool isPostDouble(string text)
        {
            string num = text.Trim();
            bool hasPoint = false;
            int length = num.Length;
            if (length == 0) return false;
            char temp;
            for (int i = 0; i < length; ++i)
            {
                temp = num[i];
                if (temp != '0' && temp != '1' && temp != '2' && temp != '3' && temp != '4' && temp != '5' && temp != '6' && temp != '7' && temp != '8' && temp != '9')
                {
                    if (temp != '.') return false;
                    else
                    {
                        if (hasPoint == false && (i < length || i == 0))
                        {
                            hasPoint = true;
                        }
                        else return false;
                    }
                }
            }
            return true;
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // 设置中保存的字体颜色
                if (_appSetting.Contains("ColorA"))
                    _appSetting["ColorA"] = aValue.ToString();
                else
                    _appSetting.Add("ColorA", aValue.ToString());  
                if (_appSetting.Contains("ColorR"))
                    _appSetting["ColorR"] = m_colorpicker.Color.R.ToString().Trim();
                else 
                    _appSetting.Add("ColorR", m_colorpicker.Color.R.ToString().Trim()); 
                if (_appSetting.Contains("ColorG"))
                    _appSetting["ColorG"] = m_colorpicker.Color.G.ToString().Trim();
                else
                    _appSetting.Add("ColorG", m_colorpicker.Color.G.ToString().Trim()); 
                if (_appSetting.Contains("ColorB"))
                    _appSetting["ColorB"] = m_colorpicker.Color.B.ToString().Trim(); 
                else
                    _appSetting.Add("ColorB", m_colorpicker.Color.B.ToString().Trim());  
                // 简单模板
                if (simpleSettingGrid.Visibility == System.Windows.Visibility.Visible)
                {
                    // 自定义的字体大小
                    if (_appSetting.Contains("FontSize"))
                    {
                        if (isPostDouble(sizeTextBox.Text))
                        {
                            if (double.Parse(sizeTextBox.Text.Trim()) < 0) sizeTextBox.Text = (-1 * double.Parse(sizeTextBox.Text.Trim())).ToString();
                            _appSetting["FontSize"] = this.sizeTextBox.Text.Trim();
                        }
                        else
                        {
                            MessageBox.Show("Only rightful positive integers and decimals accepted，like 5, 6.8","Font not saved.",MessageBoxButton.OK);
                        }
                    }
                    else
                        _appSetting.Add("FontSize", "5");
                    //自定义位置
                    if (_appSetting.Contains("SimpleTextAlignment"))
                    {
                        if (this.simpleAlignmentRight.IsChecked == true) _appSetting["SimpleTextAlignment"] = "right";
                        else if (this.simpleAlignmentCenter.IsChecked == true) _appSetting["SimpleTextAlignment"] = "center";
                        else _appSetting["SimpleTextAlignment"] = "left";
                    }
                    else
                        _appSetting.Add("SimpleTextAlignment", "left");
                    // 是否使用背景条
                    if (_appSetting.Contains("UseBackGround"))
                    {
                        if (this.useBackGround.IsChecked == true)
                            _appSetting["UseBackGround"] = "yes";
                        else
                            _appSetting["UseBackGround"] = "no";
                    }
                    else
                    {
                        _appSetting.Add("UseBackGround", "yes");
                    }
                }
                _appSetting.Save();
                if (NavigationService.CanGoBack) NavigationService.GoBack();
            }
            catch (Exception) { NavigationService.Navigate(new Uri("MainPage.xaml", UriKind.Relative)); }
        }
    }
}
//  Copyright 2011年 Renren Inc. All rights reserved.
//  - Powered by Team Pegasus. -

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
using System.Windows.Media.Imaging;
using System.Threading;
using RenrenSDKLibrary.WidgetDialog;

namespace RenrenSDKLibrary
{
    public partial class UploadPhotoPage : PhoneApplicationPage
    {
        LoginViewBS loginViewBS;
        UploadPhotoBS uploadBS;
        private string imgPath;     //图片路径
        private string imgCaption;  //图片描述
        private bool canGoBack = true;//是否可以后退
        public UploadPhotoPage()
        {
            this.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(FastUploadPage_OrientationChanged);
            this.Loaded += new RoutedEventHandler(FastUploadPage_Loaded);
        }

        //页面没有内容时先添加白色背景
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            StackPanel sp = new StackPanel() { Background = new SolidColorBrush(Colors.White)};
            this.Content = sp;
        }

        void FastUploadPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (RenrenSDK.RenrenInfo.tokenInfo.access_token == null)
            {
                if (loginViewBS == null)
                {
                    loginViewBS = new LoginViewBS();
                }
                List<string> scope = new List<string>() { "photo_upload" };
                loginViewBS.CleanDownloadStringEvent();
                loginViewBS.LoginCompleted += LoginCompletedHandler;

                loginViewBS.InitView(this);
                loginViewBS.Login(ConstantValue.Redirect_Uri, scope);
            }
            else
            {
                PrepareUploadPhotoPage();
            }
        }
        public void LoginCompletedHandler(object sender, LoginCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                PrepareUploadPhotoPage();
            }
            else
                MessageBox.Show(e.Error.Message);
        }
        private void PrepareUploadPhotoPage()
        {
            InitializeComponent();

            imgPath = NavigationContext.QueryString["path"];
            imgCaption = NavigationContext.QueryString["caption"];
            if (RenrenSDK.publishPhoto != null)
                img_pic.Source = RenrenSDK.publishPhoto;
            else
                return;

            if (imgCaption.Length>140)
            {
                string temp = imgCaption.Substring(0,140);
                tbx_caption.Text = temp;
            }
            tbx_caption.Text = imgCaption;
            img_head.Source = new BitmapImage(new Uri(RenrenSDK.RenrenInfo.detailInfo.tinyurl));
            tb_id.Text = RenrenSDK.RenrenInfo.detailInfo.name;
        }
        
        public void renren_GetUserInfoCompletedHandler(object sender, GetUsersCompletedEventArgs e)
        {
            img_head.Source = new BitmapImage(new Uri(e.Result.User_List[0].tinyurl));
            tb_id.Text = e.Result.User_List[0].name;
        }

        //横竖屏响应
        void FastUploadPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (Orientation == PageOrientation.PortraitUp || Orientation == PageOrientation.PortraitDown)
            {
                Grid.SetColumn(tbx_caption, 0);
                Grid.SetColumnSpan(tbx_caption, 3);
                Grid.SetRow(tbx_caption, 3);
                Grid.SetRowSpan(tbx_caption, 1);
                Grid.SetColumn(imgPanel, 0);
                Grid.SetRow(imgPanel,4);
                Grid.SetColumnSpan(imgPanel, 3);
                Grid.SetRowSpan(imgPanel, 1);
                Grid.SetRow(countPanel, 2);
                outborder.Margin = new Thickness(31, 9, 31, 20);
                middleborder.Margin = new Thickness(32, 10, 32, 21);
                innerborder.Margin = new Thickness(33, 11, 33, 22);
                img_pic.Margin = new Thickness(34, 12, 34, 23);
            }
            else
            {
                Grid.SetColumn(tbx_caption, 1);
                Grid.SetRow(tbx_caption, 2);
                Grid.SetRowSpan(tbx_caption, 2);
                Grid.SetColumn(imgPanel, 1);
                Grid.SetColumnSpan(imgPanel, 1);
                Grid.SetRowSpan(imgPanel, 2);
                Grid.SetRow(countPanel, 1);
                outborder.Margin = new Thickness(31, 9, 31, 36);
                middleborder.Margin = new Thickness(32, 10, 32, 37);
                innerborder.Margin = new Thickness(33, 11, 33, 38);
                img_pic.Margin = new Thickness(34, 12, 34, 39);
            }
        }
        //upload button
        private void upload_Click(object sender, RoutedEventArgs e)
        {
            canGoBack = false;
            tb_uploading.Visibility = System.Windows.Visibility.Visible;
            uploadingBar.IsIndeterminate = true;
            disableRect.Visibility = System.Windows.Visibility.Visible;

            if (uploadBS == null)
            {
                uploadBS = new UploadPhotoBS();
            }
            uploadBS.UploadCompleted -= UphotPhoto_DownloadStringCompleted;
            uploadBS.UploadCompleted += UphotPhoto_DownloadStringCompleted;

            uploadBS.UploadPhoto((BitmapImage)img_pic.Source, imgPath, imgCaption, 0);
        }
        //上传回调
        private void UphotPhoto_DownloadStringCompleted(object sender,
             RenrenSDKLibrary.UploadPhotoCompletedEventArgs e)
        {
            if (Orientation == PageOrientation.PortraitDown || Orientation == PageOrientation.PortraitUp)
                SupportedOrientations = SupportedPageOrientation.Portrait;
            else
                SupportedOrientations = SupportedPageOrientation.Landscape;
            tb_uploading.Visibility = System.Windows.Visibility.Collapsed;
            uploadingBar.IsIndeterminate = false;
            disableRect.Visibility = System.Windows.Visibility.Collapsed;
            if (e.Error != null)
            {
                MessageBox.Show("上传失败");
            }
            else
            {
                MessageBox.Show("上传成功");
            }
            NavigationService.GoBack();
        }


        //cancel button
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
        
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (!canGoBack)
                return;
            else
                NavigationService.GoBack();
               
        }
        private void tbx_caption_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbx_caption.Text.Length > 140)
            {
                tbx_caption.Text = tbx_caption.Text.Substring(0, 140);
                tbx_caption.SelectionStart = 140;
                MessageBoxResult result = MessageBox.Show("描述文字不能超过140字！");
            }
            imgCaption = tbx_caption.Text;
        }
    }
}
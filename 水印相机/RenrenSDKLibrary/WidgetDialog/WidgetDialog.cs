//  Copyright 2011年 Renren Inc. All rights reserved.
//  - Powered by Team Pegasus. -

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
using Microsoft.Phone.Controls;
using RenrenSDKLibrary.Controls;

namespace RenrenSDKLibrary.WidgetDialog
{
    public class WidgetDialog
    {
        #region Members
        protected BrowserControl browserControl;
        protected UIElement parentControl;
        protected PhoneApplicationPage parentPage;
        #endregion

        #region event
        /// <summary>
        /// Event handler for DownloadStringCompleted event.
        /// </summary>
        /// <param name="sender">Object firing the event.</param>
        /// <param name="e">Argument holding the data downloaded.</param>
        public delegate void DownloadStringCompletedHandler(object sender, DownloadStringCompletedEventArgs e);
        /// <summary>
        /// Occurs when an asynchronous resource-download operation is completed.
        /// </summary>
        public event DownloadStringCompletedHandler DownloadStringCompleted;
        #endregion

        /// <summary>
        /// clean event
        /// </summary>
        public void CleanDownloadStringEvent()
        {
            DownloadStringCompleted = null;
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        /// <param name="page"></param>
        public void InitView(PhoneApplicationPage page)
        {
            page.SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;
            page.Orientation = PageOrientation.Portrait;
            page.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(page_BackKeyPress);
            page.OrientationChanged += new EventHandler<OrientationChangedEventArgs>(page_OrientationChanged);
            double w = page.ActualWidth;
            double h = page.ActualHeight;
            browserControl = new BrowserControl()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Height = h,
                Width = w
            };

            parentControl = page.Content;
            page.Content = browserControl;
            parentPage = page;
        }

        #region private function
        /// <summary>
        /// 处理屏幕方向转换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            double w = parentPage.ActualWidth;
            double h = parentPage.ActualHeight;
            browserControl.Height = h;
            browserControl.Width = w;
        }

        /// <summary>
        /// 处理回退事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void page_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            RemoveBrowser();
        }

        /// <summary>
        /// 移除浏览器控件
        /// </summary>
        protected void RemoveBrowser()
        {
            parentPage.Content = parentControl;
            parentPage.BackKeyPress -= new EventHandler<System.ComponentModel.CancelEventArgs>(page_BackKeyPress);
            parentPage.OrientationChanged -= new EventHandler<OrientationChangedEventArgs>(page_OrientationChanged);
        }

        /// <summary>
        /// 通知成功信息
        /// </summary>
        /// <param name="msg">内容</param>
        protected void NotifyMessage(string msg)
        {
            if (DownloadStringCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    DownloadStringCompleted(this, new DownloadStringCompletedEventArgs(msg));
                });
            }
            RemoveBrowser();
        }

        /// <summary>
        /// 通知失败信息
        /// </summary>
        /// <param name="msg"></param>
        protected void NotifyError(string msg)
        {
            if (DownloadStringCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    DownloadStringCompleted(this,
                       new DownloadStringCompletedEventArgs(new Exception(msg)));
                });
            }
            RemoveBrowser();
        }

        #endregion
    }
}

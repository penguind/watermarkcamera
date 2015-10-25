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

namespace RenrenSDKLibrary.Controls
{
    public partial class BrowserControl : UserControl
    {
        #region Events
        public delegate void LoadCompletedEventHandler(object sender, NavigatingEventArgs e);
        public event LoadCompletedEventHandler LoadCompleted;
        #endregion

        /// <summary>
        /// 构造
        /// </summary>
        public BrowserControl()
        {
            InitializeComponent();
            RenrenBrowser.LoadCompleted += 
                new System.Windows.Navigation.LoadCompletedEventHandler(RenrenBrowser_LoadCompleted);
        }

        public void SetUri(string uri)
        {
            RenrenBrowser.Source = new Uri(uri, UriKind.RelativeOrAbsolute);
        }

        private void RenrenBrowser_LoadCompleted(object sender, 
            System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        private void RenrenBrowser_Navigating(object sender, NavigatingEventArgs e)
        {
            if (LoadCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    LoadCompleted(this, e);
                });
            } 
        }

        private void RenrenBrowser_Navigated(object sender, 
            System.Windows.Navigation.NavigationEventArgs e)
        {

        }
    }
}

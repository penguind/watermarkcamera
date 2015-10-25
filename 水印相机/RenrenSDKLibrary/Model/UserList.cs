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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RenrenSDKLibrary
{
    public class UserList
    {
        public int Count { get; set; }
        public ObservableCollection<UserDetails> User_List { get; set; }

        public UserList()
        {
            User_List = new ObservableCollection<UserDetails>();
        }
    }
}

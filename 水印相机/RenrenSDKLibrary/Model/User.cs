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
using System.Linq;
using System.Configuration;

namespace RenrenSDKLibrary
{
    public class UserDetails
    {
        //不输入fields参数
        public int uid { get; set; }

        public string name { get; set; }

        public string tinyurl { get; set; }

        public string headurl { get; set; }

        public int zidou { get; set; }

        public int star { get; set; }

        //---------------------------不输入fields参数不返回以下内容-----------

        public string mainurl { get; set; }

        public int sex { get; set; }
        
        public int vip{get;set;}

        public string birthday { get; set; }

        public string email_hash { get; set; }

        public Hometown_location hometown_location { get; set; }

        public Work_history work_info { get; set; }

        public University_history university_info { get; set; }

        public Hs_history hs_info { get; set; }

    }

    public class Hometown_location
    {
        public string province { get; set; }
        public string city { get; set; }
    }

    public class Hs_history
    {
        public string name;
        public string grad_year;
    }

    public class University_history
    {
        public string name;
        public string year;
        public string department;
    }

    public class Work_history
    {
        public string company_name;
        public string description;
        public string start_date;
        public string end_date;
    }
}

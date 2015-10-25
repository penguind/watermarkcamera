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

namespace RenrenSDKLibrary
{
    public class Album
    {
        #region propertise
        /// <summary>
        /// 表示相册的ID
        /// </summary>      
        public int aid { get; set; }

        /// <summary>
        /// 表示相册封面的图片地址
        /// </summary>      
        public string url { get; set; }

        /// <summary>
        /// 表示相册所有者的ID
        /// </summary>      
        public int uid { get; set; }

        /// <summary>
        /// 表示相册的名字
        /// </summary>      
        public string name { get; set; }

        /// <summary>
        /// 表示相册创建的时间
        /// </summary>      
        public DateTime create_time { get; set; }

        /// <summary>
        /// 表示相册更新的时间
        /// </summary>      
        public DateTime upload_time { get; set; }

        /// <summary>
        /// 表示相册的描述
        /// </summary>      
        public string description { get; set; }

        /// <summary>
        /// 表示相册拍摄的地点
        /// </summary>     
        public string location { get; set; }
       
        /// <summary>
        /// 表示相册的大小，照片的数量
        /// </summary>      
        public int size { get; set; }

        /// <summary>
        /// 表示相册的隐私设置，有5个int值: 99(所有人),4(密码保护) 3(同网络人),1(好友), , -1(仅自己可见)
        /// </summary>      
        public int visible { get; set; }

        /// <summary>
        /// 表示相册的评论数量
        /// </summary>      
        public int comment_count { get; set; }

        /// <summary>
        /// 表示相册的类型。0.普通相册，1.头像相册，3.彩信相册，5.上传相册，7.大头贴相册，12.应用相册
        /// </summary>      
        public int type{get;set; }

        #endregion
    }
}

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
using System.IO;
using System.Text;
using System.Linq;
using Microsoft.Phone.Info;
using RenrenSDKLibrary.Constants;

namespace RenrenSDKLibrary
{
    public class UploadPhotoRequest : RenrenClient
    {
        /// <summary>
        /// 上传照片接口
        /// </summary>
        /// <param name="photo">照片</param>
        /// <param name="caption">描述</param>
        /// <param name="callback">回调</param>
        public void UploadPhoto(int albumId, string caption, string fullpath)
        {
            List<APIParameter> paras = new List<APIParameter>() { 
                new APIParameter("method",Method.UploadPhoto)
                };
            List<APIParameter> files = new List<APIParameter>() { 
                new APIParameter("upload",fullpath)
                };
            if(albumId > 0)
                paras.Add(new APIParameter("aid", albumId.ToString()));
            if(caption != null)
                paras.Add(new APIParameter("caption", caption));
            CallMethod(paras, files);
        }
    }
}

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
using System.IO;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Text;
using System.Windows.Resources; 

namespace RenrenSDKLibrary
{
    public delegate void UploadPhotoCompletedHandler(object sender, UploadPhotoCompletedEventArgs e);
    internal class UploadPhotoBS: BusinessBase
    {
        #region Members
        private const string RENREN_PHOTO_DIR_NAME = "temp";
        const double KMaxWidth = 720.0;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when an asynchronous resource-download operation is completed.
        /// </summary>
        public event UploadPhotoCompletedHandler UploadCompleted;
        #endregion

        public void CleanUploadPhotoEvent()
        {
            UploadCompleted = null;
        }

        #region Override
        /// <summary>
        /// success
        /// </summary>
        /// <param name="resultString"></param>
        public override void RequestComplete(string resultString)
        {
            Photo photo = new Photo();
            try
            {
                photo = (Photo)JsonUtility.DeserializeObj(
                    new MemoryStream(Encoding.UTF8.GetBytes(resultString)), typeof(Photo));
            }
            catch
            {
                NotifyError("encoding error");
                return;
            }


            if (UploadCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    UploadCompleted(this, new UploadPhotoCompletedEventArgs(photo));
                });  
            }
        }

        /// <summary>
        /// error
        /// </summary>
        /// <param name="errorMsg"></param>
        public override void RequestError(string errorMsg)
        {
            NotifyError(errorMsg);
        }
        #endregion

        /// <summary>
        /// 上传照片接口
        /// </summary>
        /// <param name="photo">照片</param>
        /// <param name="caption">描述</param>
        /// <param name="callback">回调</param>
        public void UploadPhoto(BitmapImage photo, string name, 
            string caption, int albumId)
        {
            DoUploadPhoto(photo, name, caption, albumId);
        }

        /// <summary>
        /// 上传照片接口
        /// </summary>
        /// <param name="photo">照片文件全路径</param>
        /// <param name="caption">描述</param>
        /// <param name="callback">回调</param>
        public void UploadPhoto(string fileName,
            string caption, int albumId)
        {
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");

            BitmapImage uploadPhoto = new BitmapImage();
            
            using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!myIsolatedStorage.FileExists(fileName)) return;

                using (IsolatedStorageFileStream fileStream =
                    myIsolatedStorage.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                {
                    uploadPhoto.SetSource(fileStream);
                    DoUploadPhoto(uploadPhoto, fileName, caption, albumId);
                }
            }
        }

        /// <summary>
        /// 检查并上传照片
        /// </summary>
        /// <param name="carPicture">需要上传的图片</param>
        /// <param name="name">图片名称</param>
        /// <param name="caption">描述</param>
        /// <param name="albumId">相册</param>
        private void DoUploadPhoto(BitmapImage carPicture, string name, 
            string caption, int albumId)
        {
            //检查图片是否存在
            if (carPicture == null)
            {
                NotifyError("请添加图片");
                return;
            }

            //检查图片是否符合要求
            if (!CheckPhoto(carPicture))
            {
                NotifyError("文件格式不符合要求");
                return;
            }
            //保存到本地一个临时文件
            string fullpath = SavePhoto(name, carPicture);
            if (fullpath != null)
            {
                UploadPhotoRequest request = new UploadPhotoRequest();
                request.DownloadStringCompleted += new RenrenClient.DownloadStringCompletedHandler(
                    DownloadStringCompleted);
                request.UploadPhoto(albumId, caption, fullpath);
            }
            else
            {
                NotifyError("文件格式不支持");
                return;
            }
        }

        /// <summary>
        /// Saves the specified photo to isolated storage using the 
        /// specified filename.
        /// </summary>
        /// <param name="fileName">文件名.</param>
        /// <param name="carPicture">图片.</param>
        /// <param name="errorCallback">错误回调</param>
        private string SavePhoto(string fileName, BitmapImage carPicture)
        {
            if (carPicture == null) return null;

            string name = GetFileName(fileName);
            if (name == null) return null;

            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    //string s = IsolatedStorageFile.GetUserStoreForApplication().GetDirectoryNames();

                    var bitmap = new WriteableBitmap(carPicture);
                    var path = System.IO.Path.Combine(RENREN_PHOTO_DIR_NAME, name);

                    if (!store.DirectoryExists(RENREN_PHOTO_DIR_NAME))
                    {
                        store.CreateDirectory(RENREN_PHOTO_DIR_NAME);
                    }

                    using (var stream = store.OpenFile(path, FileMode.Create))
                    {
                        int width = bitmap.PixelWidth;
                        int height = bitmap.PixelHeight;
                        //if (width > KMaxWidth)
                        //{
                        //    double factor = (double)width / KMaxWidth;
                        //    width = (int)(width / factor);
                        //    height = (int)(height / factor);
                        //}
                        Extensions.SaveJpeg(bitmap, stream,
                           width, height, 0, 100);
                        stream.Close();
                    }
                    return path;
                }
            }
            catch (IsolatedStorageException)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取文件名
        /// </summary>
        /// <param name="name">原始文件名称</param>
        /// <returns></returns>
        private string GetFileName(string name)
        {
            string fileName;
            if (name.EndsWith(".bmp", StringComparison.CurrentCulture))
            {
                fileName = "temp.bmp";
            }
            else if (name.EndsWith(".gif", StringComparison.CurrentCulture))
            {
                fileName = "temp.gif";
            }
            else if (name.EndsWith(".jpg", StringComparison.CurrentCulture) ||
                name.EndsWith(".jpeg", StringComparison.CurrentCulture))
            {
                fileName = "temp.jpeg";
            }
            else if (name.EndsWith(".png", StringComparison.CurrentCulture))
            {
                fileName = "temp.png";
            }
            else
            {
                return null;
            }
            return fileName;
        }

        /// <summary>
        /// 检查图片是符合上传要求
        /// </summary>
        /// <param name="carPicture">图片</param>
        /// <returns></returns>
        private bool CheckPhoto(BitmapImage carPicture)
        {
            int w = carPicture.PixelWidth;
            int h = carPicture.PixelHeight;
            if (w <= 0 || h <= 0)
                return false;

            if (w < 50 && h < 50)
                return false;
            else if (w / h > 3 || h / w > 3)
                return false;
            return true;
        }

        /// <summary>
        /// 通知失败信息
        /// </summary>
        /// <param name="msg"></param>
        private void NotifyError(string msg)
        {
            if (UploadCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    UploadCompleted(this,
                       new UploadPhotoCompletedEventArgs(new Exception(msg)));
                });  
            }
        }
    }
}

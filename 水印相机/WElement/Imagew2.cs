/**************************************************************************************
 * Imagew类
 * 对图像进行加载、处理、保存
 * 
 * 未使用
 **************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Resources;
using System.Windows.Media.Imaging;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;

namespace WaterMark
{
    class Imagew2
    {
        public bool isSaved;     //是否保存，保存过就不能再保存
        public bool isAvailable; // 图像是否可用，原图和修改后的图片皆可以用

        public string bmpLocation; // 存放导入的照片的位置
        public int height;     // 图像高度
        public int width;      // 图像宽度

        private BitmapImage sourceimage;      // 原图像
        private WriteableBitmap previewimage; // 被修改后的图像 

        // 构造函数
        public Imagew2()
        {
            sourceimage = null;
            previewimage = null;
            isSaved = false;
            isAvailable = false;
            bmpLocation = "";
            height = 0;
            width = 0;
        }

        // 使用输入流加载原图
        // 相机拍照方式使用
        public void setBitmapSource(System.IO.Stream sourcestream, string imageLocation = "")
        {
            if (sourcestream == null) return;
            else
            {
                try
                {
                    sourceimage = new BitmapImage();
                    sourceimage.SetSource(sourcestream);
                    previewimage = new WriteableBitmap(sourceimage);
                    bmpLocation = imageLocation;
                    height = sourceimage.PixelHeight; width = sourceimage.PixelWidth;
                    isAvailable = true;
                }
                catch (Exception)
                {
                    sourceimage = null;
                    previewimage = null;
                    bmpLocation = "";
                    isSaved = true;
                    isAvailable = false;
                }
            }
        }
        // 使用路径加载原图
        // 相册方式使用
        public void setBitmapSource(string imageLocation)
        {
            try
            {
                sourceimage = new System.Windows.Media.Imaging.BitmapImage
                { 
                    UriSource = new Uri(imageLocation, UriKind.Relative),
                    CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.IgnoreImageCache
                };
                previewimage = new WriteableBitmap(sourceimage);
                bmpLocation = imageLocation;
                height = sourceimage.PixelHeight; width = sourceimage.PixelWidth;
                isAvailable = true;
            }
            catch(Exception)
            {
                sourceimage = null;
                previewimage = null;
                bmpLocation = "";
                isSaved = true;
                isAvailable = false;
            }
        }

        // 获取加载的原图
        public BitmapImage getImage()
        {
            return this.sourceimage;
        }
        // 获取WriteableImage
        public WriteableBitmap getWriteableImage()
        {
            return previewimage;
        }

    }
}

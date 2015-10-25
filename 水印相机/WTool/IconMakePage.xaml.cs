using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Devices;
using System.Windows.Threading;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media;
using System.Windows.Resources;
using System.IO;
using System.Windows.Media.Imaging;
using FluxJpeg.Core;
using ImageTools;
using ImageTools.IO.Png;
using System.IO.IsolatedStorage;

namespace WaterMark.WTool
{
    public partial class IconMakePage : PhoneApplicationPage
    {

        //访问图片库
        //MediaLibrary library;
        // SystemTray 的 Process Indicator
        ProgressIndicator systemTrayPI;
        //保存锁
        bool isSavingLock = false;

        public IconMakePage()
        {
            InitializeComponent();
            systemTrayPI = new ProgressIndicator();
            systemTrayPI.IsIndeterminate = true;
            systemTrayPI.Text = "";
            systemTrayPI.IsVisible = false;
        }

        /*工具*/
        #region ToastUtils
        // 显示Toast，等待5秒后 “第二次点击”标记量变为false
        private double toastTimeout;
        int isSecondEvent = -1;
        public void showToast(string title, double timeout = 5.0, bool isWaitingSecond = false, int waitingEventID = 0)
        {
            if (title.CompareTo(toastText.Text) == 0) return; //同一个提示只需显示一次，否则会出现频繁的动画
            if (toastgrid.Visibility == System.Windows.Visibility.Visible) closeToast();
            isSecondEvent = 0; //将等待id置为0，两个事件的id不是同一种
            if (isWaitingSecond == true) isSecondEvent = waitingEventID;
            toastTimeout = (timeout > 0.0) ? timeout : 3.5;
            DispatcherTimer tmr = new DispatcherTimer();
            tmr.Interval = TimeSpan.FromSeconds(0.1);
            toastText.Text = title;
            toastgrid.Visibility = System.Windows.Visibility.Visible;
            animation_toastshow.Begin();
            tmr.Tick += toastGridTimeTick;
            tmr.Start();
        }
        void toastGridTimeTick(object sender, EventArgs e)
        {
            toastTimeout -= 0.1;
            if (toastTimeout < 0.0)
            {
                this.toastgrid.Visibility = System.Windows.Visibility.Collapsed;
                isSecondEvent = 0;
                (sender as DispatcherTimer).Stop();
            }
        }
        public void closeToast()
        {
            this.toastgrid.Visibility = System.Windows.Visibility.Collapsed;
        }
        // 调用系统托盘显示
        void systemTrayToast(string toastText)
        {
            if (toastText.Trim().Length > 0)
            {
                systemTrayPI.IsVisible = true;
                systemTrayPI.Text = toastText.Trim();
                SystemTray.SetProgressIndicator(this, systemTrayPI);
            }
        }
        void closeSystemTrayToast()
        {
            systemTrayPI.IsVisible = false;
            systemTrayPI.Text = "";
            SystemTray.SetProgressIndicator(this, systemTrayPI);
        }
        #endregion

        #region Event handlers
        // these two fields fully define the zoom state:
        private double TotalImageScale = 1d;
        private Point ImagePosition = new Point(0, 0); 
        private const double MAX_IMAGE_ZOOM = 5;
        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor; 

        /// <summary>
        /// Initializes the zooming operation
        /// </summary>
        private void OnPinchStarted(object sender, PinchStartedGestureEventArgs e)
        {
            _oldFinger1 = e.GetPosition(ImgZoom, 0);
            _oldFinger2 = e.GetPosition(ImgZoom, 1);
            _oldScaleFactor = 1;
        }

        /// <summary>
        /// Computes the scaling and translation to correctly zoom around your fingers.
        /// </summary>
        private void OnPinchDelta(object sender, PinchGestureEventArgs e)
        {
            var scaleFactor = e.DistanceRatio / _oldScaleFactor;
            if (!IsScaleValid(scaleFactor))
                return;

            var currentFinger1 = e.GetPosition(ImgZoom, 0);
            var currentFinger2 = e.GetPosition(ImgZoom, 1);

            var translationDelta = GetTranslationDelta(
                currentFinger1,
                currentFinger2,
                _oldFinger1,
                _oldFinger2,
                ImagePosition,
                scaleFactor);

            _oldFinger1 = currentFinger1;
            _oldFinger2 = currentFinger2;
            _oldScaleFactor = e.DistanceRatio;

            UpdateImageScale(scaleFactor);
            UpdateImagePosition(translationDelta);
        }

        /// <summary>
        /// Moves the image around following your finger.
        /// </summary>
        private void OnDragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            var translationDelta = new Point(e.HorizontalChange, e.VerticalChange);

            if (IsDragValid(1, translationDelta))
                UpdateImagePosition(translationDelta);
        }

        /// <summary>
        /// Resets the image scaling and position
        /// </summary>
        private void OnDoubleTap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            ResetImagePosition();
        }

        #endregion

        #region GeUtils

        /// <summary>
        /// Computes the translation needed to keep the image centered between your fingers.
        /// </summary>
        private Point GetTranslationDelta(
            Point currentFinger1, Point currentFinger2,
            Point oldFinger1, Point oldFinger2,
            Point currentPosition, double scaleFactor)
        {
            var newPos1 = new Point(
             currentFinger1.X + (currentPosition.X - oldFinger1.X) * scaleFactor,
             currentFinger1.Y + (currentPosition.Y - oldFinger1.Y) * scaleFactor);

            var newPos2 = new Point(
             currentFinger2.X + (currentPosition.X - oldFinger2.X) * scaleFactor,
             currentFinger2.Y + (currentPosition.Y - oldFinger2.Y) * scaleFactor);

            var newPos = new Point(
                (newPos1.X + newPos2.X) / 2,
                (newPos1.Y + newPos2.Y) / 2);

            return new Point(
                newPos.X - currentPosition.X,
                newPos.Y - currentPosition.Y);
        }

        /// <summary>
        /// Updates the scaling factor by multiplying the delta.
        /// </summary>
        private void UpdateImageScale(double scaleFactor)
        {
            TotalImageScale *= scaleFactor;
            ApplyScale();
        }

        /// <summary>
        /// Applies the computed scale to the image control.
        /// </summary>
        private void ApplyScale()
        {
            ((CompositeTransform)ImgZoom.RenderTransform).ScaleX = TotalImageScale;
            ((CompositeTransform)ImgZoom.RenderTransform).ScaleY = TotalImageScale;
        }

        /// <summary>
        /// Updates the image position by applying the delta.
        /// Checks that the image does not leave empty space around its edges.
        /// </summary>
        private void UpdateImagePosition(Point delta)
        {
            var newPosition = new Point(ImagePosition.X + delta.X, ImagePosition.Y + delta.Y);

            if (newPosition.X > 0) newPosition.X = 0;
            if (newPosition.Y > 0) newPosition.Y = 0;

            if ((ImgZoom.ActualWidth * TotalImageScale) + newPosition.X < ImgZoom.ActualWidth)
                newPosition.X = ImgZoom.ActualWidth - (ImgZoom.ActualWidth * TotalImageScale);

            if ((ImgZoom.ActualHeight * TotalImageScale) + newPosition.Y < ImgZoom.ActualHeight)
                newPosition.Y = ImgZoom.ActualHeight - (ImgZoom.ActualHeight * TotalImageScale);

            ImagePosition = newPosition;

            ApplyPosition();
        }

        /// <summary>
        /// Applies the computed position to the image control.
        /// </summary>
        private void ApplyPosition()
        {
            ((CompositeTransform)ImgZoom.RenderTransform).TranslateX = ImagePosition.X;
            ((CompositeTransform)ImgZoom.RenderTransform).TranslateY = ImagePosition.Y;
        }

        /// <summary>
        /// Resets the zoom to its original scale and position
        /// </summary>
        private void ResetImagePosition()
        {
            TotalImageScale = 1;
            ImagePosition = new Point(0, 0);
            ApplyScale();
            ApplyPosition();
        }

        /// <summary>
        /// Checks that dragging by the given amount won't result in empty space around the image
        /// </summary>
        private bool IsDragValid(double scaleDelta, Point translateDelta)
        {
            if (ImagePosition.X + translateDelta.X > 0 || ImagePosition.Y + translateDelta.Y > 0)
                return false;

            if ((ImgZoom.ActualWidth * TotalImageScale * scaleDelta) + (ImagePosition.X + translateDelta.X) < ImgZoom.ActualWidth)
                return false;

            if ((ImgZoom.ActualHeight * TotalImageScale * scaleDelta) + (ImagePosition.Y + translateDelta.Y) < ImgZoom.ActualHeight)
                return false;

            return true;
        }

        /// <summary>
        /// Tells if the scaling is inside the desired range
        /// </summary>
        private bool IsScaleValid(double scaleDelta)
        {
            return (TotalImageScale * scaleDelta >= 1) && (TotalImageScale * scaleDelta <= MAX_IMAGE_ZOOM);
        }

        #endregion

        #region JpgUtils
        /// <summary>
        /// 截图
        /// </summary>
        /// <param name="element">被截图的对象</param>
        /// <param name="transform">等比缩放</param>
        /// <returns></returns>
        public static byte[] GetElementImage(FrameworkElement element, Transform transform)
        {
            byte[] buffer = null;
            WriteableBitmap bitmap = new WriteableBitmap(element, transform);
            if (bitmap != null)
            {
                MemoryStream imageStream = GetImageStream(bitmap);
                buffer = new byte[imageStream.Length];
                long num = imageStream.Read(buffer, 0, (int)imageStream.Length);
            }
            return buffer;
        }
        /// <summary>
        /// 图片内存流
        /// </summary>
        /// <param name="element">被截图的对象</param>
        /// <param name="transform">等比缩放</param>
        /// <returns></returns>
        public static MemoryStream GetElementImageStream(FrameworkElement element, Transform transform)
        {
            WriteableBitmap bitmap = new WriteableBitmap(element, transform);
            if (bitmap != null)
            {
                MemoryStream imageStream = GetImageStream(bitmap);
                return imageStream;
            }
            return null;

        }
        private static MemoryStream GetImageStream(WriteableBitmap bitmap)
        {
            return EncodeRasterInformationToStream(ReadRasterInformation(bitmap), ColorSpace.RGB);
        }
        private static MemoryStream EncodeRasterInformationToStream(byte[][,] raster, ColorSpace colorSpace)
        {
            ColorModel model2 = new ColorModel();
            model2.colorspace = colorSpace;
            ColorModel model = model2;
            FluxJpeg.Core.Image image = new FluxJpeg.Core.Image(model, raster);
            MemoryStream stream = new MemoryStream();
            FluxJpeg.Core.Encoder.JpegEncoder encoder = new FluxJpeg.Core.Encoder.JpegEncoder(image, 100, stream);
            encoder.Encode();
            stream.Seek(0L, SeekOrigin.Begin);
            return stream;
        }
        private static byte[][,] ReadRasterInformation(WriteableBitmap bitmap)
        {
            int num = bitmap.PixelWidth;
            int num2 = bitmap.PixelHeight;
            int num3 = 3;
            byte[][,] bufferArray = new byte[num3][,];
            for (int i = 0; i < num3; i++)
            {
                bufferArray[i] = new byte[num, num2];
            }
            for (int j = 0; j < num2; j++)
            {
                for (int k = 0; k < num; k++)
                {
                    int num7 = bitmap.Pixels[(num * j) + k];
                    bufferArray[0][k, j] = (byte)(num7 >> 0x10);
                    bufferArray[1][k, j] = (byte)(num7 >> 8);
                    bufferArray[2][k, j] = (byte)num7;
                }
            }
            return bufferArray;
        }

        #endregion


        #region test
        private void testPNG(Stream sImage)
        {
            ExtendedImage ei = new ExtendedImage();
            ei.SetSource(sImage);
            ImageTools.IO.Decoders.AddDecoder<PngDecoder>();
            ImageTools.IO.Png.PngEncoder pe = new PngEncoder();
        }
        #endregion

        ///按键的响应事件
        //确认
        private void confirmButton_Click(object sender, EventArgs e)
        {
            if (isSavingLock == false)
            {
                string filename = DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute + DateTime.Now.Second + ".jpg";
                try
                {
                    isSavingLock = true;
                    systemTrayToast("正在保存，请稍后...");
                    try
                    {
                        using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (iso.DirectoryExists("myIcons") == false)
                            {
                                iso.CreateDirectory("myIcons");
                            }
                            using (IsolatedStorageFileStream isostream = iso.CreateFile("myIcons\\"+ filename))
                            {
                                BitmapImage bmp = new BitmapImage();
                                bmp.SetSource(GetElementImageStream(saveGrid, null));
                                WriteableBitmap wb = new WriteableBitmap(bmp);  
                                Extensions.SaveJpeg(wb, isostream, wb.PixelWidth, wb.PixelHeight, 0, 100); 
                            }
                        }
                    }
                    catch (Exception)
                    {

                    }
                    //MediaLibrary library = new MediaLibrary();
                    //Picture pic = library.SavePicture("icon_" + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" + DateTime.Now.Minute+".jpg", GetElementImageStream(saveGrid, null));
                    //showToast("图片保存成功");
                    closeSystemTrayToast();
                    string fileURI = "myIcons\\" + filename;
                    using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (IsolatedStorageFileStream filestream = file.OpenFile(fileURI, FileMode.Open, FileAccess.Read))
                        {
                            var bmp = new BitmapImage();
                            bmp.SetSource(filestream);
                            testIMG.Source = bmp; 
                        }

                    }
                }
                catch (Exception)
                {
                    showToast("保存失败");
                }
                finally
                {
                    isSavingLock = false;
                }
            } 
        }

        private void albumButton_Click(object sender, EventArgs e)
        {

        }

        private void photoButton_Click(object sender, EventArgs e)
        {

        } 
    }
}
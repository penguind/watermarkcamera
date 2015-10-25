using System;
using System.IO;
using System.IO.IsolatedStorage;
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
using System.Windows.Resources;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Devices; 
using System.Device.Location;
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Info;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using WaterMark;
using 水印相机;

namespace WaterMark.Funs
{
    public partial class RealTimeCameraPage : PhoneApplicationPage
    {
        private int savedCounter = 0;
        PhotoCamera cam;
        MediaLibrary library = new MediaLibrary();
        //图像 
        WriteableBitmap wb;
        int cheight = 682;
        int cwidth = 480;
        int bheight = 0;
        int bwidth = 0;
        List<Object> elementList;
        //天气和位置
        WeatherSetting m_weather; 
        // Holds the current resolution index.
        int currentResIndex = 0;

        public RealTimeCameraPage()
        {
            InitializeComponent();
            wb = null;
            focusBrackets.Text = "┌   ┐\n\n└   ┘";
            systemTrayPI = new ProgressIndicator();
            m_weather = new WeatherSetting();
            m_weather.autoLocating();
            elementList = new List<object>();
        }
        //Code for initialization, capture completed, image availability events; also setting the source for the viewfinder.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {

            // Check to see if the camera is available on the device.
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                 (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
            {
                // Initialize the camera, when available.
                if (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing))
                {
                    // Use front-facing camera if available.
                    cam = new Microsoft.Devices.PhotoCamera(CameraType.FrontFacing);
                }
                else
                {
                    // Otherwise, use standard camera on back of device.
                    cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                }

                // Event is fired when the PhotoCamera object has been initialized.
                cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);

                // Event is fired when the capture sequence is complete.
                cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_CaptureCompleted);

                // Event is fired when the capture sequence is complete and an image is available.
                cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);

                // Event is fired when the capture sequence is complete and a thumbnail image is available.
                cam.CaptureThumbnailAvailable += new EventHandler<ContentReadyEventArgs>(cam_CaptureThumbnailAvailable);

                // The event is fired when auto-focus is complete.
                cam.AutoFocusCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_AutoFocusCompleted);

                // The event is fired when the viewfinder is tapped (for focus).
                cameraCanvas.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(focus_Tapped);

                // The event is fired when the shutter button receives a half press.
                CameraButtons.ShutterKeyHalfPressed += OnButtonHalfPress;

                // The event is fired when the shutter button receives a full press.
                CameraButtons.ShutterKeyPressed += OnButtonFullPress;

                // The event is fired when the shutter button is released.
                CameraButtons.ShutterKeyReleased += OnButtonRelease;

                //Set the VideoBrush source to the camera.
                viewfinderBrush.SetSource(cam);
            }
            else
            {
                showToast("无法加载相机设备");
                //// The camera is not supported on the device.
                //this.Dispatcher.BeginInvoke(delegate()
                //{
                //    // Write message.
                //    toastText.Text = "A Camera is not available on this device.";
                //});
                 
            }
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                // Dispose camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();

                // Release memory, ensure garbage collection.
                cam.Initialized -= cam_Initialized;
                cam.CaptureCompleted -= cam_CaptureCompleted;
                cam.CaptureImageAvailable -= cam_CaptureImageAvailable;
                cam.CaptureThumbnailAvailable -= cam_CaptureThumbnailAvailable;
                cam.AutoFocusCompleted -= cam_AutoFocusCompleted;
                CameraButtons.ShutterKeyHalfPressed -= OnButtonHalfPress;
                CameraButtons.ShutterKeyPressed -= OnButtonFullPress;
                CameraButtons.ShutterKeyReleased -= OnButtonRelease;
            }
        }
        /**********************************相机操作****************************************************/
        // Update the UI if initialization succeeds.
        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    flashModeChangeToast();
                });
                if(cam.AvailableResolutions.First<Size>()!=null)
                    cam.Resolution = cam.AvailableResolutions.First<Size>();
            }
            else
            {
                showToast("相机初始化失败");
            }
        }
        void flashModeChangeToast()
        {
            string flashModeStr = "";
            if (cam.FlashMode == FlashMode.On) flashModeStr = "闪光灯:开";
            else if (cam.FlashMode == FlashMode.Off) flashModeStr = "闪光灯:关";
            else if (cam.FlashMode == FlashMode.RedEyeReduction) flashModeStr = "闪光灯:红眼";
            else flashModeStr = "闪光灯:自动";
            showToast(flashModeStr);
        }

        // Ensure that the viewfinder is upright in LandscapeRight.
        // 手机旋转，切换视窗
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (cam != null)
            {
                // LandscapeRight rotation when camera is on back of device.
                int landscapeRightRotation = 180;

                // Change LandscapeRight rotation for front-facing camera.
                if (cam.CameraType == CameraType.FrontFacing)
                {
                    landscapeRightRotation = -180;
                    cheight = 682;
                    cwidth = 480;
                }

                // Rotate video brush from camera.
                if (e.Orientation == PageOrientation.LandscapeRight)
                {
                    // Rotate for LandscapeRight orientation.
                    cwidth = 682;
                    cheight = 480;
                    viewfinderBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = landscapeRightRotation };
                }
                else
                {
                    cheight = 682;
                    cwidth = 480;
                    // Rotate for standard landscape orientation.
                    viewfinderBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 0 };
                }
                previewCanvas();
            }

            base.OnOrientationChanged(e);
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            if (cam != null)
            {
                try
                {
                    // Start image capture.
                    cam.CaptureImage();
                }
                catch (Exception)
                {
                }
            }
        }

        void cam_CaptureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            // Increments the savedCounter variable used for generating JPEG file names.
            savedCounter++;
        }


        // Informs when full resolution picture has been taken, saves to local media library and isolated storage.
        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            string fileName = savedCounter + ".jpg";

            try
            {   // Write message to the UI thread.
                //Deployment.Current.Dispatcher.BeginInvoke(delegate()
                //{
                //    toastText.Text = "Captured image available, saving picture.";
                //});

                // Save picture to the library camera roll.
                library.SavePictureToCameraRoll(fileName, e.ImageStream); //保存原图到相册
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(e.ImageStream);
                wb = new WriteableBitmap(bmp);

                // Write message to the UI thread.
                //Deployment.Current.Dispatcher.BeginInvoke(delegate()
                //{
                //    toastText.Text = "Picture has been saved to camera roll.";

                //});

                // Set the position of the stream back to start
                e.ImageStream.Seek(0, SeekOrigin.Begin);

                // Save picture as JPEG to isolated storage.
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the image to isolated storage. 
                        while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }

                // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    toastText.Text = "Picture has been saved to isolated storage.";

                });
            }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }

        }

        // Informs when thumbnail picture has been taken, saves to isolated storage
        // User will select this image in the pictures application to bring up the full-resolution picture. 
        public void cam_CaptureThumbnailAvailable(object sender, ContentReadyEventArgs e)
        {
            string fileName = savedCounter + "_th.jpg";

            try
            {
                // Write message to UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    toastText.Text = "Captured image available, saving thumbnail.";
                });

                // Save thumbnail as JPEG to isolated storage.
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the thumbnail to isolated storage. 
                        while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }

                // Write message to UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    toastText.Text = "Thumbnail has been saved to isolated storage.";

                });
            }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }
        }

        // Activate a flash mode.
        // Cycle through flash mode options when the flash button is pressed.
        private void changeFlash_Clicked(object sender, RoutedEventArgs e)
        { 
            switch (cam.FlashMode)
            {
                case FlashMode.Off:
                    if (cam.IsFlashModeSupported(FlashMode.On))
                    {
                        // Specify that flash should be used.
                        cam.FlashMode = FlashMode.On;  
                    }
                    break;
                case FlashMode.On:
                    if (cam.IsFlashModeSupported(FlashMode.RedEyeReduction))
                    {
                        // Specify that the red-eye reduction flash should be used.
                        cam.FlashMode = FlashMode.RedEyeReduction;  
                    }
                    else if (cam.IsFlashModeSupported(FlashMode.Auto))
                    {
                        // If red-eye reduction is not supported, specify automatic mode.
                        cam.FlashMode = FlashMode.Auto;  
                    }
                    else
                    {
                        // If automatic is not supported, specify that no flash should be used.
                        cam.FlashMode = FlashMode.Off;  
                    }
                    break;
                case FlashMode.RedEyeReduction:
                    if (cam.IsFlashModeSupported(FlashMode.Auto))
                    {
                        // Specify that the flash should be used in the automatic mode.
                        cam.FlashMode = FlashMode.Auto;  
                    }
                    else
                    {
                        // If automatic is not supported, specify that no flash should be used.
                        cam.FlashMode = FlashMode.Off;  
                    }
                    break;
                case FlashMode.Auto:
                    if (cam.IsFlashModeSupported(FlashMode.Off))
                    {
                        // Specify that no flash should be used.
                        cam.FlashMode = FlashMode.Off;  
                    }
                    break;
            } 
            // Display current flash mode.
            this.Dispatcher.BeginInvoke(delegate()
            {
                flashModeChangeToast();
            });
        }

        // Provide auto-focus in the viewfinder.
        private void focus_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (cam.IsFocusSupported == true)
            {
                //Focus when a capture is not in progress.
                try
                {
                    cam.Focus();
                }
                catch (Exception focusError)
                {
                    // Cannot focus when a capture is in progress.
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        toastText.Text = focusError.Message;
                        
                    });
                }
            }
            else
            {
                // Write message to UI.
                this.Dispatcher.BeginInvoke(delegate()
                {
                    showToast("无法自动对焦");
                });
            }
        }

        void cam_AutoFocusCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                // Write message to UI.
                //toastText.Text = "Auto focus has completed.";

                // Hide the focus brackets.
                focusBrackets.Visibility = Visibility.Collapsed;

            });
        }

        // Provide touch focus in the viewfinder.
        void focus_Tapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (cam != null)
            {
                if (cam.IsFocusAtPointSupported == true)
                {
                    try
                    {
                        // Determine location of tap.
                        Point tapLocation = e.GetPosition(cameraCanvas);

                        // Position focus brackets with estimated offsets.
                        focusBrackets.SetValue(Canvas.LeftProperty, tapLocation.X - 30);
                        focusBrackets.SetValue(Canvas.TopProperty, tapLocation.Y - 28);

                        // Determine focus point.
                        double focusXPercentage = tapLocation.X / cameraCanvas.Width;
                        double focusYPercentage = tapLocation.Y / cameraCanvas.Height;

                        // Show focus brackets and focus at point
                        focusBrackets.Visibility = Visibility.Visible;
                        cam.FocusAtPoint(focusXPercentage, focusYPercentage);

                        // Write a message to the UI.
                        this.Dispatcher.BeginInvoke(delegate()
                        {
                            toastText.Text = String.Format("Camera focusing at point: {0:N2} , {1:N2}", focusXPercentage, focusYPercentage);
                        });
                    }
                    catch (Exception focusError)
                    {
                        // Cannot focus when a capture is in progress.
                        this.Dispatcher.BeginInvoke(delegate()
                        {
                            // Write a message to the UI.
                            toastText.Text = focusError.Message;
                            // Hide focus brackets.
                            focusBrackets.Visibility = Visibility.Collapsed;
                        });
                    }
                }
                else
                {
                    // Write a message to the UI.
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        toastText.Text = "Camera does not support FocusAtPoint().";
                    });
                }
            }
        }

        private void changeRes_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            // Variables
            IEnumerable<Size> resList = cam.AvailableResolutions;
            int resCount = resList.Count<Size>();
            Size res;

            // Poll for available camera resolutions.
            for (int i = 0; i < resCount; i++)
            {
                res = resList.ElementAt<Size>(i);
            }

            // Set the camera resolution.
            res = resList.ElementAt<Size>((currentResIndex + 1) % resCount);
            cam.Resolution = res;
            currentResIndex = (currentResIndex + 1) % resCount;

            // Update the UI.
            toastText.Text = String.Format("Setting capture resolution: {0}x{1}", res.Width, res.Height); 
        }


        // Provide auto-focus with a half button press using the hardware shutter button.
        private void OnButtonHalfPress(object sender, EventArgs e)
        {
            if (cam != null)
            {
                // Focus when a capture is not in progress.
                try
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        toastText.Text = "Half Button Press: Auto Focus";
                    });

                    cam.Focus();
                }
                catch (Exception focusError)
                {
                    // Cannot focus when a capture is in progress.
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        toastText.Text = focusError.Message;
                    });
                }
            }
        }

        // Capture the image with a full button press using the hardware shutter button.
        private void OnButtonFullPress(object sender, EventArgs e)
        {
            if (cam != null)
            {
                cam.CaptureImage();
            }
        }

        // Cancel the focus if the half button press is released using the hardware shutter button.
        private void OnButtonRelease(object sender, EventArgs e)
        {

            if (cam != null)
            {
                cam.CancelFocus();
            }
        }

        /***********************************工具函数***********************************/
        ///元素添加
        // 将元素添加到图画中
        void addItemToBMP(Object element)
        {
            TranslateTransform trans = new TranslateTransform();
            if (element is WTextBlock) //添加文字
            {
                WTextBlock wtb = (WTextBlock)element;
                trans.X = wtb.x;
                trans.Y = wtb.y;
                wb.Render(wtb.tb, trans);
            }
            else if (element is Shapw) //添加图形
            {
                Shapw wtb = (Shapw)element;
                trans.X = wtb.x;
                trans.Y = wtb.y;
                switch (wtb.wshapetype)
                {
                    case Shapw.wShapes.Ellipse:
                        {
                            Ellipse shape = (Ellipse)wtb.shape;
                            wb.Render(shape, trans);
                            break;
                        }
                    case Shapw.wShapes.Line: break;
                    case Shapw.wShapes.Rectangle:
                        {
                            Rectangle shape = (Rectangle)wtb.shape;
                            wb.Render(shape, trans);
                            break;
                        }
                    default: break;
                }
            }
            else if (element is Imagew)
            {
                Imagew wimage = (Imagew)element;
                trans.X = wimage.x;
                trans.Y = wimage.y;
                Image pic = new Image();
                BitmapImage bmpic = null;
                if (wimage.location.Contains("/WaterMark;component") || wimage.location.Contains("..\\"))//资源图片 
                {
                    bmpic = new BitmapImage(new Uri(wimage.location, UriKind.Relative));
                }
                else if (wimage.location.ToLower().Contains("http"))//网络图片
                {
                    bmpic = new BitmapImage();
                    return;
                }
                pic.Source = bmpic;
                pic.Width = wimage.width;
                pic.Height = wimage.height;
                pic.Stretch = wimage.picstretch;
                wb.Render(pic, trans);
            }
            else { }
        }
        void addItemToCanvas(Object element)
        {
            TranslateTransform trans = new TranslateTransform();
            if (element is WTextBlock) //添加文字
            { 
                this.cameraCanvas.Children.Add(((WTextBlock)element).tb);
            }
            else if (element is Shapw) //添加图形
            {
                Shapw wtb = new Shapw( (Shapw)element ); 
                switch (wtb.wshapetype)
                {
                    case Shapw.wShapes.Ellipse:
                        { 
                            this.cameraCanvas.Children.Add((Ellipse)wtb.shape);
                            break;
                        }
                    case Shapw.wShapes.Line: break;
                    case Shapw.wShapes.Rectangle:
                        {
                            this.cameraCanvas.Children.Add((Rectangle)wtb.shape); 
                            break;
                        }
                    default: break;
                }
            }
            else if (element is Imagew)
            {
                Imagew wimage = (Imagew)element;
                trans.X = wimage.x;
                trans.Y = wimage.y;
                Image pic = new Image();
                BitmapImage bmpic = null;
                if (wimage.location.Contains("/WaterMark;component") || wimage.location.Contains("..\\"))//资源图片 
                {
                    bmpic = new BitmapImage(new Uri(wimage.location, UriKind.Relative));
                }
                else if (wimage.location.ToLower().Contains("http"))//网络图片
                {
                    bmpic = new BitmapImage();
                    return;
                }
                pic.Source = bmpic;
                pic.Width = wimage.width;
                pic.Height = wimage.height;
                pic.Stretch = wimage.picstretch;
                wb.Render(pic, trans);
            }
            else { }
        }
        // 将图片加载到图片上_可以被Imagew代替==只被用于添加图章
        private void printIcon(double height, double width, double x, double y, string piclocation = "")
        {
            TranslateTransform transicon = new TranslateTransform();
            Image pic = null;
            string location = piclocation;
            if (location.Trim().Length == 0)// && (isIconAvailable == false || iconUser == null))
            {
                pic = new Image();
                BitmapImage bmpic = new BitmapImage(new Uri("/WaterMark;component/Assets2/icons/emotions/Smile2.png", UriKind.Relative));
                location = "/WaterMark;component/Assets2/icons/emotions/Smile2.png";
                if (noWarning == false) showToast("先在菜单中加载图章才能显示图章");
            }
            if (location.Trim().Length == 0) //加载图章
            {
                //pic = iconUser;
            }
            else
            {
                pic = new Image();
                BitmapImage bmpic = null;
                if (location.Contains("/WaterMark;component") || location.Contains("..\\"))//资源图片 
                {
                    bmpic = new BitmapImage(new Uri(location, UriKind.Relative));
                }
                else if (location.ToLower().Contains("http"))//网络图片,未完成
                {
                    bmpic = new BitmapImage();
                    return;
                }
                pic.Source = bmpic;
            }
            pic.Width = width;
            pic.Height = height;
            pic.Stretch = Stretch.Uniform;
            transicon.X = x;
            transicon.Y = y;
            wb.Render(pic, transicon);
        }
        //// 清除已添加的元素
        //private bool clearItemOnBMP(bool closeDevice = true)
        //{
        //    if (bmp == null) return false;
        //    if (closeDevice == true) { closeDevices(); }//清楚被占用的设备
        //    if (wb_source == null) wb_source = new WriteableBitmap(bmp);
        //    wb = new WriteableBitmap(wb_source);
        //    elementList.Clear();
        //    return true;
        //}
        private bool clearItemOnCanvas(bool closeDevice = true)
        {
            cameraCanvas.Children.Clear();
            return true;
        }
        Color fontColor = Colors.White;
        private void template1Preview()
        { 
            //添加水印文字--时间
            string time_text = string.Format("{0:D4}.{1:D2}.{2:D2} {3:D2}:{4:D2}", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute);
            double fontSize0 = (bheight * 0.05 * time_text.Length > bwidth ? bwidth / (time_text.Length + 2) : bheight * 0.05);
            double yPoint = bheight * 0.95;
            double xPoint = bwidth * 0.99  ;
            if (xPoint < 0) xPoint = fontSize0 * 8 + bwidth * 0.17; //时间约长 8 个汉字
            addItemToCanvas(new WTextBlock(bwidth * 0.17, yPoint -= fontSize0 * 1.5, "学长很忙", fontSize0, ".\\Fonts\\SentyMarukoElementary.ttf#SentyMARUKO-Elementary"/* "Segoe WP"*/, 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            //时间
            addItemToCanvas(new WTextBlock(bwidth * 0.17, yPoint -= fontSize0 * 1.2, time_text, fontSize0, ".\\Fonts\\SentyMarukoElementary.ttf#SentyMARUKO-Elementary"/* "Segoe WP"*/, 0, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            //添加城市
            double fontSize1 = fontSize0 * 1.5;
            if (m_weather.cityName.Trim().Length > 0)
            {
                addItemToCanvas(new WTextBlock(bwidth * 0.24, yPoint -= fontSize1 * 1.1, m_weather.cityName, fontSize1, ".\\Fonts\\SentyMarukoElementary.ttf#SentyMARUKO-Elementary"/* "Segoe WP"*/, 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            }
            else
            {
                addItemToCanvas(new WTextBlock(bwidth * 0.24, yPoint -= fontSize1 * 1.1, "好地方", fontSize0, ".\\Fonts\\SentyMarukoElementary.ttf#SentyMARUKO-Elementary"/* "Segoe WP"*/, 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            }
            //this.cameraCanvas.Children.Add(new Image(){Source = 
            printIcon(fontSize1, fontSize1 * 1.5, bwidth * 0.16, yPoint + fontSize1 * 0.2, "/WaterMark;component/Assets2/icons/location/LocationMarker.png");
            printIcon(bwidth * 0.16, bwidth * 0.16, bwidth * 0.005, bheight * 0.95 - bwidth * 0.16, m_weather.getMarkPic());

        }
        /// 选择模版
        // 将模板序号对应模板的id
        void setTemplatePreview(bool isSaving = false, bool showTemplateName = true) //isSaving 有时需要解决同步的问题
        {
            if (clearItemOnCanvas() == false) return;
            //int chooseNum = templateCount % (TemplateAmount + 1);
            //if (showTemplateName == true) printTemplateName();
            //showProcessStep(chooseNum);
            //if (chooseNum == 0) { timeTemplatePreview(); }
            //else if (chooseNum == 1) { template1Preview(); }
            //else if (chooseNum == 2) { template2Preview(isSaving); }
            //else if (chooseNum == 3) { template3Preview(); }
            //else if (chooseNum == 4) { template4Preview(); }
            //else if (chooseNum == 5) { template5Preview(); }
            //else if (chooseNum == 6) { template6Preview(); }
            //else if (chooseNum == 7) { template7Preview(); }
            //else if (chooseNum == 8) { template8Preview(); }
            //else { timeTemplatePreview(); chooseNum = 0; }
            template1Preview();
        }
        //// 手势 切换模板
        //private void imageGesture_DragDelta(object sender, DragDeltaGestureEventArgs e)
        //{
        //    if (backFlag != 1)
        //    {
        //        showToast("请先拍摄或选择照片，再使用模板");
        //        return;
        //    }
        //    if (e.HorizontalChange > 15)
        //    {
        //        if ((e.VerticalChange > 0 && e.HorizontalChange > e.VerticalChange) || (e.VerticalChange <= 0 && e.HorizontalChange + e.VerticalChange > 0))
        //        { gestInt = 1; }
        //    }
        //    else if (e.HorizontalChange < -15)
        //    {
        //        if ((e.VerticalChange > 0 && e.HorizontalChange + e.VerticalChange < 0) || (e.VerticalChange <= 0 && e.VerticalChange > e.HorizontalChange))
        //        { gestInt = -1; }
        //    }
        //    //else gestInt = 0;
        //}
        //private void imageGesture_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        //{
        //    if (isLastSavedPic == true || isTemplateSlide == false || m_photo.Visibility == System.Windows.Visibility.Collapsed)//已被保存或处于非可切换状态时
        //    {
        //        return;
        //    }
        //    if (m_weather == null)
        //    {
        //        if (loadpauseGrid.Visibility == System.Windows.Visibility.Visible) return;
        //        m_weather = new WeatherSetting();
        //        this.m_weather.autoLocating();
        //        loadInfoGrid();
        //        return;
        //    }
        //    if (gestInt > 0)
        //    {
        //        --templateCount;//模板个数减一
        //        if (templateCount <= 0) templateCount += TemplateAmount + 1;
        //    }
        //    else if (gestInt < 0)
        //    {
        //        templateCount++;
        //    }
        //    setTemplatePreview();
        //    if (gestInt > 0)
        //    {
        //        animation_m_photo_right.Begin();
        //    }
        //    else if (gestInt < 0)
        //    {
        //        animation_m_photo_left.Begin();
        //    }
        //    gestInt = 0;
        //}
        //private void imageGesture_DragStarted(object sender, DragStartedGestureEventArgs e)
        //{
        //    gestInt = 0;
        //}
        //private void imageGesture_DoubleTap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        //{

        //}
        //// 模板选择栏--与手势切换重叠，会暂停手势切换
        //private void templateSelectNameGrid_LostFocus(object sender, RoutedEventArgs e)
        //{ }
        //private void selectNameButtonClick(object sender, RoutedEventArgs e)
        //{
        //    if (isLastSavedPic == true || m_photo.Visibility == System.Windows.Visibility.Collapsed)//已被保存或处于非可切换状态时
        //    {
        //        showToast("请新建照片");
        //        return;
        //    }
        //    if (m_weather == null)
        //    {
        //        if (loadpauseGrid.Visibility == System.Windows.Visibility.Visible) return;
        //        m_weather = new WeatherSetting();
        //        this.m_weather.autoLocating();
        //        loadInfoGrid();
        //        return;
        //    }
        //    if (sender.Equals(templateButton0)) templateCount = 0;
        //    else if (sender.Equals(templateButton1)) templateCount = 1;
        //    else if (sender.Equals(templateButton2)) templateCount = 2;
        //    else if (sender.Equals(templateButton3)) templateCount = 3;
        //    else if (sender.Equals(templateButton4)) templateCount = 4;
        //    else if (sender.Equals(templateButton5)) templateCount = 5;
        //    else if (sender.Equals(templateButton6)) templateCount = 6;
        //    else if (sender.Equals(templateButton7)) templateCount = 7;
        //    else if (sender.Equals(templateButton8)) templateCount = 8;
        //    else templateCount = 0;
        //    templateSelectNameGrid.Visibility = System.Windows.Visibility.Collapsed;
        //    this.isTemplateSlide = true;
        //    setTemplatePreview();
        //    //animation_m_photo_both.Begin();
        //}
        //private void SelectNameTmp_Click(object sender, EventArgs e)
        //{
        //    if (m_weather == null)
        //    {
        //        if (loadpauseGrid.Visibility == System.Windows.Visibility.Visible) return;
        //        m_weather = new WeatherSetting();
        //        this.m_weather.autoLocating();
        //        loadInfoGrid();
        //        return;
        //    }
        //    if (templateSelectNameGrid.Visibility == System.Windows.Visibility.Collapsed)
        //    {
        //        templateSelectNameGrid.Visibility = System.Windows.Visibility.Visible;
        //        isTemplateSlide = false;
        //        animation_templateselectlist.Begin();
        //    }
        //    else
        //    {
        //        templateSelectNameGrid.Visibility = System.Windows.Visibility.Collapsed;
        //        isTemplateSlide = true;
        //    }
        //}

        ///提示信息
        //加载信息并前置，防止其他控件被点击 12秒+3秒错误提示
        //private double gridTimeout;
        //public void loadInfoGrid()
        //{
        //    gridTimeout = 0.0;
        //    DispatcherTimer tmr = new DispatcherTimer();
        //    tmr.Interval = TimeSpan.FromSeconds(0.1);
        //    //loadpauseGrid.Visibility = System.Windows.Visibility.Visible;
        //    this.ApplicationBar.IsVisible = false;
        //    //isTemplateSlide = false;
        //    tmr.Tick += loadGridTimeTick;
        //    tmr.Start();
        //}
        //void loadGridTimeTick(object sender, EventArgs e)
        //{
        //    gridTimeout += 0.1;
        //    if (m_weather.isInfoEnough())
        //    {
        //        this.loadpauseGrid.Visibility = System.Windows.Visibility.Collapsed;
        //        setTemplatePreview();
        //        if (this.indexPageCanvas.Visibility == System.Windows.Visibility.Collapsed) this.ApplicationBar.IsVisible = true;
        //        isTemplateSlide = true;
        //        (sender as DispatcherTimer).Stop();
        //    }
        //    if (m_weather.isError == true)
        //    {
        //        if (noWarning == false) showToast("信息加载失败.");
        //    }
        //    if (gridTimeout < 12.0)
        //    {
        //        gridTimeout += 0.1;
        //    }
        //    else
        //    {
        //        if (noWarning == false) showToast("数据未加载完，可能是网络故障");
        //        this.loadpauseGrid.Visibility = System.Windows.Visibility.Collapsed;
        //        if (this.indexPageCanvas.Visibility == System.Windows.Visibility.Collapsed) this.ApplicationBar.IsVisible = true;
        //        isTemplateSlide = true;
        //        (sender as DispatcherTimer).Stop();
        //    }
        //}
        // 显示Toast，等待5秒后 “第二次点击”标记量变为false
        private double toastTimeout;
        private bool noWarning;
        private bool isLastSavedPic;
        private int backFlag;
        private int isSecondEvent;
        public void showToast(string title, double timeout = 5.0, bool isWaitingSecond = false, int waitingEventID = 0)
        {
            if (toastgrid.Visibility == System.Windows.Visibility.Visible) closeToast();
            isSecondEvent = 0; //将等待id置为0，两个事件的id不是同一种
            if (isWaitingSecond == true) isSecondEvent = waitingEventID;
            toastTimeout = (timeout > 0.0) ? timeout : 5.0;
            DispatcherTimer tmr = new DispatcherTimer();
            tmr.Interval = TimeSpan.FromSeconds(0.1);
            toastText.Text = title;
            toastgrid.Visibility = System.Windows.Visibility.Visible;
            //animation_toastshow.Begin();
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
        ProgressIndicator systemTrayPI;
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

        // 提示模板名称
        //public void printTemplateName()
        //{
        //    if (noWarning == true) return;
        //    switch (templateCount)
        //    {
        //        case 0: showToast("左右滑动图片以切换模板", 3.1); break;
        //        case 4: showToast("图章模板", 2.0); break;
        //        case 2: showToast("分贝模板", 2.0); break;
        //        default: break;
        //    }
        //}
        //// 模板个数标记
        //private void showProcessStep(int templateNum)
        //{
        //    processStepGrid.Visibility = System.Windows.Visibility.Visible;
        //    SolidColorBrush currentBrush = new SolidColorBrush(new Color() { R = 46, G = 46, B = 46, A = 154 });
        //    SolidColorBrush waitingBrush = new SolidColorBrush(new Color() { A = 72, B = 46, G = 46, R = 46 });
        //    processStep0.Fill = waitingBrush;
        //    processStep1.Fill = waitingBrush;
        //    processStep2.Fill = waitingBrush;
        //    processStep3.Fill = waitingBrush;
        //    processStep4.Fill = waitingBrush;
        //    processStep5.Fill = waitingBrush;
        //    processStep6.Fill = waitingBrush;
        //    processStep7.Fill = waitingBrush;
        //    processStep8.Fill = waitingBrush;
        //    switch (templateNum)
        //    {
        //        case 0: processStep0.Fill = currentBrush; break;
        //        case 1: processStep1.Fill = currentBrush; break;
        //        case 2: processStep2.Fill = currentBrush; break;
        //        case 3: processStep3.Fill = currentBrush; break;
        //        case 4: processStep4.Fill = currentBrush; break;
        //        case 5: processStep5.Fill = currentBrush; break;
        //        case 6: processStep6.Fill = currentBrush; break;
        //        case 7: processStep7.Fill = currentBrush; break;
        //        case 8: processStep8.Fill = currentBrush; break;
        //        default: processStep0.Fill = currentBrush; break;
        //    }
        //}
        //private void closeProcessStep()
        //{
        //    processStepGrid.Visibility = System.Windows.Visibility.Collapsed;
        //}

        ///保存照片
        //根据List记录的文本生成预览
        //生成预览后 wb 即为目标图像，直接将其保存而无需重新绘制
        //void previewBMP()
        //{
        //    try
        //    {
        //        if (elementList.Capacity == 0)
        //        {
        //            if (noWarning == false) showToast("没有做任何修改");
        //            return;
        //        }
        //        foreach (Object element in this.elementList) // 将各个元素添加到列表中
        //        {
        //            if (element != null)
        //                addItemToBMP(element);
        //        }
        //        wb.Invalidate(); // 保存修改，并显示到UI
        //        m_photo.Source = wb;
        //        isLastSavedPic = false;
        //    }
        //    catch (Exception) { }
        //}
        void previewCanvas()
        {
            try
            {
                if (elementList.Capacity == 0)
                {
                    if (noWarning == false) showToast("没有做任何修改");
                    return;
                }
                foreach (Object element in this.elementList) // 将各个元素添加到列表中
                {
                    if (element != null)
                        addItemToBMP(element);
                }
            }
            catch (Exception) { }
        }
        // 保存WritenableBitmap
        public void savePreview()
        {
            try
            {
                if (isLastSavedPic == false && wb != null && wb_source != null) //图片没有保存
                {
                    systemTrayToast("正在保存...");
                    using (MemoryStream myFileStream = new MemoryStream())// "tempJPEG.jpg", FileMode.Create, myStore))//myStore.OpenFile("tempJEPG.jpg", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (myFileStream == null)
                        {
                            if (noWarning == false) showToast("保存失败，请稍后重试确定");
                            return;
                        }
                        //jepg编码，将wb存储成jepg
                        wb.SaveJpeg(myFileStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                        myFileStream.Seek(0, SeekOrigin.Begin);
                        MediaLibrary library = new MediaLibrary();
                        Picture pic = library.SavePicture(string.Format("water_{0}{1}{2}{3}{4}{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), myFileStream);
                        isLastSavedPic = true; 
                        closeToast();
                        backFlag = 4;//保存完成
                    }
                }
                else if (noWarning == false) showToast("新建或后退开始新的任务");
            }
            catch (Exception)
            {
                try
                {
                    using (MemoryStream myFileStream = new MemoryStream())// "tempJPEG.jpg", FileMode.Create, myStore))//myStore.OpenFile("tempJEPG.jpg", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (myFileStream == null)
                        {
                            if (noWarning == false) showToast("保存和备份失败，请稍后重试确定");
                            return;
                        }
                        wb_source.SaveJpeg(myFileStream, wb_source.PixelWidth, wb_source.PixelHeight, 0, 100);
                        myFileStream.Seek(0, SeekOrigin.Begin);
                        MediaLibrary library = new MediaLibrary();
                        Picture pic = library.SavePicture(string.Format("photo_{0}{1}{2}{3}{4}{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), myFileStream);
                        if (noWarning == false) showToast("未知原因保存失败，已备份原图");
                    }
                }
                catch (Exception) { if (noWarning == false) showToast("未知原因保存失败，原图保存失败"); }
            }
            closeSystemTrayToast();
        }
        // 按钮 确定 将水印输出到相片上，并导航到 分享页
        private void confirmMark(object sender, EventArgs e)
        {
            if (backFlag != 1 && backFlag != 4)
            {
                showToast("请先拍摄或选择照片");
                return;
            }
            if (isLastSavedPic == false)
            {
                setTemplatePreview(true); 
                savePreview(); //先保存后存到临时文件，尽量保证能存储到相册
                try
                {
                    String shareimg = "shareimg.jpg";
                    using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (iso.FileExists(shareimg))
                        {
                            iso.DeleteFile(shareimg);
                        }
                        using (IsolatedStorageFileStream isostream = iso.CreateFile(shareimg))
                        {
                            StreamResourceInfo sri = null;
                            Uri uri = new Uri(shareimg, UriKind.Relative);
                            sri = Application.GetResourceStream(uri);
                            Extensions.SaveJpeg(wb, isostream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                            isostream.Close();
                        }
                    }
                }
                catch (Exception) { showToast("图片编码失败而不能分享"); isLastSavedPic = false; return; }
            }
            NavigationService.Navigate(new Uri("../share/SharePage.xaml", UriKind.Relative));
        }
    }
}
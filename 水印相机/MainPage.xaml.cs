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
using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Info;
using Microsoft.Xna.Framework.Media;
using WaterMark;
using Microsoft.Xna.Framework.Audio;
using System.Device.Location;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Devices;
using System.Globalization; 

namespace 水印相机
{
    public partial class MainPage : PhoneApplicationPage,IDisposable
    {
        //图片选择器//相机相片启动器
        private PhotoChooserTask m_photoChooseTask; 
        //签名
        string signature;
        //显示机型
        bool showWPLogo;
        //判断是在设置图片（返回键返回到“选择”部分） 
        //1:还是在“选择”部分（返回到索引界面） 2:还是在键入自定义 3:  4:已保存的状态  5:实时相机已拍摄的状态  0:退出
        int backFlag;
        //是否是刚刚保存的图片
        bool isLastSavedPic;
        //上次设置的水印文字
        //string oldMark;
        // 是否使用简单模版的黑色背景条
        bool useBackGround;
        //字体
        //    颜色
        Color fontColor;
        Color backColor;
        //    字体大小与图片高度的百分比
        double fontSize;
        //    字体名==资源属性：如果较新则赋值，内容
        string fontName = "Segoe WP";//".\\Fonts\\SentyMarukoElementary.ttf#SentyMARUKO-Elementary";
        //FontSource m_fontSource;
        // 简单模板字体位置
        TextAlignment simpleTTextAlignment;

        //天气和位置
        WeatherSetting m_weather;
        //速度
        //private GeoCoordinateWatcher geowatcher;

        // 切换水印的计数器
        int templateCount;
        // 手势切换计数器
        int gestInt;
        // 指示是否可以切换
        bool isTemplateSlide;
        // 模板个数的常量
        public const int TemplateAmount = 15; //简单模板外的其他模板总数
        //记录模板中的inputbox的内容（如果模板需要inputbox）
        //0对应简单模板，1-TemplateAmount对应相应的模板，即使不需要input
        public string[] templateInputs; 

        //图像
        System.Windows.Media.Imaging.BitmapImage bmp;//读入的原始图像
        WriteableBitmap wb_source; //bmp保存的原始图像
        WriteableBitmap wb; //bmp编辑的动态图像
        StreamResourceInfo emptyBMPSRI;//空图片的地址
        int bheight;
        int bwidth;

        // 存储列表
        List<Object> elementList;

        //访问图片库
        MediaLibrary library;

        // 存储图章
        Image iconUser;
        bool isIconAvailable; //图章是否加载
        //图章选择器
        private PhotoChooserTask userIconChooseTask;
        private CameraCaptureTask icon_cameraCaptureTask;

        //设置
        private IsolatedStorageSettings _appSetting;
        // 是否发出错误提示的toast，true不提示
        public bool noWarning;
        // SystemTray 的 Process Indicator
        ProgressIndicator systemTrayPI;
        
        //暂时废弃
        //// 是否使用地图模板 模板6
        //private bool useMapIcon;
        //private int mapZoom;
        //private double mapHeight;
        //private double mapWidth;
        //private double mapLocationX;
        //private double mapLocationY;

        // 当有“第二次点击”事件时为对应的id，时延5秒之后变为默认的0
        // id = 1：返回 第一次触发；id = 2：未保存即拍摄 第一次；id = 3：未保存即选择
        int isSecondEvent;

        /*******************************启动设备和页面初始化*******************************/
        // 构造函数
        public MainPage()
        { 
            InitializeComponent();
            indexADControl.CountryOrRegion = RegionInfo.CurrentRegion.TwoLetterISORegionName;
            fontColor = new Color() { A = 255, B = 255, G = 255, R = 255 };
            backColor = new Color { A = 255, B = 255, R = 49, G = 189 };
            noWarning = true;
            isTemplateSlide = false;
            iconUser = new Image();
            library = new MediaLibrary(); 
            isIconAvailable = false; 
            templateCount = 0;
            //FontFamily xiaowanziFont = new System.Windows.Media.FontFamily(".\\Fonts\\SentyMarukoElementary.ttf#SentyMARUKO-Elementary");
            loadDefaultText();
            //geowatcher = new GeoCoordinateWatcher();
            systemTrayPI = new ProgressIndicator();
            systemTrayPI.IsIndeterminate = true;
            systemTrayPI.Text = "";
            systemTrayPI.IsVisible = false;
            focusBrackets.Text = "┌    ┐\n└    ┘";
            emptyBMPSRI = Application.GetResourceStream(new Uri("Assets2/EmptyCanvasImage.png", UriKind.Relative));
            initObjs();
            _appSetting = IsolatedStorageSettings.ApplicationSettings;
            // 从设置中读取信息
            try
            {
                // 设置中是否允许开启即收集信息
                if (_appSetting.Contains("autonet"))
                {
                    if (_appSetting["autonet"].ToString().Trim().CompareTo("ON") == 0)
                    {
                        if (m_weather == null)
                        {
                            m_weather = new WeatherSetting();
                            m_weather.autoLocating();
                            try
                            {
                                loadInfoGrid(false);
                            }
                            catch (Exception)
                            {
                                m_weather = null;
                                if (noWarning == false) showToast("Info loaded failed.");
                            }
                        }
                    }
                }
                else
                {
                    _appSetting.Add("autonet", "OFF");
                    _appSetting.Save();
                }
            }
            catch (Exception) { }
            //this.Loaded += (sender, e) => { this.highlightShowEditEllipseStory.Begin(); };
            this.Loaded += (sender, e) => { this.roateFlowerStory.Begin(); };
            this.ApplicationBar.IsVisible = false;
            icon_cameraCaptureTask = new CameraCaptureTask();
            icon_cameraCaptureTask.Completed += icon_cameraCaptureTask_Completed;
            DispatcherTimer suntmr = new DispatcherTimer();
            suntmr.Interval = TimeSpan.FromSeconds(0.1);
            suntmr.Tick += colorEggSunTimeTick;
            suntmr.Start();
        } 
        // 初始化函数
        public void initObjs()
        {
            m_photo.Visibility = System.Windows.Visibility.Collapsed; 
            inputGrid.Visibility = System.Windows.Visibility.Collapsed;
            backFlag = 2; //选择 部分
            isLastSavedPic = true;
            bmp = null;
            wb = null;
            wb_source = null;
            elementList = new List<object>();
            bwidth = 0;
            bheight = 0;
            isSecondEvent = 0;
            m_photoChooseTask = new PhotoChooserTask();
            m_photoChooseTask.ShowCamera = true;
            m_photoChooseTask.Completed += new EventHandler<PhotoResult>(photoChooseTask_complete);
            closeToast();
            closeProcessStep();
            closeSystemTrayToast();
            //try
            //{
            //    template5Preview();
            //}
            //catch (Exception) { showToast("无法导航定位"); }
            ///MessageBox.Show("连接电脑后若不能开启相机和相册，请拔下连接线重启软件\n麻烦您了^_^","仅测试提示",MessageBoxButton.OK);
        }
        // 加载各个模板的默认文字
        public void loadDefaultText()
        {
            // 添加默认的输入
            templateInputs = new string[TemplateAmount + 1];
            for (int i = 0; i <= TemplateAmount; ++i) templateInputs[i] = "";
            templateInputs[1] = "^_^";
            templateInputs[3] = "Hello, my day!";
            templateInputs[6] = "^_^";
            templateInputs[9] = "Great day!";
            templateInputs[10] = "Prefect music";
            templateInputs[11] = "  Hello\nMy world.";
            templateInputs[12] = "I\nCan\nFly";
            templateInputs[14] = "pm25";
            int hour = DateTime.Now.Hour;
            if (hour >= 5 && hour <= 12) templateInputs[15] = "Morning";
            else if (hour > 12 && hour <= 18) templateInputs[15] = "HELLO!";
            else templateInputs[15] = "Night";
        }

        // 导航到其他页面时刷新一遍设置 更新：
        // 作为目的被导航  签名 字体颜色 大小 使用背景条
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // 从设置中读取版本信息，决定是否加载帮助
            try
            {
                if ((_appSetting.Contains("version") == true && (_appSetting["version"].ToString().Trim().CompareTo(水印相机.App.waterMarkVersion) != 0)) || (_appSetting.Contains("version") == false))
                {
                    NavigationService.Navigate(new Uri("/Guides/GuidePage.xaml", UriKind.Relative));
                    return;
                }
            }
            catch (Exception) { MessageBox.Show("error"); return; }
            try
            {
                //设置页
                //  保存的签名
                if (_appSetting.Contains("signature"))
                {
                    this.signature = _appSetting["signature"].ToString().Trim();
                }
                else
                {
                    _appSetting.Add("signature", "");
                    _appSetting.Save();
                    this.signature = "";
                } 
                //字体页
                // 保存的字体颜色
                if (_appSetting.Contains("ColorA"))
                {
                    this.fontColor.A = (Byte.Parse(_appSetting["ColorA"].ToString().Trim()));
                    if (this.fontColor.A < 0) this.fontColor.A = 255;
                }
                else
                {
                    _appSetting.Add("ColorA", "255");
                    this.fontColor.A = 255;
                    _appSetting.Save();
                }
                if (_appSetting.Contains("ColorR"))
                {
                    this.fontColor.R = (Byte.Parse(_appSetting["ColorR"].ToString().Trim()));
                    if (this.fontColor.R < 0) this.fontColor.R = 255;
                }
                else
                {
                    _appSetting.Add("ColorR", "255");
                    this.fontColor.R = 255;
                    _appSetting.Save();
                }
                if (_appSetting.Contains("ColorG"))
                {
                    this.fontColor.G = (Byte.Parse(_appSetting["ColorG"].ToString().Trim()));
                    if (fontColor.G < 0) this.fontColor.G = 255;
                }
                else
                {
                    _appSetting.Add("ColorG", "255");
                    this.fontColor.G = 255;
                    _appSetting.Save();
                }
                if (_appSetting.Contains("ColorB"))
                {
                    this.fontColor.B = (Byte.Parse(_appSetting["ColorB"].ToString().Trim()));
                    if (fontColor.B < 0) fontColor.B = 255;
                }
                else
                {
                    _appSetting.Add("ColorB", "255");
                    this.fontColor.B = 255;
                    _appSetting.Save();
                } 
                // 自定义的字体大小
                if (_appSetting.Contains("FontSize"))
                {
                    this.fontSize = Double.Parse(_appSetting["FontSize"].ToString().Trim());
                    if (fontSize < 0) fontSize = 5;
                }
                else
                {
                    _appSetting.Add("FontSize", "5");
                    this.fontSize = 5;
                    _appSetting.Save();
                }
                // 文字停靠位置
                if (_appSetting.Contains("SimpleTextAlignment"))
                {
                    string alignmentsetting = _appSetting["SimpleTextAlignment"].ToString().Trim().ToLower();
                    if (alignmentsetting.CompareTo("right") == 0)
                    {
                        this.simpleTTextAlignment = TextAlignment.Right;
                    }
                    else
                    {
                        if (alignmentsetting.CompareTo("center") == 0) { this.simpleTTextAlignment = TextAlignment.Center; }
                        else { this.simpleTTextAlignment = TextAlignment.Left; }
                    }
                }
                else
                {
                    _appSetting.Add("SimpleTextAlignment", "left");
                    this.simpleTTextAlignment = TextAlignment.Left;
                }
                // 设置使用黑色背景条
                if (_appSetting.Contains("UseBackGround"))
                {
                    if (_appSetting["UseBackGround"].ToString().ToLower().Contains("yes") == true)
                        this.useBackGround = true;
                    else
                        this.useBackGround = false;
                }
                else
                {
                    _appSetting.Add("UseBackGround", "yes");
                    _appSetting.Save();
                    this.useBackGround = true;
                }
                // 显示机型
                if (_appSetting.Contains("showWPLogo"))
                {
                    if (_appSetting["showWPLogo"].ToString().Trim().CompareTo("ON") == 0)
                    {
                        showWPLogo = true;
                    }
                    else
                        showWPLogo = false;
                }
                else
                {
                    _appSetting.Add("showWPLogo", "OFF");
                    showWPLogo = false;
                    _appSetting.Save();
                }
                // 取景相机中保存原图--默认保存
                if (_appSetting.Contains("saveRealBackup"))
                {
                    if (_appSetting["saveRealBackup"].ToString().Trim().CompareTo("ON") == 0)
                    {
                        isSaveRealBackup = true;
                    }
                    else
                        isSaveRealBackup = false;
                }
                else
                {
                    _appSetting.Add("saveRealBackup", "ON");
                    isSaveRealBackup = true;
                    _appSetting.Save();
                }
                // 取景相机使用摄像头
                if (_appSetting.Contains("useFrontCam"))
                {
                    if (_appSetting["useFrontCam"].ToString().Trim().CompareTo("ON") == 0)
                    {
                        isUsingFrontFace = true;
                    }
                    else
                        isUsingFrontFace = false;
                }
                else
                {
                    _appSetting.Add("useFrontCam", "OFF");
                    isUsingFrontFace = false;
                    _appSetting.Save();
                }
                //分贝文字的加载
                if (_appSetting.Contains("useFenbeiByDefault"))
                {
                    if (_appSetting["useFenbeiByDefault"].ToString().Trim().CompareTo("ON") == 0)
                    {
                        this.useFBDefaultWords = true; 
                    }
                    else
                    {
                        this.useFBDefaultWords = false;
                    }
                }
                else
                {
                    _appSetting.Add("useFenbeiByDefault", "ON");
                    this.useFBDefaultWords = true; 
                    _appSetting.Save();
                }
                if (_appSetting.Contains("fenbeiWords"))
                {
                    templateInputs[10] = templateInputs[2] = _appSetting["fenbeiWords"].ToString();
                }
                else
                {
                    _appSetting.Add("fenbeiWords", "");
                    _appSetting.Save();
                    templateInputs[2] = "";
                    templateInputs[10] = "Prefect music";
                }
                this.indexADControl.CountryOrRegion = "US";
            }
            catch (Exception) { if (noWarning == false) showToast("Info loaded failed"); }
            if (isLastSavedPic == false && m_photo.Visibility == System.Windows.Visibility.Visible)
            {
                setTemplatePreview();
            }
            noWarning = false;
            hideAllEditFunctionGrid();
            closeSystemTrayToast();
            isTemplateSlide = true;
            if (this.indexPageCanvas.Visibility == System.Windows.Visibility.Visible || realCameraAppBarGrid.Visibility == System.Windows.Visibility.Visible) this.ApplicationBar.IsVisible = false;
            else this.ApplicationBar.IsVisible = true;
            if (realTimeCanvas.Visibility == System.Windows.Visibility.Visible)
            {
                // Check to see if the camera is available on the device.
                if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                     (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
                {
                    initRealCamera();
                }
                else
                {
                    showToast("Camera loaded failed");
                    //// The camera is not supported on the device.
                    //this.Dispatcher.BeginInvoke(delegate()
                    //{
                    //    // Write message.
                    //    toastText.Text = "A Camera is not available on this device.";
                    //});
                }
            }
        }
        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            closeDevices();
            closeSystemTrayToast();
            closeRealCamera();
        }
        ///加载图片
        //选择图片
        private void choosePhoto(object sender, EventArgs e)
        {
            if (isLastSavedPic == false && isSecondEvent != 3)
            {
                showToast("BackKey again if sure to don't save", 4, true, 3);
                return;
            }
            else closeToast();
            try
            {
                m_photoChooseTask.Show();
            }
            catch (System.InvalidOperationException)
            {
                if(noWarning==false) showToast("Fail to load the album.");
            }
        }
        private void photoChooseTask_complete(object sender, PhotoResult e)
        {
            try
            {
                if (e.TaskResult == TaskResult.OK) // 加载成功
                {
                    loadEditMode(e.ChosenPhoto); 
                }
                else if (e.TaskResult == TaskResult.None) // 加载错误
                {
                    showToast("Picture type is not supported");
                }
                //用户取消加载
            }
            catch (Exception) { showToast("Fail to load the picture"); }
        }
        //成功加载图片后开始编辑模式
        private void loadEditMode(Stream photoStream)
        {
            isLastSavedPic = false;
            templateSelectNameGrid.Visibility = System.Windows.Visibility.Collapsed;
            bmp = new System.Windows.Media.Imaging.BitmapImage();
            bmp.SetSource(photoStream);
            wb = new WriteableBitmap(bmp); //可编辑图
            wb_source = new WriteableBitmap(bmp); //原图副本
            m_photo.Source = wb;
            bheight = bmp.PixelHeight;
            bwidth = bmp.PixelWidth;
            m_photo.Visibility = System.Windows.Visibility.Visible;
            animation_photo_show.Begin();
            animation_index_hide.Begin();
            if(this.realCameraAppBarGrid.Visibility == System.Windows.Visibility.Collapsed) this.ApplicationBar.IsVisible = true;//系统相机和相册状态下appbar终存在
            backFlag = 1; //制作 部分
            isTemplateSlide = true;
            closeIndex();
            noWarning = false;
            setTemplatePreview();   
        }

        /************************************模板设计*************************************/
        ///--简单模板 起始模板
        // 只添加时间和自定义文字，时间和签名在顶部，字体同样大小  id=0
        private void timeTemplatePreview()
        {
            inputBox.Text = templateInputs[0]; 
            if (inputBox.Text.Length > 0)
            {
                inputBox.Text = inputBox.Text.Replace("\r", "\n");
            }
            string sentence = "" + ((editGridCheck.IsChecked == true) ? string.Format("{0:D4}.{1}.{2} {3}:{4:D2} {5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, this.signature) : "") + (inputBox.Text.Trim().Length == 0 ? "" : "\n" + inputBox.Text);
            WTextBlock userInputS = new WTextBlock(bwidth * 0.005, 0, sentence, bheight * fontSize / 100,fontName, 0, this.fontColor.R, this.fontColor.G, this.fontColor.B, this.fontColor.A, simpleTTextAlignment, bwidth * 0.99);
            userInputS.y = bheight*0.98 - userInputS.tb.ActualHeight;
            if (this.useBackGround == true && sentence.Trim().Length > 0)
            {
                Shapw inputbg = new Shapw(0, userInputS.y-bheight*0.02, Shapw.wShapes.Rectangle, 0, 0, bwidth, userInputS.tb.ActualHeight + bheight * 0.04);
                inputbg.setBrush(0, 0, 0, 100);
                this.elementList.Add(inputbg);
            }
            this.elementList.Add(userInputS);
            this.previewBMP();
            loadEditGrid("fontcolor,roate,textedit,editcheck");
            this.editGridCheck.Content = "Time";
            templateInputs[0] = inputBox.Text;
        }
        // 添加时间和带标题的自定义文字，时间和签名在底部，字体标题>文字>时间  id=11
        private void timeTemplatePreview1()
        {
            inputBox.Text = templateInputs[11];
            if (inputBox.Text.Length > 0)
            {
                inputBox.Text = inputBox.Text.Replace('\r', '\n');
            }
            string[] texts = inputBox.Text.Split('\n');  
            double fontsize0 = bheight * fontSize / 100;
            double yPoint = bheight - fontsize0 * 1.2;
            if (editGridCheck.IsChecked == true) this.elementList.Add(new WTextBlock(bwidth * 0.02, yPoint, string.Format("{0:D4}/{1}/{2} {3}:{4:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute )+(this.signature.Length>0?("By"+this.signature):("")), fontsize0 * 0.7, fontName, 0, this.fontColor.R, this.fontColor.G, this.fontColor.B, this.fontColor.A, simpleTTextAlignment, bwidth * 0.99));
            for (int i = texts.Length - 1; i > 0; --i)
            {
                this.elementList.Add(new WTextBlock(bwidth*0.02,yPoint-=fontsize0*1.15,texts[i],fontsize0,fontName, 0, this.fontColor.R, this.fontColor.G, this.fontColor.B, this.fontColor.A, simpleTTextAlignment, bwidth * 0.95));
            }  
            this.elementList.Add(new WTextBlock(bwidth * 0.02, yPoint-fontsize0*1.9, texts[0], fontsize0 * 1.6,fontName, 2, this.fontColor.R, this.fontColor.G, this.fontColor.B, this.fontColor.A, simpleTTextAlignment, bwidth * 0.95));
            this.previewBMP();
            loadEditGrid("fontcolor,roate,textedit,editcheck");
            this.editGridCheck.Content = "Time";
            templateInputs[11] = inputBox.Text;
        }
        // 只添加竖排文字  id=12
        private void timeTemplatePreview2()
        {
            inputBox.Text = templateInputs[12];
            if (inputBox.Text.Length > 0)
            {
                inputBox.Text = inputBox.Text.Replace('\r', '\n');
            }
            string[] texts = inputBox.Text.Split('\n');
            double fontsize0 = bheight * fontSize / 100;
            double fontsize0Space = fontsize0 * 1.13;
            double xPoint = 0.0;
            double yPoint = bheight * 0.2;
            string temp="";
            int txtLength = texts.Length;
            int templength = 0;
            if (simpleTTextAlignment == TextAlignment.Right)
            {
                xPoint = bwidth * 0.9 - fontsize0Space;
                for (int i = texts.Length - 1; i >= 0; --i)
                {
                    temp = texts[i];
                    templength = temp.Length;
                    if (bheight * 0.8 > fontsize0Space * temp.Length) yPoint = bheight * 0.15 - fontsize0Space;
                    else yPoint = -fontsize0Space * 0.95;
                    for (int j = 0; j < templength; ++j)
                    {
                        this.elementList.Add(new WTextBlock(xPoint, yPoint += fontsize0Space, "" + temp[j], fontsize0, fontName, 0, this.fontColor.R, this.fontColor.G, this.fontColor.B, this.fontColor.A));
                    }
                    xPoint -= fontsize0Space;
                }
            }
            else
            {
                xPoint = bwidth * 0.05;
                for (int i = 0; i < txtLength; ++i)
                {
                    temp = texts[i];
                    templength = temp.Length;
                    if (bheight * 0.8 > fontsize0Space * temp.Length) yPoint = bheight * 0.15 - fontsize0Space;
                    else yPoint = -fontsize0Space*0.95;
                    for (int j = 0; j < templength; ++j)
                    {
                        this.elementList.Add(new WTextBlock(xPoint, yPoint += fontsize0Space, "" + temp[j], fontsize0, fontName, 0, this.fontColor.R, this.fontColor.G, this.fontColor.B, this.fontColor.A));
                    }
                    xPoint += fontsize0Space;
                }
            }
            this.previewBMP();
            loadEditGrid("fontcolor,roate,textedit");
            templateInputs[12] = inputBox.Text;
        }
        // 右上角盖章 id=15
        private void timeTemplatePreview3()
        {
            inputBox.Text = templateInputs[15];
            int baseSize = bheight > bwidth ? bwidth : bheight;
            if (inputBox.Text.Length > 0)
            {
                inputBox.Text = inputBox.Text.Replace('\r', ' ');
            }
            string temp = inputBox.Text;
            double fontsize0 = baseSize * 0.13;
            double fontsize0Space = fontsize0 * 1.13;
            double xPoint = 0.0;
            double yPoint = bheight * 0.22;
            int txtLength = temp.Length;
            int templength = 0;
            xPoint = bwidth  - fontsize0Space*1.2;
            templength = temp.Length;
            if (bheight * 0.8 > fontsize0Space * temp.Length) yPoint = bheight * 0.15 - fontsize0Space;
            else yPoint = -fontsize0Space * 0.95;
            for (int j = 0; j < templength; ++j)
            {
                this.elementList.Add(new WTextBlock(xPoint, yPoint += fontsize0Space, "" + temp[j], fontsize0, fontName, 0, this.fontColor.R, this.fontColor.G, this.fontColor.B, this.fontColor.A));
            }
            var ellip = new Shapw(bwidth - baseSize * 0.3, 0 - baseSize * 0.04, Shapw.wShapes.Ellipse, 0, 0, baseSize * 0.37, baseSize * 0.25);
            ellip.setFillBrush(backColor.R, backColor.G, backColor.B, 100);
            this.elementList.Add(ellip);
            this.elementList.Add(new WTextBlock(0,baseSize*0.02,string.Format("{0:D4}.{1}.{2} ", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day),fontColor,baseSize * 0.06,"Segoe WP",1,TextAlignment.Right,bwidth));
            this.elementList.Add(new WTextBlock(0, baseSize * 0.09, string.Format("{0:D2}:{1:D2} ", DateTime.Now.Hour, DateTime.Now.Minute), fontColor, baseSize * 0.077, "Segoe WP", 1, TextAlignment.Right, bwidth));
            string sentence = "" + ((editGridCheck.IsChecked == true) ? string.Format("{0:D4}.{1}.{2} {3}:{4:D2} {5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, this.signature) : "") + (inputBox.Text.Trim().Length == 0 ? "" : "\n" + inputBox.Text);
            this.previewBMP();
            loadEditGrid("fontcolor,roate,textedit,backcolor"); 
        }

        ///联网模板
        //--模板1 加天气图标 城市，时间+签名，自定义  //使用了printicon打印图片接口  id=1
        private void template1Preview()
        {
            inputBox.Text = templateInputs[1];
            //添加水印文字--时间
            string time_text = string.Format("{0:D4}.{1:D2}.{2:D2} {3:D2}:{4:D2}", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute);
            if (editGridCheck.IsChecked == true) time_text += " " + signature; //签名
            double fontSize0 = (bheight * 0.05 * time_text.Length > bwidth ? bwidth / (time_text.Length + 2) : bheight * 0.05);
            double yPoint = bheight * 0.95;
            //添加水印文字--自定义输入 将回车和换行转为空格
            string inputSentence = inputBox.Text.Replace('\r', ' ');
            inputSentence = inputSentence.Replace('\n', ' ');
            double xPoint = bwidth * 0.99 - fontSize0 * signature.Length * 1.2;
            if (xPoint < 0) xPoint = fontSize0 * 8 + bwidth * 0.17; //时间约长 8 个汉字
            this.elementList.Add(new WTextBlock(bwidth * 0.17, yPoint -= fontSize0 * 1.5, inputSentence, fontSize0, fontName, 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            //时间
            this.elementList.Add(new WTextBlock(bwidth * 0.17, yPoint -= fontSize0 * 1.2, time_text, fontSize0, fontName, 0, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            //添加城市
            double fontSize1 = fontSize0 * 1.5;
            if (m_weather.districtName.Trim().Length > 0)
            {
                this.elementList.Add(new WTextBlock(bwidth * 0.24, yPoint -= fontSize1 * 1.1, m_weather.districtName, fontSize1, fontName, 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            }
            else
            {
                this.elementList.Add(new WTextBlock(bwidth * 0.24, yPoint -= fontSize1 * 1.1, "Great Place", fontSize0,fontName, 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            }
            printIcon(fontSize1, fontSize1 * 1.5, bwidth * 0.16, yPoint + fontSize1 * 0.2, "/WaterMark;component/Assets2/icons/location/LocationMarker.png");
            printIcon(bwidth * 0.16, bwidth * 0.16, bwidth * 0.005, bheight*0.95 - bwidth * 0.16, m_weather.getMarkPic());
            //预览
            this.previewBMP();
            loadEditGrid("fontcolor,roate,textedit,editcheck");
            this.editGridCheck.Content = "Sign";
            templateInputs[1] = inputBox.Text;
        }
        // 大时刻，小年月日，小天气。右上  id=13
        private void template1Preview2()
        {
            inputBox.Text = templateInputs[13];
            //添加水印文字--时刻（大） 
            double fontSize0 = bheight > bwidth ? bwidth : bheight;
            double fontSize1 = fontSize0 * 0.16;
            double yPoint = 0;  
            fontSize0 = fontSize1 / 3; // 年月日和天气的真实大小
            //时间
            this.elementList.Add(new WTextBlock(0, yPoint , string.Format("{0:D2}:{1:D2}", DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute), fontSize1, fontName, 0, fontColor.R, fontColor.G, fontColor.B, fontColor.A,TextAlignment.Right,bwidth * 0.98));
            if (this.editGridCheck.IsChecked == true)
            {
                this.elementList.Add(new WTextBlock(0, yPoint += fontSize1 * 1.12, string.Format("{0:D4}/{1:D2}/{2:D2} ", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day) + new string(DateTime.Now.DayOfWeek.ToString().ToCharArray(), 0, 3), fontSize0, fontName, 0, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Right, bwidth * 0.98));
                this.elementList.Add(new WTextBlock(0, yPoint + fontSize0 * 1.12, m_weather.districtName + " " + m_weather.tempratureToday + (m_weather.tempratureToday.Length > 0 ? "℃" : ""), fontSize0, fontName, 0, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Right, bwidth * 0.98));
            }
            //预览
            this.previewBMP();
            loadEditGrid("fontcolor,roate,editcheck");
            this.editGridCheck.Content = "More"; 
        }

        //--模板2 分贝模板 id=2,10
        // 分贝的所有模板都用这个函数，根据templateCount的值选择打印某个具体的模板
        private void template2Preview(bool isSaving = false)
        {
            if (isSaving == true)
            {
                closeDevices();
                fenbeiTemplatePrint();
                return;
            }
            closeEditGrid();//编辑栏没有做好如何使计数器计数同时操作图片
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(33);
            timer.Tick += delegate { try { Microsoft.Xna.Framework.FrameworkDispatcher.Update(); } catch { } };
            timer.Start();
            //设置1秒的缓存区，设获取1秒音频就会调用一次BufferReady事件
            microphone.BufferDuration = TimeSpan.FromMilliseconds(1000);
            //分配1秒音频所需要的缓存区,实际为32000长的数组
            buf = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)];
            //BufferReady事件
            microphone.BufferReady += new EventHandler<EventArgs>(microphone_BufferReady);
            //启动录音
            microphone.Start();
            //loadEditGrid("fontcolor,roate,textedit,editcheck"); 
        }
        private void fenbeiTemplatePrint()//根据templateCount的值选择打印某个具体的模板
        {
            int chooseNum = templateCount % (TemplateAmount + 1);
            if (chooseNum == 2) fenbeiPrint1(); //id=10
            else fenbeiPrint2();//id=2
        }
        private Microphone microphone = Microphone.Default; // 麦克单例
        //每次捕获音频缓存
        private byte[] buf;
        // 测试分析分贝数
        double averageDB;
        bool useFBDefaultWords; //使用默认的分贝文字还是用户指定的
        // 分贝记录算法
        void microphone_BufferReady(object sender, EventArgs e)
        {
            //将麦克风的数据复制到缓冲区中
            microphone.GetData(buf);
            //将该缓冲区写入一个流
            int buffLength = buf.Length;
            //测分贝
            averageDB = 0.0;
            int temp = 0;
            for (int i = 0; i < 500; ++i)
            {
                temp = (buf[i] >= 128 ? 255 - buf[i] : buf[i]);
                if (temp == 0) temp = 1;
                averageDB += Math.Log10((double)temp / 0.00256);
                /*分贝的算法说明 SPL=20*Log(|p1/pr|/ 2* 10e-5)  参考《praat语音软件使用手册》--熊子瑜
                p1即当前声压值，8bit采样时，如果大于等于128，则用该值减256(我觉得255更合适，而且计算时0应该调整成1，否则会出现负无穷)，最后求绝对值
                pr即采样的中值，8bit时为128，16bit为32768
                2*10e-5为标准值（0分贝对应声压）
                求log之后乘20即为分贝值
                多次分贝值求平均
                */
            }
            averageDB /= 25; //效率问题 取32000的前500次样本，算法中最后乘20放到这一步计算  averageDB = averageDB0*20 / 500
            //将值写入wb中，刷新图片
            fenbeiTemplatePrint();
        }
        // 为分贝模板提供附加图片的地址
        private string getVoiceIMG(double voiceLoud)
        {
            int chooseNum = templateCount % (TemplateAmount + 1); // 2, 10
            if (chooseNum == 10) //id=2
            {
                if (voiceLoud < 20.1) return "/WaterMark;component/Assets2/icons/voiceLoude/0-20Flower.png";
                else if (voiceLoud >= 20.1 && voiceLoud < 58.1) return "/WaterMark;component/Assets2/icons/voiceLoude/20-30Owl.png";
                else if (voiceLoud >= 58.1 && voiceLoud < 69.1) return "/WaterMark;component/Assets2/icons/voiceLoude/30-65Turtle.png";
                else if (voiceLoud >= 69.1 && voiceLoud < 90.1) return "/WaterMark;component/Assets2/icons/voiceLoude/65-90Guitar.png";
                else if (voiceLoud >= 90.1 && voiceLoud < 120) return "/WaterMark;component/Assets2/icons/voiceLoude/90-120NoHorn.png";
                else return "/WaterMark;component/Assets2/icons/voiceLoude/120-Explosion.png";
            }
            else  //id=10
            {
                if (voiceLoud < 58.1) return "/WaterMark;component/Assets2/icons/voiceLoude/small.png";
                else if (voiceLoud >= 58.1 && voiceLoud < 65.1) return "/WaterMark;component/Assets2/icons/voiceLoude/normal.png";
                else if (voiceLoud >= 65.1 && voiceLoud < 71.1) return "/WaterMark;component/Assets2/icons/voiceLoude/large.png";
                else { return "/WaterMark;component/Assets2/icons/voiceLoude/max.png"; }
            }
        }
        // 为分贝模板提供附加文字
        private string getVoiceText(double voiceLoud)
        {
            if (useFBDefaultWords == false) return ((int)averageDB).ToString() + "DB  " +templateInputs[2];
            else
            {
                if (this.templateInputs[7].Trim().Length > 0) return ((int)averageDB).ToString() + "DB  " + this.templateInputs[7];
                if (voiceLoud < 40.1) return ((int)averageDB).ToString() + "DB  " + "The leaf falling";
                else if (voiceLoud >= 40.1 && voiceLoud < 62.1) return ((int)averageDB).ToString() + "DB  " + "My quiet time";
                else if (voiceLoud >= 62.1 && voiceLoud < 70.1) return ((int)averageDB).ToString() + "DB  " + "I enjoy the peace";
                else if (voiceLoud >= 70.1 && voiceLoud < 99.1) return ((int)averageDB).ToString() + "DB  " + "Great music!";
                else if (voiceLoud >= 99.1 && voiceLoud < 120) return ((int)averageDB).ToString() + "DB  " + "Wake up!";
                else /* (voiceLoud >= 120.1)*/
                    return ((int)averageDB).ToString() + "DB  " + "Anyone turns down it!";
            }
        }
        //分贝1，id=11 底桌面音量+位置 
        private void fenbeiPrint1() //刷新图片,和其他的图片中的固定值的模板是一个感觉，每一秒刷新一次（由分贝检测的刷新频率决定）
        { 
            if (clearItemOnBMP(false) == false) return;
            //添加水印文字--时间
            string time_text = string.Format("{0:D4}.{1:D2}.{2:D2} {3:D2}:{4:D2}", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute) + "  " + (m_weather.tempratureToday.Length > 0 ? m_weather.tempratureToday + "℃  " : "  ") + (editGridCheck.IsChecked == true ? (signature) : "");
            double fontSize0 = (bheight * 0.05 * time_text.Length > bwidth ? bwidth / (time_text.Length - 4) : bheight * 0.05);
            double fontSize1 = fontSize0 * 1.25; 
            string place_text = m_weather.districtName;
            if(place_text.Length==0) place_text = "I'm here~";
            else {
                int maxlength = (int)( bwidth * 0.85 / fontSize0 );
                if(maxlength < place_text.Length){
                    place_text = new string(place_text.ToCharArray(),0,maxlength-1);
                    place_text += "..";
                }
            } 
            //时间 签名
            this.elementList.Add(new WTextBlock(bwidth * 0.13, bheight - 3.25 * fontSize0, place_text.ToString(), fontSize0, "Segoe WP SemiLight", 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Right, bwidth * 0.8));
            this.elementList.Add(new WTextBlock(bwidth * 0.13, bheight - 2.25 * fontSize0, time_text.ToString(), fontSize0, "Segoe WP SemiLight", 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Right, bwidth * 0.8));
            // 自定义 
            this.elementList.Add(new WTextBlock(bwidth * 0.12, bheight - 4.25 * fontSize0, (useFBDefaultWords) ? "Just listen." : templateInputs[10], fontSize1, "Segoe WP Bold", 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Right, bwidth * 0.8));
            //分贝
            this.elementList.Add(new WTextBlock(bwidth * 0.08, bheight - 5 * fontSize0, ((int)averageDB).ToString(), fontSize0*2.6, "Segoe WP Bold", 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            this.elementList.Add(new WTextBlock(bwidth * 0.08, bheight - 2.25 * fontSize0, "DB", fontSize0, "Segoe WP", 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            this.elementList.Add(new Imagew(-0.02*bheight, 0.7 * bheight, bheight * 0.28, bheight * 0.13, this.getVoiceIMG(averageDB)));
            //预览
            this.previewBMP();
        }
        //分贝2，id=2  顶条幅
        private void fenbeiPrint2() //刷新图片,和其他的图片中的固定值的模板是一个感觉，每一秒刷新一次（由分贝检测的刷新频率决定）
        {
            if (clearItemOnBMP(false) == false) return;
            //添加水印文字--时间
            string time_text = string.Format("{0:D4}.{1:D2}.{2:D2} {3:D2}:{4:D2}", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute) + "  " + m_weather.districtName+" " + (m_weather.tempratureToday.Length > 0 ? m_weather.tempratureToday + "℃  " : "  ") + (editGridCheck.IsChecked == true ? (signature) : "");
            double fontSize0 = (bheight * 0.05 * time_text.Length > bwidth ? bwidth / (time_text.Length - 4) : bheight * 0.05);
            double fontSize1 = fontSize0 * 1.25;
            double yPoint = bheight;
            //添加水印文字--自定义输入 将回车和换行转为空格 
            double xPoint = bwidth;
            Shapw inputbg = new Shapw(0, 0, Shapw.wShapes.Rectangle, 0, 0, bwidth, fontSize0 * 3);
            inputbg.setBrush(0, 0, 0, 80);
            this.elementList.Add(inputbg);
            //时间 签名
            this.elementList.Add(new WTextBlock(bwidth * 0.13, xPoint = 0.005 * bheight, time_text.ToString(), fontSize0, "Segoe WP SemiLight", 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            //分贝 自定义文字
            this.elementList.Add(new WTextBlock(bwidth * 0.13, xPoint += fontSize1, getVoiceText(averageDB), fontSize1, "Segoe WP", 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            this.elementList.Add(new Imagew(-0.02 * bheight, -0.02 * bheight, bwidth * 0.16, bwidth * 0.16, this.getVoiceIMG(averageDB)));
            //预览
            this.previewBMP();
        }
         
        //-- 模版3预览--图块  id=3
        private void template3Preview()
        {
            inputBox.Text = templateInputs[3];
            wb = new WriteableBitmap(wb.PixelWidth, (int)(wb.PixelHeight * 1.3)); 
            wb.Render(new Rectangle(){ Width = wb.PixelWidth, Height = wb.PixelHeight, Fill=new SolidColorBrush(backColor)}, new TranslateTransform() { X = 0, Y = 0 });
            wb.Invalidate();
            int sizeofWBold = wb_source.PixelHeight * wb_source.PixelWidth; 
            for (int i = 0; i < sizeofWBold; ++i) wb.Pixels[i] = wb_source.Pixels[i]; 
            Image pic = iconUser;
            TranslateTransform transicon = new TranslateTransform();
            if (isIconAvailable == false || iconUser.Source == null)
            {
                pic = new Image();
                BitmapImage bmpic = new BitmapImage(new Uri("/WaterMark;component/Assets2/icons/emotions/Smile2.png", UriKind.Relative));
                pic.Source = bmpic;
                //if (noWarning == false) showToast("先在菜单中加载图章才能显示图章");
            }
            double widthDHight = pic.Width / pic.Height;
            pic.Height = wb.PixelHeight * 0.21;// 3/13==0.23
            pic.Width = (pic.Height * widthDHight > wb.PixelWidth * 0.5) ? wb.PixelWidth * 0.5 : pic.Height * widthDHight;
            pic.Stretch = Stretch.Fill;
            transicon.X = wb.PixelHeight * 0.01;
            transicon.Y = wb.PixelHeight * 0.78;
            wb.Render(pic, transicon);
            double beginX = wb.PixelWidth * 0.02 + pic.ActualWidth;
            double beginY = wb.PixelHeight * 0.77;
            double textWidth = wb.PixelWidth * 0.96 - pic.ActualWidth;
            double baseSize = wb.PixelHeight > wb.PixelWidth ? wb.PixelHeight : wb.PixelWidth;
            string weatherText = m_weather.districtName +m_weather.weatherToday_full+ m_weather.tempratureToday + ((m_weather.tempratureToday.Trim().Length>0)?"℃":"") + " " + string.Format("{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}  ", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute);
            double weatherTextSize = textWidth / (weatherText.Length-10) > baseSize * 0.05 ? baseSize * 0.05 : textWidth / (weatherText.Length-9);
            //elementList.Add(new WTextBlock(beginX, beginY, weatherText, weatherTextSize, "Segoe WP SemiLight", 0, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Right, textWidth));
            templateInputs[3] = templateInputs[3].Replace('\r', '\n');
            int lineNum = templateInputs[3].Split('\n').Length;
            double fontsize0 = 0;
            switch(lineNum)
            {
                case 0: { fontsize0 = baseSize * 0.12; break; }
                case 1: { fontsize0 = (templateInputs[3].Length < 4) ? (baseSize * 0.1) : ((baseSize * 0.12) > (textWidth / 6.2) ? (textWidth / 6.2) : (baseSize * 0.12)); break; }
                case 2: { fontsize0 = (baseSize * 0.06) > (textWidth / 6.2) ? (textWidth / 6.2) : (baseSize * 0.06); break; }
                case 3: { fontsize0 = (baseSize * 0.04) > (textWidth / 6.2) ? (textWidth / 6.2) : (baseSize * 0.04); break; }
                default: { fontsize0 = (baseSize * 0.03) > (textWidth / 6.2) ? (textWidth / 6.2) : (baseSize * 0.03); break; }
            }
            var inputText = new WTextBlock(beginX, beginY + baseSize * 0.065, templateInputs[3], fontsize0,fontName/* "Segoe WP Semibold"*/, 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Center, textWidth);
            if(lineNum<4) inputText.tb.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            elementList.Add(inputText);
            previewBMP();
            loadEditGrid("fontcolor,roate,myicon,backcolor,text2edit");
        }
        //-- 模版3.2预览--带边图块 id=9
        private void template3Preview2()
        {
            inputBox.Text = templateInputs[9];
            int sHeight = wb_source.PixelHeight;
            int sWidth = wb_source.PixelWidth;
            wb = new WriteableBitmap((int)(sWidth * 1.1), (int)(sHeight * 1.39));
            wb.Render(new Rectangle() { Width = wb.PixelWidth, Height = wb.PixelHeight, Fill = new SolidColorBrush(backColor) }, new TranslateTransform() { X = 0, Y = 0 });
            wb.Render(new Image() { Source = wb_source }, new TranslateTransform() { X = wb.PixelWidth * 0.05, Y = wb.PixelHeight * 0.08 });
            wb.Invalidate();
            double fontsize0 = sHeight * 0.07;
            elementList.Add(new WTextBlock(0, 0.01 * sHeight, signature + " ", fontColor, fontsize0*0.8, "Segoe WP", 0, TextAlignment.Right, wb.PixelWidth));
            elementList.Add(new WTextBlock(0, sHeight * 1.28, string.Format("{0:D4}.{1:D2}.{2:D2}  {3:D2}:{4:D2}  ", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute), Colors.White, fontsize0, "Segoe WP", 1, TextAlignment.Center, wb.PixelWidth));
            elementList.Add(new WTextBlock(0, sHeight * 1.12, templateInputs[9], fontColor, fontsize0 * 1.45, fontName/* "Segoe WP Semibold"*/, 2, TextAlignment.Center, wb.PixelWidth));
            previewBMP();
            loadEditGrid("fontcolor,roate,backcolor,textedit");
        }

        //-- 模板4预览--图章  id=4
        private void template4Preview()
        {
            //添加水印文字--时间
            string time_text = string.Format("{0:D4}.{1:D2}.{2:D2} {3:D2}:{4:D2}  ", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute);
            time_text += m_weather.districtName+" "+signature;
            double fontSize0 = (bheight * 0.08 * time_text.Length > bwidth ? bwidth / (time_text.Length) : bheight * 0.05);
            this.elementList.Add(new WTextBlock(0, bheight * 0.98 - fontSize0, time_text, fontSize0, "Segoe WP SemiLight", 0, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Center, bwidth));
            // 添加印章
            TranslateTransform transicon = new TranslateTransform();
            Image pic = iconUser;
            if (isIconAvailable == false || iconUser.Source == null)
            {
                pic = new Image();
                BitmapImage bmpic = new BitmapImage(new Uri("/WaterMark;component/Assets2/icons/emotions/Smile2.png", UriKind.Relative));
                pic.Source = bmpic;
                //if (noWarning == false) showToast("先在菜单中加载图章才能显示图章");
            }
            pic.Width = bwidth * 0.14;
            pic.Height = bwidth * 0.14;
            pic.Stretch = Stretch.Uniform;
            transicon.X = bwidth * 0.43;
            transicon.Y = bheight*0.98 - fontSize0 - bwidth * 0.14 * iconUser.Height / iconUser.Width;
            wb.Render(pic, transicon);
            //预览
            this.previewBMP();
            loadEditGrid("fontcolor,roate,myicon");
        }  

        /**/// 模板5预览-- 旅行故事--距离 id=5
        private void template5Preview()
        {
            if(isLastSavedPic==false) loadDdistanceStory();
            if (isTripBegin || distanceBeginName.CompareTo("Start") == 0 && beginLatitude.Trim().Length == 0) tripBeginPreview();
            else tripEndPreview();
            loadEditGrid("fontcolor,roate,tripbegin,tripend");
        }
        private string distanceBeginName="Start";
        private string distanceEndName = "End";
        private string beginLatitude = "";
        private bool isTripBegin = false;
        private void loadDdistanceStory()
        {
            if (distanceBeginName.Trim().Length == 0||distanceBeginName.CompareTo("Start")==0)
            {
                if (_appSetting.Contains("LastPlaceName"))
                {
                    if (_appSetting["LastPlaceName"].ToString().Trim().Length == 0)
                    {
                        showToast("Start a story in edit grid?");
                    }
                    else
                    {
                        distanceBeginName = _appSetting["LastPlaceName"].ToString();
                        if (_appSetting.Contains("beginLatitude"))
                        {
                            beginLatitude = _appSetting["beginLatitude"].ToString();
                        }
                        else
                        {
                            _appSetting.Add("beginLatitude", "");
                            _appSetting.Save();
                            beginLatitude = "";
                        }
                    }
                }
                else
                {
                    showToast("Start a story in edit grid?");
                    _appSetting.Add("LastPlaceName", "Start");
                    _appSetting.Save();
                }
                templateInputs[5] = distanceBeginName;
            }
            else
            {
                showToast("Record here in edit grid.");
            }
        }
        private void tripBeginButton_Click(object sender, RoutedEventArgs e)
        {
            var messagePrompt = new InputPrompt
            {
                Title = "From here on",
                Message = "Name of the start point"
            };
            messagePrompt.Completed += tripBeginInputed;
            messagePrompt.Show();
        }
        private void tripBeginInputed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.PopUpResult == PopUpResult.Ok)
            {
                distanceBeginName = e.Result;
                if (_appSetting.Contains("beginLatitude"))
                {
                    if (m_weather.EorW.Trim().Length == 0 || m_weather.NorS.Trim().Length == 0) _appSetting["beginLatitude"] = "";
                    else
                    {
                        _appSetting["beginLatitude"] = m_weather.EorW+","+m_weather.NorS;
                    }
                }
                else
                {
                    string lonAndLat = "";
                    if (m_weather.EorW.Trim().Length == 0 || m_weather.NorS.Trim().Length == 0) lonAndLat = "";
                    else
                    {
                        lonAndLat = m_weather.EorW + "," + m_weather.NorS;
                    }
                    _appSetting.Add("beginLatitude", lonAndLat);
                }
                if (_appSetting.Contains("LastPlaceName"))
                {
                    _appSetting["LastPlaceName"] = distanceBeginName; 
                }
                else
                {
                    _appSetting.Add("LastPlaceName", distanceBeginName); 
                }
                _appSetting.Save();
            }
            else if (e.PopUpResult == PopUpResult.Cancelled) { }
            else { }
            isTripBegin = true;
            tripBeginPreview();
        }
        private void tripBeginPreview()
        {
            clearItemOnBMP();
            // 添加出发的单模板
            double baseSize = (wb.PixelHeight > wb.PixelWidth) ? wb.PixelWidth : wb.PixelHeight;//选定乘的基数，以小的为准
            var sendoff_time_text = new WTextBlock(0, bheight * 0.74 - baseSize*0.05, string.Format("{0:D4}/{1}/{2} {3}:{4:D2} ", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute) + m_weather.districtName, baseSize*0.05 , "Segoe WP SimeLight", 0, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Center);
            var sendoff_text = new WTextBlock(baseSize*0.13, bheight * 0.755, distanceBeginName + " Go", baseSize * 0.08, "Segoe WP", 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A);
            double backLength = (sendoff_time_text.tb.ActualWidth + bheight * 0.0625) > (sendoff_text.tb.ActualWidth + bheight * 0.13) ? sendoff_time_text.tb.ActualWidth + bheight * 0.0625 : sendoff_text.tb.ActualWidth + bheight * 0.13;
            sendoff_time_text.tb.Width = backLength;
            var backTime = new Shapw(0, bheight * 0.75 - baseSize * 0.07, Shapw.wShapes.Rectangle, 0, 0, backLength, baseSize * 0.07);
            backTime.setFillBrush(0, 0, 0, (byte)100);
            var backText = new Shapw(0, bheight * 0.75, Shapw.wShapes.Rectangle, 0, 0, backLength, baseSize * 0.125);
            backText.setFillBrush(0, 0, 0, (byte)60);
            elementList.Add(backTime);
            elementList.Add(backText);
            elementList.Add(sendoff_time_text);
            elementList.Add(sendoff_text);
            elementList.Add(new Imagew(0, bheight * 0.75, baseSize * 0.125, baseSize * 0.125, "/WaterMark;component/Assets2/icons/trip/Latitude.png"));
            this.previewBMP();
        }
        private void tripEndButton_Click(object sender, RoutedEventArgs e)
        {
            var messagePrompt = new InputPrompt
            {
                Title = "From"+distanceBeginName+"to here",
                Message = "Name of here"  
            }; 
            messagePrompt.Completed += tripEndInputed;
            messagePrompt.Show();
        }
        private void tripEndInputed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            if (e.PopUpResult == PopUpResult.Ok)
            {
                if (e.Result.Trim().Length == 0)
                {
                    showToast("Please input the name of start~");
                }
                else
                {
                    distanceEndName = e.Result;
                }
            }
            else if (e.PopUpResult == PopUpResult.Cancelled) { }
            else { } 
            isTripBegin = false;
            tripEndPreview();
        }
        private void tripEndPreview()
        {
            clearItemOnBMP();
            // 添加出发的单模板
            double baseSize = (wb.PixelHeight > wb.PixelWidth) ? wb.PixelWidth : wb.PixelHeight;//选定乘的基数，以小的为准
            var sendoff_time_text = new WTextBlock(0, bheight * 0.74 - baseSize * 0.05, string.Format("{0:D4}/{1}/{2} {3}:{4:D2} ", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute) + m_weather.districtName, baseSize * 0.05, "Segoe WP SimeLight", 0, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Center);
            var sendoff_text = new WTextBlock(baseSize * 0.13, bheight * 0.755, "Get " + distanceEndName, baseSize * 0.08, "Segoe WP", 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A);
            var fromBegining_text = new WTextBlock(0, bheight * 0.75+baseSize*0.125, " as " + distanceBeginName + WSpeed.GetDistanceGeodesicString(beginLatitude, m_weather.EorW + ',' + m_weather.NorS), baseSize * 0.06, "Segoe WP", 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A);
            double backLength = (sendoff_time_text.tb.ActualWidth + bheight * 0.0625) > (sendoff_text.tb.ActualWidth + bheight * 0.13) ? sendoff_time_text.tb.ActualWidth + bheight * 0.0625 : sendoff_text.tb.ActualWidth + bheight * 0.13;
            backLength = (backLength) > (fromBegining_text.tb.ActualWidth) ? backLength : fromBegining_text.tb.ActualWidth;
            sendoff_time_text.tb.Width = backLength;
            fromBegining_text.tb.Width = backLength;
            var backTime = new Shapw(0, bheight * 0.75 - baseSize * 0.07, Shapw.wShapes.Rectangle, 0, 0, backLength, baseSize * 0.07);
            backTime.setFillBrush(0, 0, 0, (byte)100);
            var backText = new Shapw(0, bheight * 0.75, Shapw.wShapes.Rectangle, 0, 0, backLength, baseSize * 0.2);
            backText.setFillBrush(0, 0, 0, (byte)60);
            elementList.Add(backTime);
            elementList.Add(backText);
            elementList.Add(sendoff_time_text);
            elementList.Add(sendoff_text);
            elementList.Add(fromBegining_text);
            elementList.Add(new Imagew(0, bheight * 0.75, baseSize * 0.125, baseSize * 0.125, "/WaterMark;component/Assets2/icons/trip/Latitude.png"));
            this.previewBMP(); 
        }

        //--模板6 顶部横幅：时间 气温 城市  //使用了elementlist中的打印图片的接口，以后要使用该接口   id=6
        private void template6Preview()
        {
            inputBox.Text = templateInputs[6];
            //添加水印文字--时间
            string time_text = string.Format("{0:D4}.{1:D2}.{2:D2} {3:D2}:{4:D2}", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute) + "  " + m_weather.districtName + "  " + (editGridCheck.IsChecked == true ? (signature) : "");
            string time_text2 = (m_weather.tempratureToday.Length > 0 ? m_weather.tempratureToday + "℃  " : "")+m_weather.weatherText;
            double fontSize0 = (bheight * 0.05 * time_text.Length > bwidth ? bwidth / (time_text.Length + 2) : bheight * 0.05);
            double fontSize1 = fontSize0 * 1.5;
            double yPoint = bheight;
            //添加水印文字--自定义输入 将回车和换行转为空格
            string inputSentence = inputBox.Text.Replace('\r', '\n'); 
            double xPoint = bwidth;
            //时间 签名
            var timeSignEle = new WTextBlock(bwidth * 0.13, xPoint = 0.005 * bheight, time_text, fontSize0, fontName, 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A);
            //气温 自定义文字
            var userEle = new WTextBlock(bwidth * 0.13, xPoint += fontSize0 * 1.5, time_text2 + inputSentence, fontSize1, "Segoe WP", 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Left, bwidth * 0.84);
            Shapw inputbg = new Shapw(0, 0, Shapw.wShapes.Rectangle, 0, 0, bwidth, xPoint+userEle.tb.ActualHeight+0.005*bheight);//黑框
            inputbg.setBrush(0, 0, 0, 80);
            this.elementList.Add(inputbg);//黑框
            this.elementList.Add(timeSignEle);//时间 签名
            this.elementList.Add(userEle);//气温 自定义文字
            this.elementList.Add(new Imagew(-0.02 * bheight, -0.02 * bheight, bwidth * 0.16, bwidth * 0.16, m_weather.getMarkPic())); 
            this.previewBMP();
            loadEditGrid("fontcolor,roate,textedit,editcheck");
            this.editGridCheck.Content = "Sign";
            templateInputs[6] = inputBox.Text;
        }

        //--模版7预览——安 系列  id=7
        private void template7Preview()
        {
            //添加水印文字--时间
            this.elementList.Add( new WTextBlock( bwidth * 0.25, bheight - bwidth * 0.06 - 20, string.Format("{0:D4}/{1}/{2} {3}:{4:D2} {5}", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute, (m_weather.tempratureToday_full.Length>0?(m_weather.tempratureToday+"℃"):""))+" "+(showWPLogo?DeviceStatus.DeviceName.ToString():""), bwidth * 0.04, "Segoe WP", 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            //添加水印文字--地点
            this.elementList.Add( new WTextBlock( bwidth * 0.25, bheight - bwidth * 0.115 - 20, m_weather.districtName + "-" + m_weather.weatherToday_full,bwidth * 0.052, "Segoe WP", 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A) );
            //添加水印文字--晚安
            int curr_hour = DateTime.Now.ToLocalTime().Hour;
            string greetmark = "";
            if (curr_hour >= 5 && curr_hour < 7) greetmark = "☻AM";
            else if (curr_hour >= 7 && curr_hour < 12) greetmark = "☻AM";
            else if (curr_hour >= 12 && curr_hour < 17) greetmark = "☻PM";
            else if (curr_hour >= 17 && curr_hour < 19) greetmark = "☺PM";
            else if (curr_hour >= 19 && curr_hour < 24) greetmark = "☺PM";
            else if (curr_hour >= 0 && curr_hour < 3) greetmark = "☺PM";
            else greetmark = "☺PM";//(curr_hour >= 3 && curr_hour < 5)
            if (greetmark.Length > 2) greetmark = greetmark.Substring(greetmark.Length - 2);
            this.elementList.Add(new WTextBlock(bwidth * 0.02, bheight - bwidth * 0.12 - 20, greetmark, bwidth * 0.095, fontName, 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            this.previewBMP();
            loadEditGrid();
        }
        //   竖 安  id=14
        private void template7Preview1()
        {
            inputBox.Text = templateInputs[14];
            //添加水印文字--时间
            string time_text = string.Format("{0:D4}.{1:D2}.{2:D2} {3:D2}:{4:D2}", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute) + "  " + m_weather.districtName + "  " + (editGridCheck.IsChecked == true ? (signature) : "");
            string time_text2 = (m_weather.tempratureToday.Length > 0 ? m_weather.tempratureToday + "℃  " : "");
            double fontSize0 = (bheight * 0.05 * time_text.Length > bwidth ? bwidth / (time_text.Length + 2) : bheight * 0.05);
            double fontSize1 = fontSize0 * 1.5;
            double yPoint = bheight;
            //添加水印文字--自定义输入 将回车和换行转为空格
            string inputSentence = inputBox.Text.Replace('\r', '\n');
            double xPoint = bwidth;
            //时间 签名
            var timeSignEle = new WTextBlock(bwidth * 0.13, xPoint = 0.005 * bheight, time_text, fontSize0, fontName, 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A);
            //气温 自定义文字
            var userEle = new WTextBlock(bwidth * 0.13, xPoint += fontSize0 * 1.5, time_text2 + inputSentence, fontSize1, "Segoe WP", 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Left, bwidth * 0.84);
            Shapw inputbg = new Shapw(0, 0, Shapw.wShapes.Rectangle, 0, 0, bwidth, xPoint + userEle.tb.ActualHeight + 0.005 * bheight);//黑框
            inputbg.setBrush(0, 0, 0, 80);
            this.elementList.Add(inputbg);//黑框
            this.elementList.Add(timeSignEle);//时间 签名
            this.elementList.Add(userEle);//气温 自定义文字
            this.elementList.Add(new Imagew(-0.02 * bheight, -0.02 * bheight, bwidth * 0.16, bwidth * 0.16, m_weather.getMarkPic()));
            this.previewBMP();
            loadEditGrid("fontcolor,roate,textedit,editcheck");
            this.editGridCheck.Content = "Sign";
            templateInputs[6] = inputBox.Text;
        }

        //--模版8预览--详细地址  id=8
        private void template8Preview()
        {
            //添加水印文字--详细地址
            string tem_locations = m_weather.districtName +" " + m_weather.cityName ;
            if (tem_locations.Length == 0) tem_locations = "I'm here!";
            double fontSize0 = (bheight * 0.07 * tem_locations.Length > bwidth ? bwidth / (tem_locations.Length+2) : bheight * 0.07);
            this.elementList.Add(new WTextBlock(bwidth * 0.01, bheight*0.89-fontSize0*1.8, tem_locations, fontSize0, fontName/*"Segoe WP Bold"*/, 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A,TextAlignment.Center,bwidth*0.98));
            this.elementList.Add(new WTextBlock(0, bheight * 0.94 - fontSize0, string.Format("{0:D4}/{1}/{2} {3:D2}:{4:D2} {5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.DayOfWeek), fontSize0 * 0.8, "Segoe WP SemiLight", 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A, TextAlignment.Center, bwidth));
            Image pic = iconUser;
            TranslateTransform transicon = new TranslateTransform();
            if (isIconAvailable == false || iconUser.Source == null)
            {
                pic = new Image();
                BitmapImage bmpic = new BitmapImage(new Uri("/WaterMark;component/Assets2/icons/emotions/Smile.png", UriKind.Relative));
                pic.Source = bmpic;
                //if (noWarning == false) showToast("先在菜单中加载图章才能显示图章");
            }
            double widthDHight = pic.Width / pic.Height;
            pic.Height = bheight * 0.14;
            pic.Width = (pic.Height * widthDHight > wb.PixelWidth * 0.5) ? wb.PixelWidth * 0.5 : pic.Height * widthDHight;
            pic.Stretch = Stretch.Fill;
            transicon.X = bwidth*0.5-bheight*0.08;
            transicon.Y = bheight*0.74-fontSize0*1.8;
            wb.Render(pic, transicon);
            //预览
            this.previewBMP();
            loadEditGrid("fontcolor,roate,myicon");
        }

        //--模板9预览--空气质量 
        // 右下方出现矩形和指数 id=14
        private void template9Preview()
        {
            string itemName = templateInputs[14];
            // 添加图片
            int baseSize = bheight > bwidth ? bwidth : bheight;
            this.elementList.Add(new Imagew(bwidth - baseSize * 0.3, bheight - baseSize * 0.45, baseSize * 0.5, baseSize * 0.31, this.m_weather.getAQIImage(itemName)));
            //添加水印文字--时间
            this.elementList.Add(new WTextBlock(0, bheight - baseSize * 0.35, this.m_weather.getAQIValue(itemName)+" ", fontColor, baseSize * 0.15, "SegoeWP", 2,TextAlignment.Right,bwidth));
            string time_text = string.Format("{0:D4}.{1:D2}.{2:D2} {3:D2}:{4:D2}", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute) + " " + m_weather.districtName;
            //double fontSize0 = (bheight * 0.05 * time_text.Length > bwidth ? bwidth / (time_text.Length - 4) : bheight * 0.05);
            //double fontSize1 = fontSize0 * 1.25;
            //double yPoint = bheight;
            ////添加水印文字--自定义输入 将回车和换行转为空格 
            //double xPoint = bwidth;
            //Shapw inputbg = new Shapw(0, 0, Shapw.wShapes.Rectangle, 0, 0, bwidth, fontSize0 * 3);
            //inputbg.setBrush(0, 0, 0, 80);
            //this.elementList.Add(inputbg);
            ////时间 签名
            if(this.editGridCheck.IsChecked==true) this.elementList.Add(new WTextBlock(0, bheight - baseSize*0.13, time_text.ToString(), baseSize*0.06, "Segoe WP SemiLight", 1, fontColor.R, fontColor.G, fontColor.B, fontColor.A,TextAlignment.Right,bwidth));
            ////分贝 自定义文字
            //this.elementList.Add(new WTextBlock(bwidth * 0.13, xPoint += fontSize1, getVoiceText(averageDB), fontSize1, "Segoe WP", 2, fontColor.R, fontColor.G, fontColor.B, fontColor.A));
            //this.elementList.Add(new Imagew(-0.02 * bheight, -0.02 * bheight, bwidth * 0.16, bwidth * 0.16, this.getVoiceIMG(averageDB)));
            this.loadEditGrid("fontcolor,roate,aqiswitch,editcheck");
            this.editGridCheck.Content = "时间";
            //预览
            this.previewBMP();
        }

        // 地图模板预览
        /*private void template5Preview()
        { 
            if (clearItemOnBMP() == false) return;
            // 添加地图印章 
            printIcon(bwidth * 0.2, bwidth * 0.25, 0, 0, "http://api.map.baidu.com/staticimage?center=116.403874,39.914888&width=600&height=200&zoom=15");

            //添加水印文字--时间
            string time_text = string.Format("{0:D4}.{1:D2}.{2:D2} {3:D2}:{4:D2}  ", DateTime.Now.ToLocalTime().Year, DateTime.Now.ToLocalTime().Month, DateTime.Now.ToLocalTime().Day, DateTime.Now.ToLocalTime().Hour, DateTime.Now.ToLocalTime().Minute);
            time_text += m_weather.cityName;
            double time_size = bwidth / time_text.Length;
            double time_x = bwidth * 0.27;
            double time_y = bheight * 0.96 - time_size;
            if (time_y < 0)
            {
                if (noWarning == false) showToast("图片宽高比例失调");
                time_y = 10;
            }
            this.elementList.Add(new WTextBlock(time_x, time_y, time_text, time_size, "Segoe WP SemiLight", 0, fontColor.R, fontColor.G, fontColor.B, fontColor.A));

            //添加水印文字--签名
            double sign_size = bwidth * 0.6 / 14;
            double sign_x = bwidth * 0.27;
            double sign_y = time_y - bheight * 0.01 - sign_size;
            if (sign_y < 0)
            {
                if (noWarning == false) showToast("图片宽高比例失调");
                sign_y = 10;
            }
            if (this.signature.Length > 0)
            {
                this.elementList.Add(new WTextBlock(sign_x, sign_y, signature, sign_size, "Segoe WP", 2, fontColor.R, fontColor.G, fontColor.B, 255));
            }
            if (sign_y < 0)
            {
                if (noWarning == false) showToast("图片宽高比例失调");
                sign_y = 10;
            }
            //预览
            this.previewBMP();
            loadEditGrid("fontcolor,roate,myicon");
        } */ 

        /***********************************工具函数***********************************/

        /// 选择模版
        // 将模板序号对应模板的id
        void setTemplatePreview(bool isSaving=false,bool showTemplateName=true) //isSaving 有时需要解决同步的问题
        {
            if (clearItemOnBMP() == false) return;
            if (isLastSavedPic == true) return;
            int chooseNum = templateCount % (TemplateAmount+1);
            if (showTemplateName == true) printTemplateName();
            if (chooseNum == 0) { timeTemplatePreview(); }
            else if (chooseNum == 1) { template1Preview(); }
            else if (chooseNum == 2) { template2Preview(isSaving); }
            else if (chooseNum == 3) {  template3Preview();   }
            else if (chooseNum == 4) { template4Preview(); }
            else if (chooseNum == 5) { template5Preview(); }
            else if (chooseNum == 6) { template6Preview(); }
            else if (chooseNum == 7) { template7Preview(); }
            else if (chooseNum == 8) { template8Preview(); } 
            else if (chooseNum == 9) { template3Preview2(); }
            else if (chooseNum == 10) { template2Preview(isSaving); }
            else if (chooseNum == 11) { timeTemplatePreview1(); }
            else if (chooseNum == 12) { timeTemplatePreview2(); }
            else if (chooseNum == 13) { template1Preview2(); }
            else if (chooseNum == 14) { /*template9Preview();*/ }
            else if (chooseNum == 15) { timeTemplatePreview3(); }
            else { timeTemplatePreview(); chooseNum = 0; }
            showProcessStep(chooseNum);
        }
        
        // 手势 切换模板
        private void imageGesture_DragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            if (backFlag != 1 && backFlag!=5)
            {
                showToast("New photo or choose from album");
                return;
            } 
            if (e.HorizontalChange > 15)
            {
                if ((e.VerticalChange > 0 && e.HorizontalChange > e.VerticalChange) || (e.VerticalChange <= 0 && e.HorizontalChange + e.VerticalChange > 0))
                { gestInt = 1; }
            }
            else if (e.HorizontalChange < -15)
            {
                if ((e.VerticalChange > 0 && e.HorizontalChange + e.VerticalChange < 0) || (e.VerticalChange <= 0 && e.VerticalChange > e.HorizontalChange))
                { gestInt = -1; }
            }
            //else gestInt = 0;
        }
        private void imageGesture_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            if (isLastSavedPic == true || isTemplateSlide == false || m_photo.Visibility == System.Windows.Visibility.Collapsed)//已被保存或处于非可切换状态时
            {
                return;
            }
            if (m_weather == null)
            {
                if (loadpauseGrid.Visibility == System.Windows.Visibility.Visible) return;
                m_weather = new WeatherSetting();
                this.m_weather.autoLocating();
                loadInfoGrid();
                return;
            }
            if (gestInt > 0)
            {
                --templateCount;//模板个数减一
                if (realTimeCanvas.Visibility == System.Windows.Visibility.Visible)//实时取景中不显示的模板==图块
                {
                    getPreRealAbleTemp();
                }
                if (templateCount <= 0) templateCount += TemplateAmount + 1;
            }
            else if (gestInt < 0)
            {
                templateCount++;
                if (realTimeCanvas.Visibility == System.Windows.Visibility.Visible)//实时取景中不显示的模板==图块
                {
                    getNetxRealAbleTemp();
                }
            }
            setTemplatePreview();
            if (gestInt > 0)
            {
                animation_m_photo_right.Begin();
            }
            else if (gestInt < 0)
            {
                animation_m_photo_left.Begin();
            }
            gestInt = 0;
        }
        private void imageGesture_DragStarted(object sender, DragStartedGestureEventArgs e)
        {
            gestInt = 0;
        }
        //----辅助 
        // 避免出现不可以在实时取景中出现的模板在上次保存之后继续被预览==获取可以被预览的下一个模板
        // 另一个出线此情况的函数是手势切换完成的imageGesture_DragCompleted函数，该函数跳过这种情况去上一个或下一个
        private void getNetxRealAbleTemp()
        {
            int chooseNum = templateCount % (TemplateAmount + 1);
            while (true)
            {
                if (chooseNum == 3 || chooseNum == 9)
                {
                    ++templateCount;
                    chooseNum = templateCount % (TemplateAmount + 1);
                }
                else break;
            }
        }
        private void getPreRealAbleTemp()
        {
            int chooseNum = templateCount % (TemplateAmount + 1);//0是简单模板1，不会出现小于零的状态
            while (true)
            {
                if (chooseNum == 3 || chooseNum == 9)
                {
                    --templateCount;
                    chooseNum = templateCount % (TemplateAmount + 1);
                }
                else break;
            }
        }
        //实时取景时用于聚焦
        private void imageGesture_DoubleTap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            //if (editGrid.Visibility == System.Windows.Visibility.Collapsed)
            //{
            //    showEditGrid();
            //    animation_editGrid_show.Begin();
            //}
            //else
            //{ 
            //    animation_editGrid_hide.Begin();
            //    hideEditGrid();
            //}
            //photoZoomUp();
            if (realTimeCanvas.Visibility == System.Windows.Visibility.Visible)
            {
                showToast("Please set focus",3);
                m_photo.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        // 模板选择栏--与手势切换重叠，会暂停手势切换
        private void templateSelectNameGrid_LostFocus(object sender, RoutedEventArgs e){ }
        private void selectNameButtonClick(object sender, RoutedEventArgs e)
        {
            if (isLastSavedPic == true || m_photo.Visibility == System.Windows.Visibility.Collapsed)//已被保存或处于非可切换状态时
            {
                showToast("Please choose new photo from camera or album");
                return;
            }
            if (m_weather == null)
            {
                if (loadpauseGrid.Visibility == System.Windows.Visibility.Visible) return;
                m_weather = new WeatherSetting();
                this.m_weather.autoLocating();
                loadInfoGrid();
                return;
            }
            if (sender.Equals(templateButton0)) templateCount = 0;
            else if (sender.Equals(templateButton1)) templateCount = 1;
            else if (sender.Equals(templateButton2)) templateCount = 2;
            else if (sender.Equals(templateButton3)) templateCount = 3;
            else if (sender.Equals(templateButton4)) templateCount = 4;
            else if (sender.Equals(templateButton5)) templateCount = 5;
            else if (sender.Equals(templateButton6)) templateCount = 6;
            else if (sender.Equals(templateButton7)) templateCount = 7;
            else if (sender.Equals(templateButton8)) templateCount = 8;
            else templateCount = 0;
            templateSelectNameGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.isTemplateSlide = true;
            setTemplatePreview();
            //animation_m_photo_both.Begin();
        }
        private void SelectNameTmp_Click(object sender, EventArgs e)
        {
            if (m_weather == null)
            {
                if (loadpauseGrid.Visibility == System.Windows.Visibility.Visible) return;
                m_weather = new WeatherSetting();
                this.m_weather.autoLocating();
                loadInfoGrid();
                return;
            }
            if (templateSelectNameGrid.Visibility == System.Windows.Visibility.Collapsed)
            {
                templateSelectNameGrid.Visibility = System.Windows.Visibility.Visible;
                isTemplateSlide = false;
                animation_templateselectlist.Begin();
            }
            else
            {
                templateSelectNameGrid.Visibility = System.Windows.Visibility.Collapsed;
                isTemplateSlide = true;
            }
        } 
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
        // 将图片加载到图片上_可以被Imagew代替==只被用于添加图章
        private void printIcon(double height, double width, double x, double y, string piclocation = "")
        { 
            TranslateTransform transicon = new TranslateTransform();
            Image pic = null;
            string location = piclocation;
            if (location.Trim().Length == 0 && (isIconAvailable == false || iconUser == null))
            {
                pic = new Image();
                BitmapImage bmpic = new BitmapImage(new Uri("/WaterMark;component/Assets2/icons/emotions/Smile2.png", UriKind.Relative));
                location = "/WaterMark;component/Assets2/icons/emotions/Smile2.png";
                //if (noWarning == false) showToast("先在菜单中加载图章才能显示图章");
            }
            if (location.Trim().Length == 0) //加载图章
            { 
                pic = iconUser; 
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
        // 清除已添加的元素
        private bool clearItemOnBMP(bool closeDevice=true)
        {
            if (bmp == null) return false;
            if (closeDevice == true) { closeDevices(); }//清楚被占用的设备
            if (wb_source == null) wb_source = new WriteableBitmap(bmp);
            wb = new WriteableBitmap(wb_source);
            elementList.Clear();
            return true;
        }
        // 停用正在使用的设备或委托 如模板7的麦克风
        public void closeDevices()
        {
            if (microphone.State == MicrophoneState.Started)
            {
                microphone.Stop();
                microphone.BufferReady -= new EventHandler<EventArgs>(microphone_BufferReady);
            }
        }

        ///提示信息
        //加载信息并前置，防止其他控件被点击 12秒+3秒错误提示
        private double gridTimeout;
        private bool isShowAppBar = true;
        public void loadInfoGrid(bool showGrid=true)
        {
            gridTimeout = 0.0;
            DispatcherTimer tmr = new DispatcherTimer();
            tmr.Interval = TimeSpan.FromSeconds(0.1);
            if(showGrid==true) loadpauseGrid.Visibility = System.Windows.Visibility.Visible;
            this.ApplicationBar.IsVisible = false;
            isTemplateSlide = false;
            tmr.Tick += loadGridTimeTick;
            tmr.Start();
        }
        void loadGridTimeTick(object sender, EventArgs e)
        {
            gridTimeout += 0.1;
            if (m_weather.isInfoEnough())
            {
                this.loadpauseGrid.Visibility = System.Windows.Visibility.Collapsed;
                setTemplatePreview();
                if (this.indexPageCanvas.Visibility == System.Windows.Visibility.Visible || this.realCameraAppBarGrid.Visibility == System.Windows.Visibility.Visible) this.ApplicationBar.IsVisible = false;
                else this.ApplicationBar.IsVisible = true;
                isTemplateSlide = true;
                if (m_weather != null && m_weather.EorW.Trim().Length != 0 && m_weather.NorS.Trim().Length != 0)
                {
                    try
                    {
                        this.indexADControl.Latitude = double.Parse(m_weather.NorS);
                        this.indexADControl.Longitude = double.Parse(m_weather.EorW);
                    }
                    catch (Exception) { }
                }
                (sender as DispatcherTimer).Stop();
            }
            if (m_weather.isError == true)
            {
                if (noWarning == false) showToast("Info loaded failed.");
            }
            if (gridTimeout < 12.0)
            {
                gridTimeout += 0.1;
            }
            else
            {
                if (noWarning == false) showToast("Not enough info to show everything.");
                this.loadpauseGrid.Visibility = System.Windows.Visibility.Collapsed;
                if (this.indexPageCanvas.Visibility == System.Windows.Visibility.Visible || this.realCameraAppBarGrid.Visibility == System.Windows.Visibility.Visible) this.ApplicationBar.IsVisible = false;
                else this.ApplicationBar.IsVisible = true;
                isTemplateSlide = true;
                if (m_weather != null && m_weather.EorW.Trim().Length != 0 && m_weather.NorS.Trim().Length != 0)
                {
                    try
                    {
                        this.indexADControl.Latitude = double.Parse(m_weather.NorS);
                        this.indexADControl.Longitude = double.Parse(m_weather.EorW);
                    }
                    catch (Exception) { }
                }
                (sender as DispatcherTimer).Stop();
            }
        } 
        // 刷新信息按钮
        private void InfoFlashButton_Click(object sender, EventArgs e)
        { 
            if (InfoFlashTimeout > 12.0)
            {
                flashInfo();
                m_weather = new WeatherSetting();
                m_weather.autoLocating();
            }
        }
        //加载信息，其他控件可以被点击 12秒+3秒错误提示
        private double InfoFlashTimeout = 15.0;
        public void flashInfo()
        {
            InfoFlashTimeout = 0.0;
            DispatcherTimer tmr = new DispatcherTimer();
            tmr.Interval = TimeSpan.FromSeconds(0.1);
            tmr.Tick += InfoFlashTimeoutTimeTick;
            systemTrayToast("Loading,please wait");
            tmr.Start();
        }
        void InfoFlashTimeoutTimeTick(object sender, EventArgs e)
        {
            InfoFlashTimeout += 0.1;
            if (m_weather.isInfoEnough())
            {
                this.loadpauseGrid.Visibility = System.Windows.Visibility.Collapsed;
                setTemplatePreview();
                closeSystemTrayToast();
                Dispatcher.BeginInvoke(delegate()
                {
                    if (isLastSavedPic == false) setTemplatePreview();
                });
                InfoFlashTimeout = 15.0;
                if (m_weather != null && m_weather.EorW.Trim().Length != 0 && m_weather.NorS.Trim().Length != 0)
                {
                    try
                    {
                        this.indexADControl.Latitude = double.Parse(m_weather.NorS);
                        this.indexADControl.Longitude = double.Parse(m_weather.EorW);
                    }
                    catch (Exception) { }
                }
                (sender as DispatcherTimer).Stop();
            }
            if (m_weather.isError == true)
            {
                if (noWarning == false) showToast("Info loaded failed.");
            }
            if (InfoFlashTimeout < 12.0)
            {
                InfoFlashTimeout += 0.1;
            }
            else
            {
                showToast("Not enough info to show everything.");
                closeSystemTrayToast();
                Dispatcher.BeginInvoke(delegate()
                {
                    if (isLastSavedPic == false) setTemplatePreview();
                });
                InfoFlashTimeout = 15.0;
                if (m_weather != null && m_weather.EorW.Trim().Length != 0 && m_weather.NorS.Trim().Length != 0)
                {
                    try
                    {
                        this.indexADControl.Latitude = double.Parse(m_weather.NorS);
                        this.indexADControl.Longitude = double.Parse(m_weather.EorW);
                    }
                    catch (Exception) { }
                }
                (sender as DispatcherTimer).Stop();
            } 
        }
        // 显示Toast，等待5秒后 “第二次点击”标记量变为false
        private double toastTimeout;
        public void showToast(string title, double timeout = 5.0, bool isWaitingSecond = false, int waitingEventID = 0)
        {
            try
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
            catch (Exception) { }
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

        // 提示模板名称
        public void printTemplateName()
        {
            if (noWarning == true) return;
            switch (templateCount)
            {
                case 0: showToast("Slide the picture to left or right to switch", 3.1); break;
                //case 4: showToast("图章模板", 2.0); break;
                //case 2: showToast("分贝模板", 2.0); break;
                default: break;
            }
        }
        // 模板个数标记
        private void showProcessStep(int templateNum)
        {
            processStepGrid.Visibility = System.Windows.Visibility.Visible;
            SolidColorBrush currentBrush = new SolidColorBrush(new Color() { R = 46, G = 46, B = 46, A = 154 });
            SolidColorBrush waitingBrush = new SolidColorBrush(new Color() { A = 72, B = 46, G = 46, R = 46 });
            processStep0.Fill = waitingBrush;
            processStep1.Fill = waitingBrush;
            processStep2.Fill = waitingBrush;
            processStep3.Fill = waitingBrush;
            processStep4.Fill = waitingBrush;
            processStep5.Fill = waitingBrush;
            processStep6.Fill = waitingBrush;
            processStep7.Fill = waitingBrush;
            processStep8.Fill = waitingBrush;
            processStep9.Fill = waitingBrush;
            processStep10.Fill = waitingBrush;
            processStep11.Fill = waitingBrush;
            processStep12.Fill = waitingBrush;
            processStep13.Fill = waitingBrush;
            processStep14.Fill = waitingBrush;
            processStep15.Fill = waitingBrush;
            switch (templateNum)
            {
                case 0: processStep0.Fill = currentBrush; break;
                case 1: processStep1.Fill = currentBrush; break;
                case 2: processStep2.Fill = currentBrush; break;
                case 3: processStep3.Fill = currentBrush; break;
                case 4: processStep4.Fill = currentBrush; break;
                case 5: processStep5.Fill = currentBrush; break;
                case 6: processStep6.Fill = currentBrush; break;
                case 7: processStep7.Fill = currentBrush; break;
                case 8: processStep8.Fill = currentBrush; break;
                case 9: processStep9.Fill = currentBrush; break;
                case 10: processStep10.Fill = currentBrush; break;
                case 11: processStep11.Fill = currentBrush; break;
                case 12: processStep12.Fill = currentBrush; break;
                case 13: processStep13.Fill = currentBrush; break;
                case 14: processStep14.Fill = currentBrush; break;
                case 15: processStep15.Fill = currentBrush; break;
                default: processStep0.Fill = currentBrush; break;
            }
        }
        private void closeProcessStep()
        {
            processStepGrid.Visibility = System.Windows.Visibility.Collapsed;
        }
        
        ///保存照片
        //根据List记录的文本生成预览
        //生成预览后 wb 即为目标图像，直接将其保存而无需重新绘制
        void previewBMP()
        {
            try
            {
                if (elementList.Capacity == 0)
                {
                    if (noWarning == false) showToast("No changes saved.");
                    return;
                }
                foreach (Object element in this.elementList) // 将各个元素添加到列表中
                {
                    if (element != null)
                        addItemToBMP(element);
                }
                wb.Invalidate(); // 保存修改，并显示到UI
                m_photo.Source = wb;
                isLastSavedPic = false;
            }
            catch (Exception) { }
        }
        void previewRealBMP()
        {
            try
            {
                if (elementList.Capacity == 0)
                {
                    if (noWarning == false) showToast("No changes saved.");
                    return;
                }
                foreach (Object element in this.elementList) // 将各个元素添加到列表中
                {
                    if (element != null)
                        addItemToBMP(element);
                }
                wb.Invalidate(); // 保存修改，并显示到UI
                isLastSavedPic = false;
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
                    systemTrayToast("Saving,please wait a second...");
                    using (MemoryStream myFileStream = new MemoryStream())// "tempJPEG.jpg", FileMode.Create, myStore))//myStore.OpenFile("tempJEPG.jpg", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (myFileStream == null)
                        {
                            if (noWarning == false) showToast("Saved failed.");
                            return;
                        }
                        //jepg编码，将wb存储成jepg
                        wb.SaveJpeg(myFileStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                        myFileStream.Seek(0, SeekOrigin.Begin);
                        Picture pic = library.SavePicture(string.Format("water_{0}{1}{2}{3}{4}{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), myFileStream);
                        isLastSavedPic = true;
                        closeEditGrid();
                        closeToast();
                        backFlag = 4;//保存完成
                    }
                }
                else if (noWarning == false) showToast("Please start a new task");
            }
            catch (Exception)
            {
                try
                {
                    using (MemoryStream myFileStream = new MemoryStream())// "tempJPEG.jpg", FileMode.Create, myStore))//myStore.OpenFile("tempJEPG.jpg", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (myFileStream == null)
                        {
                            if (noWarning == false) showToast("Saved failed,please try again");
                            return;
                        }
                        wb_source.SaveJpeg(myFileStream, wb_source.PixelWidth, wb_source.PixelHeight, 0, 100);
                        myFileStream.Seek(0, SeekOrigin.Begin);
                        MediaLibrary library = new MediaLibrary();
                        Picture pic = library.SavePicture(string.Format("photo_{0}{1}{2}{3}{4}{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), myFileStream);
                        if (noWarning == false) showToast("Saved failed, the original one saved.");
                    }
                }
                catch (Exception) { if (noWarning == false) showToast("Exception occurs during saving"); }
            }
            closeSystemTrayToast();
        }
        // 按钮 确定 将水印输出到相片上，并导航到 分享页
        private void confirmMark(object sender, EventArgs e)
        {
            if (backFlag != 1 && backFlag != 4 && backFlag != 5)
            {
                showToast("Please take a new photo or choose from album");
                return;
            }
            if (isLastSavedPic == false)
            {
                setTemplatePreview(true);
                closeDevices();
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
                catch (Exception) { showToast("Decoding error and failed to share."); isLastSavedPic = false; return; }
            }
            NavigationService.Navigate(new Uri("/share/SharePage.xaml", UriKind.Relative));
        }

        // 拼接长图
        private void makeBaomanButtonClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Funs/BaoManPage.xaml", UriKind.Relative)); 
        }


        /******************************组件函数****************************************/
        /// 编辑栏的相关操作
        // buttonList 参数：fontcolor 字体颜色按钮 roate 旋转按钮 textedit 第三个格文字编辑按钮   myicon 图章 editcheck 复选框 hide 按钮总存在
        // tripbegin旅行开始，tripend旅行结束，backcolor背景色，text2edit第二个格出现编辑按钮
        //Oh. bad programming,I have changed it to a better style in later version
        private void loadEditGrid(string buttonList = "fontcolor,roate")
        {
            hideEditGrid();
            //this.showEditGridButton.Visibility = System.Windows.Visibility.Visible; 
            // 加载 字体颜色 按钮
            if (buttonList.ToLower().Contains("fontcolor") == true)
            {
                fontColorButton.Visibility = System.Windows.Visibility.Visible;
                fontColorButtonToast.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                fontColorButton.Visibility = System.Windows.Visibility.Collapsed;
                fontColorButtonToast.Visibility = System.Windows.Visibility.Collapsed;
            }
            // 加载 旋转图片 按钮
            if (buttonList.ToLower().Contains("roate") == true)
            {
                roatePICButton.Visibility = System.Windows.Visibility.Visible;
                roatePICButtonToast.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                roatePICButton.Visibility = System.Windows.Visibility.Collapsed;
                roatePICButtonToast.Visibility = System.Windows.Visibility.Collapsed;
            }
            // 加载 （第三个格） 修改文字 按钮
            if (buttonList.ToLower().Contains("textedit") == true)
            {
                textEditButton.Visibility = System.Windows.Visibility.Visible;
                textEditButtonToast.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                textEditButton.Visibility = System.Windows.Visibility.Collapsed;
                textEditButtonToast.Visibility = System.Windows.Visibility.Collapsed;
            }
            // 加载 （第二个格） 修改文字 按钮
            if (buttonList.ToLower().Contains("text2edit") == true)
            {
                textEditButton2.Visibility = System.Windows.Visibility.Visible;
                textEditButtonToast2.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                textEditButton2.Visibility = System.Windows.Visibility.Collapsed;
                textEditButtonToast2.Visibility = System.Windows.Visibility.Collapsed;
            }
            // 加载 图章
            if (buttonList.ToLower().Contains("myicon") == true)
            {
                myiconButton.Visibility = System.Windows.Visibility.Visible;
                myiconButtonToast.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                myiconButton.Visibility = System.Windows.Visibility.Collapsed;
                myiconButtonToast.Visibility = System.Windows.Visibility.Collapsed;
            }
            // 加载旅行起点和终点
            if (buttonList.ToLower().Contains("tripbegin") == true)
            {
                tripBeginButton.Visibility = System.Windows.Visibility.Visible;
                tripBeginButtonToast.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                tripBeginButton.Visibility = System.Windows.Visibility.Collapsed;
                tripBeginButtonToast.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (buttonList.ToLower().Contains("tripend") == true)
            {
                tripEndButton.Visibility = System.Windows.Visibility.Visible;
                tripEndButtonToast.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                tripEndButton.Visibility = System.Windows.Visibility.Collapsed;
                tripEndButtonToast.Visibility = System.Windows.Visibility.Collapsed;
            }
            // 背景底色
            if (buttonList.ToLower().Contains("backcolor") == true)
            {
                bgColorButton.Visibility = System.Windows.Visibility.Visible;
                bgColorButtonToast.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                bgColorButton.Visibility = System.Windows.Visibility.Collapsed;
                bgColorButtonToast.Visibility = System.Windows.Visibility.Collapsed;
            }
            // 加载 复选框
            if (buttonList.ToLower().Contains("editcheck") == true)
            {
                editGridCheck.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                editGridCheck.Visibility = System.Windows.Visibility.Collapsed;
            }
            // 加载 AQI指标切换
            if (buttonList.ToLower().Contains("aqiswitch") == true)
            {
                this.aqiSwitchButton.Visibility = System.Windows.Visibility.Visible;
                this.aqiSwitchButtonToast.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.aqiSwitchButton.Visibility = System.Windows.Visibility.Collapsed;
                this.aqiSwitchButtonToast.Visibility = System.Windows.Visibility.Collapsed;
            }
            //highligthShowEditButtonTick = 1;
            //highlitEditShowButton();

        }
        //编写分贝水印文字
        private void fenbeiEditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NavigationService.Navigate(new Uri("/TemplateSetting/FenBeiSetPage.xaml", UriKind.Relative));
            }
            catch (Exception)
            {
                showToast("Setting page error,restart the app.");
            }
        }
        // 显示编辑栏
        private void showEditGrid()
        {
            //this.showEditGridButton.Visibility = System.Windows.Visibility.Collapsed;
            this.editGrid.Visibility = System.Windows.Visibility.Visible;
            animation_editGrid_show.Begin();
        }
        private void showEditGridButton_Click(object sender, RoutedEventArgs e)
        {
            showEditGrid();
        }
        // 隐藏编辑栏
        private void hideEditGrid()
        { 
            //this.showEditGridButton.Visibility = System.Windows.Visibility.Visible;
            this.editGrid.Visibility = System.Windows.Visibility.Collapsed;
            hideAllEditFunctionGrid();
        }
        private void hideAllEditFunctionGrid()
        {
            this.iconSelectGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.backcolorEditGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.inputGrid.Visibility = System.Windows.Visibility.Collapsed;
        }
        private void hideEditGridButton_Click(object sender, RoutedEventArgs e)
        {
            animation_editGrid_hide.Begin();
            hideEditGrid();
        }
        // 关闭编辑栏--退出编辑模式
        private void closeEditGrid()
        {
            this.backcolorEditGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.editGrid.Visibility = System.Windows.Visibility.Collapsed;
            //this.showEditGridButton.Visibility = System.Windows.Visibility.Collapsed;
        }
        // 字体颜色 按钮——导航到 字体颜色 页
        private void fontSettingButtonClick(object sender, RoutedEventArgs e)
        {
            hideAllEditFunctionGrid();
            if ((backFlag != 1 && backFlag != 5) || isLastSavedPic == true)
            {
                showToast("Take a new photo or choose from album");
                return;
            }
            try
            {
                try
                {
                    if(isAlignmentFont()) NavigationService.Navigate(new Uri("/WTool/Colorw2.xaml?useAlignment=true", UriKind.Relative));
                    else NavigationService.Navigate(new Uri("/WTool/Colorw2.xaml?useAlignment=false", UriKind.Relative));
                }
                catch (Exception) { }
            }
            catch (Exception) { }
        }
        private bool isAlignmentFont() //所有设置对齐的模板都要在此注册
        {
            int chooseNum = templateCount % (TemplateAmount + 1);
            if (chooseNum == 0 || chooseNum == 11 || chooseNum == 12) return true;
            else return false;
        }
        // 编辑文字 按钮——显示 文本输入框
        private void textEditButton_Click(object sender, RoutedEventArgs e)
        {
            hideAllEditFunctionGrid();
            inputGrid.Visibility = System.Windows.Visibility.Visible;
            animation_input1_click.Begin();
            isTemplateSlide = false;
            inputBox.Focus();
        }
        private void inputBox_LostFocus(object sender, RoutedEventArgs e)
        {
            inputGrid.Visibility = System.Windows.Visibility.Collapsed;
            templateInputs[templateCount % (TemplateAmount + 1)] = inputBox.Text;
            isTemplateSlide = true;
            setTemplatePreview();
        }
        private void inputBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void timeCheck_Checked(object sender, RoutedEventArgs e)
        {
            setTemplatePreview();
        } 
        private void timeCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            setTemplatePreview();
        }
        //旋转图片,修改 wb, bwidth 和 bheight
        private void roatePIC()
        {
            try
            {
                int bheight0 = bheight - 1;
                int bwith0 = bwidth - 1;
                WriteableBitmap wbTarget = null;
                wbTarget = new WriteableBitmap(wb_source.PixelHeight, wb_source.PixelWidth);
                for (int x = 0; x < wb_source.PixelWidth; x++)
                    for (int y = 0; y < wb_source.PixelHeight; y++)
                        wbTarget.Pixels[(bheight0 - y) + x * wbTarget.PixelWidth] = wb_source.Pixels[x + y * wb_source.PixelWidth];
                wb_source = wbTarget;
                bwidth = wb_source.PixelWidth;
                bheight = wb_source.PixelHeight;
            }
            catch (Exception) { if (System.Diagnostics.Debugger.IsAttached) if (noWarning == false) showToast("Can not roate image"); }
        }
        // 按钮 旋转 
        private void roatePICButton_Click(object sender, RoutedEventArgs e)
        {
            hideAllEditFunctionGrid();
            roatePIC();
            m_photo.Source = wb_source;
            setTemplatePreview();
        }
        
        ///图章
        // 添加图章
        private void addIconTask()
        {
            userIconChooseTask = new PhotoChooserTask();
            userIconChooseTask.Completed += new EventHandler<PhotoResult>(iconChooseTask_complete);
            userIconChooseTask.Show();
        }
        private void iconChooseTask_complete(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                try
                {
                    BitmapImage bimage = new BitmapImage();
                    bimage.SetSource(e.ChosenPhoto);
                    iconUser.Source = bimage;
                    iconUser.Height = bimage.PixelHeight > 0 ? bimage.PixelHeight : 1;
                    iconUser.Width = bimage.PixelHeight > 0 ? bimage.PixelWidth : 1;
                    isIconAvailable = true;
                    ImageBrush iconBrush = new ImageBrush();
                    iconBrush.ImageSource = bimage;
                    myiconButton.Background = iconBrush;
                    hubTileIcon.Source = bimage;
                    //showToast("图章加载成功");
                }
                catch (Exception)
                {
                    iconUser.Source = null;
                    isIconAvailable = false;
                    showToast("Type of icon is not support");
                }
            }
            isTemplateSlide = true;
        }
        private void myiconButton_Click(object sender, RoutedEventArgs e)
        {
            this.backcolorEditGrid.Visibility = System.Windows.Visibility.Collapsed;
            this.inputGrid.Visibility = System.Windows.Visibility.Collapsed;
            if (iconSelectGrid.Visibility == System.Windows.Visibility.Collapsed)
            {
                iconSelectGrid.Visibility = System.Windows.Visibility.Visible;
                animation_iconchoose.Begin();
                isTemplateSlide = false;
            }
            else
            {
                iconSelectGrid.Visibility = System.Windows.Visibility.Collapsed;
                isTemplateSlide = true;
            }
        } 
        //自带表情
        private void emotionButton_Click(object sender, RoutedEventArgs e)
        {
            string iconLocation = "/WaterMark;component/Assets2/icons/emotions/Smile2.png";
            if (sender.Equals(emotionButton1)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Smile.png";
            else if (sender.Equals(emotionButton2)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Happy.png";
            else if (sender.Equals(emotionButton3)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Love.png";
            else if (sender.Equals(emotionButton4)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Wink.png";
            else if (sender.Equals(emotionButton5)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Cool.png";
            else if (sender.Equals(emotionButton6)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Angry.png";
            else if (sender.Equals(emotionButton7)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Cry.png";
            else if (sender.Equals(emotionButton8)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Confused.png";
            else if (sender.Equals(emotionButton9)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Sad.png";
            else if (sender.Equals(emotionButton10)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Sleepy.png";
            else if (sender.Equals(emotionButton11)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Surprised.png";
            else if (sender.Equals(emotionButton12)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Coffee.png";
            else if (sender.Equals(emotionButton13)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Bookmark-New.png";
            else if (sender.Equals(emotionButton14)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Cheeky.png";
            else if (sender.Equals(emotionButton15)) iconLocation = "/WaterMark;component/Assets2/icons/emotions/Speechless.png";
            else iconLocation = "/WaterMark;component/Assets2/icons/emotions/Smile2.png";
            try
            {
                iconUser = new Image();
                BitmapImage bmpic = new BitmapImage(new Uri(iconLocation, UriKind.Relative));
                iconUser.Source = bmpic;
                isIconAvailable = true;
                setTemplatePreview(false,false);
                //this.previewBMP();
            }
            catch (Exception)
            {
                iconUser = null;
                isIconAvailable = false;
            }
            isTemplateSlide = true;
        }
        //选择相册图片
        private void iconGridAlbumButton_Click(object sender, RoutedEventArgs e)
        {
            noWarning = true;
            try { addIconTask(); }
            catch (Exception) { showToast("Icon loaded failed."); }
        }
        //拍照
        private void iconGridCameraButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                icon_cameraCaptureTask.Show();
            }
            catch (Exception) { showToast("Icon loaded failed."); }
        }
        private void icon_cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            try
            {
                if (e.TaskResult == TaskResult.OK)
                {
                    BitmapImage iconBMP = new BitmapImage();
                    iconBMP.SetSource(e.ChosenPhoto);
                    iconUser = new Image();
                    iconUser.Source = iconBMP;
                    isIconAvailable = true;
                }
                else if (e.TaskResult == TaskResult.None) // 加载错误
                {
                    showToast("The type of photo is not supported");
                }
            }
            catch (Exception)
            {
                if (noWarning == false) showToast("Camera cannot open");
            }
            isTemplateSlide = true;
        }

        // AQI模板中切换指标按钮的响应函数
        private void aqiSwitchButton_Click(object sender, RoutedEventArgs e)
        {
            templateInputs[templateCount % (TemplateAmount + 1)] = this.m_weather.getNextAQIName(templateInputs[templateCount % (TemplateAmount + 1)]);
            setTemplatePreview();
        }

        /// 背景颜色设置
        //设置背景按钮
        private void bgColorButton_Click(object sender, RoutedEventArgs e)
        {
            iconSelectGrid.Visibility = System.Windows.Visibility.Collapsed;
            inputGrid.Visibility = System.Windows.Visibility.Collapsed;
            if (this.backcolorEditGrid.Visibility == System.Windows.Visibility.Collapsed)
            {
                this.backcolorEditGrid.Visibility = System.Windows.Visibility.Visible;
                animation_bgChoose.Begin();
                isTemplateSlide = false;
            }
            else
            {
                this.backcolorEditGrid.Visibility = System.Windows.Visibility.Collapsed;
                isTemplateSlide = true;
            }
        }
        //背景块
        private void bgColorSetButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(bgcolor1)) backColor = new Color() { A = 0xFF, R = 0x0C, G = 0x0C, B = 0x0C };
            else if (sender.Equals(bgcolor2)) backColor = new Color() { A = 0xFF, R = 0xF7, G = 0x47, B = 0x47 };
            else if (sender.Equals(bgcolor3)) backColor = new Color() { A = 0xFF, R = 0xFB, G = 0x8F, B = 0x34 };
            else if (sender.Equals(bgcolor4)) backColor = new Color() { A = 0xFF, R = 0x31, G = 0xBD, B = 0xFF };
            else if (sender.Equals(bgcolor5)) backColor = new Color() { A = 0xFF, R = 0x66, G = 0x66, B = 0xFF };
            else if (sender.Equals(bgcolor6)) backColor = new Color() { A = 0xFF, R = 0x99, G = 0x33, B = 0xFF };
            else if (sender.Equals(bgcolor7)) backColor = new Color() { A = 0xFF, R = 0xFF, G = 0xFF, B = 0xFF };
            else if (sender.Equals(bgcolor8)) backColor = new Color() { A = 0xFF, R = 0xFF, G = 0x6A, B = 0x9A };
            else if (sender.Equals(bgcolor9)) backColor = new Color() { A = 0xFF, R = 0xFF, G = 0xFF, B = 0x99 };
            else if (sender.Equals(bgcolor10)) backColor = new Color() { A = 0xFF, R = 0x99, G = 0xCC, B = 0xFF };
            else if (sender.Equals(bgcolor11)) backColor = new Color() { A = 0xFF, R = 0x33, G = 0x99, B = 0x33 };
            else if (sender.Equals(bgcolor12)) backColor = new Color() { A = 0xFF, R = 0xFF, G = 0xCC, B = 0xCC };
            else backColor = new Color() { A = 0xFF, R = 0xFF, G = 0xFF, B = 0xFF };
            setTemplatePreview();
            isTemplateSlide = true;
        }
        //编辑框开关
        private void showBGColorpicker()
        {
            hideAllEditFunctionGrid();
            if (backcolorEditGrid.Visibility == System.Windows.Visibility.Collapsed)
            {
                backcolorEditGrid.Visibility = System.Windows.Visibility.Visible;
                isTemplateSlide = false;
            }
            else
            {
                backcolorEditGrid.Visibility = System.Windows.Visibility.Collapsed;
                isTemplateSlide = true;
            }
        }
        private void closeBGColorpicker()
        {
            backcolorEditGrid.Visibility = System.Windows.Visibility.Collapsed;
            isTemplateSlide = true;
        }

        //首页上显示的按钮集合
        private void hubTileAlbum_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                m_photoChooseTask.Show();
            }
            catch (System.InvalidOperationException)
            {
                if (noWarning == false) showToast("Album opened failed.");
            }
        }
        private void hubTilePuzzle_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Funs/BaoManPage.xaml", UriKind.Relative));
        }
        private void hubTileIcon_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            //try//测试头像
            //{
            //    NavigationService.Navigate(new Uri("/WTool/IconChoosePage.xaml", UriKind.Relative));
            //}
            //catch (Exception) { }

            //if (false) //测试头像
            //{
            noWarning = true;
            try { addIconTask(); }
            catch (Exception) { showToast("Icon loaded failed."); }
            //}
        }
        private void hubTileSetting_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            //try//测试头像
            //{
            //    NavigationService.Navigate(new Uri("/WTool/IconMakePage.xaml", UriKind.Relative));
            //}
            //catch (Exception) { }
            //if (false)
            //{
                noWarning = true;
                try
                {
                    NavigationService.Navigate(new Uri("/WTool/settingPage.xaml", UriKind.Relative));
                }
                catch (Exception) { }
            //}
        }
        // 切换到实时相机 
        private void hubTileRealCamera_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            closeIndex();
            loadEmptyBMP();
            // Check to see if the camera is available on the device.
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                 (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
            {
                initRealCamera();
                realCameraAppBarGrid.Visibility = System.Windows.Visibility.Visible;
                setTemplatePreview();
            }
            else
            {
                showToast("Camera inited failed");
                showIndex();
            }
        }
        // 关闭和显示首页
        private void showIndex()
        {
            this.ApplicationBar.IsVisible = false;
            backcolorEditGrid.Visibility = System.Windows.Visibility.Collapsed;
            editGrid.Visibility = System.Windows.Visibility.Collapsed;
            iconSelectGrid.Visibility = System.Windows.Visibility.Collapsed;
            indexPageCanvas.Visibility = System.Windows.Visibility.Visible;
            inputGrid.Visibility = System.Windows.Visibility.Collapsed;
            m_photo.Visibility = System.Windows.Visibility.Collapsed;
            //m_photo_zoom.Visibility = System.Windows.Visibility.Collapsed;
            processStepGrid.Visibility = System.Windows.Visibility.Collapsed;
            realCameraAppBarGrid.Visibility = System.Windows.Visibility.Collapsed;
            //showEditGridButton.Visibility = System.Windows.Visibility.Collapsed;
            templateSelectNameGrid.Visibility = System.Windows.Visibility.Collapsed;
            watermarkTitleGrid.Visibility = System.Windows.Visibility.Visible;
            closeToast();
        }
        private void closeIndex()
        { 
            if (indexPageCanvas.Visibility == System.Windows.Visibility.Visible)
            {
                indexPageCanvas.Visibility = System.Windows.Visibility.Collapsed;
                if(this.realCameraAppBarGrid.Visibility==System.Windows.Visibility.Collapsed) this.ApplicationBar.IsVisible = true;
                watermarkTitleGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        // 点击 设置 按钮——导航到 设置 页
        private void Setting_Click(object sender, EventArgs e)
        {
            noWarning = true;
            try
            {
                NavigationService.Navigate(new Uri("/WTool/settingPage.xaml", UriKind.Relative));
            }
            catch (Exception) { }
        }

        //返回键 事件
        private void backKeySetting(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {

                if (backFlag == 0)// 任务已保存或已经退回到开始界面，直接退出程序
                {
                    if (this.indexPageCanvas.Visibility == System.Windows.Visibility.Visible)//已经退回到开始界面，直接退出程序
                        e.Cancel = false;
                    else
                    {
                        showIndex();
                        animation_photo_show.Begin();
                        e.Cancel = true;
                    }
                } 
                if (backFlag == 1)//选择了图片且没有保存--制作部分
                {
                    if (templateSelectNameGrid.Visibility == System.Windows.Visibility.Visible)// 隐藏 选择模板 按钮框
                    {
                        templateSelectNameGrid.Visibility = System.Windows.Visibility.Collapsed;
                        e.Cancel = true;
                        return;
                    }
                    if (backcolorEditGrid.Visibility == System.Windows.Visibility.Visible)
                    {
                        backcolorEditGrid.Visibility = System.Windows.Visibility.Collapsed;
                        e.Cancel = true;
                        return;
                    }
                    if (editGrid.Visibility == System.Windows.Visibility.Visible)
                    {
                        editGrid.Visibility = System.Windows.Visibility.Collapsed;
                        e.Cancel = true;
                        return;
                    }
                    if (iconSelectGrid.Visibility == System.Windows.Visibility.Visible)
                    {
                        iconSelectGrid.Visibility = System.Windows.Visibility.Collapsed;
                        e.Cancel = true;
                        return;
                    } 
                    //实时取景
                    if (realTimeCanvas.Visibility == System.Windows.Visibility.Visible)//||(realTimeCanvas.Visibility == System.Windows.Visibility.Collapsed&&cam!=null)
                    {
                        e.Cancel = true;
                        realTimeCanvas.Visibility = System.Windows.Visibility.Collapsed;
                        closeRealCamera();
                        showIndex();
                        animation_showindex.Begin();
                        backFlag = 0;
                        return;
                    }

                    //正常返回
                    if (isSecondEvent != 1) //第一次点击,显示提示，等待五秒
                    {
                        if (noWarning == false) showToast("BackKey again if giving up saving", 5.0, true, 1);
                        e.Cancel = true;
                    }
                    else
                    {
                        isSecondEvent = 0;
                        e.Cancel = true;
                        showIndex();
                        animation_photo_hide.Begin();
                        animation_showindex.Begin();
                        backFlag = 0;
                    }
                }
                else if (backFlag == 3)//设置自定义文字时
                {
                    inputGrid.Visibility = System.Windows.Visibility.Collapsed;
                    e.Cancel = true;
                    backFlag = 1;
                }
                else if (backFlag == 4) // 已保存后返回首页
                {
                    if (indexPageCanvas.Visibility == System.Windows.Visibility.Collapsed)
                    {
                        if (realTimeCanvas.Visibility == System.Windows.Visibility.Visible)
                        {
                            realTimeCanvas.Visibility = System.Windows.Visibility.Collapsed;
                            closeRealCamera();
                        }
                        showIndex();
                        animation_photo_hide.Begin();
                        animation_showindex.Begin();
                        e.Cancel = true;
                    }
                    else e.Cancel = false;
                }
                else if (backFlag == 5) //实时相机已完成上次拍摄
                {
                    //正常返回
                    if (isSecondEvent != 1) //第一次点击,显示提示，等待五秒
                    {
                        if (noWarning == false) showToast("BackKey again if giving up saving", 5.0, true, 1);
                        e.Cancel = true;
                    }
                    else
                    {
                        isSecondEvent = 0;
                        e.Cancel = true;
                        showIndex();
                        animation_photo_hide.Begin();
                        animation_showindex.Begin();
                        backFlag = 0;
                    } 
                } 
                else
                {
                    e.Cancel = false;
                }
            }
            catch (Exception) { }
        }

        // 清除并释放资源
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: 在这里释放托管资源
                if(m_weather!=null) m_weather.Dispose();
            }
        }

        ////切换AppBar
        //ApplicationBar cameraAppBar;

        //private void useCameraAppBar()
        //{

        //}

        /******************************附加美化****************************************/
        // 彩蛋--点击向日葵出太阳
        private void colorEggShow(object sender, System.Windows.Input.GestureEventArgs e)
        {
            colorEggTime = 1.4;
            colorEggSun.Visibility = System.Windows.Visibility.Visible;
            animation_sundrop_coloregg.Begin();
        }
        double colorEggTime = 1.4;
        void colorEggSunTimeTick(object sender, EventArgs e)
        {
            if(colorEggTime > 0.06) colorEggTime -= 0.1;
            if (colorEggTime < 0.05)
            {
                colorEggSun.Visibility = System.Windows.Visibility.Collapsed;
                animation_sundrop_coloregg.Stop();
            }
        }

        //点击首页时按钮的动作和放开的动作
        private void hubTileCamera_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender.Equals(hubTileAlbum)) animation_tap_index2_on.Begin();
            else
            {
                if (sender.Equals(hubTilePuzzle)) animation_tap_index3_on.Begin();
                else
                {
                    if (sender.Equals(hubTileIcon)) animation_tap_index4_on.Begin();
                    else
                    {
                        if (sender.Equals(hubTileSetting)) animation_tap_index5_on.Begin();
                        else
                        {
                            if (sender.Equals(hubTileRealCamera)) animation_tap_index6_on.Begin();
                        }
                    }
                }
            }
        }
        // 点击弹出编辑栏
        private void editphoto_barButton(object sender, EventArgs e)
        {
            if (backFlag != 1 && backFlag != 5)
            {
                showToast("Take a new photo or choose from album");
                return;
            } 
            int chooseNum = templateCount % (TemplateAmount+1);
            if (chooseNum == 2 || chooseNum == 10) //分贝模板。id=2，10。显示到模板文字编辑页
            {
                try
                {
                    NavigationService.Navigate(new Uri("/TemplateSetting/FenBeiSetPage.xaml", UriKind.Relative));
                }
                catch (Exception)
                {
                    showToast("Failed to navigate the seeting page");
                }
            }
            else //非特殊模板，显示编辑菜单
            {
                if (editGrid.Visibility == System.Windows.Visibility.Collapsed)
                {
                    showEditGrid();
                    animation_editGrid_show.Begin();
                }
                else
                {
                    animation_editGrid_hide.Begin();
                    hideEditGrid();
                }
            }
        }
         
        /*****************************实时相机****************************************/
        ///实时相机的若干按钮
        private void realGridEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (cam != null)
            {
                showEditGrid();
            }
            else
            {
                showIndex();
                realCameraAppBarGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
        } 
        private void realGridSettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (cam != null)
            {
                noWarning = true;
                try
                {
                    NavigationService.Navigate(new Uri("/WTool/settingPage.xaml", UriKind.Relative));
                }
                catch (Exception) { }
            } 
            else
            {
                showIndex();
                realCameraAppBarGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
        } 
        private void realGridFlashButton_Click(object sender, RoutedEventArgs e)
        {
            if (cam != null)
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
            else
            {
                showIndex();
                realCameraAppBarGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        void flashModeChangeToast()
        {
            string flashModeStr = "";
            if (cam.FlashMode == FlashMode.On) flashModeStr = "Flash:ON";
            else if (cam.FlashMode == FlashMode.Off) flashModeStr = "Flash:OFF";
            else if (cam.FlashMode == FlashMode.RedEyeReduction) flashModeStr = "Flash:Red Eye Reduction";
            else flashModeStr = "Flash:Auto";
            showToast(flashModeStr,2);
        } 
        private void realGridCameraButton_Click(object sender, RoutedEventArgs e)
        {
            if (cam != null && isWaitingCameraPhoto == false)
            {
                try
                {
                    // Start image capture.
                    systemTrayToast("Loading photo,wait please...");
                    isWaitingCameraPhoto = true;
                    cam.CaptureImage();
                }
                catch (Exception)
                {
                    isWaitingCameraPhoto = false;
                }
            }
        } 
        private void realGridFocusButton_Click(object sender, RoutedEventArgs e)
        {
            if (cam != null)
            {
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
                finally
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        m_photo.Visibility = System.Windows.Visibility.Visible;
                        showToast("Auto focus mode.Double click the view area to focus.");
                    });
                }
            }
            else
            {
                showIndex();
                realCameraAppBarGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
         
        // Variables
        //private int savedCounter = 0;
        PhotoCamera cam;  
        // 当前可用分辨率列表中的计数器
        int currentResIndex = 0;
        // 是否在实时取景中保存照片备份
        private bool isSaveRealBackup=false;
        //是否使用前置摄像头
        private bool isUsingFrontFace = false;
        private int getResolutionLevel() //A最好，B较好，C社交网络，D最小体积
        {
            // 取景相机使用的分辨率等级
            if (_appSetting.Contains("camResolutionLevel"))
            {
                if (_appSetting["camResolutionLevel"].ToString().ToLower().Trim().CompareTo("b") == 0)
                {
                    return 1;
                }
                else if (_appSetting["camResolutionLevel"].ToString().ToLower().Trim().CompareTo("c") == 0)
                {
                    return 2;
                }
                else if (_appSetting["camResolutionLevel"].ToString().ToLower().Trim().CompareTo("d") == 0)
                {
                    return 3;
                }
                else
                    return 0; // a
            }
            else
            {
                _appSetting.Add("camResolutionLevel", "a");
                _appSetting.Save();
                return 0; // a
            }
        }
        private void initRealCamera()
        {
            try
            {
                // Check to see if the camera is available on the device.
                if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                     (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
                {
                    // Initialize the camera, when available.
                    if (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing))
                    {
                        if (isUsingFrontFace == true)
                        {
                            // Use front-facing camera if available.
                            cam = new Microsoft.Devices.PhotoCamera(CameraType.FrontFacing);
                            viewfinderBrush.RelativeTransform =
                                new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 90 };
                        }
                        else
                        {
                            // Otherwise, use standard camera on back of device.
                            cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                            viewfinderBrush.RelativeTransform =
                                new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 90 };
                        }
                    }
                    else
                    {
                        if (isUsingFrontFace == true) showToast("No front camera available.");
                        // Otherwise, use standard camera on back of device.
                        if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == false)
                        {
                            showToast("No back camera available.");
                        }
                        else
                        {
                            cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
                            viewfinderBrush.RelativeTransform =
                                new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 90 };
                        }
                    }
                    // Event is fired when the PhotoCamera object has been initialized.
                    cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);
                    // Event is fired when the capture sequence is complete.
                    cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_CaptureCompleted);
                    // Event is fired when the capture sequence is complete and an image is available.
                    cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);
                    // Event is fired when the capture sequence is complete and a thumbnail image is available.
                    //cam.CaptureThumbnailAvailable += new EventHandler<ContentReadyEventArgs>(cam_CaptureThumbnailAvailable);
                    // The event is fired when auto-focus is complete.
                    cam.AutoFocusCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_AutoFocusCompleted);
                    // The event is fired when the viewfinder is tapped (for focus).
                    cameraCanvas.Tap += new EventHandler<System.Windows.Input.GestureEventArgs>(focus_Tapped);
                    // The event is fired when the shutter button receives a half press.
                    //CameraButtons.ShutterKeyHalfPressed += OnButtonHalfPress;
                    // The event is fired when the shutter button receives a full press.
                    CameraButtons.ShutterKeyPressed += OnButtonFullPress;
                    // The event is fired when the shutter button is released.
                    //CameraButtons.ShutterKeyReleased += OnButtonRelease;
                    //Set the VideoBrush source to the camera.
                    viewfinderBrush.SetSource(cam);
                }
            }
            catch (Exception)
            {
                showToast("Camera loaded failed,please restart the app.");
                cam = null;
                return;
            }
        }
        private void loadEmptyBMP()
        {
            m_photo.Visibility = System.Windows.Visibility.Visible;
            realTimeCanvas.Visibility = System.Windows.Visibility.Visible;
            realCameraAppBarGrid.Visibility = System.Windows.Visibility.Visible;
            this.ApplicationBar.IsVisible = false;
            isLastSavedPic = false;
            try
            {
                //if (ebmp == null)
                //{
                //    StreamResourceInfo sri = Application.GetResourceStream(new Uri("Assets2/EmptyCanvasImage.png", UriKind.Relative));
                //    ebmp = new BitmapImage();
                //    ebmp.SetSource(sri.Stream);
                //    //ebmp = new BitmapImage(new Uri("/WaterMark;component/Assets2/EmptyCanvasImage.png", UriKind.Relative));
                //}
                //else MessageBox.Show("ee");
                if (emptyBMPSRI != null)
                {
                    getNetxRealAbleTemp(); 
                    loadEditMode(emptyBMPSRI.Stream); 
                }
                else { showToast("File exception,please reinstall app"); return; }
            }
            catch (Exception) { }
        }
        private void closeRealCamera()
        {
            try
            {
                if (cam != null)
                {
                    // Dispose camera to minimize power consumption and to expedite shutdown.
                    cam.Dispose();
                    // Release memory, ensure garbage collection.
                    cam.Initialized -= cam_Initialized;
                    cam.CaptureCompleted -= cam_CaptureCompleted;
                    cam.CaptureImageAvailable -= cam_CaptureImageAvailable;
                    //cam.CaptureThumbnailAvailable -= cam_CaptureThumbnailAvailable;
                    cam.AutoFocusCompleted -= cam_AutoFocusCompleted;
                    //CameraButtons.ShutterKeyHalfPressed -= OnButtonHalfPress;
                    CameraButtons.ShutterKeyPressed -= OnButtonFullPress;
                    //CameraButtons.ShutterKeyReleased -= OnButtonRelease;
                }
            }
            catch (Exception) { }
        }

        void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            try
            {
                if (e.Succeeded)
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        flashModeChangeToast();
                    });
                    if (cam.AvailableResolutions.Last<Size>() != null)
                    {
                        int resLevel = getResolutionLevel();
                        if (resLevel == 1)
                        {
                            foreach (Size size in cam.AvailableResolutions)
                            {
                                if ((size.Height >= 1024 && size.Width >= 768) || (size.Height >= 768 && size.Width >= 1024))
                                { cam.Resolution = size; break; }
                            }
                        }
                        else if (resLevel == 2)
                        {
                            foreach (Size size in cam.AvailableResolutions)
                            {
                                if ((size.Height >= 800 && size.Width >= 600)||(size.Height >= 600 && size.Width >= 800))
                                { cam.Resolution = size; break; }
                            }
                        }
                        else if (resLevel == 3)
                        {
                            foreach (Size size in cam.AvailableResolutions)
                            {
                                if ((size.Height >= 300 && size.Width >= 240) || (size.Height >= 240 && size.Width >= 800300))
                                { cam.Resolution = size; break; }
                            }
                        }
                        else
                            cam.Resolution = cam.AvailableResolutions.Last<Size>();

                    }
                }
                else
                {
                    showToast("Failed to load camera.");
                }
            }
            catch (Exception) { showToast("Camera loaded failed,please restart the app."); }
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
                }

                // Rotate video brush from camera.
                if (e.Orientation == PageOrientation.LandscapeRight)
                {
                    // Rotate for LandscapeRight orientation. 
                    viewfinderBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = landscapeRightRotation };
                }
                else
                {
                    // Rotate for standard landscape orientation.
                    viewfinderBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 0 };
                }

            }

            base.OnOrientationChanged(e);
        }

        private void ShutterButton_Click(object sender, RoutedEventArgs e)
        {
            if (cam != null && isWaitingCameraPhoto == false)
            {
                try
                {
                    // Start image capture.
                    systemTrayToast("Loading,please wait...");
                    isWaitingCameraPhoto = true;
                    cam.CaptureImage();
                }
                catch (Exception)
                {
                    isWaitingCameraPhoto = false;
                }
            }
        }

        void cam_CaptureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            // Increments the savedCounter variable used for generating JPEG file names.
            //savedCounter++;
        }
         
        // Informs when full resolution picture has been taken, saves to local media library and isolated storage.
        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            //保存备份，一定要放在展示的前面，否则只能保存一部分文件
            if (isSaveRealBackup)
            {
                library.SavePictureToCameraRoll(string.Format("wm_{0:D4}{1:D2}{2:D2}_{3:D2}{4:D2}{5:D2}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), e.ImageStream); //保存原图到相册
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    showToast("Original photoed saved.");
                });
            }

            try
            {   // Write message to the UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    //toastText.Text = "Captured image available, saving picture.";

                    // Save picture to the library camera roll.
                    //library.SavePictureToCameraRoll(fileName, e.ImageStream); //保存原图到相册
                    //BitmapImage bmps = new BitmapImage();
                    e.ImageStream.Seek(0, SeekOrigin.Begin);
                    bmp.SetSource(e.ImageStream);
                    wb_source = new WriteableBitmap(bmp);
                    WriteableBitmap wbTarget = null;
                    int bheight0 = wb_source.PixelHeight - 1;
                    int bwith0 = wb_source.PixelWidth - 1;
                    wbTarget = new WriteableBitmap(wb_source.PixelHeight, wb_source.PixelWidth);
                    for (int x = 0; x < wb_source.PixelWidth; x++)
                        for (int y = 0; y < wb_source.PixelHeight; y++)
                            wbTarget.Pixels[(bheight0 - y) + x * wbTarget.PixelWidth] = wb_source.Pixels[x + y * wb_source.PixelWidth];
                    wb_source = wb = wbTarget; 
                    bheight = wb.PixelHeight;
                    bwidth = wb.PixelWidth;
                    setTemplatePreview();
                    //添加下一句--按下full_camera后会保存到相机相册后会闪退，但是会向相机相册保存一个备份
                    //savePreview();
                    realTimeCanvas.Visibility = System.Windows.Visibility.Collapsed;
                    realCameraAppBarGrid.Visibility = System.Windows.Visibility.Collapsed;
                    ApplicationBar.IsVisible = true;
                    //closeRealCamera();
                    //library.SavePicture(DateTime.Now.ToLongTimeString(), e.ImageStream);
                    backFlag = 5;
                    closeSystemTrayToast();
                });
                //wb_source = new WriteableBitmap(bmps);
                //wb = new WriteableBitmap(bmps);
                //bheight = wb.PixelHeight;
                //bwidth = wb.PixelWidth;
                // Write message to the UI thread.
                //Deployment.Current.Dispatcher.BeginInvoke(delegate()
                //{
                //    toastText.Text = "Picture has been saved to camera roll.";

                //});

                // Set the position of the stream back to start
                //e.ImageStream.Seek(0, SeekOrigin.Begin);

                //// Save picture as JPEG to isolated storage.
                //using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                //{
                //    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                //    {
                //        // Initialize the buffer for 4KB disk pages.
                //        byte[] readBuffer = new byte[4096];
                //        int bytesRead = -1;

                //        // Copy the image to isolated storage. 
                //        while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                //        {
                //            targetStream.Write(readBuffer, 0, bytesRead);
                //        }
                //    }
                //}

                //// Write message to the UI thread.
                //Deployment.Current.Dispatcher.BeginInvoke(delegate()
                //{
                //    toastText.Text = "Picture has been saved to isolated storage.";

                //});
            }
            catch (Exception) { }
            finally
            {
                // Close image stream--这里是否需要关闭资源==初步测试时关闭资源导致无法保存照片

                //e.ImageStream.Close();
                isWaitingCameraPhoto = false;
            }

        }
          
        // Provide auto-focus in the viewfinder.
        private void focus_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (cam != null)
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
                    finally
                    {
                        Dispatcher.BeginInvoke(delegate()
                        {
                            m_photo.Visibility = System.Windows.Visibility.Visible;
                        });
                    }
                }
                else
                {
                    // Write message to UI.
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        showToast("Auto focus failed.");
                    });
                }
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
                        //this.Dispatcher.BeginInvoke(delegate()
                        //{
                        //    toastText.Text = String.Format("焦点: {0:N2} , {1:N2}", focusXPercentage, focusYPercentage);
                        //});
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
                    finally
                    {
                        m_photo.Visibility = System.Windows.Visibility.Visible;
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

        //废用手动调节分辨率：不适合用户使用。已在设置页设置了分辨率的四个等级
        //private void changeRes_Clicked(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    if (cam != null)
        //    {
        //        // Variables
        //        IEnumerable<Size> resList = cam.AvailableResolutions;
        //        int resCount = resList.Count<Size>();
        //        Size res;

        //        // Poll for available camera resolutions.
        //        for (int i = 0; i < resCount; i++)
        //        {
        //            res = resList.ElementAt<Size>(i);
        //        }

        //        // Set the camera resolution.
        //        res = resList.ElementAt<Size>((currentResIndex + 1) % resCount);
        //        cam.Resolution = res;
        //        currentResIndex = (currentResIndex + 1) % resCount;

        //        // Update the UI.
        //        toastText.Text = String.Format("分辨率: {0}x{1}", res.Width, res.Height);
        //    }
        //}
         
        // Provide auto-focus with a half button press using the hardware shutter button.
        //private void OnButtonHalfPress(object sender, EventArgs e)
        //{
        //    //if (cam != null)
        //    //{
        //    //    // Focus when a capture is not in progress.
        //    //    try
        //    //    {
        //    //        this.Dispatcher.BeginInvoke(delegate()
        //    //        {
        //    //            toastText.Text = "正在自动对焦";
        //    //        });

        //    //        cam.Focus();
        //    //    }
        //    //    catch (Exception focusError)
        //    //    {
        //    //        // Cannot focus when a capture is in progress.
        //    //        this.Dispatcher.BeginInvoke(delegate()
        //    //        {
        //    //            toastText.Text = focusError.Message;
        //    //        });
        //    //    }
        //    //}
        //}

        // Capture the image with a full button press using the hardware shutter button.
        private bool isWaitingCameraPhoto = false;
        private void OnButtonFullPress(object sender, EventArgs e)
        {
            try
            {
                if (cam != null && isWaitingCameraPhoto == false)
                {
                    systemTrayToast("Loading, please wait...");
                    isWaitingCameraPhoto = true;
                    cam.CaptureImage();
                }
            }
            catch (Exception)
            {
                showToast("No photo captured.Try again please.");
                isWaitingCameraPhoto = false;
            }
        }
        //广告
        private void indexADControl_AdRefreshed(object sender, EventArgs e)
        {

        }

        //废用，因易产生异常
        //private void OnButtonRelease(object sender, EventArgs e)
        //{

        //    //if (cam != null)
        //    //{
        //    //    cam.CancelFocus();
        //    //}
        //}

        /**********************************暂未完成或未归档**********************************************/

    }
}
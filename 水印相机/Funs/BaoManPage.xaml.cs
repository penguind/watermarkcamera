using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using System.IO;
using System.Windows.Media;
using Microsoft.Xna.Framework.Media;
using System.IO.IsolatedStorage;
using System.Windows.Resources; 

namespace WaterMark.Funs
{
    public partial class BaoManPage : PhoneApplicationPage
    {
        //图片选择器
        private PhotoChooserTask m_photoChooseTask;
        public int width;
        public int height;
        public int amount;
        public bool isLastSavedPic;
        private bool isMinus;

        WriteableBitmap wb;
        WriteableBitmap wb_source;

        public BaoManPage()
        {
            InitializeComponent();
            init();
        }
        private void init()
        {
            width = 400;
            height = 300;
            amount = 0;
            isLastSavedPic = false;
            isMinus = false;
            wb = new WriteableBitmap(0, 0);
            wb_source = new WriteableBitmap(wb);
            m_photo.Source = wb;
            closeToast();
        }

        //加图按钮
        private void addButton_Click(object sender, EventArgs e)
        {
            if (isLastSavedPic == true) { showToast("Start a new tast in menu"); return; }
            isMinus = false;
            if (amount >= 5)
            {
                showToast("5 picture is largest amount.");
                return;
            }
            m_photoChooseTask = new PhotoChooserTask();
            m_photoChooseTask.Completed += new EventHandler<PhotoResult>(photoChooseTask_complete);
            m_photoChooseTask.Show();
        }
        //加空白格
        private void addBlankButton_Click(object sender, EventArgs e)
        {
            if (isLastSavedPic == true) { showToast("Start a new tast in menu"); return; }
            if (amount >= 5)
            {
                showToast("5 picture is largest amount.");
                return;
            }
            isLastSavedPic =false; 
            var bmp = new BitmapImage(new Uri("/WaterMark;component/Assets2/blank.png", UriKind.Relative));
            wb_source = new WriteableBitmap(wb);
            wb = new WriteableBitmap(width, (amount + 1) * height);
            wb.FillRectangle(0, 0, wb.PixelWidth, wb.PixelHeight, Colors.White);
            Image img = new Image();
            img.Source = wb_source;
            img.Width = width;
            img.Height = amount * height;
            TranslateTransform trans = new TranslateTransform();
            trans.X = 0;
            trans.Y = 0;
            wb.Render(img, trans); 
            TranslateTransform trans2 = new TranslateTransform();
            trans2.X = 0;
            trans2.Y = amount * height;
            Image imgnew = new Image();
            imgnew.Source = bmp;
            imgnew.Width = width;
            imgnew.Height = height;
            imgnew.Stretch = Stretch.Fill;
            wb.Render(imgnew, trans2);
            wb.Invalidate();
            ++amount;
            m_photo.Source = wb;
            isMinus = false;
        }
        //删除上一个格
        private void minusButton_Click(object sender, EventArgs e)
        {
            if (isLastSavedPic == true) { showToast("Please choose a photo from album"); return; }
            if (isMinus == true) { showToast("Only last added one can be cancel"); return; }
            if (amount <= 0) { return; }
            else if (amount == 1) { init(); return; }
            else
            {
                wb = new WriteableBitmap(wb_source);
                --amount;
                m_photo.Source = wb;
                isMinus = true;
            }
        }
        //保存并分享
        private void shareButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (isLastSavedPic == false && wb != null && wb_source != null) //图片没有保存
                {
                    using (MemoryStream myFileStream = new MemoryStream())// "tempJPEG.jpg", FileMode.Create, myStore))//myStore.OpenFile("tempJEPG.jpg", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (myFileStream == null)
                        {
                            showToast("Saved failed,please try again");
                            return;
                        }
                        //jepg编码，将wb存储成jepg
                        wb.SaveJpeg(myFileStream, wb.PixelWidth, wb.PixelHeight, 0, 100);
                        myFileStream.Seek(0, SeekOrigin.Begin);
                        MediaLibrary library = new MediaLibrary();
                        Picture pic = library.SavePicture(string.Format("waterlong_{0}{1}{2}{3}{4}{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), myFileStream);
                        isLastSavedPic = true;
                    }
                }
                else showToast("Start a new tast from the menu");
            }
            catch (Exception)
            {
                try
                {
                    using (MemoryStream myFileStream = new MemoryStream())// "tempJPEG.jpg", FileMode.Create, myStore))//myStore.OpenFile("tempJEPG.jpg", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        if (myFileStream == null)
                        {
                            showToast("Saved failed,please try again later");
                            return;
                        }
                        wb_source.SaveJpeg(myFileStream, wb_source.PixelWidth, wb_source.PixelHeight, 0, 100);
                        myFileStream.Seek(0, SeekOrigin.Begin);
                        MediaLibrary library = new MediaLibrary();
                        Picture pic = library.SavePicture(string.Format("waterlong_{0}{1}{2}{3}{4}{5}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second), myFileStream);
                        showToast("Saved failed but the original saved successfully");
                    }
                }
                catch (Exception) { showToast("Unexcepted error ocurred."); }
            }
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
            catch (Exception) { showToast("Type of photo is not supported."); isLastSavedPic = false; return; }
            NavigationService.Navigate(new Uri("/share/SharePage.xaml", UriKind.Relative));
        }

        /*工具*/
        //加载图片
        private void photoChooseTask_complete(object sender, PhotoResult e)
        {
            try
            {
                if (e.TaskResult == TaskResult.OK) // 加载成功
                {
                    var bmp = new BitmapImage();
                    bmp.SetSource(e.ChosenPhoto);
                    if (amount == 0)
                    {
                        wb = new WriteableBitmap(bmp);
                        wb_source = new WriteableBitmap(wb);
                        height = wb.PixelHeight;
                        width = wb.PixelWidth;
                        isLastSavedPic = false;
                    }
                    else
                    {
                        wb_source = new WriteableBitmap(wb); 
                        wb = new WriteableBitmap(width, (amount + 1) * height);
                        wb.FillRectangle(0, 0, wb.PixelWidth, wb.PixelHeight, Colors.White);
                        Image img = new Image();
                        img.Stretch = Stretch.Uniform;
                        img.Source = wb_source;
                        img.Width = width;
                        img.Height = amount * height;
                        TranslateTransform trans = new TranslateTransform();
                        trans.X = 0;
                        trans.Y = 0;
                        wb.Render(img, trans);
                        wb.Invalidate();
                        Image imgnew = new Image();
                        //imgnew.Stretch = Stretch.Fill;
                        imgnew.Width = width;
                        imgnew.Height = height;
                        imgnew.Source = bmp;
                        TranslateTransform trans2 = new TranslateTransform();
                        trans2.X = 0;
                        trans2.Y = amount* height;
                        wb.Render(imgnew, trans2);
                        wb.Invalidate();
                    }
                    ++amount;
                    m_photo.Source = wb;
                }
                else if (e.TaskResult == TaskResult.None) // 加载错误
                {
                    showToast("Type of photo is not supported.");
                }
                //用户取消加载
            }
            catch (Exception) { showToast("Loaded failed."); }
        } 

        // 显示Toast，等待5秒后 “第二次点击”标记量变为false
        private double toastTimeout;
        private bool isWaiting = false;
        public void showToast(string title, double timeout = 5.0, bool isWaitingSecond = false, int waitingEventID = 0)
        {
            if (toastgrid.Visibility == System.Windows.Visibility.Visible) closeToast();
            toastTimeout = (timeout > 0.0) ? timeout : 5.0;
            DispatcherTimer tmr = new DispatcherTimer();
            tmr.Interval = TimeSpan.FromSeconds(0.1);
            toastText.Text = title;
            toastgrid.Visibility = System.Windows.Visibility.Visible;
            isWaiting = isWaitingSecond;
            tmr.Tick += toastGridTimeTick;
            tmr.Start();
        }
        void toastGridTimeTick(object sender, EventArgs e)
        {
            toastTimeout -= 0.1;
            if (toastTimeout < 0.0)
            {
                isWaiting = false;
                this.toastgrid.Visibility = System.Windows.Visibility.Collapsed; 
                (sender as DispatcherTimer).Stop();
            }
        }
        public void closeToast()
        {
            this.toastgrid.Visibility = System.Windows.Visibility.Collapsed;
        }

        //新建
        private void newButtonClick(object sender, EventArgs e)
        {
            if (isLastSavedPic == true)
            {
                init();
            }
            else
            {
                if (isWaiting == true)
                {
                    init();
                }
                else
                {
                    showToast("BackKey again if giving up saving", 6.0, true);
                }
            }
        }

        private void PhoneApplicationPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isLastSavedPic == false
                && MessageBox.Show("Exit without saving？", "Saving prompt", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) e.Cancel = true;
        }
    }
}
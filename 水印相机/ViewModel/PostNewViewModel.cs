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
using Microsoft.Phone.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.Threading;
using TencentWeiboSDK.Services;
using TencentWeiboSDK.Services.Util;

namespace TencentWeiboSample.ViewModel
{
    public class PostNewViewModel : BaseViewModel
    {
        /// <summary>
        /// 实例化微博相关 API 服务.
        /// </summary>
        private TService service = new TService();
        private PhotoChooserTask photoTask = new PhotoChooserTask();
        private string text = string.Empty;
        private BitmapImage imageSource;

        public PostNewViewModel()
        {
            photoTask.Completed += new EventHandler<PhotoResult>(photoTask_Completed);
        }

        void photoTask_Completed(object sender, PhotoResult e)
        {
            if (e.ChosenPhoto != null)
            {
                if (null == ImageSource)
                {
                    ImageSource = new BitmapImage();
                }

                ImageSource.SetSource(e.ChosenPhoto);
                NotifyPropertyChanged("ImageSource");
            }
        }

        /// <summary>
        /// 获取或设置当前分享微博的内容.
        /// </summary>
        public string Text
        {
            get {
                return text;
            }
            set {
                if (value != text)
                {
                    text = value;
                    NotifyPropertyChanged("Text");
                }
            }
        }


        /// <summary>
        /// 获取或设置当前分享微博的图片.
        /// </summary>
        public BitmapImage ImageSource
        {
            get {
                return imageSource;
            }
            set {
                if (value != imageSource)
                {
                    imageSource = value;
                    NotifyPropertyChanged("ImageSource");
                }
            }
        }

        /// <summary>
        /// 选择图片.
        /// </summary>
        public void ChoosePic()
        {
            photoTask.Show();
        }

        /// <summary>
        /// 分享微博
        /// </summary>
        /// <param name="action">回调委托.</param>
        public void Post(Action action)
        {
            //若用户选择了图片，则实例化 UploadPic 对象，用于上传图片
            //注意：必须在UI线程实例化该对象！
            UploadPic pic = (null != ImageSource) ? new UploadPic(ImageSource) : null;

            new Thread(() =>
                {
                    Action<Callback<object>> action1 = new Action<Callback<object>>((callback) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                            {
                                if (null == callback.InnerException)
                                {
                                    MessageBox.Show("Succeed to share to tencent weibo");
                                }
                                else
                                {
                                    MessageBox.Show("Fail to share to tencent weibo");
                                }
                            });

                        if (null != action)
                        {
                            ImageSource = null;
                            Text = string.Empty;
                            action();
                        }
                    });

                    if (null == ImageSource)
                    {
                        //发送不带图片的微博
                        service.Add(new ServiceArgument() { Content = Text }, action1);
                    }
                    else
                    {
                        //发送带图片的微博
                        service.AddPic(new ServiceArgument() { Content = Text, Pic = pic }, action1);
                    }
                }).Start();
        }
    }
}

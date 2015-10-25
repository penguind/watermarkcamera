using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;

namespace WaterMark.ViewModels
{
    public class PhotoChooseVM : INotifyPropertyChanged
    {
        /// <summary>
        /// 照片集合
        /// </summary>
        private ObservableCollection<PhotoChoose> _allPhotos;
        // 将集合定义为VM层的属性
        public ObservableCollection<PhotoChoose> allPhotos
        {
            get
            {
                if (_allPhotos == null)
                    _allPhotos = new ObservableCollection<PhotoChoose>();
                return _allPhotos;
            }
            set
            {
                if (_allPhotos != value)
                {
                    _allPhotos = value;
                    NotifyPropertyChanged("allPhotos");
                }
            }
        }
        //定义属性改变事件
        public event PropertyChangedEventHandler PropertyChanged;
        //属性改变事件
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //初始化VM层数据
        public PhotoChooseVM()
        {
            try
            {
                using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (file.DirectoryExists("myIcons"))
                    {
                        if (file.GetFileNames() != null)
                        {
                            foreach (string filename_t in file.GetFileNames("myIcons\\*.*"))
                            {
                                if (filename_t.EndsWith(".jpg") || filename_t.EndsWith(".jepg") || filename_t.EndsWith(".png"))
                                {
                                    PhotoChoose photo = new PhotoChoose(filename_t); 
                                    allPhotos.Add(photo);
                                }
                            }
                        }
                    }
                    else
                    {
                        file.CreateDirectory("myIcons");
                        
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }

}

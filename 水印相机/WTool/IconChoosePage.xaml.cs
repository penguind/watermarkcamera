using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using WaterMark.ViewModels;

namespace WaterMark.WTool
{ 
    public partial class IconChoosePage : PhoneApplicationPage
    {
        public IconChoosePage()
        {
            InitializeComponent(); 
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
             
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {  
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/MainPage.xaml",UriKind.Relative));
        } 

        //删除
        private void IconMenuItemDel_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuitem = (MenuItem)sender;
            string header = (sender as MenuItem).Header.ToString();//命令名--删除等
            PhotoChoose choose = menuitem.DataContext as PhotoChoose; //被选中的一项
            ListBoxItem slb = this.listBox1.ItemContainerGenerator.ContainerFromItem((sender as MenuItem).DataContext) as ListBoxItem;
            int seletedIndex = this.listBox1.ItemContainerGenerator.IndexFromContainer(slb);
            if (slb == null) return;
            if (header.CompareTo("删除")==0)
            { 
                if (choose == null)
                {
                    MessageBox.Show("空");
                }
                else
                {
                    MessageBox.Show(choose.iconUri_s);
                    try
                    {
                        using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            if (file.FileExists(choose.iconUri_s))
                            {
                                file.DeleteFile(choose.iconUri_s);
                            }
                            else
                            {
                                //MessageBox.Show("您要删除的文件不存在");

                            }
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("访问文件错误");
                    }

                    //效率极差
                    //最好只是对listbox进行删减
                    //listBox1.Items.RemoveAt(seletedIndex); //出现“只读”不能修改的exception
                    PhotoChooseVM newPhotoVM = new PhotoChooseVM();
                    listBox1.ItemsSource = newPhotoVM.allPhotos;
                }
            }
        }
    }
}
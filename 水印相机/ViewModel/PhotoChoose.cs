using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WaterMark.ViewModels
{
    public class PhotoChoose
    {
        public BitmapImage iconURI { get; set; }
        public string iconUri_s { get; set; }
        public PhotoChoose()
        {
            iconURI = null;
            iconUri_s = "";
        }
        public PhotoChoose(string uri)
        {
            iconURI = null;
            string fileURI = "myIcons\\"+ uri;
            iconUri_s = "myIcons\\" + uri;
            using (IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream filestream = file.OpenFile(fileURI, FileMode.Open, FileAccess.Read))
                {
                    iconURI = new BitmapImage();
                    iconURI.SetSource(filestream);
                    filestream.Flush();
                }

            }
        }
    }
}

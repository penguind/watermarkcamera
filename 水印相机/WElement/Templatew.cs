/**************************************************************
 *                        模板类
 *  用于存储从XML加载的模板
 *  通过对每个元素生成WTextBlock等类型对象加载到相应的列表中
 *  高层可以调用本类获取相应的WriteImage
 *  层次高于WTextBlock、Shapew、Imagew，低于MainPage
 **************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;

namespace WaterMark
{
    class Templatew
    {
        public int size { get; set; } // 存储已加载的模板数

        public Templatew()
        {
            size = 0;
            try
            {
                IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
                if (myIsolatedStorage.FileExists("templates.xml") == true)
                {
                    //IsolatedStorageFileStream isfs = new IsolatedStorageFileStream("templates.xml", FileMode.OpenOrCreate);
                    //XmlReader reader = new XmlReader(isfs);
                    //reader.Read();
                }
                else
                {
                    var createXMLStream = myIsolatedStorage.CreateFile("templates.xml");
                    //createXMLStream.BeginWrite
                }
            }
            catch (FileNotFoundException)
            {

            }
        }
    }
}

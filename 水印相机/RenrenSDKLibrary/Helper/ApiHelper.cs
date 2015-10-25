//  Copyright 2011年 Renren Inc. All rights reserved.
//  - Powered by Team Pegasus. -

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RenrenSDKLibrary;
using System.IO.IsolatedStorage;
using System.IO;
using System.Text.RegularExpressions;


namespace RenrenSDKLibrary
{
    public class ApiHelper
    {
        public static string GenerateTime()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }

        public static bool ContainsError(string result)
        {
            if (result == "")
                return false;
            if (result.Contains("error_code") && result.Contains("error_msg"))
                return true;
            else
                return false;
        }

        #region Log
        static string LogUri = "Log.txt";

          /// <summary>
          /// 参数为string
          /// </summary>
          /// <param name="log">写入log</param>
        public static void WriteLog(string log)
        {
            IsolatedStorageFile logstore = IsolatedStorageFile.GetUserStoreForApplication();
            
            using (StreamWriter writeFile = new StreamWriter(new IsolatedStorageFileStream(LogUri, FileMode.Create, FileAccess.Write
                , logstore)))
            {
                writeFile.WriteLine(log);
                writeFile.Close();
            }
        }

        /// <summary>
        /// 参数为  List<APIParameter> 
        /// </summary>
        /// <param name="log"></param>
        public static void WriteLog(List<APIParameter> log)
        {
            string buf = "   {" + "\r\n";
            foreach (APIParameter parameter in log)
            {
                buf += "    name:  " + parameter.Name + "   value:  " + parameter.Value + "\r\n";
            }
            buf += "   }" + "\r\n" + "\r\n" + "\r\n";
            WriteLog(buf);
        }



        /// <summary>
        /// 读log
        /// </summary>
        /// <returns> 读到的log，为string </returns>
        public  static string ReadLog()
        {
              
            IsolatedStorageFile logstore = IsolatedStorageFile.GetUserStoreForApplication();

            if (!logstore.FileExists(LogUri))
            {
                return null;
            }

            IsolatedStorageFileStream storeStream = logstore.OpenFile(LogUri, FileMode.Open, FileAccess.Read);

            using (StreamReader reader = new StreamReader(storeStream))
            {
                return reader.ReadLine();
            }
        }

        /// <summary>
        /// clean log
        /// </summary>
        public static void CleanLog()
        {
            using (IsolatedStorageFile logstore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (logstore.FileExists("logstore\\Log.txt"))
                {
                    logstore.DeleteFile("logstore\\Log.txt");
                }
            }
        }
        #endregion

        #region 文件的功能方法
        //根据文件名获取文件类型
        // 人人目前只支持四种图片格式
        public static string GetContentType(string fileName)
        {
            string contentType;
            fileName = fileName.ToLower();

            if (fileName.EndsWith(".bmp", StringComparison.CurrentCulture))
            {
                contentType = "image/bmp";
            }
            else if (fileName.EndsWith(".gif", StringComparison.CurrentCulture))
            {
                contentType = "image/gif";
            }
            else if (fileName.EndsWith(".jpg", StringComparison.CurrentCulture) || fileName.EndsWith(".jpeg", StringComparison.CurrentCulture))
            {
                contentType = "image/jpeg";
            }
            else if (fileName.EndsWith(".png", StringComparison.CurrentCulture))
            {
                contentType = "image/png";
            }
            else
            {
                contentType = "application/octet-stream";
            }

            return contentType;
        }
        // 获得文件的二进制流数据
        public static byte[] GetFileContent(string fileName)
        {
            if (String.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            byte[] content;
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!store.FileExists(fileName)) return null;

                using (var stream = store.OpenFile(fileName, FileMode.Open))
                {
                    content = new byte[stream.Length];
                    stream.Read(content, 0, content.Length);
                    stream.Close();
                }
                return content;
            }
        }
        #endregion

        #region 参数和querystring操作
        // 从Parameters中获取数据
        public static string GetQueryFromParas(List<APIParameter> paras)
        {
            if (paras == null || paras.Count == 0)
                return "";
            StringBuilder sbList = new StringBuilder();
            int count = 1;
            foreach (APIParameter para in paras)
            {
                sbList.AppendFormat("{0}={1}", para.Name, Utf8Encode(para.Value));
                if (count < paras.Count)
                    sbList.Append("&");
                count++;
            }
            return sbList.ToString();
        }
        // 把APIParameter中加入URL中
        public static string AddParametersToURL(string url, List<APIParameter> paras)
        {
            string querystring = GetQueryFromParas(paras);
            if (querystring != "")
            {
                url += "?" + querystring;
            }
            return url;
        }
        #endregion

        #region 加密编码
        // MD5 加密
        public static string MD5Encrpt(string plainText)
        {
            return MD5CryptoServiceProvider.GetMd5String(plainText);
        }

        // utf-8编码
        public static string Utf8Encode(string plainText)
        {
            if (plainText == null) plainText = " ";
            byte[] b = System.Text.Encoding.UTF8.GetBytes(plainText);
            string retString = System.Text.Encoding.UTF8.GetString(b, 0, b.Length);
            return retString;
        }
        #endregion

        #region 计算sig签名的方法
        /// <summary>
        /// 计算签名
        /// 此方法传入的是所有签名需要的参数
        /// </summary>
        /// <param name="paras">传入需要的参数</param>
        /// <returns></returns>
        public static string CalSig(List<APIParameter> paras)
        {
            paras.Sort(new ParameterComparer());
            StringBuilder sbList = new StringBuilder();
            foreach (APIParameter para in paras)
            {
                sbList.AppendFormat("{0}={1}", para.Name, para.Value);
            }
            sbList.Append(ConstantValue.SecretKey);
            return ApiHelper.MD5Encrpt(sbList.ToString());
        }

        /// <summary>
        /// 不区分大小写,获得querysring中的值
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetQueryString(Uri url, string key)
        {
            string retVal = "";
            string query = "";
            string abUrl = url.Fragment;

            if (abUrl != "")
            {
                abUrl = Uri.UnescapeDataString(abUrl);
                query = abUrl.Replace("#", "");
            }
            else
            {
                abUrl = url.AbsoluteUri;
                abUrl = Uri.UnescapeDataString(abUrl);
                query = abUrl.Substring(abUrl.IndexOf("?") + 1);
                query = query.Replace("?", "");
            }

            string[] querys = query.Split('&');
            foreach (string qu in querys)
            {
                string[] vals = qu.Split('=');
                if (vals[0].ToString().ToLower() == key.ToLower())
                {
                    retVal = vals[1].ToString();
                    break;
                }
            }
            return retVal;
        }
        
        #endregion

    }
}

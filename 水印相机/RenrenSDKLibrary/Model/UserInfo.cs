//  Copyright 2011年 Renren Inc. All rights reserved.
//  - Powered by Team Pegasus. -

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
using System.IO.IsolatedStorage;
using System.IO;

namespace RenrenSDKLibrary
{
    internal class RenrenAppInfo
    {
        #region Members
        public TokenInfo tokenInfo = null;
        public UserDetails detailInfo = null;
        IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
        string tokenInfoKey = "RenrenTokenInfo";
        string detailInfoKey = "RenrenDetailInfo";
        #endregion

        /// <summary>
        /// 构造
        /// </summary>
        public RenrenAppInfo()
        {
            if (!settings.TryGetValue<TokenInfo>(tokenInfoKey, out tokenInfo))
            {
                tokenInfo = new TokenInfo();
            }
            if (!settings.TryGetValue<UserDetails>(detailInfoKey, out detailInfo))
            {
                detailInfo = new UserDetails();
            }
        }

        #region PublicFunction
        /// <summary>
        /// 设置token信息
        /// </summary>
        /// <param name="info">信息</param>
        public void SetTokenInfo(TokenInfo info)
        {
            if (info == null)
                return;
            tokenInfo = info;
            if (!settings.Contains(tokenInfoKey))
            {
                settings.Add(tokenInfoKey, tokenInfo);
            }
            else
            {
                settings[tokenInfoKey] = tokenInfo;
            }
        }

        /// <summary>
        /// 设置详细信息
        /// </summary>
        /// <param name="info">信息</param>
        public void SetDetailInfo(UserDetails info)
        {
            if (info == null)
                return;
            detailInfo = info;
            if (!settings.Contains(detailInfoKey))
            {
                settings.Add(detailInfoKey, detailInfo);
            }
            else
            {
                settings[detailInfoKey] = detailInfo;
            }
        }

        /// <summary>
        /// 清除信息
        /// </summary>
        public void CleanUp()
        {
            tokenInfo.CleanUp();
            detailInfo = null;
            if (settings.Contains(tokenInfoKey))
                settings.Remove(tokenInfoKey);
            if (settings.Contains(detailInfoKey))
                settings.Remove(detailInfoKey);
        }
        #endregion
    }

    public class TokenInfo
    {
        public string access_token { get; set; }

        public DateTime expires_in { get; set; }

        public string refresh_token { get; set; }

        public string scope { get; set; }

        /// <summary>
        /// 清除信息
        /// </summary>
        public void CleanUp()
        {
            this.access_token = null;
            this.expires_in = DateTime.Now;
            this.refresh_token = null;
            this.scope = null;
        }
    }
}

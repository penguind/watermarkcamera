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
using System.IO;
using System.Text;
using System.Collections.Generic;
using RenrenSDKLibrary.Constants;
using RenrenSDKLibrary.Controls;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;


namespace RenrenSDKLibrary
{
    internal class LoginBS: BusinessBase
    {
        #region Members
        LoginRequest loginRequest;
        TokenInfo tokenInfo;
        enum TLoginState
        {
            KGetToken,
            KGetSessionKey,
            KGetUserInfo
        }
        TLoginState currentState;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when an asynchronous resource-download operation is completed.
        /// </summary>
        public event LoginCompletedHandler LoginCompleted;
        #endregion

        /// <summary>
        /// 初始化
        /// </summary>
        public LoginBS()
        {
            loginRequest = new LoginRequest();
            tokenInfo = new TokenInfo();
        }

        public void CleanLoginEvent()
        {
            LoginCompleted = null;
        }

        #region Override
        public override void RequestComplete(string resultString)
        {
            switch (currentState)
            {
                case TLoginState.KGetToken:
                    {
                        DecoderTokenInfo(resultString);
                        break;
                    }

                case TLoginState.KGetUserInfo:
                    {
                        DecoderUserInfo(resultString);
                        break;
                    }
            }
        }

        public override void RequestError(string errorMsg)
        {
            if (LoginCompleted != null)
            {
                string msg = errorMsg;
                if (errorMsg == "No response.")
                {
                    msg = "Invalid username or password.";
                }
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    LoginCompleted(this,
                       new LoginCompletedEventArgs(new Exception(msg)));
                });  
            }
        }
        #endregion

        #region public
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        public void LogIn(string username, string password, List<string> scope )
        {
            if (username == null || password == null)
            {
                NotifyError("username or password is null");
                return;
            }
            if (ConstantValue.ApiKey == null || ConstantValue.SecretKey == null)
            {
                NotifyError("we need apiKey and SecretKey");
                return;
            }
            currentState = TLoginState.KGetToken;
            loginRequest.LogIn(username, password, scope,
                DownloadStringCompleted);
        }


        /// <summary>
        /// 客户端授权保存token信息
        /// </summary>
        /// <param name="authorizationCode"></param>
        public void SaveAccessToken(string accessToken, string expiresIn)
        {
            tokenInfo = new TokenInfo();
            tokenInfo.access_token = accessToken;
            tokenInfo.expires_in = DateTime.Now.AddSeconds(Int32.Parse(expiresIn));

            currentState = TLoginState.KGetUserInfo;
            RenrenSDK.RenrenInfo.SetTokenInfo(tokenInfo);
            GetCurrentUserInfo();
        }

        /// <summary>
        /// password flow授权解析token信息
        /// </summary>
        /// <param name="result">数据</param>
        private void DecoderTokenInfo(string result)
        {
            AccessToken token = new AccessToken();
            try
            {
                token = (AccessToken)JsonUtility.DeserializeObj(
                new MemoryStream(Encoding.UTF8.GetBytes(result)), typeof(AccessToken));
            }
            catch
            {
                NotifyError("encoding error");
                return;
            }

            tokenInfo = new TokenInfo();
            tokenInfo.access_token = token.access_token;
            tokenInfo.expires_in = DateTime.Now.AddSeconds(Int32.Parse(token.expires_in));
            tokenInfo.refresh_token = token.refresh_token;
            tokenInfo.scope = token.scope;

            currentState = TLoginState.KGetUserInfo;
            RenrenSDK.RenrenInfo.SetTokenInfo(tokenInfo);
            GetCurrentUserInfo();
        }


        #endregion

        /// <summary>
        /// 解析用户信息
        /// </summary>
        /// <param name="result">数据</param>
        private void DecoderUserInfo(string result)
        {
            try
            {
                UserList userlist = new UserList();
                userlist.User_List = (ObservableCollection<UserDetails>)JsonUtility.DeserializeObj(
                    new MemoryStream(Encoding.UTF8.GetBytes(result)), typeof(ObservableCollection<UserDetails>));
                RenrenSDK.RenrenInfo.SetDetailInfo(userlist.User_List[0]);
            }
            catch
            {
                NotifyError("encoding error");
                return;
            }

            if (LoginCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    LoginCompleted(this, new LoginCompletedEventArgs("success"));
                });   
            }
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        private void GetCurrentUserInfo()
        {
            GetUserInfoRequest getcuruserinfoRequest = new GetUserInfoRequest();
            List<string> scope = new List<string>();
            scope.Add("uid");
            scope.Add("name");
            scope.Add("sex");
            scope.Add("star");
            scope.Add("zidou");
            scope.Add("vip");
            scope.Add("birthday");
            scope.Add("email_hash");
            scope.Add("tinyurl");
            scope.Add("headurl");
            scope.Add("mainurl");
            scope.Add("hometown_location");
            scope.Add("work_history");
            scope.Add("university_history");
            currentState = TLoginState.KGetUserInfo;
            getcuruserinfoRequest.GetUserInfo(scope, DownloadStringCompleted);
        }

        /// <summary>
        /// 通知失败信息
        /// </summary>
        /// <param name="msg"></param>
        private void NotifyError(string msg)
        {
            if (LoginCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    LoginCompleted(this,
                        new LoginCompletedEventArgs(new Exception(msg)));
                });   
            }
        }
    }
}

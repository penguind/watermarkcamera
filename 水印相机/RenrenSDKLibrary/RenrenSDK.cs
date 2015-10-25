//  Copyright 2012年 Renren Inc. All rights reserved.
//  - Powered by Open Platform. -

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
using System.Collections.Generic;
using RenrenSDKLibrary;
using RenrenSDKLibrary.Constants;
using Microsoft.Phone.Controls;
using System.IO;
using System.Windows.Media.Imaging;
using RenrenSDKLibrary.WidgetDialog;


namespace RenrenSDKLibrary
{
    internal class RenrenSDK
    {
        public static RenrenAppInfo RenrenInfo = new RenrenAppInfo();
        LoginBS loginBS;
        FriendBS friendBS;
        LoginViewBS loginViewBS;
        UploadPhotoBS uploadBS;
        GetUserInfoBS getUserInfoBS;
        GetAlbumsBS getAlbumsBS;
        CreateAlbumBS createAlbumBS;
        public static BitmapImage publishPhoto;//通过api传进来的照片

        APIRequestBS apiRequestBS;
        WidgetAPIRequestBS widgetAPIRequestBS;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="apiKey"></param>
        public RenrenSDK(string appID, string apiKey, string secretKey)
        {
            ConstantValue.AppID = appID;
            ConstantValue.ApiKey = apiKey;
            ConstantValue.SecretKey = secretKey; 
            friendBS = new FriendBS();
            getUserInfoBS = new GetUserInfoBS();
        }

        /// <summary>
        /// 清空信息
        /// </summary>
        public void LogOut()
        {
            loginBS = null;
            friendBS = null;
            loginViewBS = null;
            uploadBS = null;
            getUserInfoBS = null;
            getAlbumsBS = null;
            createAlbumBS = null;
            apiRequestBS = null;
            RenrenInfo.CleanUp();
        }

        /// <summary>
        /// 用户名密码方式，无权限
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="callback">回调</param>
        public void LogIn(string username, string password,
            LoginCompletedHandler callback)
        {
             LogIn(username, password, null, callback);
        }

        /// <summary>
        /// 用户名密码方式，有权限
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="scope">权限列表</param>
        /// <param name="callback">回调</param>
        public void LogIn(string username, string password,
            List<string> scope, LoginCompletedHandler callback)
        {
            if (loginBS == null)
            {
                loginBS = new LoginBS();
            }
            loginBS.CleanLoginEvent();
            loginBS.LoginCompleted += callback;

            loginBS.LogIn(username, password, scope);
        }

        /// <summary>
        /// 登录，页面方式，无权限设置
        /// </summary>
        /// <param name="page">当前显示页面</param>
        /// <param name="redirectUri">转向</param>
        /// <param name="callback">回调</param>
        public void LogIn(PhoneApplicationPage page, string redirectUri,
            LoginCompletedHandler callback)
        {
            LogIn(page, null, redirectUri, callback);
        }

        /// <summary>
        /// 登录，页面方式，有权限设置
        /// </summary>
        /// <param name="page">当前显示页面</param>
        /// <param name="scope">权限列表</param>
        /// <param name="redirectUri">转向</param>
        /// <param name="callback">回调</param>
        public void LogIn(PhoneApplicationPage page, List<string> scope,
            string redirectUri, LoginCompletedHandler callback)
        {
            if (loginViewBS == null)
            {
                loginViewBS = new LoginViewBS();
            }

            loginViewBS.CleanLoginEvent();
            loginViewBS.LoginCompleted += callback;

            loginViewBS.InitView(page);
            loginViewBS.Login(redirectUri, scope);
        }

        /// <summary>
        /// 获取好友id列表
        /// </summary>
        /// <param name="callback">回调</param>
        /// <param name="count">返回每页个数,默认为500</param>
        /// <param name="page">分页，默认为1</param>
        public void GetFriendsID( GetFriendsIDCompletedHandler callback , int count =500 , int page =1)
        {
            if (friendBS == null)
            {
                friendBS = new FriendBS();
            }
            friendBS.CleanGetFriendsIDEvent();
            friendBS.GetFriendsIDCompleted += callback;
            friendBS.GetFriendsID( count, page);
        }

         /// <summary>
         /// 得到当前登录用户的好友列表，带scope参数
         /// </summary>
         /// <param name="callback">回调</param>
         /// <param name="scope">需要返回的字段，目前支持如下字段: headurl_with_logo, tinyurl_with_logo</param>
         /// <param name="count">返回每页个数，默认为500</param>
         /// <param name="page">分页，默认为1</param>
         public void GetFriends(GetFriendsCompletedHandler callback ,List<string> scope ,int count = 500, int page = 1)
         {
             if (friendBS == null)
             {
                 friendBS = new FriendBS();
             }
             friendBS.CleanGetFriendsEvent();
             friendBS.GetFriendsCompleted += callback;
             friendBS.GetFriends(scope, count, page);
         }

        /// <summary>
         /// 得到当前登录用户的好友列表，不带scope参数
         /// </summary>
         /// <param name="callback">回调</param>
         /// <param name="count">返回每页个数，默认为500</param>
         /// <param name="page">分页，默认为1</param>
         public void GetFriends(GetFriendsCompletedHandler callback, int count = 500, int page = 1)
         {
             if (friendBS == null)
             {
                 friendBS = new FriendBS();
             }
             friendBS.CleanGetFriendsEvent();
             friendBS.GetFriendsCompleted += callback;
             friendBS.GetFriends( null, count, page);
         }


         /// <summary>
         /// 获取好友App列表，带Scope参数
         /// </summary>
         /// <param name="scope">参数列表</param>
         /// <param name="callback">回调</param>
         public void GetAppFriends( List<string> scope, GetAppFriendsCompletedHandler callback)
         {
             if (friendBS == null)
             {
                 friendBS = new FriendBS();
             }
             friendBS.CleanGetAppFriendsEvent();
             friendBS.GetAppFriendsCompleted += callback;
             friendBS.GetAppFriends(scope);
         }

         /// <summary>
         /// 获取App好友列表，不带Scope参数
         /// </summary>
         /// <param name="callback">回调</param>
         public void GetAppFriends( GetAppFriendsIDCompletedHandler callback)
         {
             if (friendBS == null)
             {
                 friendBS = new FriendBS();
             }
             friendBS.CleanGetAppFriendsIDEvent();
             friendBS.GetAppFriendsIDCompleted += callback;
             friendBS.GetAppFriends(null);
         }

         /// <summary>
         /// 获取当前用户相册列表
         /// </summary>
         public void GetAlbums(RenrenSDKLibrary.GetAlbumsRequest.GetAlbumsCompletedHandler callback, int page, int count, string aids)
         {
             if (getAlbumsBS == null)
             {
                 getAlbumsBS = new GetAlbumsBS();
             }

             getAlbumsBS.CleanGetAlbumsEvent();
             getAlbumsBS.GetAlbumsCompleted += callback;
             getAlbumsBS.GetAlbums(RenrenInfo.detailInfo.uid, page, count, aids);
         }

         /// <summary>
         /// 创建相册
         /// </summary>
         public void CreateAlbum(string name, string description,string location, CreateAlbumRequest.CreateAlbumCompletedHandler callback)
         {
             if (createAlbumBS == null)
             {
                 createAlbumBS = new CreateAlbumBS();
             }
             createAlbumBS.CleanCreatAlbumEvent();
             createAlbumBS.CreateAlbumCompleted += callback;
             createAlbumBS.CreateAlbum(name, description, location);
         }
         /// <summary>
         /// 照片一键上传
         /// </summary>
         internal void FastUploadPhoto(BitmapImage photo,string imgPath, string caption)
         {
             publishPhoto = photo;
             (Application.Current.RootVisual as PhoneApplicationFrame).
                 Navigate(new Uri("/RenrenSDKLibrary;component/Pages/UploadPhotoPage.xaml?path=" + imgPath + "&caption=" + caption, 
                          UriKind.Relative)); 
         }

        /// <summary>
        /// 上传照片接口
        /// </summary>
        /// <param name="photo">照片</param>
        /// <param name="caption">描述</param>
        /// <param name="callback">回调</param>
         public void UploadPhoto(BitmapImage photo, string name, UploadPhotoCompletedHandler callback,
             string caption, int albumId)
        {
            if (uploadBS == null)
            {
                uploadBS = new UploadPhotoBS();
            }
            uploadBS.CleanUploadPhotoEvent();
            uploadBS.UploadCompleted += callback;
            uploadBS.UploadPhoto(photo, name, caption, albumId);
        }

         /// <summary>
         /// 上传照片接口
         /// </summary>
         /// <param name="fileName">文件全路径</param>
         /// <param name="caption">描述</param>
         /// <param name="albumId">回调</param>
         public void UploadPhoto(string fileName,UploadPhotoCompletedHandler callback,
             string caption, int albumId)
         {
             if (uploadBS == null)
             {
                 uploadBS = new UploadPhotoBS();
             }
             uploadBS.CleanUploadPhotoEvent();
             uploadBS.UploadCompleted += callback;
             uploadBS.UploadPhoto(fileName, caption, albumId);
         }

         /// <summary>
         /// 获取log信息
         /// </summary>
         /// <returns></returns>
         public string ReadLog()
         {
             return ApiHelper.ReadLog();
         }

         /// <summary>
         /// 清空log信息
         /// </summary>
        public void Cleanlog()
        {
             ApiHelper.CleanLog();
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <param name="callback">回调</param>
        public void GetCurUserInfo(GetCurUserInfoCompletedHandler callback)
        {
            if (getUserInfoBS == null)
            {
                getUserInfoBS = new GetUserInfoBS();

            }
            getUserInfoBS.ClearGetCurUserInfoEvent();
            getUserInfoBS.GetCurUserInfoCompleted += callback;
            getUserInfoBS.GetCurUserID();
        }

        /// <summary>
        /// 获取指定用户信息
        /// </summary>
        /// <param name="sessionkey">指定用户的uid，可以是多个uid中间用逗号隔开</param>
        /// <param name="callback">回调</param>
        public void GetUserInfo(string uid, List<string> scope, GetUserInfoCompletedHandler callback)
        {
            if (getUserInfoBS == null)
            {
                getUserInfoBS = new GetUserInfoBS();

            }
            getUserInfoBS.ClearGetUserInfoEvent();
            getUserInfoBS.GetUserInfoCompleted += callback;
            getUserInfoBS.GetUsersID(uid, scope);
        }

        /// <summary>
        /// 通用API接口的调用方法。 
        /// </summary>
        /// <param name="callback">回调，返回JSON数据 </param>
        /// <param name="param">传入请求API接口所需要的参数</param>
        public void RequestAPIInterface(APIRequestCompletedHandler callback, List<APIParameter> param)
        {
            if (apiRequestBS == null)
            {
                apiRequestBS = new APIRequestBS();
            }

            apiRequestBS.ClearAPIRequestEvent();
            apiRequestBS.APIRequestCompleted += callback;
            apiRequestBS.GetAPIRequestData(param);
        }

        /// <summary>
        /// 通用WidgetAPI调用方法
        /// </summary>
        /// <param name="page">当前页面</param>
        /// <param name="dialogType">WidgetDialog的类型</param>
        /// <param name="param">请求的参数</param>
        /// <param name="callback">回调</param>
        public void WidgetDialog(PhoneApplicationPage page, string dialogType, List<APIParameter> param,
            RenrenSDKLibrary.WidgetDialog.WidgetAPIRequestBS.DownloadStringCompletedHandler callback = null)
        {
            if (widgetAPIRequestBS == null)
            {
                widgetAPIRequestBS = new WidgetAPIRequestBS();
            }

            if (callback != null)
            {
                widgetAPIRequestBS.CleanDownloadStringEvent();
                widgetAPIRequestBS.DownloadStringCompleted += callback;
            }

            widgetAPIRequestBS.InitView(page);
            widgetAPIRequestBS.RunDialog(dialogType, param);
        }

        /// <summary>
        /// 判断用户授权状态的方法
        /// </summary>
        /// <return>用户授权是否有效</return>
        public bool IsAccessTokenValid()
        {
            return (RenrenInfo.tokenInfo.access_token != null && RenrenInfo.tokenInfo.expires_in != null
                && (DateTime.Now.CompareTo(RenrenInfo.tokenInfo.expires_in) < 0));
        }
    }
}



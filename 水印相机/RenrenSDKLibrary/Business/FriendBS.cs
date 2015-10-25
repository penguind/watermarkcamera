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
using System.Collections.Generic;
using RenrenSDKLibrary.Constants;
using RenrenSDKLibrary.Controls;
using Microsoft.Phone.Controls;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;

namespace RenrenSDKLibrary
{
    internal class FriendBS : BusinessBase
    {
        #region Members
        GetFriendsIDRequest getfriendsIDRequest;
        GetFriendsRequest getfriendsRequest;
        GetAppFriendsRequest getappfriendsRequest;
        enum TGetFriendsType
        {
            KGetFriendsID,
            KGetFriends,
            KGetAppFriendsID,
            KGetAppFriendsInfo
        }
        TGetFriendsType currentState;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when an asynchronous resource-download operation is completed.
        /// </summary>
        public event GetFriendsIDCompletedHandler GetFriendsIDCompleted;
        public event GetFriendsCompletedHandler GetFriendsCompleted;
        public event GetAppFriendsCompletedHandler GetAppFriendsCompleted;
        public event GetAppFriendsIDCompletedHandler GetAppFriendsIDCompleted;
        #endregion

        #region Constructor
        /// <summary>
        /// 构造
        /// </summary>
        public FriendBS()
        {
            getfriendsIDRequest = new GetFriendsIDRequest();
            getfriendsRequest = new GetFriendsRequest();
            getappfriendsRequest = new GetAppFriendsRequest();

        }
        #endregion

        #region Clean Events
        public void CleanGetFriendsIDEvent()
        {
            GetFriendsIDCompleted = null;
        }

        public void CleanGetFriendsEvent()
        {
            GetFriendsCompleted = null;
        }

        public void CleanGetAppFriendsEvent()
        {
            GetAppFriendsCompleted = null;
        }

        public void CleanGetAppFriendsIDEvent()
        {
            GetAppFriendsIDCompleted = null;
        }
        #endregion

        #region Override
        /// <summary>
        /// success
        /// </summary>
        /// <param name="resultString">msg</param>
        public override void RequestComplete(string resultString)
        {
            switch (currentState)
            {
                case TGetFriendsType.KGetFriendsID:
                    {
                        DecoderFriendsID(resultString);
                        break;
                    }
                case TGetFriendsType.KGetFriends:
                    {
                        DecoderFriends(resultString);
                        break;
                    }
                case TGetFriendsType.KGetAppFriendsID:
                    {
                        DecoderAppFriendsID(resultString);
                        break;
                    }
                case TGetFriendsType.KGetAppFriendsInfo:
                    {
                        DecoderAppFriendsInfo(resultString);
                        break;
                    }
            }
        }

        /// <summary>
        /// error
        /// </summary>
        /// <param name="errorMsg">msg</param>
        public override void RequestError(string errorMsg)
        {
            switch (currentState)
            {
                case TGetFriendsType.KGetFriendsID:
                    {
                        NotifyFriendsIDResult(errorMsg, null);
                        break;
                    }
                case TGetFriendsType.KGetFriends:
                    {
                        NotifyFriendsResult(errorMsg, null);
                        break;
                    }
                case TGetFriendsType.KGetAppFriendsID:
                    {
                        NotifyAppFriendsIDResult(errorMsg, null);
                        break;
                    }
                case TGetFriendsType.KGetAppFriendsInfo:
                    {
                        NotifyAppFriendsInfoResult(errorMsg, null);
                        break;
                    }
            }
        }
        #endregion

        #region getfriendsIDlist

        /// <summary>
        /// 得到当前登录用户的好友id列表，带返回页数
        /// </summary>
        /// <param name="count">返回每页个数，默认为500</param>
        /// <param name="page">分页，默认为1</param>
        public void GetFriendsID(int count , int page)
        {
            getfriendsIDRequest.GetFriendsID(DownloadStringCompleted, count, page);
            currentState = TGetFriendsType.KGetFriendsID;
        }
        #endregion

        #region getfriendslist
        /// <summary>
        /// 得到当前登录用户的好友列表 ,带scope列表
        /// </summary>
        /// <param name="scope">需要返回的字段，目前支持如下字段: headurl_with_logo, tinyurl_with_logo</param>
        /// <param name="count">返回每页个数</param>
        /// <param name="page">分页</param>
        public void GetFriends(List<string> scope, int count ,int page)
        {
            getfriendsRequest.GetFriends(DownloadStringCompleted, scope, count, page);
            currentState = TGetFriendsType.KGetFriends;
        }
        #endregion

        #region getappfriendslist
        /// <summary>
        /// 得到当前登录用户的App好友列表，带Scope参数列表
        /// </summary>
        /// <param name="scope">参数列表</param>
        public void GetAppFriends( List<string> scope)
        {
            if (scope == null)
            {
                getappfriendsRequest.GetAppFriends(null, DownloadStringCompleted);
                currentState = TGetFriendsType.KGetAppFriendsID;
            }
            else
            {
                getappfriendsRequest.GetAppFriends(scope, DownloadStringCompleted);
                currentState = TGetFriendsType.KGetAppFriendsInfo;
            }
        }
        #endregion 

        #region 事件处理
        /// <summary>
        /// 解析好友id列表
        /// </summary>
        /// <param name="result"></param>
        private void DecoderFriendsID(string result)
        {
            ObservableCollection<int> uids = new ObservableCollection<int>();
            try
            {
                uids = (ObservableCollection<int>)JsonUtility.DeserializeObj(
                      new MemoryStream(Encoding.UTF8.GetBytes(result)), typeof(ObservableCollection<int>));
            }
            catch
            {
                NotifyFriendsIDResult("encoding error", null);
                return;
            }
            NotifyFriendsIDResult(null, uids);
        }

        /// <summary>
        /// 解析好友信息
        /// </summary>
        /// <param name="result"></param>
        private void DecoderFriends(string result)
        {
            ObservableCollection<Friend> friendslist = new ObservableCollection<Friend>();
            try
            {
                List<Friend> uids = (List<Friend>)JsonUtility.DeserializeObj(
                    new MemoryStream(Encoding.UTF8.GetBytes(result)), typeof(List<Friend>));

                foreach (Friend friend in uids)
                {
                    friendslist.Add(friend);
                }
            }
            catch
            {
                NotifyFriendsResult("encoding error", null);
                return;
            }
            NotifyFriendsResult(null, friendslist);
        }

        /// <summary>
        /// 解析应用好友id列表
        /// </summary>
        /// <param name="result"></param>
        private void DecoderAppFriendsID(string result)
        {
            ObservableCollection<int> uids = new ObservableCollection<int>();
            try
            {
                uids = (ObservableCollection<int>)JsonUtility.DeserializeObj(
                      new MemoryStream(Encoding.UTF8.GetBytes(result)), typeof(ObservableCollection<int>));
            }
            catch
            {
                NotifyAppFriendsIDResult("encoding error", null);
                return;
            }
            NotifyAppFriendsIDResult(null, uids);
        }

        /// <summary>
        /// 解析应用好友详细信息列表
        /// </summary>
        /// <param name="result"></param>
        private void DecoderAppFriendsInfo(string result)
        {
            ObservableCollection<AppFriend> appfriendslist = new ObservableCollection<AppFriend>();
            try
            {
                List<AppFriend> uids = (List<AppFriend>)JsonUtility.DeserializeObj(
                     new MemoryStream(Encoding.UTF8.GetBytes(result)), typeof(List<AppFriend>));

                foreach (AppFriend appfriend in uids)
                {
                    appfriendslist.Add(appfriend);
                }
            }
            catch
            {
                NotifyAppFriendsInfoResult("encoding error", null);
                return;
            }
            NotifyAppFriendsInfoResult(null, appfriendslist);
        }
        #endregion

        #region 通知信息
        /// <summary>
        /// Notify系列函数，参数要么传error，要么传reslut,另一个为null
        /// </summary>
        /// <param name="error">错误信息</param>
        /// <param name="result">返回的信息</param>
        private void NotifyFriendsIDResult(string error, ObservableCollection<int> result )
        {
            if (GetFriendsIDCompleted != null)
            { 
                if (error != null)
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                        {
                            GetFriendsIDCompleted(this, new GetFriendsIDCompletedEventArgs(new Exception(error)));
                        });
                else
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                        {
                            GetFriendsIDCompleted(this, new GetFriendsIDCompletedEventArgs(result));
                        });
            }
            return;
        }

        private void NotifyFriendsResult(string error, ObservableCollection<Friend> result)
        {
            if (GetFriendsCompleted != null)
            {
                if (error != null)
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                    GetFriendsCompleted(this, new GetFriendsCompletedEventArgs(new Exception(error)));
                    });
               else
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                    GetFriendsCompleted(this,  new GetFriendsCompletedEventArgs(result));
                    });
            }
            return;
        }

        private void NotifyAppFriendsInfoResult(string error, ObservableCollection<AppFriend> result)
        {
            if (GetAppFriendsCompleted != null)
            {
                 if (error != null)
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                        {
                            GetAppFriendsCompleted(this, new GetAppFriendsCompletedEventArgs(new Exception(error)));
                        });
                else
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        GetAppFriendsCompleted(this, new GetAppFriendsCompletedEventArgs(result));
                    });
            }
            return;
        }

        private void NotifyAppFriendsIDResult(string error, ObservableCollection<int> result)
        {
            if (GetAppFriendsIDCompleted != null)
            {
                if (error != null)
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                          GetAppFriendsIDCompleted(this, new GetAppFriendsIDCompletedEventArgs(new Exception(error)));
                     });
                else
                    System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        GetAppFriendsIDCompleted(this,  new GetAppFriendsIDCompletedEventArgs(result));
                    });
            }
            return;
        }
        #endregion
    }
}

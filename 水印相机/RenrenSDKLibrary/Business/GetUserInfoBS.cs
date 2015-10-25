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
    //获取得到当前用户信息的handler
    public delegate void GetCurUserInfoCompletedHandler(object sender, GetUserUidCompletedEventArgs e);

    internal class GetUserInfoBS : BusinessBase
    {
        #region Members
        GetCurUserInfoRequest getCurUserInfoRequest;
        GetUserInfoRequest getUserInfoRequest;
        UserList userlist;//获得的User数据
        enum TGetUserInfoType
        {
            KGetCurUserInfo,
            KGetUserInfo
        }
        TGetUserInfoType currentState;
        #endregion

        #region Events
        /// <summary>
        /// Occurs when an asynchronous resource-download operation is completed.
        /// </summary>
        public event GetCurUserInfoCompletedHandler GetCurUserInfoCompleted;
        public event GetUserInfoCompletedHandler GetUserInfoCompleted;
        #endregion

        /// <summary>
        /// 构造
        /// </summary>
        public GetUserInfoBS()
        {
            getCurUserInfoRequest = new GetCurUserInfoRequest();
            getUserInfoRequest = new GetUserInfoRequest();
            userlist = new UserList();
        }

        #region Override
        /// <summary>
        /// success
        /// </summary>
        /// <param name="resultString">msg</param>
        public override void RequestComplete(string resultString)
        {
            switch (currentState)
            {
                case TGetUserInfoType.KGetCurUserInfo:
                    {
                        DecocerCurUser(resultString);
                        break;
                    }
                case TGetUserInfoType.KGetUserInfo:
                    {
                        DecoderUsers(resultString);
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
                case TGetUserInfoType.KGetCurUserInfo:
                    {
                        GetUidNotifyError(errorMsg);
                        break;
                    }
                case TGetUserInfoType.KGetUserInfo:
                    {
                        GetUsersNotifyError(errorMsg);
                        break;
                    }
            }
        }
        #endregion

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        public void GetCurUserID()
        {
            if (RenrenSDK.RenrenInfo.detailInfo !=  null && 
                RenrenSDK.RenrenInfo.detailInfo.name != null)
            {
                GetUidNotifyMessage(RenrenSDK.RenrenInfo.detailInfo);
            }
            else
            {
                getCurUserInfoRequest.GetCurUserInfo(DownloadStringCompleted);
                currentState = TGetUserInfoType.KGetCurUserInfo;
            }
        }

        public void ClearGetCurUserInfoEvent()
        {
            GetCurUserInfoCompleted = null;
        }

        public void ClearGetUserInfoEvent()
        {
            GetUserInfoCompleted = null;
        }


        /// <summary>
        /// 获取指定用户信息
        /// </summary>
        /// <param name="uid">uid列表</param>
        /// <param name="scope">权限列表</param>
        public void GetUsersID(string uid, List<string> scope)
        {
            if (uid != "")
            {
                List<int> sArray = new List<int>();
                string[] uids = uid.Split(',');
                foreach (string aUid in uids)
                {
                    sArray.Add(Convert.ToInt32(aUid));
                }
                getUserInfoRequest.GetUserInfo(scope,
                    DownloadStringCompleted, sArray);
                currentState = TGetUserInfoType.KGetUserInfo;
            }
        }

        /// <summary>
        /// 获取当前用户信息回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecocerCurUser(string result)
        {
            UserDetails oneuid = new UserDetails();
            try
            {
                oneuid = (UserDetails)JsonUtility.DeserializeObj(
                    new MemoryStream(Encoding.UTF8.GetBytes(result)), typeof(UserDetails));
            }
            catch
            {
                GetUidNotifyError("encoding error");
                return;
            }

            GetUidNotifyMessage(oneuid);
        }

        /// <summary>
        /// 获取指定用户信息回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DecoderUsers(string result)
        {
            try
            {
                userlist.User_List = (ObservableCollection<UserDetails>)JsonUtility.DeserializeObj(
                    new MemoryStream(Encoding.UTF8.GetBytes(result)), typeof(ObservableCollection<UserDetails>));
                GetUsersNotifyMessage(userlist);
            }
            catch
            {
                GetUsersNotifyError("encoding error");
                return;
            }
        }

        /// <summary>
        /// 通知查找当前用户uid成功信息
        /// </summary>
        /// <param name="msg">内容</param>
        private void GetUidNotifyMessage(UserDetails msg)
        {
            if (GetCurUserInfoCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    GetCurUserInfoCompleted(this, new GetUserUidCompletedEventArgs(msg));
                });
            }
        }

        /// <summary>
        /// 通知查找当前用户uid失败信息
        /// </summary>
        /// <param name="msg"></param>
        private void GetUidNotifyError(string msg)
        {
            if (GetCurUserInfoCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    GetCurUserInfoCompleted(this, new GetUserUidCompletedEventArgs(new Exception(msg)));
                });
            }
        }

        /// <summary>
        /// 通知查找用户成功信息
        /// </summary>
        /// <param name="msg">内容</param>
        private void GetUsersNotifyMessage(UserList msg)
        {
            if (GetUserInfoCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    GetUserInfoCompleted(this, new GetUsersCompletedEventArgs(msg));
                }); 
            }
        }

        /// <summary>
        /// 通知查找用户失败信息
        /// </summary>
        /// <param name="msg"></param>
        private void GetUsersNotifyError(string msg)
        {
            if (GetUserInfoCompleted != null)
            {
                System.Windows.Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    GetUserInfoCompleted(this, new GetUsersCompletedEventArgs(new Exception(msg)));
                });  
            }
        }
    }
}

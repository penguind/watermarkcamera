﻿//  Copyright 2011年 Renren Inc. All rights reserved.
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

namespace RenrenSDKLibrary
{
    public class CreateAlbumCompletedEventArgs : EventArgs
    {
        #region Properties
        /// <summary>
        /// Gets the data that is downloaded by a DownloadStringAsync method.
        /// </summary>
        public Album Result { get; private set; }

        /// <summary>
        /// Gets a value that indicates which error occurred during an asynchronous operation.
        /// </summary>
        public Exception Error { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of DownloadStringCompletedEventArgs with the specified result data.
        /// </summary>
        /// <param name="result">The data that is downloaded by a DownloadStringAsync method.</param>
        public CreateAlbumCompletedEventArgs(Album result)
        {
            Result = result;
        }

        /// <summary>
        /// Creates a new instance of DownloadStringCompletedEventArgs with the specified exception.
        /// </summary>
        /// <param name="ex">The exception generated by the asynchronous operation.</param>
        public CreateAlbumCompletedEventArgs(Exception ex)
        {
            Error = ex;
        }

        #endregion
    }
}

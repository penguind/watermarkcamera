//  Copyright 2011年 Renren Inc. All rights reserved.
//  - Powered by Team Pegasus. -

using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace RenrenSDKLibrary
{
    public class JsonUtility
    {
        public static object DeserializeObj(Stream inputStream, Type objType)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(objType);

            object result = serializer.ReadObject(inputStream);

            return result;
        }
    } 
}

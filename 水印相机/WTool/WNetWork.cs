using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WaterMark.WTool
{
    class WNetWork
    {
        public static bool isNetAvailible()
        {
            if (getNetWorkType() == Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType.None) return false;
            else return true;
        }

        public static Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType getNetWorkType()
        {
            return Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType;
        }
    }
}

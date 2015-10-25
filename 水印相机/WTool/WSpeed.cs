using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Text; 

namespace WaterMark
{
    class WSpeed
    {
        public double speedPerSec;
        public double distance;
        public double lastLongitude;
        public double lastLatitude;
        // 位置监视器
        private GeoCoordinateWatcher watcher;

        public WSpeed()
        {
            speedPerSec = 0.0;
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);// using high accuracy
            //watcher.MovementThreshold = 0.1;// use MovementThreshold to ignore noise in the signal
            distance = 0;
        }
        public WSpeed(double longitude, double latitude)
        {
            speedPerSec = 0.0;
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);// using high accuracy
            //watcher.MovementThreshold = 0.1;// use MovementThreshold to ignore noise in the signal
            distance = 0;
            lastLatitude = latitude;
            lastLongitude = longitude;
        }
        public WSpeed(string longtitude, string latitude)
        {
            speedPerSec = 0.0;
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);// using high accuracy
            //watcher.MovementThreshold = 0.1;// use MovementThreshold to ignore noise in the signal
            distance = 0;
            lastLongitude = double.Parse(longtitude);
            lastLatitude = double.Parse(latitude);
        }
        
        public void start()
        {
            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            watcher.Start();
        }
        public void clearLength()
        {
            distance = 0.0;
        }
        public double getSpeed(int second)
        {
            //if (second > 0)
            //    speedPerSec = length / second;
            //else
            //    speedPerSec = 0.0; 
            return speedPerSec;
        }
        //事件
        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            //speedPerSec = e.Position.Location.Speed;
            speedPerSec = e.Position.Location.Altitude;
        }
        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    if (watcher.Permission == GeoPositionPermission.Denied)
                    {
                        speedPerSec = 0.0;
                    }
                    else
                    {
                        speedPerSec = 0.0;
                    }
                    break;
                case GeoPositionStatus.Initializing:
                    // The Location Service is initializing.
                    // Disable the Start Location button.                    
                    break;
                case GeoPositionStatus.NoData:
                    speedPerSec = 0.0;
                    break;
                case GeoPositionStatus.Ready:
                    speedPerSec = 0.0;
                    break;
            }
        }
        public static double GetDistanceGeodesicMeter(double lon1, double lat1, double lon2, double lat2)
        {
            lon1 = lon1 / 180 * Math.PI;
            lon2 = lon2 / 180 * Math.PI;
            lat1 = lat1 / 180 * Math.PI;
            lat2 = lat2 / 180 * Math.PI;
            return 2 * Math.Asin(Math.Sqrt(Math.Pow((Math.Sin((lat1 - lat2) / 2)), 2) +
             Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin((lon1 - lon2) / 2), 2))) * 6378137; 
        }
        //先经度，后纬度
        public static double GetDistanceGeodesicMeter(string location1, string location2)
        {
            if (location1.Trim().Length == 0 || location2.Trim().Length == 0 || location1.Contains(",") == false || location2.Contains(",") == false) return 0;
            string[] loc1 = location1.Split(',');
            string[] loc2 = location2.Split(',');
            if (loc1.Length != 2 && loc2.Length != 2) return 0;
            try
            {
                double lon1 = double.Parse(loc1[0]);
                double lon2 = double.Parse(loc2[0]);
                double lat1 = double.Parse(loc1[1]);
                double lat2 = double.Parse(loc2[1]);
                lon1 = lon1 / 180 * Math.PI;
                lon2 = lon2 / 180 * Math.PI;
                lat1 = lat1 / 180 * Math.PI;
                lat2 = lat2 / 180 * Math.PI;
                return 2 * Math.Asin(Math.Sqrt(Math.Pow((Math.Sin((lat1 - lat2) / 2)), 2) +
                 Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin((lon1 - lon2) / 2), 2))) * 6378137;
            }
            catch (FormatException)
            {
                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public static string GetDistanceGeodesicString(string location1, string location2)
        {
            int distanceMetre = (int)GetDistanceGeodesicMeter(location1, location2) ;
            if (distanceMetre >= 10000)
            {
                distanceMetre = (int)(distanceMetre / 1000);
                return distanceMetre.ToString() + "千米";
            }
            else
            {
                return distanceMetre.ToString() + "米";
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // TODO: 在这里释放托管资源
                watcher.Stop();
                watcher.Dispose();
            }
        }
    }
}

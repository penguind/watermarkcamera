using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Device.Location; //添加引用
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Media.Imaging;
using System.Threading;

namespace 水印相机
{
    // 获取天气的节点
    public class WeatherNode
    {
        [XmlAttribute("date")]
        public string date { get; set; }
        [XmlAttribute("dayPictureUrl")]
        public string dayPictureUrl { get; set; }
        [XmlAttribute("nightPictureUrl")]
        public string nightPictureUrl { get; set; }
        [XmlAttribute("weather")]
        public string weather { get; set; }
        [XmlAttribute("wind")]
        public string wind { get; set; }
        [XmlAttribute("temperature")]
        public string temperature { get; set; }

        public WeatherNode()
        {
            date = "";
            dayPictureUrl = "";
            nightPictureUrl = "";
            weather = "";
            wind = "";
            temperature = "";
        }
    }

    public class AirQuality
    {
        public string aqi { get; set; } //空气质量指数(AQI)，即air quality index，是定量描述空气质量状况的无纲量指数
        public string area { get; set; } //城市名称
        public string co { get; set; } //一氧化碳1小时平均
        public string co_24h { get; set; } //一氧化碳24小时滑动平均
        public string no2 { get; set; } //二氧化氮1小时平均
        public string no2_24h { get; set; } //二氧化氮24小时滑动平均
        public string o3 { get; set; }//臭氧1小时平均
        public string o3_24h { get; set; }//臭氧日最大1小时平均
        public string o3_8h { get; set; }//臭氧8小时滑动平均
        public string o3_8h_24h { get; set; }//臭氧日最大8小时滑动平均
        public string pm10 { get; set; }//颗粒物（粒径小于等于10μm）1小时平均
        public string pm10_24h { get; set; }//颗粒物（粒径小于等于10μm）24小时滑动平均
        public string pm2_5 { get; set; }//颗粒物（粒径小于等于2.5μm）1小时平均
        public string pm2_5_24h { get; set; }//颗粒物（粒径小于等于2.5μm）24小时滑动平均
        public string quality { get; set; }//空气质量指数类别，有“优、良、轻度污染、中度污染、重度污染、严重污染”6类
        public string so2 { get; set; }//二氧化硫1小时平均
        public string so2_24h { get; set; }//二氧化硫24小时滑动平均
        public string primary_pollutant { get; set; }//首要污染物
        public string time_point { get; set; }//数据发布的时间

        public bool isError;//校验是否出错

        public AirQuality()
        {
            isError = false;
            aqi = "";
            area = "";
            co = "";
            co_24h = "";
            no2 = "";
            no2_24h = "";
            o3 = "";
            o3_24h = "";
            o3_8h = "";
            o3_8h_24h = "";
            pm10 = "";
            pm10_24h = "";
            pm2_5 = "";
            pm2_5_24h = "";
            quality = "";
            so2 = "";
            so2_24h = "";
            primary_pollutant = "";
            time_point = "";
        }
    }


    public class WeatherSetting : IDisposable
    {
        // 位置监视器
        private GeoCoordinateWatcher watcher;

        // 地址
        private string location;//uri请求参数
        public string EorW; // 纬度
        public string EorW_short;
        public string NorS; // 经度
        public string NorS_short;
        public string cityName; //城市名
        public string cityName_s;//城市名缩写
        public string districtName; //街道名
        public string provinceName; //省名
        public string provinceName_s;//省名缩写
        public string street; // 街道名
        //public string cityID; //城市ID
        public string weatherText;  //天气信息的返回xml
        public string geoT;  
        
        // 预报的当天天气，最高温和最低温等
        public string tempratureToday_full;
        public string weatherToday_full;//当前天气 
        //天气列表
        List<WeatherNode> weatherList; 
        // 当前最近一次的测量值，温度
        public string tempratureToday;
        public string wind;//风力

        //空气质量
        public AirQuality aqi;

        //如果出错会置为true以提示使用者信息获取失败
        public bool isError;
        // 构造函数
        public WeatherSetting()
        {
            initSettigns();
        }
        private void initSettigns()
        {    
            watcher = null;

            //地理信息
            this.EorW = ""; 
            this.NorS = ""; 
            districtName = "";
            cityName = "";
            provinceName = "";
            street = "";
            location = "";
            //cityID = "";
            geoT = ""; 

            // 空气质量
            aqi = new AirQuality();

            // 天气预报
            weatherText = "";
            tempratureToday = "";
            weatherToday_full = "";
            tempratureToday_full = "";
            wind = "";
            weatherList = new List<WeatherNode>();
        }
        public bool isInfoEnough()
        {
            if ((tempratureToday_full.Length > 0 && geoT.Length > 0 && weatherToday_full.Length > 0) || tempratureToday.Trim().Length > 0)
            {
                return true; 
            }
            return false;
        }
        //事件
        void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            this.EorW = e.Position.Location.Longitude.ToString("0.0000000"); //经度 
            this.NorS = e.Position.Location.Latitude.ToString("0.000000");  //纬度    
            this.getLocationInfo();//加载地理信息，加载之后获取空气质量
        }
        void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    if (watcher.Permission == GeoPositionPermission.Denied)
                    {
                        // The user has disabled the Location Service on their device.
                        this.geoT = "";
                    }
                    else
                    {
                        this.geoT = "";
                    }
                    break;
                case GeoPositionStatus.Initializing:
                    // The Location Service is initializing.
                    // Disable the Start Location button.                    
                    break;
                case GeoPositionStatus.NoData:
                    this.geoT = "";
                    break;
                case GeoPositionStatus.Ready:
                    this.geoT = "";
                    break;
            }
        }
        //自动获取地址并更新天气信息
        public void autoLocating(int zoomSize = 0)
        { 
            if (watcher == null)
            {
                Thread loadInfoThread = new Thread(loadInfo);
                loadInfoThread.Name = "Weather info loading";
                loadInfoThread.IsBackground = true;
                loadInfoThread.Start();
            }
        }
        public void loadInfo()
        { 
            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);// using high accuracy
            watcher.MovementThreshold = 20;// use MovementThreshold to ignore noise in the signal
            //为 StatusChanged 和 PositionChanged 事件添加事件处理程序
            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            watcher.Start();
        }
        //通过自定义的地址获取天气
        public void userLocating(string district,string city,string province)
        {
            if (city.Trim().Length == 0)
            {
                geoT = "City name:";
                return;
            }
            geoT = city + district;
            search(district, city, province);
        }

        //通过百度地图api获取地理信息
        private void getLocationInfo()
        {
            try
            {
                this.location = "http://api.worldweatheronline.com/free/v1/search.ashx?key=" + App.OpenWeather_AK + "&q=" + this.NorS.Trim() + "," + this.EorW.Trim() + "&format=json";
                Uri uri_l = new Uri(this.location);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri_l);
                request.Method = "GET";
                IAsyncResult result = (IAsyncResult)request.BeginGetResponse(ResponseCallbackGeo, request);
            }
            catch (Exception)
            {
                this.geoT = "";
            }
        }
        //分析获取的位置json
        private void ResponseCallbackGeo(IAsyncResult result)
        {
            Stream stream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                HttpWebResponse response = (HttpWebResponse)(request.EndGetResponse(result));
                stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, false);
                string loadText = reader.ReadToEnd();
                JObject jsonObj = null;
                try
                {
                    jsonObj = JObject.Parse(loadText); 
                    districtName = (string)jsonObj["search_api"]["result"][0]["areaName"][0]["value"].ToString();//市
                    cityName = (string)jsonObj["search_api"]["result"][0]["region"][0]["value"].ToString();//省
                    provinceName = (string)jsonObj["search_api"]["result"][0]["country"][0]["value"].ToString();//国
                }
                catch (Exception e)
                {
                    var errstr = e.Message;
                    this.isError = true;
                }
            }
            catch (Exception)
            {
                this.isError = true;
            }
            this.getForeWeatherInfoO();//加载天气信息 
        }
        
        //搜索函数,并将结果添加到list中
        public void search(string district,string city,string province) //区，市，省
        {
            string district_full = district, city_full = city, province_full = province; //记录全名
            //可能包含行政区划名称，应去掉，否则可能无法查询到
            //县级行政区
            if (district.Contains("镇")) district = district.Substring(0, district.IndexOf("镇"));
            else if (district.Contains("乡")) district = district.Substring(0, district.IndexOf("乡"));
            else if (district.Contains("自治县")) district = district.Substring(0, district.IndexOf("自治县"));
            else if (district.Contains("县")) district = district.Substring(0, district.IndexOf("县"));
            else if (district.Contains("自治旗")) district = district.Substring(0, district.IndexOf("自治旗"));
            else if (district.Contains("旗")) district = district.Substring(0, district.IndexOf("旗"));
            else if (district.Contains("林区")) district = district.Substring(0, district.IndexOf("林区"));
            else if (district.Contains("特区")) district = district.Substring(0, district.IndexOf("特区"));
            else if (district.Contains("区")) district = district.Substring(0,district.IndexOf("区"));
            else if (district.Contains("市")) district = district.Substring(0, district.IndexOf("市"));
            if (district.Length == 1) district = district_full; //防止出现最简名中出现行政区划名而被删除 
            //市级行政区
            if (city.Contains("市")) city = city.Substring(0, city.IndexOf("市"));
            else if(city.Contains("地区")) city = city.Substring(0, city.IndexOf("地区"));
            else if(city.Contains("盟")) city = city.Substring(0, city.IndexOf("盟"));
            else if(city.Contains("自治州")) city = city.Substring(0, city.IndexOf("自治州"));
            if (city.Length == 1) cityName_s = city = city_full;
            else cityName_s = city;
            //省级行政区
            if (province.Contains("省")) province = province.Substring(0, province.IndexOf("省"));
            else if (province.Contains("自治区")) province = province.Substring(0, province.IndexOf("自治区"));
            else if (province.Contains("市")) province = province.Substring(0, province.IndexOf("市"));
            else if (province.Contains("特别行政区")) province = province.Substring(0, province.IndexOf("特别行政区"));
            if (province.Length == 1) provinceName_s = provinceName;
            else provinceName_s = province;
            //初步验证,至少应当包含市名
            if(city.Length==0){
                tempratureToday = "";
                weatherToday_full = "";
                tempratureToday_full = "";
                return;
            }
            geoT = city+"·"+district;
            if (geoT.Length > 7) geoT = city; //地址过长 
            getAirQualityInfo();
        }

        // 通过百度api获取天气
        private void getForeWeatherInfoO()
        {
            try
            {
                string uri_s = "http://api.worldweatheronline.com/free/v1/weather.ashx?key=" + App.OpenWeather_AK + "&q="+ this.NorS + "," + this.EorW  + "&num_of_days=3&format=json";
                Uri uri_l = new Uri(uri_s);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri_l);
                IAsyncResult result = (IAsyncResult)request.BeginGetResponse(ResponseCallbackWeatherB, request);
            }
            catch (Exception) { }
        } 
        private void ResponseCallbackWeatherB(IAsyncResult result)
        {
            Stream stream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                HttpWebResponse response = (HttpWebResponse)(request.EndGetResponse(result));
                stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, false);
                string loadText = reader.ReadToEnd();
                JObject jsonObj = null;
                try
                {
                    jsonObj = JObject.Parse(loadText);
                    this.tempratureToday =this.tempratureToday_full = (string)jsonObj["data"]["current_condition"][0]["temp_C"].ToString();//气温
                    this.weatherText = this.weatherToday_full = (string)jsonObj["data"]["current_condition"][0]["weatherDesc"][0]["value"].ToString();//天气
                    this.wind = (string)jsonObj["data"]["current_condition"][0]["windspeedMiles"].ToString();//风速
                }
                catch (Exception e)
                {
                    var errstr = e.Message;
                    this.isError = true;
                }
            }
            catch (Exception)
            {
                this.isError = true;
            }
        }

        //获取最近发布的空气质量信息
        private void getAirQualityInfo()
        {
            try
            {
                string location = "http://www.pm25.in/api/querys/aqi_details.json?city="+cityName_s+"&token="+App.PM25IN_AK+"&stations=no";
                Uri uri_l = new Uri(location);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri_l);
                IAsyncResult result = (IAsyncResult)request.BeginGetResponse(ResponseCallbackAQI, request);
            }
            catch (Exception)
            {
            }
        }
        //分析获取的最近发布的空气质量信息
        private void ResponseCallbackAQI(IAsyncResult result)
        {
            Stream stream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                HttpWebResponse response = (HttpWebResponse)(request.EndGetResponse(result));
                stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, false); 
                string aqiText = reader.ReadToEnd();
                aqiText = aqiText.Remove(0, 1);
                aqiText = aqiText.Remove(aqiText.Length - 1, 1);
                JObject jsonObj = null;
                try
                {
                    jsonObj = JObject.Parse(aqiText);
                    /*[{"aqi":319,"area":"包头","co":3.992,"co_24h":5.001,"no2":105,"no2_24h":95,"o3":20,"o3_24h":34,"o3_8h":16,"o3_8h_24h":34,"pm10":434,"pm10_24h":293,"pm2_5":215,"pm2_5_24h":142,"quality":"严重污染","so2":318,"so2_24h":214,"primary_pollutant":"颗粒物(PM10)","time_point":"2014-01-23T15:00:00Z"}]*/
                    if (jsonObj.Count==1) aqi.isError = true;
                    else
                    {
                        aqi.aqi = ((int)jsonObj["aqi"]).ToString();
                        aqi.area = (string)jsonObj["area"];
                        aqi.co = ((double)jsonObj["co"]).ToString();
                        aqi.co_24h = ((double)jsonObj["co_24h"]).ToString();
                        aqi.no2 = ((int)jsonObj["no2"]).ToString();
                        aqi.no2_24h = ((int)jsonObj["no2_24h"]).ToString();
                        aqi.o3 = ((int)jsonObj["o3"]).ToString();
                        aqi.o3_24h = ((int)jsonObj["o3_24h"]).ToString();
                        aqi.o3_8h = ((int)jsonObj["o3_8h"]).ToString();
                        aqi.o3_8h_24h = ((int)jsonObj["o3_8h_24h"]).ToString();
                        aqi.pm10 = ((int)jsonObj["pm10"]).ToString();
                        aqi.pm10_24h = ((int)jsonObj["pm10_24h"]).ToString();
                        aqi.pm2_5 = ((int)jsonObj["pm2_5"]).ToString();
                        aqi.pm2_5_24h = ((int)jsonObj["pm2_5_24h"]).ToString();
                        aqi.quality = (string)jsonObj["quality"];
                        aqi.so2 = ((int)jsonObj["so2"]).ToString();
                        aqi.so2_24h = ((int)jsonObj["so2_24h"]).ToString();
                        aqi.primary_pollutant = (string)jsonObj["primary_pollutant"];
                        aqi.time_point = (string)jsonObj["time_point"];
                        isError = false;
                    }
                }
                catch (Exception)
                {
                    aqi.isError = true;
                } 
            }
            catch (Exception)
            { 
                aqi.isError = true;
            }
        }
        public string getAQIImage(string name) //获取空气质量图片
        {
            if (name.ToLower().Contains("co"))
            {
                return "/WaterMark;component/Assets2/icons/AirQuality/CO.png";
            }
            else if (name.ToLower().Contains("pm10"))
            {
                return "/WaterMark;component/Assets2/icons/AirQuality/Pm10.png";
            }
            else if (name.ToLower().Contains("so"))
            {
                return "/WaterMark;component/Assets2/icons/AirQuality/SO2.png";
            }
            else if (name.ToLower().Contains("no"))
            {
                return "/WaterMark;component/Assets2/icons/AirQuality/NOx.png";
            }
            else if (name.ToLower().Contains("o3"))
            {
                return "/WaterMark;component/Assets2/icons/AirQuality/O3.png";
            }
            else
            {
                return "/WaterMark;component/Assets2/icons/AirQuality/PM25.png";
            }
        }
        public string getNextAQIName(string name)// 获取下一个空气质量的名称
        {
            if (name.ToLower().Contains("pm25"))
            {
                return "pm10";
            }
            else if (name.ToLower().Contains("pm10"))
            {
                return "so";
            }
            else if (name.ToLower().Contains("so"))
            {
                return "no";
            }
            else if (name.ToLower().Contains("no"))
            {
                return "o3";
            }
            else if (name.ToLower().Contains("o3"))
            {
                return "co";
            }
            else
            {
                return "pm25";
            }
        }
        public string getAQIValue(string name) //获取当前的空气质量的值
        {
            if (name.ToLower().Contains("co"))
            {
                return this.aqi.co;
            }
            else if (name.ToLower().Contains("pm10"))
            {
                return this.aqi.pm10;
            }
            else if (name.ToLower().Contains("so"))
            {
                return this.aqi.so2;
            }
            else if (name.ToLower().Contains("no"))
            {
                return this.aqi.no2;
            }
            else if (name.ToLower().Contains("o3"))
            {
                return this.aqi.o3;
            }
            else if(name.ToLower().Contains("aqi"))
            {
                return this.aqi.aqi;
            }
            else if (name.ToLower().Contains("quality"))
            {
                return this.aqi.quality;
            }
            else if (name.ToLower().Contains("primary_pollutant"))
            {
                return this.aqi.primary_pollutant;
            }
            else if (name.ToLower().Contains("time_point"))
            {
                return this.aqi.time_point;
            }
            else
            {
                if (this.aqi.isError == false) return this.aqi.pm2_5;
                else return "--";
            }
        }

        ///根据天气获取图标
        // Weather1的图标地址 /WaterMark;component
        public string getMarkPic(string test="")
        {
            if (test.Length > 0) weatherToday_full = test.ToLower();
            string picLocation = "/WaterMark;component/Assets2/icons/weather1/";

            if (this.weatherToday_full.Contains("rain"))
            {
                if (this.weatherToday_full.Contains("thunder")) return picLocation + "Storm.png";
                else return picLocation + "CloudRain.png";
            }
            else if (this.weatherToday_full.Contains("snow"))
            {
                return picLocation + "CloudSnow.png";
            } 
            else if (this.weatherToday_full.Contains("cloudy"))
            {
                if (isDaytime()) return picLocation + "Cloudsun.png";
                else return picLocation + "Cloudmoon.png";
            }
            else
            {
                if (isDaytime()) return picLocation + "Sun.png";
                else
                {
                    return picLocation + "MoonStar.png";
                }
            }
        }
        private bool isDaytime()
        {
            if (DateTime.Now.Hour >= 6 && DateTime.Now.Hour <= 19) return true;
            else return false;
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
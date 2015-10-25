using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
using System.Windows.Media;

namespace WaterMark
{
    class Imagew
    {
        public double x; //左上角的横坐标
        public double y; //左上角的纵坐标
        public double height;//高度
        public double width;//宽度
        public string location;//位置
        public Stretch picstretch;//拉伸情况
        public Imagew() { location = ""; }
        public Imagew(double location_x, double location_y, double imageHeight, double imageWidth, string imageLocation = "",Stretch img_stretch=Stretch.Uniform)
        {
            x = location_x;
            y = location_y;
            height = imageHeight;
            width = imageWidth;
            location = imageLocation;
            picstretch = img_stretch;
        }
    }
}

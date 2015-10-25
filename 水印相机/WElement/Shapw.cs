using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace WaterMark
{
    class Shapw
    {
        public System.Windows.Shapes.Shape shape;     //设置一个Text Block
        public double x;  //标记左上角顶点
        public double y;  
        public double x1;
        public double y1;
        public double x2;
        public double y2;
        //标记图形类型
        public enum wShapes{None,Line,PolyLine,Rectangle,Ellipse};
        public wShapes wshapetype;
        //标记画刷
        public enum wBrushType{None,Solid,Linear};
        public wBrushType brushType;
        public Brush wBrush;
        private Shapw shapw;
        //默认构造函数
        public Shapw()
        {
            shape = null;
            x = 0.0;
            y = 0.0;
            x1 = 0.0;
            y1 = 0.0;
            x2 = 0.0;
            y2 = 0.0;
            wshapetype = wShapes.Line;
            brushType = wBrushType.None;
            wBrush = new SolidColorBrush();
        }
        public Shapw(Shapw shap)
        {
            shape = shap.shape;
            x = shap.x;
            y = shap.y;
            x1 = shap.x1;
            y1 = shap.y1;
            x2 = shap.x2;
            y2 = shap.y2;
            wshapetype = shap.wshapetype;
            brushType = shap.brushType;
            wBrush = shap.wBrush;
        }
        //含参构造函数
        public Shapw(double location_x, double location_y, wShapes shape_type, double x1 = 0, double y1 = 0,double x2 = 0 ,double y2 = 0)
        {
            x = location_x;
            y = location_y;
            wshapetype = shape_type;
            brushType = wBrushType.None;
            if(shape is System.Windows.Shapes.Shape) ((System.Windows.Shapes.Shape)(shape)).Margin = new Thickness(x, y, 0, 0);
            switch(shape_type){
                case wShapes.Line:
                    {
                        shape = new System.Windows.Shapes.Line();
                        ((System.Windows.Shapes.Line)(shape)).X1 = x1;
                        ((System.Windows.Shapes.Line)(shape)).Y1 = y1;
                        ((System.Windows.Shapes.Line)(shape)).X2 = x2;
                        ((System.Windows.Shapes.Line)(shape)).Y2 = y2;
                        break;
                    }
                case wShapes.Rectangle:
                    {
                        shape = new System.Windows.Shapes.Rectangle();
                        ((System.Windows.Shapes.Rectangle)(shape)).Width = (x1 > x2) ? (x1 - x2) : (x2 - x1);
                        ((System.Windows.Shapes.Rectangle)(shape)).Height = (y1 > y2) ? (y1 - y2) : (y2 - y1);
                        break;
                    }
                case wShapes.PolyLine://只增加开始的两个节点
                    {
                        shape = new System.Windows.Shapes.Polyline();
                        ((System.Windows.Shapes.Polyline)(shape)).Points.Add(new Point(x1, y1));
                        ((System.Windows.Shapes.Polyline)(shape)).Points.Add(new Point(x2, y2));
                        break;
                    }
                case wShapes.Ellipse:
                    {
                        shape = new System.Windows.Shapes.Ellipse();
                        ((System.Windows.Shapes.Ellipse)(shape)).Width = (x1 > x2) ? (x1 - x2) : (x2 - x1);
                        ((System.Windows.Shapes.Ellipse)(shape)).Height = (y1 > y2) ? (y1 - y2) : (y2 - y1);
                        break;
                    }
                case wShapes.None: break;
                default: break;
            }
            shape.Fill = new SolidColorBrush(new Color()); //填充透明
            shape.Stroke = new SolidColorBrush(new Color());//边框为透明
        }

        //设置边框颜色
        public void setBorderColor(byte colorR,byte colorG,byte colorB,byte colorA)
        {
            Color blackBack = new Color();
            blackBack.A = colorA;
            blackBack.R = colorR;
            blackBack.G = colorG;
            blackBack.B = colorB;
            shape.Stroke = new System.Windows.Media.SolidColorBrush(blackBack);
        }
        //设置填充颜色
        public void setFillBrush(byte colorR,byte colorG,byte colorB,byte colorA)
        {
            this.brushType = wBrushType.Solid;
            Color blackBack = new Color();
            blackBack.A = colorA;
            blackBack.R = colorR;
            blackBack.G = colorG;
            blackBack.B = colorB;
            shape.Fill = new System.Windows.Media.SolidColorBrush(blackBack); 
        }
        //设置填充颜色
        public void setBrush(byte colorR, byte colorG, byte colorB, byte colorA)
        {
            this.brushType = wBrushType.Solid;
            Color blackBack = new Color();
            blackBack.A = colorA;
            blackBack.R = colorR;
            blackBack.G = colorG;
            blackBack.B = colorB;
            shape.Stroke = shape.Fill = new System.Windows.Media.SolidColorBrush(blackBack);
        }

        //为多边线条增加节点
        public void addPolylineNode(double x,double y)
        {
            if (this.wshapetype == wShapes.PolyLine)
            {
                ((System.Windows.Shapes.Polyline)(shape)).Points.Add(new Point(x, y));
            }
        }

    }
}

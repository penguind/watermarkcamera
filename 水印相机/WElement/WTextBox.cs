//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows;
//using System.Windows.Media;
//using System.Windows.Controls;

//namespace 水印相机
//{
//    /*********************************************
//     *                                           *
//     *          用于建立一个Textblock            *
//     *                                           *
//     *********************************************/
    
//    class WTextBox
//    {
//        public TextBox tb;     //设置一个Text Box
//        public double x;  //Text Block的横坐标
//        public double y;  //Text Block的纵坐标


//        /*默认构造函数*/
//        public WTextBox()
//        {
//            x = 0;
//            y = 0;
//            tb = new TextBox();
//            this.tb.AcceptsReturn = true;
//            tb.FontFamily = new FontFamily("Segoe WP Semibold");
//            tb.FontSize = 1.0;
//            tb.FontWeight = FontWeights.Normal;
//            tb.Foreground = new System.Windows.Media.SolidColorBrush(Colors.White);
//            tb.Text = "";
//        }

//        //全部参数的构造函数
//        public WTextBox(double x, double y,string text, double fontsize=1.0, double bmpWidth=0, 
//            byte bgr = 0,byte bgg = 0,byte bgb = 0,byte bga = 60,
//            string fontfamily = "Segoe WP", int fontweight = 0, byte colorR = 255, byte colorG = 255, byte colorB = 255, byte colorA = 255)
//        {
//            this.x = x;
//            this.y = y;
//            tb = new TextBox();
//            this.tb.AcceptsReturn = true;
//            tb.FontFamily = new FontFamily(fontfamily);
//            tb.FontSize = (fontsize<0.1)?25.0:fontsize;
//            if (fontweight == 0) tb.FontWeight = FontWeights.Thin;
//            else if (fontweight == 1) tb.FontWeight = FontWeights.Normal;
//            else if (fontweight == 2) tb.FontWeight = FontWeights.Bold;
//            else tb.FontWeight = FontWeights.ExtraBold;
//            Color fontColor = new Color();
//            fontColor.A = colorA;
//            fontColor.R = colorR;
//            fontColor.G = colorG;
//            fontColor.B = colorB;
//            tb.Foreground = new System.Windows.Media.SolidColorBrush(fontColor);
//            tb.Text = text;
//            Color bgColor = new Color();
//            bgColor.A = (byte)bga;
//            bgColor.R = (byte)bgr;
//            bgColor.G = (byte)bgg;
//            bgColor.B = (byte)bgb;
//            tb.Background = new System.Windows.Media.SolidColorBrush(bgColor);
//        }

//        //拷贝构造函数
//        public WTextBox(WTextBox wTextBox)
//        {
//            this.x = wTextBox.x;
//            this.y = wTextBox.y;
//            this.tb = new TextBox();
//            this.tb.AcceptsReturn = true;
//            this.tb.FontFamily = wTextBox.tb.FontFamily;
//            this.tb.FontSize = wTextBox.tb.FontSize;
//            this.tb.FontWeight = wTextBox.tb.FontWeight;
//            this.tb.Foreground = wTextBox.tb.Foreground;
//            this.tb.Text = wTextBox.tb.Text;
//            this.tb.Background = wTextBox.tb.Background;
//        }

//        //设置前景色
//        public void setFontColor(int R,int G,int B,int A = 255)
//        {
//            Color fontColor = new Color();
//            fontColor.A = (byte)A;
//            fontColor.R = (byte)R;
//            fontColor.G = (byte)G;
//            fontColor.B = (byte)B;
//            tb.Foreground = new System.Windows.Media.SolidColorBrush(fontColor);
//        }
//        //设置背景颜色
//        public void setBGColor(int r, int g, int b, int a = 255)
//        {
//            Color bgColor = new Color();
//            bgColor.A = (byte)a;
//            bgColor.R = (byte)r;
//            bgColor.G = (byte)g;
//            bgColor.B = (byte)b;
//        }
//        // 设置高度
//        public void setHeight()
//        {
//            int linenumber = 1;
//            if (this.tb.Text.Length > 0&&this.tb.Text.Contains('\n'))
//            {
//                linenumber = this.tb.Text.Split('\n').Length;
//            }
//            this.tb.Height = this.tb.FontSize * linenumber * 1.2; 
//        }

//        public void setFont(string fontfamily, double fontsize,int fontweight)
//        {
//            tb.FontFamily = new FontFamily(fontfamily);
//            tb.FontSize = fontsize;
//            if (fontweight == 0) tb.FontWeight = FontWeights.Thin;
//            else if (fontweight == 1) tb.FontWeight = FontWeights.Normal;
//            else if (fontweight == 2) tb.FontWeight = FontWeights.Bold;
//            else tb.FontWeight = FontWeights.ExtraBold;
//        }
//    }

//    /********************************  参考  *************************************************
//     * 
//     *     所有 Windows Phone 手机上都包含以下 Segoe WP 字体的变体：
//     *  Segoe WP Light ,Segoe WP SemiLight ,Segoe WP ,Segoe WP Semibold ,
//     *  Segoe WP Bold ,Segoe WP Black
//     *****************************************************************************************/
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace 水印相机
{
    /*********************************************
     *                                           *
     *          用于建立一个Textblock            *
     *                                           *
     *********************************************/
    
    class WTextBlock
    {
        public TextBlock tb;     //设置一个Text Block
        public double x;  //Text Block的横坐标
        public double y;  //Text Block的纵坐标


        /*默认构造函数*/
        public WTextBlock()
        {
            x = 0;
            y = 0;
            tb = new TextBlock();
            tb.FontFamily = new FontFamily("Segoe WP Semibold");
            tb.FontSize = 1.0;
            tb.FontWeight = FontWeights.Normal;
            tb.Foreground = new System.Windows.Media.SolidColorBrush(Colors.White);
            tb.Text = "";
            tb.TextAlignment = TextAlignment.Left;
        }

        //全部参数的构造函数
        public WTextBlock(double x, double y,string text, double fontsize=1.0, string fontfamily = "Segoe WP", int fontweight = 0, byte colorR = 255, byte colorG = 255, byte colorB = 255, byte colorA = 255,TextAlignment alignment = TextAlignment.Left,double picWidth=0.0)
        {
            this.x = x;
            this.y = y;
            tb = new TextBlock();
            tb.FontFamily = new FontFamily(fontfamily);
            tb.FontSize = (fontsize<0.1)?25.0:fontsize;
            if (fontweight == 0) tb.FontWeight = FontWeights.Thin;
            else if (fontweight == 1) tb.FontWeight = FontWeights.Normal;
            else if (fontweight == 2) tb.FontWeight = FontWeights.Bold;
            else tb.FontWeight = FontWeights.ExtraBold;
            tb.Margin = new Thickness(x, y, 0, 0) ; 
            Color fontColor = new Color();
            fontColor.A = colorA;
            fontColor.R = colorR;
            fontColor.G = colorG;
            fontColor.B = colorB;
            tb.Foreground = new System.Windows.Media.SolidColorBrush(fontColor);
            tb.Text = text;
            tb.TextAlignment = alignment;
            //picWidth仅用于当文字位置不是left时，即需要根据图片大小才能决定时
            if (alignment != TextAlignment.Left)
            {
                tb.Width = picWidth;
            }
            tb.TextWrapping = TextWrapping.Wrap;
        }

        //全部参数的构造函数,Color使用单独的对象而非byte
        public WTextBlock(double x, double y, string text,Color fontcolor, double fontsize = 1.0, string fontfamily = "Segoe WP", int fontweight = 0, TextAlignment alignment = TextAlignment.Left, double picWidth = 0.0)
        {
            this.x = x;
            this.y = y;
            tb = new TextBlock();
            tb.Margin = new Thickness(x, y, 0, 0);
            tb.FontFamily = new FontFamily(fontfamily);
            tb.FontSize = (fontsize < 0.1) ? 25.0 : fontsize;
            if (fontweight == 0) tb.FontWeight = FontWeights.Thin;
            else if (fontweight == 1) tb.FontWeight = FontWeights.Normal;
            else if (fontweight == 2) tb.FontWeight = FontWeights.Bold;
            else tb.FontWeight = FontWeights.ExtraBold;
            tb.Foreground = new System.Windows.Media.SolidColorBrush(fontcolor);
            tb.Text = text;
            tb.TextAlignment = alignment;
            //picWidth仅用于当文字位置不是left时，即需要根据图片大小才能决定时
            if (alignment != TextAlignment.Left)
            {
                tb.Width = picWidth;
            }
            tb.TextWrapping = TextWrapping.Wrap;
        }

        //拷贝构造函数
        public WTextBlock(WTextBlock wTextBlock)
        {
            this.x = wTextBlock.x;
            this.y = wTextBlock.y;
            this.tb = new TextBlock();
            this.tb.FontFamily = wTextBlock.tb.FontFamily;
            this.tb.FontSize = wTextBlock.tb.FontSize;
            this.tb.FontWeight = wTextBlock.tb.FontWeight;
            this.tb.Foreground = wTextBlock.tb.Foreground;
            this.tb.Text = wTextBlock.tb.Text;
            tb.TextAlignment = wTextBlock.tb.TextAlignment;
            tb.Margin = new Thickness(tb.Margin.Left, tb.Margin.Top, tb.Margin.Right, tb.Margin.Bottom);
            tb.TextWrapping = wTextBlock.tb.TextWrapping;
        }

        //set函数
        public void setFontColor(int R,int G,int B,int A = 255)
        {
            Color fontColor = new Color();
            fontColor.A = (byte)A;
            fontColor.R = (byte)R;
            fontColor.G = (byte)G;
            fontColor.B = (byte)B;
            tb.Foreground = new System.Windows.Media.SolidColorBrush(fontColor);
        }

        public void setFont(string fontfamily, double fontsize,int fontweight)
        {
            tb.FontFamily = new FontFamily(fontfamily);
            tb.FontSize = fontsize;
            if (fontweight == 0) tb.FontWeight = FontWeights.Thin;
            else if (fontweight == 1) tb.FontWeight = FontWeights.Normal;
            else if (fontweight == 2) tb.FontWeight = FontWeights.Bold;
            else tb.FontWeight = FontWeights.ExtraBold;
        }

        public void setMargin(double x = -1, double y = -1)
        {
            if (x == -1 && y == -1) return;
            else if (x == -1 && y != -1)
            {
                tb.Margin = new Thickness(this.x, y, 0, 0);
                this.y = y;
            }
            else if (x != -1 && y == -1)
            {
                tb.Margin = new Thickness(x, this.y, 0, 0);
                this.x = x;
            }
            else
            {
                tb.Margin = new Thickness(x, y, 0, 0);
                this.x = x;
                this.y = y;
            }
        }
    }

    /********************************  参考  *************************************************
     * 
     *     所有 Windows Phone 手机上都包含以下 Segoe WP 字体的变体：
     *  Segoe WP Light ,Segoe WP SemiLight ,Segoe WP ,Segoe WP Semibold ,
     *  Segoe WP Bold ,Segoe WP Black
     *****************************************************************************************/
}

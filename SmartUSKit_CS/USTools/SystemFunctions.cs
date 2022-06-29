using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SmartUSKit_CS.USTools
{
    internal class SystemFunctions
    {
        /// <summary>   
        /// 设置鼠标的坐标   
        /// </summary>   
        /// <param name="x">横坐标</param>   
        /// <param name="y">纵坐标</param>   
        [DllImport("User32")]
        public extern static void SetCursorPos(int x, int y);

        /// <summary>   
        /// 获取鼠标的坐标   
        /// </summary>   
        /// <param name="lpPoint">传址参数，坐标point类型</param>   
        /// <returns>获取成功返回真</returns>   
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out SystemScreenPoint pt);
        public struct SystemScreenPoint
        {
            public int X;
            public int Y;
            public SystemScreenPoint(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }


        public struct Dpi
        {
            public double X { get; set; }

            public double Y { get; set; }

            public Dpi(double x, double y)
            {
                X = x;
                Y = y;
            }
        }
        public static Dpi GetDpiFromVisual(Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);

            var dpiX = 96.0;
            var dpiY = 96.0;

            if (source?.CompositionTarget != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            return new Dpi(dpiX, dpiY);
        }
        public static (double scaleX,double scaleY) GetScreenScaleFromVisual(Visual visual)
        {
            var source = PresentationSource.FromVisual(visual);

            var dpiX = 96.0;
            var dpiY = 96.0;

            if (source?.CompositionTarget != null)
            {
                dpiX = 96.0 * source.CompositionTarget.TransformToDevice.M11;
                dpiY = 96.0 * source.CompositionTarget.TransformToDevice.M22;
            }

            return (source.CompositionTarget.TransformToDevice.M11, source.CompositionTarget.TransformToDevice.M22);
        }
    }
}

using SmartUSKit.SmartUSKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SmartUSKit_CS.wirelessusg3
{
    public static class USScaleView
    {
        //  public static  int COLOR_NORMAL = 0Xffffffff;
        // public static  int COLOR_COLORMODE = 0xff00ff00;



        static double TickStrokeThickness = 1;

        #region 记录上次的值，用来判断是否需要更新界面
        static Canvas Last_Canvas = new Canvas();
        static float[] Lats_Focus = new float[] { 0f };
        static double Last_Scale = -1000f;
        static Brush Last_brush = Brushes.White;
        #endregion

        static bool ArrayEqualArray(float[] ArrayA, float[] ArrayB)
        {
            //如果两个都是空，那么也认为他们是相等的
            if (ArrayA == null
                && ArrayB == null)
            {
                return true;
            }
            if (ArrayA == null)
            {
                return false;
            }
            if (ArrayB == null)
            {
                return false;
            }
            if (ArrayA.Length != ArrayB.Length)
            {
                return false;
            }
            for (int i = 0; i < ArrayA.Length; i++)
            {
                if (ArrayA[i] != ArrayB[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static void DrawScaleFocus(Canvas canvas, float[] newFocus, double newScale, Brush newBrush)
        {

            //优化绘制逻辑，主要参数未变化时，不绘制
            //主要参数包含  focus, scale,color

            if (canvas == null
                 // || newFocus==null
                 || newBrush == null
                 )
            {
                return;
            }
            var qqq = Last_Canvas.GetHashCode();
            var www = canvas.GetHashCode();
            if (Last_Canvas == canvas
                && Last_Scale == newScale
                && Last_brush == newBrush
                && ArrayEqualArray(Lats_Focus, newFocus)
                )
            {
                return;
            }
            #region 将新值保存起来
            Last_Canvas = canvas;

            if (Last_Canvas.ActualHeight <= 0
                || Last_Canvas.ActualWidth <= 0)
            {
                Last_Canvas.SizeChanged += Last_Canvas_SizeChanged;
                return;
            }

            if (newFocus != null)
            {
                Lats_Focus = new float[newFocus.Length];
                for (int i = 0; i < newFocus.Length; i++)
                {
                    Lats_Focus[i] = newFocus[i];
                }
            }
            else
            {
                Lats_Focus = newFocus;
            }

            Last_Scale = newScale;
            Last_brush = newBrush;
            #endregion

            DrawScaleFocus_内部(Last_Canvas, Lats_Focus, Last_Scale, Last_brush);
            return;




            double tick = 5;
            double tickmount = canvas.ActualHeight * Last_Scale / tick;
            if (tickmount > 30)
            {
                tick = 10;
            }
            double lenght = 5;
            canvas.Children.Clear();
            lines.Clear();

            Line verticalLine = new Line();
            verticalLine.X1 = 0;
            verticalLine.X2 = 0;
            verticalLine.Y1 = 0;
            verticalLine.Y2 = canvas.ActualHeight;
            verticalLine.Stroke = Brushes.White;
            verticalLine.StrokeThickness = 1;
            canvas.Children.Add(verticalLine);

            //Debug.WriteLine($"刻度总数：{tickmount}");

            for (int i = 0; i < 1000; i++)
            {
                if (tick * i / Last_Scale > canvas.ActualHeight)
                {
                    break;
                }
                if (i % 2 == 0)
                {
                    //长刻度
                    Line line = new Line();
                    line.X1 = 0;
                    line.X2 = lenght * 1.4;
                    line.Y1 = line.Y2 = tick * i / Last_Scale;
                    line.Stroke = Brushes.White;
                    line.StrokeThickness = TickStrokeThickness;
                    canvas.Children.Add(line);

                    TextBlock textBlock = new TextBlock();
                    textBlock.FontSize = 8;
                    textBlock.Text = (i * tick).ToString();
                    Canvas.SetLeft(textBlock, line.X2 + 3);
                    if (i == 0)
                    {
                        Canvas.SetTop(textBlock, line.Y1);
                    }
                    else
                    {
                        Canvas.SetTop(textBlock, line.Y1 - textBlock.FontSize / 2);
                    }

                    textBlock.Foreground = Brushes.White;
                    canvas.Children.Add(textBlock);
                }
                else
                {
                    //短刻度
                    Line line = new Line();
                    line.X1 = 0;
                    line.X2 = lenght;
                    line.Y1 = line.Y2 = tick * i / Last_Scale;
                    line.Stroke = Brushes.White;
                    line.StrokeThickness = TickStrokeThickness;
                    canvas.Children.Add(line);
                }
            }
            if (Lats_Focus != null)
            {
                #region Focus
                for (int i = 0; i < Lats_Focus.Length; i++)
                {
                    Path myPath = new Path
                    {
                        Stroke = Last_brush,
                        Fill = Last_brush,
                        StrokeThickness = 2,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    PathGeometry pathGeometry = new PathGeometry();
                    PathFigureCollection figures = new PathFigureCollection();
                    var pointpathSegmentCollection = new PathSegmentCollection();
                    int halfHeight = 5;
                    pointpathSegmentCollection.Add(new LineSegment(new Point(8, 0), true));
                    pointpathSegmentCollection.Add(new LineSegment(new Point(8, 2 * halfHeight), true));
                    var pathFigure = new PathFigure(new Point(2, halfHeight), pointpathSegmentCollection, true);
                    figures.Add(pathFigure);
                    pathGeometry.Figures = figures;

                    myPath.Data = pathGeometry;
                    Canvas.SetTop(myPath, Lats_Focus[i] / Last_Scale - halfHeight);
                    canvas.Children.Add(myPath);
                }
                #endregion
            }
        }

        private static void DrawScaleFocus_内部(Canvas canvas, float[] newFocus, double newScale, Brush newBrush)
        {
            Debug.WriteLine("绘制了比例尺");
            double tick = 5;
            double tickmount = canvas.ActualHeight * Last_Scale / tick;
            if (tickmount > 30)
            {
                tick = 10;
            }
            double lenght = 5;
            canvas.Children.Clear();

            Line verticalLine = new Line();
            verticalLine.X1 = 0;
            verticalLine.X2 = 0;
            verticalLine.Y1 = 0;
            verticalLine.Y2 = canvas.ActualHeight;
            verticalLine.Stroke = Brushes.White;
            verticalLine.StrokeThickness = 1;
            canvas.Children.Add(verticalLine);

            for (int i = 0; i < 1000; i++)
            {
                if (tick * i / Last_Scale > canvas.ActualHeight)
                {
                    break;
                }
                if (i % 2 == 0)
                {
                    //长刻度
                    Line line = new Line();
                    line.X1 = 0;
                    line.X2 = lenght * 1.4;
                    line.Y1 = line.Y2 = tick * i / Last_Scale;
                    line.Stroke = Brushes.White;
                    line.StrokeThickness = TickStrokeThickness;
                    lines.Add(line);
                    canvas.Children.Add(line);

                    TextBlock textBlock = new TextBlock();
                    textBlock.FontSize = 8;
                    textBlock.Text = (i * tick).ToString();
                    Canvas.SetLeft(textBlock, line.X2 + 3);
                    if (i == 0)
                    {
                        Canvas.SetTop(textBlock, line.Y1);
                    }
                    else
                    {
                        Canvas.SetTop(textBlock, line.Y1 - textBlock.FontSize / 2);
                    }

                    textBlock.Foreground = Brushes.White;
                    canvas.Children.Add(textBlock);
                }
                else
                {
                    //短刻度
                    Line line = new Line();
                    line.X1 = 0;
                    line.X2 = lenght;
                    line.Y1 = line.Y2 = tick * i / Last_Scale;
                    line.Stroke = Brushes.White;
                    line.StrokeThickness = TickStrokeThickness;
                    canvas.Children.Add(line);
                }
            }
            if (Lats_Focus != null)
            {
                #region Focus
                for (int i = 0; i < Lats_Focus.Length; i++)
                {
                    Path myPath = new Path
                    {
                        Stroke = Last_brush,
                        Fill = Last_brush,
                        StrokeThickness = 2,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    PathGeometry pathGeometry = new PathGeometry();
                    PathFigureCollection figures = new PathFigureCollection();
                    var pointpathSegmentCollection = new PathSegmentCollection();
                    int halfHeight = 5;
                    pointpathSegmentCollection.Add(new LineSegment(new Point(8, 0), true));
                    pointpathSegmentCollection.Add(new LineSegment(new Point(8, 2 * halfHeight), true));
                    var pathFigure = new PathFigure(new Point(2, halfHeight), pointpathSegmentCollection, true);
                    figures.Add(pathFigure);
                    pathGeometry.Figures = figures;

                    myPath.Data = pathGeometry;
                    Canvas.SetTop(myPath, Lats_Focus[i] / Last_Scale - halfHeight);
                    canvas.Children.Add(myPath);
                }
                #endregion
            }
        }


        private static void Last_Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Debug.WriteLine("尺寸改变了");
            //有时候canvas还未加载完，ActualHeight和ActualWidht为0，等到加载完的时候，界面canvas的尺寸会更新，更新的时候，界面上的比例尺和焦点应该重绘
            DrawScaleFocus_内部(Last_Canvas, Lats_Focus, Last_Scale, Last_brush);
        }

        [Obsolete("抛弃这个方法了，请使用静态方法：public static void DrawScaleFocus(Canvas canvas,float[] newFocus,double newScale, Brush newBrush)", true)]
        public static void DrawScaleFocus(Canvas canvas)
        {

            //优化绘制逻辑，主要参数未变化时，不绘制
            //主要参数包含  focus, scale,color


            double tick = 5;
            double tickmount = canvas.ActualHeight * scale / tick;
            if (tickmount > 30)
            {
                tick = 10;
            }
            double lenght = 5;
            canvas.Children.Clear();

            Line verticalLine = new Line();
            verticalLine.X1 = 0;
            verticalLine.X2 = 0;
            verticalLine.Y1 = 0;
            verticalLine.Y2 = canvas.ActualHeight;
            verticalLine.Stroke = Brushes.White;
            verticalLine.StrokeThickness = 1;
            lines.Add(verticalLine);
            canvas.Children.Add(verticalLine);

            //Debug.WriteLine($"刻度总数：{tickmount}");

            for (int i = 0; i < 1000; i++)
            {
                if (tick * i / scale > canvas.ActualHeight)
                {
                    break;
                }
                if (i % 2 == 0)
                {
                    //长刻度
                    Line line = new Line();
                    line.X1 = 0;
                    line.X2 = lenght * 1.4;
                    line.Y1 = line.Y2 = tick * i / scale;
                    line.Stroke = Brushes.White;
                    line.StrokeThickness = TickStrokeThickness;
                    lines.Add(line);
                    canvas.Children.Add(line);

                    TextBlock textBlock = new TextBlock();
                    textBlock.FontSize = 8;
                    textBlock.Text = (i * tick).ToString();
                    Canvas.SetLeft(textBlock, line.X2 + 3);
                    if (i == 0)
                    {
                        Canvas.SetTop(textBlock, line.Y1);
                    }
                    else
                    {
                        Canvas.SetTop(textBlock, line.Y1 - textBlock.FontSize / 2);
                    }

                    textBlock.Foreground = Brushes.White;
                    canvas.Children.Add(textBlock);
                }
                else
                {
                    //短刻度
                    Line line = new Line();
                    line.X1 = 0;
                    line.X2 = lenght;
                    line.Y1 = line.Y2 = tick * i / scale;
                    line.Stroke = Brushes.White;
                    line.StrokeThickness = TickStrokeThickness;
                    lines.Add(line);
                    canvas.Children.Add(line);
                }
            }
            if (flocusList != null)
            {
                #region Focus
                for (int i = 0; i < flocusList.Length; i++)
                {
                    Path myPath = new Path
                    {
                        Stroke = color,
                        Fill = color,
                        StrokeThickness = 2,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    PathGeometry pathGeometry = new PathGeometry();
                    PathFigureCollection figures = new PathFigureCollection();
                    var pointpathSegmentCollection = new PathSegmentCollection();
                    int halfHeight = 5;
                    pointpathSegmentCollection.Add(new LineSegment(new Point(8, 0), true));
                    pointpathSegmentCollection.Add(new LineSegment(new Point(8, 2 * halfHeight), true));
                    var pathFigure = new PathFigure(new Point(2, halfHeight), pointpathSegmentCollection, true);
                    figures.Add(pathFigure);
                    pathGeometry.Figures = figures;

                    myPath.Data = pathGeometry;
                    Canvas.SetTop(myPath, flocusList[i] / scale - halfHeight);
                    canvas.Children.Add(myPath);
                }
                #endregion
            }
        }

        public static void DrawFocus(float[] focusPos)
        {
            //if (focusPos[0] <= 0)
            //{
            //    return;
            //}
            //focusPath = new Path[2];
            //focusPath[0] = new Path();
            //focusPath[0].moveTo(0, (float)(focusPos[0] / scale));
            //focusPath[0].lineTo(20, (float)(focusPos[0] / scale) - 15);
            //focusPath[0].lineTo(20, (float)(focusPos[0] / scale) + 15);
            //focusPath[0].close();
            //if (focusPos[1] > 0)
            //{
            //    focusPath[1] = new Path();
            //    focusPath[1].moveTo(0, (float)(focusPos[1] / scale));
            //    focusPath[1].lineTo(20, (float)(focusPos[1] / scale) - 15);
            //    focusPath[1].lineTo(20, (float)(focusPos[1] / scale) + 15);
            //}
            //else
            //{
            //    focusPath[1] = null;
            //}
            //   invalidate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="focus"></param>
        /// <param name="brush">焦点三角形的颜色</param>
        static void SetFocus(float[] focus, Brush brush)
        {
            SetFocus(focus, brush, true);
        }
        static void SetFocus(float[] focus, Brush brush, bool update)
        {
            flocusList = focus;
            color = brush;
            if (update)
            {
                // invalidate();
            }
        }

        static void SetRevert(Boolean revert)
        {
            isRevert = revert;
        }

        private static void SetScale(double scale)
        {
            SetScale(scale, true);
        }
        static void SetScale(double scale, bool update)
        {
            scale = scale;
            if (update)
            {
                // invalidate();
            }
        }

        static float[] flocusList;
        /// <summary>
        /// 焦点三角形的颜色
        /// </summary>
        static Brush color;
        static Boolean isRevert = false;

        private static double scale = 0.2;
        //Path[] focusPath;

        static List<Line> lines = new List<Line>();
    }
}

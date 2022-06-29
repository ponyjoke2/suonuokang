using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using SmartUSKit.SmartUSKit;
using SmartUSKit_CS.Model;
using ShaderEffectLibrary;
using SmartUSKit_CS.USViewers;
using SmartUSKit_CS.wirelessusg3;
using System.Windows.Threading;
using SmartUSKit.Enums;

namespace SmartUSKit_CS
{
    /// <summary>
    /// USGeneralView.xaml 的交互逻辑
    /// </summary>
    public partial class USBMViewer : USViewer
    {
        #region 测量
        //USMarkView markView = new USMarkView();
        /// <summary>
        /// 保存当前点击的图标
        /// </summary>
        Point SelectedPoint = new Point();
        #region 画Trace需要的变量
        /// <summary>
        /// 记录鼠标移动过程中上一次的位置
        /// </summary>
        Point lastPosition = new Point();
        /// <summary>  
        /// 点集合  
        /// </summary>  
        List<Point> pointList = new List<Point>();
        /// <summary>  
        /// 起始位置  
        /// </summary>  
        Point startPoint;
        /// <summary>
        /// 滑动起始位置
        /// </summary>
        Point slidestartPoint = new Point();
        bool isSliding = false;
        #endregion
        #endregion
        public const String BM_DSCOR = "BM_DSC";
        private static float[] oldFocuses;
        private static double oldScale;
        USDSCor BMdscor;

        private int SelectedLine = 0;
        Image FirstPoint = new Image();
        Image SecondPoint = new Image();
        List<Line> lines = new List<Line>();

        bool isHaveMousedown = false;

        Image MovePoint = new Image();
        Line MoveLine = new Line()
        {
            Stroke = Brushes.Lime,
            StrokeThickness = 1
        };
        protected USBMImage bmImage = new USBMImage();
        protected int bmLine = -1;

        int lineCount = 128;
        int sampleCount = 512;
        private BMMoveImage MoveImage = new BMMoveImage();
        private bool Freeze = true;
        // int CurrentImageIndex = 0;

        private USDSCor.USPoint anchorStart = new USDSCor.USPoint();
        private USDSCor.USPoint anchorEnd = new USDSCor.USPoint();

        static bool loaded = false;
        protected static USBMViewer instance = null;
        public static USBMViewer GetInstance()
        {
            if (instance == null)
            {
                instance = new USBMViewer();
            }
            return instance;
        }
        private USBMViewer()
        {
            InitializeComponent();
            InitHeartbeat();
            this.UserIdLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("UserId"), Source = Patient.GetInstance() });
            this.UserNameLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("Name"), Source = Patient.GetInstance() });
            this.GenderLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("Gender"), Source = Patient.GetInstance() });
            this.AgeLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("Age"), Source = Patient.GetInstance() });

            this.PatientStackpanel.SetBinding(Label.VisibilityProperty, new Binding() { Path = new PropertyPath("PatientVisible"), Source = Preset.GetInstance() });

            this.InfoStackpanel.SetBinding(Label.VisibilityProperty, new Binding() { Path = new PropertyPath("InfoVisible"), Source = Preset.GetInstance() });
            this.InfoStackpanel.SetBinding(Label.OpacityProperty, new Binding() { Path = new PropertyPath("StackpanelOpacity"), Source = Preset.GetInstance() });

            this.LabelHeartbeat.Content = Properties.Resources.Heartbeat + "--bpm";
            this.StateLabel.Content = Properties.Resources.StateLive;
            if (!CanvasImage.Children.Contains(MoveLine))
            {
                CanvasImage.Children.Add(MoveLine);
            }
            if (!CanvasImage.Children.Contains(MovePoint))
            {
                CanvasImage.Children.Add(MovePoint);
            }
            MovePoint.Source = new BitmapImage(new Uri("/Resources/MeasureIcons/cha.png", UriKind.Relative));
            MovePoint.Cursor = Cursors.Hand;
            MovePoint.Width = 30;
            MovePoint.Height = 30;

           

            MovePoint.MouseDown += MovePoint_MouseDown;
            CanvasImage.MouseMove += MovePoint_MouseMove;
            CanvasImage.MouseUp += MovePoint_MouseUp;
        }
        #region 重写父类方法
        public override void UpdateCount(int index, int count)
        {
            int CurrentIndex = index + 1;

            ImageCountLabel.Content = CurrentIndex.ToString() + "/" + count.ToString();
        }

        public override void UpdateState(int freeze)
        {
            if (freeze == -1)
            {
                StateLabel.Content = Properties.Resources.SavedImage;
                return;
            }
            if ((freeze == (int)USViewer.STATE_LIVE))
            {
            }
            else
            {
            }
            if (freeze == (int)USViewer.STATE_FREEZE)
            {
                StateLabel.Content = Properties.Resources.StateFreeze;
                Freeze = true;

            }
            else if (freeze == (int)USViewer.STATE_LIVE)
            {
                StateLabel.Content = Properties.Resources.StateLive;
                Freeze = false;
            }
            else
            {
                StateLabel.Content = Properties.Resources.StateReplay;
                Freeze = true;
            }
            InitHeartbeatEvent();
        }
        #endregion
        public override void SetRawImage(USRawImage rawImage)
        {
            try
            {
                generalImage = rawImage;
                this.LabelHeartbeat.Content = Properties.Resources.Heartbeat + "--bpm";
                bmImage = (USBMImage)rawImage;
                lineCount = rawImage.probeCap.imagingParameter.lineCount;
                
                BMdscor = USDSCor.GetInstance(BM_DSCOR);

                int width = (int)(imageView.ActualWidth + 0.5);
                int height = (int)(imageView.ActualHeight + 0.5);
                if ((width == 0 || height == 0)
                 && (BMdscor.GetDestHeight() == 0 || BMdscor.GetDestWidth() == 0))
                {
                    NeedReloadImage = true;
                    return;
                }
                BMdscor.SetDestSize(width, height);

                sampleCount = rawImage.probeCap.imagingParameter.sampleCount;
                int dscWidth = BMdscor.GetDscWidth();
                int dscHeight = BMdscor.GetDscHeight();

                //this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() =>
                //{
                USBMImage bmImagen = (USBMImage)rawImage;


                ImageLine.Source = bmImagen.rawBM.CreateBitmap();
                
                this.imageView.Source = BMdscor.DscImage(rawImage);
                this.GrayImage.Source = BMdscor.GetGrayBar(rawImage.probeCap);
                try
                {
                    USDSCor.USPoint greenPoint = BMdscor.DscMap(new USDSCor.USPoint(0, 0));
                    greenPoint.X -= 35;
                    if (greenPoint.X < 40)
                    {
                        greenPoint.X = 40;
                    }
                    greenPoint.Y += 15;
                    Canvas.SetLeft(this.GreenPoint, greenPoint.X);
                }
                catch (Exception)
                {
                }
                timeLabel.Text = rawImage.timeCap.ToString("yyyy-MM-dd HH:mm:ss");
                gainLabel.Content = $"GN:{rawImage.gain}dB";
                int depth = rawImage.probeCap.imagingParameter.namicDepth[rawImage.zoom];
                depthLabel.Content = $"D:{depth}mm";


                bool isbmimage = rawImage.IsEnhanceImage();
                if (rawImage.IsEnhanceImage() && !rawImage.IsEnhanceImage())
                {
                    DRLabel.Visibility = Visibility.Visible;
                    ENHLabel.Visibility = Visibility.Visible;
                    FLabel.Visibility = Visibility.Visible;

                    USEnhanceImage enhanceImage = (USEnhanceImage)rawImage;
                    USEnhanceProbe enhanceProbe = (USEnhanceProbe)enhanceImage.probeCap;
                    ENHLabel.Content = "ENH: " + enhanceImage.enhanceLevel;
                    DRLabel.Content = "DR: " + enhanceImage.dynamicRange;

                    if (enhanceImage.harmonic)
                    {
                        FLabel.Content = "F: H" + enhanceProbe.enhanceParameter.harmonicFrequency.ToString("0.0") + " MHz";
                    }
                    else
                    {
                        FLabel.Content = "F: " + enhanceProbe.enhanceParameter.frequency.ToString("0.0") + " MHz";
                    }

                    if (oldFocuses == null)
                    {
                        oldFocuses = new float[((USEnhanceImage)rawImage).FocusList().Length];
                        oldScale = BMdscor.M_dbScalePixel;
                    }
                    float[] newfocuses = ((USEnhanceImage)rawImage).FocusList();
                    {
                        oldScale = BMdscor.M_dbScalePixel;
                        oldFocuses = ((USEnhanceImage)rawImage).FocusList();
                        
                        USScaleView.DrawScaleFocus(CanvasRuler, oldFocuses, BMdscor.M_dbScalePixel, Brushes.White);
                    }
                }
                else if (rawImage.IsEnhanceImage() && rawImage.IsEnhanceImage())
                {
                    DRLabel.Visibility = Visibility.Visible;
                    ENHLabel.Visibility = Visibility.Visible;
                    FLabel.Visibility = Visibility.Visible;

                    USBMEnhanceImage enhanceImage = (USBMEnhanceImage)rawImage;
                    USEnhanceProbe enhanceProbe = (USEnhanceProbe)enhanceImage.probeCap;
                    ENHLabel.Content = "ENH: " + enhanceImage.enhanceLevel;
                    DRLabel.Content = "DR: " + enhanceImage.dynamicRange;

                    if (enhanceImage.harmonic)
                    {
                        FLabel.Content = "F: H" + enhanceProbe.enhanceParameter.harmonicFrequency.ToString("0.0") + " MHz";
                    }
                    else
                    {
                        FLabel.Content = "F: " + enhanceProbe.enhanceParameter.frequency.ToString("0.0") + " MHz";
                    }

                    var usparam = rawImage.probeCap.getUSParam(ProbeMode.MODE_BM, enhanceImage.harmonic);
                    MILabel.Content = usparam.MI.ToString("0.0");
                    TISLabel.Content = usparam.TIS.ToString("0.0");

                    if (oldFocuses == null)
                    {
                        oldFocuses = new float[((USBMEnhanceImage)rawImage).FocusList().Length];
                        oldScale = BMdscor.M_dbScalePixel;
                    }
                    float[] newfocuses = ((USBMEnhanceImage)rawImage).FocusList();
                    {
                        #region 更新bmline
                        BMdscor = USDSCor.GetInstance(BM_DSCOR);

                        Point theMousePoint = new Point(Canvas.GetLeft(MovePoint) + 15, Canvas.GetTop(MovePoint) + 15);
                        USDSCor.USPoint ptNew = new USDSCor.USPoint(theMousePoint.X, theMousePoint.Y);
                        USDSCor.USPoint ptNewSamp = BMdscor.ReDSCMap(ptNew);
                        ptNewSamp.X = (int)(ptNewSamp.X + 0.5f);

                        if (ptNewSamp.X < 0)
                        {
                            ptNewSamp.X = 0;
                        }
                        else if (ptNewSamp.X > lineCount - 1)
                        {
                            ptNewSamp.X = lineCount - 1;
                        }
                        if (ptNewSamp.Y < 10)
                        {
                            ptNewSamp.Y = 10;
                        }
                        else if (ptNewSamp.Y > sampleCount - 10)
                        {
                            ptNewSamp.Y = sampleCount - 10;
                        }

                        int xxx = (int)ptNewSamp.X;
                        double yyy = (int)ptNewSamp.Y;
                        SelectedLine = xxx;
                        USDSCor.USPoint spoint = BMdscor.DscMap(new USDSCor.USPoint(xxx, 0));
                        USDSCor.USPoint epoint = BMdscor.DscMap(new USDSCor.USPoint(xxx, sampleCount));
                        USDSCor.USPoint anchorpoint = BMdscor.DscMap(new USDSCor.USPoint(xxx, yyy));
                        MoveLine.X1 = spoint.X;
                        MoveLine.Y1 = spoint.Y;
                        MoveLine.X2 = epoint.X;
                        MoveLine.Y2 = epoint.Y;

                        if (anchorpoint.Y > CanvasImage.ActualHeight)
                        {
                            anchorpoint.Y = CanvasImage.ActualHeight;
                            double multiplierY = ((MoveLine.Y2 - anchorpoint.Y) / (anchorpoint.Y - MoveLine.Y1));
                            anchorpoint.X = (MoveLine.X2 + MoveLine.X1 * multiplierY) / (1 + multiplierY);
                        }
                        Canvas.SetLeft(MovePoint, anchorpoint.X - 15);
                        Canvas.SetTop(MovePoint, anchorpoint.Y - 15);

                        USDSCor.USPoint ptNewAN = new USDSCor.USPoint(Canvas.GetLeft(MovePoint), Canvas.GetTop(MovePoint));

                        ptNewSamp.X = (int)(ptNewSamp.X + 0.5f);

                        if (ptNewSamp.X < 0)
                        {
                            ptNewSamp.X = 0;
                        }
                        else if (ptNewSamp.X > bmImage.probeCap.imagingParameter.lineCount - 1)
                        {
                            ptNewSamp.X = (float)bmImage.probeCap.imagingParameter.lineCount - 1;
                        }
                        if (ptNewSamp.Y < 10)
                        {
                            ptNewSamp.Y = 10;
                        }
                        else if (ptNewSamp.Y > bmImage.probeCap.imagingParameter.sampleCount - 10)
                        {
                            ptNewSamp.Y = bmImage.probeCap.imagingParameter.sampleCount - 10;
                        }

                        ptNewAN = BMdscor.DscMap(ptNewSamp);
                        int newLine = (int)ptNewSamp.X;
                        if (newLine != bmLine)
                        {
                            bmLine = newLine;

                            USManager mgr = USManager.GetInstance(null);
                            USDriver drv = mgr.GetCurrentDriver();
                            if (drv != null)
                            {
                                drv.SetBMLine(newLine);
                            }
                        }
                        #endregion
                        oldScale = BMdscor.M_dbScalePixel;
                        oldFocuses = ((USBMEnhanceImage)rawImage).FocusList();
                        USScaleView.DrawScaleFocus(CanvasRuler, oldFocuses, BMdscor.M_dbScalePixel, Brushes.White);
                    }
                }
                else
                {
                    //general会进入下面，机扫
                    var usparam = rawImage.probeCap.getUSParam(ProbeMode.MODE_BM, false);
                    MILabel.Content = usparam.MI.ToString("0.0");
                    TISLabel.Content = usparam.TIS.ToString("0.0");

                    DRLabel.Visibility = Visibility.Hidden;
                    ENHLabel.Visibility = Visibility.Hidden;
                    FLabel.Visibility = Visibility.Hidden;

                    USScaleView.DrawScaleFocus(CanvasRuler, null, BMdscor.M_dbScalePixel, Brushes.White);
                }
                //}));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        #region 移动控件
        bool IsMouseDown = false;
        bool MovePointIsMouseDown = false;
        Point mousePoint;
        Point MovePointmousePoint;
        object mouseCtrl = null;

        public void InitHeartbeat()
        {
            #region 线段
            FirstPoint.Source = new BitmapImage(new Uri("/Resources/transparent.png", UriKind.Relative));
            FirstPoint.MouseDown += CanvasLineImage_MouseDown;
            FirstPoint.Cursor = Cursors.Hand;
            FirstPoint.Width = 30;
            FirstPoint.Stretch = Stretch.Fill;
            FirstPoint.Height = CanvasLineImage.ActualHeight;

            SecondPoint.Source = new BitmapImage(new Uri("/Resources/transparent.png", UriKind.Relative));
            SecondPoint.MouseDown += CanvasLineImage_MouseDown;
            SecondPoint.Cursor = Cursors.Hand;
            SecondPoint.Width = 30;
            SecondPoint.Stretch = Stretch.Fill;
            SecondPoint.Height = CanvasLineImage.ActualHeight;

            if (lines.Count == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    lines.Add(new Line());
                }
            }
            #endregion
        }
        private void CanvasLineImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            bool ExistFirstPoint = true;
            if (StateLabel.Content.ToString() == Properties.Resources.StateLive
                || StateLabel.Content.ToString() == Properties.Resources.StateReplay)
            {
                return;
            }
            for (int i = 0; i < 6; i++)
            {
                if (!CanvasLineImage.Children.Contains(lines[i]))
                {
                    CanvasLineImage.Children.Add(lines[i]);
                }
            }
            if (!CanvasLineImage.Children.Contains(FirstPoint))
            {
                FirstPoint.Height = CanvasLineImage.ActualHeight;
                Canvas.SetLeft(FirstPoint, e.GetPosition(this.CanvasLineImage).X - 15);
                Canvas.SetTop(FirstPoint, 0);
                CanvasLineImage.Children.Add(FirstPoint);
                ExistFirstPoint = false;

                lines[0].Stroke = new SolidColorBrush(Color.FromArgb(0xff, 0x1e, 0xb4, 0x46));

                lines[0].StrokeThickness = 2;
                lines[0].X2 = lines[0].X1 = Canvas.GetLeft(FirstPoint) + 15;
                lines[0].Y1 = 0;
                lines[0].Y2 = CanvasLineImage.ActualHeight;
                lines[0].StrokeDashArray = new DoubleCollection() { 2, 3 };


                lines[0].Visibility = Visibility.Visible;
            }
            if (!CanvasLineImage.Children.Contains(SecondPoint)
                && ExistFirstPoint)
            {
                SecondPoint.Height = CanvasLineImage.ActualHeight;
                Canvas.SetLeft(SecondPoint, e.GetPosition(this.CanvasLineImage).X - 15);
                Canvas.SetTop(SecondPoint, 0);
                CanvasLineImage.Children.Add(SecondPoint);

                CanvasLineImage.MouseDown -= CanvasLineImage_MouseDown;
                isHaveMousedown = false;

                double space = (Canvas.GetLeft(SecondPoint) - Canvas.GetLeft(FirstPoint)) / 5;
                for (int i = 0; i < 6; i++)
                {
                    //lines[i].Stroke = Brushes.Yellow;
                    if (i == 0 || i == 5)
                    {
                        lines[i].Stroke = new SolidColorBrush(Color.FromArgb(0xff, 0x1e, 0xb4, 0x46));
                    }
                    else
                    {
                        lines[i].Stroke = new SolidColorBrush(Color.FromArgb(0xff, 0xcc, 0xcc, 0x66));
                    }

                    lines[i].StrokeThickness = 2;
                    lines[i].X1 = Canvas.GetLeft(FirstPoint) + i * space + 15;
                    lines[i].X2 = Canvas.GetLeft(FirstPoint) + i * space + 15;
                    lines[i].Y1 = 0;
                    lines[i].Y2 = CanvasLineImage.ActualHeight;
                    lines[i].StrokeDashArray = new DoubleCollection() { 2, 3 };

                    lines[i].Visibility = Visibility.Visible;
                }
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsMouseDown = true;
                mousePoint = e.GetPosition(this.CanvasLineImage);
                mouseCtrl = sender;
            }
        }
        private void CanvasLineImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMouseDown)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point theMousePoint = e.GetPosition(this.CanvasLineImage);
                    if (theMousePoint.X < 0)
                    {
                        theMousePoint.X = 0;
                    }
                    theMousePoint.X = theMousePoint.X < 0 ? 0 : theMousePoint.X;
                    theMousePoint.X = theMousePoint.X > CanvasLineImage.ActualWidth ? CanvasLineImage.ActualWidth : theMousePoint.X;
                    theMousePoint.Y = theMousePoint.Y < 0 ? 0 : theMousePoint.Y;
                    theMousePoint.Y = theMousePoint.Y > CanvasLineImage.ActualHeight ? CanvasLineImage.ActualHeight : theMousePoint.Y;

                    theMousePoint.X = theMousePoint.X < 0 ? 0 : theMousePoint.X;
                    Canvas.SetLeft((UIElement)mouseCtrl, theMousePoint.X - (15));
                    //Canvas.SetLeft((UIElement)mouseCtrl, theMousePoint.X - (mousePoint.X - Canvas.GetLeft(((UIElement)mouseCtrl))));
                    // Canvas.SetTop((UIElement)mouseCtrl, theMousePoint.Y - (mousePoint.Y - Canvas.GetTop(((UIElement)mouseCtrl))));
                    mousePoint = theMousePoint;

                    if (!CanvasLineImage.Children.Contains(SecondPoint))
                    {
                        return;
                    }
                    double space = (Canvas.GetLeft(SecondPoint) - Canvas.GetLeft(FirstPoint)) / 5;
                    for (int i = 0; i < 6; i++)
                    {
                        //lines[i].Stroke = Brushes.Yellow;
                        if (i == 0 || i == 5)
                        {
                            lines[i].Stroke = new SolidColorBrush(Color.FromArgb(0xff, 0x1e, 0xb4, 0x46));
                        }
                        else
                        {
                            lines[i].Stroke = new SolidColorBrush(Color.FromArgb(0xff, 0xcc, 0xcc, 0x66));
                        }

                        lines[i].StrokeThickness = 2;
                        lines[i].X1 = Canvas.GetLeft(FirstPoint) + i * space + 15;
                        lines[i].X2 = Canvas.GetLeft(FirstPoint) + i * space + 15;
                        lines[i].Y1 = 0;
                        lines[i].Y2 = CanvasLineImage.ActualHeight;
                        lines[i].StrokeDashArray = new DoubleCollection() { 2, 3 };


                        lines[i].Visibility = Visibility.Visible;
                    }
                    //从探头获取时间
                    double sx = Canvas.GetLeft(FirstPoint) + 15;
                    if (sx >= CanvasLineImage.ActualWidth)
                    {
                        sx = CanvasLineImage.ActualWidth;
                    }
                    if (sx <= 0)
                    {
                        sx = 0;
                    }
                    anchorStart.X = sx;
                    anchorStart.Y = Canvas.GetTop(FirstPoint) + 15;

                    double ex = Canvas.GetLeft(SecondPoint) + 15;
                    if (ex >= CanvasLineImage.ActualWidth)
                    {
                        ex = CanvasLineImage.ActualWidth;
                    }
                    if (ex <= 0)
                    {
                        ex = 0;
                    }
                    anchorEnd.X = ex;
                    anchorEnd.Y = Canvas.GetTop(SecondPoint) + 15;
                    LabelHeartbeat.Content = UpdateHBRate();
                }
            }
        }
        private void CanvasLineImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseDown)
            {
                IsMouseDown = false;
            }
        }

        private void MovePoint_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (StateLabel.Content.ToString() != Properties.Resources.StateLive)
            {
                return;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                MovePointIsMouseDown = true;
                MovePointmousePoint = e.GetPosition(this.CanvasImage);
                mouseCtrl = sender;
            }
        }
        private void MovePoint_MouseMove(object sender, MouseEventArgs e)
        {
            if (MovePointIsMouseDown)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point theMousePoint = e.GetPosition(this.CanvasImage);
                    Canvas.SetLeft((UIElement)mouseCtrl, theMousePoint.X - 15);
                    Canvas.SetTop((UIElement)mouseCtrl, theMousePoint.Y - 15);
                    MovePointmousePoint = theMousePoint;

                    BMdscor = USDSCor.GetInstance(BM_DSCOR);

                    USDSCor.USPoint ptNew = new USDSCor.USPoint(theMousePoint.X, theMousePoint.Y);
                    USDSCor.USPoint ptNewSamp = BMdscor.ReDSCMap(ptNew);
                    ptNewSamp.X = (int)(ptNewSamp.X + 0.5f);

                    if (ptNewSamp.X < 0)
                    {
                        ptNewSamp.X = 0;
                    }
                    else if (ptNewSamp.X > lineCount - 1)
                    {
                        ptNewSamp.X = lineCount - 1;
                    }
                    if (ptNewSamp.Y < 10)
                    {
                        ptNewSamp.Y = 10;
                    }
                    else if (ptNewSamp.Y > sampleCount - 10)
                    {
                        ptNewSamp.Y = sampleCount - 10;
                    }

                    int xxx = (int)ptNewSamp.X;
                    double yyy = (int)ptNewSamp.Y;
                    SelectedLine = xxx;
                    USDSCor.USPoint spoint = BMdscor.DscMap(new USDSCor.USPoint(xxx, 0));
                    USDSCor.USPoint epoint = BMdscor.DscMap(new USDSCor.USPoint(xxx, sampleCount));
                    USDSCor.USPoint anchorpoint = BMdscor.DscMap(new USDSCor.USPoint(xxx, yyy));
                    MoveLine.X1 = spoint.X;
                    MoveLine.Y1 = spoint.Y;
                    MoveLine.X2 = epoint.X;
                    MoveLine.Y2 = epoint.Y;

                    if (anchorpoint.Y > CanvasImage.ActualHeight)
                    {
                        anchorpoint.Y = CanvasImage.ActualHeight;
                        double multiplierY = ((MoveLine.Y2 - anchorpoint.Y) / (anchorpoint.Y - MoveLine.Y1));
                        anchorpoint.X = (MoveLine.X2 + MoveLine.X1 * multiplierY) / (1 + multiplierY);
                    }
                    Canvas.SetLeft(MovePoint, anchorpoint.X - 15);
                    Canvas.SetTop(MovePoint, anchorpoint.Y - 15);

                    USDSCor.USPoint ptNewAN = new USDSCor.USPoint(Canvas.GetLeft(MovePoint), Canvas.GetTop(MovePoint));

                    ptNewSamp.X = (int)(ptNewSamp.X + 0.5f);

                    if (ptNewSamp.X < 0)
                    {
                        ptNewSamp.X = 0;
                    }
                    else if (ptNewSamp.X > bmImage.probeCap.imagingParameter.lineCount - 1)
                    {
                        ptNewSamp.X = (float)bmImage.probeCap.imagingParameter.lineCount - 1;
                    }
                    if (ptNewSamp.Y < 10)
                    {
                        ptNewSamp.Y = 10;
                    }
                    else if (ptNewSamp.Y > bmImage.probeCap.imagingParameter.sampleCount - 10)
                    {
                        ptNewSamp.Y = bmImage.probeCap.imagingParameter.sampleCount - 10;
                    }

                    ptNewAN = BMdscor.DscMap(ptNewSamp);
                    int newLine = (int)ptNewSamp.X;
                    if (newLine != bmLine)
                    {
                        bmLine = newLine;

                        USManager mgr = USManager.GetInstance(null);
                        USDriver drv = mgr.GetCurrentDriver();
                        if (drv != null)
                        {
                            drv.SetBMLine(newLine);
                        }
                    }
                }
            }
        }
        private void MovePoint_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MovePointIsMouseDown)
            {
                MovePointIsMouseDown = false;
            }
        }
        private void InitHeartbeatEvent()
        {
            for (int i = 0; i < 6; i++)
            {
                if (CanvasLineImage.Children.Contains(lines[i]))
                {
                    CanvasLineImage.Children.Remove(lines[i]);
                    lines[i].Visibility = Visibility.Hidden;
                }
            }
            if (CanvasLineImage.Children.Contains(FirstPoint))
            {
                CanvasLineImage.Children.Remove(FirstPoint);
            }
            if (CanvasLineImage.Children.Contains(SecondPoint))
            {
                CanvasLineImage.Children.Remove(SecondPoint);
            }
            if (!Freeze)
            {
                CanvasLineImage.MouseDown -= CanvasLineImage_MouseDown;
                isHaveMousedown = false;
            }
            else
            {
                if (!isHaveMousedown)
                {
                    CanvasLineImage.MouseDown += CanvasLineImage_MouseDown;
                    isHaveMousedown = true;
                }
            }
        }
        #endregion

        private void USViewer_Loaded(object sender, RoutedEventArgs e)
        {
            CanvasImage.Width = imageView.ActualWidth;
            CanvasImage.Height = imageView.ActualHeight;

            if (loaded)
            {
                loaded = true;
                return;
            }
            loaded = true;
            MoveLine.X1 = this.imageView.ActualWidth / 2;
            MoveLine.Y1 = 0;
            MoveLine.X2 = this.imageView.ActualWidth / 2;
            MoveLine.Y2 = this.imageView.ActualHeight;

            Canvas.SetLeft(MovePoint, this.imageView.ActualWidth / 2 - 15);
            Canvas.SetTop(MovePoint, this.imageView.ActualHeight / 3);

            

            //int width = (int)(imageView.ActualWidth);
            //int height = (int)(imageView.ActualHeight);
            //USDSCor dscor = USDSCor.GetInstance(BM_DSCOR);
            //if (width <= 0 || height <= 0)
            //{
            //    return;
            //}
            //dscor.SetDestSize(width, height);



            BMdscor = USDSCor.GetInstance(USBMViewer.BM_DSCOR);
            var widBM = imageView.ActualWidth * 7 / 8 - 8;
            var higBM = imageView.ActualHeight;
            BMdscor.SetDestSize((int)widBM, (int)higBM);

            //USScaleView uSScaleView = new USScaleView();
            //uSScaleView.SetFocus(oldFocuses, Brushes.White);
            //uSScaleView.SetScale((float)BMdscor.M_dbScalePixel);
            //uSScaleView.DrawScaleFocus(CanvasRuler);

            USScaleView.DrawScaleFocus(CanvasRuler, null, BMdscor.M_dbScalePixel, Brushes.White);

            if (NeedReloadImage)
            {
                NeedReloadImage = false;
                if (generalImage != null)
                {
                    try
                    {
                        SetRawImage(generalImage);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
            }
        }
        private DateTime? GetTimeSpan(double currentpositon)
        {
            String heartBeat = Properties.Resources.Heartbeat + "--bpm";
            if (bmImage != null)
            {
                int width = (int)CanvasLineImage.ActualWidth;
                int startLine = (int)(currentpositon / (float)width * 100.0f + 0.5f);

                DateTime time = bmImage.rawBM.GetTime(startLine);
                return time;
            }
            return null;
        }
        protected string UpdateHBRate()
        {
            String heartBeat = Properties.Resources.Heartbeat + "--bpm";
            // if (bmImage != null && anchorStart != null && anchorEnd != null)
            if (bmImage != null)
            {
                int width = (int)CanvasLineImage.ActualWidth;
                int startLine = (int)(anchorStart.X / (float)width * 100.0f + 0.5f);
                int endLine = (int)(anchorEnd.X / (float)width * 100.0f + 0.5f);
                if (endLine < startLine)
                {
                    int mid = startLine;
                    startLine = endLine;
                    endLine = mid;
                }

                DateTime start = bmImage.rawBM.GetTime(startLine);
                DateTime end = bmImage.rawBM.GetTime(endLine);
                if (start == null || end == null)
                {
                    heartBeat = Properties.Resources.Heartbeat + "-- bpm";
                }
                else
                {
                    double time = Math.Abs(end.Subtract(start).TotalMinutes);
                    if (Math.Abs(end.Subtract(start).TotalSeconds) > 25.0f)
                    {
                        heartBeat = "overtime";
                    }
                    else
                    {
                        int heartbeatTime = (int)(5 / time);
                        if (heartbeatTime <= 0)
                        {
                            heartbeatTime = 0;
                        }
                        heartBeat = Properties.Resources.Heartbeat + heartbeatTime + "bpm";
                    }
                }
            }
            else
            {
                // beatRateLabel.setText(getResources().getString(R.string.heart_beat) + "- bpm");
            }
            return heartBeat;
        }

        static DateTime startDate;

        private void USViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
    }
}

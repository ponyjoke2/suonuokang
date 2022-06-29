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
using SmartUSKit_CS.Operations;
using SmartUSKit.Enums;

namespace SmartUSKit_CS
{
    /// <summary>
    /// USGeneralView.xaml 的交互逻辑
    /// </summary>
    public partial class USGeneralView : USViewer
    {
        public const string GENERAL_VIEWER_DSCOR = "GeneralViewerDSCor";
        //public const string GENERAL_VIEWER_HALF_DSCOR = "GeneralViewerHalfDSCor";

        //USMarkView markView = new USMarkView();

        public float oldScale = -1;

        public override bool IsSupportBiopsy()
        {
            return true;
        }

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
        /// <summary>
        /// 保存当前点击的图标
        /// </summary>
        Point SelectedPoint = new Point();

        protected static USGeneralView instance = null;
        public static USGeneralView GetInstance()
        {
            if (instance == null)
            {
                instance = new USGeneralView();
            }
            return instance;
        }
        private USGeneralView()
        {
            InitializeComponent();
            var timestr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString()
                + DateTime.Now.ToString("    HH:mm:ss");
            timeLabel.Text = timestr;
            #region 显示信息绑定
            this.UserIdLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("UserId"), Source = Patient.GetInstance() });
            this.UserNameLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("Name"), Source = Patient.GetInstance() });
            this.GenderLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("Gender"), Source = Patient.GetInstance() });
            this.AgeLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("Age"), Source = Patient.GetInstance() });

            this.PatientStackpanel.SetBinding(Label.VisibilityProperty, new Binding() { Path = new PropertyPath("PatientVisible"), Source = Preset.GetInstance() });

            this.InfoStackpanel.SetBinding(Label.VisibilityProperty, new Binding() { Path = new PropertyPath("InfoVisible"), Source = Preset.GetInstance() });
            this.InfoStackpanel.SetBinding(Label.OpacityProperty, new Binding() { Path = new PropertyPath("StackpanelOpacity"), Source = Preset.GetInstance() });
            #endregion

            #region 穿刺线绑定
            this.InPlaneCanvas.SetBinding(Canvas.VisibilityProperty, new Binding() { Path = new PropertyPath("InPlaneVisible"), Source = Biopsy.GetInstance() });
            this.InPlanePositionLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("InPlanePosition"), Source = Biopsy.GetInstance() });
            this.InPlaneAngleLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("InPlaneAngle"), Source = Biopsy.GetInstance() });

            this.InPlaneFivebuttonsGrid.SetBinding(Grid.VisibilityProperty, new Binding() { Path = new PropertyPath("InPlaneFivebuttonsVisible"), Source = Biopsy.GetInstance() });
            this.OutPlanePositionXLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("OutPlaneLeftPosition"), Source = Biopsy.GetInstance() });
            this.OutPlanePositionYLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("OutPlanePosition"), Source = Biopsy.GetInstance() });
            this.OutPlaneRadiusLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("OutPlaneDSCRadius"), Source = Biopsy.GetInstance() });

            this.InPlanePositionLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("InPlanePosition"), Source = Biopsy.GetInstance() });
            this.InPlaneAngleLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("InPlaneAngle"), Source = Biopsy.GetInstance() });
            this.InPlaneLine.SetBinding(Line.X1Property, new Binding() { Path = new PropertyPath("InPlaneX1"), Source = Biopsy.GetInstance() });
            this.InPlaneLine.SetBinding(Line.X2Property, new Binding() { Path = new PropertyPath("InPlaneX2"), Source = Biopsy.GetInstance() });
            this.InPlaneLine.SetBinding(Line.Y1Property, new Binding() { Path = new PropertyPath("InPlaneY1"), Source = Biopsy.GetInstance() });
            this.InPlaneLine.SetBinding(Line.Y2Property, new Binding() { Path = new PropertyPath("InPlaneY2"), Source = Biopsy.GetInstance() });

            this.OutPlaneCanvas.SetBinding(Canvas.VisibilityProperty, new Binding() { Path = new PropertyPath("OutPlaneVisible"), Source = Biopsy.GetInstance() });
            this.OutPlaneSevenbuttonsGrid.SetBinding(Grid.VisibilityProperty, new Binding() { Path = new PropertyPath("OutPlaneSevenbuttonsVisible"), Source = Biopsy.GetInstance() });
            this.OutPlaneUpLine.SetBinding(Line.X1Property, new Binding() { Path = new PropertyPath("OutPlaneUpperX1"), Source = Biopsy.GetInstance() });
            this.OutPlaneUpLine.SetBinding(Line.X2Property, new Binding() { Path = new PropertyPath("OutPlaneUpperX2"), Source = Biopsy.GetInstance() });
            this.OutPlaneUpLine.SetBinding(Line.Y1Property, new Binding() { Path = new PropertyPath("OutPlaneUpperY1"), Source = Biopsy.GetInstance() });
            this.OutPlaneUpLine.SetBinding(Line.Y2Property, new Binding() { Path = new PropertyPath("OutPlaneUpperY2"), Source = Biopsy.GetInstance() });

            this.OutPlaneEllipse.SetBinding(Ellipse.WidthProperty, new Binding() { Path = new PropertyPath("OutPlanePixelRadius"), Source = Biopsy.GetInstance() });
            this.OutPlaneEllipse.SetBinding(Ellipse.HeightProperty, new Binding() { Path = new PropertyPath("OutPlanePixelRadius"), Source = Biopsy.GetInstance() });
            this.OutPlaneEllipse.SetBinding(Canvas.TopProperty, new Binding() { Path = new PropertyPath("Top"), Source = Biopsy.GetInstance() });
            this.OutPlaneEllipse.SetBinding(Canvas.LeftProperty, new Binding() { Path = new PropertyPath("Left"), Source = Biopsy.GetInstance() });

            this.OutPlaneBottomLine.SetBinding(Line.X1Property, new Binding() { Path = new PropertyPath("OutPlaneBottomX1"), Source = Biopsy.GetInstance() });
            this.OutPlaneBottomLine.SetBinding(Line.X2Property, new Binding() { Path = new PropertyPath("OutPlaneBottomX2"), Source = Biopsy.GetInstance() });
            this.OutPlaneBottomLine.SetBinding(Line.Y1Property, new Binding() { Path = new PropertyPath("OutPlaneBottomY1"), Source = Biopsy.GetInstance() });
            this.OutPlaneBottomLine.SetBinding(Line.Y2Property, new Binding() { Path = new PropertyPath("OutPlaneBottomY2"), Source = Biopsy.GetInstance() });
            #endregion

            this.StateLabel.Content = Properties.Resources.StateFreeze;
             
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
            if (freeze == (int)USViewer.STATE_FREEZE)
            {
                StateLabel.Content = Properties.Resources.StateFreeze;
            }
            else if ((freeze == (int)USViewer.STATE_LIVE))
            {
                 
                StateLabel.Content = Properties.Resources.StateLive;
            }
            else
            {
                StateLabel.Content = Properties.Resources.StateReplay;
            }
        }
         
        #endregion
        public override void SetRawImage(USRawImage rawImage)
        {
            generalImage = rawImage;
            if (!Biopsy.GetInstance().HorizontalRevert && Biopsy.GetInstance().VerticalRevert)
            {
                ScaleTransform scaleTransform = new ScaleTransform(1, -1, this.imageView.ActualWidth / 2, this.imageView.ActualHeight / 2);
                this.imageView.RenderTransform = scaleTransform;
                this.GreenPointCanvas.RenderTransform = scaleTransform;
                this.InPlaneCanvas.RenderTransform = scaleTransform;
                this.OutPlaneCanvas.RenderTransform = scaleTransform;

                ScaleTransform rulerScaleTransform = new ScaleTransform(1, -1, this.CanvasRuler.ActualWidth / 2, this.CanvasRuler.ActualHeight / 2);
                CanvasRuler.RenderTransform = rulerScaleTransform;
            }
            if (Biopsy.GetInstance().HorizontalRevert && !Biopsy.GetInstance().VerticalRevert)
            {
                ScaleTransform scaleTransform = new ScaleTransform(-1, 1, this.imageView.ActualWidth / 2, this.imageView.ActualHeight / 2);
                this.imageView.RenderTransform = scaleTransform;
                this.GreenPointCanvas.RenderTransform = scaleTransform;
                this.InPlaneCanvas.RenderTransform = scaleTransform;
                this.OutPlaneCanvas.RenderTransform = scaleTransform;

                ScaleTransform rulerScaleTransform = new ScaleTransform(1, 1, this.CanvasRuler.ActualWidth / 2, this.CanvasRuler.ActualHeight / 2);
                CanvasRuler.RenderTransform = rulerScaleTransform;
            }
            if (Biopsy.GetInstance().HorizontalRevert && Biopsy.GetInstance().VerticalRevert)
            {
                ScaleTransform scaleTransform = new ScaleTransform(-1, -1, this.imageView.ActualWidth / 2, this.imageView.ActualHeight / 2);
                this.imageView.RenderTransform = scaleTransform;
                this.GreenPointCanvas.RenderTransform = scaleTransform;
                this.InPlaneCanvas.RenderTransform = scaleTransform;
                this.OutPlaneCanvas.RenderTransform = scaleTransform;

                ScaleTransform rulerScaleTransform = new ScaleTransform(1, -1, this.CanvasRuler.ActualWidth / 2, this.CanvasRuler.ActualHeight / 2);
                CanvasRuler.RenderTransform = rulerScaleTransform;
            }
            if (!Biopsy.GetInstance().HorizontalRevert && !Biopsy.GetInstance().VerticalRevert)
            {
                ScaleTransform scaleTransform = new ScaleTransform(1, 1, this.imageView.ActualWidth / 2, this.imageView.ActualHeight / 2);
                this.imageView.RenderTransform = scaleTransform;
                this.GreenPointCanvas.RenderTransform = scaleTransform;
                this.InPlaneCanvas.RenderTransform = scaleTransform;
                this.OutPlaneCanvas.RenderTransform = scaleTransform;

                ScaleTransform rulerScaleTransform = new ScaleTransform(1, 1, this.CanvasRuler.ActualWidth / 2, this.CanvasRuler.ActualHeight / 2);
                CanvasRuler.RenderTransform = rulerScaleTransform;
            }

            int width = (int)(imageView.ActualWidth);
            int height = (int)(imageView.ActualHeight);

            USDSCor dscor = USDSCor.GetInstance(GENERAL_VIEWER_DSCOR);
            if (width <= 0 || height <= 0)
            {
                NeedReloadImage = true;
                return;
            }
            dscor.SetDestSize(width, height);
            float newScale = (float)dscor.M_dbScalePixel;

            //if (oldScale != newScale)
            {
                oldScale = newScale;
                //USScaleView uSScaleView = new USScaleView();
                //uSScaleView.SetScale(oldScale);
                //uSScaleView.DrawScaleFocus(CanvasRuler);

                USScaleView.DrawScaleFocus(CanvasRuler, null, dscor.M_dbScalePixel, Brushes.White);
            }
            int dscWidth = dscor.GetDscWidth();
            int dscHeight = dscor.GetDscHeight();

            generalImage = rawImage;
            imageView.Source = dscor.DscImage(rawImage);
            this.GrayImage.Source = dscor.GetGrayBar(rawImage.probeCap);
            try
            {
                USDSCor.USPoint greenPoint = dscor.DscMap(new USDSCor.USPoint(0, 0));
                greenPoint.X -= 35;
                if (greenPoint.X < 40)
                {
                    greenPoint.X = 40;
                }
                greenPoint.Y += 15;
                Canvas.SetLeft(this.GreenPoint, greenPoint.X);
            }
            catch (Exception)
            { }

            var usparam = rawImage.probeCap.getUSParam(ProbeMode.MODE_B, false);
            MILabel.Content = usparam.MI.ToString("0.0");
            TISLabel.Content = usparam.TIS.ToString("0.0");

            timeLabel.Text = rawImage.timeCap.ToString("yyyy-MM-dd HH:mm:ss");
            gainLabel.Content = $"GN:{rawImage.gain}dB";
            int depth = rawImage.probeCap.imagingParameter.namicDepth[rawImage.zoom];
            imagedepth = depth;
            depthLabel.Content = $"D:{depth}mm";

            Biopsy.GetInstance().CalculateBiopsy(rawImage, USGeneralView.GENERAL_VIEWER_DSCOR);
        }
        

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
             

            USDSCor dscor = USDSCor.GetInstance(USGeneralView.GENERAL_VIEWER_DSCOR);
            //USScaleView uSScaleView = new USScaleView();
            //uSScaleView.SetFocus(oldFocuses, Brushes.White);
            //uSScaleView.SetScale((float)dscor.M_dbScalePixel);
            //uSScaleView.DrawScaleFocus(CanvasRuler);

            USScaleView.DrawScaleFocus(CanvasRuler, null, dscor.M_dbScalePixel, Brushes.White);

            int width = (int)(imageView.ActualWidth);
            int height = (int)(imageView.ActualHeight);
            //USDSCor dscor = USDSCor.GetInstance(GENERAL_VIEWER_DSCOR);
            if (width <= 0 || height <= 0)
            {
                return;
            }
            dscor.SetDestSize(width, height);

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

       


       
        private void btnVisible_Click(object sender, MouseButtonEventArgs e)
        {
            Biopsy.GetInstance().InPlaneFivebuttonsVisible = Visibility.Hidden;
            Biopsy.GetInstance().OutPlaneSevenbuttonsVisible = Visibility.Hidden;
        }

        private void AdjustBiopsy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image image = (Image)sender;
            Biopsy.GetInstance().StartAdjustBiopsy(image.Name.ToString());
        }
        private void AdjustBiopsy_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Biopsy.GetInstance().StopAdjustBiopsy();
        }

        private void AdjustBiopsy_TouchUp(object sender, TouchEventArgs e)
        {
            Biopsy.GetInstance().StopAdjustBiopsy();
        }

        private void InPlanebtnUp_MouseLeave(object sender, MouseEventArgs e)
        {
            Biopsy.GetInstance().StopAdjustBiopsy();
        }

        private void OutPlaneRadiusNbtn_MouseLeave(object sender, MouseEventArgs e)
        {
            Biopsy.GetInstance().StopAdjustBiopsy();
        }
        
        
    }
}

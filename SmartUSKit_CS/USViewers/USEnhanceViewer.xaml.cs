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
using System.Runtime.InteropServices;

namespace SmartUSKit_CS
{
    /// <summary>
    /// USGeneralView.xaml 的交互逻辑
    /// </summary>
    public partial class USEnhanceViewer : USViewer
    {
        //USMarkView markView = new USMarkView();
        USEnhanceImage enhanceImage;
        USEnhanceProbe enhanceProbe;


        
        public override bool IsSupportBiopsy()
        {
            return true;
        }

        double LastDscor_M_dbScalePixel=-1;

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
        #endregion
        /// <summary>
        /// 滑动起始位置
        /// </summary>
        Point slidestartPoint = new Point();
        bool isSliding = false;
        /// <summary>
        /// 保存当前点击的图标
        /// </summary>
        Point SelectedPoint = new Point();
        protected static USEnhanceViewer instance = null;
        public static USEnhanceViewer GetInstance()
        {
            if (instance == null)
            {
                instance = new USEnhanceViewer();
            }
            return instance;
        }
        private USEnhanceViewer()
        {
            InitializeComponent();

            Biopsy.GetInstance().UpdateViewer+=UpdateBiopsy;

            #region 显示信息绑定
            this.UserIdLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("UserId"), Source = Patient.GetInstance() });
            this.UserNameLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("Name"), Source = Patient.GetInstance() });
            this.GenderLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("Gender"), Source = Patient.GetInstance() });
            this.AgeLabel.SetBinding(Label.ContentProperty, new Binding() { Path = new PropertyPath("Age"), Source = Patient.GetInstance() });

            this.PatientStackpanel.SetBinding(Label.VisibilityProperty, new Binding() { Path = new PropertyPath("PatientVisible"), Source = Preset.GetInstance() });

            this.InfoStackpanel.SetBinding(Label.VisibilityProperty, new Binding() { Path = new PropertyPath("InfoVisible"), Source = Preset.GetInstance() });
            this.InfoStackpanel.SetBinding(Label.OpacityProperty, new Binding() { Path = new PropertyPath("StackpanelOpacity"), Source = Preset.GetInstance() });
            #endregion
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
            this.StateLabel.Content = Properties.Resources.StateLive;
             
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

            USDSCor dscor = USDSCor.GetInstance(USGeneralView.GENERAL_VIEWER_DSCOR);
            int width = (int)(imageView.ActualWidth + 0.5);
            int height = (int)(imageView.ActualHeight + 0.5);
            if ((width == 0 || height == 0)
                && (dscor.GetDestHeight() == 0 || dscor.GetDestWidth() == 0))
            {
                NeedReloadImage = true;
                return;
            }
            dscor.SetDestSize(width, height);

            generalImage = rawImage;
            this.imageView.Source = dscor.DscImage(rawImage);
            this.GrayImage.Source = dscor.GetGrayBar(rawImage.probeCap);

            if (LastDscor_M_dbScalePixel != dscor.M_dbScalePixel)
            {
                LastDscor_M_dbScalePixel = dscor.M_dbScalePixel;
                USDSCor.USPoint greenPoint = dscor.DscMap(new USDSCor.USPoint(0, 0));
                greenPoint.X -= 35;
                if (greenPoint.X < 40)
                {
                    greenPoint.X = 40;
                }
                greenPoint.Y += 15;
                Canvas.SetLeft(this.GreenPoint, greenPoint.X);
            }

            enhanceImage = (USEnhanceImage)rawImage;
            enhanceProbe = (USEnhanceProbe)rawImage.probeCap;

            UpdateImageInfo(rawImage);

            Biopsy.GetInstance().CalculateBiopsy(rawImage, USGeneralView.GENERAL_VIEWER_DSCOR);
            USScaleView.DrawScaleFocus(CanvasRuler, ((USEnhanceImage)rawImage).FocusList(), dscor.M_dbScalePixel, Brushes.White);

            
        }

        void UpdateBiopsy()
        {
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
            else if (Biopsy.GetInstance().HorizontalRevert && !Biopsy.GetInstance().VerticalRevert)
            {
                ScaleTransform scaleTransform = new ScaleTransform(-1, 1, this.imageView.ActualWidth / 2, this.imageView.ActualHeight / 2);
                this.imageView.RenderTransform = scaleTransform;
                this.GreenPointCanvas.RenderTransform = scaleTransform;
                this.InPlaneCanvas.RenderTransform = scaleTransform;
                this.OutPlaneCanvas.RenderTransform = scaleTransform;

                ScaleTransform rulerScaleTransform = new ScaleTransform(1, 1, this.CanvasRuler.ActualWidth / 2, this.CanvasRuler.ActualHeight / 2);
                CanvasRuler.RenderTransform = rulerScaleTransform;
            }
            else if (Biopsy.GetInstance().HorizontalRevert && Biopsy.GetInstance().VerticalRevert)
            {
                ScaleTransform scaleTransform = new ScaleTransform(-1, -1, this.imageView.ActualWidth / 2, this.imageView.ActualHeight / 2);
                this.imageView.RenderTransform = scaleTransform;
                this.GreenPointCanvas.RenderTransform = scaleTransform;
                this.InPlaneCanvas.RenderTransform = scaleTransform;
                this.OutPlaneCanvas.RenderTransform = scaleTransform;

                ScaleTransform rulerScaleTransform = new ScaleTransform(1, -1, this.CanvasRuler.ActualWidth / 2, this.CanvasRuler.ActualHeight / 2);
                CanvasRuler.RenderTransform = rulerScaleTransform;
            }
            else if (!Biopsy.GetInstance().HorizontalRevert && !Biopsy.GetInstance().VerticalRevert)
            {
                ScaleTransform scaleTransform = new ScaleTransform(1, 1, this.imageView.ActualWidth / 2, this.imageView.ActualHeight / 2);
                this.imageView.RenderTransform = scaleTransform;
                this.GreenPointCanvas.RenderTransform = scaleTransform;
                this.InPlaneCanvas.RenderTransform = scaleTransform;
                this.OutPlaneCanvas.RenderTransform = scaleTransform;

                ScaleTransform rulerScaleTransform = new ScaleTransform(1, 1, this.CanvasRuler.ActualWidth / 2, this.CanvasRuler.ActualHeight / 2);
                CanvasRuler.RenderTransform = rulerScaleTransform;
            }
        }


        int oldGain = -1;
        int oldDepth = -1;
        int oldEnhanceLevel = -1;
        int oldDynamicRange = -1;
        float oldMI = -1;
        float oldTIS = -1;
        bool? oldHarmonic = null;
        void UpdateImageInfo(USRawImage rawImage)
        {
            timeLabel.Text = rawImage.timeCap.ToString("yyyy-MM-dd HH:mm:ss");
            if (oldGain != rawImage.gain)
            {
                oldGain = rawImage.gain;
                gainLabel.Content = $"GN:{oldGain}dB";
            }

            int depth = rawImage.probeCap.imagingParameter.namicDepth[rawImage.zoom];
            USViewer.imagedepth = depth;

            if (oldDepth != depth)
            {
                oldDepth = depth;
                depthLabel.Content = $"D:{oldDepth}mm";
            }

            var usparam = rawImage.probeCap.getUSParam(ProbeMode.MODE_B, enhanceImage.harmonic);
            if (oldMI!= usparam.MI)
            {
                oldMI = usparam.MI;
                MILabel.Content = oldMI.ToString("0.0");
            }
            if (oldTIS!= usparam.TIS)
            {
                oldTIS = usparam.TIS;
                TISLabel.Content = oldTIS.ToString("0.0");
            }
            
            if (oldEnhanceLevel != enhanceImage.enhanceLevel)
            {
                oldEnhanceLevel = enhanceImage.enhanceLevel;
                ENHLabel.Content = "ENH: " + oldEnhanceLevel;
            }

            if (oldDynamicRange != enhanceImage.dynamicRange)
            {
                oldDynamicRange = enhanceImage.dynamicRange;
                DRLabel.Content = "DR: " + oldDynamicRange;
            }
            //这个地方不能判断，因为像ux1c这种探头比较特殊，因为不同的头的谐波数不一样
            //if (oldHarmonic!= enhanceImage.harmonic)
            {
                oldHarmonic = enhanceImage.harmonic;
                if (enhanceImage.harmonic)
                {
                    FLabel.Content = "F: H" + enhanceProbe.enhanceParameter.harmonicFrequency.ToString("0.0") + " MHz";
                }
                else
                {
                    FLabel.Content = "F: " + enhanceProbe.enhanceParameter.frequency.ToString("0.0") + " MHz";
                }
            }
            
        }

        
       
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
           
            USDSCor dscor = USDSCor.GetInstance(USGeneralView.GENERAL_VIEWER_DSCOR);
            
            USScaleView.DrawScaleFocus(CanvasRuler, null, dscor.M_dbScalePixel, Brushes.White);

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
       
        #region inplane各方向按键
        /// <summary>
        /// inplane上按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InPlanebtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Biopsy.GetInstance().VerticalRevert)
            {
                int angle = Biopsy.GetInstance().InPlaneAngle += 2;
                if (angle > 0)
                {
                    angle = -80;
                }
                Biopsy.GetInstance().InPlaneAngle = angle;
            }
            else
            {
                int angle = Biopsy.GetInstance().InPlaneAngle -= 2;
                if (angle <= -80)
                {
                    angle = 0;
                }
                Biopsy.GetInstance().InPlaneAngle = angle;
            }
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutInt("BIOPSY_IN_PLANE_ANGLE", Biopsy.GetInstance().InPlaneAngle);

            //USManager mgr = USManager.GetInstance(null);
            //USDriver drv = mgr.GetCurrentDriver();
            //USProbe prb = mgr.GetCurrentProbe();
            //if (prb.IsEnhanceProbe() && prb.BiopsyEnhancable())
            //{
            //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
            //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
            //}
        }
        /// <summary>
        /// inplane下按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InPlanebtnDown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Biopsy.GetInstance().VerticalRevert)
            {
                int angle = Biopsy.GetInstance().InPlaneAngle -= 2;
                if (angle <= -80)
                {
                    angle = 0;
                }
                Biopsy.GetInstance().InPlaneAngle = angle;
            }
            else
            {
                int angle = Biopsy.GetInstance().InPlaneAngle += 2;
                if (angle > 0)
                {
                    angle = -80;
                }
                Biopsy.GetInstance().InPlaneAngle = angle;
            }
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutInt("BIOPSY_IN_PLANE_ANGLE", Biopsy.GetInstance().InPlaneAngle);

            //USManager mgr = USManager.getInstance(null);
            //USDriver drv = mgr.getCurrentDriver();
            //USProbe prb = mgr.getCurrentProbe();
            //if (prb.isEnhanceProbe() && prb.biopsyEnhancable())
            //{
            //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
            //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
            //}
        }

        /// <summary>
        /// inplane左按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InPlanebtnLeft_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isBiopsyR)
            {
                if (Biopsy.GetInstance().InPlanePosition < Biopsy.GetInstance().InPlaneMaxposition)
                {
                    Biopsy.GetInstance().InPlanePosition += 0.5f;
                }
            }
            else
            {
                if (Biopsy.GetInstance().InPlanePosition > -60.0f)
                {
                    Biopsy.GetInstance().InPlanePosition -= 0.5f;
                }
            }
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutFloat("BIOPSY_IN_PLANE_POSITION", Biopsy.GetInstance().InPlanePosition);

            //USManager mgr = USManager.getInstance(null);
            //USDriver drv = mgr.getCurrentDriver();
            //USProbe prb = mgr.getCurrentProbe();
            //if (prb.isEnhanceProbe() && prb.biopsyEnhancable())
            //{
            //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
            //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
            //}
        }
        /// <summary>
        /// inplane右按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InPlanebtnRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isBiopsyR)
            {
                if (Biopsy.GetInstance().InPlanePosition > -60.0f)
                {
                    Biopsy.GetInstance().InPlanePosition -= 0.5f;
                }
            }
            else
            {
                if (Biopsy.GetInstance().InPlanePosition < Biopsy.GetInstance().InPlaneMaxposition)
                {
                    Biopsy.GetInstance().InPlanePosition += 0.5f;
                }
            }
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutFloat("BIOPSY_IN_PLANE_POSITION", Biopsy.GetInstance().InPlanePosition);

            //USManager mgr = USManager.getInstance(null);
            //USDriver drv = mgr.getCurrentDriver();
            //USProbe prb = mgr.getCurrentProbe();
            //if (prb.isEnhanceProbe() && prb.biopsyEnhancable())
            //{
            //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
            //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
            //}
        }
        #endregion

        #region outplane各方向按键
        /// <summary>
        /// outplane半径减
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutPlaneAnglePbtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            float R = Biopsy.GetInstance().OutPlaneDSCRadius - 0.5f;
            if (R < 1.0f)
            {
                R = 1.0f;
            }
            Biopsy.GetInstance().OutPlaneDSCRadius = R;
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutFloat("BIOPSY_OUT_PLANE_RADIUS", Biopsy.GetInstance().OutPlaneDSCRadius);
        }
        /// <summary>
        /// outplane半径加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutPlaneAngleNbtn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            float R = Biopsy.GetInstance().OutPlaneDSCRadius + 0.5f;
            if (R > 10.0f)
            {
                R = 10.0f;
            }
            Biopsy.GetInstance().OutPlaneDSCRadius = R;

            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutFloat("BIOPSY_OUT_PLANE_RADIUS", Biopsy.GetInstance().OutPlaneDSCRadius);
        }
        /// <summary>
        /// outplane上按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutPlanebtnUp_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isRevert)
            {
                float position = Biopsy.GetInstance().OutPlanePosition + 0.5f;
                if (position > imagedepth)
                {
                    position = imagedepth;
                }
                Biopsy.GetInstance().OutPlanePosition = position;
            }
            else
            {
                float position = Biopsy.GetInstance().OutPlanePosition - 0.5f;
                if (position < 0.0f)
                {
                    position = 0;
                }
                Biopsy.GetInstance().OutPlanePosition = position;
            }

            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutFloat("BIOPSY_OUT_PLANE_POSITION", Biopsy.GetInstance().OutPlanePosition);
        }
        /// <summary>
        /// outplane下按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutPlanebtnDown_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isRevert)
            {
                float position = Biopsy.GetInstance().OutPlanePosition - 0.5f;
                if (position < 0.0f)
                {
                    position = 0;
                }
                Biopsy.GetInstance().OutPlanePosition = position;
            }
            else
            {
                float position = Biopsy.GetInstance().OutPlanePosition + 0.5f;
                if (position > imagedepth)
                {
                    position = imagedepth;
                }
                Biopsy.GetInstance().OutPlanePosition = position;
            }
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutFloat("BIOPSY_OUT_PLANE_POSITION", Biopsy.GetInstance().OutPlanePosition);
        }

        /// <summary>
        /// outplane左按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutPlanebtnLeft_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isBiopsyR)
            {
                if (Biopsy.GetInstance().OutPlaneLeftPosition < 10.0f)
                {
                    Biopsy.GetInstance().OutPlaneLeftPosition += 0.5f;
                }
            }
            else
            {
                if (Biopsy.GetInstance().OutPlaneLeftPosition > -10.0f)
                {
                    Biopsy.GetInstance().OutPlaneLeftPosition -= 0.5f;
                }
            }
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutFloat("BIOPSY_OUT_PLANE_LEFT_POSITION", Biopsy.GetInstance().OutPlaneLeftPosition);

        }
        /// <summary>
        /// outplane右按键
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutPlanebtnRight_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isBiopsyR)
            {
                if (Biopsy.GetInstance().OutPlaneLeftPosition > -10.0f)
                {
                    Biopsy.GetInstance().OutPlaneLeftPosition -= 0.5f;
                }
            }
            else
            {
                if (Biopsy.GetInstance().OutPlaneLeftPosition < 10.0f)
                {
                    Biopsy.GetInstance().OutPlaneLeftPosition += 0.5f;
                }
            }
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutFloat("BIOPSY_OUT_PLANE_LEFT_POSITION", Biopsy.GetInstance().OutPlaneLeftPosition);
        }

        #endregion

        private void AdjustBiopsy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Image image = (Image)sender;
            Biopsy.GetInstance().StartAdjustBiopsy(image.Name.ToString());
        }
        private void AdjustBiopsy_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Biopsy.GetInstance().StopAdjustBiopsy();
        }

        private void AdjustBiopsy_MouseLeave(object sender, MouseEventArgs e)
        {
            Biopsy.GetInstance().StopAdjustBiopsy();
        }
        private void AdjustBiopsy_TouchUp(object sender, TouchEventArgs e)
        {
            Biopsy.GetInstance().StopAdjustBiopsy();
        }

       
    }
}

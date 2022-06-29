using SmartUSKit.SmartUSKit;
using SmartUSKit_CS.USViewers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SmartUSKit_CS.Model
{
    class Biopsy : INotifyPropertyChanged
    {
        public const int NONE_BIOPSY = 0;
        public const int IN_PLANE_BIOPSY = 1;
        public const int OUT_PLANE_BIOPSY = 2;

        private static Biopsy instance = null;

        private bool isadjusting = false;
        public static Biopsy GetInstance()
        {
            if (instance == null)
            {
                instance = new Biopsy();
            }
            return instance;
        }
        private Biopsy()
        {

        }
        /// <summary>
        /// 垂直或水平翻转变化时，触发
        /// </summary>
        public event Action UpdateViewer;

        public event PropertyChangedEventHandler PropertyChanged;

        private Visibility inPlaneVisible = Visibility.Hidden;
        public Visibility InPlaneVisible
        {
            get
            {
                return this.inPlaneVisible;
            }

            set
            {
                this.inPlaneVisible = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InPlaneVisible"));
                }
            }
        }
        private Visibility inPlaneFivebuttonsVisible = Visibility.Hidden;
        public Visibility InPlaneFivebuttonsVisible
        {
            get
            {
                return this.inPlaneFivebuttonsVisible;
            }

            set
            {
                this.inPlaneFivebuttonsVisible = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InPlaneFivebuttonsVisible"));
                }
            }
        }

        private float inPlaneMaxposition = 0;
        public float InPlaneMaxposition
        {
            get
            {
                return this.inPlaneMaxposition;
            }

            set
            {
                this.inPlaneMaxposition = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InPlaneMaxposition"));
                }
            }
        }

        private Visibility outPlaneVisible = Visibility.Hidden;
        public Visibility OutPlaneVisible
        {
            get
            {
                return this.outPlaneVisible;
            }

            set
            {
                this.outPlaneVisible = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneVisible"));
                }
            }
        }
        private Visibility outPlaneSevenbuttonsVisible = Visibility.Hidden;
        public Visibility OutPlaneSevenbuttonsVisible
        {
            get
            {
                return this.outPlaneSevenbuttonsVisible;
            }

            set
            {
                this.outPlaneSevenbuttonsVisible = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneSevenbuttonsVisible"));
                }
            }
        }
        private bool horizontalRevert = false;
        public bool HorizontalRevert
        {
            get
            {
                return this.horizontalRevert;
            }

            set
            {
                this.horizontalRevert = value;
                UpdateViewer();
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("HorizontalRevert"));
                }
            }
        }

        private bool verticalRevert = false;
        public bool VerticalRevert
        {
            get
            {
                return this.verticalRevert;
            }

            set
            {
                this.verticalRevert = value;
                UpdateViewer();
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("VerticalRevert"));
                }
            }
        }
        #region InPlaneLine
        private float inPlanePosition = 0;
        public float InPlanePosition
        {
            get
            {
                return this.inPlanePosition;
            }

            set
            {
                this.inPlanePosition = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InPlanePosition"));
                }
            }
        }
        private int inPlaneAngle = 0;
        public int InPlaneAngle
        {
            get
            {
                return this.inPlaneAngle;
            }

            set
            {
                this.inPlaneAngle = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InPlaneAngle"));
                }
            }
        }
        private float inPlaneX1 = 0;
        public float InPlaneX1
        {
            get
            {
                return this.inPlaneX1;
            }

            set
            {
                this.inPlaneX1 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InPlaneX1"));
                }
            }
        }
        private float inPlaneY1 = 0;
        public float InPlaneY1
        {
            get
            {
                return this.inPlaneY1;
            }

            set
            {
                this.inPlaneY1 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InPlaneY1"));
                }
            }
        }
        private float inPlaneX2 = 0;
        public float InPlaneX2
        {
            get
            {
                return this.inPlaneX2;
            }

            set
            {
                this.inPlaneX2 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InPlaneX2"));
                }
            }
        }
        private float inPlaneY2 = 0;
        public float InPlaneY2
        {
            get
            {
                return this.inPlaneY2;
            }

            set
            {
                this.inPlaneY2 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("InPlaneY2"));
                }
            }
        }
        #endregion
        //圆上方的线
        private float outPlaneUpperX1 = 0;
        public float OutPlaneUpperX1
        {
            get
            {
                return this.outPlaneUpperX1;
            }

            set
            {
                this.outPlaneUpperX1 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneUpperX1"));
                }
            }
        }
        private float outPlaneUpperY1 = 0;
        public float OutPlaneUpperY1
        {
            get
            {
                return this.outPlaneUpperY1;
            }

            set
            {
                this.outPlaneUpperY1 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneUpperY1"));
                }
            }
        }
        private float outPlaneUpperX2 = 0;
        public float OutPlaneUpperX2
        {
            get
            {
                return this.outPlaneUpperX2;
            }

            set
            {
                this.outPlaneUpperX2 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneUpperX2"));
                }
            }
        }
        private float outPlaneUpperY2 = 0;
        public float OutPlaneUpperY2
        {
            get
            {
                return this.outPlaneUpperY2;
            }

            set
            {
                this.outPlaneUpperY2 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneUpperY2"));
                }
            }
        }
        //圆
        private float left = 0;
        public float Left
        {
            get
            {
                return this.left;
            }

            set
            {
                this.left = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Left"));
                }
            }
        }

        private float top = 0;
        public float Top
        {
            get
            {
                return this.top;
            }

            set
            {
                this.top = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Top"));
                }
            }
        }
        //圆下方的线
        private float outPlaneBottomX1 = 0;
        public float OutPlaneBottomX1
        {
            get
            {
                return this.outPlaneBottomX1;
            }

            set
            {
                this.outPlaneBottomX1 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneBottomX1"));
                }
            }
        }
        private float outPlaneBottomY1 = 0;
        public float OutPlaneBottomY1
        {
            get
            {
                return this.outPlaneBottomY1;
            }

            set
            {
                this.outPlaneBottomY1 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneBottomY1"));
                }
            }
        }
        private float outPlaneBottomX2 = 0;
        public float OutPlaneBottomX2
        {
            get
            {
                return this.outPlaneBottomX2;
            }

            set
            {
                this.outPlaneBottomX2 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneBottomX2"));
                }
            }
        }
        private float outPlaneBottomY2 = 0;
        public float OutPlaneBottomY2
        {
            get
            {
                return this.outPlaneBottomY2;
            }

            set
            {
                this.outPlaneBottomY2 = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneBottomY2"));
                }
            }
        }

        private float outPlanePosition = 0;
        public float OutPlanePosition
        {
            get
            {
                return this.outPlanePosition;
            }

            set
            {
                this.outPlanePosition = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlanePosition"));
                }
            }
        }

        private float outPlaneLeftPosition = 0;
        public float OutPlaneLeftPosition
        {
            get
            {
                return this.outPlaneLeftPosition;
            }

            set
            {
                this.outPlaneLeftPosition = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneLeftPosition"));
                }
            }
        }

        private float outPlaneDSCRadius = 0;
        public float OutPlaneDSCRadius
        {
            get
            {
                return this.outPlaneDSCRadius;
            }

            set
            {
                this.outPlaneDSCRadius = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlaneDSCRadius"));
                }
            }
        }

        private float outPlanePixelRadius = 0;
        public float OutPlanePixelRadius
        {
            get
            {
                return this.outPlanePixelRadius;
            }

            set
            {
                this.outPlanePixelRadius = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("OutPlanePixelRadius"));
                }
            }
        }

        public static void LoadBiopsy()
        {
            USPreferences pref = USPreferences.GetInstance();
            int biopsy = pref.GetInt("BIOPSY_MODE", NONE_BIOPSY);
            switch (biopsy)
            {
                case NONE_BIOPSY:
                    Biopsy.GetInstance().InPlaneVisible = Visibility.Hidden;
                    Biopsy.GetInstance().OutPlaneVisible = Visibility.Hidden;
                    break;
                case IN_PLANE_BIOPSY:
                    Biopsy.GetInstance().InPlaneVisible = Visibility.Visible;
                    Biopsy.GetInstance().OutPlaneVisible = Visibility.Hidden;
                    break;
                case OUT_PLANE_BIOPSY:
                    Biopsy.GetInstance().InPlaneVisible = Visibility.Hidden;
                    Biopsy.GetInstance().OutPlaneVisible = Visibility.Visible;
                    break;
                default:
                    break;
            }
            Biopsy.GetInstance().InPlaneAngle = pref.GetInt("BIOPSY_IN_PLANE_ANGLE", -30);
            Biopsy.GetInstance().inPlanePosition = pref.GetFloat("BIOPSY_IN_PLANE_POSITION", -30.0F);
            Biopsy.GetInstance().OutPlanePosition = pref.GetFloat("BIOPSY_OUT_PLANE_POSITION", 30.0f);
            Biopsy.GetInstance().OutPlaneDSCRadius = pref.GetFloat("BIOPSY_OUT_PLANE_RADIUS", 5.0f);
            Biopsy.GetInstance().OutPlaneLeftPosition = pref.GetFloat("BIOPSY_OUT_PLANE_LEFT_POSITION", 0.0f);
        }

        DispatcherTimer timerAdjustBiopsy = new DispatcherTimer() { };
        string BTNNAME = "";
        public void StartAdjustBiopsy(string btnName)
        {
            if (isadjusting)
            {
                return;
            }
            isadjusting = true;
            timerAdjustBiopsy = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            timerAdjustBiopsy.Tick += new EventHandler(AdjustBiopsy);
            timerAdjustBiopsy.Start();

            USPreferences prefs = USPreferences.GetInstance();
            switch (btnName)
            {
                case "InPlanebtnUp":
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

                    prefs.PutInt("BIOPSY_IN_PLANE_ANGLE", Biopsy.GetInstance().InPlaneAngle);
                    //USManager mgr = USManager.GetInstance(null);
                    //USDriver drv = mgr.GetCurrentDriver();
                    //USProbe prb = mgr.GetCurrentProbe();
                    //if (prb.IsEnhanceProbe() && prb.BiopsyEnhancable())
                    //{
                    //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
                    //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
                    //}
                    break;
                case "InPlanebtnDown":
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
                    prefs.PutInt("BIOPSY_IN_PLANE_ANGLE", Biopsy.GetInstance().InPlaneAngle);

                    //USManager mgr = USManager.getInstance(null);
                    //USDriver drv = mgr.getCurrentDriver();
                    //USProbe prb = mgr.getCurrentProbe();
                    //if (prb.isEnhanceProbe() && prb.biopsyEnhancable())
                    //{
                    //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
                    //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
                    //}
                    break;
                case "InPlanebtnLeft":
                    if (Biopsy.GetInstance().HorizontalRevert)
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
                    prefs.PutFloat("BIOPSY_IN_PLANE_POSITION", Biopsy.GetInstance().InPlanePosition);

                    //USManager mgr = USManager.getInstance(null);
                    //USDriver drv = mgr.getCurrentDriver();
                    //USProbe prb = mgr.getCurrentProbe();
                    //if (prb.isEnhanceProbe() && prb.biopsyEnhancable())
                    //{
                    //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
                    //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
                    //}
                    break;
                case "InPlanebtnRight":
                    if (Biopsy.GetInstance().HorizontalRevert)
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
                    prefs.PutFloat("BIOPSY_IN_PLANE_POSITION", Biopsy.GetInstance().InPlanePosition);

                    //USManager mgr = USManager.getInstance(null);
                    //USDriver drv = mgr.getCurrentDriver();
                    //USProbe prb = mgr.getCurrentProbe();
                    //if (prb.isEnhanceProbe() && prb.biopsyEnhancable())
                    //{
                    //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
                    //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
                    //}
                    break;

                //OutPlane
                case "OutPlaneRadiusPbtn":
                    {
                        float R = Biopsy.GetInstance().OutPlaneDSCRadius + 0.5f;
                        if (R > 10.0f)
                        {
                            R = 10.0f;
                        }
                        Biopsy.GetInstance().OutPlaneDSCRadius = R;

                        prefs.PutFloat("BIOPSY_OUT_PLANE_RADIUS", Biopsy.GetInstance().OutPlaneDSCRadius);
                    }
                    break;
                case "OutPlaneRadiusNbtn":
                    {
                        float R = Biopsy.GetInstance().OutPlaneDSCRadius - 0.5f;
                        if (R < 1.0f)
                        {
                            R = 1.0f;
                        }
                        Biopsy.GetInstance().OutPlaneDSCRadius = R;
                        prefs.PutFloat("BIOPSY_OUT_PLANE_RADIUS", Biopsy.GetInstance().OutPlaneDSCRadius);
                    }
                    break;
                case "OutPlanebtnUp":
                    if (Biopsy.GetInstance().VerticalRevert)
                    {
                        float position = Biopsy.GetInstance().OutPlanePosition + 0.5f;
                        if (position > USViewer.imagedepth)
                        {
                            position = USViewer.imagedepth;
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

                    prefs.PutFloat("BIOPSY_OUT_PLANE_POSITION", Biopsy.GetInstance().OutPlanePosition);
                    break;
                case "OutPlanebtnDown":
                    if (Biopsy.GetInstance().VerticalRevert)
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
                        if (position > USViewer.imagedepth)
                        {
                            position = USViewer.imagedepth;
                        }
                        Biopsy.GetInstance().OutPlanePosition = position;
                    }
                    prefs.PutFloat("BIOPSY_OUT_PLANE_POSITION", Biopsy.GetInstance().OutPlanePosition);
                    break;
                case "OutPlanebtnLeft":
                    if (Biopsy.GetInstance().HorizontalRevert)
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
                    prefs.PutFloat("BIOPSY_OUT_PLANE_LEFT_POSITION", Biopsy.GetInstance().OutPlaneLeftPosition);
                    break;
                case "OutPlanebtnRight":
                    if (Biopsy.GetInstance().HorizontalRevert)
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
                    prefs.PutFloat("BIOPSY_OUT_PLANE_LEFT_POSITION", Biopsy.GetInstance().OutPlaneLeftPosition);
                    break;

                default:
                    break;
            }
            BTNNAME = btnName;
        }

        private void AdjustBiopsy(object sender, EventArgs e)
        {
            timerAdjustBiopsy.Interval = TimeSpan.FromMilliseconds(1);
            USPreferences prefs = USPreferences.GetInstance();
            switch (BTNNAME)
            {
                case "InPlanebtnUp":
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

                    prefs.PutInt("BIOPSY_IN_PLANE_ANGLE", Biopsy.GetInstance().InPlaneAngle);
                    //USManager mgr = USManager.GetInstance(null);
                    //USDriver drv = mgr.GetCurrentDriver();
                    //USProbe prb = mgr.GetCurrentProbe();
                    //if (prb.IsEnhanceProbe() && prb.BiopsyEnhancable())
                    //{
                    //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
                    //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
                    //}
                    break;
                case "InPlanebtnDown":
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
                    prefs.PutInt("BIOPSY_IN_PLANE_ANGLE", Biopsy.GetInstance().InPlaneAngle);

                    //USManager mgr = USManager.getInstance(null);
                    //USDriver drv = mgr.getCurrentDriver();
                    //USProbe prb = mgr.getCurrentProbe();
                    //if (prb.isEnhanceProbe() && prb.biopsyEnhancable())
                    //{
                    //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
                    //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
                    //}
                    break;
                case "InPlanebtnLeft":
                    if (Biopsy.GetInstance().HorizontalRevert)
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
                    prefs.PutFloat("BIOPSY_IN_PLANE_POSITION", Biopsy.GetInstance().InPlanePosition);

                    //USManager mgr = USManager.getInstance(null);
                    //USDriver drv = mgr.getCurrentDriver();
                    //USProbe prb = mgr.getCurrentProbe();
                    //if (prb.isEnhanceProbe() && prb.biopsyEnhancable())
                    //{
                    //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
                    //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
                    //}
                    break;
                case "InPlanebtnRight":
                    if (Biopsy.GetInstance().HorizontalRevert)
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
                    prefs.PutFloat("BIOPSY_IN_PLANE_POSITION", Biopsy.GetInstance().InPlanePosition);

                    //USManager mgr = USManager.getInstance(null);
                    //USDriver drv = mgr.getCurrentDriver();
                    //USProbe prb = mgr.getCurrentProbe();
                    //if (prb.isEnhanceProbe() && prb.biopsyEnhancable())
                    //{
                    //    USEnhanceDriver enhDrv = (USEnhanceDriver)drv;
                    //    enhDrv.setBiopsyEnhance(isBiopsyEnhance, inPlane.position, inPlane.angle);
                    //}
                    break;

                //OutPlane
                case "OutPlaneRadiusPbtn":
                    {
                        float R = Biopsy.GetInstance().OutPlaneDSCRadius + 0.5f;
                        if (R > 10.0f)
                        {
                            R = 10.0f;
                        }
                        Biopsy.GetInstance().OutPlaneDSCRadius = R;

                        prefs.PutFloat("BIOPSY_OUT_PLANE_RADIUS", Biopsy.GetInstance().OutPlaneDSCRadius);
                    }
                    break;
                case "OutPlaneRadiusNbtn":
                    {
                        float R = Biopsy.GetInstance().OutPlaneDSCRadius - 0.5f;
                        if (R < 1.0f)
                        {
                            R = 1.0f;
                        }
                        Biopsy.GetInstance().OutPlaneDSCRadius = R;
                        prefs.PutFloat("BIOPSY_OUT_PLANE_RADIUS", Biopsy.GetInstance().OutPlaneDSCRadius);
                    }
                    break;
                case "OutPlanebtnUp":
                    if (Biopsy.GetInstance().VerticalRevert)
                    {
                        float position = Biopsy.GetInstance().OutPlanePosition + 0.5f;
                        if (position > USViewer.imagedepth)
                        {
                            position = USViewer.imagedepth;
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

                    prefs.PutFloat("BIOPSY_OUT_PLANE_POSITION", Biopsy.GetInstance().OutPlanePosition);
                    break;
                case "OutPlanebtnDown":
                    if (Biopsy.GetInstance().VerticalRevert)
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
                        if (position > USViewer.imagedepth)
                        {
                            position = USViewer.imagedepth;
                        }
                        Biopsy.GetInstance().OutPlanePosition = position;
                    }
                    prefs.PutFloat("BIOPSY_OUT_PLANE_POSITION", Biopsy.GetInstance().OutPlanePosition);
                    break;
                case "OutPlanebtnLeft":
                    if (Biopsy.GetInstance().HorizontalRevert)
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
                    prefs.PutFloat("BIOPSY_OUT_PLANE_LEFT_POSITION", Biopsy.GetInstance().OutPlaneLeftPosition);
                    break;
                case "OutPlanebtnRight":
                    if (Biopsy.GetInstance().HorizontalRevert)
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
                    prefs.PutFloat("BIOPSY_OUT_PLANE_LEFT_POSITION", Biopsy.GetInstance().OutPlaneLeftPosition);
                    break;

                default:
                    break;
            }
        }

        public void StopAdjustBiopsy()
        {
            if (timerAdjustBiopsy != null)
            {
                if (timerAdjustBiopsy.IsEnabled)
                {
                    isadjusting = false;
                    timerAdjustBiopsy.Stop();
                    timerAdjustBiopsy = null;
                }
            }
        }

        public void CalculateBiopsy(USRawImage currentRawImage, string key)
        {
            if (Biopsy.GetInstance().InPlaneVisible != Visibility.Visible
                && Biopsy.GetInstance().OutPlaneVisible != Visibility.Visible)
            {
                //如果两个穿刺线都不可见，则不需要计算
                return;
            }
            USDSCor dscor = USDSCor.GetInstance(key);
            dscor.ResetCurrentDSConverter(currentRawImage);
            {
                USDSCor.USPoint ptSampleRight = new USDSCor.USPoint((currentRawImage.probeCap.imagingParameter.lineCount - 1), 0);
                USDSCor.USPoint ptSampleLeft = new USDSCor.USPoint(0, 0);
                USDSCor.USPoint ptLineLeft = dscor.DscMap(ptSampleLeft);
                USDSCor.USPoint ptLineRight = dscor.DscMap(ptSampleRight);
                USDSCor.USPoint ptLineCenter = new USDSCor.USPoint((ptLineLeft.X + ptLineRight.X) / 2.0f, 0);

                float scalePixel = (float)dscor.M_dbScalePixel; // mm/pixel
                float maxPos = (float)((ptLineLeft.X - ptLineCenter.X) * scalePixel);
                maxPos = ((int)((maxPos - 0.5f) / 0.5f)) * 0.5f;
                Biopsy.GetInstance().InPlaneMaxposition = maxPos;
                if (Biopsy.GetInstance().InPlaneMaxposition > maxPos)
                {
                    Biopsy.GetInstance().InPlaneMaxposition = maxPos;
                }
            }

            if (Biopsy.GetInstance().InPlaneVisible == Visibility.Visible)
            {
                //USDSCor dscor = USDSCor.GetInstance(ENHANCE_VIEW_DSCOR);

                USManager mgr = USManager.GetInstance(null);
                USDriver drv = mgr.GetCurrentDriver();
                USProbe prb = mgr.GetCurrentProbe();

                USDSCor.USPoint ptSampleRight = new USDSCor.USPoint((currentRawImage.probeCap.imagingParameter.lineCount - 1), 0);
                USDSCor.USPoint ptSampleLeft = new USDSCor.USPoint(0, 0);

                USDSCor.USPoint ptLineLeft = dscor.DscMap(ptSampleLeft);
                USDSCor.USPoint ptLineRight = dscor.DscMap(ptSampleRight);
                USDSCor.USPoint ptLineStart = new USDSCor.USPoint((ptLineLeft.X + ptLineRight.X) / 2.0f, 0);

                float scalePixel = (float)dscor.M_dbScalePixel; // mm/pixel
                int destHeight = dscor.GetDestHeight();
                int destWidth = dscor.GetDestWidth();
                double angle;
                USDSCor.USPoint ptLineEnd;

                ptLineStart.X += Biopsy.GetInstance().InPlanePosition / scalePixel;
                ptLineStart.Y = 0;
                ptLineEnd = new USDSCor.USPoint(0, (float)destHeight);
                angle = Biopsy.GetInstance().InPlaneAngle / 180.0 * Math.PI;
                //angle = Biopsy.GetInstance().InPlaneAngle;
                ptLineEnd.X = ptLineStart.X+(float)destHeight * (float)Math.Tan(-angle) ;//需要减去这条线在横轴上的偏移量
                ptLineEnd.Y = destHeight;



                float x1 = 0;
                float x2 = 0;
                float y1 = 0;
                float y2 = 0;
                //下面的两个if判断主要是限制穿刺线不需要超越viewer
                if (ptLineStart.X<0)
                {
                    //当x小于零时，需要重新设置一下Y
                    x1 = 0;
                    y1 = (float)Math.Abs(ptLineStart.X) * (float)Math.Tan(-(Math.PI / 2 - angle));
                }
                else
                {
                    x1 = (float)ptLineStart.X;
                    y1= (float)ptLineStart.Y;
                }
                if (ptLineEnd.X > destWidth)
                {
                    //当x小于零时，需要重新设置一下Y
                    x2 = destWidth;

                    float firstX = (float)destWidth - x1;
                    float secondX = (float)ptLineEnd.X - x1;

                    y2 = (firstX / secondX) * destHeight;
                    System.Diagnostics.Debug.WriteLine($"ptLineEnd.X；{ptLineEnd.X}");
                    System.Diagnostics.Debug.WriteLine($"第二点的Y；{y2}");
                }
                else
                {
                    x2 = (float)ptLineEnd.X;
                    y2 = (float)ptLineEnd.Y;
                }

                //Biopsy.GetInstance().InPlaneX1 = (float)ptLineStart.X;
                //Biopsy.GetInstance().InPlaneX2 =(float)ptLineEnd.X;
                //Biopsy.GetInstance().InPlaneY1 =(float)ptLineStart.Y;
                //Biopsy.GetInstance().InPlaneY2 =(float)ptLineEnd.Y;

                Biopsy.GetInstance().InPlaneX1 = x1;
                Biopsy.GetInstance().InPlaneX2 = x2;
                Biopsy.GetInstance().InPlaneY1 = y1;
                Biopsy.GetInstance().InPlaneY2 = y2;

            }
            else if (Biopsy.GetInstance().OutPlaneVisible == Visibility.Visible)
            {
                {
                    //USDSCor dscor = USDSCor.GetInstance(ENHANCE_VIEW_DSCOR);
                    //dscor.ResetCurrentDSConverter(currentRawImage);
                    USDSCor.USPoint ptSampleRight = new USDSCor.USPoint((currentRawImage.probeCap.imagingParameter.lineCount - 1), 0);
                    USDSCor.USPoint ptSampleLeft = new USDSCor.USPoint(0, 0);
                    USDSCor.USPoint ptLineLeft = dscor.DscMap(ptSampleLeft);
                    USDSCor.USPoint ptLineRight = dscor.DscMap(ptSampleRight);
                    USDSCor.USPoint ptLineStart = new USDSCor.USPoint((ptLineLeft.X + ptLineRight.X) / 2.0f, 0);

                    float scalePixel = (float)dscor.M_dbScalePixel; // mm/pixel
                    int destHeight = dscor.GetDestHeight(); // pixel
                    float height_mm = (float)destHeight * scalePixel;
                    float scaleStep = 1.0f;
                    if (height_mm >= 60 && height_mm <= 100)
                    {
                        scaleStep = 5.0f;
                    }
                    if (height_mm > 100 && height_mm <= 180)
                    {
                        scaleStep = 10.0f;
                    }
                    if (height_mm > 180.0f)
                    {
                        scaleStep = 15.0f;
                    }
                    int count = (int)(height_mm / scaleStep);

                    float y = (float)(Biopsy.GetInstance().OutPlanePosition / scalePixel);
                    float r = (float)(Biopsy.GetInstance().OutPlaneDSCRadius / scalePixel);
                    float x = (float)(ptLineStart.X + Biopsy.GetInstance().OutPlaneLeftPosition / scalePixel);

                    Biopsy.GetInstance().OutPlanePixelRadius = 2 * r;
                    Biopsy.GetInstance().Left = x - r;
                    Biopsy.GetInstance().Top = y - r;
                    //Biopsy.GetInstance().OutPlaneLeftPosition = x;
                    Biopsy.GetInstance().OutPlaneUpperX1 = x;
                    Biopsy.GetInstance().OutPlaneUpperY1 = 0;
                    Biopsy.GetInstance().OutPlaneUpperX2 = x;
                    double outplaneuppery2 = y - r;
                    if (outplaneuppery2 <= Biopsy.GetInstance().OutPlaneUpperY1)
                    {
                        outplaneuppery2 = Biopsy.GetInstance().OutPlaneUpperY1;
                    }
                    Biopsy.GetInstance().OutPlaneUpperY2 = (float)outplaneuppery2;

                    Biopsy.GetInstance().OutPlaneBottomX1 = x;
                    Biopsy.GetInstance().OutPlaneBottomY1 = destHeight;
                    Biopsy.GetInstance().OutPlaneBottomX2 = x;
                    Biopsy.GetInstance().OutPlaneBottomY2 = y + r;

                }
            }
            //方法结束

        }

        float LimitP(double val,double min,double max)
        {
            if (val<min)
            {
                val = 0;
            }
            if (val>max)
            {
                val = max;
            }
            return (float)val;
        }
    }
}

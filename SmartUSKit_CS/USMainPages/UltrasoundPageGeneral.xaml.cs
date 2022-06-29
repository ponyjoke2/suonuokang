using Newtonsoft.Json.Linq;
using SmartUSKit.Enums;
using SmartUSKit.SmartUSKit;
using SmartUSKit_CS.ExtensionMethods;
using SmartUSKit_CS.Model;
using SmartUSKit_CS.Operations;
using SmartUSKit_CS.USViewers;
using SmartUSKit_CS.USWindows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SmartUSKit_CS.USMainPages
{
    /// <summary>
    /// UltrasoundPage.xaml 的交互逻辑
    /// </summary>
    public partial class UltrasoundPage : Page, IUSManagerObserver
    {
        const string VesselMode = "vessel";
        
        private void InitVGainDrawerAnimation()
        {
            var myDoubleAnimation = new DoubleAnimation();
            //myDoubleAnimation.From = 15.0;//动画从控件的当前位置开始
            myDoubleAnimation.To = 190.0;
            myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            myStoryboard = new Storyboard();
            myStoryboard.Children.Add(myDoubleAnimation);
            Storyboard.SetTargetName(myDoubleAnimation, gridDrawer.Name);
            Storyboard.SetTargetProperty(myDoubleAnimation, new PropertyPath(Grid.WidthProperty));

            var hideDrawerDoubleAnimation = new DoubleAnimation();
            //hideDrawerDoubleAnimation.From = 190.0;//动画从控件的当前位置开始
            hideDrawerDoubleAnimation.To = 15.0;
            hideDrawerDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            hideDrawerStoryboard = new Storyboard();
            hideDrawerStoryboard.Children.Add(hideDrawerDoubleAnimation);
            Storyboard.SetTargetName(hideDrawerDoubleAnimation, gridDrawer.Name);
            Storyboard.SetTargetProperty(hideDrawerDoubleAnimation, new PropertyPath(Grid.WidthProperty));
        }
        private void BtnDrawer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.ActualHeight == 0 || this.ActualWidth == 0)
            {
                return;
            }
            if (bool.Parse(e.NewValue.ToString()))
            {
                //var idx = MainTabControl.SelectedIndex;
                //if (idx == 0)
                {
                    //hideDrawerStoryboard.Begin(this);
                }

            }
            else
            {
                //var idx = MainTabControl.SelectedIndex;
                //if (idx == 0)
                {
                    //myStoryboard.Begin(this);
                }
            }
        }
        void WSInitialized(object sender, EventArgs e)
        {
            HwndSource hs = PresentationSource.FromVisual(this) as HwndSource;
            hs.AddHook(new HwndSourceHook(WndProc));
        }
        public const int WM_NCLBUTTONDBLCLK = 0xA3;
        const int WM_NCLBUTTONDOWN = 0x00A1;
        const int HTCAPTION = 2;


        #region 移动窗口
        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        private static extern IntPtr GetSystemMenu(IntPtr hwnd, int revert);

        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        private static extern int RemoveMenu(IntPtr hmenu, int npos, int wflags);

        [DllImport("user32.dll", EntryPoint = "GetMenuItemCount")]
        private static extern int GetMenuItemCount(IntPtr hmenu);

        [DllImport("user32.dll", EntryPoint = "GetMenuStringW", CharSet = CharSet.Unicode)]
        private static extern int GetMenuString(IntPtr hMenu,
            uint uIDItem, StringBuilder lpString, int cchMax, uint flags);

        //常量
        private const int MF_BYPOSITION = 0x0400;
        private const int MF_DISABLED = 0x0002;


        private void FlashCurveWnd_SourceInitialized(object sender, EventArgs e)
        {
            //IntPtr handle = new WindowInteropHelper(this).Handle;
            //IntPtr hmenu = GetSystemMenu(handle, 0);
            //int cnt = GetMenuItemCount(hmenu);
            //for (int i = cnt - 1; i >= 0; i--)
            //{
            //    StringBuilder tmpstr = new StringBuilder(100);
            //    GetMenuString(hmenu, (uint)i, tmpstr, 255, MF_DISABLED | MF_BYPOSITION);
            //    if (tmpstr.ToString().Contains("移动"))
            //    {
            //        RemoveMenu(hmenu, i, MF_DISABLED | MF_BYPOSITION);
            //    }
            //}
        }

        #endregion
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_NCLBUTTONDOWN && wParam.ToInt32() == HTCAPTION)
            {
                handled = true;
                return (System.IntPtr)0;
            }
            //Debug.WriteLine($"{msg.ToString()},{wParam.ToString("X")},{lParam.ToString("X")}");
            if (msg == WM_NCLBUTTONDBLCLK)
            {
                handled = true;
                return (System.IntPtr)0;
            }

            //if (msg == 70 && wParam == (IntPtr)0 && lParam == (IntPtr)0x010fec24)
            //{
            //    handled = true;
            //    wParam = IntPtr.Zero;
            //}
            //if (msg == 70 && wParam == (IntPtr)0 && lParam == (IntPtr)0x010fec24)
            //{
            //    handled = true;
            //    wParam = IntPtr.Zero;
            //}
            //if (msg == 70 && wParam == (IntPtr)0 && lParam == (IntPtr)0x010fec24)
            //{
            //    handled = true;
            //    wParam = IntPtr.Zero;
            //}
            //if (msg == 70 && wParam == (IntPtr)0 && lParam == (IntPtr)0x010fec24)
            //{
            //    handled = true;
            //    wParam = IntPtr.Zero;
            //}

            //禁止窗口前台显示

            //if (msg==19&& wParam== (IntPtr)0 && lParam== (IntPtr)0)
            //{
            //    handled = true;
            //    wParam = IntPtr.Zero;
            //}
            //拦截标题栏双击和窗口移动事件
            if (msg == 0x00A3 || msg == 0x0003 || wParam == (IntPtr)0xF012)
            {
                handled = true;
                wParam = IntPtr.Zero;
            }
            #region 利用按键控制程序
            /*
            //msg 256 为键盘按下
            //wParam 37按键← 38按键↑ 39按键→ 40按键↓
            if ((int)wParam == 37 && msg == 256)
            {
                if (Biopsy.GetInstance().InPlaneVisible == Visibility.Visible
                    && USViewer.IsLive
                    && USViewer.GetCurrentViewer().IsSupportBiopsy())
                {
                    Biopsy.GetInstance().StartAdjustBiopsy("InPlanebtnLeft");
                }
                else if (Biopsy.GetInstance().OutPlaneVisible == Visibility.Visible
                     && USViewer.IsLive
                    && USViewer.GetCurrentViewer().IsSupportBiopsy())
                {
                    Biopsy.GetInstance().StartAdjustBiopsy("OutPlanebtnLeft");
                }

                handled = true;
                return (System.IntPtr)0;
            }
            if ((int)wParam == 38 && msg == 256)
            {
                if (Biopsy.GetInstance().InPlaneVisible == Visibility.Visible
                   && USViewer.IsLive
                   && USViewer.GetCurrentViewer().IsSupportBiopsy())
                {
                    Debug.WriteLine("keydown");
                    Biopsy.GetInstance().StartAdjustBiopsy("InPlanebtnUp");
                }
                else if (Biopsy.GetInstance().OutPlaneVisible == Visibility.Visible
                     && USViewer.IsLive
                    && USViewer.GetCurrentViewer().IsSupportBiopsy())
                {
                    Biopsy.GetInstance().StartAdjustBiopsy("OutPlanebtnUp");
                }

                handled = true;
                return (System.IntPtr)0;
            }
            if ((int)wParam == 39 && msg == 256)
            {
                if (Biopsy.GetInstance().InPlaneVisible == Visibility.Visible
                   && USViewer.IsLive
                   && USViewer.GetCurrentViewer().IsSupportBiopsy())
                {
                    Biopsy.GetInstance().StartAdjustBiopsy("InPlanebtnRight");
                }
                else if (Biopsy.GetInstance().OutPlaneVisible == Visibility.Visible
                     && USViewer.IsLive
                    && USViewer.GetCurrentViewer().IsSupportBiopsy())
                {
                    Biopsy.GetInstance().StartAdjustBiopsy("OutPlanebtnRight");
                }

                handled = true;
                return (System.IntPtr)0;
            }
            if ((int)wParam == 40 && msg == 256)
            {
                if (Biopsy.GetInstance().InPlaneVisible == Visibility.Visible
                   && USViewer.IsLive
                   && USViewer.GetCurrentViewer().IsSupportBiopsy())
                {
                    Biopsy.GetInstance().StartAdjustBiopsy("InPlanebtnDown");
                }
                else if (Biopsy.GetInstance().OutPlaneVisible == Visibility.Visible
                     && USViewer.IsLive
                    && USViewer.GetCurrentViewer().IsSupportBiopsy())
                {
                    Biopsy.GetInstance().StartAdjustBiopsy("OutPlanebtnDown");
                }

                handled = true;
                return (System.IntPtr)0;
            }

            //msg 257 为键盘抬起
            if (((int)wParam == 37 ||
                (int)wParam == 38 ||
                (int)wParam == 39 ||
                (int)wParam == 40) && msg == 257)
            {
                Biopsy.GetInstance().StopAdjustBiopsy();

                handled = true;
                return (System.IntPtr)0;
            }

            */
            #endregion
            return (System.IntPtr)0;

        }

        private void InitDscT(object obj)
        {
            //try
            //{
            //    USProbe probe = (USProbe)obj;
            //    USDSCor dscor = USDSCor.GetInstance(USGeneralView.GENERAL_VIEWER_DSCOR);
            //    USDSCor dscorBM = USDSCor.GetInstance(USBMViewer.BM_DSCOR);
            //    int zoom = probe.GetDefaultZoom();
            //    dscor.ResetCurrentDSConverter(probe, zoom);
            //    dscorBM.ResetCurrentDSConverter(probe, zoom);
            //    for (int i = 0; i < 4; i++)
            //    {
            //        //如果探头运行了，那么就暂停初始化,opencl这一块必须要同步，否则会导致奇怪的bug，比如gpu会莫名的增长到极高，正常情况下占用不了多少
            //        while (isLive)
            //        {
            //            System.Threading.Thread.Sleep(1);
            //        }
            //        dscor.InitDSConverter(probe, i);
            //    }

            //    //BM
            //    for (int i = 0; i < 4; i++)
            //    {
            //        while (isLive)
            //        {
            //            System.Threading.Thread.Sleep(1);
            //        }
            //        dscorBM.InitDSConverter(probe, i);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine(ex.ToString());
            //}

        }

        public void DrawToBitmap(RenderTargetBitmap bmp)
        {
            string patha = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + AppInfo.AppName;
            // string patha = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\"+App.AppName;
            if (!System.IO.Directory.Exists(patha))
            {
                System.IO.Directory.CreateDirectory(patha);
            }
            string filename = patha + "\\" + AppInfo.AppName + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + @".png";
            using (FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                //RenderTargetBitmap bmp = new RenderTargetBitmap((int)this.viewContainer.ActualWidth + 1, (int)this.viewContainer.ActualHeight + 1, 96, 96, PixelFormats.Pbgra32);
                //bmp.Render(this.viewContainer);
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(fs);
            }
            //MessageBox.Show(AppDomain.CurrentDomain.SetupInformation.ApplicationBase.ToString());
        }

        public void CoverviewContainer()
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)this.viewContainer.ActualWidth + 1, (int)this.viewContainer.ActualHeight + 1, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(this.viewContainer);
            CoverImage.Source = bmp;
            CoverImage.Visibility = Visibility.Visible;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Slider slider = (Slider)sender;
            USManager mgr = USManager.GetInstance(this.Dispatcher);
            USEnhanceDriver drv = (USEnhanceDriver)mgr.GetCurrentDriver();
            if (drv != null)
            {
                switch (slider.Name)
                {
                    case "Slider1":
                        drv.SetVGain((int)slider.Value, 0);
                        break;
                    case "Slider2":
                        drv.SetVGain((int)slider.Value, 1);
                        break;
                    case "Slider3":
                        drv.SetVGain((int)slider.Value, 2);
                        break;
                    case "Slider4":
                        drv.SetVGain((int)slider.Value, 3);
                        break;
                    case "Slider5":
                        drv.SetVGain((int)slider.Value, 4);
                        break;
                    case "Slider6":
                        drv.SetVGain((int)slider.Value, 5);
                        break;
                    case "Slider7":
                        drv.SetVGain((int)slider.Value, 6);
                        break;
                    case "Slider8":
                        drv.SetVGain((int)slider.Value, 7);
                        break;

                    default:
                        break;
                }
            }
        }

        private void btnDrawer_Click(object sender, RoutedEventArgs e)
        {
            USManager mgr = USManager.GetInstance(this.Dispatcher);
            USEnhanceDriver drv = (USEnhanceDriver)mgr.GetCurrentDriver();
            if (drv != null)
            {
                this.Slider1.Value = drv.GetVGain(0);
                this.Slider2.Value = drv.GetVGain(1);
                this.Slider3.Value = drv.GetVGain(2);
                this.Slider4.Value = drv.GetVGain(3);
                this.Slider5.Value = drv.GetVGain(4);
                this.Slider6.Value = drv.GetVGain(5);
                this.Slider7.Value = drv.GetVGain(6);
                this.Slider8.Value = drv.GetVGain(7);
            }

            btnDrawer.Visibility = Visibility.Hidden;
            gridDrawer.Width = 190;
            ShowSliders();
        }
        private void HideSliders()
        {
            this.Slider1.Visibility = Visibility.Hidden;
            this.Slider2.Visibility = Visibility.Hidden;
            this.Slider3.Visibility = Visibility.Hidden;
            this.Slider4.Visibility = Visibility.Hidden;
            this.Slider5.Visibility = Visibility.Hidden;
            this.Slider6.Visibility = Visibility.Hidden;
            this.Slider7.Visibility = Visibility.Hidden;
            this.Slider8.Visibility = Visibility.Hidden;
            this.btnReset.Visibility = Visibility.Hidden;
        }
        private void ShowSliders()
        {
            this.Slider1.Visibility = Visibility.Visible;
            this.Slider2.Visibility = Visibility.Visible;
            this.Slider3.Visibility = Visibility.Visible;
            this.Slider4.Visibility = Visibility.Visible;
            this.Slider5.Visibility = Visibility.Visible;
            this.Slider6.Visibility = Visibility.Visible;
            this.Slider7.Visibility = Visibility.Visible;
            this.Slider8.Visibility = Visibility.Visible;
            this.btnReset.Visibility = Visibility.Visible;
        }


        private void InitUSManager()
        {
            Biopsy.LoadBiopsy();

            USManager theMgr = USManager.GetInstance(this.Dispatcher);

            theMgr.RegisteObserver(this);

            theMgr.ScanProbe(true);
        }


        #region 滑屏事件
        ////MouseDown事件
        private double x = 0;
        private void Content_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            gridDrawer.Width = 15;
            btnDrawer.Visibility = Visibility.Visible;
            HideSliders();
            this.measStackpanel.Visibility = Visibility.Hidden;
            this.measNoObstetricsGrid.Visibility = Visibility.Hidden;
            this.measBM.Visibility = Visibility.Hidden;
            this.ChangeViewerStackpanel.Visibility = Visibility.Hidden;
            this.BiopsyStackpanel.Visibility = Visibility.Hidden;
            imageSwitch.Source = new BitmapImage(new Uri("/Resources/panel_in.png", UriKind.Relative));
        }
        ////MouseUp事件
        private void Content_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                Point p = e.GetPosition(this);
                var offsetX = p.X - x;
                int triggerDistance = 20;
                if (offsetX > triggerDistance)
                {
                    if (imageIndex > 0)
                    {
                        --imageIndex;
                        playImage(imageIndex);
                    }

                }
                if (offsetX < -triggerDistance)
                {
                    if (imageIndex < rawImages.Count - 1)
                    {
                        ++imageIndex;
                        playImage(imageIndex);
                    }

                }
                this.ReleaseMouseCapture();
            }
        }
        ////TouchDown事件
        private TouchPoint TouchPositionDown;
        private void Content_TouchDown(object sender, TouchEventArgs e)
        {
            FrameworkElement el = sender as FrameworkElement;
            el.CaptureTouch(e.TouchDevice);
            TouchPositionDown = e.GetTouchPoint(viewContainer);
        }
        ////TouchUp事件
        private TouchPoint TouchPositionUp;
        private void Content_TouchUp(object sender, TouchEventArgs e)
        {
            FrameworkElement el = sender as FrameworkElement;
            el.CaptureTouch(e.TouchDevice);

            TouchPositionUp = e.GetTouchPoint(viewContainer);

            double nMoveX = Math.Abs(TouchPositionUp.Position.X - TouchPositionDown.Position.X);
            double nMoveY = Math.Abs(TouchPositionUp.Position.Y - TouchPositionDown.Position.Y);

            if ((nMoveX > 50) && (TouchPositionUp.Position.X < TouchPositionDown.Position.X))
            {
                if (imageIndex < rawImages.Count - 1)
                {
                    ++imageIndex;
                    playImage(imageIndex);
                }
            }
            if ((nMoveX > 50) && (TouchPositionUp.Position.X > TouchPositionDown.Position.X))
            {
                if (imageIndex > 0)
                {
                    --imageIndex;
                    playImage(imageIndex);
                }
            }
        }
        #endregion

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            this.Slider1.Value = this.Slider2.Value = this.Slider3.Value
            = this.Slider4.Value = this.Slider5.Value = this.Slider6.Value
            = this.Slider7.Value = this.Slider8.Value = 127;
        }

        private void BiopsyButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            USPreferences prefs = USPreferences.GetInstance();
            switch (button.Name)
            {
                case "btnInPanelBiopsy"://平面内
                    Biopsy.GetInstance().InPlaneVisible = Visibility.Visible;
                    Biopsy.GetInstance().InPlaneFivebuttonsVisible = Visibility.Visible;
                    Biopsy.GetInstance().OutPlaneVisible = Visibility.Hidden;
                    Biopsy.GetInstance().OutPlaneSevenbuttonsVisible = Visibility.Hidden;

                    prefs.PutInt("BIOPSY_MODE", Biopsy.IN_PLANE_BIOPSY);
                    break;
                case "btnOutPanelBiopsy"://平面外
                    Biopsy.GetInstance().InPlaneVisible = Visibility.Hidden;
                    Biopsy.GetInstance().InPlaneFivebuttonsVisible = Visibility.Hidden;
                    Biopsy.GetInstance().OutPlaneVisible = Visibility.Visible;
                    Biopsy.GetInstance().OutPlaneSevenbuttonsVisible = Visibility.Visible;

                    prefs.PutInt("BIOPSY_MODE", Biopsy.OUT_PLANE_BIOPSY);
                    break;
                case "btnEnhanceBiopsy"://增强

                    break;
                case "btnClearBiopsy"://清除
                    Biopsy.GetInstance().InPlaneVisible = Visibility.Hidden;
                    Biopsy.GetInstance().InPlaneFivebuttonsVisible = Visibility.Hidden;
                    Biopsy.GetInstance().OutPlaneVisible = Visibility.Hidden;
                    Biopsy.GetInstance().OutPlaneSevenbuttonsVisible = Visibility.Hidden;

                    prefs.PutInt("BIOPSY_MODE", Biopsy.NONE_BIOPSY);
                    break;
                case "btnHRevrtBiopsy"://水平反转
                    if (Biopsy.GetInstance().HorizontalRevert == true)
                    {
                        Biopsy.GetInstance().HorizontalRevert = false;
                    }
                    else
                    {
                        Biopsy.GetInstance().HorizontalRevert = true;
                    }
                    break;
                case "btnVRevrtBiopsy"://垂直反转
                    if (Biopsy.GetInstance().VerticalRevert == true)
                    {
                        Biopsy.GetInstance().VerticalRevert = false;
                    }
                    else
                    {
                        Biopsy.GetInstance().VerticalRevert = true;
                    }
                    break;
                default:
                    break;
            }
            BiopsyStackpanel.Visibility = Visibility.Hidden;
            //MainButton_Click(new Button() { Name = "btnMeas" }, null);
            //this.measStackpanel.Visibility = Visibility.Hidden;
        }

        DispatcherTimer timerChangeGain = new DispatcherTimer() { };
        private void Image_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            USViewer.StartShowInformation(true);
            USManager mgrbtnPositiveGain = USManager.GetInstance(this.Dispatcher);
            USDriver drivermgrbtnPositiveGain = mgrbtnPositiveGain.GetCurrentDriver();
            if (((Button)sender).Name.ToString() == "btnPositiveGain")
            {
                if (drivermgrbtnPositiveGain != null)
                {
                    drivermgrbtnPositiveGain.IncGain();
                }
            }
            else if (((Button)sender).Name.ToString() == "btnNegativeGain")
            {
                if (drivermgrbtnPositiveGain != null)
                {
                    drivermgrbtnPositiveGain.DecGain();
                }
            }
            timerChangeGain = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            timerChangeGain.Tag = ((Button)sender).Name.ToString();
            timerChangeGain.Tick += new EventHandler(ControlGain);
            timerChangeGain.Start();
        }
        private void Image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            timerChangeGain.Stop();
        }
        private void btnGain_TouchUp(object sender, TouchEventArgs e)
        {
            if (timerChangeGain.IsEnabled)
            {
                timerChangeGain.Stop();
            }
        }
        void ControlGain(object sender, EventArgs e)
        {
            timerChangeGain.Interval = TimeSpan.FromMilliseconds(100);
            USViewer.StartShowInformation(true);
            USManager mgrbtnPositiveGain = USManager.GetInstance(this.Dispatcher);
            USDriver drivermgrbtnPositiveGain = mgrbtnPositiveGain.GetCurrentDriver();
            if (timerChangeGain.Tag.ToString() == "btnPositiveGain")
            {
                if (drivermgrbtnPositiveGain != null)
                {
                    drivermgrbtnPositiveGain.IncGain();
                    if (drivermgrbtnPositiveGain.GetGain()>=105)
                    {
                        if (timerChangeGain.IsEnabled)
                        {
                            timerChangeGain.Stop();
                        }
                    }
                }
            }
            if (timerChangeGain.Tag.ToString() == "btnNegativeGain")
            {
                if (drivermgrbtnPositiveGain != null)
                {
                    drivermgrbtnPositiveGain.DecGain();
                    if (drivermgrbtnPositiveGain.GetGain() <= 30)
                    {
                        if (timerChangeGain.IsEnabled)
                        {
                            timerChangeGain.Stop();
                        }
                    }
                }
            }
        }


        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
        }
        private void ColorGain_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void ColorGain_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }
        private void btnColorGain_TouchUp(object sender, TouchEventArgs e)
        {
        }
       
        #region 添加测量

        private void MeasureButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MeasurePWButton_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        private void SwitchViewerButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            switch (button.Name)
            {
                case "btnBViewer":
                    {
                        OperateProbe.GetInstance().SetB_Mode();
                        currentMode = ProbeMode.MODE_B;
                        UpdateButtonsState();
                        sImages.Clear();
                    }
                    break;
               
                default:
                    break;
            }
            
            if (ChangeViewerStackpanel.Visibility == Visibility.Hidden)
            {
                ChangeViewerStackpanel.Visibility = Visibility.Hidden;
            }
            else
            {
                ChangeViewerStackpanel.Visibility = Visibility.Hidden;
            }
        }

        private void ExaminationCombobox_DropDownOpened(object sender, EventArgs e)
        {
            Content_MouseDown(null, null);
        }

        
    }
}

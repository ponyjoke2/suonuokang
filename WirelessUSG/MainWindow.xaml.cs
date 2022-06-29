using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WirelessUSG
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        SmartUSKit_CS.MainWindow WirelessPage;
        public MainWindow()
        {
            InitializeComponent();

            //简体中文是0，繁体中文是1，英文是2；
            //必须在获取实例前设置语言
            //SmartUSKit_CS.MainWindow.Setlanguage(1);
            WirelessPage = SmartUSKit_CS.MainWindow.GetInstance();
            WirelessPage.MinimizeHandler += WirelessMinimize;
            WirelessPage.CloseHandler += WirelessClose;
            frame.Content = WirelessPage;

            //注册回调
            //SmartUSKit_CS.WirelessUSGSetting.RegisterCallback(this);


            this.SourceInitialized += new EventHandler(WSInitialized);
        }

        private void WirelessClose()
        {
            Task.Run(()=>
            {
                //1.5秒后强制关闭程序
                Task.Delay(1500);
                System.Diagnostics.Debug.WriteLine("强制关闭程序。。。");
                System.Environment.Exit(0);
            });
            WirelessPage.Dispose();
            this.Close();
            System.Diagnostics.Debug.WriteLine("即将关闭程序。。。");
            
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
        private void WirelessMinimize()
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            WirelessPage.Dispose();
        }
    }
}

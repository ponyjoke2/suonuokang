using SmartUSKit.SmartUSKit;
using SmartUSKit_CS.Model;
using SmartUSKit_CS.Operations;
using SmartUSKit_CS.wirelessusg3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Resources;
using System.Windows.Threading;

namespace SmartUSKit_CS.USViewers
{
    public class USViewer : Page
    {

        public const int STATE_FREEZE = 0;
        public const int STATE_LIVE = 1;
        public const int INFORMATION = 20003;
        public const int INFORMATION_ANIMATION = 20004;

        public static bool IsLive = false;
        //图像反转
        public bool isBiopsyR = false;

        protected static bool isRevert = false;

        protected bool NeedReloadImage = false;
        protected USRawImage generalImage;
        
        public virtual bool IsSupportBiopsy()
        {
            return false;
        }

        public static int imagedepth=280;


        public static bool isSizeAdjustable = false;

        protected static USViewer currentViewer;
        public static void switchViewer(ContentControl viewArea, USViewer newViewer)
        {
            if (viewArea == null)
            {
                return;
            }
            if (currentViewer == newViewer)
            {
                return;
            }
            currentViewer = newViewer;
            Frame frmbtnBM = new Frame();
            frmbtnBM.NavigationUIVisibility = NavigationUIVisibility.Hidden;
            frmbtnBM.Content = currentViewer;
            viewArea.Content = frmbtnBM;
        }
        public static void switchCopyViewer(ContentControl viewArea, USViewer newViewer)
        {
            if (viewArea == null)
            {
                return;
            }
            if (currentViewer == newViewer)
            {
                return;
            }

            //currentViewer = newViewer;
            Frame frmbtnBM = new Frame();
            frmbtnBM.NavigationUIVisibility = NavigationUIVisibility.Visible;
            frmbtnBM.Content = newViewer;
            viewArea.Content = frmbtnBM;
        }
        private static System.Threading.Timer timerShowInformation;

        public static void StartShowInformation(bool isshowinformation)
        {
            if (isshowinformation)
            {
                if (Preset.GetInstance().InfoVisible ==0&& timerShowInformation != null)
                {
                    Preset.GetInstance().InfoVisible = System.Windows.Visibility.Visible;
                    Preset.GetInstance().StackpanelOpacity = 1; 
                    timerShowInformation.Dispose();
                    timerShowInformation = null;
                    timerShowInformation = new System.Threading.Timer(new System.Threading.TimerCallback(HideImformation), null, 4000, 100);
                    return;
                }
                if (Preset.GetInstance().InfoVisible==System.Windows.Visibility.Visible)
                {
                    return;
                }
                Preset.GetInstance().InfoVisible = System.Windows.Visibility.Visible;
                if (timerShowInformation!=null)
                {
                    timerShowInformation.Dispose();
                    timerShowInformation = null;
                }
                timerShowInformation = new System.Threading.Timer(new System.Threading.TimerCallback(HideImformation), null, 4000, 10);
               
            }
            else
            {
                if (timerShowInformation!=null)
                {
                    timerShowInformation.Dispose();
                    timerShowInformation = null;
                }
               
            }
        }

        private static void HideImformation(object obj)
        {
            Preset.GetInstance().StackpanelOpacity -=0.08;
            if (Preset.GetInstance().StackpanelOpacity >=0.1)
            {
                return;
            }
            if (timerShowInformation!=null)
            {
                timerShowInformation.Dispose();
                timerShowInformation = null;
            }
            Preset.GetInstance().StackpanelOpacity=1;
            Preset.GetInstance().InfoVisible = System.Windows.Visibility.Hidden;
        }

        public static USViewer GetCurrentViewer()
        {
            return currentViewer;
        }

        public delegate void SlideLeftDelegate();
        public event SlideLeftDelegate SlideLeftEventHandler;
        public delegate void SlideRightDelegate();
        public event SlideRightDelegate SlideRightEventHandler;

        public delegate void ViewerChangedDelegate();
        public static event ViewerChangedDelegate ViewerChangedEventHandler;
        
        

       
        public virtual void UpdateCount(int index,int count)
        {
        }
        public virtual void UpdateState(int freeze)
        {
        }
        public virtual void SetRawImage(USRawImage rawImage)
        {
        }
        
        protected BitmapSource LastFrameBitmapSource;
        protected byte[] LastRawImageArray = new byte[10];

        protected bool IsRawImageChanged(USRawImage newRawImage)
        {
            bool ischanged = false;
            if (newRawImage == null)
            {
                return true;
            }
            if (newRawImage.rawData.Length != LastRawImageArray.Length)
            {
                LastRawImageArray = new byte[newRawImage.rawData.Length];
                ischanged = true;
            }

            for (int i = 0; i < newRawImage.rawData.Length; i = i + 32)
            {
                if (newRawImage.rawData[i] != LastRawImageArray[i])
                {
                    ischanged = true;
                    break;
                }
            }
            if (ischanged == true)
            {
                System.Array.Copy(newRawImage.rawData, LastRawImageArray, newRawImage.rawData.Length);
            }
            return ischanged;
        }
    }
    
}

using SmartUSKit.Enums;
using SmartUSKit.SmartUSKit;
using SmartUSKit_CS.ExtensionMethods;
using SmartUSKit_CS.LanguageFiles;
using SmartUSKit_CS.Model;
using SmartUSKit_CS.Operations;
using SmartUSKit_CS.USTools;
using SmartUSKit_CS.USViewers;
using SmartUSKit_CS.USWindows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
    public partial class UltrasoundPage : Page, IUSManagerObserver, IDisposable
    {
        private UltrasoundPage()
        {
            InitializeComponent();

            USPreferences prefs = USPreferences.GetInstance();
            string languageInt = prefs.GetString("language", "0");
            foreach (System.Windows.ResourceDictionary item in mainRes.MergedDictionaries)
            {
                string ji = item.Source.ToString();

                //这个地方指示为了获取一下语言的ResourceDictionary
                if (ji.Contains("MultiLanguages"))
                {
                    string lang = "zh-cn";
                    if (languageInt == "2")
                    {
                        lang = "en-us";
                    }
                    else if (languageInt == "1")
                    {
                        lang = "zh-tw";
                    }
                    item.Source = new Uri($"/SmartUSKit_View;component/MultiLanguages/USMainPages/UltrasoundPage/UltrasoundPage-{lang}.xaml", UriKind.RelativeOrAbsolute);
                }
            }
        }

       

        private static UltrasoundPage Instance = null;
        public static UltrasoundPage GetInstance()
        {
            if (Instance == null)
            {
                Instance = new UltrasoundPage();
            }
            return Instance;
        }

        bool isload = false;

        protected void OnLoaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Loaded");
            

            if (isload == true)
            {
                isload = true;
                return;
            }
            isload = true;

            //设置程序的优先级为AboveNormal
            var process1 = System.Diagnostics.Process.GetCurrentProcess();
            process1.PriorityClass = ProcessPriorityClass.RealTime;
            UpdateButtonsState();
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                #region 初始化图标
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                this.labelVersion.Content = "V " + version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString();
                //冻结按钮默认设置为不可用，根据连接状态来使能其是否可用
                this.btnFreeze.IsEnabled = false;

                this.ZoomMoveLabel.Content = Properties.Resources.Move;

                int height = (int)viewContainer.ActualHeight;
                int width = (int)viewContainer.ActualWidth;
                int Hless4 = height % 4;
                if (Hless4 != 0)
                {
                    viewContainer.Height = height - Hless4;
                }
                else
                {
                    viewContainer.Height = height;
                }
                int Wless4 = width % 4;
                if (Wless4 != 0)
                {
                    viewContainer.Width = width - Wless4;
                }
                else
                {
                    viewContainer.Width = width;
                }

                #region 将界面显示一遍，以保证所有Viewer都被展示过，从而使界面有实际的宽和高
                Thread thread = new Thread(() =>
                {
                    int sleep = 50;
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        viewContainer.Visibility = Visibility.Hidden;
                    }));
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        USViewer.switchViewer(viewContainer, USBMViewer.GetInstance());
                    })); Thread.Sleep(sleep);
                    
                    
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        USViewer.switchViewer(viewContainer, USEnhanceViewer.GetInstance());
                    })); Thread.Sleep(sleep);

                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        viewContainer.Visibility = Visibility.Visible;
                    }));
                });

                thread.Start();
                #endregion

                USViewer.switchViewer(viewContainer, USEnhanceViewer.GetInstance());


                currentViewer = USGeneralView.GetInstance();
                USGeneralView.GetInstance().SlideLeftEventHandler += GetLastImage;
                USGeneralView.GetInstance().SlideRightEventHandler += GetNextImage;
                USEnhanceViewer.GetInstance().SlideLeftEventHandler += GetLastImage;
                USEnhanceViewer.GetInstance().SlideRightEventHandler += GetNextImage;

                #endregion
            }));
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                #region 原本就在Onloaded中的代码
                Thread thread = new Thread(new ThreadStart(InitUSManager));
                thread.IsBackground = true;
                thread.Start();
                #endregion
            }));

            var wid = viewContainer.ActualWidth * 7 / 8;
            var hig = viewContainer.ActualHeight;
            

            USDSCor dscorBM = USDSCor.GetInstance(USBMViewer.BM_DSCOR);
            var widBM = viewContainer.ActualWidth * 7 / 8 - 8;
            var higBM = viewContainer.ActualHeight * 0.6;
            dscorBM.SetDestSize((int)widBM, (int)higBM);

            USDSCor dscorGeneral = USDSCor.GetInstance(USGeneralView.GENERAL_VIEWER_DSCOR);
            var widGeneral = viewContainer.ActualWidth * 7 / 8 - 8;
            var higGeneral = viewContainer.ActualHeight;
            dscorGeneral.SetDestSize((int)widGeneral, (int)higGeneral);

            
            
            FlashCurveWnd_SourceInitialized(null, null);
        }







        static ObservableCollection<Label> examinationsItemSource = new ObservableCollection<Label>();


        private USViewer currentViewer;

        private ProbeMode currentProbeMode;

        public ProbeMode CurrentProbeMode
        {
            get { return currentProbeMode; }
            set { currentProbeMode = value; }
        }

        bool isProbeConncet = false;
        bool isLive = false;
        /// <summary>
        /// 图片回放标志
        /// </summary>
        bool isCineLoop = false;
        bool isEnhanceProbe = false;
        bool isColorProbe = false;
        /// <summary>
        /// 保存视频标志
        /// </summary>
        bool isVideoRecording = false;
        bool isLinear = false;

        //public static string ReportImageBytes;

        private string TargetVideo;
        private string SourceVideo;
        //ProbeMode currentMode = ProbeMode.MODE_B;
        ProbeMode _currentMode = ProbeMode.MODE_B;

        public ProbeMode currentMode
        {
            get
            {
                return _currentMode;
            }
            set
            {
                _currentMode = value;
            }
        }
        List<USRawImage> rawImages = new List<USRawImage>();
        int imageIndex = 1;

        List<USImage> sImages = new List<USImage>();

        public static USImage CurrentUSImage;
        private int currentIndex = 1;

        public int CurrentIndex
        {
            get { return currentIndex; }
            set
            {
                currentIndex = value;
                if (sImages.Count > currentIndex && currentIndex >= 0)
                {
                    CurrentUSImage = sImages[currentIndex];
                }
            }
        }
        int currentsavevideoindex = 1;



        private Storyboard myStoryboard;
        private Storyboard hideDrawerStoryboard;

        public const string FullScreen = "FullScreen";

        /// <summary>
        /// 禁用所有按钮
        /// </summary>
        void DisableAllbuttons()
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                this.btnPositiveGain.IsEnabled = false;
                this.btnNegativeGain.IsEnabled = false;
                this.btnDepth.IsEnabled = false;
                this.btnFocus.IsEnabled = true;
                this.btnDyn.IsEnabled = false;
                this.btnHarmonic.IsEnabled = false;
                this.btnDenoise.IsEnabled = false;
                this.btnB.IsEnabled = false;
                
                this.btnPlay.IsEnabled = false;
                this.btnMeas.IsEnabled = false;
                this.btnAnnotate.IsEnabled = false;
                this.btnBiopsy.IsEnabled = false;
                this.btnClear.IsEnabled = false;
                this.btnSave.IsEnabled = false;
                this.btnSaveVideo.IsEnabled = false;
                this.btnPreset.IsEnabled = false;
            }));
        }
        private void IsEnableColorProbeBtn(bool enable = true)
        {
            USManager mgrbtnFocus = USManager.GetInstance(this.Dispatcher);
            USProbe uSProbe = mgrbtnFocus.GetCurrentProbe();

            UpdateButtonsState();
        }

        private void GetLastImage()
        {
            if (CurrentIndex > 0)
            {
                if (CurrentIndex - 1 >= sImages.Count)
                {
                    return;
                }
                CurrentIndex--;
                playImage(CurrentIndex, 0);

                currentsavevideoindex = CurrentIndex;
            }
        }
        void ChangeUSViewer(USRawImage rawImage)
        {
            if (rawImage == null)
            {
                return;
            }

            ProbeMode tismode = 0;
            if (rawImage.IsBMImage())
            {
                tismode = ProbeMode.MODE_BM;
                currentMode = ProbeMode.MODE_BM;
                USViewer.switchViewer(viewContainer, USBMViewer.GetInstance());
                currentViewer = USBMViewer.GetInstance();
            }

            else
            {
                if (rawImage.probeCap.IsLinearProbe())
                {
                    isLinear = true;
                }
                else
                {
                    isLinear = false;
                }

                
                 if (rawImage.IsEnhanceImage())
                {
                    tismode = ProbeMode.MODE_B;
                    USViewer.switchViewer(viewContainer, USEnhanceViewer.GetInstance());
                    currentViewer = USEnhanceViewer.GetInstance();
                    currentMode = ProbeMode.MODE_B;
                }
                else
                {
                    tismode = ProbeMode.MODE_B;
                    USViewer.switchViewer(viewContainer, USGeneralView.GetInstance());
                    currentViewer = USGeneralView.GetInstance();
                    currentMode = ProbeMode.MODE_B;
                }
            }

            try
            {
                bool isharmonic = false;

                if (rawImage is USEnhanceImage)
                {
                    USEnhanceImage enhanceImage = (USEnhanceImage)rawImage;
                    isharmonic = enhanceImage.harmonic;
                }
                if (rawImage is USBMEnhanceImage)
                {
                    USBMEnhanceImage enhanceImage = (USBMEnhanceImage)rawImage;
                    isharmonic = enhanceImage.harmonic;
                }
                

                var usparam = rawImage.probeCap.getUSParam(tismode, isharmonic);

                MILabel.Content = usparam.MI.ToString("0.0");
                TISLabel.Content = usparam.TIS.ToString("0.0");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            UpdateButtonsState();
        }

        private void playImage(int index)
        {
            USImage image = sImages[index];
            ChangeUSViewer(image.RawImage);
            USViewer.GetCurrentViewer().UpdateCount(index, (sImages.Count));
            USViewer.GetCurrentViewer().UpdateState(2);
            USViewer.GetCurrentViewer().SetRawImage(image.RawImage);
        }
        private void playImage(int index, int state)
        {
            if (sImages.Count <= 0)
            {
                return;
            }
            USImage image = sImages[index];
            ChangeUSViewer(image.RawImage);
            USViewer.GetCurrentViewer().UpdateCount(index, (sImages.Count));
            USViewer.GetCurrentViewer().UpdateState(state);
            
            USViewer.GetCurrentViewer().SetRawImage(image.RawImage);
            CurrentUSImage = image;
        }
        private void GetNextImage()
        {
            if (CurrentIndex < sImages.Count - 1)
            {
                CurrentIndex++;
                playImage(CurrentIndex, 0);

                currentsavevideoindex = CurrentIndex;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="e"></param>
        protected void OnClosed(EventArgs e)
        {
            try
            {
                USPreferences.GetInstance().Save();
                if (USViewer.IsLive)
                {
                    USManager mgrbtnFreeze = USManager.GetInstance(this.Dispatcher);
                    USDriver driverbtnFreeze = mgrbtnFreeze.GetCurrentDriver();
                    if (driverbtnFreeze != null)
                    {
                        driverbtnFreeze.ToggleLive();
                    }
                    Thread.Sleep(500);
                }
                //Thread.Sleep(500);
                USManager theMgr = USManager.GetInstance(this.Dispatcher);
                if (theMgr.IsScanning())
                {
                    theMgr.ScanProbe(false);
                }
                Debug.WriteLine("OnClosed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            finally
            {
            }
        }
        public async void OnProbeFound(USProbe probe)
        {
            isColorProbe = false;
            isEnhanceProbe = false;
            int defaultExaminationIndex = 0;

            if (probe != null)
            {
                //probe = probe.GetCurrentProbe();
                labelSSID.Content = probe.probeSSID;

               
                
                if (probe.IsEnhanceProbe())
                {
                    isEnhanceProbe = true;
                }
                
                //USExamUSManager.GetInstance().examinations = probe.supportedExaminations;

                string transducerMark = probe.TransducerMark();
            }
            else
            {
                labelSSID.Content = "";
                examinationsItemSource.Clear();
            }

            if (probe != null)
            {
                USDSCor dscor = USDSCor.GetInstance(USGeneralView.GENERAL_VIEWER_DSCOR);
                var wid = viewContainer.ActualWidth * 7 / 8 - 8;
                var hig = viewContainer.ActualHeight;
                dscor.SetDestSize((int)wid, (int)hig);

                USDSCor dscorBM = USDSCor.GetInstance(USBMViewer.BM_DSCOR);
                var widBM = viewContainer.ActualWidth * 7 / 8 - 8;
                var higBM = viewContainer.ActualHeight * 0.6;
                dscorBM.SetDestSize((int)widBM, (int)higBM);

                Thread InitDscThread = new Thread(new ParameterizedThreadStart(InitDscT));
                InitDscThread.IsBackground = true;
                InitDscThread.Priority = ThreadPriority.Normal;
                InitDscThread.Start(probe);
            }
            UpdateButtonsState();
        }

        public void OnProbeConnection(bool conn)
        {
            USPreferences.GetInstance().Save();
            isProbeConncet = conn;
            if (conn == true)
            {
                USManager mgr = USManager.GetInstance(this.Dispatcher);
                USProbe probe = mgr.GetCurrentProbe();
                labelSSID.Content = probe.probeSSID;
                if (probe is USUsbProbe)
                {
                    this.ConnectionWayImage.Source = BitmapToImagesource.ChangeBitmapToBitmapSource(Properties.Resources.USBConnection);
                    this.btnConnection.IsEnabled = false;
                    this.btnConnection.Effect = null;
                }
                else
                {
                    this.ConnectionWayImage.Source = BitmapToImagesource.ChangeBitmapToBitmapSource(Properties.Resources.WIFIConnection);
                    this.btnConnection.IsEnabled = true;
                }
                this.btnFreeze.IsEnabled = true;
            }
            else
            {
                this.ConnectionWayImage.Source = BitmapToImagesource.ChangeBitmapToBitmapSource(Properties.Resources.WIFIConnection);
                this.btnConnection.IsEnabled = true;
                labelSSID.Content = "";

                this.btnFreeze.IsEnabled = false;
                isLive = false;
            }
            UpdateButtonsState();
        }

        public async void OnProbeLive(bool live)
        {
            Content_MouseDown(null, null);
            if (live && isVideoRecording)
            {
                Thread.Sleep(1000);
                Operations.OperateProbe.GetInstance().ToggleLive();
                return;
            }
            //Debug.WriteLine($"探头状态更新了：{live}");
            USViewer.IsLive = isLive = live;
            if (isCineLoop)
            {
                isCineLoop = false;
                timer.Stop();
            }

            this.BiopsyStackpanel.Visibility = Visibility.Hidden;

            if (live)
            {
               
                sImages.Clear();
                USViewer.GetCurrentViewer().UpdateState(1);
                DisableAllbuttons();
                if (currentMode == ProbeMode.MODE_BM)
                {
                    BMGenerator bmGen = BMGenerator.GetInstance();
                    bmGen.Reset();
                }
                USPreferences.GetInstance().Save();
            }
            else
            {
                Biopsy.GetInstance().InPlaneFivebuttonsVisible = Visibility.Hidden;
                Biopsy.GetInstance().OutPlaneSevenbuttonsVisible = Visibility.Hidden;
                USViewer.GetCurrentViewer().UpdateState(0);
                //GC.Collect();
            }

            IsEnableColorProbeBtn(live);
            UpdateButtonsState();

            #region 

            #endregion
        }

        public void OnProbeMenu(int menuIndex)
        {
        }
        public void OnMiscs(Dictionary<string, string> commands)
        {
           
        }
        public async void OnRefreshProbe(int probeindex)
        {
            Debug.WriteLine($"探头更新：{probeindex}");
            USManager mgr = USManager.GetInstance(this.Dispatcher);
            USProbe probe = mgr.GetCurrentProbe();

            if (probe == null)
            {
                return;
            }

            if (probe != null)
            {
                USDSCor dscor = USDSCor.GetInstance(USGeneralView.GENERAL_VIEWER_DSCOR);
                var wid = viewContainer.ActualWidth * 7 / 8 - 8;
                var hig = viewContainer.ActualHeight;
                dscor.SetDestSize((int)wid, (int)hig);

                USDSCor dscorBM = USDSCor.GetInstance(USBMViewer.BM_DSCOR);
                var widBM = viewContainer.ActualWidth * 7 / 8 - 8;
                var higBM = viewContainer.ActualHeight * 0.6;
                dscorBM.SetDestSize((int)widBM, (int)higBM);

                await Task.Factory.StartNew(InitDscT, probe);
            }
        }



        public void OnImageCaptured(USRawImage rawImage)
        {
            //CountFPS.GetFPS();  

            if (isCineLoop)
            {
                isCineLoop = false;
                timer.Stop();
            }
            USManager mgr = USManager.GetInstance(this.Dispatcher);
            USDriver drv = mgr.GetCurrentDriver();
            if (rawImage == null)
            {
                return;
            }

            Type ImageType = rawImage.GetType();

            var ddd = typeof(USEnhanceImage);

            if (ImageType == ddd)
            {
                //Debug.WriteLine("");
            }

            ProbeMode tismode = 0;
            if (rawImage.IsBMImage())
            {
                tismode = ProbeMode.MODE_BM;
                currentMode = ProbeMode.MODE_BM;
                USViewer.switchViewer(viewContainer, USBMViewer.GetInstance());
                currentViewer = USBMViewer.GetInstance();
            }
            else
            {
                if (rawImage.probeCap.IsLinearProbe())
                {
                    isLinear = true;
                }
                else
                {
                    isLinear = false;
                }
                
                 if (rawImage.IsEnhanceImage())
                {
                    tismode = ProbeMode.MODE_B;
                    USViewer.switchViewer(viewContainer, USEnhanceViewer.GetInstance());
                    currentViewer = USEnhanceViewer.GetInstance();
                    currentMode = ProbeMode.MODE_B;
                }
                else
                {
                    tismode = ProbeMode.MODE_B;
                    USViewer.switchViewer(viewContainer, USGeneralView.GetInstance());
                    currentViewer = USGeneralView.GetInstance();
                    currentMode = ProbeMode.MODE_B;
                }
            }
            if (USViewer.GetCurrentViewer() != null)
            {
                USImage image = new USImage(rawImage);
                sImages.Add(image);
                if (sImages.Count > Preset.GetInstance().ImageMaxAmount)
                {
                    sImages.RemoveAt(0);
                }
                if (sImages.Count <= 0)
                {
                    return;
                }
                CurrentIndex = sImages.Count - 1;
                currentsavevideoindex = CurrentIndex;
                USViewer.GetCurrentViewer().UpdateCount(CurrentIndex, (sImages.Count));
                //USViewer.GetCurrentViewer().UpdateState(1);
                USViewer.GetCurrentViewer().UpdateState(1);
                USViewer.GetCurrentViewer().SetRawImage(rawImage);
            }

            try
            {
                bool isharmonic = false;

                if (rawImage is USEnhanceImage)
                {
                    USEnhanceImage enhanceImage = (USEnhanceImage)rawImage;
                    isharmonic = enhanceImage.harmonic;
                }
                if (rawImage is USBMEnhanceImage)
                {
                    USBMEnhanceImage enhanceImage = (USBMEnhanceImage)rawImage;
                    isharmonic = enhanceImage.harmonic;
                }

                var usparam = rawImage.probeCap.getUSParam(tismode, isharmonic);

                MILabel.Content = usparam.MI.ToString("0.0");
                TISLabel.Content = usparam.TIS.ToString("0.0");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            UpdateButtonsState();
        }

        //Play回放
        DispatcherTimer timer = null;
        //private bool IsPlay;

        void timer_Tick(object sender, EventArgs e)
        {
            //GetNextImage();
            int ind = PlayNextImage();
            if (CurrentIndex + 1 >= sImages.Count)
            {
                USViewer.GetCurrentViewer().UpdateState(0);
                timer.Stop();
                isCineLoop = false;
                UpdateButtonsState();
                //currentIndex--;
                return;
            }
        }

        DateTime ImageReplayTime = DateTime.Now;
        /// <summary>
        /// 用来记录图片回放过程中，第N张图片的时间，并用该时间计算第N+1张图片该何时显示
        /// </summary>
        DateTime cineLoopLastDateTime = DateTime.Now;
        bool PlayImageCallback()
        {
            DateTime currenttime = DateTime.Now;
            Debug.WriteLine($"时间差：{currenttime.Subtract(cineLoopLastDateTime).TotalMilliseconds}");
            cineLoopLastDateTime = currenttime;
            //GetNextImage();
            int ind = PlayNextImage();
            if (CurrentIndex + 1 >= sImages.Count)
            {
                USViewer.GetCurrentViewer().UpdateState(0);
                timer?.Stop();
                isCineLoop = false;
                UpdateButtonsState();
                //currentIndex--;
                return true;
            }
            return false;
        }
        private int PlayNextImage()
        {
            if (CurrentIndex < sImages.Count)
            {
                if (CurrentIndex >= sImages.Count)
                {
                    CurrentIndex = 0;
                }
                CurrentIndex++;
                if (CurrentIndex >= sImages.Count)
                {
                    CurrentIndex = sImages.Count - 1;
                }

                playImage(CurrentIndex, 2);

                currentsavevideoindex = CurrentIndex;
            }
            return CurrentIndex + 1;
        }

        List<BitmapImage> bitmaps = new List<BitmapImage>();

        private void Grid_GotFocus(object sender, RoutedEventArgs e)
        {
            gridDrawer.Width = 15;
            btnDrawer.Visibility = Visibility.Visible;
            imageSwitch.Source = new BitmapImage(new Uri("Images/open.jpg", UriKind.Relative));
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            //隐藏滑动条
            gridDrawer.Width = 15;
            btnDrawer.Visibility = Visibility.Visible;
            HideSliders();
            imageSwitch.Source = new BitmapImage(new Uri("/Resources/panel_in.png", UriKind.Relative));

            Button button = (Button)sender;
            if (button.Name.ToString() != "btnB")
            {
                if (ChangeViewerStackpanel.Visibility == Visibility.Visible)
                {
                    ChangeViewerStackpanel.Visibility = Visibility.Hidden;
                }
            }

            if (button.Name.ToString() != "btnMeas")
            {
                if (measStackpanel.Visibility == Visibility.Visible)
                {
                    measStackpanel.Visibility = Visibility.Hidden;
                }
                if (measNoObstetricsGrid.IsVisible)
                {
                    measNoObstetricsGrid.Visibility = Visibility.Hidden;
                }
               
                if (measBM.IsVisible)
                {
                    measBM.Visibility = Visibility.Hidden;
                }
                
            }
            if (button.Name.ToString() != "btnBiopsy")
            {
                if (BiopsyStackpanel.Visibility == Visibility.Visible)
                {
                    this.BiopsyStackpanel.Visibility = Visibility.Hidden;
                }
            }

            switch (button.Name)
            {
                case "btnPositiveGain":
                    {
                        USManager mgrbtnPositiveGain = USManager.GetInstance(this.Dispatcher);
                        USDriver drivermgrbtnPositiveGain = mgrbtnPositiveGain.GetCurrentDriver();

                        if (drivermgrbtnPositiveGain != null)
                        {
                            drivermgrbtnPositiveGain.IncGain();
                        }
                    }
                    
                    break;
                case "btnNegativeGain":
                    {
                        USManager mgrbtnPositiveGain = USManager.GetInstance(this.Dispatcher);
                        USDriver drivermgrbtnPositiveGain = mgrbtnPositiveGain.GetCurrentDriver();

                        if (drivermgrbtnPositiveGain != null)
                        {
                            drivermgrbtnPositiveGain.DecGain();
                        }
                    }
                    
                    break;
                case "btnDepth":
                    {
                        OperateProbe.GetInstance().ChangeZoom();
                        //OperateProbe.GetInstance().IncreseZoom();
                    }
                    break;
                case "btnFocus":
                    {
                        USViewer.StartShowInformation(true);
                        USManager mgr = USManager.GetInstance(this.Dispatcher);
                        USEnhanceDriver drv = (USEnhanceDriver)mgr.GetCurrentDriver();
                        if (drv != null)
                        {
                            int focuspos = drv.GetFocusPos();
                            focuspos++;
                            if (focuspos > 3)
                            {
                                focuspos = 0;
                            }
                            drv.SetFocusPos(focuspos);
                        }
                    }
                    break;
                case "btnDyn":
                    {
                        USViewer.StartShowInformation(true);
                        USManager mgr = USManager.GetInstance(this.Dispatcher);
                        USEnhanceDriver drv = (USEnhanceDriver)mgr.GetCurrentDriver();
                        if (drv != null)
                        {
                            int dr = drv.GetDynamicRange();
                            dr += 10;
                            if (dr > 110)
                            {
                                dr = 40;
                            }
                            drv.SetDynamicRange(dr);
                        }
                    }
                    break;
                case "btnHarmonic":
                    {
                        try
                        {
                            USViewer.StartShowInformation(true);
                            USManager mgr = USManager.GetInstance(this.Dispatcher);
                            USEnhanceDriver drv = (USEnhanceDriver)mgr.GetCurrentDriver();
                            if (drv != null)
                            {
                                bool harmonicEnable = false;
                                if (drv.GetHarmonic())
                                {
                                    harmonicEnable = false;
                                    drv.SetHarmonic(harmonicEnable);
                                }
                                else
                                {
                                    harmonicEnable = true;
                                    drv.SetHarmonic(harmonicEnable);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    break;
                case "btnDenoise":
                    {
                        USViewer.StartShowInformation(true);
                        USManager mgr = USManager.GetInstance(this.Dispatcher);
                        USEnhanceDriver drv = (USEnhanceDriver)mgr.GetCurrentDriver();
                        if (drv != null)
                        {
                            int enhLevel = drv.GetEnhanceLevel();
                            enhLevel++;
                            if (enhLevel > 4)
                            {
                                enhLevel = 0;
                            }
                            drv.SetEnhanceLevel(enhLevel);
                        }
                    }
                    break;
                case "btnB":
                    if (isColorProbe)
                    {
                        this.btnColorViewer.IsEnabled = true;
                        this.btnPDIViewer.IsEnabled = true;
                        this.btnPWViewer.IsEnabled = true;
                        this.btnBCDViewer.IsEnabled = true;
                    }
                    else
                    {
                        this.btnColorViewer.IsEnabled = false;
                        this.btnPDIViewer.IsEnabled = false;
                        this.btnPWViewer.IsEnabled = false;
                        this.btnBCDViewer.IsEnabled = false;
                    }
                    if (true)
                    {
                        if (ChangeViewerStackpanel.Visibility == Visibility.Hidden)
                        {
                            ChangeViewerStackpanel.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            ChangeViewerStackpanel.Visibility = Visibility.Hidden;
                        }
                    }
                    else
                    {
                        USManager mgr = USManager.GetInstance(this.Dispatcher);
                        USDriver drv = mgr.GetCurrentDriver();
                        if (drv != null)
                        {
                            drv.SetMode(ProbeMode.MODE_B);
                        }
                        currentMode = ProbeMode.MODE_B;
                       
                        UpdateButtonsState();
                        sImages.Clear();
                    }
                    break;
                case "btnBM":
                    {
                        USManager mgr = USManager.GetInstance(this.Dispatcher);
                        USDriver drv = mgr.GetCurrentDriver();
                        if (drv != null)
                        {
                            ProbeMode prevMode = drv.GetMode();
                            if (prevMode != ProbeMode.MODE_BM)
                            {
                                BMGenerator.GetInstance().Reset();
                                drv.SetMode(ProbeMode.MODE_BM);
                            }
                        }
                    }
                    currentMode = ProbeMode.MODE_BM;
                    currentMode = ProbeMode.MODE_BM;
                    UpdateButtonsState();
                    sImages.Clear();
                    break;
               
                case "btnFreeze":
                    Biopsy.GetInstance().InPlaneFivebuttonsVisible = Visibility.Hidden;
                    Biopsy.GetInstance().OutPlaneSevenbuttonsVisible = Visibility.Hidden;
                    if (timer != null)
                    {
                        if (timer.IsEnabled)
                        {
                            timer.Stop();
                        }
                    }
                    Operations.OperateProbe.GetInstance().ToggleLive();
                    break;
                case "btnPlay":
                    if (!isCineLoop)
                    {
                        DisableAllbuttons();

                        if (CurrentIndex + 1 > (sImages.Count - 1))
                        {
                            CurrentIndex = -1;
                        }
                        timer = new DispatcherTimer();
                        Thread thread = new Thread(() =>
                        {
                            if (sImages == null || sImages.Count <= 0)
                            {
                                return;
                            }
                            int CineLoopInterval = (1000 / GetFrameRate(sImages));
                            {
                                while (isCineLoop)
                                {
                                    if (DateTime.Now.Subtract(ImageReplayTime).TotalMilliseconds > CineLoopInterval)
                                    {
                                        ImageReplayTime = DateTime.Now;
                                        this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                                        {
                                            PlayImageCallback();
                                        }));
                                    }
                                    else
                                    {
                                        Thread.Sleep(2);
                                    }
                                }
                            }
                        });
                        thread.Start();
                        isCineLoop = true;
                        UpdateButtonsState();
                    }
                    else
                    {
                        timer.Stop();
                        USViewer.GetCurrentViewer().UpdateState(0);
                        currentsavevideoindex = CurrentIndex;
                        isCineLoop = false;

                        UpdateButtonsState();
                    }
                    break;
                case "btnDepthIncrease":
                    {
                    }
                    break;
                case "btnDepthDecrease":
                    {
                    }
                    break;
                case "btnBiopsy":
                    if (BiopsyStackpanel.Visibility == Visibility.Visible)
                    {
                        this.BiopsyStackpanel.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        this.BiopsyStackpanel.Visibility = Visibility.Visible;
                    }
                    break;
                case "btnSave":

                    //截取图像
                    RenderTargetBitmap bmp = new RenderTargetBitmap((int)this.viewContainer.ActualWidth + 1, (int)this.viewContainer.ActualHeight + 1, 96, 96, PixelFormats.Pbgra32);
                    bmp.Render(this.viewContainer);
                    //保存到本地
                    DrawToBitmap(bmp);

                    break;
                
                case "btnPreset"://
                    PresetWindow pre = new PresetWindow();
                    pre.Owner= Application.Current.MainWindow;
                    pre.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    pre.ShowDialog();
                    break;
               
                case "btnLastImage":
                    GetNextImage();
                    break;
                case "btnNextImage":
                    GetLastImage();
                    break;
                default:
                    break;
            }
        }

       
        private void UpdateButtonsState()
        {
            try
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    if (isProbeConncet)
                    {
                        this.btnFreeze.IsEnabled = true;
                    }
                    else
                    {
                        this.btnFreeze.IsEnabled = false;
                    }
                    USManager mgr = USManager.GetInstance(this.Dispatcher);
                    USProbe probe = mgr.GetCurrentProbe();
                    USDriver statedriver = mgr.GetCurrentDriver();
                    if (isLive)
                    {
                        this.btnHarmonic.IsEnabled = true;
                        this.btnHarmonic.Visibility = Visibility.Visible;
                        #region 切换界面按钮
                        if (currentMode == ProbeMode.MODE_B) btnBViewer.Tag = "run"; else btnBViewer.Tag = "stop";
                        if (currentMode == ProbeMode.MODE_BM) btnBM.Tag = "run"; else btnBM.Tag = "stop";

                        if (isColorProbe)
                        {
                            this.btnBViewer.IsEnabled = true;
                            this.btnBM.IsEnabled = true;
                            this.btnColorViewer.IsEnabled = true;
                            this.btnPDIViewer.IsEnabled = true;
                            this.btnPWViewer.IsEnabled = true;
                            this.btnBCDViewer.IsEnabled = true;
                        }
                        else
                        {
                            this.btnBViewer.IsEnabled = true;
                            this.btnBM.IsEnabled = true;
                            this.btnColorViewer.IsEnabled = false;
                            this.btnPDIViewer.IsEnabled = false;
                            this.btnPWViewer.IsEnabled = false;
                            this.btnBCDViewer.IsEnabled = false;
                        }
                        #endregion

                        changeimagegrid.Visibility = Visibility.Hidden;
                        this.btnPositiveGain.IsEnabled = true;

                        this.btnNegativeGain.IsEnabled = true;


                        this.btnDepth.IsEnabled = true;
                        this.btnDepth.Visibility = Visibility.Visible;

                        this.btnB.IsEnabled = true;

                        if (currentMode == ProbeMode.MODE_BM)
                        {
                            this.btnBiopsy.IsEnabled = false;
                            this.btnBiopsy.Visibility = Visibility.Visible;
                        }
                        
                        if (isEnhanceProbe)
                        {

                            this.btnDyn.IsEnabled = true;
                            this.btnDyn.Visibility = Visibility.Visible;
                            
                            this.btnDenoise.IsEnabled = true;
                        }
                        this.btnPlay.IsEnabled = false;

                        this.btnMeas.IsEnabled = false;
                        if (currentMode == ProbeMode.MODE_B
                        || currentMode == ProbeMode.MODE_BM)
                        {
                            this.btnMeas.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            this.btnMeas.Visibility = Visibility.Hidden;
                        }

                        this.btnAnnotate.IsEnabled = false;

                        this.btnClear.IsEnabled = false;
                        this.btnClear.Visibility = Visibility.Hidden;

                        this.btnSave.IsEnabled = false;
                        this.btnSaveVideo.IsEnabled = false;

                        this.btnPreset.IsEnabled = false;

                        if (isEnhanceProbe)
                        {
                            this.btnDrawer.IsEnabled = true;
                            this.btnSave.Visibility = Visibility.Hidden;
                            this.btnSaveVideo.Visibility = Visibility.Hidden;
                            this.btnAnnotate.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            this.btnDrawer.IsEnabled = false;
                            this.btnSave.Visibility = Visibility.Visible;
                            this.btnSaveVideo.Visibility = Visibility.Visible;
                            this.btnAnnotate.Visibility = Visibility.Visible;
                            this.btnBiopsy.Visibility = Visibility.Visible;
                        }
                    }
                    else
                    {
                        this.btnClear.Visibility = Visibility.Visible;
                        {
                            this.btnBViewer.Tag = "stop";
                            this.btnBM.Tag = "stop";
                            this.btnColorViewer.Tag = "stop";
                            this.btnPDIViewer.Tag = "stop";
                            this.btnPWViewer.Tag = "stop";
                            this.btnBCDViewer.Tag = "stop";
                            this.btnBViewer.IsEnabled = false;
                            this.btnBM.IsEnabled = false;
                            this.btnColorViewer.IsEnabled = false;
                            this.btnPDIViewer.IsEnabled = false;
                            this.btnPWViewer.IsEnabled = false;
                            this.btnBCDViewer.IsEnabled = false;
                        }
                        this.btnPositiveGain.IsEnabled = false;

                        this.btnNegativeGain.IsEnabled = false;

                        this.btnDepth.IsEnabled = false;
                        this.btnDepth.Visibility = Visibility.Hidden;

                        this.btnB.IsEnabled = false;

                        this.btnBiopsy.IsEnabled = false;
                        this.btnBiopsy.Visibility = Visibility.Hidden;

                        this.btnFocus.IsEnabled = true;

                        this.btnDyn.IsEnabled = false;
                        this.btnDyn.Visibility = Visibility.Hidden;

                        this.btnHarmonic.IsEnabled = false;
                        this.btnHarmonic.Visibility = Visibility.Visible;

                        this.btnDenoise.IsEnabled = false;

                        if (sImages.Count > 0)
                        {
                            this.btnNextImage.IsEnabled = true;
                            this.btnLastImage.IsEnabled = true;

                            this.btnBiopsy.Visibility = Visibility.Hidden;
                            this.changeimagegrid.Visibility = Visibility.Visible;
                            this.btnPlay.IsEnabled = true;
                            this.btnMeas.IsEnabled = true;
                            this.btnMeas.Visibility = Visibility.Visible;
                            this.btnClear.IsEnabled = true;
                            this.btnClear.Visibility = Visibility.Visible;
                            if (!(currentMode == ProbeMode.MODE_BM))
                            {
                                this.btnMeas.IsEnabled = true;
                                this.btnMeas.Visibility = Visibility.Visible;
                                

                                this.btnClear.IsEnabled = true;
                                this.btnClear.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                this.btnAnnotate.IsEnabled = false;
                                this.btnAnnotate.Visibility = Visibility.Visible;
                            }
                            this.btnSave.IsEnabled = true;
                            this.btnSave.Visibility = Visibility.Visible;
                            this.btnSaveVideo.IsEnabled = true;
                            this.btnSaveVideo.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            this.btnBiopsy.Visibility = Visibility.Visible;
                            this.changeimagegrid.Visibility = Visibility.Hidden;

                            this.btnPlay.IsEnabled = false;

                            this.btnMeas.IsEnabled = false;

                            this.btnAnnotate.IsEnabled = false;
                            this.btnAnnotate.Visibility = Visibility.Visible;

                            this.btnClear.IsEnabled = false;

                            this.btnSave.IsEnabled = false;
                            this.btnSave.Visibility = Visibility.Visible;

                            this.btnSaveVideo.IsEnabled = false;
                            this.btnSaveVideo.Visibility = Visibility.Visible;

                            this.btnPreset.IsEnabled = false;
                        }
                        
                        this.btnPreset.IsEnabled = true;
                        this.btnPreset.Visibility = Visibility.Visible;

                        this.btnDrawer.IsEnabled = false;
                    }


                    if (isLive &&
                            false)
                    {
                        this.btnBiopsy.Visibility = Visibility.Hidden;

                        this.btnSteer.Visibility = Visibility.Visible;
                        this.btnPlay.Visibility = Visibility.Hidden;

                        

                        this.btnColorPositiveGain.Visibility = Visibility.Visible;

                        this.btnColorNegativeGain.Visibility = Visibility.Visible;

                        

                        switch (currentMode)
                        {
                            case ProbeMode.MODE_BM:
                                this.Bbtnlbl.Content = "BM";
                                break;
                            default:
                                break;
                        }
                        
                        
                    }
                    else
                    {
                        this.btnSteer.Visibility = Visibility.Hidden;
                        this.btnPlay.Visibility = Visibility.Visible;

                        this.btnColorPositiveGain.Visibility = Visibility.Hidden;

                        this.btnColorNegativeGain.Visibility = Visibility.Hidden;

                        this.btnSampleVolume.Visibility = Visibility.Hidden;

                        this.btnZoomMove.Visibility = Visibility.Hidden;

                        this.btnAngle.Visibility = Visibility.Hidden;

                        this.btnWF.Visibility = Visibility.Hidden;

                        this.btnPRF.Visibility = Visibility.Hidden;
                        this.btnBaseline.Visibility = Visibility.Hidden;
                    }

                    if (isLive)
                    {
                        
                        if ((currentMode == ProbeMode.MODE_B))
                        {
                            this.Bbtnlbl.Content = "B";
                            this.btnBiopsy.IsEnabled = true;
                            this.btnBiopsy.Visibility = Visibility.Visible;
                            this.btnZoomMove.Visibility = Visibility.Hidden;
                        }
                    }
                    else
                    {
                        this.btnBiopsy.IsEnabled = false;
                        //this.btnBiopsy.Visibility = Visibility.Visible;
                        this.btnZoomMove.Visibility = Visibility.Hidden;
                        
                    }
                    if (isCineLoop)
                    {
                        this.btnMeas.IsEnabled = false;

                        this.btnAnnotate.IsEnabled = false;

                        this.btnClear.IsEnabled = false;

                        this.btnSave.IsEnabled = false;

                        this.btnSaveVideo.IsEnabled = false;

                        this.btnPreset.IsEnabled = false;

                        //this.btnReport.IsEnabled = false;
                        this.btnNextImage.IsEnabled = false;
                        this.btnLastImage.IsEnabled = false;
                    }

                    if (isVideoRecording)
                    {
                        this.btnFreeze.IsEnabled = false;

                        this.btnPlay.IsEnabled = false;
                        this.btnPlay.Visibility = Visibility.Visible;

                        this.btnMeas.IsEnabled = false;

                        this.btnAnnotate.IsEnabled = false;

                        this.btnClear.IsEnabled = false;

                        this.btnSave.IsEnabled = false;

                        this.btnSaveVideo.IsEnabled = false;

                        this.btnPreset.IsEnabled = false;

                        //this.btnReport.IsEnabled = false;
                        this.btnNextImage.IsEnabled = false;
                        this.btnLastImage.IsEnabled = false;
                    }
                   
                }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void Savefilebtn_Click(object sender, RoutedEventArgs e)
        {
          
        }






        /// <summary>
        /// 获取探头图像的帧率
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        int GetFrameRate(List<USImage> images)
        {
            int fps = 10;
            try
            {
                int interval = 100;
                List<int> intervals = new List<int>();

                for (int i = 1; i < images.Count; i++)
                {
                    TimeSpan timeSpan = images[i].RawImage.timeCap.Subtract(images[i - 1].RawImage.timeCap);
                    interval = (int)timeSpan.TotalMilliseconds;
                    intervals.Add((int)timeSpan.TotalMilliseconds);
                }
                intervals.Sort();
                int median = intervals[intervals.Count / 2];
                fps = 1000 / median;
            }
            catch (Exception ex)
            {
                fps = 10;
                Debug.WriteLine(ex.ToString());
            }

            return fps;
        }

        public void Dispose()
        {
            OnClosed(null);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            Debug.WriteLine("Page的尺寸开始改变了");
            Debug.WriteLine($"{e.NewSize}");
        }
    }
}

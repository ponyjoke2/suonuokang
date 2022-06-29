//using Microsoft.Analytics.Interfaces;
//using Microsoft.Analytics.Types.Sql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NativeWifi;
using System.Collections;
using System.Windows.Threading;
using SmartUSKit.SmartUSKit;
using System.Management;
using Newtonsoft.Json.Linq;
using CyUSB;
using System.Threading;
using System.Reflection;
using static NativeWifi.WlanClient;
using System.Net.NetworkInformation;
using SmartUSKit.Enums;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public interface IUSManagerObserver
    {
        void OnProbeFound(USProbe probe);
        void OnProbeConnection(bool conn);
        void OnProbeLive(bool live);
        void OnProbeMenu(int menuIndex);
        void OnImageCaptured(USRawImage rawImage);
        void OnRefreshProbe(int probeindex);
    }
    public class USManager : IUSDriverDelegate
    {
        int scanTickTime = 500;

        bool IsConnection = false;

        //
        //  Messages
        //
        public const int ON_PROBE_FOUND = 8001;
        public const int ON_PROBE_CONNECTION = 8002;
        public const int ON_PROBE_LIVE = 8003;
        public const int ON_PROBE_MENU = 8004;
        public const int ON_RFID = 8005;
        public const int ON_IMAGE_CAPTURED = 8006;

        protected static USManager instance = null;// new USManager();
        protected static object _lock = new object();

        private ProbeMode currentMode;

        public ProbeMode CurrentMode
        {
            get { return currentMode; }
            set { currentMode = value; }
        }

        private ProbeMode driverCurrentMode;


        public static USManager GetInstance(Dispatcher disp = null)
        {
            if (instance == null)
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        if (disp == null)
                        {
                            throw new ArgumentNullException("首次初始化，Dispatcher不能为空");
                        }

                        instance = new USManager(disp);
                    }
                }
            }
            return instance;
        }

        internal USManager(Dispatcher disp)
        {
            theObservers = new List<IUSManagerObserver>();
            dispatcher = disp;
        }

        //
        //  当前探头与驱动
        //
        protected USProbe currentProbe;
        protected USDriver currentDriver;
        protected Dispatcher dispatcher;        //  通过它与UI线程通信

        //
        //  注册事件的接收者
        //
        protected List<IUSManagerObserver> theObservers;
        public void RegisteObserver(IUSManagerObserver observer)
        {
            Debug.Assert(observer != null);
            foreach (IUSManagerObserver obj in theObservers)
            {
                if (obj == observer)
                {
                    return;
                }
            }
            theObservers.Add(observer);
        }

        SaveDriverParmetersDelegate saveDriverParmeters;

        public void SetsaveDriverParameters(SaveDriverParmetersDelegate mydelegate)
        {
            saveDriverParmeters = mydelegate;
        }
        //
        //  探头扫描，并获取其驱动器
        //
        private System.Threading.Timer scanTimer;
        protected string[] supportProbes=new string[] { "UL-3C"};
        protected bool keepScanning;

        Thread ScanSSIDThread;

       

        public void ScanProbe(bool toScan)
        {
            keepScanning = toScan;
            if (keepScanning)
            {
                if (scanTimer != null)
                {
                    scanTimer.Dispose();
                    scanTimer = null;
                }
                //scanTimer = new System.Threading.Timer(new System.Threading.TimerCallback(ScanTick), this, 2000, 2000);

                if (ScanSSIDThread != null)
                {
                    if (ScanSSIDThread.IsAlive == true)
                    {
                        ScanSSIDThread.Abort();
                    }
                    ScanSSIDThread = null;
                }
                ScanSSIDThread = new Thread(new ThreadStart(() =>
                {
                    while (keepScanning)
                    {
                        //明确知道探头状态或者距离上次获取图片太久，则重新获取ssid，为了防止运行过程中，探头被直接断开导致的无法获取探头冻结运行状态的问题
                        if (!IsLive
                        || DateTime.Now.Subtract(LastReceiveImageTime).TotalMilliseconds > 1500)
                        {
                            ScanTick(null);
                        }

                        //驱动或者探头或者连接状态为false，则扫描频繁一些
                        if (currentDriver == null || currentProbe == null || IsConnection == false)
                        {
                            Thread.Sleep(1000);
                            //添加这个判断，是为了在关闭程序时，尽快退出这个线程
                            if (keepScanning == false)
                            {
                                break;
                            }
                        }
                        else
                        {
                            Thread.Sleep(1000);
                            if (keepScanning == false)
                            {
                                break;
                            }
                            //中途检测一下连接状态，如果断开了连接则尽快进行一次WiFi扫描，主要是使连接更加迅速
                            if (IsConnection == false)
                            {
                                ScanTick(null);
                            }
                            Thread.Sleep(1000);
                            if (keepScanning == false)
                            {
                                break;
                            }
                        }
                    }
                }));
                ScanSSIDThread.Priority = ThreadPriority.Highest;
                ScanSSIDThread.Start();
            }
            else
            {
                USPreferences.GetInstance().Save();
                if (scanTimer != null)
                {
                    scanTimer.Dispose();
                    scanTimer = null;
                }
                if (ScanSSIDThread != null)
                {
                    if (ScanSSIDThread.IsAlive == true)
                    {
                        ScanSSIDThread.Abort();
                    }
                    ScanSSIDThread = null;
                }
                //  对象销毁，结束操作
                if (currentProbe != null)
                {
                    currentProbe.SaveDriverParmetersEventHandler -= saveDriverParmeters;
                    currentProbe = null;
                }
                if (currentDriver != null)
                {
                    currentDriver.Close();
                    currentDriver = null;
                }
            }
        }
        public bool IsScanning()
        {
            return keepScanning;
        }

        private void ScanTick(object obj)
        {
            var ssid = FetchSSID();

            if (!ssid.isUsb)
            {
                if (currentProbe != null
                      && currentDriver != null
                      && currentProbe.probeSSID.Equals(ssid.ssid))
                {
                    if (currentDriver.theManager != this)
                    {
                        currentDriver.theManager = this;
                    }
                    return;
                }
            }

            if (ssid.isUsb)
            {
                if (currentProbe != null
                    && currentDriver != null
                    && currentProbe.probeSSID.Equals(ssid.ssid)
                    && IsConnection == true
                )
                {
                    if (currentDriver.theManager != this)
                    {
                        currentDriver.theManager = this;
                    }
                    return;
                }
            }



            // current probe
            USProbe probe = null;
            if (ssid.isUsb)
            {
                probe = USBProbeFactory(ssid.ssid);
            }
            else
            {
                probe = ProbeFactory(ssid.ssid);
            }
            if (probe == currentProbe)
            {
                return;
            }

            currentProbe = probe;

            if (currentDriver != null)
            {
                currentDriver.Close();
                currentDriver = null;
            }
            if (currentProbe != null)
            {
                lock (currentProbe)
                {
                    currentDriver = currentProbe.MakeDriver();
                    currentDriver.theManager = this;
                    currentProbe.SaveDriverParmetersEventHandler += saveDriverParmeters;
                    currentDriver.Activate();
                }
            }
            else
            {
                currentDriver = null;
            }

            // Notice
            NoticeProbeFound(currentProbe);
        }

        private USProbe USBProbeFactory(string ssid)
        {
            if (ssid.Split(' ').Length <= 1)
            {
                return null;
            }
            USProbe probe = null;

            if (ssid.StartsWith("UL-3C "))
            {
                var dtprobe = new USProbeUL3C(ssid);
                probe = dtprobe;
            }


            return probe;
        }

        private void NoticeProbeFound(USProbe probe)
        {
            Debug.Assert(dispatcher != null);
            foreach (IUSManagerObserver obs in theObservers)
            {
                dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        obs.OnProbeFound(probe);
                    }));
            }
        }

        public void OnRefreshProbe(int probeindex)
        {
            Debug.Assert(dispatcher != null);
            foreach (IUSManagerObserver obs in theObservers)
            {
                dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        obs.OnRefreshProbe(probeindex);
                    }));
            }
        }
        protected static WlanClient client = null;
        public (string ssid, bool isUsb) FetchSSID()
        {
            string name = "-_-";
            bool isUSB = false;
            try
            {
                using (USBDeviceList usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB))
                {
                    if (usbDevices.Count > 0)
                    {
                        foreach (var usbitem in usbDevices)
                        {
                            foreach (var probeitem in supportProbes)
                            {
                                using (CyUSBDevice device = ((CyUSBDevice)usbitem))
                                {
                                    string sn = readSN(device);
                                    if (sn.Length > 7)
                                    {
                                        sn = sn.Remove(0, sn.Length - 7);
                                    }

                                    USUsbManager.getInstance(dispatcher);
                                    if (device.ProductID == 0xbcff && device.VendorID == 0x04b4)
                                    {
                                        string probetype = ReadProbetype(device);
                                        var strArr = device.Name.Split(' ');

                                        if (probetype == "UL3C")
                                        {
                                            probetype = "UL-3C";
                                        }
                                        name = probetype + " " + sn;
                                        isUSB = true;
                                        return (name, isUSB);
                                    }
                                }
                            }
                        }
                    }
                }

                name = FetchWiFiSSID();
                isUSB = false;
            }
            catch (ThreadAbortException e)
            {
                Debug.WriteLine("usb异常：" + e.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                name = "--";
            }
            finally
            {

            }
            return (name, isUSB);
        }

        string FetchWiFiSSID()
        {
            string name = "";
            if (client == null)
            {
                client = new WlanClient();
            }
            if (client.Interfaces != null)
            {
                foreach (WlanInterface wlanInterface in client.Interfaces)
                {
                    if (wlanInterface.InterfaceState == Wlan.WlanInterfaceState.Connected
                     && wlanInterface.CurrentConnection.isState == Wlan.WlanInterfaceState.Connected)
                    {
                        name = wlanInterface.CurrentConnection.profileName;
                        string[] names = name.Split(' ');
                        if (supportProbes.Contains(names.First()))
                        {
                            break;
                        }
                    }
                }
            }
            return name;
        }

        protected String ReadProbetype(CyUSBDevice device)
        {
            if (device == null)
            {
                return "GVBGCA001";
            }
            byte[] probeTypeBytes = new byte[16];
            int len = probeTypeBytes.Length;
            var theConnection = device.ControlEndPt;
            if (theConnection == null)
            {
                return "GVBGCA001";
            }
            //theConnection.controlTransfer(UsbConstants.USB_DIR_OUT, 0xc2, 0x01, 0, null, 0, 100);
            if (theConnection != null)
            {
                theConnection.Target = CyConst.TGT_DEVICE;
                theConnection.ReqType = CyConst.REQ_VENDOR;
                theConnection.Direction = CyConst.DIR_FROM_DEVICE;
                theConnection.ReqCode = 0xC0; // 读取探头类型
                theConnection.Value = 0;
                theConnection.Index = 0;
                theConnection.TimeOut = 100;

                bool issuccess = theConnection.XferData(ref probeTypeBytes, ref len);
            }

            String strProbeType = "";
            for (int i = 0; i < 16; i++)
            {
                if ((probeTypeBytes[i] >= '0' && probeTypeBytes[i] <= '9')
                        || (probeTypeBytes[i] >= 'A' && probeTypeBytes[i] <= 'Z')
                )
                {
                    if ((probeTypeBytes[i] >= '0' && probeTypeBytes[i] <= '9')
                            || (probeTypeBytes[i] >= 'A' && probeTypeBytes[i] <= 'Z')
                    )
                    {
                        strProbeType = strProbeType + (char)(probeTypeBytes[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return strProbeType;
        }
        protected String readSN(CyUSBDevice device)
        {
            if (device == null)
            {
                return "GVBGCA001";
            }
            byte[] SerialNumber = new byte[16];
            int len = SerialNumber.Length;
            var theConnection = device.ControlEndPt;
            if (theConnection == null)
            {
                return "GVBGCA001";
            }
            //theConnection.controlTransfer(UsbConstants.USB_DIR_OUT, 0xc2, 0x01, 0, null, 0, 100);
            if (theConnection != null)
            {
                theConnection.Target = CyConst.TGT_DEVICE;
                theConnection.ReqType = CyConst.REQ_VENDOR;
                theConnection.Direction = CyConst.DIR_FROM_DEVICE;
                theConnection.ReqCode = 0xC4; // Some vendor-specific request code
                theConnection.Value = 0;
                theConnection.Index = 0;
                theConnection.TimeOut = 100;

                bool issuccess = theConnection.XferData(ref SerialNumber, ref len);
            }

            String strSN = "";
            for (int i = 0; i < 16; i++)
            {
                if ((SerialNumber[i] >= '0' && SerialNumber[i] <= '9')
                        || (SerialNumber[i] >= 'A' && SerialNumber[i] <= 'Z')
                )
                {
                    if ((SerialNumber[i] >= '0' && SerialNumber[i] <= '9')
                            || (SerialNumber[i] >= 'A' && SerialNumber[i] <= 'Z')
                    )
                    {
                        strSN = strSN + (char)(SerialNumber[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            if (strSN == "")
            {
                strSN = "GVBGCA001";
            }
            return strSN;
        }
        protected USProbe ProbeFactory(string ssid)
        {
            USProbe probe = null;
            if (ssid.StartsWith("UL-3C "))
            {
                var dtprobe = new USProbeUL3CA(ssid);
                probe = dtprobe;
            }
            return probe;
        }

        public void OnProbeConnection(bool isConn)
        {
            IsConnection = isConn;
            Debug.WriteLine($"探头连接状态：{isConn}");

            //探头重新连接之后，需要重新设置一下模式，默认模式是B模式
            CurrentMode = ProbeMode.MODE_B;

            Debug.Assert(dispatcher != null);
            foreach (IUSManagerObserver obs in theObservers)
            {
                dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        obs.OnProbeConnection(isConn);
                    }));
            }
        }

        bool IsLive = false;
        public void OnUSLive(bool isLive)
        {
            IsLive = isLive;
            Debug.Assert(dispatcher != null);
            foreach (IUSManagerObserver obs in theObservers)
            {
                dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        obs.OnProbeLive(isLive);

                    }));
            }
        }


        DateTime LastReceiveImageTime = DateTime.Now;
        public void OnUSImage(USRawImage rawImage)
        {
            LastReceiveImageTime = DateTime.Now;
            Debug.Assert(dispatcher != null);
            foreach (IUSManagerObserver obs in theObservers)
            {
                dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send,
                    new Action(() =>
                    {
                        obs.OnImageCaptured(rawImage);
                    }));
            }
        }
        public void OnUSMenuChanged(int menu)
        {
            Debug.Assert(dispatcher != null);
            foreach (IUSManagerObserver obs in theObservers)
            {
                dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        obs.OnProbeMenu(menu);
                    }));
            }
        }

        public USProbe GetCurrentProbe()
        {
            return currentProbe;
        }

        public USDriver GetCurrentDriver()
        {
            return currentDriver;
        }
    }
}

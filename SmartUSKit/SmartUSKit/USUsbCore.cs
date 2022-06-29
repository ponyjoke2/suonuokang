using CyUSB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SmartUSKit.SmartUSKit
{

    public class UsbCore : Object
    {
        //  handle context
        public static Dispatcher theContext;
        public static void prepareUsbCore(Dispatcher contx)
        {
            UsbCore.theContext = contx;
        }

        //
        USDriver theDriver;
        private static CyControlEndPoint theConnection;
        private static CyBulkEndPoint epIn;
        private static CyBulkEndPoint epOut;

        CyUSBDevice StreamDevice;

        DataFIFO theDataFIFO = null;
        public UsbCore(USDriver driver)
        {
            theDriver = driver;


            ConnectedUsbDevice connectedUsbDevice = USUsbManager.getInstance(null).getUsbConnection(driver.theProbe.probeType);
            Debug.WriteLine($"UsbCore :{connectedUsbDevice?.usbKey}");
            if (connectedUsbDevice != null)
            {
                StreamDevice?.Dispose();
                StreamDevice = null;

                StreamDevice = connectedUsbDevice.device;
                {
                    theConnection = StreamDevice.ControlEndPt;

                    foreach (CyUSBEndPoint ept in StreamDevice.EndPoints)
                    {
                        if (!ept.bIn && (ept.Address == 0x02))
                            epOut = ept as CyBulkEndPoint;
                        if (ept.bIn && (ept.Address == 0x86))
                            epIn = ept as CyBulkEndPoint;
                    }
                }
            }
        }

        private bool toReloadFPGA = false;
        private byte transSelIndex = 0;
        public void ReloadFPGA(byte transSel)
        {
            toReloadFPGA = true;
            transSelIndex = transSel;
        }

        public void Exit()
        {
            exit = true;
        }
        private bool exit = false;

        private int waitTime = 100;

        Stopwatch stopwatch = new Stopwatch();
        byte[] keyStat = new byte[4];

        public void Run(Object obj)
        {
            if (theDriver != null)
            {
                theDriver.OnCoreConnection(true);
            }
            Thread.Sleep(500);


            keyStateDown = false;

            int tick = 0;
            int failedCount = 0;
            //int tickTime = 0;
            stopwatch.Start();
            while (!exit)
            {
                try
                {
                    bool down = isRunButtonDown();
                    if (keyStateDown && !down)
                    {
                        //byte[] stat = new byte[4]; 
                        keyStat[0] = 0x5A;
                        keyStat[1] = (byte)0xA5;
                        keyStat[2] = 0x01; //  Pop UP
                        theDriver.OnCoreState(keyStat);
                    }
                    else if (!keyStateDown && down)
                    {
                        //byte[] stat = new byte[4];
                        keyStat[0] = 0x5A;
                        keyStat[1] = (byte)0xA5;
                        keyStat[2] = 0x02; //  Press Down
                        theDriver.OnCoreState(keyStat);
                    }
                    keyStateDown = down;

                    if ((!isLive) || (theDataFIFO == null) || theDataFIFO.isOverflow())
                    {
                        Thread.Sleep(5);
                        continue;
                    }

                    if (stopwatch.ElapsedMilliseconds < this.waitTime)
                    {
                        continue;
                    }

                    if (toReloadFPGA)
                    {
                        toReloadFPGA = false;
                        sendReloadFPGACmd(transSelIndex);

                        Thread.Sleep(800);

                        resetFpga();
                        Thread.Sleep(800);

                        enableUsb(true);

                        Thread.Sleep(100);
                    }

                    stopwatch.Restart();
                    tick++;
                    byte[] command = theDriver.OnCoreControl(tick);
                    int len = sendCommand(command);

                    if (len < 0)
                    {
                        Thread.Sleep(5);
                        len = sendCommand(command);
                        if (len < 0)
                        {
                            continue;
                        }
                    }

                    //  读取数据
                    byte[] rawData = recvData();
                    if (rawData == null)
                    {
                        failedCount++;
                        if (failedCount >= 20)
                        {
                            break;
                        }
                    }
                    else
                    {
                        failedCount = 0;
                        if (theDataFIFO != null)
                        {
                            theDataFIFO.pushData(rawData);
                        }
                    }

                    if (theDataFIFO != null)
                    {
                        theDataFIFO.run();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    break;
                }
            }
            if (theDriver != null)
            {
                suspendCore();
            }
        }

        public void suspendCore()
        {
            Exit();
            if (theDriver != null)
            {
                theDriver.OnCoreConnection(false);
            }
            theConnection = null;
            epIn = null;
            epOut = null;
            this.enableUsb(false);

            var usbMgr = USUsbManager.getInstance(null);
            if (usbMgr != null)
            {
                usbMgr?.disConnectUsbDevice();
            }

            StreamDevice?.Dispose();
            StreamDevice = null;
            if (theDataFIFO != null)
            {
                theDataFIFO?.Exit();
                theDataFIFO = null;
            }
        }

        public void resumeCore()
        {

            this.resetFpga();
            this.enableUsb(true);
            Thread pushThread = new Thread(new ParameterizedThreadStart(Run));
            pushThread.Priority = ThreadPriority.Highest;
            pushThread.Start();

            if (theDataFIFO == null)
            {
                theDataFIFO = new DataFIFO();
                theDataFIFO.theDriver = this.theDriver;
            }
        }



        //0xc1
        protected void resetFpga()
        {
            if (theConnection != null)
            {
                if (theConnection != null)
                {
                    theConnection.Target = CyConst.TGT_DEVICE;
                    theConnection.ReqType = CyConst.REQ_VENDOR;
                    theConnection.Direction = CyConst.DIR_TO_DEVICE;
                    theConnection.ReqCode = 0xC1; // Some vendor-specific request code
                    theConnection.Value = 0;
                    theConnection.Index = 0;
                    theConnection.TimeOut = 100;
                    int len = 0;
                    byte[] buf = new byte[1];
                    bool issuccess = theConnection.XferData(ref buf, ref len);
                }
            }
        }
        //0xc2
        protected void enableUsb(bool enable)
        {
            if (enable)
            {
                if (theConnection != null)
                {
                    theConnection.Target = CyConst.TGT_DEVICE;
                    theConnection.ReqType = CyConst.REQ_VENDOR;
                    theConnection.Direction = CyConst.DIR_TO_DEVICE;
                    theConnection.ReqCode = 0xC2; // Some vendor-specific request code
                    theConnection.Value = 1;
                    theConnection.Index = 0;
                    theConnection.TimeOut = 100;
                    int len = 0;
                    byte[] buf = new byte[1];
                    bool issuccess = theConnection.XferData(ref buf, ref len);
                }
            }
            else
            {
                if (theConnection != null)
                {
                    theConnection.Target = CyConst.TGT_DEVICE;
                    theConnection.ReqType = CyConst.REQ_VENDOR;
                    theConnection.Direction = CyConst.DIR_TO_DEVICE;
                    theConnection.ReqCode = 0xC2; // Some vendor-specific request code
                    theConnection.Value = 0;
                    theConnection.Index = 0;
                    theConnection.TimeOut = 100;
                    int len = 0;
                    byte[] buf = new byte[1];
                    bool issuccess = theConnection.XferData(ref buf, ref len);

                    theConnection = null;
                }
            }
        }

        byte[] isRunButtonDownStat = new byte[1];
        //0xC3
        protected bool isRunButtonDown()
        {
            //byte[] stat = new byte[1];
            int len = 1;
            bool issuccess = false;

            if (theConnection != null)
            {
                theConnection.Target = CyConst.TGT_DEVICE;
                theConnection.ReqType = CyConst.REQ_VENDOR;
                theConnection.Direction = CyConst.DIR_FROM_DEVICE;
                theConnection.ReqCode = 0xC3; // Some vendor-specific request code
                theConnection.Value = 0;
                theConnection.Index = 0;
                theConnection.TimeOut = 100;

                issuccess = theConnection.Read(ref isRunButtonDownStat, ref len);

                if (issuccess == false)
                {
                    throw new Exception("isRunButtonDown 中命令读取状态失败");
                }
            }
            if (isRunButtonDownStat[0] == 0)
            {
                return false;
            }
            return true;
        }
        protected int sendCommand(byte[] command)
        {
            int len = command.Length;
            if (epOut != null)
            {
                len = command.Length;
                epOut.TimeOut = 10;
                bool issuccess = epOut.XferData(ref command, ref len);
                if (!issuccess)
                {
                    throw new Exception("sendCommand:失败");
                }
            }
            return len;
        }
        internal int sendReloadFPGACmd(byte transSelIndex)
        {
            byte[] sendBuff = new byte[512];
            sendBuff[24] = transSelIndex;

            int len = sendBuff.Length;
            if (epOut != null)
            {
                // len = command.Length;
                byte[] buf = new byte[len];
                epOut.TimeOut = 10;
                bool issuccess = epOut.XferData(ref sendBuff, ref len);
            }

            return len;
        }
        private static Object readLineCountLocker = new Object();
        private int readLineCount = 128;

        byte[] rawData = new byte[128 * LINE_LENGTH];
        internal void setReadLineCount(int linecount)
        {
            lock (readLineCountLocker)
            {
                readLineCount = linecount;

                lock (readLineCountLocker)
                {
                    //如果长度变化了，则重新初始化数组
                    if (rawData.Length != readLineCount * LINE_LENGTH)
                    {
                        rawData = new byte[readLineCount * LINE_LENGTH];
                    }
                }
            }
        }

        internal void setWaitTime(int waitMS)
        {
            if (waitMS < 20)
            {
                waitMS = 20;
            }
            else if (waitMS > 200)
            {
                waitMS = 200;
            }
            this.waitTime = waitMS;
        }

        const int LINE_LENGTH = 512;

        private long time = 0;

        private bool keyStateDown = false;


        const int MAX_LENGTH = 16384;
        byte[] readBuff = new byte[MAX_LENGTH];
        protected byte[] recvData()
        {
            int destLineCount = readLineCount;
            int doneLineCount = 0;
            int readTime = 0;
            while (doneLineCount < destLineCount)
            {
                if (readTime > 20)
                {
                    break;
                }
                readTime++;
                int readLen = MAX_LENGTH;
                if (epIn != null)
                {
                    {
                        //byte[] buf = new byte[len];
                        epIn.TimeOut = 100;
                        bool issuccess = epIn.XferData(ref readBuff, ref readLen);
                    }
                }
                if (readLen < 0)
                {
                    continue;
                }
                if (readLen != MAX_LENGTH)
                {
                    break;
                }
                //bool prevValidLine = false;
                for (int l = 0; l < MAX_LENGTH / LINE_LENGTH; l++)
                {
                    bool isValidLine = false;
                    for (int i = 0; i < 256; i += 64)
                    {
                        if (readBuff[l * 512 + i] != (byte)0xAA || readBuff[l * 512 + i + 1] != (byte)0x55)
                        {
                            isValidLine = true;
                            break;
                        }
                    }
                    if (isValidLine)
                    {
                        if (doneLineCount < destLineCount)
                        {
                            Array.Copy(readBuff, l * LINE_LENGTH, rawData, doneLineCount * LINE_LENGTH, LINE_LENGTH);
                        }
                        if (doneLineCount < 1 || doneLineCount > destLineCount - 2)
                        {
                            //Log.d("DRIVER", "----- LINE:" + doneLineCount);
                        }
                        doneLineCount++;
                        if (doneLineCount >= destLineCount)
                        {
                            if (epIn != null)
                            {
                                int len = 512;
                                epIn.TimeOut = 100;
                                bool issuccess = epIn.XferData(ref readBuff, ref len);
                            }
                            break;
                        }
                    }
                    else
                    {
                        //Log.d("DRIVER", "----- LINE: ----");
                    }
                }
            }
            if (doneLineCount < destLineCount)
            {
                return null;
            }
            return rawData;
        }

        private void clearFIFO()
        {
            byte[] buff = new byte[16 * 1024];

            if (epIn != null)
            {
                int len = 512;
                byte[] buf = new byte[len];
                epIn.TimeOut = 5;
                bool issuccess = epIn.XferData(ref buff, ref len);
            }
        }

        private bool isLive = false;
        public void Run(bool bLive)
        {
            isLive = bLive;
        }


        private class DataFIFO //extends Thread
        {
            public void Exit()
            {
                exit = true;
            }
            private bool exit = false;

            public USDriver theDriver = null;
            private List<byte[]> theFifo = new List<byte[]>();
            private int totalLength = 0;
            public bool isOverflow()
            {
                bool ret = false;
                lock (theFifo)
                {
                    if (totalLength > 200 * 512)
                    {
                        ret = true;
                    }
                }
                return ret;
            }
            public void pushData(byte[] data)
            {
                if (data != null)
                {
                    lock (theFifo)
                    {
                        theFifo.Add(data);
                        totalLength += data.Length;
                        //Log.d("FIFI", "LENGTH: " + totalLength);
                    }
                }
            }

            private byte[] popData()
            {
                byte[] data = null;
                lock (theFifo)
                {
                    if (theFifo.Count > 0)
                    {
                        data = theFifo.First();
                        theFifo.RemoveAt(0);
                        totalLength -= data.Length;
                        if (totalLength < 0)
                        {
                            totalLength = 0;
                        }
                    }
                }
                return data;
            }

            public void run()
            {
                byte[] data = popData();

                if (data == null)
                {

                }
                else
                {
                    theDriver.OnCoreData(data);
                    //stopwatch.Restart();
                }

            }
        }
    }
}

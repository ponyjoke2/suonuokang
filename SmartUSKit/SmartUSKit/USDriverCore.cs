using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.Threading;

namespace SmartUSKit.SmartUSKit
{
    interface IUSDriverCoreDelegate
    {
        void OnCoreConnection(bool isConn);
        void OnCoreData(byte[] data);
        void OnCoreState(byte[] stat);
        byte[] OnCoreControl(int tick);
    }

    internal class USDriverCore : IAsyncSocketDelegate
    {

        protected static AsyncSocket socketData;
        protected static AsyncSocket socketStat;

        //private System.Threading.Timer tickTimer;
        protected bool isResuming;
        protected int tickIndex;

        protected const string HOST = "192.168.1.1";
        protected const int PORT_DATA = 5002;
        protected const int PORT_STAT = 5003;

        /// <summary>
        /// 这个时间设置的长一些，多等待一些时间
        /// </summary>
        protected const int ConnectTimeout = 990;

        protected USDriver theDriver;

        protected USDriverCore()
        {
            theDriver = null;
        }
        public USDriverCore(USDriver driver)
        {
            Debug.Assert(driver != null);
            theDriver = driver;
        }

        public virtual String coreType()
        {
            return "DF";
        }

        //Thread ConectThread;
        /// <summary>
        /// 用来控制OnTick所在的线程
        /// </summary>
        bool ConectThreadEnable = false;
        public virtual void ResumeCore()
        {
            // tickTimer = new System.Threading.Timer(new System.Threading.TimerCallback(OnTick), this, 100, 200);
            isResuming = true;

            ConectThreadEnable = true;
            Task.Run(() =>
            {
                while (ConectThreadEnable)
                {
                    Connect();
                    Thread.Sleep(2000);
                }

            });
            Task.Run(() =>
            {
                while (ConectThreadEnable)
                {
                    OnTick(null);
                    Thread.Sleep(200);
                }
            });


            Debug.WriteLine("ResumeCore");
        }

        public virtual void SuspendCore()
        {
            Debug.WriteLine("SuspendCore");
            isResuming = false;
            ConectThreadEnable = false;

            Disconnect();
        }

        internal virtual void OnTick(object obj)
        {
            if (!isResuming)
            {
                return;
            }

            tickIndex++;
            if (!IsConnected())
            {
                //  连接尝试
                if (tickIndex % 15 == 1)
                {
                    //这个地方应该阻塞
                    //单独拿出去连接
                    //Connect();
                }
            }
            else
            {

                //  数据应答
                ResponseData();
                //  发送控制命令
                if (theDriver != null)
                {
                    byte[] ctrlData = theDriver.OnCoreControl(tickIndex);
                    if (ctrlData != null)
                    {
                        try
                        {
                            socketStat?.WriteData(ctrlData, 1000);
                        }
                        catch (Exception)
                        {

                        }
                    }

                }
            }
        }

        private class pushFIFO
        {
            public USDriver theDriver = null;
            private List<byte[]> dataList = new List<byte[]>();
            private int dataBufferLength = 0;
            private List<byte[]> statList = new List<byte[]>();

            public void pushData(byte[] data)
            {
                bool supress = false;
                lock (dataList)
                {
                    dataList.Add(data);
                    dataBufferLength += data.Length;
                    if (dataBufferLength > 160 * 525)
                    {
                        supress = true;
                    }
                }
                if (supress)
                {
                    //USLog.LogInfo("数据大于一帧"+dataBufferLength.ToString());
                    Thread.Sleep(10);
                }
            }

            public void pushStat(byte[] stat)
            {
                lock (statList)
                {
                    statList.Add(stat);
                }
            }

            private bool toQuit = false;
            public void Quite()
            {
                dataList.Clear();
                statList.Clear();
                toQuit = true;
            }

            public void run(Object obj)
            {
                while (!toQuit)
                {
                    while ((dataList.Count > 0)
                                  || (statList.Count > 0)
                                  )
                    {
                        byte[] sendData = null;
                        lock (dataList)
                        {
                            if (dataList.Count > 0)
                            {
                                sendData = dataList[0];
                                dataList.RemoveAt(0);
                                dataBufferLength -= sendData.Length;
                            }
                        }
                        if (sendData != null && theDriver != null)
                        {
                            theDriver.OnCoreData(sendData);
                        }

                        byte[] sendStat = null;
                        lock (statList)
                        {
                            if (statList.Count > 0)
                            {
                                sendStat = statList[0];
                                statList.RemoveAt(0);
                            }
                        }
                        if (sendStat != null && theDriver != null)
                        {
                            theDriver.OnCoreState(sendStat);
                        }
                    }
                    Thread.Sleep(5);
                }
            }
        }

        private pushFIFO thePushFIFO = null;

        public virtual void Connect()
        {

            if (socketData == null)
            {
                socketData = new AsyncSocket(this);
                socketData.tag = 2;
            }
            if (socketStat == null)
            {
                socketStat = new AsyncSocket(this);
                socketStat.tag = 3;
            }

            //注册driverCore
            socketData?.RegisteDriverCore(this);
            socketData?.ReadData(-1);
            socketStat?.RegisteDriverCore(this);
            socketStat?.ReadData(2000);

            #region 直接检查端口的状态，以此来判断端口是否被连接成功
            System.Net.NetworkInformation.IPGlobalProperties ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

            System.Net.NetworkInformation.TcpConnectionInformation[] ipsTCPConnections = ipGlobalProperties.GetActiveTcpConnections();

            bool connected5002 = false;
            bool connected5003 = false;

            foreach (var currentConnection in ipsTCPConnections)
            {
                if (currentConnection.RemoteEndPoint.ToString() == $"{HOST}:{PORT_DATA}")
                {
                    //Debug.WriteLine($"端口：{PORT_DATA}状态：{currentConnection.State}");
                    if (currentConnection.State == System.Net.NetworkInformation.TcpState.Established)
                    {
                        connected5002 = true;
                    }
                }
                if (currentConnection.RemoteEndPoint.ToString() == $"{HOST}:{PORT_STAT}")
                {
                    //Debug.WriteLine($"端口：{PORT_STAT}状态：{currentConnection.State}");
                    if (currentConnection.State == System.Net.NetworkInformation.TcpState.Established)
                    {
                        connected5003 = true;
                    }
                }
            }
            if (connected5002 && connected5003)
            {
                return;
            }
            Debug.WriteLine($"套接字连接不成功：5002：{connected5002 }，5003：{ connected5003}");
            #endregion

            if (socketData == null)
            {
                socketData = new AsyncSocket(this);
                socketData.tag = 2;
            }
            if (!socketData.IsConnected())
            {
                socketData?.ConnectToHost(HOST, PORT_DATA, ConnectTimeout);
            }

            if (socketStat == null)
            {
                socketStat = new AsyncSocket(this);
                socketStat.tag = 3;
            }
            if (!socketStat.IsConnected())
            {
                socketStat?.ConnectToHost(HOST, PORT_STAT, ConnectTimeout);
            }
        }

        private void ResponseData()
        {
            byte[] block = new byte[4];
            socketData?.WriteData(block, 1000);
        }

        public virtual void Disconnect()
        {
            try
            {
                if (socketData != null)
                {
                    socketData?.Disconnect();
                    socketData = null;
                }
                if (socketStat != null)
                {
                    socketStat?.Disconnect();
                    socketStat = null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public virtual bool IsConnected()
        {
            if (socketData != null && socketStat != null)
            {
                if (socketData.IsSocketConnected && socketStat.IsSocketConnected)
                {
                    return true;
                }
            }
            return false;


            System.Net.NetworkInformation.IPGlobalProperties ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

            System.Net.NetworkInformation.TcpConnectionInformation[] ipsTCPConnections = ipGlobalProperties.GetActiveTcpConnections();

            bool connected5002 = false;
            bool connected5003 = false;

            foreach (var currentConnection in ipsTCPConnections)
            {
                if (currentConnection.RemoteEndPoint.ToString() == $"{HOST}:{PORT_DATA}")
                {
                    if (currentConnection.State == System.Net.NetworkInformation.TcpState.Established)
                    {
                        connected5002 = true;
                    }
                }
                if (currentConnection.RemoteEndPoint.ToString() == $"{HOST}:{PORT_STAT}")
                {
                    if (currentConnection.State == System.Net.NetworkInformation.TcpState.Established)
                    {
                        connected5003 = true;
                    }
                }
            }
            if (connected5002 && connected5003)
            {
                return true;
            }
            return false;
        }

        public virtual void OnSocketConnected(AsyncSocket sock)
        {
            if (socketStat == null || socketData == null)
            {
                return;
            }
            if (socketStat.IsConnected() && socketData.IsConnected())
            {
                //  发送事件
                if (theDriver != null)
                {
                    Debug.WriteLine($"OnSocketConnected:true");
                    theDriver?.OnCoreConnection(true);
                }

                //  启动数据读取
                if (socketData != null)
                {
                    socketData?.ReadData(-1);
                }
                if (socketStat != null)
                {
                    socketStat?.ReadData(2000);
                }
            }


            if (IsConnected())
            {
                if (thePushFIFO == null)
                {
                    thePushFIFO = new pushFIFO();
                    thePushFIFO.theDriver = this.theDriver;
                    //Thread pushThread = new Thread(new ParameterizedThreadStart(thePushFIFO.run));
                    //pushThread.Priority = ThreadPriority.Normal;
                    //pushThread.Start();

                    //Task.Factory.StartNew(thePushFIFO.run,null);
                    Task.Run(() =>
                    {
                        thePushFIFO.run(null);
                    });
                }
            }
        }

        public virtual void OnSocketDisconnected(AsyncSocket sock)
        {
            Debug.WriteLine($"OnSocketDisconnected: {sock.thePort}");
            if (!IsConnected())
            {
                // 发送消息
                if (theDriver != null)
                {
                    theDriver?.OnCoreConnection(false);
                }
            }

            if (!IsConnected())
            {
                if (thePushFIFO != null)
                {
                    thePushFIFO.Quite();
                    thePushFIFO = null;
                }
            }
        }

        public virtual void OnSocketDidReadData(AsyncSocket sock, byte[] data, int length)
        {
            if (sock.thePort == PORT_DATA)
            {
                if (IsConnected())
                {
                    if (theDriver != null)
                    {
                        byte[] outdata = new byte[length];
                        System.Array.Copy(data, outdata, length);
                        //theDriver.OnCoreData(outdata);


                        if (thePushFIFO == null)
                        {
                            thePushFIFO = new pushFIFO();
                            thePushFIFO.theDriver = this.theDriver;
                            Task.Run(() =>
                            {
                                thePushFIFO.run(null);
                            });
                        }

                        if (thePushFIFO != null)
                        {
                            thePushFIFO.pushData(outdata);
                        }
                    }
                }
                sock.ReadData(-1);
            }
            else if (sock.thePort == PORT_STAT)
            {
                if (IsConnected())
                {
                    if (theDriver != null)
                    {
                        //theDriver.OnCoreState(data);

                        byte[] outdata = new byte[length];
                        for (int i = 0; i < length; i++)
                        {
                            outdata[i] = data[i];
                        }
                        if (thePushFIFO != null)
                        {
                            thePushFIFO.pushStat(outdata);
                        }
                    }
                }
                sock?.ReadData(2000);
            }
        }
    }
}

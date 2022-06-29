using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace SmartUSKit.SmartUSKit
{
    interface IAsyncSocketDelegate
    {
        void OnSocketConnected(AsyncSocket sock);
        void OnSocketDisconnected(AsyncSocket sock);
        void OnSocketDidReadData(AsyncSocket sock, byte[] data, int length);
    }
    internal class AsyncSocket
    {
        public int thePort;
        public string theHost;
        public int tag;
        protected int theConnectTimeOut;
        protected Socket theSocket;

        USDriverCore theDriverCore;


        public AsyncSocket(USDriverCore core)
        {
            theDriverCore = core;
        }

        public void RegisteDriverCore(USDriverCore core)
        {
            if (theDriverCore != core)
            {
                theDriverCore = core;
            }
        }

        /// <summary>
        /// 用来指示socket是否已经连接的
        /// </summary>
        public bool IsSocketConnected { get; set; } = false;
        public bool IsConnected()
        {
            return IsSocketConnected;

            System.Net.NetworkInformation.IPGlobalProperties ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

            System.Net.NetworkInformation.TcpConnectionInformation[] ipsTCPConnections = ipGlobalProperties.GetActiveTcpConnections();

            bool isSocketConnected = false;

            foreach (var currentConnection in ipsTCPConnections)
            {
                if (currentConnection.RemoteEndPoint.ToString() == $"{theHost}:{thePort}")
                {
                    if (currentConnection.State == System.Net.NetworkInformation.TcpState.Established)
                    {
                        isSocketConnected = true;
                    }
                }
            }
            if (IsSocketConnected != IsSocketConnected)
            {
                Debug.WriteLine($"自定义连接：{IsSocketConnected},系统获取的socket连接状态：{isSocketConnected}");
            }

            //Debug.Assert(isSocketConnected == IsSocketConnected);

            return IsSocketConnected;
            if (theSocket != null && IsSocketConnected)
            {
                return true;
            }
            return false;
        }


        ManualResetEvent connectResetEvent = new ManualResetEvent(false);

        public void ConnectToHost(string host, int port, int timeout)
        {
            theHost = host;
            thePort = port;
            theConnectTimeOut = timeout;

            connectResetEvent.Reset();
            Task.Run(() =>
            {
                this.Run();
            });
            connectResetEvent.WaitOne();
        }

        int read0bytes = 0;
        byte[] buffer = new byte[4096];

        int SendFailTimes = 0;

        protected void Run()
        {
            try
            {
                ResetSocket();

                theSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IAsyncResult connResult = theSocket.BeginConnect(theHost, thePort, null, null);
                Debug.WriteLine($"开始连接：{theHost}:{thePort},线程ID：{Thread.CurrentThread.ManagedThreadId}");
                IsSocketConnected = false;
                connResult.AsyncWaitHandle.WaitOne(theConnectTimeOut, true);
                if (!connResult.IsCompleted)
                {
                    connectResetEvent.Set();
                    //  连接不成功
                    Debug.WriteLine($"连接失败：{theHost}:{thePort},线程ID：{Thread.CurrentThread.ManagedThreadId}");


                    #region 连接失败时，在这里输出一下当前端口的状态
                    System.Net.NetworkInformation.IPGlobalProperties ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();

                    System.Net.NetworkInformation.TcpConnectionInformation[] ipsTCPConnections = ipGlobalProperties.GetActiveTcpConnections();
                    foreach (var currentConnection in ipsTCPConnections)
                    {
                        if (currentConnection.RemoteEndPoint.ToString() == $"{theHost}:{thePort}")
                        {
                            Debug.WriteLine($"连接到：{theHost}：{thePort}失败,此时的端口状态：{currentConnection.State.ToString()}");
                            //USLog.LogInfoForModifySN($"连接到：{theHost}：{thePort}失败,此时的端口状态：{currentConnection.State.ToString()}");
                        }
                    }
                    #endregion

                    ResetSocket();
                    return;
                }
                connectResetEvent.Set();
                IsSocketConnected = true;
                Debug.WriteLine($"连接成功：{theHost}:{thePort},线程ID：{Thread.CurrentThread.ManagedThreadId}");
                //  连接成功
                if (theDriverCore != null)
                {
                    theDriverCore.OnSocketConnected(this);
                }
                //while (theSocket != null && theSocket.Connected)
                while (theSocket != null)
                {
                    if (outBuff != null && outBuffLength > 0)
                    {
                        byte[] outData = null;
                        lock (this)
                        {
                            outData = new byte[outBuffLength];
                            for (int i = 0; i < outData.Length; i++)
                            {
                                outData[i] = outBuff[i];
                            }
                            outBuffLength = 0;
                        }

                        if (outData != null)
                        {
                            try
                            {
                                //theSocket.SendTimeout = 2000;
                                theSocket.Send(outData);
                                SendFailTimes = 0;
                            }
                            catch (Exception ex)
                            {
                                SendFailTimes++;
                                Thread.Sleep(5);
                                if (SendFailTimes >= 30)
                                {
                                    SendFailTimes = 0;
                                    Debug.WriteLine("Send Failed." + ex.ToString());
                                    Disconnect();
                                    break;
                                }
                            }
                        }
                    }
                    if (EnableRead)
                    {
                        try
                        {
                            if (theSocket.Available <= 0)
                            {
                                Thread.Sleep(1);
                                continue;
                            }

                            int count = theSocket.Receive(buffer);
                            if (count > 0)
                            {
                                read0bytes = 0;
                                if (theDriverCore != null)
                                {
                                    theDriverCore.OnSocketDidReadData(this, buffer, count);
                                }
                            }
                            else
                            {
                                read0bytes++;
                                Debug.WriteLine($"Read 0 bytes. {thePort}");
                                Thread.Sleep(1);
                                if (read0bytes >= 4)
                                {
                                    Disconnect();
                                    break;
                                }
                            }

                        }
                        catch (SocketException error)
                        {
                            Debug.WriteLine($"SocketException:{error.ToString()}");
                            Debug.WriteLine($"SocketErrorCode:{error.SocketErrorCode}");
                            Disconnect();
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"{thePort} Read Failed:" + ex.ToString());
                            Disconnect();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Run Failed: " + ex.ToString());
            }
            finally
            {
                IsSocketConnected = false;
                ResetSocket();
                if (theDriverCore != null)
                {
                    theDriverCore.OnSocketDisconnected(this);
                }
            }
        }
        void ResetSocket()
        {
            try
            {
                if (theSocket != null)
                {
                    try
                    {
                        theSocket?.Shutdown(SocketShutdown.Both);
                    }
                    catch (Exception bothEx)
                    {
                        Debug.WriteLine($"theSocket.Shutdown:{bothEx.ToString()}");
                    }

                    theSocket?.Close();
                    theSocket = null;
                }

                IsSocketConnected = false;
            }
            catch (Exception ee)
            {
                Debug.WriteLine(ee.ToString());
            }

        }
        public void Disconnect()
        {
            ResetSocket();
        }

        protected const int MAX_OUT_BUFF = 8192;
        protected byte[] outBuff = new byte[MAX_OUT_BUFF];
        protected int outBuffLength = 0;
        public void WriteData(byte[] data, int timeout)
        {
            lock (this)
            {
                if (data.Length + outBuffLength < MAX_OUT_BUFF)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        outBuff[outBuffLength + i] = data[i];
                    }
                    outBuffLength += data.Length;
                }
            }
        }

        protected bool EnableRead = false;
        protected int theReadTimeout = 0;
        public void ReadData(int timeout)
        {
            if (theSocket != null)
            {
                if (theReadTimeout != timeout)
                {
                    theReadTimeout = timeout;
                    if (theSocket != null)
                    {
                        theSocket.ReceiveTimeout = theReadTimeout;
                    }
                }

                EnableRead = true;
            }
        }
    }
}

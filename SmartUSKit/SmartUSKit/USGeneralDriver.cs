using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using SmartUSKit;
using SmartUSKit.Enums;

namespace SmartUSKit.SmartUSKit
{
    public class USGeneralDriver : USDriver
    {
        internal USDriverCore theDriverCore;

        //  收到的状态
        protected bool stateLive;
        protected int stateGain;
        protected int stateZoom;

        //  发送的控制
        protected bool ctrlLive;
        protected int ctrlLiveTick;
        protected int ctrlGain;
        protected int ctrlZoom;

        //  WiFi信道
        protected int newWiFiChannel;

        public USGeneralDriver()
        {

        }
        public override void Activate()
        {
            if (theDriverCore == null)
            {
                theDriverCore = new USDriverCore(this);
            }
            theDriverCore?.ResumeCore();
        }

        protected int GetDepthCount()
        {
            int maxZoom = 3;
            if (this != null)
            {
                if (this.theProbe != null)
                {
                    if (this.theProbe.imagingParameter != null)
                    {
                        if (this.theProbe.imagingParameter.namicDepth != null)
                        {
                            maxZoom = this.theProbe.imagingParameter.namicDepth.Length - 1;
                        }
                    }
                }
            }
            return 3;
        }

        //
        //  连接状态
        //
        public override bool IsConnected()
        {
            return false;
        }
        public override void Close()
        {
            if (theDriverCore != null)
            {
                theDriverCore.SuspendCore();
            }
            base.Close();
        }

        public override void SetWifiChannel(int channel)
        {
            if (this.theProbe == null)
            {
                return;
            }
            if (channel > 0 && channel <= 13)
            {
                newWiFiChannel = channel;
            }
            else if (this.theProbe.Is5GProbe())
            {
                if (channel == 40 || channel == 44 || channel == 48 ||
                        channel == 149 || channel == 153 || channel == 157 || channel == 161 || channel == 165
                        )
                {
                    newWiFiChannel = channel;
                }
            }
        }

        //
        //  触发事件
        //
        public override void ToggleLive()
        {
            SetLive(!stateLive);
        }

        public override void SetLive(bool live)
        {
            ctrlLive = live;
            ctrlLiveTick = 8;
        }
        public override bool IsLive()
        {
            return stateLive;
        }

       
        public override void DecGain()
        {
            if (ctrlGain > 30)
            {
                ctrlGain--;
            }
            else
            {
                ctrlGain = 30;
            }
            theProbe.SetDefaultGain(ctrlGain);
        }

       

        public override int GetGain()
        {
            return stateGain;
        }

        public override void IncZoom()
        {
            int maxZoom = GetDepthCount();

            int zoom = stateZoom;
            if (zoom < maxZoom)
            {
                zoom++;
            }
            SetZoom(zoom);
        }

        public override void DecZoom()
        {
            int zoom = stateZoom;
            if (zoom > 0)
            {
                zoom--;
            }
            SetZoom(zoom);
        }

        public override void SetZoom(int zoom)
        {
            if (zoom > 3)
            {
                zoom = 3;
            }
            else if (zoom < 0)
            {
                zoom = 0;
            }
            //stateZoom = zoom;
            ctrlZoom = zoom;
            zoomTick = 8;
            theProbe.SetDefaultZoom(ctrlZoom);
        }

        public override int GetZoom()
        {
            return stateZoom;
        }


        public override void OnCoreConnection(bool isConn)
        {
            theManager?.OnProbeConnection(isConn);

            if (isConn)
            {
                int gain = theProbe.GetDefaultGain();
                int zoom = theProbe.GetDefaultZoom();
                SetGain(gain);
                SetZoom(zoom);
                SetLive(false);
            }
        }

        public virtual bool JitFreeze()
        {
            if (!stateLive || (ctrlLiveTick > 0 && !ctrlLive))
            {
                return true;
            }
            return false;
        }

        public override void OnCoreData(byte[] data)
        {
            if (!JitFreeze())
            {
                if (thePackager == null)
                {
                    return;
                }

                if (thePackager.Package(data) >= USPackager.PACKAGE_SUCC)
                {
                    USRawImage rawImage = new USRawImage();
                    rawImage.rawData = thePackager.RawData();
                    rawImage.timeCap = thePackager.CaptureTime();
                    rawImage.probeCap = theProbe;
                    rawImage.gain = stateGain;
                    rawImage.zoom = stateZoom;

                    if (currentMode == ProbeMode.MODE_BM)
                    {
                        rawImage = USBMImage.MakeBMImage(rawImage, GetBMLine());
                    }

                    //取消下面的注册，利用更合适的方式做
                    //if (theDelegate != null)
                    //{
                    //    IUSDriverDelegate deleg = theDelegate as IUSDriverDelegate;
                    //    deleg.OnUSImage(rawImage);
                    //}
                    //更合适的方式
                    theManager?.OnUSImage(rawImage);
                }
            }
        }

        public override void OnCoreState(byte[] stat)
        {
            //  解析数据包
            if (stat.Length >= 4)
            {
                if (stat[0] == 0x5A && stat[1] == 0xA5)
                {
                    bool prevStateLive = stateLive;
                    if ((stat[2] & 0x80) != 0x00)
                    {
                        stateLive = true;
                    }
                    else
                    {
                        stateLive = false;
                    }

                    stateZoom = stat[2] & 0x03;

                    byte gain = (byte)(stat[3] & 0x7F);
                    stateGain = (int)((float)gain / 127.0 * (105.0 - 30.0) + 30.0 + 0.5);

                    if (prevStateLive != stateLive)
                    {
                        theManager?.OnUSLive(stateLive);
                    }
                }
            }
        }

        public override byte[] OnCoreControl(int tick)
        {
            byte[] wifiBlock = MakeWifiChannelCommand(tick);
            byte[] ctrlBlock = MakeGeneralControlCommand(tick);
            return LinkByteArray(wifiBlock, ctrlBlock);
        }

        protected virtual byte[] MakeWifiChannelCommand(int tick)
        {
            return df_makeWifiChannelCommand(tick);
        }

        private byte[] df_makeWifiChannelCommand(int tick)
        {
            if (newWiFiChannel > 0)
            {
                byte[] sendData = new byte[4];
                sendData[0] = (byte)0x5c;
                sendData[1] = (byte)0xc5;
                sendData[2] = (byte)newWiFiChannel;
                sendData[3] = (byte)newWiFiChannel;

                //  5G信道，使用默认国家码 0
                if (theProbe != null && theProbe.Is5GProbe())
                {
                    sendData[3] = 0;
                }

                newWiFiChannel = 0;
                return sendData;
            }
            return null;
        }


        public virtual byte[] MakeGeneralControlCommand(int tick)
        {
            return df_makeGeneralControlCommand(tick);

        }
        protected int zoomTick;

        private byte[] df_makeGeneralControlCommand(int tick)
        {
            byte[] ctrlblock = new byte[4];
            ctrlblock[0] = (byte)(0x5a & 0xFF);
            ctrlblock[1] = (byte)(0xa5 & 0xFF);
            ctrlblock[2] = 0x00;
            ctrlblock[3] = 0x00;

            if (ctrlLiveTick > 0)
            {
                ctrlLiveTick--;
            }
            else
            {
                ctrlLive = stateLive;
                ctrlblock[2] |= 0x20 & 0xFF;
            }
            if (ctrlLive)
            {
                ctrlblock[2] |= 0x80 & 0xFF;
            }
            ctrlblock[2] |= (byte)(ctrlZoom & 0x03);

            int gain = (int)(((float)ctrlGain - 30.0) / (105.0 - 30.0) * 127.0 + 0.5);
            ctrlblock[3] |= (byte)(gain & 0x7F);

            return ctrlblock;
        }
        public override void ExaminationSetting(JObject settings)
        {
            int gain = 0;
            try
            {
                if (settings.ContainsKey("gain"))
                {
                    gain = int.Parse(settings["gain"].ToString());
                    this.SetGain(gain);
                }
                if (settings.ContainsKey("zoom"))
                {
                    int focuspos = int.Parse(settings["zoom"].ToString());
                    SetZoom(focuspos);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                //USLog.LogInfo(ex.ToString());
            }
        }

        public virtual void onNTCoreConnection(bool isConn)
        {
            if (theManager != null)
            {
                theManager.OnProbeConnection(isConn);
            }


            if (isConn)
            {
                int zoom = theProbe.GetDefaultZoom();
                int gain = theProbe.GetDefaultGain();
                SetZoom(zoom);
                SetGain(gain);
            }
        }

        protected bool updatePowerSave = false;

        private int APCode;
    }

}

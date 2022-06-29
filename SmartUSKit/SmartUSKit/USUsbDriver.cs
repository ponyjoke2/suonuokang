using SmartUSKit.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class USUsbDriver : USEnhanceDriver
    {
        private static object mylock = new object();
        private int normalLineCount = 128;
        private int suffixLineCount = 32;


        public void setLineCount(int normalLC, int suffixLC)
        {
            normalLineCount = normalLC;
            suffixLineCount = suffixLC;
        }

        protected UsbCore theUsbCore = null;// new UsbCore(this);
                                            //@Override
        public override void Connect()
        {
            //super.connect();
            if (theUsbCore != null)
            {
                theUsbCore.resumeCore();
            }
        }
        public USUsbProbe theUsbProbe()
        {
            return (USUsbProbe)this.theProbe;
        }
        ~USUsbDriver()
        {

        }
        //@Override
        public override void Disconnect()
        {
            //super.disconnect();
            if (theUsbCore != null)
            {
                theUsbCore.suspendCore();
            }
        }
        public override void Close()
        {
            base.Close();
            if (theUsbCore != null)
            {
                theUsbCore.suspendCore();
            }
        }
        //@Override
        public override bool IsConnected()
        {
            //return super.isConnected();
            if (theUsbCore != null)
            {
                return true;
            }
            return false;
        }

        //@Override
        public override void Activate()
        {
            if (theUsbCore == null)
            {
                theUsbCore = new UsbCore(this);
                theUsbCore.resumeCore();
            }
        }

        protected bool isLive = false;
        //@Override
        public override void SetLive(bool live)
        {
            isLive = live;

            if (theUsbCore != null)
            {
                theUsbCore.Run(isLive);
            }

            this.OnCoreState(null);
        }

        //@Override
        public override void ToggleLive()
        {
            isLive = !isLive;
            theUsbCore.Run(isLive);

            this.OnCoreState(null);
        }

        //@Override
        public override bool IsLive()
        {
            return isLive;
        }

        public override void SetZoom(int zoom)
        {
            int maxZoom = GetDepthCount();
            if (zoom > maxZoom)
            {
                zoom = maxZoom;
            }
            if (zoom <= 0)
            {
                zoom = 0;
            }
            base.SetZoom(zoom);
        }

        //@Override
        public override int GetZoom()
        {
            return ctrlZoom;
        }

        //@Override
        public override void IncZoom()
        {
            SetZoom(ctrlZoom + 1);
        }

        //@Override
        public override void DecZoom()
        {
            SetZoom(ctrlZoom - 1);
        }

        private int gain = 80;
        //@Override
        public override void SetGain(int gain)
        {
            if (gain >= 30 && gain <= 105)
            {
                this.gain = gain;
            }
            base.SetGain(this.gain);
            //Log.d("DRIVER", "SET Gain");
        }

        //@Override
        public override int GetGain()
        {
            return this.gain;
        }

        //@Override
        public override void IncGain()
        {
            if (gain < 105)
            {
                gain++;
            }
            SetGain(gain);
            //Log.d("DRIVER", "inc Gain");
        }

        //@Override
        public override void DecGain()
        {
            if (gain > 30)
            {
                gain--;
            }
            SetGain(gain);
            //Log.d("DRIVER", "dec Gain");
        }

        private byte[] vGainLine = new byte[128];
        //@Override
        public override void SetVGain(int value, int index)
        {
            base.SetVGain(value, index);

            //  生成128位的VGain列表
            int nIndex = 0;
            float[] vGainCurve = new float[128];
            for (int seg = 0; seg < 7; seg++)
            {
                float fStart = (float)theVGain.nVGain[seg];
                float fEnd = (float)theVGain.nVGain[seg + 1];
                float fStep = (fEnd - fStart) / 18.0f;
                for (int c = 0; c < 18; c++)
                {
                    float fCur = fStart + fStep * (float)c;
                    if (fCur > 255.0f)
                    {
                        fCur = 255.0f;
                    }
                    else if (fCur < 0.0)
                    {
                        fCur = 0.0f;
                    }
                    vGainCurve[nIndex] = fCur;
                    nIndex++;
                }
            }
            while (nIndex < 128)
            {
                vGainCurve[nIndex] = theVGain.nVGain[7];
                nIndex++;
            }
            float[] tmp = new float[128];
            for (int t = 0; t < 2; t++)
            {
                for (int i = 0; i < 128; i++)
                {
                    tmp[i] = vGainCurve[i];
                }
                for (int i = 1; i < 128 - 1; i++)
                {
                    vGainCurve[i] = (tmp[i - 1] + tmp[i] + tmp[i + 1]) / 3.0f;
                }
            }
            for (int i = 0; i < 128; i++)
            {
                this.vGainLine[i] = (byte)(((int)(vGainCurve[i])) & 0xFF);
            }

        }

        //@Override
        public override void OnCoreConnection(bool isConn)
        {
            //Log.d("DRIVER", "----------------------onCoreConnection: " + isConn);
            base.OnCoreConnection(isConn);
        }

        private int bwSteerAngle = 0;
        private int bwSteerAngleNext = 0;
        //@Override
        public override byte[] OnCoreControl(int tick)
        {
            //
            //  发送数据包的长度信息
            //
            ProbeMode mode = this.GetMode();
            if (mode == ProbeMode.MODE_B || mode == ProbeMode.MODE_BM)
            {
                theUsbCore.setReadLineCount(this.normalLineCount);
                USUsbProbe usbProbe = (USUsbProbe)theProbe;
                int waitMS = usbProbe.getWaitTime(mode, this.GetHarmonic(), this);
                theUsbCore.setWaitTime(waitMS);
            }
            else
            {
                theUsbCore.setReadLineCount(this.normalLineCount + this.suffixLineCount);
                USUsbProbe usbProbe = (USUsbProbe)theProbe;
                int waitMS = usbProbe.getWaitTime(mode, this.GetHarmonic(), this);
                theUsbCore.setWaitTime(waitMS);
            }

            //
            //  发送控制信息
            //
            byte[] command = new byte[512];

            command[0] = (byte)0x55;
            command[1] = (byte)0xAA;

            //  Live / Freeze
            if (this.IsLive())
            {
                command[2] = 0x01;          //  LIVE/FREEZE
            }
            else
            {
                command[2] = 0x00;
            }
            //  Zoom
            command[3] = (byte)(GetZoom() & 0xFF);    //  ZOOM

            //  Gain
            int gain_user = this.GetGain();
            gain_user = Math.Min(gain_user, 105);
            gain_user = Math.Max(gain_user, 30);
            int gain_fpga = (int)((float)(gain_user - 30) / (105.0f - 30.0f) * 127.0f + 0.5f);
            gain_fpga = Math.Min(gain_fpga, 127);
            gain_fpga = Math.Max(gain_fpga, 0);
            command[4] = (byte)(gain_fpga & 0xFF);

            command[5] = 0;
            command[6] = 0;
            command[7] = 0;

            command[8] = (byte)0x55;
            command[9] = (byte)0xAA;
            command[10] = this.theEnhanceParameters.getUcFrequency();
            command[11] = 0;
            command[12] = (byte)(this.theEnhanceParameters.getnDynamicRange() & 0xFF);
            command[13] = this.theEnhanceParameters.getUcFrameMean();
            command[14] = this.theEnhanceParameters.getUcCompound();
            command[15] = this.theEnhanceParameters.getUcAcousticalPower();
            command[16] = this.theEnhanceParameters.getUcHarmonic();
            command[17] = this.theEnhanceParameters.getUcFocusPos();
            command[18] = this.theEnhanceParameters.getUcFocusCnt();


            if (this.theEnhanceParameters.getUcCompound() != 0x00)
            {
                int index = tick % 3;
                command[19] = theUsbProbe().getFPGAAngleIndex(index);

                this.bwSteerAngle = this.bwSteerAngleNext;
                this.bwSteerAngleNext = ((USEnhanceProbe)theProbe).getCompoundSteer(index);
            }
            else
            {
                command[19] = 0;
                this.bwSteerAngle = 0;
            }

            command[20] = 0;
            command[21] = 0;
            command[22] = 0;
            command[23] = 0;
            //  current Mode
            byte mode_fpga = 0x00;   //  MODE_B
            if (this.GetMode() == ProbeMode.MODE_B || this.currentMode == ProbeMode.MODE_BM)
            {
                mode_fpga = 0x00;
            }
            else
            {
                mode_fpga = 0x01;
            }
            command[60] = mode_fpga;
            command[61] = mode_fpga;
            command[62] = mode_fpga;
            command[63] = mode_fpga;

            command[32] = (byte)0x55;
            command[33] = (byte)0xAA;

            command[64] = (byte)0x55;
            command[65] = (byte)0xAA;

            // VGain
            for (int i = 0; i < 128; i++)
            {
                command[128 + i] = this.vGainLine[i];
            }
            return command;
        }
        protected bool lockPopup = false;
        //@Override
        public override void OnCoreState(byte[] stat)
        {
            if (stat != null)
            {
                const byte popup = 1;
                const byte pressdown = 2;
                int action = 0; //  0 none, 1 pop up, 2 press down
                if ((stat[0] == (byte)0x5A)
                        && (stat[1] == (byte)0xA5))
                {
                    action = stat[2];
                }
                if (action == popup && !isLive)
                {
                    if (lockPopup)
                    {
                        lockPopup = false;
                    }
                    else
                    {
                        this.SetLive(true);
                    }
                }
                else if (action == pressdown && isLive)
                {
                    lockPopup = true;
                    this.SetLive(false);
                }
            }

            if (theManager != null)
            {
                theManager.OnUSLive(this.isLive);
            }
        }

        const int LINE_LENGTH = 512;
        USEnhanceImage theEnhImage;

        private static object LockOnCoreData = new object();
        //@Override
        public override void OnCoreData(byte[] rawImageData)
        {
            lock (LockOnCoreData)
            {
                if (!isLive)
                {
                    return;
                }
                byte[] rawData = new byte[this.normalLineCount * LINE_LENGTH];
                Array.Copy(rawImageData, 0, rawData, 0, rawData.Length);
                byte[] suffixData = null;
                if (rawImageData.Length > rawData.Length)
                {
                    suffixData = new byte[this.suffixLineCount * LINE_LENGTH];
                    Array.Copy(rawImageData, rawData.Length, suffixData, 0, suffixData.Length);
                }

                USEnhanceImage theImage = null;
                ProbeMode mode = GetMode();


                bool enCompound = true;

                if (enCompound)
                {
                    if (this.theCompounder != null)
                    {
                        if (this.theEnhanceParameters.getUcCompound() != 0x00)
                        {
                            int angle = this.bwSteerAngle;
                            //System.Diagnostics.Debug.WriteLine("角度：" + angle);
                            //                    Log.d("DRIVER","ANGLE: " + angle);
                            if (angle != 0)
                            {
                                //    return;
                            }
                            int zoom = this.GetZoom();
                            //System.Diagnostics.Debug.WriteLine($"角度：{angle}");
                            rawData = this.theCompounder.FlushFrameEx(rawData, angle, zoom);
                        }
                        if (rawData == null)
                        {
                            rawData = new byte[this.normalLineCount * LINE_LENGTH];
                            return;
                        }
                    }
                }

                if (mode == ProbeMode.MODE_B || mode == ProbeMode.MODE_BM)
                {
                    //if (ret == USPackager.PACKAGE_SUCC_WITH_SUFFIX) {
                    if (suffixData != null)
                    {
                        return;
                    }
                    else
                    {
                        theImage = new USEnhanceImage();
                    }
                }
                else
                {
                    if (suffixData == null)
                    {
                        return;
                    }
                }

                USEnhanceProbe usColorExProbe = (USEnhanceProbe)theProbe;
                USProbe probeCap = theProbe;
                if (probeCap == null)
                {
                    probeCap = theProbe;
                }
                //  basic parameters
                theImage.timeCap = DateTime.Now;
                theImage.probeCap = probeCap;
                theImage.zoom = GetZoom();
                theImage.gain = GetGain();

                //  Enhance Parameters
                theImage.dynamicRange = GetDynamicRange();
                theImage.harmonic = GetHarmonic();
                theImage.focusPos = GetFocusPos();
                theImage.enhanceLevel = GetEnhanceLevel();

                theImage.rawData = rawData;
                theEnhImage = theImage;

                if (probeCap.imagingParameter.lineCount <= 0 ||
                        probeCap.imagingParameter.sampleCount <= 0 ||
                        (probeCap.imagingParameter.lineCount *
                        probeCap.imagingParameter.sampleCount) != rawData.Length)
                {
                    return;
                }

                USUsbDriver drv = this as USUsbDriver;

                if (drv.currentMode == ProbeMode.MODE_BM)
                {
                    USBMImage bmImage = USBMImage.MakeBMImage(drv.theEnhImage, drv.GetBMLine());

                    if (bmImage != null && drv.theManager != null)
                    {
                        drv.theManager.OnUSImage(bmImage);
                    }
                }
                else
                {
                    if (drv.theManager != null)
                    {
                        drv.theManager.OnUSImage(drv.theEnhImage);
                    }
                }

            }
        }

    }
}

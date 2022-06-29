using Newtonsoft.Json.Linq;
using SmartUSKit.SmartUSKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SmartUSKit.Enums;

namespace SmartUSKit.SmartUSKit
{
    public class USEnhanceDriver : USGeneralDriver
    {
        internal EnhanceParameters theEnhanceParameters = new EnhanceParameters();

        public class VGAIN
        {
            public byte ucVersion = (byte)0x00;
            public byte ucRevision = (byte)0x00;
            public int[] nVGain = new int[8];
            public byte[] ucReserved = new byte[2];
        }

        private static object mylock = new object();

        public USEnhanceProbe TheEnhanceProbe()
        {
            return (USEnhanceProbe)theProbe;
        }

        public USCompounder theCompounder;

        //protected MISCS_PARAM theMiscsParam = new MISCS_PARAM();
        protected bool updateMiscsParam;
        protected VGAIN theVGain = new VGAIN();
        protected bool updateVGain;
        protected int imageEnhanceLevel;
        protected int imageEnhanceMode = 0;

        //  穿刺线的位置
        protected float biopsyPosition;
        protected int biopsyAngle;
        
        //  可变增益调节
        public virtual void SetVGain(int value, int index)
        {
            if (index >= 0 && index < 8)
            {
                theVGain.nVGain[index] = value;
                updateVGain = true;

                TheEnhanceProbe().SetDefaultVGain(value, index);
            }
        }

        public int GetVGain(int index)
        {
            if (index >= 0 && index < 8)
            {
                return ((USEnhanceProbe)theProbe).GetDefaultVGain(index);
            }
            return 0;
        }

        //  频率调节
        public void SetFreq(int freq)
        {
            theEnhanceParameters.setUcFrequency((byte)(freq & 0xFF));
        }
        public int GetFreq()
        {
            return theEnhanceParameters.getUcFrequency() & 0xFF;
        }

        //  动态范围
        public virtual void SetDynamicRange(int dr)
        {
            //theMiscsParam.nDynamicRange = dr;
            //updateMiscsParam = true;

            theEnhanceParameters.setnDynamicRange(dr);
            TheEnhanceProbe().SetDefaultDynamicRange(dr);
        }
        public virtual int GetDynamicRange()
        {
            return theEnhanceParameters.getnDynamicRange();
        }

        //  谐波成像
        public virtual void SetHarmonic(bool bHarmonic)
        {
            try
            {
                if (bHarmonic)
                {
                    theEnhanceParameters.setUcHarmonic((byte)0x01);
                }
                else
                {
                    theEnhanceParameters.setUcHarmonic((byte)0x00);
                }
                updateMiscsParam = true;

                TheEnhanceProbe().SetDefaultHarmonic(bHarmonic);
            }
            catch (Exception ex)
            {
            }
        }

        
        public virtual bool GetHarmonic()
        {
            if (theEnhanceParameters.getUcHarmonic() == 0x01)
            {
                return true;
            }
            return false;
        }

        //  空间复合
        public void SetCompound(bool bCmpnd)
        {
            if (bCmpnd)
            {
                theEnhanceParameters.setUcCompound((byte)0x01);
            }
            else
            {
                theEnhanceParameters.setUcCompound((byte)0x00);
            }
            updateMiscsParam = true;
        }
        public bool GetCompound()
        {
            if (theEnhanceParameters.getUcCompound() == (byte)0x01)
            {
                return true;
            }
            return false;
        }
        
        //  焦点位置
        public virtual void SetFocusPos(int focusPos)
        {
            theEnhanceParameters.setUcFocusPos((byte)(focusPos & 0xFF));
            updateMiscsParam = true;

            TheEnhanceProbe().SetDefaultFocusPos(focusPos);
        }

        public virtual int GetFocusPos()
        {
            return theEnhanceParameters.getUcFocusPos() & 0xFF;
        }
        //  焦点数目
        public void setFocusCnt(int focusCnt)
        {
            theEnhanceParameters.setUcFocusCnt((byte)(focusCnt & 0xFF));
        }
        public int getFocusCnt()
        {
            return (int)(theEnhanceParameters.getUcFocusCnt() & 0xFF);
        }
        //  图像增强等级
        public void SetEnhanceLevel(int enhLevel)
        {
            if (enhLevel >= 0 && enhLevel <= 4)
            {
                imageEnhanceLevel = enhLevel;
                TheEnhanceProbe().SetDefaultEnhanceLevel(enhLevel);
            }
        }
        public void setEnhanceMode(int enhMode)
        {
            imageEnhanceMode = enhMode;
        }
        public int GetEnhanceLevel()
        {
            return imageEnhanceLevel;
        }

        public virtual int getEnhanceMode()
        {
            return imageEnhanceMode;
        }
        public override void OnCoreConnection(bool isConn)
        {
            if (isConn)
            {   //  初始化相关参数
                USEnhanceProbe theEnhProbe = (USEnhanceProbe)theProbe;
                bool harmonic = theEnhProbe.DefaultHarmonic();
                int dynamic_range = theEnhProbe.DefaultDynamicRange();
                int focus_pos = theEnhProbe.DefaultFocusPos();
                int enh_level = theEnhProbe.GetDefaultEnhanceLevel();
                bool compound = theEnhProbe.DefaultCompound();

                SetHarmonic(harmonic);
                SetDynamicRange(dynamic_range);
                SetFocusPos(focus_pos);
                SetEnhanceLevel(enh_level);
                SetCompound(compound);
                for (int i = 0; i < 8; i++)
                {
                    int vgain = theEnhProbe.GetDefaultVGain(i);
                    SetVGain(vgain, i);
                }

                //  初始化图像增强器
                //ImageEnhancer.GetInstance();
            }
            base.OnCoreConnection(isConn);
        }

        protected byte[] makeVGainCommand(int tick)
        {
            if (tick % 10 != 0)
            {
                return null;
            }
            if (!updateVGain)
            {
                return null;
            }
            updateVGain = false;
            byte[] ctrlblock = new byte[16];
            ctrlblock[0] = (byte)0x5E;
            ctrlblock[1] = (byte)0xE5;
            ctrlblock[2] = theVGain.ucVersion;
            ctrlblock[3] = theVGain.ucRevision;
            ctrlblock[4] = (byte)(theVGain.nVGain[0] & 0xFF);
            ctrlblock[5] = (byte)(theVGain.nVGain[1] & 0xFF);
            ctrlblock[6] = (byte)(theVGain.nVGain[2] & 0xFF);
            ctrlblock[7] = (byte)(theVGain.nVGain[3] & 0xFF);
            ctrlblock[8] = (byte)(theVGain.nVGain[4] & 0xFF);
            ctrlblock[9] = (byte)(theVGain.nVGain[5] & 0xFF);
            ctrlblock[10] = (byte)(theVGain.nVGain[6] & 0xFF);
            ctrlblock[11] = (byte)(theVGain.nVGain[7] & 0xFF);
            ctrlblock[12] = (byte)(theVGain.ucReserved[0] & 0xFF);
            ctrlblock[13] = (byte)(theVGain.ucReserved[1] & 0xFF);
            ctrlblock[14] = 0;
            ctrlblock[15] = 0;

            return ctrlblock;
        }

        protected virtual byte[] MakeEnhanceCommand(int tick)
        {

            if (tick % 4 != 0)
                return null;
            
            return theEnhanceParameters.makeCommand();
        }
        public virtual bool isHarmonicAvailable()
        {
            return true;
        }
        public override byte[] OnCoreControl(int tick)
        {
            byte[] basedata = base.OnCoreControl(tick);
            byte[] vgain = makeVGainCommand(tick);
            byte[] enh = MakeEnhanceCommand(tick);

            byte[] ret = null;
            ret = LinkByteArray(basedata, vgain);
            ret = LinkByteArray(ret, enh);
            return ret;
        }

        USEnhanceImage theEnhImage;
        internal int OnCoreDatacount = 0;
        public override void OnCoreData(byte[] data)
        {
            if (!JitFreeze())
            {
                int ret = thePackager.Package(data);
                if (ret >= USPackager.PACKAGE_SUCC)
                {
                    byte cmpdAngleIndex = thePackager.CompoundAngleIndex();

                    USEnhanceImage enhImage = new USEnhanceImage();
                    enhImage.timeCap = thePackager.CaptureTime();
                    enhImage.probeCap = theProbe;
                    enhImage.zoom = GetZoom();
                    enhImage.gain = GetGain();

                    //  Enhance Parameters
                    enhImage.dynamicRange = GetDynamicRange();
                    enhImage.harmonic = GetHarmonic();
                    enhImage.focusPos = GetFocusPos();
                    enhImage.enhanceLevel = GetEnhanceLevel();

                    //  Image Enhance
                    byte[] rawData = thePackager.RawData();
                    rawData = theProbe.RecoverDefect(rawData);

                    //  空间复合相关操作
                    //if (theCompounder != null)
                    //{
                    //    byte cmpAngleIndex = thePackager.CompoundAngleIndex();
                    //    int zoom = GetZoom();
                    //    if ((cmpAngleIndex & 0x08) != 0)
                    //    {
                    //        //  BiopsyEnhance Frame
                    //        byte angleIndex = (byte)(cmpAngleIndex & 0x07);
                    //        float angle = TheEnhanceProbe().GetBiopsyEnhanceAngle(angleIndex);
                    //        rawData = theCompounder.FlushBiopsyEnhanceFrame(rawData, (int)angle, zoom);
                    //        rawData = null;
                    //    }
                    //    else
                    //    {   // Non-Biopsy Frame
                    //        cmpAngleIndex = (byte)((cmpAngleIndex & 0x70) >> 4);
                    //        //Debug.WriteLine("Index" + cmpAngleIndex);
                    //        rawData = theCompounder.FlushFrame(rawData, cmpAngleIndex, zoom);
                    //    }
                    //}
                    //  空间复合
                    if (this.theCompounder != null)
                    {
                        if ((cmpdAngleIndex & 0x80) != 0)
                        {
                            int angleIndex = ((USEnhanceProbe)theProbe).compoundCodeToIndex(cmpdAngleIndex);
                            int angle = ((USEnhanceProbe)theProbe).getCompoundSteer(angleIndex);
                            int zoom = this.GetZoom();
                            rawData = this.theCompounder.FlushFrameEx(rawData, angle, zoom);
                        }
                        if (rawData == null)
                        {
                            return;
                        }
                    }

                    enhImage.rawData = rawData;
                    if (rawData != null)
                    {
                        if (theProbe.imagingParameter.lineCount <= 0 ||
                                     theProbe.imagingParameter.sampleCount <= 0 ||
                                    (theProbe.imagingParameter.lineCount *
                                    theProbe.imagingParameter.sampleCount) != rawData.Length)
                        {
                            return;
                        }

                        theEnhImage = enhImage;

                        if (this.currentMode == ProbeMode.MODE_BM)
                        {
                            USBMImage bmImage = USBMImage.MakeBMImage(this.theEnhImage, this.GetBMLine());

                            if (bmImage != null && this.theManager != null)
                            {
                                this.theManager.OnUSImage(bmImage);
                            }
                        }
                        else
                        {
                            if (this.theManager != null)
                            {
                                this.theManager.OnUSImage(this.theEnhImage);
                            }
                        }
                    }
                }
            }
        }


        public override void ExaminationSetting(JObject settings)
        {
            base.ExaminationSetting(settings);
            try
            {
                if (settings.ContainsKey("harmonic"))
                {
                    int h = int.Parse(settings["harmonic"].ToString());
                    bool bHarmornic = false;
                    if (h == 1)
                    {
                        bHarmornic = true;
                    }
                    if (isHarmonicAvailable())
                    {
                        SetHarmonic(bHarmornic);
                    }
                    else
                    {
                        SetHarmonic(false);
                    }
                }
                if (settings.ContainsKey("dr"))
                {
                    int dr = int.Parse(settings["dr"].ToString());
                    SetDynamicRange(dr);
                }
                if (settings.ContainsKey("enhance"))
                {
                    int enhLevel = int.Parse(settings["enhance"].ToString()); ;
                    SetEnhanceLevel(enhLevel);
                }
                if (settings.ContainsKey("focus"))
                {
                    int focuspos = int.Parse(settings["focus"].ToString()); ;
                    SetFocusPos(focuspos);
                }

                for (int i = 0; i < 8; i++)
                {
                    if (settings.ContainsKey("ENH_VGAIN_" + i.ToString()))
                    {
                        theVGain.nVGain[i] = int.Parse(settings["ENH_VGAIN_" + i.ToString()].ToString());
                        TheEnhanceProbe().SetDefaultVGain(theVGain.nVGain[i], i);
                        updateVGain = true;
                    }
                    else
                    {
                        theVGain.nVGain[i] = 127;
                        TheEnhanceProbe().SetDefaultVGain(theVGain.nVGain[i], i);
                        updateVGain = true;
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
        
        public override void onNTCoreConnection(bool isConn)
        {
            if (isConn)
            {
                //  默认参数
                USEnhanceProbe theEnhProbe = (USEnhanceProbe)theProbe;
                bool harmonic = theEnhProbe.DefaultHarmonic();
                int dynamic_range = theEnhProbe.DefaultDynamicRange();
                int focus_pos = theEnhProbe.DefaultFocusPos();
                int enh_level = theEnhProbe.GetDefaultEnhanceLevel();
                bool compound = theEnhProbe.DefaultCompound();
                SetHarmonic(harmonic);
                SetDynamicRange(dynamic_range);
                SetFocusPos(focus_pos);
                SetEnhanceLevel(enh_level);
                SetCompound(compound);
                for (int i = 0; i < 8; i++)
                {
                    int vgain = theEnhProbe.GetDefaultVGain(i);
                    SetVGain(vgain, i);
                }
            }
            base.onNTCoreConnection(isConn);
        }





        public override byte[] MakeGeneralControlCommand(int tick)
        {
            return df_makeGeneralControlCommand_color_ex(tick);
        }
        protected int gainTick;
        public byte[] df_makeGeneralControlCommand_color_ex(int tick)
        {
            byte[] ctrlblock = base.MakeGeneralControlCommand(tick);
            if (ctrlblock != null && ctrlblock.Length >= 4)
            {
                byte ctrlMode = (byte)0x00;

                ctrlblock[2] |= (byte)(ctrlMode << 2);

                // Zoom
                //ctrlblock[2] &= ~0x13;
                ctrlblock[2] &= 0xEC;
                ctrlblock[2] |= (byte)(ctrlZoom & 0x03);
                // Gain
                //  注意： 此处从 30-105 到 0～127的转换在此处不作处理，而由MCU来处理，只有这样才能保证它在MCU中的调整能够明确地反映到上位机。
                ctrlblock[3] = 0x00;

                Debug.WriteLine("发送的ctrlGain：" + ctrlGain);
                ctrlblock[3] |= (byte)(ctrlGain & 0x7F);

                if (gainTick > 0)
                {
                    gainTick--;
                    ctrlblock[3] |= 0x80;
                }
                else
                {
                    if (ctrlGain != stateGain)
                    {
                        ctrlGain = stateGain;

                        this.theProbe.SetDefaultGain(stateGain);
                    }
                }

                if (zoomTick > 0)
                {
                    zoomTick--;
                    ctrlblock[2] |= 0x10;
                }
                else
                {
                    int stateZoom = GetZoom();
                    if (ctrlZoom != stateZoom)
                    {
                        ctrlZoom = stateZoom;

                        theProbe.SetDefaultZoom(stateZoom);
                    }
                }
            }
            return ctrlblock;
        }
        public override void IncGain()
        {
            if (ctrlGain < 105)
            {
                SetGain(ctrlGain + 1);
            }

        }


        public override void DecGain()
        {
            if (ctrlGain > 30)
            {
                SetGain(ctrlGain - 1);
            }
        }
        public override void SetGain(int gain)
        {
            if (gain <= 30)
            {
                gain = 30;
            }
            else if (gain >= 105)
            {
                gain = 105;
            }

            System.Diagnostics.Debug.WriteLine("增益设置：" + gain);

            ctrlGain = gain;
            gainTick = 8;
            theProbe.SetDefaultGain(ctrlGain);
        }
        public override void OnCoreState(byte[] stat)
        {
            base.OnCoreState(stat);
            miscsStateParse(stat);
            //  解析数据包
            if (stat.Length >= 4)
            {
                if (stat[0] == (byte)0x5A && stat[1] == (byte)0xA5)
                {
                    int gain = (int)(stat[3] & 0x7F);
                    stateGain = gain;
                }
            }
        }

        void miscsStateParse(byte[] stat)
        {
            //static NSMutableData* s_MiscsData = nil;
            List<byte> s_MiscsData = stat.ToList();
           
            if (s_MiscsData.Count < 8)
            {
                return;
            }
            //uint8_t* bytes = (uint8_t*)[s_MiscsData bytes];
            byte[] bytes = s_MiscsData.ToArray();
            bool aligned = false;
            for (int i = 0; i <= s_MiscsData.Count - 8; i++)
            {
                if (bytes[i + 0] == 0x55
                    && bytes[i + 1] == 0xAA
                    && bytes[i + 2] == 0xFF
                    && bytes[i + 3] == 0x00
                    )
                {
                    if (i > 0)
                    {
                        //[s_MiscsData replaceBytesInRange:NSMakeRange(0, i) withBytes:NULL length:0];

                        s_MiscsData.RemoveRange(0, i);
                    }
                    aligned = true;
                    break;
                }
            }
            UInt32 need_length = 999999;
            //bytes = (uint8_t*)[s_MiscsData bytes];
            bytes = s_MiscsData.ToArray();
            if (s_MiscsData.Count > 4)
            {
                need_length = (UInt32)bytes[4];
            }
            
        }
    }
}

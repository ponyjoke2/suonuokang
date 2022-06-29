using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class USEnhanceProbe : USGeneralProbe
    {
        protected USEnhanceProbe()
        {
        }

        public USEnhanceProbe(string ssid) : base(ssid)
        {
            //  准备默认参数
            USPreferences prefs = USPreferences.GetInstance();
            defaultDynamicRange = prefs.GetInt(KeyDynamicRange(), 70);
            defaultEnhanceLevel = prefs.GetInt(KeyEnhanceLevel(), 0);
            defaultFocusPos = prefs.GetInt(KeyFocusPos(), 3);
            defaultHarmonic = prefs.GetBoolean(KeyHarmonic(), false);
            //defaultCompound = prefs.GetBoolean(KeyCompound(), false);
            for (int i = 0; i < 8; i++)
            {
                defaultVGain[i] = prefs.GetInt(KeyVGain(i), 127);
            }
        }
        public virtual int getCompoundSteer(int index)
        {
            //Debug.Assert(index >= 0 && index < colorParameter.bcSteerLevel);

            if (index < 0)
            {
                index = 0;
            }
            if (enhanceParameter.version==1)
            {
                return enhanceParameter.compoundSteer[index];
            }

            int bcSteerLevel = 5;
            int[] steers = new int[9];

            steers[0] = 0;
            steers[1] = 7;
            steers[2] = 12;
            steers[3] = -7;
            steers[4] = -12;

            if (index >= bcSteerLevel)
            {
                index = bcSteerLevel - 1;
            }

            return steers[index];
        }

        public virtual USPackager MakePackager()
        {
            USPackager packager = new USPackager(imagingParameter.lineCount, 32);
            return packager;
        }
        public virtual USCompounder MakeCompounder()
        {
            //  准备空间复合
            float[] sampleScales = new float[4];
            for (int i = 0; i < 4; i++)
            {
                float scale = SampleScale(i);
                sampleScales[i] = scale;
            }
            int lineCount = this.LineCount();
            int sampleCount = this.SampleCount();
            float pitch = 0.2f;
            float angle = 7.0f / 180.0f * (float)Math.PI; ;
            USCompounder compounder = new USCompounder();
            compounder.InitWithParams(lineCount, sampleCount, pitch, angle, sampleScales);
            return compounder;
        }



        public class IMAGING_ENHANCE_PARAMETER
        {
            public int version;              //  版本号，默认为0， 后续版本为 1
            public int focusCount;
            public float[,] focusPos = new float[4, 4];
            public int harmonicFocusCount;
            public float[,] harmonicFocusPos = new float[4, 4];
            public float frequency;
            public float harmonicFrequency;

            public int compoundLevel;        //  空间复合等级（1 为不空间复合，角度为0） version 1
            public int[] compoundSteer =new int[9];     //  空间复合偏转角度
        }

        public IMAGING_ENHANCE_PARAMETER enhanceParameter = new IMAGING_ENHANCE_PARAMETER();

        protected bool defaultHarmonic;
        protected int defaultDynamicRange;
        protected int defaultFocusPos;
        protected int defaultAcousticalPower;
        protected bool defaultCompound;
        protected int[] defaultVGain = new int[8];
        protected int defaultEnhanceLevel;

        public virtual int GetFocusLevel()
        {
            return 4;
        }

        public virtual int GetFocusCount()
        {
            return enhanceParameter.focusCount;
        }

        public virtual float GetFocusPos(int level, int pos)
        {
            if (level >= 0 && level < 4 && pos >= 0 && pos < GetFocusCount())
            {
                return enhanceParameter.focusPos[level, pos];
            }
            return 0;
        }
        //
        //  默认参数的保存与恢复
        //
        protected string KeyHarmonic()
        {
            //var probe = this.GetCurrentProbe();
            //string fiwehfa = this.GetCurrentProbe()?.GetCurrentProbe()?.probeType;
            //return this.GetCurrentProbe()?.probeType + "_ENH_" + "HARMONIC";
            return probeType + "_ENH_" + "HARMONIC";
        }
        protected string KeyDynamicRange()
        {
            return probeType + "_ENH_" + "DYNAMIC_RANGE";
        }
        protected string KeyFocusPos()
        {
            return probeType + "_ENH_" + "FOCUS_POS";
        }
        protected string KeyCompound()
        {
            return probeType + "_ENH_" + "COMPOUND";
        }
        protected string KeyVGain(int index)
        {
            return probeType + "_ENH_" + "VGAIN_" + index;
        }
        protected string KeyEnhanceLevel()
        {
            return probeType + "_ENH_" + "ENHANCE_LEVEL";
        }

        public virtual bool DefaultHarmonic()
        {
            return defaultHarmonic;
        }
        public virtual void SetDefaultHarmonic(bool harmonic)
        {
            if (defaultHarmonic != harmonic)
            {
                if (harmonic)
                {
                    InvokeExamiantionDelegate("harmonic", "1");
                }
                else
                {
                    InvokeExamiantionDelegate("harmonic", "0");
                }
            }
            defaultHarmonic = harmonic;
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutBoolean(KeyHarmonic(), harmonic);
        }

        public virtual int GetDefaultVGain(int index)
        {
            if (index >= 0 && index < 8)
            {
                return defaultVGain[index];
            }
            else
            {
                return 128;
            }

        }

        public virtual void SetDefaultVGain(int vgain, int index)
        {
            if (index >= 0 && index < 8)
            {
                if (defaultVGain[index] != vgain)
                {
                    InvokeExamiantionDelegate("ENH_" + "VGAIN_" + index.ToString(), vgain.ToString());
                }
                defaultVGain[index] = vgain;
                USPreferences prefs = USPreferences.GetInstance();
                prefs.PutInt(KeyVGain(index), vgain);
            }
        }

        public virtual int DefaultDynamicRange()
        {
            return defaultDynamicRange;
        }

        public virtual void SetDefaultDynamicRange(int dr)
        {
            if (defaultDynamicRange != dr)
            {
                InvokeExamiantionDelegate("dr", dr.ToString());
            }
            defaultDynamicRange = dr;
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutInt(KeyDynamicRange(), dr);
        }

        public virtual int DefaultFocusPos()
        {
            return defaultFocusPos;
        }

        public virtual void SetDefaultFocusPos(int fp)
        {
            if (defaultFocusPos != fp)
            {
                InvokeExamiantionDelegate("focus", fp.ToString());
            }
            defaultFocusPos = fp;
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutInt(KeyFocusPos(), fp);
        }

        public virtual void SetDefaultAcousticalPower(int fp)
        {
            if (defaultAcousticalPower != fp)
            {
                InvokeExamiantionDelegate("acousticalPower", fp.ToString());
            }
            defaultAcousticalPower = fp;
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutInt(KeyFocusPos(), fp);
        }

        public virtual bool DefaultCompound()
        {
            return defaultCompound;
        }
        public virtual void SetDefaultCompound(bool compound)
        {
            if (defaultCompound != compound)
            {
                if (compound)
                {
                    InvokeExamiantionDelegate("ENH_" + "COMPOUND", "1");
                }
                else
                {
                    InvokeExamiantionDelegate("ENH_" + "COMPOUND", "0");
                }
            }
            defaultCompound = compound;
            USPreferences prefs = USPreferences.GetInstance();
            prefs.PutBoolean(KeyCompound(), compound);
        }

        public virtual int GetDefaultEnhanceLevel()
        {
            return defaultEnhanceLevel;
        }
        public virtual void SetDefaultEnhanceLevel(int enhLevel)
        {
            if (enhLevel >= 0 && enhLevel <= 4)
            {
                if (defaultEnhanceLevel != enhLevel)
                {
                    InvokeExamiantionDelegate("enhance", enhLevel.ToString());
                }
                defaultEnhanceLevel = enhLevel;
                USPreferences prefs = USPreferences.GetInstance();
                prefs.PutInt(KeyEnhanceLevel(), enhLevel);
            }
        }

        //  获取换能器pitch
        public virtual float GetPitch()
        {
            return 0.0f;
        }
        
        //@Override
        public override void loadDefaultParams()
        {
        }
        //  获取穿刺增强图像的偏转角度
        public virtual float GetBiopsyEnhanceAngle(int index)
        {
            return 0.0f;
        }

        public override USDriver MakeDriver()
        {
            USEnhanceDriver driver = new USEnhanceDriver();
            USPackager packager = new USPackager(imagingParameter.lineCount, 0);
            driver.thePackager = packager;
            driver.theProbe = this;
            driver.SetGain(76);
            return driver;
        }

        public override bool IsEnhanceProbe()
        {
            return true;
        }

        public float[] FocusList(bool harmonic, int focusPos)
        {
            float[] ret = null;
            if (!harmonic)
            {
                int count = enhanceParameter.focusCount;
                ret = new float[count];
                for (int i = 0; i < count; i++)
                {
                    ret[i] = enhanceParameter.focusPos[focusPos, i];
                }
            }
            else
            {
                int count = enhanceParameter.harmonicFocusCount;
                ret = new float[count];
                for (int i = 0; i < count; i++)
                {
                    ret[i] = enhanceParameter.harmonicFocusPos[focusPos, i];
                }
            }
            return ret;
        }

        public virtual int compoundCodeToIndex(byte cmpdAngleCode)
        {
            int angleIndex = (int)(cmpdAngleCode & 0x0F);
            return angleIndex;
        }
    }
}

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartUSKit.Enums;
using SmartUSKit.SmartUSKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    
    public delegate void SaveDriverParmetersDelegate(string key, string value);
    public class USProbe
    {
        public virtual USProbe GetCurrentProbe()
        {
            return this;
        }
        static object mylock = new object();

        internal event SaveDriverParmetersDelegate SaveDriverParmetersEventHandler;

        internal void InvokeExamiantionDelegate(string key, string value)
        {
            if (SaveDriverParmetersEventHandler == null)
            {
                return;
            }
            SaveDriverParmetersEventHandler(key, value);
        }

        protected const float LINEAR_PROBE_DEAD_REGION = 100000.0f;
        protected static float LINEAR_PROBE_SECTOR_ANGLE(float linear_width)
        {
            return (float)(Math.Asin(linear_width / 2.0 / LINEAR_PROBE_DEAD_REGION) * 2.0f);
        }
        public string probeType;
        public string probeSSID;
        public string probeSN;
        public char salesCode;
        public int wifiChannel;


        public class IMAGING_PARAMETER
        {
            public float deadRegion;    //  死区长度（mm）
            public float sectorAngle;   //  扇形角度（弧度）
            public bool isLinear;       //  是否为线阵探头
            public float linearWidth;   //  线阵探头宽度（mm）
            public int lineCount;       //  每帧采样线数
            public int sampleCount;     //  每线采样点数
            public int[] usingSample = new int[4];  //  重建中使用的采样点数  
            public int[] namicDepth = new int[4];   //  名义深度
            public int[] mapTable = new int[256];   //  灰度隐射表
            public float originSampleRate;          //  原始采样率（FPGA中使用），通常为50MHz，部分为40MHz
            public int[] abstractRate = new int[4]; //  不同Zoom下的抽取率

            public float sampleRate(int zoom) { return originSampleRate / abstractRate[zoom]; }
            public float scaleSample(int zoom)
            {
                return (float)(1560.0 * 1000.0 / 2.0 / (float)sampleRate(zoom));
            }
        }
        public IMAGING_PARAMETER imagingParameter = new IMAGING_PARAMETER();

        protected USProbe()
        {
        }
        public USProbe(string ssid)
        {
            probeSSID = ssid;
            int index = ssid.IndexOf(" ");
            probeType = ssid.Substring(0, index);
            salesCode = ssid.ElementAt(index + 1);

            //  准备默认参数
            USPreferences thePrefs = USPreferences.GetInstance();
            defaultZoom = thePrefs.GetInt(KeyZoom(), 2);
            if (defaultZoom < 0)
            {
                defaultZoom = 0;
            }
            else if (defaultZoom > 3)
            {
                defaultZoom = 3;
            }
            defaultGain = thePrefs.GetInt(KeyGain(), 80);
            if (defaultGain < 30)
            {
                defaultGain = 30;
            }
            else if (defaultGain > 105)
            {
                defaultGain = 105;
            }

            defaultExamination = thePrefs.GetString(keyExamination(), "");
        }
        public virtual string TransducerMark()
        {
            return probeType;
        }

        protected void MakeProbeSN(string prefix)
        {
            int index = probeSSID.IndexOf(' ');
            string sn = probeSSID.Substring(index + 3, probeSSID.Length - (index + 3));
            probeSN = prefix + sn;
        }

        public int LineCount()
        {
            return imagingParameter.lineCount;
        }

        public int SampleCount()
        {
            return imagingParameter.sampleCount;
        }
       
        public virtual byte[] RecoverDefect(byte[] rawData)
        {
            return rawData;
        }

        public virtual USDriver MakeDriver()
        {
            return null;
        }

        
        public virtual bool IsEnhanceProbe()
        {
            return false;
        }
        
        public virtual bool Is5GProbe()
        {
            return false;
        }

        public virtual bool IsLinearProbe()
        {
            return imagingParameter.isLinear;
        }


        //  默认参数的保存与恢复
        protected string KeyGain()
        {
            return probeType + "_GAIN";
        }
        protected string KeyZoom()
        {
            return probeType + "_ZOOM";
        }
        protected int defaultZoom;
        protected int defaultGain;
        public virtual void SetDefaultZoom(int zoom)
        {
            if (defaultZoom != zoom)
            {
                InvokeExamiantionDelegate("zoom", zoom.ToString());
            }
            defaultZoom = zoom;
            USPreferences thePrefs = USPreferences.GetInstance();
            var keyzoom = KeyZoom();
            thePrefs.PutInt(keyzoom, zoom);
        }
        public virtual int GetDefaultZoom()
        {
            return defaultZoom;
        }

        public virtual void SetDefaultGain(int gain)
        {
            if (defaultGain != gain)
            {
                InvokeExamiantionDelegate("gain", gain.ToString());
            }
            defaultGain = gain;
            USPreferences thePrefs = USPreferences.GetInstance();
            thePrefs.PutInt(KeyGain(), gain);
        }
        public virtual int GetDefaultGain()
        {
            return defaultGain;
        }

        protected virtual String keyExamination()
        {
            return probeType + "_EXAM";
        }
        public virtual String DefaultExamination(String transMark)
        {
            return defaultExamination;
        }
        public String defaultExamination;
        public virtual void SetDefaultExamination(String exam, String transMark)
        {
            defaultExamination = exam;

            USPreferences thePrefs = USPreferences.GetInstance();
            thePrefs.PutString(keyExamination(), defaultExamination);
        }

        

        public virtual void loadDefaultParams()
        {
        }

        public class USParam
        {
            public int version;
            public bool isAvailable;
            public float MI;
            public float TIS;
        }



        bool oldIsHarmonic = false;
        ProbeMode? oldMode = null;
        USParam param = new USParam();
        public USParam getUSParam(ProbeMode mode, bool isHarmonic)
        {
            if (mode==oldMode
                &&isHarmonic==oldIsHarmonic)
            {
                return param;
            }
            else
            {
                oldIsHarmonic = isHarmonic;
                oldMode = mode;
            }
            bool isLineOrIntracavity = false;

            if (this.IsLinearProbe() ||
                     (this.imagingParameter.deadRegion > 5 && this.imagingParameter.deadRegion < 15))
            {
                isLineOrIntracavity = true;
            }

            float MI = 0, TIS = 0;
            
            
            param.MI = MI;
            param.TIS = TIS;
            param.version = 1;

            return param;
        }

        public virtual IMAGING_PARAMETER getImagingParameterWithZoom(int zoom)
        {
            return this.imagingParameter;
        }
        
    }
}

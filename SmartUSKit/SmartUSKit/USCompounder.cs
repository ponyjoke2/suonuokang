using SmartUSKit.SmartUSKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class USCompounder
    {
        protected List<byte> angleFIFO = new List<byte>();
        protected List<byte[]> dataFIFO = new List<byte[]>();
        internal Compounder[] compounders = new Compounder[4];
        protected bool usingExCompounder = false;
        protected CompounderEx[] ex_compounders = new CompounderEx[4];
        protected int line;
        protected int sample;
        protected int currentZoom;

        protected bool biopsyEnhanceEnabled;

        //  传刺线的位置
        protected float biopsyPosition;
        protected int biopsyAngle;
        protected int biopsyEnhanceAngle;

        public USCompounder()
        {
        }

        

        public void InitWithParams(int line, int sample, float pitch, float angle, float[] sampleScale)
        {
            Debug.Assert(4 == sampleScale.Length);
            this.angleFIFO.Clear();
            this.dataFIFO.Clear();
            this.line = line;
            this.sample = sample;
            for (int i = 0; i < 4; i++)
            {
                float scale = sampleScale[i];
                compounders[i] = new Compounder(line, sample, pitch, angle, scale);
            }
        }
        public void initWithLine(int lineCount, int sampleCount, float pitch, float angle, float[] sampleScales)
        {
            usingExCompounder = false;
            //self = [super init];
            //if (self)
            {
                line = lineCount;
                sample = sampleCount;
                //angleFIFO = new List<byte>();
                //        dataFIFO = new List<byte[]>();
                for (int i = 0; i < 4; i++)
                {
                    float sampleScale = sampleScales[i];
                    compounders[i] = new Compounder(line, sample, pitch, angle, sampleScale);
                }
            }
            //return self;
        }

        public void initWithLine(int lineCount, int sampleCount, double deadRgn, double scanAngle, double steerAngle, float[] sampleScale)
        {
            this.usingExCompounder = true;

            this.angleFIFO.Clear();
            this.dataFIFO.Clear();
            this.line = lineCount;
            this.sample = sampleCount;
            for (int i = 0; i < 4; i++)
            {
                float scale = sampleScale[i];
                ex_compounders[i] = new CompounderEx(lineCount, sampleCount, deadRgn, scanAngle, steerAngle, scale);
            }
        }
        public byte[] FlushFrame(byte[] rawData, int angleIndex, int zoom)
        {
            if (angleIndex < 0 || angleIndex > 2)
            {
                return null;
            }
            if (zoom < 0 || zoom >= 4)
            {
                return null;
            }
            if (zoom != currentZoom)
            {
                angleFIFO.Clear();
                dataFIFO.Clear();
                currentZoom = zoom;
            }

            byte ai = (byte)(angleIndex & 0xFF);
            angleFIFO.Add(ai);
            dataFIFO.Add(rawData);
            if (angleFIFO.Count() > 3)
            {
                angleFIFO.RemoveAt(0);
            }
            if (dataFIFO.Count() > 3)
            {
                dataFIFO.RemoveAt(0);
            }
            byte[] cmpdata = null;
            if (angleFIFO.Count() >= 3)
            {
                cmpdata = CompoundData();
            }
            if (cmpdata != null)
            {
                byte[] retdata = new byte[cmpdata.Length];
                cmpdata.CopyTo(retdata, 0);
                return retdata;
            }
            return null;
        }

        public virtual byte[] FlushFrameEx(byte[] rawData, int angle, int zoom)
        {
            //  注意： 顺时针方向为正， 逆时针方向为负
            int angleIndex = 0;
            if (angle > 0)
            {
                angleIndex = 2;         //  " / "
            }
            else if (angle < 0)
            {
                angleIndex = 1;         //  " \ "
            }
            return this.FlushFrame(rawData, angleIndex, zoom);
        }
        protected byte[] CompoundData()
        {
            if (angleFIFO.Count() != 3)
            {
                return null;
            }
            if (currentZoom < 0 || currentZoom >= 4)
            {
                return null;
            }

            byte[] left = null;
            byte[] mid = null;
            byte[] right = null;
            for (int i = 0; i < 3; i++)
            {
                Byte angle = angleFIFO[i];
                if (angle == 2)
                {
                    left = dataFIFO[i];
                }
                else if (angle == 1)
                {
                    right = dataFIFO[i];
                }
                else
                {
                    mid = dataFIFO[i];
                }
            }
            if (left == null || right == null || mid == null)
            {
                return null;
            }
            byte[] ret;
            if (!usingExCompounder)
            {
                Compounder cmper = compounders[currentZoom];
                ret = cmper.CompoundData(left, mid, right);
                //  穿刺增强
                if (biopsyEnhanceEnabled)
                {
                    cmper.SetBiopsy(biopsyPosition, biopsyAngle);
                    ret = cmper.CompoundBiopsy(ret);
                }
            }
            else
            {
                CompounderEx cmper = ex_compounders[currentZoom];
                ret = cmper.CompoundData(left, mid, right);
            }

            if (!enableCompoundCheck)
            {
                return ret;
            }
            if (ret == null)
            {
                return ret;
            }

            var last = dataFIFO[2];
            var prev = dataFIFO[1];
            bool steerValid = true;
            int sameCount = 0;
            for (int i = 512 * 4 + 10; i < 512 * 5; i++)
            {
                if (last[i] > 0)
                {
                    if (last[i] == prev[i])
                    {
                        sameCount++;
                    }
                    else
                    {
                        sameCount = 0;
                        break;
                    }
                }
            }
            if (sameCount > 0)
            {
                steerValid = false;
                //NSLog(@"CompoundCheck FAILED.");
                //Debug.WriteLine(@"CompoundCheck FAILED.");

            }

            if (steerValid)
            {
                if (ret != null)
                {
                    prevOutput = new byte[ret.Length];
                    Array.Copy(ret, prevOutput, ret.Length);
                    //prevOutput = ret;
                }

            }
            else
            {
                if (enableOutput)
                {
                    ret = prevOutput;
                }
                else
                {
                    ret = null;
                }
            }
            return ret;
        }

        public void EnableBiopsyEnhance(bool enable)
        {
            biopsyEnhanceEnabled = enable;
            /*
            if (enable)
            {
                Log.d("AAA", "biopsy enhance ENABLED");
            }
            else
            {
                Log.d("AAA", "biopsy enhance DISABLED");
            }
            */
        }
        public bool IsBiopsyEnhanceEnabled()
        {
            return biopsyEnhanceEnabled;
        }

        public byte[] FlushBiopsyEnhanceFrame(byte[] rawData, int angle, int zoom)
        {
            if (zoom != currentZoom)
            {
                currentZoom = zoom;
            }
            biopsyEnhanceAngle = angle;


            Compounder cmper = compounders[currentZoom];
            byte[] pBEFrame = rawData;
            byte[] pret = cmper.FlushBiopsyEnhanceFrame(pBEFrame, angle);
            return pret;
        }
        public int GetBiopsyEnhanceAngle()
        {
            return biopsyEnhanceAngle;
        }
        public void SetBiopsyPosition(float position, int angle)
        {
            biopsyPosition = position;
            biopsyAngle = angle;
        }

        public void Clear()
        {
            if (angleFIFO != null)
            {
                angleFIFO.Clear();
            }
            if (dataFIFO != null)
            {
                dataFIFO.Clear();
            }
        }



        //  空间复合数据检测
        bool enableCompoundCheck;
        bool enableOutput;
        byte[] prevOutput;
        public void EnableCompoundCheck(bool enCheck, bool toOutput)
        {
            enableCompoundCheck = enCheck;
            enableOutput = toOutput;
            prevOutput = null;
        }


    }
}

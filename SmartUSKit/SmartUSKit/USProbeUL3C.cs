using SmartUSKit.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class USProbeUL3C : USUsbProbe
    {
        protected USProbeUL3C() : base()
        {
        }
        public USProbeUL3C(String ssid) : base(ssid)
        {
            //super(ssid);
            MakeProbeSN("ULC");

            imagingParameter.deadRegion = LINEAR_PROBE_DEAD_REGION;
            imagingParameter.sectorAngle = LINEAR_PROBE_SECTOR_ANGLE(25.6f);
            imagingParameter.isLinear = true;
            imagingParameter.linearWidth = 25.6f;
            imagingParameter.lineCount = 128;
            imagingParameter.sampleCount = 512;
            imagingParameter.usingSample[0] = 502;
            imagingParameter.usingSample[1] = 502;
            imagingParameter.usingSample[2] = 502;
            imagingParameter.usingSample[3] = 502;

            imagingParameter.namicDepth[0] = 20;
            imagingParameter.namicDepth[1] = 40;
            imagingParameter.namicDepth[2] = 60;
            imagingParameter.namicDepth[3] = 100;
            imagingParameter.mapTable = GrayMapTable.GetInstance().MapTableEx(4);

            imagingParameter.originSampleRate = 40.0f * 1000.0f * 1000.0f;
            imagingParameter.abstractRate[0] = 2;
            imagingParameter.abstractRate[1] = 4;
            imagingParameter.abstractRate[2] = 6;
            imagingParameter.abstractRate[3] = 10;

            enhanceParameter.version = 1;
            enhanceParameter.focusCount = 2;
            enhanceParameter.focusPos[0, 0] = 6.0f;
            enhanceParameter.focusPos[0, 1] = 15.0f;
            enhanceParameter.focusPos[1, 0] = 9.0f;
            enhanceParameter.focusPos[1, 1] = 20.0f;
            enhanceParameter.focusPos[2, 0] = 12.0f;
            enhanceParameter.focusPos[2, 1] = 25.0f;
            enhanceParameter.focusPos[3, 0] = 15.0f;
            enhanceParameter.focusPos[3, 1] = 30.0f;
            enhanceParameter.harmonicFocusCount = 2;
            enhanceParameter.harmonicFocusPos[0, 0] = 4.0f;
            enhanceParameter.harmonicFocusPos[0, 1] = 9.0f;
            enhanceParameter.harmonicFocusPos[1, 0] = 9.0f;
            enhanceParameter.harmonicFocusPos[1, 1] = 15.0f;
            enhanceParameter.harmonicFocusPos[2, 0] = 9.0f;
            enhanceParameter.harmonicFocusPos[2, 1] = 20.0f;
            enhanceParameter.harmonicFocusPos[3, 0] = 12.0f;
            enhanceParameter.harmonicFocusPos[3, 1] = 25.0f;
            enhanceParameter.frequency = 10.0f;
            enhanceParameter.harmonicFrequency = 14.0f;

            // version 1
            enhanceParameter.compoundLevel = 3;
            enhanceParameter.compoundSteer[0] = 0;
            enhanceParameter.compoundSteer[1] = 7;
            enhanceParameter.compoundSteer[2] = -7;
            
            loadDefaultParams();
        }

        //@Override
        public override void loadDefaultParams()
        {
            base.loadDefaultParams();

            //  一律开启空间复合
            this.SetDefaultCompound(true);

        }

        //@Override
        public override bool Is5GProbe()
        {
            return true;
        }

        //@Override
        public override USDriver MakeDriver()
        {
            USUsbDriver drv = (USUsbDriver)base.MakeDriver();
            drv.setLineCount(imagingParameter.lineCount, 32);
            return drv;
        }

        //@Override
        internal override int getWaitTime(ProbeMode mode, bool harmonic, USDriver waitTimeDriver)
        {
            if (mode == ProbeMode.MODE_B || mode == ProbeMode.MODE_BM)
            {
                if (harmonic)
                {
                    return 77;  //  12.95，12.94，12.93
                }
                else
                {
                    return 57;  //  17.45，17.47，17.49
                }
            }
            else
            {
                return 120; // 8.29，8.29，8.30
            }
        }
        public USCompounder MakeCompounder()
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class USProbeUL3CA : USEnhanceProbe
    {
        protected USProbeUL3CA() : base()
        {
            //super();
        }
        public USProbeUL3CA(String ssid) : base(ssid)
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

            enhanceParameter.version = 0;
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


            loadDefaultParams();
        }

        //@Override
        public override void loadDefaultParams()
        {
            base.loadDefaultParams();

            //  一律开启空间复合
            this.SetDefaultCompound(true);
        }

        

        public override bool Is5GProbe()
        {
            return true;
        }
        public override USCompounder MakeCompounder()
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
        public override USDriver MakeDriver()
        {
            USEnhanceDriver driver = new USEnhanceDriver();
            
            driver.theProbe = this;
            driver.SetBMLine(this.imagingParameter.lineCount / 2);
            driver.thePackager = this.MakePackager();
            driver.theCompounder = this.MakeCompounder();
            return driver;
        }
    }
}

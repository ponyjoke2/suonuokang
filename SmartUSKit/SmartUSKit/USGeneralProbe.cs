using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class USGeneralProbe : USProbe
    {
        protected USGeneralProbe()
        {

        }
        public USGeneralProbe(string ssid) : base(ssid)
        {

        }

        public override USDriver MakeDriver()
        {
            USGeneralDriver driver = new USGeneralDriver();
            USPackager packager = new USPackager(imagingParameter.lineCount, 0);
            driver.thePackager = packager;
            driver.theProbe = this;
            return driver;
        }
        //@Override
        public override void loadDefaultParams()
        {
        }

        public float SampleScale(int zoom)
        {
            float sampleScale = this.imagingParameter.scaleSample(zoom);
            return sampleScale;
        }
    }
}

using SmartUSKit.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class USUsbProbe : USEnhanceProbe
    {
        protected USUsbProbe() { }
        public USUsbProbe(String ssid) : base(ssid)
        {
            //super(ssid);
        }

        //@Override
        public override USDriver MakeDriver()
        {
            USUsbDriver driver = new USUsbDriver();

            driver.theProbe = this;
            driver.SetBMLine(this.imagingParameter.lineCount / 2);
            driver.thePackager = this.MakePackager();
            driver.theCompounder = this.MakeCompounder();

            return driver;
        }
        

        internal virtual int getWaitTime(ProbeMode mode, bool harmonic,USDriver driver)
        {
            return 100;
        }

        public virtual byte getFPGAAngleIndex(int index)
        {
            byte[] fpgaAngleIndex = new byte[] { 0, 1, 3 };
            if (index>=0 && index<=2)
            {
                return fpgaAngleIndex[index];
            }
            return 0;
        }
        
    }
}

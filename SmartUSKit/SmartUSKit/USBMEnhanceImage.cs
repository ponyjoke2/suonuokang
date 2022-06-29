using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class USBMEnhanceImage : USBMImage
    {
        public bool harmonic;
        public int dynamicRange;
        public int focusPos;
        public int enhanceLevel;

        public float[] FocusList()
        {
            USEnhanceProbe enhProbe = (USEnhanceProbe)probeCap;
            return enhProbe.FocusList(harmonic, focusPos);
        }

        public override bool IsEnhanceImage()
        {
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class USRawImage
    {
        public byte[] rawData;
        public USProbe probeCap;
        public DateTime timeCap;

        public int zoom;
        public int gain;

        public virtual string Description()
        {
            return "Raw Image";
        }

        public virtual bool IsEnhanceImage()
        {
            if (probeCap != null && probeCap.IsEnhanceProbe())
            {
                return true;
            }
            return false;
        }

        
        public virtual bool IsBMImage()
        {
            return false;
        }
       
    }
}

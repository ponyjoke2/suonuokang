using SmartUSKit.SmartUSKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit_CS.Model
{
    public class USImage
    {
        //public USRawImage rawImage;
        //public USMarkGroup theMarks;

        private USRawImage rawImage;
        public USRawImage RawImage
        {
            get
            {
                if (rawImage==null)
                {
                    rawImage = new USRawImage();
                }
                return rawImage;
            }
            set { rawImage = value; }
        }
        

        public USImage()
        {
        }
        public USImage(USRawImage rawImage)
        {
            this.rawImage = rawImage;
        }
        
    }
}

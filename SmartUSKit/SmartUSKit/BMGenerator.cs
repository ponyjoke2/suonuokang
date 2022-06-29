using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class BMGenerator
    {
        protected static BMGenerator instance = new BMGenerator();
        public static BMGenerator GetInstance()
        {
            return instance;
        }

        protected List<USRawBMSample> bmList = new List<USRawBMSample>();

        public void Reset()
        {
            bmList.Clear();
        }
        public void FlushRawImage(USRawImage rawImage, int sampleLine)
        {
            if (sampleLine < 0)
            {
                sampleLine = 0;
            }
            else if (sampleLine >= rawImage.probeCap.imagingParameter.lineCount - 1)
            {
                sampleLine = rawImage.probeCap.imagingParameter.lineCount - 1;
            }
            int sampleCount = rawImage.probeCap.imagingParameter.sampleCount;
            byte[] sample = new byte[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                sample[i] = rawImage.rawData[sampleLine * sampleCount + i];
            }
            DateTime time = rawImage.timeCap;

            USRawBMSample bmSample = new USRawBMSample(sample, sampleLine, time);
            bmList.Add(bmSample);
            if (bmList.Count() > USRawBM.MAX_BM_LINE)
            {
                bmList.RemoveAt(0);
            }
        }
        public USRawBM SnapShot()
        {
            USRawBM rawBM = new USRawBM();
            for (int i = 0; i < bmList.Count(); i++)
            {
                USRawBMSample bmSample = bmList[i];
                rawBM.FlushBMSample(bmSample);
            }
            return rawBM;
        }
    }
}

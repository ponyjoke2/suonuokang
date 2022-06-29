using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmartUSKit.SmartUSKit
{
    public class USBMImage : USRawImage
    {


        public USRawBM rawBM;
        public int bmLine;

        public override bool IsBMImage()
        {
            return true;
        }

        public static USBMImage MakeBMImage(USRawImage rawImage, int bmLine)
        {
            if (rawImage == null)
            {
                return null;
            }
            if (rawImage.IsBMImage())
            {
                return (USBMImage)rawImage;
            }
           

            USBMImage bmImage = null;
            if (!rawImage.IsEnhanceImage())
            {
                bmImage = new USBMImage();
            }
            else
            {
                bmImage = new USBMEnhanceImage();
            }
            bmImage.timeCap = rawImage.timeCap;
            bmImage.probeCap = rawImage.probeCap;
            bmImage.rawData = rawImage.rawData;
            bmImage.zoom = rawImage.zoom;
            bmImage.gain = rawImage.gain;
            bmImage.bmLine = bmLine;
            BMGenerator bmGen = BMGenerator.GetInstance();
            bmGen.FlushRawImage(rawImage, bmLine);
            bmImage.rawBM = bmGen.SnapShot();

            if (rawImage.IsEnhanceImage())
            {
                USBMEnhanceImage enhBMImage = (USBMEnhanceImage)bmImage;
                USEnhanceImage enhImage = (USEnhanceImage)rawImage;
                enhBMImage.harmonic = enhImage.harmonic;
                enhBMImage.dynamicRange = enhImage.dynamicRange;
                enhBMImage.focusPos = enhImage.focusPos;
                enhBMImage.enhanceLevel = enhImage.enhanceLevel;
            }
            return bmImage;
        }
    }
    public class USRawBMSample
    {
        public DateTime timeCap;
        public int sampleline;
        public byte[] sampleData;
        public USRawBMSample(byte[] sample, int line, DateTime time)
        {
            this.timeCap = time;
            this.sampleline = line;
            this.sampleData = sample;
        }
        public DateTime GetCapTime()
        {
            return timeCap;
        }
        public byte[] GetSampleData()
        {
            return sampleData;
        }
        public int GetSampleLine()
        {
            return sampleline;
        }
    }

    public class USRawBM
    {
        public const int MAX_BM_LINE = 100;
        public List<USRawBMSample> bmRawLines = new List<USRawBMSample>();
        public void FlushBMSample(USRawBMSample bmSample)
        {
            bmRawLines.Add(bmSample);
            if (bmRawLines.Count() > MAX_BM_LINE)
            {
                bmRawLines.RemoveAt(0);
            }
        }
        public DateTime GetTime(int index)
        {
            int start = MAX_BM_LINE - bmRawLines.Count() + 1;
            if (index < start)
            {
                return new DateTime(0);
            }
            return bmRawLines[index - start].GetCapTime();
        }
        public BitmapSource CreateBitmap()
        {
            int width = MAX_BM_LINE;
            int height = 512;
            UInt32[] pixels = new UInt32[width * height];
            int startX = 0;
            if (bmRawLines.Count() < MAX_BM_LINE)
            {
                startX = MAX_BM_LINE - bmRawLines.Count();
            }
            for (int x = startX; x < width; x++)
            {
                USRawBMSample bmSample = bmRawLines[x - startX];
                byte[] sampData = bmSample.GetSampleData();
                for (int y = 0; y < 512; y++)
                {
                    int pos = y * width + x;
                    UInt32 color = 0xFF000000;
                    UInt32 gray = (UInt32)(sampData[y] & 0xFF);
                    color |= (gray << 16) | (gray << 8) | gray;
                    pixels[pos] = color;
                }
            }

            #region 生成可显示图像
            // Define parameters used to create the BitmapSource.
            //int width = GetDscWidth();
            //int height = GetDscHeight();
            int rawStride = (width * PixelFormats.Bgra32.BitsPerPixel + 7) / 8;
            byte[] SrawImage = new byte[rawStride * height];

            BitmapSource Sbitmap = BitmapSource.Create(width, height,
                96, 96, PixelFormats.Bgra32, null,
                pixels, rawStride);
            #endregion
            
            return Sbitmap;
        }
        public int GetbmRawLines()
        {
            return bmRawLines.Count;
        }
    }
}

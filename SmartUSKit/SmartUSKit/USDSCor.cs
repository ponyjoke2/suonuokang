using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static SmartUSKit.SmartUSKit.DSConverter;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace SmartUSKit.SmartUSKit
{
    public class USDSCor
    {
        private static Dictionary<String, USDSCor> map = new Dictionary<string, USDSCor>();
        private static object _lock = new object();
        private static object DSConverter_lock = new object();
        [DllImport("Dll3.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static float Execute1(byte[] array, int bitmapWidth, int bitmapHeight, int test);
        [DllImport("Dll3.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static float Execute2(byte[] array, int bitmapWidth, int bitmapHeight);
        [DllImport("Dll3.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static float Execute3(byte[] array, int bitmapWidth, int bitmapHeight);
        [DllImport("Dll3.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static float Execute4(byte[] array, int bitmapWidth, int bitmapHeight);

        //创建socket,指定地址簇协议、套接字类型和通信协议
        private Socket socketSendAll = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //定义一网络端点
        private IPEndPoint endPointAll = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 34569);

        //创建socket,指定地址簇协议、套接字类型和通信协议
        private Socket socketSendImage = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //定义一网络端点
        private IPEndPoint endPointImage = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 34568);
        //float dpiX = 96;
        //float dpiY = 96;
        private static int flag = 0;
        //计算数据
        private float vesselDepth = 0;
        private float vesselAbscissa = 0;
        private float vesselLWratio = 0;
        private float vesselArea = 0;
        public volatile byte[] pixels;
        //测试
        int test = 0;

        private static ReaderWriterLockSlim toolLock = new ReaderWriterLockSlim();
        public USDSCor()
        {
            if (flag == 1)
            {
                socketSendAll.Connect(endPointAll);
                socketSendImage.Connect(endPointImage);
            }
            flag += 1;
        }

        public static USDSCor GetInstance(String key)
        {



            if (key == null)
            {
                return null;
            }
            USDSCor ret = null;
            if (map.ContainsKey(key))
            {
                ret = map[key];
            }
            if (ret == null)
            {
                lock (_lock)
                {
                    if (ret == null)
                    {
                        ret = new USDSCor();
                        map.Add(key, ret);
                    }
                }
            }
            return ret;
        }

        public static void MemoryFree()
        {   //  释放内存
            //  Dictionary<String, USDSCor>.Enumerator enumer = map.GetEnumerator();
            //  ...
        }

        public const int DSC_MODE_MATCH_HEIGHT = 0;
        public const int DSC_MODE_WIDTH_RESERVE = 1;

        protected int destWidth;
        protected int destHeight;
        protected int dscWidth;
        protected int dscHeight;
        protected int dscMode;
        protected float scaleExpand;

        internal DSConverter currentConverter;
        internal Dictionary<string, DSConverter> theConverterPool = new Dictionary<string, DSConverter>();

        public bool SetDestSize(int width, int height)
        {
            return SetDestSize(width, height, DSC_MODE_MATCH_HEIGHT);
        }

        public bool SetDestSize(int width, int height, int dsc_mode)
        {
            //  有效的值只能设置一次
            if (destWidth > 0 && destHeight > 0)
            {
                return true;
            }
            if (width <= 1 || height <= 0)
            {
                return false;
            }

            destWidth = width;
            destHeight = height;
            float maxDscArea = 640 * 480;
            if (destWidth * destHeight <= maxDscArea)
            {
                dscWidth = destWidth;
                dscHeight = destHeight;
                scaleExpand = (float)destHeight / (float)dscHeight;
            }
            else
            {
                float scale = (float)destWidth * destHeight / maxDscArea;
                scale = (float)Math.Sqrt(scale);
                dscHeight = (int)(destHeight / scale + 0.5);
                dscWidth = (int)(destWidth / scale + 0.5);
                scaleExpand = (float)destHeight / (float)dscHeight;
            }
            dscWidth = (dscWidth + 1) / 2 * 2;  //  因为DSC算法采用左右对称算法原因，要求重建宽度为偶数
            dscMode = dsc_mode;
            return true;
        }

        public int GetDscWidth()
        {
            return dscWidth;
        }
        public int GetDscHeight()
        {
            return dscHeight;
        }
        public int GetDestWidth()
        {
            return destWidth;
        }
        public int GetDestHeight()
        {
            return destHeight;
        }

        static string ShowMessage(string x)
        {
            string current = string.Format("当前线程id为{0}", Thread.CurrentThread.ManagedThreadId);
            return string.Format("{0},输入为{1}", current, x);
        }


        public BitmapSource DscImage(USRawImage rawImage)
        {
            ResetCurrentDSConverter(rawImage);
            this.pixels = currentConverter.DSC(rawImage.rawData);
            //Console.WriteLine("数组长度：" + pixels.Length);

            //int row = 435;
            //int col = 67;
            //Console.WriteLine(pixels[704*4*row+4*col+0]+" "+ pixels[704 * 4 * row + 4 * col + 1]+ " "+ pixels[704 * 4 * row + 4 * col + 2]+" "+ pixels[704 * 4 * row + 4 * col + 3]);
            #region 生成可显示图像
            // Define parameters used to create the BitmapSource.
            int width = GetDscWidth();
            int height = GetDscHeight();
            int rawStride = (width * PixelFormats.Bgra32.BitsPerPixel + 7) / 8;
            int rowPixel = 0;
            int colPixel = 0;


            byte[] widthMsg = Encoding.UTF8.GetBytes(width.ToString());
            byte[] heightMsg = Encoding.UTF8.GetBytes(height.ToString());
            byte[] imageMsg = new byte[pixels.Length + widthMsg.Length + heightMsg.Length];
            //public static void Copy(Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int length);
            //width:704 heigth:436



            //算法在此
            DateTime beforeDT = System.DateTime.Now;
            try
            {
                float vesselAbscissaTemp = Execute2(pixels, width, height);
                

                if (vesselAbscissaTemp > -2 && vesselAbscissaTemp < 2)
                {
                    //创建委托，指定任务
                    Func<string, string> showMessage1 = ShowMessage;
                    //耗时操作异步回调函数中执行
                    showMessage1.BeginInvoke("：这是一个委托任务的输入参数", new AsyncCallback((result) =>
                    {
                        vesselAbscissa = vesselAbscissaTemp;
                        //Console.WriteLine("血管横向偏移：" + string.Format("{0:N2}", vesselAbscissa));

                        vesselDepth = Execute1(pixels, width, height, test);
                        //Console.WriteLine("血管深度：" + string.Format("{0:N2}", vesselDepth));


                        vesselLWratio = Execute3(pixels, width, height);
                        //Console.WriteLine("血管长宽比：" + string.Format("{0:N2}", vesselLWratio));

                        vesselArea = Execute4(pixels, width, height);
                        //Console.WriteLine("血管面积：" + string.Format("{0:N2}", vesselArea) + "");
                        Console.WriteLine(showMessage1.EndInvoke(result));
                    }), null);

                }
                else
                {
                    vesselAbscissa = -5;
                    vesselDepth = -5;
                    vesselLWratio = -5;
                    vesselArea = -5;
                    //Console.WriteLine("图像过暗或横向偏移太大，重新计算");
                }

                //创建委托，指定任务
                Func<string, string> showMessage2 = ShowMessage;
                //耗时操作异步回调函数中执行
                showMessage2.BeginInvoke("：这是一个委托任务的输入参数", new AsyncCallback((result) =>
                {
                    //传输深度，宽高比，和面积信息
                    String strDepth = vesselDepth.ToString();
                    String strWHR = vesselLWratio.ToString();
                    String strArea = vesselArea.ToString();
                    String strAbscissa = vesselAbscissa.ToString();
                    String strAll = strDepth + "#" + strWHR + "#" + strArea + "#" + strAbscissa;
                    byte[] msgAll = Encoding.UTF8.GetBytes(strAll);
                    socketSendAll.Send(msgAll);
                    
                    rowPixel = (int)(vesselDepth * 22);
                    colPixel = (int)(vesselAbscissa * 23 + width / 2);
                    if (rowPixel > 5 && colPixel > 5)
                    {
                        //可视化
                        try
                        {
                            toolLock.EnterWriteLock();           // 获取写锁                                    
                            pixels[width * 4 * rowPixel + 4 * colPixel] = 255;
                            pixels[width * 4 * rowPixel + 4 * colPixel + 1] = 255;
                            pixels[width * 4 * rowPixel + 4 * colPixel + 2] = 255;
                            pixels[width * 4 * (rowPixel - 1) + 4 * colPixel] = 255;
                            pixels[width * 4 * (rowPixel - 1) + 4 * colPixel + 1] = 255;
                            pixels[width * 4 * (rowPixel - 1) + 4 * colPixel + 2] = 255;
                            pixels[width * 4 * (rowPixel + 1) + 4 * colPixel] = 255;
                            pixels[width * 4 * (rowPixel + 1) + 4 * colPixel + 1] = 255;
                            pixels[width * 4 * (rowPixel + 1) + 4 * colPixel + 2] = 255;
                            pixels[width * 4 * rowPixel + 4 * colPixel + 4 * 1] = 255;
                            pixels[width * 4 * rowPixel + 4 * colPixel + 4 * 1 + 1] = 255;
                            pixels[width * 4 * rowPixel + 4 * colPixel + 4 * 1 + 2] = 255;
                            pixels[width * 4 * rowPixel + 4 * colPixel + 4 * (-1)] = 255;
                            pixels[width * 4 * rowPixel + 4 * colPixel + 4 * (-1) - 1] = 255;
                            pixels[width * 4 * rowPixel + 4 * colPixel + 4 * (-1) - 2] = 255;

                        }
                        catch { }
                        finally
                        {
                            toolLock.ExitWriteLock();            // 释放写锁
                        }

                    }
                    //传输图片
                    Array.Copy(widthMsg, 0, imageMsg, 0, widthMsg.Length);
                    Array.Copy(heightMsg, 0, imageMsg, widthMsg.Length, heightMsg.Length);
                    Array.Copy(pixels, 0, imageMsg, widthMsg.Length + heightMsg.Length, pixels.Length);

                    socketSendImage.Send(imageMsg);
                    Console.WriteLine(showMessage2.EndInvoke(result));
                }), null);


            }
            catch (Exception ex)
            {

                Console.WriteLine($"ex:{ex}");
            }
            DateTime afterDT = System.DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforeDT);
            Console.WriteLine("DateTime costed for function is: {0}ms\n", ts.TotalMilliseconds);


            //Console.WriteLine("width:" + width);
            //Console.WriteLine("height:" + height);

            BitmapSource Sbitmap = BitmapSource.Create(width, height,
                96, 96, PixelFormats.Bgra32, null,
                pixels, rawStride);

            #endregion
            return Sbitmap;
        }


        internal DSConverter FetchConverter(USProbe probe, int zoom)
        {
            if (zoom == -1)
            {
                Debug.Assert(zoom == -1);
            }
            Debug.Assert(probe != null);
            string key = probe.TransducerMark() + "_" + zoom;
            DSConverter converter = null;

            if (theConverterPool.ContainsKey(key))
            {
                converter = theConverterPool[key];
            }
            else
            {
                lock (DSConverter_lock)
                {
                    if (theConverterPool.ContainsKey(key))
                    {
                        converter = theConverterPool[key];
                    }
                    else
                    {
                        if (theConverterPool.Count > 3)
                        {
                            List<string> keytodelete = new List<string>();
                            foreach (var item in theConverterPool)
                            {
                                if (!item.Key.StartsWith(probe.TransducerMark()))
                                {
                                    keytodelete.Add(item.Key);
                                }
                            }
                            foreach (var item in keytodelete)
                            {
                                theConverterPool.Remove(item);
                            }
                        }
                        converter = new DSConverter();

                        USProbe.IMAGING_PARAMETER imaging_parameter = probe.getImagingParameterWithZoom(zoom);
                        converter.InitDSC(imaging_parameter.deadRegion,
                                imaging_parameter.sectorAngle,
                                (int)imaging_parameter.sampleRate(zoom),
                                imaging_parameter.lineCount,
                                imaging_parameter.sampleCount,
                                dscWidth, dscHeight,
                                imaging_parameter.usingSample[zoom],
                                0,
                                imaging_parameter.mapTable,
                                dscMode);

                        converter.PrepareSteerDSC();

                        if (theConverterPool.ContainsKey(key))
                        {
                            converter = theConverterPool[key];
                        }
                        else
                        {
                            theConverterPool.Add(key, converter);
                        }

                    }

                }
            }
            return converter;
        }

        public void ResetCurrentDSConverter(USProbe probe, int zoom)
        {
            DSConverter converter = FetchConverter(probe, zoom);
            currentConverter = converter;
        }

        public void ResetCurrentDSConverter(USRawImage rawImage)
        {
            ResetCurrentDSConverter(rawImage.probeCap, rawImage.zoom);
        }
        public struct USPoint
        {
            //public USPoint()
            //{

            //}
            public USPoint(double x, double y)
            {
                X = x;
                Y = y;
            }
            public double X;
            public double Y;
        }
        public USPoint GetCenterPoint()
        {
            DSConverter.IMAGE_DOT dot = currentConverter.CenterDot();
            USPoint ret = new USPoint();
            ret.X = (float)(dot.dbX * scaleExpand);
            ret.Y = (float)(dot.dbY * scaleExpand);
            return ret;
        }
        /// <summary>
        /// 返回结果为屏幕坐标点
        /// </summary>
        /// <param name="samplePoint"></param>
        /// <returns></returns>
        public USPoint DscMap(USPoint samplePoint)
        {
            SAMPLE_DOT sampDot = new SAMPLE_DOT();
            sampDot.dbLineIndex = samplePoint.X;
            sampDot.dbSampleIndex = samplePoint.Y;
            IMAGE_DOT imgDot = currentConverter.DSCMap(sampDot);
            USPoint ret = new USPoint();
            ret.X = (float)(imgDot.dbX * scaleExpand);
            ret.Y = (float)(imgDot.dbY * scaleExpand);
            return ret;
        }

        /// <summary>
        /// 获取dsc点（非屏幕上点）
        /// </summary>
        /// <param name="pImageDot"></param>
        /// <returns></returns>
        public USPoint ReDSCMap(USPoint imagePoint)
        {
            DSConverter.IMAGE_DOT imgDot = new DSConverter.IMAGE_DOT();
            imgDot.dbX = imagePoint.X / scaleExpand;
            imgDot.dbY = imagePoint.Y / scaleExpand;
            DSConverter.SAMPLE_DOT sampDot = currentConverter.ReDSCMap(imgDot);
            USPoint ret = new USPoint();
            ret.X = (float)sampDot.dbLineIndex;
            ret.Y = (float)sampDot.dbSampleIndex;
            return ret;
        }
        /// <summary>
        /// 获取dsc点（非屏幕上点）
        /// </summary>
        /// <param name="imagePoint"></param>
        /// <param name="steer"></param>
        /// <returns></returns>

        public double M_dbScalePixel
        {
            get
            {
                if (currentConverter == null)
                {
                    return 0.2;
                }
                return currentConverter.GetM_dbScalePixel() / scaleExpand;
            }
        }
        public BitmapSource GetGrayBar(USProbe probe)
        {
            int[] grayBar = new int[256];
            if (probe == null)
            {
                for (int i = 0; i < 256; i++)
                {
                    int gray = i;
                    grayBar[255 - i] = (255 << 24) + (i << 16) + (i << 8) + i;
                }
            }
            else
            {
                for (int i = 0; i < 256; i++)
                {
                    int gray = probe.imagingParameter.mapTable[i] / 256;
                    grayBar[255 - i] = (255 << 24) + (gray << 16) + (gray << 8) + gray;
                }
            }

            #region 生成可显示图像
            // Define parameters used to create the BitmapSource.
            int width = 1;
            int height = 256;
            int rawStride = (width * PixelFormats.Bgra32.BitsPerPixel + 7) / 8;
            //byte[] SrawImage = new byte[rawStride * height];

            BitmapSource Sbitmap = BitmapSource.Create(width, height,
                96, 96, PixelFormats.Bgra32, null,
                grayBar, rawStride);
            #endregion

            return Sbitmap;
        }


    }
}

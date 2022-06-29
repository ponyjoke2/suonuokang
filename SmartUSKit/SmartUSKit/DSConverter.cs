using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
//using static SmartUSKit_CS.SmartUSKit.USDSCor;

namespace SmartUSKit.SmartUSKit
{
    public class DSC_INDEX
    {
        public uint uiAlpha;
        public uint uiLine;
        public uint uiSample;
        public uint uiPartLine;
        public uint uiPartSample;
    }
    public class DSConverter
    {
        protected static readonly uint UINT_MAX = 0xFFFFFFFF;

        DSC_INDEX[] m_pDscIndex;
        DSC_INDEX[] m_pDscIndexPSeven;
        DSC_INDEX[] m_pDscIndexNSeven;
        DSC_INDEX[] m_pDscIndexP12;
        DSC_INDEX[] m_pDscIndexN12;

        //  凸阵形状参数
        protected double m_dbDeadRgn;           //  死区长度（单位：mm）
        protected double m_dbSectorAngle;       //  扇形角度（单位：弧度）
        //  采样参数
        protected double m_dbSampleRate;        //
        protected int m_nLineCnt;               //
        protected int m_nSampleCnt;             //
        //重建参数
        protected int m_nWidth;                 //
        protected int m_nHeight;                //
        protected int m_nUseSampleCnt;          //
        //编码模式
        protected int m_nCodingMode;            //
        protected int m_nPixelByteCnt;          //
        //Gama
        protected double m_dbGama;              //

        protected int[] m_MapTable;

        //  DSC结果
        protected byte[] m_pDSCedImage;

        //  比例尺
        protected double m_dbScaleSample;
        protected double m_dbScalePixel;


        public class SAMPLE_DOT
        {
            public double dbLineIndex;
            public double dbSampleIndex;
        }

        public class IMAGE_DOT
        {
            public double dbX;
            public double dbY;
        }

        //protected USPoint m_CenterDot = new USPoint();

        protected IMAGE_DOT m_CenterDot = new IMAGE_DOT();


        public bool InitDSC(    //  凸阵形状参数
                                double dbDeadRgn,       //  死区长度（单位：mm）
                                double dbSectorAngle,   //  扇形角度（单位：弧度）
                                                        //  采样参数
                                double dbSampleRate,    //  采样频率（单位：Hz）
                                int nLineCnt,           //  扫描线数
                                int nSampleCnt,         //  每条扫描线的采样点数
                                                        //  重建参数
                                int nWidth,             //  重建宽度（注意：重建图像宽度必须为偶数个像素点）
                                int nHeight,            //  重建高度
                                int nUseSampleCnt,      //  重建中使用的点数（一般情况下与采样点相同，但是对于SV-1和SF-1等某些深度，实际采样点只有部分被用于重建）
                                                        //  编码模式
                                int nCodingMode,
                                //  Gama映射
                                int[] mapTable,        //  灰度编码
                                                       //  对齐方式
                                int nDSCMode            //  0: MATCH_WIDTH ( 宽度对其） 1：MATCH_HEIGHT （高度对齐）
                                )
        {
            //  1. 读入参数
            //Debug.Assert(dbDeadRgn > 0);
            Debug.Assert(dbSectorAngle > 0);
            Debug.Assert(dbSampleRate > 0);
            Debug.Assert(nLineCnt > 0);
            Debug.Assert(nSampleCnt > 0);
            Debug.Assert(nWidth > 0);
            Debug.Assert(nWidth % 2 == 0);
            Debug.Assert(nHeight > 0);
            Debug.Assert(nUseSampleCnt > 0);
            Debug.Assert(nUseSampleCnt <= nSampleCnt);

            //Debug.Assert(nCodingMode == CODE_GRAY || nCodingMode == CODE_RGB || nCodingMode == CODE_RGBA);

            m_MapTable = mapTable;
            m_dbDeadRgn = dbDeadRgn;
            m_dbSectorAngle = dbSectorAngle;
            m_dbSampleRate = dbSampleRate;
            m_nLineCnt = nLineCnt;
            m_nSampleCnt = nSampleCnt;
            m_nWidth = nWidth;
            m_nHeight = nHeight;
            m_nUseSampleCnt = nUseSampleCnt;
            m_nCodingMode = nCodingMode;

            //  2. 分配空间用于保存系数和图像
            m_nPixelByteCnt = 4;    // BGRA
            m_pDSCedImage = new byte[(m_nWidth * m_nPixelByteCnt + 3) / 4 * 4 * m_nHeight];
            m_pDscIndex = new DSC_INDEX[m_nWidth / 2 * m_nHeight];
            for (int i = 0; i < m_pDscIndex.Length; i++)
            {
                m_pDscIndex[i] = new DSC_INDEX();
            }

            //  3. 计算比例尺
            m_dbScaleSample = (1560.0 * 1000.0) / 2.0 * 1.0 / m_dbSampleRate;   //  每个采样点对应多少mm
            double dbHeightInMM = m_dbDeadRgn * (1.0 - Math.Cos(m_dbSectorAngle / 2.0)) + m_dbScaleSample * (double)m_nUseSampleCnt;    //  图像对应多少mm
            m_dbScalePixel = dbHeightInMM / (double)m_nHeight;

            //  3.1 如果在宽度保留模式下，检测宽度，必要时调整比例尺
            {
                //......
            }

            //  4. 计算中心点（像素坐标）
            m_CenterDot.dbX = (double)(m_nWidth - 1.0) / 2.0;
            m_CenterDot.dbY = -m_dbDeadRgn * Math.Cos(m_dbSectorAngle / 2.0) / m_dbScalePixel;

            //  5. 计算系数矩阵
            int nIndex = 0;
            for (int y = 0; y < m_nHeight; y++)
            {
                for (int x = 0; x < m_nWidth / 2; x++)
                {
                    DSC_INDEX pDscIndex = m_pDscIndex[nIndex];
                    nIndex++;
                    IMAGE_DOT imgDot = new IMAGE_DOT();
                    imgDot.dbX = (double)x;
                    imgDot.dbY = (double)y;
                    SAMPLE_DOT smpDot = ReDSCMap(imgDot);

                    //USPoint imgDot = new USPoint();
                    //imgDot.X = (double)x; imgDot.Y = (double)y;
                    //USPoint smpDot = ReDSCMap(imgDot);

                    if ((smpDot.dbLineIndex <= -1.0)
                        || (smpDot.dbLineIndex >= (double)m_nLineCnt)
                        || (smpDot.dbSampleIndex <= -1.0)
                        || (smpDot.dbSampleIndex >= (double)m_nSampleCnt)
                        )
                    {
                        pDscIndex.uiAlpha = 0;
                        continue;
                    }
                    smpDot.dbLineIndex += 1.0;
                    smpDot.dbSampleIndex += 1.0;
                    pDscIndex.uiLine = (uint)(smpDot.dbLineIndex);
                    Debug.Assert(pDscIndex.uiLine <= m_nLineCnt / 2);
                    pDscIndex.uiSample = (uint)(smpDot.dbSampleIndex);
                    double partline = smpDot.dbLineIndex - (double)(pDscIndex.uiLine);
                    double partsample = smpDot.dbSampleIndex - (double)(pDscIndex.uiSample);
                    pDscIndex.uiPartLine = (uint)(partline * 255.999999);
                    pDscIndex.uiPartSample = (uint)(partsample * 255.999999);
                    if (pDscIndex.uiLine > 0
                        && pDscIndex.uiSample > 0
                        && pDscIndex.uiSample < m_nSampleCnt
                        )
                    {
                        pDscIndex.uiAlpha = 255;
                    }
                    else
                    {
                        double tl, tr, bl, br;
                        tl = tr = bl = br = 0;
                        if (pDscIndex.uiLine <= 0)
                        {
                            tl = bl = 0.0;
                        }
                        if (pDscIndex.uiSample <= 0)
                        {
                            tl = tr = 0.0;
                        }
                        else if (pDscIndex.uiSample >= m_nSampleCnt)
                        {
                            bl = br = 0.0;
                        }
                        double alpha = (tl * (1.0 - partline) + tr * partline) * (1.0 - partsample)
                                    + (bl * (1.0 - partline) + br * partline) * partsample;
                        alpha *= 255.9999;
                        if (alpha < 0.0)
                        {
                            pDscIndex.uiAlpha = 0;
                        }
                        else if (alpha > 255.0)
                        {
                            pDscIndex.uiAlpha = 255;
                        }
                        else
                        {
                            pDscIndex.uiAlpha = (uint)alpha;
                        }
                    }
                }
            }
            return true;
        }


        public bool PrepareSteerDSC()
        {
            m_pDscIndexPSeven = new DSC_INDEX[m_nWidth * m_nHeight];
            m_pDscIndexNSeven = new DSC_INDEX[m_nWidth * m_nHeight];
            m_pDscIndexP12 = new DSC_INDEX[m_nWidth * m_nHeight];
            m_pDscIndexN12 = new DSC_INDEX[m_nWidth * m_nHeight];
            for (int i = 0; i < m_nWidth * m_nHeight; i++)
            {
                m_pDscIndexNSeven[i] = new DSC_INDEX();
                m_pDscIndexPSeven[i] = new DSC_INDEX();
                m_pDscIndexP12[i] = new DSC_INDEX();
                m_pDscIndexN12[i] = new DSC_INDEX();
            }
            for (int y = 0; y < m_nHeight; y++)
            {
                for (int x = 0; x < m_nWidth; x++)
                {
                    int pos = y * m_nWidth + x;
                    IMAGE_DOT imgDot = new IMAGE_DOT();
                    imgDot.dbX = x; imgDot.dbY = y;

                    //  Steer: 7°
                    DSC_INDEX pDscIndex = m_pDscIndexPSeven[pos];// = new DSC_INDEX();
                    SAMPLE_DOT sampDot = SteerReDSCMap(imgDot, 7);
                    if (sampDot.dbLineIndex < 0 || sampDot.dbLineIndex > m_nLineCnt - 1)
                    {
                        pDscIndex.uiAlpha = 0;
                    }
                    else
                    {
                        pDscIndex.uiAlpha = 255;
                        pDscIndex.uiLine = (uint)(sampDot.dbLineIndex + 1.0);
                        pDscIndex.uiPartLine = (uint)((1.0 - ((float)(pDscIndex.uiLine) - sampDot.dbLineIndex)) * 255.0);
                        pDscIndex.uiSample = (uint)(sampDot.dbSampleIndex + 1.0);
                        pDscIndex.uiPartSample = (uint)((1.0 - ((float)(pDscIndex.uiSample) - sampDot.dbSampleIndex)) * 255.0);
                    }

                    //  Steer: -7°
                    pDscIndex = m_pDscIndexNSeven[pos];// = new DSC_INDEX();
                    imgDot.dbX = x; imgDot.dbY = y;
                    sampDot = SteerReDSCMap(imgDot, -7);
                    if (sampDot.dbLineIndex < 0 || sampDot.dbLineIndex > m_nLineCnt - 1)
                    {
                        pDscIndex.uiAlpha = 0;
                    }
                    else
                    {
                        pDscIndex.uiAlpha = 255;
                        pDscIndex.uiLine = (uint)(sampDot.dbLineIndex + 1.0);
                        pDscIndex.uiPartLine = (uint)((1.0 - ((float)(pDscIndex.uiLine) - sampDot.dbLineIndex)) * 255.0);
                        pDscIndex.uiSample = (uint)(sampDot.dbSampleIndex + 1.0);
                        pDscIndex.uiPartSample = (uint)((1.0 - ((float)(pDscIndex.uiSample) - sampDot.dbSampleIndex)) * 255.0);
                    }

                    //  Steer: +12°
                    pDscIndex = m_pDscIndexP12[pos];// = new DSC_INDEX();
                    imgDot.dbX = x; imgDot.dbY = y;
                    sampDot = SteerReDSCMap(imgDot, +12);
                    if (sampDot.dbLineIndex < 0 || sampDot.dbLineIndex > m_nLineCnt - 1)
                    {
                        pDscIndex.uiAlpha = 0;
                    }
                    else
                    {
                        pDscIndex.uiAlpha = 255;
                        pDscIndex.uiLine = (uint)(sampDot.dbLineIndex + 1.0);
                        pDscIndex.uiPartLine = (uint)((1.0 - ((float)(pDscIndex.uiLine) - sampDot.dbLineIndex)) * 255.0);
                        pDscIndex.uiSample = (uint)(sampDot.dbSampleIndex + 1.0);
                        pDscIndex.uiPartSample = (uint)((1.0 - ((float)(pDscIndex.uiSample) - sampDot.dbSampleIndex)) * 255.0);
                    }

                    //  Steer: -12°
                    pDscIndex = m_pDscIndexN12[pos];// = new DSC_INDEX();
                    imgDot.dbX = x; imgDot.dbY = y;
                    sampDot = SteerReDSCMap(imgDot, -12);
                    if (sampDot.dbLineIndex < 0 || sampDot.dbLineIndex > m_nLineCnt - 1)
                    {
                        pDscIndex.uiAlpha = 0;
                    }
                    else
                    {
                        pDscIndex.uiAlpha = 255;
                        pDscIndex.uiLine = (uint)(sampDot.dbLineIndex + 1.0);
                        pDscIndex.uiPartLine = (uint)((1.0 - ((float)(pDscIndex.uiLine) - sampDot.dbLineIndex)) * 255.0);
                        pDscIndex.uiSample = (uint)(sampDot.dbSampleIndex + 1.0);
                        pDscIndex.uiPartSample = (uint)((1.0 - ((float)(pDscIndex.uiSample) - sampDot.dbSampleIndex)) * 255.0);
                    }
                }
            }
            return true;
        }

        public byte[] DSC(byte[] pRawData)
        {
            Debug.Assert(pRawData != null);
            Debug.Assert(pRawData.Length >= m_nLineCnt * m_nSampleCnt);

            //for (int i = 0; i < m_pDSCedImage.Length; i++)
            //{
            //    m_pDSCedImage[i] = 0;
            //}

            int nDscIndexPos = 0;
            int nImagePos = 0;
            for (int y = 0; y < m_nHeight; y++)
            {
                nImagePos = (m_nWidth * m_nPixelByteCnt + 3) / 4 * 4 * y;   //  4字节对齐
                int nLeftPos = nImagePos;
                int nRightPos = nImagePos + (m_nWidth - 1) * m_nPixelByteCnt;
                for (int x = 0; x < m_nWidth / 2; x++)
                {
                    DSC_INDEX pDscIndex = m_pDscIndex[nDscIndexPos]; nDscIndexPos++;
                    if (pDscIndex.uiAlpha == 0)
                    {
                        //  有效图像区以外，透明

                        m_pDSCedImage[nRightPos + 3] = 255;
                        m_pDSCedImage[nLeftPos + 3] = 255;

                    }
                    else
                    {
                        uint tl, tr, bl, br;
                        uint l, r;
                        uint gray;
                        //  左半部分
                        {
                            br = (uint)(pDscIndex.uiLine * m_nSampleCnt + pDscIndex.uiSample);
                            bl = (uint)(br - m_nSampleCnt);
                            tr = br - 1;
                            tl = tr - (uint)m_nSampleCnt;
                            if (pDscIndex.uiLine == 0)
                            {
                                tl = bl = UINT_MAX;
                            }
                            if (pDscIndex.uiSample == 0)
                            {
                                tl = tr = UINT_MAX;
                            }
                            else if (pDscIndex.uiSample >= m_nSampleCnt)
                            {
                                bl = br = UINT_MAX;
                            }
                            tl = (tl == UINT_MAX) ? (uint)0 : (uint)m_MapTable[pRawData[tl]];
                            tr = (tr == UINT_MAX) ? (uint)0 : (uint)m_MapTable[pRawData[tr]];
                            bl = (bl == UINT_MAX) ? (uint)0 : (uint)m_MapTable[pRawData[bl]];
                            br = (br == UINT_MAX) ? (uint)0 : (uint)m_MapTable[pRawData[br]];
                            l = (tl * (255 - pDscIndex.uiPartSample) + bl * pDscIndex.uiPartSample) >> 8;
                            r = (tr * (255 - pDscIndex.uiPartSample) + br * pDscIndex.uiPartSample) >> 8;
                            gray = l * (255 - pDscIndex.uiPartLine) + r * pDscIndex.uiPartLine;
                            gray >>= 16;
                            if (gray > 255)
                            {
                                gray = 255;
                            }

                            m_pDSCedImage[nLeftPos + 3] = (byte)pDscIndex.uiAlpha;
                            m_pDSCedImage[nLeftPos] = (byte)gray;
                            m_pDSCedImage[nLeftPos + 1] = (byte)gray;
                            m_pDSCedImage[nLeftPos + 2] = (byte)gray;
                        }
                        //  右半部分
                        {
                            br = (uint)((m_nLineCnt - 1 - pDscIndex.uiLine) * m_nSampleCnt + pDscIndex.uiSample);
                            bl = (uint)(br + m_nSampleCnt);
                            tr = br - 1;
                            tl = (uint)(tr + m_nSampleCnt);

                            if (pDscIndex.uiLine == 0)
                            {
                                tl = bl = UINT_MAX;
                            }
                            if (pDscIndex.uiSample == 0)
                            {
                                tl = tr = UINT_MAX;
                            }
                            else if (pDscIndex.uiSample >= m_nSampleCnt)
                            {
                                bl = br = UINT_MAX;
                            }

                            tl = (tl == UINT_MAX) ? (uint)0 : (uint)m_MapTable[pRawData[tl]];
                            tr = (tr == UINT_MAX) ? (uint)0 : (uint)m_MapTable[pRawData[tr]];
                            bl = (bl == UINT_MAX) ? (uint)0 : (uint)m_MapTable[pRawData[bl]];
                            br = (br == UINT_MAX) ? (uint)0 : (uint)m_MapTable[pRawData[br]];

                            l = (tl * (255 - pDscIndex.uiPartSample) + bl * pDscIndex.uiPartSample) >> 8;
                            r = (tr * (255 - pDscIndex.uiPartSample) + br * pDscIndex.uiPartSample) >> 8;
                            gray = (l * (255 - pDscIndex.uiPartLine) + r * pDscIndex.uiPartLine) >> 16;
                            if (gray > 255)
                            {
                                gray = 255;
                            }

                            m_pDSCedImage[nRightPos + 3] = (byte)pDscIndex.uiAlpha;
                            m_pDSCedImage[nRightPos] = (byte)gray;
                            m_pDSCedImage[nRightPos + 1] = (byte)gray;
                            m_pDSCedImage[nRightPos + 2] = (byte)gray;

                        }
                    }
                    nLeftPos += m_nPixelByteCnt;
                    nRightPos -= m_nPixelByteCnt;
                }
            }
            return m_pDSCedImage;
        }

        public IMAGE_DOT DSCMap(SAMPLE_DOT pSampleDot)
        {
            Debug.Assert(pSampleDot != null);

            //  计算像素坐标下，极坐标参数（以圆心为原点）
            double radius = (pSampleDot.dbSampleIndex * m_dbScaleSample + m_dbDeadRgn);
            double angle = m_dbSectorAngle / (double)(m_nLineCnt - 1) * ((double)(pSampleDot.dbLineIndex) - (double)(m_nLineCnt - 1) / 2.0);
            double dbX = radius * Math.Sin(angle);
            double dbY = radius * Math.Cos(angle);
            IMAGE_DOT imgDot = new IMAGE_DOT();
            imgDot.dbX = dbX / m_dbScalePixel + m_CenterDot.dbX;
            imgDot.dbY = dbY / m_dbScalePixel + m_CenterDot.dbY;
            return imgDot;
        }

        public SAMPLE_DOT ReDSCMap(IMAGE_DOT pImageDot)
        {
            Debug.Assert(pImageDot != null);
            //  计算mm单位下，极坐标参数（以原点为中心）
            double dbX = (pImageDot.dbX - m_CenterDot.dbX) * m_dbScalePixel;
            double dbY = (pImageDot.dbY - m_CenterDot.dbY) * m_dbScalePixel;
            double radius = Math.Sqrt(dbX * dbX + dbY * dbY);
            double angle = Math.Atan(dbX / dbY);
            double sampleindex = (radius - m_dbDeadRgn) / m_dbScaleSample;
            double lineindex = (m_dbSectorAngle / 2.0 + angle) / (m_dbSectorAngle / (double)(m_nLineCnt - 1.0));
            SAMPLE_DOT sampDot = new SAMPLE_DOT();
            sampDot.dbLineIndex = lineindex;
            sampDot.dbSampleIndex = sampleindex;
            return sampDot;
        }

        private int prevSteer = 0;
        private double cos_angle = Math.Cos(0);
        private double tan_angle = Math.Tan(0);
        public SAMPLE_DOT SteerReDSCMap(IMAGE_DOT pImageDot, int nSteer)
        {
            if (nSteer != prevSteer)
            {
                prevSteer = nSteer;
                double angle = (double)(nSteer / 180.0) * Math.PI;
                cos_angle = Math.Cos(angle);
                tan_angle = Math.Tan(angle);
            }
            double r = pImageDot.dbY / cos_angle;
            double deltaX = pImageDot.dbY * tan_angle;

            IMAGE_DOT imgDot = new IMAGE_DOT();
            imgDot.dbX = pImageDot.dbX + deltaX;
            imgDot.dbY = r;

            SAMPLE_DOT sampDot = ReDSCMap(imgDot);
            return sampDot;
        }

        public IMAGE_DOT CenterDot()
        {
            return m_CenterDot;
        }
        public double GetM_dbScaleSample()
        {
            return m_dbScaleSample;
        }
        public double GetM_dbScalePixel()
        {
            return m_dbScalePixel;
        }
    }
}

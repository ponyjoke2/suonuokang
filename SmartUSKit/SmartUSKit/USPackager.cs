using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SmartUSKit.SmartUSKit
{
    public class USPackager
    {
        internal int lineCount;
        protected int suffixLineCount;
        protected int nPackageRev;
        protected bool flipOver;
        protected bool lineMean;

        protected int m_nPackSize;
        protected byte[] m_pRawImg;     //  Raw Image Buffer for output

        protected int m_nStreamCap;
        protected int m_nStreamLen;
        protected byte[] m_pStreamBuf;
        protected int m_nNeedFrame = 0;
        protected int m_nNeedLine = 0;
        protected byte[] tmpBuffers;

        internal DateTime capTime;
        protected byte cmpdAngleIndex;

        public const int PACKAGE_ING = 0;
        public const int PACKAGE_TIME_CAP = 1;
        public const int PACKAGE_SUCC = 2;
        public const int PACKAGE_SUCC_WITH_SUFFIX = 3;
       
        protected USPackager()
        {
        }

        public USPackager(int line, int suffixline)
        {
            lineCount = line;
            suffixLineCount = suffixline;

            m_nStreamLen = 0;
            m_nStreamCap = (lineCount + suffixLineCount) * 512 * 2;
            m_pStreamBuf = new byte[m_nStreamCap];

            m_nPackSize = (lineCount + suffixLineCount) * 512;
            m_pRawImg = new byte[m_nPackSize];
            tmpBuffers = new byte[m_nPackSize];
        }
        
        public virtual int Package(byte[] data)
        {
            //  将新数据加入打包数据流缓存器中
            if (data.Length + m_nStreamLen >= m_nStreamCap)
            {
                return PACKAGE_ING;
            }
            //for (int i=0; i<data.Length; i++)
            //{
            //    m_pStreamBuf[m_nStreamLen + i] = data[i];
            //}
            System.Array.Copy(data, 0, m_pStreamBuf, m_nStreamLen, data.Length);
            m_nStreamLen += data.Length;

            int ret = PACKAGE_ING;
            //  解析数据流
            while (m_nStreamLen >= 525)
            {
                //  查找数据包头
                int head;
                for (head = 0; head <= m_nStreamLen - 8; head++)
                {
                    if ((m_pStreamBuf[head + 0] == (byte)0x5A)
                        && (m_pStreamBuf[head + 1] == (byte)0xA5)
                        && (m_pStreamBuf[head + 2] == (byte)0xFF)
                        && (m_pStreamBuf[head + 3] == (byte)0x00)
                        && (m_pStreamBuf[head + 4] == (byte)0x5A)
                        && (m_pStreamBuf[head + 5] == (byte)0xA5)
                        && (m_pStreamBuf[head + 6] == (byte)0xFF)
                        && (m_pStreamBuf[head + 7] == (byte)0x00))
                    {
                        if (head > 0)
                        {
                            m_nStreamLen -= head;
                            //for (int i=0; i<m_nStreamLen; i++)
                            //{
                            //    m_pStreamBuf[i] = m_pStreamBuf[i + head];
                            //}
                            System.Array.Copy(m_pStreamBuf, head, m_pStreamBuf, 0, m_nStreamLen);
                        }
                        head = 0;
                        break;
                    }
                }

                //  一直没有找到头：丢弃前面的一段
                if (head >= m_nStreamLen - 8)
                {
                    m_nStreamLen -= head;
                    for (int i = 0; i < m_nStreamLen; i++)
                    {
                        m_pStreamBuf[i] = m_pStreamBuf[i + head];
                    }
                    break;
                }

                //  数据流中的数据长度超过525则表示数据流中至少有一个完整的数据包
                if (m_nStreamLen >= 525)
                {
                    //  提取帧号与线号
                    int nCurFrame = m_pStreamBuf[8] & 0xFF;
                    int nCurLine = m_pStreamBuf[9] & 0xFF;

                    if (nPackageRev == 1)
                    {   //  线数超过256时，需要用到Frame的低4位
                        nCurLine |= (nCurFrame & 0x000F) << 8;
                        nCurFrame = nCurFrame & 0xF0;
                    }

                    //if (m_nNeedFrame == nCurFrame && m_nNeedLine == nCurLine)
                    if (m_nNeedFrame == nCurFrame && m_nNeedLine == nCurLine && (m_nNeedLine * 512 + 512) <= m_pRawImg.Length)
                    {
                        //for (int i = 0; i < 512; i++)
                        //{
                        //    m_pRawImg[512 * m_nNeedLine + i] = m_pStreamBuf[10 + i];
                        //}
                        System.Array.Copy(m_pStreamBuf, 10, m_pRawImg, 512 * m_nNeedLine, 512);

                        m_nStreamLen -= 525;
                        //for (int i = 0; i < m_nStreamLen; i++)
                        //{
                        //    m_pStreamBuf[i] = m_pStreamBuf[i + 525];
                        //}
                        System.Array.Copy(m_pStreamBuf, 525, m_pStreamBuf, 0, m_nStreamLen);

                        m_nNeedLine++;
                        if (m_nNeedLine == 1)
                        {
                            cmpdAngleIndex = (byte)(nCurFrame & 0xFF);
                            //  给数据打时间戳的时机
                            //capTime = new DateTime();
                            capTime = DateTime.Now;

                            if (nPackageRev == 1)
                            {
                                cmpdAngleIndex = (byte)(nCurFrame & 0xF0);
                            }

                            return PACKAGE_TIME_CAP;
                        }
                    }
                    else
                    {
                        if (suffixLineCount > 0 && m_nNeedLine >= lineCount + suffixLineCount)
                        {
                            ret = PACKAGE_SUCC_WITH_SUFFIX;
                        }
                        else if (m_nNeedLine >= lineCount)
                        {
                            ret = PACKAGE_SUCC;
                        }
                        else
                        {
                            ret = PACKAGE_ING;
                        }

                        m_nNeedFrame = nCurFrame;
                        m_nNeedLine = 0;
                        if (m_nNeedLine != nCurLine)
                        {
                            m_nStreamLen -= 525;
                            //for (int i=0; i<m_nStreamLen;i++)
                            //{
                            //    m_pStreamBuf[i] = m_pStreamBuf[i + 525];
                            //}
                            System.Array.Copy(m_pStreamBuf, 525, m_pStreamBuf, 0, m_nStreamLen);
                        }

                        if (ret >= PACKAGE_SUCC)
                        {
                            break;
                        }
                    }
                }
            }

            return ret;
        }

        public DateTime CaptureTime()
        {
            return capTime;
        }

        public virtual byte CompoundAngleIndex()
        {
            return cmpdAngleIndex;
        }

        public virtual byte[] RawData()
        {
            byte[] rawImg = new byte[lineCount * 512];
            //for (int i=0; i<lineCount*512; i++)
            //{
            //    rawImg[i] = m_pRawImg[i];
            //}
            //数组拷贝的方式更快一些
            System.Array.Copy(m_pRawImg, rawImg, rawImg.Length);

            if (flipOver)
            {
                rawImg = FlipOverData(rawImg, lineCount, 512);
            }
            if (lineMean)
            {
                //byte[] tmp = new byte[lineCount * 512];
            }
            return rawImg;
        }

        protected byte[] FlipOverData(byte[] srcData, int lineCount, int sampleCount)
        {
            byte[] dstData = new byte[srcData.Length];
            for (int l = 0; l < lineCount; l++)
            {
                for (int s = 0; s < sampleCount; s++)
                {
                    dstData[l * sampleCount + s] = srcData[(lineCount - 1 - l) * sampleCount + s];
                }
            }
            return dstData;
        }
        
    }
}

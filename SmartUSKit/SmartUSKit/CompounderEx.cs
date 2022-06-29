using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class CompounderEx : Object
    {
        private int m_nLine;
        private int m_nSample;
        private double m_dbDeadRgn;
        private double m_dbScanAngle;
        private double m_dbSteerAngle;
        private double m_dbSampleScale;

        protected class COMPOUND_INDEX
        {
            public bool covered;
            public short line;
            public short sample;
            public short partial;
            public short linePartial;
            public short samplePartial;
        };

        private const short COMPOUND_SMOOTH_SIZE = 4;

        byte[] m_pCompoundData = null;
        COMPOUND_INDEX[] m_pCmpIndexLeft = null;
        COMPOUND_INDEX[] m_pCmpIndexRight = null;

        public CompounderEx()
        {
        }

        public CompounderEx(int line,           //  线数
                                int sample,         //  采样数
                                double deadRegion,     //  死区
                                double scanAngle,      //  扫描角度
                                double steerAngle,     //  偏转角度（弧度）
                                double sampleScale)    //  采样点比例尺（mm/sample）
        {
            if ((m_pCompoundData == null)
                || (m_nLine != line)
                || (m_nSample != sample)
                    )
            {
                m_nLine = line;
                m_nSample = sample;
                m_pCompoundData = new byte[m_nLine * m_nSample];
            }

            m_dbDeadRgn = deadRegion;
            m_dbScanAngle = scanAngle;
            m_dbSteerAngle = steerAngle;
            m_dbSampleScale = sampleScale;
            m_pCmpIndexLeft = null;
            m_pCmpIndexRight = null;

            m_pCmpIndexLeft = new COMPOUND_INDEX[line * sample];
            m_pCmpIndexRight = new COMPOUND_INDEX[line * sample];

            //  计算系数
            InitCompoundIndexes();
        }

        protected void InitCompoundIndexes()
        {
            double theta = this.m_dbSteerAngle;    //  偏转角度（旋转三角形的顶角）
            double theta_grid = (double)(this.m_dbScanAngle / (m_nLine - 1));
            double R = this.m_dbDeadRgn;
            /*double L_base = Math.sin(theta/2.0) * R * 2.0;   //  底边长度
            double theta_base = (Math.PI - theta)/2.0;       //  底角
            double R_tang = L_base * Math.sin(theta_base);   //  相切圆半径*/
            double R_tang = R * Math.Sin(theta);
            double angle_start = Math.Acos(R_tang / R);

            for (int s = 0; s < m_nSample; s++)
            {
                double radius = R + s * m_dbSampleScale;
                short partial = 0;
                for (int l = m_nLine - 1; l >= 0; l--)
                {
                    double alfa = l * theta_grid;
                    double angle = Math.Acos(R_tang / radius) + alfa;
                    // calc for line
                    double steer_line = (angle - angle_start) / theta_grid;
                    // calc for sample
                    double steer_sample = (double)s / Math.Cos(m_dbSteerAngle);

                    COMPOUND_INDEX cmpIndex = new COMPOUND_INDEX();
                    //memset(&cmpIndex,0,sizeof(COMPOUND_INDEX));
                    int nLine = (int)steer_line;
                    int nSample = (int)steer_sample;
                    if (nLine < 0
                            || nLine >= m_nLine - 1
                            || nSample < 0
                            //|| nSample >= m_nSample - 1
                            )
                    {
                        cmpIndex.covered = false;
                        cmpIndex.partial = partial;
                    }
                    else
                    {
                        if (partial < COMPOUND_SMOOTH_SIZE)
                        {
                            partial++;
                        }

                        //  处理最后一个采样点的问题
                        if (nSample >= m_nSample - 1)
                        {
                            nSample = m_nSample - 1;
                            steer_sample = nSample;
                        }

                        cmpIndex.covered = true;
                        cmpIndex.partial = partial;
                        int nLinePartial = (int)((nLine + 1.0 - steer_line) * 256.0);
                        int nSamplePartial = (int)((nSample + 1.0 - steer_sample) * 256.0);
                        cmpIndex.line = (short)nLine;
                        cmpIndex.linePartial = (short)nLinePartial;
                        cmpIndex.sample = (short)nSample;
                        cmpIndex.samplePartial = (short)nSamplePartial;
                    }

                    m_pCmpIndexLeft[l * m_nSample + s] = cmpIndex;

                    //
                    COMPOUND_INDEX cmpIndexMirror = new COMPOUND_INDEX();
                    //COMPOUND_INDEX cmpIndexMirror = cmpIndex;


                    cmpIndexMirror.covered = cmpIndex.covered;
                    cmpIndexMirror.partial = cmpIndex.partial;
                    cmpIndexMirror.sample = cmpIndex.sample;
                    cmpIndexMirror.samplePartial = cmpIndex.samplePartial;


                    cmpIndexMirror.line = (short)(m_nLine - 1 - cmpIndex.line);
                    cmpIndexMirror.linePartial = (short)(256 - cmpIndex.linePartial);
                    m_pCmpIndexRight[(m_nLine - 1 - l) * m_nSample + s] = cmpIndexMirror;
                }
            }
        }

        public byte[] CompoundData(byte[] pLeftData, byte[] pMidData, byte[] pRightData)
        {
            for (int i = 0; i < m_nLine * m_nSample; i++)
            {
                int sum = (pMidData[i] & 0xFF) * COMPOUND_SMOOTH_SIZE;
                int div = COMPOUND_SMOOTH_SIZE;

                if (m_pCmpIndexLeft[i].covered)
                {
                    COMPOUND_INDEX cmpIndex = m_pCmpIndexLeft[i];
                    int nLine = cmpIndex.line;
                    int nSample = cmpIndex.sample;

                    int index = nLine * m_nSample + nSample;
                    int indexTL = index;
                    int indexTR = index;
                    int indexBL = index;
                    int indexBR = index;
                    if (nLine < m_nLine - 1)
                    {
                        indexTR += m_nSample;
                        indexBR += m_nSample;
                    }
                    if (nSample < m_nSample - 1)
                    {
                        indexBL += 1;
                        indexBR += 1;
                    }
                    short TL = (short)(pLeftData[indexTL] & 0x00FF);
                    short TR = (short)(pLeftData[indexTR] & 0x00FF);
                    short BL = (short)(pLeftData[indexBL] & 0x00FF);
                    short BR = (short)(pLeftData[indexBR] & 0x00FF);
                   
                    int L = TL * cmpIndex.samplePartial + BL * (256 - cmpIndex.samplePartial);
                    int R = TR * cmpIndex.samplePartial + BR * (256 - cmpIndex.samplePartial);
                    int inter = L * cmpIndex.linePartial + R * (256 - cmpIndex.linePartial);

                    int left = (inter >> 16);
                    left = left > 255 ? 255 : left;
                    sum += left * cmpIndex.partial;
                    div += cmpIndex.partial;
                }

                if (m_pCmpIndexRight[i].covered)
                {
                    COMPOUND_INDEX cmpIndex = m_pCmpIndexRight[i];
                    int nLine = cmpIndex.line;
                    int nSample = cmpIndex.sample;

                    int index = nLine * m_nSample + nSample;
                    int indexTL = index;
                    int indexTR = index;
                    int indexBL = index;
                    int indexBR = index;
                    if (nLine < m_nLine - 1)
                    {
                        indexTR += m_nSample;
                        indexBR += m_nSample;
                    }
                    if (nSample < m_nSample - 1)
                    {
                        indexBL += 1;
                        indexBR += 1;
                    }

                    short TL = (short)(pRightData[indexTL] & 0x00FF);
                    short TR = (short)(pRightData[indexTR] & 0x00FF);
                    short BL = (short)(pRightData[indexBL] & 0x00FF);
                    short BR = (short)(pRightData[indexBR] & 0x00FF);

                    int L = TL * cmpIndex.samplePartial + BL * (256 - cmpIndex.samplePartial);
                    int R = TR * cmpIndex.samplePartial + BR * (256 - cmpIndex.samplePartial);
                    int inter = L * cmpIndex.linePartial + R * (256 - cmpIndex.linePartial);

                    int right = (inter >> 16);
                    right = right > 255 ? 255 : right;
                    sum += right * cmpIndex.partial;
                    div += cmpIndex.partial;
                }

                int Vout = sum / div;
                Vout = Vout > 255 ? 255 : Vout;
                Vout = Vout < 0 ? 0 : Vout;

                m_pCompoundData[i] = (byte)(Vout & 0xFF);
            }
            return m_pCompoundData;
        }
    }

}

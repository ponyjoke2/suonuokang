using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    internal class Compounder
    {
        protected int m_nLine;
        protected int m_nSample;
        protected float m_fPitch;
        protected float m_fAngle;
        protected float m_fSampleScale;

        protected struct CMPINDEX
        {
            public int division;
            public int leftLine;
            public int leftSample;
            public int leftPartial;
            public int rightLine;
            public int rightSample;
            public int rightPartial;
            public int leftIndex;
            public int midIndex;
            public int rightIndex;
        };
        protected CMPINDEX[] m_pCmpIndexes;
        protected byte[] m_pCompoundData;

        //
        //  穿刺相关参数
        //
        protected byte[] m_pBiopsyEnhanceFrame;
        protected float m_fBiopsyPosition;
        protected int m_nBiopsyAngle;
        protected byte[] m_pBiopsyMaskFrame;
        protected byte[] m_pCompoundBiopsy;

        public Compounder()
        {
            m_nLine = 0;
            m_nSample = 0;
            m_fPitch = 0;
            m_fAngle = 0;
            m_fSampleScale = 0;
        }

        public Compounder(int line,               //  线数
                            int sample,             //  每线采样数
                            float pitch,            //  阵元间距（mm）
                            float angle,            //  偏转角度（弧度）
                            float sampleScale       //  采样点比例尺（mm/sample)
)
        {
            m_nLine = line;
            m_nSample = sample;
            m_fPitch = pitch;
            m_fAngle = angle;
            m_fSampleScale = sampleScale;

            m_pCompoundData = new byte[m_nLine * m_nSample];
            m_pCmpIndexes = new CMPINDEX[m_nLine * m_nSample];
            InitCompoundIndexes();

            m_pBiopsyEnhanceFrame = new byte[m_nLine * m_nSample];
            m_pBiopsyMaskFrame = new byte[m_nLine * m_nSample];
            m_pCompoundBiopsy = new byte[m_nLine * m_nSample];
        }

        protected void InitCompoundIndexes()
        {
            int index = 0;
            float tan_m_fAngle=(float)Math.Tan(m_fAngle) ;
            float cos_m_fAngle = (float)Math.Cos(m_fAngle);
            for (int l = 0; l < m_nLine; l++)
            {
                for (int s = 0; s < m_nSample; s++)
                {
                    float y = s * m_fSampleScale;
                    float deltax = tan_m_fAngle * y;
                    float deltal = deltax / m_fPitch;
                    int partial = 32 - (int)((deltal - (float)((int)deltal)) * 32.0f);
                    float r = y / cos_m_fAngle;
                    int sample = (int)(r / m_fSampleScale + 0.5f);
                    int left = (int)((float)l + deltal);
                    int right = (int)((float)l - deltal);
                    int div = 3;
                    int midIndex = 16;
                    int leftIndex = 16;
                    int rightIndex = 16;
                    sample += 0;

                    if (sample >= m_nSample || sample < 0)
                    {
                        left = right = -1;
                        div = 1;
                        leftIndex = 0;
                        rightIndex = 0;
                    }
                    else
                    {
                        if (left < 0 || left >= m_nLine - 1)
                        {
                            left = -1;
                            div--;
                            leftIndex = 0;
                        }
                        else if (left < 16)
                        {
                            leftIndex = left;
                        }
                        else if (left >= m_nLine - 16)
                        {
                            leftIndex = m_nLine - 1 - left;
                        }
                        else
                        {
                            leftIndex = 16;
                        }

                        if (right < 0 || right >= m_nLine - 1)
                        {
                            right = -1;
                            div--;
                            rightIndex = 0;
                        }
                        else if (right < 16)
                        {
                            rightIndex = right;
                        }
                        else if (right >= m_nLine - 16)
                        {
                            rightIndex = m_nLine - 1 - right;
                        }
                        else
                        {
                            rightIndex = 16;
                        }
                    }
                    m_pCmpIndexes[index] = new CMPINDEX();
                    m_pCmpIndexes[index].leftLine = left;
                    m_pCmpIndexes[index].leftSample = sample;
                    m_pCmpIndexes[index].leftPartial = partial;
                    m_pCmpIndexes[index].rightLine = right;
                    m_pCmpIndexes[index].rightSample = sample;
                    m_pCmpIndexes[index].rightPartial = 32 - partial;
                    m_pCmpIndexes[index].division = div;
                    m_pCmpIndexes[index].midIndex = midIndex;
                    m_pCmpIndexes[index].leftIndex = leftIndex;
                    m_pCmpIndexes[index].rightIndex = rightIndex;

                    index++;
                }
            }
        }

        CMPINDEX pCmpIndex;
        public byte[] CompoundData(byte[] pLeftData, byte[] pMidData, byte[] pRightData)
        {
            int index = 0;
            for (int l = 0; l < m_nLine; l++)
            {
                for (int s = 0; s < m_nSample; s++)
                {
                    pCmpIndex = m_pCmpIndexes[index];
                    int sum = (int)(pMidData[index] & 0xFF) * pCmpIndex.midIndex;
                    int divid = pCmpIndex.midIndex;
                    if (pCmpIndex.leftLine >= 0)
                    {
                        int l_index = pCmpIndex.leftLine * m_nSample + pCmpIndex.leftSample;
                        int r_index = l_index + m_nSample;
                        l_index = (int)(pLeftData[l_index] & 0xFF);
                        r_index = (int)(pLeftData[r_index] & 0xFF);
                        int data = (l_index * pCmpIndex.leftPartial + r_index * (32 - pCmpIndex.leftPartial)) / 32;
                        if (data > 255)
                        {
                            data = 255;
                        }
                        sum += data * pCmpIndex.leftIndex;
                        divid += pCmpIndex.leftIndex;
                    }
                    if (pCmpIndex.rightLine >= 0)
                    {
                        int l_index = pCmpIndex.rightLine * m_nSample + pCmpIndex.rightSample;
                        int r_index = l_index + m_nSample;
                        l_index = (int)(pRightData[l_index] & 0xFF);
                        r_index = (int)(pRightData[r_index] & 0xFF);

                        int data = (l_index * pCmpIndex.rightPartial + r_index * (32 - pCmpIndex.rightPartial)) / 32;
                        if (data > 255)
                        {
                            data = 255;
                        }
                        sum += data * pCmpIndex.rightIndex;
                        divid += pCmpIndex.rightIndex;

                    }

                    //divid = pCmpIndex.midIndex + pCmpIndex.leftIndex + pCmpIndex.rightIndex;
                    int output = sum / divid;
                    if (output > 255)
                    {
                        output = 255;
                    }
                    m_pCompoundData[index] = (byte)(output & 0xFF);
                    index++;
                }
            }

            return m_pCompoundData;
        }
        public byte[] FlushBiopsyEnhanceFrame(byte[] pBEFrame, int nAngle)
        {
            float angle = (float)(nAngle) / 180.0f * (float)Math.PI;
            float deltaSample = 1.0f / (float)Math.Cos(angle);
            float deltaLine = m_fSampleScale * (float)Math.Tan(angle) / m_fPitch;

            int nIndex = 0;
            for (int l = 0; l < m_nLine; l++)
            {
                float sample = 0.0f;
                float line = (float)l;
                for (int s = 0; s < m_nSample; s++)
                {
                    int nSample = (int)(sample + 0.5f);
                    int nLine = (int)(line + 0.5f);

                    if (nSample >= 0 && nSample < m_nSample && nLine >= 0 && nLine < m_nLine)
                    {
                        m_pBiopsyEnhanceFrame[nIndex] = pBEFrame[nLine * m_nSample + nSample];
                    }
                    else
                    {
                        m_pBiopsyEnhanceFrame[nIndex] = 0;
                    }
                    nIndex++;
                    sample += deltaSample;
                    line += deltaLine;
                }
            }
            return m_pBiopsyEnhanceFrame;
        }

        public void SetBiopsy(float position, int angle)
        {
            if ((m_fBiopsyPosition != position)
                    || (m_nBiopsyAngle != angle)
                    )
            {
                m_fBiopsyPosition = position;
                m_nBiopsyAngle = angle;

                for (int i = 0; i < m_nLine * m_nSample; i++)
                {
                    m_pBiopsyMaskFrame[i] = 0;
                }

                //  合适的角度范围才打开
                if (angle >= -76 && angle <= -50)
                {
                    angle = 90 + angle;
                    float fAngle = (float)angle / 180.0f * (float)Math.PI;
                    float org_line = (position + m_nLine / 2.0f * m_fPitch) / m_fPitch;

                    float sample_div_line = (float)Math.Tan(fAngle) * m_fPitch / m_fSampleScale;
                    float cursample = (0 - org_line) * sample_div_line;

                    for (int l = 0; l < m_nLine; l++)
                    {
                        int pos = l * m_nSample;
                        int smp = (int)(cursample + 0.5);
                        if (smp >= 0 && smp < m_nSample)
                        {
                            m_pBiopsyMaskFrame[pos + smp] = 16;
                        }
                        int prev = smp - 24; prev = prev > 0 ? prev : 0;
                        int post = smp + 24; post = post < m_nSample ? post : m_nSample - 1;
                        for (int s = prev; s <= post; s++)
                        {
                            m_pBiopsyMaskFrame[pos + s] = 16;
                        }

                        int value = 16;
                        while (prev >= 0 && value > 0)
                        {
                            if (prev < m_nSample)
                            {
                                m_pBiopsyMaskFrame[pos + prev] = (byte)(value & 0xFF);
                            }
                            prev--;
                            value--;
                        }
                        value = 16;
                        while (post < m_nSample && value > 0)
                        {
                            m_pBiopsyMaskFrame[pos + post] = (byte)(value & 0xFF);
                            post++;
                            value--;
                        }
                        cursample += sample_div_line;
                    }
                }
            }
        }

        public byte[] CompoundBiopsy(byte[] pData)
        {
            if (pData == null || m_pBiopsyMaskFrame == null || m_pBiopsyEnhanceFrame == null)
            {
                return null;
            }

            for (int i = 0; i < m_nLine * m_nSample; i++)
            {
                if (m_pBiopsyMaskFrame[i] == 0)
                {
                    m_pCompoundBiopsy[i] = pData[i];
                }
                else
                {
                    //m_pCompoundBiopsy[i] = (byte)( (m_pBiopsyMaskFrame[i] * 15) & 0xFF);
                    int mask = (m_pBiopsyMaskFrame[i] & 0xFF) * (m_pBiopsyEnhanceFrame[i] & 0xFF) / 16;

                    if ((mask & 0xFF) > (pData[i] & 0xFF))
                    {
                        m_pCompoundBiopsy[i] = (byte)(mask & 0xFF);
                    }
                    else
                    {
                        m_pCompoundBiopsy[i] = (byte)(pData[i] & 0xFF);
                    }
                }
            }
            return m_pCompoundBiopsy;
        }

    }
}

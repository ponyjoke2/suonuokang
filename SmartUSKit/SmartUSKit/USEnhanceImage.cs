using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    public class USEnhanceImage : USRawImage
    {
        public bool harmonic;       //  谐波成像
        public int dynamicRange;    //  动态范围
        public int focusPos;        //  焦点位置
        public int enhanceLevel;    //  图像增强等级：（0 - 4）

        public int biopsyEnhanceAngle;  //  穿刺增强角度

        public override string Description()
        {
            return "Enhance Image";
        }

        public float[] FocusList()
        {
            USEnhanceProbe enhProbe = (USEnhanceProbe)probeCap;
            return enhProbe.FocusList(harmonic, focusPos);
        }
    }
}

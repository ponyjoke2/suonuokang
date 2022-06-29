using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit_CS.USTools
{
    public class CountFPS
    {
        static DateTime lastTime = DateTime.Now;
        static int Freq_ImageCount = 0;
        static double alltime = 0;

        /// <summary>
        /// 输出FPS
        /// </summary>
        public static void GetFPS()
        {
#if DEBUG
            Freq_ImageCount++;
            if (Freq_ImageCount >= 20)
            {
                alltime = DateTime.Now.Subtract(lastTime).TotalMilliseconds;
                double FreqPerImageFreqt = 1000 / (alltime / 20);
                System.Diagnostics.Debug.WriteLine($"图像频率：{FreqPerImageFreqt}");
                Freq_ImageCount = 0;
                alltime = 0;
                lastTime = DateTime.Now;
            }
#endif
        }
    }
}

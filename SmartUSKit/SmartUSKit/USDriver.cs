using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using SmartUSKit.Enums;

namespace SmartUSKit.SmartUSKit
{
   internal interface IUSDriverDelegate
    {
        void OnProbeConnection(bool isConn);

        void OnUSLive(bool live);

        void OnUSImage(USRawImage rawImg);

        void OnRefreshProbe(int probeindex);
        void OnUSMenuChanged(int menu);
    }

    public abstract class USDriver : IUSDriverCoreDelegate
    {
        //public const ProbeMode MODE_B = ProbeMode.MODE_B;
        //public const ProbeMode MODE_BM = ProbeMode.MODE_BM;

        //public object theDelegate;

        private IUSDriverDelegate _theManager;

        internal IUSDriverDelegate theManager
        {
            get
            {
                return _theManager;
            }
            set { _theManager = value; }
        }

        public USPackager thePackager = null;
        public USProbe theProbe = null;
        
        public USDriver()
        {
        }

        ~USDriver()
        {
            //Debug.WriteLine("~USDriver");
        }
        
        //
        //  连接状态
        //
        public virtual bool IsConnected()
        {
            return false;
        }
        public virtual void Close()
        {
        }
        public virtual void Activate()
        {
        }
        public virtual void Connect()
        {
        }
        public virtual void Disconnect()
        {
        }
        public virtual void SetWifiChannel(int channel)
        {
        }

        //
        //  触发事件
        //
        public virtual void ToggleLive()
        {
        }

        public virtual void SetLive(bool live)
        {
        }
        public virtual bool IsLive()
        {
            return false;
        }

        public virtual void IncGain()
        {
        }
        public virtual void DecGain()
        {
        }

        public virtual void SetGain(int gain)
        {
        }

        public virtual int GetGain()
        {
            return 0;
        }

        public virtual void IncZoom()
        {
        }

        public virtual void DecZoom()
        {
        }

        public virtual void SetZoom(int zoom)
        {
        }

        public virtual int GetZoom()
        {
            return 0;
        }
        

        public virtual void OnCoreConnection(bool isConn)
        {
            if (isConn)
            {
                Debug.WriteLine("PROBE CONNECTED");
            } else
            {
                Debug.WriteLine("PROBE DISCONNECTED");
            }
        }

        public virtual void OnCoreData(byte[] data)
        {
        }

        public virtual void OnCoreState(byte[] stat)
        {
        }

        protected static byte[] LinkArray(byte[] arry1, byte[] arry2)
        {
            int length = 0;
            if (arry1 != null)
            {
                length += arry1.Length;
            }
            if (arry2 != null)
            {
                length += arry2.Length;
            }
            if (length <= 0)
            {
                return null;
            }
            byte[] ret = new byte[length];
            length = 0;
            if (arry1 != null)
            {
                Array.Copy(arry1, 0, ret, 0, arry1.Length);
                length += arry1.Length;
            }
            if (arry2 != null)
            {
                Array.Copy(arry2, 0, ret, length, arry2.Length);
                length += arry2.Length;
            }
            return ret;
        }

        public virtual byte[] OnCoreControl(int tick)
        {
            byte[] ctrl = new byte[4];
            return ctrl;
        }


        protected byte[] LinkByteArray(byte[] firstArray, byte[] secondArray)
        {
            if (firstArray == null)
            {
                return secondArray;
            } 
            if (secondArray == null)
            {
                return firstArray;
            }

            byte[] ret = new byte[firstArray.Length + secondArray.Length];
            firstArray.CopyTo(ret, 0);
            secondArray.CopyTo(ret, firstArray.Length);
            return ret;
        }

        //
        //  BM 相关操作
        //
        protected int bmLine = 0;
        protected ProbeMode currentMode = ProbeMode.MODE_B;

        public void SetBMLine(int line)
        {
            if (line < 0)
            {
                bmLine = 0;
            } else if (line >= theProbe.imagingParameter.lineCount)
            {
                bmLine = theProbe.imagingParameter.lineCount - 1;
            } else
            {
                bmLine = line;
            }
        }
        public int GetBMLine()
        {
            return bmLine;
        }

        public virtual void SetMode(ProbeMode mode)
        {
            if (currentMode != mode)
            {
                if(currentMode == ProbeMode.MODE_BM)
                {
                    BMGenerator bmGen = BMGenerator.GetInstance();
                    bmGen.Reset();
                }
                currentMode = mode;
            }
        }

        public virtual ProbeMode GetMode()
        {
            return currentMode;
        }

        public virtual void ExaminationSetting(JObject settings)
        {
        }


    }
}

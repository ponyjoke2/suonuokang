using SmartUSKit.Enums;
using SmartUSKit.SmartUSKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SmartUSKit_CS.Operations
{
    public class OperateProbe
    {
        private OperateProbe()
        {
        }

        static OperateProbe instance;

        public static OperateProbe GetInstance()
        {
            if (instance == null)
            {
                instance = new OperateProbe();
            }
            return instance;
        }

        public void DecreseZoom()
        {
            USManager mgrbtnFocus = USManager.GetInstance();
            USDriver driverbtnFocus = mgrbtnFocus.GetCurrentDriver();
            if (driverbtnFocus != null)
            {
                driverbtnFocus.DecZoom();
            }
        }

        public void IncreseZoom()
        {
            USManager mgrbtnFocus = USManager.GetInstance();
            USDriver driverbtnFocus = mgrbtnFocus.GetCurrentDriver();
            if (driverbtnFocus != null)
            {
                driverbtnFocus.IncZoom();
            }
        }

        public void ToggleLive()
        {
            USManager mgrbtnFreeze = USManager.GetInstance();
            USDriver driverbtnFreeze = mgrbtnFreeze.GetCurrentDriver();
            if (driverbtnFreeze != null)
            {
                driverbtnFreeze.ToggleLive();
            }
        }
        public void ChangeZoom()
        {
            try
            {
                USManager mgr = USManager.GetInstance();
                USDriver drv = mgr.GetCurrentDriver();
                if (drv != null)
                {
                    int zoom = drv.GetZoom() - 1 + 4;
                    zoom = zoom % 4;

                    //var cp = drv.theProbe.GetCurrentProbe();
                    zoom = LimitZoom(drv, zoom);
                    Debug.WriteLine($"新设置的Zoom：{zoom}");
                    drv.SetZoom(zoom);
                }
            }
            catch (Exception ex)
            {
            }
        }
        public void SetB_Mode()
        {
            USManager mgr = USManager.GetInstance();
            var drv = mgr.GetCurrentDriver();
            drv?.SetMode(ProbeMode.MODE_B);
        }
       

        public int LimitZoom(USDriver drv, int zoom = 0)
        {
            if (drv == null)
            {
                return zoom;
            }
            return zoom;
        }
        public int CheckAndLimitZoom(USDriver drv, int zoom = 0)
        {
            if (drv == null)
            {
                return zoom;
            }
            
            return zoom;
        }

    }
}

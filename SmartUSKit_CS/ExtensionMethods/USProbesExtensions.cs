using SmartUSKit.SmartUSKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit_CS.ExtensionMethods
{
    public static class USProbesExtensions
    {
   
        public static string GetWorkingProbe(this USProbe probe)
        {
            var currentTransuder = probe.TransducerMark();
            currentTransuder = currentTransuder.Split('_').First();
            return currentTransuder;
        }
    }
}

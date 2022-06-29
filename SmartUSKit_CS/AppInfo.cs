using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SmartUSKit_CS
{
    public class AppInfo
    {
        public const string AppName = "WirelessUSG";
        public static string GetAppPath()
        {
            string path = Application.ResourceAssembly.CodeBase.ToString();
            path = path.Remove(0, 8);
            var strs = Application.ResourceAssembly.FullName.Split(',');
            path = path.Remove(path.Length - strs[0].Length - 4, strs[0].Length + 4);

            return path;
        }
    }



}

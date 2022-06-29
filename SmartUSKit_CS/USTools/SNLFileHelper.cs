using Newtonsoft.Json.Linq;
using SmartUSKit.SmartUSKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit_CS.USTools
{
    public class SNLFileHelper
    {
        /// <summary>
        /// List<string>导出为CSV
        /// </summary>
        /// <param name="dt">List<string></param>
        /// <param name="strFilePath">路径</param>
        public static void StringToCsv(string str, string strFilePath)
        {
            try
            {
                if (File.Exists(strFilePath))
                {
                    return;
                }
                if (str == null)   //确保数据不为空
                    return;
                StreamWriter strmWriterObj = new StreamWriter(strFilePath, false, System.Text.Encoding.Default);
                strmWriterObj.WriteLine(str);
                strmWriterObj.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void CreateBinaryFile( string Message)
        {
            try
            {
                using (BinaryWriter bw = new BinaryWriter(new FileStream(AppInfo.GetAppPath() + "\\Authorize", FileMode.Create)))
                {
                    bw.Write(Message);
                    //bw.Close();
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static string OpenBinaryFile()
        {
            string content="";
            //if (!File.Exists(AppDomain.CurrentDomain.SetupInformation.ApplicationBase+"Authorize"))
            //USLog.LogInfo($"OpenBinaryFile:{AppInfo.GetAppPath()}");
            if (!File.Exists(AppInfo.GetAppPath()+"\\Authorize"))
            {
                return "文件不存在！";
            }
            try
            {
                using (BinaryReader br = new BinaryReader(new FileStream(AppInfo.GetAppPath()+"\\Authorize",
                        FileMode.Open)))
                {
                    content=br.ReadString();
                    //br.Close();
                }
            }
            catch (Exception ex)
            {
            }
            return content;
        }
    }
}

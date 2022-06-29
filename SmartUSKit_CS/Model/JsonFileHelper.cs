using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit_CS.Model
{
    public class JsonFileHelper
    {
        private static object SaveFilelock = new object();
        public static void WriteNewFile(string FileName, string JsonString,bool filevisible=true)
        {
            lock (SaveFilelock)
            {
                if (File.Exists(FileName))
                {
                    File.Delete(FileName);
                }
                using (FileStream fileStream = File.Create(FileName))
                {
                    byte[] bytes = new UTF8Encoding(true).GetBytes(JsonString);
                    fileStream.Write(bytes, 0, bytes.Length);
                }
                if (!filevisible)
                {
                    FileInfo info = new FileInfo(FileName);
                    if (info.Exists)
                    {
                        info.Attributes = FileAttributes.Hidden;
                    }
                }

            }
        }
    }
}

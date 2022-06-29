using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Newtonsoft.Json.Linq;
using System.IO;

namespace SmartUSKit.SmartUSKit
{
    public class USPreferences
    {
        JObject PreferenceJson = new JObject();
        string PreferenceJsonFile = "preference.json";
        private object mylock = new object();
        protected static USPreferences instance;
        public static USPreferences GetInstance()
        {
            if (instance == null)
            {
                instance = new USPreferences();
            }
            return instance;
        }
        private static Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        protected USPreferences()
        {
            if (!File.Exists(PreferenceJsonFile))
            {
                PreferenceJson = new JObject();
                WriteNewFile(PreferenceJsonFile, PreferenceJson.ToString());
            }

            StringBuilder sb = new StringBuilder();
            using (FileStream fs = new FileStream(PreferenceJsonFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader fileStream = new StreamReader(fs))
                {
                    string line;
                    while ((line = fileStream.ReadLine()) != null)
                    {
                        sb.Append(line);
                    }
                }
            }

            try
            {
                PreferenceJson = JObject.Parse(sb.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                PreferenceJson = new JObject();
                WriteNewFile(PreferenceJsonFile,PreferenceJson.ToString());
            }
            KeyValueConfigurationCollection kvs = cfa.AppSettings.Settings;
            if (kvs.Count==0)
            {
                return;
            }
            List<string> keys = new List<string>();
            foreach (System.Configuration.KeyValueConfigurationElement kv in kvs)
            {
                keys.Add(kv.Key);
            }
            foreach (string key in keys)
            {
                if (!PreferenceJson.ContainsKey(key))
                {
                    string value = kvs[key].Value.ToString();
                    //string Objectvalue = kvs[key].ToString();
                    PreferenceJson.Add(key,value);
                }
                kvs.Remove(key);
            }
            cfa.Save();
            WriteNewFile(PreferenceJsonFile, PreferenceJson.ToString());
        }
        public void Save()
        {
            //cfa.Save();
            WriteNewFile(PreferenceJsonFile, PreferenceJson.ToString());
        }
        private static double ReadfileTimes = 0;
        public void SetValue(string key, string value)
        {
            try
            {
                lock (mylock)
                {
                    if (!PreferenceJson.ContainsKey(key))
                    {
                        PreferenceJson.Add(key,value); ;
                    }
                    else
                    {
                        PreferenceJson[key]= value; 
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("保存默认值异常："+ex.ToString());
            }
        }
        [HandleProcessCorruptedStateExceptions]
        public string GetValue(string key,string defaultvalue)
        {
            string value;
            try
            {
                if (PreferenceJson.ContainsKey(key))
                {
                    value=PreferenceJson[key].ToString() ;
                }
                else
                {
                    value = defaultvalue;
                }
            }
            catch (Exception)
            {
                value = defaultvalue;
            }
            return value;
        }
        public string GetString(string key, string defaultValue)
        {
            return GetValue(key, defaultValue);
        }
        public int GetInt(string key, int defaultValue)
        {
            return int.Parse(GetValue(key, defaultValue.ToString()));
        }
        public bool GetBoolean(string key, bool defaultValue)
        {
            return bool.Parse(GetValue(key, defaultValue.ToString()));
        }
        public float GetFloat(string key, float defaultValue)
        {
            return float.Parse(GetValue(key, defaultValue.ToString()));
        }
        public void PutString(string key, string value)
        {
            SetValue(key, value);
        }
        public void PutInt(string key, int value)
        {
            SetValue(key, value.ToString());
        }
        public void PutBoolean(string key, bool value)
        {
            SetValue(key, value.ToString());
        }
        public void PutFloat(string key, float value)
        {
            SetValue(key, value.ToString());
        }

        //static int count = 0;
        private static object SaveFilelock = new object();
        private static void WriteNewFile(string FileName, string JsonString, bool filevisible = true)
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

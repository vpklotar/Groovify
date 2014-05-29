using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace Launcher
{
    class Settings
    {
        private String pathToFile;
        private Dictionary<String, String> settings = new Dictionary<String, String>();

        public Settings(String pathToSettingsFile)
        {
            pathToFile = pathToSettingsFile;
            readSettingsFile();
        }

        public Dictionary<String, String> GetKeyValPairs()
        {
            return settings;
        }

        private void readSettingsFile()
        {
            if (File.Exists(pathToFile))
            {
                using (StreamReader sr = new StreamReader(pathToFile))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (!line.Contains("::")) continue;
                        String[] pair = line.Split(new String[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                        String key = pair[0];
                        String val = line.Substring(key.Length + 2);
                        settings.Add(key, val);
                    }
                    sr.Close();
                    Console.WriteLine("Found the file " + pathToFile + " and loaded " + settings.Count + " keys");
                }
            }
            else
            {
                Console.WriteLine("The file " + pathToFile + " does not exsist!");
            }
        }

        public void Save()
        {
            String txt = "";
            for (int index = 0; index < settings.Count; index++)
            {
                var item = settings.ElementAt(index);
                txt += item.Key + "::" + item.Value + System.Environment.NewLine;
            }
            txt = txt.Trim();
            using (StreamWriter sw = new StreamWriter(pathToFile))
            {
                sw.Write(txt);
                sw.Close();
            }
        }

        public String GetString(String key)
        {
            return settings.ContainsKey(key) ? settings[key] : "";
        }

        public int GetInt(String key)
        {
            return Int32.Parse(GetString(key));
        }

        public Boolean ContainsKey(String key)
        {
            return settings.ContainsKey(key);
        }

        public void SetString(String key, String value)
        {
            if (ContainsKey(key))
            {
                settings[key] = value;
                return;
            }
            settings.Add(key, value);
        }

        public void SetInt(String key, int value)
        {
            settings.Add(key, value.ToString());
        }
    }
}

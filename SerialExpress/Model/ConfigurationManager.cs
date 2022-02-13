using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SerialExpress.Model
{
    public class ConfigurationManager
    {
        public ConfigurationManager()
        {
        }
        public static int Save(Dictionary<string, object> dict)
        {
            try
            {
                using (var sw = new StreamWriter(Properties.Resources.ConfigurationFileName, false, Encoding.UTF8))
                {
                    sw.Write(JsonConvert.SerializeObject(dict, Formatting.Indented));
                    sw.Flush();
                }
            }
            catch
            {
                return -1;
            }
            return 0;
        }
        public static IConfigurationRoot? Load()
        {
            try
            {
                return new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(Properties.Resources.ConfigurationFileName).Build();
            }
            catch { }
            return null;
        }
    }
}

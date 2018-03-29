using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookLibrary.Functions
{
    public static class Config
    {
        public static T Get<T>(string key)
        {
            try
            {
                return (T)Convert.ChangeType(ConfigurationManager.AppSettings[key], typeof(T));
            }
            catch
            {
                return (T)Convert.ChangeType(null, typeof(T));
            }
        }
    }
}

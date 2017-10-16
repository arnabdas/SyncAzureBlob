using System;
using System.Configuration;
using System.Globalization;

namespace SyncAzureBlob
{
    /// <summary>
    /// This class provides some worker functions for the specific need for this project
    /// But these functions can also be used in other projects also, as they are generic in nature
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// This is an extension method for the actual get settings method
        /// </summary>
        /// <param name="key"></param>
        /// <returns>string</returns>
        public static string GetSettings(this string key)
        {
            return Setting<string>(key);
        }

        /// <summary>
        /// This method can be used as an extractor method to get a value against the supplied key
        /// from the AppSettings of the config file in current context
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns>string</returns>
        private static T Setting<T>(string key)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (value == null)
            {
                throw new Exception(String.Format("Could not find '{0}' in app settings", key));
            }

            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Xml;
#if WINDOWS_PHONE
using Microsoft.Phone.Info;
#endif

namespace Telegram.Api.Helpers
{
    public class PhoneHelper
    {
        const string AppManifestName = "WMAppManifest.xml";
        const string AppNodeName = "App";
        public const string AppVersion = "Version";

        public static bool IsLowMemoryDevice()
        {
#if WINDOWS_PHONE
            try
            {
                var result = (long)DeviceExtendedProperties.GetValue("ApplicationWorkingSetLimit");

                if (result < 94371840L)
                {
                    return true;
                }

                return false;
            }

            catch (ArgumentOutOfRangeException)
            {
                // The device does not support querying for this value. This occurs
                // on Windows Phone OS 7.1 and older phones without OS updates.

                return true;
            }
#else
            return false;
#endif
        }

        public static string GetOSVersion()
        {
#if WINDOWS_PHONE

            return Environment.OSVersion.Version.ToString();
#else
            return "TestWinRT";
#endif
        }

        public static string GetAppVersion()
        {
            return GetAppAttribute(AppVersion);
        }

        /// <summary>
        /// Gets the value from the WMAppManifest in runtime
        /// Example: PhoneHelper.GetAppAttribute("Title");
        /// 
        /// http://stackoverflow.com/questions/3411377/get-the-windows-phone-7-application-title-from-code
        /// </summary>
        /// <param name="attributeName"></param>
        /// <returns></returns>
        public static string GetAppAttribute(string attributeName)
        {
#if WINDOWS_PHONE
            try
            {
                var settings = new XmlReaderSettings { XmlResolver = new XmlXapResolver() };

                using (var rdr = XmlReader.Create(AppManifestName, settings))
                {
                    rdr.ReadToDescendant(AppNodeName);
                    if (!rdr.IsStartElement())
                    {
                        throw new FormatException(AppManifestName + " is missing " + AppNodeName);
                    }

                    return rdr.GetAttribute(attributeName);
                }
            }
            catch (Exception)
            {
                return "";
            }
#else
            return "TestWinRT";
#endif
        }

        public static string GetDeviceFullName()
        {
#if WINDOWS_PHONE
            return (string)DeviceExtendedProperties.GetValue("DeviceName");
#else
            return "TestWinRT";
#endif
        }
        
        public static bool IsWiFiEnabled()
        {
#if WINDOWS_PHONE
            return Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsWiFiEnabled;
#else
            return true;
#endif
        }

        public static bool IsCellularDataEnabled()
        {
#if WINDOWS_PHONE
            return Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsCellularDataEnabled;
#else
            return true;
#endif
        }

        public static string GetShortPhoneModel(string phoneCode)
        {
            var cleanCode = phoneCode.Replace("-", string.Empty).ToLowerInvariant();

            foreach (var model in _models)
            {
                if (cleanCode.StartsWith(model.Key))
                {
                    return model.Value;
                }
            }

            return string.Empty;
        }
       
        private static Dictionary<string, string> _models = new Dictionary<string, string>
        {
            {"rm923", "Lumia505"},
            {"rm898", "Lumia510"},
            {"rm889", "Lumia510"},
            {"rm915", "Lumia520"},
            {"rm917", "Lumia521"},
            {"rm998", "Lumia525"},
            {"rm997", "Lumia526"},
            {"rm1017", "Lumia530"},
            {"rm1018", "Lumia530"},
            {"rm1019", "Lumia530"},
            {"rm1020", "Lumia530"},
            {"rm1090", "Lumia535"},
            {"rm836", "Lumia610"},
            {"rm849", "Lumia610"},
            {"rm846", "Lumia620"},
            {"rm941", "Lumia625"},
            {"rm942", "Lumia625"},
            {"rm943", "Lumia625"},
            {"rm974", "Lumia630"},
            {"rm976", "Lumia630"},
            {"rm977", "Lumia630"},
            {"rm978", "Lumia630"},
            {"rm975", "Lumia635"},
            {"rm803", "Lumia710"},
            {"rm809", "Lumia710"},
            {"rm885", "Lumia720"},
            {"rm887", "Lumia720"},
            {"rm1038", "Lumia730"},
            {"rm801", "Lumia800"},
            {"rm802", "Lumia800"},
            {"rm819", "Lumia800"},
            {"rm878", "Lumia810"},
            {"rm824", "Lumia820"},
            {"rm825", "Lumia820"},
            {"rm826", "Lumia820"},
            {"rm845", "Lumia822"},
            {"rm983", "Lumia830"},
            {"rm984", "Lumia830"},
            {"rm985", "Lumia830"},
            {"rm808", "Lumia900"},
            {"rm823", "Lumia900"},
            {"rm820", "Lumia920"},
            {"rm821", "Lumia920"},
            {"rm822", "Lumia920"},
            {"rm867", "Lumia920"},
            {"rm892", "Lumia925"},
            {"rm893", "Lumia925"},
            {"rm910", "Lumia925"},
            {"rm955", "Lumia925"},
            {"rm860", "Lumia928"},
            {"rm1045", "Lumia930"},
            {"rm875", "Lumia1020"},
            {"rm876", "Lumia1020"},
            {"rm877", "Lumia1020"},
            {"rm994", "Lumia1320"},
            {"rm995", "Lumia1320"},
            {"rm996", "Lumia1320"},
            {"rm937", "Lumia1520"},
            {"rm938", "Lumia1520"},
            {"rm939", "Lumia1520"},
            {"rm940", "Lumia1520"},
            {"rm927", "LumiaIcon"},

        };
        
    }
}

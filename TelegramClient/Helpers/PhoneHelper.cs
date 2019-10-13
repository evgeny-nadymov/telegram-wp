using System;
using System.Threading;
using System.Xml;
using Microsoft.Phone.Info;

namespace TelegramClient.Helpers
{
    public class PhoneHelper
    {
        const string AppManifestName = "WMAppManifest.xml";
        const string AppNodeName = "App";
        public const string AppVersion = "Version";


        public static bool IsLowMemoryDevice()
        {
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
        }

        public static string GetOSVersion()
        {
            return Environment.OSVersion.Version.ToString();
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
        }

        public static string GetDeviceName()
        {
            return (string)DeviceExtendedProperties.GetValue("DeviceName");
        }

        public static string GetDeviceFullName()
        {
            return GetDeviceManufacturer() + " " + GetDeviceName();
        }

        public static string GetDeviceUniqueId()
        {
            return DeviceExtendedProperties.GetValue("DeviceUniqueId").ToString();
        }

        public static string GetDeviceHardwareVersion()
        {
            return DeviceExtendedProperties.GetValue("DeviceHardwareVersion").ToString();
        }

        public static string GetDeviceFirmwareVersion()
        {
            return DeviceExtendedProperties.GetValue("DeviceFirmwareVersion").ToString();
        }

        public static string GetDeviceManufacturer()
        {
            return DeviceExtendedProperties.GetValue("DeviceManufacturer").ToString();
        }

        public static string GetLocation()
        {
            return Thread.CurrentThread.CurrentUICulture.ToString();
        }

        public static bool IsWiFiEnabled()
        {
            return Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsWiFiEnabled;
        }

        public static bool IsCellularDataEnabled()
        {
            return Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsCellularDataEnabled;
        }
    }
}

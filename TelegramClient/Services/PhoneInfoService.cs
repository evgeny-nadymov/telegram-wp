// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Phone.Info;
using Telegram.Api.Services.DeviceInfo;

namespace TelegramClient.Services
{
    public interface IExtendedDeviceInfoService : IDeviceInfoService
    {
        bool IsLowMemoryDevice { get; }
        bool IsWiFiEnabled { get; }
        bool IsCellularDataEnabled { get; }
    }

    public abstract class PhoneInfoServiceBase : IDeviceInfoService
    {
        private const string AppManifestName = "WMAppManifest.xml";
        private const string AppNodeName = "App";

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
                return String.Empty;
            }
        }

        public abstract string Model { get; }
        public abstract string AppVersion { get; }
        public abstract string SystemVersion { get; }
        public abstract bool IsBackground { get; }
        public abstract string BackgroundTaskName { get; }
        public abstract int BackgroundTaskId { get; }
    }

    public class PhoneInfoService : PhoneInfoServiceBase, IExtendedDeviceInfoService
    {
        public override bool IsBackground { get { return false; } }
        public override string BackgroundTaskName { get { return string.Empty; } }
        public override int BackgroundTaskId { get { return default(int); } }

        public override string Model
        {
            get
            {
                var model = (string) DeviceExtendedProperties.GetValue("DeviceName");

                return GetShortModel(model) ?? model;
            }
        }

        public override string AppVersion { get { return GetAppAttribute("Version"); } }

        public override string SystemVersion { get { return Environment.OSVersion.Version.ToString(); } }

        public bool IsLowMemoryDevice
        {
            get
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
        }

        public bool IsWiFiEnabled { get { return Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsWiFiEnabled; } }

        public bool IsCellularDataEnabled { get { return Microsoft.Phone.Net.NetworkInformation.DeviceNetworkInformation.IsCellularDataEnabled; } }

        private static string GetShortModel(string phoneCode)
        {
            var cleanCode = phoneCode.Replace("-", string.Empty).ToLowerInvariant();

            foreach (var model in _models)
            {
                if (cleanCode.StartsWith(model.Key))
                {
                    return model.Value;
                }
            }

            return null;
        }

        private static readonly Dictionary<string, string> _models = new Dictionary<string, string>
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
            {"rm1116", "Lumia950XL"},
            {"rm1085", "Lumia950XL"},
            {"rm1118", "Lumia950"},
            {"rm1104", "Lumia950"},

        };
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Xml;
using Windows.System;
using TelegramClient.Converters;

namespace TelegramClient.Services
{
    public class WindowsPhoneStoreUpdateService : IWindowsPhoneStoreUpdateService
    {

        private string GetManifestAttributeValue(string attributeName)
        {
            var xmlReaderSettings = new XmlReaderSettings
            {
                XmlResolver = new XmlXapResolver()
            };

            using (var xmlReader = XmlReader.Create("WMAppManifest.xml", xmlReaderSettings))
            {
                xmlReader.ReadToDescendant("App");

                return xmlReader.GetAttribute(attributeName);
            }
        }

        public void LaunchAppUpdateAsync()
        {
            Launcher.LaunchUriAsync(PrivateBetaIdentityToVisibilityConverter.IsPrivateBeta
                ? new Uri(Constants.PreviewUpdateUri)
                : new Uri(Constants.UpdateUri));
        }

        /// <summary>
        /// Checks the Windows Phone Store to see if a newer version of the app is available
        /// If it is, a dialog is shown
        /// </summary>
        /// <param name="updateDialogText">Dialog text</param>
        /// <param name="updateDialogTitle">Dialog title</param>
        public void CheckForUpdatedVersion(string updateDialogText, string updateDialogTitle)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp/?productid=9WZDNCRDZHS0"));
            return;
            Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:navigate?appid=0872b696-d84b-4279-8627-7ed8b15bf4f0"));
            return;
            var cultureInfoName = CultureInfo.CurrentUICulture.Name;

            var url =
                string.Format(
                    "http://marketplaceedgeservice.windowsphone.com/v8/catalog/apps/{0}?os={1}&cc={2}&oc=&lang={3}​",
                    "0872b696-d84b-4279-8627-7ed8b15bf4f0",//GetManifestAttributeValue("ProductID"),
                    Environment.OSVersion.Version,
                    cultureInfoName.Substring(cultureInfoName.Length - 2).ToUpperInvariant(),
                    cultureInfoName);

            var wc = new WebClient();
            wc.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error != null) return;
                var content = e.Result;

                try
                {
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                    {
                        using (var reader = XmlReader.Create(ms))
                        {
                            reader.MoveToContent();

                            var aNamespace = reader.LookupNamespace("a");
                            reader.ReadToFollowing("entry", aNamespace);
                            reader.ReadToDescendant("version");

                            var updatedVersion = new Version(reader.ReadElementContentAsString());
                            var currentVersion = new Version(GetManifestAttributeValue("Version"));
                            if (updatedVersion > currentVersion
                    &&
                    MessageBox.Show(updateDialogText, updateDialogTitle, MessageBoxButton.OKCancel) ==
                    MessageBoxResult.OK)
                            {
                                Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:navigate?appid=0872b696-d84b-4279-8627-7ed8b15bf4f0"));
                                //WebBrowserTask task = new WebBrowserTask();
                                //task.URL = "http://windowsphone.com/s?appid=0872b696-d84b-4279-8627-7ed8b15bf4f0";
                                //task.Show();
                                //new MarketplaceDetailTask().Show();
                            }
                        }
                    }
                }
                catch
                {

                }
            };
            wc.DownloadStringAsync(new Uri(url));


        }
    }
}

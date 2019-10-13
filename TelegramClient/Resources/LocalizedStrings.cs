// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows;
using Microsoft.Phone.Info;
using Telegram.Api.Services;
using Telegram.EmojiPanel;
using Telegram.EmojiPanel.Controls.Emoji;
#if WP81
using Windows.UI.ViewManagement;
#endif
using Caliburn.Micro;

namespace TelegramClient.Resources
{
    public class LocalizedStrings : TelegramPropertyChangedBase
    {
        private static readonly AppResources _resources = new AppResources();

        public AppResources Resources { get { return _resources; } }

        public void SetLanguage(CultureInfo culture)
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            NotifyOfPropertyChange(() => Resources);
        }
    }

    public class ScaledText : TelegramPropertyChangedBase
    {
#if WP81
        private UISettings _settings;

        public UISettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new UISettings();
                    _settings.TextScaleFactorChanged += OnTextScaleFactorChanged;
                }

                return _settings; 
                
            }
        }

        private void OnTextScaleFactorChanged(UISettings sender, object args)
        {
            BrowserNavigationService.FontScaleFactor = TextScaleFactor;
            NotifyOfPropertyChange(() => TextScaleFactor);
        }

#endif

        public double ApplicationBarHeight
        {
            get
            {
#if WP8
                if (Application.Current.Host.Content.ScaleFactor == 225)
                {
                    return 56.0;
                }
#endif

                return 72.0;
            }
        }

        public double TextScaleFactor
        {
            get
            {
                var textScaleFactor = 1.0;

#if WP81
                textScaleFactor = Settings.TextScaleFactor;
#endif

                return textScaleFactor;
            }
        }

        public double DefaultFontSizeLarge
        {
            get { return DefaultFontSize / 23.0 * 27.0; }
        }

        public double DefaultFontSizeSmall
        {
            get { return DefaultFontSize / 23.0 * 20.0; }
        }

        private double? _defaultFontSize;

        public double DefaultFontSize
        {
            get
            {
                if (_defaultFontSize != null)
                {
                    return _defaultFontSize.Value;
                }

                const double defaultFontSize = 23.0;

#if WP8
                var deviceName = DeviceStatus.DeviceName;
                switch (Application.Current.Host.Content.ScaleFactor)
                {
                    case 100:   //Lumia 820
                        return defaultFontSize;
                        break;
                    case 150:   //HTC 8X
                        return 20;
                        break;
                    case 160:   //Lumia 925
                        if (!string.IsNullOrEmpty(deviceName))
                        {
                            deviceName = deviceName.Replace("-", string.Empty).ToLowerInvariant();

                            //Lumia 950, 950XL    5,2 5,7 inch   QHD     2560x1440
                            if (deviceName.StartsWith("rm1116")         // 950XL dual sim
                                || deviceName.StartsWith("rm1085")      // 950XL single sim
                                || deviceName.StartsWith("rm1118")      // 950 dual sim
                                || deviceName.StartsWith("rm1104"))     // 950 single sim
                            {
                                return 17.778;
                            }
                        }

                        return 20;
                        break;
                    case 225:   // Lumia 1520, Lumia 930
                        return 17.667;
                        break;
                }
#endif

                _defaultFontSize = defaultFontSize;

                return _defaultFontSize.Value;
            }
        }

        public double DefaultSystemSegoeUISymbolFontSize
        {
            get { return DefaultSystemFontSize - 3.0; }
        }

        public double DefaultSystemFontSize
        {
            get
            {
                const double defaultFontSize = 20;

#if WP8
                var deviceName = DeviceStatus.DeviceName;
                switch (Application.Current.Host.Content.ScaleFactor)
                {
                    case 100:   //Lumia 820
                        return defaultFontSize;
                        break;
                    case 150:   //HTC 8X
                        return 20;
                        break;
                    case 160:   //Lumia 925
                        if (!string.IsNullOrEmpty(deviceName))
                        {
                            deviceName = deviceName.Replace("-", string.Empty).ToLowerInvariant();

                            //Lumia 950, 950XL    5,2 5,7 inch   QHD     2560x1440
                            if (deviceName.StartsWith("rm1116")         // 950XL dual sim
                                || deviceName.StartsWith("rm1085")      // 950XL single sim
                                || deviceName.StartsWith("rm1118")      // 950 dual sim
                                || deviceName.StartsWith("rm1104"))     // 950 single sim
                            {
                                return 14;
                            }
                        }

                        return 20;
                        break;
                    case 225:   // Lumia 1520, Lumia 930
                        return 17.667;
                        break;
                }
#endif

                return defaultFontSize;
            }
        }

        public double DefaultSystemIconSize
        {
            get
            {
                const double defaultFontSize = 15;

#if WP8
                var deviceName = DeviceStatus.DeviceName;
                switch (Application.Current.Host.Content.ScaleFactor)
                {
                    case 100:   //Lumia 820
                        return defaultFontSize;
                        break;
                    case 150:   //HTC 8X
                        return 15;
                        break;
                    case 160:   //Lumia 925
                        if (!string.IsNullOrEmpty(deviceName))
                        {
                            deviceName = deviceName.Replace("-", string.Empty).ToLowerInvariant();

                            //Lumia 950, 950XL    5,2 5,7 inch   QHD     2560x1440
                            if (deviceName.StartsWith("rm1116")         // 950XL dual sim
                                || deviceName.StartsWith("rm1085")      // 950XL single sim
                                || deviceName.StartsWith("rm1118")      // 950 dual sim
                                || deviceName.StartsWith("rm1104"))     // 950 single sim
                            {
                                return 9.3;
                            }
                        }

                        return 15;
                        break;
                    case 225:   // Lumia 1520, Lumia 930
                        return 12;
                        break;
                }
#endif

                return defaultFontSize;
            }
        }

        public double DefaultSystemIconWidth
        {
            get { return DefaultSystemFontSize*34/29; }
        }
    }
}

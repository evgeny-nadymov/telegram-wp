// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;

namespace TelegramClient.Views.Additional
{
    public partial class ChooseGeoLivePeriodView
    {
        public ChooseGeoLivePeriodView()
        {
            InitializeComponent();

            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225;
            if (isFullHD || Environment.OSVersion.Version.Major >= 10)
            {
                Period15Minutes.FontSize = 20.0;
                Period1Hour.FontSize = 20.0;
                Period8Hours.FontSize = 20.0;
            }
        }
    }
}

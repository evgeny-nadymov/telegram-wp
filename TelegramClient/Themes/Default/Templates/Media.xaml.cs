// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Telegram.Api.Services.Location;
using Telegram.Api.TL;
using TelegramClient.Views.Controls;

namespace TelegramClient.Themes.Default.Templates
{
    public partial class Media : ResourceDictionary
    {
        public Media() 
        {
            InitializeComponent();
        }

        private void Grid_Tap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void HandledManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;
        }

        private void LiveLocationProgress_OnCompleted(object sender, System.EventArgs e)
        {
            var liveLocationProgress = sender as LiveLocationProgress;
            if (liveLocationProgress != null)
            {
                var messageMediaGeoLive = liveLocationProgress.Media as TLMessageMediaGeoLive;
                if (messageMediaGeoLive != null)
                {
                    messageMediaGeoLive.Period.Value = 0;
                    messageMediaGeoLive.NotifyOfPropertyChange(() => messageMediaGeoLive.Active);

                    var liveLocationService = IoC.Get<ILiveLocationService>();

                    liveLocationService.UpdateAll();
                }
            }
        }
    }
}

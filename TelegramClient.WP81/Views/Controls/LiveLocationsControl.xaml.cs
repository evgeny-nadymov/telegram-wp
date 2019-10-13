// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows.Input;
using Telegram.Api.TL;

namespace TelegramClient.Views.Controls
{
    public partial class LiveLocationsControl
    {
        public LiveLocationsControl()
        {
            InitializeComponent();
        }

        public event EventHandler<MediaEventArgs> LiveLocationCompleted;

        private void LiveLocationProgress_OnCompleted(object sender, System.EventArgs e)
        {
            var progress = sender as LiveLocationProgress;
            if (progress != null)
            {
                RaiseCompleted(new MediaEventArgs{ Media = progress.Media });
            }
        }

        protected virtual void RaiseCompleted(MediaEventArgs e)
        {
            var handler = LiveLocationCompleted;
            if (handler != null) handler(this, e);
        }
    }

    public class MediaEventArgs : System.EventArgs
    {
        public TLMessageMediaBase Media { get; set; }
    }
}

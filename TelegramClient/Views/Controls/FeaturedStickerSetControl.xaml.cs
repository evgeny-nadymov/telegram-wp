// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Input;

namespace TelegramClient.Views.Controls
{
    public partial class FeaturedStickerSetControl
    {
        public FeaturedStickerSetControl()
        {
            InitializeComponent();
        }

        public event EventHandler Added;

        protected virtual void RaiseAdded()
        {
            var handler = Added;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void AddStickerSet_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseAdded();
        }

        public event EventHandler Opened;

        protected virtual void RaiseOpened()
        {
            var handler = Opened;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void OpenStickerSet_OnTap(object sender, GestureEventArgs e)
        {
            RaiseOpened();
        }
    }
}

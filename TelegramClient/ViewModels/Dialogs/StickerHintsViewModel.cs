// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.ObjectModel;
using TelegramClient.Views.Dialogs;

namespace TelegramClient.ViewModels.Dialogs
{
    public class StickerHintsViewModel
    {
        public ObservableCollection<TLStickerItem> Hints { get; protected set; }

        public StickerHintsViewModel()
        {
            Hints = new ObservableCollection<TLStickerItem>();
        }
    }
}

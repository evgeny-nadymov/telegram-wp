// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.ObjectModel;
using Telegram.Api.TL;

namespace TelegramClient.ViewModels.Dialogs
{
    public class CommandHintsViewModel
    {
        public ObservableCollection<TLBotCommand> Hints { get; protected set; }

        private readonly TLObject _with;

        public CommandHintsViewModel(TLObject with)
        {
            _with = with;

            Hints = new ObservableCollection<TLBotCommand>();
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.ObjectModel;
using TelegramClient.Views.Controls;
using TelegramClient_Native;

namespace TelegramClient.ViewModels.Dialogs
{
    public class EmojiHintsViewModel
    {
        public ObservableCollection<EmojiSuggestion> Hints { get; protected set; }

        public EmojiSuggestionParams Parameters { get; protected set; }

        public EmojiHintsViewModel()
        {
            Hints = new ObservableCollection<EmojiSuggestion>();
        }

        public void SetParameters(EmojiSuggestionParams parameters)
        {
            Parameters = parameters;

            Hints.Clear();
            if (Parameters != null
                && Parameters.Results != null)
            {
                int maxCount = 5;
                for (int index = 0; index < parameters.Results.Length && index < maxCount; index++)
                {
                    var result = parameters.Results[index];
                    Hints.Add(result);
                }
            }
        }
    }
}

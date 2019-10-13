// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Caliburn.Micro;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Dialogs
{
    public class MessageViewerViewModel : Screen
    {
        public IMessage Message { get; set; }

        private readonly IStateService _stateService;

        public MessageViewerViewModel(IStateService stateService)
        {
            _stateService = stateService;

            if (_stateService.MediaMessage != null)
            {
                Message = _stateService.MediaMessage;
                _stateService.MediaMessage = null;
            }

            if (_stateService.DecryptedMediaMessage != null)
            {
                Message = _stateService.DecryptedMediaMessage;
                _stateService.DecryptedMediaMessage = null;
            }
        }
    }
}

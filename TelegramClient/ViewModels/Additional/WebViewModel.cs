// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using Windows.Data.Json;
using Caliburn.Micro;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.ViewModels.Additional
{
    public class WebViewModel : Screen
    {
        public Uri Url { get; set; }

        public string GameTitle { get; set; }

        public string Subtitle { get; set; }

        private readonly TLUser _sharedContact;

        private readonly List<TLMessageBase> _forwardMessages;

        private readonly TLBotCallbackAnswer _botCallbackAnswer;

        private readonly TLGame _game;

        private readonly IStateService _stateService;

        private readonly INavigationService _navigationService;

        private readonly IMTProtoService _mtProtoService;

        private readonly TLInputPeerBase _inputPeer;

        private OutputTypingManager _gamePlayingManager;

        public OutputTypingManager GamePlayingManager
        {
            get
            {
                return _gamePlayingManager =
                    _gamePlayingManager ??
                    new OutputTypingManager(_inputPeer, Constants.SetTypingIntervalInSeconds,
                        action => _mtProtoService.SetTypingAsync(_inputPeer, action ?? new TLSendMessageGamePlayAction(), result => { }),
                        () => _mtProtoService.SetTypingAsync(_inputPeer, new TLSendMessageCancelAction(), result => { }));
            }
        }

        private readonly Timer _timer;

        public WebViewModel(IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService)
        {
            _stateService = stateService;
            _navigationService = navigationService;
            _mtProtoService = mtProtoService;

            _sharedContact = stateService.SharedContact as TLUser;
            stateService.SharedContact = null;

            var userName = _sharedContact as IUserName;
            if (userName != null && !TLString.IsNullOrEmpty(userName.UserName))
            {
                Subtitle = "@" + userName.UserName;
            }

            _botCallbackAnswer = stateService.BotCallbackAnswer;
            stateService.BotCallbackAnswer = null;

            _forwardMessages = stateService.ForwardMessages;
            stateService.ForwardMessages = null;

            var botCallbackAnswer54 = _botCallbackAnswer as TLBotCallbackAnswer54;
            if (botCallbackAnswer54 != null)
            {
                Url = new Uri(botCallbackAnswer54.Url.ToString(), UriKind.RelativeOrAbsolute);
#if DEBUG

                Clipboard.SetText(botCallbackAnswer54.Url.ToString());
                MessageBox.Show(botCallbackAnswer54.Url.ToString());
#endif
            }

            _game = stateService.Game;
            stateService.Game = null;

            if (_game != null)
            {
                GameTitle = _game.Title.ToString();
            }

            _inputPeer = stateService.InputPeer;
            stateService.InputPeer = null;

            _timer = new Timer(OnTimerTick);
        }

        private void OnTimerTick(object state)
        {
            GamePlayingManager.SetTyping();

            _timer.Change(TimeSpan.FromSeconds(8.0), Timeout.InfiniteTimeSpan);
        }

        protected override void OnActivate()
        {
            OnTimerTick(_timer);

            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);

            GamePlayingManager.CancelTyping();

            base.OnDeactivate(close);
        }

        public void ScriptNotify(string value)
        {
            try
            {
                var obj = JsonObject.Parse(value);
                IJsonValue eventType;
                if (obj.TryGetValue("eventType", out eventType))
                {
                    IJsonValue eventData;
                    if (obj.TryGetValue("eventData", out eventData))
                    {

                    }

                    if (string.Equals(eventType.GetString(), "share_game", StringComparison.OrdinalIgnoreCase))
                    {
                        if (_sharedContact == null) return;
                        if (_game == null) return;

                        _stateService.GameString = _game.ShortName.ToString();
                        _stateService.SharedContact = _sharedContact;
                        _navigationService.UriFor<ChooseDialogViewModel>().Navigate();

                        return;
                    }

                    if (string.Equals(eventType.GetString(), "share_score", StringComparison.OrdinalIgnoreCase))
                    {
                        Share();

                        return;
                    }

                    if (string.Equals(eventType.GetString(), "game_over", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Game over!");
                        return;
                    }
                }

                MessageBox.Show(value);
            }
            catch (Exception ex)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage(string.Format("WebViewModel.ScriptNotify {0} exception {1}", value, ex));
            }
        }

        public void Share()
        {
            if (_forwardMessages == null) return;

            _stateService.WithMyScore = true;
            _stateService.ForwardMessages = _forwardMessages;
            _navigationService.UriFor<ChooseDialogViewModel>().Navigate();
        }

        public void CopyLink()
        {
            if (_sharedContact == null || TLString.IsNullOrEmpty(_sharedContact.UserName)) return;

            var link = string.Format(Constants.UsernameLinkPlaceholder, _sharedContact.UserName);
            if (_game != null
                && !TLString.IsNullOrEmpty(_game.ShortName))
            {
                link += "?game=" + _game.ShortName;
            }

            Clipboard.SetText(link);
            MessageBox.Show(AppResources.CopyLinkHint);
        }
    }
}

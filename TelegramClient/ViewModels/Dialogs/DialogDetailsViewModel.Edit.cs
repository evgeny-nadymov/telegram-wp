// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Microsoft.Devices;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Resources;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogDetailsViewModel :
        Telegram.Api.Aggregator.IHandle<TLUpdateEditChannelMessage>,
        Telegram.Api.Aggregator.IHandle<TLUpdateEditMessage>
    {
        private bool _isEditingEnabled;

        public bool IsEditingEnabled
        {
            get { return _isEditingEnabled; }
            set { SetField(ref _isEditingEnabled, value, () => IsEditingEnabled); }
        }

        private TLMessage _editedMessage;

        private DispatcherTimer _editMessageTimer;

        public void StartEditMessage(TLString text, TLMessage message)
        {
            if (text == null) return;
            if (message == null) return;

            _editedMessage = message;
            var config = CacheService.GetConfig() as TLConfig48;
            var editUntil = config != null ? message.DateIndex + config.EditTimeLimit.Value + 5 * 60 : 0;
            if (message.FromId != null
                && message.ToId is TLPeerUser
                && message.FromId.Value == message.ToId.Id.Value)
            {
                editUntil = 0;
            }

            Reply = new TLMessagesContainter { EditMessage = _editedMessage as TLMessage25, EditUntil = editUntil };

            if (_editMessageTimer == null)
            {
                _editMessageTimer = new DispatcherTimer();
                _editMessageTimer.Tick += OnEditMessageTimerTick;
                _editMessageTimer.Interval = TimeSpan.FromSeconds(1.0);
            }

            _editMessageTimer.Start();

            IsEditingEnabled = true;
            Text = text.ToString();

            CurrentInlineBot = null;
            ClearStickerHints();
            ClearInlineBotResults();
            ClearUsernameHints();
            ClearHashtagHints();
            ClearCommandHints();
        }

        private void OnEditMessageTimerTick(object sender, System.EventArgs e)
        {
            var editedMessage48 = _editedMessage as TLMessage48;
            if (editedMessage48 == null)
            {
                _editMessageTimer.Stop();
                return;
            }

            var messagesContainer = Reply as TLMessagesContainter;
            if (messagesContainer == null || messagesContainer.EditUntil == 0)
            {
                _editMessageTimer.Stop();
                return;
            }

            messagesContainer.NotifyOfPropertyChange(() => messagesContainer.EditTimerString);

#if DEBUG
            //VibrateController.Default.Start(TimeSpan.FromMilliseconds(100));
#endif
        }

        public void StopEditMessage()
        {
            ClearMentions();

            _editedMessage = null;
            Reply = null;
            _previousReply = null;

            Text = string.Empty;
            IsEditingEnabled = false;

            if (_editMessageTimer != null) _editMessageTimer.Stop();
        }

        public void EditMessage(TLMessage message)
        {
            if (message == null) return;

            var inputPeer = With as IInputPeer;
            if (inputPeer == null) return;

            IsWorking = true;
            MTProtoService.GetMessageEditDataAsync(inputPeer.ToInputPeer(), message.Id,
                editData => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    var text = GetMessageEditText(editData, message);

                    StartEditMessage(text, message);
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                    {
                        if (error.TypeEquals(ErrorType.MESSAGE_ID_INVALID))
                        {
                            MessageBox.Show(AppResources.EditMessageError, AppResources.Error, MessageBoxButton.OK);
                            return;
                        }
                    }

                    Telegram.Api.Helpers.Execute.ShowDebugMessage("channel.getMessageEditData error " + error);
                }));
        }

        private TLString GetMessageEditText(TLMessageEditData editData, TLMessage message)
        {
            var text = message.Message.ToString();

            var textBuilder = new StringBuilder();
            var message48 = message as TLMessage48;
            if (message48 != null && message48.Entities != null && message48.Entities.Count > 0)
            {
                ClearMentions();

                for (var i = 0; i < message48.Entities.Count; i++)
                {
                    // prefix
                    var offset = i == 0
                        ? 0
                        : message48.Entities[i - 1].Offset.Value + message48.Entities[i - 1].Length.Value;
                    var length = i == 0
                        ? message48.Entities[i].Offset.Value
                        : message48.Entities[i].Offset.Value - offset;

                    textBuilder.Append(text.Substring(offset, length));

                    var mentionName = message48.Entities[i] as TLMessageEntityMentionName;
                    if (mentionName != null)
                    {
                        var user = CacheService.GetUser(mentionName.UserId);
                        if (user != null)
                        {
                            AddMention(user);

                            var mention = text.Substring(message48.Entities[i].Offset.Value, message48.Entities[i].Length.Value);

                            textBuilder.Append(string.Format("@({0})", mention));
                        }

                        continue;
                    }

                    var inputMentionName = message48.Entities[i] as TLInputMessageEntityMentionName;
                    if (inputMentionName != null)
                    {
                        var inputUser = inputMentionName.User as TLInputUser;
                        if (inputUser != null)
                        {
                            var user = CacheService.GetUser(inputUser.UserId);
                            if (user != null)
                            {
                                AddMention(user);

                                var mention = text.Substring(message48.Entities[i].Offset.Value, message48.Entities[i].Length.Value);

                                textBuilder.Append(string.Format("@({0})", mention));
                            }
                        }

                        continue;
                    }

                    var bold = message48.Entities[i] as TLMessageEntityBold;
                    if (bold != null)
                    {
                        var entity = text.Substring(message48.Entities[i].Offset.Value, message48.Entities[i].Length.Value);

                        textBuilder.Append(string.Format("**{0}**", entity));

                        continue;
                    }

                    var italic = message48.Entities[i] as TLMessageEntityItalic;
                    if (italic != null)
                    {
                        var entity = text.Substring(message48.Entities[i].Offset.Value, message48.Entities[i].Length.Value);

                        textBuilder.Append(string.Format("__{0}__", entity));

                        continue;
                    }

                    var code = message48.Entities[i] as TLMessageEntityCode;
                    if (code != null)
                    {
                        var entity = text.Substring(message48.Entities[i].Offset.Value, message48.Entities[i].Length.Value);

                        textBuilder.Append(string.Format("`{0}`", entity));

                        continue;
                    }

                    var pre = message48.Entities[i] as TLMessageEntityPre;
                    if (pre != null)
                    {
                        var entity = text.Substring(message48.Entities[i].Offset.Value, message48.Entities[i].Length.Value);

                        textBuilder.Append(string.Format("```{0}```", entity));

                        continue;
                    }

                    offset = message48.Entities[i].Offset.Value;
                    length = message48.Entities[i].Length.Value;

                    textBuilder.Append(text.Substring(offset, length));
                }

                var lastEntity = message48.Entities[message48.Entities.Count - 1];
                if (lastEntity != null)
                {
                    textBuilder.Append(text.Substring(lastEntity.Offset.Value + lastEntity.Length.Value));
                }

            }
            else
            {
                textBuilder.Append(text);
            }

            return new TLString(textBuilder.ToString());
        }

        public void CancelSaveMessage()
        {
            StopEditMessage();
        }

        public void SaveMessage()
        {
            var message = _editedMessage as TLMessage34;
            if (message == null) return;

            var inputPeer = With as IInputPeer;
            if (inputPeer == null) return;

            var text = GetTrimmedText(Text) ?? string.Empty;

            //check maximum message length
            if (text.Length > Constants.MaximumMessageLength)
            {
                MessageBox.Show(String.Format(AppResources.MaximumMessageLengthExceeded, Constants.MaximumMessageLength), AppResources.Error, MessageBoxButton.OK);

                return;
            }

            var noWebPage = true;
            if (Reply != null && IsWebPagePreview(Reply))
            {
                message._media = ((TLMessagesContainter)Reply).WebPageMedia;
                Reply = null;
                noWebPage = false;
            }

            string processedText;
            var entities = GetEntities(text, out processedText);
            TLVector<TLMessageEntityBase> entitiesVector = null;
            if (entities != null)
            {
                var message48 = message as TLMessage48;
                if (message48 != null)
                {
                    entitiesVector = new TLVector<TLMessageEntityBase>(entities);
                    message48.Entities = entitiesVector;
                }
            }

            IsWorking = true;
            MTProtoService.EditMessageAsync(inputPeer.ToInputPeer(), message.Id, new TLString(processedText), entitiesVector, null, null, noWebPage, false, null,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    var message48 = message as TLMessage48;
                    if (message48 != null)
                    {
                        message48.NotifyOfPropertyChange(() => message48.EditDate);
                        message48.NotifyOfPropertyChange(() => message48.EditDateVisibility);
                    }

                    StopEditMessage();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                    {
                        if (error.TypeEquals(ErrorType.MESSAGE_ID_INVALID))
                        {
                            MessageBox.Show(AppResources.EditMessageError, AppResources.Error, MessageBoxButton.OK);
                        }
                        if (error.TypeEquals(ErrorType.MESSAGE_NOT_MODIFIED))
                        {

                        }
                        StopEditMessage();
                    }

                    Telegram.Api.Helpers.Execute.ShowDebugMessage("channel.getMessageEditData error " + error);
                }));
        }

        public void Handle(TLUpdateEditChannelMessage update)
        {
            var channel = With as TLChannel;
            if (channel == null) return;

            var message = update.Message as TLMessage31;
            if (message == null || !(message.ToId is TLPeerChannel)) return;

            if (channel.Id.Value == message.ToId.Id.Value)
            {
                Execute.BeginOnUIThread(() =>
                {
                    var item = Items.FirstOrDefault(x => x.Index == update.Message.Index) as TLMessage31;
                    if (item == null) return;

                    if (item != message)
                    {
                        item.Edit(message);
                    }

                    message = item;
                    var item48 = item as TLMessage48;
                    var message48 = message as TLMessage48;
                    if (message48 != null && item48 != null)
                    {
                        message48.Entities = item48.Entities;
                    }
                    var message31 = message as TLMessage48;
                    if (message31 != null)
                    {
                        var mediaGeoLive = message31.Media as TLMessageMediaGeoLive;
                        if (mediaGeoLive != null)
                        {
                            mediaGeoLive.EditDate = message31.EditDate;
                            mediaGeoLive.Date = message31.Date;
                            mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Geo);
                            mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.EditDate);
                            mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Active);

                            message31.NotifyOfPropertyChange(() => message31.Message);
                            message31.NotifyOfPropertyChange(() => message31.ReplyMarkup);

                            if (LocationPicker != null)
                            {
                                LocationPicker.UpdateLiveLocation(message31);
                            }
                            if (LiveLocationBadge != null)
                            {
                                LiveLocationBadge.UpdateLiveLocation(message31);
                            }

                            return;
                        }

                        message31.NotifyOfPropertyChange(() => message31.Message);
                        message31.NotifyOfPropertyChange(() => message31.Media);
                        message31.NotifyOfPropertyChange(() => message31.ReplyMarkup);

                        var mediaGame = message31.Media as TLMessageMediaGame;
                        if (mediaGame != null)
                        {
                            mediaGame.NotifyOfPropertyChange(() => mediaGame.Message);
                            mediaGame.NotifyOfPropertyChange(() => mediaGame.MessageVisibility);
                            mediaGame.NotifyOfPropertyChange(() => mediaGame.DescriptionVisibility);
                        }
                    }
                });
            }
        }

        public void Handle(TLUpdateEditMessage update)
        {
            var message = update.Message as TLMessageCommon;
            if (message == null) return;

            var editMessage = false;

            var user = With as TLUserBase;
            var chat = With as TLChatBase;

            if (user != null
                && message.ToId is TLPeerUser
                && !message.Out.Value
                && user.Id.Value == message.FromId.Value)
            {
                editMessage = true;
            }
            else if (user != null
                && message.ToId is TLPeerUser
                && message.Out.Value
                && user.Id.Value == message.ToId.Id.Value)
            {
                editMessage = true;
            }
            else if (chat != null
                && message.ToId is TLPeerChat
                && chat.Id.Value == message.ToId.Id.Value)
            {
                editMessage = true;
            }

            if (editMessage)
            {
                Execute.BeginOnUIThread(() =>
                {
                    var item = Items.FirstOrDefault(x => x.Index == update.Message.Index) as TLMessageCommon;
                    if (item == null) return;

                    if (item != message)
                    {
                        item.Edit(message);
                    }

                    message = item;
                    var item48 = item as TLMessage48;
                    var message48 = message as TLMessage48;
                    if (message48 != null && item48 != null)
                    {
                        message48.Entities = item48.Entities;
                    }
                    var message31 = message as TLMessage48;
                    if (message31 != null)
                    {
                        var mediaGeoLive = message31.Media as TLMessageMediaGeoLive;
                        if (mediaGeoLive != null)
                        {
                            mediaGeoLive.EditDate = message31.EditDate;
                            mediaGeoLive.Date = message31.Date;
                            mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Geo);
                            mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.EditDate);
                            mediaGeoLive.NotifyOfPropertyChange(() => mediaGeoLive.Active);

                            message31.NotifyOfPropertyChange(() => message31.Message);
                            message31.NotifyOfPropertyChange(() => message31.ReplyMarkup);

                            if (LocationPicker != null)
                            {
                                LocationPicker.UpdateLiveLocation(message31);
                            }
                            if (LiveLocationBadge != null)
                            {
                                LiveLocationBadge.UpdateLiveLocation(message31);
                            }

                            return;
                        }

                        message31.NotifyOfPropertyChange(() => message31.Message);
                        message31.NotifyOfPropertyChange(() => message31.Media);
                        message31.NotifyOfPropertyChange(() => message31.ReplyMarkup);

                        var mediaGame = message31.Media as TLMessageMediaGame;
                        if (mediaGame != null)
                        {
                            mediaGame.NotifyOfPropertyChange(() => mediaGame.Message);
                            mediaGame.NotifyOfPropertyChange(() => mediaGame.MessageVisibility);
                            mediaGame.NotifyOfPropertyChange(() => mediaGame.DescriptionVisibility);
                        }
                    }

                    var messageService = item as TLMessageService;
                    if (messageService != null)
                    {
                        var actionGameScore = messageService.Action as TLMessageActionGameScore;
                        if (actionGameScore != null)
                        {
                            messageService.NotifyOfPropertyChange(() => messageService.Self);
                        }
                    }
                });
            }
        }
    }
}

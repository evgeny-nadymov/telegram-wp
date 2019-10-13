// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Linq;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Api.TL.Interfaces;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Media
{
    public class MusicViewModel<T> : FilesViewModelBase<T> where T : IInputPeer
    {
        public override TLInputMessagesFilterBase InputMessageFilter
        {
            get { return new TLInputMessagesFilterMusic(); }
        }

        public string EmptyListImageSource
        {
            get { return "/Images/Messages/nomusic.png"; }
        }

        public MusicViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            DisplayName = LowercaseConverter.Convert(AppResources.SharedMusic);
        }

        protected override void OnInitialize()
        {
            Status = string.Empty;  //AppResources.Loading;
            var limit = 25;
            var messages = CacheService.GetHistory(TLUtils.InputPeerToPeer(CurrentItem.ToInputPeer(), StateService.CurrentUserId), limit);
            BeginOnUIThread(() =>
            {
                Items.Clear();

                AddMessages(messages);

                var channel = CurrentItem as TLChannel;
                if (channel != null && channel.MigratedFromChatId != null)
                {
                    var lastMessage = messages != null ? messages.LastOrDefault() : null;
                    if (lastMessage != null && lastMessage.Index == 1)
                    {
                        IsLastSliceLoaded = true;
                        var chatMessages = CacheService.GetHistory(new TLPeerChat { Id = channel.MigratedFromChatId }, limit);

                        AddMessages(chatMessages);
                    }
                }

                Status = Items.Count > 0 ? string.Empty : Status;

                LoadNextSlice();
            });

            base.OnInitialize();
        }

        protected override bool SkipMessage(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage;
            if (message == null)
            {
                return true;
            }

            var mediaDocument = message.Media as TLMessageMediaDocument;
            if (mediaDocument == null)
            {
                return true;
            }

            var document = mediaDocument.Document as TLDocument22;
            if (document == null)
            {
                return true;
            }

            var audioAttribute = document.Attributes.FirstOrDefault(x => x is TLDocumentAttributeAudio46) as TLDocumentAttributeAudio46;
            if (audioAttribute == null || audioAttribute.Voice)
            {
                return true;
            }

            return false;
        }
    }
}

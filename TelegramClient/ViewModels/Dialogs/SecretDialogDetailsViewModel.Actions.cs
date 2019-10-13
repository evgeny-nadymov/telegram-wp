// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class SecretDialogDetailsViewModel
    {
        public event EventHandler<ScrollToEventArgs> ScrollTo;

        protected virtual void RaiseScrollTo(ScrollToEventArgs args)
        {
            var handler = ScrollTo;
            if (handler != null) handler(this, args);
        }

        public event EventHandler ScrollToBottom;

        protected virtual void RaiseScrollToBottom()
        {
            BeginOnUIThread(() =>
            {
                var handler = ScrollToBottom;
                if (handler != null) handler(this, System.EventArgs.Empty);
            });
        }

        private TLDecryptedMessageBase _previousScrollPosition;

        public void ProcessScroll()
        {
            // replies
            if (_previousScrollPosition != null)
            {
                RaiseScrollTo(new ScrollToEventArgs(_previousScrollPosition));
                _previousScrollPosition = null;
                return;
            }


            // unread separator
            //if (!_isFirstSliceLoaded)
            //{
            //    Items.Clear();
            //    var messages = CacheService.GetHistory(TLUtils.InputPeerToPeer(Peer, StateService.CurrentUserId));
            //    ProcessRepliesAndAudio(messages);

            //    const int maxCount = 5;
            //    for (var i = 0; i < messages.Count && i < maxCount; i++)
            //    {
            //        Items.Add(messages[i]);
            //    }

            //    //wait to complete animation for hiding ScrollToBottomButton
            //    BeginOnUIThread(TimeSpan.FromSeconds(0.35), () =>
            //    {
            //        for (var i = maxCount; i < messages.Count; i++)
            //        {
            //            Items.Add(messages[i]);
            //        }
            //        _isFirstSliceLoaded = true;

            //        UpdateItemsAsync(0, 0, Constants.FileSliceLength, false);
            //    });
            //}
            //else
            {
                RaiseScrollToBottom();
            }
        }

        public void Call()
        {
            ShellViewModel.StartVoiceCall(With as TLUser, IoC.Get<IVoIPService>(), IoC.Get<ICacheService>());
        }
    }
}

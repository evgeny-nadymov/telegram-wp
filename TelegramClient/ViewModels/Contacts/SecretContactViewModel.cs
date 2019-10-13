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
using Telegram.Api.TL;
using TelegramClient.Services;
using TelegramClient.ViewModels.Media;

namespace TelegramClient.ViewModels.Contacts
{
    public class SecretContactViewModel : Conductor<object>.Collection.OneActive
    {
        public SecretContactDetailsViewModel ContactDetails { get; protected set; }

        public SecretMediaViewModel Media { get; protected set; }

        public string ChatName
        {
            get
            {
                return _contact != null ? _contact.FullName.ToUpperInvariant() : string.Empty;
            }
        }

        public Uri ChatImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                return isLightTheme?
                    new Uri("/Images/Dialogs/secretchat-white-WXGA.png", UriKind.Relative):
                    new Uri("/Images/Dialogs/secretchat-black-WXGA.png", UriKind.Relative);
            }
        }

        private readonly TLUserBase _contact;

        private readonly IStateService _stateService;

        public DecryptedImageViewerViewModel ImageViewer { get; set; }

        public SecretContactViewModel(SecretContactDetailsViewModel contactDetails, SecretMediaViewModel media, IStateService stateService, INavigationService navigationService)
        {
            //tombstoning
            if (stateService.CurrentContact == null)
            {
                ShellViewModel.Navigate(navigationService);
                return;
            }

            ContactDetails = contactDetails;
            Media = media;
            Media.Contact = this;

            _stateService = stateService;

            var key = stateService.CurrentKey;
            stateService.CurrentKey = null;

            _contact = stateService.CurrentContact;
            stateService.CurrentContact = null;

            var chat = stateService.CurrentEncryptedChat;
            stateService.CurrentEncryptedChat = null;

            ContactDetails.CurrentItem = _contact;
            ContactDetails.Chat = chat;
            ContactDetails.Key = key;
        }

        protected override void OnInitialize()
        {
            if (_contact == null) return;

            Items.Add(ContactDetails);
            Items.Add(Media);
            if (_stateService.MediaTab)
            {
                _stateService.MediaTab = false;
                //Items.Add(Media);
                //Items.Add(ContactDetails);
                ActivateItem(Media);
            }
            else
            {
                //Items.Add(ContactDetails);
                //Items.Add(Media);
                ActivateItem(ContactDetails);
            }

            base.OnInitialize();
        }

        protected override void OnActivate()
        {
            if (_stateService.SelectedTimerSpan != null)
            {
                ContactDetails.SelectedSpan = _stateService.SelectedTimerSpan;
                _stateService.SelectedTimerSpan = null;
            }

            base.OnActivate();
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO.IsolatedStorage;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Extensions;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.ViewModels.Chats
{
    public class ChannelIntroViewModel : Screen
    {
        public Uri ImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                return isLightTheme ?
                    new Uri("/Images/Startup/intro.channel.white-WXGA.png", UriKind.Relative) :
                    new Uri("/Images/Startup/intro.channel.black-WXGA.png", UriKind.Relative);
            }
        }

        private readonly IStateService _stateService;

        private readonly INavigationService _navigationService;

        private static readonly object _channelIntroFileSyncRoot = new object();

        public ChannelIntroViewModel(IStateService stateService, INavigationService navigationService)
        {
            _stateService = stateService;
            _navigationService = navigationService;
        }


        protected override void OnActivate()
        {
            base.OnActivate();

            if (_stateService.RemoveBackEntry)
            {
                _stateService.RemoveBackEntry = false;
                _navigationService.RemoveBackEntry();
            }
        }

        public void CreateChannel()
        {
            _stateService.RemoveBackEntry = true;
            _navigationService.UriFor<CreateChannelStep1ViewModel>().Navigate();

            SetIntroDisabledAsync();
        }

        private static void SetIntroDisabledAsync()
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                lock (_channelIntroFileSyncRoot)
                {
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        store.CreateFile(Constants.ChannelIntroFileName);
                    }
                }
            });
        }

        public static void CheckIntroEnabledAsync(Action<bool> callback)
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {

                var exists = false;

                try
                {
                    lock (_channelIntroFileSyncRoot)
                    {
                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            exists = store.FileExists(Constants.ChannelIntroFileName);
                        }
                    }
                }
                catch (Exception e)
                {

                }

                callback.SafeInvoke(!exists); 
            });
        }
    }
}

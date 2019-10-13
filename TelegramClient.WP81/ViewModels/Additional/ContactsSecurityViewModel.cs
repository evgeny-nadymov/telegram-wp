// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Windows.ApplicationModel.Contacts;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.ViewModels.Additional
{
    public class ContactsSecurityViewModel : ViewModelBase
    {
        protected string _syncContactsSubtitle;

        public string SyncContactsSubtitle
        {
            get { return _syncContactsSubtitle; }
            set { SetField(ref _syncContactsSubtitle, value, () => SyncContactsSubtitle); }
        }

        protected string _syncContactsHint;

        public string SyncContactsHint
        {
            get { return _syncContactsHint; }
            set { SetField(ref _syncContactsHint, value, () => SyncContactsHint); }
        }

        protected bool _suggestFrequentContacts;

        public bool SuggestFrequentContacts
        {
            get { return _suggestFrequentContacts; }
            set { SetField(ref _suggestFrequentContacts, value, () => SuggestFrequentContacts); }
        }

        public ContactsSecurityViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _suggestFrequentContacts = StateService.GetTopPeers() is TLTopPeers;

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => SuggestFrequentContacts))
                {
                    if (!SuggestFrequentContacts)
                    {
                        ShellViewModel.ShowCustomMessageBox(
                            AppResources.SuggestFrequentContactsNotification,
                            AppResources.AppName,
                            AppResources.Ok, null,
                            dismissed =>
                            {

                            });
                    }

                    MTProtoService.ToggleTopPeersAsync(
                        new TLBool(SuggestFrequentContacts),
                        result => Execute.BeginOnUIThread(() =>
                        {
                            if (!SuggestFrequentContacts)
                            {
                                StateService.SaveTopPeers(new TLTopPeersDisabled());
                                EventAggregator.Publish(new ClearTopPeersEventArgs());
                            }
                        }));
                }
            };
        }

        protected override void OnActivate()
        {
            EventAggregator.Subscribe(this);

            Telegram.Api.Helpers.Execute.BeginOnUIThread(async () =>
            {
                var contactStore = await ContactManager.RequestStoreAsync();
                SyncContactsSubtitle = contactStore != null ? AppResources.Enabled : AppResources.Disabled;
                SyncContactsHint = contactStore != null
                    ? AppResources.SyncContactsInfoOn
                    : AppResources.SyncContactsInfoOff;
            });
        }

        protected override void OnDeactivate(bool close)
        {
            EventAggregator.Unsubscribe(this);
        }

        public void DeleteSyncedContacts()
        {
            ShellViewModel.ShowCustomMessageBox(
                AppResources.DeleteSyncedContactsConfirmation, AppResources.AppName,
                AppResources.Ok.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        EventAggregator.Publish(new InvokeDeleteContacts());
                    }
                });
        }

        public async void SyncContacts()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-contacts"));
        }
    }

    public class ClearTopPeersEventArgs
    {
        
    }
}

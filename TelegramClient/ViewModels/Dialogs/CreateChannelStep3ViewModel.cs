// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Views;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Dialogs
{
    public class CreateChannelStep3ViewModel : CreateDialogViewModel
    {
        private readonly TLChannel _newChannel;

        public CreateChannelStep3ViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            GroupedUsers = new ObservableCollection<TLUserBase>();

            _newChannel = StateService.NewChannel;
            StateService.NewChannel = null;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }
        }

        public override void Create()
        {
            if (IsWorking) return;
            if (_newChannel == null) return;

            var participants = new TLVector<TLInputUserBase>();
            foreach (var item in SelectedUsers)
            {
                participants.Add(item.ToInputUser());
            }
            participants.Add(new TLInputUser { UserId = new TLInt(StateService.CurrentUserId), AccessHash = new TLLong(0) });

            if (participants.Count == 0)
            {
                MessageBox.Show(AppResources.PleaseChooseAtLeastOneParticipant, AppResources.Error, MessageBoxButton.OK);
                return;
            }

            _newChannel.ParticipantIds = new TLVector<TLInt> { Items = SelectedUsers.Select(x => x.Id).ToList() };

            IsWorking = true;
            MTProtoService.InviteToChannelAsync(_newChannel.ToInputChannel(), participants,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    StateService.With = _newChannel;
                    StateService.RemoveBackEntries = true;
                    NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    if (error.TypeEquals(ErrorType.PEER_FLOOD))
                    {
                        //MessageBox.Show(AppResources.PeerFloodAddContact, AppResources.Error, MessageBoxButton.OK);
                        ShellViewModel.ShowCustomMessageBox(AppResources.PeerFloodAddContact, AppResources.Error, AppResources.MoreInfo.ToLowerInvariant(), AppResources.Ok.ToLowerInvariant(),
                            result =>
                            {
                                if (result == CustomMessageBoxResult.RightButton)
                                {
                                    TelegramViewBase.NavigateToUsername(MTProtoService, Constants.SpambotUsername, null, null, null);
                                }
                            });
                    }
                    else if (error.TypeEquals(ErrorType.USERS_TOO_MUCH))
                    {
                        MessageBox.Show(AppResources.UsersTooMuch, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.USER_CHANNELS_TOO_MUCH))
                    {
                        MessageBox.Show(AppResources.UserChannelsTooMuch, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.BOTS_TOO_MUCH))
                    {
                        MessageBox.Show(AppResources.BotsTooMuch, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.USER_NOT_MUTUAL_CONTACT))
                    {
                        MessageBox.Show(AppResources.UserNotMutualContact, AppResources.Error, MessageBoxButton.OK);
                    }

                    IsWorking = false;
                    Execute.ShowDebugMessage("channels.inviteToChannel error " + error);
                }));
        }
    }
}

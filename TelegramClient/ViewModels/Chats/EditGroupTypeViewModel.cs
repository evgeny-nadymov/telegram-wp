// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Views.Chats;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Chats
{
    public class EditGroupTypeViewModel : ViewModelBase
    {
        public string TypeCaption
        {
            get
            {
                return IsMegaGroup ? AppResources.GroupType : AppResources.ChannelType;
            }
        }

        public string PublicTypeTitle
        {
            get
            {
                return IsMegaGroup ? AppResources.Public : AppResources.PublicChannel;
            }
        }

        public string PublicTypeDescription
        {
            get
            {
                return IsMegaGroup ? AppResources.PublicGroupDescription : AppResources.PublicChannelDescription;
            }
        }

        public string PrivateTypeTitle
        {
            get
            {
                return IsMegaGroup ? AppResources.Private : AppResources.PrivateChannel;
            }
        }

        public string PrivateTypeDescription
        {
            get
            {
                return IsMegaGroup ? AppResources.PrivateGroupDescription : AppResources.PrivateChannelDescription;
            }
        }

        public string PublicLinkDescription
        {
            get
            {
                return IsMegaGroup ? AppResources.PublicGroupLinkDescription : AppResources.PublicLinkDescription;
            }
        }

        public string PrivateLinkDescription
        {
            get
            {
                return IsMegaGroup ? AppResources.PrivateGroupLinkDescription : AppResources.PrivateLinkDescription;
            }
        }

        public bool IsMegaGroup
        {
            get { return _newChannel != null && _newChannel.IsMegaGroup; }
        }

        public ObservableCollection<TLChatBase> AdminedPublicChannels { get; set; }

        private bool _tooMuchUsernames;

        public bool TooMuchUsernames
        {
            get { return _tooMuchUsernames; }
            set { SetField(ref _tooMuchUsernames, value, () => TooMuchUsernames); }
        }

        private bool _isPublic = true;

        public bool IsPublic
        {
            get { return _isPublic; }
            set
            {
                SetField(ref _isPublic, value, () => IsPublic);
                NotifyOfPropertyChange(() => ChannelTypeDescription);
                NotifyOfPropertyChange(() => ChannelLinkDescription);
            }
        }

        public string ChannelTypeDescription
        {
            get { return IsPublic ? AppResources.PublicGroupDescription : AppResources.PrivateGroupDescription; }
        }

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set { SetField(ref _userName, value, () => UserName); }
        }

        private bool _isUsernameAvailable;

        public bool IsUsernameAvailable
        {
            get { return _isUsernameAvailable; }
            set { SetField(ref _isUsernameAvailable, value, () => IsUsernameAvailable); }
        }

        private string _usernameAvailableString;

        public string UsernameAvailableString
        {
            get { return _usernameAvailableString; }
            set { SetField(ref _usernameAvailableString, value, () => UsernameAvailableString); }
        }

        private bool _hasError;

        public bool HasError
        {
            get { return _hasError; }
            set
            {
                SetField(ref _hasError, value, () => HasError);
                if (value)
                {
                    IsUsernameAvailable = false;
                }
            }
        }

        private string _error = " ";

        public string Error
        {
            get { return _error; }
            set { SetField(ref _error, value, () => Error); }
        }

        public string ChannelLinkDescription
        {
            get { return IsPublic ? AppResources.PublicGroupLinkDescription : AppResources.PrivateGroupLinkDescription; }
        }

        public TLExportedChatInvite Invite { get; set; }


        private string _inviteLink;

        public string InviteLink
        {
            get { return _inviteLink; }
            set { SetField(ref _inviteLink, value, () => InviteLink); }
        }

        private readonly TLChannel _newChannel;

        public EditGroupTypeViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            AdminedPublicChannels = new ObservableCollection<TLChatBase>();

            InviteLink = AppResources.Loading;

            _newChannel = StateService.CurrentChat as TLChannel;
            StateService.CurrentChat = null;

            if (_newChannel != null)
            {
                if (!TLString.IsNullOrEmpty(_newChannel.UserName))
                {
                    _userName = _newChannel.UserName.ToString();
                }
                var chatInviteExported = _newChannel.ExportedInvite as TLChatInviteExported;
                if (chatInviteExported != null)
                {
                    InviteLink = chatInviteExported.Link.ToString();
                }

                if (TLString.IsNullOrEmpty(_newChannel.UserName))
                {
                    MTProtoService.CheckUsernameAsync(new TLInputChannelEmpty(), new TLString("username"), 
                        result => BeginOnUIThread(() =>
                        {
                            TooMuchUsernames = false;
                        }),
                        error => BeginOnUIThread(() =>
                        {
                            if (TLRPCError.TypeEquals(error, ErrorType.CHANNELS_ADMIN_PUBLIC_TOO_MUCH))
                            {
                                HasError = true;
                                Error = AppResources.ChannelsAdminPublicTooMuchShort;
                                TooMuchUsernames = true;
                                GetAdminedPublichChannelsAsync();
                            }
                        }));
                }
            }

            IsPublic = !string.IsNullOrEmpty(UserName);
            if (!IsPublic && InviteLink == AppResources.Loading)
            {
                ExportInvite();
            }

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => IsPublic))
                {
                    if (!IsPublic && Invite == null)
                    {
                        ExportInvite();
                    }
                }
                else if (Property.NameEquals(args.PropertyName, () => UserName))
                {
                    var userName = UserName;
                    BeginOnUIThread(TimeSpan.FromSeconds(0.3), () =>
                    {
                        if (string.Equals(userName, UserName))
                        {
                            Check();
                        }
                    });
                }
            };
        }

        private void GetAdminedPublichChannelsAsync()
        {
            if (AdminedPublicChannels.Count > 0) return;

            MTProtoService.GetAdminedPublicChannelsAsync(
                result => BeginOnUIThread(() =>
                {
                    AdminedPublicChannels.Clear();
                    foreach (var chat in result.Chats)
                    {
                        AdminedPublicChannels.Add(chat);
                    }
                }),
                error => BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage("channels.getAdminedPublicChannels error " + error);
                }));
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

        private static bool IsValidSymbol(char symbol)
        {
            if ((symbol >= 'a' && symbol <= 'z')
                || (symbol >= 'A' && symbol <= 'Z')
                || (symbol >= '0' && symbol <= '9')
                || symbol == '_')
            {
                return true;
            }

            return false;
        }

        public void Check()
        {
            var checkedUsername = UserName;
            
            var username = UserName;
            if (username != null
                && username.StartsWith("@"))
            {
                username = username.Substring(1, username.Length - 1);
            }

            if (string.IsNullOrEmpty(username))
            {
                HasError = false;
                IsUsernameAvailable = false;
                //Error = string.Empty;
                return;
            }

            var isValidSymbols = username.All(IsValidSymbol);
            if (!isValidSymbols)
            {
                HasError = true;
                Error =  IsMegaGroup ? AppResources.GroupNameInvalid : AppResources.ChannelNameInvalid;
                return;
            }

            if (username[0] >= '0' && username[0] <= '9')
            {
                HasError = true;
                Error = IsMegaGroup ? AppResources.GroupNameStartsWithNumber : AppResources.ChannelNameStartsWithNumber;
                return;
            }

            if (username.Length < Constants.UsernameMinLength)
            {
                HasError = true;
                Error = IsMegaGroup ? AppResources.GroupNameShort : AppResources.ChannelNameShort;
                return;
            }

            HasError = false;
            IsUsernameAvailable = false;
            //if (string.Equals(username, _newChannel.UserName.ToString()))

            MTProtoService.CheckUsernameAsync(_newChannel.ToInputChannel(), new TLString(username),
                result => BeginOnUIThread(() =>
                {
                    HasError = !result.Value;
                    if (HasError)
                    {
                        Error = IsMegaGroup ? AppResources.GroupNameOccupied : AppResources.ChannelNameOccupied;
                    }

                    if (string.Equals(checkedUsername, UserName) && result.Value)
                    {
                        IsUsernameAvailable = true;
                        UsernameAvailableString = string.Format(AppResources.NameIsAvailable, checkedUsername);
                    }
                }),
                error => BeginOnUIThread(() =>
                {
                    HasError = true;
                    if (TLRPCError.CodeEquals(error, ErrorCode.FLOOD))
                    {
                        Error = AppResources.FloodWaitString;
                    }
                    else if (TLRPCError.CodeEquals(error, ErrorCode.INTERNAL))
                    {
                        var messageBuilder = new StringBuilder();
                        messageBuilder.AppendLine(AppResources.ServerErrorMessage);
                        messageBuilder.AppendLine();
                        messageBuilder.AppendLine("Method: channels.checkUsername");
                        messageBuilder.AppendLine("Result: " + error);

                        Error = AppResources.ServerError;
                    }
                    else if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST))
                    {
                        if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_INVALID))
                        {
                            Error = IsMegaGroup ? AppResources.GroupNameInvalid : AppResources.ChannelNameInvalid;
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_OCCUPIED))
                        {
                            Error = IsMegaGroup ? AppResources.GroupNameOccupied : AppResources.ChannelNameOccupied;
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.CHANNELS_ADMIN_PUBLIC_TOO_MUCH))
                        {
                            MessageBox.Show(AppResources.ChannelsAdminPublicTooMuch, AppResources.Error, MessageBoxButton.OK);

                            Error = AppResources.ChannelsAdminPublicTooMuchShort;
                            TooMuchUsernames = true;
                            GetAdminedPublichChannelsAsync();
                        }
                        else 
                        {
                            Error = error.ToString();
                        }
                    }
                    else
                    {
                        Error = string.Empty;
                        Execute.ShowDebugMessage("channels.checkUsername error " + error);
                    }
                }));
        }

        public void RevokeLink(TLChatBase chat)
        {
            var channel = chat as TLChannel;
            if (channel == null) return;

            var richTextBox = new RichTextBox{ TextWrapping = TextWrapping.Wrap };
            ConvertToSupergroupView.SetFormattedText(richTextBox, string.Format(AppResources.RevokeLinkConfirmation2, "t.me/" + channel.UserName, channel.FullName));
#if WP8
            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225;
            if (isFullHD)
            {
                richTextBox.FontSize = 17.667;
            }
#endif
            ShellViewModel.ShowCustomMessageBox(null, AppResources.RevokeLink,
                AppResources.Cancel.ToLowerInvariant(), AppResources.Revoke.ToLowerInvariant(),
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.LeftButton)
                    {
                        IsWorking = true;
                        MTProtoService.UpdateUsernameAsync(channel.ToInputChannel(), TLString.Empty,
                            user => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;

                                channel.UserName = TLString.Empty;
                                channel.NotifyOfPropertyChange(() => channel.UserName);

                                TooMuchUsernames = false;
                                HasError = false;
                                Error = string.Empty;
                                Check();
                            }),
                            error => Execute.BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                if (TLRPCError.CodeEquals(error, ErrorCode.FLOOD))
                                {
                                    //HasError = true;
                                    //Error = AppResources.FloodWaitString;
                                    MessageBox.Show(AppResources.FloodWaitString, AppResources.Error, MessageBoxButton.OK);
                                }
                                else if (TLRPCError.CodeEquals(error, ErrorCode.INTERNAL))
                                {
                                    var messageBuilder = new StringBuilder();
                                    messageBuilder.AppendLine(AppResources.ServerErrorMessage);
                                    messageBuilder.AppendLine();
                                    messageBuilder.AppendLine("Method: channels.updateUsername");
                                    messageBuilder.AppendLine("Result: " + error);

                                    //HasError = true;
                                    //Error = AppResources.ServerError;
                                    MessageBox.Show(messageBuilder.ToString(), AppResources.ServerError, MessageBoxButton.OK);
                                }
                                else if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST))
                                {
                                    if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_NOT_MODIFIED))
                                    {
                                        channel.UserName = TLString.Empty;
                                        channel.NotifyOfPropertyChange(() => channel.UserName);

                                        HasError = false;
                                        Error = string.Empty;
                                        Check();
                                        return;
                                    }
                                    else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_INVALID))
                                    {
                                        //HasError = true;
                                        //Error = AppResources.UsernameInvalid;
                                        MessageBox.Show(IsMegaGroup ? AppResources.GroupNameInvalid : AppResources.ChannelNameInvalid, AppResources.Error, MessageBoxButton.OK);
                                    }
                                    else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_OCCUPIED))
                                    {
                                        //HasError = true;
                                        //Error = AppResources.UsernameOccupied;
                                        MessageBox.Show(IsMegaGroup ? AppResources.GroupNameOccupied : AppResources.ChannelNameOccupied, AppResources.Error, MessageBoxButton.OK);
                                    }
                                    else if (TLRPCError.TypeEquals(error, ErrorType.CHANNELS_ADMIN_PUBLIC_TOO_MUCH))
                                    {
                                        //HasError = true;
                                        //Error = AppResources.ChannelsAdminPublicTooMuchShort;
                                        MessageBox.Show(AppResources.ChannelsAdminPublicTooMuch, AppResources.Error, MessageBoxButton.OK);
                                    }
                                    else
                                    {
                                        //HasError = true;
                                        //Error = error.ToString();
                                        Execute.ShowDebugMessage("channels.updateUsername error " + error);
                                    }
                                }
                                else
                                {
                                    //HasError = true;
                                    //Error = string.Empty;
                                    Execute.ShowDebugMessage("channels.updateUsername error " + error);
                                }
                            }));
                    }
                },
                richTextBox);
        }

        private void ExportInvite()
        {
            if (IsWorking) return;
            if (Invite != null) return;
            if (_newChannel.ExportedInvite is TLChatInviteExported) return;

            IsWorking = true;
            MTProtoService.ExportInviteAsync(_newChannel.ToInputChannel(),
                result => Execute.BeginOnUIThread(() =>
                {
                    _newChannel.ExportedInvite = result;

                    IsWorking = false;
                    Invite = result;
                    var inviteExported = Invite as TLChatInviteExported;
                    if (inviteExported != null)
                    {
                        if (!TLString.IsNullOrEmpty(inviteExported.Link))
                        {
                            InviteLink = inviteExported.Link.ToString();
                        }
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("channels.exportInvite error " + error);
                }));
        }

        public void CopyInvite()
        {
            var inviteExported = Invite as TLChatInviteExported;
            if (inviteExported != null)
            {
                if (!TLString.IsNullOrEmpty(inviteExported.Link))
                {
                    Clipboard.SetText(inviteExported.Link.ToString());
                }
            }
        }

        public event EventHandler EmptyUserName;

        protected virtual void RaiseEmptyUserName()
        {
            var handler = EmptyUserName;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public void Done()
        {
            if (IsWorking) return;

            var username = IsPublic? UserName : string.Empty;
            if (username != null
                && username.StartsWith("@"))
            {
                username = username.Substring(1, username.Length - 1);
            }

            IsWorking = true;
            MTProtoService.UpdateUsernameAsync(_newChannel.ToInputChannel(), new TLString(username),
                user => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    _newChannel.UserName = new TLString(username);
                    _newChannel.NotifyOfPropertyChange(() => _newChannel.UserName);
                    NavigationService.GoBack();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    if (TLRPCError.CodeEquals(error, ErrorCode.FLOOD))
                    {
                        HasError = true;
                        Error = AppResources.FloodWaitString;
                        MessageBox.Show(AppResources.FloodWaitString, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (TLRPCError.CodeEquals(error, ErrorCode.INTERNAL))
                    {
                        var messageBuilder = new StringBuilder();
                        messageBuilder.AppendLine(AppResources.ServerErrorMessage);
                        messageBuilder.AppendLine();
                        messageBuilder.AppendLine("Method: channels.updateUsername");
                        messageBuilder.AppendLine("Result: " + error);

                        HasError = true;
                        Error = AppResources.ServerError;
                        MessageBox.Show(messageBuilder.ToString(), AppResources.ServerError, MessageBoxButton.OK);
                    }
                    else if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST))
                    {
                        if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_NOT_MODIFIED))
                        {
                            _newChannel.UserName = new TLString(username);
                            _newChannel.NotifyOfPropertyChange(() => _newChannel.UserName);

                            HasError = false;
                            Error = string.Empty;
                            NavigationService.GoBack();
                            return;
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_INVALID))
                        {
                            HasError = true;
                            Error = IsMegaGroup ? AppResources.GroupNameInvalid : AppResources.ChannelNameInvalid;
                            MessageBox.Show(IsMegaGroup ? AppResources.GroupNameInvalid : AppResources.ChannelNameInvalid, AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_OCCUPIED))
                        {
                            HasError = true;
                            Error = IsMegaGroup ? AppResources.GroupNameOccupied : AppResources.ChannelNameOccupied;
                            MessageBox.Show(IsMegaGroup ? AppResources.GroupNameOccupied : AppResources.ChannelNameOccupied, AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.CHANNELS_ADMIN_PUBLIC_TOO_MUCH))
                        {
                            HasError = true;
                            Error = AppResources.ChannelsAdminPublicTooMuchShort;
                            MessageBox.Show(AppResources.ChannelsAdminPublicTooMuch, AppResources.Error, MessageBoxButton.OK);
                        }
                        else
                        {
                            HasError = true;
                            Error = error.ToString();
                        }
                    }
                    else
                    {
                        HasError = true;
                        Error = string.Empty;
                        Execute.ShowDebugMessage("channels.updateUsername error " + error);
                    }
                }));
        }

        public void Next()
        {
            if (IsPublic)
            {
                if (string.IsNullOrEmpty(UserName))
                {
                    MessageBox.Show(AppResources.ChoosePublicGroupLinkNotification);

                    RaiseEmptyUserName();

                    return;
                }

                Done();

                return;
            }
            else
            {
                Done();

                return;
            }
        }

        public void Cancel()
        {
            NavigationService.GoBack();
        }
    }
}

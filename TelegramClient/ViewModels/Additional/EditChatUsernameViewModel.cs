// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeniy Nadymov, 2013-2018.
// 
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class EditChatUsernameViewModel : ViewModelBase
    {
        private readonly TLChannel _currentItem;

        public bool IsMegaGroup
        {
            get { return (_currentItem != null && _currentItem.IsMegaGroup); }
        }

        public string Caption
        {
            get
            {
                if (IsMegaGroup)
                {
                    return AppResources.SupergroupName;
                }

                return AppResources.ChannelName;
            }
        }

        public string Description
        {
            get
            {
                if (IsMegaGroup)
                {
                    return AppResources.SupergroupNameDescription;
                }

                return AppResources.ChannelNameDescription;
            }
        }

        public string Description2
        {
            get
            {
                if (IsMegaGroup)
                {
                    return AppResources.SupergroupNameDescription2;
                }

                return AppResources.ChannelNameDescription2;
            }
        }

        public string Link
        {
            get { return Environment.NewLine + string.Format(Constants.UsernameLinkPlaceholder, Username); }
        }

        private string _username;

        public string Username
        {
            get { return _username; }
            set { SetField(ref _username, value, () => Username); }
        }

        private bool _hasError;

        public bool HasError
        {
            get { return _hasError; }
            set
            {
                SetField(ref _hasError, value, () => HasError);
                NotifyOfPropertyChange(() => ErrorBrush);
            }
        }

        private string _error = " ";

        public string Error
        {
            get { return _error; }
            set { SetField(ref _error, value, () => Error); }
        }

        public SolidColorBrush ErrorBrush
        {
            get { return HasError ? new SolidColorBrush(Color.FromArgb(255, 178, 54, 46)) : new SolidColorBrush(Colors.Green); }
        }

        public EditChatUsernameViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _currentItem = StateService.CurrentChat as TLChannel;
            StateService.CurrentChat = null;

            var userName = _currentItem as IUserName;
            if (userName != null && userName.UserName != null)
            {
                Username = userName.UserName.ToString();
            }

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => Username))
                {
                    NotifyOfPropertyChange(() => Link);
                }
            };
        }

        public void Done()
        {
            if (_currentItem == null) return;

            if (IsWorking) return;

            var username = Username;
            if (username != null
                && username.StartsWith("@"))
            {
                username = username.Substring(1, username.Length - 1);
            }

            IsWorking = true;
            MTProtoService.UpdateUsernameAsync(_currentItem.ToInputChannel(), new TLString(username),
                user => BeginOnUIThread(() =>
                {
                    var userName = _currentItem as IUserName;
                    if (userName != null)
                    {
                        userName.UserName = new TLString(username);
                        CacheService.Commit();

                        EventAggregator.Publish(new ChannelNameChangedEventArgs(_currentItem));
                    }

                    IsWorking = false;
                    NavigationService.GoBack();
                }),
                error => BeginOnUIThread(() =>
                {
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
                        messageBuilder.AppendLine("Method: account.updateUsername");
                        messageBuilder.AppendLine("Result: " + error);

                        HasError = true;
                        Error = AppResources.ServerError;
                        MessageBox.Show(messageBuilder.ToString(), AppResources.ServerError, MessageBoxButton.OK);
                    }
                    else if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST))
                    {
                        if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_NOT_MODIFIED))
                        {
                            HasError = false;
                            Error = " ";
                            NavigationService.GoBack();
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.CHANNELS_ADMIN_PUBLIC_TOO_MUCH))
                        {
                            var message = AppResources.ChannelsAdminPublicTooMuchShort;
                            HasError = true;
                            Error = message;
                            MessageBox.Show(message, AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_INVALID))
                        {
                            var message = IsMegaGroup
                                ? AppResources.SupergroupNameInvalid
                                : AppResources.ChannelNameInvalid;
                            HasError = true;
                            Error = message;
                            MessageBox.Show(message, AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_OCCUPIED))
                        {
                            var message = IsMegaGroup
                                ? AppResources.SupergroupNameOccupied
                                : AppResources.ChannelNameOccupied;
                            HasError = true;
                            Error = message;
                            MessageBox.Show(message, AppResources.Error, MessageBoxButton.OK);
                        }
                        else
                        {
                            HasError = true;
                            Error = error.ToString();
                            //Execute.BeginOnUIThread(() => NavigationService.GoBack());
                        }
                    }
                    else
                    {
                        HasError = true;
                        Error = string.Empty;
                        Execute.ShowDebugMessage("channel.updateUsername error " + error);
                    }

                    IsWorking = false;
                }));
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
            var local = Username;
            var username = Username;
            if (username != null
                && username.StartsWith("@"))
            {
                username = username.Substring(1, username.Length - 1);
            }

            if (string.IsNullOrEmpty(username))
            {
                HasError = false;
                //Error = string.Empty;
                return;
            }

            var isValidSymbols = username.All(IsValidSymbol);
            if (!isValidSymbols)
            {
                var message = IsMegaGroup ? AppResources.SupergroupNameInvalid : AppResources.ChannelNameInvalid;
                HasError = true;
                Error = message;
                return;
            }

            if (username[0] >= '0' && username[0] <= '9')
            {
                var message = IsMegaGroup ? AppResources.SupergroupNameStartsWithNumber : AppResources.ChannelNameStartsWithNumber;
                HasError = true;
                Error = message;
                return;
            }

            if (username.Length < Constants.UsernameMinLength)
            {
                var message = IsMegaGroup ? AppResources.SupergroupNameShort : AppResources.ChannelNameShort;
                HasError = true;
                Error = message;
                return;
            }

            if (_currentItem == null) return;

            MTProtoService.CheckUsernameAsync(_currentItem.ToInputChannel(), new TLString(username),
                result => Execute.BeginOnUIThread(() =>
                {
                    if (local != Username) return;

                    HasError = !result.Value;
                    if (HasError)
                    {
                        Error = AppResources.UsernameOccupied;
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
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
                        messageBuilder.AppendLine("Method: channel.checkUsername");
                        messageBuilder.AppendLine("Result: " + error);

                        Error = AppResources.ServerError;
                    }
                    else if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST))
                    {
                        if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_INVALID))
                        {
                            var message = IsMegaGroup
                                ? AppResources.SupergroupNameInvalid
                                : AppResources.ChannelNameInvalid;
                            Error = message;
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.CHANNELS_ADMIN_PUBLIC_TOO_MUCH))
                        {
                            var message = AppResources.ChannelsAdminPublicTooMuchShort;
                            Error = message;
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_OCCUPIED))
                        {
                            var message = IsMegaGroup
                                ? AppResources.SupergroupNameOccupied
                                : AppResources.ChannelNameOccupied;
                            Error = message;
                        }
                        else
                        {
                            Error = error.ToString();
                            //Execute.BeginOnUIThread(() => NavigationService.GoBack());
                        }
                    }
                    else
                    {
                        Error = string.Empty;
                        Execute.ShowDebugMessage("channel.updateUsername error " + error);
                    }
                }));
        }

        public void CopyLink()
        {
            if (string.IsNullOrEmpty(Username)) return;

            Clipboard.SetText(Link.Trim());
            MessageBox.Show(AppResources.CopyLinkHint, AppResources.AppName, MessageBoxButton.OK);
        }

        public void Cancel()
        {
            NavigationService.GoBack();
        }
    }

    public class ChannelNameChangedEventArgs
    {
        public TLChannel Channel { get; set; }

        public ChannelNameChangedEventArgs(TLChannel channel)
        {
            Channel = channel;
        }
    }
}

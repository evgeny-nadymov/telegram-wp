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
    public class EditUsernameViewModel : ViewModelBase
    {
        private string _username;

        public string Username
        {
            get { return _username; }
            set { SetField(ref _username, value, () => Username); }
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

        public EditUsernameViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            var currentUser = CacheService.GetUser(new TLInt(StateService.CurrentUserId)) as IUserName;

            if (currentUser != null
                && currentUser.UserName != null)
            {
                _username = currentUser.UserName.ToString();
            }

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => Username))
                {
                    var userName = Username;
                    BeginOnUIThread(TimeSpan.FromSeconds(0.3), () =>
                    {
                        if (string.Equals(userName, Username))
                        {
                            Check();
                        }
                    });
                }
            };
        }

        public void Done()
        {
            if (IsWorking) return;

            var username = Username;
            if (username != null
                && username.StartsWith("@"))
            {
                username = username.Substring(1, username.Length - 1);
            }

            IsWorking = true;
            MTProtoService.UpdateUsernameAsync(new TLString(username),
                user =>
                {
                    CacheService.SyncUser(user, result => EventAggregator.Publish(new UserNameChangedEventArgs(result)));

                    IsWorking = false;
                    BeginOnUIThread(() => NavigationService.GoBack());
                },
                error => Execute.BeginOnUIThread(() => 
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
                        if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_INVALID))
                        {
                            HasError = true;
                            Error = AppResources.UsernameInvalid;
                            MessageBox.Show(AppResources.UsernameInvalid, AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_OCCUPIED))
                        {
                            HasError = true;
                            Error = AppResources.UsernameOccupied;
                            MessageBox.Show(AppResources.UsernameOccupied, AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_NOT_MODIFIED))
                        {
                            HasError = false;
                            Error = string.Empty;
                            NavigationService.GoBack();
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
                        Execute.ShowDebugMessage("account.updateUsername error " + error);
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
            var checkedUsername = Username;

            var username = Username;
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
                Error = AppResources.UsernameInvalid;
                return;
            }

            if (username[0] >= '0' && username[0] <= '9')
            {
                HasError = true;
                Error = AppResources.UsernameStartsWithNumber;
                return;
            }

            if (username.Length < Constants.UsernameMinLength)
            {
                HasError = true;
                Error = AppResources.UsernameShort;
                return;
            }

            HasError = false;
            IsUsernameAvailable = false;

            MTProtoService.CheckUsernameAsync(new TLString(username),
                result =>
                {
                    HasError = !result.Value;
                    if (HasError)
                    {
                        Error = AppResources.UsernameOccupied;
                    }

                    if (string.Equals(checkedUsername, Username) && result.Value)
                    {
                        IsUsernameAvailable = true;
                        UsernameAvailableString = string.Format(AppResources.NameIsAvailable, checkedUsername);
                    }
                },
                error =>
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
                        messageBuilder.AppendLine("Method: account.checkUsername");
                        messageBuilder.AppendLine("Result: " + error);

                        Error = AppResources.ServerError;
                    }
                    else if (TLRPCError.CodeEquals(error, ErrorCode.BAD_REQUEST))
                    {
                        if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_INVALID))
                        {
                            Error = AppResources.UsernameInvalid;
                        }
                        else if (TLRPCError.TypeEquals(error, ErrorType.USERNAME_OCCUPIED))
                        {
                            Error = AppResources.UsernameOccupied;
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
                        Execute.ShowDebugMessage("account.checkUsername error " + error);
                    }
                });
        }

        public void Cancel()
        {
            NavigationService.GoBack();
        }
    }

    public class UserNameChangedEventArgs
    {
        public TLUserBase User { get; set; }

        public UserNameChangedEventArgs(TLUserBase user)
        {
            User = user;
        }
    }
}

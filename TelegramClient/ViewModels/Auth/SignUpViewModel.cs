// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.EmojiPanel;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.ViewModels.Auth
{
    public class SignUpViewModel : ViewModelBase, Telegram.Api.Aggregator.IHandle<UploadableItem>, Telegram.Api.Aggregator.IHandle<string>
    {
        public byte[] PhotoBytes { get; set; }

        private string _firstName;

        public string FirstName
        {
            get { return _firstName; }
            set { SetField(ref _firstName, value, () => FirstName); }
        }

        private string _lastName;

        public string LastName
        {
            get { return _lastName; }
            set { SetField(ref _lastName, value, () => LastName); }
        }

        public IUploadFileManager FileManager { get; private set; }

        public TLSentCodeBase SentCode { get; private set; }

        public SignUpViewModel(IUploadFileManager fileManager, ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            SentCode = StateService.SentCode;
            StateService.SentCode = null;

            SuppressUpdateStatus = true;

            FileManager = fileManager;
            EventAggregator.Subscribe(this);
            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => FirstName)
                    || Property.NameEquals(args.PropertyName, () => LastName))
                {
                    NotifyOfPropertyChange(() => CanSignUp);
                }
            };
        }

        protected override void OnActivate()
        {
            if (StateService.ClearNavigationStack)
            {
                StateService.ClearNavigationStack = false;
                while (NavigationService.RemoveBackEntry() != null) { }
            }

            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }

            base.OnActivate();
        }

        public bool CanSignUp
        {
            get
            {
                return !IsWorking
                    && FirstName != null && FirstName.Length >= 2;
            }
        }

        public void ChoosePhoto()
        {
            EditCurrentUserActions.EditPhoto(bytes =>
            {
                StateService.ProfilePhotoBytes = bytes;
                PhotoBytes = bytes;
                NotifyOfPropertyChange(() => PhotoBytes);
            });
        }

        public void SignUp()
        {
            var result = MessageBox.Show(
                AppResources.ConfirmAgeMessage,
                AppResources.ConfirmAgeTitle,
                MessageBoxButton.OKCancel);

            if (result != MessageBoxResult.OK) return;

            IsWorking = true;
            NotifyOfPropertyChange(() => CanSignUp);
            MTProtoService.SignUpAsync(
                StateService.PhoneNumber, StateService.PhoneCodeHash, StateService.PhoneCode,
                new TLString(FirstName), new TLString(LastName),
                auth => BeginOnUIThread(() =>
                {
                    TLUtils.IsLogEnabled = false;
                    TLUtils.LogItems.Clear();

                    result = MessageBox.Show(
                        AppResources.ConfirmPushMessage,
                        AppResources.ConfirmPushTitle,
                        MessageBoxButton.OKCancel);

                    if (result != MessageBoxResult.OK)
                    {
                        Notifications.Disable();
                    }
                    else
                    {
                        Notifications.Enable();
                    }

                    ConfirmViewModel.UpdateNotificationsAsync(MTProtoService, StateService);

                    MTProtoService.SetInitState();

                    StateService.CurrentUserId = auth.User.Index;
                    StateService.FirstRun = true;
                    SettingsHelper.SetValue(Constants.IsAuthorizedKey, true);

                    ShellViewModel.Navigate(NavigationService);

                    IsWorking = false;
                    NotifyOfPropertyChange(() => CanSignUp);

                    if (StateService.ProfilePhotoBytes != null)
                    {
                        var bytes = StateService.ProfilePhotoBytes;
                        StateService.ProfilePhotoBytes = null;
                        var fileId = TLLong.Random();
                        FileManager.UploadFile(fileId, new TLUser66 { IsSelf = true }, bytes);
                    }
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    NotifyOfPropertyChange(() => CanSignUp);

                    if (error.TypeEquals(ErrorType.PHONE_NUMBER_INVALID))
                    {
                        MessageBox.Show(AppResources.PhoneNumberInvalidString, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_CODE_INVALID))
                    {
                        MessageBox.Show(AppResources.PhoneCodeInvalidString, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_CODE_EMPTY))
                    {
                        MessageBox.Show(AppResources.PhoneCodeEmpty, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.PHONE_CODE_EXPIRED))
                    {
                        MessageBox.Show(AppResources.PhoneCodeExpiredString, AppResources.Error, MessageBoxButton.OK);
                        ClearViewModel();
                        NavigationService.GoBack();
                    }
                    else if (error.CodeEquals(ErrorCode.FLOOD))
                    {
                        MessageBox.Show(AppResources.FloodWaitString, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.FIRSTNAME_INVALID))
                    {
                        MessageBox.Show(AppResources.FirstNameInvalidString, AppResources.Error, MessageBoxButton.OK);
                    }
                    else if (error.TypeEquals(ErrorType.LASTNAME_INVALID))
                    {
                        MessageBox.Show(AppResources.LastNameInvalidString, AppResources.Error, MessageBoxButton.OK);
                    }
                    else
                    {
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("auth.signUp error " + error);
                    }
                }));
        }

        public void OpenTermsOfService()
        {
            var sentCode80 = SentCode as TLSentCode80;
            if (sentCode80 == null) return;

            var termsOfService = sentCode80.TermsOfService as TLTermsOfService80;
            if (termsOfService == null) return;

            ShowTermsOfService(termsOfService);
            return;
            if (IsWorking) return;

            var countryISO2 = new TLString("en");
            IsWorking = true;
            MTProtoService.GetTermsOfServiceAsync(countryISO2,
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    ShowTermsOfService(result);
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                }));
        }

        public void ShowTermsOfService(TLTermsOfService termsOfService)
        {
            var termsOfService80 = termsOfService as TLTermsOfService80;
            var entities = termsOfService80 != null ? termsOfService80.Entities : null;
            var content = new TelegramRichTextBox
            {
                FontSize = 17.776,
                Margin = new Thickness(0.0, 11.0, 0.0, 0.0),
                TextWrapping = TextWrapping.Wrap,
                DataContext = new TLMessage73 { Flags = new TLInt(0), Message = termsOfService.Text, Entities = entities },
                Text = termsOfService.Text.ToString()
            };

            ShellViewModel.ShowCustomMessageBox(
                string.Empty, AppResources.TermsOfService,
                AppResources.Ok.ToLowerInvariant(), null,
                dismissed =>
                {

                },
                content);
        }

        public void Handle(string command)
        {
            if (string.Equals(command, Commands.LogOutCommand))
            {
                ClearViewModel();
            }
        }

        private void ClearViewModel()
        {
            FirstName = string.Empty;
            LastName = string.Empty;
            PhotoBytes = null;
            IsWorking = false;
        }

        public void Handle(UploadableItem item)
        {

        }
    }
}

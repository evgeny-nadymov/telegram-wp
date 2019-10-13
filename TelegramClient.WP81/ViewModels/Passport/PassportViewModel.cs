// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Windows.Data.Json;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.Views.Additional;

namespace TelegramClient.ViewModels.Passport
{
    public class PassportViewModel : ViewModelBase
    {
        public TLUserBase Bot { get; set; }

        public string Hint
        {
            get
            {
                return Bot == null
                    ? string.Empty
                    : string.Format(AppResources.PassportHint, Bot.FullName);
            }
        }

        public string AuthorizationFormHint
        {
            get
            {
                var username = Bot as IUserName;

                return username == null
                    ? string.Empty
                    : string.Format(AppResources.PassportNoPolicy, Bot.FullName, username.UserName);
            }
        }

        public Visibility ResidentialAddressVisibility
        {
            get
            {
                return _authorizationForm.RequiredTypes.FirstOrDefault(ResidentialAddressViewModel.IsValidType) != null
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public Visibility ResidentialAddressCheckVisibility
        {
            get
            {
                var dataType = _authorizationForm.RequiredTypes.FirstOrDefault(ResidentialAddressViewModel.IsValidDataType);
                if (dataType != null)
                {
                    var data = _authorizationForm.Values.FirstOrDefault(x => x.Type.GetType() == dataType.GetType());
                    if (data == null)
                    {
                        return Visibility.Collapsed;
                    }
                }
                
                var dataProofType = _authorizationForm.RequiredTypes.FirstOrDefault(ResidentialAddressViewModel.IsValidProofType);
                if (dataProofType != null)
                {
                    var dataProof = _authorizationForm.Values.FirstOrDefault(x => x.Type.GetType() == dataProofType.GetType());
                    if (dataProof == null)
                    {
                        return Visibility.Collapsed;
                    }
                }

                var type = _authorizationForm.RequiredTypes.FirstOrDefault(ResidentialAddressViewModel.IsValidType);
                if (type == null)
                {
                    return Visibility.Collapsed;
                }

                var error = _authorizationForm.Errors.FirstOrDefault(x => ResidentialAddressViewModel.IsValidType(x.Type));
                if (error != null)
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        public string ResidentialAddressHint
        {
            get
            {
                var value = _authorizationForm.Values.FirstOrDefault(x => ResidentialAddressViewModel.IsValidDataType(x.Type));
                if (value != null)
                {
                    return SecureValueToHintConverter.Convert(value);
                }

                return AppResources.PassportAddressInfo;
            }
        }

        public string ResidentialAddressError
        {
            get
            {
                var value = _authorizationForm.Errors.FirstOrDefault(x => ResidentialAddressViewModel.IsValidType(x.Type));
                if (value != null)
                {
                    return AppResources.PassportCorrectErrors;
                }

                return null;
            }
        }
        
        public ObservableCollection<SecureRequiredType> RequiredTypes { get; set; }

        private readonly TLAuthorizationForm _authorizationForm;

        private readonly TLPasswordBase _passwordBase;

        public PassportViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            RequiredTypes = new ObservableCollection<SecureRequiredType>();

            _authorizationForm = stateService.AuthorizationForm;
            stateService.AuthorizationForm = null;

            _passwordBase = stateService.Password;
            stateService.Password = null;

            if (_authorizationForm != null)
            {
                Bot = _authorizationForm.Users.LastOrDefault();
            }

            GetRequiredTypes();
        }

        private void GetRequiredTypes()
        {
            var authorizationForm85 = _authorizationForm as TLAuthorizationForm85;
            if (authorizationForm85 != null)
            {
                var hasPersonalDetails = false;
                var hasResidentialAddress = false;
                var personalDetailsProofCount = 0;
                var residentialAddressProofCount = 0;
                foreach (var requiredTypeBase in authorizationForm85.NewRequiredTypes)
                {
                    var requiredType = requiredTypeBase as TLSecureRequiredType;
                    if (requiredType != null)
                    {
                        if (PersonalDetailsViewModel.IsValidDataType(requiredType.Type))
                        {
                            hasPersonalDetails = true;
                        }
                        else if (ResidentialAddressViewModel.IsValidDataType(requiredType.Type))
                        {
                            hasResidentialAddress = true;
                        }
                        else if (PersonalDetailsViewModel.IsValidProofType(requiredType.Type))
                        {
                            personalDetailsProofCount++;
                        }
                        else if (ResidentialAddressViewModel.IsValidProofType(requiredType.Type))
                        {
                            residentialAddressProofCount++;
                        }
                    }

                    var requiredTypeOneOf = requiredTypeBase as TLSecureRequiredTypeOneOf;
                    if (requiredTypeOneOf != null && requiredTypeOneOf.Types.Count > 0)
                    {
                        requiredType = requiredTypeOneOf.Types[0] as TLSecureRequiredType;
                        if (requiredType != null)
                        {
                            if (PersonalDetailsViewModel.IsValidProofType(requiredType.Type))
                            {
                                personalDetailsProofCount++;
                            }
                            else if (ResidentialAddressViewModel.IsValidProofType(requiredType.Type))
                            {
                                residentialAddressProofCount++;
                            }
                        }
                    }
                }

                var joinPersonalDetails = hasPersonalDetails && personalDetailsProofCount == 1;
                var joinResidentialAddress = hasResidentialAddress && residentialAddressProofCount == 1;

                SecureRequiredType personalDetails = null;
                SecureRequiredType residentialAddress = null;
                var requiredTypes = new ObservableCollection<SecureRequiredType>();
                foreach (var requiredTypeBase in authorizationForm85.NewRequiredTypes)
                {
                    var requiredTypeOneOf = requiredTypeBase as TLSecureRequiredTypeOneOf;
                    if (requiredTypeOneOf != null && requiredTypeOneOf.Types.Count > 0)
                    {
                        var firstRequiredType = requiredTypeOneOf.Types[0] as TLSecureRequiredType;
                        if (firstRequiredType != null)
                        {
                            if (PersonalDetailsViewModel.IsValidType(firstRequiredType.Type))
                            {
                                personalDetails = AddPersonalDetailsType(joinPersonalDetails, requiredTypes, requiredTypeOneOf, authorizationForm85, personalDetails, PersonalDetailsViewModel.IsValidDataType(firstRequiredType.Type));
                            }
                            else if (ResidentialAddressViewModel.IsValidType(firstRequiredType.Type))
                            {
                                residentialAddress = AddResidentialAddressType(joinResidentialAddress, requiredTypes, requiredTypeOneOf, authorizationForm85, residentialAddress, ResidentialAddressViewModel.IsValidDataType(firstRequiredType.Type));
                            }
                        }
                    }

                    var requiredType = requiredTypeBase as TLSecureRequiredType;
                    if (requiredType != null)
                    {
                        if (PhoneNumberViewModel.IsValidType(requiredType.Type))
                        {
                            requiredTypes.Add(new SecureRequiredType(requiredType, null, authorizationForm85));
                        }
                        else if (EmailViewModel.IsValidType(requiredType.Type))
                        {
                            requiredTypes.Add(new SecureRequiredType(requiredType, null, authorizationForm85));
                        }
                        else if (PersonalDetailsViewModel.IsValidType(requiredType.Type))
                        {
                            personalDetails = AddPersonalDetailsType(joinPersonalDetails, requiredTypes, requiredType, authorizationForm85, personalDetails, PersonalDetailsViewModel.IsValidDataType(requiredType.Type));
                        }
                        else if (ResidentialAddressViewModel.IsValidType(requiredType.Type))
                        {
                            residentialAddress = AddResidentialAddressType(joinResidentialAddress, requiredTypes, requiredType, authorizationForm85, residentialAddress, ResidentialAddressViewModel.IsValidDataType(requiredType.Type));
                        }
                    }
                }

                RequiredTypes = requiredTypes;
            }
        }

        private static SecureRequiredType AddResidentialAddressType(
            bool joinResidentialAddress, ObservableCollection<SecureRequiredType> requiredTypes, 
            TLSecureRequiredTypeBase requiredType, TLAuthorizationForm85 authorizationForm85, SecureRequiredType residentialAddress,
            bool isValidDataType)
        {
            if (!joinResidentialAddress)
            {
                requiredTypes.Add(isValidDataType
                    ? new SecureRequiredType(requiredType as TLSecureRequiredType, null, authorizationForm85)
                    : new SecureRequiredType(null, requiredType, authorizationForm85));
            }
            else
            {
                if (residentialAddress == null)
                {
                    residentialAddress = isValidDataType
                        ? new SecureRequiredType(requiredType as TLSecureRequiredType, null, authorizationForm85)
                        : new SecureRequiredType(null, requiredType, authorizationForm85);

                    requiredTypes.Add(residentialAddress);
                }
                else
                {
                    if (isValidDataType)
                    {
                        residentialAddress.SetData(requiredType as TLSecureRequiredType);
                    }
                    else
                    {
                        residentialAddress.SetDataProof(requiredType);
                    }
                }
            }
            return residentialAddress;
        }

        private static SecureRequiredType AddPersonalDetailsType(
            bool joinPersonalDetails, ObservableCollection<SecureRequiredType> requiredTypes,
            TLSecureRequiredTypeBase requiredType, TLAuthorizationForm85 authorizationForm85, SecureRequiredType personalDetails,
            bool isValidDataType)
        {
            if (!joinPersonalDetails)
            {
                requiredTypes.Add(isValidDataType
                    ? new SecureRequiredType(requiredType as TLSecureRequiredType, null, authorizationForm85)
                    : new SecureRequiredType(null, requiredType, authorizationForm85));
            }
            else
            {
                if (personalDetails == null)
                {
                    personalDetails = isValidDataType
                        ? new SecureRequiredType(requiredType as TLSecureRequiredType, null, authorizationForm85)
                        : new SecureRequiredType(null, requiredType, authorizationForm85);

                    requiredTypes.Add(personalDetails);
                }
                else
                {
                    if (isValidDataType)
                    {
                        personalDetails.SetData(requiredType as TLSecureRequiredType);
                    }
                    else
                    {
                        personalDetails.SetDataProof(requiredType);
                    }
                }
            }
            return personalDetails;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            NotifyOfPropertyChange(() => ResidentialAddressHint);
            NotifyOfPropertyChange(() => ResidentialAddressCheckVisibility);
            NotifyOfPropertyChange(() => ResidentialAddressError);

            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }
        }
        
        public void EditPersonalDetails(SecureRequiredType secureRequiredType)
        {
            var requiredProofTypes = secureRequiredType.DataProofRequiredType as TLSecureRequiredTypeOneOf;
            if (requiredProofTypes != null && requiredProofTypes.Types.Count > 1)
            {
                var addressProof = secureRequiredType.DataProofValue;
                if (addressProof == null)
                {
                    var panel = new StackPanel { Margin = new Thickness(0.0, 12.0, 0.0, 0.0) };
                    var messageBox = ShellViewModel.ShowCustomMessageBox(
                        null, AppResources.PassportAddress,
                        null, null,
                        dismissed =>
                        {

                        },
                        requiredProofTypes.Types.Count > 10 ?
                        (object)new ScrollViewer { MaxHeight = 650.0, Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled } :
                        panel);

                    for (var i = 0; i < requiredProofTypes.Types.Count; i++)
                    {
                        var requiredProofType = requiredProofTypes.Types[i] as TLSecureRequiredType;
                        if (requiredProofType != null)
                        {
                            var listBoxItem = new ListBoxItem
                            {
                                Content =
                                    new TextBlock
                                    {
                                        Text = SecureRequiredTypeToCaptionConverter.Convert(requiredProofType.Type),
                                        FontSize = 27,
                                        Margin = new Thickness(12.0)
                                    },
                                DataContext = requiredProofType
                            };
                            TiltEffect.SetIsTiltEnabled(listBoxItem, true);
                            listBoxItem.Tap += (sender, args) =>
                            {
                                messageBox.Dismiss();
                                var item = sender as ListBoxItem;
                                if (item != null)
                                {
                                    requiredProofType = item.DataContext as TLSecureRequiredType;
                                    if (requiredProofType != null)
                                    {
                                        secureRequiredType.SetSelectedDataProof(requiredProofType);
                                        EditPersonalDetailsInternal(secureRequiredType);
                                    }
                                }

                            };

                            panel.Children.Add(listBoxItem);
                        }
                    }
                }
                else
                {
                    EditPersonalDetailsInternal(secureRequiredType);
                }
            }
            else
            {
                EditPersonalDetailsInternal(secureRequiredType);
            }
        }

        private void EditPersonalDetailsInternal(SecureRequiredType secureRequiredType)
        {
            var authorizationForm85 = _authorizationForm as TLAuthorizationForm85;
            if (authorizationForm85 != null && authorizationForm85.Config != null && authorizationForm85.Config.CountriesLangsObject == null)
            {
                JsonObject result;
                if (JsonObject.TryParse(authorizationForm85.Config.CountriesLangs.Data.ToString(), out result))
                {
                    authorizationForm85.Config.CountriesLangsObject = result;
                }
            }

            StateService.SecureRequiredType = secureRequiredType;
            StateService.AuthorizationForm = _authorizationForm;
            StateService.Password = _passwordBase;
            NavigationService.UriFor<PersonalDetailsViewModel>().Navigate();
        }

        public void EditResidentialAddress(SecureRequiredType secureRequiredType)
        {
            var requiredProofTypes = secureRequiredType.DataProofRequiredType as TLSecureRequiredTypeOneOf;
            if (requiredProofTypes != null && requiredProofTypes.Types.Count > 1)
            {
                var addressProof = secureRequiredType.DataProofValue;
                if (addressProof == null)
                {
                    var panel = new StackPanel { Margin = new Thickness(0.0, 12.0, 0.0, 0.0) };
                    var messageBox = ShellViewModel.ShowCustomMessageBox(
                        null, AppResources.PassportAddress,
                        null, null,
                        dismissed =>
                        {

                        },
                        requiredProofTypes.Types.Count > 10 ?
                        (object)new ScrollViewer { MaxHeight = 650.0, Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled } :
                        panel);

                    for (var i = 0; i < requiredProofTypes.Types.Count; i++)
                    {
                        var requiredProofType = requiredProofTypes.Types[i] as TLSecureRequiredType;
                        if (requiredProofType != null)
                        {
                            var listBoxItem = new ListBoxItem
                            {
                                Content =
                                    new TextBlock
                                    {
                                        Text = SecureRequiredTypeToCaptionConverter.Convert(requiredProofType.Type),
                                        FontSize = 27,
                                        Margin = new Thickness(12.0)
                                    },
                                DataContext = requiredProofType
                            };
                            TiltEffect.SetIsTiltEnabled(listBoxItem, true);
                            listBoxItem.Tap += (sender, args) =>
                            {
                                messageBox.Dismiss();
                                var item = sender as ListBoxItem;
                                if (item != null)
                                {
                                    requiredProofType = item.DataContext as TLSecureRequiredType;
                                    if (requiredProofType != null)
                                    {
                                        secureRequiredType.SetSelectedDataProof(requiredProofType);
                                        EditResidentialAddressInternal(secureRequiredType);
                                    }
                                }

                            };

                            panel.Children.Add(listBoxItem);
                        }
                    }
                }
                else
                {
                    EditResidentialAddressInternal(secureRequiredType);
                }
            }
            else
            {
                EditResidentialAddressInternal(secureRequiredType);
            }
        }

        private void EditResidentialAddressInternal(SecureRequiredType secureRequiredType)
        {
            StateService.SecureRequiredType = secureRequiredType;
            StateService.AuthorizationForm = _authorizationForm;
            StateService.Password = _passwordBase;
            NavigationService.UriFor<ResidentialAddressViewModel>().Navigate();
        }

        public void EditPhoneNumber(SecureRequiredType secureRequiredType)
        {
            if (secureRequiredType == null) return;

            if (secureRequiredType.IsCompleted)
            {
                ShellViewModel.ShowCustomMessageBox(AppResources.PassportDeletePhoneNumberAlert, AppResources.AppName,
                    AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                    dismissed =>
                    {
                        if (dismissed == CustomMessageBoxResult.RightButton)
                        {
                            IsWorking = true;
                            MTProtoService.DeleteSecureValueAsync(new TLVector<TLSecureValueTypeBase>{ new TLSecureValueTypePhone() },
                                result => Execute.BeginOnUIThread(() =>
                                {
                                    IsWorking = false;

                                    for (var i = 0; i < _authorizationForm.Values.Count; i++)
                                    {
                                        if (PhoneNumberViewModel.IsValidType(_authorizationForm.Values[i].Type))
                                        {
                                            _authorizationForm.Values.RemoveAt(i--);
                                        }
                                    }

                                    secureRequiredType.UpdateValue();
                                }),
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    IsWorking = false;
                                }));
                        }
                    });
            }
            else
            {
                StateService.SecureRequiredType = secureRequiredType;
                StateService.AuthorizationForm = _authorizationForm;
                StateService.Password = _passwordBase;
                NavigationService.UriFor<PhoneNumberViewModel>().Navigate();
            }
        }

        public void EditEmail(SecureRequiredType secureRequiredType)
        {
            if (secureRequiredType == null) return;

            if (secureRequiredType.IsCompleted)
            {
                ShellViewModel.ShowCustomMessageBox(AppResources.PassportDeleteEmailAlert, AppResources.AppName,
                    AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                    dismissed =>
                    {
                        if (dismissed == CustomMessageBoxResult.RightButton)
                        {
                            IsWorking = true;
                            MTProtoService.DeleteSecureValueAsync(new TLVector<TLSecureValueTypeBase> { new TLSecureValueTypeEmail() },
                                result => Execute.BeginOnUIThread(() =>
                                {
                                    IsWorking = false;

                                    for (var i = 0; i < _authorizationForm.Values.Count; i++)
                                    {
                                        if (EmailViewModel.IsValidType(_authorizationForm.Values[i].Type))
                                        {
                                            _authorizationForm.Values.RemoveAt(i--);
                                        }
                                    }

                                    secureRequiredType.UpdateValue();
                                }),
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    IsWorking = false;
                                }));
                        }
                    });
            }
            else
            {
                StateService.SecureRequiredType = secureRequiredType;
                StateService.AuthorizationForm = _authorizationForm;
                StateService.Password = _passwordBase;
                NavigationService.UriFor<EmailViewModel>().Navigate();
            }
        }

        public void Edit(SecureRequiredType secureRequiredType)
        {
            TLSecureRequiredType requiredType = null;
            var requiredTypeOneOf = secureRequiredType.DataProofRequiredType as TLSecureRequiredTypeOneOf;
            if (requiredTypeOneOf != null)
            {
                var firstRequiredType = requiredTypeOneOf.Types.FirstOrDefault() as TLSecureRequiredType;
                if (firstRequiredType != null)
                {
                    requiredType = firstRequiredType;
                }
            }

            requiredType = requiredType ?? secureRequiredType.DataProofRequiredType as TLSecureRequiredType;
            requiredType = requiredType ?? secureRequiredType.DataRequiredType ?? secureRequiredType.SelectedDataProofRequiredType;

            if (requiredType != null)
            {
                if (PersonalDetailsViewModel.IsValidType(requiredType.Type))
                {
                    EditPersonalDetails(secureRequiredType);
                    return;
                }

                if (ResidentialAddressViewModel.IsValidType(requiredType.Type))
                {
                    EditResidentialAddress(secureRequiredType);
                    return;
                }

                if (PhoneNumberViewModel.IsValidType(requiredType.Type))
                {
                    EditPhoneNumber(secureRequiredType);
                    return;
                }

                if (EmailViewModel.IsValidType(requiredType.Type))
                {
                    EditEmail(secureRequiredType);
                    return;
                }
            }
        }

        public string Error { get; set; }

        private bool HasErrors()
        {
            foreach (var requiredType in RequiredTypes)
            {
                if (!requiredType.IsCompleted)
                {
                    Error = "REQUIRED_TYPE";
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }
            }

            Error = null;
            NotifyOfPropertyChange(() => Error);
            return false;
        }

        public void Authorize()
        {
            if (HasErrors())
            {
                return;
            }

            var password = _passwordBase as TLPassword;
            if (password == null)
            {
                return;
            }

            var passwordSettings = password.Settings as TLPasswordSettings81;
            if (passwordSettings == null)
            {
                return;
            }

            var credentialSecret = Utils.Passport.GenerateSecret(TLString.Empty);
            
            var credentials = Utils.Passport.GenerateSecureCredentialsEncrypted(RequiredTypes, _authorizationForm, credentialSecret, EnterPasswordViewModel.Secret);
            if (credentials == null) return;

            var valueHashes = Utils.Passport.GenerateValueHashes(RequiredTypes);

            IsWorking = true;
            MTProtoService.AcceptAuthorizationAsync(
                _authorizationForm.BotId,
                _authorizationForm.Scope,
                _authorizationForm.PublicKey,
                valueHashes,
                credentials,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    if (!TLString.IsNullOrEmpty(_authorizationForm.CallbackUrl))
                    {
                        var uriBuilder = new UriBuilder(new Uri(_authorizationForm.CallbackUrl.ToString()));
                        var query = TelegramUriMapper.ParseQueryString(uriBuilder.Query);
                        query["tg_passport"] = "success";
                        var queryBuilder = new List<string>();
                        foreach (var item in query)
                        {
                            queryBuilder.Add(string.Format("{0}={1}", item.Key, item.Value));
                        }
                        uriBuilder.Query = string.Join("&", queryBuilder);
                        var url = uriBuilder.ToString();

                        var task = new WebBrowserTask();
                        task.URL = HttpUtility.UrlEncode(url);
                        task.Show();
                    }
                    NavigationService.GoBack();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                    {
                        ShellViewModel.ShowCustomMessageBox(
                           "account.acceptAuthorization" + Environment.NewLine + error.Message,
                           AppResources.AppName,
                           AppResources.Ok);
                    }
                }));
        }

        public bool IsCancelConfirmed { get; protected set; }

        public void Cancel()
        {
            IsCancelConfirmed = true;

            if (!TLString.IsNullOrEmpty(_authorizationForm.CallbackUrl))
            {
                var uriBuilder = new UriBuilder(new Uri(_authorizationForm.CallbackUrl.ToString()));
                var query = TelegramUriMapper.ParseQueryString(uriBuilder.Query);
                query["tg_passport"] = "cancel";
                var queryBuilder = new List<string>();
                foreach (var item in query)
                {
                    queryBuilder.Add(string.Format("{0}={1}", item.Key, item.Value));
                }
                uriBuilder.Query = string.Join("&", queryBuilder);
                var url = HttpUtility.UrlEncode(uriBuilder.ToString());

                var handled = false;
                ShellViewModel.ShowCustomMessageBox(
                    string.Format(AppResources.OpenUrlConfirmation, HttpUtility.UrlDecode(url)),
                    AppResources.AppName,
                    AppResources.Ok,
                    AppResources.Cancel,
                    dismissed =>
                    {
                        if (handled)
                        {
                            return;
                        }

                        handled = true;
                        if (dismissed == CustomMessageBoxResult.RightButton)
                        {
                            var task = new WebBrowserTask();
                            task.URL = url;
                            task.Show();
                            NavigationService.GoBack();
                        }
                        else
                        {
                            BeginOnUIThread(TimeSpan.FromSeconds(0.15), () =>
                            {
                                NavigationService.GoBack();
                            });
                        }
                    });
            }
        }
    }

    public class SecureRequiredType : PropertyChangedBase
    {
        public string Caption
        {
            get
            {
                TLSecureRequiredType requiredType = null;

                var requiredTypeOneOf = DataProofRequiredType as TLSecureRequiredTypeOneOf;
                if (requiredTypeOneOf != null && requiredTypeOneOf.Types.Count > 0)
                {
                    if (requiredTypeOneOf.Types.Count == 1)
                    {
                        requiredType = requiredTypeOneOf.Types[0] as TLSecureRequiredType;
                        if (requiredType != null)
                        {
                            return SecureRequiredTypeToCaptionConverter.Convert(requiredType.Type);
                        }
                    }
                    else if (requiredTypeOneOf.Types.Count == 2)
                    {
                        requiredType = requiredTypeOneOf.Types[0] as TLSecureRequiredType;
                        var requiredType2 = requiredTypeOneOf.Types[1] as TLSecureRequiredType;
                        if (requiredType != null && requiredType2 != null)
                        {
                            return string.Format(AppResources.PassportOr, SecureRequiredTypeToCaptionConverter.Convert(requiredType.Type), SecureRequiredTypeToCaptionConverter.Convert(requiredType2.Type));
                        }
                    }
                    else
                    {
                        requiredType = requiredTypeOneOf.Types[0] as TLSecureRequiredType;
                        if (requiredType != null)
                        {
                            if (PersonalDetailsViewModel.IsValidType(requiredType.Type))
                            {
                                return AppResources.PassportIdentityDocument;
                            }
                            if (ResidentialAddressViewModel.IsValidType(requiredType.Type))
                            {
                                return AppResources.PassportResidentialAddress;
                            }
                            if (PhoneNumberViewModel.IsValidType(requiredType.Type))
                            {
                                return AppResources.PassportPhoneNumber;
                            }
                            if (EmailViewModel.IsValidType(requiredType.Type))
                            {
                                return AppResources.PassportEmail;
                            }
                        }
                    }
                }

                requiredType = DataProofRequiredType as TLSecureRequiredType;
                if (requiredType != null)
                {
                    return SecureRequiredTypeToCaptionConverter.Convert(requiredType.Type);
                }

                requiredType = DataRequiredType as TLSecureRequiredType;
                if (requiredType != null)
                {
                    return SecureRequiredTypeToCaptionConverter.Convert(requiredType.Type);
                }

#if DEBUG
                return DataRequiredType != null ? DataRequiredType.ToString() : null;
#endif

                return string.Empty;
            }
        }

        public string Hint
        {
            get
            {
                if (DataValue != null)
                {
                    return SecureValueToHintConverter.Convert(DataValue);
                }

                if (DataProofValue != null)
                {
                    return SecureValueToHintConverter.Convert(DataProofValue);
                }

                TLSecureRequiredType requiredType = null;

                var requiredTypeOneOf = DataProofRequiredType as TLSecureRequiredTypeOneOf;
                if (requiredTypeOneOf != null && requiredTypeOneOf.Types.Count > 0)
                {
                    if (requiredTypeOneOf.Types.Count == 1)
                    {
                        requiredType = requiredTypeOneOf.Types[0] as TLSecureRequiredType;
                        if (requiredType != null)
                        {
                            return SecureRequiredTypeToHintConverter.Convert(requiredType.Type);
                        }
                    }
                    else
                    {
                        requiredType = requiredTypeOneOf.Types[0] as TLSecureRequiredType;
                        if (requiredType != null)
                        {
                            if (PersonalDetailsViewModel.IsValidType(requiredType.Type))
                            {
                                return AppResources.PassportIdentityDocumentInfo;
                            }
                            if (ResidentialAddressViewModel.IsValidType(requiredType.Type))
                            {
                                return AppResources.PassportAddressInfo;
                            }
                            if (PhoneNumberViewModel.IsValidType(requiredType.Type))
                            {
                                return AppResources.PassportPhoneInfo;
                            }
                            if (EmailViewModel.IsValidType(requiredType.Type))
                            {
                                return AppResources.PassportEmailInfo;
                            }
                        }
                    }
                }

                requiredType = DataProofRequiredType as TLSecureRequiredType;
                if (requiredType != null)
                {
                    return SecureRequiredTypeToHintConverter.Convert(requiredType.Type);
                }

                requiredType = DataRequiredType as TLSecureRequiredType;
                if (requiredType != null)
                {
                    return SecureRequiredTypeToHintConverter.Convert(requiredType.Type);
                }

                return string.Empty;
            }
        }

        public string Error
        {
            get
            {
                if (DataRequiredType != null && PersonalDetailsViewModel.IsValidType(DataRequiredType.Type))
                {
                    var error = GetFirstOrDefaultError(AuthorizationForm.Errors.Where(x => PersonalDetailsViewModel.IsValidDataType(x.Type)));
                    
                    return error != null ? error.Text.ToString() : null;
                }
                if (SelectedDataProofRequiredType != null && PersonalDetailsViewModel.IsValidType(SelectedDataProofRequiredType.Type))
                {
                    var error = GetFirstOrDefaultError(AuthorizationForm.Errors.Where(x => PersonalDetailsViewModel.IsValidProofType(x.Type)));

                    return error != null ? error.Text.ToString() : null;
                }
                if (DataRequiredType != null && ResidentialAddressViewModel.IsValidType(DataRequiredType.Type))
                {
                    var error = GetFirstOrDefaultError(AuthorizationForm.Errors.Where(x => ResidentialAddressViewModel.IsValidDataType(x.Type)));

                    return error != null ? error.Text.ToString() : null;
                }
                if (SelectedDataProofRequiredType != null && ResidentialAddressViewModel.IsValidType(SelectedDataProofRequiredType.Type))
                {
                    var error = GetFirstOrDefaultError(AuthorizationForm.Errors.Where(x => ResidentialAddressViewModel.IsValidProofType(x.Type)));

                    return error != null ? error.Text.ToString() : null;
                }
                if (DataRequiredType != null && PhoneNumberViewModel.IsValidType(DataRequiredType.Type))
                {
                    var error = GetFirstOrDefaultError(AuthorizationForm.Errors.Where(x => PhoneNumberViewModel.IsValidType(x.Type)));

                    return error != null ? error.Text.ToString() : null;
                }
                if (DataRequiredType != null && EmailViewModel.IsValidType(DataRequiredType.Type))
                {
                    var error = GetFirstOrDefaultError(AuthorizationForm.Errors.Where(x => EmailViewModel.IsValidType(x.Type)));

                    return error != null ? error.Text.ToString() : null;
                }

                return null;
            }
        }

        public bool IsCompleted
        {
            get
            {
                var dataRequiredType = DataRequiredType;
                var dataProofRequiredType = DataProofRequiredType;
                var selectedDataProofRequiredType = SelectedDataProofRequiredType;

                if (dataRequiredType != null)
                {
                    if (PersonalDetailsViewModel.IsValidDataType(dataRequiredType.Type))
                    {
                        if (DataValue == null)
                        {
                            return false;
                        }

                        if (dataRequiredType.NativeNames)
                        {
                            var personalDetails = DataValue.Data.DecryptedData as PersonalDetailsRootObject;
                            if (personalDetails != null)
                            {
                                if (string.IsNullOrEmpty(personalDetails.first_name_native)) return false;
                                if (string.IsNullOrEmpty(personalDetails.last_name_native)) return false;
                            }
                        }

                        var error = AuthorizationForm.Errors.FirstOrDefault(x => PersonalDetailsViewModel.IsValidDataType(x.Type));
                        if (error != null)
                        {
                            return false;
                        }
                    }
                    else if (ResidentialAddressViewModel.IsValidDataType(dataRequiredType.Type))
                    {
                        if (DataValue == null)
                        {
                            return false;
                        }

                        var error = AuthorizationForm.Errors.FirstOrDefault(x => ResidentialAddressViewModel.IsValidDataType(x.Type));
                        if (error != null)
                        {
                            return false;
                        }
                    }
                    else if (PhoneNumberViewModel.IsValidType(dataRequiredType.Type))
                    {
                        if (DataValue != null)
                        {
                            var plainData = DataValue.PlainData as TLSecurePlainPhone;
                            if (plainData != null && !TLString.IsNullOrEmpty(plainData.Phone))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                    else if (EmailViewModel.IsValidType(dataRequiredType.Type))
                    {
                        if (DataValue != null)
                        {
                            var plainData = DataValue.PlainData as TLSecurePlainEmail;
                            if (plainData != null && !TLString.IsNullOrEmpty(plainData.Email))
                            {
                                return true;
                            }
                        }

                        return false;
                    }
                }

                if (dataProofRequiredType != null && selectedDataProofRequiredType == null)
                {
                    return false;
                }

                if (selectedDataProofRequiredType != null)
                {
                    if (PersonalDetailsViewModel.IsValidProofType(selectedDataProofRequiredType.Type))
                    {
                        if (DataProofValue == null)
                        {
                            return false;
                        }

                        if (selectedDataProofRequiredType.SelfieRequired && DataProofValue.Selfie == null)
                        {
                            return false;
                        }

                        if (selectedDataProofRequiredType.TranslationRequired && (DataProofValue.Translation == null || DataProofValue.Translation.Count == 0))
                        {
                            return false;
                        }

                        var error = AuthorizationForm.Errors.FirstOrDefault(x => PersonalDetailsViewModel.IsValidProofType(x.Type));
                        if (error != null)
                        {
                            return false;
                        }
                    }
                    else if (ResidentialAddressViewModel.IsValidProofType(selectedDataProofRequiredType.Type))
                    {
                        if (DataProofValue == null)
                        {
                            return false;
                        }

                        if (selectedDataProofRequiredType.SelfieRequired && DataProofValue.Selfie == null)
                        {
                            return false;
                        }

                        if (selectedDataProofRequiredType.TranslationRequired && (DataProofValue.Translation == null || DataProofValue.Translation.Count == 0))
                        {
                            return false;
                        }

                        var error = AuthorizationForm.Errors.FirstOrDefault(x => ResidentialAddressViewModel.IsValidProofType(x.Type));
                        if (error != null)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public TLSecureRequiredType DataRequiredType { get; protected set; }

        public TLSecureValue85 DataValue { get; protected set; }

        public TLSecureRequiredTypeBase DataProofRequiredType { get; protected set; }

        public TLSecureRequiredType SelectedDataProofRequiredType { get; protected set; }

        public TLSecureValue85 DataProofValue { get; protected set; }

        public TLAuthorizationForm85 AuthorizationForm { get; protected set; }

        public SecureRequiredType(TLSecureRequiredType dataRequiredType, TLSecureRequiredTypeBase dataProofRequiredType, TLAuthorizationForm85 authorizationForm)
        {
            AuthorizationForm = authorizationForm;

            SetData(dataRequiredType);
            SetDataProof(dataProofRequiredType);
        }

        private static TLSecureValueErrorBase GetFirstOrDefaultError(IEnumerable<TLSecureValueErrorBase> errors)
        {
            if (errors == null) return null;

            return errors.OrderBy(x => x.Priority).FirstOrDefault();
        }

        public void SetData(TLSecureRequiredType dataRequiredType)
        {
            DataRequiredType = dataRequiredType;
            DataValue = GetValue(dataRequiredType, AuthorizationForm);
        }

        public void SetDataProof(TLSecureRequiredTypeBase dataProofRequiredTypeBase)
        {
            DataProofRequiredType = dataProofRequiredTypeBase;
            DataProofValue = GetValue(dataProofRequiredTypeBase, AuthorizationForm);

            if (DataProofValue != null)
            {
                var dataProofRequiredType = dataProofRequiredTypeBase as TLSecureRequiredType;
                var dataProofRequiredTypeOneOf = dataProofRequiredTypeBase as TLSecureRequiredTypeOneOf;
                if (dataProofRequiredType != null)
                {
                    SetSelectedDataProof(dataProofRequiredType);
                }
                else if (dataProofRequiredTypeOneOf != null)
                {
                    foreach (var type in dataProofRequiredTypeOneOf.Types.OfType<TLSecureRequiredType>())
                    {
                        if (DataProofValue.Type.GetType() == type.Type.GetType())
                        {
                            SetSelectedDataProof(type);
                        }
                    }
                }
            }
            else if (DataProofRequiredType is TLSecureRequiredType)
            {
                SelectedDataProofRequiredType = (TLSecureRequiredType)DataProofRequiredType;
            }
        }

        public void SetSelectedDataProof(TLSecureRequiredType selectedDataProofRequiredType)
        {
            SelectedDataProofRequiredType = selectedDataProofRequiredType;
        }

        private static TLSecureValue85 GetValue(TLSecureRequiredTypeBase requiredTypeBase, TLAuthorizationForm85 authorizationForm)
        {
            if (requiredTypeBase == null)
            {
                return null;
            }

            if (authorizationForm.Values.Count > 0)
            {
                var requiredType = requiredTypeBase as TLSecureRequiredType;
                if (requiredType != null)
                {
                    return authorizationForm.Values.FirstOrDefault(x => requiredType.Type.GetType() == x.Type.GetType()) as TLSecureValue85;
                }

                var requiredTypeOneOf = requiredTypeBase as TLSecureRequiredTypeOneOf;
                if (requiredTypeOneOf != null)
                {
                    var dict = new Dictionary<Type, Type>();
                    foreach (var type in requiredTypeOneOf.Types.OfType<TLSecureRequiredType>())
                    {
                        dict[type.Type.GetType()] = type.Type.GetType();
                    }

                    return authorizationForm.Values.FirstOrDefault(x => dict.ContainsKey(x.Type.GetType())) as TLSecureValue85;
                }
            }

            return null;
        }

        public void UpdateValue()
        {
            DataValue = GetValue(DataRequiredType, AuthorizationForm);
            DataProofValue = GetValue(DataProofRequiredType, AuthorizationForm);

            NotifyOfPropertyChange(() => Hint);
            NotifyOfPropertyChange(() => Error);
            NotifyOfPropertyChange(() => IsCompleted);
        }
    }
}
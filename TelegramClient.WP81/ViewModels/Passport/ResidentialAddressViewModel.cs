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
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.Storage;
using Windows.Storage.Pickers;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Models;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Media;
using TelegramClient.Views.Additional;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Passport
{
    public class ResidentialAddressViewModel : ViewModelBase, Telegram.Api.Aggregator.IHandle<UploadableItem>, Telegram.Api.Aggregator.IHandle<UploadProgressChangedEventArgs>
    {
        public ObservableCollection<string> Errors { get; protected set; }

        private string _streetLine1;

        public string StreetLine1
        {
            get { return _streetLine1; }
            set
            {
                if (_streetLine1 != value)
                {
                    _streetLine1 = value;
                    if (!string.IsNullOrEmpty(StreetLine1Error))
                    {
                        StreetLine1Error = null;
                        NotifyOfPropertyChange(() => StreetLine1Error);
                    }
                }
            }
        }

        public string StreetLine1Error { get; set; }

        private string _streetLine2;

        public string StreetLine2
        {
            get { return _streetLine2; }
            set
            {
                if (_streetLine2 != value)
                {
                    _streetLine2 = value;
                    if (!string.IsNullOrEmpty(StreetLine2Error))
                    {
                        StreetLine2Error = null;
                        NotifyOfPropertyChange(() => StreetLine2Error);
                    }
                }
            }
        }

        public string StreetLine2Error { get; set; }

        private string _postCode;

        public string PostCode
        {
            get { return _postCode; }
            set
            {
                if (_postCode != value)
                {
                    _postCode = value;
                    if (!Utils.Passport.IsValidPostCode(_postCode))
                    {
                        PostCodeError = AppResources.PassportUseLatinOnly;
                        NotifyOfPropertyChange(() => PostCodeError);
                    }
                    else if (!string.IsNullOrEmpty(PostCodeError))
                    {
                        PostCodeError = null;
                        NotifyOfPropertyChange(() => PostCodeError);
                    }
                }
            }
        }

        public string PostCodeError { get; set; }

        private string _city;

        public string City
        {
            get { return _city; }
            set
            {
                if (_city != value)
                {
                    _city = value;
                    if (!string.IsNullOrEmpty(CityError))
                    {
                        CityError = null;
                        NotifyOfPropertyChange(() => CityError);
                    }
                }
            }
        }

        public string CityError { get; set; }

        private string _state;

        public string State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    if (!string.IsNullOrEmpty(StateError))
                    {
                        StateError = null;
                        NotifyOfPropertyChange(() => StateError);
                    }
                }
            }
        }

        public string StateError { get; set; }

        private Country _selectedCountry;

        public Country SelectedCountry
        {
            get { return _selectedCountry; }
            set
            {
                if (_selectedCountry != value)
                {
                    SetField(ref _selectedCountry, value, () => SelectedCountry);
                    if (!string.IsNullOrEmpty(SelectedCountryError))
                    {
                        SelectedCountryError = null;
                        NotifyOfPropertyChange(() => SelectedCountryError);
                    }
                }
            }
        }

        public string SelectedCountryError { get; set; }

        public string FilesError { get; set; }

        public string TranslationsError { get; set; }

        public TLRPCError Error { get; set; }

        public string Caption
        {
            get
            {
                if (_secureType != null)
                {
                    return SecureRequiredTypeToCaptionConverter.Convert(_secureType);
                }
                if (_dataProofValue != null)
                {
                    return SecureRequiredTypeToCaptionConverter.Convert(_dataProofValue.Type);
                }
                if (_dataValue != null)
                {
                    return SecureRequiredTypeToCaptionConverter.Convert(_dataValue.Type);
                }
                if (_secureRequiredType != null)
                {
                    var type = _secureRequiredType.SelectedDataProofRequiredType;
                    if (type != null)
                    {
                        return SecureRequiredTypeToCaptionConverter.Convert(type.Type);
                    }
                }
                //if (_authorizationForm != null)
                //{
                //    var type = _authorizationForm.RequiredTypes.FirstOrDefault(IsValidProofType);
                //    if (type != null)
                //    {
                //        return SecureRequiredTypeToCaptionConverter.Convert(type);
                //    }
                //}

                return AppResources.PassportResidentialAddress;
            }
        }

        public string AttachDocumentHint
        {
            get
            {
                TLSecureValueTypeBase type = null;
                if (_secureType != null)
                {
                    type = _secureType;
                }
                else if (_dataProofValue != null)
                {
                    type = _dataProofValue.Type;
                }
                else if (_secureRequiredType != null && _secureRequiredType.SelectedDataProofRequiredType != null)
                {
                    type = _secureRequiredType.SelectedDataProofRequiredType.Type;
                }
                //else if (_authorizationForm != null)
                //{
                //    type = _authorizationForm.RequiredTypes.FirstOrDefault(IsValidProofType);
                //}

                if (type != null)
                {
                    if (type is TLSecureValueTypeUtilityBill)
                    {
                        return AppResources.PassportAddBillInfo;
                    }
                    if (type is TLSecureValueTypeBankStatement)
                    {
                        return AppResources.PassportAddBankInfo;
                    }
                    if (type is TLSecureValueTypeRentalAgreement)
                    {
                        return AppResources.PassportAddAgreementInfo;
                    }
                    if (type is TLSecureValueTypePassportRegistration)
                    {
                        return AppResources.PassportAddPassportRegistrationInfo;
                    }
                    if (type is TLSecureValueTypeTemporaryRegistration)
                    {
                        return AppResources.PassportAddTemporaryRegistrationInfo;
                    }
                }

                return null;
            }
        }

        public string AttachDocumentCommand
        {
            get
            {
                if (Files.Count > 0)
                {
                    return AppResources.PassportUploadAdditinalDocument;
                }

                return AppResources.PassportUploadDocument;
            }
        }

        public string DeleteCommand
        {
            get
            {
                if (_dataValue != null && _dataProofValue == null)
                {
                    return AppResources.PassportDelete;
                }

                return AppResources.PassportDeleteDocument;
            }
        }

        public ObservableCollection<TLSecureFileBase> Files { get; set; }

        public string AttachTranslationHint
        {
            get
            {
                TLSecureValueTypeBase type = null;
                if (_secureType != null)
                {
                    type = _secureType;
                }
                else if (_dataProofValue != null)
                {
                    type = _dataProofValue.Type;
                }
                else if (_secureRequiredType != null && _secureRequiredType.SelectedDataProofRequiredType != null)
                {
                    type = _secureRequiredType.SelectedDataProofRequiredType.Type;
                }
                //else if (_authorizationForm != null)
                //{
                //    type = _authorizationForm.RequiredTypes.FirstOrDefault(IsValidProofType);
                //}

                if (type != null)
                {
                    if (type is TLSecureValueTypeUtilityBill)
                    {
                        return AppResources.PassportAddTranslationBillInfo;
                    }
                    if (type is TLSecureValueTypeBankStatement)
                    {
                        return AppResources.PassportAddTranslationBankInfo;
                    }
                    if (type is TLSecureValueTypeRentalAgreement)
                    {
                        return AppResources.PassportAddTranslationAgreementInfo;
                    }
                    if (type is TLSecureValueTypePassportRegistration)
                    {
                        return AppResources.PassportAddTranslationPassportRegistrationInfo;
                    }
                    if (type is TLSecureValueTypeTemporaryRegistration)
                    {
                        return AppResources.PassportAddTranslationTemporaryRegistrationInfo;
                    }
                }

                return AppResources.PassportAddTranslationUploadInfo;
            }
        }

        public string AttachTranslationCommand
        {
            get
            {
                if (Translations.Count > 0)
                {
                    return AppResources.PassportUploadAdditinalDocument;
                }

                return AppResources.PassportUploadDocument;
            }
        }

        public ObservableCollection<TLSecureFileBase> Translations { get; set; }

        public bool IsFileUploading
        {
            get
            {
                foreach (var file in Files)
                {
                    if (file.UploadingProgress > 0.0 && file.UploadingProgress < 1.0)
                    {
                        return true;
                    }
                }

                foreach (var file in Translations)
                {
                    if (file.UploadingProgress > 0.0 && file.UploadingProgress < 1.0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsDataProofEnabled
        {
            get
            {
                if (_secureRequiredType != null && _secureRequiredType.SelectedDataProofRequiredType != null)
                {
                    return IsValidProofType(_secureRequiredType.SelectedDataProofRequiredType.Type);
                }
                //if (_authorizationForm != null)
                //{
                //    return _authorizationForm.RequiredTypes.Any(IsValidProofType);
                //}
                if (_secureType != null)
                {
                    return IsValidProofType(_secureType);
                }

                return _dataProofValue != null;
            }
        }

        public bool IsDataProofTranslationEnabled
        {
            get
            {
                if (_secureRequiredType != null && _secureRequiredType.SelectedDataProofRequiredType != null)
                {
                    return _secureRequiredType.SelectedDataProofRequiredType.TranslationRequired;
                }
                if (_secureType != null)
                {
                    return false;   // Important: default value from Passport Settings page
                }
                if (_dataProofValue != null && _secureRequiredType == null)
                {
                    return false;   // Important: default value from Passport Settings page
                }
                if (_dataProofValue != null)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsDataEnabled
        {
            get
            {
                if (_secureRequiredType != null)
                {
                    return _secureRequiredType.DataRequiredType != null;
                }
                //if (_authorizationForm != null)
                //{
                //    return _authorizationForm.RequiredTypes.Any(IsValidDataType);
                //}
                if (_secureType != null)
                {
                    return IsValidDataType(_secureType);
                }

                return _dataValue != null;
            }
        }

        public bool IsDeleteEnabled
        {
            get { return _dataValue != null || _dataProofValue != null; }
        }

        public static bool IsValidType(TLSecureValueTypeBase type)
        {
            return IsValidDataType(type)
                || IsValidProofType(type);
        }

        public static bool IsValidDataType(TLSecureValueTypeBase type)
        {
            return type is TLSecureValueTypeAddress;
        }

        public static bool IsValidProofType(TLSecureValueTypeBase type)
        {
            return type is TLSecureValueTypeUtilityBill
                || type is TLSecureValueTypeBankStatement
                || type is TLSecureValueTypeRentalAgreement
                || type is TLSecureValueTypePassportRegistration
                || type is TLSecureValueTypeTemporaryRegistration;
        }

        private readonly TLAuthorizationForm _authorizationForm;

        private readonly TLPasswordBase _passwordBase;

        private readonly TLSecureValue _dataValue;

        private readonly TLSecureValue _dataProofValue;

        private readonly TLSecureValueTypeBase _secureType;

        private readonly SecureRequiredType _secureRequiredType;

        public ResidentialAddressViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Errors = new ObservableCollection<string>();

            Translations = new ObservableCollection<TLSecureFileBase>();
            Translations.CollectionChanged += (sender, args) =>
            {
                NotifyOfPropertyChange(() => AttachTranslationCommand);
            };

            Files = new ObservableCollection<TLSecureFileBase>();
            Files.CollectionChanged += (sender, args) =>
            {
                NotifyOfPropertyChange(() => AttachDocumentCommand);
            };

            _authorizationForm = stateService.AuthorizationForm;
            stateService.AuthorizationForm = null;

            _passwordBase = stateService.Password;
            stateService.Password = null;

            var secureValue = StateService.SecureValue;
            StateService.SecureValue = null;

            _secureType = StateService.SecureType;
            StateService.SecureType = null;

            _secureRequiredType = StateService.SecureRequiredType;
            StateService.SecureRequiredType = null;

            _dataValue = GetData(_secureRequiredType, secureValue);
            if (_dataValue != null && _dataValue.Data != null)
            {
                var rootObject = _dataValue.Data.DecryptedData as ResidentialAddressRootObject;
                if (rootObject != null)
                {
                    _streetLine1 = rootObject.street_line1;
                    _streetLine2 = rootObject.street_line2;
                    _postCode = rootObject.post_code;
                    _city = rootObject.city;
                    _state = rootObject.state;
                    if (!string.IsNullOrEmpty(rootObject.country_code))
                    {
                        var country = CountryUtils.CountriesSource.FirstOrDefault(x => string.Equals(rootObject.country_code, x.Code, StringComparison.OrdinalIgnoreCase));
                        _selectedCountry = country;
                    }
                }
            }

            _dataProofValue = GetDataProof(_secureRequiredType, secureValue);
            if (_dataProofValue != null)
            {
                foreach (var file in _dataProofValue.Files)
                {
                    Files.Add(file);
                }
            } 
            var dataProofValue85 = _dataProofValue as TLSecureValue85;
            if (dataProofValue85 != null && dataProofValue85.Translation != null)
            {
                Translations.Clear();
                foreach (var translation in dataProofValue85.Translation)
                {
                    Translations.Add(translation);
                }
            }


            GetErrors(_authorizationForm);
        }

        private bool HasErrors()
        {
            if (IsDataProofEnabled)
            {
                if (Files != null && Files.Count > 0)
                {
                    foreach (var file in Files.OfType<ISecureFileError>())
                    {
                        if (!string.IsNullOrEmpty(file.Error))
                        {
                            Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.FILE_ERROR.ToString()) };
                            NotifyOfPropertyChange(() => Error);
                            return true;
                        }
                    }
                }
                else
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.FILES_EMPTY.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(FilesError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.FILES_EMPTY.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (IsDataProofTranslationEnabled)
                {
                    if (Translations != null && Translations.Count > 0)
                    {
                        foreach (var file in Translations.OfType<ISecureFileError>())
                        {
                            if (!string.IsNullOrEmpty(file.Error))
                            {
                                Error = new TLRPCError((int)ErrorCode.BAD_REQUEST)
                                {
                                    Message = new TLString(ErrorType.TRANSLATION_ERROR.ToString())
                                };
                                NotifyOfPropertyChange(() => Error);
                                return true;
                            }
                        }
                    }
                    else
                    {
                        Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.TRANSLATION_EMPTY.ToString()) };
                        NotifyOfPropertyChange(() => Error);
                        return true;
                    }

                    if (!string.IsNullOrEmpty(TranslationsError))
                    {
                        Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.TRANSLATION_EMPTY.ToString()) };
                        NotifyOfPropertyChange(() => Error);
                        return true;
                    }
                }
            }

            if (IsDataEnabled)
            {
                if (string.IsNullOrEmpty(StreetLine1) || !string.IsNullOrEmpty(StreetLine1Error))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.ADDRESS_STREET_LINE1_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(StreetLine2Error))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.ADDRESS_STREET_LINE2_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (string.IsNullOrEmpty(PostCode) || !string.IsNullOrEmpty(PostCodeError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.ADDRESS_POSTCODE_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (string.IsNullOrEmpty(City) || !string.IsNullOrEmpty(CityError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.ADDRESS_CITY_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(StateError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.ADDRESS_STATE_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (_selectedCountry == null || !string.IsNullOrEmpty(SelectedCountryError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.ADDRESS_COUNTRY_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }
            }

            Error = null;
            NotifyOfPropertyChange(() => Error);
            return false;
        }

        private void GetErrors(TLAuthorizationForm form)
        {
            if (form == null) return;
            if (form.Errors == null || form.Errors.Count == 0) return;

            var dataFieldAction = new Dictionary<string, Action<TLSecureValueErrorBase>>
            {
                {"street_line1", error => { StreetLine1Error = error.Text.ToString(); }},
                {"street_line2", error => { StreetLine2Error = error.Text.ToString(); }},
                {"post_code", error => { PostCodeError = error.Text.ToString(); }},
                {"state", error => { StateError = error.Text.ToString(); }},
                {"city", error => { CityError = error.Text.ToString(); }},
                {"country_code", error => { SelectedCountryError = error.Text.ToString(); }}
            };

            var errorTypeAction = new Dictionary<Type, Action<TLSecureValueErrorBase>>
            {
                {
                    typeof(TLSecureValueError), errorBase =>
                    {
                        var error = errorBase as TLSecureValueError;
                        if (error != null)
                        {
                            if (_secureRequiredType != null)
                            {
                                if (_secureRequiredType.DataRequiredType != null
                                    && _secureRequiredType.DataRequiredType.Type.GetType() == error.Type.GetType())
                                {
                                    Errors.Add(error.Text.ToString());
                                }
                                else if (_secureRequiredType.SelectedDataProofRequiredType != null
                                    && _secureRequiredType.SelectedDataProofRequiredType.Type.GetType() == error.Type.GetType())
                                {
                                    Errors.Add(error.Text.ToString());
                                }
                            }
                        }
                    }
                },
                {
                    typeof(TLSecureValueErrorData), errorBase =>
                    {
                        var error = errorBase as TLSecureValueErrorData;
                        if (error != null)
                        {
                            Action<TLSecureValueErrorBase> fieldAction;
                            if (dataFieldAction.TryGetValue(error.Field.ToString(), out fieldAction))
                            {
                                fieldAction.SafeInvoke(error);
                            }
                        }
                    }
                },
                {
                    typeof(TLSecureValueErrorFile), errorBase =>
                    {
                        var error = errorBase as TLSecureValueErrorFile;
                        if (error != null)
                        {
                            foreach (var file in Files.OfType<ISecureFileError>())
                            {
                                if (TLString.Equals(file.FileHash, error.FileHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    file.Error = error.Text.ToString();
                                    break;
                                }
                            }
                        }
                    }
                },
                {
                    typeof(TLSecureValueErrorFiles), errorBase =>
                    {
                        var error = errorBase as TLSecureValueErrorFiles;
                        if (error != null)
                        {
                            FilesError = error.Text.ToString();
                        }
                    }
                },
                {
                    typeof(TLSecureValueErrorTranslationFile), errorBase =>
                    {
                        var error = errorBase as TLSecureValueErrorTranslationFile;
                        if (error != null)
                        {
                            foreach (var file in Translations.OfType<ISecureFileError>())
                            {
                                if (TLString.Equals(file.FileHash, error.FileHash, StringComparison.OrdinalIgnoreCase))
                                {
                                    file.Error = error.Text.ToString();
                                    break;
                                }
                            }
                        }
                    }
                },
                {
                    typeof(TLSecureValueErrorTranslationFiles), errorBase =>
                    {
                        var error = errorBase as TLSecureValueErrorTranslationFiles;
                        if (error != null)
                        {
                            TranslationsError = error.Text.ToString();
                        }
                    }
                }
            };

            Errors.Clear();
            foreach (var error in _authorizationForm.Errors)
            {
                Action<TLSecureValueErrorBase> action;
                if ((_dataValue != null && _dataValue.Type.GetType() == error.Type.GetType()
                    || _dataProofValue != null && _dataProofValue.Type.GetType() == error.Type.GetType())
                    && errorTypeAction.TryGetValue(error.GetType(), out action))
                {
                    action.SafeInvoke(error);
                }
            }
        }

        private void RemoveDataErrors()
        {
            if (_authorizationForm != null && _authorizationForm.Errors != null)
            {
                for (var i = 0; i < _authorizationForm.Errors.Count; i++)
                {
                    var error = _authorizationForm.Errors[i];
                    if (error != null && IsValidDataType(error.Type))
                    {
                        _authorizationForm.Errors.RemoveAt(i--);
                    }
                }
            }
        }

        private void RemoveProofErrors()
        {
            if (_authorizationForm != null && _authorizationForm.Errors != null)
            {
                for (var i = 0; i < _authorizationForm.Errors.Count; i++)
                {
                    var error = _authorizationForm.Errors[i];
                    if (error != null && IsValidProofType(error.Type))
                    {
                        _authorizationForm.Errors.RemoveAt(i--);
                    }
                }
            }
        }

        private TLSecureValue GetData(SecureRequiredType requiredType, TLSecureValue secureValue)
        {
            if (requiredType != null)
            {
                return requiredType.DataValue;
            }

            return secureValue != null && IsValidDataType(secureValue.Type) ? secureValue : null;
        }

        private TLSecureValue GetDataProof(SecureRequiredType requiredType, TLSecureValue secureValue)
        {
            if (requiredType != null)
            {
                return requiredType.DataProofValue;
            }

            return secureValue != null && IsValidProofType(secureValue.Type) ? secureValue : null;
        }

        public void AttachTranslation()
        {
            if (Translations.Count >= Constants.MaxPassportFilesCount)
            {
                ShellViewModel.ShowCustomMessageBox(string.Format(AppResources.PassportUploadMaxReached, Constants.MaxPassportFilesCount), AppResources.AppName,
                    AppResources.Ok.ToLowerInvariant(), null,
                    dismissed =>
                    {

                    });

                return;
            }

            AttachDocument("Translation");
        }

        public void AttachDocument()
        {
            if (Files.Count >= Constants.MaxPassportFilesCount)
            {
                ShellViewModel.ShowCustomMessageBox(string.Format(AppResources.PassportUploadMaxReached, Constants.MaxPassportFilesCount), AppResources.AppName,
                    AppResources.Ok.ToLowerInvariant(), null,
                    dismissed =>
                    {

                    });

                return;
            }

            AttachDocument("Document");
        }

        public async void AttachDocument(string type)
        {

            ((App)Application.Current).ChooseFileInfo = new ChooseFileInfo(DateTime.Now);
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.ContinuationData.Add("From", "ResidentialAddressView");
            picker.ContinuationData.Add("Type", type);

            if (Environment.OSVersion.Version.Major >= 10)
            {
                var result = await picker.PickSingleFileAsync();
                if (result != null)
                {
                    Execute.BeginOnThreadPool(() =>
                    {
                        SendDocument(type, result);
                    });
                }
            }
            else
            {
                picker.PickSingleFileAndContinue();
            }
        }

        public async void SendDocument(string type, StorageFile file)
        {
            if (file == null) return;

            var password = _passwordBase as TLPassword;
            if (password == null) return;

            var passwordSettings = password.Settings as TLPasswordSettings81;
            if (passwordSettings == null) return;

            var fileId = TLLong.Random();
            var compressedFileName = string.Format("secureid_compressed_{0}.dat", fileId);
            var fileName = string.Format("secureid_{0}.dat", fileId);
            var previewFileName = string.Format("secureid_preview_{0}.dat", fileId);
            var localFileName = string.Format("document{0}_{1}.dat", fileId, 0);

            var inputStream = await file.OpenReadAsync();

            await DialogDetailsViewModel.ResizeJpeg(inputStream, Constants.DefaultPassportImageSize, file.DisplayName, compressedFileName, 0.89);

            file = await ApplicationData.Current.LocalFolder.GetFileAsync(compressedFileName);

            var padding = await Utils.Passport.GenerateRandomPadding(file);
            var fileSecret = Utils.Passport.GenerateSecret(TLString.Empty);
            var fileHash = TLString.FromBigEndianData(await Utils.Passport.GetSha256(padding, file));

            var fileSecureSecret = Utils.Passport.EncryptValueSecret(
                fileSecret,
                EnterPasswordViewModel.Secret,
                fileHash);

            var encryptedFile = await Utils.Passport.EncryptFile(fileName, file, fileSecret, fileHash, padding);
            var stream = await file.OpenReadAsync();

            await DialogDetailsViewModel.ResizeJpeg(stream, 180, localFileName, previewFileName);

            int length;
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var fileStream = store.OpenFile(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    length = (int)fileStream.Length;
                }
            }

            var secureFileUploaded = new TLSecureFileUploaded
            {
                Id = fileId,
                Parts = null,
                MD5Checksum = TLString.Empty,
                Size = new TLInt(length),
                Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                FileHash = fileHash,
                Secret = fileSecureSecret
            };

            Execute.BeginOnUIThread(() =>
            {
                switch (type)
                {
                    case "Document":
                        Files.Add(secureFileUploaded);
                        FilesError = null;
                        NotifyOfPropertyChange(() => FilesError);
                        break;
                    case "Translation":
                        Translations.Add(secureFileUploaded);
                        TranslationsError = null;
                        NotifyOfPropertyChange(() => TranslationsError);
                        break;
                }

                NotifyOfPropertyChange(() => IsFileUploading);
                secureFileUploaded.UploadingProgress = 0.01;
                secureFileUploaded.UploadingSize = (int)(secureFileUploaded.Size.Value * secureFileUploaded.UploadingProgress);
                var documentFileManager = IoC.Get<IUploadFileManager>();
                documentFileManager.UploadFile(fileId, secureFileUploaded, encryptedFile);
            });
        }

        public void DeleteFile(TLSecureFileBase file)
        {
            if (file == null) return;

            ShellViewModel.ShowCustomMessageBox(AppResources.PassportDeleteScanAlert, AppResources.AppName,
                AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        Files.Remove(file);
                        Translations.Remove(file);
                    }
                });
        }

        public void Delete()
        {
            if (_dataValue == null && _dataProofValue == null)
            {
                NavigationService.GoBack();
                return;
            }

            if (_dataValue != null && _dataProofValue != null)
            {
                object content = null;
                var textBlock = new TextBlock { IsHitTestVisible = false };
                var checkBox = new CheckBox { IsChecked = true, IsHitTestVisible = false };
                if (_dataValue != null)
                {
                    textBlock.SetValue(TextBlock.FontSizeProperty, DependencyProperty.UnsetValue);

                    var text = AppResources.PassportDeleteAddressDetails;
                    textBlock.Margin = new Thickness(-18.0, 0.0, 12.0, 0.0);
                    textBlock.Text = text;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;

                    var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0.0, -2.0, 0.0, -20.0), Background = new SolidColorBrush(Colors.Transparent) };
                    panel.Tap += (sender, args) =>
                    {
                        checkBox.IsChecked = !checkBox.IsChecked;
                    };
                    panel.Children.Add(checkBox);
                    panel.Children.Add(textBlock);
                    content = panel;
                }
                ShellViewModel.ShowCustomMessageBox(AppResources.PassportDeleteDocumentAlert, AppResources.AppName,
                    AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                    dismissed =>
                    {
                        if (dismissed == CustomMessageBoxResult.RightButton)
                        {
                            var deleteData = _dataValue != null && checkBox.IsChecked == true;
                            var values = new TLVector<TLSecureValue> { _dataProofValue };
                            var types = new TLVector<TLSecureValueTypeBase> { _dataProofValue.Type };
                            if (deleteData)
                            {
                                values.Add(_dataValue);
                                types.Add(_dataValue.Type);
                            }
                            IsWorking = true;
                            MTProtoService.DeleteSecureValueAsync(
                                types,
                                result => BeginOnUIThread(() =>
                                {
                                    IsWorking = false;
                                    if (_authorizationForm != null)
                                    {
                                        foreach (var value in values)
                                        {
                                            _authorizationForm.Values.Remove(value);
                                        }
                                    }
                                    RemoveDataErrors();
                                    RemoveProofErrors();
                                    if (_secureRequiredType != null)
                                    {
                                        _secureRequiredType.UpdateValue();
                                    }
                                    EventAggregator.Publish(new DeleteSecureValueEventArgs { Values = values });
                                    NavigationService.GoBack();
                                }),
                                error => BeginOnUIThread(() =>
                                {
                                    IsWorking = false;
                                }));
                        }
                    },
                    content);

                return;
            }

            // delete data
            if (_dataValue != null)
            {
                ShellViewModel.ShowCustomMessageBox(AppResources.PassportDeleteAddressAlert, AppResources.AppName,
                AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        IsWorking = true;
                        MTProtoService.DeleteSecureValueAsync(
                            new TLVector<TLSecureValueTypeBase> { _dataValue.Type },
                            result => BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                if (_authorizationForm != null)
                                {
                                    _authorizationForm.Values.Remove(_dataValue);
                                }
                                RemoveDataErrors();
                                if (_secureRequiredType != null)
                                {
                                    _secureRequiredType.UpdateValue();
                                }
                                EventAggregator.Publish(new DeleteSecureValueEventArgs { Values = new List<TLSecureValue> { _dataValue } });
                                NavigationService.GoBack();
                            }),
                            error => BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                            }));
                    }
                });

                return;
            }

            // delete proof
            if (_dataProofValue != null)
            {
                ShellViewModel.ShowCustomMessageBox(AppResources.PassportDeleteDocumentAlert, AppResources.AppName,
                AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        IsWorking = true;
                        MTProtoService.DeleteSecureValueAsync(
                            new TLVector<TLSecureValueTypeBase> { _dataProofValue.Type },
                            result => BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                                if (_authorizationForm != null)
                                {
                                    _authorizationForm.Values.Remove(_dataProofValue);
                                }
                                RemoveProofErrors();
                                if (_secureRequiredType != null)
                                {
                                    _secureRequiredType.UpdateValue();
                                }
                                EventAggregator.Publish(new DeleteSecureValueEventArgs { Values = new List<TLSecureValue> { _dataProofValue } });
                                NavigationService.GoBack();
                            }),
                            error => BeginOnUIThread(() =>
                            {
                                IsWorking = false;
                            }));
                    }
                });

                return;
            }
        }

        public void Done()
        {
            if (IsFileUploading) return;
            if (IsWorking) return;

            SaveDataAsync(
                () => SaveProofAsync(
                    () => NavigationService.GoBack()));
        }

        private void SaveDataAsync(System.Action callback, System.Action faultCallback = null)
        {
            var password = _passwordBase as TLPassword;
            if (password == null)
            {
                faultCallback.SafeInvoke();
                return;
            }

            var passwordSettings = password.Settings as TLPasswordSettings83;
            if (passwordSettings == null)
            {
                faultCallback.SafeInvoke();
                return;
            }

            var dataValue = _dataValue;
            if (dataValue == null)
            {
                var secureType =
                    _secureRequiredType != null && _secureRequiredType.DataRequiredType != null
                        ? _secureRequiredType.DataRequiredType.Type
                        : null;

                    //_authorizationForm != null
                    //? _authorizationForm.RequiredTypes.FirstOrDefault(IsValidDataType)
                    //: null;

                // add new address from passport settings
                if (_secureType != null && IsValidDataType(_secureType))
                {
                    dataValue = new TLSecureValue85
                    {
                        Flags = new TLInt(0),
                        Type = _secureType
                    };
                }
                // add new address from authorization form
                else if (secureType != null)
                {
                    dataValue = new TLSecureValue85
                    {
                        Flags = new TLInt(0),
                        Type = secureType
                    };
                }
                else
                {
                    callback.SafeInvoke();
                    return; 
                }
            }

            if (HasErrors())
            {
                faultCallback.SafeInvoke();
                return;
            }

            var rootObject = new ResidentialAddressRootObject
            {
                street_line1 = StreetLine1,
                street_line2 = StreetLine2,
                post_code = PostCode,
                city = City,
                state = State,
                country_code = _selectedCountry.Code.ToUpperInvariant()
            };

            var data = JsonUtils.ToJSON(rootObject);
            if (data == null) return;

            var valueSecret = Utils.Passport.GenerateSecret(TLString.Empty);
            var newSecureValue = Utils.Passport.EncryptSecureValue(
                dataValue,
                new TLString(data),
                valueSecret,
                EnterPasswordViewModel.Secret);
            var secureSecretHash = passwordSettings.SecureSettings.SecureSecretId;

            IsWorking = true;
            MTProtoService.SaveSecureValueAsync(
                newSecureValue.ToInputSecureValue(), secureSecretHash,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    result.Data.DecryptedData = rootObject;
                    if (_authorizationForm != null)
                    {
                        _authorizationForm.Values.Remove(dataValue);
                        _authorizationForm.Values.Add(result);
                    }

                    RemoveDataErrors();

                    dataValue.Update(result);
                    dataValue.NotifyOfPropertyChange(() => dataValue.Self);

                    if (_secureType != null)
                    {
                        EventAggregator.Publish(new AddSecureValueEventArgs{ Values = new List<TLSecureValue> { dataValue }});
                    }
                    if (_secureRequiredType != null)
                    {
                        _secureRequiredType.UpdateValue();
                    }

                    callback.SafeInvoke();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    
                    if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                    {
                        ShellViewModel.ShowCustomMessageBox(
                           "account.saveSecureValue" + Environment.NewLine + error.Message,
                           AppResources.AppName,
                           AppResources.Ok);
                    }

                    faultCallback.SafeInvoke();
                }));
        }

        private void SaveProofAsync(System.Action callback, System.Action faultCallback = null)
        {
            var password = _passwordBase as TLPassword;
            if (password == null)
            {
                callback.SafeInvoke();
                return;
            }

            var passwordSettings = password.Settings as TLPasswordSettings83;
            if (passwordSettings == null)
            {
                callback.SafeInvoke();
                return;
            }

            var dataProofValue = _dataProofValue;
            if (dataProofValue == null)
            {
                var secureType =
                    _secureRequiredType != null && _secureRequiredType.SelectedDataProofRequiredType != null
                        ? _secureRequiredType.SelectedDataProofRequiredType.Type
                        : null;

                    //_authorizationForm != null ? 
                    //_authorizationForm.RequiredTypes.FirstOrDefault(IsValidProofType) 
                    //: null;

                if (_secureType != null && IsValidProofType(_secureType))
                {
                    dataProofValue = new TLSecureValue85
                    {
                        Flags = new TLInt(0),
                        Type = _secureType
                    };
                }
                else if (secureType != null)
                {
                    dataProofValue = new TLSecureValue85
                    {
                        Flags = new TLInt(0),
                        Type = secureType
                    };
                }
                else
                {
                    callback.SafeInvoke();
                    return;
                }
            }

            if (HasErrors())
            {
                faultCallback.SafeInvoke();
                return;
            }

            var files = new TLVector<TLInputSecureFileBase>();
            foreach (var file in Files)
            {
                files.Add(file.ToInputSecureFile());
            }

            var secureSecretId = passwordSettings.SecureSettings.SecureSecretId;
            var inputSecureValue = dataProofValue.ToInputSecureValue();
            inputSecureValue.Files = files;

            var inputSecureValue85 = inputSecureValue as TLInputSecureValue85;
            if (inputSecureValue85 != null)
            {
                if (Translations.Count > 0)
                {
                    var translation = new TLVector<TLInputSecureFileBase>();
                    foreach (var file in Translations)
                    {
                        translation.Add(file.ToInputSecureFile());
                    }
                    inputSecureValue85.Translation = translation;
                }
            }

            IsWorking = true;
            MTProtoService.SaveSecureValueAsync(
                inputSecureValue, secureSecretId,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    if (_authorizationForm != null)
                    {
                        _authorizationForm.Values.Remove(dataProofValue);
                        _authorizationForm.Values.Add(result);
                    }
                    RemoveProofErrors();

                    dataProofValue.Update(result);
                    dataProofValue.NotifyOfPropertyChange(() => dataProofValue.Self);

                    if (_secureType != null)
                    {
                        EventAggregator.Publish(new AddSecureValueEventArgs { Values = new List<TLSecureValue> { dataProofValue } });
                    }
                    if (_secureRequiredType != null)
                    {
                        _secureRequiredType.UpdateValue();
                    }

                    callback.SafeInvoke();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    if (error.TypeEquals(ErrorType.FILES_EMPTY))
                    {
                        Error = error;
                        NotifyOfPropertyChange(() => Error);
                    }
                    else if (error.TypeEquals(ErrorType.FILES_TOO_MUCH))
                    {
                        ShellViewModel.ShowCustomMessageBox(
                            string.Format(AppResources.PassportUploadMaxReached, Constants.MaxPassportFilesCount), 
                            AppResources.AppName,
                            AppResources.Ok.ToLowerInvariant());
                    }
                    else if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                    {
                        ShellViewModel.ShowCustomMessageBox(
                           "account.saveSecureValue" + Environment.NewLine + error.Message,
                           AppResources.AppName,
                           AppResources.Ok);
                    }

                    faultCallback.SafeInvoke();
                }));
        }

        public void OpenScan(TLSecureFileBase file)
        {
            return;
            StateService.CurrentPhotoMessage = new TLMessage{Status = MessageStatus.Confirmed};
            OpenImageViewer();
        }

        public void SelectCountry()
        {
            StateService.HideCountryCode = true;
            NavigationService.UriFor<ChooseCountryViewModel>().Navigate();
        }

        private void OnCountrySelected(Country country)
        {
            SelectedCountry = country;
        }

        public ImageViewerViewModel ImageViewer { get; set; }

        public void OpenImageViewer()
        {
            if (ImageViewer == null)
            {
                ImageViewer = new ImageViewerViewModel(StateService, null, false, true)
                {
                    //DialogDetails = this
                };
                NotifyOfPropertyChange(() => ImageViewer);
            }
            BeginOnUIThread(() => ImageViewer.OpenViewer());
        }

        protected override void OnActivate()
        {
            if (StateService.SelectedCountry != null)
            {
                var country = StateService.SelectedCountry;
                StateService.SelectedCountry = null;
                OnCountrySelected(country);
            }

            EventAggregator.Subscribe(this);
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            EventAggregator.Unsubscribe(this);
            base.OnDeactivate(close);
        }

        public void Handle(UploadableItem item)
        {
            var secureFileUploaded = item.Owner as TLSecureFileUploaded;
            if (secureFileUploaded != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    secureFileUploaded.UploadingProgress = 1.0;
                    secureFileUploaded.UploadingSize = (int)(secureFileUploaded.Size.Value * secureFileUploaded.UploadingProgress);
                    secureFileUploaded.Parts = new TLInt(item.Parts.Count);
                    NotifyOfPropertyChange(() => IsFileUploading);
                });
            }
        }

        public void Handle(UploadProgressChangedEventArgs args)
        {
            var secureFileUploaded = args.Item.Owner as TLSecureFileUploaded;
            if (secureFileUploaded != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    secureFileUploaded.UploadingProgress = args.Progress;
                    secureFileUploaded.UploadingSize = (int)(secureFileUploaded.Size.Value * secureFileUploaded.UploadingProgress);
                    NotifyOfPropertyChange(() => IsFileUploading);
                });
            }
        }
    }

    public class ResidentialAddressRootObject
    {
        public string street_line1 { get; set; }
        public string street_line2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country_code { get; set; }
        public string post_code { get; set; }

        public IEnumerable<string> Fields()
        {
            if (!string.IsNullOrEmpty(street_line1)) yield return street_line1;
            if (!string.IsNullOrEmpty(street_line2)) yield return street_line2;
            if (!string.IsNullOrEmpty(post_code)) yield return post_code;
            if (!string.IsNullOrEmpty(city)) yield return city;
            if (!string.IsNullOrEmpty(state)) yield return state;
            if (!string.IsNullOrEmpty(country_code)) yield return country_code;
        }

        public IEnumerable<string> Fields(Func<string, string> getCountryByCode)
        {
            if (!string.IsNullOrEmpty(street_line1)) yield return street_line1;
            if (!string.IsNullOrEmpty(street_line2)) yield return street_line2;
            if (!string.IsNullOrEmpty(post_code)) yield return post_code;
            if (!string.IsNullOrEmpty(city)) yield return city;
            if (!string.IsNullOrEmpty(state)) yield return state;
            if (!string.IsNullOrEmpty(country_code)) yield return getCountryByCode(country_code) ?? country_code;
        }

        public override string ToString()
        {
            return string.Join(", ", Fields());
        }

        public string ToString(Func<string, string> getCountryByCode)
        {
            return string.Join(", ", Fields(getCountryByCode));
        }
    }
}

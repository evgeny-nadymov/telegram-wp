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
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Windows.Data.Json;
using Windows.Storage;
using Windows.Storage.Pickers;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Primitives;
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
using TelegramClient.Views.Controls;
using TelegramClient.Views.Passport;

namespace TelegramClient.ViewModels.Passport
{
    public class PersonalDetailsViewModel : ViewModelBase, Telegram.Api.Aggregator.IHandle<UploadableItem>, Telegram.Api.Aggregator.IHandle<UploadProgressChangedEventArgs>
    {
        public ObservableCollection<string> Errors { get; protected set; }

        public string DataProofCaption
        {
            get
            {
                return _authorizationForm != null
                    ? AppResources.PassportRequiredDocuments
                    : AppResources.PassportDocuments;
            }
        }

        private string _firstNameNative;

        public string FirstNameNative
        {
            get { return _firstNameNative; }
            set
            {
                if (_firstNameNative != value)
                {
                    _firstNameNative = value;

                    if (!string.IsNullOrEmpty(FirstNameNativeError))
                    {
                        FirstNameNativeError = null;
                        NotifyOfPropertyChange(() => FirstNameNativeError);
                    }
                }
            }
        }

        public string FirstNameNativeHint
        {
            get { return _selectedResidenceCountry != null ? string.Format(AppResources.PassportNameCountry, _selectedResidenceCountry.Name) : null; }
        }

        public string FirstNameNativeError { get; set; }

        private string _middleNameNative;

        public string MiddleNameNative
        {
            get { return _middleNameNative; }
            set
            {
                if (_middleNameNative != value)
                {
                    _middleNameNative = value;

                    if (!string.IsNullOrEmpty(MiddleNameNativeError))
                    {
                        MiddleNameNativeError = null;
                        NotifyOfPropertyChange(() => MiddleNameNativeError);
                    }
                }
            }
        }

        public string MiddleNameNativeHint
        {
            get { return _selectedResidenceCountry != null ? string.Format(AppResources.PassportMidnameCountry, _selectedResidenceCountry.Name) : null; }
        }

        public string MiddleNameNativeError { get; set; }

        private string _lastNameNative;

        public string LastNameNative
        {
            get { return _lastNameNative; }
            set
            {
                if (_lastNameNative != value)
                {
                    _lastNameNative = value;

                    if (!string.IsNullOrEmpty(LastNameNativeError))
                    {
                        LastNameNativeError = null;
                        NotifyOfPropertyChange(() => LastNameNativeError);
                    }
                }
            }
        }

        public string LastNameNativeHint
        {
            get
            {
                return _selectedResidenceCountry != null ? string.Format(AppResources.PassportSurnameCountry, _selectedResidenceCountry.Name) : null;
            }
        }

        public string LastNameNativeError { get; set; }

        public string PassportNativeFooter
        {
            get { return _selectedResidenceCountry != null ? string.Format(AppResources.PassportNativeInfo, _selectedResidenceCountry.Name) : null; }
        }

        private string _firstName;

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (_firstName != value)
                {
                    _firstName = value;

                    if (!Utils.Passport.IsValidName(_firstName))
                    {
                        FirstNameError = AppResources.PassportUseLatinOnly;
                        NotifyOfPropertyChange(() => FirstNameError);
                    }
                    else if (!string.IsNullOrEmpty(FirstNameError))
                    {
                        FirstNameError = null;
                        NotifyOfPropertyChange(() => FirstNameError);
                    }
                }
            }
        }

        public string FirstNameHint
        {
            get { return IsDataNativeNamesEnabled ? AppResources.PassportNameLatin : AppResources.PassportName; }
        }

        public string FirstNameError { get; set; }

        private string _middleName;

        public string MiddleName
        {
            get { return _middleName; }
            set
            {
                if (_middleName != value)
                {
                    _middleName = value;
                    if (!Utils.Passport.IsValidName(_middleName))
                    {
                        MiddleNameError = AppResources.PassportUseLatinOnly;
                        NotifyOfPropertyChange(() => MiddleNameError);
                    }
                    else if (!string.IsNullOrEmpty(MiddleNameError))
                    {
                        MiddleNameError = null;
                        NotifyOfPropertyChange(() => MiddleNameError);
                    }
                }
            }
        }

        public string MiddleNameHint
        {
            get { return IsDataNativeNamesEnabled ? AppResources.PassportMidnameLatin : AppResources.PassportMidname; }
        }

        public string MiddleNameError { get; set; }

        private string _lastName;

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (_lastName != value)
                {
                    _lastName = value;
                    if (!Utils.Passport.IsValidName(_lastName))
                    {
                        LastNameError = AppResources.PassportUseLatinOnly;
                        NotifyOfPropertyChange(() => LastNameError);
                    }
                    else if (!string.IsNullOrEmpty(LastNameError))
                    {
                        LastNameError = null;
                        NotifyOfPropertyChange(() => LastNameError);
                    }
                }
            }
        }

        public string LastNameHint
        {
            get { return IsDataNativeNamesEnabled ? AppResources.PassportSurnameLatin : AppResources.PassportSurname; }
        }

        public string LastNameError { get; set; }

        private string _birthDate;

        public string BirthDate
        {
            get { return _birthDate; }
            set
            {
                if (_birthDate != value)
                {
                    _birthDate = value;
                    if (!string.IsNullOrEmpty(BirthDateError))
                    {
                        BirthDateError = null;
                        NotifyOfPropertyChange(() => BirthDateError);
                    }
                }
            }
        }

        public string BirthDateError { get; set; }

        private string _gender;

        public string Gender
        {
            get { return _gender; }
            set
            {
                if (_gender != value)
                {
                    _gender = value;
                    if (!string.IsNullOrEmpty(GenderError))
                    {
                        GenderError = null;
                        NotifyOfPropertyChange(() => GenderError);
                    }
                }
            }
        }

        public string GenderError { get; set; }

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

        private Country _selectedResidenceCountry;

        public Country SelectedResidenceCountry
        {
            get { return _selectedResidenceCountry; }
            set
            {
                if (_selectedResidenceCountry != value)
                {
                    SetField(ref _selectedResidenceCountry, value, () => SelectedResidenceCountry);

                    NotifyOfPropertyChange(() => IsDataNativeNamesVisible);
                    NotifyOfPropertyChange(() => FirstNameNativeHint);
                    NotifyOfPropertyChange(() => MiddleNameNativeHint);
                    NotifyOfPropertyChange(() => LastNameNativeHint);
                    NotifyOfPropertyChange(() => PassportNativeFooter);

                    if (!string.IsNullOrEmpty(SelectedResidenceCountryError))
                    {
                        SelectedResidenceCountryError = null;
                        NotifyOfPropertyChange(() => SelectedResidenceCountryError);
                    }
                }
            }
        }

        public string SelectedResidenceCountryError { get; set; }

        private string _documentNumber;

        public string DocumentNumber
        {
            get { return _documentNumber; }
            set
            {
                if (_documentNumber != value)
                {
                    _documentNumber = value;
                    if (!string.IsNullOrEmpty(DocumentNumberError))
                    {
                        DocumentNumberError = null;
                        NotifyOfPropertyChange(() => DocumentNumberError);
                    }
                }
            }
        }

        public string DocumentNumberError { get; set; }

        private string _expiryDate;

        public string ExpiryDate
        {
            get { return _expiryDate; }
            set
            {
                if (_expiryDate != value)
                {
                    _expiryDate = value;
                    if (!string.IsNullOrEmpty(ExpiryDateError))
                    {
                        ExpiryDateError = null;
                        NotifyOfPropertyChange(() => ExpiryDateError);
                    }
                }
            }
        }

        public string ExpiryDateError { get; set; }

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

        public TLSecureFileBase FrontSide { get; set; }

        public string FrontSideError { get; set; }

        public string FrontSideTitle
        {
            get
            {
                TLSecureValueTypeBase type = null;
                if (_secureType != null)
                {
                    type = _secureType;
                }
                if (_dataProofValue != null)
                {
                    type = _dataProofValue.Type;
                }

                if (type is TLSecureValueTypePassport
                    || type is TLSecureValueTypeInternalPassport)
                {
                    return AppResources.PassportMainPage;
                }

                return AppResources.PassportFrontSide;
            }
        }

        public string FrontSideSubtitle
        {
            get
            {
                TLSecureValueTypeBase type = null;
                if (_secureType != null)
                {
                    type = _secureType;
                }
                if (_dataProofValue != null)
                {
                    type = _dataProofValue.Type;
                }

                if (type is TLSecureValueTypePassport
                    || type is TLSecureValueTypeInternalPassport)
                {
                    return AppResources.PassportMainPageInfo;
                }

                return AppResources.PassportFrontSideInfo;
            }
        }

        public TLSecureFileBase ReverseSide { get; set; }

        public string ReverseSideError { get; set; }

        public TLSecureFileBase Selfie { get; set; }

        public string SelfieError { get; set; }

        public IEnumerable<TLSecureFileBase> GetFiles()
        {
            if (FrontSide != null) yield return FrontSide;
            if (ReverseSide != null) yield return ReverseSide;
            if (Selfie != null) yield return Selfie;

            foreach (var file in Translations)
            {
                yield return file;
            }
        }

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
                    if (type is TLSecureValueTypeDriverLicense)
                    {
                        return AppResources.PassportAddTranslationDriverLicenceInfo;
                    }
                    if (type is TLSecureValueTypeIdentityCard)
                    {
                        return AppResources.PassportAddTranslationIdentityCardInfo;
                    }
                    if (type is TLSecureValueTypeInternalPassport)
                    {
                        return AppResources.PassportAddTranslationInternalPassportInfo;
                    }
                    if (type is TLSecureValueTypePassport)
                    {
                        return AppResources.PassportAddTranslationPassportInfo;
                    }
                }

                return AppResources.PassportAddTranslationUploadInfo;
            }
        }

        public string AttachDocumentCommand
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
                foreach (var file in GetFiles())
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

        public bool IsDataProofReverseEnabled
        {
            get
            {
                if (_secureRequiredType != null && _secureRequiredType.SelectedDataProofRequiredType != null)
                {
                    return IsValidReversedProofType(_secureRequiredType.SelectedDataProofRequiredType.Type);
                }
                if (_dataProofValue != null)
                {
                    return IsValidReversedProofType(_dataProofValue.Type);
                }
                if (_secureType != null)
                {
                    return IsValidReversedProofType(_secureType);
                }

                return false;
            }
        }

        public bool IsDataProofSelfieEnabled
        {
            get
            {
                if (_secureRequiredType != null && _secureRequiredType.SelectedDataProofRequiredType != null)
                {
                    return _secureRequiredType.SelectedDataProofRequiredType.SelfieRequired;
                }
                if (_secureType != null)
                {
                    return true;
                }
                if (_dataProofValue != null)
                {
                    return true;
                }

                return false;
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
                    return true;
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

        public bool IsDataNativeNamesEnabled
        {
            get
            {
                if (_secureRequiredType != null && _secureRequiredType.DataRequiredType != null)
                {
                    return _secureRequiredType.DataRequiredType.NativeNames;
                }
                
                return false;
            }
        }

        public bool IsDataNativeNamesVisible
        {
            get
            {
                if (IsDataNativeNamesEnabled)
                {
                    if (!string.IsNullOrEmpty(FirstNameNativeError)) return true;
                    if (!string.IsNullOrEmpty(MiddleNameNativeError)) return true;
                    if (!string.IsNullOrEmpty(LastNameNativeError)) return true;

                    IJsonValue langCode;
                    if (_secureRequiredType != null
                        && _secureRequiredType.AuthorizationForm != null
                        && _secureRequiredType.AuthorizationForm.Config != null
                        && _secureRequiredType.AuthorizationForm.Config.CountriesLangsObject != null
                        && _selectedResidenceCountry != null
                        && _secureRequiredType.AuthorizationForm.Config.CountriesLangsObject.TryGetValue(_selectedResidenceCountry.Code.ToUpperInvariant(), out langCode)
                        && string.Equals(langCode.GetString(), "en", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    return true;
                }

                return false;
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
            return type is TLSecureValueTypePersonalDetails;
        }

        public static bool IsValidProofType(TLSecureValueTypeBase type)
        {
            return type is TLSecureValueTypePassport
                || type is TLSecureValueTypeInternalPassport
                || type is TLSecureValueTypeDriverLicense
                || type is TLSecureValueTypeIdentityCard;
        }

        public static bool IsValidReversedProofType(TLSecureValueTypeBase type)
        {
            return type is TLSecureValueTypeDriverLicense
                || type is TLSecureValueTypeIdentityCard;
        }

        private readonly TLAuthorizationForm _authorizationForm;

        private readonly TLPasswordBase _passwordBase;

        private readonly TLSecureValue _dataValue;

        private readonly TLSecureValue _dataProofValue;

        private readonly TLSecureValueTypeBase _secureType;

        private readonly SecureRequiredType _secureRequiredType;

        public PersonalDetailsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler,
            IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService,
            ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Errors = new ObservableCollection<string>();

            Translations = new ObservableCollection<TLSecureFileBase>();
            Translations.CollectionChanged += (sender, args) =>
            {
                NotifyOfPropertyChange(() => AttachDocumentCommand);
            };

            _authorizationForm = StateService.AuthorizationForm;
            StateService.AuthorizationForm = null;

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
                var rootObject = _dataValue.Data.DecryptedData as PersonalDetailsRootObject;
                if (rootObject != null)
                {
                    _firstName = rootObject.first_name;
                    _middleName = rootObject.middle_name;
                    _lastName = rootObject.last_name;
                    _birthDate = rootObject.birth_date;
                    _gender = rootObject.gender;
                    if (!string.IsNullOrEmpty(rootObject.country_code))
                    {
                        var country =
                            CountryUtils.CountriesSource.FirstOrDefault(
                                x => string.Equals(rootObject.country_code, x.Code, StringComparison.OrdinalIgnoreCase));
                        _selectedCountry = country;
                    }
                    if (!string.IsNullOrEmpty(rootObject.residence_country_code))
                    {
                        var country =
                            CountryUtils.CountriesSource.FirstOrDefault(
                                x =>
                                    string.Equals(rootObject.residence_country_code, x.Code,
                                        StringComparison.OrdinalIgnoreCase));
                        _selectedResidenceCountry = country;
                    }

                    _firstNameNative = rootObject.first_name_native;
                    _middleNameNative = rootObject.middle_name_native;
                    _lastNameNative = rootObject.last_name_native;
                }
            }

            _dataProofValue = GetDataProof(_secureRequiredType, secureValue);
            if (_dataProofValue != null)
            {
                FrontSide = _dataProofValue.FrontSide;
                ReverseSide = _dataProofValue.ReverseSide;
                Selfie = _dataProofValue.Selfie;

                var dataProofValue85 = _dataProofValue as TLSecureValue85;
                if (dataProofValue85 != null && dataProofValue85.Translation != null)
                {
                    Translations.Clear();
                    foreach (var translation in dataProofValue85.Translation)
                    {
                        Translations.Add(translation);
                    }
                }

                var rootObject = _dataProofValue.Data.DecryptedData as PersonalDetailsDocumentRootObject;
                if (rootObject != null)
                {
                    _documentNumber = rootObject.document_no;
                    _expiryDate = rootObject.expiry_date;
                }
            }

            GetErrors(_authorizationForm);
        }

        private bool HasErrors()
        {
            if (IsDataProofEnabled)
            {
                var frontSideError = FrontSide as ISecureFileError;
                if (frontSideError != null && !string.IsNullOrEmpty(frontSideError.Error))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.FRONT_SIDE_REQUIRED.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (FrontSide == null)
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.FRONT_SIDE_REQUIRED.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (IsDataProofReverseEnabled)
                {
                    var reverseSideError = ReverseSide as ISecureFileError;
                    if (reverseSideError != null && !string.IsNullOrEmpty(reverseSideError.Error))
                    {
                        Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.REVERSE_SIDE_REQUIRED.ToString()) };
                        NotifyOfPropertyChange(() => Error);
                        return true;
                    }

                    if (ReverseSide == null)
                    {
                        Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.REVERSE_SIDE_REQUIRED.ToString()) };
                        NotifyOfPropertyChange(() => Error);
                        return true;
                    }
                }

                if (IsDataProofSelfieEnabled)
                {
                    var selfieSideError = Selfie as ISecureFileError;
                    if (selfieSideError != null && !string.IsNullOrEmpty(selfieSideError.Error))
                    {
                        Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.SELFIE_REQUIRED.ToString()) };
                        NotifyOfPropertyChange(() => Error);
                        return true;
                    }

                    if (Selfie == null)
                    {
                        Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.SELFIE_REQUIRED.ToString()) };
                        NotifyOfPropertyChange(() => Error);
                        return true;
                    }
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
                if (!string.IsNullOrEmpty(FirstNameError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.FIRSTNAME_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (string.IsNullOrEmpty(FirstName))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.FIRSTNAME_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }


                if (!string.IsNullOrEmpty(MiddleNameError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.MIDDLENAME_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(LastNameError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.LASTNAME_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (string.IsNullOrEmpty(LastName))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.LASTNAME_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(BirthDateError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.BIRTHDATE_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (string.IsNullOrEmpty(BirthDate))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.BIRTHDATE_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(GenderError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.GENDER_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (string.IsNullOrEmpty(Gender))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.GENDER_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(SelectedCountryError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.ADDRESS_COUNTRY_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (_selectedCountry == null)
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.ADDRESS_COUNTRY_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(SelectedResidenceCountryError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.ADDRESS_RESIDENCE_COUNTRY_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (_selectedResidenceCountry == null)
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.ADDRESS_RESIDENCE_COUNTRY_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }
            }

            if (IsDataProofEnabled)
            {
                if (!string.IsNullOrEmpty(DocumentNumberError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.DOCUMENT_NUMBER_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (string.IsNullOrEmpty(DocumentNumber))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.DOCUMENT_NUMBER_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(ExpiryDateError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.EXPIRYDATE_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }
            }

            if (IsDataNativeNamesVisible)
            {
                if (!string.IsNullOrEmpty(FirstNameNativeError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.FIRSTNAMENATIVE_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (string.IsNullOrEmpty(FirstNameNative))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.FIRSTNAMENATIVE_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(MiddleNameNativeError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.MIDDLENAMENATIVE_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (!string.IsNullOrEmpty(LastNameNativeError))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.LASTNAMENATIVE_INVALID.ToString()) };
                    NotifyOfPropertyChange(() => Error);
                    return true;
                }

                if (string.IsNullOrEmpty(LastNameNative))
                {
                    Error = new TLRPCError((int)ErrorCode.BAD_REQUEST) { Message = new TLString(ErrorType.LASTNAMENATIVE_INVALID.ToString()) };
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
                {"first_name", error => { FirstNameError = error.Text.ToString(); }},
                {"middle_name", error => { MiddleNameError = error.Text.ToString(); }},
                {"last_name", error => { LastNameError = error.Text.ToString(); }},
                {"birth_date", error => { BirthDateError = error.Text.ToString(); }},
                {"gender", error => { GenderError = error.Text.ToString(); }},
                {"country_code", error => { SelectedCountryError = error.Text.ToString(); }},
                {"residence_country_code", error => { SelectedResidenceCountryError = error.Text.ToString(); }},
                {"document_no", error => { DocumentNumberError = error.Text.ToString(); }},
                {"expiry_date", error => { ExpiryDateError = error.Text.ToString(); }},
                {"first_name_native", error => { FirstNameNativeError = error.Text.ToString(); }},
                {"middle_name_native", error => { MiddleNameNativeError = error.Text.ToString(); }},
                {"last_name_native", error => { LastNameNativeError = error.Text.ToString(); }},
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
                            var frontSideError = FrontSide as ISecureFileError;
                            if (frontSideError != null && TLString.Equals(frontSideError.FileHash, error.FileHash, StringComparison.OrdinalIgnoreCase))
                            {
                                frontSideError.Error = error.Text.ToString(); 
                            }
                            var reverseSideError = ReverseSide as ISecureFileError;
                            if (reverseSideError != null && TLString.Equals(reverseSideError.FileHash, error.FileHash, StringComparison.OrdinalIgnoreCase))
                            {
                                reverseSideError.Error = error.Text.ToString();
                            }
                            var selfieError = Selfie as ISecureFileError;
                            if (selfieError != null && TLString.Equals(selfieError.FileHash, error.FileHash, StringComparison.OrdinalIgnoreCase))
                            {
                                selfieError.Error = error.Text.ToString();
                            }
                        }
                    }
                },
                {
                    typeof(TLSecureValueErrorFrontSide), errorBase =>
                    {
                        var error = errorBase as TLSecureValueErrorFrontSide;
                        if (error != null)
                        {
                            var frontSideError = FrontSide as ISecureFileError;
                            if (frontSideError != null && TLString.Equals(frontSideError.FileHash, error.FileHash, StringComparison.OrdinalIgnoreCase))
                            {
                                frontSideError.Error = error.Text.ToString(); 
                            }
                        }
                    }
                },
                {
                    typeof(TLSecureValueErrorReverseSide), errorBase =>
                    {
                        var error = errorBase as TLSecureValueErrorReverseSide;
                        if (error != null)
                        {
                            var reverseSideError = ReverseSide as ISecureFileError;
                            if (reverseSideError != null && TLString.Equals(reverseSideError.FileHash, error.FileHash, StringComparison.OrdinalIgnoreCase))
                            {
                                reverseSideError.Error = error.Text.ToString(); 
                            }
                        }
                    }
                },
                {
                    typeof(TLSecureValueErrorSelfie), errorBase =>
                    {
                        var error = errorBase as TLSecureValueErrorSelfie;
                        if (error != null)
                        {
                            var selfieError = Selfie as ISecureFileError;
                            if (selfieError != null && TLString.Equals(selfieError.FileHash, error.FileHash, StringComparison.OrdinalIgnoreCase))
                            {
                                selfieError.Error = error.Text.ToString(); 
                            }
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

        public void AttachFrontSide()
        {
            AttachDocument("FrontSide");
        }

        public void AttachReverseSide()
        {
            AttachDocument("ReverseSide");
        }

        public void AttachSelfie()
        {
            AttachDocument("Selfie");
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

        public async void AttachDocument(string type)
        {
            ((App)Application.Current).ChooseFileInfo = new ChooseFileInfo(DateTime.Now);
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.ContinuationData.Add("From", "PersonalDetailsView");
            picker.ContinuationData.Add("Type", type);

            if (Environment.OSVersion.Version.Major >= 10)
            {
                var result = await picker.PickSingleFileAsync();
                if (result != null)
                {
                    Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
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
            if (string.IsNullOrEmpty(type)) return;
            if (file == null) return;

            var password = _passwordBase as TLPassword;
            if (password == null) return;

            var passwordSettings = password.Settings as TLPasswordSettings83;
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

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                switch (type)
                {
                    case "FrontSide":
                        FrontSide = secureFileUploaded;
                        NotifyOfPropertyChange(() => FrontSide);
                        break;
                    case "ReverseSide":
                        ReverseSide = secureFileUploaded;
                        NotifyOfPropertyChange(() => ReverseSide);
                        break;
                    case "Selfie":
                        Selfie = secureFileUploaded;
                        NotifyOfPropertyChange(() => Selfie);
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

        public void DeleteFrontSide()
        {
            DeleteFile(FrontSide);
        }

        public void DeleteReverseSide()
        {
            DeleteFile(ReverseSide);
        }

        public void DeleteSelfie()
        {
            DeleteFile(Selfie);
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
                        if (FrontSide == file)
                        {
                            FrontSide = null;
                            NotifyOfPropertyChange(() => FrontSide);
                        }
                        else if (ReverseSide == file)
                        {
                            ReverseSide = null;
                            NotifyOfPropertyChange(() => ReverseSide);
                        }
                        else if (Selfie == file)
                        {
                            Selfie = null;
                            NotifyOfPropertyChange(() => Selfie);
                        }
                        else
                        {
                            Translations.Remove(file);
                        }
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

                    var text = AppResources.PassportDeleteDocumentPersonal;
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
                                if (_secureRequiredType != null)
                                {
                                    _secureRequiredType.UpdateValue();
                                }
                                RemoveDataErrors();
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
                                RemoveProofErrors();
                                if (_authorizationForm != null)
                                {
                                    _authorizationForm.Values.Remove(_dataProofValue);
                                }
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

        private bool UseSameNativeNames()
        {
            IJsonValue langCode;
            if (_secureRequiredType != null
                && _secureRequiredType.AuthorizationForm != null
                && _secureRequiredType.AuthorizationForm.Config != null
                && _secureRequiredType.AuthorizationForm.Config.CountriesLangsObject != null
                && _selectedResidenceCountry != null
                && _secureRequiredType.AuthorizationForm.Config.CountriesLangsObject.TryGetValue(_selectedResidenceCountry.Code.ToUpperInvariant(), out langCode)
                && string.Equals(langCode.GetString(), "en", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
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

            var rootObject = new PersonalDetailsRootObject
            {
                first_name = FirstName,
                middle_name = MiddleName,
                last_name = LastName,
                birth_date = BirthDate,
                gender = Gender,
                country_code = _selectedCountry.Code.ToUpperInvariant(),
                residence_country_code = _selectedResidenceCountry.Code.ToUpperInvariant()
            };

            if (IsDataNativeNamesEnabled)
            {
                if (UseSameNativeNames())
                {
                    rootObject.first_name_native = FirstName;
                    rootObject.middle_name_native = MiddleName;
                    rootObject.last_name_native = LastName;
                }
                else
                {
                    rootObject.first_name_native = FirstNameNative;
                    rootObject.middle_name_native = MiddleNameNative;
                    rootObject.last_name_native = LastNameNative;
                }
            }

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
                        EventAggregator.Publish(new AddSecureValueEventArgs { Values = new List<TLSecureValue> { dataValue } });
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
            
            var rootObject = new PersonalDetailsDocumentRootObject
            {
                document_no = DocumentNumber,
                expiry_date = ExpiryDate
            };

            var data = JsonUtils.ToJSON(rootObject);
            if (data == null) return;

            var valueSecret = Utils.Passport.GenerateSecret(TLString.Empty);
            var newSecureValue = Utils.Passport.EncryptSecureValue(
                dataProofValue,
                new TLString(data),
                valueSecret,
                EnterPasswordViewModel.Secret);

            var inputSecureValue = newSecureValue.ToInputSecureValue();
            inputSecureValue.FrontSide = FrontSide != null ? FrontSide.ToInputSecureFile() : null;
            inputSecureValue.ReverseSide = ReverseSide != null ? ReverseSide.ToInputSecureFile() : null;
            inputSecureValue.Selfie = Selfie != null ? Selfie.ToInputSecureFile() : null;

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

            var secureSecretId = passwordSettings.SecureSettings.SecureSecretId;

            IsWorking = true;
            MTProtoService.SaveSecureValueAsync(
                inputSecureValue, secureSecretId,
                result => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    result.Data.DecryptedData = rootObject;
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
                    else if (error.TypeEquals(ErrorType.FRONT_SIDE_REQUIRED))
                    {
                        Error = error;
                        NotifyOfPropertyChange(() => Error);
                    }
                    else if (error.TypeEquals(ErrorType.REVERSE_SIDE_REQUIRED))
                    {
                        Error = error;
                        NotifyOfPropertyChange(() => Error);
                    }
                    else if (error.TypeEquals(ErrorType.SELFIE_REQUIRED))
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
            StateService.CurrentPhotoMessage = new TLMessage { Status = MessageStatus.Confirmed };
            OpenImageViewer();
        }

        public void SelectBirthDate()
        {
            DateTime currentDate;
            if (!DateTime.TryParse(BirthDate, out currentDate))
            {
                currentDate = DateTime.Now.Date;
            }

            var datePickerPage = new TelegramDatePickerPage(new YearDataSource{ MaxYear = DateTime.Now.Year, MinYear = 1900 }) { Height = 160.0, Value = currentDate, Margin = new Thickness(0.0, 18.0, 0.0, 6.0) };
            ShellViewModel.ShowCustomMessageBox(null, AppResources.PassportSelectDate.ToUpperInvariant(), AppResources.Ok.ToLowerInvariant(), null,
                result =>
                {
                    if (result == CustomMessageBoxResult.RightButton)
                    {
                        var selector = GetChildOfType<LoopingSelector>(datePickerPage);
                        if (selector != null)
                        {
                            var value = ((DateTimeWrapper)selector.DataSource.SelectedItem).DateTime;
                            value = value > DateTime.Now.Date ? DateTime.Now.Date : value;
                            BirthDate = value.ToString("dd.MM.yyyy");
                            NotifyOfPropertyChange(() => BirthDate);
                        }
                    }
                },
                datePickerPage);
        }

        public static T GetChildOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        public void SelectGender()
        {
            var types = new [] { "male", "female" };

            var panel = new StackPanel { Margin = new Thickness(0.0, 12.0, 0.0, 0.0) };
            var messageBox = ShellViewModel.ShowCustomMessageBox(
                null, AppResources.PassportSelectGender.ToUpperInvariant(),
                null, null,
                dismissed =>
                {

                },
                panel);

            for (var i = 0; i < types.Length; i++)
            {
                var listBoxItem = new ListBoxItem
                {
                    Content =
                        new TextBlock
                        {
                            Text = GenderToStringConverter.Convert(types[i]),
                            FontSize = 27,
                            Margin = new Thickness(12.0)
                        },
                    DataContext = types[i]
                };
                TiltEffect.SetIsTiltEnabled(listBoxItem, true);
                listBoxItem.Tap += (sender, args) =>
                {
                    messageBox.Dismiss();
                    var item = sender as ListBoxItem;
                    if (item != null)
                    {
                        var gender = item.DataContext as string;
                        if (gender != null)
                        {
                            Gender = gender;
                            NotifyOfPropertyChange(() => Gender);
                        }
                    }

                };

                panel.Children.Add(listBoxItem);
            }
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

        public void SelectResidenceCountry()
        {
            StateService.HideCountryCode = true;
            StateService.ResidenceCountry = true;
            NavigationService.UriFor<ChooseCountryViewModel>().Navigate();
        }

        private void OnResidenceCountrySelected(Country country)
        {
            SelectedResidenceCountry = country;
        }

        public void SelectExpiryDate()
        {
            DateTime currentDate;
            if (!DateTime.TryParse(ExpiryDate, out currentDate))
            {
                currentDate = DateTime.Now.Date;
            }

            var datePickerPage = new TelegramDatePickerPage(new YearDataSource { MaxYear = DateTime.Now.Year + 20, MinYear = 1900 }) { Height = 160.0, Value = currentDate, Margin = new Thickness(0.0, 18.0, 0.0, 6.0) };
            ShellViewModel.ShowCustomMessageBox(null, AppResources.PassportSelectDate.ToUpperInvariant(), AppResources.Ok.ToLowerInvariant(), AppResources.PassportSelectNotExpire,
                result =>
                {
                    if (result == CustomMessageBoxResult.RightButton)
                    {
                        var selector = GetChildOfType<LoopingSelector>(datePickerPage);
                        if (selector != null)
                        {
                            var value = ((DateTimeWrapper)selector.DataSource.SelectedItem).DateTime;
                            ExpiryDate = value.ToString("dd.MM.yyyy");
                            NotifyOfPropertyChange(() => ExpiryDate);
                        }
                    }
                    else if (result == CustomMessageBoxResult.LeftButton)
                    {
                        ExpiryDate = string.Empty;
                        NotifyOfPropertyChange(() => ExpiryDate);
                    }
                },
                datePickerPage);
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

            if (StateService.SelectedResidenceCountry != null)
            {
                var country = StateService.SelectedResidenceCountry;
                StateService.SelectedResidenceCountry = null;
                OnResidenceCountrySelected(country);
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

    public class PersonalDetailsDocumentRootObject
    {
        public string document_no { get; set; }
        public string expiry_date { get; set; }

        public IEnumerable<string> Fields()
        {
            if (!string.IsNullOrEmpty(document_no)) yield return document_no;
            if (!string.IsNullOrEmpty(expiry_date)) yield return expiry_date;
        }

        public override string ToString()
        {
            return string.Join(", ", Fields());
        }
    }

    [DataContract]
    public class PersonalDetailsRootObject
    {
        [DataMember(EmitDefaultValue = false, Order = 1)]
        public string first_name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 2)]
        public string middle_name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 3)]
        public string last_name { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 4)]
        public string birth_date { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 5)]
        public string gender { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 6)]
        public string country_code { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 7)]
        public string residence_country_code { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 8)]
        public string first_name_native { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 9)]
        public string middle_name_native { get; set; }

        [DataMember(EmitDefaultValue = false, Order = 10)]
        public string last_name_native { get; set; }

        public IEnumerable<string> Fields()
        {
            if (!string.IsNullOrEmpty(first_name)) yield return first_name;
            if (!string.IsNullOrEmpty(middle_name)) yield return middle_name;
            if (!string.IsNullOrEmpty(last_name)) yield return last_name;
            if (!string.IsNullOrEmpty(birth_date)) yield return birth_date;
            if (!string.IsNullOrEmpty(gender)) yield return gender;
            if (!string.IsNullOrEmpty(country_code)) yield return country_code;
            if (!string.IsNullOrEmpty(residence_country_code)) yield return residence_country_code;
            if (!string.IsNullOrEmpty(first_name_native)) yield return first_name_native;
            if (!string.IsNullOrEmpty(middle_name_native)) yield return middle_name_native;
            if (!string.IsNullOrEmpty(last_name_native)) yield return last_name_native;
        }

        public override string ToString()
        {
            return string.Join(", ", Fields());
        }

        public IEnumerable<string> Fields(Func<string, string> getGenderByCode, Func<string, string> getCountryByCode)
        {
            if (!string.IsNullOrEmpty(first_name)) yield return first_name;
            if (!string.IsNullOrEmpty(middle_name)) yield return middle_name;
            if (!string.IsNullOrEmpty(last_name)) yield return last_name;
            if (!string.IsNullOrEmpty(birth_date)) yield return birth_date;
            if (!string.IsNullOrEmpty(gender)) yield return getGenderByCode(gender) ?? gender;
            if (!string.IsNullOrEmpty(country_code)) yield return getCountryByCode(country_code) ?? country_code;
            if (!string.IsNullOrEmpty(residence_country_code)) yield return getCountryByCode(residence_country_code) ?? residence_country_code;
            if (!string.IsNullOrEmpty(first_name_native)) yield return first_name_native;
            if (!string.IsNullOrEmpty(middle_name_native)) yield return middle_name_native;
            if (!string.IsNullOrEmpty(last_name_native)) yield return last_name_native;
        }

        public string ToString(Func<string, string> getGenderByCode, Func<string, string> getCountryByCode)
        {
            return string.Join(", ", Fields(getGenderByCode, getCountryByCode));
        }
    }
}
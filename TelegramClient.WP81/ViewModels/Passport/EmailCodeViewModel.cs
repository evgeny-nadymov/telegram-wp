// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.ViewModels.Passport
{
    public class EmailCodeViewModel : ViewModelBase
    {
        private string _code;

        public string Code
        {
            get { return _code; }
            set { SetField(ref _code, value, () => Code); }
        }

        public string Subtitle { get; set; }

        public int CodeLength { get; set; }

        public TLString CurrentEmail { get; set; }

        private TLSentEmailCode _sentCode;
        
        private readonly TLSecureValue _emailValue;

        private readonly TLPasswordBase _passwordBase;

        private readonly TLAuthorizationForm _authorizationForm;

        private readonly IList<TLSecureValue> _secureValues;

        private readonly TLSecureValueTypeBase _secureType;

        private readonly SecureRequiredType _secureRequiredType;

        public EmailCodeViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _passwordBase = stateService.Password;
            stateService.Password = null;

            _authorizationForm = stateService.AuthorizationForm;
            stateService.AuthorizationForm = null;

            _secureValues = stateService.SecureValues;
            stateService.SecureValues = null;

            _secureType = stateService.SecureType;
            stateService.SecureType = null;

            _secureRequiredType = stateService.SecureRequiredType;
            stateService.SecureRequiredType = null;

            CurrentEmail = stateService.CurrentEmail;
            stateService.CurrentEmail = null;

            _emailValue = _secureRequiredType != null ? _secureRequiredType.DataValue : null;

            _sentCode = stateService.SentEmailCode;
            stateService.SentEmailCode = null;

            Subtitle = GetSubtitle();

            var length = _sentCode as ILength;
            CodeLength = length != null ? length.Length.Value : Constants.DefaultCodeLength;
        }

        private string GetSubtitle()
        {
            return string.Format(AppResources.PassportEmailVerifyInfo, CurrentEmail);
        }

        public void Confirm()
        {
            IsWorking = true;
            MTProtoService.VerifyEmailAsync(
                CurrentEmail,
                new TLString(Code),
                auth => BeginOnUIThread(() =>
                {
                    var emailValue = _emailValue;
                    if (emailValue == null)
                    {
                        var secureType = _authorizationForm != null
                            ? _authorizationForm.RequiredTypes.FirstOrDefault(EmailViewModel.IsValidType)
                            : null;

                        // add new email from passport settings
                        if (_secureType != null && EmailViewModel.IsValidType(_secureType))
                        {
                            emailValue = new TLSecureValue85
                            {
                                Flags = new TLInt(0),
                                Type = _secureType
                            };
                        }
                        // add new email from authorization form
                        else if (secureType != null)
                        {
                            emailValue = new TLSecureValue85
                            {
                                Flags = new TLInt(0),
                                Type = secureType
                            };
                        }
                        else
                        {
                            return;
                        }
                    }

                    IsWorking =
                    EmailViewModel.SaveEmailAsync(
                        CurrentEmail, _passwordBase as TLPassword, MTProtoService,
                        result => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            if (_authorizationForm != null)
                            {
                                _authorizationForm.Values.Remove(emailValue);
                                _authorizationForm.Values.Add(result);
                            }

                            emailValue.Update(result);
                            emailValue.NotifyOfPropertyChange(() => emailValue.Self);

                            if (_secureType != null)
                            {
                                EventAggregator.Publish(new AddSecureValueEventArgs { Values = new List<TLSecureValue> { emailValue } });
                            }

                            if (_secureRequiredType != null)
                            {
                                _secureRequiredType.UpdateValue();
                            }

                            NavigationService.RemoveBackEntry();
                            NavigationService.GoBack();
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;

                            if (error.CodeEquals(ErrorCode.BAD_REQUEST)
                                && error.TypeEquals(ErrorType.EMAIL_VERIFICATION_NEEDED))
                            {
                                MTProtoService.SendVerifyEmailCodeAsync(CurrentEmail, 
                                    sentCode => BeginOnUIThread(() =>
                                    {
                                        _sentCode = sentCode;

                                        Subtitle = GetSubtitle();
                                        NotifyOfPropertyChange(() => Subtitle);

                                        var length = _sentCode as ILength;
                                        CodeLength = length != null ? length.Length.Value : Constants.DefaultCodeLength;
                                        NotifyOfPropertyChange(() => CodeLength);
                                    }),
                                    error2 => BeginOnUIThread(() =>
                                    {
                                        if (error.TypeEquals(ErrorType.EMAIL_INVALID))
                                        {
                                            ShellViewModel.ShowCustomMessageBox(AppResources.EmailInvalidString, AppResources.Error, AppResources.Ok);
                                        }
                                        else if (error.CodeEquals(ErrorCode.FLOOD))
                                        {
                                            ShellViewModel.ShowCustomMessageBox(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, AppResources.Ok);
                                        }
                                        else
                                        {
                                            Telegram.Api.Helpers.Execute.ShowDebugMessage("account.sendVerifyEmailCode error " + error);
                                        }
                                    }));
                            }
                            else if (error.TypeEquals(ErrorType.EMAIL_INVALID))
                            {
                                ShellViewModel.ShowCustomMessageBox(AppResources.EmailInvalidString, AppResources.Error, AppResources.Ok);
                            }
                        }));
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    if (error.TypeEquals(ErrorType.CODE_INVALID))
                    {
                        ShellViewModel.ShowCustomMessageBox(AppResources.EmailCodeInvalidString, AppResources.Error, AppResources.Ok);
                    }
                    else if (error.TypeEquals(ErrorType.CODE_EMPTY))
                    {
                        ShellViewModel.ShowCustomMessageBox(AppResources.EmailCodeEmpty, AppResources.Error, AppResources.Ok);
                    }
                    else if (error.TypeEquals(ErrorType.EMAIL_VERIFY_EXPIRED))
                    {
                        ShellViewModel.ShowCustomMessageBox(AppResources.EmailCodeExpiredString, AppResources.Error, AppResources.Ok);
                    }
                    else if (error.TypeEquals(ErrorType.EMAIL_INVALID))
                    {
                        ShellViewModel.ShowCustomMessageBox(AppResources.EmailInvalidString, AppResources.Error, AppResources.Ok);
                    }
                    else if (error.CodeEquals(ErrorCode.FLOOD))
                    {
                        ShellViewModel.ShowCustomMessageBox(AppResources.FloodWaitString + Environment.NewLine + "(" + error.Message + ")", AppResources.Error, AppResources.Ok);
                    }
                    else
                    {
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("account.verifyEmail error " + error);
                    }
                }));
        }
    }
}

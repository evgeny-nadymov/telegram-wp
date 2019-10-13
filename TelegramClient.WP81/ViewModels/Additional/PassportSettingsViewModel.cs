using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Extensions;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Passport;
using TelegramClient.Views.Additional;

namespace TelegramClient.ViewModels.Additional
{
    public class PassportSettingsViewModel : ItemsViewModelBase<TLSecureValue>,
        Telegram.Api.Aggregator.IHandle<AddSecureValueEventArgs>,
        Telegram.Api.Aggregator.IHandle<DeleteSecureValueEventArgs>
    {
        private readonly TLPasswordBase _passwordBase;

        public PassportSettingsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _passwordBase = stateService.Password;
            stateService.Password = null;

            Items.Clear();
            if (stateService.SecureValues != null)
            {
                Items.AddRange(stateService.SecureValues);
                stateService.SecureValues = null;
            }

            EventAggregator.Subscribe(this);
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

        public void Edit(TLSecureValue value)
        {
            if (value == null) return;

            if (PhoneNumberViewModel.IsValidType(value.Type))
            {
                ShellViewModel.ShowCustomMessageBox(AppResources.PassportDeletePhoneNumberAlert, AppResources.AppName,
                    AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                    dismissed =>
                    {
                        if (dismissed == CustomMessageBoxResult.RightButton)
                        {
                            IsWorking = true;
                            MTProtoService.DeleteSecureValueAsync(new TLVector<TLSecureValueTypeBase> { new TLSecureValueTypePhone() },
                                result => Execute.BeginOnUIThread(() =>
                                {
                                    IsWorking = false;

                                    for (var i = 0; i < Items.Count; i++)
                                    {
                                        if (PhoneNumberViewModel.IsValidType(Items[i].Type))
                                        {
                                            Items.RemoveAt(i--);
                                        }
                                    }
                                }),
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    IsWorking = false;
                                }));
                        }
                    });
            }
            else if (EmailViewModel.IsValidType(value.Type))
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

                                    for (var i = 0; i < Items.Count; i++)
                                    {
                                        if (EmailViewModel.IsValidType(Items[i].Type))
                                        {
                                            Items.RemoveAt(i--);
                                        }
                                    }
                                }),
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    IsWorking = false;
                                }));
                        }
                    });
            }
            else if (ResidentialAddressViewModel.IsValidType(value.Type))
            {
                StateService.SecureValue = value;
                StateService.Password = _passwordBase;
                NavigationService.UriFor<ResidentialAddressViewModel>().Navigate();
            }
            else if (PersonalDetailsViewModel.IsValidType(value.Type))
            {
                StateService.SecureValue = value;
                StateService.Password = _passwordBase;
                NavigationService.UriFor<PersonalDetailsViewModel>().Navigate();
            }
        }

        public void AddDocument()
        {
            var items = new List<TLSecureValueTypeBase>
            {
                new TLSecureValueTypePhone(),
                new TLSecureValueTypeEmail(),
                new TLSecureValueTypePersonalDetails(),
                new TLSecureValueTypePassport(),
                new TLSecureValueTypeDriverLicense(),
                new TLSecureValueTypeIdentityCard(),
                new TLSecureValueTypeInternalPassport(),
                new TLSecureValueTypeAddress(),
                new TLSecureValueTypePassportRegistration(),
                new TLSecureValueTypeUtilityBill(),
                new TLSecureValueTypeBankStatement(),
                new TLSecureValueTypeRentalAgreement(),
                new TLSecureValueTypeTemporaryRegistration(),
            };

            var dict = new Dictionary<Type, Type>();
            foreach (var item in Items)
            {
                dict[item.Type.GetType()] = item.Type.GetType();
            }

            var panel = new StackPanel{ Margin = new Thickness(0.0, 12.0, 0.0, 0.0) };
            var messageBox = ShellViewModel.ShowCustomMessageBox(
                null, AppResources.PassportNoDocumentsAdd,
                null, null,
                dismissed =>
                {

                },
                items.Count > 10 ? 
                (object) new ScrollViewer { MaxHeight = 650.0, Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled } : 
                panel);

            for (var i = 0; i < items.Count; i++)
            {
                if (!dict.ContainsKey(items[i].GetType()))
                {
                    var listBoxItem = new ListBoxItem
                    {
                        Content =
                            new TextBlock
                            {
                                Text = SecureRequiredTypeToCaptionConverter.Convert(items[i]),
                                FontSize = 27,
                                Margin = new Thickness(12.0)
                            },
                        DataContext = items[i]
                    };
                    TiltEffect.SetIsTiltEnabled(listBoxItem, true);
                    listBoxItem.Tap += (sender, args) =>
                    {
                        messageBox.Dismiss();
                        var item = sender as ListBoxItem;
                        if (item != null)
                        {
                            var secureValueType = item.DataContext as TLSecureValueTypeBase;
                            if (secureValueType != null)
                            {
                                if (PhoneNumberViewModel.IsValidType(secureValueType))
                                {
                                    StateService.SecureType = secureValueType;
                                    StateService.Password = _passwordBase;
                                    NavigationService.UriFor<PhoneNumberViewModel>().Navigate();
                                }
                                else if (EmailViewModel.IsValidType(secureValueType))
                                {
                                    StateService.SecureType = secureValueType;
                                    StateService.Password = _passwordBase;
                                    NavigationService.UriFor<EmailViewModel>().Navigate();
                                }
                                else if (ResidentialAddressViewModel.IsValidType(secureValueType))
                                {
                                    StateService.SecureType = secureValueType;
                                    StateService.Password = _passwordBase;
                                    NavigationService.UriFor<ResidentialAddressViewModel>().Navigate();
                                }
                                else if (PersonalDetailsViewModel.IsValidType(secureValueType))
                                {
                                    StateService.SecureType = secureValueType;
                                    StateService.Password = _passwordBase;
                                    NavigationService.UriFor<PersonalDetailsViewModel>().Navigate();
                                }
                            }
                        }

                    };

                    panel.Children.Add(listBoxItem);
                }
            }
        }

        public void DeletePassport()
        {
            if (Items.Count == 0) return;

            ShellViewModel.ShowCustomMessageBox(
                AppResources.PassportDeleteConfirmation, AppResources.AppName,
                AppResources.Delete, AppResources.Cancel,
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        var items = new TLVector<TLSecureValueTypeBase>();
                        foreach (var item in Items)
                        {
                            items.Add(item.Type);
                        }
                        if (items.Count > 0)
                        {
                            MTProtoService.DeleteSecureValueAsync(
                                items,
                                result => BeginOnUIThread(() =>
                                {
                                    IsWorking = false;

                                    Items.Clear();
                                }),
                                error => BeginOnUIThread(() =>
                                {
                                    IsWorking = false;
                                }));
                        }
                    }
                });
        }

        public void Handle(DeleteSecureValueEventArgs args)
        {
            foreach (var value in args.Values)
            {
                Items.Remove(value);
            }
        }

        public void Handle(AddSecureValueEventArgs args)
        {
            foreach (var value in args.Values)
            {
                Items.Add(value);
            }
        }
    }

    public class DeleteSecureValueEventArgs
    {
        public IList<TLSecureValue> Values { get; set; }
    }

    public class AddSecureValueEventArgs
    {
        public IList<TLSecureValue> Values { get; set; }
    }
}

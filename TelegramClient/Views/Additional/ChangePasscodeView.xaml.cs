// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class ChangePasscodeView
    {
        public ChangePasscodeViewModel ViewModel
        {
            get { return DataContext as ChangePasscodeViewModel; }
        }

        private readonly AppBarButton _doneButton = new AppBarButton
        {
            Text = AppResources.Done,
            IsEnabled = false,
            IconUri = new Uri("/Images/ApplicationBar/appbar.check.png", UriKind.Relative)
        };

        private readonly AppBarButton _cancelButton = new AppBarButton
        {
            Text = AppResources.Cancel,
            IconUri = new Uri("/Images/ApplicationBar/appbar.cancel.rest.png", UriKind.Relative)
        };

        public ChangePasscodeView()
        {
            InitializeComponent();

            _doneButton.Click += (sender, args) => ViewModel.Done();
            _cancelButton.Click += (sender, args) => ViewModel.Cancel();

            Loaded += (sender, args) =>
            {
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() => PasscodeBox.Focus());
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };

            Unloaded += (sender, args) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };

            BuildLocalizedAppBar();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsPasscodeValid))
            {
                _doneButton.IsEnabled = ViewModel.IsPasscodeValid;
            }
        }

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar == null)
            {
                ApplicationBar = new ApplicationBar();

                ApplicationBar.Buttons.Add(_doneButton);
                ApplicationBar.Buttons.Add(_cancelButton);
            }
        }

        private void Passcode_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel.SelectedPasscodeType != null)
            {
                if (ViewModel.SelectedPasscodeType.Type == PasscodeType.Pin)
                {
                    if (e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key == Key.Back || e.Key == Key.Enter)
                    {
                        if (e.Key >= Key.D0 && e.Key <= Key.D9)
                        {
                            if (PasscodeBox.IsFocused
                                && PasscodeBox.Length == 4
                                && ConfirmPasscodeBox.Length < 4)
                            {
                                ConfirmPasscodeBox.Focus();
                            }
                        }
                    }
                    else
                    {
                        e.Handled = true;
                        return;
                    }
                }

                if (e.Key == Key.Enter)
                {
                    ConfirmOrCompletePasscode();
                }
                else if (e.Key == Key.Back)
                {
                    if (ConfirmPasscodeBox.IsFocused
                        && ConfirmPasscodeBox.Length == 0)
                    {
                        PasscodeBox.Focus();
                    }
                }
            }
        }

        private void ConfirmOrCompletePasscode()
        {
            if (PasscodeBox.IsFocused)
            {
                if (PasscodeBox.Length > 0)
                {
                    ConfirmPasscodeBox.Focus();
                }
            }
            else if (ConfirmPasscodeBox.IsFocused)
            {
                ViewModel.Done();
            }
        }

        private void Passcode_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null
                && ViewModel.SelectedPasscodeType != null
                && ViewModel.SelectedPasscodeType.Type == PasscodeType.Pin
                && PasscodeBox.Length == 4)
            {
                ConfirmOrCompletePasscode();
            }
        }
    }
}
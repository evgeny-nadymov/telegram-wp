// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using Microsoft.Devices;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Payments;

namespace TelegramClient.Views.Payments
{
    public partial class ShippingInfoView
    {
        public ShippingInfoViewModel ViewModel
        {
            get { return DataContext as ShippingInfoViewModel; }
        }

        public ShippingInfoView()
        {
            //LogManager.GetLog = type => new DebugLog(type);

            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) =>
            {
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };
            Unloaded += (sender, args) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.Error))
            {
                if (ViewModel.Error != null)
                {
                    var type = ViewModel.Error.GetErrorType();
                    switch (type)
                    {
                        case ErrorType.REQ_INFO_NAME_INVALID:
                            Shake(NameLabel, NameLabel.Input);
                            break;
                        case ErrorType.REQ_INFO_PHONE_INVALID:
                            Shake(Phone, PhoneNumberLabel.Input);
                            break;
                        case ErrorType.REQ_INFO_EMAIL_INVALID:
                            Shake(EmailLabel, EmailLabel.Input);
                            break;
                        case ErrorType.ADDRESS_COUNTRY_INVALID:
                            Shake(SelectCountry);
                            break;
                        case ErrorType.ADDRESS_CITY_INVALID:
                            Shake(CityLabel, CityLabel.Input);
                            break;
                        case ErrorType.ADDRESS_POSTCODE_INVALID:
                            Shake(PostCodeLabel, PostCodeLabel.Input);
                            break;
                        case ErrorType.ADDRESS_STATE_INVALID:
                            Shake(StateLabel, StateLabel.Input);
                            break;
                        case ErrorType.ADDRESS_STREET_LINE1_INVALID:
                            Shake(StreetLine1Label, StreetLine1Label.Input);
                            break;
                    }
                }
            }
        }

        public static void Shake(FrameworkElement element, PasswordBox focusControl)
        {
            var translateTransform = new TranslateTransform();
            element.RenderTransform = translateTransform;
            var storyboard = new Storyboard();
            var doubleAnimtion = new DoubleAnimationUsingKeyFrames();
            doubleAnimtion.KeyFrames.Add(new DiscreteDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -3.0 });
            doubleAnimtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 3.0, EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseIn, Oscillations = 5, Springiness = 1.0 } });
            doubleAnimtion.KeyFrames.Add(new DiscreteDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });

            storyboard.Children.Add(doubleAnimtion);
            Storyboard.SetTarget(storyboard, translateTransform);
            Storyboard.SetTargetProperty(doubleAnimtion, new PropertyPath("X"));
            storyboard.Begin();

            if (focusControl != null)
            {
                storyboard.Completed += (sender, args) =>
                {
                    focusControl.Focus();
                    focusControl.SelectAll();
                };
            }

            VibrateController.Default.Start(TimeSpan.FromSeconds(0.25));
        }

        public static void Shake(FrameworkElement element, TextBox focusControl = null)
        {
            var translateTransform = new TranslateTransform();
            element.RenderTransform = translateTransform;
            var storyboard = new Storyboard();
            var doubleAnimtion = new DoubleAnimationUsingKeyFrames();
            doubleAnimtion.KeyFrames.Add(new DiscreteDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = -3.0 });
            doubleAnimtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 3.0, EasingFunction = new ElasticEase { EasingMode = EasingMode.EaseIn, Oscillations = 5, Springiness = 1.0 } });
            doubleAnimtion.KeyFrames.Add(new DiscreteDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });

            storyboard.Children.Add(doubleAnimtion);
            Storyboard.SetTarget(storyboard, translateTransform);
            Storyboard.SetTargetProperty(doubleAnimtion, new PropertyPath("X"));
            storyboard.Begin();

            if (focusControl != null)
            {
                storyboard.Completed += (sender, args) =>
                {
                    focusControl.Focus();
                    focusControl.SelectAll();
                };
            }

            VibrateController.Default.Start(TimeSpan.FromSeconds(0.25));
        }

        private void Code_OnKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                PhoneNumberLabel.Focus();
                PhoneNumberLabel.SelectionStart = PhoneNumberLabel.Text.Length;
            }

            if (args.Key == Key.D8 || // *
                    args.Key == Key.Unknown || // +(long tap on 0) or -/.
                    args.Key == Key.D3 || // #
                    args.Key == Key.A)
            {

                args.Handled = true;
            }

            if (args.Key == Key.Space)
            {
                args.Handled = true;
                PhoneNumberLabel.Focus();
            }

            if (args.Key >= Key.NumPad0 && args.Key <= Key.NumPad9
                && PCode.Text.Length == 3)
            {
                var codeTail = string.Empty;
                if (ViewModel.IsPhoneCodeInvalid)
                {
                    var countryCode2 = CountryUtils.CountriesSource.FirstOrDefault(x => x.PhoneCode == PCode.Text.Substring(0, 2));
                    if (countryCode2 != null)
                    {
                        codeTail = PCode.Text.Substring(2, 1);
                        PCode.Text = PCode.Text.Substring(0, 2);
                    }

                    var countryCode1 = CountryUtils.CountriesSource.FirstOrDefault(x => x.PhoneCode == PCode.Text.Substring(0, 1));
                    if (countryCode1 != null)
                    {
                        codeTail = PCode.Text.Substring(1, 2);
                        PCode.Text = PCode.Text.Substring(0, 1);
                    }
                }


                args.Handled = true;
                PhoneNumberLabel.Text = codeTail + args.Key.ToString().Replace("NumPad", string.Empty) + PhoneNumberLabel.Text;
                PhoneNumberLabel.SelectionStart = codeTail.Length + 1;
                PhoneNumberLabel.Focus();
            }
        }

        private void PhoneNumber_OnKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.Enter)
            {
                ViewModel.Validate();
            }

            if (args.Key == Key.D8 || // *
                    args.Key == Key.Unknown || // +(long tap on 0) or -/.
                    args.Key == Key.D3 || // #
                    args.Key == Key.A ||
                    args.Key == Key.Space)
            {
                args.Handled = true;
            }

            if (args.Key == Key.Back && PhoneNumberLabel.Text.Length == 0)
            {
                args.Handled = true;
                PCode.Focus();
                PCode.Select(PCode.Text.Length, 0);
            }
        }

        private void PostCode_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ReceiverForm.Visibility == Visibility.Visible)
                {
                    if (NameLabel.Visibility == Visibility.Visible)
                    {
                        NameLabel.Focus();
                    }
                    else if (EmailLabel.Visibility == Visibility.Visible)
                    {
                        EmailLabel.Focus();
                    }
                    else if (Phone.Visibility == Visibility.Visible)
                    {
                        PCode.Focus();
                        PCode.Select(PCode.Text.Length, 0);
                    }
                    else
                    {
                        ViewModel.Validate();
                    }
                }
                else
                {
                    ViewModel.Validate();
                }
            }
        }

        private void Name_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ReceiverForm.Visibility == Visibility.Visible)
                {
                    if (EmailLabel.Visibility == Visibility.Visible)
                    {
                        EmailLabel.Focus();
                    }
                    else if (Phone.Visibility == Visibility.Visible)
                    {
                        PCode.Focus();
                        PCode.Select(PCode.Text.Length, 0);
                    }
                    else
                    {
                        ViewModel.Validate();
                    }
                }
                else
                {
                    ViewModel.Validate();
                }
            }
        }

        private void Email_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Phone.Visibility == Visibility.Visible)
                {
                    PCode.Focus();
                    PCode.Select(PCode.Text.Length, 0);
                }
                else
                {
                    ViewModel.Validate();
                }
            }
        }
    }
}
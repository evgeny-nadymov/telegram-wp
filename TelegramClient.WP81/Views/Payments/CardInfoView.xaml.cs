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
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using TelegramClient.Helpers;
using TelegramClient.ViewModels.Payments;
using TelegramClient.Views.Controls;

namespace TelegramClient.Views.Payments
{
    public partial class CardInfoView
    {
        public static readonly DependencyProperty FormattedTextProperty = DependencyProperty.RegisterAttached(
            "FormattedText", typeof(string), typeof(CardInfoView), new PropertyMetadata(default(string), OnFormattedTextChanged));

        public static void SetFormattedText(DependencyObject element, string value)
        {
            element.SetValue(FormattedTextProperty, value);
        }

        public static string GetFormattedText(DependencyObject element)
        {
            return (string)element.GetValue(FormattedTextProperty);
        }

        private static void OnFormattedTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var r = d as TextBlock;
            if (r != null)
            {
                var text = e.NewValue as string;
                if (text != null)
                {
                    r.Inlines.Clear();
                    var splittedText = text.Split(new[] { "*" }, StringSplitOptions.None);
                    for (var i = 0; i < splittedText.Length; i++)
                    {
                        if (i % 2 == 1)
                        {
                            var bold = new Run();
                            bold.FontWeight = FontWeights.SemiBold;
                            bold.Foreground = (Brush) Application.Current.Resources["TelegramBadgeAccentBrush"];
                            bold.Text = splittedText[i];
                            r.Inlines.Add(bold);

                        }
                        else
                        {
                            r.Inlines.Add(splittedText[i]);
                        }
                    }
                }
            }
        }

        public CardInfoViewModel ViewModel
        {
            get { return DataContext as CardInfoViewModel; }
        }

        public CardInfoView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            CardNumberLabel.SetTextBox(new CardTextBox());
            ExpirationDateLabel.SetTextBox(new DateTextBox());

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
                    var type = ViewModel.Error;
                    if (!string.IsNullOrEmpty(type))
                    {
                        switch (type)
                        {
                            case "invalid_number":
                            case "CARD_NUMBER_INVALID":
                                ShippingInfoView.Shake(CardNumberLabel, CardNumberLabel.Input);
                                break;
                            case "invalid_expiry_year":
                            case "invalid_expiry_month":
                            case "CARD_EXPIRE_DATE_INVALID":
                                ShippingInfoView.Shake(ExpirationDateLabel, ExpirationDateLabel.Input);
                                break;
                            case "CARD_HOLDER_NAME_INVALID":
                                ShippingInfoView.Shake(CardholderNameLabel, CardholderNameLabel.Input);
                                break;
                            case "invalid_cvc":
                            case "CARD_CVC_INVALID":
                                ShippingInfoView.Shake(SecurityCodeLabel, SecurityCodeLabel.Input);
                                break;
                            case "CARD_COUNTRY_INVALID":
                                ShippingInfoView.Shake(SelectCountry);
                                break;
                            case "CARD_ZIP_INVALID":
                                ShippingInfoView.Shake(PostCodeLabel, PostCodeLabel.Input);
                                break;
                            case "invalid_button":
                                ShippingInfoView.Shake(Validate);
                                break;
                        }
                    }
                }
            }
        }

        private void SecurityCodeLabel_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (PostCodeLabel.Visibility == Visibility.Visible)
                {
                    PostCodeLabel.Focus();
                }
                else
                {
                    ViewModel.SecurityCode = SecurityCodeLabel.TextBox.Password;
                    ViewModel.Validate();
                }
            }
        }

        private void PostCodeLabel_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Validate();
            }
        }
    }
}
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
using System.Windows.Navigation;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Auth;

namespace TelegramClient.Views.Auth
{
    public partial class SignInView
    {
        public SignInViewModel ViewModel
        {
            get { return DataContext as SignInViewModel; }
        }

        private readonly AppBarButton _logButton = new AppBarButton
        {
            Text = "Log",
            IsEnabled = true,
            IconUri = new Uri("/Images/ApplicationBar/appbar.manage.rest.png", UriKind.Relative)
        };

        public SignInView()
        { 
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            _logButton.Click += ButtonBase_OnClick;

            Loaded += (sender, args) =>
            {
                var elapsed = App.Timer.Elapsed.ToString();

                BuildLocalizedAppBar();

                TLUtils.WriteLineAtBegin("Startup Time SignInView: " + elapsed);
                TLUtils.WritePerformance("Startup Time SignInView: " + elapsed);

                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.3), () =>
                {
                    if (LogControl.Visibility == Visibility.Collapsed)
                    {
                        PhoneNumber.Focus();
                    }
                });
            };
        }

        private void BuildLocalizedAppBar()
        {
            if (ApplicationBar != null) return;

#if !PRIVATE_BETA
            return;
#endif


            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_logButton);
        }

        private void PhoneNumber_OnGotFocus(object sender, RoutedEventArgs e)
        {
            //SignIn.Margin = new Thickness(0, 0, 0, 300);
            //ThreadPool.QueueUserWorkItem(state =>
            //{
            //    Thread.Sleep(1000);
            //    Deployment.Current.Dispatcher.BeginInvoke(() =>
            //    {

            //        Scroll.ScrollIntoView(SignIn);
            //        //Scroll.ScrollIntoView(SignIn);
            //    });
            //});
        }

        private void PhoneNumber_OnKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.D8 || //звёздочка
                    args.Key == Key.Unknown || //плюс (долгий тап по 0), или чёрточка/точка
                    args.Key == Key.D3 || //решётка
                    args.Key == Key.A ||
                    args.Key == Key.Space)
            {
                args.Handled = true;
            }

            if (args.Key == Key.Back && PhoneNumber.Text.Length == 0)
            {
                args.Handled = true;
                PCode.Focus();
                PCode.Select(PCode.Text.Length, 0);
            }
        }

        //private Dictionary<>

        private void PCode_OnKeyDown(object sender, KeyEventArgs args)
        {
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
                PhoneNumber.Focus();
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
                PhoneNumber.Text = codeTail + args.Key.ToString().Replace("NumPad", string.Empty) + PhoneNumber.Text;
                PhoneNumber.SelectionStart = codeTail.Length + 1;
                PhoneNumber.Focus();
            }

            //if (args.Key == Key.Back
            //    && PCode.Text.Length > 0
            //    && PCode.Text[0] == '+'
            //    && PCode.SelectionStart == 1)
            //{
            //    args.Handled = true;
            //}
        }

        private void PhoneNumber_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _logButton.IsEnabled = PhoneNumber.Text.Length >= 2;
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (LogControl.Visibility == Visibility.Visible)
            {
                ButtonBase_OnClick(this, e);
                e.Cancel = true;
            }

            base.OnBackKeyPress(e);
        }

        private void ButtonBase_OnClick(object sender, System.EventArgs eventArgs)
        {
            if (FocusManager.GetFocusedElement() == PhoneNumber)
            {
                Self.Focus();
            }

            LogControl.Visibility = LogControl.Visibility == Visibility.Visible? Visibility.Collapsed : Visibility.Visible;
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SendMail();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
#if LOG_REGISTRATION
            TLUtils.WriteLog("SignInView.OnNavigatedTo");
#endif

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
#if LOG_REGISTRATION
            TLUtils.WriteLog("SignInView.OnNavigatedFrom");
#endif

            base.OnNavigatedFrom(e);
        }
    }
}
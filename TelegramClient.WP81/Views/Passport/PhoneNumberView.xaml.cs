// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 

using System.Linq;
using System.Windows.Input;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Passport;

namespace TelegramClient.Views.Passport
{
    public partial class PhoneNumberView
    {
        public PhoneNumberViewModel ViewModel
        {
            get { return DataContext as PhoneNumberViewModel; }
        }

        public PhoneNumberView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private void DoneIcon_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.Done();
        }

        private void PhoneNumber_OnKeyDown(object sender, KeyEventArgs args)
        {
            if (args.Key == Key.D8 ||
                args.Key == Key.Unknown ||
                args.Key == Key.D3 ||
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
        }
    }
}
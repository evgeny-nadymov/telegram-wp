// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Additional;

namespace TelegramClient.Views.Additional
{
    public partial class BioView
    {
        private readonly AppBarButton _doneButton = new AppBarButton
        {
            Text = AppResources.Done,
            IconUri = new Uri("/Images/ApplicationBar/appbar.check.png", UriKind.Relative)
        };

        private readonly AppBarButton _cancelButton = new AppBarButton
        {
            Text = AppResources.Cancel,
            IconUri = new Uri("/Images/ApplicationBar/appbar.cancel.rest.png", UriKind.Relative)
        };

        public BioViewModel ViewModel
        {
            get { return DataContext as BioViewModel; }
        }

        public BioView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;

            _doneButton.Click += (sender, args) => ViewModel.Done();
            _cancelButton.Click += (sender, args) => ViewModel.Cancel();

            Loaded += (sender, args) =>
            {
                Counter.Text = (Input.MaxLength - Input.Text.Length).ToString(CultureInfo.InvariantCulture);

                BuildLocalizedAppBar();
            };
        }

        private void BuildLocalizedAppBar()
        {
            return;
            if (ApplicationBar != null) return;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_doneButton);
            ApplicationBar.Buttons.Add(_cancelButton);
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Counter.Text = (Input.MaxLength - Input.Text.Length).ToString(CultureInfo.InvariantCulture);
        }

        private void DoneButton_OnClick(object sender, GestureEventArgs e)
        {
            ViewModel.Done();
        }
    }
}
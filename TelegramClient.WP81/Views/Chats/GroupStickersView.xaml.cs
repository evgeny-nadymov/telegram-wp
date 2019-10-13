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
using System.Windows.Data;
using System.Windows.Input;
using Telegram.Api.TL;
using TelegramClient.ViewModels.Chats;

namespace TelegramClient.Views.Chats
{
    public partial class GroupStickersView
    {
        public GroupStickersViewModel ViewModel
        {
            get { return DataContext as GroupStickersViewModel; }
        }

        public GroupStickersView()
        {
            InitializeComponent();

            Caption.Background = ShellView.CaptionBrush;
        }

        private bool _once;

        private TextBox _searchBox;

        private Border _removeButton;

        private void ForwardTransition_OnEndTransition(object sender, RoutedEventArgs e)
        {
            if (!_once)
            {
                _once = true;
                ViewModel.ForwardInAnimationComplete();
            }
        }

        private void DoneButton_OnClick(object sender, GestureEventArgs e)
        {
            ViewModel.Done();
        }

        private void Remove_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Remove();
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_removeButton == null) return;
            if (_searchBox == null) return;

            _removeButton.Visibility = string.IsNullOrEmpty(_searchBox.Text) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void LongListSelector_OnScrollingStarted(object sender, System.EventArgs e)
        {
            if (_searchBox == null) return;

            var focusedElement = FocusManager.GetFocusedElement();
            if (focusedElement == _searchBox)
            {
                Main.Focus();
            }
        }

        private void SearchBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            _searchBox = sender as TextBox;
        }

        private void RemoveButton_OnLoaded(object sender, RoutedEventArgs e)
        {
            _removeButton = sender as Border;
        }

        public void MoveCursorToEnd()
        {
            if (_searchBox == null) return;

            _searchBox.Select(_searchBox.Text.Length, 0);
        }
    }

    public class StickerSetTemplateSelector : IValueConverter
    {
        public DataTemplate StickerSetTemplate { get; set; }

        public DataTemplate NotFoundStickerSetTemplate { get; set; }

        public DataTemplate EmptyStickerSetTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TLStickerSet32)
            {
                return StickerSetTemplate;
            }
            if (value is TLStickerSetNotFound)
            {
                return NotFoundStickerSetTemplate;
            }

            return EmptyStickerSetTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
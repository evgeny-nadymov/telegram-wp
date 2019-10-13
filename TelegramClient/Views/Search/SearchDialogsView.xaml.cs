// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telegram.Api.TL;
using Telegram.Controls;
using Telegram.Controls.Extensions;
using TelegramClient.ViewModels.Search;

namespace TelegramClient.Views.Search
{
    public partial class SearchDialogsView
    {
        public SearchDialogsViewModel ViewModel
        {
            get { return DataContext as SearchDialogsViewModel; }
        }

        public SearchDialogsView()
        {
            InitializeComponent();
        }

        private FrameworkElement _lastTapedItem;

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            var obj = frameworkElement.DataContext as TLObject;
            if (obj == null) return;

            if (!ViewModel.OpenDialogDetails(obj))
            {
                return;
            }

            _lastTapedItem = sender as FrameworkElement;

            if (_lastTapedItem != null)
            {
                //foreach (var descendant in _lastTapedItem.GetVisualDescendants().OfType<HighlightingTextBlock>())
                //{
                //    if (AnimatedBasePage.GetIsAnimationTarget(descendant))
                //    {
                //        _lastTapedItem = descendant;
                //        break;
                //    }
                //}

                if (!(_lastTapedItem.RenderTransform is CompositeTransform))
                {
                    _lastTapedItem.RenderTransform = new CompositeTransform();
                }

                var tapedItemContainer = _lastTapedItem.FindParentOfType<ListBoxItem>();
                if (tapedItemContainer != null)
                {
                    tapedItemContainer = tapedItemContainer.FindParentOfType<ListBoxItem>();
                }

                SearchShellView.StartContinuumForwardOutAnimation(_lastTapedItem, tapedItemContainer);
            }
        }

        private void Items_OnScrollingStateChanged(object sender, ScrollingStateChangedEventArgs e)
        {
            if (e.NewValue)
            {
                var focusElement = FocusManager.GetFocusedElement();
                if (focusElement != null
                    && focusElement.GetType() == typeof(WatermarkedTextBox))
                {
                    Self.Focus();
                }
            }
        }
    }
}
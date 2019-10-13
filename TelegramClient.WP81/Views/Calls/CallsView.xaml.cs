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
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Telegram.Api.TL;
using Telegram.Controls;
using Telegram.Controls.Extensions;
using TelegramClient.ViewModels.Calls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Calls
{
    public partial class CallsView
    {
        public CallsViewModel ViewModel
        {
            get { return DataContext as CallsViewModel; }
        }

        public CallsView()
        {
            InitializeComponent();
        }

        public FrameworkElement TapedItem;

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            TapedItem = (FrameworkElement) sender;

            var tapedItemContainer = TapedItem.FindParentOfType<ListBoxItem>();

            var result = ViewModel.OpenDialogDetails(TapedItem.DataContext as TLDialogBase);
            if (result)
            {
                ShellView.StartContinuumForwardOutAnimation(TapedItem, tapedItemContainer);
            }
        }

        private void Items_OnCloseToEnd(object sender, System.EventArgs e)
        {
            
        }

        private void Items_OnManipulationStarted(object sender, ScrollingStateChangedEventArgs e)
        {
            
        }

        private void Items_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            
        }
    }
}
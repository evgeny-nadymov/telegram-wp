// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Interactivity;
using Microsoft.Phone.Controls;

namespace TelegramClient.Behaviors
{
    public class ToggleSwitchLocalizedContentBehavior : Behavior<ToggleSwitch>
    {
        public static readonly DependencyProperty OnContentProperty =
            DependencyProperty.Register("OnContent", typeof (string), typeof (ToggleSwitchLocalizedContentBehavior), new PropertyMetadata(default(string)));

        public string OnContent
        {
            get { return (string) GetValue(OnContentProperty); }
            set { SetValue(OnContentProperty, value); }
        }

        public static readonly DependencyProperty OffContentProperty =
            DependencyProperty.Register("OffContent", typeof (string), typeof (ToggleSwitchLocalizedContentBehavior), new PropertyMetadata(default(string)));

        public string OffContent
        {
            get { return (string) GetValue(OffContentProperty); }
            set { SetValue(OffContentProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Loaded += ToggleSwitch_Click;
            //AssociatedObject.Click += ToggleSwitch_Click;
            AssociatedObject.Checked += ToggleSwitch_Click;
            AssociatedObject.Unchecked += ToggleSwitch_Click;
        }

        private void ToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Content = AssociatedObject.IsChecked == true ? OnContent : OffContent;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Unchecked -= ToggleSwitch_Click;
            AssociatedObject.Checked -= ToggleSwitch_Click;
            //AssociatedObject.Click -= ToggleSwitch_Click;
            AssociatedObject.Loaded -= ToggleSwitch_Click;

            base.OnDetaching();
        }
    }
}

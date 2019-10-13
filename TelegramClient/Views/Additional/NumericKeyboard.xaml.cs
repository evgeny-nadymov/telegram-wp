// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace TelegramClient.Views.Additional
{
    public partial class NumericKeyboard
    {
        public Uri BackButtonImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                return isLightTheme
                    ? new Uri("/Images/NumericKeyboard/back.white.png", UriKind.Relative)
                    : new Uri("/Images/NumericKeyboard/back.png", UriKind.Relative);
            }
        }

        private readonly IList<Border> _buttons = new List<Border>();

        public NumericKeyboard()
        {
            InitializeComponent();

            _buttons.Add(Button0);
            _buttons.Add(Button1);
            _buttons.Add(Button2);
            _buttons.Add(Button3);
            _buttons.Add(Button4);
            _buttons.Add(Button5);
            _buttons.Add(Button6);
            _buttons.Add(Button7);
            _buttons.Add(Button8);
            _buttons.Add(Button9);

            SetButtons();
            
            _timer.Interval = TimeSpan.FromSeconds(0.15);
            _timer.Tick += OnTimerTick;
        }

        private void SetButtons()
        {
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            var background = isLightTheme ? (Brush)Resources["ButtonLightBackground"] : (Brush)Resources["ButtonBackground"];

            foreach (var button in _buttons)
            {
                button.Background = background;
                button.MouseLeftButtonUp += UIElement_OnMouseLeftButtonUp;
                button.MouseEnter += UIElement_OnMouseEnter;
                button.MouseLeave += UIElement_OnMouseLeave;
            }

            Button.Background = background;
            ButtonBack.Background = background;
            ButtonBack.MouseLeftButtonDown += ButtonBack_OnMouseLeftButtonDown;
            ButtonBack.MouseEnter += UIElement_OnMouseEnter;
            ButtonBack.MouseLeave += ButtonBack_OnMouseLeave;
            ButtonBack.MouseLeftButtonUp += ButtonBack_OnMouseLeftButtonUp;
        }

        private void ButtonBack_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _timer.Stop();

            var border = sender as Border;
            if (border != null)
            {
                border.Background = _normalBrush;
            }
        }

        private void ButtonBack_OnMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            OnTimerTick(sender, e);
            _timer.Start();
        }

        private void ButtonBack_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _timer.Stop();
        }

        private void OnTimerTick(object sender, System.EventArgs e)
        {
            if (Input != null)
            {
                var length = Input.Text.Length;
                Input.Text = Input.Text.Substring(0, Math.Max(0, length - 1));
            }
        }

        public static readonly DependencyProperty InputProperty = DependencyProperty.Register(
            "Input", typeof (TextBox), typeof (NumericKeyboard), new PropertyMetadata(default(TextBox)));

        public TextBox Input
        {
            get { return (TextBox) GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        private Brush _normalBrush;

        private readonly DispatcherTimer _timer = new DispatcherTimer();

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                _normalBrush = border.Background;
                border.Background = (Brush)Application.Current.Resources["PhoneAccentBrush"];
            }
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                border.Background = _normalBrush;
            }
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var border = sender as Border;
            if (border != null)
            {
                border.Background = _normalBrush;

                var tag = border.Tag;
                if (tag != null && Input != null)
                {
                    Input.Text += tag.ToString();
                }
            }
        }
    }
}

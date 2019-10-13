// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Telegram.Controls
{
    public partial class UnreadCounter
    {
        public static readonly DependencyProperty BorderBackgroundProperty = DependencyProperty.Register(
            "BorderBackground", typeof (Brush), typeof (UnreadCounter), new PropertyMetadata(default(Brush), OnBorderBackgroundChanged));

        private static void OnBorderBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var unreadCounter = d as UnreadCounter;
            if (unreadCounter != null)
            {
                unreadCounter.Border.Background = (Brush) e.NewValue;
            }
        }

        public Brush BorderBackground
        {
            get { return (Brush) GetValue(BorderBackgroundProperty); }
            set { SetValue(BorderBackgroundProperty, value); }
        }

        public static readonly DependencyProperty CounterProperty = DependencyProperty.Register(
            "Counter", typeof(int), typeof(UnreadCounter), new PropertyMetadata(OnCounterChanged));

        private static void OnCounterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var unreadCounter = d as UnreadCounter;
            if (unreadCounter != null)
            {
                var counter = (int)e.NewValue;

                unreadCounter.Visibility = counter <= 0 ? Visibility.Collapsed : Visibility.Visible;
                if (counter < 1000)
                {
                    unreadCounter.CounterText.Text = counter.ToString(CultureInfo.InvariantCulture);
                }
                else
                {

                    unreadCounter.CounterText.Text = counter / 1000 + "K";
                }
            }
        }

        public int Counter
        {
            get { return (int)GetValue(CounterProperty); }
            set { SetValue(CounterProperty, value); }
        }

        public UnreadCounter()
        {
            InitializeComponent();

            Background = (Brush) Resources["PhoneAccentBrush"];

            Visibility = Visibility.Collapsed;
            CounterText.Text = null;
        }
    }
}

using System.Globalization;
using System.Windows;

namespace TelegramClient.Views.Controls
{
    public partial class UnreadCounter
    {
        public static readonly DependencyProperty CounterProperty = DependencyProperty.Register(
            "Counter", typeof (int), typeof (UnreadCounter), new PropertyMetadata(OnCounterChanged));

        private static void OnCounterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var unreadCounter = d as UnreadCounter;
            if (unreadCounter != null)
            {
                var counter = (int) e.NewValue;

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
            get { return (int) GetValue(CounterProperty); }
            set { SetValue(CounterProperty, value); }
        }

        public UnreadCounter()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;
            CounterText.Text = null;
        }
    }
}

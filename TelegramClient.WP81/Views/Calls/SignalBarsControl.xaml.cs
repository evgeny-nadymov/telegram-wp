using System.Windows;

namespace TelegramClient.Views.Calls
{
    public partial class SignalBarsControl
    {
        public static readonly DependencyProperty SignalProperty = DependencyProperty.Register(
            "Signal", typeof(int), typeof(SignalBarsControl), new PropertyMetadata(OnSignalChanged));

        private static void OnSignalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as SignalBarsControl;
            if (control != null)
            {
                var signal = (int) e.NewValue;

                control.Bar1.Opacity = signal >= 1 ? 1.0 : 0.5;
                control.Bar2.Opacity = signal >= 2 ? 1.0 : 0.5;
                control.Bar3.Opacity = signal >= 3 ? 1.0 : 0.5;
                control.Bar4.Opacity = signal >= 4 ? 1.0 : 0.5;
            }
        }

        public int Signal
        {
            get { return (int) GetValue(SignalProperty); }
            set { SetValue(SignalProperty, value); }
        }

        public SignalBarsControl()
        {
            InitializeComponent();
        }
    }
}

using System.Windows;

namespace Vk.Messenger.Views.Controls
{
    public partial class MainTitleControl
    {
        public static readonly DependencyProperty OnlyLogoProperty =
            DependencyProperty.Register("OnlyLogo", typeof (bool), typeof (MainTitleControl), new PropertyMetadata(default(bool)));

        public bool OnlyLogo
        {
            get { return (bool) GetValue(OnlyLogoProperty); }
            set { SetValue(OnlyLogoProperty, value); }
        }

        public MainTitleControl()
        {
            InitializeComponent();
        }
    }
}

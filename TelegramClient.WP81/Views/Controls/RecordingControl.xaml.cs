// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace TelegramClient.Views.Controls
{
    public partial class RecordingControl
    {
        public RecordingControl()
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                Storyboard1.Begin();
                Storyboard2.Begin();
                Storyboard3.Begin();
            };

            Unloaded += (sender, args) =>
            {
                Storyboard1.Stop();
                Storyboard2.Stop();
                Storyboard3.Stop();
            };
        }
    }
}

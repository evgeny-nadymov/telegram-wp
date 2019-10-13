// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using Telegram.Api.TL;

namespace TelegramClient.Views.Controls
{
    public partial class TypingControl
    {
        public static readonly DependencyProperty TypingProperty = DependencyProperty.Register(
                    "Typing", typeof(Typing), typeof(TypingControl), new PropertyMetadata(default(Typing), OnTypingChanged));

        private static void OnTypingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var typingControl = d as TypingControl;
            if (typingControl != null)
            {
                var oldTyping = e.OldValue as Typing;
                var newTyping = e.NewValue as Typing;
                if (Typing.Equals(oldTyping, newTyping)) return;

                if (newTyping == null)
                {
                    typingControl.Content = null;
                }
                else
                {
                    switch (newTyping.Type)
                    {
                        case TypingType.Record:
                            typingControl.Content = new RecordingControl { Height = 26.0 };
                            break;
                        case TypingType.Upload:
                            typingControl.Content = new UploadingControl { Height = 26.0 };
                            break;
                        default:
                            typingControl.Content = new TextingControl { Height = 26.0 };
                            break;
                    }
                }
            }
        }

        public Typing Typing
        {
            get { return (Typing)GetValue(TypingProperty); }
            set { SetValue(TypingProperty, value); }
        }

        public TypingControl()
        {
            InitializeComponent();
        }
    }
}

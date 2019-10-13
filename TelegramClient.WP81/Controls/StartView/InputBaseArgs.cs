// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;

namespace TelegramClient.Controls.StartView
{
    internal class InputBaseArgs
    {
        protected InputBaseArgs(UIElement source, Point origin)
        {
            Source = source;
            Origin = origin;
        }

        public UIElement Source { get; private set; }

        public Point Origin { get; private set; }
    }
}

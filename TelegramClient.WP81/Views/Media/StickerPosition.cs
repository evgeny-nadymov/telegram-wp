// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;

namespace TelegramClient.Views.Media
{
    public class StickerPosition
    {
        public Point Position { get; set; }

        public float Scale { get; set; }

        public float Angle { get; set; }

        public StickerPosition(Point position, float scale, float angle)
        {
            Position = position;
            Scale = scale;
            Angle = angle;
        }
    }
}

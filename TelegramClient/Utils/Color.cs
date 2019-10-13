// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows.Media;

namespace TelegramClient.Utils
{
    public static class ColorUtils
    {
        public static Color MergeColors(this Color c1, Color c2)
        {
            return MixColors(c1, c2);
        }

        private static Color MixColors(Color c1, Color c2)
        {
            var c1a = c1.A / 255.0;
            var c2a = c2.A / 255.0;
            var alp = AlphaBlend(c1a, c2a);

            var a = alp * 255;
            var r = ColorBlend(c1.R, c1a, c2.R, c2a, alp);
            var g = ColorBlend(c1.G, c1a, c2.G, c2a, alp);
            var b = ColorBlend(c1.B, c1a, c2.B, c2a, alp);

            return Color.FromArgb(Convert.ToByte(a), Convert.ToByte(r), Convert.ToByte(g), Convert.ToByte(b));
        }


        private static double ColorBlend(byte c1r, double c1a, byte c2r, double c2a, double alp)
        {
            return (c2r * c2a + c1r * c1a * (1 - c2a)) / alp;
        }

        private static double AlphaBlend(double alphaBelow, double alphaAbove)
        {
            return alphaBelow + (1.0 - alphaBelow) * alphaAbove;
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.EmojiPanel.Controls.Utilites
{
    public static class Helpers
    {
        public static Uri GetAssetUri(string assetName)
        {
            return new Uri(String.Format("/Assets/{0}-WXGA.png", assetName), UriKind.Relative);

            //switch (Application.Current.Host.Content.ScaleFactor)
            //{
            //    case 100:
            //        return new Uri(String.Format("/Assets/{0}-WVGA.png", assetName), UriKind.Relative);
            //    case 160:
            //        return new Uri(String.Format("/Assets/{0}-WXGA.png", assetName), UriKind.Relative);
            //    case 150:
            //        return new Uri(String.Format("/Assets/{0}-720p.png", assetName), UriKind.Relative);
            //    default:
            //        return new Uri(String.Format("/Assets/{0}-WVGA.png", assetName), UriKind.Relative);
            //}
        }
    }
}

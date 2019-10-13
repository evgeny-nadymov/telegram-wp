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

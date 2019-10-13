// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using Microsoft.Phone.Controls.Maps;

namespace TelegramClient.Views.Media.MapTileSources
{
    public class OpenStreetMapTileSource : TileSource
    {
        public OpenStreetMapTileSource()
	    {
	        //Uri base of an OpenSTreetMap server
            UriFormat = "https://a.tile.openstreetmap.org/{0}/{1}/{2}.png";
	    }
 
	    public override Uri GetUri(int x, int y, int zoomLevel)
        { 
            if (zoomLevel > 0 && zoomLevel <= 18)
            {
                var url = string.Format(UriFormat,
                    zoomLevel,
                    x,
                    y);
 
                return new Uri(url);
            }
			//if zoom level is not supported, return null
            return null;
        }
    }
}

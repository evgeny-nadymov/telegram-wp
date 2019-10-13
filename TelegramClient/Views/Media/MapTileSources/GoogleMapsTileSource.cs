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
    public enum GoogleMapsTileSourceType
    {
        Street,
        Hybrid,
        Satellite,
        Physical,
        PhysicalHybrid,
        StreetOverlay,
        WaterOverlay
    }

    public class GoogleMapsTileSource : TileSource
    {
        public GoogleMapsTileSource()
        {
            UriFormat = @"https://mt{0}.google.com/vt/lyrs={1}&z={2}&x={3}&y={4}";
            MapsTileSourceType = GoogleMapsTileSourceType.Street;
        }
        private int _servernr;
        private char _mapMode;

        private int Server
        {
            get
            {
                return _servernr = (_servernr + 1) % 4;
            }
        }

        private GoogleMapsTileSourceType _mapsTileSourceType;
        public GoogleMapsTileSourceType MapsTileSourceType
        {
            get { return _mapsTileSourceType; }
            set
            {
                _mapsTileSourceType = value;
                _mapMode = TypeToMapMode(value);
            }
        }

        public override Uri GetUri(int x, int y, int zoomLevel)
        {
            {
                if (zoomLevel > 0)
                {
                    var url = string.Format(UriFormat, Server, _mapMode, zoomLevel, x, y);
                    return new Uri(url);
                }
            }
            return null;
        }

        private static char TypeToMapMode(GoogleMapsTileSourceType mapsTileSourceType)
        {
            switch (mapsTileSourceType)
            {
                case GoogleMapsTileSourceType.Hybrid:
                    return 'y';
                case GoogleMapsTileSourceType.Satellite:
                    return 's';
                case GoogleMapsTileSourceType.Street:
                    return 'm';
                case GoogleMapsTileSourceType.Physical:
                    return 't';
                case GoogleMapsTileSourceType.PhysicalHybrid:
                    return 'p';
                case GoogleMapsTileSourceType.StreetOverlay:
                    return 'h';
                case GoogleMapsTileSourceType.WaterOverlay:
                    return 'r';
            } return ' ';
        }
    }
}

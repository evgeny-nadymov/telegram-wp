// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Globalization;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public abstract class TLInputGeoPointBase : TLObject
    {
        public abstract bool GeoPointEquals(TLInputGeoPointBase locationGeoPoint);
    }

    public class TLInputGeoPointEmpty : TLInputGeoPointBase
    {
        public const uint Signature = TLConstructors.TLInputGeoPointEmpty;

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override string ToString()
        {
            return "empty";
        }

        public override bool GeoPointEquals(TLInputGeoPointBase geoPointBase)
        {
            var geoPointEmpty = geoPointBase as TLInputGeoPointEmpty;
            if (geoPointEmpty != null)
            {
                return true;
            }

            return false;
        }
    }

    public class TLInputGeoPoint : TLInputGeoPointBase
    {
        public const uint Signature = TLConstructors.TLInputGeoPoint;

        public TLDouble Lat { get; set; }
        public TLDouble Long { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Lat.ToBytes(),
                Long.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Lat = GetObject<TLDouble>(input);
            Long = GetObject<TLDouble>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Lat.ToStream(output);
            Long.ToStream(output);
        }

        public string GetFileName()
        {
            return string.Format("staticmap{0}_{1}.jpg", Lat.Value.ToString(new CultureInfo("en-US")), Long.Value.ToString(new CultureInfo("en-US")));
        }

        public override string ToString()
        {
            return string.Format("{0}_{1}", Lat, Long);
        }

        public override bool GeoPointEquals(TLInputGeoPointBase geoPointBase)
        {
            var geoPoint = geoPointBase as TLInputGeoPoint;
            if (geoPoint != null)
            {
                return 
                    Lat.Value == geoPoint.Lat.Value 
                    && Long.Value == geoPoint.Long.Value;
            }

            return false;
        }
    }
}

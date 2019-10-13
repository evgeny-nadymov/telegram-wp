// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [KnownType(typeof(TLGeoPointEmpty))]
    [KnownType(typeof(TLGeoPoint))]
    [DataContract]
    public abstract class TLGeoPointBase : TLObject { }

    [DataContract]
    public class TLGeoPointEmpty : TLGeoPointBase
    {
        public const uint Signature = TLConstructors.TLGeoPointEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }

    [DataContract]
    public class TLGeoPoint82 : TLGeoPoint
    {
        public new const uint Signature = TLConstructors.TLGeoPoint82;

        [DataMember]
        public TLLong AccessHash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Long = GetObject<TLDouble>(bytes, ref position);
            Lat = GetObject<TLDouble>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Long = GetObject<TLDouble>(input);
            Lat = GetObject<TLDouble>(input);
            AccessHash = GetObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Long.ToBytes());
            output.Write(Lat.ToBytes());
            output.Write(AccessHash.ToBytes());
        }

        public override string ToString()
        {
            return string.Format("TLGeoPoint82 [lat={0} long={1} access_hash={2}]", Lat, Long, AccessHash);
        }
    }

    [DataContract]
    public class TLGeoPoint : TLGeoPointBase
    {
        public const uint Signature = TLConstructors.TLGeoPoint;

        [DataMember]
        public TLDouble Long { get; set; }

        [DataMember]
        public TLDouble Lat { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Long = GetObject<TLDouble>(bytes, ref position);
            Lat = GetObject<TLDouble>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Long = GetObject<TLDouble>(input);
            Lat = GetObject<TLDouble>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Long.ToBytes());
            output.Write(Lat.ToBytes());
        }

        public string GetFileName()
        {
            return string.Format("staticmap{0}_{1}.jpg", Lat.Value.ToString(new CultureInfo("en-US")), Long.Value.ToString(new CultureInfo("en-US")));
        }

        public override string ToString()
        {
            return string.Format("TLGeoPoint[lat={0} long={1}]", Lat, Long);
        }
    }
}

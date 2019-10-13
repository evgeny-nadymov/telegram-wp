// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Messages
{
    [Flags]
    public enum GetInlineBotResultsFlags
    {
        GeoPoint = 0x1
    }

    class TLGetInlineBotResults : TLObject
    {
        public const uint Signature = 0x514e999d;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInputUserBase Bot { get; set; }

        public TLInputPeerBase Peer { get; set; }

        private TLInputGeoPointBase _geoPoint;

        public TLInputGeoPointBase GeoPoint
        {
            get { return _geoPoint; }
            set { SetField(out _geoPoint, value, ref _flags, (int) GetInlineBotResultsFlags.GeoPoint); }
        }

        public TLString Query { get; set; }

        public TLString Offset { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Bot.ToBytes(),
                Peer.ToBytes(),
                ToBytes(GeoPoint, Flags, (int) GetInlineBotResultsFlags.GeoPoint),
                Query.ToBytes(),
                Offset.ToBytes());
        }
    }
}

using System;

namespace Telegram.Api.TL.Functions.Messages
{
    [Flags]
    public enum EditGeoLiveFlags
    {
        Stop = 0x1,
        GeoPoint = 0x2
    }

    public class TLEditGeoLive : TLObject
    {
        public const uint Signature = 0x9a92304e;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Stop
        {
            get { return IsSet(_flags, (int) EditGeoLiveFlags.Stop); }
            set { SetUnset(ref _flags, value, (int) EditGeoLiveFlags.Stop); }
        }

        public TLInputPeerBase Peer { get; set; }

        public TLInt Id { get; set; }

        private TLInputGeoPointBase _geoPoint;

        public TLInputGeoPointBase GeoPoint
        {
            get { return _geoPoint; }
            set { SetField(out _geoPoint, value, ref _flags, (int) EditGeoLiveFlags.GeoPoint); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Peer.ToBytes(),
                Id.ToBytes(),
                ToBytes(GeoPoint, _flags, (int) EditGeoLiveFlags.GeoPoint));
        }
    }
}

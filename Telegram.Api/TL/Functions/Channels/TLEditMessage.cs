// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Channels
{
    [Flags]
    public enum EditMessageFlags
    {
        // 0
        NoWebPage = 0x2,        // 1
        ReplyMarkup = 0x4,      // 2
        Entities = 0x8,         // 3

        Message = 0x800,        // 11
        StopGeoLive = 0x1000,   // 12
        GeoPoint = 0x2000,      // 13
        Media = 0x4000,         // 14
    }

    class TLEditMessage : TLObject
    {
        public const uint Signature = 0xc000e4c8;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInputPeerBase Peer { get; set; }

        public TLInt Id { get; set; }

        private TLString _message;

        public TLString Message
        {
            get { return _message; }
            set { SetField(out _message, value, ref _flags, (int)EditMessageFlags.Message); }
        }

        private TLVector<TLMessageEntityBase> _entities;

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set { SetField(out _entities, value, ref _flags, (int)EditMessageFlags.Entities); }
        }

        private TLInputMediaBase _media;

        public TLInputMediaBase Media
        {
            get { return _media; }
            set { SetField(out _media, value, ref _flags, (int)EditMessageFlags.Media); }
        }

        private TLReplyKeyboardBase _replyMarkup;

        public TLReplyKeyboardBase ReplyMarkup
        {
            get { return _replyMarkup; }
            set { SetField(out _replyMarkup, value, ref _flags, (int)EditMessageFlags.ReplyMarkup); }
        }

        public bool NoWebPage
        {
            get { return IsSet(Flags, (int)EditMessageFlags.NoWebPage); }
            set { SetUnset(ref _flags, value, (int)EditMessageFlags.NoWebPage); }
        }

        private TLInputGeoPointBase _geoPoint;

        public TLInputGeoPointBase GeoPoint
        {
            get { return _geoPoint; }
            set { SetField(out _geoPoint, value, ref _flags, (int)EditMessageFlags.GeoPoint); }
        }

        public bool StopGeoLive
        {
            get { return IsSet(Flags, (int)EditMessageFlags.StopGeoLive); }
            set { SetUnset(ref _flags, value, (int)EditMessageFlags.StopGeoLive); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Peer.ToBytes(),
                Id.ToBytes(),
                ToBytes(Message, Flags, (int)EditMessageFlags.Message),
                ToBytes(Media, Flags, (int)EditMessageFlags.Media),
                ToBytes(ReplyMarkup, Flags, (int)EditMessageFlags.ReplyMarkup),
                ToBytes(Entities, Flags, (int)EditMessageFlags.Entities),
                ToBytes(GeoPoint, Flags, (int)EditMessageFlags.GeoPoint));
        }
    }
}

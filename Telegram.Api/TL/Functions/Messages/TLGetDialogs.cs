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
    public enum GetDialogsFlags
    {
        ExcludePinned = 0x1,
        FeedId = 0x2,
    }

    class TLGetDialogs : TLObject
    {
        public const uint Signature = 0xb098aee6;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool ExcludePinned
        {
            get { return IsSet(Flags, (int) GetDialogsFlags.ExcludePinned); }
            set { SetUnset(ref _flags, value, (int) GetDialogsFlags.ExcludePinned); }
        }

        //private TLInt _feedId;

        //public TLInt FeedId
        //{
        //    get { return _feedId; }
        //    set { SetField(out _feedId, value, ref _flags, (int)GetDialogsFlags.FeedId); }
        //}

        public TLInt OffsetDate { get; set; }

        public TLInt OffsetId { get; set; }

        public TLInputPeerBase OffsetPeer { get; set; }

        public TLInt Limit { get; set; }

        public TLInt Hash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                //ToBytes(FeedId, Flags, (int)GetDialogsFlags.FeedId),
                OffsetDate.ToBytes(),
                OffsetId.ToBytes(),
                OffsetPeer.ToBytes(),
                Limit.ToBytes(),
                Hash.ToBytes());
        }
    }
}

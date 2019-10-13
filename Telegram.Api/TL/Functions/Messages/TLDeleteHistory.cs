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
    public enum DeleteHistoryFlags
    {
        JustClear = 0x1,
    }

    class TLDeleteHistory : TLObject
    {
        public const uint Signature = 0x1c015b09;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInputPeerBase Peer { get; set; }

        public TLInt MaxId { get; set; }

        public void SetJustClear()
        {
            Set(ref _flags, (int) DeleteHistoryFlags.JustClear);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Peer.ToBytes(),
                MaxId.ToBytes());
        }
    }
}

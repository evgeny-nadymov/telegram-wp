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
    public enum MarkDialogUnreadFlags
    {
        Unread = 0x1
    }

    class TLMarkDialogUnread : TLObject
    {
        public const uint Signature = 0xc286d98f;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Unread
        {
            get { return IsSet(Flags, (int)MarkDialogUnreadFlags.Unread); }
            set { SetUnset(ref _flags, value, (int)MarkDialogUnreadFlags.Unread); }
        }

        public TLInputDialogPeerBase Peer { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Peer.ToBytes());
        }
    }
}

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
    public enum ToggleDialogPinFlags
    {
        Pinned = 0x1,
    }

    class TLToggleDialogPin : TLObject
    {
        public const uint Signature = 0x3289be6a;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Pinned
        {
            get { return IsSet(Flags, (int)ToggleDialogPinFlags.Pinned); }
            set { SetUnset(ref _flags, value, (int)ToggleDialogPinFlags.Pinned); }
        }

        public TLInputPeerBase Peer { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Peer.ToBytes());
        }
    }
}

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
    public enum GetBotCallbackAnswerFlags
    {
        Data = 0x1,
        Game = 0x2,
    }

    class TLGetBotCallbackAnswer : TLObject
    {
        public const uint Signature = 0x810a9fec;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInputPeerBase Peer { get; set; }

        public TLInt MessageId { get; set; }

        private TLString _data;

        public TLString Data
        {
            get { return _data; }
            set { SetField(out _data, value, ref _flags, (int) GetBotCallbackAnswerFlags.Data); }
        }

        public void SetGame()
        {
            Set(ref _flags, (int) GetBotCallbackAnswerFlags.Game);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Peer.ToBytes(),
                MessageId.ToBytes(),
                ToBytes(Data, Flags, (int) GetBotCallbackAnswerFlags.Data));
        }
    }
}

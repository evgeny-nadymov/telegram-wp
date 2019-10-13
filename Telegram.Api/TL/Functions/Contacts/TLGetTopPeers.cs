// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Contacts
{
    [Flags]
    public enum GetTopPeersFlags
    {
        Correspondents = 0x1,
        BotsPM = 0x2,
        BotsInline = 0x4,
        Groups = 0x400,
        Channels = 0x8000,
    }

    class TLGetTopPeers : TLObject
    {
        public const uint Signature = 0xd4982db5;

        public TLInt Flags { get; set; }

        public TLInt Offset { get; set; }

        public TLInt Limit { get; set; }

        public TLInt Hash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Offset.ToBytes(),
                Limit.ToBytes(),
                Hash.ToBytes());
        }
    }
}

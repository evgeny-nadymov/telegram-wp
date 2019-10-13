// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL.Functions.Messages
{
    public class TLStartBot : TLObject, IRandomId
    {
        public const uint Signature = 0xe6df7378;

        public TLInputUserBase Bot { get; set; }

        public TLInputPeerBase Peer { get; set; }

        public TLLong RandomId { get; set; }

        public TLString StartParam { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Bot.ToBytes(),
                Peer.ToBytes(),
                RandomId.ToBytes(),
                StartParam.ToBytes()
            );
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Bot.ToStream(output);
            Peer.ToStream(output);
            RandomId.ToStream(output);
            StartParam.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Bot = GetObject<TLInputUserBase>(input);
            Peer = GetObject<TLInputPeerBase>(input);
            RandomId = GetObject<TLLong>(input);
            StartParam = GetObject<TLString>(input);

            return this;
        }
    }
}

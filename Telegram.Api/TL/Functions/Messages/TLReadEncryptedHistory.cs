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
    public class TLReadEncryptedHistory : TLObject, IRandomId
    {
        public const uint Signature = 0x7f4b690a;

        public TLInputEncryptedChat Peer { get; set; }

        public TLInt MaxDate { get; set; }

        public TLLong RandomId { get; set; }

        public TLReadEncryptedHistory()
        {
            RandomId = TLLong.Random();
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Peer.ToBytes(),
                MaxDate.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Peer.ToStream(output);
            MaxDate.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLInputEncryptedChat>(input);
            MaxDate = GetObject<TLInt>(input);

            return this;
        }
    }
}
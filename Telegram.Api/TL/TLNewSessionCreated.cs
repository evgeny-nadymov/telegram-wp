// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    internal class TLNewSessionCreated : TLObject
    {
        public const uint Signature = TLConstructors.TLNewSessionCreated;

        public TLLong FirstMessageId { get; set; }

        public TLLong UniqueId { get; set; }

        public TLLong ServerSalt { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            FirstMessageId = GetObject<TLLong>(bytes, ref position);
            UniqueId = GetObject<TLLong>(bytes, ref position);
            ServerSalt = GetObject<TLLong>(bytes, ref position);

            return this;
        }
    }
}
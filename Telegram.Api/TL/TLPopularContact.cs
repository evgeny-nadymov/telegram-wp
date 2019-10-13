// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLPopularContact : TLObject
    {
        public const uint Signature = TLConstructors.TLPopularContact;

        public TLLong ClientId { get; set; }

        public TLInt Importers { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ClientId = GetObject<TLLong>(bytes, ref position);
            Importers = GetObject<TLInt>(bytes, ref position);

            return this;
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Langpack
{
    class TLGetStrings : TLObject
    {
        public const uint Signature = 0x2e1ee318;

        public TLString LangCode { get; set; }

        public TLVector<TLString> Keys { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                LangCode.ToBytes(),
                Keys.ToBytes());
        }
    }
}

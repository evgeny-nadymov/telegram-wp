// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLExportedMessageLink : TLObject
    {
        public const uint Signature = TLConstructors.TLExportedMessageLink;

        public TLString Link { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Link = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }
    public class TLExportedMessageLink74 : TLExportedMessageLink
    {
        public new const uint Signature = TLConstructors.TLExportedMessageLink74;

        public TLString Html { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Link = GetObject<TLString>(bytes, ref position);
            Html = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }
}

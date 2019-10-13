// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLWebFile : TLFile
    {
        public new const uint Signature = TLConstructors.TLWebFile;

        public TLInt Size { get; set; }

        public TLString MimeType { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Size = GetObject<TLInt>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            Type = GetObject<TLFileTypeBase>(bytes, ref position);
            MTime = GetObject<TLInt>(bytes, ref position);
            Bytes = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }
}

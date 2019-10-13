// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLSentEncryptedFile : TLObject
    {
        public const uint Signature = TLConstructors.TLSentEncryptedFile;

        public TLInt Date { get; set; }

        public TLEncryptedFileBase EncryptedFile { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Date = GetObject<TLInt>(bytes, ref position);
            EncryptedFile = GetObject<TLEncryptedFileBase>(bytes, ref position);

            return this;
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLInputSecureFileBase : TLObject { }

    public class TLInputSecureFileUploaded : TLInputSecureFileBase
    {
        public const uint Signature = TLConstructors.TLInputSecureFileUploaded;

        public TLLong Id { get; set; }

        public TLInt Parts { get; set; }

        public TLString MD5Checksum { get; set; }

        public TLString FileHash { get; set; }

        public TLString Secret { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Parts.ToBytes(),
                MD5Checksum.ToBytes(),
                FileHash.ToBytes(),
                Secret.ToBytes());
        }
    }

    public class TLInputSecureFile : TLInputSecureFileBase
    {
        public const uint Signature = TLConstructors.TLInputSecureFile;

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes());
        }


    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLCdnFileBase : TLObject { }

    public class TLCdnFile : TLCdnFileBase
    {
        public const uint Signature = TLConstructors.TLCdnFile;

        public TLString Bytes { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Bytes = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLCdnFileReuploadNeeded : TLCdnFileBase
    {
        public const uint Signature = TLConstructors.TLCdnFileReuploadNeeded;

        public TLString RequestToken { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            RequestToken = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }
}

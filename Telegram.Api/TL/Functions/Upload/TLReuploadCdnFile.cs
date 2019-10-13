// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Upload
{
    public class TLReuploadCdnFile : TLObject
    {
        public const uint Signature = 0x9b2754a8;

        public TLString FileToken { get; set; }

        public TLString RequestToken { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                FileToken.ToBytes(),
                RequestToken.ToBytes());
        }
    }
}

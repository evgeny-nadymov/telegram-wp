// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Auth
{
    public class TLImportAuthorization : TLObject
    {
        public const string Signature = "#e3ef9613";

        public TLInt Id { get; set; }

        public TLString Bytes { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Bytes.ToBytes());
        }
    }
}

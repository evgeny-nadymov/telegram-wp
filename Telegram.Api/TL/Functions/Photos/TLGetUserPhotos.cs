// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Photos
{
    class TLGetUserPhotos : TLObject
    {
        public const string Signature = "#91cd32a8";

        public TLInputUserBase UserId { get; set; }

        public TLInt Offset { get; set; }

        public TLLong MaxId { get; set; }

        public TLInt Limit { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                UserId.ToBytes(),
                Offset.ToBytes(),
                MaxId.ToBytes(),
                Limit.ToBytes());
        }
    }
}

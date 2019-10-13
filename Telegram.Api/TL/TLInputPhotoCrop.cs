// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLInputPhotoCropBase : TLObject { }

    public class TLInputPhotoCropAuto : TLInputPhotoCropBase
    {
        public const uint Signature = TLConstructors.TLInputPhotoCropAuto;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputPhotoCrop : TLInputPhotoCropBase
    {
        public const uint Signature = TLConstructors.TLInputPhotoCrop;

        public TLDouble CropLeft { get; set; }

        public TLDouble CropTop { get; set; }

        public TLDouble CropWidth { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            CropLeft = GetObject<TLDouble>(bytes, ref position);
            CropTop = GetObject<TLDouble>(bytes, ref position);
            CropWidth = GetObject<TLDouble>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                CropLeft.ToBytes(),
                CropTop.ToBytes(),
                CropWidth.ToBytes());
        }
    }
}

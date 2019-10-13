// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLShippingOption : TLObject
    {
        public const uint Signature = TLConstructors.TLShippingOption;

        public TLString Id { get; set; }

        public TLString Title { get; set; }

        public TLVector<TLLabeledPrice> Prices { get; set; }

        #region Additional
        public bool IsSelected { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLString>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Prices = GetObject<TLVector<TLLabeledPrice>>(bytes, ref position);

            return this;
        }
    }
}
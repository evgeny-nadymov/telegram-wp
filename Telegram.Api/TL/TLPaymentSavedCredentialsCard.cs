// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public abstract class TLPaymentSavedCredentials : TLObject { }

    public class TLPaymentSavedCredentialsCard : TLPaymentSavedCredentials
    {
        public const uint Signature = TLConstructors.TLPaymentSavedCredentialsCard;

        public TLString Id { get; set; }

        public TLString Title { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLString>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Title.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLString>(input);
            Title = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(Title.ToBytes());
        }
    }
}

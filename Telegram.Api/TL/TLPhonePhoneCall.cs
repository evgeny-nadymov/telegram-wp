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
    public class TLPhonePhoneCall : TLObject
    {
        public const uint Signature = TLConstructors.TLPhonePhoneCall;

        public TLPhoneCallBase PhoneCall { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PhoneCall = GetObject<TLPhoneCallBase>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhoneCall.ToBytes(),
                Users.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            PhoneCall.ToStream(output);
            Users.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            PhoneCall = GetObject<TLPhoneCallBase>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);

            return this;
        }
    }
}

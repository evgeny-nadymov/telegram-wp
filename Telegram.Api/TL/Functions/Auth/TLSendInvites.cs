// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Auth
{
    public class TLSendInvites : TLObject
    {
        public const string Signature = "#771c1d97";

        public TLVector<TLString> PhoneNumbers { get; set; }

        public TLString Message { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhoneNumbers.ToBytes(),
                Message.ToBytes());
        }
    }
}

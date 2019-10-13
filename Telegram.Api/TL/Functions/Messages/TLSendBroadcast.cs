// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Messages
{
    public class TLSendBroadcast : TLObject
    {
        public const string Signature = "#bf73f4da";

        public TLVector<TLInputUserBase> Contacts { get; set; }

        public TLVector<TLLong> RandomId { get; set; }

        public TLString Message { get; set; }

        public TLInputMediaBase Media { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Contacts.ToBytes(),
                RandomId.ToBytes(),
                Message.ToBytes(),
                Media.ToBytes());
        }
    }
}

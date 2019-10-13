// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Channels
{
    class TLKickFromChannel : TLObject
    {
        public const uint Signature = 0xa672de14;

        public TLInputChannelBase Channel { get; set; }

        public TLInputUserBase UserId { get; set; }

        public TLBool Kicked { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Channel.ToBytes(),
                UserId.ToBytes(),
                Kicked.ToBytes());
        }
    }
}
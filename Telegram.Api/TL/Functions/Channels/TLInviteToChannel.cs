// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Channels
{
    class TLInviteToChannel : TLObject
    {
        public const uint Signature = 0x199f3a6c;

        public TLInputChannelBase Channel { get; set; }

        public TLVector<TLInputUserBase> Users { get; set; } 

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Channel.ToBytes(),
                Users.ToBytes());
        }
    }
}
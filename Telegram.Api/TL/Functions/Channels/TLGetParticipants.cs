// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Channels
{
    class TLGetParticipants : TLObject
    {
        public const uint Signature = 0x123e05e9;

        public TLInputChannelBase Channel { get; set; }

        public TLChannelParticipantsFilterBase Filter { get; set; }

        public TLInt Offset { get; set; }

        public TLInt Limit { get; set; }

        public TLInt Hash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Channel.ToBytes(),
                Filter.ToBytes(),
                Offset.ToBytes(),
                Limit.ToBytes(),
                Hash.ToBytes());
        }
    }
}

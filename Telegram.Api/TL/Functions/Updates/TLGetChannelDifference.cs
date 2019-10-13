// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Updates
{
    enum GetChannelDifferenceFlags
    {
        Force = 0x1
    }

    class TLGetChannelDifference : TLObject
    {
        public const uint Signature = 0xbb32d7c0;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInputChannelBase Channel { get; set; }

        public TLChannelMessagesFilerBase Filter { get; set; }

        public TLInt Pts { get; set; }

        public TLInt Limit { get; set; }

        public void SetForce()
        {
            Set(ref _flags, (int) GetChannelDifferenceFlags.Force);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Channel.ToBytes(),
                Filter.ToBytes(),
                Pts.ToBytes(),
                Limit.ToBytes());
        }
    }
}

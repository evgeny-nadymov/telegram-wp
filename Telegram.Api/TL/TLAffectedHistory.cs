// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLAffectedHistory : TLObject
    {
        public const uint Signature = TLConstructors.TLAffectedHistory;

        public TLInt Pts { get; set; }

        public TLInt Seq { get; set; }

        public TLInt Offset { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Pts = GetObject<TLInt>(bytes, ref position);
            Seq = GetObject<TLInt>(bytes, ref position);
            Offset = GetObject<TLInt>(bytes, ref position);

            return this;
        }
    }

    public class TLAffectedHistory24 : TLAffectedHistory, IMultiPts, IMultiChannelPts
    {
        public new const uint Signature = TLConstructors.TLAffectedHistory24;

        public TLInt PtsCount { get; set; }

        public TLInt ChannelPts
        {
            get { return Pts; }
            set { Pts = value; }
        }

        public TLInt ChannelPtsCount
        {
            get { return PtsCount; }
            set { PtsCount = value; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Pts = GetObject<TLInt>(bytes, ref position);
            PtsCount = GetObject<TLInt>(bytes, ref position);
            Offset = GetObject<TLInt>(bytes, ref position);

            return this;
        }
    }
}

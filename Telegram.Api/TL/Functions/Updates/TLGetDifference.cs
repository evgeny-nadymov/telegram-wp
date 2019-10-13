// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL.Functions.Updates
{
    enum GetDifferenceFlags
    {
        PtsTotalLimit = 0x1
    }

    class TLGetDifference : TLObject
    {
        public const uint Signature = 0x25939651;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInt Pts { get; set; }

        private TLInt _ptsTotalLimit;

        public TLInt PtsTotalLimit
        {
            get { return _ptsTotalLimit; }
            set { SetField(out _ptsTotalLimit, value, ref _flags, (int) GetDifferenceFlags.PtsTotalLimit); }
        }

        public TLInt Date { get; set; }

        public TLInt Qts { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Pts.ToBytes(),
                ToBytes(PtsTotalLimit, Flags, (int)GetDifferenceFlags.PtsTotalLimit),
                Date.ToBytes(),
                Qts.ToBytes());
        }
    }
}

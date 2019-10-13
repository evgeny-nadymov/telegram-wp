// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Messages
{
    [Flags]
    public enum SetInlineBotResultsFlags
    {
        Gallery = 0x1,          // 0
        Private = 0x2,          // 1
        NextOffset = 0x4,       // 2
        SwitchPM = 0x8,         // 3
    }

    class TLSetInlineBotResults : TLObject
    {
        public const uint Signature = 0xeb5ea206;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLBool _gallery;

        public TLBool Gallery
        {
            get { return _gallery; }
            set { SetField(out _gallery, value, ref _flags, (int) SetInlineBotResultsFlags.Gallery); }
        }

        private TLBool _private;

        public TLBool Private
        {
            get { return _private; }
            set { SetField(out _private, value, ref _flags, (int) SetInlineBotResultsFlags.Private); }
        }

        public TLLong QueryId { get; set; }

        public TLVector<TLInputBotInlineResult> Results { get; set; }

        public TLInt CacheTime { get; set; }

        private TLString _nextOffset;

        public TLString NextOffset
        {
            get { return _nextOffset; }
            set { SetField(out _nextOffset, value, ref _flags, (int)SetInlineBotResultsFlags.NextOffset); }
        }

        private TLInlineBotSwitchPM _switchPM;

        public TLInlineBotSwitchPM SwitchPM
        {
            get { return _switchPM; }
            set { SetField(out _switchPM, value, ref _flags, (int)SetInlineBotResultsFlags.NextOffset); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                QueryId.ToBytes(),
                Results.ToBytes(),
                CacheTime.ToBytes(),
                ToBytes(NextOffset, Flags, (int) SetInlineBotResultsFlags.NextOffset),
                ToBytes(SwitchPM, Flags, (int) SetInlineBotResultsFlags.SwitchPM));
        }
    }
}

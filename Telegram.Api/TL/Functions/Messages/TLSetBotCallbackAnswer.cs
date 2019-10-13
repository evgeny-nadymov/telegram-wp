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
    public enum SetBotCallbackAnswerFlags
    {
        Message = 0x1,          // 0
        Alert = 0x2,            // 1
    }

    class TLSetBotCallbackAnswer : TLObject
    {
        public const uint Signature = 0xa6e94f04;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInt QueryId { get; set; }

        private TLString _message;

        public TLString Message
        {
            get { return _message; }
            set { SetField(out _message, value, ref _flags, (int) SetBotCallbackAnswerFlags.Message); }
        }

        public void SetAlert()
        {
            Set(ref _flags, (int) SetBotCallbackAnswerFlags.Alert);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                QueryId.ToBytes(),
                Message.ToBytes());
        }
    }
}

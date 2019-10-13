// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL
{
    public abstract class TLInputPeerNotifyEventsBase : TLObject { }

    [Obsolete]
    public class TLInputPeerNotifyEventsEmpty : TLInputPeerNotifyEventsBase
    {
        public const uint Signature = TLConstructors.TLInputPeerNotifyEventsEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    [Obsolete]
    public class TLInputPeerNotifyEventsAll : TLInputPeerNotifyEventsBase
    {
        public const uint Signature = TLConstructors.TLInputPeerNotifyEventsAll;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }
}

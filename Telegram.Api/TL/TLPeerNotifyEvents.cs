// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Runtime.Serialization;

namespace Telegram.Api.TL
{
    [KnownType(typeof(TLPeerNotifyEventsEmpty))]
    [KnownType(typeof(TLPeerNotifyEventsAll))]
    [DataContract]
    public abstract class TLPeerNotifyEventsBase : TLObject { }

    [Obsolete]
    [DataContract]
    public class TLPeerNotifyEventsEmpty : TLPeerNotifyEventsBase
    {
        public const uint Signature = TLConstructors.TLPeerNotifyEventsEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }
    }

    [Obsolete]
    [DataContract]
    public class TLPeerNotifyEventsAll : TLPeerNotifyEventsBase
    {
        public const uint Signature = TLConstructors.TLPeerNotifyEventsAll;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }
    }
}

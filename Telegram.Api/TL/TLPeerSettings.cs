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
    [Flags]
    public enum PeerSettingsFlags
    {
        ReportSpam = 0x1,
    }

    public class TLPeerSettings : TLObject
    {
        public const uint Signature = TLConstructors.TLPeerSettings;

        public TLInt Flags { get; set; }

        public bool ReportSpam { get { return IsSet(Flags, (int)PeerSettingsFlags.ReportSpam); } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);

            return this;
        }
    }
}

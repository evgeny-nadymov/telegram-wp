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
    public enum AuthorizationFlags
    {
        TmpSessions = 0x1,          // 0
    }

    public class TLAuthorization : TLObject
    {
        public const uint Signature = TLConstructors.TLAuthorization;

        public TLInt Expires { get; set; }

        public TLUserBase User { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Expires = GetObject<TLInt>(bytes, ref position);
            User = GetObject<TLUserBase>(bytes, ref position);

            return this;
        }
    }

    public class TLAuthorization31 : TLAuthorization
    {
        public new const uint Signature = TLConstructors.TLAuthorization31;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            User = GetObject<TLUserBase>(bytes, ref position);

            return this;
        }
    }

    public class TLAuthorization55 : TLAuthorization
    {
        public new const uint Signature = TLConstructors.TLAuthorization55;

        public TLInt Flags { get; set; }

        public TLInt TempSessions { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            User = GetObject<TLUserBase>(bytes, ref position);
            TempSessions = GetObject<TLInt>(Flags, (int) AuthorizationFlags.TmpSessions, null, bytes, ref position);

            return this;
        }
    }
}

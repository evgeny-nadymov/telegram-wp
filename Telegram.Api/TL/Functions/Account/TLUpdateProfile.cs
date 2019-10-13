// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL.Functions.Account
{
    [Flags]
    public enum UpdateProfileFlags
    {
        FirstName = 0x1,    // 0
        LastName = 0x2,     // 1
        About = 0x4,        // 2
    }

    public class TLUpdateProfile : TLObject
    {
        public const uint Signature = 0x78515775;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLString _firstName;

        public TLString FirstName
        {
            get { return _firstName; }
            set { SetField(out _firstName, value, ref _flags, (int)UpdateProfileFlags.FirstName); }
        }

        private TLString _lastName;

        public TLString LastName
        {
            get { return _lastName; }
            set { SetField(out _lastName, value, ref _flags, (int)UpdateProfileFlags.LastName); }
        }

        private TLString _about;

        public TLString About
        {
            get { return _about; }
            set { SetField(out _about, value, ref _flags, (int)UpdateProfileFlags.About); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(FirstName, Flags, (int)UpdateProfileFlags.FirstName),
                ToBytes(LastName, Flags, (int)UpdateProfileFlags.LastName),
                ToBytes(About, Flags, (int)UpdateProfileFlags.About));
        }
    }
}

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
    public enum TermsOfServiceFlags
    {
        Popup = 0x1,
        MinAgeConfirm = 0x2
    }

    public abstract class TLTermsOfServiceBase : TLObject { }

    public class TLTermsOfService80 : TLTermsOfService
    {
        public new const uint Signature = TLConstructors.TLTermsOfService80;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Popup
        {
            get { return IsSet(_flags, (int)TermsOfServiceFlags.Popup); }
            set { SetUnset(ref _flags, value, (int)TermsOfServiceFlags.Popup); }
        }

        public TLDataJSON Id { get; set; }

        public TLVector<TLMessageEntityBase> Entities { get; set; }

        public bool MinAgeConfirm
        {
            get { return IsSet(_flags, (int)TermsOfServiceFlags.MinAgeConfirm); }
            set { SetUnset(ref _flags, value, (int)TermsOfServiceFlags.MinAgeConfirm); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            _flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLDataJSON>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);

            return this;
        }
    }

    public class TLTermsOfService : TLTermsOfServiceBase
    {
        public const uint Signature = TLConstructors.TLTermsOfService;

        public TLString Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }
}

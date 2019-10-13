// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLSavedPhoneContact : TLObject
    {
        public const uint Signature = TLConstructors.TLSavedPhoneContact;

        public TLString Phone { get; set; }

        public TLString FirstName { get; set; }

        public TLString LastName { get; set; }

        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Phone = GetObject<TLString>(bytes, ref position);
            FirstName = GetObject<TLString>(bytes, ref position);
            LastName = GetObject<TLString>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }
    }
}

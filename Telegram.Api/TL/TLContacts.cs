// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLContactsBase : TLObject
    {
        public abstract TLContactsBase GetEmptyObject();
    }

    public class TLContacts71 : TLContacts
    {
        public new const uint Signature = TLConstructors.TLContacts71;

        public TLInt SavedCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Contacts = GetObject<TLVector<TLContact>>(bytes, ref position);
            SavedCount = GetObject<TLInt>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLContactsBase GetEmptyObject()
        {
            return new TLContacts71
            {
                Contacts = new TLVector<TLContact>(Contacts.Count),
                SavedCount = SavedCount,
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }

        public override string ToString()
        {
            return string.Format("TLContacts contacts={0} saved_count={1} users={2}", Contacts.Count, SavedCount, Users.Count);
        }
    }

    public class TLContacts : TLContactsBase
    {
        public const uint Signature = TLConstructors.TLContacts;

        public TLVector<TLUserBase> Users { get; set; }

        public TLVector<TLContact> Contacts { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Contacts = GetObject<TLVector<TLContact>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLContactsBase GetEmptyObject()
        {
            return new TLContacts
            {
                Contacts = new TLVector<TLContact>(Contacts.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }

    public class TLContactsNotModified : TLContactsBase
    {
        public const uint Signature = TLConstructors.TLContactsNotModified;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override TLContactsBase GetEmptyObject()
        {
            return new TLContactsNotModified();
        }
    }
}

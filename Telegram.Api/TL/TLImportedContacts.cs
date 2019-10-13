// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLImportedContacts : TLObject
    {
        public const uint Signature = TLConstructors.TLImportedContacts;

        public TLVector<TLImportedContact> Imported { get; set; }

        public TLVector<TLLong> RetryContacts { get; set; } 

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Imported = GetObject<TLVector<TLImportedContact>>(bytes, ref position);
            RetryContacts = GetObject<TLVector<TLLong>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public virtual TLImportedContacts GetEmptyObject()
        {
            return new TLImportedContacts
            {
                Imported = new TLVector<TLImportedContact>(Imported.Count),
                RetryContacts = new TLVector<TLLong>(RetryContacts.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }
    }

    public class TLImportedContacts69 : TLImportedContacts
    {
        public new const uint Signature = TLConstructors.TLImportedContacts69;

        public TLVector<TLPopularContact> PopularInvites { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Imported = GetObject<TLVector<TLImportedContact>>(bytes, ref position);
            PopularInvites = GetObject<TLVector<TLPopularContact>>(bytes, ref position);
            RetryContacts = GetObject<TLVector<TLLong>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLImportedContacts GetEmptyObject()
        {
            return new TLImportedContacts69
            {
                Imported = new TLVector<TLImportedContact>(Imported.Count),
                PopularInvites = new TLVector<TLPopularContact>(PopularInvites.Count),
                RetryContacts = new TLVector<TLLong>(RetryContacts.Count),
                Users = new TLVector<TLUserBase>(Users.Count)
            };
        }

        public override string ToString()
        {
            return string.Format("TLImportedContacts imported={0} popular_invites={1} retry_contacts={2} users={3}", Imported.Count, PopularInvites.Count, RetryContacts.Count, Users.Count);
        }
    }
}

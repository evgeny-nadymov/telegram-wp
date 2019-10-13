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
    public enum UserFullFlags
    {
        Blocked = 0x1,          // 0
        About = 0x2,            // 1
        ProfilePhoto = 0x4,     // 2
        BotInfo = 0x8,          // 3
    }

    public class TLUserFull : TLObject
    {
        public const uint Signature = TLConstructors.TLUserFull;

        public TLUserBase User { get; set; }

        public TLLinkBase Link { get; set; }

        public TLPhotoBase ProfilePhoto { get; set; }

        public TLPeerNotifySettingsBase NotifySettings { get; set; }

        public virtual TLBool Blocked { get; set; }

        public TLString RealFirstName { get; set; }

        public TLString RealLastName { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            User = GetObject<TLUserBase>(bytes, ref position);
            Link = GetObject<TLLinkBase>(bytes, ref position);
            ProfilePhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            Blocked = GetObject<TLBool>(bytes, ref position);
            RealFirstName = GetObject<TLString>(bytes, ref position);
            RealLastName = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public virtual TLUserBase ToUser()
        {
            User.Link = Link;
            User.ProfilePhoto = ProfilePhoto;
            User.NotifySettings = NotifySettings;
            User.Blocked = Blocked;

            return User;
        }
    }

    public class TLUserFull31 : TLUserFull
    {
        public new const uint Signature = TLConstructors.TLUserFull31;

        public TLBotInfoBase BotInfo { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            User = GetObject<TLUserBase>(bytes, ref position);
            Link = GetObject<TLLinkBase>(bytes, ref position);
            ProfilePhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            Blocked = GetObject<TLBool>(bytes, ref position);
            BotInfo = GetObject<TLBotInfoBase>(bytes, ref position);

            return this;
        }

        public override TLUserBase ToUser()
        {
            User.Link = Link;
            User.ProfilePhoto = ProfilePhoto;
            User.NotifySettings = NotifySettings;
            User.Blocked = Blocked;
            User.BotInfo = BotInfo;

            return User;
        }
    }

    public class TLUserFull49 : TLUserFull31
    {
        public new const uint Signature = TLConstructors.TLUserFull49;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLBool Blocked
        {
            get { return new TLBool(IsSet(Flags, (int) UserFullFlags.Blocked)); }
            set { SetUnset(ref _flags, value != null && value.Value, (int) UserFullFlags.Blocked); }
        }

        public TLString About { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            User = GetObject<TLUserBase>(bytes, ref position);
            About = GetObject(Flags, (int)UserFullFlags.About, TLString.Empty, bytes, ref position);
            Link = GetObject<TLLinkBase>(bytes, ref position);
            ProfilePhoto = GetObject<TLPhotoBase>(Flags, (int)UserFullFlags.ProfilePhoto, null, bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            BotInfo = GetObject<TLBotInfoBase>(Flags, (int) UserFullFlags.BotInfo, new TLBotInfoEmpty(), bytes, ref position);

            return this;
        }

        public override TLUserBase ToUser()
        {
            User.Link = Link;
            var user45 = User as TLUser45;
            if (user45 != null) user45.About = About;
            if (ProfilePhoto != null) User.ProfilePhoto = ProfilePhoto;
            User.NotifySettings = NotifySettings;
            User.Blocked = Blocked;
            User.BotInfo = BotInfo;

            return User;
        }
    }

    public class TLUserFull58 : TLUserFull49
    {
        public new const uint Signature = TLConstructors.TLUserFull58;

        public TLInt CommonChatsCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            User = GetObject<TLUserBase>(bytes, ref position);
            About = GetObject(Flags, (int)UserFullFlags.About, TLString.Empty, bytes, ref position);
            Link = GetObject<TLLinkBase>(bytes, ref position);
            ProfilePhoto = GetObject<TLPhotoBase>(Flags, (int)UserFullFlags.ProfilePhoto, null, bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            BotInfo = GetObject<TLBotInfoBase>(Flags, (int)UserFullFlags.BotInfo, new TLBotInfoEmpty(), bytes, ref position);
            CommonChatsCount = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLUserBase ToUser()
        {
            User.Link = Link;
            var user45 = User as TLUser45;
            if (user45 != null)
            {
                user45.About = About;
                user45.CommonChatsCount = CommonChatsCount;
            }
            if (ProfilePhoto != null) User.ProfilePhoto = ProfilePhoto;
            User.NotifySettings = NotifySettings;
            User.Blocked = Blocked;
            User.BotInfo = BotInfo;

            return User;
        }
    }
}

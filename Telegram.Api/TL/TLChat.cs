// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.IO;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.TL.Interfaces;

namespace Telegram.Api.TL
{
    [Flags]
    public enum ChatFlags
    {
        Creator = 0x1,
        Kicked = 0x2,
        Left = 0x4,
        AdminsEnabled = 0x8,
        Admin = 0x10,
        Deactivated = 0x20,
        MigratedTo = 0x40
    }

    [Flags]
    public enum ChatCustomFlags
    {
        ReadInboxMaxId = 0x1,
        ReadOutboxMaxId = 0x2,
    }

    [Flags]
    public enum ChannelFlags
    {
        Creator = 0x1,
        Kicked = 0x2,
        Left = 0x4,
        Editor = 0x8,
        Moderator = 0x10,
        Broadcast = 0x20,
        Public = 0x40,
        Verified = 0x80,
        MegaGroup = 0x100,
        Restricted = 0x200,
        Democracy = 0x400,
        Signatures = 0x800,
        Min = 0x1000,
        AccessHash = 0x2000,
        AdminRights = 0x4000,
        BannedRights = 0x8000,
        UntilDate = 0x10000,
        ParticipantsCount = 0x20000,
        FeedId = 0x40000,
    }

    [Flags]
    public enum ChannelCustomFlags
    {
        MigratedFromChatId = 0x1,
        MigratedFromMaxId = 0x2,
        Silent = 0x4,
        PinnedMsgId = 0x8,
        HiddenPinnedMsgId = 0x10,
        ReadOutboxMaxId = 0x20,
        ReadInboxMaxId = 0x40,
        CanSetStickers = 0x80,
        StickerSet = 0x100,
        AvailableMinId = 0x200,
        HiddenPrehistory = 0x400
    }

    public abstract class TLChatBase : TLObject, IInputPeer, IFullName, INotifySettings
    {

        public static string ChatFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (ChatFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public static string ChannelFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (ChannelFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public static string ChannelCustomFlagsString(TLLong flags)
        {
            if (flags == null) return string.Empty;

            var list = (ChannelCustomFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public int Index
        {
            get { return Id.Value; }
            set { Id = new TLInt(value); }
        }

        public TLInt Id { get; set; }

        public virtual void Update(TLChatBase chat)
        {
            Id = chat.Id;

            if (chat.Participants != null)
            {
                Participants = chat.Participants;
            }

            if (chat.ChatPhoto != null)
            {
                ChatPhoto = chat.ChatPhoto;
            }

            if (chat.NotifySettings != null)
            {
                NotifySettings = chat.NotifySettings;
            }
        }

        public abstract TLInputPeerBase ToInputPeer();

        public abstract string GetUnsendedTextFileName();

        #region Full chat information

        public TLChatParticipantsBase Participants { get; set; }

        public TLPhotoBase ChatPhoto { get; set; }

        public TLPeerNotifySettingsBase NotifySettings { get; set; }

        public int UsersOnline { get; set; }

        public TLExportedChatInvite ExportedInvite { get; set; }
        #endregion

        public TLInputNotifyPeerBase ToInputNotifyPeer()
        {
            return new TLInputNotifyPeer { Peer = ToInputPeer() };
        }

        public abstract string FullName { get; }

        public virtual string FullName2 { get { return FullName; } }

        public virtual string ShortName { get { return FullName; } }

        public abstract bool IsForbidden { get; }

        #region Additional
        public IList<string> FullNameWords { get; set; }
        public TLVector<TLBotInfoBase> BotInfo { get; set; }

        #endregion
    }

    public class TLChatEmpty : TLChatBase
    {
        public const uint Signature = TLConstructors.TLChatEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
        }

        public override string FullName
        {
            get { return string.Empty; }
        }

        public override TLInputPeerBase ToInputPeer()
        {
            return new TLInputPeerChat { ChatId = Id };
        }

        public override string GetUnsendedTextFileName()
        {
            return "c" + Id + ".dat";
        }

        public override bool IsForbidden
        {
            get { return true; }
        }
    }

    public class TLChat : TLChatBase
    {
        public const uint Signature = TLConstructors.TLChat;

        protected TLString _title;

        public TLString Title
        {
            get { return _title; }
            set
            {
                SetField(ref _title, value, () => Title);
                NotifyOfPropertyChange(() => FullName);
            }
        }

        protected TLPhotoBase _photo;

        public TLPhotoBase Photo
        {
            get { return _photo; }
            set { SetField(ref _photo, value, () => Photo); }
        }

        public TLInt ParticipantsCount { get; set; }

        public TLInt Date { get; set; }

        public virtual TLBool Left { get; set; }

        public TLInt Version { get; set; }

        public override string ToString()
        {
            return Title.ToString();
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            _title = GetObject<TLString>(bytes, ref position);
            _photo = GetObject<TLPhotoBase>(bytes, ref position);
            ParticipantsCount = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Left = GetObject<TLBool>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            _title = GetObject<TLString>(input);
            _photo = GetObject<TLPhotoBase>(input);
            ParticipantsCount = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);
            Left = GetObject<TLBool>(input);
            Version = GetObject<TLInt>(input);

            Participants = GetObject<TLObject>(input) as TLChatParticipantsBase;
            NotifySettings = GetObject<TLObject>(input) as TLPeerNotifySettingsBase;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(Title.ToBytes());
            Photo.ToStream(output);
            output.Write(ParticipantsCount.ToBytes());
            output.Write(Date.ToBytes());
            output.Write(Left.ToBytes());
            output.Write(Version.ToBytes());

            Participants.NullableToStream(output);
            NotifySettings.NullableToStream(output);
        }

        public override string FullName
        {
            get { return Title != null ? Title.ToString() : string.Empty; }
        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);
            var c = chat as TLChat;
            if (c != null)
            {
                _title = c.Title;
                if (Photo.GetType() != c.Photo.GetType())
                {
                    _photo = c.Photo;    // при удалении фото чата не обновляется UI при _photo = c.Photo
                }
                else
                {
                    Photo.Update(c.Photo);
                }
                ParticipantsCount = c.ParticipantsCount;
                Date = c.Date;
                Left = c.Left;
                Version = c.Version;
            }
        }

        public override TLInputPeerBase ToInputPeer()
        {
            return new TLInputPeerChat { ChatId = Id };
        }

        public override string GetUnsendedTextFileName()
        {
            return "c" + Id + ".dat";
        }

        public override bool IsForbidden
        {
            get { return Left.Value; }
        }
    }

    public class TLChat40 : TLChat
    {
        public new const uint Signature = TLConstructors.TLChat40;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLBool Left
        {
            get { return new TLBool(IsSet(_flags, (int)ChatFlags.Left)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)ChatFlags.Left);
                }
            }
        }

        public bool Creator
        {
            get { return IsSet(_flags, (int)ChatFlags.Creator); }
        }

        public TLBool Admin
        {
            get { return new TLBool(IsSet(_flags, (int)ChatFlags.Admin)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)ChatFlags.Admin);
                }
            }
        }

        public TLBool AdminsEnabled
        {
            get { return new TLBool(IsSet(_flags, (int)ChatFlags.AdminsEnabled)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)ChatFlags.AdminsEnabled);
                }
            }
        }

        public bool Deactivated
        {
            get { return IsSet(_flags, (int)ChatFlags.Deactivated); }
        }

        protected TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            _title = GetObject<TLString>(bytes, ref position);
            _photo = GetObject<TLPhotoBase>(bytes, ref position);
            ParticipantsCount = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            //Left = GetObject<TLBool>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            _title = GetObject<TLString>(input);
            _photo = GetObject<TLPhotoBase>(input);
            ParticipantsCount = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);
            //Left = GetObject<TLBool>(input);
            Version = GetObject<TLInt>(input);

            Participants = GetObject<TLObject>(input) as TLChatParticipantsBase;
            NotifySettings = GetObject<TLObject>(input) as TLPeerNotifySettingsBase;

            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(Title.ToBytes());
            Photo.ToStream(output);
            output.Write(ParticipantsCount.ToBytes());
            output.Write(Date.ToBytes());
            //output.Write(Left.ToBytes());
            output.Write(Version.ToBytes());

            Participants.NullableToStream(output);
            NotifySettings.NullableToStream(output);

            CustomFlags.NullableToStream(output);
        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);
            var c = chat as TLChat40;
            if (c != null)
            {
                Flags = c.Flags;
                if (c.CustomFlags != null)
                {
                    CustomFlags = c.CustomFlags;
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() + " flags=" + ChatFlagsString(Flags);
        }
    }

    public class TLChat41 : TLChat40, IReadMaxId
    {
        public new const uint Signature = TLConstructors.TLChat41;

        public bool IsMigrated { get { return IsSet(Flags, (int)ChatFlags.MigratedTo); } }

        private TLInputChannelBase _migratedTo;

        public TLInputChannelBase MigratedTo
        {
            get { return _migratedTo; }
            set { SetField(out _migratedTo, value, ref _flags, (int)ChatFlags.MigratedTo); }
        }

        private TLInt _readInboxMaxId;

        public TLInt ReadInboxMaxId
        {
            get { return _readInboxMaxId; }
            set { SetField(out _readInboxMaxId, value, ref _customFlags, (int)ChatCustomFlags.ReadInboxMaxId); }
        }

        private TLInt _readOutboxMaxId;

        public TLInt ReadOutboxMaxId
        {
            get { return _readOutboxMaxId; }
            set { SetField(out _readOutboxMaxId, value, ref _customFlags, (int)ChatCustomFlags.ReadOutboxMaxId); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            _title = GetObject<TLString>(bytes, ref position);
            _photo = GetObject<TLPhotoBase>(bytes, ref position);
            ParticipantsCount = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            //Left = GetObject<TLBool>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);
            _migratedTo = GetObject<TLInputChannelBase>(Flags, (int)ChatFlags.MigratedTo, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            _title = GetObject<TLString>(input);
            _photo = GetObject<TLPhotoBase>(input);
            ParticipantsCount = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);
            //Left = GetObject<TLBool>(input);
            Version = GetObject<TLInt>(input);
            _migratedTo = GetObject<TLInputChannelBase>(Flags, (int)ChatFlags.MigratedTo, null, input);

            Participants = GetObject<TLObject>(input) as TLChatParticipantsBase;
            NotifySettings = GetObject<TLObject>(input) as TLPeerNotifySettingsBase;

            CustomFlags = GetNullableObject<TLLong>(input);

            _readInboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChatCustomFlags.ReadInboxMaxId, null, input);
            _readOutboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChatCustomFlags.ReadOutboxMaxId, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            output.Write(Id.ToBytes());
            output.Write(Title.ToBytes());
            Photo.ToStream(output);
            output.Write(ParticipantsCount.ToBytes());
            output.Write(Date.ToBytes());
            //output.Write(Left.ToBytes());
            output.Write(Version.ToBytes());
            ToStream(output, MigratedTo, Flags, (int)ChatFlags.MigratedTo);

            Participants.NullableToStream(output);
            NotifySettings.NullableToStream(output);

            CustomFlags.NullableToStream(output);

            ToStream(output, ReadInboxMaxId, CustomFlags, (int)ChatCustomFlags.ReadInboxMaxId);
            ToStream(output, ReadOutboxMaxId, CustomFlags, (int)ChatCustomFlags.ReadOutboxMaxId);
        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);

            var chat41 = chat as TLChat41;
            if (chat41 != null)
            {
                if (chat41.MigratedTo != null)
                {
                    MigratedTo = chat41.MigratedTo;
                }

                if (chat41.ReadInboxMaxId != null
                    && (ReadInboxMaxId == null || ReadInboxMaxId.Value < chat41.ReadInboxMaxId.Value))
                {
                    ReadInboxMaxId = chat41.ReadInboxMaxId;
                }

                if (chat41.ReadOutboxMaxId != null
                    && (ReadOutboxMaxId == null || ReadOutboxMaxId.Value < chat41.ReadOutboxMaxId.Value))
                {
                    ReadOutboxMaxId = chat41.ReadOutboxMaxId;
                }
            }
        }
    }

    public class TLChatForbidden : TLChatBase
    {
        public const uint Signature = TLConstructors.TLChatForbidden;

        public TLString Title { get; set; }

        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            Title = GetObject<TLString>(input);
            Date = GetObject<TLInt>(input);

            Participants = GetObject<TLObject>(input) as TLChatParticipantsBase;
            NotifySettings = GetObject<TLObject>(input) as TLPeerNotifySettingsBase;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(Title.ToBytes());
            output.Write(Date.ToBytes());

            Participants.NullableToStream(output);
            NotifySettings.NullableToStream(output);
        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);
            var c = (TLChatForbidden)chat;
            Title = c.Title;
            Date = c.Date;
        }

        public override string FullName
        {
            get
            {
                return Title != null ? Title.ToString() : string.Empty;
            }
        }

        public override TLInputPeerBase ToInputPeer()
        {
            return new TLInputPeerChat { ChatId = Id };
        }

        public override string GetUnsendedTextFileName()
        {
            return "c" + Id + ".dat";
        }

        public override bool IsForbidden
        {
            get { return true; }
        }
    }

    public class TLChatForbidden40 : TLChatForbidden, IReadMaxId
    {
        public new const uint Signature = TLConstructors.TLChatForbidden40;

        protected TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        private TLInt _readInboxMaxId;

        public TLInt ReadInboxMaxId
        {
            get { return _readInboxMaxId; }
            set { SetField(out _readInboxMaxId, value, ref _customFlags, (int)ChatCustomFlags.ReadInboxMaxId); }
        }

        private TLInt _readOutboxMaxId;

        public TLInt ReadOutboxMaxId
        {
            get { return _readOutboxMaxId; }
            set { SetField(out _readOutboxMaxId, value, ref _customFlags, (int)ChatCustomFlags.ReadOutboxMaxId); }
        }

        #region Additional
        public TLPhotoBase Photo { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            //Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            Title = GetObject<TLString>(input);
            //Date = GetObject<TLInt>(input);

            Participants = GetObject<TLObject>(input) as TLChatParticipantsBase;
            NotifySettings = GetObject<TLObject>(input) as TLPeerNotifySettingsBase;

            CustomFlags = GetNullableObject<TLLong>(input);

            _readInboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChatCustomFlags.ReadInboxMaxId, null, input);
            _readOutboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChatCustomFlags.ReadOutboxMaxId, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(Title.ToBytes());
            //output.Write(Date.ToBytes());

            Participants.NullableToStream(output);
            NotifySettings.NullableToStream(output);

            CustomFlags.NullableToStream(output);

            ToStream(output, ReadInboxMaxId, CustomFlags, (int)ChatCustomFlags.ReadInboxMaxId);
            ToStream(output, ReadOutboxMaxId, CustomFlags, (int)ChatCustomFlags.ReadOutboxMaxId);
        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);
            var c = chat as TLChatForbidden40;
            if (c != null)
            {
                if (c.ReadInboxMaxId != null
                    && (ReadInboxMaxId == null || ReadInboxMaxId.Value < c.ReadInboxMaxId.Value))
                {
                    ReadInboxMaxId = c.ReadInboxMaxId;
                }

                if (c.ReadOutboxMaxId != null
                    && (ReadOutboxMaxId == null || ReadOutboxMaxId.Value < c.ReadOutboxMaxId.Value))
                {
                    ReadOutboxMaxId = c.ReadOutboxMaxId;
                }
            }
        }
    }

    public class TLBroadcastChat : TLChatBase
    {
        public const uint Signature = TLConstructors.TLBroadcastChat;

        public TLVector<TLInt> ParticipantIds { get; set; }

        protected TLString _title;

        public TLString Title
        {
            get { return _title; }
            set
            {
                SetField(ref _title, value, () => Title);
                NotifyOfPropertyChange(() => FullName);
            }
        }

        public TLPhotoBase _photo;

        public TLPhotoBase Photo
        {
            get { return _photo; }
            set { SetField(ref _photo, value, () => Photo); }
        }

        public override string FullName
        {
            get { return Title != null ? Title.ToString() : string.Empty; }
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            _title = GetObject<TLString>(input);
            _photo = GetObject<TLPhotoBase>(input);
            ParticipantIds = GetObject<TLVector<TLInt>>(input);

            Participants = GetObject<TLObject>(input) as TLChatParticipantsBase;
            NotifySettings = GetObject<TLObject>(input) as TLPeerNotifySettingsBase;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(Title.ToBytes());
            Photo.ToStream(output);
            ParticipantIds.ToStream(output);

            Participants.NullableToStream(output);
            NotifySettings.NullableToStream(output);
        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);
            var c = chat as TLBroadcastChat;
            if (c != null)
            {
                _title = c.Title;
                if (Photo.GetType() != c.Photo.GetType())
                {
                    _photo = c.Photo;
                }
                else
                {
                    Photo.Update(c.Photo);
                }
                ParticipantIds = c.ParticipantIds;
            }
        }

        public override TLInputPeerBase ToInputPeer()
        {
            return new TLInputPeerBroadcast { ChatId = Id };
        }

        public override string GetUnsendedTextFileName()
        {
            return "b" + Id + ".dat";
        }

        public override bool IsForbidden
        {
            get { return false; }
        }
    }

    public class TLChannel76 : TLChannel73
    {
        public new const uint Signature = TLConstructors.TLChannel76;

        protected TLInt _feedId;

        public TLInt FeedId
        {
            get { return _feedId; }
            set { SetField(out _feedId, value, ref _flags, (int)ChannelFlags.FeedId); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject(Flags, (int)ChannelFlags.AccessHash, new TLLong(0), bytes, ref position);
            _title = GetObject<TLString>(bytes, ref position);
            UserName = GetObject<TLString>(Flags, (int)ChannelFlags.Public, TLString.Empty, bytes, ref position);
            _photo = GetObject<TLPhotoBase>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);
            RestrictionReason = GetObject<TLString>(Flags, (int)ChannelFlags.Restricted, TLString.Empty, bytes, ref position);
            _adminRights = GetObject<TLChannelAdminRights>(Flags, (int)ChannelFlags.AdminRights, null, bytes, ref position);
            _bannedRights = GetObject<TLChannelBannedRights>(Flags, (int)ChannelFlags.BannedRights, null, bytes, ref position);
            _participantsCount = GetObject<TLInt>(Flags, (int)ChannelFlags.ParticipantsCount, null, bytes, ref position);
            _feedId = GetObject<TLInt>(Flags, (int)ChannelFlags.FeedId, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject(Flags, (int)ChannelFlags.AccessHash, new TLLong(0), input);
            _title = GetObject<TLString>(input);
            _userName = GetObject<TLString>(Flags, (int)ChannelFlags.Public, TLString.Empty, input);
            _photo = GetObject<TLPhotoBase>(input);
            Date = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);
            RestrictionReason = GetObject<TLString>(Flags, (int)ChannelFlags.Restricted, TLString.Empty, input);
            _adminRights = GetObject<TLChannelAdminRights>(Flags, (int)ChannelFlags.AdminRights, null, input);
            _bannedRights = GetObject<TLChannelBannedRights>(Flags, (int)ChannelFlags.BannedRights, null, input);
            _participantsCount = GetObject<TLInt>(Flags, (int)ChannelFlags.ParticipantsCount, null, input);
            _feedId = GetObject<TLInt>(Flags, (int)ChannelFlags.FeedId, null, input);

            CustomFlags = GetNullableObject<TLLong>(input);

            ParticipantIds = GetNullableObject<TLVector<TLInt>>(input);
            About = GetNullableObject<TLString>(input);
            AdminsCount = GetNullableObject<TLInt>(input);
            KickedCount = GetNullableObject<TLInt>(input);
            ReadInboxMaxId = GetNullableObject<TLInt>(input);
            Pts = GetNullableObject<TLInt>(input);
            Participants = GetNullableObject<TLChatParticipantsBase>(input);
            NotifySettings = GetNullableObject<TLPeerNotifySettingsBase>(input);

            _migratedFromChatId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId, null, input);
            _migratedFromMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId, null, input);
            _pinnedMsgId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.PinnedMsgId, null, input);
            _hiddenPinnedMsgId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.HiddenPinnedMsgId, null, input);
            _readOutboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId, null, input);
            _stickerSet = GetObject<TLStickerSetBase>(CustomFlags, (int)ChannelCustomFlags.StickerSet, null, input);
            _availableMinId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.AvailableMinId, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            try
            {
                output.Write(TLUtils.SignatureToBytes(Signature));

                output.Write(Flags.ToBytes());
                output.Write(Id.ToBytes());
                ToStream(output, AccessHash, Flags, (int)ChannelFlags.AccessHash);
                output.Write(Title.ToBytes());
                ToStream(output, UserName, Flags, (int)ChannelFlags.Public);
                Photo.ToStream(output);
                Date.ToStream(output);
                Version.ToStream(output);
                ToStream(output, RestrictionReason, Flags, (int)ChannelFlags.Restricted);
                ToStream(output, AdminRights, Flags, (int)ChannelFlags.AdminRights);
                ToStream(output, BannedRights, Flags, (int)ChannelFlags.BannedRights);
                ToStream(output, ParticipantsCount, Flags, (int)ChannelFlags.ParticipantsCount);
                ToStream(output, FeedId, Flags, (int)ChannelFlags.FeedId);

                CustomFlags.NullableToStream(output);

                ParticipantIds.NullableToStream(output);
                About.NullableToStream(output);
                AdminsCount.NullableToStream(output);
                KickedCount.NullableToStream(output);
                ReadInboxMaxId.NullableToStream(output);
                Pts.NullableToStream(output);
                Participants.NullableToStream(output);
                NotifySettings.NullableToStream(output);

                ToStream(output, _migratedFromChatId, CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId);
                ToStream(output, _migratedFromMaxId, CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId);
                ToStream(output, PinnedMsgId, CustomFlags, (int)ChannelCustomFlags.PinnedMsgId);
                ToStream(output, HiddenPinnedMsgId, CustomFlags, (int)ChannelCustomFlags.HiddenPinnedMsgId);
                ToStream(output, ReadOutboxMaxId, CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId);
                ToStream(output, _stickerSet, CustomFlags, (int)ChannelCustomFlags.StickerSet);
                ToStream(output, _availableMinId, CustomFlags, (int)ChannelCustomFlags.AvailableMinId);
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(
                    string.Format(
                        "TLChannel76.ToStream access_hash={0} user_name={1} restriction_reason={2} migrated_from_chat_id={3} migrated_from_max_id={4} pinned_msg_id={5} hidden_pinned_msg_id={9} flags={6} custom_flags={7} ex {8}",
                        AccessHash, UserName, RestrictionReason, MigratedFromChatId, MigratedFromMaxId, PinnedMsgId,
                        ChannelFlagsString(Flags), ChannelCustomFlagsString(CustomFlags), HiddenPinnedMsgId, ex));
            }

        }

        public override void Update(TLChatBase chat)
        {
            var c = chat as TLChannel76;
            if (c != null)
            {
                FeedId = c.FeedId;
            }

            base.Update(chat);
        }
    }

    public class TLChannel73 : TLChannel68
    {
        public new const uint Signature = TLConstructors.TLChannel73;

        protected TLInt _participantsCount;

        public override TLInt ParticipantsCount
        {
            get { return _participantsCount; }
            set { SetField(out _participantsCount, value, ref _flags, (int)ChannelFlags.ParticipantsCount); }
        }

        public override bool CanPinMessages
        {
            get
            {
                if (IsMegaGroup)
                {
                    return Creator || (AdminRights != null && AdminRights.PinMessages);
                }

                return Creator || (AdminRights != null && AdminRights.EditMessages);
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject(Flags, (int)ChannelFlags.AccessHash, new TLLong(0), bytes, ref position);
            _title = GetObject<TLString>(bytes, ref position);
            UserName = GetObject<TLString>(Flags, (int)ChannelFlags.Public, TLString.Empty, bytes, ref position);
            _photo = GetObject<TLPhotoBase>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);
            RestrictionReason = GetObject<TLString>(Flags, (int)ChannelFlags.Restricted, TLString.Empty, bytes, ref position);
            _adminRights = GetObject<TLChannelAdminRights>(Flags, (int)ChannelFlags.AdminRights, null, bytes, ref position);
            _bannedRights = GetObject<TLChannelBannedRights>(Flags, (int)ChannelFlags.BannedRights, null, bytes, ref position);
            _participantsCount = GetObject<TLInt>(Flags, (int)ChannelFlags.ParticipantsCount, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject(Flags, (int)ChannelFlags.AccessHash, new TLLong(0), input);
            _title = GetObject<TLString>(input);
            _userName = GetObject<TLString>(Flags, (int)ChannelFlags.Public, TLString.Empty, input);
            _photo = GetObject<TLPhotoBase>(input);
            Date = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);
            RestrictionReason = GetObject<TLString>(Flags, (int)ChannelFlags.Restricted, TLString.Empty, input);
            _adminRights = GetObject<TLChannelAdminRights>(Flags, (int)ChannelFlags.AdminRights, null, input);
            _bannedRights = GetObject<TLChannelBannedRights>(Flags, (int)ChannelFlags.BannedRights, null, input);
            _participantsCount = GetObject<TLInt>(Flags, (int)ChannelFlags.ParticipantsCount, null, input);

            CustomFlags = GetNullableObject<TLLong>(input);

            ParticipantIds = GetNullableObject<TLVector<TLInt>>(input);
            About = GetNullableObject<TLString>(input);
            AdminsCount = GetNullableObject<TLInt>(input);
            KickedCount = GetNullableObject<TLInt>(input);
            ReadInboxMaxId = GetNullableObject<TLInt>(input);
            Pts = GetNullableObject<TLInt>(input);
            Participants = GetNullableObject<TLChatParticipantsBase>(input);
            NotifySettings = GetNullableObject<TLPeerNotifySettingsBase>(input);

            _migratedFromChatId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId, null, input);
            _migratedFromMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId, null, input);
            _pinnedMsgId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.PinnedMsgId, null, input);
            _hiddenPinnedMsgId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.HiddenPinnedMsgId, null, input);
            _readOutboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId, null, input);
            _stickerSet = GetObject<TLStickerSetBase>(CustomFlags, (int)ChannelCustomFlags.StickerSet, null, input);
            _availableMinId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.AvailableMinId, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            try
            {
                output.Write(TLUtils.SignatureToBytes(Signature));

                output.Write(Flags.ToBytes());
                output.Write(Id.ToBytes());
                ToStream(output, AccessHash, Flags, (int)ChannelFlags.AccessHash);
                output.Write(Title.ToBytes());
                ToStream(output, UserName, Flags, (int)ChannelFlags.Public);
                Photo.ToStream(output);
                Date.ToStream(output);
                Version.ToStream(output);
                ToStream(output, RestrictionReason, Flags, (int)ChannelFlags.Restricted);
                ToStream(output, AdminRights, Flags, (int)ChannelFlags.AdminRights);
                ToStream(output, BannedRights, Flags, (int)ChannelFlags.BannedRights);
                ToStream(output, ParticipantsCount, Flags, (int)ChannelFlags.ParticipantsCount);

                CustomFlags.NullableToStream(output);

                ParticipantIds.NullableToStream(output);
                About.NullableToStream(output);
                AdminsCount.NullableToStream(output);
                KickedCount.NullableToStream(output);
                ReadInboxMaxId.NullableToStream(output);
                Pts.NullableToStream(output);
                Participants.NullableToStream(output);
                NotifySettings.NullableToStream(output);

                ToStream(output, _migratedFromChatId, CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId);
                ToStream(output, _migratedFromMaxId, CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId);
                ToStream(output, PinnedMsgId, CustomFlags, (int)ChannelCustomFlags.PinnedMsgId);
                ToStream(output, HiddenPinnedMsgId, CustomFlags, (int)ChannelCustomFlags.HiddenPinnedMsgId);
                ToStream(output, ReadOutboxMaxId, CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId);
                ToStream(output, _stickerSet, CustomFlags, (int)ChannelCustomFlags.StickerSet);
                ToStream(output, _availableMinId, CustomFlags, (int)ChannelCustomFlags.AvailableMinId);
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(
                    string.Format(
                        "TLChannel68.ToStream access_hash={0} user_name={1} restriction_reason={2} migrated_from_chat_id={3} migrated_from_max_id={4} pinned_msg_id={5} hidden_pinned_msg_id={9} flags={6} custom_flags={7} ex {8}",
                        AccessHash, UserName, RestrictionReason, MigratedFromChatId, MigratedFromMaxId, PinnedMsgId,
                        ChannelFlagsString(Flags), ChannelCustomFlagsString(CustomFlags), HiddenPinnedMsgId, ex));
            }

        }

        public override void Update(TLChatBase chat)
        {
            var c = chat as TLChannel73;
            if (c != null)
            {
                if (c.ParticipantsCount != null)
                {
                    ParticipantsCount = c.ParticipantsCount;
                }
            }

            base.Update(chat);
        }
    }

    public class TLChannel68 : TLChannel49
    {
        public new const uint Signature = TLConstructors.TLChannel68;

        protected TLChannelAdminRights _adminRights;

        public TLChannelAdminRights AdminRights
        {
            get { return _adminRights; }
            set { SetField(out _adminRights, value, ref _flags, (int)ChannelFlags.AdminRights); }
        }

        protected TLChannelBannedRights _bannedRights;

        public TLChannelBannedRights BannedRights
        {
            get { return _bannedRights; }
            set { SetField(out _bannedRights, value, ref _flags, (int)ChannelFlags.BannedRights); }
        }

        public bool CanSetStickers
        {
            get { return IsSet(CustomFlags, (int)ChannelCustomFlags.CanSetStickers); }
            set { SetUnset(ref _customFlags, value, (int)ChannelCustomFlags.CanSetStickers); }
        }

        protected TLStickerSetBase _stickerSet;

        public TLStickerSetBase StickerSet
        {
            get { return _stickerSet; }
            set { SetField(out _stickerSet, value, ref _customFlags, (int)ChannelCustomFlags.StickerSet); }
        }

        public bool HiddenPrehistory
        {
            get { return IsSet(CustomFlags, (int)ChannelCustomFlags.HiddenPrehistory); }
            set { SetUnset(ref _customFlags, value, (int)ChannelCustomFlags.HiddenPrehistory); }
        }

        protected TLInt _availableMinId;

        public TLInt AvailableMinId
        {
            get { return _availableMinId; }
            set { SetField(out _availableMinId, value, ref _customFlags, (int)ChannelCustomFlags.AvailableMinId); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject(Flags, (int)ChannelFlags.AccessHash, new TLLong(0), bytes, ref position);
            _title = GetObject<TLString>(bytes, ref position);
            UserName = GetObject<TLString>(Flags, (int)ChannelFlags.Public, TLString.Empty, bytes, ref position);
            _photo = GetObject<TLPhotoBase>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);
            RestrictionReason = GetObject<TLString>(Flags, (int)ChannelFlags.Restricted, TLString.Empty, bytes, ref position);
            _adminRights = GetObject<TLChannelAdminRights>(Flags, (int)ChannelFlags.AdminRights, null, bytes, ref position);
            _bannedRights = GetObject<TLChannelBannedRights>(Flags, (int)ChannelFlags.BannedRights, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject(Flags, (int)ChannelFlags.AccessHash, new TLLong(0), input);
            _title = GetObject<TLString>(input);
            _userName = GetObject<TLString>(Flags, (int)ChannelFlags.Public, TLString.Empty, input);
            _photo = GetObject<TLPhotoBase>(input);
            Date = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);
            RestrictionReason = GetObject<TLString>(Flags, (int)ChannelFlags.Restricted, TLString.Empty, input);
            _adminRights = GetObject<TLChannelAdminRights>(Flags, (int)ChannelFlags.AdminRights, null, input);
            _bannedRights = GetObject<TLChannelBannedRights>(Flags, (int)ChannelFlags.BannedRights, null, input);

            CustomFlags = GetNullableObject<TLLong>(input);

            ParticipantIds = GetNullableObject<TLVector<TLInt>>(input);
            About = GetNullableObject<TLString>(input);
            ParticipantsCount = GetNullableObject<TLInt>(input);
            AdminsCount = GetNullableObject<TLInt>(input);
            KickedCount = GetNullableObject<TLInt>(input);
            ReadInboxMaxId = GetNullableObject<TLInt>(input);
            Pts = GetNullableObject<TLInt>(input);
            Participants = GetNullableObject<TLChatParticipantsBase>(input);
            NotifySettings = GetNullableObject<TLPeerNotifySettingsBase>(input);

            _migratedFromChatId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId, null, input);
            _migratedFromMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId, null, input);
            _pinnedMsgId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.PinnedMsgId, null, input);
            _hiddenPinnedMsgId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.HiddenPinnedMsgId, null, input);
            _readOutboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId, null, input);
            _stickerSet = GetObject<TLStickerSetBase>(CustomFlags, (int)ChannelCustomFlags.StickerSet, null, input);
            _availableMinId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.AvailableMinId, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            try
            {
                output.Write(TLUtils.SignatureToBytes(Signature));

                output.Write(Flags.ToBytes());
                output.Write(Id.ToBytes());
                ToStream(output, AccessHash, Flags, (int)ChannelFlags.AccessHash);
                output.Write(Title.ToBytes());
                ToStream(output, UserName, Flags, (int)ChannelFlags.Public);
                Photo.ToStream(output);
                Date.ToStream(output);
                Version.ToStream(output);
                ToStream(output, RestrictionReason, Flags, (int)ChannelFlags.Restricted);
                ToStream(output, AdminRights, Flags, (int)ChannelFlags.AdminRights);
                ToStream(output, BannedRights, Flags, (int)ChannelFlags.BannedRights);

                CustomFlags.NullableToStream(output);

                ParticipantIds.NullableToStream(output);
                About.NullableToStream(output);
                ParticipantsCount.NullableToStream(output);
                AdminsCount.NullableToStream(output);
                KickedCount.NullableToStream(output);
                ReadInboxMaxId.NullableToStream(output);
                Pts.NullableToStream(output);
                Participants.NullableToStream(output);
                NotifySettings.NullableToStream(output);

                ToStream(output, _migratedFromChatId, CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId);
                ToStream(output, _migratedFromMaxId, CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId);
                ToStream(output, PinnedMsgId, CustomFlags, (int)ChannelCustomFlags.PinnedMsgId);
                ToStream(output, HiddenPinnedMsgId, CustomFlags, (int)ChannelCustomFlags.HiddenPinnedMsgId);
                ToStream(output, ReadOutboxMaxId, CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId);
                ToStream(output, _stickerSet, CustomFlags, (int)ChannelCustomFlags.StickerSet);
                ToStream(output, _availableMinId, CustomFlags, (int)ChannelCustomFlags.AvailableMinId);
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(
                    string.Format(
                        "TLChannel68.ToStream access_hash={0} user_name={1} restriction_reason={2} migrated_from_chat_id={3} migrated_from_max_id={4} pinned_msg_id={5} hidden_pinned_msg_id={9} flags={6} custom_flags={7} ex {8}",
                        AccessHash, UserName, RestrictionReason, MigratedFromChatId, MigratedFromMaxId, PinnedMsgId,
                        ChannelFlagsString(Flags), ChannelCustomFlagsString(CustomFlags), HiddenPinnedMsgId, ex));
            }

        }

        public override void Update(TLChatBase chat)
        {
            var c = chat as TLChannel68;
            if (c != null)
            {
                AdminRights = c.AdminRights;
                BannedRights = c.BannedRights;

                if (c.Full)
                {
                    CanSetStickers = c.CanSetStickers;
                    if (c.StickerSet != null)
                    {
                        if (c.StickerSet is TLStickerSetEmpty)
                        {
                            StickerSet = null;
                        }
                        else
                        {
                            StickerSet = c.StickerSet;
                        }
                    }
                    HiddenPrehistory = c.HiddenPrehistory;
                    AvailableMinId = c.AvailableMinId;
                }
            }

            base.Update(chat);
        }
    }

    public class TLChannel49 : TLChannel44, IReadMaxId
    {
        public new const uint Signature = TLConstructors.TLChannel49;

        public bool Min
        {
            get { return IsSet(Flags, (int)ChannelFlags.Min); }
        }

        protected TLInt _pinnedMsgId;

        public TLInt PinnedMsgId
        {
            get { return _pinnedMsgId; }
            set { SetField(out _pinnedMsgId, value, ref _customFlags, (int)ChannelCustomFlags.PinnedMsgId); }
        }

        protected TLInt _hiddenPinnedMsgId;

        public TLInt HiddenPinnedMsgId
        {
            get { return _hiddenPinnedMsgId; }
            set { SetField(out _hiddenPinnedMsgId, value, ref _customFlags, (int)ChannelCustomFlags.HiddenPinnedMsgId); }
        }

        protected TLInt _readOutboxMaxId;

        public TLInt ReadOutboxMaxId
        {
            get { return _readOutboxMaxId; }
            set { SetField(out _readOutboxMaxId, value, ref _customFlags, (int)ChannelCustomFlags.ReadOutboxMaxId); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject(Flags, (int)ChannelFlags.AccessHash, new TLLong(0), bytes, ref position);
            _title = GetObject<TLString>(bytes, ref position);
            UserName = GetObject<TLString>(Flags, (int)ChannelFlags.Public, TLString.Empty, bytes, ref position);
            _photo = GetObject<TLPhotoBase>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);
            RestrictionReason = GetObject<TLString>(Flags, (int)ChannelFlags.Restricted, TLString.Empty, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            //try
            ////{
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject(Flags, (int)ChannelFlags.AccessHash, new TLLong(0), input);
            _title = GetObject<TLString>(input);
            _userName = GetObject<TLString>(Flags, (int)ChannelFlags.Public, TLString.Empty, input);
            _photo = GetObject<TLPhotoBase>(input);
            Date = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);
            RestrictionReason = GetObject<TLString>(Flags, (int)ChannelFlags.Restricted, TLString.Empty, input);

            CustomFlags = GetNullableObject<TLLong>(input);

            ParticipantIds = GetNullableObject<TLVector<TLInt>>(input);
            About = GetNullableObject<TLString>(input);
            ParticipantsCount = GetNullableObject<TLInt>(input);
            AdminsCount = GetNullableObject<TLInt>(input);
            KickedCount = GetNullableObject<TLInt>(input);
            ReadInboxMaxId = GetNullableObject<TLInt>(input);
            Pts = GetNullableObject<TLInt>(input);
            Participants = GetNullableObject<TLChatParticipantsBase>(input);
            NotifySettings = GetNullableObject<TLPeerNotifySettingsBase>(input);

            _migratedFromChatId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId, null, input);
            _migratedFromMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId, null, input);
            _pinnedMsgId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.PinnedMsgId, null, input);
            _hiddenPinnedMsgId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.HiddenPinnedMsgId, null, input);
            _readOutboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId, null, input);
            //}
            //catch (Exception ex)
            //{
            //    Execute.ShowDebugMessage("TLChannel44.FromStream ex " + ex);
            //}

            return this;
        }

        public override void ToStream(Stream output)
        {
            try
            {
                output.Write(TLUtils.SignatureToBytes(Signature));

                output.Write(Flags.ToBytes());
                output.Write(Id.ToBytes());
                ToStream(output, AccessHash, Flags, (int)ChannelFlags.AccessHash);
                output.Write(Title.ToBytes());
                ToStream(output, UserName, Flags, (int)ChannelFlags.Public);
                Photo.ToStream(output);
                Date.ToStream(output);
                Version.ToStream(output);
                ToStream(output, RestrictionReason, Flags, (int)ChannelFlags.Restricted);

                CustomFlags.NullableToStream(output);

                ParticipantIds.NullableToStream(output);
                About.NullableToStream(output);
                ParticipantsCount.NullableToStream(output);
                AdminsCount.NullableToStream(output);
                KickedCount.NullableToStream(output);
                ReadInboxMaxId.NullableToStream(output);
                Pts.NullableToStream(output);
                Participants.NullableToStream(output);
                NotifySettings.NullableToStream(output);

                ToStream(output, _migratedFromChatId, CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId);
                ToStream(output, _migratedFromMaxId, CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId);
                ToStream(output, PinnedMsgId, CustomFlags, (int)ChannelCustomFlags.PinnedMsgId);
                ToStream(output, HiddenPinnedMsgId, CustomFlags, (int)ChannelCustomFlags.HiddenPinnedMsgId);
                ToStream(output, ReadOutboxMaxId, CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId);
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(
                    string.Format(
                        "TLChannel49.ToStream access_hash={0} user_name={1} restriction_reason={2} migrated_from_chat_id={3} migrated_from_max_id={4} pinned_msg_id={5} hidden_pinned_msg_id={9} flags={6} custom_flags={7} ex {8}",
                        AccessHash, UserName, RestrictionReason, MigratedFromChatId, MigratedFromMaxId, PinnedMsgId,
                        ChannelFlagsString(Flags), ChannelCustomFlagsString(CustomFlags), HiddenPinnedMsgId, ex));
            }

        }

        public override void Update(TLChatBase chat)
        {
            var c = chat as TLChannel49;
            if (c != null)
            {
                if (c.Min)
                {
                    // copy flags: broadcast, verified, megagroup, democracy
                    IsBroadcast = c.IsBroadcast;
                    IsVerified = c.IsVerified;
                    if (IsMegaGroup && !c.IsMegaGroup)
                    {

                    }
                    IsMegaGroup = c.IsMegaGroup;
                    IsDemocracy = c.IsDemocracy;

                    Id = c.Id;
                    _title = c.Title;
                    UserName = c.UserName;
                    _photo = c.Photo;

                    return;
                }

                if (c.PinnedMsgId != null)
                {
                    PinnedMsgId = c.PinnedMsgId;
                }
                if (c.HiddenPinnedMsgId != null)
                {
                    HiddenPinnedMsgId = c.HiddenPinnedMsgId;
                }
                if (c.ReadOutboxMaxId != null && (ReadOutboxMaxId == null || ReadOutboxMaxId.Value < c.ReadOutboxMaxId.Value))
                {
                    ReadOutboxMaxId = c.ReadOutboxMaxId;
                }
            }

            base.Update(chat);
        }
    }

    public class TLChannel44 : TLChannel
    {
        public new const uint Signature = TLConstructors.TLChannel44;

        protected TLString _restrictionReason;

        public TLString RestrictionReason
        {
            get { return _restrictionReason; }
            set { SetField(out _restrictionReason, value, ref _flags, (int)ChannelFlags.Restricted); }
        }

        public bool IsRestricted
        {
            get { return IsSet(Flags, (int)ChannelFlags.Restricted); }
        }

        public bool Silent
        {
            get { return IsSet(CustomFlags, (int)ChannelCustomFlags.Silent); }
            set { SetUnset(ref _customFlags, value, (int)ChannelCustomFlags.Silent); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            _title = GetObject<TLString>(bytes, ref position);
            if (IsSet(Flags, (int)ChannelFlags.Public))
            {
                UserName = GetObject<TLString>(bytes, ref position);
            }
            _photo = GetObject<TLPhotoBase>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);
            if (IsSet(Flags, (int)ChannelFlags.Restricted))
            {
                RestrictionReason = GetObject<TLString>(bytes, ref position);
            }

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            //try
            ////{
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);
            _title = GetObject<TLString>(input);
            if (IsSet(Flags, (int)ChannelFlags.Public))
            {
                UserName = GetObject<TLString>(input);
            }
            _photo = GetObject<TLPhotoBase>(input);
            Date = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);
            if (IsSet(Flags, (int)ChannelFlags.Restricted))
            {
                RestrictionReason = GetObject<TLString>(input);
            }

            CustomFlags = GetNullableObject<TLLong>(input);

            ParticipantIds = GetNullableObject<TLVector<TLInt>>(input);
            About = GetNullableObject<TLString>(input);
            ParticipantsCount = GetNullableObject<TLInt>(input);
            AdminsCount = GetNullableObject<TLInt>(input);
            KickedCount = GetNullableObject<TLInt>(input);
            ReadInboxMaxId = GetNullableObject<TLInt>(input);
            Pts = GetNullableObject<TLInt>(input);
            Participants = GetNullableObject<TLChatParticipantsBase>(input);
            NotifySettings = GetNullableObject<TLPeerNotifySettingsBase>(input);

            if (IsSet(CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId))
            {
                _migratedFromChatId = GetObject<TLInt>(input);
            }
            if (IsSet(CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId))
            {
                _migratedFromMaxId = GetObject<TLInt>(input);
            }
            //}
            //catch (Exception ex)
            //{
            //    Execute.ShowDebugMessage("TLChannel44.FromStream ex " + ex);
            //}

            return this;
        }

        public override void ToStream(Stream output)
        {
            try
            {
                output.Write(TLUtils.SignatureToBytes(Signature));

                output.Write(Flags.ToBytes());
                output.Write(Id.ToBytes());
                output.Write(AccessHash.ToBytes());
                output.Write(Title.ToBytes());
                if (IsSet(Flags, (int)ChannelFlags.Public))
                {
                    UserName.ToStream(output);
                }
                Photo.ToStream(output);
                Date.ToStream(output);
                Version.ToStream(output);
                if (IsSet(Flags, (int)ChannelFlags.Restricted))
                {
                    RestrictionReason.ToStream(output);
                }

                CustomFlags.NullableToStream(output);

                ParticipantIds.NullableToStream(output);
                About.NullableToStream(output);
                ParticipantsCount.NullableToStream(output);
                AdminsCount.NullableToStream(output);
                KickedCount.NullableToStream(output);
                ReadInboxMaxId.NullableToStream(output);
                Pts.NullableToStream(output);
                Participants.NullableToStream(output);
                NotifySettings.NullableToStream(output);

                if (IsSet(CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId))
                {
                    _migratedFromChatId.ToStream(output);
                }
                if (IsSet(CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId))
                {
                    _migratedFromMaxId.ToStream(output);
                }
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage("TLChannel44.ToStream ex " + ex);
            }

        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);
            var c = chat as TLChannel44;
            if (c != null)
            {
                RestrictionReason = c.RestrictionReason ?? TLString.Empty;
            }
        }
    }

    public class TLChannel : TLBroadcastChat, IUserName, IInputChannel
    {
        public new const uint Signature = TLConstructors.TLChannel;

        public bool Full { get; set; }

        protected TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public virtual TLBool Left
        {
            get { return new TLBool(IsSet(_flags, (int)ChatFlags.Left)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)ChatFlags.Left);
                }
            }
        }

        public TLLong AccessHash { get; set; }

        protected TLString _userName;

        public TLString UserName
        {
            get { return _userName; }
            set { SetField(out _userName, value, ref _flags, (int)ChannelFlags.Public); }
        }

        public TLInt Date { get; set; }

        public TLInt Version { get; set; }

        public TLString About { get; set; }

        public virtual TLInt ParticipantsCount { get; set; }

        public TLInt AdminsCount { get; set; }

        public TLInt KickedCount { get; set; }

        public TLInt ReadInboxMaxId { get; set; }

        private TLInt _pts;

        public TLInt Pts
        {
            get { return _pts; }
            set { _pts = value; }
        }

        public override string FullName
        {
            get { return Title != null ? Title.ToString() : string.Empty; }
        }

        public bool Creator
        {
            get { return IsSet(Flags, (int)ChannelFlags.Creator); }
            set { SetUnset(ref _flags, value, (int)ChannelFlags.Creator); }
        }

        public bool IsEditor
        {
            get { return IsSet(Flags, (int)ChannelFlags.Editor); }
            set { SetUnset(ref _flags, value, (int)ChannelFlags.Editor); }
        }

        public bool IsModerator
        {
            get { return IsSet(Flags, (int)ChannelFlags.Moderator); }
            set { SetUnset(ref _flags, value, (int)ChannelFlags.Moderator); }
        }

        public bool IsBroadcast
        {
            get { return IsSet(Flags, (int)ChannelFlags.Broadcast); }
            set { SetUnset(ref _flags, value, (int)ChannelFlags.Broadcast); }
        }

        public bool IsPublic { get { return IsSet(Flags, (int)ChannelFlags.Public); } }

        public bool IsKicked { get { return IsSet(Flags, (int)ChannelFlags.Kicked); } }

        public bool IsVerified
        {
            get { return IsSet(Flags, (int)ChannelFlags.Verified); }
            set { SetUnset(ref _flags, value, (int)ChannelFlags.Verified); }
        }

        public bool IsMegaGroup
        {
            get { return IsSet(Flags, (int)ChannelFlags.MegaGroup); }
            set { SetUnset(ref _flags, value, (int)ChannelFlags.MegaGroup); }
        }

        public bool IsDemocracy
        {
            get { return IsSet(Flags, (int)ChannelFlags.Democracy); }
            set { SetUnset(ref _flags, value, (int)ChannelFlags.Democracy); }
        }

        public bool Signatures
        {
            get { return IsSet(Flags, (int)ChannelFlags.Signatures); }
            set { SetUnset(ref _flags, value, (int)ChannelFlags.Signatures); }
        }

        #region Additional

        public virtual bool CanPinMessages
        {
            get
            {
                return Creator;
            }
        }

        public string ChannelParticipantsFileName
        {
            get { return string.Format("{0}_participants.dat", Id); }
        }

        public TLChannelParticipants ChannelParticipants { get; set; }

        protected TLInt _migratedFromChatId;

        public TLInt MigratedFromChatId
        {
            get { return _migratedFromChatId; }
            set { SetField(out _migratedFromChatId, value, ref _customFlags, (int)ChannelCustomFlags.MigratedFromChatId); }
        }

        protected TLInt _migratedFromMaxId;

        public TLInt MigratedFromMaxId
        {
            get { return _migratedFromMaxId; }
            set { SetField(out _migratedFromMaxId, value, ref _customFlags, (int)ChannelCustomFlags.MigratedFromMaxId); }
        }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            _title = GetObject<TLString>(bytes, ref position);
            if (IsSet(Flags, (int)ChannelFlags.Public))
            {
                UserName = GetObject<TLString>(bytes, ref position);
            }
            _photo = GetObject<TLPhotoBase>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);
            _title = GetObject<TLString>(input);
            if (IsSet(Flags, (int)ChannelFlags.Public))
            {
                UserName = GetObject<TLString>(input);
            }
            _photo = GetObject<TLPhotoBase>(input);
            Date = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);

            CustomFlags = GetNullableObject<TLLong>(input);

            ParticipantIds = GetNullableObject<TLVector<TLInt>>(input);
            About = GetNullableObject<TLString>(input);
            ParticipantsCount = GetNullableObject<TLInt>(input);
            AdminsCount = GetNullableObject<TLInt>(input);
            KickedCount = GetNullableObject<TLInt>(input);
            ReadInboxMaxId = GetNullableObject<TLInt>(input);
            Pts = GetNullableObject<TLInt>(input);
            Participants = GetNullableObject<TLChatParticipantsBase>(input);
            NotifySettings = GetNullableObject<TLPeerNotifySettingsBase>(input);

            _migratedFromChatId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId, null, input);
            _migratedFromMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            try
            {
                output.Write(TLUtils.SignatureToBytes(Signature));

                output.Write(Flags.ToBytes());
                output.Write(Id.ToBytes());
                output.Write(AccessHash.ToBytes());
                output.Write(Title.ToBytes());
                if (IsSet(Flags, (int)ChannelFlags.Public))
                {
                    UserName.ToStream(output);
                }
                Photo.ToStream(output);
                Date.ToStream(output);
                Version.ToStream(output);

                CustomFlags.NullableToStream(output);

                ParticipantIds.NullableToStream(output);
                About.NullableToStream(output);
                ParticipantsCount.NullableToStream(output);
                AdminsCount.NullableToStream(output);
                KickedCount.NullableToStream(output);
                ReadInboxMaxId.NullableToStream(output);
                Pts.NullableToStream(output);
                Participants.NullableToStream(output);
                NotifySettings.NullableToStream(output);

                ToStream(output, _migratedFromChatId, CustomFlags, (int)ChannelCustomFlags.MigratedFromChatId);
                ToStream(output, _migratedFromMaxId, CustomFlags, (int)ChannelCustomFlags.MigratedFromMaxId);
            }
            catch (Exception ex)
            {

            }

        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);

            var c = chat as TLChannel;
            if (c != null)
            {
                //if (c.Flags != null) Flags = c.Flags;
                Creator = c.Creator;
                Left = c.Left;
                IsEditor = c.IsEditor;
                IsBroadcast = c.IsBroadcast;
                IsVerified = c.IsVerified;
                IsMegaGroup = c.IsMegaGroup;
                IsDemocracy = c.IsDemocracy;
                Signatures = c.Signatures;

                AccessHash = c.AccessHash ?? new TLLong(0);
                UserName = c.UserName ?? TLString.Empty;

                //if (c.CustomFlags != null) CustomFlags = c.CustomFlags;
                if (c.MigratedFromChatId != null) MigratedFromChatId = c.MigratedFromChatId;
                if (c.MigratedFromMaxId != null) MigratedFromMaxId = c.MigratedFromMaxId;

                if (c.ParticipantIds != null) ParticipantIds = c.ParticipantIds;
                if (c.About != null) About = c.About;
                if (c.ParticipantsCount != null) ParticipantsCount = c.ParticipantsCount;
                if (c.AdminsCount != null) AdminsCount = c.AdminsCount;
                if (c.KickedCount != null) KickedCount = c.KickedCount;
                if (c.ReadInboxMaxId != null) ReadInboxMaxId = c.ReadInboxMaxId;
                if (c.Participants != null) Participants = c.Participants;
                if (c.NotifySettings != null) NotifySettings = c.NotifySettings;
            }
        }

        public override string ToString()
        {
            return Title + " flags=" + ChannelFlagsString(Flags) + " custom_flags=" + ChannelCustomFlagsString(CustomFlags); ;
        }

        public override TLInputPeerBase ToInputPeer()
        {
            return new TLInputPeerChannel { ChatId = Id, AccessHash = AccessHash };
        }

        public TLInputChannelBase ToInputChannel()
        {
            return new TLInputChannel { ChannelId = Id, AccessHash = AccessHash };
        }

        public override string GetUnsendedTextFileName()
        {
            return "ch" + Id + ".dat";
        }

        public override bool IsForbidden
        {
            get { return Left.Value; }
        }
    }

    public class TLChannelForbidden : TLChatBase, IInputChannel
    {
        public const uint Signature = TLConstructors.TLChannelForbidden;

        public TLLong AccessHash { get; set; }

        public TLString Title { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);
            Title = GetObject<TLString>(input);

            Participants = GetObject<TLObject>(input) as TLChatParticipantsBase;
            NotifySettings = GetObject<TLObject>(input) as TLPeerNotifySettingsBase;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Id.ToBytes());
            output.Write(AccessHash.ToBytes());
            output.Write(Title.ToBytes());

            Participants.NullableToStream(output);
            NotifySettings.NullableToStream(output);
        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);
            var c = chat as TLChannelForbidden;
            if (c != null)
            {
                Id = c.Id;
                AccessHash = c.AccessHash;
                Title = c.Title;
            }
        }

        public override string FullName
        {
            get { return Title != null ? Title.ToString() : string.Empty; }
        }

        public override TLInputPeerBase ToInputPeer()
        {
            return new TLInputPeerChannel { ChatId = Id, AccessHash = AccessHash };
        }

        public TLInputChannelBase ToInputChannel()
        {
            return new TLInputChannel { ChannelId = Id, AccessHash = AccessHash };
        }

        public override string GetUnsendedTextFileName()
        {
            return "ch" + Id + ".dat";
        }

        public override bool IsForbidden
        {
            get { return true; }
        }
    }

    public class TLChannelForbidden53 : TLChannelForbidden, IReadMaxId
    {
        public new const uint Signature = TLConstructors.TLChannelForbidden53;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        public bool IsBroadcast
        {
            get { return IsSet(Flags, (int)ChannelFlags.Broadcast); }
            set { SetUnset(ref _flags, value, (int)ChannelFlags.Broadcast); }
        }

        public bool IsMegaGroup
        {
            get { return IsSet(Flags, (int)ChannelFlags.MegaGroup); }
            set { SetUnset(ref _flags, value, (int)ChannelFlags.MegaGroup); }
        }

        protected TLInt _readInboxMaxId;

        public TLInt ReadInboxMaxId
        {
            get { return _readInboxMaxId; }
            set { SetField(out _readInboxMaxId, value, ref _customFlags, (int)ChatCustomFlags.ReadInboxMaxId); }
        }

        protected TLInt _readOutboxMaxId;

        public TLInt ReadOutboxMaxId
        {
            get { return _readOutboxMaxId; }
            set { SetField(out _readOutboxMaxId, value, ref _customFlags, (int)ChatCustomFlags.ReadOutboxMaxId); }
        }

        #region Additional
        public TLPhotoBase Photo { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);
            Title = GetObject<TLString>(input);

            Participants = GetObject<TLObject>(input) as TLChatParticipantsBase;
            NotifySettings = GetObject<TLObject>(input) as TLPeerNotifySettingsBase;

            CustomFlags = GetNullableObject<TLLong>(input);

            _readInboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.ReadInboxMaxId, null, input);
            _readOutboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            output.Write(Id.ToBytes());
            output.Write(AccessHash.ToBytes());
            output.Write(Title.ToBytes());

            Participants.NullableToStream(output);
            NotifySettings.NullableToStream(output);

            CustomFlags.NullableToStream(output);

            ToStream(output, ReadInboxMaxId, CustomFlags, (int)ChannelCustomFlags.ReadInboxMaxId);
            ToStream(output, ReadOutboxMaxId, CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId);
        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);

            var c = chat as TLChannelForbidden53;
            if (c != null)
            {
                IsBroadcast = c.IsBroadcast;
                IsMegaGroup = c.IsMegaGroup;

                if (c.ReadInboxMaxId != null
                    && (ReadInboxMaxId == null || ReadInboxMaxId.Value < c.ReadInboxMaxId.Value))
                {
                    ReadInboxMaxId = c.ReadInboxMaxId;
                }

                if (c.ReadOutboxMaxId != null
                    && (ReadOutboxMaxId == null || ReadOutboxMaxId.Value < c.ReadOutboxMaxId.Value))
                {
                    ReadOutboxMaxId = c.ReadOutboxMaxId;
                }
            }
        }
    }

    public class TLChannelForbidden68 : TLChannelForbidden53
    {
        public new const uint Signature = TLConstructors.TLChannelForbidden68;

        protected TLInt _untilDate;

        public TLInt UntilDate
        {
            get { return _untilDate; }
            set { SetField(out _untilDate, value, ref _flags, (int)ChannelFlags.UntilDate); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            _untilDate = GetObject<TLInt>(Flags, (int)ChannelFlags.UntilDate, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInt>(input);
            AccessHash = GetObject<TLLong>(input);
            Title = GetObject<TLString>(input);
            _untilDate = GetObject<TLInt>(Flags, (int)ChannelFlags.UntilDate, null, input);

            Participants = GetObject<TLObject>(input) as TLChatParticipantsBase;
            NotifySettings = GetObject<TLObject>(input) as TLPeerNotifySettingsBase;

            CustomFlags = GetNullableObject<TLLong>(input);

            _readInboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.ReadInboxMaxId, null, input);
            _readOutboxMaxId = GetObject<TLInt>(CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            output.Write(Id.ToBytes());
            output.Write(AccessHash.ToBytes());
            output.Write(Title.ToBytes());
            ToStream(output, UntilDate, Flags, (int)ChannelFlags.UntilDate);

            Participants.NullableToStream(output);
            NotifySettings.NullableToStream(output);

            CustomFlags.NullableToStream(output);

            ToStream(output, ReadInboxMaxId, CustomFlags, (int)ChannelCustomFlags.ReadInboxMaxId);
            ToStream(output, ReadOutboxMaxId, CustomFlags, (int)ChannelCustomFlags.ReadOutboxMaxId);
        }

        public override void Update(TLChatBase chat)
        {
            base.Update(chat);

            var c = chat as TLChannelForbidden68;
            if (c != null)
            {
                UntilDate = c.UntilDate;
            }
        }
    }
}

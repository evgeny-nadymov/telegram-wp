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
    public enum ChannelFullFlags
    {
        ParticipantsCount = 0x1,
        AdminsCount = 0x2,
        KickedCount = 0x4,
        CanViewParticipants = 0x8,
        Migrated = 0x10,
        PinnedMsgId = 0x20,
        CanSetUsername = 0x40,
        CanSetStickers = 0x80,
        StickerSet = 0x100,
        AvailableMinId = 0x200,
        HiddenPrehistory = 0x400
    }

    public class TLChatFull : TLObject
    {
        public const uint Signature = TLConstructors.TLChatFull;

        public TLInt Id { get; set; }

        public TLChatParticipantsBase Participants { get; set; }

        public TLPhotoBase ChatPhoto { get; set; }

        public TLPeerNotifySettingsBase NotifySettings { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            Participants = GetObject<TLChatParticipantsBase>(bytes, ref position);
            ChatPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);

            return this;
        }

        public virtual TLChatBase ToChat(TLChatBase chat)
        {
            chat.NotifySettings = NotifySettings;
            chat.Participants = Participants;
            chat.ChatPhoto = ChatPhoto;

            return chat;
        }
    }

    public class TLChatFull28 : TLChatFull
    {
        public new const uint Signature = TLConstructors.TLChatFull28;

        public TLExportedChatInvite ExportedInvite { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            Participants = GetObject<TLChatParticipantsBase>(bytes, ref position);
            ChatPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            ExportedInvite = GetObject<TLExportedChatInvite>(bytes, ref position);

            return this;
        }
    }

    public class TLChatFull31 : TLChatFull28
    {
        public new const uint Signature = TLConstructors.TLChatFull31;

        public TLVector<TLBotInfoBase> BotInfo { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            Participants = GetObject<TLChatParticipantsBase>(bytes, ref position);
            ChatPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            ExportedInvite = GetObject<TLExportedChatInvite>(bytes, ref position);
            BotInfo = GetObject<TLVector<TLBotInfoBase>>(bytes, ref position);

            return this;
        }
    }

    public class TLChannelFull : TLChatFull
    {
        public new const uint Signature = TLConstructors.TLChannelFull;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLString About { get; set; }

        protected TLInt _participantsCount;

        public TLInt ParticipantsCount
        {
            get { return _participantsCount; }
            set { SetField(out _participantsCount, value, ref _flags, (int)ChannelFullFlags.ParticipantsCount); }
        }

        protected TLInt _adminsCount;

        public TLInt AdminsCount
        {
            get { return _adminsCount; }
            set { SetField(out _adminsCount, value, ref _flags, (int)ChannelFullFlags.AdminsCount); }
        }

        protected TLInt _kickedCount;

        public TLInt KickedCount
        {
            get { return _kickedCount; }
            set { SetField(out _kickedCount, value, ref _flags, (int)ChannelFullFlags.KickedCount); }
        }

        public TLInt ReadInboxMaxId { get; set; }

        public TLInt UnreadCount { get; set; }

        public TLInt UnreadImportantCount { get; set; }

        public TLExportedChatInvite ExportedInvite { get; set; }

        public bool CanViewParticipants { get { return IsSet(Flags, (int) ChannelFullFlags.CanViewParticipants); } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            //Participants = GetObject<TLChatParticipantsBase>(bytes, ref position);
            About = GetObject<TLString>(bytes, ref position);
            if (IsSet(Flags, (int)ChannelFullFlags.ParticipantsCount))
            {
                ParticipantsCount = GetObject<TLInt>(bytes, ref position);
            } 
            if (IsSet(Flags, (int) ChannelFullFlags.AdminsCount))
            {
                AdminsCount = GetObject<TLInt>(bytes, ref position);
            }
            if (IsSet(Flags, (int)ChannelFullFlags.KickedCount))
            {
                KickedCount = GetObject<TLInt>(bytes, ref position);
            }
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadImportantCount = GetObject<TLInt>(bytes, ref position);        
            ChatPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            ExportedInvite = GetObject<TLExportedChatInvite>(bytes, ref position);
            
            return this;
        }

        public override TLChatBase ToChat(TLChatBase chat)
        {
            chat.NotifySettings = NotifySettings;
            chat.Participants = Participants;
            chat.ChatPhoto = ChatPhoto;

            var channel = chat as TLChannel;
            if (channel != null)
            {
                channel.ExportedInvite = ExportedInvite;
                channel.About = About;
                channel.ParticipantsCount = ParticipantsCount;
                channel.AdminsCount = AdminsCount;
                channel.KickedCount = KickedCount;
                channel.ReadInboxMaxId = ReadInboxMaxId;
            }

            return chat;
        }
    }

    public class TLChannelFull41 : TLChannelFull
    {
        public new const uint Signature = TLConstructors.TLChannelFull41;

        public TLVector<TLBotInfoBase> BotInfo { get; set; }

        protected TLInt _migratedFromChatId;

        public TLInt MigratedFromChatId
        {
            get { return _migratedFromChatId; }
            set { SetField(out _migratedFromChatId, value, ref _flags, (int)ChannelFullFlags.Migrated); }
        }

        protected TLInt _migratedFromMaxId;

        public TLInt MigratedFromMaxId
        {
            get { return _migratedFromMaxId; }
            set { SetField(out _migratedFromMaxId, value, ref _flags, (int)ChannelFullFlags.Migrated); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            //Participants = GetObject<TLChatParticipantsBase>(bytes, ref position);
            About = GetObject<TLString>(bytes, ref position);
            if (IsSet(Flags, (int)ChannelFullFlags.ParticipantsCount))
            {
                ParticipantsCount = GetObject<TLInt>(bytes, ref position);
            }
            if (IsSet(Flags, (int)ChannelFullFlags.AdminsCount))
            {
                AdminsCount = GetObject<TLInt>(bytes, ref position);
            }
            if (IsSet(Flags, (int)ChannelFullFlags.KickedCount))
            {
                KickedCount = GetObject<TLInt>(bytes, ref position);
            }
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadImportantCount = GetObject<TLInt>(bytes, ref position);
            ChatPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            ExportedInvite = GetObject<TLExportedChatInvite>(bytes, ref position);
            BotInfo = GetObject<TLVector<TLBotInfoBase>>(bytes, ref position);
            if (IsSet(Flags, (int)ChannelFullFlags.Migrated))
            {
                MigratedFromChatId = GetObject<TLInt>(bytes, ref position);
            }
            if (IsSet(Flags, (int)ChannelFullFlags.Migrated))
            {
                MigratedFromMaxId = GetObject<TLInt>(bytes, ref position);
            }

            return this;
        }

        public override TLChatBase ToChat(TLChatBase chat)
        {
            chat.NotifySettings = NotifySettings;
            chat.Participants = Participants;
            chat.ChatPhoto = ChatPhoto;

            var channel = chat as TLChannel;
            if (channel != null)
            {
                channel.ExportedInvite = ExportedInvite;
                channel.About = About;
                channel.ParticipantsCount = ParticipantsCount;
                channel.AdminsCount = AdminsCount;
                channel.KickedCount = KickedCount;
                channel.ReadInboxMaxId = ReadInboxMaxId;
            }

            return chat;
        }
    }

    public class TLChannelFull49 : TLChannelFull41
    {
        public new const uint Signature = TLConstructors.TLChannelFull49;

        protected TLInt _pinnedMsgId;

        public TLInt PinnedMsgId
        {
            get { return _pinnedMsgId; }
            set { SetField(out _pinnedMsgId, value, ref _flags, (int)ChannelFullFlags.PinnedMsgId); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            //Participants = GetObject<TLChatParticipantsBase>(bytes, ref position);
            About = GetObject<TLString>(bytes, ref position);
            ParticipantsCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.ParticipantsCount, null, bytes, ref position);
            AdminsCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.AdminsCount, null, bytes, ref position);
            KickedCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.KickedCount, null, bytes, ref position);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadImportantCount = GetObject<TLInt>(bytes, ref position);
            ChatPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            ExportedInvite = GetObject<TLExportedChatInvite>(bytes, ref position);
            BotInfo = GetObject<TLVector<TLBotInfoBase>>(bytes, ref position);
            MigratedFromChatId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.Migrated, null, bytes, ref position);
            MigratedFromMaxId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.Migrated, null, bytes, ref position);
            PinnedMsgId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.PinnedMsgId, null, bytes, ref position);

            return this;
        }

        public override TLChatBase ToChat(TLChatBase chat)
        {
            chat.NotifySettings = NotifySettings;
            chat.Participants = Participants;
            chat.ChatPhoto = ChatPhoto;

            var channel = chat as TLChannel;
            if (channel != null)
            {
                channel.About = About;
                channel.ParticipantsCount = ParticipantsCount;
                channel.AdminsCount = AdminsCount;
                channel.KickedCount = KickedCount;
                channel.ReadInboxMaxId = ReadInboxMaxId;
                channel.ExportedInvite = ExportedInvite;
                channel.BotInfo = BotInfo;
                channel.MigratedFromChatId = MigratedFromChatId;
                channel.MigratedFromMaxId = MigratedFromMaxId;
            }

            var channel49 = chat as TLChannel49;
            if (channel49 != null)
            {
                channel49.PinnedMsgId = PinnedMsgId;
            }

            return chat;
        }
    }

    public class TLChannelFull53 : TLChannelFull49
    {
        public new const uint Signature = TLConstructors.TLChannelFull53;

        public TLInt ReadOutboxMaxId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            //Participants = GetObject<TLChatParticipantsBase>(bytes, ref position);
            About = GetObject<TLString>(bytes, ref position);
            ParticipantsCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.ParticipantsCount, null, bytes, ref position);
            AdminsCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.AdminsCount, null, bytes, ref position);
            KickedCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.KickedCount, null, bytes, ref position);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            ReadOutboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadImportantCount = new TLInt(0);
            ChatPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            ExportedInvite = GetObject<TLExportedChatInvite>(bytes, ref position);
            BotInfo = GetObject<TLVector<TLBotInfoBase>>(bytes, ref position);
            MigratedFromChatId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.Migrated, null, bytes, ref position);
            MigratedFromMaxId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.Migrated, null, bytes, ref position);
            PinnedMsgId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.PinnedMsgId, null, bytes, ref position);

            return this;
        }

        public override TLChatBase ToChat(TLChatBase chat)
        {
            chat.NotifySettings = NotifySettings;
            chat.Participants = Participants;
            chat.ChatPhoto = ChatPhoto;

            var channel = chat as TLChannel;
            if (channel != null)
            {
                channel.About = About;
                channel.ParticipantsCount = ParticipantsCount;
                channel.AdminsCount = AdminsCount;
                channel.KickedCount = KickedCount;
                channel.ReadInboxMaxId = ReadInboxMaxId;
                channel.ExportedInvite = ExportedInvite;
                channel.BotInfo = BotInfo;
                channel.MigratedFromChatId = MigratedFromChatId;
                channel.MigratedFromMaxId = MigratedFromMaxId;
            }

            var channel49 = chat as TLChannel49;
            if (channel49 != null)
            {
                channel49.ReadOutboxMaxId = ReadOutboxMaxId;
                channel49.PinnedMsgId = PinnedMsgId;
            }

            return chat;
        }
    }

    public class TLChannelFull68 : TLChannelFull53
    {
        public new const uint Signature = TLConstructors.TLChannelFull68;

        protected TLInt _bannedCount;

        public TLInt BannedCount
        {
            get { return _bannedCount; }
            set { SetField(out _bannedCount, value, ref _flags, (int)ChannelFullFlags.KickedCount); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            About = GetObject<TLString>(bytes, ref position);
            ParticipantsCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.ParticipantsCount, null, bytes, ref position);
            _adminsCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.AdminsCount, null, bytes, ref position);            
            _kickedCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.KickedCount, null, bytes, ref position);
            _bannedCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.KickedCount, null, bytes, ref position);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            ReadOutboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadImportantCount = new TLInt(0);
            ChatPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            ExportedInvite = GetObject<TLExportedChatInvite>(bytes, ref position);
            BotInfo = GetObject<TLVector<TLBotInfoBase>>(bytes, ref position);
            _migratedFromChatId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.Migrated, null, bytes, ref position);
            _migratedFromMaxId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.Migrated, null, bytes, ref position);
            _pinnedMsgId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.PinnedMsgId, null, bytes, ref position);

            return this;
        }

        public override TLChatBase ToChat(TLChatBase chat)
        {
            chat.NotifySettings = NotifySettings;
            chat.Participants = Participants;
            chat.ChatPhoto = ChatPhoto;

            var channel = chat as TLChannel;
            if (channel != null)
            {
                channel.About = About;
                channel.ParticipantsCount = ParticipantsCount;
                channel.AdminsCount = AdminsCount;
                channel.KickedCount = KickedCount;
                channel.ReadInboxMaxId = ReadInboxMaxId;
                channel.ExportedInvite = ExportedInvite;
                channel.BotInfo = BotInfo;
                channel.MigratedFromChatId = MigratedFromChatId;
                channel.MigratedFromMaxId = MigratedFromMaxId;
            }

            var channel49 = chat as TLChannel49;
            if (channel49 != null)
            {
                channel49.ReadOutboxMaxId = ReadOutboxMaxId;
                channel49.PinnedMsgId = PinnedMsgId;
            }

            var channel68 = chat as TLChannel68;
            if (channel68 != null)
            {
                
            }

            return chat;
        }
    }

    public class TLChannelFull71 : TLChannelFull68
    {
        public new const uint Signature = TLConstructors.TLChannelFull71;

        public bool CanSetStickers
        {
            get { return IsSet(Flags, (int) ChannelFullFlags.CanSetStickers); }
        }

        protected TLStickerSetBase _stickerSet;

        public TLStickerSetBase StickerSet
        {
            get { return _stickerSet; }
            set { SetField(out _stickerSet, value, ref _flags, (int)ChannelFullFlags.StickerSet); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            About = GetObject<TLString>(bytes, ref position);
            ParticipantsCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.ParticipantsCount, null, bytes, ref position);
            _adminsCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.AdminsCount, null, bytes, ref position);
            _kickedCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.KickedCount, null, bytes, ref position);
            _bannedCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.KickedCount, null, bytes, ref position);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            ReadOutboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadImportantCount = new TLInt(0);
            ChatPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            ExportedInvite = GetObject<TLExportedChatInvite>(bytes, ref position);
            BotInfo = GetObject<TLVector<TLBotInfoBase>>(bytes, ref position);
            _migratedFromChatId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.Migrated, null, bytes, ref position);
            _migratedFromMaxId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.Migrated, null, bytes, ref position);
            _pinnedMsgId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.PinnedMsgId, null, bytes, ref position);
            _stickerSet = GetObject<TLStickerSetBase>(Flags, (int)ChannelFullFlags.StickerSet, null, bytes, ref position);

            return this;
        }

        public override TLChatBase ToChat(TLChatBase chat)
        {
            chat.NotifySettings = NotifySettings;
            chat.Participants = Participants;
            chat.ChatPhoto = ChatPhoto;

            var channel = chat as TLChannel;
            if (channel != null)
            {
                channel.Full = true;

                channel.About = About;
                channel.ParticipantsCount = ParticipantsCount;
                channel.AdminsCount = AdminsCount;
                channel.KickedCount = KickedCount;
                channel.ReadInboxMaxId = ReadInboxMaxId;
                channel.ExportedInvite = ExportedInvite;
                channel.BotInfo = BotInfo;
                channel.MigratedFromChatId = MigratedFromChatId;
                channel.MigratedFromMaxId = MigratedFromMaxId;
            }

            var channel49 = chat as TLChannel49;
            if (channel49 != null)
            {
                channel49.ReadOutboxMaxId = ReadOutboxMaxId;
                channel49.PinnedMsgId = PinnedMsgId;
            }

            var channel68 = chat as TLChannel68;
            if (channel68 != null)
            {
                channel68.CanSetStickers = CanSetStickers;
                channel68.StickerSet = StickerSet ?? new TLStickerSetEmpty();
            }

            return chat;
        }
    }

    public class TLChannelFull72 : TLChannelFull71
    {
        public new const uint Signature = TLConstructors.TLChannelFull72;

        public bool HiddenPrehistory
        {
            get { return IsSet(Flags, (int)ChannelFullFlags.HiddenPrehistory); }
        }

        protected TLInt _availableMinId;

        public TLInt AvailableMinId
        {
            get { return _availableMinId; }
            set { SetField(out _availableMinId, value, ref _flags, (int)ChannelFullFlags.AvailableMinId); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            About = GetObject<TLString>(bytes, ref position);
            ParticipantsCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.ParticipantsCount, null, bytes, ref position);
            _adminsCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.AdminsCount, null, bytes, ref position);
            _kickedCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.KickedCount, null, bytes, ref position);
            _bannedCount = GetObject<TLInt>(Flags, (int)ChannelFullFlags.KickedCount, null, bytes, ref position);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            ReadOutboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadImportantCount = new TLInt(0);
            ChatPhoto = GetObject<TLPhotoBase>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            ExportedInvite = GetObject<TLExportedChatInvite>(bytes, ref position);
            BotInfo = GetObject<TLVector<TLBotInfoBase>>(bytes, ref position);
            _migratedFromChatId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.Migrated, null, bytes, ref position);
            _migratedFromMaxId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.Migrated, null, bytes, ref position);
            _pinnedMsgId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.PinnedMsgId, null, bytes, ref position);
            _stickerSet = GetObject<TLStickerSetBase>(Flags, (int)ChannelFullFlags.StickerSet, null, bytes, ref position);
            _availableMinId = GetObject<TLInt>(Flags, (int)ChannelFullFlags.AvailableMinId, null, bytes, ref position);

            return this;
        }

        public override TLChatBase ToChat(TLChatBase chat)
        {
            chat.NotifySettings = NotifySettings;
            chat.Participants = Participants;
            chat.ChatPhoto = ChatPhoto;

            var channel = chat as TLChannel;
            if (channel != null)
            {
                channel.Full = true;

                channel.About = About;
                channel.ParticipantsCount = ParticipantsCount;
                channel.AdminsCount = AdminsCount;
                channel.KickedCount = KickedCount;
                channel.ReadInboxMaxId = ReadInboxMaxId;
                channel.ExportedInvite = ExportedInvite;
                channel.BotInfo = BotInfo;
                channel.MigratedFromChatId = MigratedFromChatId;
                channel.MigratedFromMaxId = MigratedFromMaxId;
            }

            var channel49 = chat as TLChannel49;
            if (channel49 != null)
            {
                channel49.ReadOutboxMaxId = ReadOutboxMaxId;
                channel49.PinnedMsgId = PinnedMsgId;
            }

            var channel68 = chat as TLChannel68;
            if (channel68 != null)
            {
                channel68.CanSetStickers = CanSetStickers;
                channel68.StickerSet = StickerSet ?? new TLStickerSetEmpty();

                channel68.HiddenPrehistory = HiddenPrehistory;
                channel68.AvailableMinId = AvailableMinId;
            }

            return chat;
        }
    }
}

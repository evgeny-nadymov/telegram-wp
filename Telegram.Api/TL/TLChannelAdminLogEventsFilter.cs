// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum ChannelAdminLogEventsFilterFlags
    {
        Join = 0x1,
        Leave = 0x2,
        Invite = 0x4,
        Ban = 0x8,
        Unban = 0x10,
        Kick = 0x20,
        Unkick = 0x40,
        Promote = 0x80,
        Demote = 0x100,
        Info = 0x200,
        Settings = 0x400,
        Pinned = 0x800,
        Edit = 0x1000,
        Delete = 0x2000,
    }

    public class TLChannelAdminLogEventsFilter : TLObject
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEventsFilter;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Join
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Join); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Join); }
        }

        public bool Leave
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Leave); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Leave); }
        }

        public bool Invite
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Invite); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Invite); }
        }

        public bool Ban
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Ban); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Ban); }
        }

        public bool Unban
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Unban); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Unban); }
        }

        public bool Kick
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Kick); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Kick); }
        }

        public bool Unkick
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Unkick); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Unkick); }
        }

        public bool Promote
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Promote); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Promote); }
        }

        public bool Demote
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Demote); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Demote); }
        }

        public bool Info
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Info); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Info); }
        }

        public bool Settings
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Settings); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Settings); }
        }

        public bool Pinned
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Pinned); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Pinned); }
        }

        public bool Edit
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Edit); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Edit); }
        }

        public bool Delete
        {
            get { return IsSet(_flags, (int)ChannelAdminLogEventsFilterFlags.Delete); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminLogEventsFilterFlags.Delete); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
        }
    }
}

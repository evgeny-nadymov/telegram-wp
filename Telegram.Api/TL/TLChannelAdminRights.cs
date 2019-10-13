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
    public enum ChannelAdminRightsFlags
    {
        ChannelInfo = 0x1,
        PostMessages = 0x2,
        EditMessages = 0x4,
        DeleteMessages = 0x8,
        BanUsers = 0x10,
        InviteUsers = 0x20,
        InviteLinks = 0x40,
        PinMessages = 0x80,

        AddAdmins = 0x200,
    }

    public class TLChannelAdminRights : TLObject
    {
        public const uint Signature = TLConstructors.TLChannelAdminRights;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool ChannelInfo
        {
            get { return IsSet(_flags, (int)ChannelAdminRightsFlags.ChannelInfo); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminRightsFlags.ChannelInfo); }
        }

        public bool PostMessages
        {
            get { return IsSet(_flags, (int)ChannelAdminRightsFlags.PostMessages); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminRightsFlags.PostMessages); }
        }

        public bool EditMessages
        {
            get { return IsSet(_flags, (int)ChannelAdminRightsFlags.EditMessages); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminRightsFlags.EditMessages); }
        }

        public bool DeleteMessages
        {
            get { return IsSet(_flags, (int)ChannelAdminRightsFlags.DeleteMessages); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminRightsFlags.DeleteMessages); }
        }

        public bool BanUsers
        {
            get { return IsSet(_flags, (int)ChannelAdminRightsFlags.BanUsers); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminRightsFlags.BanUsers); }
        }

        public bool InviteUsers
        {
            get { return IsSet(_flags, (int)ChannelAdminRightsFlags.InviteUsers); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminRightsFlags.InviteUsers); }
        }

        public bool InviteLinks
        {
            get { return IsSet(_flags, (int)ChannelAdminRightsFlags.InviteLinks); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminRightsFlags.InviteLinks); }
        }

        public bool PinMessages
        {
            get { return IsSet(_flags, (int)ChannelAdminRightsFlags.PinMessages); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminRightsFlags.PinMessages); }
        }

        public bool AddAdmins
        {
            get { return IsSet(_flags, (int)ChannelAdminRightsFlags.AddAdmins); }
            set { SetUnset(ref _flags, value, (int)ChannelAdminRightsFlags.AddAdmins); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
        }
    }
}

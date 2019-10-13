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
    public enum ChannelParticipantBannedFlags
    {
        Left = 0x1
    }

    public enum ChannelParticipantAdminFlags
    {
        CanEdit = 0x1
    }

    public interface IChannelInviter
    {
        TLInt InviterId { get; set; }

        TLInt Date { get; set; }
    }

    public abstract class TLChannelParticipantBase : TLObject
    {
        public TLInt UserId { get; set; }
    }

    public class TLChannelParticipant : TLChannelParticipantBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipant;

        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(UserId.ToBytes());
            output.Write(Date.ToBytes());
        }
    }

    public class TLChannelParticipantSelf : TLChannelParticipantBase, IChannelInviter
    {
        public const uint Signature = TLConstructors.TLChannelParticipantSelf;

        public TLInt InviterId { get; set; }

        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            InviterId = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            InviterId = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(UserId.ToBytes());
            output.Write(InviterId.ToBytes());
            output.Write(Date.ToBytes());
        }
    }

    [Obsolete]
    public class TLChannelParticipantModerator : TLChannelParticipantBase, IChannelInviter
    {
        public const uint Signature = TLConstructors.TLChannelParticipantModerator;

        public TLInt InviterId { get; set; }

        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            InviterId = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            InviterId = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(UserId.ToBytes());
            output.Write(InviterId.ToBytes());
            output.Write(Date.ToBytes());
        }
    }

    [Obsolete]
    public class TLChannelParticipantEditor : TLChannelParticipantBase, IChannelInviter
    {
        public const uint Signature = TLConstructors.TLChannelParticipantEditor;

        public TLInt InviterId { get; set; }

        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            InviterId = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            InviterId = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(UserId.ToBytes());
            output.Write(InviterId.ToBytes());
            output.Write(Date.ToBytes());
        }
    }

    [Obsolete]
    public class TLChannelParticipantKicked : TLChannelParticipantBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantKicked;

        public TLInt KickedBy { get; set; }

        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            KickedBy = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            KickedBy = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(UserId.ToBytes());
            output.Write(KickedBy.ToBytes());
            output.Write(Date.ToBytes());
        }
    }

    public class TLChannelParticipantCreator : TLChannelParticipantBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantCreator;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            
            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);
            
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(UserId.ToBytes());
        }
    }

    public class TLChannelParticipantAdmin : TLChannelParticipantBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantAdmin;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool CanEdit
        {
            get { return IsSet(Flags, (int)ChannelParticipantAdminFlags.CanEdit); }
            set { SetUnset(ref _flags, value, (int)ChannelParticipantAdminFlags.CanEdit); }
        }

        public TLInt InviterId { get; set; }

        public TLInt PromotedById { get; set; }

        public TLInt Date { get; set; }

        public TLChannelAdminRights AdminRights { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            InviterId = GetObject<TLInt>(bytes, ref position);
            PromotedById = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            AdminRights = GetObject<TLChannelAdminRights>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            UserId = GetObject<TLInt>(input);
            InviterId = GetObject<TLInt>(input);
            PromotedById = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);
            AdminRights = GetObject<TLChannelAdminRights>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            UserId.ToStream(output);
            InviterId.ToStream(output);
            PromotedById.ToStream(output);
            Date.ToStream(output);
            AdminRights.ToStream(output);
        }
    }

    public class TLChannelParticipantBanned : TLChannelParticipantKicked
    {
        public new const uint Signature = TLConstructors.TLChannelParticipantBanned;

        public TLInt Flags { get; set; }

        public bool Left { get { return IsSet(Flags, (int) ChannelParticipantBannedFlags.Left); } }

        public TLChannelBannedRights BannedRights { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            KickedBy = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            BannedRights = GetObject<TLChannelBannedRights>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            UserId = GetObject<TLInt>(input);
            KickedBy = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);
            BannedRights = GetObject<TLChannelBannedRights>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            UserId.ToStream(output);
            KickedBy.ToStream(output);
            Date.ToStream(output);
            BannedRights.ToStream(output);
        }
    }
}

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
    public enum ChannelBannedRightsFlags
    {
        ViewMessages = 0x1,
        SendMessages = 0x2,
        SendMedia = 0x4,
        SendStickers = 0x8,
        SendGifs = 0x10,
        SendGames = 0x20,
        SendInline = 0x40,
        EmbedLinks = 0x80,
    }

    public class TLChannelBannedRights : TLObject
    {
        public const uint Signature = TLConstructors.TLChannelBannedRights;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInt UntilDate { get; set; }

        public bool ViewMessages
        {
            get { return IsSet(_flags, (int)ChannelBannedRightsFlags.ViewMessages); }
            set { SetUnset(ref _flags, value, (int)ChannelBannedRightsFlags.ViewMessages); }
        }

        public bool SendMessages
        {
            get { return IsSet(_flags, (int)ChannelBannedRightsFlags.SendMessages); }
            set { SetUnset(ref _flags, value, (int)ChannelBannedRightsFlags.SendMessages); }
        }

        public bool SendMedia
        {
            get { return IsSet(_flags, (int)ChannelBannedRightsFlags.SendMedia); }
            set { SetUnset(ref _flags, value, (int)ChannelBannedRightsFlags.SendMedia); }
        }

        public bool SendStickers
        {
            get { return IsSet(_flags, (int)ChannelBannedRightsFlags.SendStickers); }
            set { SetUnset(ref _flags, value, (int)ChannelBannedRightsFlags.SendStickers); }
        }

        public bool SendGifs
        {
            get { return IsSet(_flags, (int)ChannelBannedRightsFlags.SendGifs); }
            set { SetUnset(ref _flags, value, (int)ChannelBannedRightsFlags.SendGifs); }
        }

        public bool SendGames
        {
            get { return IsSet(_flags, (int)ChannelBannedRightsFlags.SendGames); }
            set { SetUnset(ref _flags, value, (int)ChannelBannedRightsFlags.SendGames); }
        }

        public bool SendInline
        {
            get { return IsSet(_flags, (int)ChannelBannedRightsFlags.SendInline); }
            set { SetUnset(ref _flags, value, (int)ChannelBannedRightsFlags.SendInline); }
        }

        public bool EmbedLinks
        {
            get { return IsSet(_flags, (int)ChannelBannedRightsFlags.EmbedLinks); }
            set { SetUnset(ref _flags, value, (int)ChannelBannedRightsFlags.EmbedLinks); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            UntilDate = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            UntilDate = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            UntilDate.ToStream(output);
        }
    }
}

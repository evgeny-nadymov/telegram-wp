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
    public enum BotInlineMessageFlags
    {
        NoWebpage = 0x1,            // 0
        Entities = 0x2,             // 1
        ReplyMarkup = 0x4,          // 2
    }

    public abstract class TLBotInlineMessageBase : TLObject
    {
        public TLReplyKeyboardBase ReplyMarkup { get; set; }

        public static string BotInlineMessageFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (BotInlineMessageFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }
    }

    public class TLBotInlineMessageMediaAuto75 : TLBotInlineMessageMediaAuto51
    {
        public new const uint Signature = TLConstructors.TLBotInlineMessageMediaAuto75;

        protected TLVector<TLMessageEntityBase> _entities;

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set { SetField(out _entities, value, ref _flags, (int)InputBotInlineMessageFlags.Entities); }
        }

        public override string ToString()
        {
            return string.Format("TLBotInlineMessageMediaAuto75 flags={0} caption={1} reply_markup={2}", BotInlineMessageFlagsString(Flags), Caption, ReplyMarkup);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Caption = GetObject<TLString>(bytes, ref position);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)BotInlineMessageFlags.Entities, null, bytes, ref position);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Caption.ToBytes(),
                ToBytes(Entities, Flags, (int)InputBotInlineMessageFlags.Entities),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Caption = GetObject<TLString>(input);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)BotInlineMessageFlags.Entities, null, input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, Entities, Flags, (int)BotInlineMessageFlags.Entities);
            ToStream(output, ReplyMarkup, Flags, (int)BotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLBotInlineMessageMediaAuto51 : TLBotInlineMessageMediaAuto
    {
        public new const uint Signature = TLConstructors.TLBotInlineMessageMediaAuto51;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override string ToString()
        {
            return string.Format("TLBotInlineMessageMediaAuto51 flags={0} caption={1} reply_markup={2}", BotInlineMessageFlagsString(Flags), Caption, ReplyMarkup);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Caption = GetObject<TLString>(bytes, ref position);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Caption.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Caption = GetObject<TLString>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)BotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLBotInlineMessageMediaAuto : TLBotInlineMessageBase
    {
        public const uint Signature = TLConstructors.TLBotInlineMessageMediaAuto;

        public TLString Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Caption = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Caption = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Caption.ToStream(output);
        }
    }

    public class TLBotInlineMessageText51 : TLBotInlineMessageText
    {
        public new const uint Signature = TLConstructors.TLBotInlineMessageText51;

        public override string ToString()
        {
            return string.Format("TLBotInlineMessageText51 flags={0} message={1} entities={2} reply_markup={3}", BotInlineMessageFlagsString(Flags), Message, Entities, ReplyMarkup);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)BotInlineMessageFlags.Entities, null, bytes, ref position);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Message.ToBytes(),
                ToBytes(Entities, Flags, (int)InputBotInlineMessageFlags.Entities),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)BotInlineMessageFlags.Entities, null, input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Message.ToStream(output);
            ToStream(output, Entities, Flags, (int)InputBotInlineMessageFlags.Entities);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLBotInlineMessageText : TLBotInlineMessageBase
    {
        public const uint Signature = TLConstructors.TLBotInlineMessageText;

        public TLInt Flags { get; set; }

        public bool NoWebpage { get { return IsSet(Flags, (int)BotInlineMessageFlags.NoWebpage); } }

        public TLString Message { get; set; }

        public TLVector<TLMessageEntityBase> Entities { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);

            if (IsSet(Flags, (int)BotInlineMessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);
            }

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Message.ToBytes(),
                ToBytes(Entities, Flags, (int)InputBotInlineMessageFlags.Entities));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            if (IsSet(Flags, (int)InputBotInlineMessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(input);
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Message.ToStream(output);
            ToStream(output, Entities, Flags, (int)InputBotInlineMessageFlags.Entities);
        }
    }

    public class TLBotInlineMessageMediaGeo : TLBotInlineMessageBase
    {
        public const uint Signature = TLConstructors.TLBotInlineMessageMediaGeo;

        public TLInt Flags { get; set; }

        public TLGeoPointBase Geo { get; set; }

        public override string ToString()
        {
            return string.Format("TLBotInlineMessageMediaGeo flags={0} geo={1} reply_markup={2}", BotInlineMessageFlagsString(Flags), Geo, ReplyMarkup);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Geo = GetObject<TLGeoPointBase>(bytes, ref position);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Geo.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Geo = GetObject<TLGeoPointBase>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Geo.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLBotInlineMessageMediaVenue78 : TLBotInlineMessageMediaVenue
    {
        public new const uint Signature = TLConstructors.TLBotInlineMessageMediaVenue78;

        public TLString VenueType { get; set; }

        public override string ToString()
        {
            return string.Format("TLBotInlineMessageMediaVenue flags={0} geo={1} title={2} address={3} provider={4} venue_id={5} venue_type={6} reply_markup={7}", BotInlineMessageFlagsString(Flags), Geo, Title, Address, Provider, VenueId, VenueType, ReplyMarkup);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Geo = GetObject<TLGeoPointBase>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Address = GetObject<TLString>(bytes, ref position);
            Provider = GetObject<TLString>(bytes, ref position);
            VenueId = GetObject<TLString>(bytes, ref position);
            VenueType = GetObject<TLString>(bytes, ref position);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Geo.ToBytes(),
                Title.ToBytes(),
                Address.ToBytes(),
                Provider.ToBytes(),
                VenueId.ToBytes(),
                VenueType.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Geo = GetObject<TLGeoPointBase>(input);
            Title = GetObject<TLString>(input);
            Address = GetObject<TLString>(input);
            Provider = GetObject<TLString>(input);
            VenueId = GetObject<TLString>(input);
            VenueType = GetObject<TLString>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Geo.ToStream(output);
            Title.ToStream(output);
            Address.ToStream(output);
            Provider.ToStream(output);
            VenueId.ToStream(output);
            VenueType.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLBotInlineMessageMediaVenue : TLBotInlineMessageMediaGeo
    {
        public new const uint Signature = TLConstructors.TLBotInlineMessageMediaVenue;

        public TLString Title { get; set; }

        public TLString Address { get; set; }

        public TLString Provider { get; set; }

        public TLString VenueId { get; set; }

        public override string ToString()
        {
            return string.Format("TLBotInlineMessageMediaVenue flags={0} geo={1} title={2} address={3} provider={4} venue_id={5} reply_markup={6}", BotInlineMessageFlagsString(Flags), Geo, Title, Address, Provider, VenueId, ReplyMarkup);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Geo = GetObject<TLGeoPointBase>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Address = GetObject<TLString>(bytes, ref position);
            Provider = GetObject<TLString>(bytes, ref position);
            VenueId = GetObject<TLString>(bytes, ref position);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Geo.ToBytes(),
                Title.ToBytes(),
                Address.ToBytes(),
                Provider.ToBytes(),
                VenueId.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Geo = GetObject<TLGeoPointBase>(input);
            Title = GetObject<TLString>(input);
            Address = GetObject<TLString>(input);
            Provider = GetObject<TLString>(input);
            VenueId = GetObject<TLString>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Geo.ToStream(output);
            Title.ToStream(output);
            Address.ToStream(output);
            Provider.ToStream(output);
            VenueId.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLBotInlineMessageMediaContact82 : TLBotInlineMessageMediaContact
    {
        public new const uint Signature = TLConstructors.TLBotInlineMessageMediaContact82;

        public TLString VCard { get; set; }

        public override string ToString()
        {
            return string.Format("TLBotInlineMessageMediaContact82 flags={0} phone_number={1} first_name={2} last_name={3} vcard={4} reply_markup={5}", BotInlineMessageFlagsString(Flags), PhoneNumber, FirstName, LastName, VCard, ReplyMarkup);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            PhoneNumber = GetObject<TLString>(bytes, ref position);
            FirstName = GetObject<TLString>(bytes, ref position);
            LastName = GetObject<TLString>(bytes, ref position);
            VCard = GetObject<TLString>(bytes, ref position);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                PhoneNumber.ToBytes(),
                FirstName.ToBytes(),
                LastName.ToBytes(),
                VCard.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            PhoneNumber = GetObject<TLString>(input);
            FirstName = GetObject<TLString>(input);
            LastName = GetObject<TLString>(input);
            VCard = GetObject<TLString>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            PhoneNumber.ToStream(output);
            FirstName.ToStream(output);
            LastName.ToStream(output);
            VCard.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLBotInlineMessageMediaContact : TLBotInlineMessageBase
    {
        public const uint Signature = TLConstructors.TLBotInlineMessageMediaContact;

        public TLInt Flags { get; set; }

        public TLString PhoneNumber { get; set; }

        public TLString FirstName { get; set; }

        public TLString LastName { get; set; }

        public TLUserBase User
        {
            get { return new TLUser { Id = new TLInt(0), Photo = new TLPhotoEmpty { Id = new TLLong(0) }, FirstName = FirstName, LastName = LastName, Phone = PhoneNumber }; }
        }

        public override string ToString()
        {
            return string.Format("TLBotInlineMessageMediaContact flags={0} phone_number={1} first_name={2} last_name={3} reply_markup={4}", BotInlineMessageFlagsString(Flags), PhoneNumber, FirstName, LastName, ReplyMarkup);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            PhoneNumber = GetObject<TLString>(bytes, ref position);
            FirstName = GetObject<TLString>(bytes, ref position);
            LastName = GetObject<TLString>(bytes, ref position);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                PhoneNumber.ToBytes(),
                FirstName.ToBytes(),
                LastName.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            PhoneNumber = GetObject<TLString>(input);
            FirstName = GetObject<TLString>(input);
            LastName = GetObject<TLString>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)BotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            PhoneNumber.ToStream(output);
            FirstName.ToStream(output);
            LastName.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }
}

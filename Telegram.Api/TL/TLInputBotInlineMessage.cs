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
    public enum InputBotInlineMessageFlags
    {
        NoWebpage = 0x1,            // 0
        Entities = 0x2,             // 1
        ReplyMarkup = 0x4,          // 2
    }

    public abstract class TLInputBotInlineMessageBase : TLObject
    {
        public TLReplyKeyboardBase ReplyMarkup { get; set; }
    }

    public class TLInputBotInlineMessageMediaAuto75 : TLInputBotInlineMessageMediaAuto51
    {
        public new const uint Signature = TLConstructors.TLInputBotInlineMessageMediaAuto75;

        protected TLVector<TLMessageEntityBase> _entities;

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set { SetField(out _entities, value, ref _flags, (int)InputBotInlineMessageFlags.Entities); }
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
            Caption = TLString.Empty;
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)InputBotInlineMessageFlags.Entities, null, input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)InputBotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLInputBotInlineMessageMediaAuto51 : TLInputBotInlineMessageMediaAuto
    {
        public new const uint Signature = TLConstructors.TLInputBotInlineMessageMediaAuto51;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
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
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)InputBotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLInputBotInlineMessageMediaAuto : TLInputBotInlineMessageBase
    {
        public const uint Signature = TLConstructors.TLInputBotInlineMessageMediaAuto;

        public TLString Caption { get; set; }

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

    public class TLInputBotInlineMessageText51 : TLInputBotInlineMessageText
    {
        public new const uint Signature = TLConstructors.TLInputBotInlineMessageText51;

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(NoWebpage, Flags, (int)InputBotInlineMessageFlags.NoWebpage),
                Message.ToBytes(),
                ToBytes(Entities, Flags, (int)InputBotInlineMessageFlags.Entities),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            NoWebpage = GetObject<TLBool>(Flags, (int)InputBotInlineMessageFlags.NoWebpage, null, input);
            Message = GetObject<TLString>(input);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)InputBotInlineMessageFlags.Entities, null, input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)InputBotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, NoWebpage, Flags, (int)InputBotInlineMessageFlags.NoWebpage);
            Message.ToStream(output);
            ToStream(output, Entities, Flags, (int)InputBotInlineMessageFlags.Entities);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLInputBotInlineMessageText : TLInputBotInlineMessageBase
    {
        public const uint Signature = TLConstructors.TLInputBotInlineMessageText;

        public TLInt Flags { get; set; }

        public TLBool NoWebpage { get; set; }

        public TLString Message { get; set; }

        public TLVector<TLMessageEntityBase> Entities { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(NoWebpage, Flags, (int)InputBotInlineMessageFlags.NoWebpage),
                Message.ToBytes(),
                ToBytes(Entities, Flags, (int)InputBotInlineMessageFlags.Entities));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            NoWebpage = GetObject<TLBool>(Flags, (int)InputBotInlineMessageFlags.NoWebpage, null, input);
            Message = GetObject<TLString>(input);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)InputBotInlineMessageFlags.Entities, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, NoWebpage, Flags, (int)InputBotInlineMessageFlags.NoWebpage);
            Message.ToStream(output);
            ToStream(output, Entities, Flags, (int)InputBotInlineMessageFlags.Entities);
        }
    }

    public class TLInputBotInlineMessageMediaGeo : TLInputBotInlineMessageBase
    {
        public const uint Signature = TLConstructors.TLInputBotInlineMessageMediaGeo;

        public TLInt Flags { get; set; }

        public TLGeoPointBase GeoPoint { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                GeoPoint.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            GeoPoint = GetObject<TLGeoPointBase>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)InputBotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            GeoPoint.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLInputBotInlineMessageMediaVenue78 : TLInputBotInlineMessageMediaVenue
    {
        public new const uint Signature = TLConstructors.TLInputBotInlineMessageMediaVenue78;

        public TLString VenueType { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                GeoPoint.ToBytes(),
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
            GeoPoint = GetObject<TLGeoPointBase>(input);
            Title = GetObject<TLString>(input);
            Address = GetObject<TLString>(input);
            Provider = GetObject<TLString>(input);
            VenueId = GetObject<TLString>(input);
            VenueType = GetObject<TLString>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)InputBotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            GeoPoint.ToStream(output);
            Title.ToStream(output);
            Address.ToStream(output);
            Provider.ToStream(output);
            VenueId.ToStream(output);
            VenueType.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLInputBotInlineMessageMediaVenue : TLInputBotInlineMessageMediaGeo
    {
        public new const uint Signature = TLConstructors.TLInputBotInlineMessageMediaVenue;

        public TLString Title { get; set; }

        public TLString Address { get; set; }

        public TLString Provider { get; set; }

        public TLString VenueId { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                GeoPoint.ToBytes(),
                Title.ToBytes(),
                Address.ToBytes(),
                Provider.ToBytes(),
                VenueId.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            GeoPoint = GetObject<TLGeoPointBase>(input);
            Title = GetObject<TLString>(input);
            Address = GetObject<TLString>(input);
            Provider = GetObject<TLString>(input);
            VenueId = GetObject<TLString>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)InputBotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            GeoPoint.ToStream(output);
            Title.ToStream(output);
            Address.ToStream(output);
            Provider.ToStream(output);
            VenueId.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }

    public class TLInputBotInlineMessageMediaContact82 : TLInputBotInlineMessageMediaContact
    {
        public new const uint Signature = TLConstructors.TLInputBotInlineMessageMediaContact82;

        public TLString VCard { get; set; }

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
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)InputBotInlineMessageFlags.ReplyMarkup, null, input);

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

    public class TLInputBotInlineMessageMediaContact : TLInputBotInlineMessageBase
    {
        public const uint Signature = TLConstructors.TLInputBotInlineMessageMediaContact;

        public TLInt Flags { get; set; }

        public TLString PhoneNumber { get; set; }

        public TLString FirstName { get; set; }

        public TLString LastName { get; set; }

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
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)InputBotInlineMessageFlags.ReplyMarkup, null, input);

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

    public class TLInputBotInlineMessageGame : TLInputBotInlineMessageBase
    {
        public const uint Signature = TLConstructors.TLInputBotInlineMessageGame;

        public TLInt Flags { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            ReplyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)InputBotInlineMessageFlags.ReplyMarkup, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, ReplyMarkup, Flags, (int)InputBotInlineMessageFlags.ReplyMarkup);
        }
    }
}

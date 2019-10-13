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
    public enum InputSingleMediaFlags
    {
        Entities = 0x1, // 0
    }

    public class TLInputSingleMedia76 : TLInputSingleMedia75
    {
        public new const uint Signature = TLConstructors.TLInputSingleMedia76;

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Media.ToBytes(),
                RandomId.ToBytes(),
                Message.ToBytes(),
                ToBytes(Entities, Flags, (int)InputSingleMediaFlags.Entities));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Media = GetObject<TLInputMediaBase>(input);
            RandomId = GetObject<TLLong>(input);
            Message = GetObject<TLString>(input);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)InputSingleMediaFlags.Entities, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Media.ToStream(output);
            RandomId.ToStream(output);
            Message.ToStream(output);
            ToStream(output, Entities, Flags, (int)InputSingleMediaFlags.Entities);
        }
    }

    public class TLInputSingleMedia75 : TLInputSingleMedia
    {
        public new const uint Signature = TLConstructors.TLInputSingleMedia75;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLString Message { get; set; }

        protected TLVector<TLMessageEntityBase> _entities;

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set { SetField(out _entities, value, ref _flags, (int) InputSingleMediaFlags.Entities); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Media.ToBytes(),
                Flags.ToBytes(),
                RandomId.ToBytes(),
                Message.ToBytes(),
                ToBytes(Entities, Flags, (int) InputSingleMediaFlags.Entities));
        }

        public override TLObject FromStream(Stream input)
        {
            Media = GetObject<TLInputMediaBase>(input);
            Flags = GetObject<TLInt>(input);
            RandomId = GetObject<TLLong>(input);
            Message = GetObject<TLString>(input);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int) InputSingleMediaFlags.Entities, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Media.ToStream(output);
            Flags.ToStream(output);
            RandomId.ToStream(output);
            Message.ToStream(output);
            ToStream(output, Entities, Flags, (int) InputSingleMediaFlags.Entities);
        }
    }

    public class TLInputSingleMedia : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputSingleMedia;

        public TLInputMediaBase Media { get; set; }

        public TLLong RandomId { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Media.ToBytes(),
                RandomId.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Media = GetObject<TLInputMediaBase>(input);
            RandomId = GetObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Media.ToStream(output);
            RandomId.ToStream(output);
        }
    }
}

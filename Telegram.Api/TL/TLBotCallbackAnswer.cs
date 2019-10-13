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
    public enum BotCallbackAnswer
    {
        Message = 0x1,      // 0
        Alert = 0x2,        // 1
        Url = 0x4,          // 2
        HasUrl = 0x8,       // 3
    }

    public class TLBotCallbackAnswer : TLObject
    {
        public const uint Signature = TLConstructors.TLBotCallbackAnswer;

        public TLInt Flags { get; set; }

        public TLString Message { get; set; }

        public bool Alert { get { return IsSet(Flags, (int) BotCallbackAnswer.Alert); } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(Flags, (int)BotCallbackAnswer.Message, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Message.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Message = GetObject<TLString>(Flags, (int)BotCallbackAnswer.Message, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            ToBytes(Message, Flags, (int) BotCallbackAnswer.Message);
        }
    }

    public class TLBotCallbackAnswer54 : TLBotCallbackAnswer
    {
        public new const uint Signature = TLConstructors.TLBotCallbackAnswer54;

        public TLString Url { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(Flags, (int) BotCallbackAnswer.Message, null, bytes, ref position);
            Url = GetObject<TLString>(Flags, (int) BotCallbackAnswer.Url, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Message.ToBytes(),
                Url.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Message = GetObject<TLString>(Flags, (int) BotCallbackAnswer.Message, null, input);
            Url = GetObject<TLString>(Flags, (int) BotCallbackAnswer.Url, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            ToBytes(Message, Flags, (int) BotCallbackAnswer.Message);
            ToBytes(Url, Flags, (int) BotCallbackAnswer.Url);
        }
    }

    public class TLBotCallbackAnswer58 : TLBotCallbackAnswer54, ICachedObject
    {
        public new const uint Signature = TLConstructors.TLBotCallbackAnswer58;

        public TLInt CacheTime { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(Flags, (int)BotCallbackAnswer.Message, null, bytes, ref position);
            Url = GetObject<TLString>(Flags, (int)BotCallbackAnswer.Url, null, bytes, ref position);
            CacheTime = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(Message, Flags, (int)BotCallbackAnswer.Message),
                ToBytes(Url, Flags, (int)BotCallbackAnswer.Url),
                CacheTime.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Message = GetObject<TLString>(Flags, (int)BotCallbackAnswer.Message, null, input);
            Url = GetObject<TLString>(Flags, (int)BotCallbackAnswer.Url, null, input);
            CacheTime = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            ToStream(output, Message, Flags, (int) BotCallbackAnswer.Message);
            ToStream(output, Url, Flags, (int) BotCallbackAnswer.Url);
            output.Write(CacheTime.ToBytes());
        }
    }

    public interface ICachedObject
    {
        TLInt CacheTime { get; set; }
    }
}
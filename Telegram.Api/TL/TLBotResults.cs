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
    public enum BotResultsFlags
    {
        Gallery = 0x1,          // 0
        NextOffset = 0x2,       // 1
        SwitchPM = 0x4,         // 2
    }

    public class TLBotResults72 : TLBotResults58
    {
        public new const uint Signature = TLConstructors.TLBotResults72;

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            QueryId = GetObject<TLLong>(bytes, ref position);
            NextOffset = GetObject<TLString>(Flags, (int)BotResultsFlags.NextOffset, null, bytes, ref position);
            SwitchPM = GetObject<TLInlineBotSwitchPM>(Flags, (int)BotResultsFlags.SwitchPM, null, bytes, ref position);
            Results = GetObject<TLVector<TLBotInlineResultBase>>(bytes, ref position);
            CacheTime = GetObject<TLInt>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                QueryId.ToBytes(),
                ToBytes(NextOffset, Flags, (int)BotResultsFlags.NextOffset),
                ToBytes(SwitchPM, Flags, (int)BotResultsFlags.SwitchPM),
                Results.ToBytes(),
                CacheTime.ToBytes(),
                Users.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            QueryId = GetObject<TLLong>(input);
            NextOffset = GetObject<TLString>(Flags, (int)BotResultsFlags.NextOffset, null, input);
            SwitchPM = GetObject<TLInlineBotSwitchPM>(Flags, (int)BotResultsFlags.SwitchPM, null, input);
            Results = GetObject<TLVector<TLBotInlineResultBase>>(input);
            CacheTime = GetObject<TLInt>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            QueryId.ToStream(output);
            ToStream(output, NextOffset, Flags, (int)BotResultsFlags.NextOffset);
            ToStream(output, SwitchPM, Flags, (int)BotResultsFlags.SwitchPM);
            Results.ToStream(output);
            CacheTime.ToStream(output);
            Users.ToStream(output);
        }
    }

    public class TLBotResults58 : TLBotResults51, ICachedObject
    {
        public new const uint Signature = TLConstructors.TLBotResults58;

        public TLInt CacheTime { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            QueryId = GetObject<TLLong>(bytes, ref position);
            NextOffset = GetObject<TLString>(Flags, (int)BotResultsFlags.NextOffset, null, bytes, ref position);
            SwitchPM = GetObject<TLInlineBotSwitchPM>(Flags, (int)BotResultsFlags.SwitchPM, null, bytes, ref position);
            Results = GetObject<TLVector<TLBotInlineResultBase>>(bytes, ref position);
            CacheTime = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                QueryId.ToBytes(),
                ToBytes(NextOffset, Flags, (int)BotResultsFlags.NextOffset),
                ToBytes(SwitchPM, Flags, (int)BotResultsFlags.SwitchPM),
                Results.ToBytes(),
                CacheTime.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            QueryId = GetObject<TLLong>(input);
            NextOffset = GetObject<TLString>(Flags, (int)BotResultsFlags.NextOffset, null, input);
            SwitchPM = GetObject<TLInlineBotSwitchPM>(Flags, (int)BotResultsFlags.SwitchPM, null, input);
            Results = GetObject<TLVector<TLBotInlineResultBase>>(input);
            CacheTime = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            QueryId.ToStream(output);
            ToStream(output, NextOffset, Flags, (int)BotResultsFlags.NextOffset);
            ToStream(output, SwitchPM, Flags, (int)BotResultsFlags.SwitchPM);
            Results.ToStream(output);
            CacheTime.ToStream(output);
        }
    }

    public class TLBotResults51 : TLBotResults
    {
        public new const uint Signature = TLConstructors.TLBotResults51;

        public TLInlineBotSwitchPM SwitchPM { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            QueryId = GetObject<TLLong>(bytes, ref position);
            NextOffset = GetObject<TLString>(Flags, (int)BotResultsFlags.NextOffset, null, bytes, ref position);
            SwitchPM = GetObject<TLInlineBotSwitchPM>(Flags, (int)BotResultsFlags.SwitchPM, null, bytes, ref position);
            Results = GetObject<TLVector<TLBotInlineResultBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                QueryId.ToBytes(),
                ToBytes(NextOffset, Flags, (int)BotResultsFlags.NextOffset),
                ToBytes(SwitchPM, Flags, (int)BotResultsFlags.SwitchPM),
                Results.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            QueryId = GetObject<TLLong>(input);
            NextOffset = GetObject<TLString>(Flags, (int)BotResultsFlags.NextOffset, null, input);
            SwitchPM = GetObject<TLInlineBotSwitchPM>(Flags, (int)BotResultsFlags.SwitchPM, null, input);
            Results = GetObject<TLVector<TLBotInlineResultBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            QueryId.ToStream(output);
            ToStream(output, NextOffset, Flags, (int)BotResultsFlags.NextOffset);
            ToStream(output, SwitchPM, Flags, (int)BotResultsFlags.SwitchPM);
            Results.ToStream(output);
        }
    }

    public class TLBotResults : TLBotInlineResultBase
    {
        public const uint Signature = TLConstructors.TLBotResults;

        public TLInt Flags { get; set; }

        public bool Gallery { get { return IsSet(Flags, (int)BotResultsFlags.Gallery); } }

        public TLString NextOffset { get; set; }

        public TLVector<TLBotInlineResultBase> Results { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            QueryId = GetObject<TLLong>(bytes, ref position);
            NextOffset = GetObject<TLString>(Flags, (int) BotResultsFlags.NextOffset, null, bytes, ref position);
            Results = GetObject<TLVector<TLBotInlineResultBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                QueryId.ToBytes(),
                ToBytes(NextOffset, Flags, (int)BotResultsFlags.NextOffset),
                Results.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            QueryId = GetObject<TLLong>(input);
            NextOffset = GetObject<TLString>(Flags, (int)BotResultsFlags.NextOffset, null, input);
            Results = GetObject<TLVector<TLBotInlineResultBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            QueryId.ToStream(output);
            ToStream(output, NextOffset, Flags, (int)BotResultsFlags.NextOffset);
            Results.ToStream(output);
        }
    }
}

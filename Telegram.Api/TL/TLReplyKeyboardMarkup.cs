// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    enum ReplyKeyboardFlags
    {
        Resize = 0x1,
        SingleUse = 0x2,
        Personal = 0x4
    }

    enum ReplyKeyboardCustomFlags
    {
        HasResponse = 0x1,
    }

    public interface IReplyKeyboardRows
    {
        TLVector<TLKeyboardButtonRow> Rows { get; set; }
    }

    public abstract class TLReplyKeyboardBase : TLObject
    {
        public TLInt Flags { get; set; }

        private TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        public bool IsResizable
        {
            get { return IsSet(Flags, (int)ReplyKeyboardFlags.Resize); }
        }

        public bool IsSingleUse
        {
            get { return IsSet(Flags, (int)ReplyKeyboardFlags.SingleUse); }
        }

        public bool IsPersonal
        {
            get { return IsSet(Flags, (int)ReplyKeyboardFlags.Personal); }
        }

        public bool HasResponse
        {
            get { return IsSet(CustomFlags, (int) ReplyKeyboardCustomFlags.HasResponse); }
            set { Set(ref _customFlags, (int) ReplyKeyboardCustomFlags.HasResponse);}
        }

        public override string ToString()
        {
            var isPersonal = IsPersonal ? "p" : string.Empty;
            var isResizable = IsResizable ? "r" : string.Empty;
            var isSingleUse = IsSingleUse ? "s" : string.Empty;
            var hasResponse = HasResponse ? "h" : string.Empty;
            return string.Format("{0} {1} {2} {3}", isPersonal, isResizable, isSingleUse, hasResponse);
        }
    }

    public class TLReplyInlineMarkup : TLReplyKeyboardBase, IReplyKeyboardRows
    {
        public const uint Signature = TLConstructors.TLReplyInlineMarkup;

        public TLVector<TLKeyboardButtonRow> Rows { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Rows = GetObject<TLVector<TLKeyboardButtonRow>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Rows.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Rows = GetObject<TLVector<TLKeyboardButtonRow>>(input);

            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Rows.ToBytes());

            CustomFlags.NullableToStream(output);
        }

        public override string ToString()
        {
            var rowsString = new List<string>();
            foreach (var row in Rows)
            {
                rowsString.Add(row.Buttons.Count.ToString());
            }

            return "IM " + string.Join(" ", rowsString) + base.ToString();
        }
    }

    public class TLReplyKeyboardMarkup : TLReplyKeyboardBase, IReplyKeyboardRows
    {
        public const uint Signature = TLConstructors.TLReplyKeyboardMarkup;

        public TLVector<TLKeyboardButtonRow> Rows { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Rows = GetObject<TLVector<TLKeyboardButtonRow>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Rows.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Rows = GetObject<TLVector<TLKeyboardButtonRow>>(input);

            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            output.Write(Rows.ToBytes());

            CustomFlags.NullableToStream(output);
        }

        public override string ToString()
        {
            var rowsString = new List<string>();
            foreach (var row in Rows)
            {
                rowsString.Add(row.Buttons.Count.ToString());
            }

            return "KM " + string.Join(" ", rowsString) + base.ToString();
        }
    }

    public class TLReplyKeyboardHide : TLReplyKeyboardBase
    {
        public const uint Signature = TLConstructors.TLReplyKeyboardHide;

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

            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());

            CustomFlags.NullableToStream(output);
        }

        public override string ToString()
        {
            return "KH " + base.ToString();
        }
    }

    public class TLReplyKeyboardForceReply : TLReplyKeyboardBase
    {
        public const uint Signature = TLConstructors.TLReplyKeyboardForceReply;

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

            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());

            CustomFlags.NullableToStream(output);
        }

        public override string ToString()
        {
            return "KFR " + base.ToString();
        }
    }
}

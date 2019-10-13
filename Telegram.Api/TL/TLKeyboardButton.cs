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
    public enum KeyboardButtonSwitchInlineFlags
    {
        SamePeer = 0x1,          // 0
    }

    public abstract class TLKeyboardButtonBase : TLObject
    {
        public TLString Text { get; set; }
    }

    public class TLKeyboardButton : TLKeyboardButtonBase
    {
        public const uint Signature = TLConstructors.TLKeyboardButton;

        public TLKeyboardButton() { }

        public TLKeyboardButton(TLString text)
        {
            Text = text;
        }

        public override string ToString()
        {
            return string.Format("TLKeyboardButton text={0}", Text);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Text.ToBytes());
        }
    }

    public class TLKeyboardButtonUrl : TLKeyboardButtonBase
    {
        public const uint Signature = TLConstructors.TLKeyboardButtonUrl;

        public TLString Url { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);
            Url = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLKeyboardButtonUrl text={0} url={1}", Text, Url);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes(),
                Url.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLString>(input);
            Url = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Text.ToBytes());
            output.Write(Url.ToBytes());
        }
    }

    public class TLKeyboardButtonCallback : TLKeyboardButtonBase
    {
        public const uint Signature = TLConstructors.TLKeyboardButtonCallback;

        public TLString Data { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);
            Data = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLKeyboardButtonCallback text={0} data={1}", Text, Data);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes(),
                Data.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLString>(input);
            Data = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Text.ToBytes());
            output.Write(Data.ToBytes());
        }
    }

    public class TLKeyboardButtonRequestPhone : TLKeyboardButtonBase
    {
        public const uint Signature = TLConstructors.TLKeyboardButtonRequestPhone;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLKeyboardButtonRequestPhone text={0}", Text);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Text.ToBytes());
        }
    }

    public class TLKeyboardButtonRequestGeoLocation : TLKeyboardButtonBase
    {
        public const uint Signature = TLConstructors.TLKeyboardButtonRequestGeoLocation;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLKeyboardButtonRequestLocation text={0}", Text);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Text.ToBytes());
        }
    }

    public class TLKeyboardButtonSwitchInline : TLKeyboardButtonBase
    {
        public const uint Signature = TLConstructors.TLKeyboardButtonSwitchInline;

        public TLString Query { get; set; }

        #region Additional
        public TLUser Bot { get; set; }
        #endregion

        public override string ToString()
        {
            return string.Format("TLKeyboardButtonSwitchInline text={0} query={1}", Text, Query);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);
            Query = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes(),
                Query.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLString>(input);
            Query = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Text.ToBytes());
            output.Write(Query.ToBytes());
        }
    }

    public class TLKeyboardButtonSwitchInline55 : TLKeyboardButtonSwitchInline
    {
        public new const uint Signature = TLConstructors.TLKeyboardButtonSwitchInline55;

        public TLInt Flags { get; set; }

        public bool IsSamePeer { get { return IsSet(Flags, (int) KeyboardButtonSwitchInlineFlags.SamePeer); } }

        public static string KeyboardButtonSwitchInlineFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (KeyboardButtonSwitchInlineFlags) flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public override string ToString()
        {
            return string.Format("TLKeyboardButtonSwitchInline55 flags={0} text={1} query={2}", KeyboardButtonSwitchInlineFlagsString(Flags), Text, Query);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);
            Query = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Text.ToBytes(),
                Query.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Text = GetObject<TLString>(input);
            Query = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            output.Write(Text.ToBytes());
            output.Write(Query.ToBytes());
        }
    }

    public class TLKeyboardButtonGame : TLKeyboardButtonBase
    {
        public const uint Signature = TLConstructors.TLKeyboardButtonGame;

        public override string ToString()
        {
            return string.Format("TLKeyboardButtonGame text={0}", Text);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Text.ToBytes());
        }
    }

    public class TLKeyboardButtonBuy : TLKeyboardButtonBase
    {
        public const uint Signature = TLConstructors.TLKeyboardButtonBuy;

        public override string ToString()
        {
            return string.Format("TLKeyboardButtonBuy text={0}", Text);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Text.ToBytes());
        }
    }
}

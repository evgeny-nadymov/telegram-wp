// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public abstract class TLRichTextBase : TLObject { }

    public class TLTextEmpty : TLRichTextBase
    {
        public const uint Signature = TLConstructors.TLTextEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }
    }

    public class TLTextPlain : TLRichTextBase
    {
        public const uint Signature = TLConstructors.TLTextPlain;

        public TLString Text { get; set; }

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


        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLString>(input);

            return this;
        }
    }

    public class TLTextBold : TLRichTextBase
    {
        public const uint Signature = TLConstructors.TLTextBold;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }


        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLTextItalic : TLRichTextBase
    {
        public const uint Signature = TLConstructors.TLTextItalic;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }


        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLTextUnderline : TLRichTextBase
    {
        public const uint Signature = TLConstructors.TLTextUnderline;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }


        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLTextStrike : TLRichTextBase
    {
        public const uint Signature = TLConstructors.TLTextStrike;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }


        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLTextFixed : TLRichTextBase
    {
        public const uint Signature = TLConstructors.TLTextFixed;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }


        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLTextUrl : TLRichTextBase
    {
        public const uint Signature = TLConstructors.TLTextUrl;

        public TLRichTextBase Text { get; set; }

        public TLString Url { get; set; }

        public TLLong WebPageId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);
            Url = GetObject<TLString>(bytes, ref position);
            WebPageId = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes(),
                Url.ToBytes(),
                WebPageId.ToBytes());
        }


        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
            Url.ToStream(output);
            WebPageId.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);
            Url = GetObject<TLString>(input);
            WebPageId = GetObject<TLLong>(input);

            return this;
        }
    }

    public class TLTextEmail : TLRichTextBase
    {
        public const uint Signature = TLConstructors.TLTextEmail;

        public TLRichTextBase Text { get; set; }

        public TLString Email { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);
            Email = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes(),
                Email.ToBytes());
        }


        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
            Email.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);
            Email = GetObject<TLString>(input);

            return this;
        }
    }

    public class TLTextConcat : TLRichTextBase
    {
        public const uint Signature = TLConstructors.TLTextConcat;

        public TLVector<TLRichTextBase> Texts { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Texts = GetObject<TLVector<TLRichTextBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Texts.ToBytes());
        }


        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Texts.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Texts = GetObject<TLVector<TLRichTextBase>>(input);

            return this;
        }
    }
}

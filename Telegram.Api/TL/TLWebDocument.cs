// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.Linq;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public abstract class TLWebDocumentBase : TLObject, IAttributes
    {
        public TLString Url { get; set; }

        public TLInt Size { get; set; }

        public TLString MimeType { get; set; }

        public TLVector<TLDocumentAttributeBase> Attributes { get; set; }

        public TLInt W
        {
            get
            {
                var attributeSize = Attributes.FirstOrDefault(x => x is IAttributeSize) as IAttributeSize;
                if (attributeSize != null)
                {
                    return attributeSize.W;
                }

                return null;
            }
        }

        public TLInt H
        {
            get
            {
                var attributeSize = Attributes.FirstOrDefault(x => x is IAttributeSize) as IAttributeSize;
                if (attributeSize != null)
                {
                    return attributeSize.H;
                }

                return null;
            }
        }

        public TLInt Duration
        {
            get
            {
                var attributeDuration = Attributes.FirstOrDefault(x => x is IAttributeDuration) as IAttributeDuration;
                if (attributeDuration != null)
                {
                    return attributeDuration.Duration;
                }

                return null;
            }
        }

        public Uri Uri
        {
            get
            {
                if (TLString.IsNullOrEmpty(Url)) return null;

                return new Uri(Url.ToString(), UriKind.Absolute);
            }
        }
    }

    public class TLWebDocument82 : TLWebDocumentBase
    {
        public const uint Signature = TLConstructors.TLWebDocument82;

        public TLLong AccessHash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Url.ToBytes(),
                AccessHash.ToBytes(),
                Size.ToBytes(),
                MimeType.ToBytes(),
                Attributes.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);
            AccessHash = GetObject<TLLong>(input);
            Size = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Url.ToStream(output);
            AccessHash.ToStream(output);
            Size.ToStream(output);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
        }
    }

    public class TLWebDocument : TLWebDocumentBase
    {
        public const uint Signature = TLConstructors.TLWebDocument;

        public TLLong AccessHash { get; set; }

        public TLInt DCId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(bytes, ref position);
            DCId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Url.ToBytes(),
                AccessHash.ToBytes(),
                Size.ToBytes(),
                MimeType.ToBytes(),
                Attributes.ToBytes(),
                DCId.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);
            AccessHash = GetObject<TLLong>(input);
            Size = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);
            DCId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Url.ToStream(output);
            AccessHash.ToStream(output);
            Size.ToStream(output);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
            DCId.ToStream(output);
        }
    }

    public class TLWebDocumentNoProxy : TLWebDocumentBase
    {
        public const uint Signature = TLConstructors.TLWebDocumentNoProxy;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Url.ToBytes(),
                Size.ToBytes(),
                MimeType.ToBytes(),
                Attributes.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);
            Size = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Url.ToStream(output);
            Size.ToStream(output);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
        }
    }
}
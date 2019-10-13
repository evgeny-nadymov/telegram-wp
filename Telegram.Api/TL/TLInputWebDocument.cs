// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using System.Linq;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public class TLInputWebDocument : TLInputDocumentBase
    {
        public const uint Signature = TLConstructors.TLInputWebDocument;

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

        public override string ToString()
        {
            return string.Format("TLInputWebDocument url={0}", Url);
        }
    }
}

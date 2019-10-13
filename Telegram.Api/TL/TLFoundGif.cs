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
    public abstract class TLFoundGifBase : TLObject { }

    public class TLFoundGif : TLFoundGifBase
    {
        public const uint Signature = TLConstructors.TLFoundGif;

        public TLString Url { get; set; }

        public TLString ThumbUrl { get; set; }

        public TLString ContentUrl { get; set; }

        public TLString ContentType { get; set; }

        public TLInt W { get; set; }

        public TLInt H { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);
            ThumbUrl = GetObject<TLString>(bytes, ref position);
            ContentUrl = GetObject<TLString>(bytes, ref position);
            ContentType = GetObject<TLString>(bytes, ref position);
            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);
            ThumbUrl = GetObject<TLString>(input);
            ContentUrl = GetObject<TLString>(input);
            ContentType = GetObject<TLString>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Url.ToStream(output);
            ThumbUrl.ToStream(output);
            ContentUrl.ToStream(output);
            ContentType.ToStream(output);
            W.ToStream(output);
            H.ToStream(output);
        }
    }

    public class TLFoundGifCached : TLFoundGifBase
    {
        public const uint Signature = TLConstructors.TLFoundGifCached;

        public TLString Url { get; set; }

        public TLPhotoBase Photo { get; set; }

        public TLDocumentBase Document { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);
            Photo = GetObject<TLPhotoBase>(bytes, ref position);
            Document = GetObject<TLDocumentBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);
            Photo = GetObject<TLPhotoBase>(input);
            Document = GetObject<TLDocumentBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Url.ToStream(output);
            Photo.ToStream(output);
            Document.ToStream(output);
        }
    }
}

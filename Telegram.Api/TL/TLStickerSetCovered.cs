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
    public abstract class TLStickerSetCoveredBase : TLObject
    {
        public TLStickerSetBase StickerSet { get; set; }
    }
    public class TLStickerSetCovered : TLStickerSetCoveredBase
    {
        public const uint Signature = TLConstructors.TLStickerSetCovered;

        public TLDocumentBase Cover { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            StickerSet = GetObject<TLStickerSetBase>(bytes, ref position);
            Cover = GetObject<TLDocumentBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                StickerSet.ToBytes(),
                Cover.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            StickerSet = GetObject<TLStickerSetBase>(input);
            Cover = GetObject<TLDocumentBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            StickerSet.ToStream(output);
            Cover.ToStream(output);
        }
    }

    public class TLStickerSetMultiCovered : TLStickerSetCoveredBase
    {
        public const uint Signature = TLConstructors.TLStickerSetMultiCovered;

        public TLVector<TLDocumentBase> Covers { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            StickerSet = GetObject<TLStickerSetBase>(bytes, ref position);
            Covers = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                StickerSet.ToBytes(),
                Covers.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            StickerSet = GetObject<TLStickerSetBase>(input);
            Covers = GetObject<TLVector<TLDocumentBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            StickerSet.ToStream(output);
            Covers.ToStream(output);
        }
    }
}

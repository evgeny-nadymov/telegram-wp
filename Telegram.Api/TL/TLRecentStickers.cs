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
    public abstract class TLRecentStickersBase : TLObject { }

    public class TLRecentStickersNotModified : TLRecentStickersBase
    {
        public const uint Signature = TLConstructors.TLRecentStickersNotModified;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLRecentStickers76 : TLRecentStickers
    {
        public new const uint Signature = TLConstructors.TLRecentStickers76;

        public TLVector<TLStickerPack> Packs { get; set; }

        public TLVector<TLInt> Dates { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Hash = GetObject<TLInt>(bytes, ref position);
            Packs = GetObject<TLVector<TLStickerPack>>(bytes, ref position);
            Documents = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);
            Dates = GetObject<TLVector<TLInt>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Hash.ToBytes(),
                Packs.ToBytes(),
                Documents.ToBytes(),
                Dates.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Hash = GetObject<TLInt>(input);
            Packs = GetObject<TLVector<TLStickerPack>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);
            Dates = GetObject<TLVector<TLInt>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Hash.ToStream(output);
            Packs.ToStream(output);
            Documents.ToStream(output);
            Dates.ToStream(output);
        }
    }

    public class TLRecentStickers : TLRecentStickersBase
    {
        public const uint Signature = TLConstructors.TLRecentStickers;

        public virtual TLInt Hash { get; set; }

        public TLVector<TLDocumentBase> Documents { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Hash = GetObject<TLInt>(bytes, ref position);
            Documents = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Hash.ToBytes(),
                Documents.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Hash = GetObject<TLInt>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Hash.ToStream(output);
            Documents.ToStream(output);
        }
    }
}

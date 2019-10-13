// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;

namespace Telegram.Api.TL
{
    public abstract class TLFeaturedStickersBase : TLObject { }

    public class TLFeaturedStickersNotModified : TLFeaturedStickersBase
    {
        public const uint Signature = TLConstructors.TLFeaturedStickersNotModified;

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

    public class TLFeaturedStickers : TLFeaturedStickersBase, IStickers
    {
        public const uint Signature = TLConstructors.TLFeaturedStickers;

        public TLInt HashValue { get; set; }

        public TLVector<TLStickerSetCoveredBase> SetsCovered { get; set; }

        public TLVector<TLStickerSetBase> Sets
        {
            get
            {
                var sets = new TLVector<TLStickerSetBase>();
                foreach (var setCovered in SetsCovered)
                {
                    sets.Add(setCovered.StickerSet);
                }
                return sets;
            }
            set
            {
                Execute.ShowDebugMessage("TLFeaturedStickers.Sets set");
            }
        } 

        public TLVector<TLLong> Unread { get; set; }

        public TLVector<TLStickerPack> Packs { get; set; }

        public TLVector<TLDocumentBase> Documents { get; set; }

        public TLString Hash { get; set; }

        public TLVector<TLMessagesStickerSet> MessagesStickerSets { get; set; } 

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            HashValue = GetObject<TLInt>(bytes, ref position);
            SetsCovered = GetObject<TLVector<TLStickerSetCoveredBase>>(bytes, ref position);
            Unread = GetObject<TLVector<TLLong>>(bytes, ref position); 
            
            Packs = new TLVector<TLStickerPack>();
            Documents = new TLVector<TLDocumentBase>();
            MessagesStickerSets = new TLVector<TLMessagesStickerSet>();

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                HashValue.ToBytes(),
                SetsCovered.ToBytes(),
                Unread.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            HashValue = GetObject<TLInt>(input);
            SetsCovered = GetObject<TLVector<TLStickerSetCoveredBase>>(input);
            Unread = GetObject<TLVector<TLLong>>(input);

            Packs = GetObject<TLVector<TLStickerPack>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);
            MessagesStickerSets = GetObject<TLVector<TLMessagesStickerSet>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            HashValue.ToStream(output);
            SetsCovered.ToStream(output);
            Unread.ToStream(output);

            Packs.ToStream(output);
            Documents.ToStream(output);
            MessagesStickerSets.ToStream(output);
        }
    }
}

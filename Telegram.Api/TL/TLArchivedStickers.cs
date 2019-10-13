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
    public class TLArchivedStickers : TLObject, IStickers
    {
        public const uint Signature = TLConstructors.TLArchivedStickers;

        public TLInt Count { get; set; }

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
                Execute.ShowDebugMessage("TLArchivedStickers.Sets set");
            }
        }

        public TLVector<TLStickerPack> Packs { get; set; }

        public TLVector<TLDocumentBase> Documents { get; set; }

        public TLString Hash { get; set; }

        public TLVector<TLMessagesStickerSet> MessagesStickerSets { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Count = GetObject<TLInt>(bytes, ref position);
            SetsCovered = GetObject<TLVector<TLStickerSetCoveredBase>>(bytes, ref position);

            Packs = new TLVector<TLStickerPack>();
            Documents = new TLVector<TLDocumentBase>();
            MessagesStickerSets = new TLVector<TLMessagesStickerSet>();

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Count.ToBytes(),
                SetsCovered.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Count = GetObject<TLInt>(input);
            SetsCovered = GetObject<TLVector<TLStickerSetCoveredBase>>(input);

            Packs = GetObject<TLVector<TLStickerPack>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);
            MessagesStickerSets = GetObject<TLVector<TLMessagesStickerSet>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Count.ToStream(output);
            SetsCovered.ToStream(output);

            Packs.ToStream(output);
            Documents.ToStream(output);
            MessagesStickerSets.ToStream(output);
        }
    }
}

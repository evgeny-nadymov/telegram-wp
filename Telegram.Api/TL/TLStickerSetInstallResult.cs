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
using Telegram.Api.Helpers;

namespace Telegram.Api.TL
{
    public abstract class TLStickerSetInstallResultBase : TLObject { }

    public class TLStickerSetInstallResult : TLStickerSetInstallResultBase
    {
        public const uint Signature = TLConstructors.TLStickerSetInstallResult;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override string ToString()
        {
            return "TLStickerSetInstallResult";
        }
    }

    public class TLStickerSetInstallResultArchive : TLStickerSetInstallResultBase, IStickers
    {
        public const uint Signature = TLConstructors.TLStickerSetInstallResultArchive;

        public TLString Hash { get; set; }

        public TLVector<TLStickerSetCoveredBase> SetsCovered { get; set; }

        #region Additional

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
                Execute.ShowDebugMessage("TLStickerSetInstallResultArchive.Sets set");
            }
        }
        public TLVector<TLStickerPack> Packs { get; set; }
        public TLVector<TLDocumentBase> Documents { get; set; }
        public TLVector<TLMessagesStickerSet> MessagesStickerSets { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            SetsCovered = GetObject<TLVector<TLStickerSetCoveredBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                SetsCovered.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            SetsCovered = GetObject<TLVector<TLStickerSetCoveredBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            SetsCovered.ToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLStickerSetInstallResultArchive sets=[{0}]", string.Join(", ", Sets.Select(x => x.Id)));
        }
    }
}

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
    public enum AllStickersCustomFlags
    {
        RecentStickers = 0x1,
        FavedStickers = 0x2,
        ShowStickersByEmoji = 0x4,
    }

    public interface IStickers
    {
        TLString Hash { get; set; }

        TLVector<TLStickerSetBase> Sets { get; set; } 
        
        TLVector<TLStickerPack> Packs { get; set; }

        TLVector<TLDocumentBase> Documents { get; set; }
    }

    public abstract class TLAllStickersBase : TLObject { }

    public class TLAllStickersNotModified : TLAllStickersBase
    {
        public const uint Signature = TLConstructors.TLAllStickersNotModified;

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

    public class TLAllStickers : TLAllStickersBase
    {
        public const uint Signature = TLConstructors.TLAllStickers;

        public virtual TLString Hash { get; set; }

        public TLVector<TLStickerPack> Packs { get; set; } 

        public TLVector<TLDocumentBase> Documents { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Hash = GetObject<TLString>(bytes, ref position);
            Packs = GetObject<TLVector<TLStickerPack>>(bytes, ref position);
            Documents = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Hash.ToBytes(),
                Packs.ToBytes(),
                Documents.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Hash = GetObject<TLString>(input);
            Packs = GetObject<TLVector<TLStickerPack>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Hash.ToStream(output);
            Packs.ToStream(output);
            Documents.ToStream(output);
        }
    }

    public class TLAllStickers29 : TLAllStickers
    {
        public new const uint Signature = TLConstructors.TLAllStickers29;

        public TLVector<TLStickerSetBase> Sets { get; set; }

        #region Additional
        public TLInt Date { get; set; }

        public TLBool ShowStickersTab { get; set; }

        public TLVector<TLRecentlyUsedSticker> RecentlyUsed { get; set; } 
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Hash = GetObject<TLString>(bytes, ref position);
            Packs = GetObject<TLVector<TLStickerPack>>(bytes, ref position);
            Sets = GetObject<TLVector<TLStickerSetBase>>(bytes, ref position);
            Documents = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);

            ShowStickersTab = new TLBool(true);
            RecentlyUsed = new TLVector<TLRecentlyUsedSticker>();
            Date = TLUtils.DateToUniversalTimeTLInt(0, DateTime.Now);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Hash.ToBytes(),
                Packs.ToBytes(),
                Sets.ToBytes(),
                Documents.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Hash = GetObject<TLString>(input);
            Packs = GetObject<TLVector<TLStickerPack>>(input);
            Sets = GetObject<TLVector<TLStickerSetBase>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);

            ShowStickersTab = GetNullableObject<TLBool>(input);
            RecentlyUsed = GetNullableObject<TLVector<TLRecentlyUsedSticker>>(input);
            Date = GetNullableObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Hash.ToStream(output);
            Packs.ToStream(output);
            Sets.ToStream(output);
            Documents.ToStream(output);

            ShowStickersTab.NullableToStream(output);
            RecentlyUsed.NullableToStream(output);
            Date.NullableToStream(output);
        }
    }

    public class TLAllStickers32 : TLAllStickers29
    {
        public new const uint Signature = TLConstructors.TLAllStickers32;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Hash = GetObject<TLString>(bytes, ref position);
            Sets = GetObject<TLVector<TLStickerSetBase>>(bytes, ref position);
            
            Packs = new TLVector<TLStickerPack>();
            Documents = new TLVector<TLDocumentBase>();
            ShowStickersTab = TLBool.True;
            RecentlyUsed = new TLVector<TLRecentlyUsedSticker>();
            Date = TLUtils.DateToUniversalTimeTLInt(0, DateTime.Now);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Hash.ToBytes(),
                Sets.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Hash = GetObject<TLString>(input);
            Packs = GetObject<TLVector<TLStickerPack>>(input);
            Sets = GetObject<TLVector<TLStickerSetBase>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);

            ShowStickersTab = GetNullableObject<TLBool>(input);
            RecentlyUsed = GetNullableObject<TLVector<TLRecentlyUsedSticker>>(input);
            Date = GetNullableObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Hash.ToStream(output);
            Packs.ToStream(output);
            Sets.ToStream(output);
            Documents.ToStream(output);

            ShowStickersTab.NullableToStream(output);
            RecentlyUsed.NullableToStream(output);
            Date.NullableToStream(output);
        }
    }

    public class TLAllStickers43 : TLAllStickers32, IStickers
    {
        public new const uint Signature = TLConstructors.TLAllStickers43;

        public TLInt HashValue { get; set; }

        public override TLString Hash
        {
            get { return TLUtils.ToTLString(HashValue) ?? TLString.Empty; }
            set { HashValue = TLUtils.ToTLInt(value) ?? new TLInt(0); }
        }

        private TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        private TLRecentStickers _recentStickers;

        public TLRecentStickers RecentStickers
        {
            get { return _recentStickers; }
            set { SetField(out _recentStickers, value, ref _customFlags, (int) AllStickersCustomFlags.RecentStickers); }
        }

        private TLFavedStickers _favedStickers;

        public TLFavedStickers FavedStickers
        {
            get { return _favedStickers; }
            set { SetField(out _favedStickers, value, ref _customFlags, (int)AllStickersCustomFlags.FavedStickers); }
        }

        protected TLInt _showStickersByEmoji;

        // null - all sets
        // 1 - my sets
        // 0 - none
        public ShowStickersByEmoji ShowStickersByEmoji
        {
            get
            {
                if (_showStickersByEmoji == null)
                {
                    return ShowStickersByEmoji.AllSets;
                }
                if (_showStickersByEmoji.Value == 1)
                {
                    return ShowStickersByEmoji.MySets;
                }

                return ShowStickersByEmoji.None;
            }
            set
            {
                switch (value)
                {
                    case ShowStickersByEmoji.AllSets:
                        SetField(out _showStickersByEmoji, null, ref _customFlags, (int)AllStickersCustomFlags.ShowStickersByEmoji);
                        break;
                    case ShowStickersByEmoji.MySets:
                        SetField(out _showStickersByEmoji, new TLInt(1), ref _customFlags, (int)AllStickersCustomFlags.ShowStickersByEmoji);
                        break;
                    default:
                        SetField(out _showStickersByEmoji, new TLInt(0), ref _customFlags, (int)AllStickersCustomFlags.ShowStickersByEmoji);
                        break;
                }
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            HashValue = GetObject<TLInt>(bytes, ref position);
            Sets = GetObject<TLVector<TLStickerSetBase>>(bytes, ref position);
            
            Packs = new TLVector<TLStickerPack>();
            Documents = new TLVector<TLDocumentBase>();
            ShowStickersTab = TLBool.True;
            RecentlyUsed = new TLVector<TLRecentlyUsedSticker>();
            Date = TLUtils.DateToUniversalTimeTLInt(0, DateTime.Now);
            CustomFlags = new TLLong(0);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                HashValue.ToBytes(),
                Sets.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            HashValue = GetObject<TLInt>(input);
            Packs = GetObject<TLVector<TLStickerPack>>(input);
            
            Sets = GetObject<TLVector<TLStickerSetBase>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);
            ShowStickersTab = GetNullableObject<TLBool>(input);
            RecentlyUsed = GetNullableObject<TLVector<TLRecentlyUsedSticker>>(input);
            Date = GetNullableObject<TLInt>(input);
            CustomFlags = GetNullableObject<TLLong>(input);
            RecentStickers = GetObject<TLRecentStickers>(CustomFlags, (int)AllStickersCustomFlags.RecentStickers, null, input);
            FavedStickers = GetObject<TLFavedStickers>(CustomFlags, (int)AllStickersCustomFlags.FavedStickers, null, input);
            _showStickersByEmoji = GetObject<TLInt>(CustomFlags, (int)AllStickersCustomFlags.ShowStickersByEmoji, null, input);

            // move showStickersTab flag to ShowStickersByEmoji flag
            if (ShowStickersTab != null && !ShowStickersTab.Value)
            {
                ShowStickersByEmoji = ShowStickersByEmoji.MySets;
                ShowStickersTab = TLBool.True;
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            HashValue.ToStream(output);
            Packs.ToStream(output);
            
            Sets.ToStream(output);
            Documents.ToStream(output);
            ShowStickersTab.NullableToStream(output);
            RecentlyUsed.NullableToStream(output);
            Date.NullableToStream(output);
            CustomFlags.ToStream(output);
            ToStream(output, RecentStickers, CustomFlags, (int)AllStickersCustomFlags.RecentStickers);
            ToStream(output, FavedStickers, CustomFlags, (int)AllStickersCustomFlags.FavedStickers);
            ToStream(output, _showStickersByEmoji, CustomFlags, (int)AllStickersCustomFlags.ShowStickersByEmoji);
        }
    }

    public enum ShowStickersByEmoji
    {
        AllSets,
        MySets,
        None
    }
}

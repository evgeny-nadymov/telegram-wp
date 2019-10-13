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
    public enum StickerSetFlags
    {
        Installed = 0x1,
        Disabled = 0x2,
        Official = 0x4,
        Masks = 0x8
    }

    public abstract class TLStickerSetBase : TLObject
    {
        public TLLong Id { get; set; }
        public TLLong AccessHash { get; set; }
        public TLString Title { get; set; }
        public TLString ShortName { get; set; }

        #region Additional
        public TLVector<TLObject> Stickers { get; set; }
        public bool Unread { get; set; }
        public bool IsSelected { get; set; }
        #endregion
    }

    public class TLStickerSet : TLStickerSetBase
    {
        public const uint Signature = TLConstructors.TLStickerSet;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            ShortName = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Title.ToBytes(),
                ShortName.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Title = GetObject<TLString>(input);
            ShortName = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            AccessHash.ToStream(output);
            Title.ToStream(output);
            ShortName.ToStream(output);
        }
    }

    public class TLStickerSet76 : TLStickerSet32
    {
        public new const uint Signature = TLConstructors.TLStickerSet76;

        protected TLInt _installedDate;

        public TLInt InstalledDate
        {
            get { return _installedDate; }
            set { SetField(out _installedDate, value, ref _flags, (int) StickerSetFlags.Installed); }
        }

        public int SortDate { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            InstalledDate = GetObject<TLInt>(Flags, (int) StickerSetFlags.Installed, null, bytes, ref position);
            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            ShortName = GetObject<TLString>(bytes, ref position);
            Count = GetObject<TLInt>(bytes, ref position);
            Hash = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(InstalledDate, Flags, (int) StickerSetFlags.Installed),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Title.ToBytes(),
                ShortName.ToBytes(),
                Count.ToBytes(),
                Hash.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            InstalledDate = GetObject<TLInt>(Flags, (int) StickerSetFlags.Installed, null, input);
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Title = GetObject<TLString>(input);
            ShortName = GetObject<TLString>(input);
            Count = GetObject<TLInt>(input);
            Hash = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, InstalledDate, Flags, (int) StickerSetFlags.Installed);
            Id.ToStream(output);
            AccessHash.ToStream(output);
            Title.ToStream(output);
            ShortName.ToStream(output);
            Count.ToStream(output);
            Hash.ToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLStickerSet76 id={0} short_name={1} flags={2}", Id, ShortName, StickerSetFlagsString(Flags));
        }
    }

    public class TLStickerSet32 : TLStickerSet
    {
        public new const uint Signature = TLConstructors.TLStickerSet32;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInt Count { get; set; }

        public TLInt Hash { get; set; }

        public bool Installed
        {
            get { return IsSet(Flags, (int) StickerSetFlags.Installed); }
            set { SetUnset(ref _flags, value, (int) StickerSetFlags.Installed); }
        }

        public bool Archived
        {
            get { return IsSet(Flags, (int)StickerSetFlags.Disabled); }
            set { SetUnset(ref _flags, value, (int)StickerSetFlags.Disabled); }
        }

        public bool Official
        {
            get { return IsSet(Flags, (int)StickerSetFlags.Official); }
            set { SetUnset(ref _flags, value, (int)StickerSetFlags.Official); }
        }

        public bool Masks
        {
            get { return IsSet(Flags, (int)StickerSetFlags.Masks); }
            set { SetUnset(ref _flags, value, (int)StickerSetFlags.Masks); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            
            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            ShortName = GetObject<TLString>(bytes, ref position);
            Count = GetObject<TLInt>(bytes, ref position);
            Hash = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Title.ToBytes(),
                ShortName.ToBytes(), 
                Count.ToBytes(),
                Hash.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Title = GetObject<TLString>(input);
            ShortName = GetObject<TLString>(input);
            Count = GetObject<TLInt>(input);
            Hash = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            AccessHash.ToStream(output);
            Title.ToStream(output);
            ShortName.ToStream(output);
            Count.ToStream(output);
            Hash.ToStream(output);
        }

        public static string StickerSetFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (StickerSetFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public override string ToString()
        {
            return string.Format("TLStickerSet32 id={0} short_name={1} flags={2}", Id, ShortName, StickerSetFlagsString(Flags));
        }
    }

    public class TLStickerSetEmpty : TLStickerSetBase
    {
        public const uint Signature = TLConstructors.TLStickerSetEmpty;

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
    }
}

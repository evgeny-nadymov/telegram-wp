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
    public enum InputMediaUploadedPhotoFlags
    {
        Stickers = 0x1,
        TTLSeconds = 0x2
    }

    [Flags]
    public enum InputMediaPhotoFlags
    {
        TTLSeconds = 0x1
    }

    [Flags]
    public enum InputMediaUploadedDocumentFlags
    {
        Stickers = 0x1,
        TTLSeconds = 0x2,
        Thumb = 0x4,
        NosoundVideo = 0x8,
    }

    [Flags]
    public enum InputMediaDocumentFlags
    {
        TTLSeconds = 0x1,
    }

    [Flags]
    public enum InputMediaPhotoExternalFlags
    {
        TTLSeconds = 0x1,
    }

    [Flags]
    public enum InputMediaDocumentExternalFlags
    {
        TTLSeconds = 0x1,
    }

    [Flags]
    public enum InputMediaInvoiceFlags
    {
        Photo = 0x1
    }

    public interface IInputMediaCaption
    {
        TLString Caption { get; set; }
    }

    public interface IInputTTLMedia
    {
        TLInt TTLSeconds { get; set; }
    }

    public abstract class TLInputMediaBase : TLObject
    {
#region Additional
        public byte[] MD5Hash { get; set; }
#endregion
    }

    public class TLInputMediaEmpty : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaEmpty;

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

    public class TLInputMediaUploadedDocument : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaUploadedDocument;

        public TLInputFileBase File { get; set; }

        public TLString FileName { get; set; }

        public TLString MimeType { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                FileName.ToBytes(),
                MimeType.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            FileName = GetObject<TLString>(input);
            MimeType = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            FileName.ToStream(output);
            MimeType.ToStream(output);
        }
    }

    public class TLInputMediaUploadedDocument22 : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaUploadedDocument22;

        public TLInputFileBase File { get; set; }

        public TLString MimeType { get; set; }

        public TLVector<TLDocumentAttributeBase> Attributes { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                MimeType.ToBytes(),
                Attributes.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
        }
    }

    public class TLInputMediaUploadedDocument75 : TLInputMediaUploadedDocument70
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedDocument75;
        
        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                File.ToBytes(),
                ToBytes(Thumb, Flags, (int)InputMediaUploadedDocumentFlags.Thumb),
                MimeType.ToBytes(),
                Attributes.ToBytes(),
                ToBytes(Stickers, Flags, (int)InputMediaUploadedDocumentFlags.Stickers),
                ToBytes(TTLSeconds, Flags, (int)InputMediaUploadedDocumentFlags.TTLSeconds));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            File = GetObject<TLInputFile>(input);
            Thumb = GetObject<TLInputFileBase>(Flags, (int)InputMediaUploadedDocumentFlags.Thumb, null, input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);
            Stickers = GetObject<TLVector<TLInputDocumentBase>>(Flags, (int)InputMediaUploadedDocumentFlags.Stickers, null, input);
            TTLSeconds = GetObject<TLInt>(Flags, (int)InputMediaUploadedDocumentFlags.TTLSeconds, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            File.ToStream(output);
            ToStream(output, Thumb, Flags, (int)InputMediaUploadedDocumentFlags.Thumb);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
            ToStream(output, Stickers, Flags, (int)InputMediaUploadedDocumentFlags.Stickers);
            ToStream(output, TTLSeconds, Flags, (int)InputMediaUploadedDocumentFlags.TTLSeconds);
        }
    }

    public class TLInputMediaUploadedDocument70 : TLInputMediaUploadedDocument56, IInputTTLMedia
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedDocument70;

        private TLInputFileBase _thumb;

        public TLInputFileBase Thumb
        {
            get { return _thumb; }
            set { SetField(out _thumb, value, ref _flags, (int)InputMediaUploadedDocumentFlags.Thumb); }
        }

        private TLInt _ttlSeconds;

        public TLInt TTLSeconds
        {
            get { return _ttlSeconds; }
            set { SetField(out _ttlSeconds, value, ref _flags, (int)InputMediaUploadedDocumentFlags.TTLSeconds); }
        }

        public bool NosoundVideo
        {
            get { return IsSet(_flags, (int) InputMediaUploadedDocumentFlags.NosoundVideo); }
            set { SetUnset(ref _flags, value, (int) InputMediaUploadedDocumentFlags.NosoundVideo); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                File.ToBytes(),
                ToBytes(Thumb, Flags, (int)InputMediaUploadedDocumentFlags.Thumb),
                MimeType.ToBytes(),
                Attributes.ToBytes(),
                Caption.ToBytes(),
                ToBytes(Stickers, Flags, (int)InputMediaUploadedDocumentFlags.Stickers),
                ToBytes(TTLSeconds, Flags, (int)InputMediaUploadedDocumentFlags.TTLSeconds));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            File = GetObject<TLInputFile>(input);
            Thumb = GetObject<TLInputFileBase>(Flags, (int)InputMediaUploadedDocumentFlags.Thumb, null, input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);
            Caption = GetObject<TLString>(input);
            Stickers = GetObject<TLVector<TLInputDocumentBase>>(Flags, (int)InputMediaUploadedDocumentFlags.Stickers, null, input);
            TTLSeconds = GetObject<TLInt>(Flags, (int)InputMediaUploadedDocumentFlags.TTLSeconds, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            File.ToStream(output);
            ToStream(output, Thumb, Flags, (int)InputMediaUploadedDocumentFlags.Thumb);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, Stickers, Flags, (int)InputMediaUploadedDocumentFlags.Stickers);
            ToStream(output, TTLSeconds, Flags, (int)InputMediaUploadedDocumentFlags.TTLSeconds);
        }
    }

    public class TLInputMediaUploadedDocument56 : TLInputMediaUploadedDocument45
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedDocument56;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLVector<TLInputDocumentBase> _stickers;

        public TLVector<TLInputDocumentBase> Stickers
        {
            get { return _stickers; }
            set { SetField(out _stickers, value, ref _flags, (int)InputMediaUploadedDocumentFlags.Stickers); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                File.ToBytes(),
                MimeType.ToBytes(),
                Attributes.ToBytes(),
                Caption.ToBytes(),
                ToBytes(Stickers, Flags, (int)InputMediaUploadedDocumentFlags.Stickers));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            File = GetObject<TLInputFile>(input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);
            Caption = GetObject<TLString>(input);
            Stickers = GetObject<TLVector<TLInputDocumentBase>>(Flags, (int)InputMediaUploadedDocumentFlags.Stickers, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            File.ToStream(output);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, Stickers, Flags, (int)InputMediaUploadedDocumentFlags.Stickers);
        }
    }

    public class TLInputMediaUploadedDocument45 : TLInputMediaBase, IAttributes, IInputMediaCaption
    {
        public const uint Signature = TLConstructors.TLInputMediaUploadedDocument45;

        public TLInputFileBase File { get; set; }

        public TLString MimeType { get; set; }

        public TLVector<TLDocumentAttributeBase> Attributes { get; set; }

        public TLString Caption { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                MimeType.ToBytes(),
                Attributes.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);
            Caption = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
            Caption.ToStream(output);
        }
    }

    public class TLInputMediaUploadedThumbDocument : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaUploadedThumbDocument;

        public TLInputFileBase File { get; set; }

        public TLInputFileBase Thumb { get; set; }

        public TLString FileName { get; set; }

        public TLString MimeType { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Thumb.ToBytes(),
                FileName.ToBytes(),
                MimeType.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFileBase>(input);
            Thumb = GetObject<TLInputFileBase>(input);
            FileName = GetObject<TLString>(input);
            MimeType = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Thumb.ToStream(output);
            FileName.ToStream(output);
            MimeType.ToStream(output);
        }
    }

    public class TLInputMediaUploadedThumbDocument22 : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaUploadedThumbDocument22;

        public TLInputFileBase File { get; set; }

        public TLInputFileBase Thumb { get; set; }

        public TLString MimeType { get; set; }

        public TLVector<TLDocumentAttributeBase> Attributes { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Thumb.ToBytes(),
                MimeType.ToBytes(),
                Attributes.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFileBase>(input);
            Thumb = GetObject<TLInputFileBase>(input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Thumb.ToStream(output);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
        }
    }

    [Obsolete]
    public class TLInputMediaUploadedThumbDocument56 : TLInputMediaUploadedThumbDocument45
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedThumbDocument56;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLVector<TLInputDocumentBase> _stickers;

        public TLVector<TLInputDocumentBase> Stickers
        {
            get { return _stickers; }
            set { SetField(out _stickers, value, ref _flags, (int)InputMediaUploadedDocumentFlags.Stickers); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                File.ToBytes(),
                Thumb.ToBytes(),
                MimeType.ToBytes(),
                Attributes.ToBytes(),
                Caption.ToBytes(),
                ToBytes(Stickers, Flags, (int)InputMediaUploadedDocumentFlags.Stickers));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            File = GetObject<TLInputFileBase>(input);
            Thumb = GetObject<TLInputFileBase>(input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);
            Caption = GetObject<TLString>(input);
            Stickers = GetObject<TLVector<TLInputDocumentBase>>(Flags, (int)InputMediaUploadedDocumentFlags.Stickers, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            File.ToStream(output);
            Thumb.ToStream(output);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, Stickers, Flags, (int)InputMediaUploadedDocumentFlags.Stickers);
        }
    }

    public class TLInputMediaUploadedThumbDocument45 : TLInputMediaBase, IAttributes, IInputMediaCaption
    {
        public const uint Signature = TLConstructors.TLInputMediaUploadedThumbDocument45;

        public TLInputFileBase File { get; set; }

        public TLInputFileBase Thumb { get; set; }

        public TLString MimeType { get; set; }

        public TLVector<TLDocumentAttributeBase> Attributes { get; set; }

        public TLString Caption { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Thumb.ToBytes(),
                MimeType.ToBytes(),
                Attributes.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFileBase>(input);
            Thumb = GetObject<TLInputFileBase>(input);
            MimeType = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);
            Caption = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Thumb.ToStream(output);
            MimeType.ToStream(output);
            Attributes.ToStream(output);
            Caption.ToStream(output);
        }
    }

    public class TLInputMediaDocument : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaDocument;

        public TLInputDocumentBase Id { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInputDocumentBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
        }
    }

    public class TLInputMediaDocument75 : TLInputMediaDocument70
    {
        public new const uint Signature = TLConstructors.TLInputMediaDocument75;

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                ToBytes(TTLSeconds, Flags, (int)InputMediaDocumentFlags.TTLSeconds));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInputDocumentBase>(input);
            TTLSeconds = GetObject<TLInt>(Flags, (int)InputMediaDocumentFlags.TTLSeconds, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            ToStream(output, TTLSeconds, Flags, (int)InputMediaDocumentFlags.TTLSeconds);
        }
    }

    public class TLInputMediaDocument70 : TLInputMediaDocument45, IInputTTLMedia
    {
        public new const uint Signature = TLConstructors.TLInputMediaDocument70;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLInt _ttlSeconds;

        public TLInt TTLSeconds
        {
            get { return _ttlSeconds; }
            set { SetField(out _ttlSeconds, value, ref _flags, (int)InputMediaDocumentFlags.TTLSeconds); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                Caption.ToBytes(),
                ToBytes(TTLSeconds, Flags, (int)InputMediaDocumentFlags.TTLSeconds));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInputDocumentBase>(input);
            Caption = GetObject<TLString>(input);
            TTLSeconds = GetObject<TLInt>(Flags, (int)InputMediaDocumentFlags.TTLSeconds, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, TTLSeconds, Flags, (int)InputMediaDocumentFlags.TTLSeconds);
        }
    }

    public class TLInputMediaDocument45 : TLInputMediaBase, IInputMediaCaption
    {
        public const uint Signature = TLConstructors.TLInputMediaDocument45;

        public TLInputDocumentBase Id { get; set; }

        public TLString Caption { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInputDocumentBase>(input);
            Caption = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            Caption.ToStream(output);
        }
    }

    public class TLInputMediaUploadedAudio : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaUploadedAudio;

        public TLInputFile File { get; set; }

        public TLInt Duration { get; set; }

        public TLString MimeType { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Duration.ToBytes(),
                MimeType.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            Duration = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Duration.ToStream(output);
            MimeType.ToStream(output);
        }
    }

    public class TLInputMediaAudio : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaAudio;

        public TLInputAudioBase Id { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }
    }

    public class TLInputMediaUploadedPhoto : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaUploadedPhoto;

        public TLInputFileBase File { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
        }
    }

    public class TLInputMediaUploadedPhoto28 : TLInputMediaUploadedPhoto, IInputMediaCaption
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedPhoto28;

        public TLString Caption { get; set; }
   
        public TLInputMediaUploadedPhoto28()
        {
            
        }

        public TLInputMediaUploadedPhoto28(TLInputMediaUploadedPhoto inputMediaUploadedPhoto, TLString caption)
        {
            File = inputMediaUploadedPhoto.File;
            Caption = caption;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            Caption = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Caption.ToStream(output);
        }
    }

    public class TLInputMediaUploadedPhoto56 : TLInputMediaUploadedPhoto28
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedPhoto56;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLVector<TLInputDocumentBase> _stickers;

        public TLVector<TLInputDocumentBase> Stickers
        {
            get { return _stickers; }
            set { SetField(out _stickers, value, ref _flags, (int)InputMediaUploadedPhotoFlags.Stickers); }
        }

        public TLInputMediaUploadedPhoto56()
        {

        }

        public TLInputMediaUploadedPhoto56(TLInputMediaUploadedPhoto inputMediaUploadedPhoto, TLString caption, TLVector<TLInputDocumentBase> stickers)
        {
            Flags = new TLInt(0);
            File = inputMediaUploadedPhoto.File;
            Caption = caption;
            Stickers = stickers;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                File.ToBytes(),
                Caption.ToBytes(),
                ToBytes(Stickers, Flags, (int)InputMediaUploadedPhotoFlags.Stickers));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            File = GetObject<TLInputFile>(input);
            Caption = GetObject<TLString>(input);
            Stickers = GetObject<TLVector<TLInputDocumentBase>>(Flags, (int)InputMediaUploadedPhotoFlags.Stickers, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            File.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, Stickers, Flags, (int)InputMediaUploadedPhotoFlags.Stickers);
        }
    }

    public class TLInputMediaUploadedPhoto70 : TLInputMediaUploadedPhoto56, IInputTTLMedia
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedPhoto70;

        private TLInt _ttlSeconds;

        public TLInt TTLSeconds
        {
            get { return _ttlSeconds; }
            set { SetField(out _ttlSeconds, value, ref _flags, (int)InputMediaUploadedPhotoFlags.TTLSeconds); }
        }

        public TLInputMediaUploadedPhoto70()
        {

        }

        public TLInputMediaUploadedPhoto70(TLInputMediaUploadedPhoto inputMediaUploadedPhoto, TLString caption, TLVector<TLInputDocumentBase> stickers)
        {
            Flags = new TLInt(0);
            File = inputMediaUploadedPhoto.File;
            Caption = caption;
            Stickers = stickers;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                File.ToBytes(),
                Caption.ToBytes(),
                ToBytes(Stickers, Flags, (int)InputMediaUploadedPhotoFlags.Stickers),
                ToBytes(TTLSeconds, Flags, (int)InputMediaUploadedPhotoFlags.TTLSeconds));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            File = GetObject<TLInputFile>(input);
            Caption = GetObject<TLString>(input);
            Stickers = GetObject<TLVector<TLInputDocumentBase>>(Flags, (int)InputMediaUploadedPhotoFlags.Stickers, null, input);
            TTLSeconds = GetObject<TLInt>(Flags, (int)InputMediaUploadedPhotoFlags.TTLSeconds, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            File.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, Stickers, Flags, (int)InputMediaUploadedPhotoFlags.Stickers);
            ToStream(output, TTLSeconds, Flags, (int)InputMediaUploadedPhotoFlags.TTLSeconds);
        }
    }

    public class TLInputMediaUploadedPhoto75 : TLInputMediaUploadedPhoto70
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedPhoto75;

        public TLInputMediaUploadedPhoto75()
        {

        }

        public TLInputMediaUploadedPhoto75(TLInputMediaUploadedPhoto inputMediaUploadedPhoto, TLVector<TLInputDocumentBase> stickers)
        {
            Flags = new TLInt(0);
            File = inputMediaUploadedPhoto.File;
            Caption = TLString.Empty;
            Stickers = stickers;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                File.ToBytes(),
                ToBytes(Stickers, Flags, (int)InputMediaUploadedPhotoFlags.Stickers),
                ToBytes(TTLSeconds, Flags, (int)InputMediaUploadedPhotoFlags.TTLSeconds));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            File = GetObject<TLInputFile>(input);
            Stickers = GetObject<TLVector<TLInputDocumentBase>>(Flags, (int)InputMediaUploadedPhotoFlags.Stickers, null, input);
            TTLSeconds = GetObject<TLInt>(Flags, (int)InputMediaUploadedPhotoFlags.TTLSeconds, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            File.ToStream(output);
            ToStream(output, Stickers, Flags, (int)InputMediaUploadedPhotoFlags.Stickers);
            ToStream(output, TTLSeconds, Flags, (int)InputMediaUploadedPhotoFlags.TTLSeconds);
        }
    }

    public class TLInputMediaPhoto : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaPhoto;

        public TLInputPhotoBase Id { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInputPhotoBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
        }
    }

    public class TLInputMediaPhoto28 : TLInputMediaPhoto, IInputMediaCaption
    {
        public new const uint Signature = TLConstructors.TLInputMediaPhoto28;

        public TLString Caption { get; set; }

        public TLInputMediaPhoto28()
        {
            
        }

        public TLInputMediaPhoto28(TLInputMediaPhoto inputMediaPhoto, TLString caption)
        {
            Id = inputMediaPhoto.Id;
            Caption = caption;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInputPhotoBase>(input);
            Caption = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            Caption.ToStream(output);
        }
    }

    public class TLInputMediaPhoto70 : TLInputMediaPhoto28, IInputTTLMedia
    {
        public new const uint Signature = TLConstructors.TLInputMediaPhoto70;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLInt _ttlSeconds;

        public TLInt TTLSeconds
        {
            get { return _ttlSeconds; }
            set { SetField(out _ttlSeconds, value, ref _flags, (int)InputMediaPhotoFlags.TTLSeconds); }
        }

        public TLInputMediaPhoto70()
        {

        }

        public TLInputMediaPhoto70(TLInputMediaPhoto inputMediaPhoto, TLString caption)
        {
            Flags = new TLInt(0);
            Id = inputMediaPhoto.Id;
            Caption = caption;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                Caption.ToBytes(),
                ToBytes(TTLSeconds, Flags, (int) InputMediaPhotoFlags.TTLSeconds));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInputPhotoBase>(input);
            Caption = GetObject<TLString>(input);
            TTLSeconds = GetObject<TLInt>(Flags, (int) InputMediaPhotoFlags.TTLSeconds, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            Caption.ToStream(output);
            ToStream(output, TTLSeconds, Flags, (int)InputMediaPhotoFlags.TTLSeconds);
        }
    }

    public class TLInputMediaPhoto75 : TLInputMediaPhoto70
    {
        public new const uint Signature = TLConstructors.TLInputMediaPhoto75;

        public TLInputMediaPhoto75()
        {

        }

        public TLInputMediaPhoto75(TLInputMediaPhoto inputMediaPhoto)
        {
            Flags = new TLInt(0);
            Id = inputMediaPhoto.Id;
            Caption = TLString.Empty;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                ToBytes(TTLSeconds, Flags, (int)InputMediaPhotoFlags.TTLSeconds));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLInputPhotoBase>(input);
            Caption = TLString.Empty;
            TTLSeconds = GetObject<TLInt>(Flags, (int)InputMediaPhotoFlags.TTLSeconds, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            ToStream(output, TTLSeconds, Flags, (int)InputMediaPhotoFlags.TTLSeconds);
        }
    }

    public class TLInputMediaGeoPoint : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaGeoPoint;

        public TLInputGeoPointBase GeoPoint { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                GeoPoint.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            GeoPoint = GetObject<TLInputGeoPointBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            GeoPoint.ToStream(output);
        }
    }

    public class TLInputMediaVenue : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaVenue;

        public TLInputGeoPointBase GeoPoint { get; set; }

        public TLString Title { get; set; }

        public TLString Address { get; set; }

        public TLString Provider { get; set; }

        public TLString VenueId { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                GeoPoint.ToBytes(),
                Title.ToBytes(),
                Address.ToBytes(),
                Provider.ToBytes(),
                VenueId.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            GeoPoint = GetObject<TLInputGeoPointBase>(input);
            Title = GetObject<TLString>(input);
            Address = GetObject<TLString>(input);
            Provider = GetObject<TLString>(input);
            VenueId = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            GeoPoint.ToStream(output);
            Title.ToStream(output);
            Address.ToStream(output);
            Provider.ToStream(output);
            VenueId.ToStream(output);
        }
    }

    public class TLInputMediaVenue72 : TLInputMediaVenue
    {
        public new const uint Signature = TLConstructors.TLInputMediaVenue72;

        public TLString VenueType { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                GeoPoint.ToBytes(),
                Title.ToBytes(),
                Address.ToBytes(),
                Provider.ToBytes(),
                VenueId.ToBytes(),
                VenueType.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            GeoPoint = GetObject<TLInputGeoPointBase>(input);
            Title = GetObject<TLString>(input);
            Address = GetObject<TLString>(input);
            Provider = GetObject<TLString>(input);
            VenueId = GetObject<TLString>(input);
            VenueType = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            GeoPoint.ToStream(output);
            Title.ToStream(output);
            Address.ToStream(output);
            Provider.ToStream(output);
            VenueId.ToStream(output);
            VenueType.ToStream(output);
        }
    }

    public class TLInputMediaContact82 : TLInputMediaContact
    {
        public new const uint Signature = TLConstructors.TLInputMediaContact82;

        public TLString VCard { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhoneNumber.ToBytes(),
                FirstName.ToBytes(),
                LastName.ToBytes(),
                VCard.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            PhoneNumber = GetObject<TLString>(input);
            FirstName = GetObject<TLString>(input);
            LastName = GetObject<TLString>(input);
            VCard = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PhoneNumber.ToStream(output);
            FirstName.ToStream(output);
            LastName.ToStream(output);
            VCard.ToStream(output);
        }
    }

    public class TLInputMediaContact : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaContact;

        public TLString PhoneNumber { get; set; }

        public TLString FirstName { get; set; }

        public TLString LastName { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhoneNumber.ToBytes(),
                FirstName.ToBytes(),
                LastName.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            PhoneNumber = GetObject<TLString>(input);
            FirstName = GetObject<TLString>(input);
            LastName = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PhoneNumber.ToStream(output);
            FirstName.ToStream(output);
            LastName.ToStream(output);
        }
    }

    public class TLInputMediaUploadedVideo : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaUploadedVideo;

        public TLInputFileBase File { get; set; }
        public TLInt Duration { get; set; }
        public TLInt W { get; set; }
        public TLInt H { get; set; }
        public TLString MimeType { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Duration.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                MimeType.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            Duration = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Duration.ToStream(output);
            W.ToStream(output);
            H.ToStream(output);
            MimeType.ToStream(output);
        }
    }

    public class TLInputMediaUploadedVideo28 : TLInputMediaUploadedVideo, IInputMediaCaption
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedVideo28;

        public TLString Caption { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Duration.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            Duration = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            Caption = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Duration.ToStream(output);
            W.ToStream(output);
            H.ToStream(output);
            Caption.ToStream(output);
        }
    }

    public class TLInputMediaUploadedVideo36 : TLInputMediaUploadedVideo28
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedVideo36;

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Duration.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                MimeType.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            Duration = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            Caption = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Duration.ToStream(output);
            W.ToStream(output);
            H.ToStream(output);
            MimeType.ToStream(output);
            Caption.ToStream(output);
        }
    }

    public class TLInputMediaUploadedThumbVideo : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaUploadedThumbVideo;

        public TLInputFileBase File { get; set; }
        public TLInputFile Thumb { get; set; }
        public TLInt Duration { get; set; }
        public TLInt W { get; set; }
        public TLInt H { get; set; }
        public TLString MimeType { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Thumb.ToBytes(),
                Duration.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                MimeType.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            Thumb = GetObject<TLInputFile>(input);
            Duration = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Thumb.ToStream(output);
            Duration.ToStream(output);
            W.ToStream(output);
            H.ToStream(output);
            MimeType.ToStream(output);
        }
    }

    public class TLInputMediaUploadedThumbVideo28 : TLInputMediaUploadedThumbVideo, IInputMediaCaption
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedThumbVideo28;

        public TLString Caption { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Thumb.ToBytes(),
                Duration.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            Thumb = GetObject<TLInputFile>(input);
            Duration = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            Caption = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Thumb.ToStream(output);
            Duration.ToStream(output);
            W.ToStream(output);
            H.ToStream(output);
            Caption.ToStream(output);
        }
    }

    public class TLInputMediaUploadedThumbVideo36 : TLInputMediaUploadedThumbVideo28
    {
        public new const uint Signature = TLConstructors.TLInputMediaUploadedThumbVideo36;

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                File.ToBytes(),
                Thumb.ToBytes(),
                Duration.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                MimeType.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            File = GetObject<TLInputFile>(input);
            Thumb = GetObject<TLInputFile>(input);
            Duration = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            Caption = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            File.ToStream(output);
            Thumb.ToStream(output);
            Duration.ToStream(output);
            W.ToStream(output);
            H.ToStream(output);
            MimeType.ToStream(output);
            Caption.ToStream(output);
        }
    }

    public class TLInputMediaVideo : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaVideo;

        public TLInputVideoBase Id { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }
    }

    public class TLInputMediaVideo28 : TLInputMediaVideo, IInputMediaCaption
    {
        public new const uint Signature = TLConstructors.TLInputMediaVideo28;

        public TLString Caption { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Caption.ToBytes());
        }
    }

    public class TLInputMediaGifExternal : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaGifExternal;

        public TLString Url { get; set; }

        public TLString Q { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Url.ToBytes(),
                Q.ToBytes());
        }
    }

    public class TLInputMediaPhotoExternal75 : TLInputMediaPhotoExternal70
    {
        public new const uint Signature = TLConstructors.TLInputMediaPhotoExternal75;

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Url.ToBytes(),
                ToBytes(TTLSeconds, Flags, (int)InputMediaPhotoExternalFlags.TTLSeconds));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Url = GetObject<TLString>(input);
            TTLSeconds = GetObject<TLInt>(Flags, (int)InputMediaPhotoFlags.TTLSeconds, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Url.ToStream(output);
            ToStream(output, TTLSeconds, Flags, (int)InputMediaPhotoFlags.TTLSeconds);
        }
    }

    public class TLInputMediaPhotoExternal70 : TLInputMediaPhotoExternal, IInputTTLMedia
    {
        public new const uint Signature = TLConstructors.TLInputMediaPhotoExternal70;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLInt _ttlSeconds;

        public TLInt TTLSeconds
        {
            get { return _ttlSeconds; }
            set { SetField(out _ttlSeconds, value, ref _flags, (int)InputMediaPhotoExternalFlags.TTLSeconds); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Url.ToBytes(),
                Caption.ToBytes(),
                ToBytes(TTLSeconds, Flags, (int)InputMediaPhotoExternalFlags.TTLSeconds));
        }
    }

    public class TLInputMediaPhotoExternal : TLInputMediaBase, IInputMediaCaption
    {
        public const uint Signature = TLConstructors.TLInputMediaPhotoExternal;

        public TLString Url { get; set; }

        public TLString Caption { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Url.ToBytes(),
                Caption.ToBytes());
        }
    }

    public class TLInputMediaDocumentExternal75 : TLInputMediaDocumentExternal70
    {
        public new const uint Signature = TLConstructors.TLInputMediaDocumentExternal75;

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Url.ToBytes(),
                ToBytes(TTLSeconds, Flags, (int)InputMediaDocumentExternalFlags.TTLSeconds));
        }
    }

    public class TLInputMediaDocumentExternal70 : TLInputMediaDocumentExternal, IInputTTLMedia
    {
        public new const uint Signature = TLConstructors.TLInputMediaDocumentExternal70;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLInt _ttlSeconds;

        public TLInt TTLSeconds
        {
            get { return _ttlSeconds; }
            set { SetField(out _ttlSeconds, value, ref _flags, (int)InputMediaDocumentExternalFlags.TTLSeconds); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Url.ToBytes(),
                Caption.ToBytes(),
                ToBytes(TTLSeconds, Flags, (int)InputMediaDocumentExternalFlags.TTLSeconds));
        }
    }

    public class TLInputMediaDocumentExternal : TLInputMediaBase, IInputMediaCaption
    {
        public const uint Signature = TLConstructors.TLInputMediaDocumentExternal;

        public TLString Url { get; set; }

        public TLString Caption { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Url.ToBytes(),
                Caption.ToBytes());
        }
    }

    public class TLInputMediaGame : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaGame;

        public TLInputGameBase Id { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInputGameBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
        }
    }

    public class TLInputMediaInvoice73 : TLInputMediaInvoice
    {
        public new const uint Signature = TLConstructors.TLInputMediaInvoice73;

        public TLDataJSON ProviderData { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Title.ToBytes(),
                Description.ToBytes(),
                ToBytes(Photo, Flags, (int)InputMediaInvoiceFlags.Photo),
                Invoice.ToBytes(),
                Payload.ToBytes(),
                Provider.ToBytes(),
                ProviderData.ToBytes(),
                StartParam.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Title = GetObject<TLString>(input);
            Description = GetObject<TLString>(input);
            Photo = GetObject<TLInputWebDocument>(Flags, (int)InputMediaInvoiceFlags.Photo, null, input);
            Invoice = GetObject<TLInvoice>(input);
            Payload = GetObject<TLString>(input);
            Provider = GetObject<TLString>(input);
            ProviderData = GetObject<TLDataJSON>(input);
            StartParam = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Title.ToStream(output);
            Description.ToStream(output);
            ToStream(output, Photo, Flags, (int)InputMediaInvoiceFlags.Photo);
            Invoice.ToStream(output);
            Payload.ToStream(output);
            Provider.ToStream(output);
            ProviderData.ToStream(output);
            StartParam.ToStream(output);
        }
    }

    public class TLInputMediaInvoice : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaInvoice;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLString Title { get; set; }

        public TLString Description { get; set; }

        private TLInputWebDocument _photo;

        public TLInputWebDocument Photo
        {
            get { return _photo; }
            set { SetField(out _photo, value, ref _flags, (int) InputMediaInvoiceFlags.Photo); }
        }

        public TLInvoice Invoice { get; set; }

        public TLString Payload { get; set; }

        public TLString Provider { get; set; }

        public TLString StartParam { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Title.ToBytes(),
                Description.ToBytes(),
                ToBytes(Photo, Flags, (int) InputMediaInvoiceFlags.Photo),
                Invoice.ToBytes(),
                Payload.ToBytes(),
                Provider.ToBytes(),
                StartParam.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Title = GetObject<TLString>(input);
            Description = GetObject<TLString>(input);
            Photo = GetObject<TLInputWebDocument>(Flags, (int) InputMediaInvoiceFlags.Photo, null, input);
            Invoice = GetObject<TLInvoice>(input);
            Payload = GetObject<TLString>(input);
            Provider = GetObject<TLString>(input);
            StartParam = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Title.ToStream(output);
            Description.ToStream(output);
            ToStream(output, Photo, Flags, (int) InputMediaInvoiceFlags.Photo);
            Invoice.ToStream(output);
            Payload.ToStream(output);
            Provider.ToStream(output);
            StartParam.ToStream(output);
        }
    }

    public class TLInputMediaGeoLive : TLInputMediaBase
    {
        public const uint Signature = TLConstructors.TLInputMediaGeoLive;

        public TLInputGeoPointBase GeoPoint { get; set; }

        public TLInt Period { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                GeoPoint.ToBytes(),
                Period.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            GeoPoint = GetObject<TLInputGeoPointBase>(input);
            Period = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            GeoPoint.ToStream(output);
            Period.ToStream(output);
        }
    }
}

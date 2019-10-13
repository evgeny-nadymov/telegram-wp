// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.IO;
#if WP81
using Windows.Media.Transcoding;
#endif
using System.Linq;
#if WIN_RT
using Windows.UI.Xaml;
#else
using System.Windows;
#endif
#if WP8
using Windows.Storage;
#endif
using Telegram.Api.Services.Cache;
using Telegram.Api.Extensions;
using Telegram.Api.Services;

namespace Telegram.Api.TL
{
    [Flags]
    public enum MessageMediaPhotoFlags
    {
        Photo = 0x1,                        // 0
        Caption = 0x2,                      // 1
        TTLSeconds = 0x4,                   // 2
    }

    [Flags]
    public enum MessageMediaDocumentFlags
    {
        Document = 0x1,                     // 0
        Caption = 0x2,                      // 1
        TTLSeconds = 0x4,                   // 2
    }

    [Flags]
    public enum MessageMediaInvoiceFlags
    {
        Photo = 0x1,                        // 0
        ShippingAddressRequested = 0x2,     // 1
        ReceiptMsgId = 0x4,                 // 2
        Test = 0x8,                         // 3
    }

    public interface IMediaCaption
    {
        TLString Caption { get; set; }
    }

    public interface ITTLMessageMedia
    {
        TLInt TTLSeconds { get; set; }

        TTLParams TTLParams { get; set; }
    }

    public interface IMediaGifBase
    {
        double DownloadingProgress { get; set; }

        double LastProgress { get; set; }

        bool IsCanceled { get; set; }

        string IsoFileName { get; set; }

        bool? AutoPlayGif { get; set; }

        bool Forbidden { get; set; }
    }

    public interface IMediaGif : IMediaGifBase
    {
        TLDocumentBase Document { get; }
    }

    public interface IDecryptedMediaGif : IMediaGifBase
    {
        TLDecryptedMessageMediaDocument Document { get; }
    }

    public interface IMessageMediaGeoPoint
    {
        TLGeoPointBase Geo { get; }
    }

    public abstract class TLMessageMediaBase : TLObject
    {
        public bool Forbidden { get; set; }

        public virtual bool? AutoPlayGif { get; set; }

        public TLMessageMediaBase Self { get { return this; } }

        public TLMessageMediaBase ThumbSelf { get { return this; } }

        public virtual int MediaSize { get { return 0; } }

        private double _uploadingProgress;

        public double UploadingProgress
        {
            get { return _uploadingProgress; }
            set { SetField(ref _uploadingProgress, value, () => UploadingProgress); }
        }

        private double _downloadingProgress;

        public double DownloadingProgress
        {
            get { return _downloadingProgress; }
            set { SetField(ref _downloadingProgress, value, () => DownloadingProgress); }
        }

        private double _compressingProgress;

        public double CompressingProgress
        {
            get { return _compressingProgress; }
            set { SetField(ref _compressingProgress, value, () => CompressingProgress); }
        }

        public double LastProgress { get; set; }

        private bool _notListened;

        public bool NotListened
        {
            get { return _notListened; }
            set { _notListened = value; }
        }

        private bool _out = true;

        public bool Out
        {
            get { return _out; }
            set { _out = value; }
        }

        /// <summary>
        /// For internal use
        /// </summary>
        public TLLong FileId { get; set; }

        public string IsoFileName { get; set; }

#if WP8
        public StorageFile File { get; set; }
#endif
#if WP81
        public PrepareTranscodeResult TranscodeResult { get; set; }
#endif

        private bool _isCanceled;

        public bool IsCanceled
        {
            get { return _isCanceled; }
            set { _isCanceled = value; }
        }

        public virtual double MediaWidth
        {
            get { return 12.0 + 311.0 + 12.0; }
        }
    }

    public class TLMessageMediaEmpty : TLMessageMediaBase
    {
        public const uint Signature = TLConstructors.TLMessageMediaEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
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

    public class TLMessageMediaDocument75 : TLMessageMediaDocument70
    {
        public new const uint Signature = TLConstructors.TLMessageMediaDocument75;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _document = GetObject<TLDocumentBase>(Flags, (int)MessageMediaDocumentFlags.Document, null, bytes, ref position);
            Caption = null;
            _ttlSeconds = GetObject<TLInt>(Flags, (int)MessageMediaPhotoFlags.TTLSeconds, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            _document = GetObject<TLDocumentBase>(Flags, (int)MessageMediaDocumentFlags.Document, null, input);
            Caption = null;
            _ttlSeconds = GetObject<TLInt>(Flags, (int)MessageMediaDocumentFlags.TTLSeconds, null, input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, Document, Flags, (int)MessageMediaDocumentFlags.Document);
            ToStream(output, TTLSeconds, Flags, (int)MessageMediaDocumentFlags.TTLSeconds);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }

        public override string ToString()
        {
            return Document != null ? "TLMessageMediaDocument75 " + Document : "TLMessageMediaDocument70";
        }
    }

    public class TLMessageMediaDocument70 : TLMessageMediaDocument45, ITTLMessageMedia
    {
        public new const uint Signature = TLConstructors.TLMessageMediaDocument70;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        protected TLDocumentBase _document;

        public override TLDocumentBase Document
        {
            get { return _document; }
            set { SetField(out _document, value, ref _flags, (int)MessageMediaDocumentFlags.Document); }
        }

        protected TLString _caption;

        public override TLString Caption
        {
            get { return _caption; }
            set { SetField(out _caption, value, ref _flags, (int)MessageMediaDocumentFlags.Caption); }
        }

        protected TLInt _ttlSeconds;

        public TLInt TTLSeconds
        {
            get { return _ttlSeconds; }
            set { SetField(out _ttlSeconds, value, ref _flags, (int)MessageMediaDocumentFlags.TTLSeconds); }
        }

        public TTLParams TTLParams { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _document = GetObject<TLDocumentBase>(Flags, (int)MessageMediaDocumentFlags.Document, null, bytes, ref position);
            Caption = GetObject(Flags, (int)MessageMediaPhotoFlags.Caption, TLString.Empty, bytes, ref position);
            _ttlSeconds = GetObject<TLInt>(Flags, (int)MessageMediaPhotoFlags.TTLSeconds, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            _document = GetObject<TLDocumentBase>(Flags, (int)MessageMediaDocumentFlags.Document, null, input);
            Caption = GetObject(Flags, (int)MessageMediaDocumentFlags.Caption, TLString.Empty, input);
            _ttlSeconds = GetObject<TLInt>(Flags, (int)MessageMediaDocumentFlags.TTLSeconds, null, input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, Document, Flags, (int)MessageMediaDocumentFlags.Document);
            ToStream(output, Caption, Flags, (int)MessageMediaDocumentFlags.Caption);
            ToStream(output, TTLSeconds, Flags, (int)MessageMediaDocumentFlags.TTLSeconds);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }

        public override string ToString()
        {
            return Document != null ? "TLMessageMediaDocument70 " + Document : "TLMessageMediaDocument70";
        }
    }

    public class TLMessageMediaDocument45 : TLMessageMediaDocument, IMediaCaption
    {
        public new const uint Signature = TLConstructors.TLMessageMediaDocument45;

        private bool? _autoPlayGif;

        public override bool? AutoPlayGif
        {
            get
            {
                if (TLMessageBase.IsRoundVideo(Document))
                {
                    return true;
                }

                return _autoPlayGif;
            }
            set { _autoPlayGif = value; }
        }

        public virtual TLString Caption { get; set; }

        public TLString Waveform
        {
            get
            {
                var document = Document as TLDocument22;
                if (document != null)
                {
                    var documentAttributeAudio = document.Attributes.FirstOrDefault(x => x is TLDocumentAttributeAudio46) as TLDocumentAttributeAudio46;
                    if (documentAttributeAudio != null)
                    {
                        return documentAttributeAudio.Waveform;
                    }
                }

                return null;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Document = GetObject<TLDocumentBase>(bytes, ref position);
            Caption = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Document = GetObject<TLDocumentBase>(input);
            Caption = GetObject<TLString>(input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Document.ToStream(output);
            Caption.ToStream(output);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }
    }

    public class TLMessageMediaDocument : TLMessageMediaBase, IMediaGif
    {
        public const uint Signature = TLConstructors.TLMessageMediaDocument;

        public virtual TLDocumentBase Document { get; set; }

        public TLDocumentBase Video { get { return Document; } }

        public override int MediaSize
        {
            get
            {
                return Document.DocumentSize;
            }
        }

        public override double MediaWidth
        {
            get
            {
                var document = Document as TLDocument22;
                if (document != null)
                {
                    if (TLMessageBase.HasTTL(this))
                    {
                        return 2.0 + 256.0 + 2.0;
                    }
                    if (TLMessageBase.IsSticker(document))
                    {
                        var maxStickerDimension = 196.0;

                        return 6.0 + GetStickerDimension(document, true, maxStickerDimension) + 6.0;
                    }
                    if (TLMessageBase.IsVoice(document))
                    {
                        return 12.0 + 284.0 + 12.0;
                    }
                    if (TLMessageBase.IsRoundVideo(document))
                    {
                        return 2.0 + 240.0 + 2.0;
                    }
                    if (TLMessageBase.IsVideo(document))
                    {
                        return 2.0 + 230.0 + 2.0;
                    }
                    if (TLMessageBase.IsGif(document))
                    {
                        var maxGifDimension = 323.0;

                        return 2.0 + GetGifDimension(document.Thumb as IPhotoSize, document, true, maxGifDimension) + 2.0;
                    }
                }

                return base.MediaWidth;

            }
        }

        public static double GetGifDimension(IPhotoSize thumb, IAttributes source, bool isWidth, double maxGifDimension)
        {
            TLDocumentAttributeVideo videoAttribute = null;
            if (source != null)
            {
                for (var i = 0; i < source.Attributes.Count; i++)
                {
                    videoAttribute = source.Attributes[i] as TLDocumentAttributeVideo;
                    if (videoAttribute != null)
                    {
                        break;
                    }
                }
            }

            if (videoAttribute != null)
            {
                var width = videoAttribute.W.Value;
                var height = videoAttribute.H.Value;

                var maxDimension = width;
                if (maxDimension > 0)
                {
                    var scaleFactor = maxGifDimension / maxDimension;

                    return isWidth ? scaleFactor * width : scaleFactor * height;
                }
            }

            if (thumb != null)
            {
                var width = thumb.W.Value;
                var height = thumb.H.Value;

                var maxDimension = width;
                if (maxDimension > 0)
                {
                    var scaleFactor = maxGifDimension / maxDimension;

                    return isWidth ? scaleFactor * width : scaleFactor * height;
                }
            }

            return maxGifDimension;
        }

        public static double GetStickerDimension(IAttributes source, bool isWidth, double maxImageDimension)
        {
            TLDocumentAttributeImageSize imageSizeAttribute = null;
            for (var i = 0; i < source.Attributes.Count; i++)
            {
                imageSizeAttribute = source.Attributes[i] as TLDocumentAttributeImageSize;
                if (imageSizeAttribute != null)
                {
                    break;
                }
            }

            if (imageSizeAttribute != null)
            {
                var width = imageSizeAttribute.W.Value;
                var height = imageSizeAttribute.H.Value;

                var maxDimension = Math.Max(width, height);
                if (maxDimension > maxImageDimension)
                {
                    var scaleFactor = maxImageDimension / maxDimension;

                    return isWidth ? scaleFactor * width : scaleFactor * height;
                }

                return isWidth ? width : height;
            }

            return isWidth ? double.NaN : maxImageDimension;
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Document = GetObject<TLDocumentBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Document = GetObject<TLDocumentBase>(input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Document.ToStream(output);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }
    }

    public class TLMessageMediaAudio : TLMessageMediaBase
    {
        public const uint Signature = TLConstructors.TLMessageMediaAudio;

        public TLAudioBase Audio { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Audio = GetObject<TLAudioBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Audio = GetObject<TLAudioBase>(input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Audio.ToStream(output);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }
    }

    public class TLMessageMediaPhoto75 : TLMessageMediaPhoto70
    {
        public new const uint Signature = TLConstructors.TLMessageMediaPhoto75;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _photo = GetObject<TLPhotoBase>(Flags, (int)MessageMediaPhotoFlags.Photo, null, bytes, ref position);
            Caption = null;
            _ttlSeconds = GetObject<TLInt>(Flags, (int)MessageMediaPhotoFlags.TTLSeconds, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            _photo = GetObject<TLPhotoBase>(Flags, (int)MessageMediaPhotoFlags.Photo, null, input);
            Caption = null;
            _ttlSeconds = GetObject<TLInt>(Flags, (int)MessageMediaPhotoFlags.TTLSeconds, null, input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, Photo, Flags, (int)MessageMediaPhotoFlags.Photo);
            ToStream(output, TTLSeconds, Flags, (int)MessageMediaPhotoFlags.TTLSeconds);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }

        public override string ToString()
        {
            return Photo != null ? "TLMessageMediaPhoto75 " + Photo : "TLMessageMediaPhoto75";
        }
    }

    public class TLMessageMediaPhoto70 : TLMessageMediaPhoto28, ITTLMessageMedia
    {
        public new const uint Signature = TLConstructors.TLMessageMediaPhoto70;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        protected TLPhotoBase _photo;

        public override TLPhotoBase Photo
        {
            get { return _photo; }
            set { SetField(out _photo, value, ref _flags, (int)MessageMediaPhotoFlags.Photo); }
        }

        protected TLString _caption;

        public override TLString Caption
        {
            get { return _caption; }
            set { SetField(out _caption, value, ref _flags, (int)MessageMediaPhotoFlags.Caption); }
        }

        protected TLInt _ttlSeconds;

        public TLInt TTLSeconds
        {
            get { return _ttlSeconds; }
            set { SetField(out _ttlSeconds, value, ref _flags, (int)MessageMediaPhotoFlags.TTLSeconds); }
        }

        public TTLParams TTLParams { get; set; }

        public override double MediaWidth
        {
            get
            {
                if (TLMessageBase.HasTTL(this) && Photo != null)
                {
                    return 2.0 + 256.0 + 2.0;
                }

                return base.MediaWidth;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _photo = GetObject<TLPhotoBase>(Flags, (int)MessageMediaPhotoFlags.Photo, null, bytes, ref position);
            Caption = GetObject(Flags, (int)MessageMediaPhotoFlags.Caption, TLString.Empty, bytes, ref position);
            _ttlSeconds = GetObject<TLInt>(Flags, (int)MessageMediaPhotoFlags.TTLSeconds, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            _photo = GetObject<TLPhotoBase>(Flags, (int)MessageMediaPhotoFlags.Photo, null, input);
            Caption = GetObject(Flags, (int)MessageMediaPhotoFlags.Caption, TLString.Empty, input);
            _ttlSeconds = GetObject<TLInt>(Flags, (int)MessageMediaPhotoFlags.TTLSeconds, null, input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, Photo, Flags, (int)MessageMediaPhotoFlags.Photo);
            ToStream(output, Caption, Flags, (int)MessageMediaPhotoFlags.Caption);
            ToStream(output, TTLSeconds, Flags, (int)MessageMediaPhotoFlags.TTLSeconds);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }

        public override string ToString()
        {
            return Photo != null ? "TLMessageMediaPhoto70" + Photo : "TLMessageMediaPhoto70";
        }
    }

    public class TLMessageMediaPhoto28 : TLMessageMediaPhoto, IMediaCaption
    {
        public new const uint Signature = TLConstructors.TLMessageMediaPhoto28;

        public virtual TLString Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Photo = GetObject<TLPhotoBase>(bytes, ref position);
            Caption = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Photo = GetObject<TLPhotoBase>(input);
            Caption = GetObject<TLString>(input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Photo.ToStream(output);
            Caption.ToStream(output);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }

        public override string ToString()
        {
            return Photo.ToString();
        }
    }

    public class TLMessageMediaPhoto : TLMessageMediaBase
    {
        public const uint Signature = TLConstructors.TLMessageMediaPhoto;

        public virtual TLPhotoBase Photo { get; set; }

        public override double MediaWidth
        {
            get
            {
                var minVerticalRatioToScale = 1.2;
                var scale = 1.2; // must be less than minVerticalRatioToScale to avoid large square photos
                var maxDimension = 323.0;

                var photo = Photo as TLPhoto;
                if (photo != null)
                {
                    IPhotoSize size = null;
                    var sizes = photo.Sizes.OfType<IPhotoSize>();
                    foreach (var photoSize in sizes)
                    {
                        if (size == null
                            || Math.Abs(maxDimension - size.H.Value) > Math.Abs(maxDimension - photoSize.H.Value))
                        {
                            size = photoSize;
                        }
                    }

                    if (size != null)
                    {
                        if (size.H.Value > size.W.Value)
                        {
                            if (IsScaledVerticalPhoto(minVerticalRatioToScale, size.H, size.W))
                            {
                                return 2.0 + scale * maxDimension / size.H.Value * size.W.Value + 2.0;
                            }

                            return 2.0 + maxDimension / size.H.Value * size.W.Value + 2.0;
                        }

                        return 2.0 + maxDimension + 2.0;
                    }
                }

                return base.MediaWidth;
            }
        }

        public static bool IsScaledVerticalPhoto(double minRatio, TLInt heigth, TLInt width)
        {
            var ratio = (double)heigth.Value / width.Value;

            return ratio > minRatio;
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Photo = GetObject<TLPhotoBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Photo = GetObject<TLPhotoBase>(input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Photo.ToStream(output);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }

        public override string ToString()
        {
            return Photo.ToString();
        }
    }

    public class TLMessageMediaVideo28 : TLMessageMediaVideo, IMediaCaption
    {
        public new const uint Signature = TLConstructors.TLMessageMediaVideo28;

        public TLString Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Video = GetObject<TLVideoBase>(bytes, ref position);
            Caption = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Video = GetObject<TLVideoBase>(input);
            Caption = GetObject<TLString>(input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Video.ToStream(output);
            Caption.ToStream(output);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }
    }

    public class TLMessageMediaVideo : TLMessageMediaBase
    {
        public const uint Signature = TLConstructors.TLMessageMediaVideo;

        public TLVideoBase Video { get; set; }

        public override int MediaSize
        {
            get
            {
                return Video.VideoSize;
            }
        }


        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Video = GetObject<TLVideoBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Video = GetObject<TLVideoBase>(input);

            var isoFileName = GetObject<TLString>(input);
            IsoFileName = isoFileName.ToString();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Video.ToStream(output);

            var isoFileName = new TLString(IsoFileName);
            isoFileName.ToStream(output);
        }
    }

    public class TLMessageMediaGeo : TLMessageMediaBase, IMessageMediaGeoPoint
    {
        public const uint Signature = TLConstructors.TLMessageMediaGeo;

        public TLGeoPointBase Geo { get; set; }

        public override double MediaWidth
        {
            get { return 2.0 + 320.0 + 2.0; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Geo = GetObject<TLGeoPointBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Geo = GetObject<TLGeoPointBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Geo.ToStream(output);
        }
    }

    public class TLMessageMediaVenue72 : TLMessageMediaVenue
    {
        public new const uint Signature = TLConstructors.TLMessageMediaVenue72;

        public TLString VenueType { get; set; }

        #region Additional

        public Uri IconSource { get; set; }

        public TLUserBase User { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Geo = GetObject<TLGeoPointBase>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Address = GetObject<TLString>(bytes, ref position);
            Provider = GetObject<TLString>(bytes, ref position);
            VenueId = GetObject<TLString>(bytes, ref position);
            VenueType = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Geo = GetObject<TLGeoPointBase>(input);
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
            Geo.ToStream(output);
            Title.ToStream(output);
            Address.ToStream(output);
            Provider.ToStream(output);
            VenueId.ToStream(output);
            VenueType.ToStream(output);
        }
    }

    public class TLMessageMediaVenue : TLMessageMediaGeo
    {
        public new const uint Signature = TLConstructors.TLMessageMediaVenue;

        public TLString Title { get; set; }

        public TLString Address { get; set; }

        public TLString Provider { get; set; }

        public TLString VenueId { get; set; }

        public override double MediaWidth
        {
            get { return 2.0 + 320.0 + 2.0; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Geo = GetObject<TLGeoPointBase>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Address = GetObject<TLString>(bytes, ref position);
            Provider = GetObject<TLString>(bytes, ref position);
            VenueId = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Geo = GetObject<TLGeoPointBase>(input);
            Title = GetObject<TLString>(input);
            Address = GetObject<TLString>(input);
            Provider = GetObject<TLString>(input);
            VenueId = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Geo.ToStream(output);
            Title.ToStream(output);
            Address.ToStream(output);
            Provider.ToStream(output);
            VenueId.ToStream(output);
        }
    }

    public class TLMessageMediaContact82 : TLMessageMediaContact
    {
        public new const uint Signature = TLConstructors.TLMessageMediaContact82;

        public TLString VCard { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PhoneNumber = GetObject<TLString>(bytes, ref position);
            FirstName = GetObject<TLString>(bytes, ref position);
            LastName = GetObject<TLString>(bytes, ref position);
            VCard = GetObject<TLString>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PhoneNumber = GetObject<TLString>(input);
            FirstName = GetObject<TLString>(input);
            LastName = GetObject<TLString>(input);
            VCard = GetObject<TLString>(input);
            UserId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PhoneNumber.ToStream(output);
            FirstName.ToStream(output);
            LastName.ToStream(output);
            VCard.ToStream(output);
            UserId.ToStream(output);
        }
    }

    public class TLMessageMediaContact : TLMessageMediaBase
    {
        public const uint Signature = TLConstructors.TLMessageMediaContact;

        public TLString PhoneNumber { get; set; }

        public TLString FirstName { get; set; }

        public TLString LastName { get; set; }

        public TLInt UserId { get; set; }

        public override double MediaWidth
        {
            get { return 12.0 + 311.0 + 12.0; }
        }

        #region Additional

        public virtual string FullName { get { return string.Format("{0} {1}", FirstName, LastName); } }

        private TLUserBase _user;

        public TLUserBase User
        {
            get
            {
                if (_user != null) return _user;

                var cacheService = InMemoryCacheService.Instance;
                _user = cacheService.GetUser(UserId);
                if (_user == null)
                {
                    if (UserId.Value > 0)
                    {
                        _user = new TLUser
                        {
                            FirstName = FirstName,
                            LastName = LastName,
                            Id = UserId,
                            Phone = PhoneNumber,
                            Photo = new TLPhotoEmpty { Id = new TLLong(0) }
                        };
                    }
                    else
                    {
                        _user = new TLUserNotRegistered
                        {
                            FirstName = FirstName,
                            LastName = LastName,
                            Id = UserId,
                            Phone = PhoneNumber,
                            Photo = new TLPhotoEmpty { Id = new TLLong(0) }
                        };
                    }
                }

                return _user;
            }
        }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PhoneNumber = GetObject<TLString>(bytes, ref position);
            FirstName = GetObject<TLString>(bytes, ref position);
            LastName = GetObject<TLString>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            PhoneNumber = GetObject<TLString>(input);
            FirstName = GetObject<TLString>(input);
            LastName = GetObject<TLString>(input);
            UserId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            PhoneNumber.ToStream(output);
            FirstName.ToStream(output);
            LastName.ToStream(output);
            UserId.ToStream(output);
        }

        public override string ToString()
        {
            return FullName;
        }
    }

    public abstract class TLMessageMediaUnsupportedBase : TLMessageMediaBase { }

    public class TLMessageMediaUnsupported : TLMessageMediaUnsupportedBase
    {
        public const uint Signature = TLConstructors.TLMessageMediaUnsupported;

        public TLString Bytes { get; set; }

        public override double MediaWidth
        {
            get { return 12.0 + 311.0 + 12.0; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Bytes = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Bytes = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Bytes.ToStream(output);
        }
    }

    public class TLMessageMediaUnsupported24 : TLMessageMediaUnsupportedBase
    {
        public const uint Signature = TLConstructors.TLMessageMediaUnsupported24;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
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

    public class TLMessageMediaWebPage : TLMessageMediaBase, IMediaGif
    {
        public const uint Signature = TLConstructors.TLMessageMediaWebPage;

        public TLWebPageBase WebPage { get; set; }

        #region Additional

        public TLDocumentBase Document
        {
            get
            {
                var webPage = WebPage as TLWebPage35;
                if (webPage != null)
                {
                    return webPage.Document;
                }

                return null;
            }
        }

        public TLPhotoBase Photo
        {
            get
            {
                var webPage = WebPage as TLWebPage;
                if (webPage != null)
                {
                    return webPage.Photo;
                }

                return null;
            }
        }

        #endregion

        public override double MediaWidth
        {
            get { return 12.0 + 311.0 + 12.0; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            WebPage = GetObject<TLWebPageBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            WebPage = GetObject<TLWebPageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            WebPage.ToStream(output);
        }
    }

    public class TLMessageMediaGame : TLMessageMediaBase, IMediaGif
    {
        public const uint Signature = TLConstructors.TLMessageMediaGame;

        public TLGame Game { get; set; }

        #region Additional
        public TLDocumentBase Document
        {
            get
            {
                if (Game != null)
                {
                    return Game.Document;
                }

                return null;
            }
        }

        public TLPhotoBase Photo
        {
            get
            {
                if (Game != null)
                {
                    return Game.Photo;
                }

                return null;
            }
        }
        #endregion

        public override double MediaWidth
        {
            get { return 12.0 + 311.0 + 12.0; }
        }

        public Visibility MessageVisibility
        {
            get
            {
                return !TLString.IsNullOrEmpty(Message)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public Visibility DescriptionVisibility
        {
            get
            {
                return TLString.IsNullOrEmpty(Message) && Game != null && !TLString.IsNullOrEmpty(Game.Description)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public TLString Message { get; set; }

        public TLMessageBase SourceMessage { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Game = GetObject<TLGame>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Game = GetObject<TLGame>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Game.ToStream(output);
        }
    }

    public class TLMessageMediaInvoice : TLMessageMediaBase
    {
        public const uint Signature = TLConstructors.TLMessageMediaInvoice;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool ShippingAddressRequested
        {
            get { return IsSet(Flags, (int)MessageMediaInvoiceFlags.ShippingAddressRequested); }
            set { SetUnset(ref _flags, value, (int)MessageMediaInvoiceFlags.ShippingAddressRequested); }
        }

        public bool Test
        {
            get { return IsSet(Flags, (int)MessageMediaInvoiceFlags.Test); }
            set { SetUnset(ref _flags, value, (int)MessageMediaInvoiceFlags.Test); }
        }

        public TLString Title { get; set; }

        public TLString Description { get; set; }

        private TLWebDocumentBase _photo;

        public TLWebDocumentBase Photo
        {
            get { return _photo; }
            set { SetField(out _photo, value, ref _flags, (int)MessageMediaInvoiceFlags.Photo); }
        }

        private TLInt _receiptMsgId;

        public TLInt ReceiptMsgId
        {
            get { return _receiptMsgId; }
            set { SetField(out _receiptMsgId, value, ref _flags, (int)MessageMediaInvoiceFlags.ReceiptMsgId); }
        }

        public TLString Currency { get; set; }

        public TLLong TotalAmount { get; set; }

        public TLString StartParam { get; set; }

        public Visibility DescriptionVisibility
        {
            get
            {
                return !TLString.IsNullOrEmpty(Description)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public override double MediaWidth
        {
            get { return 2.0 + 323.0 + 2.0; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Description = GetObject<TLString>(bytes, ref position);
            _photo = GetObject<TLWebDocumentBase>(Flags, (int)MessageMediaInvoiceFlags.Photo, null, bytes, ref position);
            _receiptMsgId = GetObject<TLInt>(Flags, (int)MessageMediaInvoiceFlags.ReceiptMsgId, null, bytes, ref position);
            Currency = GetObject<TLString>(bytes, ref position);
            TotalAmount = GetObject<TLLong>(bytes, ref position);
            StartParam = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Title = GetObject<TLString>(input);
            Description = GetObject<TLString>(input);
            _photo = GetObject<TLWebDocumentBase>(Flags, (int)MessageMediaInvoiceFlags.Photo, null, input);
            _receiptMsgId = GetObject<TLInt>(Flags, (int)MessageMediaInvoiceFlags.ReceiptMsgId, null, input);
            Currency = GetObject<TLString>(input);
            TotalAmount = GetObject<TLLong>(input);
            StartParam = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Title.ToStream(output);
            Description.ToStream(output);
            ToStream(output, Photo, Flags, (int)MessageMediaInvoiceFlags.Photo);
            ToStream(output, ReceiptMsgId, Flags, (int)MessageMediaInvoiceFlags.ReceiptMsgId);
            Currency.ToStream(output);
            TotalAmount.ToStream(output);
            StartParam.ToStream(output);
        }
    }

    public class TLMessageMediaGeoLive : TLMessageMediaGeo
    {
        public new const uint Signature = TLConstructors.TLMessageMediaGeoLive;

        public TLInt Period { get; set; }

        #region Additional

        public TLObject From { get; set; }

        public TLInt EditDate { get; set; }

        public TLInt Date { get; set; }

        public bool Active
        {
            get
            {
                if (Date != null)
                {
                    if (Period.Value == 0)
                    {
                        return false;
                    }

                    var mtProtoService = MTProtoService.Instance;
                    var clientDelta = mtProtoService.ClientTicksDelta;

                    var now = TLUtils.DateToUniversalTimeTLInt(clientDelta, DateTime.Now);

                    var expired = Date.Value + Period.Value <= now.Value;

                    var defaultPeriod = Period.Value == 15 * 60 || Period.Value == 60 * 60 || Period.Value == 8 * 60 * 60;

                    return defaultPeriod && !expired;

                    //var mtProtoService = MTProtoService.Instance;
                    //var clientDelta = mtProtoService.ClientTicksDelta;

                    //if (clientDelta == 0)
                    //{
                    //    var defaultPeriod = Period.Value == 15 * 60 || Period.Value == 60 * 60 || Period.Value == 8 * 60 * 60;

                    //    if (!defaultPeriod)
                    //    {
                    //        return false;
                    //    }
                    //}

                    //var date = TLUtils.DateToUniversalTimeTLInt(clientDelta, DateTime.Now);

                    //return Date.Value + Period.Value > date.Value;
                }

                return false;
            }
        }

        #endregion

        public override double MediaWidth
        {
            get { return 2.0 + 320.0 + 2.0; }
        }

        public TLMessageMediaBase ToMediaGeo()
        {
            return new TLMessageMediaGeo
            {
                Geo = Geo
            };
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Geo = GetObject<TLGeoPointBase>(bytes, ref position);
            Period = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Geo = GetObject<TLGeoPointBase>(input);
            Period = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Geo.ToStream(output);
            Period.ToStream(output);
        }
    }

    public interface IMessageMediaGroup
    {
        TLVector<TLObject> GroupCommon { get; }
        event EventHandler Calculate;
    }

    public class TLMessageMediaGroup : TLMessageMediaBase, IMessageMediaGroup
    {
        public const uint Signature = TLConstructors.TLMessageMediaGroup;

        public TLVector<TLMessageBase> Group { get; set; }

        public TLVector<TLObject> GroupCommon
        {
            get
            {
                if (Group == null)
                {
                    return new TLVector<TLObject>();
                }

                var group = new TLVector<TLObject>();
                foreach (var item in Group)
                {
                    group.Add(item);
                }
                return group;
            }
        }

        public override double MediaWidth
        {
            get { return 1.0 + 311.0 + 1.0; }
        }

        public event EventHandler Calculate;

        public virtual void RaiseCalculate()
        {
            var handler = Calculate;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Group = GetObject<TLVector<TLMessageBase>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Group = GetObject<TLVector<TLMessageBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Group.ToStream(output);
        }
    }
}

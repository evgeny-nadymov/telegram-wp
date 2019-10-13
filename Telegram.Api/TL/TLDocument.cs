// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.Linq;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public interface IAttributes
    {
        TLVector<TLDocumentAttributeBase> Attributes { get; set; }
    }

    public abstract class TLDocumentBase : TLObject
    {
        public long Index
        {
            get { return Id != null ? Id.Value : 0; }
        }

        public TLLong Id { get; set; }

        public string ShortId { get { return Id != null ? (Id.Value % 1000).ToString() : "unknown"; } }

        public virtual int DocumentSize { get { return 0; } }

        public static bool DocumentEquals(TLDocumentBase document1, TLDocumentBase document2)
        {
            var doc1 = document1 as TLDocument;
            var doc2 = document2 as TLDocument;

            if (doc1 == null || doc2 == null) return false;

            return doc1.Id.Value == doc2.Id.Value
                   && doc1.DCId.Value == doc2.DCId.Value
                   && doc1.AccessHash.Value == doc2.AccessHash.Value;
        }
    }

    public class TLDocumentExternal : TLDocumentBase, IAttributes
    {
        public const uint Signature = TLConstructors.TLDocumentExternal;

        public TLString ResultId { get; set; }

        public TLString Type { get; set; }

        public TLString Url { get; set; }

        public TLString ThumbUrl { get; set; }

        public TLString ContentUrl { get; set; }

        public TLString ContentType { get; set; }

        public TLVector<TLDocumentAttributeBase> Attributes { get; set; }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            ResultId = GetObject<TLString>(input);
            Type = GetObject<TLString>(input);
            ThumbUrl = GetObject<TLString>(input);
            ContentType = GetObject<TLString>(input);
            ContentUrl = GetObject<TLString>(input);
            Url = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
            ResultId.ToStream(output);
            Type.ToStream(output);
            ThumbUrl.ToStream(output);
            ContentType.ToStream(output);
            ContentUrl.ToStream(output);
            Url.ToStream(output);
            Attributes.ToStream(output);
        }

        public string GetFileName()
        {
            string extension;
            if (!TLString.IsNullOrEmpty(Url))
            {
                extension = Path.GetExtension(Url.ToString());

                if (!string.IsNullOrEmpty(extension))
                {
                    return ResultId + extension;
                }
            }
            if (!TLString.IsNullOrEmpty(ContentUrl))
            {
                extension = Path.GetExtension(ContentUrl.ToString());

                if (!string.IsNullOrEmpty(extension))
                {
                    return ResultId + extension;
                }
            }

            return ResultId + TLUtils.ContentTypeToFileExt(ContentType);
        }
    }

    public class TLDocumentEmpty : TLDocumentBase
    {
        public const uint Signature = TLConstructors.TLDocumentEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(TLUtils.SignatureToBytes(Signature), Id.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
        }
    }

    public abstract class TLDocument : TLDocumentBase
    {
        public TLLong AccessHash { get; set; }

        public TLInt Date { get; set; }

        public TLString MimeType { get; set; }

        public TLInt Size { get; set; }

        public override int DocumentSize
        {
            get { return Size != null ? Size.Value : 0; }
        }

        public TLPhotoSizeBase Thumb { get; set; }

        public TLInt DCId { get; set; }

        public byte[] Buffer { get; set; }

        public TLInputFileBase ThumbInputFile { get; set; }

        public virtual TLInputDocumentFileLocation ToInputFileLocation()
        {
            return new TLInputDocumentFileLocation { AccessHash = AccessHash, Id = Id };
        }

        public abstract TLString FileName { get; set; }

        public abstract string DocumentName { get; }

        public string FileExt
        {
            get { return Path.GetExtension(FileName.ToString()).Replace(".", string.Empty); }
        }

        public virtual string GetFileName()
        {
            return string.Format("document{0}_{1}.{2}", Id, AccessHash, FileExt);
        }
    }

    public class TLDocument10 : TLDocument
    {
        public const uint Signature = TLConstructors.TLDocument;

        public TLInt UserId { get; set; }

        public override TLString FileName { get; set; }

        public override string DocumentName { get { return FileName != null ? FileName.ToString() : string.Empty; } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            FileName = GetObject<TLString>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Thumb = GetObject<TLPhotoSizeBase>(bytes, ref position);
            DCId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                UserId.ToBytes(),
                Date.ToBytes(),
                FileName.ToBytes(),
                MimeType.ToBytes(),
                Size.ToBytes(),
                Thumb.ToBytes(),
                DCId.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            UserId = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);
            FileName = GetObject<TLString>(input);
            MimeType = GetObject<TLString>(input);
            Size = GetObject<TLInt>(input);
            Thumb = GetObject<TLPhotoSizeBase>(input);
            DCId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            AccessHash.ToStream(output);
            UserId.ToStream(output);
            Date.ToStream(output);
            FileName.ToStream(output);
            MimeType.ToStream(output);
            Size.ToStream(output);
            Thumb.ToStream(output);
            DCId.ToStream(output);
        }
    }

    public class TLDocument22 : TLDocument, IAttributes
    {
        public const uint Signature = TLConstructors.TLDocument22;

        public TLVector<TLDocumentAttributeBase> Attributes { get; set; }

        public bool Music
        {
            get
            {
                var documentAttributeAudio = Attributes.FirstOrDefault(x => x is TLDocumentAttributeAudio46) as TLDocumentAttributeAudio46;
                if (documentAttributeAudio != null)
                {
                    return !documentAttributeAudio.Voice;
                }

                return false;
            }
        }

        public override string DocumentName
        {
            get
            {
                var documentAttributeAudio = Attributes.FirstOrDefault(x => x is TLDocumentAttributeAudio46) as TLDocumentAttributeAudio46;
                if (documentAttributeAudio != null)
                {
                    if (documentAttributeAudio.Title != null && documentAttributeAudio.Performer != null)
                    {
                        return string.Format("{0} — {1}", documentAttributeAudio.Title, documentAttributeAudio.Performer);
                    }

                    if (documentAttributeAudio.Title != null)
                    {
                        return string.Format("{0}", documentAttributeAudio.Title);
                    }

                    if (documentAttributeAudio.Performer != null)
                    {
                        return string.Format("{0}", documentAttributeAudio.Performer);
                    }
                }

                return FileName.ToString();
            }
        }

        public override string GetFileName()
        {
            if (TLMessageBase.IsVideo(this))
            {
                return string.Format("video{0}_{1}.{2}", Id, AccessHash, "mp4");
            }
            if (TLMessageBase.IsVoice(this))
            {
                return string.Format("audio{0}_{1}.{2}", Id, AccessHash, "mp3");
            }

            return string.Format("document{0}_{1}.{2}", Id, AccessHash, FileExt);
        }

        public override TLInputDocumentFileLocation ToInputFileLocation()
        {
            return new TLInputDocumentFileLocation
            {
                Id = Id,
                AccessHash = AccessHash
            };
        }

        public TLInt Duration
        {
            get
            {
                if (Attributes != null)
                {
                    for (var i = 0; i < Attributes.Count; i++)
                    {
                        var durationAttribute = Attributes[i] as IAttributeDuration;
                        if (durationAttribute != null)
                        {
                            return durationAttribute.Duration;
                        }
                    }
                }

                return new TLInt(0);
            }
        }

        public string DurationString
        {
            get
            {
                var timeSpan = TimeSpan.FromSeconds(Duration.Value);

                if (timeSpan.Hours > 0)
                {
                    return timeSpan.ToString(@"h\:mm\:ss");
                }

                return timeSpan.ToString(@"m\:ss");
            }
        }

        public TLInt ImageSizeH
        {
            get
            {
                if (Attributes != null)
                {
                    for (var i = 0; i < Attributes.Count; i++)
                    {
                        var imageSizeAttribute = Attributes[i] as TLDocumentAttributeImageSize;
                        if (imageSizeAttribute != null)
                        {
                            return imageSizeAttribute.H;
                        }
                    }
                }

                return new TLInt(0);
            }
        }

        public TLInt ImageSizeW
        {
            get
            {
                if (Attributes != null)
                {
                    for (var i = 0; i < Attributes.Count; i++)
                    {
                        var imageSizeAttribute = Attributes[i] as TLDocumentAttributeImageSize;
                        if (imageSizeAttribute != null)
                        {
                            return imageSizeAttribute.W;
                        }
                    }
                }

                return new TLInt(0);
            }
        }

        public override TLString FileName
        {
            get
            {
                if (Attributes != null)
                {
                    for (var i = 0; i < Attributes.Count; i++)
                    {
                        var fileNameAttribute = Attributes[i] as TLDocumentAttributeFileName;
                        if (fileNameAttribute != null)
                        {
                            return fileNameAttribute.FileName;
                        }
                    }
                }

                return TLString.Empty;
            }
            set
            {
                Attributes = Attributes ?? new TLVector<TLDocumentAttributeBase>();

                for (var i = 0; i < Attributes.Count; i++)
                {
                    if (Attributes[i] is TLDocumentAttributeFileName)
                    {
                        Attributes.RemoveAt(i--);
                    }
                }

                Attributes.Add(new TLDocumentAttributeFileName{FileName = value});
            }
        }

        public TLInputStickerSetBase StickerSet
        {
            get
            {
                if (Attributes != null)
                {
                    for (var i = 0; i < Attributes.Count; i++)
                    {
                        var stickerAttribute = Attributes[i] as TLDocumentAttributeSticker29;
                        if (stickerAttribute != null)
                        {
                            return stickerAttribute.Stickerset;
                        }
                    }
                }

                return null;
            }
        }

        #region Additional

        private string _emoticon;

        public string Emoticon
        {
            get
            {
                if (_emoticon != null) return _emoticon;

                if (Attributes != null)
                {
                    for (var i = 0; i < Attributes.Count; i++)
                    {
                        var stickerAttribute = Attributes[i] as TLDocumentAttributeSticker29;
                        if (stickerAttribute != null)
                        {
                            _emoticon = stickerAttribute.Alt.ToString();
                        }
                    }
                }

                _emoticon = _emoticon ?? string.Empty;

                return _emoticon;
            }
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0} Id={1}", GetType().Name, Id) + (StickerSet != null ? StickerSet.ToString() : string.Empty);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Thumb = GetObject<TLPhotoSizeBase>(bytes, ref position);
            DCId = GetObject<TLInt>(bytes, ref position);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Date.ToBytes(),
                MimeType.ToBytes(),
                Size.ToBytes(),
                Thumb.ToBytes(),
                DCId.ToBytes(),
                Attributes.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            Size = GetObject<TLInt>(input);
            Thumb = GetObject<TLPhotoSizeBase>(input);
            DCId = GetObject<TLInt>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            AccessHash.ToStream(output);
            Date.ToStream(output);
            MimeType.ToStream(output);
            Size.ToStream(output);
            Thumb.ToStream(output);
            DCId.ToStream(output);
            Attributes.ToStream(output);
        }
    }

    public class TLDocument54 : TLDocument22
    {
        public new const uint Signature = TLConstructors.TLDocument54;

        public bool Mask
        {
            get
            {
                if (Attributes != null)
                {
                    for (var i = 0; i < Attributes.Count; i++)
                    {
                        var stickerAttribute = Attributes[i] as TLDocumentAttributeSticker56;
                        if (stickerAttribute != null)
                        {
                            return stickerAttribute.Mask;
                        }
                    }
                }

                return false;
            }
        }

        public TLInt Version { get; set; }

        public override string ToString()
        {
            return string.Format("{0} id={1} version={2}", GetType().Name, Id, Version) + (StickerSet != null ? " stickerset=[" + StickerSet + "]" : string.Empty);
        }

        public override string GetFileName()
        {
            if (TLMessageBase.IsVideo(this))
            {
                return string.Format("video{0}_{1}.{2}", Id, AccessHash, "mp4");
            }
            if (TLMessageBase.IsVoice(this))
            {
                return string.Format("audio{0}_{1}.{2}", Id, AccessHash, "mp3");
            }

            var documentVersion = Version;
            if (documentVersion != null && documentVersion.Value > 0)
            {
                return string.Format("document{0}_{1}.{2}", Id, documentVersion, FileExt);
            }

            return string.Format("document{0}_{1}.{2}", Id, AccessHash, FileExt);
        }

        public override TLInputDocumentFileLocation ToInputFileLocation()
        {
            return new TLInputDocumentFileLocation54
            {
                Id = Id,
                AccessHash = AccessHash,
                Version = Version
            };
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Thumb = GetObject<TLPhotoSizeBase>(bytes, ref position);
            DCId = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Date.ToBytes(),
                MimeType.ToBytes(),
                Size.ToBytes(),
                Thumb.ToBytes(),
                DCId.ToBytes(),
                Version.ToBytes(),
                Attributes.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            Size = GetObject<TLInt>(input);
            Thumb = GetObject<TLPhotoSizeBase>(input);
            DCId = GetObject<TLInt>(input);
            Version = GetObject<TLInt>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            AccessHash.ToStream(output);
            Date.ToStream(output);
            MimeType.ToStream(output);
            Size.ToStream(output);
            Thumb.ToStream(output);
            DCId.ToStream(output);
            Version.ToStream(output);
            Attributes.ToStream(output);
        }
    }
}

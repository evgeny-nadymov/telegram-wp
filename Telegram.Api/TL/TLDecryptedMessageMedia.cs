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
#if WP8
using Windows.Storage;
#endif
using Telegram.Api.Extensions;
using Telegram.Api.Services.Cache;

namespace Telegram.Api.TL
{
    public class TTLParams
    {
        public bool IsStarted { get; set; }
        public int Total { get; set; }
        public DateTime StartTime { get; set; }
        public bool Out { get; set; }
    }

    public abstract class TLDecryptedMessageMediaBase : TLObject
    {
        public double LastProgress { get; set; }
        public string IsoFileName { get; set; }
        public bool? AutoPlayGif { get; set; }
        public bool Forbidden { get; set; }
        public TLInt TTLSeconds { get; set; }

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
        #region Additional
#if WP8
        public StorageFile StorageFile { get; set; }
#endif
        private TLEncryptedFileBase _file;

        public TLEncryptedFileBase File
        {
            get { return _file; }
            set { _file = value; }
        }

        public TLDecryptedMessageMediaBase Self { get { return this; } }

        public TLDecryptedMessageMediaBase ThumbSelf { get { return this; } }

        public bool IsCanceled { get; set; }

        public TTLParams _ttlParams;

        public TTLParams TTLParams
        {
            get { return _ttlParams; }
            set { SetField(ref _ttlParams, value, () => TTLParams); }
        }

        public bool NotListened { get; set; }

        private bool _out = true;

        public bool Out
        {
            get { return _out; }
            set { _out = value; }
        }

        public virtual double MediaWidth
        {
            get { return 12.0 + 311.0 + 12.0; }
        }

        #endregion
    }

    public class TLDecryptedMessageMediaEmpty : TLDecryptedMessageMediaBase
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageMediaEmpty;

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

    public class TLDecryptedMessageThumbMediaBase : TLDecryptedMessageMediaBase
    {
        public TLString Thumb { get; set; }

        public TLInt ThumbW { get; set; }

        public TLInt ThumbH { get; set; }

        public TLInt Size { get; set; }

        public TLString Key { get; set; }

        public TLString IV { get; set; }
    }

    public class TLDecryptedMessageMediaPhoto45 : TLDecryptedMessageMediaPhoto, IMediaCaption
    {
        public new const uint Signature = TLConstructors.TLDecryptedMessageMediaPhoto45;

        public TLString Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Thumb = GetObject<TLString>(bytes, ref position);
            ThumbW = GetObject<TLInt>(bytes, ref position);
            ThumbH = GetObject<TLInt>(bytes, ref position);
            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Key = GetObject<TLString>(bytes, ref position);
            IV = GetObject<TLString>(bytes, ref position);
            Caption = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Thumb.ToBytes(),
                ThumbW.ToBytes(),
                ThumbH.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                Size.ToBytes(),
                Key.ToBytes(),
                IV.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Thumb = GetObject<TLString>(input);
            ThumbW = GetObject<TLInt>(input);
            ThumbH = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            Size = GetObject<TLInt>(input);
            Key = GetObject<TLString>(input);
            IV = GetObject<TLString>(input);
            Caption = GetObject<TLString>(input);

            File = GetNullableObject<TLEncryptedFileBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Thumb.ToBytes());
            output.Write(ThumbW.ToBytes());
            output.Write(ThumbH.ToBytes());
            output.Write(W.ToBytes());
            output.Write(H.ToBytes());
            output.Write(Size.ToBytes());
            output.Write(Key.ToBytes());
            output.Write(IV.ToBytes());
            output.Write(Caption.ToBytes());

            File.NullableToStream(output);
        }
    }

    public class TLDecryptedMessageMediaPhoto : TLDecryptedMessageThumbMediaBase
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageMediaPhoto;

        public TLInt W { get; set; }

        public TLInt H { get; set; }

        #region Additional

        public TLEncryptedFileBase Photo { get { return File; } }
        #endregion

        public override double MediaWidth
        {
            get
            {
                var ttl = TTLSeconds;
                if (ttl != null && ttl.Value > 0 && ttl.Value <= 60.0)
                {
                    return 2.0 + 256.0 + 2.0;
                }

                var minVerticalRatioToScale = 1.2;
                var scale = 1.2; // must be less than minVerticalRatioToScale to avoid large square photos
                var maxDimension = 323.0;

                if (H.Value > W.Value)
                {
                    if (TLMessageMediaPhoto.IsScaledVerticalPhoto(minVerticalRatioToScale, H, W))
                    {
                        return 2.0 + scale * maxDimension / H.Value * W.Value + 2.0;
                    }

                    return 2.0 + maxDimension / H.Value * W.Value + 2.0;
                }

                return 2.0 + maxDimension + 2.0;
            }
        }

        //public override double MediaWidth
        //{
        //    get
        //    {
        //        if (TLMessageBase.HasTTL(this) && Photo != null)
        //        {
        //            return 2.0 + 256.0 + 2.0;
        //        }

        //        return base.MediaWidth;
        //    }
        //}

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Thumb = GetObject<TLString>(bytes, ref position);
            ThumbW = GetObject<TLInt>(bytes, ref position);
            ThumbH = GetObject<TLInt>(bytes, ref position);
            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Key = GetObject<TLString>(bytes, ref position);
            IV = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Thumb.ToBytes(),
                ThumbW.ToBytes(),
                ThumbH.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                Size.ToBytes(),
                Key.ToBytes(),
                IV.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Thumb = GetObject<TLString>(input);
            ThumbW = GetObject<TLInt>(input);
            ThumbH = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            Size = GetObject<TLInt>(input);
            Key = GetObject<TLString>(input);
            IV = GetObject<TLString>(input);

            File = GetNullableObject<TLEncryptedFileBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Thumb.ToBytes());
            output.Write(ThumbW.ToBytes());
            output.Write(ThumbH.ToBytes());
            output.Write(W.ToBytes());
            output.Write(H.ToBytes());
            output.Write(Size.ToBytes());
            output.Write(Key.ToBytes());
            output.Write(IV.ToBytes());

            File.NullableToStream(output);
        }
    }

    public class TLDecryptedMessageMediaVideo45 : TLDecryptedMessageMediaVideo17, IMediaCaption
    {
        public new const uint Signature = TLConstructors.TLDecryptedMessageMediaVideo45;

        public TLString Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Thumb = GetObject<TLString>(bytes, ref position);
            ThumbW = GetObject<TLInt>(bytes, ref position);
            ThumbH = GetObject<TLInt>(bytes, ref position);
            Duration = GetObject<TLInt>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Key = GetObject<TLString>(bytes, ref position);
            IV = GetObject<TLString>(bytes, ref position);
            Caption = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Thumb.ToBytes(),
                ThumbW.ToBytes(),
                ThumbH.ToBytes(),
                Duration.ToBytes(),
                MimeType.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                Size.ToBytes(),
                Key.ToBytes(),
                IV.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Thumb = GetObject<TLString>(input);
            ThumbW = GetObject<TLInt>(input);
            ThumbH = GetObject<TLInt>(input);
            Duration = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            Size = GetObject<TLInt>(input);
            Key = GetObject<TLString>(input);
            IV = GetObject<TLString>(input);
            Caption = GetObject<TLString>(input);

            File = GetNullableObject<TLEncryptedFileBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Thumb.ToBytes());
            output.Write(ThumbW.ToBytes());
            output.Write(ThumbH.ToBytes());
            output.Write(Duration.ToBytes());
            output.Write(MimeType.ToBytes());
            output.Write(W.ToBytes());
            output.Write(H.ToBytes());
            output.Write(Size.ToBytes());
            output.Write(Key.ToBytes());
            output.Write(IV.ToBytes());
            output.Write(Caption.ToBytes());

            File.NullableToStream(output);
        }
    }

    public class TLDecryptedMessageMediaVideo17 : TLDecryptedMessageMediaVideo
    {
        public new const uint Signature = TLConstructors.TLDecryptedMessageMediaVideo17;

        public TLString MimeType { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Thumb = GetObject<TLString>(bytes, ref position);
            ThumbW = GetObject<TLInt>(bytes, ref position);
            ThumbH = GetObject<TLInt>(bytes, ref position);
            Duration = GetObject<TLInt>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Key = GetObject<TLString>(bytes, ref position);
            IV = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Thumb.ToBytes(),
                ThumbW.ToBytes(),
                ThumbH.ToBytes(),
                Duration.ToBytes(),
                MimeType.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                Size.ToBytes(),
                Key.ToBytes(),
                IV.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Thumb = GetObject<TLString>(input);
            ThumbW = GetObject<TLInt>(input);
            ThumbH = GetObject<TLInt>(input);
            Duration = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            Size = GetObject<TLInt>(input);
            Key = GetObject<TLString>(input);
            IV = GetObject<TLString>(input);

            File = GetNullableObject<TLEncryptedFileBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Thumb.ToBytes());
            output.Write(ThumbW.ToBytes());
            output.Write(ThumbH.ToBytes());
            output.Write(Duration.ToBytes());
            output.Write(MimeType.ToBytes());
            output.Write(W.ToBytes());
            output.Write(H.ToBytes());
            output.Write(Size.ToBytes());
            output.Write(Key.ToBytes());
            output.Write(IV.ToBytes());

            File.NullableToStream(output);
        }
    }

    public class TLDecryptedMessageMediaVideo : TLDecryptedMessageThumbMediaBase
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageMediaVideo;

        public TLInt Duration { get; set; }

        public TLInt W { get; set; }

        public TLInt H { get; set; }

        #region Additional

        public TLDecryptedMessageMediaVideo Video { get { return this; } }

        public string DurationString
        {
            get
            {
                if (Duration == null) return string.Empty;

                var timeSpan = TimeSpan.FromSeconds(Duration.Value);

                if (timeSpan.Hours > 0)
                {
                    return timeSpan.ToString(@"h\:mm\:ss");
                }

                return timeSpan.ToString(@"m\:ss");
            }
        }
        #endregion

        public override double MediaWidth
        {
            get { return 2.0 + 230.0 + 2.0; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Thumb = GetObject<TLString>(bytes, ref position);
            ThumbW = GetObject<TLInt>(bytes, ref position);
            ThumbH = GetObject<TLInt>(bytes, ref position);
            Duration = GetObject<TLInt>(bytes, ref position);
            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Key = GetObject<TLString>(bytes, ref position);
            IV = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Thumb.ToBytes(),
                ThumbW.ToBytes(),
                ThumbH.ToBytes(),
                Duration.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                Size.ToBytes(),
                Key.ToBytes(),
                IV.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Thumb = GetObject<TLString>(input);
            ThumbW = GetObject<TLInt>(input);
            ThumbH = GetObject<TLInt>(input);
            Duration = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            Size = GetObject<TLInt>(input);
            Key = GetObject<TLString>(input);
            IV = GetObject<TLString>(input);

            File = GetNullableObject<TLEncryptedFileBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Thumb.ToBytes());
            output.Write(ThumbW.ToBytes());
            output.Write(ThumbH.ToBytes());
            output.Write(Duration.ToBytes());
            output.Write(W.ToBytes());
            output.Write(H.ToBytes());
            output.Write(Size.ToBytes());
            output.Write(Key.ToBytes());
            output.Write(IV.ToBytes());

            File.NullableToStream(output);
        }
    }

    public class TLDecryptedMessageMediaVenue : TLDecryptedMessageMediaGeoPoint
    {
        public new const uint Signature = TLConstructors.TLDecryptedMessageMediaVenue;

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

            Lat = GetObject<TLDouble>(bytes, ref position);
            Long = GetObject<TLDouble>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Address = GetObject<TLString>(bytes, ref position);
            Provider = GetObject<TLString>(bytes, ref position);
            VenueId = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Lat.ToBytes(),
                Long.ToBytes(),
                Title.ToBytes(),
                Address.ToBytes(),
                Provider.ToBytes(),
                VenueId.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Lat = GetObject<TLDouble>(input);
            Long = GetObject<TLDouble>(input);
            Title = GetObject<TLString>(input);
            Address = GetObject<TLString>(input);
            Provider = GetObject<TLString>(input);
            VenueId = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Lat.ToStream(output);
            Long.ToStream(output);
            Title.ToStream(output);
            Address.ToStream(output);
            Provider.ToStream(output);
            VenueId.ToStream(output);
        }
    }

    public class TLDecryptedMessageMediaGeoPoint : TLDecryptedMessageMediaBase, IMessageMediaGeoPoint
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageMediaGeoPoint;

        public TLDouble Lat { get; set; }

        public TLDouble Long { get; set; }

        #region Additional

        public TLGeoPointBase Geo
        {
            get
            {
                return Lat != null && Long != null
                    ? (TLGeoPointBase) new TLGeoPoint{ Lat = Lat, Long = Long }
                    : new TLGeoPointEmpty();
            }
        }

        #endregion

        public override double MediaWidth
        {
            get { return 2.0 + 320.0 + 2.0; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Lat = GetObject<TLDouble>(bytes, ref position);
            Long = GetObject<TLDouble>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Lat.ToBytes(),
                Long.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Lat = GetObject<TLDouble>(input);
            Long = GetObject<TLDouble>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Lat.ToBytes());
            output.Write(Long.ToBytes());
        }
    }

    public class TLDecryptedMessageMediaContact : TLDecryptedMessageMediaBase
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageMediaContact;

        public TLString PhoneNumber { get; set; }

        public TLString FirstName { get; set; }

        public TLString LastName { get; set; }

        public TLInt UserId { get; set; }

        public override string ToString()
        {
            return FullName;
        }

        #region Additional

        public virtual string FullName { get { return string.Format("{0} {1}", FirstName, LastName); } }

        public TLUserBase User
        {
            get
            {
                var cacheService = InMemoryCacheService.Instance;
                var user = cacheService.GetUser(UserId);

                if (user == null)
                {
                    return new TLUser{Phone = PhoneNumber, FirstName = FirstName, LastName = LastName, Id = new TLInt(0)};
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

            PhoneNumber = GetObject<TLString>(bytes, ref position);
            FirstName = GetObject<TLString>(bytes, ref position);
            LastName = GetObject<TLString>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhoneNumber.ToBytes(),
                FirstName.ToBytes(),
                LastName.ToBytes(),
                UserId.ToBytes());
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
            output.Write(PhoneNumber.ToBytes());
            output.Write(FirstName.ToBytes());
            output.Write(LastName.ToBytes());
            output.Write(UserId.ToBytes());
        }
    }

    public class TLDecryptedMessageMediaAudio : TLDecryptedMessageMediaBase
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageMediaAudio;

        public TLInt Duration { get; set; }

        public TLInt Size { get; set; }

        public TLString Key { get; set; }

        public TLString IV { get; set; }

        public override double MediaWidth
        {
            get { return 12.0 + 284.0 + 12.0; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Duration = GetObject<TLInt>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Key = GetObject<TLString>(bytes, ref position);
            IV = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Duration.ToBytes(),
                Size.ToBytes(),
                Key.ToBytes(),
                IV.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Duration = GetObject<TLInt>(input);
            Size = GetObject<TLInt>(input);
            Key = GetObject<TLString>(input);
            IV = GetObject<TLString>(input);

            UserId = GetNullableObject<TLInt>(input);
            File = GetNullableObject<TLEncryptedFileBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Duration.ToBytes());
            output.Write(Size.ToBytes());
            output.Write(Key.ToBytes());
            output.Write(IV.ToBytes());

            UserId.NullableToStream(output);
            File.NullableToStream(output);
        }

        #region Additional

        private TLInt _userId;

        public TLInt UserId
        {
            get { return _userId; }
            set { _userId = value; }
        }

        public TLDecryptedMessageMediaAudio Audio { get { return this; } }

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

        public TLUserBase User
        {
            get
            {
                if (UserId == null) return null;

                var cacheService = InMemoryCacheService.Instance;
                return cacheService.GetUser(UserId);
            }
        }
        #endregion
    }

    public class TLDecryptedMessageMediaAudio17 : TLDecryptedMessageMediaAudio
    {
        public new const uint Signature = TLConstructors.TLDecryptedMessageMediaAudio17;

        public TLString MimeType { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Duration = GetObject<TLInt>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Key = GetObject<TLString>(bytes, ref position);
            IV = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Duration.ToBytes(),
                MimeType.ToBytes(),
                Size.ToBytes(),
                Key.ToBytes(),
                IV.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Duration = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            Size = GetObject<TLInt>(input);
            Key = GetObject<TLString>(input);
            IV = GetObject<TLString>(input);

            UserId = GetNullableObject<TLInt>(input);
            File = GetNullableObject<TLEncryptedFileBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Duration.ToBytes());
            output.Write(MimeType.ToBytes());
            output.Write(Size.ToBytes());
            output.Write(Key.ToBytes());
            output.Write(IV.ToBytes());

            UserId.NullableToStream(output);
            File.NullableToStream(output);
        }
    }

    public class TLDecryptedMessageMediaDocument45 : TLDecryptedMessageMediaDocument, IMediaCaption, IAttributes
    {
        public new const uint Signature = TLConstructors.TLDecryptedMessageMediaDocument45;

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

                Attributes.Add(new TLDocumentAttributeFileName { FileName = value });
            }
        }


        public bool Music
        {
            get
            {
                var documentAttributeAudio = Attributes.FirstOrDefault(x => x is TLDocumentAttributeAudio32) as TLDocumentAttributeAudio32;
                if (documentAttributeAudio != null)
                {
                    return true;
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

        public TLVector<TLDocumentAttributeBase> Attributes { get; set; }

        public TLString Caption { get; set; }

        public TLInt Duration
        {
            get
            {
                if (Attributes != null)
                {
                    for (var i = 0; i < Attributes.Count; i++)
                    {
                        var audioAttribute = Attributes[i] as TLDocumentAttributeAudio;
                        if (audioAttribute != null)
                        {
                            return audioAttribute.Duration;
                        }
                    }
                }

                return new TLInt(0);
            }
        }

        public TLString Waveform
        {
            get
            {
                if (Attributes != null)
                {
                    for (var i = 0; i < Attributes.Count; i++)
                    {
                        var audioAttribute = Attributes[i] as TLDocumentAttributeAudio46;
                        if (audioAttribute != null)
                        {
                            return audioAttribute.Waveform;
                        }
                    }
                }

                return null;
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

        public override double MediaWidth
        {
            get
            {
                var document = Document as TLDecryptedMessageMediaDocument45;
                if (document != null)
                {
                    if (TLMessageBase.IsSticker(document, document.Size))
                    {
                        var maxStickerDimension = 196.0;

                        return 6.0 + TLMessageMediaDocument.GetStickerDimension(document, true, maxStickerDimension) + 6.0;
                    }

                    if (TLMessageBase.IsVoice(document, document.Size))
                    {
                        return 12.0 + 284.0 + 12.0;
                    }

                    //if (TLMessageBase.IsRoundVideo(document))
                    //{
                    //    return 2.0 + 240.0 + 2.0;
                    //}

                    if (TLMessageBase.IsVideo(document, document.Size))
                    {
                        return 2.0 + 230.0 + 2.0;
                    }

                    if (TLMessageBase.IsGif(document, document.Size))
                    {
                        var maxGifDimension = 323.0;

                        return 2.0 + TLMessageMediaDocument.GetGifDimension(new TLPhotoCachedSize { W = ThumbW, H = ThumbH }, document, true, maxGifDimension) + 2.0;
                    }
                }

                return base.MediaWidth;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Thumb = GetObject<TLString>(bytes, ref position);
            ThumbW = GetObject<TLInt>(bytes, ref position);
            ThumbH = GetObject<TLInt>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Key = GetObject<TLString>(bytes, ref position);
            IV = GetObject<TLString>(bytes, ref position);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(bytes, ref position);
            Caption = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Thumb.ToBytes(),
                ThumbW.ToBytes(),
                ThumbH.ToBytes(),
                MimeType.ToBytes(),
                Size.ToBytes(),
                Key.ToBytes(),
                IV.ToBytes(),
                Attributes.ToBytes(),
                Caption.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Thumb = GetObject<TLString>(input);
            ThumbW = GetObject<TLInt>(input);
            ThumbH = GetObject<TLInt>(input);
            MimeType = GetObject<TLString>(input);
            Size = GetObject<TLInt>(input);
            Key = GetObject<TLString>(input);
            IV = GetObject<TLString>(input);
            Attributes = GetObject<TLVector<TLDocumentAttributeBase>>(input);
            Caption = GetObject<TLString>(input);

            File = GetNullableObject<TLEncryptedFileBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Thumb.ToBytes());
            output.Write(ThumbW.ToBytes());
            output.Write(ThumbH.ToBytes());
            output.Write(MimeType.ToBytes());
            output.Write(Size.ToBytes());
            output.Write(Key.ToBytes());
            output.Write(IV.ToBytes());
            output.Write(Attributes.ToBytes());
            output.Write(Caption.ToBytes());

            File.NullableToStream(output);
        }
    }

    public class TLDecryptedMessageMediaDocument : TLDecryptedMessageMediaBase, IDecryptedMediaGif
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageMediaDocument;

        public TLString Thumb { get; set; }

        public TLInt ThumbW { get; set; }

        public TLInt ThumbH { get; set; }

        public virtual TLString FileName { get; set; }

        public TLString MimeType { get; set; }

        public TLInt Size { get; set; }

        public TLString Key { get; set; }

        public TLString IV { get; set; }

        #region Additional

        public TLDecryptedMessageMediaDocument Document { get { return this; } }

        public virtual string DocumentName { get { return FileName != null ? FileName.ToString() : string.Empty; } }

        public string FileExt
        {
            get { return FileName != null ? Path.GetExtension(FileName.ToString()).Replace(".", string.Empty) : null; }
        }

        public int DocumentSize
        {
            get { return Size != null ? Size.Value : 0; }
        }

        public string GetFileName()
        {
            var fileLocation = File as TLEncryptedFile;
            if (fileLocation == null) return null;;

            var fileName = String.Format("{0}_{1}_{2}.{3}",
                fileLocation.Id,
                fileLocation.DCId,
                fileLocation.AccessHash,
                FileExt);

            return fileName;
        }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Thumb = GetObject<TLString>(bytes, ref position);
            ThumbW = GetObject<TLInt>(bytes, ref position);
            ThumbH = GetObject<TLInt>(bytes, ref position);
            FileName = GetObject<TLString>(bytes, ref position);
            MimeType = GetObject<TLString>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            Key = GetObject<TLString>(bytes, ref position);
            IV = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Thumb.ToBytes(),
                ThumbW.ToBytes(),
                ThumbH.ToBytes(),
                FileName.ToBytes(),
                MimeType.ToBytes(),
                Size.ToBytes(),
                Key.ToBytes(),
                IV.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Thumb = GetObject<TLString>(input);
            ThumbW = GetObject<TLInt>(input);
            ThumbH = GetObject<TLInt>(input);
            FileName = GetObject<TLString>(input);
            MimeType = GetObject<TLString>(input);
            Size = GetObject<TLInt>(input);
            Key = GetObject<TLString>(input);
            IV = GetObject<TLString>(input);

            File = GetNullableObject<TLEncryptedFileBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Thumb.ToBytes());
            output.Write(ThumbW.ToBytes());
            output.Write(ThumbH.ToBytes());
            output.Write(FileName.ToBytes());
            output.Write(MimeType.ToBytes());
            output.Write(Size.ToBytes());
            output.Write(Key.ToBytes());
            output.Write(IV.ToBytes());
            File.NullableToStream(output);
        }
    }

    public class TLDecryptedMessageMediaExternalDocument : TLDecryptedMessageMediaBase, IAttributes
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageMediaExternalDocument;

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public TLInt Date { get; set; }

        public TLString MimeType { get; set; }

        public TLInt Size { get; set; }

        public TLPhotoSizeBase Thumb { get; set; }

        public TLInt DCId { get; set; }

        public TLVector<TLDocumentAttributeBase> Attributes { get; set; }

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

        public string FileExt
        {
            get { return Path.GetExtension(FileName.ToString()).Replace(".", string.Empty); }
        }

        public TLString FileName
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

                Attributes.Add(new TLDocumentAttributeFileName { FileName = value });
            }
        }

        public string GetFileName()
        {
            return string.Format("document{0}_{1}.{2}", Id, AccessHash, FileExt);
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
        public string Emoticon { get; set; }

        public TLDecryptedMessageMediaExternalDocument Document { get { return this; } }
        #endregion

        public override string ToString()
        {
            return string.Format("{0} Id={1}", GetType().Name, Id) + (StickerSet != null ? StickerSet.ToString() : string.Empty);
        }

        public TLInputDocumentFileLocation ToInputFileLocation()
        {
            return new TLInputDocumentFileLocation54
            {
                Id = Id,
                AccessHash = AccessHash,
                Version = new TLInt(0)
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
    
    public class TLDecryptedMessageMediaWebPage : TLDecryptedMessageMediaBase
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageMediaWebPage;

        public TLString Url { get; set; }

        public TLWebPageBase WebPage { get; set; }

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

        public override double MediaWidth
        {
            get { return 12.0 + 311.0 + 12.0; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Url.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);

            WebPage = GetNullableObject<TLWebPageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Url.ToBytes());

            WebPage.NullableToStream(output);
        }
    }

    public class TLDecryptedMessageMediaGroup : TLDecryptedMessageMediaBase, IMessageMediaGroup
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageMediaGroup;

        public TLVector<TLDecryptedMessageBase> Group { get; set; }

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

            Group = GetObject<TLVector<TLDecryptedMessageBase>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Group = GetObject<TLVector<TLDecryptedMessageBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Group.ToStream(output);
        }
    }
}

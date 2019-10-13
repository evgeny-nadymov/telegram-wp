// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL
{
    public abstract class TLInputFileLocationBase : TLObject
    {
        public abstract bool LocationEquals(TLInputFileLocationBase location);

        public abstract string GetPartFileName(int partNumbert, string prefix = "file");

        public abstract string GetFileName(string prefix = "file", string ext = ".dat");

        public abstract string GetLocationString();
    }

    public class TLInputFileLocation : TLInputFileLocationBase
    {
        public const uint Signature = TLConstructors.TLInputFileLocation;

        public TLLong VolumeId { get; set; }

        public TLInt LocalId { get; set; }

        public TLLong Secret { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                VolumeId.ToBytes(),
                LocalId.ToBytes(),
                Secret.ToBytes());
        }

        public override bool LocationEquals(TLInputFileLocationBase location)
        {
            if (location == null) return false;

            var fileLocation = location as TLInputFileLocation;
            if (fileLocation == null) return false;

            return
                VolumeId.Value == fileLocation.VolumeId.Value
                && LocalId.Value == fileLocation.LocalId.Value
                && Secret.Value == fileLocation.Secret.Value;
        }

        public override string GetPartFileName(int partNumbert, string prefix = "file")
        {
            return string.Format(prefix + "{0}_{1}_{2}_{3}.dat", VolumeId.Value, LocalId.Value, Secret.Value, partNumbert);
        }

        public override string GetFileName(string prefix = "file", string ext = ".dat")
        {
            return string.Format(prefix + "{0}_{1}_{2}{3}", VolumeId.Value, LocalId.Value, Secret.Value, ext);
        }

        public override string GetLocationString()
        {
            return string.Format("volume_id={0} local_id={1}", VolumeId, LocalId);
        }
    }

    [Obsolete]
    public class TLInputVideoFileLocation : TLInputFileLocationBase
    {
        public const uint Signature = TLConstructors.TLInputVideoFileLocation;

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes());
        }

        public override bool LocationEquals(TLInputFileLocationBase location)
        {
            if (location == null) return false;

            var fileLocation = location as TLInputVideoFileLocation;
            if (fileLocation == null) return false;

            return
                Id.Value == fileLocation.Id.Value
                && AccessHash.Value == fileLocation.AccessHash.Value;
        }

        public override string GetPartFileName(int partNumbert, string prefix = "video")
        {
            return string.Format(prefix + "{0}_{1}_{2}.dat", Id.Value, AccessHash.Value, partNumbert);
        }

        public override string GetFileName(string prefix = "video", string ext = ".dat")
        {
            return string.Format(prefix + "{0}_{1}{2}", Id.Value, AccessHash.Value, ext);
        }

        public override string GetLocationString()
        {
            return string.Format("id={0}", Id);
        }
    }

    [Obsolete]
    public class TLInputAudioFileLocation : TLInputFileLocationBase
    {
        public const uint Signature = TLConstructors.TLInputAudioFileLocation;

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes());
        }

        public override bool LocationEquals(TLInputFileLocationBase location)
        {
            if (location == null) return false;

            var fileLocation = location as TLInputAudioFileLocation;
            if (fileLocation == null) return false;

            return
                Id.Value == fileLocation.Id.Value
                && AccessHash.Value == fileLocation.AccessHash.Value;
        }

        public override string GetPartFileName(int partNumbert, string prefix = "audio")
        {
            return string.Format(prefix + "{0}_{1}_{2}.dat", Id.Value, AccessHash.Value, partNumbert);
        }

        public override string GetFileName(string prefix = "audio", string ext = ".dat")
        {
            return string.Format(prefix + "{0}_{1}{2}", Id.Value, AccessHash.Value, ext);
        }

        public override string GetLocationString()
        {
            return string.Format("id={0}", Id);
        }
    }

    public class TLInputDocumentFileLocation : TLInputFileLocationBase
    {
        public const uint Signature = TLConstructors.TLInputDocumentFileLocation;

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes());
        }

        public override bool LocationEquals(TLInputFileLocationBase location)
        {
            if (location == null) return false;

            var fileLocation = location as TLInputDocumentFileLocation;
            if (fileLocation == null) return false;

            return
                Id.Value == fileLocation.Id.Value
                && AccessHash.Value == fileLocation.AccessHash.Value;
        }

        public override string GetPartFileName(int partNumbert, string prefix = "document")
        {
            return string.Format(prefix + "{0}_{1}_{2}.dat", Id.Value, AccessHash.Value, partNumbert);
        }

        public override string GetFileName(string prefix = "document", string ext = ".dat")
        {
            return string.Format(prefix + "{0}_{1}{2}", Id.Value, AccessHash.Value, ext);
        }

        public override string GetLocationString()
        {
            return string.Format("id={0}", Id);
        }
    }

    public class TLInputDocumentFileLocation54 : TLInputDocumentFileLocation
    {
        public new const uint Signature = TLConstructors.TLInputDocumentFileLocation54;

        public TLInt Version { get; set; }
        
        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Version.ToBytes());
        }

        public override bool LocationEquals(TLInputFileLocationBase location)
        {
            if (location == null) return false;

            var fileLocation = location as TLInputDocumentFileLocation;
            if (fileLocation == null) return false;

            var fileLocation54 = location as TLInputDocumentFileLocation54;
            if (fileLocation54 != null)
            {
                return
                    Id.Value == fileLocation54.Id.Value
                    && AccessHash.Value == fileLocation54.AccessHash.Value
                    && Version.Value == fileLocation54.Version.Value;
            }

            return
                Id.Value == fileLocation.Id.Value
                && AccessHash.Value == fileLocation.AccessHash.Value;
        }

        public override string GetPartFileName(int partNumbert, string prefix = "document")
        {
            if (Version.Value > 0)
            {
                return string.Format(prefix + "{0}_{1}_{2}.dat", Id.Value, Version.Value, partNumbert);
            }

            return string.Format(prefix + "{0}_{1}_{2}.dat", Id.Value, AccessHash.Value, partNumbert);
        }

        public override string GetFileName(string prefix = "document", string ext = ".dat")
        {
            if (Version.Value > 0)
            {
                return string.Format(prefix + "{0}_{1}{2}", Id.Value, Version.Value, ext);
            }

            return string.Format(prefix + "{0}_{1}{2}", Id.Value, AccessHash.Value, ext);
        }

        public override string GetLocationString()
        {
            return string.Format("id={0} version={1}", Id, Version);
        }
    }

    public class TLInputEncryptedFileLocation : TLInputFileLocationBase
    {
        public const uint Signature = TLConstructors.TLInputEncryptedFileLocation;

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes());
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override bool LocationEquals(TLInputFileLocationBase location)
        {
            if (location == null) return false;

            var fileLocation = location as TLInputEncryptedFileLocation;
            if (fileLocation == null) return false;

            return
                Id.Value == fileLocation.Id.Value
                && AccessHash.Value == fileLocation.AccessHash.Value;
        }

        public override string GetPartFileName(int partNumbert, string prefix = "encrypted")
        {
            return string.Format(prefix + "{0}_{1}_{2}.dat", Id.Value, AccessHash.Value, partNumbert);
        }

        public override string GetFileName(string prefix = "encrypted", string ext = ".dat")
        {
            return string.Format(prefix + "{0}_{1}{2}", Id.Value, AccessHash.Value, ext);
        }

        public override string GetLocationString()
        {
            return string.Format("id={0}", Id);
        }
    }

    public abstract class TLInputWebFileLocationBase : TLInputFileLocationBase
    {
        public TLLong AccessHash { get; set; }
    }

    public class TLInputWebFileGeoPointLocation : TLInputWebFileLocationBase
    {
        public const uint Signature = TLConstructors.TLInputWebFileGeoPointLocation;

        public TLInputGeoPointBase GeoPoint { get; set; }

        public TLInt W { get; set; }

        public TLInt H { get; set; }

        public TLInt Zoom { get; set; }

        public TLInt Scale { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                GeoPoint.ToBytes(),
                AccessHash.ToBytes(),
                W.ToBytes(),
                H.ToBytes(),
                Zoom.ToBytes(),
                Scale.ToBytes());
        }

        public override string GetPartFileName(int partNumbert, string prefix = "map")
        {
            return string.Format(prefix + "{0}_{1}_{2}_{3}_{4}_{5}_{6}.dat", GeoPoint, AccessHash.Value, W, H, Zoom, Scale, partNumbert);
        }

        public override string GetFileName(string prefix = "map", string ext = ".dat")
        {
            return string.Format(prefix + "{0}_{1}_{2}_{3}_{4}_{5}{6}", GeoPoint, AccessHash.Value, W, H, Zoom, Scale, ext);
        }

        public override string GetLocationString()
        {
            return string.Format("point={0} w={1} h={2} zoom={3} scale={4}", GeoPoint, W, H, Zoom, Scale);
        }

        public override bool LocationEquals(TLInputFileLocationBase locationBase)
        {
            if (locationBase == null) return false;

            var location = locationBase as TLInputWebFileGeoPointLocation;
            if (location == null) return false;

            return
                GeoPoint.GeoPointEquals(location.GeoPoint)
                && AccessHash.Value == location.AccessHash.Value
                && W.Value == location.W.Value
                && H.Value == location.H.Value
                && Zoom.Value == location.Zoom.Value
                && Scale.Value == location.Scale.Value;
        }
    }

    public class TLInputWebFileLocation : TLInputWebFileLocationBase
    {
        public const uint Signature = TLConstructors.TLInputWebFileLocation;

        public TLString Url { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Url.ToBytes(),
                AccessHash.ToBytes());
        }

        public override string GetPartFileName(int partNumbert, string prefix = "url")
        {
            return string.Format(prefix + "{0}_{1}_{2}.dat", Url.ToString().GetHashCode(), AccessHash.Value, partNumbert);
        }

        public override string GetFileName(string prefix = "url", string ext = ".dat")
        {
            return string.Format(prefix + "{0}_{1}{2}", Url.ToString().GetHashCode(), AccessHash.Value, ext);
        }

        public override string GetLocationString()
        {
            return string.Format("id={0}", Url.ToString().GetHashCode());
        }

        public override bool LocationEquals(TLInputFileLocationBase location)
        {
            if (location == null) return false;

            var fileLocation = location as TLInputWebFileLocation;
            if (fileLocation == null) return false;

            return
                Url.Value == fileLocation.Url.Value
                && AccessHash.Value == fileLocation.AccessHash.Value;
        }
    }

    public class TLInputSecureFileLocation : TLInputFileLocationBase
    {
        public const uint Signature = TLConstructors.TLInputSecureFileLocation;

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes());
        }

        public override bool LocationEquals(TLInputFileLocationBase location)
        {
            if (location == null) return false;

            var fileLocation = location as TLInputSecureFileLocation;
            if (fileLocation == null) return false;

            return
                Id.Value == fileLocation.Id.Value
                && AccessHash.Value == fileLocation.AccessHash.Value;
        }

        public override string GetPartFileName(int partNumbert, string prefix = "document")
        {
            return string.Format(prefix + "{0}_{1}_{2}.dat", Id.Value, AccessHash.Value, partNumbert);
        }

        public override string GetFileName(string prefix = "document", string ext = ".dat")
        {
            return string.Format(prefix + "{0}_{1}{2}", Id.Value, AccessHash.Value, ext);
        }

        public override string GetLocationString()
        {
            return string.Format("id={0}", Id);
        }
    }

    public class TLInputTakeoutFileLocation : TLInputFileLocationBase
    {
        public const uint Signature = TLConstructors.TLInputTakeoutFileLocation;

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override bool LocationEquals(TLInputFileLocationBase location)
        {
            var fileLocation = location as TLInputSecureFileLocation;
            if (fileLocation == null) return false;

            return true;
        }

        public override string GetPartFileName(int partNumbert, string prefix = "document")
        {
            return string.Format(prefix + "{0}.dat", partNumbert);
        }

        public override string GetFileName(string prefix = "document", string ext = ".dat")
        {
            return string.Format(prefix + "{2}", ext);
        }

        public override string GetLocationString()
        {
            return "";
        }
    }
}

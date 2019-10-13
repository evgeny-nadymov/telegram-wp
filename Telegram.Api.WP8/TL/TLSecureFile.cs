// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    public interface ISecureFileError
    {
        TLString FileHash { get; set; }

        string Error { get; set; }
    }

    public abstract class TLSecureFileBase : TLObject
    {
        private double _uploadingProgress;

        public double UploadingProgress
        {
            get { return _uploadingProgress; }
            set { SetField(ref _uploadingProgress, value, () => UploadingProgress); }
        }

        private int _uploadingSize;

        public int UploadingSize
        {
            get { return _uploadingSize; }
            set { SetField(ref _uploadingSize, value, () => UploadingSize); }
        }

        private double _downloadingProgress;

        public double DownloadingProgress
        {
            get { return _downloadingProgress; }
            set { SetField(ref _downloadingProgress, value, () => DownloadingProgress); }
        }

        public TLSecureFileBase Self { get { return this; } }

        public abstract TLInputSecureFileBase ToInputSecureFile();
    }

    public class TLSecureFileEmpty : TLSecureFileBase
    {
        public const uint Signature = TLConstructors.TLSecureFileEmpty;

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

        public override TLInputSecureFileBase ToInputSecureFile()
        {
            return null;
        }
    }

    public class TLSecureFile : TLSecureFileBase, ISecureFileError
    {
        public const uint Signature = TLConstructors.TLSecureFile;

        public TLLong Id { get; set; }

        public TLLong AccessHash { get; set; }

        public TLInt Size { get; set; }

        public TLInt DCId { get; set; }

        public TLInt Date { get; set; }

        public TLString FileHash { get; set; }

        public TLString Secret { get; set; }

        public string Error { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Size = GetObject<TLInt>(bytes, ref position);
            DCId = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            FileHash = GetObject<TLString>(bytes, ref position);
            Secret = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Size = GetObject<TLInt>(input);
            DCId = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);
            FileHash = GetObject<TLString>(input);
            Secret = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            AccessHash.ToStream(output);
            Size.ToStream(output);
            DCId.ToStream(output);
            Date.ToStream(output);
            FileHash.ToStream(output);
            Secret.ToStream(output);
        }

        public override TLInputSecureFileBase ToInputSecureFile()
        {
            return new TLInputSecureFile { Id = Id, AccessHash = AccessHash };
        }
    }

    public class TLSecureFileUploaded : TLSecureFileBase, ISecureFileError
    {
        public const uint Signature = TLConstructors.TLSecureFileUploaded;

        public TLLong Id { get; set; }

        public TLInt Parts { get; set; }

        public TLString MD5Checksum { get; set; }

        public TLInt Size { get; set; }

        public TLInt Date { get; set; }

        public TLString FileHash { get; set; }

        public TLString Secret { get; set; }

        public string Error { get; set; }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            Parts = GetObject<TLInt>(input);
            MD5Checksum = GetObject<TLString>(input);
            Size = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(input);
            FileHash = GetObject<TLString>(input);
            Secret = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            Parts.ToStream(output);
            MD5Checksum.ToStream(output);
            Size.ToStream(output);
            Date.ToStream(output);
            FileHash.ToStream(output);
            Secret.ToStream(output);
        }

        public override TLInputSecureFileBase ToInputSecureFile()
        {
            return new TLInputSecureFileUploaded { Id = Id, Parts = Parts, MD5Checksum = MD5Checksum, FileHash = FileHash, Secret = Secret };
        }
    }
}

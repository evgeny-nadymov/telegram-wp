// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLFileBase : TLObject { }

    public class TLFile : TLFileBase
    {
        public const uint Signature = TLConstructors.TLFile;

        public TLFileTypeBase Type { get; set; }

        public TLInt MTime { get; set; }

        public TLString Bytes { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Type = GetObject<TLFileTypeBase>(bytes, ref position);
            MTime = GetObject<TLInt>(bytes, ref position);
            Bytes = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLFileCdnRedirect : TLFileBase
    {
        public const uint Signature = TLConstructors.TLFileCdnRedirect;

        public TLInt DCId { get; set; }

        public TLString FileToken { get; set; }

        public TLString EncryptionKey { get; set; }

        public TLString EncryptionIV { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            DCId = GetObject<TLInt>(bytes, ref position);
            FileToken = GetObject<TLString>(bytes, ref position);
            EncryptionKey = GetObject<TLString>(bytes, ref position);
            EncryptionIV = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLFileCdnRedirect76 : TLFileCdnRedirect
    {
        public new const uint Signature = TLConstructors.TLFileCdnRedirect76;

        public TLVector<TLFileHash> FileHashes { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            DCId = GetObject<TLInt>(bytes, ref position);
            FileToken = GetObject<TLString>(bytes, ref position);
            EncryptionKey = GetObject<TLString>(bytes, ref position);
            EncryptionIV = GetObject<TLString>(bytes, ref position);
            FileHashes = GetObject<TLVector<TLFileHash>>(bytes, ref position);

            return this;
        }
    }

    public class TLFileCdnRedirect70 : TLFileCdnRedirect
    {
        public new const uint Signature = TLConstructors.TLFileCdnRedirect70;

        public TLVector<TLCdnFileHash> CdnFileHashes { get; set; } 

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            DCId = GetObject<TLInt>(bytes, ref position);
            FileToken = GetObject<TLString>(bytes, ref position);
            EncryptionKey = GetObject<TLString>(bytes, ref position);
            EncryptionIV = GetObject<TLString>(bytes, ref position);
            CdnFileHashes = GetObject<TLVector<TLCdnFileHash>>(bytes, ref position);

            return this;
        }
    }

    public class TLCdnFileHash : TLObject
    {
        public const uint Signature = TLConstructors.TLCdnFileHash;

        public TLInt Offset { get; set; }

        public TLInt Limit { get; set; }

        public TLString Bytes { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Offset = GetObject<TLInt>(bytes, ref position);
            Limit = GetObject<TLInt>(bytes, ref position);
            Bytes = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLFileHash : TLObject
    {
        public const uint Signature = TLConstructors.TLFileHash;

        public TLInt Offset { get; set; }

        public TLInt Limit { get; set; }

        public TLString Bytes { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Offset = GetObject<TLInt>(bytes, ref position);
            Limit = GetObject<TLInt>(bytes, ref position);
            Bytes = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }
}

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
    public abstract class TLPageBase : TLObject
    {
        public TLVector<TLPageBlockBase> Blocks { get; set; }

        public TLVector<TLPhotoBase> Photos { get; set; }

        public TLVector<TLDocumentBase> Documents { get; set; } 
    }

    public class TLPagePart : TLPageBase
    {
        public const uint Signature = TLConstructors.TLPagePart;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Blocks = GetObject<TLVector<TLPageBlockBase>>(bytes, ref position);
            Photos = GetObject<TLVector<TLPhotoBase>>(bytes, ref position);
            Documents = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Blocks.ToBytes(),
                Photos.ToBytes(),
                Documents.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Blocks.ToStream(output);
            Photos.ToStream(output);
            Documents.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Blocks = GetObject<TLVector<TLPageBlockBase>>(input);
            Photos = GetObject<TLVector<TLPhotoBase>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);

            return this;
        }
    }

    public class TLPagePart68 : TLPageBase
    {
        public const uint Signature = TLConstructors.TLPagePart68;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Blocks = GetObject<TLVector<TLPageBlockBase>>(bytes, ref position);
            Photos = GetObject<TLVector<TLPhotoBase>>(bytes, ref position);
            Documents = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Blocks.ToBytes(),
                Photos.ToBytes(),
                Documents.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Blocks.ToStream(output);
            Photos.ToStream(output);
            Documents.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Blocks = GetObject<TLVector<TLPageBlockBase>>(input);
            Photos = GetObject<TLVector<TLPhotoBase>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);

            return this;
        }
    }

    public class TLPageFull : TLPageBase
    {
        public const uint Signature = TLConstructors.TLPageFull;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Blocks = GetObject<TLVector<TLPageBlockBase>>(bytes, ref position);
            Photos = GetObject<TLVector<TLPhotoBase>>(bytes, ref position);
            Documents = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Blocks.ToBytes(),
                Photos.ToBytes(),
                Documents.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Blocks.ToStream(output);
            Photos.ToStream(output);
            Documents.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Blocks = GetObject<TLVector<TLPageBlockBase>>(input);
            Photos = GetObject<TLVector<TLPhotoBase>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);

            return this;
        }
    }

    public class TLPageFull68 : TLPageBase
    {
        public const uint Signature = TLConstructors.TLPageFull68;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Blocks = GetObject<TLVector<TLPageBlockBase>>(bytes, ref position);
            Photos = GetObject<TLVector<TLPhotoBase>>(bytes, ref position);
            Documents = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Blocks.ToBytes(),
                Photos.ToBytes(),
                Documents.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Blocks.ToStream(output);
            Photos.ToStream(output);
            Documents.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Blocks = GetObject<TLVector<TLPageBlockBase>>(input);
            Photos = GetObject<TLVector<TLPhotoBase>>(input);
            Documents = GetObject<TLVector<TLDocumentBase>>(input);

            return this;
        }
    }
}

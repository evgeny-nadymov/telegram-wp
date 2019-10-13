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
    public abstract class TLInputStickeredMediaBase : TLObject { }

    public class TLInputStickeredMediaPhoto : TLInputStickeredMediaBase
    {
        public const uint Signature = TLConstructors.TLInputStickeredMediaPhoto;

        public TLInputPhotoBase Id { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes()
            );
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInputPhotoBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInputPhotoBase>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputStickeredMediaPhoto id=[{0}]", Id);
        }
    }

    public class TLInputStickeredMediaDocument : TLInputStickeredMediaBase
    {
        public const uint Signature = TLConstructors.TLInputStickeredMediaDocument;

        public TLInputDocumentBase Id { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes()
            );
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInputDocumentBase>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInputDocumentBase>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLInputStickeredMediaDocument id=[{0}]", Id);
        }
    }
}

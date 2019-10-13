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
    public abstract class TLSavedGifsBase : TLObject { }

    public class TLSavedGifsNotModified : TLSavedGifsBase
    {
        public const uint Signature = TLConstructors.TLSavedGifsNotModified;

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

    public class TLSavedGifs : TLSavedGifsBase
    {
        public const uint Signature = TLConstructors.TLSavedGifs;

        public TLInt Hash { get; set; }

        public TLVector<TLDocumentBase> Gifs { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Hash = GetObject<TLInt>(bytes, ref position);
            Gifs = GetObject<TLVector<TLDocumentBase>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Hash = GetObject<TLInt>(input);
            Gifs = GetObject<TLVector<TLDocumentBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Hash.ToStream(output);
            Gifs.ToStream(output);
        }
    }
}

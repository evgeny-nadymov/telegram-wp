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
    public abstract class TLWallPaperBase : TLObject
    {
        public TLInt Id { get; set; }

        public TLString Title { get; set; }

        public TLInt Color { get; set; }

        public TLLong CustomFlags { get; set; }
    }

    public class TLWallPaper : TLWallPaperBase
    {
        public const uint Signature = TLConstructors.TLWallPaper;

        public TLVector<TLPhotoSizeBase> Sizes { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Sizes = GetObject<TLVector<TLPhotoSizeBase>>(bytes, ref position);
            Color = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            Title = GetObject<TLString>(input);
            Sizes = GetObject<TLVector<TLPhotoSizeBase>>(input);
            Color = GetObject<TLInt>(input);
            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            Title.ToStream(output);
            Sizes.ToStream(output);
            Color.ToStream(output);

            CustomFlags.NullableToStream(output);
        }
    }

    public class TLWallPaperSolid : TLWallPaperBase
    {
        public const uint Signature = TLConstructors.TLWallPaperSolid;

        public TLInt BgColor { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            BgColor = GetObject<TLInt>(bytes, ref position);
            Color = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);
            Title = GetObject<TLString>(input);
            BgColor = GetObject<TLInt>(input);
            Color = GetObject<TLInt>(input);
            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            Title.ToStream(output);
            BgColor.ToStream(output);
            Color.ToStream(output);

            CustomFlags.NullableToStream(output);
        }
    }
}

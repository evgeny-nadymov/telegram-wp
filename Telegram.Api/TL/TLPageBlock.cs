// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum PageBlockVideoFlags
    {
        Autoplay = 0x1,     // 0
        Loop = 0x2,         // 1 
    }

    [Flags]
    public enum PageBlockEmbedFlags
    {
        FullWidth = 0x1,        // 0
        Url = 0x2,              // 1 
        Html = 0x4,             // 2
        AllowScrolling = 0x8,   // 3
        PosterPhotoId = 0x10,   // 4  
    }

    public abstract class TLPageBlockBase : TLObject { }

    public class TLPageBlockUnsupported : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockUnsupported;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }
    }

    public class TLPageBlockTitle : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockTitle;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockSubtitle : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockSubtitle;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public abstract class TLPageBlockPublishedDateBase : TLPageBlockBase
    {
        public TLInt PublishedDate { get; set; }
    }

    public class TLPageBlockAuthorDate : TLPageBlockPublishedDateBase
    {
        public const uint Signature = TLConstructors.TLPageBlockAuthorDate;

        public TLString Author { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Author = GetObject<TLString>(bytes, ref position);
            PublishedDate = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Author.ToBytes(),
                PublishedDate.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Author.ToStream(output);
            PublishedDate.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Author = GetObject<TLString>(input);
            PublishedDate = GetObject<TLInt>(input);

            return this;
        }
    }

    public class TLPageBlockAuthorDate61 : TLPageBlockPublishedDateBase
    {
        public const uint Signature = TLConstructors.TLPageBlockAuthorDate61;

        public TLRichTextBase Author { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Author = GetObject<TLRichTextBase>(bytes, ref position);
            PublishedDate = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Author.ToBytes(),
                PublishedDate.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Author.ToStream(output);
            PublishedDate.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Author = GetObject<TLRichTextBase>(input);
            PublishedDate = GetObject<TLInt>(input);

            return this;
        }
    }

    public class TLPageBlockHeader : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockHeader;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockSubheader : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockSubheader;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockParagraph : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockParagraph;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockPreformatted : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockPreformatted;

        public TLRichTextBase Text { get; set; }

        public TLString Language { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);
            Language = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes(),
                Language.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
            Language.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);
            Language = GetObject<TLString>(input);

            return this;
        }
    }

    public class TLPageBlockFooter : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockFooter;

        public TLRichTextBase Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockDivider : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockDivider;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }
    }

    public class TLPageBlockAnchor : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockAnchor;

        public TLString Name { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Name = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Name.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Name.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Name = GetObject<TLString>(input);

            return this;
        }
    }

    public class TLPageBlockList : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockList;

        public TLBool Ordered { get; set; }

        public TLVector<TLRichTextBase> Items { get; set; } 

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Ordered = GetObject<TLBool>(bytes, ref position);
            Items = GetObject<TLVector<TLRichTextBase>>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Ordered.ToBytes(),
                Items.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Ordered.ToStream(output);
            Items.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Ordered = GetObject<TLBool>(input);
            Items = GetObject<TLVector<TLRichTextBase>>(input);

            return this;
        }
    }

    public class TLPageBlockBlockquote : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockBlockquote;

        public TLRichTextBase Text { get; set; }

        public TLRichTextBase Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);
            Caption = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes(),
                Caption.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
            Caption.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);
            Caption = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockPullquote : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockPullquote;

        public TLRichTextBase Text { get; set; }

        public TLRichTextBase Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLRichTextBase>(bytes, ref position);
            Caption = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Text.ToBytes(),
                Caption.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Text.ToStream(output);
            Caption.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Text = GetObject<TLRichTextBase>(input);
            Caption = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockPhoto : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockPhoto;

        public TLLong PhotoId { get; set; }

        public TLRichTextBase Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            PhotoId = GetObject<TLLong>(bytes, ref position);
            Caption = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                PhotoId.ToBytes(),
                Caption.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            PhotoId.ToStream(output);
            Caption.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            PhotoId = GetObject<TLLong>(input);
            Caption = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockVideo : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockVideo;

        public TLInt Flags { get; set; }

        public bool Autoplay { get { return IsSet(Flags, (int) PageBlockVideoFlags.Autoplay); } }

        public bool Loop { get { return IsSet(Flags, (int) PageBlockVideoFlags.Loop); } }

        public TLLong VideoId { get; set; }

        public TLRichTextBase Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            VideoId = GetObject<TLLong>(bytes, ref position);
            Caption = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                VideoId.ToBytes(),
                Caption.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            VideoId.ToStream(output);
            Caption.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            VideoId = GetObject<TLLong>(input);
            Caption = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockCover : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockCover;

        public TLPageBlockBase Cover { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Cover = GetObject<TLPageBlockBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Cover.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Cover.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Cover = GetObject<TLPageBlockBase>(input);

            return this;
        }
    }

    public class TLPageBlockEmbed : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockEmbed;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool FullWidth { get { return IsSet(Flags, (int) PageBlockEmbedFlags.FullWidth); } }

        public bool AllowScrolling { get { return IsSet(Flags, (int) PageBlockEmbedFlags.AllowScrolling); } }

        protected TLString _url;

        public TLString Url
        {
            get { return _url; }
            set { SetField(out _url, value, ref _flags, (int) PageBlockEmbedFlags.Url); }
        }

        protected TLString _html;

        public TLString Html
        {
            get { return _html; }
            set { SetField(out _html, value, ref _flags, (int) PageBlockEmbedFlags.Html); }
        }

        public TLInt W { get; set; }

        public TLInt H { get; set; }

        public TLRichTextBase Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _url = GetObject<TLString>(Flags, (int) PageBlockEmbedFlags.Url, null, bytes, ref position);
            _html = GetObject<TLString>(Flags, (int) PageBlockEmbedFlags.Html, null, bytes, ref position);
            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);
            Caption = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(Url, Flags, (int) PageBlockEmbedFlags.Url),
                ToBytes(Html, Flags, (int) PageBlockEmbedFlags.Html),
                W.ToBytes(),
                H.ToBytes(),
                Caption.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            ToStream(output, Url, Flags, (int) PageBlockEmbedFlags.Url);
            ToStream(output, Html, Flags, (int) PageBlockEmbedFlags.Html);
            W.ToStream(output);
            H.ToStream(output);
            Caption.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            _url = GetObject<TLString>(Flags, (int) PageBlockEmbedFlags.Url, null, input);
            _html = GetObject<TLString>(Flags, (int) PageBlockEmbedFlags.Html, null, input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            Caption = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockEmbed61 : TLPageBlockEmbed
    {
        public new const uint Signature = TLConstructors.TLPageBlockEmbed61;

        protected TLLong _posterPhotoId;

        public TLLong PosterPhotoId
        {
            get { return _posterPhotoId; }
            set { SetField(out _posterPhotoId, value, ref _flags, (int) PageBlockEmbedFlags.PosterPhotoId); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _url = GetObject<TLString>(Flags, (int)PageBlockEmbedFlags.Url, null, bytes, ref position);
            _html = GetObject<TLString>(Flags, (int)PageBlockEmbedFlags.Html, null, bytes, ref position);
            _posterPhotoId = GetObject<TLLong>(Flags, (int)PageBlockEmbedFlags.PosterPhotoId, null, bytes, ref position);
            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);
            Caption = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(Url, Flags, (int)PageBlockEmbedFlags.Url),
                ToBytes(Html, Flags, (int)PageBlockEmbedFlags.Html),
                ToBytes(PosterPhotoId, Flags, (int)PageBlockEmbedFlags.PosterPhotoId),
                W.ToBytes(),
                H.ToBytes(),
                Caption.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            ToStream(output, Url, Flags, (int)PageBlockEmbedFlags.Url);
            ToStream(output, Html, Flags, (int)PageBlockEmbedFlags.Html);
            ToStream(output, PosterPhotoId, Flags, (int)PageBlockEmbedFlags.PosterPhotoId);
            W.ToStream(output);
            H.ToStream(output);
            Caption.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            _url = GetObject<TLString>(Flags, (int)PageBlockEmbedFlags.Url, null, input);
            _html = GetObject<TLString>(Flags, (int)PageBlockEmbedFlags.Html, null, input);
            _posterPhotoId = GetObject<TLLong>(Flags, (int)PageBlockEmbedFlags.PosterPhotoId, null, input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);
            Caption = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockEmbedPost : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockEmbedPost;

        public TLString Url { get; set; }

        public TLLong WebPageId { get; set; }

        public TLLong AuthorPhotoId { get; set; }

        public TLString Author { get; set; }

        public TLInt Date { get; set; }

        public TLVector<TLPageBlockBase> Blocks { get; set; } 

        public TLRichTextBase Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Url = GetObject<TLString>(bytes, ref position);
            WebPageId = GetObject<TLLong>(bytes, ref position);
            AuthorPhotoId = GetObject<TLLong>(bytes, ref position);
            Author = GetObject<TLString>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            Blocks = GetObject<TLVector<TLPageBlockBase>>(bytes, ref position);
            Caption = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Url.ToBytes(),
                WebPageId.ToBytes(),
                AuthorPhotoId.ToBytes(),
                Author.ToBytes(),
                Date.ToBytes(),
                Blocks.ToBytes(),
                Caption.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Url.ToStream(output);
            WebPageId.ToStream(output);
            AuthorPhotoId.ToStream(output);
            Author.ToStream(output);
            Date.ToStream(output);
            Blocks.ToStream(output);
            Caption.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Url = GetObject<TLString>(input);
            WebPageId = GetObject<TLLong>(input);
            AuthorPhotoId = GetObject<TLLong>(input);
            Author = GetObject<TLString>(input);
            Date = GetObject<TLInt>(input);
            Blocks = GetObject<TLVector<TLPageBlockBase>>(input);
            Caption = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockCollage : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockCollage;

        public TLVector<TLPageBlockBase> Items { get; set; }

        public TLRichTextBase Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Items = GetObject<TLVector<TLPageBlockBase>>(bytes, ref position);
            Caption = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Items.ToBytes(),
                Caption.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Items.ToStream(output);
            Caption.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Items = GetObject<TLVector<TLPageBlockBase>>(input);
            Caption = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockSlideshow : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockSlideshow;

        public TLVector<TLPageBlockBase> Items { get; set; }

        public TLRichTextBase Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Items = GetObject<TLVector<TLPageBlockBase>>(bytes, ref position);
            Caption = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Items.ToBytes(),
                Caption.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Items.ToStream(output);
            Caption.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Items = GetObject<TLVector<TLPageBlockBase>>(input);
            Caption = GetObject<TLRichTextBase>(input);

            return this;
        }
    }

    public class TLPageBlockChannel : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockChannel;

        public TLChatBase Channel { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Channel = GetObject<TLChatBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Channel.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Channel.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Channel = GetObject<TLChannel>(input);

            return this;
        }
    }

    public class TLPageBlockAudio : TLPageBlockBase
    {
        public const uint Signature = TLConstructors.TLPageBlockAudio;

        public TLLong AudioId { get; set; }

        public TLRichTextBase Caption { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            AudioId = GetObject<TLLong>(bytes, ref position);
            Caption = GetObject<TLRichTextBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                AudioId.ToBytes(),
                Caption.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            AudioId.ToStream(output);
            Caption.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            AudioId = GetObject<TLLong>(input);
            Caption = GetObject<TLRichTextBase>(input);

            return this;
        }
    }
}

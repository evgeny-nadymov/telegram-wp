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
    public enum InputBotInlineResultFlags
    {
        //Unread = 0x1,         // 0
        Title = 0x2,            // 1
        Description = 0x4,      // 2
        Url = 0x8,              // 3
        Thumb = 0x10,        // 4
        Content = 0x20,         // 5
        Size = 0x40,            // 6
        Duration = 0x80,        // 7
    }

    public abstract class TLInputBotInlineResultBase : TLObject { }

    public class TLInputBotInlineResult : TLInputBotInlineResultBase
    {
        public const uint Signature = TLConstructors.TLInputBotInlineResult;

        public TLInt Flags { get; set; }

        public TLString Id { get; set; }

        public TLString Type { get; set; }

        public TLString Title { get; set; }

        public TLString Description { get; set; }

        public TLString Url { get; set; }

        public virtual TLString ThumbUrl { get; protected set; }

        public virtual TLString ContentUrl { get; protected set; }

        public virtual TLString ContentType { get; protected set; }

        public virtual TLInt W { get; protected set; }

        public virtual TLInt H { get; protected set; }

        public virtual TLInt Duration { get; protected set; }

        public TLInputBotInlineMessageBase SendMessage { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                Type.ToBytes(),
                ToBytes(Title, Flags, (int)InputBotInlineResultFlags.Title),
                ToBytes(Description, Flags, (int)InputBotInlineResultFlags.Description),
                ToBytes(Url, Flags, (int)InputBotInlineResultFlags.Url),
                ToBytes(ThumbUrl, Flags, (int)InputBotInlineResultFlags.Thumb),
                ToBytes(ContentUrl, Flags, (int)InputBotInlineResultFlags.Content),
                ToBytes(ContentType, Flags, (int)InputBotInlineResultFlags.Content),
                ToBytes(W, Flags, (int)InputBotInlineResultFlags.Size),
                ToBytes(H, Flags, (int)InputBotInlineResultFlags.Size),
                ToBytes(Duration, Flags, (int)InputBotInlineResultFlags.Duration),
                SendMessage.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLString>(input);
            Type = GetObject<TLString>(input);
            if (IsSet(Flags, (int)InputBotInlineResultFlags.Title))
            {
                Title = GetObject<TLString>(input);
            }
            if (IsSet(Flags, (int)InputBotInlineResultFlags.Description))
            {
                Description = GetObject<TLString>(input);
            }
            if (IsSet(Flags, (int)InputBotInlineResultFlags.Url))
            {
                Url = GetObject<TLString>(input);
            }
            if (IsSet(Flags, (int)InputBotInlineResultFlags.Thumb))
            {
                ThumbUrl = GetObject<TLString>(input);
            }
            if (IsSet(Flags, (int)InputBotInlineResultFlags.Content))
            {
                ContentUrl = GetObject<TLString>(input);
                ContentType = GetObject<TLString>(input);
            }
            if (IsSet(Flags, (int)InputBotInlineResultFlags.Size))
            {
                W = GetObject<TLInt>(input);
                H = GetObject<TLInt>(input);
            }
            if (IsSet(Flags, (int)InputBotInlineResultFlags.Duration))
            {
                Duration = GetObject<TLInt>(input);
            }
            SendMessage = GetObject<TLInputBotInlineMessageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            Type.ToStream(output);
            ToStream(output, Title, Flags, (int) InputBotInlineResultFlags.Title);
            ToStream(output, Description, Flags, (int)InputBotInlineResultFlags.Description);
            ToStream(output, Url, Flags, (int)InputBotInlineResultFlags.Url);
            ToStream(output, ThumbUrl, Flags, (int)InputBotInlineResultFlags.Thumb);
            ToStream(output, ContentUrl, Flags, (int)InputBotInlineResultFlags.Content);
            ToStream(output, ContentType, Flags, (int)InputBotInlineResultFlags.Content);
            ToStream(output, W, Flags, (int)InputBotInlineResultFlags.Size);
            ToStream(output, H, Flags, (int)InputBotInlineResultFlags.Size);
            ToStream(output, Duration, Flags, (int)InputBotInlineResultFlags.Duration);
            SendMessage.ToStream(output);
        }
    }

    public class TLInputBotInlineResult76 : TLInputBotInlineResult
    {
        public new const uint Signature = TLConstructors.TLInputBotInlineResult76;

        public TLInputWebDocument Thumb { get; set; }

        public override TLString ThumbUrl { get { return Thumb != null ? Thumb.Url : null; } }

        public TLInputWebDocument Content { get; set; }

        public override TLString ContentUrl { get { return Content != null ? Content.Url : null; } }

        public override TLString ContentType { get { return Content != null ? Content.MimeType : null; } }

        public override TLInt W
        {
            get { return Content != null ? Content.W : null; }
        }

        public override TLInt H
        {
            get { return Content != null ? Content.H : null; }
        }

        public override TLInt Duration
        {
            get { return Content != null ? Content.Duration : null; }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                Type.ToBytes(),
                ToBytes(Title, Flags, (int)InputBotInlineResultFlags.Title),
                ToBytes(Description, Flags, (int)InputBotInlineResultFlags.Description),
                ToBytes(Url, Flags, (int)InputBotInlineResultFlags.Url),
                ToBytes(Thumb, Flags, (int)InputBotInlineResultFlags.Thumb),
                ToBytes(Content, Flags, (int)InputBotInlineResultFlags.Content),
                SendMessage.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLString>(input);
            Type = GetObject<TLString>(input);
            Title = GetObject<TLString>(Flags, (int)InputBotInlineResultFlags.Title, null, input);
            Description = GetObject<TLString>(Flags, (int)InputBotInlineResultFlags.Description, null, input);
            Url = GetObject<TLString>(Flags, (int)InputBotInlineResultFlags.Url, null, input);
            Thumb = GetObject<TLInputWebDocument>(Flags, (int)InputBotInlineResultFlags.Thumb, null, input);
            Content = GetObject<TLInputWebDocument>(Flags, (int)InputBotInlineResultFlags.Content, null, input);
            SendMessage = GetObject<TLInputBotInlineMessageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            Type.ToStream(output);
            ToStream(output, Title, Flags, (int)InputBotInlineResultFlags.Title);
            ToStream(output, Description, Flags, (int)InputBotInlineResultFlags.Description);
            ToStream(output, Url, Flags, (int)InputBotInlineResultFlags.Url);
            ToStream(output, Thumb, Flags, (int)InputBotInlineResultFlags.Thumb);
            ToStream(output, Content, Flags, (int)InputBotInlineResultFlags.Content);
            SendMessage.ToStream(output);
        }
    }

    public class TLInputBotInlineResultPhoto : TLInputBotInlineResultBase
    {
        public const uint Signature = TLConstructors.TLInputBotInlineResultPhoto;

        public TLString Id { get; set; }

        public TLString Type { get; set; }

        public TLInputPhotoBase Photo { get; set; }

        public TLInputBotInlineMessageBase SendMessage { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                Type.ToBytes(),
                Photo.ToBytes(),
                SendMessage.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLString>(input);
            Type = GetObject<TLString>(input);
            Photo = GetObject<TLInputPhotoBase>(input);
            SendMessage = GetObject<TLInputBotInlineMessageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            Type.ToStream(output);
            Photo.ToStream(output);
            SendMessage.ToStream(output);
        }
    }

    public class TLInputBotInlineResultDocument : TLInputBotInlineResultBase
    {
        public const uint Signature = TLConstructors.TLInputBotInlineResultDocument;

        public TLInt Flags { get; set; }

        public TLString Id { get; set; }

        public TLString Type { get; set; }

        public TLString Title { get; set; }

        public TLString Description { get; set; }

        public TLInputDocumentBase Document { get; set; }

        public TLInputBotInlineMessageBase SendMessage { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                Type.ToBytes(),
                ToBytes(Title, Flags, (int)InputBotInlineResultFlags.Title),
                ToBytes(Description, Flags, (int)InputBotInlineResultFlags.Description),
                Document.ToBytes(),
                SendMessage.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLString>(input);
            Type = GetObject<TLString>(input);
            if (IsSet(Flags, (int)InputBotInlineResultFlags.Title))
            {
                Title = GetObject<TLString>(input);
            }
            if (IsSet(Flags, (int)InputBotInlineResultFlags.Description))
            {
                Description = GetObject<TLString>(input);
            }
            Document = GetObject<TLInputDocumentBase>(input);
            SendMessage = GetObject<TLInputBotInlineMessageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            Type.ToStream(output);
            ToStream(output, Title, Flags, (int)InputBotInlineResultFlags.Title);
            ToStream(output, Description, Flags, (int)InputBotInlineResultFlags.Description);
            Document.ToStream(output);
            SendMessage.ToStream(output);
        }
    }

    public class TLInputBotInlineResultGame : TLInputBotInlineResultBase
    {
        public const uint Signature = TLConstructors.TLInputBotInlineResultGame;

        public TLString Id { get; set; }

        public TLString ShortName { get; set; }

        public TLInputBotInlineMessageBase SendMessage { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                ShortName.ToBytes(),
                SendMessage.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLString>(input);
            ShortName = GetObject<TLString>(input);
            SendMessage = GetObject<TLInputBotInlineMessageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            ShortName.ToStream(output);
            SendMessage.ToStream(output);
        }
    }
}

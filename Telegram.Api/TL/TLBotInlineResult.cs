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
    public enum BotInlineResultFlags
    {
        //Unread = 0x1,         // 0
        Title = 0x2,            // 1
        Description = 0x4,      // 2
        Url = 0x8,              // 3
        Thumb = 0x10,           // 4
        Content = 0x20,         // 5
        Size = 0x40,            // 6
        Duration = 0x80,        // 7
    }

    [Flags]
    public enum BotInlineMediaResultFlags
    {
        Photo = 0x1,            // 0
        Document = 0x2,         // 1
        Title = 0x4,            // 2
        Description = 0x8,      // 3
    }

    public abstract class TLBotInlineResultBase : TLObject
    {
        public TLBotInlineResultBase Self { get { return this; } }

        public TLString Id { get; set; }

        public TLString Type { get; set; }

        public TLBotInlineMessageBase SendMessage { get; set; }

        public TLLong QueryId { get; set; }
    }

    public class TLBotInlineMediaResult : TLBotInlineResultBase, IMediaGif
    {
        public const uint Signature = TLConstructors.TLBotInlineMediaResult;

        public TLInt Flags { get; set; }

        public TLPhotoBase Photo { get; set; }

        public TLDocumentBase Document { get; set; }

        public TLString Title { get; set; }

        public TLString Description { get; set; }

        public TLBotInlineMediaResult ThumbSelf { get { return this; } }

        private double _downloadingProgress;

        public double DownloadingProgress
        {
            get { return _downloadingProgress; }
            set { SetField(ref _downloadingProgress, value, () => DownloadingProgress); }
        }

        public double LastProgress { get; set; }
        public bool IsCanceled { get; set; }
        public string IsoFileName { get; set; }
        public bool? AutoPlayGif { get; set; }
        public bool Forbidden { get; set; }

        public static string BotInlineMediaResultFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (BotInlineMediaResultFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public override string ToString()
        {
            return string.Format("TLBotInlineMediaResult flags={0} type={2} id={1} photo={3} document={4} title={5} description={6} send_message=[{7}]", BotInlineMediaResultFlagsString(Flags), Id, Type, Photo != null, Document != null, Title, Description, SendMessage);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLString>(bytes, ref position);
            Type = GetObject<TLString>(bytes, ref position);
            Photo = GetObject<TLPhotoBase>(Flags, (int)BotInlineMediaResultFlags.Photo, null, bytes, ref position);
            Document = GetObject<TLDocumentBase>(Flags, (int)BotInlineMediaResultFlags.Document, null, bytes, ref position);
            Title = GetObject<TLString>(Flags, (int)BotInlineMediaResultFlags.Title, null, bytes, ref position);
            Description = GetObject<TLString>(Flags, (int)BotInlineMediaResultFlags.Description, null, bytes, ref position);
            SendMessage = GetObject<TLBotInlineMessageBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                Type.ToBytes(),
                ToBytes(Photo, Flags, (int)BotInlineMediaResultFlags.Photo),
                ToBytes(Document, Flags, (int)BotInlineMediaResultFlags.Document),
                ToBytes(Title, Flags, (int)BotInlineMediaResultFlags.Title),
                ToBytes(Description, Flags, (int)BotInlineMediaResultFlags.Description),
                SendMessage.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLString>(input);
            Type = GetObject<TLString>(input);
            Photo = GetObject<TLPhotoBase>(Flags, (int)BotInlineMediaResultFlags.Photo, null, input);
            Document = GetObject<TLDocumentBase>(Flags, (int)BotInlineMediaResultFlags.Document, null, input);
            Title = GetObject<TLString>(Flags, (int)BotInlineMediaResultFlags.Title, null, input);
            Description = GetObject<TLString>(Flags, (int)BotInlineMediaResultFlags.Description, null, input);
            SendMessage = GetObject<TLBotInlineMessageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            Type.ToStream(output);
            ToStream(output, Photo, Flags, (int)BotInlineMediaResultFlags.Photo);
            ToStream(output, Document, Flags, (int)BotInlineMediaResultFlags.Document);
            ToStream(output, Title, Flags, (int)BotInlineMediaResultFlags.Title);
            ToStream(output, Description, Flags, (int)BotInlineMediaResultFlags.Description);
            SendMessage.ToStream(output);
        }
    }

    //public class TLBotInlineMediaResultDocument : TLBotInlineResultBase, IMediaGif
    //{
    //    public const uint Signature = TLConstructors.TLBotInlineMediaResultDocument;

    //    public TLDocumentBase Document { get; set; }

    //    public TLBotInlineMediaResultDocument ThumbSelf { get { return this; } }

    //    private double _downloadingProgress;

    //    public double DownloadingProgress
    //    {
    //        get { return _downloadingProgress; }
    //        set { SetField(ref _downloadingProgress, value, () => DownloadingProgress); }
    //    }

    //    public double LastProgress { get; set; }
    //    public bool IsCanceled { get; set; }
    //    public string IsoFileName { get; set; }
    //    public bool? AutoPlayGif { get; set; }
    //    public bool Forbidden { get; set; }

    //    public override string ToString()
    //    {
    //        return string.Format("TLBotInlineMediaResultDocument id={0} type={1} document={2} send_message={3}", Id, Type, Document, SendMessage);
    //    }

    //    public override TLObject FromBytes(byte[] bytes, ref int position)
    //    {
    //        bytes.ThrowExceptionIfIncorrect(ref position, Signature);

    //        Id = GetObject<TLString>(bytes, ref position);
    //        Type = GetObject<TLString>(bytes, ref position);
    //        Document = GetObject<TLDocumentBase>(bytes, ref position);
    //        SendMessage = GetObject<TLBotInlineMessageBase>(bytes, ref position);

    //        return this;
    //    }

    //    public override byte[] ToBytes()
    //    {
    //        return TLUtils.Combine(
    //            TLUtils.SignatureToBytes(Signature),
    //            Id.ToBytes(),
    //            Type.ToBytes(),
    //            Document.ToBytes(),
    //            SendMessage.ToBytes());
    //    }

    //    public override TLObject FromStream(Stream input)
    //    {
    //        Id = GetObject<TLString>(input);
    //        Type = GetObject<TLString>(input);
    //        Document = GetObject<TLDocumentBase>(input);
    //        SendMessage = GetObject<TLBotInlineMessageBase>(input);

    //        return this;
    //    }

    //    public override void ToStream(Stream output)
    //    {
    //        output.Write(TLUtils.SignatureToBytes(Signature));
    //        Id.ToStream(output);
    //        Type.ToStream(output);
    //        Document.ToStream(output);
    //        SendMessage.ToStream(output);
    //    }
    //}

    public class TLBotInlineMediaResultPhoto : TLBotInlineResultBase
    {
        public const uint Signature = TLConstructors.TLBotInlineMediaResultPhoto;

        public TLPhotoBase Photo { get; set; }

        public override string ToString()
        {
            return string.Format("TLBotInlineMediaResultPhoto id={0} type={1} photo={2} send_message={3}", Id, Type, Photo, SendMessage);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLString>(bytes, ref position);
            Type = GetObject<TLString>(bytes, ref position);
            Photo = GetObject<TLPhotoBase>(bytes, ref position);
            SendMessage = GetObject<TLBotInlineMessageBase>(bytes, ref position);

            return this;
        }

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
            Photo = GetObject<TLPhotoBase>(input);
            SendMessage = GetObject<TLBotInlineMessageBase>(input);

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
    
    public class TLBotInlineResult : TLBotInlineResultBase, IMediaGif
    {
        public const uint Signature = TLConstructors.TLBotInlineResult;

        public TLInt Flags { get; set; }

        public TLString Title { get; set; }

        public TLString Description { get; set; }

        public TLString Url { get; set; }

        public virtual TLString ThumbUrl { get; protected set; }

        public string ThumbUrlString { get { return ThumbUrl != null ? ThumbUrl.ToString() : string.Empty; } }

        public virtual TLString ContentUrl { get; protected set; }

        public string ContentUrlString { get { return ContentUrl != null ? ContentUrl.ToString() : string.Empty; } }

        public virtual TLString ContentType { get; protected set; }

        public virtual TLInt W { get; protected set; }

        public virtual TLInt H { get; protected set; }

        public virtual TLInt Duration { get; protected set; }

        public static string BotInlineResultFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (BotInlineResultFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public override string ToString()
        {
            return string.Format("TLBotInlineResult flags={0} type={2} id={1} title={3} description={4} url={5} thumb_url={6} content_url={7} content_type={8} w={9} h={10} duration={11} send_message=[{12}]", 
                BotInlineResultFlagsString(Flags), Id, Type, Title, Description, Url, ThumbUrl, ContentUrl, ContentType, W, H, Duration, SendMessage);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLString>(bytes, ref position);
            Type = GetObject<TLString>(bytes, ref position);

            if (IsSet(Flags, (int)BotInlineResultFlags.Title))
            {
                Title = GetObject<TLString>(bytes, ref position);
            }

            if (IsSet(Flags, (int)BotInlineResultFlags.Description))
            {
                Description = GetObject<TLString>(bytes, ref position);
            }

            if (IsSet(Flags, (int)BotInlineResultFlags.Url))
            {
                Url = GetObject<TLString>(bytes, ref position);
            }

            if (IsSet(Flags, (int)BotInlineResultFlags.Thumb))
            {
                ThumbUrl = GetObject<TLString>(bytes, ref position);
            }
            
            if (IsSet(Flags, (int)BotInlineResultFlags.Content))
            {
                ContentUrl = GetObject<TLString>(bytes, ref position);
                ContentType = GetObject<TLString>(bytes, ref position);
            }

            if (IsSet(Flags, (int)BotInlineResultFlags.Size))
            {
                W = GetObject<TLInt>(bytes, ref position);
                H = GetObject<TLInt>(bytes, ref position);
            }

            if (IsSet(Flags, (int)BotInlineResultFlags.Duration))
            {
                Duration = GetObject<TLInt>(bytes, ref position);
            }

            SendMessage = GetObject<TLBotInlineMessageBase>(bytes, ref position);

            return this;
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
            SendMessage = GetObject<TLBotInlineMessageBase>(input);

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
            ToStream(output, ThumbUrl, Flags, (int)InputBotInlineResultFlags.Thumb);
            ToStream(output, ContentUrl, Flags, (int)InputBotInlineResultFlags.Content);
            ToStream(output, ContentType, Flags, (int)InputBotInlineResultFlags.Content);
            ToStream(output, W, Flags, (int)InputBotInlineResultFlags.Size);
            ToStream(output, H, Flags, (int)InputBotInlineResultFlags.Size);
            ToStream(output, Duration, Flags, (int)InputBotInlineResultFlags.Duration);
            SendMessage.ToStream(output);
        }

        public TLBotInlineResult ThumbSelf { get { return this; } }

        #region IGifMedia

        private TLDocumentBase _document;

        public TLDocumentBase Document
        {
            get
            {
                if (_document != null) return _document;

                if (TLString.Equals(Type, new TLString("gif"), StringComparison.OrdinalIgnoreCase))
                {
                    var document = new TLDocumentExternal();
                    document.Id = TLLong.Random();
                    document.ResultId = Id;
                    document.Type = Type;
                    document.Url = Url ?? TLString.Empty;
                    document.ThumbUrl = ThumbUrl ?? TLString.Empty;
                    document.ContentUrl = ContentUrl ?? TLString.Empty;
                    document.ContentType = ContentType ?? TLString.Empty;
                    document.Attributes = new TLVector<TLDocumentAttributeBase>{ new TLDocumentAttributeAnimated() };
                    if (W != null && H != null && W.Value > 0 && H.Value > 0)
                    {
                        var duration = Duration ?? new TLInt(0);

                        var videoAttribute = new TLDocumentAttributeVideo66 { Flags = new TLInt(0), W = W, H = H, Duration = duration };
                        document.Attributes.Add(videoAttribute);
                    }

                    _document = document;
                }
                else if (TLString.Equals(Type, new TLString("photo"), StringComparison.OrdinalIgnoreCase))
                {
                    
                }

                return _document;
            }
        }

        private double _downloadingProgress;

        public double DownloadingProgress
        {
            get { return _downloadingProgress; }
            set { SetField(ref _downloadingProgress, value, () => DownloadingProgress); }
        }

        public double LastProgress { get; set; }

        public bool IsCanceled { get; set; }
        
        public string IsoFileName { get; set; }
        
        public bool? AutoPlayGif { get; set; }

        public bool Forbidden { get; set; }
        #endregion
    }


    public class TLBotInlineResult76 : TLBotInlineResult
    {
        public new const uint Signature = TLConstructors.TLBotInlineResult76;

        public TLWebDocumentBase Thumb { get; set; }

        public override TLString ThumbUrl
        {
            get { return Thumb != null ? Thumb.Url : null; }
        }

        public TLWebDocumentBase Content { get; set; }

        public override TLString ContentUrl
        {
            get { return Content != null ? Content.Url : null; }
        }

        public override TLString ContentType
        {
            get { return Content != null ? Content.MimeType : null; }
        }

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

        public override string ToString()
        {
            return string.Format("TLBotInlineResult76 flags={0} type={2} id={1} title={3} description={4} url={5} thumb_url={6} content_url={7} content_type={8} w={9} h={10} duration={11} send_message=[{12}]",
                BotInlineResultFlagsString(Flags), Id, Type, Title, Description, Url, ThumbUrl, ContentUrl, ContentType, W, H, Duration, SendMessage);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLString>(bytes, ref position);
            Type = GetObject<TLString>(bytes, ref position);
            Title = GetObject<TLString>(Flags, (int)BotInlineResultFlags.Title, null, bytes, ref position);
            Description = GetObject<TLString>(Flags, (int)BotInlineResultFlags.Description, null, bytes, ref position);
            Url = GetObject<TLString>(Flags, (int)BotInlineResultFlags.Url, null, bytes, ref position);
            Thumb = GetObject<TLWebDocumentBase>(Flags, (int)BotInlineResultFlags.Thumb, null, bytes, ref position);
            Content = GetObject<TLWebDocumentBase>(Flags, (int)BotInlineResultFlags.Content, null, bytes, ref position);
            SendMessage = GetObject<TLBotInlineMessageBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                Type.ToBytes(),
                ToBytes(Title, Flags, (int)BotInlineResultFlags.Title),
                ToBytes(Description, Flags, (int)BotInlineResultFlags.Description),
                ToBytes(Url, Flags, (int)BotInlineResultFlags.Url),
                ToBytes(Thumb, Flags, (int)BotInlineResultFlags.Thumb),
                ToBytes(Content, Flags, (int)BotInlineResultFlags.Content),
                SendMessage.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLString>(input);
            Type = GetObject<TLString>(input);
            Title = GetObject<TLString>(Flags, (int)BotInlineResultFlags.Title, null, input);
            Description = GetObject<TLString>(Flags, (int)BotInlineResultFlags.Description, null, input);
            Url = GetObject<TLString>(Flags, (int)BotInlineResultFlags.Url, null, input);
            Thumb = GetObject<TLWebDocumentBase>(Flags, (int)BotInlineResultFlags.Thumb, null, input);
            Content = GetObject<TLWebDocumentBase>(Flags, (int)BotInlineResultFlags.Content, null, input);
            SendMessage = GetObject<TLBotInlineMessageBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id.ToStream(output);
            Type.ToStream(output);
            ToStream(output, Title, Flags, (int)BotInlineResultFlags.Title);
            ToStream(output, Description, Flags, (int)BotInlineResultFlags.Description);
            ToStream(output, Url, Flags, (int)BotInlineResultFlags.Url);
            ToStream(output, Thumb, Flags, (int)BotInlineResultFlags.Thumb);
            ToStream(output, Content, Flags, (int)BotInlineResultFlags.Content);
            SendMessage.ToStream(output);
        }
    }
}

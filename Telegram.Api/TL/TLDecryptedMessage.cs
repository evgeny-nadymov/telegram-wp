// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.Linq;
using Telegram.Api.Services.Cache;
#if WIN_RT
using Windows.UI.Xaml;
#else
using System.Windows;
#endif
using Telegram.Api.Extensions;
using Telegram.Api.Services;

namespace Telegram.Api.TL
{

    public interface IMessage
    {
        TLString Message { get; set; }

        TLObject From { get; }
    }

    public interface ISeqNo
    {
        TLInt InSeqNo { get; set; }

        TLInt OutSeqNo { get; set; }
    }

    public abstract class TLDecryptedMessageBase : TLObject
    {
        public TLDecryptedMessageBase Self { get { return this; } }

        public TLDecryptedMessageBase MediaSelf { get { return this; } }

        public TLLong RandomId { get; set; }

        public long RandomIndex
        {
            get { return RandomId != null ? RandomId.Value : 0; }
            set { RandomId = new TLLong(value); }
        }

        public TLString RandomBytes { get; set; }

        private bool _isHighlighted;

        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set { SetField(ref _isHighlighted, value, () => IsHighlighted); }
        }

        private TLObject _from;

        public TLObject From
        {
            get
            {
                if (_from != null) return _from;

                var cacheService = InMemoryCacheService.Instance;
                _from = cacheService.GetUser(FromId);

                return _from;
            }
        }

        #region Additional
        public TLInt ChatId { get; set; }
        public TLInputEncryptedFileBase InputFile { get; set; }     // to send media

        public TLInt FromId { get; set; }
        public TLBool Out { get; set; }
        public TLBool Unread { get; set; }

        public TLInt Date { get; set; }
        public int DateIndex
        {
            get { return Date != null ? Date.Value : 0; }
            set { Date = new TLInt(value); }
        }

        public TLInt Qts { get; set; }
        public int QtsIndex
        {
            get { return Qts != null ? Qts.Value : 0; }
            set { Qts = new TLInt(value); }
        }

        public TLLong DeleteDate { get; set; }

        public long DeleteIndex
        {
            get { return DeleteDate != null ? DeleteDate.Value : 0; }
            set { DeleteDate = new TLLong(value); }
        }

        public MessageStatus Status { get; set; }

        public virtual bool ShowFrom
        {
            get { return false; }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetField(ref _isSelected, value, () => IsSelected); }
        }

        public abstract Visibility SelectionVisibility { get; }

        public TLInt TTL { get; set; }

        private bool _isTTLStarted;

        public bool IsTTLStarted
        {
            get { return _isTTLStarted; }
            set { SetField(ref _isTTLStarted, value, () => IsTTLStarted); }
        }

        public abstract Visibility SecretPhotoMenuVisibility { get; }

        public abstract Visibility MessageVisibility { get; }

        public TLDecryptedMessageBase Reply { get; set; }

        public virtual ReplyInfo ReplyInfo
        {
            get { return null; }
        }

        public virtual Visibility ReplyVisibility { get { return Visibility.Collapsed; } }

        public virtual double MediaWidth { get { return 12.0 + 311.0 + 12.0; } }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            RandomId = GetObject<TLLong>(bytes, ref position);
            RandomBytes = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public virtual void Update(TLDecryptedMessageBase message)
        {
            ChatId = message.ChatId ?? ChatId;
            InputFile = message.InputFile ?? InputFile;
            FromId = message.FromId ?? FromId;
            Out = message.Out ?? Out;
            Unread = message.Unread ?? Unread;
            Date = message.Date ?? Date;
            Qts = message.Qts ?? Qts;
            DeleteDate = message.DeleteDate ?? DeleteDate;
            Status = message.Status;
            TTL = message.TTL ?? TTL;
        }

        public virtual bool IsSticker()
        {
            return false;
        }

        public static bool IsSticker(TLDecryptedMessageMediaExternalDocument document)
        {
#if WP8
            if (document != null
                && document.Size.Value > 0
                && document.Size.Value < Constants.StickerMaxSize)
            {
                //var documentStickerAttribute = document22.Attributes.FirstOrDefault(x => x is TLDocumentAttributeSticker);

                if (//documentStickerAttribute != null
                    //&& 
                    string.Equals(document.MimeType.ToString(), "image/webp", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(document.FileExt, "webp", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
#endif

            return false;
        }

        public virtual bool IsGif()
        {
            return false;
        }

        public virtual bool IsVoice()
        {
            return false;
        }

        public static bool IsVoice(IAttributes attributes, TLInt size)
        {
#if WP8
            if (size == null || size.Value > 0)
            {
                var audioAttribute = attributes.Attributes.FirstOrDefault(x => x is TLDocumentAttributeAudio46) as TLDocumentAttributeAudio46;
                if (audioAttribute != null && audioAttribute.Voice)
                {
                    return true;
                }
            }
#endif

            return false;
        }

        public static bool IsVoice(TLDecryptedMessageMediaDocument45 document)
        {
#if WP8
            var document22 = document;
            if (document22 != null)
            {
                return IsVoice(document22, document22.Size);
            }
#endif

            return false;
        }

        public virtual bool IsVideo()
        {
            return false;
        }

        public static bool IsVideo(IAttributes attributes, TLInt size)
        {
#if WP8
            if (size == null || size.Value > 0)
            {
                var videoAttribute = attributes.Attributes.FirstOrDefault(x => x is TLDocumentAttributeVideo) as TLDocumentAttributeVideo;
                var animatedAttribute = attributes.Attributes.FirstOrDefault(x => x is TLDocumentAttributeAnimated) as TLDocumentAttributeAnimated;
                if (videoAttribute != null && animatedAttribute == null)
                {
                    return true;
                }
            }
#endif

            return false;
        }

        public static bool IsVideo(TLDecryptedMessageMediaDocument45 document)
        {
#if WP8
            var document22 = document;
            if (document22 != null)
            {
                return IsVideo(document22, document22.Size);
            }
#endif

            return false;
        }
    }

    public class TLDecryptedMessagesContainter : TLDecryptedMessageBase
    {
        public const uint Signature = TLConstructors.TLDecryptedMessagesContainter;

        public TLMessageMediaBase WebPageMedia { get; set; }

        public TLVector<TLDecryptedMessage> FwdMessages { get; set; }

        public override Visibility SelectionVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public override Visibility SecretPhotoMenuVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public override Visibility MessageVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public TLObject From
        {
            get
            {
                //if (FwdMessages != null && FwdMessages.Count > 0)
                //{
                //    var fwdMessage = FwdMessages[0] as TLDecryptedMessage;
                //    if (fwdMessage != null)
                //    {
                //        var fwdPeer = fwdMessage.FwdFromPeer;
                //        if (fwdPeer != null)
                //        {
                //            var cacheService = InMemoryCacheService.Instance;
                //            if (fwdPeer is TLPeerChannel)
                //            {
                //                return cacheService.GetChat(fwdPeer.Id);
                //            }

                //            return cacheService.GetUser(fwdPeer.Id);
                //        }
                //    }

                //    return FwdMessages[0].FwdFrom;
                //}

                return null;
            }
        }

        public TLString Message
        {
            get
            {
                if (FwdMessages != null && FwdMessages.Count > 0)
                {
                    return FwdMessages[0].Message;
                }

                return null;
            }
        }

        public TLDecryptedMessageMediaBase Media
        {
            get
            {
                if (FwdMessages != null && FwdMessages.Count > 0)
                {
                    return FwdMessages[0].Media;
                }

                return null;
            }
        }
    }

    public class TLDecryptedMessage73 : TLDecryptedMessage45
    {
        public new const uint Signature = TLConstructors.TLDecryptedMessage73;

        private TLLong _groupedId;

        public TLLong GroupedId
        {
            get { return _groupedId; }
            set { SetField(out _groupedId, value, ref _flags, (int) MessageFlags.GroupedId); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            RandomId = GetObject<TLLong>(bytes, ref position);
            TTL = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            Media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLDecryptedMessageMediaBase>(bytes, ref position)
                : new TLDecryptedMessageMediaEmpty();
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int) MessageFlags.Entities, null, bytes, ref position);
            ViaBotName = GetObject<TLString>(Flags, (int) MessageFlags.ViaBotId, null, bytes, ref position);
            ReplyToRandomMsgId = GetObject<TLLong>(Flags, (int) MessageFlags.ReplyToMsgId, null, bytes, ref position);
            GroupedId = GetObject<TLLong>(Flags, (int) MessageFlags.GroupedId, null, bytes, ref position);

            if (IsVoice())
            {
                NotListened = true;
            }

            System.Diagnostics.Debug.WriteLine("  >>TLDecryptedMessage73.FromBytes random_id={0} ttl={1} message={2} media=[{3}]", RandomId, TTL, Message, Media);

            return this;
        }

        public override byte[] ToBytes()
        {

            System.Diagnostics.Debug.WriteLine("  <<TLDecryptedMessage73.ToBytes random_id={0} ttl={1} message={2} media=[{3}]", RandomId, TTL, Message, Media);

            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                RandomId.ToBytes(),
                TTL.ToBytes(),
                Message.ToBytes(),
                ToBytes(Media, Flags, (int)MessageFlags.Media),
                ToBytes(Entities, Flags, (int)MessageFlags.Entities),
                ToBytes(ViaBotName, Flags, (int)MessageFlags.ViaBotId),
                ToBytes(ReplyToRandomMsgId, Flags, (int)MessageFlags.ReplyToMsgId),
                ToBytes(GroupedId, Flags, (int)MessageFlags.GroupedId));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            RandomId = GetObject<TLLong>(input);
            TTL = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            Media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLDecryptedMessageMediaBase>(input)
                : new TLDecryptedMessageMediaEmpty();
            Entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)MessageFlags.Entities, null, input);
            ViaBotName = GetObject<TLString>(Flags, (int)MessageFlags.ViaBotId, null, input);
            ReplyToRandomMsgId = GetObject<TLLong>(Flags, (int)MessageFlags.ReplyToMsgId, null, input);
            GroupedId = GetObject<TLLong>(Flags, (int)MessageFlags.GroupedId, null, input);

            ChatId = GetNullableObject<TLInt>(input);
            InputFile = GetNullableObject<TLInputEncryptedFileBase>(input);
            FromId = GetNullableObject<TLInt>(input);
            Out = GetNullableObject<TLBool>(input);
            Unread = GetNullableObject<TLBool>(input);
            Date = GetNullableObject<TLInt>(input);
            DeleteDate = GetNullableObject<TLLong>(input);
            Qts = GetNullableObject<TLInt>(input);

            var status = GetObject<TLInt>(input);
            Status = (MessageStatus)status.Value;

            InSeqNo = GetNullableObject<TLInt>(input);
            OutSeqNo = GetNullableObject<TLInt>(input);
            CustomFlags = GetNullableObject<TLInt>(input);

            if (IsSet(CustomFlags, (int)MessageCustomFlags.BotInlineResult))
            {
                _inlineBotResult = GetObject<TLBotInlineResultBase>(input);
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            output.Write(RandomId.ToBytes());
            output.Write(TTL.ToBytes());
            output.Write(Message.ToBytes());
            ToStream(output, Media, Flags, (int)MessageFlags.Media);
            ToStream(output, Entities, Flags, (int)MessageFlags.Entities);
            ToStream(output, ViaBotName, Flags, (int)MessageFlags.ViaBotId);
            ToStream(output, ReplyToRandomMsgId, Flags, (int)MessageFlags.ReplyToMsgId);
            ToStream(output, GroupedId, Flags, (int)MessageFlags.GroupedId);

            ChatId.NullableToStream(output);
            InputFile.NullableToStream(output);
            FromId.NullableToStream(output);
            Out.NullableToStream(output);
            Unread.NullableToStream(output);
            Date.NullableToStream(output);
            DeleteDate.NullableToStream(output);
            Qts.NullableToStream(output);

            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());

            InSeqNo.NullableToStream(output);
            OutSeqNo.NullableToStream(output);
            CustomFlags.NullableToStream(output);

            if (IsSet(CustomFlags, (int)MessageCustomFlags.BotInlineResult))
            {
                _inlineBotResult.ToStream(output);
            }
        }

        public override string ToString()
        {
            if (Media is TLDecryptedMessageMediaEmpty)
            {
                return string.Format("TLDecryptedMessage73 random_id={7} qts={0} in_seq_no={1} out_seq_no={2} flags=[{3}] date={4} delete_date={5} message={6}", Qts, InSeqNo, OutSeqNo, TLMessageBase.MessageFlagsString(Flags), Date, DeleteDate, Message, RandomId);
            }

            return string.Format("TLDecryptedMessage73 random_id={7} qts={0} in_seq_no={1} out_seq_no={2} flags=[{3}] date={4} delete_date={5} media={6}", Qts, InSeqNo, OutSeqNo, TLMessageBase.MessageFlagsString(Flags), Date, DeleteDate, Media, RandomId);
        }
    }

    public class TLDecryptedMessage45 : TLDecryptedMessage17
    {
        public new const uint Signature = TLConstructors.TLDecryptedMessage45;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLVector<TLMessageEntityBase> _entities; 

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set
            {
                if (value != null)
                {
                    _entities = value;
                    Set(ref _flags, (int)MessageFlags.Entities);
                }
                else
                {
                    Unset(ref _flags, (int)MessageFlags.Entities);
                }
            }
        }

        private TLString _viaBotBane;

        public TLString ViaBotName
        {
            get { return _viaBotBane; }
            set
            {
                if (value != null)
                {
                    _viaBotBane = value;
                    Set(ref _flags, (int)MessageFlags.ViaBotId);
                }
                else
                {
                    Unset(ref _flags, (int)MessageFlags.ViaBotId);
                }
            }
        }

        private TLLong _replyToRandomMsgId;

        public TLLong ReplyToRandomMsgId
        {
            get { return _replyToRandomMsgId; }
            set
            {
                if (value != null && value.Value != 0)
                {
                    _replyToRandomMsgId = value;
                    Set(ref _flags, (int)MessageFlags.ReplyToMsgId);
                }
                else
                {
                    Unset(ref _flags, (int)MessageFlags.ReplyToMsgId);
                }
            }
        }

        protected TLBotInlineResultBase _inlineBotResult;

        public TLBotInlineResultBase InlineBotResult
        {
            get { return _inlineBotResult; }
            set
            {
                if (value != null)
                {
                    Set(ref _customFlags, (int)MessageCustomFlags.BotInlineResult);
                    _inlineBotResult = value;
                }
                else
                {
                    Unset(ref _customFlags, (int)MessageCustomFlags.BotInlineResult);
                    _inlineBotResult = null;
                }
            }
        }

        public void SetMedia()
        {
            Set(ref _flags, (int)MessageFlags.Media);
        }

        public void SetListened()
        {
            Unset(ref _flags, (int)MessageFlags.MediaUnread);
        }

        public bool NotListened
        {
            get { return IsSet(_flags, (int)MessageFlags.MediaUnread); }
            set { SetUnset(ref _flags, value, (int)MessageFlags.MediaUnread); }
        }

        public override ReplyInfo ReplyInfo
        {
            get { return ReplyToRandomMsgId != null ? new ReplyInfo { ReplyToRandomMsgId = ReplyToRandomMsgId, Reply = Reply } : null; }
        }

        public override Visibility ReplyVisibility
        {
            get { return ReplyToRandomMsgId != null && ReplyToRandomMsgId.Value != 0 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public override bool IsVoice()
        {
            var mediaAudio = Media as TLDecryptedMessageMediaAudio;
            if (mediaAudio != null)
            {
                return true;
            }

            var mediaDocument = Media as TLDecryptedMessageMediaDocument45;
            if (mediaDocument != null)
            {
                return IsVoice(mediaDocument);
            }

            return false;
        }

        public override bool IsVideo()
        {
            var mediaVideo = Media as TLDecryptedMessageMediaVideo;
            if (mediaVideo != null)
            {
                return true;
            }

            var mediaDocument = Media as TLDecryptedMessageMediaDocument45;
            if (mediaDocument != null)
            {
                return IsVideo(mediaDocument);
            }

            return false;
        }

        public override bool IsGif()
        {
            var mediaDocument = Media as TLDecryptedMessageMediaDocument45;
            if (mediaDocument != null && TLString.Equals(mediaDocument.MimeType, new TLString("video/mp4"), StringComparison.OrdinalIgnoreCase))
            {
                return TLMessageBase.IsGif(mediaDocument, mediaDocument.Size);
            }

            return false;
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            RandomId = GetObject<TLLong>(bytes, ref position);
            TTL = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            Media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLDecryptedMessageMediaBase>(bytes, ref position)
                : new TLDecryptedMessageMediaEmpty();
            if (IsSet(Flags, (int) MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);
            }
            if (IsSet(Flags, (int)MessageFlags.ViaBotId))
            {
                ViaBotName = GetObject<TLString>(bytes, ref position);
            }
            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToRandomMsgId = GetObject<TLLong>(bytes, ref position);
            }

            if (IsVoice())
            {
                NotListened = true;
            }

            System.Diagnostics.Debug.WriteLine("  >>TLDecryptedMessage45.FromBytes random_id={0} ttl={1} message={2} media=[{3}]", RandomId, TTL, Message, Media);

            return this;
        }

        public override byte[] ToBytes()
        {
            System.Diagnostics.Debug.WriteLine("  <<TLDecryptedMessage45.ToBytes random_id={0} ttl={1} message={2} media=[{3}]", RandomId, TTL, Message, Media);

            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                RandomId.ToBytes(),
                TTL.ToBytes(),
                Message.ToBytes(),
                ToBytes(Media, Flags, (int)MessageFlags.Media),
                ToBytes(Entities, Flags, (int)MessageFlags.Entities),
                ToBytes(ViaBotName, Flags, (int)MessageFlags.ViaBotId),
                ToBytes(ReplyToRandomMsgId, Flags, (int)MessageFlags.ReplyToMsgId));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            RandomId = GetObject<TLLong>(input);
            TTL = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            Media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLDecryptedMessageMediaBase>(input)
                : new TLDecryptedMessageMediaEmpty();
            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(input);
            }
            if (IsSet(Flags, (int)MessageFlags.ViaBotId))
            {
                ViaBotName = GetObject<TLString>(input);
            }
            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToRandomMsgId = GetObject<TLLong>(input);
            }

            ChatId = GetNullableObject<TLInt>(input);
            InputFile = GetNullableObject<TLInputEncryptedFileBase>(input);
            FromId = GetNullableObject<TLInt>(input);
            Out = GetNullableObject<TLBool>(input);
            Unread = GetNullableObject<TLBool>(input);
            Date = GetNullableObject<TLInt>(input);
            DeleteDate = GetNullableObject<TLLong>(input);
            Qts = GetNullableObject<TLInt>(input);

            var status = GetObject<TLInt>(input);
            Status = (MessageStatus)status.Value;

            InSeqNo = GetNullableObject<TLInt>(input);
            OutSeqNo = GetNullableObject<TLInt>(input);
            CustomFlags = GetNullableObject<TLInt>(input);

            if (IsSet(CustomFlags, (int)MessageCustomFlags.BotInlineResult))
            {
                _inlineBotResult = GetObject<TLBotInlineResultBase>(input);
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            output.Write(RandomId.ToBytes());
            output.Write(TTL.ToBytes());
            output.Write(Message.ToBytes());
            ToStream(output, Media, Flags, (int)MessageFlags.Media);
            ToStream(output, Entities, Flags, (int)MessageFlags.Entities);
            ToStream(output, ViaBotName, Flags, (int)MessageFlags.ViaBotId);
            ToStream(output, ReplyToRandomMsgId, Flags, (int)MessageFlags.ReplyToMsgId);

            ChatId.NullableToStream(output);
            InputFile.NullableToStream(output);
            FromId.NullableToStream(output);
            Out.NullableToStream(output);
            Unread.NullableToStream(output);
            Date.NullableToStream(output);
            DeleteDate.NullableToStream(output);
            Qts.NullableToStream(output);

            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());

            InSeqNo.NullableToStream(output);
            OutSeqNo.NullableToStream(output);
            CustomFlags.NullableToStream(output);
            
            if (IsSet(CustomFlags, (int)MessageCustomFlags.BotInlineResult))
            {
                _inlineBotResult.ToStream(output);
            }
        }

        public override string ToString()
        {
            if (Media is TLDecryptedMessageMediaEmpty)
            {
                return string.Format("TLDecryptedMessage45 qts={0} in_seq_no={1} out_seq_no={2} flags=[{3}] date={4} delete_date={5} message={6}", Qts, InSeqNo, OutSeqNo, TLMessageBase.MessageFlagsString(Flags), Date, DeleteDate, Message);
            }

            return string.Format("TLDecryptedMessage45 qts={0} in_seq_no={1} out_seq_no={2} flags=[{3}] date={4} delete_date={5} media={6}", Qts, InSeqNo, OutSeqNo, TLMessageBase.MessageFlagsString(Flags), Date, DeleteDate, Media);
        }
    }

    public class TLDecryptedMessage17 : TLDecryptedMessage, ISeqNo
    {
        public new const uint Signature = TLConstructors.TLDecryptedMessage17;

        public TLInt InSeqNo { get; set; }

        public TLInt OutSeqNo { get; set; }

        protected TLInt _customFlags;

        public TLInt CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        public override Visibility SecretPhotoMenuVisibility
        {
            get
            {
                var isSecretPhoto = Media is TLDecryptedMessageMediaPhoto;
                return isSecretPhoto && TTL.Value > 0.0 && TTL.Value <= 60.0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            RandomId = GetObject<TLLong>(bytes, ref position);
            TTL = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            Media = GetObject<TLDecryptedMessageMediaBase>(bytes, ref position);

            System.Diagnostics.Debug.WriteLine("  >>TLDecryptedMessage17.FromBytes random_id={0} ttl={1} message={2} media=[{3}]", RandomId, TTL, Message, Media);

            return this;
        }

        public override byte[] ToBytes()
        {
            System.Diagnostics.Debug.WriteLine("  <<TLDecryptedMessage17.ToBytes random_id={0} ttl={1} message={2} media=[{3}]", RandomId, TTL, Message, Media);

            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                RandomId.ToBytes(),
                TTL.ToBytes(),
                Message.ToBytes(),
                Media.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            RandomId = GetObject<TLLong>(input);
            TTL = GetObject<TLInt>(input);
            //RandomBytes = GetObject<TLString>(input);
            Message = GetObject<TLString>(input);
            Media = GetObject<TLDecryptedMessageMediaBase>(input);

            ChatId = GetNullableObject<TLInt>(input);
            InputFile = GetNullableObject<TLInputEncryptedFileBase>(input);
            FromId = GetNullableObject<TLInt>(input);
            Out = GetNullableObject<TLBool>(input);
            Unread = GetNullableObject<TLBool>(input);
            Date = GetNullableObject<TLInt>(input);
            DeleteDate = GetNullableObject<TLLong>(input);
            Qts = GetNullableObject<TLInt>(input);

            var status = GetObject<TLInt>(input);
            Status = (MessageStatus)status.Value;

            InSeqNo = GetNullableObject<TLInt>(input);
            OutSeqNo = GetNullableObject<TLInt>(input);
            CustomFlags = GetNullableObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(RandomId.ToBytes());
            output.Write(TTL.ToBytes());
            //output.Write(RandomBytes.ToBytes());
            output.Write(Message.ToBytes());
            Media.ToStream(output);

            ChatId.NullableToStream(output);
            InputFile.NullableToStream(output);
            FromId.NullableToStream(output);
            Out.NullableToStream(output);
            Unread.NullableToStream(output);
            Date.NullableToStream(output);
            DeleteDate.NullableToStream(output);
            Qts.NullableToStream(output);

            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());

            InSeqNo.NullableToStream(output);
            OutSeqNo.NullableToStream(output);
            CustomFlags.NullableToStream(output);
        }

        public override string ToString()
        {
            if (Media is TLDecryptedMessageMediaEmpty)
            {
                return string.Format("TLDecryptedMessage17 qts={0} in_seq_no={1} out_seq_no={2} date={3} delete_date={4} message={5}", Qts, InSeqNo, OutSeqNo, Date, DeleteDate, Message);
            }

            return string.Format("TLDecryptedMessage17 qts={0} in_seq_no={1} out_seq_no={2} date={3} delete_date={4} media={5}", Qts, InSeqNo, OutSeqNo, Date, DeleteDate, Media);
        }
    }

    public class TLDecryptedMessage : TLDecryptedMessageBase, IMessage
    {
        public const uint Signature = TLConstructors.TLDecryptedMessage;

        public TLString Message { get; set; }

        public TLDecryptedMessageMediaBase Media { get; set; }

        public override double MediaWidth
        {
            get
            {
                if (Media != null)
                {
                    return Media.MediaWidth;
                }

                return base.MediaWidth;
            }
        }

        public override bool IsSticker()
        {
            var mediaDocument = Media as TLDecryptedMessageMediaExternalDocument;
            if (mediaDocument != null)
            {
                return IsSticker(mediaDocument);
            }

            return false;
        }

        public override Visibility MessageVisibility
        {
            get { return Message == null || string.IsNullOrEmpty(Message.ToString()) ? Visibility.Collapsed : Visibility.Visible; }
        }

        public override Visibility SelectionVisibility
        {
            get { return Visibility.Visible; }
        }

        public override Visibility SecretPhotoMenuVisibility
        {
            get { return Visibility.Visible; }
        }

        public override string ToString()
        {
            if (Media is TLDecryptedMessageMediaEmpty)
            {
                return string.Format("TLDecryptedMessage qts={0} date={1} delete_date={2} message={3}", Qts, Date, DeleteDate, Message);
            }

            return string.Format("TLDecryptedMessage qts={0} date={1} delete_date={2} media={3}", Qts, Date, DeleteDate, Media);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            base.FromBytes(bytes, ref position);

            Message = GetObject<TLString>(bytes, ref position);
            Media = GetObject<TLDecryptedMessageMediaBase>(bytes, ref position);

            System.Diagnostics.Debug.WriteLine("  >>TLDecryptedMessage.FromBytes message={0} media=[{1}]", Message, Media);

            return this;
        }

        public override byte[] ToBytes()
        {
            System.Diagnostics.Debug.WriteLine("  <<TLDecryptedMessage.ToBytes message={0} media=[{1}]", Message, Media);

            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                RandomId.ToBytes(),
                RandomBytes.ToBytes(),
                Message.ToBytes(),
                Media.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            RandomId = GetObject<TLLong>(input);
            RandomBytes = GetObject<TLString>(input);
            Message = GetObject<TLString>(input);
            Media = GetObject<TLDecryptedMessageMediaBase>(input);

            ChatId = GetNullableObject<TLInt>(input);
            InputFile = GetNullableObject<TLInputEncryptedFileBase>(input);
            FromId = GetNullableObject<TLInt>(input);
            Out = GetNullableObject<TLBool>(input);
            Unread = GetNullableObject<TLBool>(input);
            Date = GetNullableObject<TLInt>(input);
            DeleteDate = GetNullableObject<TLLong>(input);
            Qts = GetNullableObject<TLInt>(input);

            var status = GetObject<TLInt>(input);
            Status = (MessageStatus)status.Value;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(RandomId.ToBytes());
            output.Write(RandomBytes.ToBytes());
            output.Write(Message.ToBytes());
            Media.ToStream(output);

            ChatId.NullableToStream(output);
            InputFile.NullableToStream(output);
            FromId.NullableToStream(output);
            Out.NullableToStream(output);
            Unread.NullableToStream(output);
            Date.NullableToStream(output);
            DeleteDate.NullableToStream(output);
            Qts.NullableToStream(output);

            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());
        }
    }

    public class TLDecryptedMessageService17 : TLDecryptedMessageService, ISeqNo
    {
        public new const uint Signature = TLConstructors.TLDecryptedMessageService17;

        public TLInt InSeqNo { get; set; }

        public TLInt OutSeqNo { get; set; }

        public TLInt Flags { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            RandomId = GetObject<TLLong>(bytes, ref position);
            Action = GetObject<TLDecryptedMessageActionBase>(bytes, ref position);

            System.Diagnostics.Debug.WriteLine("  >>TLDecryptedMessageService17.FromBytes random_id={0} action=[{1}]", RandomId, Action);

            return this;
        }

        public override byte[] ToBytes()
        {

            System.Diagnostics.Debug.WriteLine("  <<TLDecryptedMessageService17.ToBytes random_id={0} action=[{1}]", RandomId, Action);

            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                RandomId.ToBytes(),
                //RandomBytes.ToBytes(),
                Action.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            RandomId = GetObject<TLLong>(input);
            //RandomBytes = GetObject<TLString>(input);
            Action = GetObject<TLDecryptedMessageActionBase>(input);

            ChatId = GetNullableObject<TLInt>(input);
            FromId = GetNullableObject<TLInt>(input);
            Out = GetNullableObject<TLBool>(input);
            Unread = GetNullableObject<TLBool>(input);
            Date = GetNullableObject<TLInt>(input);
            DeleteDate = GetNullableObject<TLLong>(input);
            Qts = GetNullableObject<TLInt>(input);

            var status = GetObject<TLInt>(input);
            Status = (MessageStatus)status.Value;

            InSeqNo = GetNullableObject<TLInt>(input);
            OutSeqNo = GetNullableObject<TLInt>(input);
            Flags = GetNullableObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(RandomId.ToBytes());
            //output.Write(RandomBytes.ToBytes());
            Action.ToStream(output);

            ChatId.NullableToStream(output);
            FromId.NullableToStream(output);
            Out.NullableToStream(output);
            Unread.NullableToStream(output);
            Date.NullableToStream(output);
            DeleteDate.NullableToStream(output);
            Qts.NullableToStream(output);

            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());

            InSeqNo.NullableToStream(output);
            OutSeqNo.NullableToStream(output);
            Flags.NullableToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLDecryptedMessageService17 qts={0} in_seq_no={1} out_seq_no={2} random_id={3} date={4} delete_date={5} action=[{6}]", Qts, InSeqNo, OutSeqNo, RandomId, Date, DeleteDate, Action);
        }
    }

    public class TLDecryptedMessageService : TLDecryptedMessageBase
    {
        public const uint Signature = TLConstructors.TLDecryptedMessageService;

        public TLDecryptedMessageActionBase Action { get; set; }

        public override Visibility SelectionVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public override Visibility SecretPhotoMenuVisibility
        {
            get { return Visibility.Visible; }
        }

        public override Visibility MessageVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            base.FromBytes(bytes, ref position);

            Action = GetObject<TLDecryptedMessageActionBase>(bytes, ref position);

            System.Diagnostics.Debug.WriteLine("  >>TLDecryptedMessageService.FromBytes random_id={0} action=[{1}]", RandomId, Action);

            return this;
        }

        public override byte[] ToBytes()
        {
            System.Diagnostics.Debug.WriteLine("  <<TLDecryptedMessageService.ToBytes random_id={0} action=[{1}]", RandomId, Action);

            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                RandomId.ToBytes(),
                RandomBytes.ToBytes(),
                Action.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            RandomId = GetObject<TLLong>(input);
            RandomBytes = GetObject<TLString>(input);
            Action = GetObject<TLDecryptedMessageActionBase>(input);

            ChatId = GetNullableObject<TLInt>(input);
            FromId = GetNullableObject<TLInt>(input);
            Out = GetNullableObject<TLBool>(input);
            Unread = GetNullableObject<TLBool>(input);
            Date = GetNullableObject<TLInt>(input);
            DeleteDate = GetNullableObject<TLLong>(input);
            Qts = GetNullableObject<TLInt>(input);

            var status = GetObject<TLInt>(input);
            Status = (MessageStatus)status.Value;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(RandomId.ToBytes());
            output.Write(RandomBytes.ToBytes());
            output.Write(Action.ToBytes());

            ChatId.NullableToStream(output);
            FromId.NullableToStream(output);
            Out.NullableToStream(output);
            Unread.NullableToStream(output);
            Date.NullableToStream(output);
            DeleteDate.NullableToStream(output);
            Qts.NullableToStream(output);

            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());
        }

        public override string ToString()
        {
            var dateTimeString = "null";
            try
            {
                if (Date != null)
                {
                    var clientDelta = MTProtoService.Instance.ClientTicksDelta;
                    //var utc0SecsLong = Date.Value * 4294967296 - clientDelta;
                    var utc0SecsInt = Date.Value - clientDelta / 4294967296.0;
                    DateTime? dateTime = Helpers.Utils.UnixTimestampToDateTime(utc0SecsInt);
                    dateTimeString = dateTime.Value.ToString("H:mm:ss");
                }
            }
            catch (Exception ex)
            {

            }

            return string.Format("TLDecryptedMessageService qts={0} random_id={1} date={2} delete_date={3} action=[{4}]", Qts, RandomId, Date, DeleteDate, Action);
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
#if WIN_RT
using Windows.UI.Xaml;
#endif
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Extensions;
using Telegram.Api.TL.Interfaces;
using Telegram.Logs;

namespace Telegram.Api.TL
{
    public enum MessageStatus
    {
        Sending = 1,
        Confirmed = 0,
        Failed = 2,
        Read = 3,
        Broadcast = 4,
        Compressing = 5,
    }

    [Flags]
    public enum MessageCustomFlags
    {
        FwdMessageId = 0x1,
        FwdFromChannelPeer = 0x2,
        BotInlineResult = 0x4,
        Documents = 0x8,
        InputPeer = 0x10,
    }

    [Flags]
    public enum MessageFlags
    {
        Unread = 0x1,           // 0
        Out = 0x2,              // 1
        FwdFrom = 0x4,          // 2
        ReplyToMsgId = 0x8,     // 3
        Mentioned = 0x10,       // 4
        MediaUnread = 0x20,     // 5
        ReplyMarkup = 0x40,     // 6
        Entities = 0x80,        // 7
        FromId = 0x100,         // 8
        Media = 0x200,          // 9
        Views = 0x400,          // 10
        ViaBotId = 0x800,       // 11

        Silent = 0x2000,        // 13
        Post = 0x4000,          // 14
        EditDate = 0x8000,      // 15
        PostAuthor = 0x10000,   // 16
        GroupedId = 0x20000,    // 17
    }

    public interface IReplyToMsgId
    {
        TLInt ReplyToMsgId { get; set; }
        TLMessageBase Reply { get; set; }
        ReplyInfo ReplyInfo { get; }
        TLPeerBase ToId { get; set; }
    }

    public abstract class TLMessageBase : TLObject, ISelectable
    {
        public virtual TLObject FwdFrom { get; set; }

        public virtual Visibility ViaBotVisibility { get { return Visibility.Collapsed; } }

        public virtual Visibility ReplyOrViaBotVisibility { get { return Visibility.Collapsed; } }

        public virtual Visibility FwdViaBotVisibility { get { return Visibility.Collapsed; } }

        public virtual TLUserBase ViaBot { get { return null; } }

        public static string MessageFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (MessageFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public static string MessageCustomFlagsString(TLLong flags)
        {
            if (flags == null) return string.Empty;

            var list = (MessageCustomFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public abstract int DateIndex { get; set; }

        private TLLong _randomId;

        public TLLong RandomId
        {
            get { return _randomId; }
            set
            {
                _randomId = value;
            }
        }

        public long RandomIndex
        {
            get { return RandomId != null ? RandomId.Value : 0; }
            set { RandomId = new TLLong(value); }
        }

        /// <summary>
        /// Message Id
        /// </summary>
        public TLInt Id { get; set; }

        public int Index
        {
            get { return Id != null ? Id.Value : 0; }
            set { Id = new TLInt(value); }
        }

        public virtual void Update(TLMessageBase message)
        {
            Id = message.Id;
            Status = message.Status;
        }

        public override string ToString()
        {
            return "Id=" + Index + " RndId=" + RandomIndex;
        }

        #region Additional

        public string WebPageTitle { get; set; }

        public bool NoWebpage { get; set; }

        public virtual TLMessageBase Reply { get; set; }

        public virtual ReplyInfo ReplyInfo
        {
            get { return null; }
        }

        public virtual Visibility ReplyVisibility { get { return Visibility.Collapsed; } }

        public virtual double MediaWidth { get { return 12.0 + 311.0 + 12.0; } }

        public MessageStatus _status;

        public virtual MessageStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public bool _isAnimated;

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

        private bool _isHighlighted;

        public bool IsHighlighted
        {
            get { return _isHighlighted; }
            set { SetField(ref _isHighlighted, value, () => IsHighlighted); }
        }

        public virtual int MediaSize { get { return 0; } }

        public virtual Visibility MediaSizeVisibility { get { return Visibility.Collapsed; } }

        public virtual bool IsSelf()
        {
            return false;
        }

        public virtual bool IsAudioVideoMessage()
        {
            return false;
        }

        public virtual bool HasTTL()
        {
            return false;
        }

        public static bool HasTTL(TLMessageMediaBase mediaBase)
        {
            var mediaPhoto = mediaBase as TLMessageMediaPhoto70;
            if (mediaPhoto != null
                && mediaPhoto.TTLSeconds != null
                && mediaPhoto.TTLSeconds.Value > 0)
            {
                return true;
            }

            var mediaDocument = mediaBase as TLMessageMediaDocument70;
            if (mediaDocument != null
                && mediaDocument.TTLSeconds != null
                && mediaDocument.TTLSeconds.Value > 0)
            {
                return true;
            }

            return false;
        }

        public virtual bool IsExpired()
        {
            return false;
        }

        public static bool IsExpired(TLMessageMediaBase mediaBase)
        {
            var mediaPhoto = mediaBase as TLMessageMediaPhoto70;
            if (mediaPhoto != null
                && mediaPhoto.Photo == null
                && mediaPhoto.TTLSeconds != null
                && mediaPhoto.TTLSeconds.Value > 0)
            {
                return true;
            }

            var mediaDocument = mediaBase as TLMessageMediaDocument70;
            if (mediaDocument != null
                && mediaDocument.Document == null
                && mediaDocument.TTLSeconds != null
                && mediaDocument.TTLSeconds.Value > 0)
            {
                return true;
            }

            return false;
        }

        public virtual bool IsSticker()
        {
            return false;
        }

        public static bool IsSticker(TLDocumentBase document)
        {
#if WP8
            var document22 = document as TLDocument22;
            if (document22 != null
                && document22.DocumentSize > 0
                && document22.DocumentSize < Constants.StickerMaxSize)
            {
                var documentStickerAttribute = document22.Attributes.FirstOrDefault(x => x is TLDocumentAttributeSticker);

                if (documentStickerAttribute != null
                    && string.Equals(document22.MimeType.ToString(), "image/webp", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
#endif

            return false;
        }

        public static bool IsSticker(IAttributes attributes, TLInt size)
        {
#if WP8
            if (size != null
                && size.Value > 0
                && size.Value < Constants.StickerMaxSize)
            {
                var documentStickerAttribute = attributes.Attributes.FirstOrDefault(x => x is TLDocumentAttributeSticker);

                if (documentStickerAttribute != null)
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

        public static bool IsGif(IAttributes attributes, TLInt size)
        {
#if WP8
            if (size == null
                || (size.Value > 0
                    && size.Value < Constants.GifMaxSize))
            {
                var animatedAttribute = attributes.Attributes.FirstOrDefault(x => x is TLDocumentAttributeAnimated);
                var videoAttribute = attributes.Attributes.FirstOrDefault(x => x is TLDocumentAttributeVideo);
                if (animatedAttribute != null
                    && videoAttribute != null)
                {
                    return true;
                }
            }
#endif

            return false;
        }

        public static bool IsGif(TLDocumentBase document)
        {
#if WP8
            var document22 = document as TLDocument22;
            if (document22 != null && TLString.Equals(document22.MimeType, new TLString("video/mp4"), StringComparison.OrdinalIgnoreCase))
            {
                return IsGif(document22, document22.Size);
            }

            var documentExternal = document as TLDocumentExternal;
            if (documentExternal != null
                && string.Equals(documentExternal.Type.ToString(), "gif", StringComparison.OrdinalIgnoreCase))
            {
                return IsGif(documentExternal, null);
            }
#endif

            return false;
        }

        public virtual bool IsMusic()
        {
            return false;
        }

        public static bool IsMusic(IAttributes attributes, TLInt size)
        {
#if WP8
            if (size == null || size.Value > 0)
            {
                var audioAttribute = attributes.Attributes.FirstOrDefault(x => x is TLDocumentAttributeAudio46) as TLDocumentAttributeAudio46;
                if (audioAttribute != null && !audioAttribute.Voice)
                {
                    return true;
                }
            }
#endif

            return false;
        }

        public static bool IsMusic(TLDocumentBase document)
        {
#if WP8
            var document22 = document as TLDocument22;
            if (document22 != null)
            {
                return IsMusic(document22, document22.Size);
            }
#endif

            return false;
        }

        public virtual bool IsVoice()
        {
            return false;
        }

        public static bool IsVoice(IAttributes attributes, TLInt size)
        {
#if WP8
            //if (size == null || size.Value > 0) // TLInlineBotResult non cached voice with unknown size
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

        public static bool IsVoice(TLDocumentBase document)
        {
#if WP8
            var document22 = document as TLDocument22;
            if (document22 != null)
            {
                return IsVoice(document22, document22.Size);
            }
#endif

            return false;
        }

        public virtual bool IsRoundVideo()
        {
            return false;
        }

        public static bool IsRoundVideo(IAttributes attributes, TLInt size)
        {
#if WP8
            if (size == null || size.Value > 0)
            {
                var videoAttribute = attributes.Attributes.FirstOrDefault(x => x is TLDocumentAttributeVideo66) as TLDocumentAttributeVideo66;
                var animatedAttribute = attributes.Attributes.FirstOrDefault(x => x is TLDocumentAttributeAnimated) as TLDocumentAttributeAnimated;
                if (videoAttribute != null && animatedAttribute == null)
                {
                    return videoAttribute.RoundMessage;
                }
            }
#endif

            return false;
        }

        public static bool IsRoundVideo(TLDocumentBase document)
        {
#if WP8
            var document22 = document as TLDocument22;
            if (document22 != null)
            {
                return IsRoundVideo(document22, document22.Size);
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

        public static bool IsVideo(TLDocumentBase document)
        {
#if WP8
            var document22 = document as TLDocument22;
            if (document22 != null)
            {
                return IsVideo(document22, document22.Size);
            }
#endif

            return false;
        }

        public TLMessageBase Self { get { return this; } }

        public bool ShowSeparator { get; set; }

        #endregion

        public virtual void Edit(TLMessageBase messageBase)
        {

        }
    }

    public class ReplyInfo
    {
        public TLInt ReplyToMsgId { get; set; }

        public TLLong ReplyToRandomMsgId { get; set; }

        public TLObject Reply { get; set; }
    }

    public abstract class TLMessageCommon : TLMessageBase
    {
        private TLInt _fromId;

        public virtual TLInt FromId
        {
            get { return _fromId; }
            set { _fromId = value; }
        }

        public TLPeerBase ToId { get; set; }

        public virtual TLBool Out { get; set; }

        private TLBool _unread;

        public virtual void SetUnread(TLBool value)
        {
            _unread = value;
        }

        public virtual void SetUnreadSilent(TLBool value)
        {
            _unread = value;
        }

        public virtual TLBool Unread
        {
            get { return _unread; }
            set
            {
                SetField(ref _unread, value, () => Unread);
                NotifyOfPropertyChange(() => Status);
            }
        }

        public bool IsChannelMessage
        {
            get
            {
                if (FromId == null || FromId.Value <= 0) return true;

                if (ToId is TLPeerChannel)
                {
                    var cacheService = InMemoryCacheService.Instance;
                    var channel = cacheService.GetChat(ToId.Id) as TLChannel;
                    if (channel != null && channel.IsBroadcast) return true;
                }

                return false;
            }
        }

        public override MessageStatus Status
        {
            get
            {
                if (_status == MessageStatus.Broadcast)
                {
                    return _status;
                }

                if (!Unread.Value)
                {
                    return MessageStatus.Read;
                }

                return _status;
            }
            set
            {
                if (_status == MessageStatus.Broadcast) return;
                if (_status == MessageStatus.Read) return;

                //System.Diagnostics.Debug.WriteLine("SetStatus hash={0} status={1}", GetHashCode(), value);

                SetField(ref _status, value, () => Status);
            }
        }

        public override int DateIndex
        {
            get { return Date.Value; }
            set { Date = new TLInt(value); }
        }

        public TLInt _date;

        public TLInt Date
        {
            get { return _date; }
            set { _date = value; }
        }

        public virtual Visibility ShareButtonVisibility { get { return Visibility.Collapsed; } }

        public override bool IsSelf()
        {
            var peerUser = ToId as TLPeerUser;
            if (peerUser != null)
            {
                return FromId.Value != -1 && FromId.Value == peerUser.Id.Value;
            }

            return false;
        }

        public override string ToString()
        {
            string dateTimeString = null;
            try
            {
                var clientDelta = MTProtoService.Instance.ClientTicksDelta;
                //var utc0SecsLong = Date.Value * 4294967296 - clientDelta;
                var utc0SecsInt = Date.Value - clientDelta / 4294967296.0;
                DateTime? dateTime = Helpers.Utils.UnixTimestampToDateTime(utc0SecsInt);
                dateTimeString = dateTime.Value.ToString("H:mm:ss dd.MM");
            }
            catch (Exception ex)
            {

            }

            return base.ToString() + string.Format(" [{0} {4}] FromId={1} ToId=[{2}] U={3} S={5}", Date, FromId, ToId, Unread, dateTimeString, Status);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            FromId = GetObject<TLInt>(bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);
            Out = GetObject<TLBool>(bytes, ref position);
            _unread = GetObject<TLBool>(bytes, ref position);
            _date = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);
            Out = GetObject<TLBool>(input);
            _unread = GetObject<TLBool>(input);
            _date = GetObject<TLInt>(input);

            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            var status = GetObject<TLInt>(input);
            Status = (MessageStatus)status.Value;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(FromId.ToBytes());
            ToId.ToStream(output);
            output.Write(Out.ToBytes());
            output.Write(Unread.ToBytes());
            output.Write(Date.ToBytes());

            RandomId = RandomId ?? new TLLong(0);
            RandomId.ToStream(output);
            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());
        }


        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = (TLMessageCommon)message;
            FromId = m.FromId;
            ToId = m.ToId;
            Out = m.Out;
            if (Unread.Value != m.Unread.Value)
            {
                if (Unread.Value)
                {
                    _unread = m.Unread;
                }
            }
            _date = m.Date;
        }

        #region Additional

        protected TLObject _from;

        public virtual TLObject From
        {
            get
            {
                if (_from != null) return _from;

                var cacheService = InMemoryCacheService.Instance;

                if (FromId == null || FromId.Value <= 0)
                {
                    _from = cacheService.GetChat(ToId.Id);
                    return _from;
                }
                if (ToId is TLPeerChannel)
                {
                    var channel = cacheService.GetChat(ToId.Id) as TLChannel;
                    if (channel != null && !channel.IsMegaGroup)
                    {
                        _from = channel;
                        return _from;
                    }
                }

                _from = cacheService.GetUser(FromId);
                return _from;
            }
        }

        protected TLObject _to;

        public virtual TLObject To
        {
            get
            {
                if (_to != null) return _to;

                var cacheService = InMemoryCacheService.Instance;

                if (ToId is TLPeerUser)
                {
                    var user = cacheService.GetUser(ToId.Id);
                    if (user != null)
                    {
                        _to = user;
                        return _to;
                    }
                }
                if (ToId is TLPeerChat)
                {
                    var chat = cacheService.GetChat(ToId.Id);
                    if (chat != null)
                    {
                        _to = chat;
                        return _to;
                    }
                }
                if (ToId is TLPeerChannel)
                {
                    var channel = cacheService.GetChat(ToId.Id);
                    if (channel != null)
                    {
                        _to = channel;
                        return _to;
                    }
                }

                return null;
            }
        }


        public override bool ShowFrom
        {
            get
            {
                if (this is TLMessageService)
                {
                    return false;
                }

                if (FromId == null || FromId.Value <= 0)
                {
                    return false;
                }

                if (ToId is TLPeerChat) return true;

                if (ToId is TLPeerChannel)
                {
                    var cacheService = InMemoryCacheService.Instance;
                    var channel = cacheService.GetChat(ToId.Id) as TLChannel;
                    if (channel != null && channel.IsMegaGroup) return true;
                }

                return false;
            }
        }
        #endregion
    }

    public class TLMessageEmpty : TLMessageBase
    {
        public const uint Signature = TLConstructors.TLMessageEmpty;

        public override int DateIndex { get; set; }

        public override string ToString()
        {
            return base.ToString() + ", TLMessageEmpty";
        }

        public override Visibility SelectionVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            RandomId = GetObject<TLObject>(input) as TLLong;
            var status = GetObject<TLInt>(input);
            _status = (MessageStatus)status.Value;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id = Id ?? new TLInt(0);
            output.Write(Id.ToBytes());

            RandomId.NullableToStream(output);
            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());
        }
    }

    public class TLMessagesContainter : TLMessageBase
    {
        public const uint Signature = TLConstructors.TLMessagesContainter;

        public TLMessageMediaBase WebPageMedia { get; set; }

        public TLVector<TLMessage25> FwdMessages { get; set; }

        public bool WithMyScore { get; set; }

        public TLMessage25 EditMessage { get; set; }

        public int EditUntil { get; set; }

        public string EditTimerString
        {
            get
            {
                var editUntil = EditUntil;
                var now = TLUtils.DateToUniversalTimeTLInt(MTProtoService.Instance.ClientTicksDelta, DateTime.Now).Value;

                if (editUntil < now)
                {
                    return string.Empty;
                }

                var timeSpan = TimeSpan.FromSeconds(editUntil - now);
                if (timeSpan.TotalMinutes > 5.0)
                {
                    return string.Empty;
                }

                if (timeSpan.TotalDays > 1.0)
                {
                    return string.Format("({0})", TimeSpan.FromSeconds(editUntil - now));
                }

                if (timeSpan.TotalHours > 1.0)
                {
                    return string.Format("({0:hh\\:mm\\:ss})", TimeSpan.FromSeconds(editUntil - now));
                }

                return string.Format("({0:mm\\:ss})", TimeSpan.FromSeconds(editUntil - now));
            }
        }

        public override int DateIndex { get; set; }

        public override Visibility SelectionVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public TLObject From
        {
            get
            {
                if (FwdMessages != null && FwdMessages.Count > 0)
                {
                    var fwdMessage48 = FwdMessages[0] as TLMessage48;
                    if (fwdMessage48 != null)
                    {
                        var fwdHeader = fwdMessage48.FwdHeader;
                        if (fwdHeader != null)
                        {
                            var cacheService = InMemoryCacheService.Instance;
                            if (fwdHeader.ChannelId != null)
                            {
                                return cacheService.GetChat(fwdHeader.ChannelId);
                            }

                            if (fwdHeader.FromId != null)
                            {
                                return cacheService.GetUser(fwdHeader.FromId);
                            }
                        }
                    }

                    var fwdMessage = FwdMessages[0] as TLMessage40;
                    if (fwdMessage != null)
                    {
                        var fwdPeer = fwdMessage.FwdFromPeer;
                        if (fwdPeer != null)
                        {
                            var cacheService = InMemoryCacheService.Instance;
                            if (fwdPeer is TLPeerChannel)
                            {
                                return cacheService.GetChat(fwdPeer.Id);
                            }

                            return cacheService.GetUser(fwdPeer.Id);
                        }
                    }

                    return FwdMessages[0].FwdFrom;
                }

                if (EditMessage != null)
                {
                    var fwdMessage48 = EditMessage as TLMessage48;
                    if (fwdMessage48 != null)
                    {
                        var fwdHeader = fwdMessage48.FwdHeader;
                        if (fwdHeader != null)
                        {
                            var cacheService = InMemoryCacheService.Instance;
                            if (fwdHeader.ChannelId != null)
                            {
                                return cacheService.GetChat(fwdHeader.ChannelId);
                            }

                            if (fwdHeader.FromId != null)
                            {
                                return cacheService.GetUser(fwdHeader.FromId);
                            }
                        }
                    }

                    return EditMessage.From;
                }

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
                if (EditMessage != null)
                {
                    return EditMessage.Message;
                }

                return null;
            }
        }

        public TLMessageMediaBase Media
        {
            get
            {
                if (FwdMessages != null && FwdMessages.Count > 0)
                {
                    return FwdMessages[0].Media;
                }
                if (EditMessage != null)
                {
                    return EditMessage.Media;
                }

                return null;
            }
        }
    }

    public class TLMessage73 : TLMessage70
    {
        public new const uint Signature = TLConstructors.TLMessage73;

        protected TLLong _groupedId;

        public TLLong GroupedId
        {
            get { return _groupedId; }
            set { SetField(out _groupedId, value, ref _flags, (int)MessageFlags.GroupedId); }
        }

        public override void Edit(TLMessageBase messageBase)
        {
            base.Edit(messageBase);

            var message = messageBase as TLMessage73;
            if (message != null)
            {
                GroupedId = message.GroupedId;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject(Flags, (int)MessageFlags.FromId, new TLInt(-1), bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);
            _fwdHeader = GetObject<TLMessageFwdHeader73>(Flags, (int)MessageFlags.FwdFrom, null, bytes, ref position);
            _viaBotId = GetObject<TLInt>(Flags, (int)MessageFlags.ViaBotId, null, bytes, ref position);
            _replyToMsgId = GetObject<TLInt>(Flags, (int)MessageFlags.ReplyToMsgId, null, bytes, ref position);
            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _media = GetObject<TLMessageMediaBase>(Flags, (int)MessageFlags.Media, new TLMessageMediaEmpty(), bytes, ref position);
            _replyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)MessageFlags.ReplyMarkup, null, bytes, ref position);
            Entities = GetObject(Flags, (int)MessageFlags.Entities, new TLVector<TLMessageEntityBase>(), bytes, ref position);  // Important to add empty Entities to avoid BrowseNavigationService.ParseText calls
            _views = GetObject(Flags, (int)MessageFlags.Views, new TLInt(0), bytes, ref position);
            _editDate = GetObject(Flags, (int)MessageFlags.EditDate, new TLInt(0), bytes, ref position);
            _postAuthor = GetObject<TLString>(Flags, (int)MessageFlags.PostAuthor, null, bytes, ref position);
            _groupedId = GetObject<TLLong>(Flags, (int)MessageFlags.GroupedId, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);
            _fwdHeader = GetObject<TLMessageFwdHeader73>(Flags, (int)MessageFlags.FwdFrom, null, input);
            _viaBotId = GetObject<TLInt>(Flags, (int)MessageFlags.ViaBotId, null, input);
            _replyToMsgId = GetObject<TLInt>(Flags, (int)MessageFlags.ReplyToMsgId, null, input);
            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            _media = GetObject<TLMessageMediaBase>(Flags, (int)MessageFlags.Media, new TLMessageMediaEmpty(), input);
            _replyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)MessageFlags.ReplyMarkup, null, input);
            Entities = GetObject(Flags, (int)MessageFlags.Entities, new TLVector<TLMessageEntityBase>(), input);  // Important to add empty Entities to avoid BrowseNavigationService.ParseText calls
            _views = GetObject(Flags, (int)MessageFlags.Views, new TLInt(0), input);
            _editDate = GetObject(Flags, (int)MessageFlags.EditDate, new TLInt(0), input);
            _postAuthor = GetObject<TLString>(Flags, (int)MessageFlags.PostAuthor, null, input);
            _groupedId = GetObject<TLLong>(Flags, (int)MessageFlags.GroupedId, null, input);

            CustomFlags = GetNullableObject<TLLong>(input);
            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            _status = (MessageStatus)GetObject<TLInt>(input).Value;
            _fwdMessageId = GetObject<TLInt>(CustomFlags, (int)MessageCustomFlags.FwdMessageId, null, input);
            _fwdFromChannelPeer = GetObject<TLInputPeerBase>(CustomFlags, (int)MessageCustomFlags.FwdFromChannelPeer, null, input);
            _inlineBotResultQueryId = GetObject<TLLong>(CustomFlags, (int)MessageCustomFlags.BotInlineResult, null, input);
            _inlineBotResultId = GetObject<TLString>(CustomFlags, (int)MessageCustomFlags.BotInlineResult, null, input);
            _documents = GetObject<TLVector<TLDocumentBase>>(CustomFlags, (int)MessageCustomFlags.Documents, null, input);
            _inputPeer = GetObject<TLInputPeerBase>(CustomFlags, (int)MessageCustomFlags.InputPeer, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            try
            {
                // to set flags before writing to file
                Views = Views ?? new TLInt(0);
                EditDate = EditDate ?? new TLInt(0);

                Flags.ToStream(output);
                Id = Id ?? new TLInt(0);
                Id.ToStream(output);
                FromId.ToStream(output);
                ToId.ToStream(output);
                ToStream(output, FwdHeader, Flags, (int)MessageFlags.FwdFrom);
                ToStream(output, ViaBotId, Flags, (int)MessageFlags.ViaBotId);
                ToStream(output, ReplyToMsgId, Flags, (int)MessageFlags.ReplyToMsgId);
                Date.ToStream(output);
                Message.ToStream(output);
                ToStream(output, Media, Flags, (int)MessageFlags.Media);
                ToStream(output, ReplyMarkup, Flags, (int)MessageFlags.ReplyMarkup);
                ToStream(output, Entities, Flags, (int)MessageFlags.Entities);
                ToStream(output, Views, Flags, (int)MessageFlags.Views);
                ToStream(output, EditDate, Flags, (int)MessageFlags.EditDate);
                ToStream(output, PostAuthor, Flags, (int)MessageFlags.PostAuthor);
                ToStream(output, GroupedId, Flags, (int)MessageFlags.GroupedId);

                CustomFlags.NullableToStream(output);
                RandomId = RandomId ?? new TLLong(0);
                RandomId.ToStream(output);
                var status = new TLInt((int)Status);
                status.ToStream(output);
                ToStream(output, _fwdMessageId, CustomFlags, (int)MessageCustomFlags.FwdMessageId);
                ToStream(output, _fwdFromChannelPeer, CustomFlags, (int)MessageCustomFlags.FwdFromChannelPeer);
                ToStream(output, _inlineBotResultQueryId, CustomFlags, (int)MessageCustomFlags.BotInlineResult);
                ToStream(output, _inlineBotResultId, CustomFlags, (int)MessageCustomFlags.BotInlineResult);
                ToStream(output, _documents, CustomFlags, (int)MessageCustomFlags.Documents);
                ToStream(output, _inputPeer, CustomFlags, (int)MessageCustomFlags.InputPeer);
            }
            catch (Exception ex)
            {
                var logString = string.Format("TLMessage73.ToStream id={0} flags={1} fwd_from_peer={2} fwd_date={3} reply_to_msg_id={4} media={5} reply_markup={6} entities={7} views={8} from_id={9} edit_date={10} fwd_header={11}", Index, MessageFlagsString(Flags), FwdFromPeer, FwdDate, ReplyToMsgId, Media, ReplyMarkup, Entities, Views, FromId, EditDate, FwdHeader != null);

                TLUtils.WriteException(logString, ex);
            }
        }

        public override void Update(TLMessageBase message)
        {
            var message73 = message as TLMessage73;
            if (message73 != null)
            {
                GroupedId = message73.GroupedId;

                base.Update(message);
            }
        }
    }

    public class TLMessage70 : TLMessage48
    {
        public new const uint Signature = TLConstructors.TLMessage70;

        protected TLString _postAuthor;

        public TLString PostAuthor
        {
            get { return _postAuthor; }
            set { SetField(out _postAuthor, value, ref _flags, (int)MessageFlags.PostAuthor); }
        }

        private string _author;

        public override string Author
        {
            get
            {
                if (_author != null) return _author;

                if (!TLString.IsNullOrEmpty(PostAuthor))
                {
                    _author = PostAuthor.ToString();
                    return _author;
                }

                if (!(ToId is TLPeerChannel)) return null;
                if (FromId == null || FromId.Value < 0) return null;

                var cacheService = InMemoryCacheService.Instance;
                var user = cacheService.GetUser(FromId);
                _author = user != null ? user.FullName2 : string.Empty;

                return _author;
            }
        }

        public override Visibility AuthorVisibility
        {
            get { return !string.IsNullOrEmpty(Author) && !IsMusic() ? Visibility.Visible : Visibility.Collapsed; }
        }

        public bool TTLMediaExpired
        {
            get
            {
                var mediaPhoto = Media as TLMessageMediaPhoto70;
                if (mediaPhoto != null)
                {
                    return mediaPhoto.Photo == null;
                }

                var mediaDocument = Media as TLMessageMediaDocument70;
                if (mediaDocument != null)
                {
                    return mediaDocument.Document == null;
                }

                return false;
            }
        }

        protected TLInputPeerBase _inputPeer;

        public TLInputPeerBase InputPeer
        {
            get { return _inputPeer; }
            set
            {
                if (_inputPeer != null && value == null)
                {

                }

                SetField(out _inputPeer, value, ref _customFlags, (int)MessageCustomFlags.InputPeer);
            }
        }

        public override void Edit(TLMessageBase messageBase)
        {
            base.Edit(messageBase);

            var message = messageBase as TLMessage48;
            if (message != null)
            {
                EditDate = message.EditDate;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject(Flags, (int)MessageFlags.FromId, new TLInt(-1), bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);
            _fwdHeader = GetObject<TLMessageFwdHeader>(Flags, (int)MessageFlags.FwdFrom, null, bytes, ref position);
            _viaBotId = GetObject<TLInt>(Flags, (int)MessageFlags.ViaBotId, null, bytes, ref position);
            _replyToMsgId = GetObject<TLInt>(Flags, (int)MessageFlags.ReplyToMsgId, null, bytes, ref position);
            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _media = GetObject<TLMessageMediaBase>(Flags, (int)MessageFlags.Media, new TLMessageMediaEmpty(), bytes, ref position);
            _replyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)MessageFlags.ReplyMarkup, null, bytes, ref position);
            _entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)MessageFlags.Entities, null, bytes, ref position);
            _views = GetObject(Flags, (int)MessageFlags.Views, new TLInt(0), bytes, ref position);
            _editDate = GetObject(Flags, (int)MessageFlags.EditDate, new TLInt(0), bytes, ref position);
            _postAuthor = GetObject<TLString>(Flags, (int)MessageFlags.PostAuthor, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            //#if DEBUG
            //            var flagsString = MessageFlagsString(Flags);
            //#endif
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);
            _fwdHeader = GetObject<TLMessageFwdHeader>(Flags, (int)MessageFlags.FwdFrom, null, input);
            _viaBotId = GetObject<TLInt>(Flags, (int)MessageFlags.ViaBotId, null, input);
            _replyToMsgId = GetObject<TLInt>(Flags, (int)MessageFlags.ReplyToMsgId, null, input);
            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            _media = GetObject<TLMessageMediaBase>(Flags, (int)MessageFlags.Media, new TLMessageMediaEmpty(), input);
            _replyMarkup = GetObject<TLReplyKeyboardBase>(Flags, (int)MessageFlags.ReplyMarkup, null, input);
            _entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)MessageFlags.Entities, null, input);
            _views = GetObject(Flags, (int)MessageFlags.Views, new TLInt(0), input);
            _editDate = GetObject(Flags, (int)MessageFlags.EditDate, new TLInt(0), input);
            _postAuthor = GetObject<TLString>(Flags, (int)MessageFlags.PostAuthor, null, input);

            CustomFlags = GetNullableObject<TLLong>(input);
            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            _status = (MessageStatus)GetObject<TLInt>(input).Value;
            _fwdMessageId = GetObject<TLInt>(CustomFlags, (int)MessageCustomFlags.FwdMessageId, null, input);
            _fwdFromChannelPeer = GetObject<TLInputPeerBase>(CustomFlags, (int)MessageCustomFlags.FwdFromChannelPeer, null, input);
            _inlineBotResultQueryId = GetObject<TLLong>(CustomFlags, (int)MessageCustomFlags.BotInlineResult, null, input);
            _inlineBotResultId = GetObject<TLString>(CustomFlags, (int)MessageCustomFlags.BotInlineResult, null, input);
            _documents = GetObject<TLVector<TLDocumentBase>>(CustomFlags, (int)MessageCustomFlags.Documents, null, input);
            _inputPeer = GetObject<TLInputPeerBase>(CustomFlags, (int)MessageCustomFlags.InputPeer, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            try
            {
                // to set flags before writing to file
                Views = Views ?? new TLInt(0);
                EditDate = EditDate ?? new TLInt(0);

                Flags.ToStream(output);
                Id = Id ?? new TLInt(0);
                Id.ToStream(output);
                FromId.ToStream(output);
                ToId.ToStream(output);
                ToStream(output, FwdHeader, Flags, (int)MessageFlags.FwdFrom);
                ToStream(output, ViaBotId, Flags, (int)MessageFlags.ViaBotId);
                ToStream(output, ReplyToMsgId, Flags, (int)MessageFlags.ReplyToMsgId);
                Date.ToStream(output);
                Message.ToStream(output);
                ToStream(output, Media, Flags, (int)MessageFlags.Media);
                ToStream(output, ReplyMarkup, Flags, (int)MessageFlags.ReplyMarkup);
                ToStream(output, Entities, Flags, (int)MessageFlags.Entities);
                ToStream(output, Views, Flags, (int)MessageFlags.Views);
                ToStream(output, EditDate, Flags, (int)MessageFlags.EditDate);
                ToStream(output, PostAuthor, Flags, (int)MessageFlags.PostAuthor);

                CustomFlags.NullableToStream(output);
                RandomId = RandomId ?? new TLLong(0);
                RandomId.ToStream(output);
                var status = new TLInt((int)Status);
                status.ToStream(output);
                ToStream(output, _fwdMessageId, CustomFlags, (int)MessageCustomFlags.FwdMessageId);
                ToStream(output, _fwdFromChannelPeer, CustomFlags, (int)MessageCustomFlags.FwdFromChannelPeer);
                ToStream(output, _inlineBotResultQueryId, CustomFlags, (int)MessageCustomFlags.BotInlineResult);
                ToStream(output, _inlineBotResultId, CustomFlags, (int)MessageCustomFlags.BotInlineResult);
                ToStream(output, _documents, CustomFlags, (int)MessageCustomFlags.Documents);
                ToStream(output, _inputPeer, CustomFlags, (int)MessageCustomFlags.InputPeer);
            }
            catch (Exception ex)
            {
                var logString = string.Format("TLMessage70.ToStream id={0} flags={1} fwd_from_peer={2} fwd_date={3} reply_to_msg_id={4} media={5} reply_markup={6} entities={7} views={8} from_id={9} edit_date={10} fwd_header={11}", Index, MessageFlagsString(Flags), FwdFromPeer, FwdDate, ReplyToMsgId, Media, ReplyMarkup, Entities, Views, FromId, EditDate, FwdHeader != null);

                TLUtils.WriteException(logString, ex);
            }
        }

        public override void Update(TLMessageBase message)
        {
            var message70 = message as TLMessage70;
            if (message70 != null)
            {
                // begin copy flags
                Out = message70.Out;
                IsMention = message70.IsMention;
                NotListened = message70.NotListened;
                Silent = message70.Silent;
                Post = message70.Post;
                // end copy flags

                Id = message70.Id;
                Status = message70.Status;
                FromId = message70.FromId;
                ToId = message70.ToId;

                if (Unread.Value && !message70.Unread.Value)
                {
                    SetUnreadSilent(message70.Unread);
                }
                if (!Unread.Value && message70.Unread.Value)
                {
#if DEBUG && WINDOWS_PHONE
                    var builder = new StringBuilder();
                    var stackTrace = new StackTrace();
                    var frames = stackTrace.GetFrames();
                    foreach (var r in frames)
                    {
                        builder.AppendLine(string.Format("Method: {0}", r.GetMethod()));
                    }
                    //Helpers.Execute.ShowDebugMessage("Set read message as unread\ncurrent=" + this + "\nnew=" + message + "\n\n" + builder.ToString());
#endif
                }

                _date = message70.Date;
                Message = message70.Message;

                UpdateMedia(message);

                FwdFromId = message70.FwdFromId;
                FwdDate = message70.FwdDate;
                ReplyToMsgId = message70.ReplyToMsgId;

                if (message70.Reply != null)
                {
                    Reply = message70.Reply;
                }

                if (message70.ReplyMarkup != null)
                {
                    var oldCustomFlags = ReplyMarkup != null ? ReplyMarkup.CustomFlags : null;
                    ReplyMarkup = message70.ReplyMarkup;
                    ReplyMarkup.CustomFlags = oldCustomFlags;
                }

                if (message70.Entities != null)
                {
                    Entities = message70.Entities;
                }

                FwdFromPeer = message70.FwdFromPeer;
                if (message70.FwdMessageId != null) FwdMessageId = message70.FwdMessageId;
                if (message70.FwdFromChannelPeer != null) FwdFromChannelPeer = message70.FwdFromChannelPeer;
                if (message70.Views != null)
                {
                    var currentViews = Views != null ? Views.Value : 0;
                    if (currentViews < message70.Views.Value)
                    {
                        Views = message70.Views;
                    }
                }

                if (message70.ViaBotId != null)
                {
                    ViaBotId = message70.ViaBotId;
                }

                if (message70.InlineBotResultQueryId != null)
                {
                    InlineBotResultQueryId = message70.InlineBotResultQueryId;
                }

                if (message70.InlineBotResultId != null)
                {
                    InlineBotResultId = message70.InlineBotResultId;
                }

                FwdHeader = message70.FwdHeader;

                if (message70.EditDate != null)
                {
                    EditDate = message70.EditDate;
                }
                PostAuthor = message70.PostAuthor;

                if (message70.InputPeer != null)
                {
                    InputPeer = message70.InputPeer;
                }
            }
        }

        private void UpdateMedia(TLMessageBase message)
        {
            var m = (TLMessage)message;
            var oldMedia = Media;
            var newMedia = m.Media;
            if (oldMedia.GetType() != newMedia.GetType())
            {
                _media = m.Media;
                if (_media != null) SetMedia();
            }
            else
            {
                var oldMediaGeoLive = oldMedia as TLMessageMediaGeoLive;
                var newMediaGeoLive = newMedia as TLMessageMediaGeoLive;
                if (oldMediaGeoLive != null && newMediaGeoLive != null)
                {
                    oldMediaGeoLive.Geo = newMediaGeoLive.Geo;
                    oldMediaGeoLive.Period = newMediaGeoLive.Period;

                    return;
                }

                var oldMediaVenue = oldMedia as TLMessageMediaVenue;
                var newMediaVenue = newMedia as TLMessageMediaVenue;
                if (oldMediaVenue != null && newMediaVenue != null)
                {
                    oldMediaVenue.Geo = newMediaVenue.Geo;

                    return;
                }

                var oldMediaGeo = oldMedia as TLMessageMediaGeo;
                var newMediaGeo = newMedia as TLMessageMediaGeo;
                if (oldMediaGeo != null && newMediaGeo != null)
                {
                    oldMediaGeo.Geo = newMediaGeo.Geo;

                    return;
                }

                var oldInvoice = oldMedia as TLMessageMediaInvoice;
                var newInvoice = newMedia as TLMessageMediaInvoice;
                if (oldInvoice != null && newInvoice != null && newInvoice.ReceiptMsgId != null)
                {
                    oldInvoice.ReceiptMsgId = newInvoice.ReceiptMsgId;

                    return;
                }

                var oldMediaGame = oldMedia as TLMessageMediaGame;
                var newMediaGame = newMedia as TLMessageMediaGame;
                if (oldMediaGame != null && newMediaGame != null)
                {
                    newMediaGame.Message = m.Message;
                    if (oldMediaGame.Game.GetType() != newMediaGame.Game.GetType())
                    {
                        _media = m.Media;
                    }
                    else
                    {
                        var oldGame = oldMediaGame.Game;
                        var newGame = newMediaGame.Game;
                        if (oldGame != null
                            && newGame != null
                            && (oldGame.Id.Value != newGame.Id.Value))
                        {
                            newMediaGame.SourceMessage = this;
                            _media = newMediaGame;
                        }
                        else
                        {
                            oldMediaGame.Message = m.Message;
                        }
                    }

                    return;
                }

                var oldMediaWebPage = oldMedia as TLMessageMediaWebPage;
                var newMediaWebPage = newMedia as TLMessageMediaWebPage;
                if (oldMediaWebPage != null && newMediaWebPage != null)
                {
                    if (oldMediaWebPage.WebPage.GetType() != newMediaWebPage.WebPage.GetType())
                    {
                        _media = m.Media;
                    }
                    else
                    {
                        var oldWebPage = oldMediaWebPage.WebPage as TLWebPage35;
                        var newWebPage = newMediaWebPage.WebPage as TLWebPage35;
                        if (oldWebPage != null
                            && newWebPage != null
                            && (oldWebPage.Id.Value != newWebPage.Id.Value))
                        {
                            _media = m.Media;
                        }
                    }

                    return;
                }

                var oldMediaDocument = oldMedia as TLMessageMediaDocument;
                var newMediaDocument = newMedia as TLMessageMediaDocument;
                if (oldMediaDocument != null && newMediaDocument != null)
                {
                    var oldDocument = oldMediaDocument.Document as TLDocument;
                    var newDocument = newMediaDocument.Document as TLDocument;
                    if (oldDocument == null || newDocument == null)
                    {
                        _media = m.Media;

                        if (HasTTL())
                        {
                            if (oldDocument != null)
                            {
                                newMediaDocument.Document = oldDocument;
                            }

                            var oldTTLMessageMedia = oldMediaDocument as ITTLMessageMedia;
                            var newTTLMessageMedia = newMediaDocument as ITTLMessageMedia;
                            if (oldTTLMessageMedia != null && newTTLMessageMedia != null)
                            {
                                newTTLMessageMedia.TTLParams = oldTTLMessageMedia.TTLParams;
                            }
                        }
                    }
                    else
                    {
                        if (oldDocument.Id.Value != newDocument.Id.Value
                            || oldDocument.AccessHash.Value != newDocument.AccessHash.Value)
                        {
                            oldMediaDocument.Document = newMediaDocument.Document;
                        }
                    }

                    return;
                }

                var oldMediaVideo = oldMedia as TLMessageMediaVideo;
                var newMediaVideo = newMedia as TLMessageMediaVideo;
                if (oldMediaVideo != null && newMediaVideo != null)
                {
                    if (oldMediaVideo.Video.GetType() != newMediaVideo.Video.GetType())
                    {
                        _media = m.Media;
                    }
                    else
                    {
                        var oldVideo = oldMediaVideo.Video as TLVideo;
                        var newVideo = newMediaVideo.Video as TLVideo;
                        if (oldVideo != null
                            && newVideo != null
                            && (oldVideo.Id.Value != newVideo.Id.Value
                                || oldVideo.AccessHash.Value != newVideo.AccessHash.Value))
                        {
                            var isoFileName = Media.IsoFileName;
                            _media = m.Media;
                            _media.IsoFileName = isoFileName;
                        }
                    }

                    return;
                }

                var oldMediaAudio = oldMedia as TLMessageMediaAudio;
                var newMediaAudio = newMedia as TLMessageMediaAudio;
                if (oldMediaAudio != null && newMediaAudio != null)
                {
                    if (oldMediaAudio.Audio.GetType() != newMediaAudio.Audio.GetType())
                    {
                        _media = m.Media;
                    }
                    else
                    {
                        var oldAudio = oldMediaAudio.Audio as TLAudio;
                        var newAudio = newMediaAudio.Audio as TLAudio;
                        if (oldAudio != null
                            && newAudio != null
                            && (oldAudio.Id.Value != newAudio.Id.Value
                                || oldAudio.AccessHash.Value != newAudio.AccessHash.Value))
                        {
                            var isoFileName = Media.IsoFileName;
                            var notListened = Media.NotListened;
                            _media = m.Media;
                            _media.IsoFileName = isoFileName;
                            _media.NotListened = notListened;
                        }
                    }

                    return;
                }

                var oldMediaPhoto = oldMedia as TLMessageMediaPhoto;
                var newMediaPhoto = newMedia as TLMessageMediaPhoto;
                if (oldMediaPhoto == null || newMediaPhoto == null)
                {
                    _media = m.Media;
                }
                else
                {
                    var oldPhoto = oldMediaPhoto.Photo as TLPhoto;
                    var newPhoto = newMediaPhoto.Photo as TLPhoto;
                    if (oldPhoto == null || newPhoto == null)
                    {
                        _media = m.Media;

                        if (HasTTL())
                        {
                            if (oldPhoto != null)
                            {
                                newMediaPhoto.Photo = oldPhoto;
                            }

                            var oldTTLMessageMedia = oldMediaPhoto as ITTLMessageMedia;
                            var newTTLMessageMedia = newMediaPhoto as ITTLMessageMedia;
                            if (oldTTLMessageMedia != null && newTTLMessageMedia != null)
                            {
                                newTTLMessageMedia.TTLParams = oldTTLMessageMedia.TTLParams;
                            }
                        }
                    }
                    else
                    {
                        if (oldPhoto.AccessHash.Value != newPhoto.AccessHash.Value)
                        {
                            var oldCachedSize =
                                oldPhoto.Sizes.FirstOrDefault(x => x is TLPhotoCachedSize) as TLPhotoCachedSize;
                            var oldMSize =
                                oldPhoto.Sizes.FirstOrDefault(
                                    x => TLString.Equals(x.Type, new TLString("m"), StringComparison.OrdinalIgnoreCase));
                            foreach (var size in newPhoto.Sizes)
                            {
                                if (size is TLPhotoCachedSize)
                                {
                                    size.TempUrl = oldCachedSize != null ? oldCachedSize.TempUrl : null;
                                }
                                else if (TLString.Equals(size.Type, new TLString("s"), StringComparison.OrdinalIgnoreCase))
                                {
                                    size.TempUrl = oldCachedSize != null ? oldCachedSize.TempUrl : null;
                                }
                                else
                                {
                                    size.TempUrl = oldMSize != null ? oldMSize.TempUrl : null;
                                }
                            }

                            oldMediaPhoto.Photo = newMediaPhoto.Photo;
                        }
                    }
                }
            }
        }
    }

    public class TLMessage48 : TLMessage45
    {
        public new const uint Signature = TLConstructors.TLMessage48;

        protected TLVector<TLDocumentBase> _documents;

        public TLVector<TLDocumentBase> Documents
        {
            get { return _documents; }
            set { SetField(out _documents, value, ref _customFlags, (int)MessageCustomFlags.Documents); }
        }

        public Visibility HasStickers
        {
            get
            {
                var mediaPhoto = Media as TLMessageMediaPhoto;
                if (mediaPhoto != null)
                {
                    var photo = mediaPhoto.Photo as TLPhoto56;
                    if (photo != null)
                    {
                        return photo.HasStickers ? Visibility.Visible : Visibility.Collapsed;
                    }
                }

                return Visibility.Collapsed;
            }
        }

        public bool Silent
        {
            get { return IsSet(Flags, (int)MessageFlags.Silent); }
            set { SetUnset(ref _flags, value, (int)MessageFlags.Silent); }
        }

        public bool Post
        {
            get { return IsSet(Flags, (int)MessageFlags.Post); }
            set { SetUnset(ref _flags, value, (int)MessageFlags.Post); }
        }

        protected TLMessageFwdHeader _fwdHeader;

        public TLMessageFwdHeader FwdHeader
        {
            get { return _fwdHeader; }
            set { SetField(out _fwdHeader, value, ref _flags, (int)MessageFlags.FwdFrom); }
        }

        protected TLInt _editDate;

        public TLInt EditDate
        {
            get { return _editDate; }
            set { SetField(out _editDate, value, ref _flags, (int)MessageFlags.EditDate); }
        }

        public Visibility EditDateVisibility
        {
            get
            {
                if (ViaBotId != null) return Visibility.Collapsed;

                var from = From;
                var user = from as TLUser;
                if (user != null && user.IsBot) return Visibility.Collapsed;

                return EditDate != null && EditDate.Value > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public override Visibility ReplyOrViaBotVisibility
        {
            get
            {
                var replyVisibility = ReplyVisibility;
                if (replyVisibility == Visibility.Visible) return Visibility.Visible;

                return ViaBotVisibility;
            }
        }

        public override Visibility ViaBotVisibility
        {
            get
            {
                if (ViaBotId != null && Media is TLMessageMediaContact)
                {
                    return Visibility.Collapsed;
                }

                var viaBot = ViaBot;

                return viaBot != null && !viaBot.IsDeleted ? Visibility.Visible : Visibility.Collapsed;
                //return FwdHeader == null && ViaBotId != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public override Visibility FwdViaBotVisibility
        {
            get
            {
                if (ViaBotId != null && Media is TLMessageMediaContact)
                {
                    return Visibility.Collapsed;
                }

                return FwdHeader != null && ViaBotId != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        public override Visibility FwdFromPeerVisibility
        {
            get
            {
                var peerChannel = FwdHeader != null && FwdHeader.ChannelId != null;
                if (peerChannel)
                {
                    var channelMediaGroup = Media as TLMessageMediaGroup;
                    if (channelMediaGroup != null)
                    {
                        return Visibility.Visible;
                    }

                    var channelMediaPhoto = Media as TLMessageMediaPhoto;
                    if (channelMediaPhoto != null)
                    {
                        return Visibility.Visible;
                    }

                    var channelMediaDocument = Media as TLMessageMediaDocument;
                    if (channelMediaDocument != null)
                    {
                        if (IsMusic())
                        {
                            return Visibility.Collapsed;
                        }

                        return Visibility.Visible;
                    }

                    var channelMediaVideo = Media as TLMessageMediaVideo;
                    if (channelMediaVideo != null)
                    {
                        return Visibility.Visible;
                    }

                    var channelGeo = Media as TLMessageMediaGeo;
                    if (channelGeo != null)
                    {
                        return Visibility.Visible;
                    }
                }

                var mediaGroup = Media as TLMessageMediaGroup;
                if (FwdHeader != null && mediaGroup != null)
                {
                    return Visibility.Visible;
                }

                var mediaPhoto = Media as TLMessageMediaPhoto;
                if (FwdHeader != null && mediaPhoto != null)
                {
                    return Visibility.Visible;
                }

                var mediaVideo = Media as TLMessageMediaVideo;
                if (FwdHeader != null && mediaVideo != null)
                {
                    return Visibility.Visible;
                }

                var mediaDocument = Media as TLMessageMediaDocument;
                if (FwdHeader != null && mediaDocument != null)
                {
                    if (IsMusic())
                    {
                        return Visibility.Collapsed;
                    }

                    return Visibility.Visible;
                }

                var mediaGeo = Media as TLMessageMediaGeo;
                if (FwdHeader != null && mediaGeo != null)
                {
                    return Visibility.Visible;
                }

                var emptyMedia = Media as TLMessageMediaEmpty;
                var webPageMedia = Media as TLMessageMediaWebPage;
                return FwdHeader != null && !TLString.IsNullOrEmpty(Message) && (emptyMedia != null || webPageMedia != null) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public double FwdMaxWidth
        {
            get
            {
                if (IsSticker()) return 171.0;

                if (IsVideo()) return 212.0;

                var mediaVideo = Media as TLMessageMediaVideo;
                if (mediaVideo != null) return 212.0;

                var mediaGeo = Media as TLMessageMediaGeo;
                var mediaVenue = Media as TLMessageMediaVenue;
                if (mediaGeo != null && mediaVenue == null)
#if DEBUG
                    return 302.0;//120.0;
#else
                    return 302.0;//161.0;
#endif

                return 311.0;
            }
        }

        public override TLObject FwdFrom
        {
            get
            {
                if (FwdHeader != null)
                {
                    return FwdHeader;
                }

                if (FwdFromPeer != null)
                {
                    var cacheService = InMemoryCacheService.Instance;

                    if (FwdFromPeer is TLPeerChannel)
                    {
                        return cacheService.GetChat(FwdFromPeer.Id);
                    }

                    return cacheService.GetUser(FwdFromPeer.Id);
                }

                if (FwdFromId != null)
                {
                    var cacheService = InMemoryCacheService.Instance;
                    return cacheService.GetUser(FwdFromId);
                }

                return null;
            }
            set
            {

            }
        }

        public override Visibility ShareButtonVisibility
        {
            get
            {
                if (Out.Value) return Visibility.Collapsed;

                var user = From as TLUser;
                if (user != null && (user.IsBot || ViaBotId != null))
                {
                    var entities = Entities;
                    if (entities != null)
                    {
                        var url = entities.FirstOrDefault(x => x is TLMessageEntityUrl || x is TLMessageEntityTextUrl);
                        if (url != null) return Visibility.Visible;
                    }

                    var mediaGame = Media as TLMessageMediaGame;
                    if (mediaGame != null) return Visibility.Visible;

                    var mediaPhoto = Media as TLMessageMediaPhoto;
                    if (mediaPhoto != null) return Visibility.Visible;

                    var mediaVideo = Media as TLMessageMediaVideo;
                    if (mediaVideo != null) return Visibility.Visible;

                    if (IsVideo()) return Visibility.Visible;
                }

                return Visibility.Collapsed;
            }
        }

        public override TLObject From
        {
            get
            {
                if (IsSelf())
                {
                    var fwdHeader = FwdHeader as TLMessageFwdHeader73;
                    if (fwdHeader != null)
                    {
                        var user = fwdHeader.From as TLUser;
                        if (user == null || !user.IsSelf)
                        {
                            return fwdHeader.From;
                        }
                    }
                }

                return base.From;
            }
        }

        public override void Edit(TLMessageBase messageBase)
        {
            base.Edit(messageBase);

            var message = messageBase as TLMessage48;
            if (message != null)
            {
                EditDate = message.EditDate;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = IsSet(Flags, (int)MessageFlags.FromId)
                ? GetObject<TLInt>(bytes, ref position)
                : new TLInt(-1);
            ToId = GetObject<TLPeerBase>(bytes, ref position);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdHeader = GetObject<TLMessageFwdHeader>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.ViaBotId))
            {
                _viaBotId = GetObject<TLInt>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(bytes, ref position);
            }

            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);

            _media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLMessageMediaBase>(bytes, ref position)
                : new TLMessageMediaEmpty();

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);
            }

            _views = IsSet(Flags, (int)MessageFlags.Views)
                ? GetObject<TLInt>(bytes, ref position)
                : new TLInt(0);

            EditDate = IsSet(Flags, (int)MessageFlags.EditDate)
                ? GetObject<TLInt>(bytes, ref position)
                : new TLInt(0);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdHeader = GetObject<TLMessageFwdHeader>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.ViaBotId))
            {
                _viaBotId = GetObject<TLInt>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(input);
            }

            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            _media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLMessageMediaBase>(input)
                : new TLMessageMediaEmpty();

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.Views))
            {
                Views = GetObject<TLInt>(input);
            }
            else
            {
                Views = new TLInt(0);
            }

            if (IsSet(Flags, (int)MessageFlags.EditDate))
            {
                EditDate = GetObject<TLInt>(input);
            }
            else
            {
                EditDate = new TLInt(0);
            }

            CustomFlags = GetNullableObject<TLLong>(input);

            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            var status = GetObject<TLInt>(input);
            _status = (MessageStatus)status.Value;

            if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdMessageId))
            {
                _fwdMessageId = GetObject<TLInt>(input);
            }

            if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdFromChannelPeer))
            {
                _fwdFromChannelPeer = GetObject<TLInputPeerBase>(input);
            }

            if (IsSet(CustomFlags, (int)MessageCustomFlags.BotInlineResult))
            {
                _inlineBotResultQueryId = GetObject<TLLong>(input);
                _inlineBotResultId = GetObject<TLString>(input);
            }

            if (IsSet(CustomFlags, (int)MessageCustomFlags.Documents))
            {
                _documents = GetObject<TLVector<TLDocumentBase>>(input);
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            try
            {

                Flags.ToStream(output);
                Id = Id ?? new TLInt(0);
                output.Write(Id.ToBytes());
                output.Write(FromId.ToBytes());
                ToId.ToStream(output);

                if (IsSet(Flags, (int)MessageFlags.FwdFrom))
                {
                    FwdHeader.ToStream(output);
                    //FwdFromPeer.ToStream(output);
                    //FwdDate.ToStream(output);
                }

                if (IsSet(Flags, (int)MessageFlags.ViaBotId))
                {
                    _viaBotId.ToStream(output);
                }

                if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
                {
                    ReplyToMsgId.ToStream(output);
                }

                output.Write(Date.ToBytes());
                Message.ToStream(output);

                if (IsSet(Flags, (int)MessageFlags.Media))
                {
                    _media.ToStream(output);
                }

                if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
                {
                    ReplyMarkup.ToStream(output);
                }
                if (IsSet(Flags, (int)MessageFlags.Entities))
                {
                    Entities.ToStream(output);
                }
                if (IsSet(Flags, (int)MessageFlags.Views))
                {
                    if (Views == null)
                    {
                        var logString = string.Format("TLMessage48.ToStream id={0} flags={1} fwd_from_peer={2} fwd_date={3} reply_to_msg_id={4} media={5} reply_markup={6} entities={7} views={8} from_id={9} edit_date={10} fwd_header={11}", Index, MessageFlagsString(Flags), FwdFromPeer, FwdDate, ReplyToMsgId, Media, ReplyMarkup, Entities, Views, FromId, EditDate, FwdHeader != null);
                        Log.Write(logString);
                    }

                    Views = Views ?? new TLInt(0);
                    Views.ToStream(output);
                }
                if (IsSet(Flags, (int)MessageFlags.EditDate))
                {
                    EditDate = EditDate ?? new TLInt(0);
                    EditDate.ToStream(output);
                }

                CustomFlags.NullableToStream(output);

                RandomId = RandomId ?? new TLLong(0);
                RandomId.ToStream(output);
                var status = new TLInt((int)Status);
                output.Write(status.ToBytes());

                if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdMessageId))
                {
                    _fwdMessageId.ToStream(output);
                }

                if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdFromChannelPeer))
                {
                    _fwdFromChannelPeer.ToStream(output);
                }

                if (IsSet(CustomFlags, (int)MessageCustomFlags.BotInlineResult))
                {
                    _inlineBotResultQueryId.ToStream(output);
                    _inlineBotResultId.ToStream(output);
                }

                if (IsSet(CustomFlags, (int)MessageCustomFlags.Documents))
                {
                    _documents.ToStream(output);
                }
            }
            catch (Exception ex)
            {
                var logString = string.Format("TLMessage48.ToStream id={0} flags={1} fwd_from_peer={2} fwd_date={3} reply_to_msg_id={4} media={5} reply_markup={6} entities={7} views={8} from_id={9} edit_date={10} fwd_header={11}", Index, MessageFlagsString(Flags), FwdFromPeer, FwdDate, ReplyToMsgId, Media, ReplyMarkup, Entities, Views, FromId, EditDate, FwdHeader != null);

                TLUtils.WriteException(logString, ex);
            }
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = message as TLMessage48;
            if (m != null)
            {
                FwdHeader = m.FwdHeader;

                if (m.EditDate != null)
                {
                    EditDate = m.EditDate;
                }
            }
        }
    }

    public class TLMessage45 : TLMessage40
    {
        private string _author;

        public virtual string Author
        {
            get
            {
                if (_author != null) return _author;

                if (!(ToId is TLPeerChannel)) return null;
                if (FromId == null || FromId.Value < 0) return null;

                var cacheService = InMemoryCacheService.Instance;
                var user = cacheService.GetUser(FromId);
                _author = user != null ? user.FullName2 : string.Empty;

                return _author;
            }
        }

        public virtual Visibility AuthorVisibility
        {
            get { return ToId is TLPeerChannel && FromId != null && FromId.Value >= 0 && !IsMusic() ? Visibility.Visible : Visibility.Collapsed; }
        }

        public new const uint Signature = TLConstructors.TLMessage45;

        protected TLInt _viaBotId;

        public TLInt ViaBotId
        {
            get { return _viaBotId; }
            set
            {
                if (value != null)
                {
                    Set(ref _flags, (int)MessageFlags.ViaBotId);
                    _viaBotId = value;
                }
                else
                {
                    Unset(ref _flags, (int)MessageFlags.ViaBotId);
                    _viaBotId = null;
                }
            }
        }

        private TLUserBase _viaBot;

        public override TLUserBase ViaBot
        {
            get
            {
                if (_viaBot != null) return _viaBot;
                if (ViaBotId == null) return null;

                var cacheService = InMemoryCacheService.Instance;
                _viaBot = cacheService.GetUser(ViaBotId);

                return _viaBot;
            }
        }

        public override Visibility ViaBotVisibility
        {
            get { return FwdFromPeer == null && ViaBotId != null ? Visibility.Visible : Visibility.Collapsed; }
        }

        public override Visibility FwdViaBotVisibility
        {
            get { return FwdFromPeer != null && ViaBotId != null ? Visibility.Visible : Visibility.Collapsed; }
        }

        protected TLLong _inlineBotResultQueryId;

        public TLLong InlineBotResultQueryId
        {
            get { return _inlineBotResultQueryId; }
            set
            {
                if (value != null)
                {
                    Set(ref _customFlags, (int)MessageCustomFlags.BotInlineResult);
                    _inlineBotResultQueryId = value;
                }
                else
                {
                    Unset(ref _customFlags, (int)MessageCustomFlags.BotInlineResult);
                    _inlineBotResultQueryId = null;
                }
            }
        }

        protected TLString _inlineBotResultId;

        public TLString InlineBotResultId
        {
            get { return _inlineBotResultId; }
            set
            {
                if (value != null)
                {
                    Set(ref _customFlags, (int)MessageCustomFlags.BotInlineResult);
                    _inlineBotResultId = value;
                }
                else
                {
                    Unset(ref _customFlags, (int)MessageCustomFlags.BotInlineResult);
                    _inlineBotResultId = null;
                }
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = IsSet(Flags, (int)MessageFlags.FromId)
                ? GetObject<TLInt>(bytes, ref position)
                : new TLInt(-1);
            ToId = GetObject<TLPeerBase>(bytes, ref position);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromPeer = GetObject<TLPeerBase>(bytes, ref position);
                FwdDate = GetObject<TLInt>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.ViaBotId))
            {
                _viaBotId = GetObject<TLInt>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(bytes, ref position);
            }

            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);

            _media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLMessageMediaBase>(bytes, ref position)
                : new TLMessageMediaEmpty();

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);
            }

            _views = IsSet(Flags, (int)MessageFlags.Views)
                ? GetObject<TLInt>(bytes, ref position)
                : new TLInt(0);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromPeer = GetObject<TLPeerBase>(input);
                FwdDate = GetObject<TLInt>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.ViaBotId))
            {
                _viaBotId = GetObject<TLInt>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(input);
            }

            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);

            _media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLMessageMediaBase>(input)
                : new TLMessageMediaEmpty();

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.Views))
            {
                Views = GetObject<TLInt>(input);
            }
            else
            {
                Views = new TLInt(0);
            }

            CustomFlags = GetNullableObject<TLLong>(input);

            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            var status = GetObject<TLInt>(input);
            _status = (MessageStatus)status.Value;

            if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdMessageId))
            {
                _fwdMessageId = GetObject<TLInt>(input);
            }

            if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdFromChannelPeer))
            {
                _fwdFromChannelPeer = GetObject<TLInputPeerBase>(input);
            }

            if (IsSet(CustomFlags, (int)MessageCustomFlags.BotInlineResult))
            {
                _inlineBotResultQueryId = GetObject<TLLong>(input);
                _inlineBotResultId = GetObject<TLString>(input);
            }
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            try
            {

                Flags.ToStream(output);
                Id = Id ?? new TLInt(0);
                output.Write(Id.ToBytes());
                output.Write(FromId.ToBytes());
                ToId.ToStream(output);

                if (IsSet(Flags, (int)MessageFlags.FwdFrom))
                {
                    FwdFromPeer.ToStream(output);
                    FwdDate.ToStream(output);
                }

                if (IsSet(Flags, (int)MessageFlags.ViaBotId))
                {
                    _viaBotId.ToStream(output);
                }

                if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
                {
                    ReplyToMsgId.ToStream(output);
                }

                output.Write(Date.ToBytes());
                Message.ToStream(output);

                if (IsSet(Flags, (int)MessageFlags.Media))
                {
                    _media.ToStream(output);
                }

                if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
                {
                    ReplyMarkup.ToStream(output);
                }
                if (IsSet(Flags, (int)MessageFlags.Entities))
                {
                    Entities.ToStream(output);
                }
                if (IsSet(Flags, (int)MessageFlags.Views))
                {
                    if (Views == null)
                    {
                        var logString = string.Format("TLMessage40.ToStream id={0} flags={1} fwd_from_peer={2} fwd_date={3} reply_to_msg_id={4} media={5} reply_markup={6} entities={7} views={8} from_id={9}", Index, MessageFlagsString(Flags), FwdFromPeer, FwdDate, ReplyToMsgId, Media, ReplyMarkup, Entities, Views, FromId);
                        Log.Write(logString);
                    }

                    Views = Views ?? new TLInt(0);
                    Views.ToStream(output);
                }

                CustomFlags.NullableToStream(output);

                RandomId = RandomId ?? new TLLong(0);
                RandomId.ToStream(output);
                var status = new TLInt((int)Status);
                output.Write(status.ToBytes());

                if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdMessageId))
                {
                    _fwdMessageId.ToStream(output);
                }

                if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdFromChannelPeer))
                {
                    _fwdFromChannelPeer.ToStream(output);
                }

                if (IsSet(CustomFlags, (int)MessageCustomFlags.BotInlineResult))
                {
                    _inlineBotResultQueryId.ToStream(output);
                    _inlineBotResultId.ToStream(output);
                }
            }
            catch (Exception ex)
            {
                var logString = string.Format("TLMessage40.ToStream id={0} flags={1} fwd_from_peer={2} fwd_date={3} reply_to_msg_id={4} media={5} reply_markup={6} entities={7} views={8} from_id={9}", Index, MessageFlagsString(Flags), FwdFromPeer, FwdDate, ReplyToMsgId, Media, ReplyMarkup, Entities, Views, FromId);

                TLUtils.WriteException(logString, ex);
            }
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = message as TLMessage45;
            if (m != null)
            {
                if (m.ViaBotId != null)
                {
                    ViaBotId = m.ViaBotId;
                }

                if (m.InlineBotResultQueryId != null)
                {
                    InlineBotResultQueryId = m.InlineBotResultQueryId;
                }

                if (m.InlineBotResultId != null)
                {
                    InlineBotResultId = m.InlineBotResultId;
                }
            }
        }
    }

    public class TLMessage40 : TLMessage36
    {
        public new const uint Signature = TLConstructors.TLMessage40;

        public TLPeerBase FwdFromPeer { get; set; }

        protected TLInputPeerBase _fwdFromChannelPeer;

        public TLInputPeerBase FwdFromChannelPeer
        {
            get { return _fwdFromChannelPeer; }
            set
            {
                if (value != null)
                {
                    Set(ref _customFlags, (int)MessageCustomFlags.FwdFromChannelPeer);
                    _fwdFromChannelPeer = value;
                }
                else
                {
                    Unset(ref _customFlags, (int)MessageCustomFlags.FwdFromChannelPeer);
                    _fwdFromChannelPeer = null;
                }
            }
        }

        protected TLInt _fwdMessageId;

        public TLInt FwdMessageId
        {
            get { return _fwdMessageId; }
            set
            {
                if (value != null)
                {
                    Set(ref _customFlags, (int)MessageCustomFlags.FwdMessageId);
                    _fwdMessageId = value;
                }
                else
                {
                    Unset(ref _customFlags, (int)MessageCustomFlags.FwdMessageId);
                    _fwdMessageId = null;
                }
            }
        }

        public virtual Visibility FwdFromPeerVisibility
        {
            get
            {
                var peerChannel = FwdFromPeer as TLPeerChannel;
                if (peerChannel != null)
                {
                    var channelMediaPhoto = Media as TLMessageMediaPhoto;
                    if (channelMediaPhoto != null)
                    {
                        return Visibility.Visible;
                    }

                    var channelMediaDocument = Media as TLMessageMediaDocument;
                    if (channelMediaDocument != null && IsVideo(channelMediaDocument.Document))
                    {
                        return Visibility.Visible;
                    }

                    var channelMediaVideo = Media as TLMessageMediaVideo;
                    if (channelMediaVideo != null)
                    {
                        return Visibility.Visible;
                    }
                }

                var mediaPhoto = Media as TLMessageMediaPhoto;
                if (FwdFromPeer != null && mediaPhoto != null)
                {
                    return Visibility.Visible;
                }

                var mediaVideo = Media as TLMessageMediaVideo;
                if (FwdFromPeer != null && mediaVideo != null)
                {
                    return Visibility.Visible;
                }

                var mediaDocument = Media as TLMessageMediaDocument;
                if (FwdFromPeer != null && mediaDocument != null)
                {
                    return Visibility.Visible;
                }

                var emptyMedia = Media as TLMessageMediaEmpty;
                var webPageMedia = Media as TLMessageMediaWebPage;
                return FwdFromPeer != null && !TLString.IsNullOrEmpty(Message) && (emptyMedia != null || webPageMedia != null) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public TLMessageBase Group { get; set; }

        public void SetFromId()
        {
            Set(ref _flags, (int)MessageFlags.FromId);
        }

        public void SetMedia()
        {
            Set(ref _flags, (int)MessageFlags.Media);
        }

        public override TLObject FwdFrom
        {
            get
            {
                if (FwdFromId != null)
                {
                    var cacheService = InMemoryCacheService.Instance;
                    return cacheService.GetUser(FwdFromId);
                }

                if (FwdFromPeer != null)
                {
                    var cacheService = InMemoryCacheService.Instance;

                    if (FwdFromPeer is TLPeerChannel)
                    {
                        return cacheService.GetChat(FwdFromPeer.Id);
                    }

                    return cacheService.GetUser(FwdFromPeer.Id);
                }

                return null;
            }
            set
            {

            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = IsSet(Flags, (int)MessageFlags.FromId)
                ? GetObject<TLInt>(bytes, ref position)
                : new TLInt(-1);
            ToId = GetObject<TLPeerBase>(bytes, ref position);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromPeer = GetObject<TLPeerBase>(bytes, ref position);
                FwdDate = GetObject<TLInt>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(bytes, ref position);
            }

            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);

            _media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLMessageMediaBase>(bytes, ref position)
                : new TLMessageMediaEmpty();

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);
            }

            _views = IsSet(Flags, (int)MessageFlags.Views)
                ? GetObject<TLInt>(bytes, ref position)
                : new TLInt(0);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromPeer = GetObject<TLPeerBase>(input);
                FwdDate = GetObject<TLInt>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(input);
            }

            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);

            _media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLMessageMediaBase>(input)
                : new TLMessageMediaEmpty();

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.Views))
            {
                Views = GetObject<TLInt>(input);
            }
            else
            {
                Views = new TLInt(0);
            }

            CustomFlags = GetNullableObject<TLLong>(input);

            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            var status = GetObject<TLInt>(input);
            _status = (MessageStatus)status.Value;

            if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdMessageId))
            {
                _fwdMessageId = GetObject<TLInt>(input);
            }

            if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdFromChannelPeer))
            {
                _fwdFromChannelPeer = GetObject<TLInputPeerBase>(input);
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            try
            {

                Flags.ToStream(output);
                Id = Id ?? new TLInt(0);
                output.Write(Id.ToBytes());
                output.Write(FromId.ToBytes());
                ToId.ToStream(output);

                if (IsSet(Flags, (int)MessageFlags.FwdFrom))
                {
                    FwdFromPeer.ToStream(output);
                    FwdDate.ToStream(output);
                }

                if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
                {
                    ReplyToMsgId.ToStream(output);
                }

                output.Write(Date.ToBytes());
                Message.ToStream(output);

                if (IsSet(Flags, (int)MessageFlags.Media))
                {
                    _media.ToStream(output);
                }

                if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
                {
                    ReplyMarkup.ToStream(output);
                }
                if (IsSet(Flags, (int)MessageFlags.Entities))
                {
                    Entities.ToStream(output);
                }
                if (IsSet(Flags, (int)MessageFlags.Views))
                {
                    if (Views == null)
                    {
                        var logString = string.Format("TLMessage40.ToStream id={0} flags={1} fwd_from_peer={2} fwd_date={3} reply_to_msg_id={4} media={5} reply_markup={6} entities={7} views={8} from_id={9}", Index, MessageFlagsString(Flags), FwdFromPeer, FwdDate, ReplyToMsgId, Media, ReplyMarkup, Entities, Views, FromId);
                        Log.Write(logString);
                    }

                    Views = Views ?? new TLInt(0);
                    Views.ToStream(output);
                }

                CustomFlags.NullableToStream(output);

                RandomId = RandomId ?? new TLLong(0);
                RandomId.ToStream(output);
                var status = new TLInt((int)Status);
                output.Write(status.ToBytes());

                if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdMessageId))
                {
                    _fwdMessageId.ToStream(output);
                }

                if (IsSet(CustomFlags, (int)MessageCustomFlags.FwdFromChannelPeer))
                {
                    _fwdFromChannelPeer.ToStream(output);
                }
            }
            catch (Exception ex)
            {
                var logString = string.Format("TLMessage40.ToStream id={0} flags={1} fwd_from_peer={2} fwd_date={3} reply_to_msg_id={4} media={5} reply_markup={6} entities={7} views={8} from_id={9}", Index, MessageFlagsString(Flags), FwdFromPeer, FwdDate, ReplyToMsgId, Media, ReplyMarkup, Entities, Views, FromId);

                TLUtils.WriteException(logString, ex);
            }
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = message as TLMessage40;
            if (m != null)
            {
                FwdFromPeer = m.FwdFromPeer;
                if (m.FwdMessageId != null) FwdMessageId = m.FwdMessageId;
                if (m.FwdFromChannelPeer != null) FwdFromChannelPeer = m.FwdFromChannelPeer;
                if (m.Views != null)
                {
                    var currentViews = Views != null ? Views.Value : 0;
                    if (currentViews < m.Views.Value)
                    {
                        Views = m.Views;
                    }
                }
            }
        }
    }

    public class TLMessage36 : TLMessage34
    {
        public new const uint Signature = TLConstructors.TLMessage36;

        protected TLInt _views;

        public TLInt Views
        {
            get { return _views; }
            set
            {
                if (value != null)
                {
                    if (_views == null || _views.Value < value.Value)
                    {
                        Set(ref _flags, (int)MessageFlags.Views);
                        _views = value;
                    }
                }
            }
        }

        public Visibility ViewsVisibility
        {
            get
            {
                var message40 = this as TLMessage40;
                if (message40 != null)
                {
                    return Views != null && Views.Value > 0 ? Visibility.Visible : Visibility.Collapsed;
                }

                return Visibility.Collapsed;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject<TLInt>(bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId = GetObject<TLInt>(bytes, ref position);
                FwdDate = GetObject<TLInt>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(bytes, ref position);
            }

            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);

            _media = IsSet(Flags, (int)MessageFlags.Media)
                ? GetObject<TLMessageMediaBase>(bytes, ref position)
                : new TLMessageMediaEmpty();

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);
            }

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId = GetObject<TLInt>(input);
                FwdDate = GetObject<TLInt>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(input);
            }

            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            _media = GetObject<TLMessageMediaBase>(input);

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(input);
            }

            CustomFlags = GetNullableObject<TLLong>(input);

            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            var status = GetObject<TLInt>(input);
            _status = (MessageStatus)status.Value;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Id = Id ?? new TLInt(0);
            output.Write(Id.ToBytes());
            output.Write(FromId.ToBytes());
            ToId.ToStream(output);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId.ToStream(output);
                FwdDate.ToStream(output);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId.ToStream(output);
            }

            output.Write(Date.ToBytes());
            Message.ToStream(output);
            _media.ToStream(output);

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup.ToStream(output);
            }
            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities.ToStream(output);
            }

            CustomFlags.NullableToStream(output);

            RandomId = RandomId ?? new TLLong(0);
            RandomId.ToStream(output);
            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());
        }
    }

    public class TLMessage34 : TLMessage31
    {
        public new const uint Signature = TLConstructors.TLMessage34;

        protected TLVector<TLMessageEntityBase> _entities;

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set { SetField(out _entities, value, ref _flags, (int)MessageFlags.Entities); }
        }

        public override void Edit(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessage34;
            if (message != null)
            {
                Message = message.Message;
                Entities = message.Entities;
                ReplyMarkup = message.ReplyMarkup;

                var oldGeoLive = Media as TLMessageMediaGeoLive;
                var newGeoLive = message.Media as TLMessageMediaGeoLive;
                if (oldGeoLive != null && newGeoLive != null)
                {
                    oldGeoLive.Geo = newGeoLive.Geo;
                    oldGeoLive.Period = newGeoLive.Period;
                }

                var oldInvoice = Media as TLMessageMediaInvoice;
                var newInvoice = message.Media as TLMessageMediaInvoice;
                if (oldInvoice != null && newInvoice != null && newInvoice.ReceiptMsgId != null)
                {
                    oldInvoice.ReceiptMsgId = newInvoice.ReceiptMsgId;
                }

                var oldWebPage = Media as TLMessageMediaWebPage;
                var newWebPage = message.Media as TLMessageMediaWebPage;
                if ((oldWebPage == null && newWebPage != null)
                    || (oldWebPage != null && newWebPage == null)
                    || (oldWebPage != null && newWebPage != null && oldWebPage.WebPage.Id.Value != newWebPage.WebPage.Id.Value))
                {
                    _media = (TLMessageMediaBase)newWebPage ?? new TLMessageMediaEmpty();
                }

                var oldGame = Media as TLMessageMediaGame;
                var newGame = message.Media as TLMessageMediaGame;
                if (oldGame != null && newGame != null)
                {
                    oldGame.Message = message.Message;
                }

                var mediaCaption = message.Media as IMediaCaption;
                var cachedMediaCaption = Media as IMediaCaption;
                if (cachedMediaCaption != null && mediaCaption != null)
                {
                    cachedMediaCaption.Caption = mediaCaption.Caption;
                }
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject<TLInt>(bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId = GetObject<TLInt>(bytes, ref position);
                FwdDate = GetObject<TLInt>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(bytes, ref position);
            }

            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _media = GetObject<TLMessageMediaBase>(bytes, ref position);

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);
            }

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId = GetObject<TLInt>(input);
                FwdDate = GetObject<TLInt>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(input);
            }

            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            _media = GetObject<TLMessageMediaBase>(input);

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities = GetObject<TLVector<TLMessageEntityBase>>(input);
            }

            CustomFlags = GetNullableObject<TLLong>(input);

            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            var status = GetObject<TLInt>(input);
            _status = (MessageStatus)status.Value;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Id = Id ?? new TLInt(0);
            output.Write(Id.ToBytes());
            output.Write(FromId.ToBytes());
            ToId.ToStream(output);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId.ToStream(output);
                FwdDate.ToStream(output);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId.ToStream(output);
            }

            output.Write(Date.ToBytes());
            Message.ToStream(output);
            _media.ToStream(output);

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup.ToStream(output);
            }
            if (IsSet(Flags, (int)MessageFlags.Entities))
            {
                Entities.ToStream(output);
            }

            CustomFlags.NullableToStream(output);

            RandomId = RandomId ?? new TLLong(0);
            RandomId.ToStream(output);
            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = message as TLMessage34;
            if (m != null)
            {
                if (m.Entities != null)
                {
                    Entities = m.Entities;
                }
            }
        }
    }

    public class TLMessage31 : TLMessage25
    {
        public new const uint Signature = TLConstructors.TLMessage31;

        protected TLReplyKeyboardBase _replyMarkup;

        public TLReplyKeyboardBase ReplyMarkup
        {
            get { return _replyMarkup; }
            set { SetField(out _replyMarkup, value, ref _flags, (int)MessageFlags.ReplyMarkup); }
        }

        protected TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject<TLInt>(bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId = GetObject<TLInt>(bytes, ref position);
                FwdDate = GetObject<TLInt>(bytes, ref position);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(bytes, ref position);
            }

            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _media = GetObject<TLMessageMediaBase>(bytes, ref position);

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(bytes, ref position);
            }

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId = GetObject<TLInt>(input);
                FwdDate = GetObject<TLInt>(input);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(input);
            }

            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            _media = GetObject<TLMessageMediaBase>(input);

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup = GetObject<TLReplyKeyboardBase>(input);
            }

            CustomFlags = GetNullableObject<TLLong>(input);

            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            var status = GetObject<TLInt>(input);
            _status = (MessageStatus)status.Value;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Id = Id ?? new TLInt(0);
            output.Write(Id.ToBytes());
            output.Write(FromId.ToBytes());
            ToId.ToStream(output);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId.ToStream(output);
                FwdDate.ToStream(output);
            }

            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId.ToStream(output);
            }

            output.Write(Date.ToBytes());
            Message.ToStream(output);
            _media.ToStream(output);

            if (IsSet(Flags, (int)MessageFlags.ReplyMarkup))
            {
                ReplyMarkup.ToStream(output);
            }

            CustomFlags.NullableToStream(output);

            RandomId = RandomId ?? new TLLong(0);
            RandomId.ToStream(output);
            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = message as TLMessage31;
            if (m != null)
            {
                if (m.ReplyMarkup != null)
                {
                    var oldCustomFlags = ReplyMarkup != null ? ReplyMarkup.CustomFlags : null;
                    ReplyMarkup = m.ReplyMarkup;
                    ReplyMarkup.CustomFlags = oldCustomFlags;
                }

                if (m.CustomFlags != null)
                {
                    CustomFlags = m.CustomFlags;
                }
            }
        }
    }

    public class TLMessage25 : TLMessage17, IReplyToMsgId
    {
        public new const uint Signature = TLConstructors.TLMessage25;

        public TLInt FwdFromId { get; set; }

        public TLInt FwdDate { get; set; }

        protected TLInt _replyToMsgId;

        public TLInt ReplyToMsgId
        {
            get { return _replyToMsgId; }
            set { SetField(out _replyToMsgId, value, ref _flags, (int)MessageFlags.ReplyToMsgId); }
        }

        public override ReplyInfo ReplyInfo
        {
            get { return ReplyToMsgId != null ? new ReplyInfo { ReplyToMsgId = ReplyToMsgId, Reply = Reply } : null; }
        }

        public override Visibility ReplyVisibility
        {
            get { return ReplyToMsgId != null && ReplyToMsgId.Value != 0 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public void SetFwd()
        {
            Set(ref _flags, (int)MessageFlags.FwdFrom);
        }

        public void SetReply()
        {
            Set(ref _flags, (int)MessageFlags.ReplyToMsgId);
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

        public override TLObject FwdFrom
        {
            get
            {
                if (FwdFromId == null) return null;

                var cacheService = InMemoryCacheService.Instance;
                return cacheService.GetUser(FwdFromId);
            }
            set
            {

            }
        }

        public override string ToString()
        {
            var messageString = Message != null ? Message.ToString() : string.Empty;

            var mediaString = Media.GetType().Name;
            //var mediaPhoto = Media as TLMessageMediaPhoto;
            //if (mediaPhoto != null)
            //{
            //    mediaString = mediaPhoto.ToString() + " ";
            //}
            var str = Media == null || Media is TLMessageMediaEmpty
                ? " Msg=" + messageString.Substring(0, Math.Min(messageString.Length, 5))
                : " Media=" + mediaString;

            return base.ToString() + string.Format(" Flags={0}" + str, MessageFlagsString(Flags));
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject<TLInt>(bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId = GetObject<TLInt>(bytes, ref position);
                FwdDate = GetObject<TLInt>(bytes, ref position);
            }
            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(bytes, ref position);
            }

            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _media = GetObject<TLMessageMediaBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId = GetObject<TLInt>(input);
                FwdDate = GetObject<TLInt>(input);
            }
            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(input);
            }

            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            _media = GetObject<TLMessageMediaBase>(input);

            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            var status = GetObject<TLInt>(input);
            _status = (MessageStatus)status.Value;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Id = Id ?? new TLInt(0);
            output.Write(Id.ToBytes());
            output.Write(FromId.ToBytes());
            ToId.ToStream(output);

            if (IsSet(Flags, (int)MessageFlags.FwdFrom))
            {
                FwdFromId.ToStream(output);
                FwdDate.ToStream(output);
            }
            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId.ToStream(output);
            }

            output.Write(Date.ToBytes());
            Message.ToStream(output);
            _media.ToStream(output);

            RandomId = RandomId ?? new TLLong(0);
            RandomId.ToStream(output);
            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = message as TLMessage25;
            if (m != null)
            {
                FwdFromId = m.FwdFromId;
                FwdDate = m.FwdDate;
                ReplyToMsgId = m.ReplyToMsgId;

                if (m.Reply != null)
                {
                    Reply = m.Reply;
                }
            }
        }
    }

    public class TLMessage17 : TLMessage
    {
        public new const uint Signature = TLConstructors.TLMessage17;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLBool Out
        {
            get { return new TLBool(IsSet(_flags, (int)MessageFlags.Out)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)MessageFlags.Out);
                }
            }
        }

        public override void SetUnread(TLBool value)
        {
            Unread = value;
        }

        public override void SetUnreadSilent(TLBool value)
        {
            if (value != null)
            {
                SetUnset(ref _flags, value.Value, (int)MessageFlags.Unread);
            }
        }

        public override TLBool Unread
        {
            get { return new TLBool(IsSet(_flags, (int)MessageFlags.Unread)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)MessageFlags.Unread);
                    NotifyOfPropertyChange(() => Status);
                }
            }
        }

        public bool IsMention
        {
            get { return IsSet(_flags, (int)MessageFlags.Mentioned); }
            set { SetUnset(ref _flags, value, (int)MessageFlags.Mentioned); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject<TLInt>(bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);
            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _media = GetObject<TLMessageMediaBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);
            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            _media = GetObject<TLMessageMediaBase>(input);

            var randomId = GetObject<TLLong>(input);
            if (randomId.Value != 0)
            {
                RandomId = randomId;
            }
            var status = GetObject<TLInt>(input);
            _status = (MessageStatus)status.Value;

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Id = Id ?? new TLInt(0);
            output.Write(Id.ToBytes());
            output.Write(FromId.ToBytes());
            ToId.ToStream(output);
            output.Write(Date.ToBytes());
            Message.ToStream(output);
            _media.ToStream(output);

            RandomId = RandomId ?? new TLLong(0);
            RandomId.ToStream(output);
            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = (TLMessage17)message;
            var fixUnread = false;
            if (!Unread.Value && m.Unread.Value)
            {
                fixUnread = true;
#if DEBUG
                var builder = new StringBuilder();
#if  WINDOWS_PHONE
                var stackTrace = new StackTrace();
                var frames = stackTrace.GetFrames();
                foreach (var r in frames)
                {
                    builder.AppendLine(string.Format("Method: {0}", r.GetMethod()));
                }
#endif
                Telegram.Api.Helpers.Execute.ShowDebugMessage("Set read message as unread\ncurrent=" + this + "\nnew=" + message + "\n\n" + builder.ToString());
#endif
            }

            Flags = m.Flags;

            if (fixUnread)
            {
                SetUnreadSilent(TLBool.False);
            }
        }
    }

    public class TLMessage : TLMessageCommon, IMessage
    {
        public const uint Signature = TLConstructors.TLMessage;

        public TLString Message { get; set; }

        public TLMessageMediaBase _media;

        public TLMessageMediaBase Media
        {
            get { return _media; }
            set { SetField(ref _media, value, () => Media); }
        }

        public override Visibility SelectionVisibility
        {
            get { return IsExpired() ? Visibility.Collapsed : Visibility.Visible; }
        }

        public override int MediaSize
        {
            get { return Media.MediaSize; }
        }

        public override Visibility MediaSizeVisibility
        {
            get { return _media is TLMessageMediaVideo ? Visibility.Visible : Visibility.Collapsed; }
        }

        public override bool IsMusic()
        {
            var mediaDocument = _media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                return IsMusic(mediaDocument.Document);
            }

            return false;
        }

        public override bool IsVoice()
        {
            var mediaDocument = _media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                return IsVoice(mediaDocument.Document);
            }

            return false;
        }

        public override bool IsRoundVideo()
        {
            var mediaDocument = _media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                return IsRoundVideo(mediaDocument.Document);
            }

            return false;
        }

        public override bool IsVideo()
        {
            var mediaDocument = _media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                return IsVideo(mediaDocument.Document);
            }

            return false;
        }

        public override bool IsSticker()
        {
            var mediaDocument = _media as TLMessageMediaDocument;
            if (mediaDocument != null)
            {
                return IsSticker(mediaDocument.Document);
            }

            return false;
        }

        public override bool IsExpired()
        {
            return IsExpired(_media);
        }

        public override bool HasTTL()
        {
            return HasTTL(_media);
        }

        public override bool IsGif()
        {
            var mediaDocument = _media as IMediaGif;
            if (mediaDocument != null)
            {
                return IsGif(mediaDocument.Document);
            }

            return false;
        }

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

        public override string ToString()
        {
            return base.ToString();
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            base.FromBytes(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _media = GetObject<TLMessageMediaBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            base.FromStream(input);
            Message = GetObject<TLString>(input);
            _media = GetObject<TLMessageMediaBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id = Id ?? new TLInt(0);
            output.Write(Id.ToBytes());
            base.ToStream(output);
            Message.ToStream(output);
            _media.ToStream(output);
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = (TLMessage)message;
            Message = m.Message;
            var oldMedia = Media;
            var newMedia = m.Media;
            if (oldMedia.GetType() != newMedia.GetType())
            {
                _media = m.Media;
            }
            else
            {
                var oldInvoice = oldMedia as TLMessageMediaInvoice;
                var newInvoice = newMedia as TLMessageMediaInvoice;
                if (oldInvoice != null && newInvoice != null && newInvoice.ReceiptMsgId != null)
                {
                    oldInvoice.ReceiptMsgId = newInvoice.ReceiptMsgId;

                    return;
                }

                var oldMediaGame = oldMedia as TLMessageMediaGame;
                var newMediaGame = newMedia as TLMessageMediaGame;
                if (oldMediaGame != null && newMediaGame != null)
                {
                    newMediaGame.Message = m.Message;
                    if (oldMediaGame.Game.GetType() != newMediaGame.Game.GetType())
                    {
                        _media = m.Media;
                    }
                    else
                    {
                        var oldGame = oldMediaGame.Game;
                        var newGame = newMediaGame.Game;
                        if (oldGame != null
                            && newGame != null
                            && (oldGame.Id.Value != newGame.Id.Value))
                        {
                            newMediaGame.SourceMessage = this;
                            _media = newMediaGame;
                        }
                        else
                        {
                            oldMediaGame.Message = m.Message;
                        }
                    }

                    return;
                }

                var oldMediaWebPage = oldMedia as TLMessageMediaWebPage;
                var newMediaWebPage = newMedia as TLMessageMediaWebPage;
                if (oldMediaWebPage != null && newMediaWebPage != null)
                {
                    if (oldMediaWebPage.WebPage.GetType() != newMediaWebPage.WebPage.GetType())
                    {
                        _media = m.Media;
                    }
                    else
                    {
                        var oldWebPage = oldMediaWebPage.WebPage as TLWebPage35;
                        var newWebPage = newMediaWebPage.WebPage as TLWebPage35;
                        if (oldWebPage != null
                            && newWebPage != null
                            && (oldWebPage.Id.Value != newWebPage.Id.Value))
                        {
                            _media = m.Media;
                        }
                    }

                    return;
                }

                var oldMediaDocument = oldMedia as TLMessageMediaDocument;
                var newMediaDocument = newMedia as TLMessageMediaDocument;
                if (oldMediaDocument != null && newMediaDocument != null)
                {
                    if (oldMediaDocument.Document.GetType() != newMediaDocument.Document.GetType())
                    {
                        _media = m.Media;
                    }
                    else
                    {
                        var oldDocument = oldMediaDocument.Document as TLDocument;
                        var newDocument = newMediaDocument.Document as TLDocument;
                        if (oldDocument != null
                            && newDocument != null
                            && (oldDocument.Id.Value != newDocument.Id.Value
                                || oldDocument.AccessHash.Value != newDocument.AccessHash.Value))
                        {
                            var isoFileName = Media.IsoFileName;
                            var notListened = Media.NotListened;
#if WP8
                            var file = Media.File;
#endif
                            _media = m.Media;
                            _media.IsoFileName = isoFileName;
                            _media.NotListened = notListened;

#if WP8
                            _media.File = file;
#endif
                        }
                    }

                    return;
                }

                var oldMediaVideo = oldMedia as TLMessageMediaVideo;
                var newMediaVideo = newMedia as TLMessageMediaVideo;
                if (oldMediaVideo != null && newMediaVideo != null)
                {
                    if (oldMediaVideo.Video.GetType() != newMediaVideo.Video.GetType())
                    {
                        _media = m.Media;
                    }
                    else
                    {
                        var oldVideo = oldMediaVideo.Video as TLVideo;
                        var newVideo = newMediaVideo.Video as TLVideo;
                        if (oldVideo != null
                            && newVideo != null
                            && (oldVideo.Id.Value != newVideo.Id.Value
                                || oldVideo.AccessHash.Value != newVideo.AccessHash.Value))
                        {
                            var isoFileName = Media.IsoFileName;
                            _media = m.Media;
                            _media.IsoFileName = isoFileName;
                        }
                    }

                    return;
                }

                var oldMediaAudio = oldMedia as TLMessageMediaAudio;
                var newMediaAudio = newMedia as TLMessageMediaAudio;
                if (oldMediaAudio != null && newMediaAudio != null)
                {
                    if (oldMediaAudio.Audio.GetType() != newMediaAudio.Audio.GetType())
                    {
                        _media = m.Media;
                    }
                    else
                    {
                        var oldAudio = oldMediaAudio.Audio as TLAudio;
                        var newAudio = newMediaAudio.Audio as TLAudio;
                        if (oldAudio != null
                            && newAudio != null
                            && (oldAudio.Id.Value != newAudio.Id.Value
                                || oldAudio.AccessHash.Value != newAudio.AccessHash.Value))
                        {
                            var isoFileName = Media.IsoFileName;
                            var notListened = Media.NotListened;
                            _media = m.Media;
                            _media.IsoFileName = isoFileName;
                            _media.NotListened = notListened;
                        }
                    }

                    return;
                }

                var oldMediaPhoto = oldMedia as TLMessageMediaPhoto;
                var newMediaPhoto = newMedia as TLMessageMediaPhoto;
                if (oldMediaPhoto == null || newMediaPhoto == null)
                {
                    _media = m.Media;
                }
                else
                {
                    var oldPhoto = oldMediaPhoto.Photo as TLPhoto;
                    var newPhoto = newMediaPhoto.Photo as TLPhoto;
                    if (oldPhoto == null || newPhoto == null)
                    {
                        _media = m.Media;
                    }
                    else
                    {
                        if (oldPhoto.AccessHash.Value != newPhoto.AccessHash.Value)
                        {
                            var oldCachedSize = oldPhoto.Sizes.FirstOrDefault(x => x is TLPhotoCachedSize) as TLPhotoCachedSize;
                            var oldMSize = oldPhoto.Sizes.FirstOrDefault(x => TLString.Equals(x.Type, new TLString("m"), StringComparison.OrdinalIgnoreCase));
                            foreach (var size in newPhoto.Sizes)
                            {
                                if (size is TLPhotoCachedSize)
                                {
                                    size.TempUrl = oldCachedSize != null ? oldCachedSize.TempUrl : null;
                                }
                                else if (TLString.Equals(size.Type, new TLString("s"), StringComparison.OrdinalIgnoreCase))
                                {
                                    size.TempUrl = oldCachedSize != null ? oldCachedSize.TempUrl : null;
                                }
                                else
                                {
                                    size.TempUrl = oldMSize != null ? oldMSize.TempUrl : null;
                                }
                            }

                            _media = m.Media;
                        }
                    }
                }
            }
        }

        #region Additional

        private TLInputMediaBase _inputMedia;

        /// <summary>
        /// To resend canceled message
        /// </summary>
        public TLInputMediaBase InputMedia
        {
            get { return _inputMedia; }
            set { SetField(ref _inputMedia, value, () => InputMedia); }
        }

        public List<string> Links { get; set; }

        #endregion
    }

    [Obsolete]
    public class TLMessageForwarded17 : TLMessageForwarded
    {
        public new const uint Signature = TLConstructors.TLMessageForwarded17;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLBool Out
        {
            get { return new TLBool(IsSet(_flags, (int)MessageFlags.Out)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)MessageFlags.Out);
                }
            }
        }

        public override void SetUnread(TLBool value)
        {
            Unread = value;
        }

        public override TLBool Unread
        {
            get { return new TLBool(IsSet(_flags, (int)MessageFlags.Unread)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)MessageFlags.Unread);
                    NotifyOfPropertyChange(() => Status);
                }
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FwdFromId = GetObject<TLInt>(bytes, ref position);
            FwdDate = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject<TLInt>(bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);
            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _media = GetObject<TLMessageMediaBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FwdFromId = GetObject<TLInt>(input);
            FwdDate = GetObject<TLInt>(input);
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);
            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            _media = GetObject<TLMessageMediaBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Id = Id ?? new TLInt(0);
            Id.ToStream(output);
            FwdFromId.ToStream(output);
            FwdDate.ToStream(output);
            FromId.ToStream(output);
            ToId.ToStream(output);
            _date.ToStream(output);
            Message.ToStream(output);
            _media.ToStream(output);
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = (TLMessageForwarded17)message;
            Flags = m.Flags;
        }
    }

    [Obsolete]
    public class TLMessageForwarded : TLMessage
    {
        public new const uint Signature = TLConstructors.TLMessageForwarded;

        public TLInt FwdFromId { get; set; }

        public TLUserBase FwdFrom
        {
            get
            {
                var cacheService = InMemoryCacheService.Instance;
                return cacheService.GetUser(FwdFromId);
            }
        }

        public TLInt FwdDate { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            FwdFromId = GetObject<TLInt>(bytes, ref position);
            FwdDate = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject<TLInt>(bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);
            Out = GetObject<TLBool>(bytes, ref position);
            Unread = GetObject<TLBool>(bytes, ref position);
            _date = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _media = GetObject<TLMessageMediaBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FwdFromId = GetObject<TLInt>(input);
            FwdDate = GetObject<TLInt>(input);
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);
            Out = GetObject<TLBool>(input);
            Unread = GetObject<TLBool>(input);
            _date = GetObject<TLInt>(input);
            Message = GetObject<TLString>(input);
            _media = GetObject<TLMessageMediaBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id = Id ?? new TLInt(0);
            Id.ToStream(output);
            FwdFromId.ToStream(output);
            FwdDate.ToStream(output);
            FromId.ToStream(output);
            ToId.ToStream(output);
            Out.ToStream(output);
            Unread.ToStream(output);
            _date.ToStream(output);
            Message.ToStream(output);
            _media.ToStream(output);
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = (TLMessageForwarded)message;
            FwdFromId = m.FwdFromId;
            FwdDate = m.FwdDate;
        }
    }

    public class TLMessageService49 : TLMessageService40, IReplyToMsgId
    {
        public new const uint Signature = TLConstructors.TLMessageService49;

        private TLInt _replyToMsgId;

        public TLInt ReplyToMsgId
        {
            get { return _replyToMsgId; }
            set { SetField(out _replyToMsgId, value, ref _flags, (int)MessageFlags.ReplyToMsgId); }
        }

        public override ReplyInfo ReplyInfo
        {
            get { return ReplyToMsgId != null ? new ReplyInfo { ReplyToMsgId = ReplyToMsgId, Reply = Reply } : null; }
        }

        public TLMessageService49 Media
        {
            get { return this; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = IsSet(Flags, (int)MessageFlags.FromId)
                ? GetObject<TLInt>(bytes, ref position)
                : new TLInt(-1);
            ToId = GetObject<TLPeerBase>(bytes, ref position);
            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(bytes, ref position);
            }
            _date = GetObject<TLInt>(bytes, ref position);
            Action = GetObject<TLMessageActionBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);
            if (IsSet(Flags, (int)MessageFlags.ReplyToMsgId))
            {
                ReplyToMsgId = GetObject<TLInt>(input);
            }
            _date = GetObject<TLInt>(input);

            // workaround: RandomId and Status were missing here, so Flags.31 (bits 100000...000) is recerved to handle this issue
            if (IsSet(Flags, int.MinValue))
            {
                var randomId = GetObject<TLLong>(input);
                if (randomId.Value != 0)
                {
                    RandomId = randomId;
                }
                var status = GetObject<TLInt>(input);
                _status = (MessageStatus)status.Value;
            }

            Action = GetObject<TLMessageActionBase>(input);

            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Set(ref _flags, int.MinValue); // workaround

            Flags.ToStream(output);
            Id = Id ?? new TLInt(0);
            Id.ToStream(output);
            FromId.ToStream(output);
            ToId.ToStream(output);
            ToStream(output, ReplyToMsgId, Flags, (int)MessageFlags.ReplyToMsgId);
            _date.ToStream(output);

            // workaround: RandomId and Status were missing here, so Flags.31 is recerved to handle this issue

            RandomId = RandomId ?? new TLLong(0);
            RandomId.ToStream(output);
            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());

            Action.ToStream(output);

            CustomFlags.NullableToStream(output);
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);

            var m = message as TLMessageService49;
            if (m != null)
            {
                if (m.ReplyToMsgId != null)
                {
                    ReplyToMsgId = m.ReplyToMsgId;
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() + string.Format("ReplyToMsgId={0} Reply={1}", ReplyToMsgId != null ? ReplyToMsgId.Value.ToString(CultureInfo.InvariantCulture) : "null", Reply != null ? Reply.GetType().Name : "null");
        }
    }

    public class TLMessageService40 : TLMessageService17
    {
        public new const uint Signature = TLConstructors.TLMessageService40;

        private TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        public bool Silent { get { return IsSet(Flags, (int)MessageFlags.Silent); } }

        public bool Post { get { return IsSet(Flags, (int)MessageFlags.Post); } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = IsSet(Flags, (int)MessageFlags.FromId)
                ? GetObject<TLInt>(bytes, ref position)
                : new TLInt(-1);
            ToId = GetObject<TLPeerBase>(bytes, ref position);
            _date = GetObject<TLInt>(bytes, ref position);
            Action = GetObject<TLMessageActionBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);
            _date = GetObject<TLInt>(input);

            // workaround: RandomId and Status were missing here, so Flags.31 (bits 100000...000) is recerved to handle this issue
            if (IsSet(Flags, int.MinValue))
            {
                var randomId = GetObject<TLLong>(input);
                if (randomId.Value != 0)
                {
                    RandomId = randomId;
                }
                var status = GetObject<TLInt>(input);
                _status = (MessageStatus)status.Value;
            }

            Action = GetObject<TLMessageActionBase>(input);

            CustomFlags = GetNullableObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Set(ref _flags, int.MinValue); // workaround

            Flags.ToStream(output);
            Id = Id ?? new TLInt(0);
            Id.ToStream(output);
            FromId.ToStream(output);
            ToId.ToStream(output);
            _date.ToStream(output);

            // workaround: RandomId and Status were missing here, so Flags.31 is recerved to handle this issue

            RandomId = RandomId ?? new TLLong(0);
            RandomId.ToStream(output);
            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());

            Action.ToStream(output);

            CustomFlags.NullableToStream(output);
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);

            var m = message as TLMessageService40;
            if (m != null)
            {
                if (m.CustomFlags != null)
                {
                    CustomFlags = m.CustomFlags;
                }
            }
        }

        public override string ToString()
        {
            return base.ToString() + " CustomFlags=" + MessageCustomFlagsString(CustomFlags);
        }
    }

    public class TLMessageService17 : TLMessageService
    {
        public new const uint Signature = TLConstructors.TLMessageService17;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLBool Out
        {
            get { return new TLBool(IsSet(_flags, (int)MessageFlags.Out)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)MessageFlags.Out);
                }
            }
        }

        public override void SetUnread(TLBool value)
        {
            Unread = value;
        }

        public override void SetUnreadSilent(TLBool value)
        {
            if (value != null)
            {
                SetUnset(ref _flags, value.Value, (int)MessageFlags.Unread);
            }
        }

        public override TLBool Unread
        {
            get { return new TLBool(IsSet(_flags, (int)MessageFlags.Unread)); }
            set
            {
                if (value != null)
                {
                    SetUnset(ref _flags, value.Value, (int)MessageFlags.Unread);
                    NotifyOfPropertyChange(() => Status);
                }
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            FromId = GetObject<TLInt>(bytes, ref position);
            ToId = GetObject<TLPeerBase>(bytes, ref position);
            _date = GetObject<TLInt>(bytes, ref position);
            Action = GetObject<TLMessageActionBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            FromId = GetObject<TLInt>(input);
            ToId = GetObject<TLPeerBase>(input);
            _date = GetObject<TLInt>(input);

            // workaround: RandomId and Status were missing here, so Flags.31 (bits 100000...000) is recerved to handle this issue
            if (IsSet(_flags, int.MinValue))
            {
                var randomId = GetObject<TLLong>(input);
                if (randomId.Value != 0)
                {
                    RandomId = randomId;
                }
                var status = GetObject<TLInt>(input);
                _status = (MessageStatus)status.Value;
            }

            Action = GetObject<TLMessageActionBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Set(ref _flags, int.MinValue); // workaround

            Flags.ToStream(output);
            Id = Id ?? new TLInt(0);
            Id.ToStream(output);
            FromId.ToStream(output);
            ToId.ToStream(output);
            _date.ToStream(output);

            // workaround: RandomId and Status were missing here, so Flags.31 is recerved to handle this issue

            RandomId = RandomId ?? new TLLong(0);
            RandomId.ToStream(output);
            var status = new TLInt((int)Status);
            output.Write(status.ToBytes());

            Action.ToStream(output);
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = (TLMessageService17)message;

            Flags = m.Flags;
        }

        public override string ToString()
        {
            return base.ToString() + " Flags=" + MessageFlagsString(Flags);
        }
    }

    public class TLMessageService : TLMessageCommon
    {
        public const uint Signature = TLConstructors.TLMessageService;

        public TLMessageActionBase Action { get; set; }

        public override double MediaWidth
        {
            get
            {
                var phoneCall = Action as TLMessageActionPhoneCall;
                if (phoneCall != null)
                {
                    return 12.0 + 284.0 + 12.0;
                }

                return base.MediaWidth;
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLInt>(bytes, ref position);
            base.FromBytes(bytes, ref position);
            Action = GetObject<TLMessageActionBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            var id = GetObject<TLInt>(input);
            if (id.Value != 0)
            {
                Id = id;
            }
            base.FromStream(input);
            Action = GetObject<TLMessageActionBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id = Id ?? new TLInt(0);
            Id.ToStream(output);
            base.ToStream(output);
            Action.ToStream(output);
        }

        public override void Update(TLMessageBase message)
        {
            base.Update(message);
            var m = (TLMessageService)message;

            if (Action != null)
            {
                Action.Update(m.Action);
            }
            else
            {
                Action = m.Action;
            }
        }


        public override void Edit(TLMessageBase messageBase)
        {
            var message = messageBase as TLMessageService;
            if (message != null)
            {
                var oldMessageMediaActionGame = Action as TLMessageActionGameScore;
                var newMessageMediaActionGame = message.Action as TLMessageActionGameScore;

                if (oldMessageMediaActionGame != null
                    && newMessageMediaActionGame != null
                    && oldMessageMediaActionGame.Score.Value != newMessageMediaActionGame.Score.Value)
                {
                    oldMessageMediaActionGame.Score = newMessageMediaActionGame.Score;
                }
            }
        }

        public override Visibility SelectionVisibility
        {
            get { return Visibility.Collapsed; }
        }

        public override string ToString()
        {
            return base.ToString() + " action=" + Action;
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Telegram.Api.Helpers;
#if WIN_RT
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#elif WINDOWS_PHONE
using System.Windows;
using System.Windows.Media;
#endif
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum DialogCustomFlags
    {
        PinnedId = 0x1,             // 0
        Promo = 0x2,                // 1
        PromoExpires = 0x4,         // 2
        PromoNotification = 0x8,    // 3
        UnreadMark = 0x10,          // 4
    }

    [Flags]
    public enum DialogFlags
    {
        Pts = 0x1,                  // 0
        Draft = 0x2,                // 1
        Pinned = 0x4,               // 2
        UnreadMark = 0x8,           // 3
        ReadMaxPosition = 0x10,     // 4
    }

    public interface IDialogPts
    {
        TLInt Pts { get; set; }
    }

    public enum TypingType
    {
        Text,
        Record,
        Upload
    }

    public class Typing
    {
        public TypingType Type { get; protected set; }

        public string Description { get; protected set; }

        public override string ToString()
        {
            return Description;
        }

        public Typing(TypingType type, string description)
        {
            Description = description;
            Type = type;
        }

        public static bool Equals(Typing typing1, Typing typing2)
        {
            if (typing1 == null && typing2 == null) return true;
            if (typing1 == null) return false;
            if (typing2 == null) return false;

            return typing1.Type == typing2.Type;
        }
    }

    public abstract class TLDialogBase : TLObject
    {
        public static string DialogFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (DialogFlags)flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public object MessagesSyncRoot = new object();

        public int Index
        {
            get { return Peer != null && Peer.Id != null ? Peer.Id.Value : default(int); }
            set
            {
                //NOTE: No need to set Index during deserialization. Possible null reference
            }
        }

        public TLPeerBase Peer { get; set; }

        public TLInt TopMessageId { get; set; }

        private TLInt _unreadCount;

        public TLInt UnreadCount
        {
            get { return _unreadCount; }
            set { _unreadCount = value; }
        }

        public virtual bool IsPinned { get; set; }

        public virtual TLInt PinnedId { get; set; }

        #region Additional

        public Typing Typing { get; set; }

        public TLDialogBase Self { get { return this; } }

        /// <summary>
        /// If top message is sending message, than it has RandomId instead of Id
        /// </summary>
        public TLLong TopMessageRandomId { get; set; }

        public TLLong TopDecryptedMessageRandomId { get; set; }

        public TLPeerNotifySettingsBase NotifySettings { get; set; }

        public TLObject _with;

        public TLObject With
        {
            get { return _with; }
            set { SetField(ref _with, value, () => With); }
        }

        public int WithId
        {
            get
            {
                if (With is TLChatBase)
                {
                    return ((TLChatBase)With).Index;
                }
                if (With is TLUserBase)
                {
                    return ((TLUserBase)With).Index;
                }
                return -1;
            }
        }

        public Visibility ChatIconVisibility
        {
            get { return Peer is TLPeerChat ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility ChatVisibility
        {
            get { return Peer is TLPeerChat || Peer is TLPeerEncryptedChat || Peer is TLPeerBroadcast ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility UserVisibility
        {
            get { return Peer is TLPeerUser || _with is TLChatForbidden || _with is TLChatEmpty ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility BotVisibility
        {
            get
            {
                var user = _with as TLUser;
                return user != null && user.IsBot ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility EncryptedChatVisibility
        {
            get { return Peer is TLPeerEncryptedChat ? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility VerifiedVisibility
        {
            get
            {
                var user = With as TLUserBase;
                if (user != null)
                {
                    return user.IsVerified ? Visibility.Visible : Visibility.Collapsed;
                }

                var channel = With as TLChannel;
                if (channel != null)
                {
                    return channel.IsVerified ? Visibility.Visible : Visibility.Collapsed;
                }

                return Visibility.Collapsed;
            }
        }

        public Uri EncryptedImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                return !isLightTheme ?
                    new Uri("/Images/Dialogs/secretchat-white-WXGA.png", UriKind.Relative) :
                    new Uri("/Images/Dialogs/secretchat-black-WXGA.png", UriKind.Relative);
            }
        }

        public virtual Brush ForegroundBrush
        {
            get { return (Brush)Application.Current.Resources["PhoneForegroundBrush"]; }
        }

        public Brush MuteIconBackground
        {
            get { return new SolidColorBrush(Color.FromArgb(255, 39, 164, 236)); }
        }

        public bool IsChat
        {
            get { return Peer is TLPeerChat; }
        }

        public bool IsEncryptedChat
        {
            get { return Peer is TLPeerEncryptedChat; }
        }

        public DateTime? LastNotificationTime { get; set; }

        public int UnmutedCount { get; set; }
        #endregion

        public abstract int GetDateIndex();
        public abstract int GetDateIndexWithDraft();
        public abstract int CountMessages();
    }

    public class TLEncryptedDialog : TLDialogBase
    {
        public const uint Signature = TLConstructors.TLDialogSecret;

        #region Additional

        public TLInt UnreadMentionsCount { get; set; }

        public TLDecryptedMessageBase _topMessage;

        public TLDecryptedMessageBase TopMessage
        {
            get
            {
                if (TLUtils.IsDisplayedDecryptedMessage(_topMessage, true))
                {
                    return _topMessage;
                }

                if (Messages != null)
                {
                    for (var i = 0; i < Messages.Count; i++)
                    {
                        if (TLUtils.IsDisplayedDecryptedMessage(Messages[i], true))
                        {
                            return Messages[i];
                        }
                    }
                }

                return null;
            }
            set { SetField(ref _topMessage, value, () => TopMessage); }
        }

        public ObservableCollection<TLDecryptedMessageBase> Messages { get; set; }
        #endregion

        public override Brush ForegroundBrush
        {
            get { return new SolidColorBrush(Color.FromArgb(255, 0, 170, 8)); }
        }

        public override int GetDateIndex()
        {
            return _topMessage != null ? _topMessage.DateIndex : 0;
        }

        public override int GetDateIndexWithDraft()
        {
            return _topMessage != null ? _topMessage.DateIndex : 0;
        }

        public override int CountMessages()
        {
            return Messages.Count;
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLPeerBase>(input);
            var topDecryptedMessageRandomId = GetObject<TLLong>(input);
            if (topDecryptedMessageRandomId.Value != 0)
            {
                TopDecryptedMessageRandomId = topDecryptedMessageRandomId;
            }

            UnreadCount = GetObject<TLInt>(input);

            _with = GetObject<TLObject>(input);
            if (_with is TLNull) { _with = null; }

            var messages = GetObject<TLVector<TLDecryptedMessageBase>>(input);
            Messages = messages != null ?
                new ObservableCollection<TLDecryptedMessageBase>(messages.Items) :
                new ObservableCollection<TLDecryptedMessageBase>();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Peer.ToStream(output);

            TopDecryptedMessageRandomId = TopDecryptedMessageRandomId ?? new TLLong(0);
            TopDecryptedMessageRandomId.ToStream(output);

            output.Write(UnreadCount.ToBytes());

            With.NullableToStream(output);

            if (Messages != null)
            {
                var messages = new TLVector<TLDecryptedMessageBase> { Items = Messages };
                messages.ToStream(output);
            }
            else
            {
                var messages = new TLVector<TLDecryptedMessageBase>();
                messages.ToStream(output);
            }
        }

        public override string ToString()
        {
            return string.Format("peer=[{0}] unread_count={1} top_message_id={2} top_message={3}", Peer, UnreadCount, TopMessageId, TopMessage);
        }

        public static int InsertMessageInOrder(IList<TLDecryptedMessageBase> messages, TLDecryptedMessageBase message)
        {
            var position = -1;

            if (messages.Count == 0)
            {
                position = 0;
            }

            for (var i = 0; i < messages.Count; i++)
            {
                if (messages[i].DateIndex < message.DateIndex)
                {
                    position = i;
                    break;
                }
            }

            if (position != -1)
            {
                messages.Insert(position, message);
            }

            return position;
        }
    }

    public class TLBroadcastDialog : TLDialogBase
    {
        public const uint Signature = TLConstructors.TLBroadcastDialog;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Peer = GetObject<TLPeerBase>(bytes, ref position);
            TopMessageId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLPeerBase>(input);
            var topMessageId = GetObject<TLInt>(input);
            if (topMessageId.Value != 0)
            {
                TopMessageId = topMessageId;
            }

            UnreadCount = GetObject<TLInt>(input);

            var notifySettingsObject = GetObject<TLObject>(input);
            NotifySettings = notifySettingsObject as TLPeerNotifySettingsBase;

            var topMessageRandomId = GetObject<TLLong>(input);
            if (topMessageRandomId.Value != 0)
            {
                TopMessageRandomId = topMessageRandomId;
            }

            _with = GetObject<TLObject>(input);
            if (_with is TLNull) { _with = null; }

            var messages = GetObject<TLVector<TLMessageBase>>(input);
            Messages = messages != null ? new ObservableCollection<TLMessageBase>(messages.Items) : new ObservableCollection<TLMessageBase>();

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Peer.ToStream(output);

            TopMessageId = TopMessageId ?? new TLInt(0);
            TopMessageId.ToStream(output);

            output.Write(UnreadCount.ToBytes());

            NotifySettings.NullableToStream(output);

            TopMessageRandomId = TopMessageRandomId ?? new TLLong(0);
            TopMessageRandomId.ToStream(output);

            With.NullableToStream(output);
            if (Messages != null)
            {
                var messages = new TLVector<TLMessageBase> { Items = Messages };
                messages.ToStream(output);
            }
            else
            {
                var messages = new TLVector<TLMessageBase>();
                messages.ToStream(output);
            }
        }

        #region Additional

        public TLMessageBase _topMessage;

        public TLMessageBase TopMessage
        {
            get { return _topMessage; }
            set { SetField(ref _topMessage, value, () => TopMessage); }
        }

        public ObservableCollection<TLMessageBase> Messages { get; set; }

        public bool ShowFrom
        {
            get { return Peer is TLPeerChat && !(TopMessage is TLMessageService); }
        }

        #endregion

        public override int GetDateIndex()
        {
            return _topMessage != null ? _topMessage.DateIndex : 0;
        }

        public override int GetDateIndexWithDraft()
        {
            return _topMessage != null ? _topMessage.DateIndex : 0;
        }

        public override int CountMessages()
        {
            return Messages.Count;
        }

        public override string ToString()
        {
            return string.Format("peer=[{0}] unread_count={1} top_message_id={2} top_message={3}", Peer, UnreadCount, TopMessageId, TopMessage);
        }

        public static int InsertMessageInOrder(IList<TLMessageBase> messages, TLMessageBase message)
        {
            var position = -1;

            if (messages.Count == 0)
            {
                position = 0;
            }

            for (var i = 0; i < messages.Count; i++)
            {
                if (messages[i].Index == 0)
                {
                    if (messages[i].DateIndex < message.DateIndex)
                    {
                        position = i;
                        break;
                    }

                    continue;
                }

                if (messages[i].Index == message.Index)
                {
                    position = -1;
                    break;
                }
                if (messages[i].Index < message.Index)
                {
                    position = i;
                    break;
                }
            }

            if (position != -1)
            {
                //message.IsAnimated = position == 0;
                Execute.BeginOnUIThread(() => messages.Insert(position, message));
            }

            return position;
        }

        public virtual void Update(TLDialog dialog)
        {
            Peer = dialog.Peer;
            UnreadCount = dialog.UnreadCount;

            //если последнее сообщение отправляется и имеет дату больше, то не меняем
            if (TopMessageId == null && TopMessage.DateIndex > dialog.TopMessage.DateIndex)
            {
                //добавляем сообщение в список в нужное место
                InsertMessageInOrder(Messages, dialog.TopMessage);

                return;
            }
            TopMessageId = dialog.TopMessageId;
            TopMessageRandomId = dialog.TopMessageRandomId;
            TopMessage = dialog.TopMessage;
            if (Messages.Count > 0)
            {
                for (int i = 0; i < Messages.Count; i++)
                {
                    if (Messages[i].DateIndex < TopMessage.DateIndex)
                    {
                        Messages.Insert(i, TopMessage);
                        break;
                    }
                    if (Messages[i].DateIndex == TopMessage.DateIndex)
                    {
                        break;
                    }
                }
            }
            else
            {
                Messages.Add(TopMessage);
            }
        }
    }

    public class TLDialog : TLDialogBase
    {
        public const uint Signature = TLConstructors.TLDialog;

        #region Additional

        public TLMessageBase _topMessage;

        public TLMessageBase TopMessage
        {
            get { return _topMessage; }
            set
            {

                SetField(ref _topMessage, value, () => TopMessage);
            }
        }

        public ObservableCollection<TLMessageBase> Messages { get; set; }

        public List<TLMessageBase> CommitMessages { get; set; }

        public bool ShowFrom
        {
            get { return Peer is TLPeerChat && !(TopMessage is TLMessageService); }
        }

        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Peer = GetObject<TLPeerBase>(bytes, ref position);
            TopMessageId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLPeerBase>(input);
            var topMessageId = GetObject<TLInt>(input);
            if (topMessageId.Value != 0)
            {
                TopMessageId = topMessageId;
            }

            UnreadCount = GetObject<TLInt>(input);

            var notifySettingsObject = GetObject<TLObject>(input);
            NotifySettings = notifySettingsObject as TLPeerNotifySettingsBase;

            var topMessageRandomId = GetObject<TLLong>(input);
            if (topMessageRandomId.Value != 0)
            {
                TopMessageRandomId = topMessageRandomId;
            }

            _with = GetObject<TLObject>(input);
            if (_with is TLNull) { _with = null; }

            var messages = GetObject<TLVector<TLMessageBase>>(input);
            Messages = messages != null ? new ObservableCollection<TLMessageBase>(messages.Items) : new ObservableCollection<TLMessageBase>();

            var dialog71 = new TLDialog71();
            dialog71.Flags = new TLInt(0);
            dialog71.Peer = Peer;
            dialog71.TopMessageId = TopMessageId;
            dialog71.ReadInboxMaxId = new TLInt(0);
            dialog71.ReadOutboxMaxId = new TLInt(0);
            dialog71.UnreadCount = UnreadCount;
            dialog71.UnreadMentionsCount = new TLInt(0);
            dialog71.NotifySettings = NotifySettings;
            //dialog53.Pts = Pts;
            dialog71.Draft = null;

            dialog71.TopMessageRandomId = topMessageRandomId;
            dialog71._with = _with;
            dialog71.Messages = Messages;

            return dialog71;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Peer.ToStream(output);

            TopMessageId = TopMessageId ?? new TLInt(0);
            TopMessageId.ToStream(output);

            output.Write(UnreadCount.ToBytes());

            NotifySettings.NullableToStream(output);

            TopMessageRandomId = TopMessageRandomId ?? new TLLong(0);
            TopMessageRandomId.ToStream(output);

            With.NullableToStream(output);
            if (Messages != null)
            {
                var messages = new TLVector<TLMessageBase> { Items = CommitMessages };
                messages.ToStream(output);
            }
            else
            {
                var messages = new TLVector<TLMessageBase>();
                messages.ToStream(output);
            }
        }


        public virtual void Update(TLDialog dialog)
        {
            Peer = dialog.Peer;
            UnreadCount = dialog.UnreadCount;

            //если последнее сообщение отправляется и имеет дату больше, то не меняем
            if (TopMessageId == null && (TopMessage == null || TopMessage.DateIndex > dialog.TopMessage.DateIndex))
            {
                //добавляем сообщение в список в нужное место, если его еще нет
                var insertRequired = false;
                if (Messages != null && dialog.TopMessage != null)
                {
                    var oldMessage = Messages.FirstOrDefault(x => x.Index == dialog.TopMessage.Index);
                    if (oldMessage == null)
                    {
                        insertRequired = true;
                    }
                }

                if (insertRequired)
                {
                    InsertMessageInOrder(Messages, dialog.TopMessage);
                }

                return;
            }

            if (TopMessageId != null
                && dialog.TopMessageId != null
                && TopMessageId.Value == dialog.TopMessageId.Value)
            {
            }
            else if (TopMessage != null && TopMessage.RandomIndex != 0)
            {

            }
            else
            {
                TopMessageId = dialog.TopMessageId;
                _topMessage = dialog.TopMessage;
                TopMessageRandomId = dialog.TopMessageRandomId;

                lock (MessagesSyncRoot)
                {
                    InsertMessageInOrder(Messages, TopMessage);
                }
            }
        }

        #region Methods

        public override int GetDateIndex()
        {
            return _topMessage != null ? _topMessage.DateIndex : 0;
        }

        public override int GetDateIndexWithDraft()
        {
            return _topMessage != null ? _topMessage.DateIndex : 0;
        }

        public override int CountMessages()
        {
            return Messages.Count;
        }

        public override string ToString()
        {
            return string.Format("peer=[{0}] pinned_id={1} unread_count={2} top_message_id={3}    top_message={4}", Peer, PinnedId, UnreadCount, TopMessageId, TopMessage);
        }

        public static int InsertMessageInOrder(IList<TLMessageBase> messages, TLMessageBase message)
        {
            var position = -1;

            if (messages.Count == 0)
            {
                position = 0;
            }

            for (var i = 0; i < messages.Count; i++)
            {
                if (messages[i].Index == 0)
                {
                    if (messages[i].DateIndex < message.DateIndex)
                    {
                        position = i;
                        break;
                    }

                    continue;
                }

                if (messages[i].Index == message.Index)
                {
                    position = -1;
                    break;
                }
                if (messages[i].Index < message.Index)
                {
                    position = i;
                    break;
                }
            }

            if (position != -1)
            {
                //message._isAnimated = position == 0;
                messages.Insert(position, message);
            }

            return position;
        }
        #endregion
    }

    public class TLDialog24 : TLDialog
    {
        public new const uint Signature = TLConstructors.TLDialog24;

        public TLInt ReadInboxMaxId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Peer = GetObject<TLPeerBase>(bytes, ref position);
            TopMessageId = GetObject<TLInt>(bytes, ref position);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Peer = GetObject<TLPeerBase>(input);
            var topMessageId = GetObject<TLInt>(input);
            if (topMessageId.Value != 0)
            {
                TopMessageId = topMessageId;
            }
            ReadInboxMaxId = GetObject<TLInt>(input);
            UnreadCount = GetObject<TLInt>(input);

            var notifySettingsObject = GetObject<TLObject>(input);
            NotifySettings = notifySettingsObject as TLPeerNotifySettingsBase;

            var topMessageRandomId = GetObject<TLLong>(input);
            if (topMessageRandomId.Value != 0)
            {
                TopMessageRandomId = topMessageRandomId;
            }

            _with = GetObject<TLObject>(input);
            if (_with is TLNull) { _with = null; }

            var messages = GetObject<TLVector<TLMessageBase>>(input);
            Messages = messages != null ? new ObservableCollection<TLMessageBase>(messages.Items) : new ObservableCollection<TLMessageBase>();


            var dialog71 = new TLDialog71();
            dialog71.Flags = new TLInt(0);
            dialog71.Peer = Peer;
            dialog71.TopMessageId = TopMessageId;
            dialog71.ReadInboxMaxId = ReadInboxMaxId;
            dialog71.ReadOutboxMaxId = new TLInt(0);
            dialog71.UnreadCount = UnreadCount;
            dialog71.UnreadMentionsCount = new TLInt(0);
            dialog71.NotifySettings = NotifySettings;
            //dialog53.Pts = Pts;
            dialog71.Draft = null;

            dialog71.TopMessageRandomId = topMessageRandomId;
            dialog71._with = _with;
            dialog71.Messages = Messages;

            return dialog71;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Peer.ToStream(output);

            TopMessageId = TopMessageId ?? new TLInt(0);
            TopMessageId.ToStream(output);

            ReadInboxMaxId = ReadInboxMaxId ?? new TLInt(0);
            ReadInboxMaxId.ToStream(output);

            output.Write(UnreadCount.ToBytes());

            NotifySettings.NullableToStream(output);

            TopMessageRandomId = TopMessageRandomId ?? new TLLong(0);
            TopMessageRandomId.ToStream(output);

            With.NullableToStream(output);
            if (Messages != null)
            {
                var messages = new TLVector<TLMessageBase> { Items = Messages };
                messages.ToStream(output);
            }
            else
            {
                var messages = new TLVector<TLMessageBase>();
                messages.ToStream(output);
            }
        }

        public override void Update(TLDialog dialog)
        {
            try
            {
                base.Update(dialog);
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }

            var dialog24 = dialog as TLDialog24;
            if (dialog24 != null)
            {
                ReadInboxMaxId = dialog24.ReadInboxMaxId;
            }
        }
    }

    public class TLDialog53 : TLDialog24, IReadMaxId, IDialogPts
    {
        public new const uint Signature = TLConstructors.TLDialog53;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInt ReadOutboxMaxId { get; set; }

        protected TLLong _customFlags;

        public TLLong CustomFlags
        {
            get { return _customFlags; }
            set { _customFlags = value; }
        }

        protected TLInt _pts;

        public TLInt Pts
        {
            get { return _pts; }
            set { SetField(out _pts, value, ref _flags, (int)DialogFlags.Pts); }
        }

        protected TLDraftMessageBase _draft;

        public TLDraftMessageBase Draft
        {
            get { return _draft; }
            set { SetField(out _draft, value, ref _flags, (int)DialogFlags.Draft); }
        }

        protected TLInt _pinnedId;

        public override TLInt PinnedId
        {
            get { return _pinnedId; }
            set { SetField(out _pinnedId, value, ref _customFlags, (int)DialogCustomFlags.PinnedId); }
        }

        public bool UnreadMark
        {
            get { return IsSet(_customFlags, (int)DialogFlags.UnreadMark); }
            set { SetUnset(ref _customFlags, value, (int)DialogFlags.UnreadMark); }
        }

        public override int GetDateIndex()
        {
            if (IsPinned)
            {
                if (PinnedId != null)
                {
                    return int.MaxValue - PinnedId.Value;
                }

                Execute.ShowDebugMessage("GetDateIndex IsPinned=true PinnedId=null with=" + With);
            }

            return base.GetDateIndex();
        }

        public override int GetDateIndexWithDraft()
        {
            var dateIndex = GetDateIndex();

            if (IsPinned)
            {
                if (PinnedId != null)
                {
                    return int.MaxValue - PinnedId.Value;
                }
            }

            var draft = Draft as TLDraftMessage;
            if (draft != null)
            {
                return Math.Max(draft.Date.Value, dateIndex);
            }

            return dateIndex;
        }

        public override bool IsPinned
        {
            get { return IsSet(Flags, (int)DialogFlags.Pinned); }
            set { SetUnset(ref _flags, value, (int)DialogFlags.Pinned); }
        }

        public override string ToString()
        {
            return string.Format("flags={0} ", DialogFlagsString(Flags)) + "count=" + (Messages != null ? Messages.Count : 0) + " commit_count=" + (CommitMessages != null ? CommitMessages.Count : 0) + " " + base.ToString();
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Peer = GetObject<TLPeerBase>(bytes, ref position);
            TopMessageId = GetObject<TLInt>(bytes, ref position);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            ReadOutboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            _pts = GetObject<TLInt>(Flags, (int)DialogFlags.Pts, null, bytes, ref position);
            _draft = GetObject<TLDraftMessageBase>(Flags, (int)DialogFlags.Draft, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Peer = GetObject<TLPeerBase>(input);
            var topMessageId = GetObject<TLInt>(input);
            if (topMessageId.Value != 0)
            {
                TopMessageId = topMessageId;
            }
            ReadInboxMaxId = GetObject<TLInt>(input);
            ReadOutboxMaxId = GetObject<TLInt>(input);
            UnreadCount = GetObject<TLInt>(input);

            var notifySettingsObject = GetObject<TLObject>(input);
            NotifySettings = notifySettingsObject as TLPeerNotifySettingsBase;
            _pts = GetObject<TLInt>(Flags, (int)DialogFlags.Pts, null, input);
            _draft = GetObject<TLDraftMessageBase>(Flags, (int)DialogFlags.Draft, null, input);

            var topMessageRandomId = GetObject<TLLong>(input);
            if (topMessageRandomId.Value != 0)
            {
                TopMessageRandomId = topMessageRandomId;
            }

            _with = GetObject<TLObject>(input);
            if (_with is TLNull) { _with = null; }

            var messages = GetObject<TLVector<TLMessageBase>>(input);
            Messages = messages != null ? new ObservableCollection<TLMessageBase>(messages.Items) : new ObservableCollection<TLMessageBase>();

            CustomFlags = GetNullableObject<TLLong>(input);
            PinnedId = GetObject<TLInt>(CustomFlags, (int)DialogCustomFlags.PinnedId, null, input);


            var dialog71 = new TLDialog71();
            dialog71.Flags = new TLInt(0);
            dialog71.Peer = Peer;
            dialog71.TopMessageId = TopMessageId;
            dialog71.ReadInboxMaxId = ReadInboxMaxId;
            dialog71.ReadOutboxMaxId = ReadOutboxMaxId;
            dialog71.UnreadCount = UnreadCount;
            dialog71.UnreadMentionsCount = new TLInt(0);
            dialog71.NotifySettings = NotifySettings;
            dialog71.Pts = Pts;
            dialog71.Draft = Draft;

            dialog71.TopMessageRandomId = TopMessageRandomId;
            dialog71._with = _with;
            dialog71.Messages = Messages;

            dialog71.PinnedId = PinnedId;

            return dialog71;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Peer.ToStream(output);

            TopMessageId = TopMessageId ?? new TLInt(0);
            TopMessageId.ToStream(output);

            ReadInboxMaxId = ReadInboxMaxId ?? new TLInt(0);
            ReadInboxMaxId.ToStream(output);

            ReadOutboxMaxId = ReadOutboxMaxId ?? new TLInt(0);
            ReadOutboxMaxId.ToStream(output);

            UnreadCount.ToStream(output);

            NotifySettings.NullableToStream(output);
            ToStream(output, Pts, Flags, (int)DialogFlags.Pts);
            ToStream(output, Draft, Flags, (int)DialogFlags.Draft);

            TopMessageRandomId = TopMessageRandomId ?? new TLLong(0);
            TopMessageRandomId.ToStream(output);

            With.NullableToStream(output);
            if (Messages != null)
            {
                var messages = new TLVector<TLMessageBase> { Items = CommitMessages };
                messages.ToStream(output);
            }
            else
            {
                var messages = new TLVector<TLMessageBase>();
                messages.ToStream(output);
            }

            CustomFlags.NullableToStream(output);
            ToStream(output, PinnedId, CustomFlags, (int)DialogCustomFlags.PinnedId);
        }

        public override void Update(TLDialog dialog)
        {
            try
            {
                base.Update(dialog);
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }

            var dialog53 = dialog as TLDialog53;
            if (dialog53 != null)
            {
                ReadOutboxMaxId = dialog53.ReadOutboxMaxId;
                Pts = dialog53.Pts;
                Draft = dialog53.Draft;

                if (dialog53.IsPinned)
                {
                    IsPinned = true;
                    if (dialog53.PinnedId != null) PinnedId = dialog53.PinnedId;
                }
                else
                {
                    IsPinned = false;
                    dialog53.PinnedId = null;
                }
            }
        }
    }

    public class TLDialog71 : TLDialog53
    {
        public new const uint Signature = TLConstructors.TLDialog71;

        public TLInt UnreadMentionsCount { get; set; }

        public TLVector<TLMessageBase> UnreadMentions { get; set; }

        public IList<TLMessageBase> MigratedHistory { get; set; }

        public bool IsPromo
        {
            get { return IsSet(_customFlags, (int)DialogCustomFlags.Promo); }
            set { SetUnset(ref _customFlags, value, (int)DialogCustomFlags.Promo); }
        }

        protected TLInt _promoExpires;

        public TLInt PromoExpires
        {
            get { return _promoExpires; }
            set { SetField(out _promoExpires, value, ref _customFlags, (int)DialogCustomFlags.PromoExpires); }
        }

        public bool PromoNotification
        {
            get { return IsSet(_customFlags, (int)DialogCustomFlags.PromoNotification); }
            set { SetUnset(ref _customFlags, value, (int)DialogCustomFlags.PromoNotification); }
        }

        public override int GetDateIndex()
        {
            if (IsPromo)
            {
                return int.MaxValue;
            }

            if (IsPinned)
            {
                if (PinnedId != null)
                {
                    return int.MaxValue - PinnedId.Value - 1;
                }

                Execute.ShowDebugMessage("GetDateIndex IsPinned=true PinnedId=null with=" + With);
            }

            return base.GetDateIndex();
        }

        public override int GetDateIndexWithDraft()
        {
            var dateIndex = GetDateIndex();

            if (IsPromo)
            {
                return int.MaxValue;
            }

            if (IsPinned)
            {
                if (PinnedId != null)
                {
                    return int.MaxValue - PinnedId.Value - 1;
                }
            }

            var draft = Draft as TLDraftMessage;
            if (draft != null)
            {
                return Math.Max(draft.Date.Value, dateIndex);
            }

            return dateIndex;
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Peer = GetObject<TLPeerBase>(bytes, ref position);
            TopMessageId = GetObject<TLInt>(bytes, ref position);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            ReadOutboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadMentionsCount = GetObject<TLInt>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            _pts = GetObject<TLInt>(Flags, (int)DialogFlags.Pts, null, bytes, ref position);
            _draft = GetObject<TLDraftMessageBase>(Flags, (int)DialogFlags.Draft, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Peer = GetObject<TLPeerBase>(input);
            var topMessageId = GetObject<TLInt>(input);
            if (topMessageId.Value != 0)
            {
                TopMessageId = topMessageId;
            }
            ReadInboxMaxId = GetObject<TLInt>(input);
            ReadOutboxMaxId = GetObject<TLInt>(input);
            UnreadCount = GetObject<TLInt>(input);
            UnreadMentionsCount = GetObject<TLInt>(input);

            var notifySettingsObject = GetObject<TLObject>(input);
            NotifySettings = notifySettingsObject as TLPeerNotifySettingsBase;
            _pts = GetObject<TLInt>(Flags, (int)DialogFlags.Pts, null, input);
            _draft = GetObject<TLDraftMessageBase>(Flags, (int)DialogFlags.Draft, null, input);

            var topMessageRandomId = GetObject<TLLong>(input);
            if (topMessageRandomId.Value != 0)
            {
                TopMessageRandomId = topMessageRandomId;
            }

            _with = GetObject<TLObject>(input);
            if (_with is TLNull) { _with = null; }

            var messages = GetObject<TLVector<TLMessageBase>>(input);
            Messages = messages != null ? new ObservableCollection<TLMessageBase>(messages.Items) : new ObservableCollection<TLMessageBase>();

            CustomFlags = GetNullableObject<TLLong>(input);
            _pinnedId = GetObject<TLInt>(CustomFlags, (int)DialogCustomFlags.PinnedId, null, input);
            _promoExpires = GetObject<TLInt>(CustomFlags, (int)DialogCustomFlags.PromoExpires, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Peer.ToStream(output);

            TopMessageId = TopMessageId ?? new TLInt(0);
            TopMessageId.ToStream(output);

            ReadInboxMaxId = ReadInboxMaxId ?? new TLInt(0);
            ReadInboxMaxId.ToStream(output);

            ReadOutboxMaxId = ReadOutboxMaxId ?? new TLInt(0);
            ReadOutboxMaxId.ToStream(output);

            UnreadCount.ToStream(output);
            UnreadMentionsCount.ToStream(output);

            NotifySettings.NullableToStream(output);
            ToStream(output, Pts, Flags, (int)DialogFlags.Pts);
            ToStream(output, Draft, Flags, (int)DialogFlags.Draft);

            TopMessageRandomId = TopMessageRandomId ?? new TLLong(0);
            TopMessageRandomId.ToStream(output);

            With.NullableToStream(output);
            if (Messages != null)
            {
                var messages = new TLVector<TLMessageBase> { Items = CommitMessages };
                messages.ToStream(output);
            }
            else
            {
                var messages = new TLVector<TLMessageBase>();
                messages.ToStream(output);
            }

            CustomFlags.NullableToStream(output);
            ToStream(output, PinnedId, CustomFlags, (int)DialogCustomFlags.PinnedId);
            ToStream(output, PromoExpires, CustomFlags, (int)DialogCustomFlags.PromoExpires);
        }

        public override void Update(TLDialog dialog)
        {
            try
            {
                base.Update(dialog);
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }

            var dialog71 = dialog as TLDialog71;
            if (dialog71 != null)
            {
                UnreadMentionsCount = dialog71.UnreadMentionsCount;
                IsPromo = dialog71.IsPromo;
                PromoExpires = dialog71.PromoExpires;
            }
        }
    }

    public class TLDialogChannel : TLDialog24, IDialogPts
    {
        public new const uint Signature = TLConstructors.TLDialogChannel;

        public TLInt TopImportantMessageId { get; set; }

        public TLInt UnreadImportantCount { get; set; }

        public TLInt Pts { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Peer = GetObject<TLPeerBase>(bytes, ref position);
            TopMessageId = GetObject<TLInt>(bytes, ref position);
            TopImportantMessageId = GetObject<TLInt>(bytes, ref position);
            ReadInboxMaxId = GetObject<TLInt>(bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadImportantCount = GetObject<TLInt>(bytes, ref position);
            NotifySettings = GetObject<TLPeerNotifySettingsBase>(bytes, ref position);
            Pts = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            try
            {
                Peer = GetObject<TLPeerBase>(input);
                var topMessageId = GetObject<TLInt>(input);
                if (topMessageId.Value != 0)
                {
                    TopMessageId = topMessageId;
                }
                var topImportantMessageId = GetObject<TLInt>(input);
                if (topImportantMessageId.Value != 0)
                {
                    TopImportantMessageId = topImportantMessageId;
                }
                ReadInboxMaxId = GetObject<TLInt>(input);
                UnreadCount = GetObject<TLInt>(input);
                UnreadImportantCount = GetObject<TLInt>(input);

                var notifySettingsObject = GetObject<TLObject>(input);
                NotifySettings = notifySettingsObject as TLPeerNotifySettingsBase;
                Pts = GetObject<TLInt>(input);

                var topMessageRandomId = GetObject<TLLong>(input);
                if (topMessageRandomId.Value != 0)
                {
                    TopMessageRandomId = topMessageRandomId;
                }

                _with = GetObject<TLObject>(input);
                if (_with is TLNull) { _with = null; }

                var messages = GetObject<TLVector<TLMessageBase>>(input);
                Messages = messages != null ? new ObservableCollection<TLMessageBase>(messages.Items) : new ObservableCollection<TLMessageBase>();

                var dialog71 = new TLDialog71();
                dialog71.Flags = new TLInt(0);
                dialog71.Peer = Peer;
                dialog71.TopMessageId = TopMessageId;
                dialog71.ReadInboxMaxId = ReadInboxMaxId;
                dialog71.ReadOutboxMaxId = new TLInt(0);
                dialog71.UnreadCount = UnreadCount;
                dialog71.UnreadMentionsCount = new TLInt(0);
                dialog71.NotifySettings = NotifySettings;
                dialog71.Pts = Pts;
                dialog71.Draft = null;

                dialog71.TopMessageRandomId = topMessageRandomId;
                dialog71._with = _with;
                dialog71.Messages = Messages;

                return dialog71;
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Peer.ToStream(output);

            TopMessageId = TopMessageId ?? new TLInt(0);
            TopMessageId.ToStream(output);
            TopImportantMessageId = TopImportantMessageId ?? new TLInt(0);
            TopImportantMessageId.ToStream(output);

            ReadInboxMaxId = ReadInboxMaxId ?? new TLInt(0);
            ReadInboxMaxId.ToStream(output);

            output.Write(UnreadCount.ToBytes());
            output.Write(UnreadImportantCount.ToBytes());

            NotifySettings.NullableToStream(output);

            output.Write(Pts.ToBytes());

            TopMessageRandomId = TopMessageRandomId ?? new TLLong(0);
            TopMessageRandomId.ToStream(output);

            With.NullableToStream(output);
            if (Messages != null)
            {
                var messages = new TLVector<TLMessageBase> { Items = Messages.Where(x => x != null).ToList() };
#if DEBUG
                var indexes = new List<int>();
                for (var i = 0; i < Messages.Count; i++)
                {
                    if (Messages[i] == null)
                    {
                        indexes.Add(i);
                    }
                }
                if (indexes.Count > 0)
                {
                    Execute.ShowDebugMessage("TLDialogChannel.ToStream Items has null values total=" + Messages.Count + " null indexes=" + string.Join(",", indexes));
                }
#endif

                messages.ToStream(output);
            }
            else
            {
                var messages = new TLVector<TLMessageBase>();
                messages.ToStream(output);
            }
        }

        public override void Update(TLDialog dialog)
        {
            try
            {
                base.Update(dialog);
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }

            var d = dialog as TLDialogChannel;
            if (d != null)
            {
                TopImportantMessageId = d.TopImportantMessageId;
                UnreadImportantCount = d.UnreadImportantCount;
                Pts = d.Pts;
            }
        }
    }

    public class TLDialogFeed : TLDialog
    {
        public new const uint Signature = TLConstructors.TLDialogFeed;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLLong CustomFlags { get; set; }

        public TLInt FeedId { get; set; }

        public TLVector<TLInt> FeedOtherChannels { get; set; }

        protected TLFeedPosition _readMaxPosition;

        public TLFeedPosition ReadMaxPosition
        {
            get { return _readMaxPosition; }
            set { SetField(out _readMaxPosition, value, ref _flags, (int)DialogFlags.ReadMaxPosition); }
        }

        public TLInt UnreadMutedCount { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Peer = GetObject<TLPeerBase>(bytes, ref position);
            TopMessageId = GetObject<TLInt>(bytes, ref position);
            FeedId = GetObject<TLInt>(bytes, ref position);
            FeedOtherChannels = GetObject<TLVector<TLInt>>(bytes, ref position);
            _readMaxPosition = GetObject<TLFeedPosition>(Flags, (int)DialogFlags.ReadMaxPosition, null, bytes, ref position);
            UnreadCount = GetObject<TLInt>(bytes, ref position);
            UnreadMutedCount = GetObject<TLInt>(bytes, ref position);
            NotifySettings = new TLPeerNotifySettingsEmpty();

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Peer = GetObject<TLPeerBase>(input);
            TopMessageId = GetObject<TLInt>(input);
            FeedId = GetObject<TLInt>(input);
            FeedOtherChannels = GetObject<TLVector<TLInt>>(input);
            _readMaxPosition = GetObject<TLFeedPosition>(Flags, (int)DialogFlags.ReadMaxPosition, null, input);
            UnreadCount = GetObject<TLInt>(input);
            UnreadMutedCount = GetObject<TLInt>(input);

            NotifySettings = GetNullableObject<TLPeerNotifySettingsBase>(input);
            TopMessageRandomId = GetObject<TLLong>(input);
            //_with = GetNullableObject<TLObject>(input);
            var messages = GetObject<TLVector<TLMessageBase>>(input) ?? new TLVector<TLMessageBase>();
            Messages = new ObservableCollection<TLMessageBase>(messages.Items);

            CustomFlags = GetNullableObject<TLLong>(input);
            PinnedId = GetObject<TLInt>(CustomFlags, (int)DialogCustomFlags.PinnedId, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Peer.ToStream(output);
            TopMessageId = TopMessageId ?? new TLInt(0);
            TopMessageId.ToStream(output);
            FeedId.ToStream(output);
            FeedOtherChannels.ToStream(output);
            ToStream(output, ReadMaxPosition, Flags, (int)DialogFlags.ReadMaxPosition);
            UnreadCount.ToStream(output);
            UnreadMutedCount.ToStream(output);

            NotifySettings.NullableToStream(output);
            TopMessageRandomId = TopMessageRandomId ?? new TLLong(0);
            TopMessageRandomId.ToStream(output);
            //With.NullableToStream(output);
            if (Messages != null)
            {
                var messages = new TLVector<TLMessageBase> { Items = CommitMessages };
                messages.ToStream(output);
            }
            else
            {
                var messages = new TLVector<TLMessageBase>();
                messages.ToStream(output);
            }

            CustomFlags.NullableToStream(output);
            ToStream(output, PinnedId, CustomFlags, (int)DialogCustomFlags.PinnedId);
        }

        public override void Update(TLDialog dialog)
        {
            base.Update(dialog);

            var dialogFeed = dialog as TLDialogFeed;
            if (dialogFeed != null)
            {
                FeedId = dialogFeed.FeedId;
                FeedOtherChannels = dialogFeed.FeedOtherChannels;
                ReadMaxPosition = dialogFeed.ReadMaxPosition;
                UnreadMutedCount = dialogFeed.UnreadMutedCount;

                if (dialogFeed.IsPinned)
                {
                    IsPinned = true;
                    if (dialogFeed.PinnedId != null) PinnedId = dialogFeed.PinnedId;
                }
                else
                {
                    IsPinned = false;
                    dialogFeed.PinnedId = null;
                }
            }
        }
    }
}

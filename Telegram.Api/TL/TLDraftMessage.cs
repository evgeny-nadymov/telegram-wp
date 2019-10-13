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
using Telegram.Api.TL.Functions.Messages;

namespace Telegram.Api.TL
{
    [Flags]
    public enum DraftMessageEmptyFlags
    {
        Date = 0x1      // 0
    }

    public abstract class TLDraftMessageBase : TLObject
    {
        public abstract bool DraftEquals(TLDraftMessageBase draft);

        public abstract bool IsEmpty();

        public abstract TLSaveDraft ToSaveDraftObject(TLInputPeerBase peer);
    }

    public class TLDraftMessageEmpty82 : TLDraftMessageEmpty
    {
        public new const uint Signature = TLConstructors.TLDraftMessageEmpty82;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        protected TLInt _date;

        public TLInt Date
        {
            get { return _date; }
            set { SetField(out _date, value, ref _flags, (int) DraftMessageEmptyFlags.Date); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Date = GetObject<TLInt>(_flags, (int) DraftMessageEmptyFlags.Date, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Date = GetObject<TLInt>(_flags, (int)DraftMessageEmptyFlags.Date, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, Date, _flags, (int)DraftMessageEmptyFlags.Date);
        }

        public override bool DraftEquals(TLDraftMessageBase draft)
        {
            var emptyDraft = draft as TLDraftMessageEmpty;
            return draft == null || emptyDraft != null;
        }

        public override TLSaveDraft ToSaveDraftObject(TLInputPeerBase peer)
        {
            return new TLSaveDraft
            {
                Flags = new TLInt(0),
                Peer = peer,
                Message = TLString.Empty
            };
        }

        public override string ToString()
        {
            return "TLDraftMessageEmpty82";
        }
    }

    public class TLDraftMessageEmpty : TLDraftMessageBase
    {
        public const uint Signature = TLConstructors.TLDraftMessageEmpty;

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

        public override bool DraftEquals(TLDraftMessageBase draft)
        {
            return draft == null || draft is TLDraftMessageEmpty;
        }

        public override bool IsEmpty()
        {
            return true;
        }

        public override TLSaveDraft ToSaveDraftObject(TLInputPeerBase peer)
        {
            return new TLSaveDraft
            {
                Flags = new TLInt(0),
                Peer = peer,
                Message = TLString.Empty
            };
        }

        public override string ToString()
        {
            return "TLDraftMessageEmpty";
        }
    }

    public class TLDraftMessage : TLDraftMessageBase
    {
        public const uint Signature = TLConstructors.TLDraftMessage;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool NoWebpage
        {
            get { return IsSet(Flags, (int) SendFlags.NoWebpage); }
            set { SetUnset(ref _flags, value, (int) SendFlags.NoWebpage); }
        }

        private TLInt _replyToMsgId;

        public TLInt ReplyToMsgId
        {
            get { return _replyToMsgId; }
            set { SetField(out _replyToMsgId, value, ref _flags, (int) SendFlags.ReplyToMsgId); }
        }

        public TLString Message { get; set; }

        private TLVector<TLMessageEntityBase> _entities;

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set { SetField(out _entities, value, ref _flags, (int) SendFlags.Entities); }
        }

        public TLInt Date { get; set; }

        public override TLSaveDraft ToSaveDraftObject(TLInputPeerBase peer)
        {
            var obj = new TLSaveDraft
            {
                Flags = new TLInt(0),
                ReplyToMsgId = ReplyToMsgId,
                Peer = peer,
                Message = Message,
                Entities = Entities,
            };

            if (NoWebpage)
            {
                obj.DisableWebPagePreview();
            }

            return obj;
        }

        public override bool DraftEquals(TLDraftMessageBase draftBase)
        {
            var draftEmpty = draftBase as TLDraftMessageEmpty;
            if (draftEmpty != null)
            {
                return IsEmpty();
            }

            var draft = draftBase as TLDraftMessage;
            if (draft != null)
            {
                if (Flags.Value != draft.Flags.Value) return false;

                if (!TLString.Equals(Message, draft.Message, StringComparison.Ordinal)) return false;

                if (ReplyToMsgId != null && draft.ReplyToMsgId != null && ReplyToMsgId.Value != draft.ReplyToMsgId.Value) return false;

                if (Entities != null && draft.Entities != null && Entities.Count != draft.Entities.Count) return false;
            }
            else
            {
                return false;
            }

            return true;
        }

        public override bool IsEmpty()
        {
            return TLString.IsNullOrEmpty(Message) && ReplyToMsgId == null;
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            _replyToMsgId = GetObject<TLInt>(Flags, (int)SendFlags.ReplyToMsgId, null, bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)SendFlags.Entities, null, bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            
            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            _replyToMsgId = GetObject<TLInt>(Flags, (int)SendFlags.ReplyToMsgId, null, input);
            Message = GetObject<TLString>(input);
            _entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)SendFlags.Entities, null, input);
            Date = GetObject<TLInt>(input);
            
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            ToStream(output, _replyToMsgId, Flags, (int)SendFlags.ReplyToMsgId);
            Message.ToStream(output);
            ToStream(output, _entities, Flags, (int)SendFlags.Entities);
            Date.ToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLDraftMessage reply_to_msg_id={0} message={1} no_webpage={2} entities={3}", ReplyToMsgId, Message, NoWebpage, Entities != null? Entities.Count : 0);
        }
    }
}

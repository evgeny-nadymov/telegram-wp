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
    public enum MessageActionPhoneCallFlags
    {
        Reason = 0x1,           // 0
        Duration = 0x2,         // 1
    }

    [Flags]
    public enum MessageActionPaymentSentMeFlags
    {
        Info = 0x1,             // 0
        ShippingOptionId = 0x2, // 1
    }

    public abstract class TLMessageActionBase : TLObject
    {
        public abstract void Update(TLMessageActionBase newAction);

        public TLPhotoBase Photo { get; set; }
    }

    public class TLMessageActionEmpty : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionEmpty;

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

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionChatCreate : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatCreate;

        public TLString Title { get; set; }

        public TLVector<TLInt> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Title = GetObject<TLString>(bytes, ref position);
            Users = GetObject<TLVector<TLInt>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Title = GetObject<TLString>(input);
            Users = GetObject<TLVector<TLInt>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Title.ToStream(output);
            Users.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
            var action = newAction as TLMessageActionChatCreate;
            if (action != null)
            {
                Title = action.Title;
                Users = action.Users;
            }
        }
    }

    public class TLMessageActionChannelCreate : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChannelCreate;

        public TLString Title { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Title = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Title = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Title.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
            var action = newAction as TLMessageActionChannelCreate;
            if (action != null)
            {
                Title = action.Title;
            }
        }
    }

    public class TLMessageActionToggleComments : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionToggleComments;

        public TLBool Enabled { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Enabled = GetObject<TLBool>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Enabled = GetObject<TLBool>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Enabled.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
            var action = newAction as TLMessageActionToggleComments;
            if (action != null)
            {
                Enabled = action.Enabled;
            }
        }
    }

    public class TLMessageActionChatEditTitle : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatEditTitle;

        public TLString Title { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Title = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Title = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Title.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
            var action = newAction as TLMessageActionChatEditTitle;
            if (action != null)
            {
                Title = action.Title;
            }
        }
    }

    public class TLMessageActionChatEditPhoto : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatEditPhoto;

        //public TLPhotoBase Photo { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Photo = GetObject<TLPhotoBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Photo = GetObject<TLPhotoBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Photo.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
            var action = newAction as TLMessageActionChatEditPhoto;
            if (action != null)
            {
                if (Photo != null)
                {
                    Photo.Update(action.Photo);
                }
                else
                {
                    Photo = action.Photo;
                }
            }
        }
    }

    public class TLMessageActionChatDeletePhoto : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatDeletePhoto;

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

        public override void Update(TLMessageActionBase newAction)
        {
            
        }
    }

    public class TLMessageActionChannelJoined : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChannelJoined;

        public TLInt InviterId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            InviterId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            InviterId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            InviterId.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
            var action = newAction as TLMessageActionChannelJoined;
            if (action != null)
            {
                InviterId = action.InviterId;
            }
        }
    }

    public abstract class TLMessageActionChatAddUserBase : TLMessageActionBase
    {
        
    }

    public class TLMessageActionChatAddUser : TLMessageActionChatAddUserBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatAddUser;

        public TLInt UserId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
            var action = newAction as TLMessageActionChatAddUser;
            if (action != null)
            {
                UserId = action.UserId;
            }
        }
    }

    public class TLMessageActionChatAddUser41 : TLMessageActionChatAddUserBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatAddUser41;

        public TLVector<TLInt> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Users = GetObject<TLVector<TLInt>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Users = GetObject<TLVector<TLInt>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Users.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
            var action = newAction as TLMessageActionChatAddUser41;
            if (action != null)
            {
                Users = action.Users;
            }
        }
    }

    public class TLMessageActionChatDeleteUser : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatDeleteUser;

        public TLInt UserId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
            var action = newAction as TLMessageActionChatDeleteUser;
            if (action != null)
            {
                UserId = action.UserId;
            }
        }
    }

    public class TLMessageActionChatJoinedByLink : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatJoinedByLink;

        public TLInt InviterId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            InviterId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            InviterId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            InviterId.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionUnreadMessages : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionUnreadMessages;

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

        public override void Update(TLMessageActionBase newAction)
        {
            
        }
    }

    public class TLMessageActionContactRegistered : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionContactRegistered;

        public TLInt UserId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            UserId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            UserId.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionMessageGroup : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionMessageGroup;

        public TLMessageGroup Group { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Group = GetObject<TLMessageGroup>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Group = GetObject<TLMessageGroup>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Group.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionChatMigrateTo : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatMigrateTo;

        public TLInt ChannelId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            ChannelId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            ChannelId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            ChannelId.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionChatDeactivate : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatDeactivate;

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

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionChatActivate : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChatActivate;

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

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionChannelMigrateFrom : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionChannelMigrateFrom;

        public TLString Title { get; set; }

        public TLInt ChatId { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Title = GetObject<TLString>(bytes, ref position);
            ChatId = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Title = GetObject<TLString>(input);
            ChatId = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Title.ToStream(output);
            ChatId.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionPinMessage : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionPinMessage;

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

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionClearHistory : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionClearHistory;

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

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionGameScore : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionGameScore;

        public TLLong GameId { get; set; }

        public TLInt Score { get; set; }

        public override string ToString()
        {
            return string.Format("{0} game_id={1} score={2}", GetType().Name, GameId, Score);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            GameId = GetObject<TLLong>(bytes, ref position);
            Score = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            GameId = GetObject<TLLong>(input);
            Score = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            GameId.ToStream(output);
            Score.ToStream(output);
        }

        public override void Update(TLMessageActionBase action)
        {
            var actionGameScore = action as TLMessageActionGameScore;
            if (actionGameScore != null)
            {
                GameId = actionGameScore.GameId;
                Score = actionGameScore.Score;
            }
        }
    }

    public class TLMessageActionPhoneCall : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionPhoneCall;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLLong CallId { get; set; }

        protected TLPhoneCallDiscardReasonBase _reason;

        public TLPhoneCallDiscardReasonBase Reason
        {
            get { return _reason; }
            set { SetField(out _reason, value, ref _flags, (int) MessageActionPhoneCallFlags.Reason); }
        }

        protected TLInt _duration;

        public TLInt Duration
        {
            get { return _duration; }
            set { SetField(out _duration, value, ref _flags, (int) MessageActionPhoneCallFlags.Duration); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            CallId = GetObject<TLLong>(bytes, ref position);
            _reason = GetObject<TLPhoneCallDiscardReasonBase>(Flags, (int) MessageActionPhoneCallFlags.Reason, null, bytes, ref position);
            _duration = GetObject<TLInt>(Flags, (int) MessageActionPhoneCallFlags.Duration, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            CallId = GetObject<TLLong>(input);
            _reason = GetObject<TLPhoneCallDiscardReasonBase>(Flags, (int) MessageActionPhoneCallFlags.Reason, null, input);
            _duration = GetObject<TLInt>(Flags, (int) MessageActionPhoneCallFlags.Duration, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            CallId.ToStream(output);
            ToStream(output, Reason, Flags, (int) MessageActionPhoneCallFlags.Reason);
            ToStream(output, Duration, Flags, (int) MessageActionPhoneCallFlags.Duration);
        }

        public override void Update(TLMessageActionBase action)
        {
            var actionPhoneCall = action as TLMessageActionPhoneCall;
            if (actionPhoneCall != null)
            {
                CallId = actionPhoneCall.CallId;
                Reason = actionPhoneCall.Reason;
                Duration = actionPhoneCall.Duration;
            }
        }
    }

    public abstract class TLMessageActionPaymentSentBase : TLMessageActionBase
    {
        public TLString Currency { get; set; }

        public TLLong TotalAmount { get; set; }
    }

    public class TLMessageActionPaymentSentMe : TLMessageActionPaymentSentBase
    {
        public const uint Signature = TLConstructors.TLMessageActionPaymentSentMe;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLString Payload { get; set; }

        protected TLPaymentRequestedInfo _info;

        public TLPaymentRequestedInfo Info
        {
            get { return _info; }
            set { SetField(out _info, value, ref _flags, (int) MessageActionPaymentSentMeFlags.Info); }
        }

        protected TLString _shippingOptionId;

        public TLString ShippingOptionId
        {
            get { return _shippingOptionId; }
            set { SetField(out _shippingOptionId, value, ref _flags, (int) MessageActionPaymentSentMeFlags.ShippingOptionId); }
        }

        public TLPaymentCharge Charge { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Currency = GetObject<TLString>(bytes, ref position);
            TotalAmount = GetObject<TLLong>(bytes, ref position);
            Payload = GetObject<TLString>(bytes, ref position);
            _info = GetObject<TLPaymentRequestedInfo>(Flags, (int) MessageActionPaymentSentMeFlags.Info, null, bytes, ref position);
            _shippingOptionId = GetObject<TLString>(Flags, (int) MessageActionPaymentSentMeFlags.ShippingOptionId, null, bytes, ref position);
            Charge = GetObject<TLPaymentCharge>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Currency = GetObject<TLString>(input);
            TotalAmount = GetObject<TLLong>(input);
            Payload = GetObject<TLString>(input);
            _info = GetObject<TLPaymentRequestedInfo>(Flags, (int) MessageActionPaymentSentMeFlags.Info, null, input);
            _shippingOptionId = GetObject<TLString>(Flags, (int) MessageActionPaymentSentMeFlags.ShippingOptionId, null, input);
            Charge = GetObject<TLPaymentCharge>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Currency.ToStream(output);
            TotalAmount.ToStream(output);
            Payload.ToStream(output);
            ToStream(output, _info, Flags, (int) MessageActionPaymentSentMeFlags.Info);
            ToStream(output, _shippingOptionId, Flags, (int) MessageActionPaymentSentMeFlags.ShippingOptionId);
            Charge.ToStream(output);
        }

        public override void Update(TLMessageActionBase action)
        {
            var actionPaymentSentMe = action as TLMessageActionPaymentSentMe;
            if (actionPaymentSentMe != null)
            {
                Currency = actionPaymentSentMe.Currency;
                TotalAmount = actionPaymentSentMe.TotalAmount;
                Payload = actionPaymentSentMe.Payload;
                Info = actionPaymentSentMe.Info;
                ShippingOptionId = actionPaymentSentMe.ShippingOptionId;
                Charge = actionPaymentSentMe.Charge;
            }
        }
    }

    public class TLMessageActionPaymentSent : TLMessageActionPaymentSentBase
    {
        public const uint Signature = TLConstructors.TLMessageActionPaymentSent;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Currency = GetObject<TLString>(bytes, ref position);
            TotalAmount = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Currency = GetObject<TLString>(input);
            TotalAmount = GetObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Currency.ToStream(output);
            TotalAmount.ToStream(output);
        }

        public override void Update(TLMessageActionBase action)
        {
            var actionPaymentSent = action as TLMessageActionPaymentSent;
            if (actionPaymentSent != null)
            {
                Currency = actionPaymentSent.Currency;
                TotalAmount = actionPaymentSent.TotalAmount;
            }
        }
    }

    public class TLMessageActionScreenshotTaken : TLMessageActionPaymentSentBase
    {
        public const uint Signature = TLConstructors.TLMessageActionScreenshotTaken;

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

        public override void Update(TLMessageActionBase action)
        {

        }
    }

    public class TLMessageActionCustomAction : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionCustomAction;

        public TLString Message { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Message = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Message = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Message.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionBotAllowed : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionBotAllowed;

        public TLString Domain { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Domain = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Domain = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Domain.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionSecureValuesSentMe : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionSecureValuesSentMe;

        public TLVector<TLSecureValue> Values { get; set; }

        public TLSecureCredentialsEncrypted Credentials { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Values = GetObject<TLVector<TLSecureValue>>(bytes, ref position);
            Credentials = GetObject<TLSecureCredentialsEncrypted>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Values = GetObject<TLVector<TLSecureValue>>(input);
            Credentials = GetObject<TLSecureCredentialsEncrypted>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Values.ToStream(output);
            Credentials.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }

    public class TLMessageActionSecureValuesSent : TLMessageActionBase
    {
        public const uint Signature = TLConstructors.TLMessageActionSecureValuesSent;

        public TLVector<TLSecureValueTypeBase> Types { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Types = GetObject<TLVector<TLSecureValueTypeBase>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Types = GetObject<TLVector<TLSecureValueTypeBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Types.ToStream(output);
        }

        public override void Update(TLMessageActionBase newAction)
        {
        }
    }
}
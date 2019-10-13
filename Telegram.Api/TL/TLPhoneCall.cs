// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using Org.BouncyCastle.Security;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum PhoneCallWaitingFlags
    {
        ReceiveDate = 0x1,      // 0
    }

    [Flags]
    public enum PhoneCallDiscardedFlags
    {
        Reason = 0x1,           // 0
        Duration = 0x2,         // 1
        NeedDebug = 0x4,        // 2
        NeedRating = 0x8,       // 3
    }

    public abstract class TLPhoneCallBase : TLObject
    {
        public TLLong Id { get; set; }
    }

    public interface IInputPhoneCall
    {
        TLLong Id { get; }
        TLLong AccessHash { get; }
        TLInputPhoneCall ToInputPhoneCall();
    }

    public class TLPhoneCallEmpty : TLPhoneCallBase
    {
        public const uint Signature = TLConstructors.TLPhoneCallEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLPhoneCallEmpty id={0}", Id);
        }
    }

    public class TLPhoneCallWaiting : TLPhoneCallBase, IInputPhoneCall
    {
        public const uint Signature = TLConstructors.TLPhoneCallWaiting;

        public TLInt Flags { get; set; }

        public TLLong AccessHash { get; set; }

        public TLInt Date { get; set; }

        public TLInt AdminId { get; set; }

        public TLInt ParticipantId { get; set; }

        public TLPhoneCallProtocol Protocol { get; set; }

        public TLInt ReceiveDate { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            AdminId = GetObject<TLInt>(bytes, ref position);
            ParticipantId = GetObject<TLInt>(bytes, ref position);
            Protocol = GetObject<TLPhoneCallProtocol>(bytes, ref position);
            ReceiveDate = GetObject<TLInt>(Flags, (int) PhoneCallWaitingFlags.ReceiveDate, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Date.ToBytes(),
                AdminId.ToBytes(),
                ParticipantId.ToBytes(),
                Protocol.ToBytes(),
                ToBytes(ReceiveDate, Flags, (int) PhoneCallWaitingFlags.ReceiveDate));
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Id.ToStream(output);
            AccessHash.ToStream(output);
            Date.ToStream(output);
            AdminId.ToStream(output);
            ParticipantId.ToStream(output);
            Protocol.ToStream(output);
            ToStream(output, ReceiveDate, Flags, (int) PhoneCallWaitingFlags.ReceiveDate);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            AdminId = GetObject<TLInt>(input);
            ParticipantId = GetObject<TLInt>(input);
            Protocol = GetObject<TLPhoneCallProtocol>(input);
            ReceiveDate = GetObject<TLInt>(Flags, (int) PhoneCallWaitingFlags.ReceiveDate, null, input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLPhoneCallWaiting flags={0} id={1} access_hash={2} date={3} admin_id={4} participant_id={5} protocol={6} receive_date={7}", Flags, Id, AccessHash, Date, AdminId, ParticipantId, Protocol, ReceiveDate);
        }

        public TLInputPhoneCall ToInputPhoneCall()
        {
            return new TLInputPhoneCall { Id = Id, AccessHash = AccessHash };
        }
    }

    public class TLPhoneCallRequestedBase : TLPhoneCallBase, IInputPhoneCall
    {
        public TLLong AccessHash { get; set; }

        public TLInt Date { get; set; }

        public TLInt AdminId { get; set; }

        public TLInt ParticipantId { get; set; }

        public TLPhoneCallProtocol Protocol { get; set; }

        public TLInputPhoneCall ToInputPhoneCall()
        {
            return new TLInputPhoneCall { Id = Id, AccessHash = AccessHash };
        }
    }

    public class TLPhoneCallRequested64 : TLPhoneCallRequestedBase
    {
        public const uint Signature = TLConstructors.TLPhoneCallRequested64;

        public TLString GAHash { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            AdminId = GetObject<TLInt>(bytes, ref position);
            ParticipantId = GetObject<TLInt>(bytes, ref position);
            GAHash = GetObject<TLString>(bytes, ref position);
            Protocol = GetObject<TLPhoneCallProtocol>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Date.ToBytes(),
                AdminId.ToBytes(),
                ParticipantId.ToBytes(),
                GAHash.ToBytes(),
                Protocol.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
            AccessHash.ToStream(output);
            Date.ToStream(output);
            AdminId.ToStream(output);
            ParticipantId.ToStream(output);
            GAHash.ToStream(output);
            Protocol.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            AdminId = GetObject<TLInt>(input);
            ParticipantId = GetObject<TLInt>(input);
            GAHash = GetObject<TLString>(input);
            Protocol = GetObject<TLPhoneCallProtocol>(input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLPhoneCallRequested id={0} access_hash={1} date={2} admin_id={3} participant_id={4} protocol={5}", Id, AccessHash, Date, AdminId, ParticipantId, Protocol);
        }
    }

    public class TLPhoneCallRequested : TLPhoneCallRequestedBase
    {
        public const uint Signature = TLConstructors.TLPhoneCallRequested;

        public TLString GA { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            AdminId = GetObject<TLInt>(bytes, ref position);
            ParticipantId = GetObject<TLInt>(bytes, ref position);
            GA = GetObject<TLString>(bytes, ref position);
            Protocol = GetObject<TLPhoneCallProtocol>(bytes, ref position);
            
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Date.ToBytes(),
                AdminId.ToBytes(),
                ParticipantId.ToBytes(),
                GA.ToBytes(),
                Protocol.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
            AccessHash.ToStream(output);
            Date.ToStream(output);
            AdminId.ToStream(output);
            ParticipantId.ToStream(output);
            GA.ToStream(output);
            Protocol.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            AdminId = GetObject<TLInt>(input);
            ParticipantId = GetObject<TLInt>(input);
            GA = GetObject<TLString>(input);
            Protocol = GetObject<TLPhoneCallProtocol>(input);

            return this;
        }
    }

    public class TLPhoneCall : TLPhoneCallBase, IInputPhoneCall
    {
        public const uint Signature = TLConstructors.TLPhoneCall;

        public TLLong AccessHash { get; set; }

        public TLInt Date { get; set; }

        public TLInt AdminId { get; set; }

        public TLInt ParticipantId { get; set; }

        public TLString GAorB { get; set; }

        public TLLong KeyFingerprint { get; set; }

        public TLPhoneCallProtocol Protocol { get; set; }

        public TLPhoneConnection Connection { get; set; }

        public TLVector<TLPhoneConnection> AlternativeConnections { get; set; }

        public TLInt StartDate { get; set; }

        public override string ToString()
        {
            return string.Format("TLPhoneCall id={0} access_hash={1} date={2} admin_id={3} participant_id={4} protocol={5} start_date={6}", Id, AccessHash, Date, AdminId, ParticipantId, Protocol, StartDate);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            AdminId = GetObject<TLInt>(bytes, ref position);
            ParticipantId = GetObject<TLInt>(bytes, ref position);
            GAorB = GetObject<TLString>(bytes, ref position);
            KeyFingerprint = GetObject<TLLong>(bytes, ref position);
            Protocol = GetObject<TLPhoneCallProtocol>(bytes, ref position);
            Connection = GetObject<TLPhoneConnection>(bytes, ref position);
            AlternativeConnections = GetObject<TLVector<TLPhoneConnection>>(bytes, ref position);
            StartDate = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Date.ToBytes(),
                AdminId.ToBytes(),
                ParticipantId.ToBytes(),
                GAorB.ToBytes(),
                KeyFingerprint.ToBytes(),
                Protocol.ToBytes(),
                Connection.ToBytes(),
                AlternativeConnections.ToBytes(),
                StartDate.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
            AccessHash.ToStream(output);
            Date.ToStream(output);
            AdminId.ToStream(output);
            ParticipantId.ToStream(output);
            GAorB.ToStream(output);
            KeyFingerprint.ToStream(output);
            Protocol.ToStream(output);
            Connection.ToStream(output);
            AlternativeConnections.ToStream(output);
            StartDate.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            AdminId = GetObject<TLInt>(input);
            ParticipantId = GetObject<TLInt>(input);
            GAorB = GetObject<TLString>(input);
            KeyFingerprint = GetObject<TLLong>(input);
            Protocol = GetObject<TLPhoneCallProtocol>(input);
            Connection = GetObject<TLPhoneConnection>(input);
            AlternativeConnections = GetObject<TLVector<TLPhoneConnection>>(input);
            StartDate = GetObject<TLInt>(input);

            return this;
        }

        public TLInputPhoneCall ToInputPhoneCall()
        {
            return new TLInputPhoneCall { Id = Id, AccessHash = AccessHash };
        }
    }

    public class TLPhoneCallDiscarded : TLPhoneCallBase
    {
        public const uint Signature = TLConstructors.TLPhoneCallDiscarded;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);

            return this;
        }
    }

    public class TLPhoneCallDiscarded61 : TLPhoneCallDiscarded
    {
        public new const uint Signature = TLConstructors.TLPhoneCallDiscarded61;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLPhoneCallDiscardReasonBase Reason { get; set; }

        public TLInt Duration { get; set; }

        public bool NeedDebug
        {
            get { return IsSet(Flags, (int) PhoneCallDiscardedFlags.NeedDebug); }
            set { SetUnset(ref _flags, value, (int) PhoneCallDiscardedFlags.NeedDebug); }
        }

        public bool NeedRating
        {
            get { return IsSet(Flags, (int) PhoneCallDiscardedFlags.NeedRating); }
            set { SetUnset(ref _flags, value, (int) PhoneCallDiscardedFlags.NeedRating); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLLong>(bytes, ref position);
            Reason = GetObject<TLPhoneCallDiscardReasonBase>(Flags, (int) PhoneCallDiscardedFlags.Reason, null, bytes, ref position);
            Duration = GetObject<TLInt>(Flags, (int) PhoneCallDiscardedFlags.Duration, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Id.ToBytes(),
                ToBytes(Reason, Flags, (int) PhoneCallDiscardedFlags.Reason),
                ToBytes(Duration, Flags, (int) PhoneCallDiscardedFlags.Duration));
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Id.ToStream(output);
            ToStream(output, Reason, Flags, (int) PhoneCallDiscardedFlags.Reason);
            ToStream(output, Duration, Flags, (int) PhoneCallDiscardedFlags.Duration);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Id = GetObject<TLLong>(input);
            Reason = GetObject<TLPhoneCallDiscardReasonBase>(Flags, (int) PhoneCallDiscardedFlags.Reason, null, input);
            Duration = GetObject<TLInt>(Flags, (int) PhoneCallDiscardedFlags.Duration, null, input);

            return this;
        }

        public override string ToString()
        {
            return string.Format("TLPhoneCallDiscarded id={0} reason={1} duration={2}", Id, Reason, Duration);
        }
    }

    public class TLPhoneCallAccepted : TLPhoneCallBase, IInputPhoneCall
    {
        public const uint Signature = TLConstructors.TLPhoneCallAccepted;

        public TLLong AccessHash { get; set; }

        public TLInt Date { get; set; }

        public TLInt AdminId { get; set; }

        public TLInt ParticipantId { get; set; }

        public TLPhoneCallProtocol Protocol { get; set; }

        public TLString GB { get; set; }

        public override string ToString()
        {
            return string.Format("TLPhoneAccepted id={0} access_hash={1} date={2} admin_id={3} participant_id={4} protocol={5}", Id, AccessHash, Date, AdminId, ParticipantId, Protocol);
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            AccessHash = GetObject<TLLong>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            AdminId = GetObject<TLInt>(bytes, ref position);
            ParticipantId = GetObject<TLInt>(bytes, ref position);
            GB = GetObject<TLString>(bytes, ref position);
            Protocol = GetObject<TLPhoneCallProtocol>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes(),
                AccessHash.ToBytes(),
                Date.ToBytes(),
                AdminId.ToBytes(),
                ParticipantId.ToBytes(),
                GB.ToBytes(),
                Protocol.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Id.ToStream(output);
            AccessHash.ToStream(output);
            Date.ToStream(output);
            AdminId.ToStream(output);
            ParticipantId.ToStream(output);
            GB.ToStream(output);
            Protocol.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            AccessHash = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            AdminId = GetObject<TLInt>(input);
            ParticipantId = GetObject<TLInt>(input);
            GB = GetObject<TLString>(input);
            Protocol = GetObject<TLPhoneCallProtocol>(input);

            return this;
        }

        public TLInputPhoneCall ToInputPhoneCall()
        {
            return new TLInputPhoneCall { Id = Id, AccessHash = AccessHash };
        }
    }
}

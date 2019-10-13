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
    public abstract class TLChannelParticipantsBase : TLObject { }

    public class TLChannelParticipants : TLChannelParticipantsBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipants;

        public TLInt Count { get; set; }

        public TLVector<TLChannelParticipantBase> Participants { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Count = GetObject<TLInt>(bytes, ref position);
            Participants = GetObject<TLVector<TLChannelParticipantBase>>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Count = GetObject<TLInt>(input);
            Participants = GetObject<TLVector<TLChannelParticipantBase>>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Count.ToStream(output);
            Participants.ToStream(output);
            Users.ToStream(output);
        }
    }

    public class TLChannelParticipantsNotModified : TLChatParticipantsBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantsNotModified;

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
    }

    public class TLChannelsChannelParticipant : TLObject
    {
        public const uint Signature = TLConstructors.TLChannelsChannelParticipant;

        public TLChannelParticipantBase Participant { get; set; }

        public TLVector<TLUserBase> Users { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Participant = GetObject<TLChannelParticipantBase>(bytes, ref position);
            Users = GetObject<TLVector<TLUserBase>>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Participant = GetObject<TLChannelParticipantBase>(input);
            Users = GetObject<TLVector<TLUserBase>>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Participant.ToStream(output);
            Users.ToStream(output);
        }
    }
}

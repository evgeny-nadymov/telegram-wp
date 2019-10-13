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
    public abstract class TLChannelParticipantsFilterBase : TLObject { }

    public class TLChannelParticipantsRecent : TLChannelParticipantsFilterBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantsRecent;

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
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

    public class TLChannelParticipantsAdmins : TLChannelParticipantsFilterBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantsAdmins;

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
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

    public class TLChannelParticipantsKicked68 : TLChannelParticipantsFilterBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantsKicked68;

        public TLString Q { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Q.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Q.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Q = GetObject<TLString>(input);

            return this;
        }
    }

    public class TLChannelParticipantsKicked : TLChannelParticipantsFilterBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantsKicked;

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
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

    public class TLChannelParticipantsBots : TLChannelParticipantsFilterBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantsBots;

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
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

    public class TLChannelParticipantsBanned : TLChannelParticipantsFilterBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantsBanned;

        public TLString Q { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Q.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Q.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Q = GetObject<TLString>(input);

            return this;
        }
    }

    public class TLChannelParticipantsSearch : TLChannelParticipantsFilterBase
    {
        public const uint Signature = TLConstructors.TLChannelParticipantsSearch;

        public TLString Q { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Q.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Q.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Q = GetObject<TLString>(input);

            return this;
        }
    }
}

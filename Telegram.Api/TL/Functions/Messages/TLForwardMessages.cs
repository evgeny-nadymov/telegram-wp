// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL.Functions.Messages
{
    public class TLForwardMessages : TLObject, IRandomId
    {
        public const uint Signature = 0x708e0195;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInputPeerBase FromPeer { get; set; }

        public TLInputPeerBase ToPeer { get; set; }

        public TLVector<TLInt> Id { get; set; }

        public TLVector<TLLong> RandomIds { get; set; }

        public TLLong RandomId
        {
            get
            {
                if (RandomIds != null && RandomIds.Count > 0)
                {
                    return RandomIds[0];
                }

                return new TLLong(0);
            }
        }

        public void SetChannelMessage()
        {
            Set(ref _flags, (int) SendFlags.Channel);
        }

        public void SetSilent()
        {
            Set(ref _flags, (int) SendFlags.Silent);
        }

        public void SetWithMyScore()
        {
            Set(ref _flags, (int) SendFlags.WithMyScore);
        }

        public void SetGrouped()
        {
            Set(ref _flags, (int)SendFlags.Grouped);
        }

        public static string ForwardMessagesFlagsString(TLInt flags)
        {
            if (flags == null) return string.Empty;

            var list = (SendFlags) flags.Value;

            return string.Format("{0} [{1}]", flags, list);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                FromPeer.ToBytes(),
                Id.ToBytes(),
                RandomIds.ToBytes(),
                ToPeer.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            FromPeer.ToStream(output);
            Id.ToStream(output);
            RandomIds.ToStream(output);
            ToPeer.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {

            Flags = GetObject<TLInt>(input);
            FromPeer = GetObject<TLInputPeerBase>(input);
            Id = GetObject<TLVector<TLInt>>(input);
            RandomIds = GetObject<TLVector<TLLong>>(input);
            ToPeer = GetObject<TLInputPeerBase>(input);

            return this;
        }
    }
}

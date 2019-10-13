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

namespace Telegram.Api.TL.Functions.Channels
{
    [Flags]
    public enum UpdatePinnedMessageFlags
    {
        Silent = 0x1,
    }

    class TLUpdatePinnedMessage : TLObject
    {
        public const uint Signature = 0xa72ded52;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLInputChannelBase Channel { get; set; }

        public TLInt Id { get; set; }

        public void SetSilent()
        {
            Set(ref _flags, (int)UpdatePinnedMessageFlags.Silent);
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Channel.ToBytes(),
                Id.ToBytes()
                );
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Channel.ToStream(output);
            Id.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Channel = GetObject<TLInputChannelBase>(input);
            Id = GetObject<TLInt>(input);

            return this;
        }
    }
}

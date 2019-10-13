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
    public abstract class TLInputMessageBase :  TLObject { }

    public class TLInputMessageId : TLInputMessageBase
    {
        public const uint Signature = TLConstructors.TLInputMessageId;

        public TLInt Id { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLInputMessageId id={0}", Id);
        }
    }

    public class TLInputMessageReplyTo : TLInputMessageBase
    {
        public const uint Signature = TLConstructors.TLInputMessageReplyTo;

        public TLInt Id { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Id.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLInputMessageId id={0}", Id);
        }
    }

    public class TLInputMessagePinned : TLInputMessageBase
    {
        public const uint Signature = TLConstructors.TLInputMessagePinned;

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }

        public override string ToString()
        {
            return "TLInputMessagePinned";
        }
    }
}

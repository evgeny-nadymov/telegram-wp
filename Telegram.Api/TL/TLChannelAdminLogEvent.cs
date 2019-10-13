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
    public class TLChannelAdminLogEvent : TLObject
    {
        public const uint Signature = TLConstructors.TLChannelAdminLogEvent;

        public TLLong Id { get; set; }

        public TLInt Date { get; set; }

        public TLInt UserId { get; set; }

        public TLChannelAdminLogEventActionBase Action { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Id = GetObject<TLLong>(bytes, ref position);
            Date = GetObject<TLInt>(bytes, ref position);
            UserId = GetObject<TLInt>(bytes, ref position);
            Action = GetObject<TLChannelAdminLogEventActionBase>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Id = GetObject<TLLong>(input);
            Date = GetObject<TLInt>(input);
            UserId = GetObject<TLInt>(input);
            Action = GetObject<TLChannelAdminLogEventActionBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Id.ToStream(output);
            Date.ToStream(output);
            UserId.ToStream(output);
            Action.ToStream(output);
        }
    }
}

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
    public abstract class TLInputReportReasonBase : TLObject { }

    public class TLInputReportReasonSpam : TLInputReportReasonBase
    {
        public const uint Signature = TLConstructors.TLInputReportReasonSpam;

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

    public class TLInputReportReasonViolence : TLInputReportReasonBase
    {
        public const uint Signature = TLConstructors.TLInputReportReasonViolence;

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

    public class TLInputReportReasonPornography : TLInputReportReasonBase
    {
        public const uint Signature = TLConstructors.TLInputReportReasonPornography;

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

    public class TLInputReportReasonCopyright : TLInputReportReasonBase
    {
        public const uint Signature = TLConstructors.TLInputReportReasonCopyright;

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

    public class TLInputReportReasonOther : TLInputReportReasonBase
    {
        public const uint Signature = TLConstructors.TLInputReportReasonOther;

        public TLString Text { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(TLUtils.SignatureToBytes(Signature), Text.ToBytes());
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Text.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }
    }
}

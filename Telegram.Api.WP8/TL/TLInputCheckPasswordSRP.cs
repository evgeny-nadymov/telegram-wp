// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLInputCheckPasswordBase : TLObject { }

    public class TLInputCheckPasswordEmpty : TLInputCheckPasswordBase
    {
        public const uint Signature = TLConstructors.TLInputCheckPasswordEmpty;

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputCheckPasswordSRP : TLInputCheckPasswordBase
    {
        public const uint Signature = TLConstructors.TLInputCheckPasswordSRP;

        public TLLong SRPId { get; set; }

        public TLString A { get; set; }

        public TLString M1 { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                SRPId.ToBytes(),
                A.ToBytes(),
                M1.ToBytes());
        }
    }
}

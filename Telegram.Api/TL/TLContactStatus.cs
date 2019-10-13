// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLContactStatusBase : TLObject
    {
        public TLInt UserId { get; set; }
    }

    public class TLContactStatus : TLContactStatusBase
    {
        public const uint Signature = TLConstructors.TLContactStatus;

        public TLInt Expires { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            Expires = GetObject<TLInt>(bytes, ref position);

            return this;
        }
    }

    public class TLContactStatus19 : TLContactStatusBase
    {
        public const uint Signature = TLConstructors.TLContactStatus19;

        public TLUserStatus Status { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);
            Status = GetObject<TLUserStatus>(bytes, ref position);

            return this;
        }
    }
}

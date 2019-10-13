// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLContactBlocked : TLObject
    {
        public const uint Signature = TLConstructors.TLContactBlocked;

        public TLInt UserId { get; set; }

        public TLInt Date { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            TLUtils.WriteLine("--Parse TLContactBlocked--");
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            UserId = GetObject<TLInt>(bytes, ref position);

            Date = GetObject<TLInt>(bytes, ref position);

            TLUtils.WriteLine("UserId: " + UserId);
            TLUtils.WriteLine("Date: " + TLUtils.MessageIdString(Date));

            return this;
        }
    }
}

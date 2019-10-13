// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLAppChangelogBase : TLObject { }

    public class TLAppChangelogEmpty : TLAppChangelogBase
    {
        public const uint Signature = TLConstructors.TLAppChangelogEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }
    }

    public class TLAppChangelog : TLAppChangelogBase
    {
        public const uint Signature = TLConstructors.TLAppChangelog;

        public TLString Text { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Text = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLAppChangelog59 : TLAppChangelogBase
    {
        public const uint Signature = TLConstructors.TLAppChangelog59;

        public TLString Message { get; set; }

        public TLMessageMediaBase Media { get; set; }

        public TLVector<TLMessageEntityBase> Entities { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Message = GetObject<TLString>(bytes, ref position);
            Media = GetObject<TLMessageMediaBase>(bytes, ref position);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);

            return this;
        }
    }
}

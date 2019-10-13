// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public abstract class TLDHConfigBase : TLObject { }

    public class TLDHConfig : TLDHConfigBase
    {
        public const uint Signature = TLConstructors.TLDHConfig;

        public TLInt G { get; set; }

        public TLString P { get; set; }

        public TLInt Version { get; set; }

        public TLString Random { get; set; }

        #region Additional

        public TLString A { get; set; }

        public TLString GA { get; set; }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            G = GetObject<TLInt>(bytes, ref position);
            P = GetObject<TLString>(bytes, ref position);
            Version = GetObject<TLInt>(bytes, ref position);
            Random = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }

    public class TLDHConfigNotModified : TLDHConfigBase
    {
        public const uint Signature = TLConstructors.TLDHConfigNotModified;

        public TLString Random { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Random = GetObject<TLString>(bytes, ref position);

            return this;
        }
    }
}

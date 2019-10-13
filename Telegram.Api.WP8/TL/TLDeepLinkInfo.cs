// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;

namespace Telegram.Api.TL
{
    [Flags]
    public enum DeepLinkInfoFlags
    {
        UpdateApp = 0x1,    // 0
        Entities = 0x2,     // 1
    }

    public abstract class TLDeepLinkInfoBase : TLObject { }

    public class TLDeepLinkInfoEmpty : TLDeepLinkInfoBase
    {
        public const uint Signature = TLConstructors.TLDeepLinkInfoEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }
    }

    public class TLDeepLinkInfo : TLDeepLinkInfoBase
    {
        public const uint Signature = TLConstructors.TLDeepLinkInfo;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool UpdateApp { get { return IsSet(Flags, (int)DeepLinkInfoFlags.UpdateApp); } }

        public TLString Message { get; set; }

        protected TLVector<TLMessageEntityBase> _entities;

        public TLVector<TLMessageEntityBase> Entities
        {
            get { return _entities; }
            set { SetField(out _entities, value, ref _flags, (int)DeepLinkInfoFlags.Entities); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            _flags = GetObject<TLInt>(bytes, ref position);
            Message = GetObject<TLString>(bytes, ref position);
            _entities = GetObject<TLVector<TLMessageEntityBase>>(Flags, (int)DeepLinkInfoFlags.Entities, null, bytes, ref position);

            return this;
        }
    }
}

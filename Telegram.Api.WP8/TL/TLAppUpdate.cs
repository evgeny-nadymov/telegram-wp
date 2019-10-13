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
    public enum AppUpdateFlags
    {
        Popup = 0x1,            // 0
        Document = 0x2,         // 1
        Url = 0x4,              // 2
    }

    public abstract class TLAppUpdateBase : TLObject { }

    public class TLNoAppUpdate : TLAppUpdateBase
    {
        public const uint Signature = TLConstructors.TLNoAppUpdate;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }
    }

    public class TLAppUpdate : TLAppUpdateBase
    {
        public const uint Signature = TLConstructors.TLAppUpdate;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }
        public bool Popup
        {
            get { return IsSet(_flags, (int)AppUpdateFlags.Popup); }
            set { SetUnset(ref _flags, value, (int)AppUpdateFlags.Popup); }
        }

        public TLInt Id { get; set; }

        public TLString Version { get; set; }

        public TLString Text { get; set; }

        public TLVector<TLMessageEntityBase> Entities { get; set; }

        protected TLDocumentBase _document;

        public TLDocumentBase Document
        {
            get { return _document; }
            set { SetField(out _document, value, ref _flags, (int)AppUpdateFlags.Document); }
        }

        protected TLString _url;

        public TLString Url
        {
            get { return _url; }
            set { SetField(out _url, value, ref _flags, (int)AppUpdateFlags.Url); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Id = GetObject<TLInt>(bytes, ref position);
            Version = GetObject<TLString>(bytes, ref position);
            Text = GetObject<TLString>(bytes, ref position);
            Entities = GetObject<TLVector<TLMessageEntityBase>>(bytes, ref position);
            _document = GetObject<TLDocumentBase>(Flags, (int)AppUpdateFlags.Document, null, bytes, ref position);
            _url = GetObject<TLString>(Flags, (int)AppUpdateFlags.Url, null, bytes, ref position);

            return this;
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using Telegram.Api.Extensions;

namespace Telegram.Api.TL
{
    [Flags]
    public enum ContactsSettingsFlags
    {
        SuggestFrequentContacts = 0x1,        // 0 obsolete
    }

    public class TLContactsSettings : TLObject
    {
        public const uint Signature = TLConstructors.TLContactsSettings;

        private TLLong _flags;

        public TLLong Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLLong>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(TLUtils.SignatureToBytes(Signature), Flags.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLLong>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
        }
    }
}
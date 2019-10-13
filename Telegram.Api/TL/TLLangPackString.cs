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
    public enum LangPackStringPluralizedFlags
    {
        ZeroValue = 0x1,                // 0
        OneValue = 0x2,                 // 1
        TwoValue = 0x4,                 // 2
        FewValue = 0x8,                 // 3
        ManyValue = 0x10,               // 4
        OtherValue = 0x20,              // 5
    }

    public abstract class TLLangPackStringBase : TLObject { }

    public class TLLangPackString : TLLangPackStringBase
    {
        public const uint Signature = TLConstructors.TLLangPackString;

        public TLString Key { get; set; }

        public TLString Value { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Key = GetObject<TLString>(bytes, ref position);
            Value = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Key.ToStream(output);
            Value.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Key = GetObject<TLString>(input);
            Value = GetObject<TLString>(input);

            return this;
        }
    }

    public class TLLangPackStringPluralized : TLLangPackStringBase
    {
        public const uint Signature = TLConstructors.TLLangPackStringPluralized;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLString Key { get; set; }

        private TLString _zeroValue;

        public TLString ZeroValue
        {
            get { return _zeroValue; }
            set { SetField(out _zeroValue, value, ref _flags, (int) LangPackStringPluralizedFlags.ZeroValue); }
        }

        private TLString _oneValue;

        public TLString OneValue
        {
            get { return _oneValue; }
            set { SetField(out _oneValue, value, ref _flags, (int)LangPackStringPluralizedFlags.OneValue); }
        }

        private TLString _twoValue;

        public TLString TwoValue
        {
            get { return _twoValue; }
            set { SetField(out _twoValue, value, ref _flags, (int)LangPackStringPluralizedFlags.TwoValue); }
        }

        private TLString _fewValue;

        public TLString FewValue
        {
            get { return _fewValue; }
            set { SetField(out _fewValue, value, ref _flags, (int)LangPackStringPluralizedFlags.FewValue); }
        }

        private TLString _manyValue;

        public TLString ManyValue
        {
            get { return _manyValue; }
            set { SetField(out _manyValue, value, ref _flags, (int)LangPackStringPluralizedFlags.ManyValue); }
        }

        private TLString _otherValue;

        public TLString OtherValue
        {
            get { return _otherValue; }
            set { SetField(out _otherValue, value, ref _flags, (int)LangPackStringPluralizedFlags.OtherValue); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Key = GetObject<TLString>(bytes, ref position);
            ZeroValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.ZeroValue, null, bytes, ref position);
            OneValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.OneValue, null, bytes, ref position);
            TwoValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.TwoValue, null, bytes, ref position);
            FewValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.FewValue, null, bytes, ref position);
            ManyValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.ManyValue, null, bytes, ref position);
            OtherValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.OtherValue, null, bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Key.ToStream(output);
            ToStream(output, ZeroValue, Flags, (int)LangPackStringPluralizedFlags.ZeroValue);
            ToStream(output, OneValue, Flags, (int)LangPackStringPluralizedFlags.OneValue);
            ToStream(output, TwoValue, Flags, (int)LangPackStringPluralizedFlags.TwoValue);
            ToStream(output, FewValue, Flags, (int)LangPackStringPluralizedFlags.FewValue);
            ToStream(output, ManyValue, Flags, (int)LangPackStringPluralizedFlags.ManyValue);
            ToStream(output, OtherValue, Flags, (int)LangPackStringPluralizedFlags.OtherValue);
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Key = GetObject<TLString>(input);
            ZeroValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.ZeroValue, null, input);
            OneValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.OneValue, null, input);
            TwoValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.TwoValue, null, input);
            FewValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.FewValue, null, input);
            ManyValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.ManyValue, null, input);
            OtherValue = GetObject<TLString>(Flags, (int)LangPackStringPluralizedFlags.OtherValue, null, input);

            return this;
        }
    }

    public class TLLangPackStringDeleted : TLLangPackStringBase
    {
        public const uint Signature = TLConstructors.TLLangPackStringDeleted;

        public TLString Key { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Key = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Key.ToStream(output);
        }

        public override TLObject FromStream(Stream input)
        {
            Key = GetObject<TLString>(input);

            return this;
        }
    }
}

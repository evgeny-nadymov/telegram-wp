// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLInputSecureValue : TLObject
    {
        public const uint Signature = TLConstructors.TLInputSecureValue;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public TLSecureValueTypeBase Type { get; set; }

        protected TLSecureData _data;

        public TLSecureData Data
        {
            get { return _data; }
            set { SetField(out _data, value, ref _flags, (int)SecureValueFlags.Data); }
        }

        protected TLInputSecureFileBase _frontSide;

        public TLInputSecureFileBase FrontSide
        {
            get { return _frontSide; }
            set { SetField(out _frontSide, value, ref _flags, (int)SecureValueFlags.FrontSide); }
        }

        protected TLInputSecureFileBase _reverseSide;

        public TLInputSecureFileBase ReverseSide
        {
            get { return _reverseSide; }
            set { SetField(out _reverseSide, value, ref _flags, (int)SecureValueFlags.ReverseSide); }
        }

        protected TLInputSecureFileBase _selfie;

        public TLInputSecureFileBase Selfie
        {
            get { return _selfie; }
            set { SetField(out _selfie, value, ref _flags, (int)SecureValueFlags.Selfie); }
        }

        protected TLVector<TLInputSecureFileBase> _files;

        public TLVector<TLInputSecureFileBase> Files
        {
            get { return _files; }
            set { SetField(out _files, value, ref _flags, (int)SecureValueFlags.Files); }
        }

        protected TLSecurePlainDataBase _plainData;

        public TLSecurePlainDataBase PlainData
        {
            get { return _plainData; }
            set { SetField(out _plainData, value, ref _flags, (int)SecureValueFlags.PlainData); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Type.ToBytes(),
                ToBytes(Data, Flags, (int)SecureValueFlags.Data),
                ToBytes(_frontSide, Flags, (int)SecureValueFlags.FrontSide),
                ToBytes(_reverseSide, Flags, (int)SecureValueFlags.ReverseSide),
                ToBytes(_selfie, Flags, (int)SecureValueFlags.Selfie),
                ToBytes(_files, Flags, (int)SecureValueFlags.Files),
                ToBytes(_plainData, Flags, (int)SecureValueFlags.PlainData));
        }
    }

    public class TLInputSecureValue85 : TLInputSecureValue
    {
        public new const uint Signature = TLConstructors.TLInputSecureValue85;

        protected TLVector<TLInputSecureFileBase> _translation;

        public TLVector<TLInputSecureFileBase> Translation
        {
            get { return _translation; }
            set { SetField(out _translation, value, ref _flags, (int)SecureValueFlags.Translation); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Type.ToBytes(),
                ToBytes(Data, Flags, (int)SecureValueFlags.Data),
                ToBytes(_frontSide, Flags, (int)SecureValueFlags.FrontSide),
                ToBytes(_reverseSide, Flags, (int)SecureValueFlags.ReverseSide),
                ToBytes(_selfie, Flags, (int)SecureValueFlags.Selfie),
                ToBytes(_translation, Flags, (int)SecureValueFlags.Translation),
                ToBytes(_files, Flags, (int)SecureValueFlags.Files),
                ToBytes(_plainData, Flags, (int)SecureValueFlags.PlainData));
        }
    }
}

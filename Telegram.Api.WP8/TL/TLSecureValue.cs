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
    public enum SecureValueFlags
    {
        Data = 0x1,         // 0
        FrontSide = 0x2,    // 1
        ReverseSide = 0x4,  // 2
        Selfie = 0x8,       // 3
        Files = 0x10,       // 4
        PlainData = 0x20,   // 5
        Translation = 0x40, // 6
    }

    public class TLSecureValue : TLObject
    {
        public const uint Signature = TLConstructors.TLSecureValue;

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

        protected TLSecureFileBase _frontSide;

        public TLSecureFileBase FrontSide
        {
            get { return _frontSide; }
            set { SetField(out _frontSide, value, ref _flags, (int)SecureValueFlags.FrontSide); }
        }

        protected TLSecureFileBase _reverseSide;

        public TLSecureFileBase ReverseSide
        {
            get { return _reverseSide; }
            set { SetField(out _reverseSide, value, ref _flags, (int)SecureValueFlags.ReverseSide); }
        }

        protected TLSecureFileBase _selfie;

        public TLSecureFileBase Selfie
        {
            get { return _selfie; }
            set { SetField(out _selfie, value, ref _flags, (int)SecureValueFlags.Selfie); }
        }

        protected TLVector<TLSecureFileBase> _files;

        public TLVector<TLSecureFileBase> Files
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

        public TLString Hash { get; set; }

        #region Additional
        public TLSecureValue Self { get { return this; } }
        #endregion

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            _data = GetObject<TLSecureData>(Flags, (int)SecureValueFlags.Data, null, bytes, ref position);
            _frontSide = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.FrontSide, null, bytes, ref position);
            _reverseSide = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.ReverseSide, null, bytes, ref position);
            _selfie = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.Selfie, null, bytes, ref position);
            _files = GetObject<TLVector<TLSecureFileBase>>(Flags, (int)SecureValueFlags.Files, null, bytes, ref position);
            _plainData = GetObject<TLSecurePlainDataBase>(Flags, (int)SecureValueFlags.PlainData, null, bytes, ref position);
            Hash = GetObject<TLString>(bytes, ref position);
            
            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Type = GetObject<TLSecureValueTypeBase>(input);
            _data = GetObject<TLSecureData>(Flags, (int)SecureValueFlags.Data, null, input);
            _frontSide = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.FrontSide, null, input);
            _reverseSide = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.ReverseSide, null, input);
            _selfie = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.Selfie, null, input);
            _files = GetObject<TLVector<TLSecureFileBase>>(Flags, (int)SecureValueFlags.Files, null, input);
            _plainData = GetObject<TLSecurePlainDataBase>(Flags, (int)SecureValueFlags.PlainData, null, input);
            Hash = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Type.ToStream(output);
            ToStream(output, _data, Flags, (int)SecureValueFlags.Data);
            ToStream(output, _frontSide, Flags, (int)SecureValueFlags.FrontSide);
            ToStream(output, _reverseSide, Flags, (int)SecureValueFlags.ReverseSide);
            ToStream(output, _selfie, Flags, (int)SecureValueFlags.Selfie);
            ToStream(output, _files, Flags, (int)SecureValueFlags.Files);
            ToStream(output, _plainData, Flags, (int)SecureValueFlags.PlainData);
            Hash.ToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLSecureValue type={0} data={1} front_side={2} reverse_side={3} selfie={4} files={5} plain_data={6} hash={7}", 
                Type,
                _data != null ? "[data]" : "null",
                _frontSide != null ? "[front_side]" : "null",
                _reverseSide != null ? "[reverse_side]" : "null",
                _selfie != null ? "[selfie]" : "null",
                _files != null ? _files.Count.ToString() : "null",
                _plainData != null ? "[plain_data]" : "null", 
                Hash);
        }

        public virtual TLInputSecureValue ToInputSecureValue()
        {
            TLVector<TLInputSecureFileBase> files = null;
            if (_files != null && _files.Count > 0)
            {
                files = new TLVector<TLInputSecureFileBase>();
                foreach (var file in _files)
                {
                    files.Add(file.ToInputSecureFile());
                }
            }

            return new TLInputSecureValue
            {
                Flags = new TLInt(0),
                Type = Type,
                Data = _data,
                FrontSide = _frontSide != null ? _frontSide.ToInputSecureFile() : null,
                ReverseSide = _reverseSide != null ? _reverseSide.ToInputSecureFile() : null,
                Selfie = _selfie != null ? _selfie.ToInputSecureFile() : null,
                Files = files,
                PlainData = _plainData
            };
        }

        public virtual void Update(TLSecureValue result)
        {
            Flags = new TLInt(0);
            Type = result.Type;
            Data = result.Data;
            FrontSide = result.FrontSide;
            ReverseSide = result.ReverseSide;
            Selfie = result.Selfie;
            Files = result.Files;
            PlainData = result.PlainData;
            Hash = result.Hash;
        }
    }

    public class TLSecureValue85 : TLSecureValue
    {
        public new const uint Signature = TLConstructors.TLSecureValue85;

        protected TLVector<TLSecureFileBase> _translation;

        public TLVector<TLSecureFileBase> Translation
        {
            get { return _translation; }
            set { SetField(out _translation, value, ref _flags, (int)SecureValueFlags.Translation); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Type = GetObject<TLSecureValueTypeBase>(bytes, ref position);
            _data = GetObject<TLSecureData>(Flags, (int)SecureValueFlags.Data, null, bytes, ref position);
            _frontSide = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.FrontSide, null, bytes, ref position);
            _reverseSide = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.ReverseSide, null, bytes, ref position);
            _selfie = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.Selfie, null, bytes, ref position);
            _translation = GetObject<TLVector<TLSecureFileBase>>(Flags, (int)SecureValueFlags.Translation, null, bytes, ref position);
            _files = GetObject<TLVector<TLSecureFileBase>>(Flags, (int)SecureValueFlags.Files, null, bytes, ref position);
            _plainData = GetObject<TLSecurePlainDataBase>(Flags, (int)SecureValueFlags.PlainData, null, bytes, ref position);
            Hash = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Type = GetObject<TLSecureValueTypeBase>(input);
            _data = GetObject<TLSecureData>(Flags, (int)SecureValueFlags.Data, null, input);
            _frontSide = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.FrontSide, null, input);
            _reverseSide = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.ReverseSide, null, input);
            _selfie = GetObject<TLSecureFileBase>(Flags, (int)SecureValueFlags.Selfie, null, input);
            _translation = GetObject<TLVector<TLSecureFileBase>>(Flags, (int)SecureValueFlags.Translation, null, input);
            _files = GetObject<TLVector<TLSecureFileBase>>(Flags, (int)SecureValueFlags.Files, null, input);
            _plainData = GetObject<TLSecurePlainDataBase>(Flags, (int)SecureValueFlags.PlainData, null, input);
            Hash = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Type.ToStream(output);
            ToStream(output, _data, Flags, (int)SecureValueFlags.Data);
            ToStream(output, _frontSide, Flags, (int)SecureValueFlags.FrontSide);
            ToStream(output, _reverseSide, Flags, (int)SecureValueFlags.ReverseSide);
            ToStream(output, _selfie, Flags, (int)SecureValueFlags.Selfie);
            ToStream(output, _translation, Flags, (int)SecureValueFlags.Translation);
            ToStream(output, _files, Flags, (int)SecureValueFlags.Files);
            ToStream(output, _plainData, Flags, (int)SecureValueFlags.PlainData);
            Hash.ToStream(output);
        }

        public override string ToString()
        {
            return string.Format("TLSecureValue85 type={0} data={1} front_side={2} reverse_side={3} selfie={4} translation{5} files={6} plain_data={7} hash={8}",
                Type,
                _data != null ? "[data]" : "null",
                _frontSide != null ? "[front_side]" : "null",
                _reverseSide != null ? "[reverse_side]" : "null",
                _selfie != null ? "[selfie]" : "null",
                _translation != null ? _translation.Count.ToString() : "null",
                _files != null ? _files.Count.ToString() : "null",
                _plainData != null ? "[plain_data]" : "null",
                Hash);
        }

        public override TLInputSecureValue ToInputSecureValue()
        {
            TLVector<TLInputSecureFileBase> files = null;
            if (_files != null && _files.Count > 0)
            {
                files = new TLVector<TLInputSecureFileBase>();
                foreach (var file in _files)
                {
                    files.Add(file.ToInputSecureFile());
                }
            } 
            TLVector<TLInputSecureFileBase> translation = null;
            if (_translation != null && _translation.Count > 0)
            {
                translation = new TLVector<TLInputSecureFileBase>();
                foreach (var file in _translation)
                {
                    translation.Add(file.ToInputSecureFile());
                }
            }

            return new TLInputSecureValue85
            {
                Flags = new TLInt(0),
                Type = Type,
                Data = _data,
                FrontSide = _frontSide != null ? _frontSide.ToInputSecureFile() : null,
                ReverseSide = _reverseSide != null ? _reverseSide.ToInputSecureFile() : null,
                Selfie = _selfie != null ? _selfie.ToInputSecureFile() : null,
                Translation = translation,
                Files = files,
                PlainData = _plainData
            };
        }

        public override void Update(TLSecureValue result)
        {
            base.Update(result);

            var result85 = result as TLSecureValue85;
            if (result85 != null)
            {
                Translation = result85.Translation;
            }
        }
    }
}

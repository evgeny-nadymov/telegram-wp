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
    public enum DocumentAttributeVideoFlags
    {
        RoundMessage = 0x1,       // 0
    }

    [Flags]
    public enum DocumentAttributeStickerFlags
    {
        MaskCoords = 0x1,       // 0
        Mask = 0x2,             // 1
    }

    [Flags]
    public enum DocumentAttributeAudioFlags
    {
        Title = 0x1,            // 0
        Performer = 0x2,        // 1
        Waveform = 0x4,         // 2

        Voice = 0x400,          // 10
    }

    public interface IAttributeDuration
    {
        TLInt Duration { get; set; }
    }

    public interface IAttributeSize
    {
        TLInt W { get; set; }

        TLInt H { get; set; }
    }

    public abstract class TLDocumentAttributeBase  : TLObject { }

    public class TLDocumentAttributeImageSize : TLDocumentAttributeBase, IAttributeSize
    {
        public const uint Signature = TLConstructors.TLDocumentAttributeImageSize;

        public TLInt W { get; set; }

        public TLInt H { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                W.ToBytes(),
                H.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            W.ToStream(output);
            H.ToStream(output);
        }
    }

    public class TLDocumentAttributeAnimated : TLDocumentAttributeBase
    {
        public const uint Signature = TLConstructors.TLDocumentAttributeAnimated;
        
        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLDocumentAttributeSticker : TLDocumentAttributeBase
    {
        public const uint Signature = TLConstructors.TLDocumentAttributeSticker;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }

    public class TLDocumentAttributeSticker25 : TLDocumentAttributeSticker
    {
        public new const uint Signature = TLConstructors.TLDocumentAttributeSticker25;

        public TLString Alt { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Alt = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Alt.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Alt = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Alt.ToStream(output);
        }
    }

    public class TLDocumentAttributeSticker29 : TLDocumentAttributeSticker25
    {
        public new const uint Signature = TLConstructors.TLDocumentAttributeSticker29;

        public TLInputStickerSetBase Stickerset { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Alt = GetObject<TLString>(bytes, ref position);
            Stickerset = GetObject<TLInputStickerSetBase>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Alt.ToBytes(),
                Stickerset.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Alt = GetObject<TLString>(input);
            Stickerset = GetObject<TLInputStickerSetBase>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Alt.ToStream(output);
            Stickerset.ToStream(output);
        }
    }

    public class TLDocumentAttributeSticker56 : TLDocumentAttributeSticker29
    {
        public new const uint Signature = TLConstructors.TLDocumentAttributeSticker56;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        private TLMaskCoords _maskCoords;

        public TLMaskCoords MaskCoords
        {
            get { return _maskCoords; }
            set { SetField(out _maskCoords, value, ref _flags, (int) DocumentAttributeStickerFlags.MaskCoords); }
        }

        public bool Mask { get { return IsSet(Flags, (int) DocumentAttributeStickerFlags.Mask); } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Alt = GetObject<TLString>(bytes, ref position);
            Stickerset = GetObject<TLInputStickerSetBase>(bytes, ref position);
            MaskCoords = GetObject<TLMaskCoords>(Flags, (int) DocumentAttributeStickerFlags.MaskCoords, null, bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Alt.ToBytes(),
                Stickerset.ToBytes(),
                ToBytes(MaskCoords, Flags, (int) DocumentAttributeStickerFlags.MaskCoords));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Alt = GetObject<TLString>(input);
            Stickerset = GetObject<TLInputStickerSetBase>(input);
            MaskCoords = GetObject<TLMaskCoords>(Flags, (int) DocumentAttributeStickerFlags.MaskCoords, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Alt.ToStream(output);
            Stickerset.ToStream(output);
            ToStream(output, MaskCoords, Flags, (int) DocumentAttributeStickerFlags.MaskCoords);
        }
    }

    public class TLDocumentAttributeVideo66 : TLDocumentAttributeVideo
    {
        public new const uint Signature = TLConstructors.TLDocumentAttributeVideo66;

        private TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool RoundMessage
        {
            get { return IsSet(Flags, (int) DocumentAttributeVideoFlags.RoundMessage); }
            set { SetUnset(ref _flags, value, (int) DocumentAttributeVideoFlags.RoundMessage); }
        }

        public bool Mask { get { return IsSet(Flags, (int)DocumentAttributeStickerFlags.Mask); } }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Duration = GetObject<TLInt>(bytes, ref position);
            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Duration.ToBytes(),
                W.ToBytes(),
                H.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Duration = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));

            Flags.ToStream(output);
            Duration.ToStream(output);
            W.ToStream(output);
            H.ToStream(output);
        }
    }

    public class TLDocumentAttributeVideo : TLDocumentAttributeBase, IAttributeDuration, IAttributeSize
    {
        public const uint Signature = TLConstructors.TLDocumentAttributeVideo;

        public TLInt Duration { get; set; }

        public TLInt W { get; set; }

        public TLInt H { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Duration = GetObject<TLInt>(bytes, ref position);
            W = GetObject<TLInt>(bytes, ref position);
            H = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Duration.ToBytes(),
                W.ToBytes(),
                H.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Duration = GetObject<TLInt>(input);
            W = GetObject<TLInt>(input);
            H = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Duration.ToStream(output);
            W.ToStream(output);
            H.ToStream(output);
        }
    }

    public class TLDocumentAttributeAudio : TLDocumentAttributeBase, IAttributeDuration
    {
        public const uint Signature = TLConstructors.TLDocumentAttributeAudio;

        public TLInt Duration { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Duration = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Duration.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Duration = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Duration.ToStream(output);
        }
    }

    public class TLDocumentAttributeAudio32 : TLDocumentAttributeAudio
    {
        public new const uint Signature = TLConstructors.TLDocumentAttributeAudio32;

        public virtual TLString Title { get; set; }

        public virtual TLString Performer { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Duration = GetObject<TLInt>(bytes, ref position);
            Title = GetObject<TLString>(bytes, ref position);
            Performer = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Duration.ToBytes(),
                Title.ToBytes(),
                Performer.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            Duration = GetObject<TLInt>(input);
            Title = GetObject<TLString>(input);
            Performer = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Duration.ToStream(output);
            Title.ToStream(output);
            Performer.ToStream(output);
        }
    }

    public class TLDocumentAttributeAudio46 : TLDocumentAttributeAudio32
    {
        public new const uint Signature = TLConstructors.TLDocumentAttributeAudio46;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Voice
        {
            get { return IsSet(Flags, (int) DocumentAttributeAudioFlags.Voice); }
            set { SetUnset(ref _flags, value, (int) DocumentAttributeAudioFlags.Voice); }
        }

        protected TLString _title;

        public override TLString Title
        {
            get { return _title; }
            set { SetFlagValue(value, out _title, ref _flags, (int)DocumentAttributeAudioFlags.Title); }
        }

        protected TLString _performer;

        public override TLString Performer
        {
            get { return _performer; }
            set { SetFlagValue(value, out _performer, ref _flags, (int)DocumentAttributeAudioFlags.Performer); }
        }

        protected TLString _waveform;

        public TLString Waveform
        {
            get { return _waveform; }
            set { SetFlagValue(value, out _waveform, ref _flags, (int)DocumentAttributeAudioFlags.Waveform); }
        }

        private static void SetFlagValue<T>(T value, out T field, ref TLInt flags, int flag) where T : TLObject
        {
            if (value != null)
            {
                Set(ref flags, flag);
                field = value;
            }
            else
            {
                Unset(ref flags, flag);
                field = default(T);
            }
        }

        private static void GetObject<T>(byte[] bytes, ref int position, ref T field, TLInt flags, int flag) where T : TLObject
        {
            if (IsSet(flags, flag))
            {
                field = GetObject<T>(bytes, ref position);
            }
        }

        private static void GetObject<T>(Stream input, ref T field, TLInt flags, int flag) where T : TLObject
        {
            if (IsSet(flags, flag))
            {
                field = GetObject<T>(input);
            }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            Duration = GetObject<TLInt>(bytes, ref position);
            GetObject(bytes, ref position, ref _title, _flags, (int) DocumentAttributeAudioFlags.Title);
            GetObject(bytes, ref position, ref _performer, _flags, (int) DocumentAttributeAudioFlags.Performer);
            GetObject(bytes, ref position, ref _waveform, _flags, (int) DocumentAttributeAudioFlags.Waveform);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                Duration.ToBytes(),
                ToBytes(Title, Flags, (int) DocumentAttributeAudioFlags.Title),
                ToBytes(Performer, Flags, (int) DocumentAttributeAudioFlags.Performer),
                ToBytes(Waveform, Flags, (int) DocumentAttributeAudioFlags.Waveform));
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            Duration = GetObject<TLInt>(input);
            GetObject(input, ref _title, _flags, (int) DocumentAttributeAudioFlags.Title);
            GetObject(input, ref _performer, _flags, (int) DocumentAttributeAudioFlags.Performer);
            GetObject(input, ref _waveform, _flags, (int) DocumentAttributeAudioFlags.Waveform);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            Flags.ToStream(output);
            Duration.ToStream(output);
            ToStream(output, Title, Flags, (int) DocumentAttributeAudioFlags.Title);
            ToStream(output, Performer, Flags, (int) DocumentAttributeAudioFlags.Performer);
            ToStream(output, Waveform, Flags, (int) DocumentAttributeAudioFlags.Waveform);
        }
    }

    public class TLDocumentAttributeFileName : TLDocumentAttributeBase
    {
        public const uint Signature = TLConstructors.TLDocumentAttributeFileName;

        public TLString FileName { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            FileName = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                FileName.ToBytes());
        }

        public override TLObject FromStream(Stream input)
        {
            FileName = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            FileName.ToStream(output);
        }
    }

    public class TLDocumentAttributeHasStickers : TLDocumentAttributeBase
    {
        public const uint Signature = TLConstructors.TLDocumentAttributeHasStickers;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature));
        }

        public override TLObject FromStream(Stream input)
        {
            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
        }
    }
}

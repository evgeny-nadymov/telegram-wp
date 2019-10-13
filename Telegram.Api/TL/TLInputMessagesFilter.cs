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
    public enum InputMessagesFilterPhoneCallsFlags
    {
        Missed = 0x1,           // 0
    }

    public abstract class TLInputMessagesFilterBase : TLObject { }

    public class TLInputMessagesFilterEmpty : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessageFilterEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterPhoto : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessageFilterPhoto;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterVideo : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessageFilterVideo;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterPhotoVideo : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessageFilterPhotoVideo;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterPhotoVideoDocument : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessageFilterPhotoVideoDocument;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterDocument : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessageFilterDocument;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterAudio : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessageFilterAudio;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterAudioDocuments : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessageFilterAudioDocuments;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterUrl : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessageFilterUrl;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterGif : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessagesFilterGif;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterVoice : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessagesFilterVoice;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterMusic : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessagesFilterMusic;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterChatPhotos : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessagesFilterChatPhotos;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterPhoneCalls : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessagesFilterPhoneCalls;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool Missed
        {
            get { return IsSet(Flags, (int) InputMessagesFilterPhoneCallsFlags.Missed); }
            set { SetUnset(ref _flags, value, (int) InputMessagesFilterPhoneCallsFlags.Missed); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes());
        }
    }

    public class TLInputMessagesFilterRoundVoice : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessagesFilterRoundVoice;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterRoundVideo : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessagesFilterRoundVideo;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterMyMentions : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessagesFilterMyMentions;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterGeo : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessagesFilterGeo;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }

    public class TLInputMessagesFilterContacts : TLInputMessagesFilterBase
    {
        public const uint Signature = TLConstructors.TLInputMessagesFilterContacts;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);
            return this;
        }

        public override byte[] ToBytes()
        {
            return TLUtils.SignatureToBytes(Signature);
        }
    }
}

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
    public enum ChatSettingsFlags
    {
        AutoDownloadPhotoPrivateChats = 0x1,        // 0
        AutoDownloadPhotoGroups = 0x2,              // 1
        AutoDownloadAudioPrivateChats = 0x4,        // 2
        AutoDownloadAudioGroups = 0x8,              // 3
        AutoDownloadGifPrivateChats = 0x10,
        AutoDownloadGifGroups = 0x20,
        AutoPlayGif = 0x40
    }

    public class TLChatSettings : TLObject
    {
        public const uint Signature = TLConstructors.TLChatSettings;

        private TLLong _flags;

        public TLLong Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public bool AutoDownloadPhotoPrivateChats
        {
            get { return IsSet(Flags, (int) ChatSettingsFlags.AutoDownloadPhotoPrivateChats); }
            set { SetUnset(ref _flags, value, (int)ChatSettingsFlags.AutoDownloadPhotoPrivateChats); }
        }

        public bool AutoDownloadPhotoGroups
        {
            get { return IsSet(Flags, (int)ChatSettingsFlags.AutoDownloadPhotoGroups); }
            set { SetUnset(ref _flags, value, (int)ChatSettingsFlags.AutoDownloadPhotoGroups); }
        }

        public bool AutoDownloadAudioPrivateChats
        {
            get { return IsSet(Flags, (int)ChatSettingsFlags.AutoDownloadAudioPrivateChats); }
            set { SetUnset(ref _flags, value, (int)ChatSettingsFlags.AutoDownloadAudioPrivateChats); }
        }

        public bool AutoDownloadAudioGroups
        {
            get { return IsSet(Flags, (int)ChatSettingsFlags.AutoDownloadAudioGroups); }
            set { SetUnset(ref _flags, value, (int)ChatSettingsFlags.AutoDownloadAudioGroups); }
        }

        public bool AutoDownloadGifPrivateChats
        {
            get { return IsSet(Flags, (int)ChatSettingsFlags.AutoDownloadGifPrivateChats); }
            set { SetUnset(ref _flags, value, (int)ChatSettingsFlags.AutoDownloadGifPrivateChats); }
        }

        public bool AutoDownloadGifGroups
        {
            get { return IsSet(Flags, (int)ChatSettingsFlags.AutoDownloadGifGroups); }
            set { SetUnset(ref _flags, value, (int)ChatSettingsFlags.AutoDownloadGifGroups); }
        }

        public bool AutoPlayGif
        {
            get { return IsSet(Flags, (int)ChatSettingsFlags.AutoPlayGif); }
            set { SetUnset(ref _flags, value, (int)ChatSettingsFlags.AutoPlayGif); }
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

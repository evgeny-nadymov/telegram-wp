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
    public enum PeerNotifySettingsFlags
    {
        ShowPreviews = 0x1,         // 0
        Silent = 0x2,               // 1
        MuteUntil = 0x4,            // 2
        Sound = 0x8,                // 3
    }

    public abstract class TLPeerNotifySettingsBase : TLObject
    {
        #region Additional
        public DateTime? LastNotificationTime { get; set; }
        #endregion
    }

    public class TLPeerNotifySettings78 : TLPeerNotifySettings48
    {
        public new const uint Signature = TLConstructors.TLPeerNotifySettings78;

        protected TLBool _showPreviews;

        public override TLBool ShowPreviews
        {
            get { return _showPreviews; }
            set { SetField(out _showPreviews, value, ref _flags, (int)PeerNotifySettingsFlags.ShowPreviews); }
        }

        protected TLBool _silent;

        public override TLBool Silent
        {
            get { return _silent; }
            set { SetField(out _silent, value, ref _flags, (int)PeerNotifySettingsFlags.Silent); }
        }

        protected TLInt _muteUntil;

        public override TLInt MuteUntil
        {
            get { return _muteUntil; }
            set { SetField(out _muteUntil, value, ref _flags, (int)PeerNotifySettingsFlags.MuteUntil); }
        }

        protected TLString _sound;

        public override TLString Sound
        {
            get { return _sound; }
            set { SetField(out _sound, value, ref _flags, (int)PeerNotifySettingsFlags.Sound); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            ShowPreviews = GetObject<TLBool>(Flags, (int)PeerNotifySettingsFlags.ShowPreviews, null, bytes, ref position);
            Silent = GetObject<TLBool>(Flags, (int)PeerNotifySettingsFlags.Silent, null, bytes, ref position);
            MuteUntil = GetObject<TLInt>(Flags, (int)PeerNotifySettingsFlags.MuteUntil, null, bytes, ref position);
            Sound = GetObject<TLString>(Flags, (int)PeerNotifySettingsFlags.Sound, null, bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            ShowPreviews = GetObject<TLBool>(Flags, (int)PeerNotifySettingsFlags.ShowPreviews, null, input);
            Silent = GetObject<TLBool>(Flags, (int)PeerNotifySettingsFlags.Silent, null, input);
            MuteUntil = GetObject<TLInt>(Flags, (int)PeerNotifySettingsFlags.MuteUntil, null, input);
            Sound = GetObject<TLString>(Flags, (int)PeerNotifySettingsFlags.Sound, null, input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            ToStream(output, ShowPreviews, Flags, (int)PeerNotifySettingsFlags.ShowPreviews);
            ToStream(output, Silent, Flags, (int)PeerNotifySettingsFlags.Silent);
            ToStream(output, MuteUntil, Flags, (int)PeerNotifySettingsFlags.MuteUntil);
            ToStream(output, Sound, Flags, (int)PeerNotifySettingsFlags.Sound);
        }
    }

    public class TLPeerNotifySettings48 : TLPeerNotifySettings
    {
        public new const uint Signature = TLConstructors.TLPeerNotifySettings48;

        protected TLInt _flags;

        public TLInt Flags
        {
            get { return _flags; }
            set { _flags = value; }
        }

        public override TLBool ShowPreviews
        {
            get { return new TLBool(IsSet(Flags, (int)PeerNotifySettingsFlags.ShowPreviews)); }
            set { SetUnset(ref _flags, value != null && value.Value, (int)PeerNotifySettingsFlags.ShowPreviews); }
        }

        public virtual TLBool Silent
        {
            get { return new TLBool(IsSet(Flags, (int)PeerNotifySettingsFlags.Silent)); }
            set { SetUnset(ref _flags, value != null && value.Value, (int)PeerNotifySettingsFlags.Silent); }
        }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            Flags = GetObject<TLInt>(bytes, ref position);
            MuteUntil = GetObject<TLInt>(bytes, ref position);
            Sound = GetObject<TLString>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            Flags = GetObject<TLInt>(input);
            MuteUntil = GetObject<TLInt>(input);
            Sound = GetObject<TLString>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(Flags.ToBytes());
            output.Write(MuteUntil.ToBytes());
            output.Write(Sound.ToBytes());
        }
    }

    public class TLPeerNotifySettings : TLPeerNotifySettingsBase
    {
        public const uint Signature = TLConstructors.TLPeerNotifySettings;

        public virtual TLInt MuteUntil { get; set; }

        public virtual TLString Sound { get; set; }

        public virtual TLBool ShowPreviews { get; set; }

        public TLInt EventsMask { get; set; }

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            MuteUntil = GetObject<TLInt>(bytes, ref position);
            Sound = GetObject<TLString>(bytes, ref position);
            ShowPreviews = GetObject<TLBool>(bytes, ref position);
            EventsMask = GetObject<TLInt>(bytes, ref position);

            return this;
        }

        public override TLObject FromStream(Stream input)
        {
            MuteUntil = GetObject<TLInt>(input);
            Sound = GetObject<TLString>(input);
            ShowPreviews = GetObject<TLBool>(input);
            EventsMask = GetObject<TLInt>(input);

            return this;
        }

        public override void ToStream(Stream output)
        {
            output.Write(TLUtils.SignatureToBytes(Signature));
            output.Write(MuteUntil.ToBytes());
            output.Write(Sound.ToBytes());
            output.Write(ShowPreviews.ToBytes());
            output.Write(EventsMask.ToBytes());
        }
    }

    [Obsolete]
    public class TLPeerNotifySettingsEmpty : TLPeerNotifySettingsBase
    {
        public const uint Signature = TLConstructors.TLPeerNotifySettingsEmpty;

        public override TLObject FromBytes(byte[] bytes, ref int position)
        {
            bytes.ThrowExceptionIfIncorrect(ref position, Signature);

            return this;
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

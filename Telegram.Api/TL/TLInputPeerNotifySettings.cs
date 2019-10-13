// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
namespace Telegram.Api.TL
{
    public class TLInputPeerNotifySettings78 : TLInputPeerNotifySettings48
    {
        public new const uint Signature = TLConstructors.TLInputPeerNotifySettings78;

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

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                ToBytes(ShowPreviews, Flags, (int)PeerNotifySettingsFlags.ShowPreviews),
                ToBytes(Silent, Flags, (int)PeerNotifySettingsFlags.Silent),
                ToBytes(MuteUntil, Flags, (int)PeerNotifySettingsFlags.MuteUntil),
                ToBytes(Sound, Flags, (int)PeerNotifySettingsFlags.Sound));
        }

        public override string ToString()
        {
            return string.Format("mute_until={0} sound={1} show_previews={2} silent={3}", MuteUntil, Sound, ShowPreviews, Silent);
        }
    }

    public class TLInputPeerNotifySettings48 : TLInputPeerNotifySettings
    {
        public new const uint Signature = TLConstructors.TLInputPeerNotifySettings48;

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

        public TLBool Silent
        {
            get { return new TLBool(IsSet(Flags, (int)PeerNotifySettingsFlags.Silent)); }
            set { SetUnset(ref _flags, value != null && value.Value, (int)PeerNotifySettingsFlags.Silent); }
        }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                Flags.ToBytes(),
                MuteUntil.ToBytes(),
                Sound.ToBytes());
        }

        public override string ToString()
        {
            return string.Format("mute_until={0} sound={1} show_previews={2} silent={3}", MuteUntil, Sound, ShowPreviews, Silent);
        }
    }

    public class TLInputPeerNotifySettings : TLObject
    {
        public const uint Signature = TLConstructors.TLInputPeerNotifySettings;

        public virtual TLInt MuteUntil { get; set; }

        public virtual TLString Sound { get; set; }

        public virtual TLBool ShowPreviews { get; set; }

        public TLInt EventsMask { get; set; }

        public override byte[] ToBytes()
        {
            return TLUtils.Combine(
                TLUtils.SignatureToBytes(Signature),
                MuteUntil.ToBytes(),
                Sound.ToBytes(),
                ShowPreviews.ToBytes(),
                EventsMask.ToBytes());
        }

        public override string ToString()
        {
            return string.Format("mute_until={0} sound={1} show_previews={2} events_mask={3}", MuteUntil, Sound, ShowPreviews, EventsMask);
        }
    }
}

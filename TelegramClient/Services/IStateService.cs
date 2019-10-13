// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Device.Location;
using System.IO;
using System.Windows.Media.Imaging;
using OpenCVComponent;
using Telegram.Api.Services;
using Telegram.Api.TL.Interfaces;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Payments;
#if WP8
using Windows.Storage;
#endif
using ImageTools;
using Telegram.Api.TL;
using TelegramClient.Models;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Media;
using TelegramClient.ViewModels.Passport;

namespace TelegramClient.Services
{
    public interface IStateService
    {
        TLProxyBase Proxy { get; set; }
        TLString CurrentEmail { get; set; }
        TLSentEmailCode SentEmailCode { get; set; }
        SecureRequiredType SecureRequiredType { get; set; }
        TLSecureValueTypeBase SecureType { get; set; }
        TLSecureValue SecureValue { get; set; }
        IList<TLSecureValue> SecureValues { get; set; }
        TLAuthorizationForm AuthorizationForm { get; set; }
        TLVector<TLChatBase> CurrentFeed { get; set; }

        TLInt MessageId { get; set; }
        bool SuppressProxySharing { get; set; }
        string PhoneCallId { get; set; }
        bool RemoveCheckoutAndCardView { get; set; }
        bool ShowScrollDownButton { get; set; }
        bool HideCountryCode { get; set; }
        bool ResidenceCountry { get; set; }
        PaymentInfo PaymentInfo { get; set; }
        TLVector<TLWallPaperBase> Wallpapers { get; set; }
        Settings Settings { get; set; }
        string GameString { get; set; }
        TLInputPeerBase InputPeer { get; set; }
        TLGame Game { get; set; }
        TLBotCallbackAnswer BotCallbackAnswer { get; set; }
        TLObject SwitchPMWith { get; set; }
        TLInlineBotSwitchPM SwitchPM { get; set; }
        TLKeyboardButtonSwitchInline SwitchInlineButton { get; set; }
        bool UpdateSubtitle { get; set; }
        bool UpdateChannelAdministrators { get; set; }
        string Post { get; set; }
        bool LinkPreviews { get; set; }
        string Url { get; set; }
        string UrlText { get; set; }

        TLChannelAdminRights CurrentAdminRights { get; set; }
        TLChannelParticipantRoleBase CurrentRole { get; set; }
        TLChannel NewChannel { get; set; }

        int SelectedAutolockTimeout { get; set; }
        string LogFileName { get; set; }
        GeoCoordinate GeoCoordinate { get; set; }
        bool AnimateTitle { get; set; }

        string ShareCaption { get; set; }
        string ShareLink { get; set; }
        string ShareMessage { get; set; }

        string Hashtag { get; set; }

        bool FirstRun { get; set; }
        TLString PhoneNumber { get; set; }
        TLString PhoneCode { get; set; }
        TLString PhoneCodeHash { get; set; }
        TLInt SendCallTimeout { get; set; }
        TLBool PhoneRegistered { get; set; }
        TLSentCodeTypeBase Type { get; set; }
        TLCodeTypeBase NextType { get; set; }
        bool ClearNavigationStack { get; set; }
        TLUserBase CurrentContact { get; set; }
        TLString CurrentContactPhone { get; set; }
        int CurrentUserId { get; set; }
        TLObject With { get; set; }
        bool RemoveBackEntry { get; set; }
        bool RemoveBackEntries { get; set; }
        TLMessage MediaMessage { get; set; }
        TLDecryptedMessage DecryptedMediaMessage { get; set; }
        Photo Photo { get; set; }
        string FileId { get; set; }
        Photo Document { get; set; }
        byte[] ProfilePhotoBytes { get; set; }
        TLChatBase CurrentChat { get; set; }
        IInputPeer CurrentInputPeer { get; set; }

        List<string> Sounds { get; }
        TLUserBase Participant { get; set; }
        TLMessage CurrentPhotoMessage { get; set; }
        TLDecryptedMessage CurrentDecryptedPhotoMessage { get; set; }
        int CurrentMediaMessageId { get; set; }
        IList<TLMessage> CurrentMediaMessages { get; set; }
        IList<TLDecryptedMessage> CurrentDecryptedMediaMessages { get; set; }
        TLPhotoBase CurrentPhoto { get; set; }



        void GetNotifySettingsAsync(Action<Settings> callback);
        Settings GetNotifySettings();
        void SaveNotifySettingsAsync(Settings settings);


        void GetServerFilesAsync(Action<TLVector<TLServerFile>> callback);
        void SaveServerFilesAsync(TLVector<TLServerFile> serverFiles);


        bool IsMainViewOpened { get; set; }
        //bool IsDialogOpened { get; set; }
        TLObject ActiveDialog { get; set; }
        TLUserBase SharedContact { get; set; }
        TLMessageMediaContact SharedContactMedia { get; set; }
        string IsoFileName { get; set; }
        Country SelectedCountry { get; set; }
        Country SelectedResidenceCountry { get; set; }
        bool FocusOnInputMessage { get; set; }
        string VideoIsoFileName { get; set; }
        long Duration { get; set; }
        RecordedVideo RecordedVideo { get; set; }
        IList<TLUserBase> RemovedUsers { get; set; }
        List<TLMessageBase> ForwardMessages { get; set; }
        bool WithMyScore { get; set; }
        bool SuppressNotifications { get; set; }
        string PhoneNumberString { get; set; }
        IList<TLMessageBase> DialogMessages { get; set; }
        IList<TLDialogBase> LoadedDialogs { get; set; }
        BackgroundItem CurrentBackground { get; set; }
        bool IsEmptyBackground { get; }
        bool SendByEnter { get; set; }

        string Passcode { get; set; }
        bool IsSimplePasscode { get; set; }
        DateTime CloseTime { get; set; }
        bool Locked { get; set; }
        int AutolockTimeout { get; set; }

        void ResetPasscode();

        WriteableBitmap ActiveBitmap { get; set; }
        bool CreateSecretChat { get; set; }
        TLString CurrentKey { get; set; }
        TLLong CurrentKeyFingerprint { get; set; }
        bool MediaTab { get; set; }
        TLEncryptedChatBase CurrentEncryptedChat { get; set; }
        bool Tombstoning { get; set; }
        string UserId { get; set; }
        string ChatId { get; set; }
        string BroadcastId { get; set; }
        int ForwardingMessagesCount { get; set; }
        bool RequestForwardingCount { get; set; }
        ExtendedImage ExtendedImage { get; set; }
        int AccountDaysTTL { get; set; }
        TLPrivacyRules PrivacyRules { get; set; }
        IPrivacyValueUsersRule UsersRule { get; set; }

        IList<TLUserBase> SelectedUsers { get; set; }
        IList<TLInt> SelectedUserIds { get; set; }
        bool NavigateToDialogDetails { get; set; }
        bool NavigateToSecretChat { get; set; }
        string Domain { get; set; }
        bool ChangePhoneNumber { get; set; }
        TimerSpan SelectedTimerSpan { get; set; }
        List<TimerSpan> TimerSpans { get; set; }
        bool IsEncryptedTimer { get; set; }
        TLDCOption DCOption { get; set; }
        TLDHConfig DHConfig { get; set; }
        List<TLMessageBase> Source { get; set; }
        TLDialogBase Dialog { get; set; }
        TLPasswordBase Password { get; set; }
        TLPasswordInputSettings NewPasswordSettings { get; set; }
        bool IsInviteVisible { get; set; }
        string AccessToken { get; set; }
        TLUserBase Bot { get; set; }
        Uri WebLink { get; set; }
        IReadOnlyList<IStorageItem> StorageItems { get; set; }
        TLMessageBase Message { get; set; }
        IList<TLMessageBase> Messages { get; set; }
        int ConfirmWait { get; set; }
        TLSentCodeBase SentCode { get; set; }
        bool CollapseSearchControl { get; set; }
        TLMessageMediaContact PhoneContact { get; set; }
        //bool SearchDialogs { get; set; }

        //void GetRecentStickersAsync(Action<TLRecentStickers> callback);
        //void SaveRecentStickersAsync(TLRecentStickers recentStickers);
        //void ClearResentStickersAsync();

        void GetFeaturedStickersAsync(Action<TLFeaturedStickers> callback);
        TLFeaturedStickers GetFeaturedStickers();
        void SaveFeaturedStickersAsync(TLFeaturedStickers featuredStickers);
        void ClearFeaturedStickersAsync();

        void GetArchivedStickersAsync(Action<TLArchivedStickers> callback);
        TLArchivedStickers GetArchivedStickers();
        void SaveArchivedStickersAsync(TLArchivedStickers archivedStickers);
        void ClearArchivedStickersAsync();

        void GetAllStickersAsync(Action<TLAllStickers> callback);
        TLAllStickers GetAllStickers();
        void SaveAllStickersAsync(TLAllStickers allStickers);
        void ClearAllStickersAsync();

        void GetMasksAsync(Action<TLAllStickers> callback);
        TLAllStickers GetMasks();
        void SaveMasksAsync(TLAllStickers allStickers);
        void ClearMasksAsync();

        void GetWallpapersAsync(Action<TLVector<TLWallPaperBase>> callback);
        TLVector<TLWallPaperBase> GetWallpapers();
        void SaveWallpapersAsync(TLVector<TLWallPaperBase> allStickers);
        void ClearWallpapersAsync();

        TLChatSettings GetChatSettings();
        void SaveChatSettings(TLChatSettings chatSettings);

        TLContactsSettings GetContactsSettings();
        void SaveContactsSettings(TLContactsSettings contactsSettings);

        TLCameraSettings GetCameraSettings();
        void SaveCameraSettings(TLCameraSettings cameraSettings);

        TLPhotoPickerSettings GetPhotoPickerSettings();
        void SavePhotoPickerSettings(TLPhotoPickerSettings photoPickerSettings);

        TLTmpPassword GetTmpPassword();
        void SaveTmpPassword(TLTmpPassword tmpPassword);

        TLTopPeersBase GetTopPeers();
        void SaveTopPeers(TLTopPeersBase topPeers);

        TLPassportConfig GetPassportConfig();
        void SavePassportConfig(TLPassportConfig passportConfig);

        TLCallsSecurity GetCallsSecurity(bool defaultP2PContacts);
        void SaveCallsSecurity(TLCallsSecurity callsSecurity);

        event PropertyChangedEventHandler PropertyChanged;
    }

    public class Photo
    {
        public string FileName { get; set; }

        public byte[] Bytes { get; set; }

#if WP8
        public StorageFile File { get; set; }
#endif

        public byte[] PreviewBytes { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }

    public class PhotoFile : TelegramPropertyChangedBase
    {
        private TimerSpan _timerSpan;

        public TimerSpan TimerSpan
        {
            get { return _timerSpan; }
            set
            {
                if (value != _timerSpan)
                {
                    _timerSpan = value;
                    NotifyOfPropertyChange(() => TimerSpan);
                }
            }
        }

        public StorageFile File { get; set; }

        public Stream Thumbnail { get; set; }

        public TLMessage Message { get; set; }

        public Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject> DecryptedTuple { get; set; }

        public object Object
        {
            get { return (TLObject)Message ?? (DecryptedTuple != null ? DecryptedTuple.Item1 : null); }
            set
            {
                if (value is TLMessage) Message = (TLMessage)value;
                if (value is Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>) DecryptedTuple = (Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>)value;
            }
        }

        public TLInt Date
        {
            get
            {
                if (Message != null) return Message.Date;
                if (DecryptedTuple != null) return DecryptedTuple.Item1.Date;

                return null;
            }
            set
            {
                if (Message != null) Message.Date = value;
                if (DecryptedTuple != null) DecryptedTuple.Item1.Date = value;
            }
        }

        public PhotoFile Self { get { return this; } }

        public bool IsButton { get; set; }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyOfPropertyChange(() => IsSelected);
                }
            }
        }

        public FacesImage FacesImage { get; set; }

        public List<TLUserBase> Mentions { get; set; }
    }
}

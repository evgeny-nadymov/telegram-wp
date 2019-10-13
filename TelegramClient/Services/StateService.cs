// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telegram.Api.TL.Interfaces;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Payments;
using TelegramClient.Views.Dialogs;
#if WP81
using Windows.Storage;
#endif
using Caliburn.Micro;
using ImageTools;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Models;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Media;
using TelegramClient.ViewModels.Passport;

namespace TelegramClient.Services
{
    public class StateService : TelegramPropertyChangedBase, IStateService
    {
        public TLString CurrentEmail { get; set; }
        public TLSentEmailCode SentEmailCode { get; set; }
        public SecureRequiredType SecureRequiredType { get; set; }
        public TLSecureValueTypeBase SecureType { get; set; }
        public TLSecureValue SecureValue { get; set; }
        public IList<TLSecureValue> SecureValues { get; set; }
        public TLProxyBase Proxy { get; set; }
        public TLAuthorizationForm AuthorizationForm { get; set; }
        public TLVector<TLChatBase> CurrentFeed { get; set; }
        public TLInt MessageId { get; set; }
        public bool SuppressProxySharing { get; set; }
        public string PhoneCallId { get; set; }
        public bool RemoveCheckoutAndCardView { get; set; }
        public bool ShowScrollDownButton { get; set; }
        public bool HideCountryCode { get; set; }
        public bool ResidenceCountry { get; set; }
        public PaymentInfo PaymentInfo { get; set; }
        public bool CollapseSearchControl { get; set; }
        public TLMessageMediaContact PhoneContact { get; set; }
        public TLVector<TLWallPaperBase> Wallpapers { get; set; }
        public Settings Settings { get; set; }
        public string GameString { get; set; }
        public TLInputPeerBase InputPeer { get; set; }
        public TLGame Game { get; set; }
        public TLBotCallbackAnswer BotCallbackAnswer { get; set; }
        public TLSentCodeBase SentCode { get; set; }
        public int ConfirmWait { get; set; }
        public TLObject SwitchPMWith { get; set; }
        public TLInlineBotSwitchPM SwitchPM { get; set; }
        public TLKeyboardButtonSwitchInline SwitchInlineButton { get; set; }
        public bool UpdateSubtitle { get; set; }
        public bool UpdateChannelAdministrators { get; set; }
        public string Post { get; set; }
        public bool LinkPreviews { get; set; }
        public string Url { get; set; }
        public string UrlText { get; set; }

        public TLChannelAdminRights CurrentAdminRights { get; set; }
        public TLChannelParticipantRoleBase CurrentRole { get; set; }
        public TLChannel NewChannel { get; set; }

        public TLMessageBase Message { get; set; }
        public IList<TLMessageBase> Messages { get; set; }
        public Uri WebLink { get; set; }
        public IReadOnlyList<IStorageItem> StorageItems { get; set; }
        public string AccessToken { get; set; }
        public TLUserBase Bot { get; set; }
        public int SelectedAutolockTimeout { get; set; }
        public string LogFileName { get; set; }

        public GeoCoordinate GeoCoordinate { get; set; }

        public bool AnimateTitle { get; set; }
        public string ShareCaption { get; set; }
        public string ShareLink { get; set; }
        public string ShareMessage { get; set; }

        public bool IsInviteVisible { get; set; }

        public TLPasswordBase Password { get; set; }
        public TLPasswordInputSettings NewPasswordSettings { get; set; }

        public string Hashtag { get; set; }

        public TLDialogBase Dialog { get; set; }

        public bool FirstRun { get; set; }
        public TLString PhoneNumber { get; set; }
        public string PhoneNumberString { get; set; }
        public TLString PhoneCode { get; set; }
        public TLString PhoneCodeHash { get; set; }
        public TLInt SendCallTimeout { get; set; }
        public TLBool PhoneRegistered { get; set; }
        public TLSentCodeTypeBase Type { get; set; }
        public TLCodeTypeBase NextType { get; set; }
        public bool ClearNavigationStack { get; set; }
        public TLUserBase CurrentContact { get; set; }
        public TLString CurrentContactPhone { get; set; }

        private int? _currentUserId;

        public int CurrentUserId
        {
            get
            {
                if (_currentUserId == null)
                {
                    _currentUserId = SettingsHelper.GetValue<int>(Constants.CurrentUserKey);
                }
                return _currentUserId.Value;
            }
            set
            {
                var mtProtoService = IoC.Get<IMTProtoService>();
                mtProtoService.CurrentUserId = new TLInt(value);
                _currentUserId = value;

                SettingsHelper.CrossThreadAccess(
                    settings =>
                    {
                        settings[Constants.CurrentUserKey] = value;
                        settings.Save();
                    });
            }
        }

        private Color? _currentBackgroundColor;

        public Color CurrentBackgroundColor
        {
            get
            {
                if (_currentBackgroundColor.HasValue)
                {
                    return _currentBackgroundColor.Value;
                }

                var background = CurrentBackground;

                var color = Colors.Black;
                color.A = 153;  //99000000

                var blackTransparent = Colors.Black;
                blackTransparent.A = 0;

                _currentBackgroundColor = background != null && background.Name != "Empty" ? color : blackTransparent;

                return _currentBackgroundColor.Value;
            }
        }

        private Brush _currentForegroundBrush;

        public Brush CurrentForegroundBrush
        {
            get
            {
                if (_currentForegroundBrush != null)
                {
                    return _currentForegroundBrush;
                }

                var background = CurrentBackground;

                var brush = new SolidColorBrush(Colors.White);
                var defaultBrush = (Brush)Application.Current.Resources["PhoneForegroundBrush"];

                _currentForegroundBrush = background != null && background.Name != "Empty" ? brush : defaultBrush;

                return _currentForegroundBrush;
            }
        }

        private Brush _currentForegroundSubtleBrush;

        public Brush CurrentForegroundSubtleBrush
        {
            get
            {
                if (_currentForegroundSubtleBrush != null)
                {
                    return _currentForegroundSubtleBrush;
                }

                var background = CurrentBackground;

                var brush = new SolidColorBrush(Colors.White) { Opacity = 0.7 };

                var defaultBrush = (Brush)Application.Current.Resources["PhoneSubtleBrush"];

                _currentForegroundSubtleBrush = background != null && background.Name != "Empty" ? brush : defaultBrush;

                return _currentForegroundSubtleBrush;
            }
        }

        private BackgroundItem _currentBackground;

        public BackgroundItem CurrentBackground
        {
            get
            {
                if (_currentBackground == null)
                {
                    var currentBackground = SettingsHelper.GetValue<BackgroundItem>(Constants.CurrentBackgroundKey);
                    if (currentBackground == null)
                    {
                        _currentBackground = new BackgroundItem { Name = "Empty" };
                    }
                    else
                    {
                        _currentBackground = currentBackground;
                    }
                }

                return _currentBackground;
            }
            set
            {
                _currentBackground = value;

                SettingsHelper.CrossThreadAccess(
                    settings =>
                    {
                        settings[Constants.CurrentBackgroundKey] = value;
                        settings.Save();

                        _currentBackgroundColor = null;
                        _currentForegroundBrush = null;
                        _currentForegroundSubtleBrush = null;

                        NotifyOfPropertyChange(() => CurrentBackground);
                    });
            }
        }

        public bool IsEmptyBackground
        {
            get { return CurrentBackground == null || CurrentBackground.Name == "Empty"; }
        }

        public bool SendByEnter
        {
            get
            {
                return SettingsHelper.GetValue<bool>(Constants.SendByEnterKey);
            }
            set
            {
                SettingsHelper.CrossThreadAccess(
                    settings =>
                    {
                        settings[Constants.SendByEnterKey] = value;
                        settings.Save();
                        NotifyOfPropertyChange(() => SendByEnter);
                    });
            }
        }

        public void ResetPasscode()
        {
            PasscodeUtils.Reset();
        }

        public DateTime CloseTime
        {
            get
            {
                return SettingsHelper.GetValue<DateTime>(Constants.AppCloseTimeKey);
            }
            set
            {
                SettingsHelper.CrossThreadAccess(
                    settings =>
                    {
                        settings[Constants.AppCloseTimeKey] = value;
                        settings.Save();
                    });

                NotifyOfPropertyChange(() => CloseTime);
            }
        }

        public string Passcode
        {
            get
            {
                return SettingsHelper.GetValue<string>(Constants.PasscodeKey);
            }
            set
            {
                SettingsHelper.CrossThreadAccess(
                    settings =>
                    {
                        settings[Constants.PasscodeKey] = value;
                        settings.Save();
                    });

                NotifyOfPropertyChange(() => Passcode);
            }
        }

        public bool IsSimplePasscode
        {
            get
            {
                return SettingsHelper.GetValue<bool>(Constants.IsSimplePasscodeKey);
            }
            set
            {
                SettingsHelper.CrossThreadAccess(
                    settings =>
                    {
                        settings[Constants.IsSimplePasscodeKey] = value;
                        settings.Save();
                    });

                NotifyOfPropertyChange(() => IsSimplePasscode);
            }
        }

        public bool Locked
        {
            get
            {
                return SettingsHelper.GetValue<bool>(Constants.IsPasscodeEnabledKey);
            }
            set
            {
                SettingsHelper.CrossThreadAccess(
                    settings =>
                    {
                        settings[Constants.IsPasscodeEnabledKey] = value;
                        settings.Save();
                    });

                NotifyOfPropertyChange(() => Locked);
            }
        }

        public int AutolockTimeout
        {
            get
            {
                return SettingsHelper.GetValue<int>(Constants.PasscodeAutolockKey);
            }
            set
            {
                SettingsHelper.CrossThreadAccess(
                    settings =>
                    {
                        settings[Constants.PasscodeAutolockKey] = value;
                        settings.Save();
                    });

                NotifyOfPropertyChange(() => AutolockTimeout);
            }
        }

        public WriteableBitmap ActiveBitmap { get; set; }

        public bool CreateSecretChat { get; set; }
        public TLEncryptedChatBase CurrentEncryptedChat { get; set; }
        public TLString CurrentKey { get; set; }
        public TLLong CurrentKeyFingerprint { get; set; }
        public bool MediaTab { get; set; }

        public TLObject With { get; set; }
        public IList<TLMessageBase> DialogMessages { get; set; }
        public IList<TLDialogBase> LoadedDialogs { get; set; }
        public bool RemoveBackEntry { get; set; }
        public bool RemoveBackEntries { get; set; }
        public TLMessage MediaMessage { get; set; }
        public TLDecryptedMessage DecryptedMediaMessage { get; set; }

        public Photo Photo { get; set; }
        public string FileId { get; set; }
        public Photo Document { get; set; }
        public byte[] ProfilePhotoBytes { get; set; }
        public TLChatBase CurrentChat { get; set; }
        public IInputPeer CurrentInputPeer { get; set; }

        private readonly List<string> _sounds = new List<string> { "Default", "Sound1", "Sound2", "Sound3", "Sound4", "Sound5", "Sound6" };

        public List<string> Sounds
        {
            get { return _sounds; }
        }

        public TLUserBase Participant { get; set; }

        public TLMessage CurrentPhotoMessage { get; set; }
        public TLDecryptedMessage CurrentDecryptedPhotoMessage { get; set; }
        public int CurrentMediaMessageId { get; set; }
        public IList<TLMessage> CurrentMediaMessages { get; set; }
        public IList<TLDecryptedMessage> CurrentDecryptedMediaMessages { get; set; }

        public TLPhotoBase CurrentPhoto { get; set; }


        // public bool InAppVibration { get; set; }
        // public bool InAppSound { get; set; }
        // public bool InAppMessagePreview { get; set; }


        public bool IsMainViewOpened { get; set; }
        public bool IsDialogOpened { get; set; }
        public TLObject ActiveDialog { get; set; }

        public TLUserBase SharedContact { get; set; }
        public TLMessageMediaContact SharedContactMedia { get; set; }
        public string IsoFileName { get; set; }
        public Country SelectedCountry { get; set; }
        public Country SelectedResidenceCountry { get; set; }
        public bool FocusOnInputMessage { get; set; }
        public string VideoIsoFileName { get; set; }
        public long Duration { get; set; }
        public RecordedVideo RecordedVideo { get; set; }
        public IList<TLUserBase> RemovedUsers { get; set; }
        public List<TLMessageBase> ForwardMessages { get; set; }
        public bool WithMyScore { get; set; }

        public bool SuppressNotifications { get; set; }

        public bool Tombstoning { get; set; }

        public string UserId { get; set; }
        public string ChatId { get; set; }
        public string BroadcastId { get; set; }

        public int ForwardingMessagesCount { get; set; }
        public bool RequestForwardingCount { get; set; }

        public ExtendedImage ExtendedImage { get; set; }

#if WP81
        public CompressingVideoFile CompressingVideoFile { get; set; }
#endif
        public int AccountDaysTTL { get; set; }
        public TLPrivacyRules PrivacyRules { get; set; }
        public IPrivacyValueUsersRule UsersRule { get; set; }


        public IList<TLUserBase> SelectedUsers { get; set; }
        public IList<TLInt> SelectedUserIds { get; set; }
        public bool NavigateToDialogDetails { get; set; }
        public bool NavigateToSecretChat { get; set; }
        public string Domain { get; set; }
        public bool ChangePhoneNumber { get; set; }
        public TimerSpan SelectedTimerSpan { get; set; }
        public List<TimerSpan> TimerSpans { get; set; }
        public bool IsEncryptedTimer { get; set; }
        public TLDCOption DCOption { get; set; }
        public TLDHConfig DHConfig { get; set; }
        public List<TLMessageBase> Source { get; set; }
        //public bool SearchDialogs { get; set; }

        #region Settings

        private readonly object _settingsRoot = new object();

        private Settings _settings;

        public void GetNotifySettingsAsync(Action<Settings> callback)
        {
            if (_settings != null)
            {
                callback(_settings);
                return;
            }

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _settings = TLUtils.OpenObjectFromFile<Settings>(_settingsRoot, Constants.CommonNotifySettingsFileName) ?? new Settings();
                _settings.IsNotifying = true;
                callback(_settings);
            });
        }

        public Settings GetNotifySettings()
        {
            if (_settings != null)
            {
                return _settings;
            }

            _settings = TLUtils.OpenObjectFromFile<Settings>(_settingsRoot, Constants.CommonNotifySettingsFileName) ?? new Settings();
            _settings.IsNotifying = true;
            return _settings;
        }

        public void SaveNotifySettingsAsync(Settings settings)
        {
            TLUtils.SaveObjectToFile(_settingsRoot, Constants.CommonNotifySettingsFileName, settings);
        }

        #endregion

        private readonly object _serverFilesRoot = new object();

        private TLVector<TLServerFile> _serverFiles;

        public void GetServerFilesAsync(Action<TLVector<TLServerFile>> callback)
        {
            if (_serverFiles != null)
            {
                callback(_serverFiles);
                return;
            }

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _serverFiles = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLServerFile>>(_serverFilesRoot, Constants.CachedServerFilesFileName) ?? new TLVector<TLServerFile>();
                callback(_serverFiles);
            });
        }

        public void SaveServerFilesAsync(TLVector<TLServerFile> serverFiles)
        {
            TLUtils.SaveObjectToMTProtoFile(_serverFilesRoot, Constants.CachedServerFilesFileName, serverFiles);
        }

        private readonly object _archivedStickersRoot = new object();

        private TLArchivedStickers _archivedStickers;

        public void GetArchivedStickersAsync(Action<TLArchivedStickers> callback)
        {
            if (_archivedStickers != null)
            {
                callback(_archivedStickers);
                return;
            }

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _archivedStickers = TLUtils.OpenObjectFromMTProtoFile<TLArchivedStickers>(_archivedStickersRoot, Constants.ArchivedStickersFileName);
                callback(_archivedStickers);
            });
        }

        public TLArchivedStickers GetArchivedStickers()
        {
            if (_archivedStickers != null)
            {
                return _archivedStickers;
            }

            _archivedStickers = TLUtils.OpenObjectFromMTProtoFile<TLArchivedStickers>(_archivedStickersRoot, Constants.ArchivedStickersFileName);
            return _archivedStickers;
        }

        public void SaveArchivedStickersAsync(TLArchivedStickers archivedStickers)
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _archivedStickers = archivedStickers;

                TLUtils.SaveObjectToMTProtoFile(_archivedStickersRoot, Constants.ArchivedStickersFileName, archivedStickers);
            });
        }

        public void ClearArchivedStickersAsync()
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _archivedStickers = null;

                FileUtils.Delete(_archivedStickersRoot, Constants.ArchivedStickersFileName);
            });
        }

        private readonly object _featuredStickersRoot = new object();

        private TLFeaturedStickers _featuredStickers;

        public void GetFeaturedStickersAsync(Action<TLFeaturedStickers> callback)
        {
            if (_featuredStickers != null)
            {
                callback(_featuredStickers);
                return;
            }

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _featuredStickers = TLUtils.OpenObjectFromMTProtoFile<TLFeaturedStickers>(_featuredStickersRoot, Constants.FeaturedStickersFileName);

                ProcessFeaturedStickers(_featuredStickers);

                callback(_featuredStickers);
            });
        }

        public TLFeaturedStickers GetFeaturedStickers()
        {
            if (_featuredStickers != null)
            {
                return _featuredStickers;
            }

            _featuredStickers = TLUtils.OpenObjectFromMTProtoFile<TLFeaturedStickers>(_featuredStickersRoot, Constants.FeaturedStickersFileName);

            ProcessFeaturedStickers(_featuredStickers);

            return _featuredStickers;
        }

        private void ProcessFeaturedStickers(TLFeaturedStickers featuredStickers)
        {
            if (featuredStickers == null) return;
#if DEBUG
            featuredStickers.Unread.Clear();
            foreach (var set in featuredStickers.Sets)
            {
                featuredStickers.Unread.Add(set.Id);
            }
#endif

            var stickerSets = new Dictionary<string, TLVector<TLStickerItem>>();
            var unreadDict = new Dictionary<long, long>();
            foreach (var unreadId in featuredStickers.Unread)
            {
                unreadDict[unreadId.Value] = unreadId.Value;
            }

            for (var i = 0; i < featuredStickers.Documents.Count; i++)
            {
                var document22 = featuredStickers.Documents[i] as TLDocument22;
                if (document22 != null)
                {
                    if (document22.StickerSet != null)
                    {
                        var setId = document22.StickerSet.Name;
                        TLVector<TLStickerItem> stickers;
                        if (stickerSets.TryGetValue(setId, out stickers))
                        {
                            stickers.Add(new TLStickerItem { Document = document22 });
                        }
                        else
                        {
                            stickerSets[setId] = new TLVector<TLStickerItem> { new TLStickerItem { Document = document22 } };
                        }
                    }
                }
            }

            for (var i = 0; i < featuredStickers.Sets.Count; i++)
            {
                var set = featuredStickers.Sets[i];
                if (unreadDict.ContainsKey(set.Id.Value))
                {
                    set.Unread = true;
                }

                var setName = set.Id.ToString();
                TLVector<TLStickerItem> stickers;
                if (stickerSets.TryGetValue(setName, out stickers))
                {
                    var objects = new TLVector<TLObject>();
                    foreach (var sticker in stickers)
                    {
                        objects.Add(sticker);
                    }

                    set.Stickers = objects;
                }
            }
        }

        public void SaveFeaturedStickersAsync(TLFeaturedStickers featuredStickers)
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _featuredStickers = featuredStickers;

                TLUtils.SaveObjectToMTProtoFile(_featuredStickersRoot, Constants.FeaturedStickersFileName, featuredStickers);
            });
        }

        public void ClearFeaturedStickersAsync()
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _featuredStickers = null;

                FileUtils.Delete(_featuredStickersRoot, Constants.FeaturedStickersFileName);
            });
        }


        private readonly object _masksRoot = new object();

        private TLAllStickers _masks;

        public void GetMasksAsync(Action<TLAllStickers> callback)
        {
            if (_masks != null)
            {
                callback(_masks);
                return;
            }

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _masks = TLUtils.OpenObjectFromMTProtoFile<TLAllStickers>(_masksRoot, Constants.MasksFileName);

                callback(_masks);
            });
        }

        public TLAllStickers GetMasks()
        {
            if (_masks != null)
            {
                return _masks;
            }

            _masks = TLUtils.OpenObjectFromMTProtoFile<TLAllStickers>(_masksRoot, Constants.MasksFileName);

            return _masks;
        }

        public void SaveMasksAsync(TLAllStickers masks)
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _masks = masks;
                if (masks != null)
                {
                    var allStickers29 = masks as TLAllStickers29;
                    if (allStickers29 != null && allStickers29.RecentlyUsed != null)
                    {
                        allStickers29.RecentlyUsed = new TLVector<TLRecentlyUsedSticker>(allStickers29.RecentlyUsed.Take(20).ToList());
                    }
                }

                TLUtils.SaveObjectToMTProtoFile(_masksRoot, Constants.MasksFileName, masks);
            });
        }

        public void ClearMasksAsync()
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _masks = null;

                FileUtils.Delete(_masksRoot, Constants.MasksFileName);
            });
        }

        private readonly object _allStickersRoot = new object();

        private TLAllStickers _allStickers;

        public void GetAllStickersAsync(Action<TLAllStickers> callback)
        {
            if (_allStickers != null)
            {
                callback(_allStickers);
                return;
            }

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _allStickers = TLUtils.OpenObjectFromMTProtoFile<TLAllStickers>(_allStickersRoot, Constants.AllStickersFileName);

                ProcessAllStickers();

                callback(_allStickers);
            });
        }

        private void ProcessAllStickers()
        {
            return;
#if DEBUG
            var allStickers43 = _allStickers as TLAllStickers43;
            if (allStickers43 != null)
            {
                allStickers43.RecentStickers = new TLRecentStickers
                {
                    Hash = new TLInt(0),
                    Documents = new TLVector<TLDocumentBase>()
                };
            }
#endif
        }

        public TLAllStickers GetAllStickers()
        {
            if (_allStickers != null)
            {
                return _allStickers;
            }

            _allStickers = TLUtils.OpenObjectFromMTProtoFile<TLAllStickers>(_allStickersRoot, Constants.AllStickersFileName);

            ProcessAllStickers();

            return _allStickers;
        }

        public void SaveAllStickersAsync(TLAllStickers allStickers)
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _allStickers = allStickers;
                if (allStickers != null)
                {
                    var allStickers29 = allStickers as TLAllStickers29;
                    if (allStickers29 != null && allStickers29.RecentlyUsed != null)
                    {
                        allStickers29.RecentlyUsed = new TLVector<TLRecentlyUsedSticker>(allStickers29.RecentlyUsed.Take(20).ToList());
                    }
                }

                TLUtils.SaveObjectToMTProtoFile(_allStickersRoot, Constants.AllStickersFileName, allStickers);
            });
        }

        public void ClearAllStickersAsync()
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _allStickers = null;

                FileUtils.Delete(_allStickersRoot, Constants.AllStickersFileName);
            });
        }

        private TLChatSettings _chatSettings;

        private readonly object _chatSettingsSyncRoot = new object();

        public TLChatSettings GetChatSettings()
        {
            if (_chatSettings != null)
            {
                return _chatSettings;
            }

            _chatSettings = TLUtils.OpenObjectFromMTProtoFile<TLChatSettings>(_chatSettingsSyncRoot, Constants.ChatSettingsFileName)
                ?? new TLChatSettings { AutoDownloadPhotoPrivateChats = true, AutoDownloadPhotoGroups = true, AutoDownloadAudioPrivateChats = true, AutoDownloadAudioGroups = true };

            return _chatSettings;
        }

        public void SaveChatSettings(TLChatSettings chatSettings)
        {
            _chatSettings = chatSettings;

            TLUtils.SaveObjectToMTProtoFile(_chatSettingsSyncRoot, Constants.ChatSettingsFileName, chatSettings);
        }

        private TLContactsSettings _contactsSettings;

        private readonly object _contactsSettingsSyncRoot = new object();

        public TLContactsSettings GetContactsSettings()
        {
            if (_contactsSettings != null)
            {
                return _contactsSettings;
            }

            _contactsSettings = TLUtils.OpenObjectFromMTProtoFile<TLContactsSettings>(_contactsSettingsSyncRoot, Constants.ContactsSettingsFileName)
                ?? new TLContactsSettings { Flags = new TLLong(0) };

            return _contactsSettings;
        }

        public void SaveContactsSettings(TLContactsSettings contactsSettings)
        {
            _contactsSettings = contactsSettings;

            TLUtils.SaveObjectToMTProtoFile(_contactsSettingsSyncRoot, Constants.ContactsSettingsFileName, contactsSettings);
        }

        private TLCameraSettings _cameraSettings;

        private readonly object _cameraSettingsSyncRoot = new object();

        public TLCameraSettings GetCameraSettings()
        {
            if (_cameraSettings != null)
            {
                return _cameraSettings;
            }

            _cameraSettings = TLUtils.OpenObjectFromMTProtoFile<TLCameraSettings>(_cameraSettingsSyncRoot, Constants.CameraSettingsFileName)
                ?? new TLCameraSettings { External = false };

            return _cameraSettings;
        }

        public void SaveCameraSettings(TLCameraSettings cameraSettings)
        {
            _cameraSettings = cameraSettings;

            TLUtils.SaveObjectToMTProtoFile(_cameraSettingsSyncRoot, Constants.CameraSettingsFileName, cameraSettings);
        }

        private TLPhotoPickerSettings _photoPickerSettings;

        private readonly object _photoPickerSettingsSyncRoot = new object();

        public TLPhotoPickerSettings GetPhotoPickerSettings()
        {
            if (_photoPickerSettings != null)
            {
                return _photoPickerSettings;
            }

            _photoPickerSettings = TLUtils.OpenObjectFromMTProtoFile<TLPhotoPickerSettings>(_photoPickerSettingsSyncRoot, Constants.PhotoPickerSettingsFileName)
                ?? new TLPhotoPickerSettings { External = false };

            return _photoPickerSettings;
        }

        public void SavePhotoPickerSettings(TLPhotoPickerSettings photoPickerSettings)
        {
            _photoPickerSettings = photoPickerSettings;

            TLUtils.SaveObjectToMTProtoFile(_photoPickerSettingsSyncRoot, Constants.PhotoPickerSettingsFileName, photoPickerSettings);
        }

        private TLTmpPassword _tmpPassword;

        private readonly object _tmpPasswordSyncRoot = new object();

        public TLTmpPassword GetTmpPassword()
        {
            if (_tmpPassword != null)
            {
                return _tmpPassword;
            }

            _tmpPassword = TLUtils.OpenObjectFromMTProtoFile<TLTmpPassword>(_tmpPasswordSyncRoot, Constants.TmpPasswordFileName);

            return _tmpPassword;
        }

        public void SaveTmpPassword(TLTmpPassword tmpPassword)
        {
            _tmpPassword = tmpPassword;

            TLUtils.SaveObjectToMTProtoFile(_tmpPasswordSyncRoot, Constants.TmpPasswordFileName, tmpPassword);
        }

        private readonly object _wallpapersRoot = new object();

        private TLVector<TLWallPaperBase> _wallpapers;

        public void GetWallpapersAsync(Action<TLVector<TLWallPaperBase>> callback)
        {
            if (_wallpapers != null)
            {
                callback(_wallpapers);
                return;
            }

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _wallpapers = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLWallPaperBase>>(_wallpapersRoot, Constants.WallpapersFileName);

                callback(_wallpapers);
            });
        }

        public TLVector<TLWallPaperBase> GetWallpapers()
        {
            if (_wallpapers != null)
            {
                return _wallpapers;
            }

            _wallpapers = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLWallPaperBase>>(_wallpapersRoot, Constants.WallpapersFileName);

            return _wallpapers;
        }

        public void SaveWallpapersAsync(TLVector<TLWallPaperBase> wallpapers)
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _wallpapers = wallpapers;

                TLUtils.SaveObjectToMTProtoFile(_wallpapersRoot, Constants.WallpapersFileName, wallpapers);
            });
        }

        public void ClearWallpapersAsync()
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _wallpapers = null;

                FileUtils.Delete(_wallpapersRoot, Constants.WallpapersFileName);
            });
        }

        private static readonly object _topPeersSyncRoot = new object();

        private static TLTopPeersBase _topPeers;

        public TLTopPeersBase GetTopPeers()
        {
            if (_topPeers != null)
            {
                return _topPeers;
            }

            _topPeers = TLUtils.OpenObjectFromMTProtoFile<TLTopPeersBase>(_topPeersSyncRoot, Constants.TopPeersFileName) as TLTopPeers;

            return _topPeers;
        }

        public void SaveTopPeers(TLTopPeersBase topPeers)
        {
            _topPeers = topPeers;

            if (topPeers == null)
            {
                FileUtils.Delete(_topPeersSyncRoot, Constants.TopPeersFileName);
            }
            else
            {
                TLUtils.SaveObjectToMTProtoFile(_topPeersSyncRoot, Constants.TopPeersFileName, topPeers);
            }
        }

        private static readonly object _passportConfigSyncRoot = new object();

        private static TLPassportConfig _passportConfig;

        public TLPassportConfig GetPassportConfig()
        {
            if (_passportConfig != null)
            {
                return _passportConfig;
            }

            _passportConfig = TLUtils.OpenObjectFromMTProtoFile<TLPassportConfig>(_passportConfigSyncRoot, Constants.PassportConfigFileName) as TLPassportConfig;

            return _passportConfig;
        }

        public void SavePassportConfig(TLPassportConfig passportConfig)
        {
            _passportConfig = passportConfig;

            if (passportConfig == null)
            {
                FileUtils.Delete(_passportConfigSyncRoot, Constants.PassportConfigFileName);
            }
            else
            {
                TLUtils.SaveObjectToMTProtoFile(_passportConfigSyncRoot, Constants.PassportConfigFileName, passportConfig);
            }
        }

        private static readonly object _callsSecuritySyncRoot = new object();

        private static TLCallsSecurity _callsSecurity;

        public TLCallsSecurity GetCallsSecurity(bool defaultP2PContacts)
        {
            if (_callsSecurity != null)
            {
                return _callsSecurity;
            }

            _callsSecurity = TLUtils.OpenObjectFromMTProtoFile<TLCallsSecurity>(_callsSecuritySyncRoot, Constants.CallsSecurityFileName) ?? new TLCallsSecurity{ PeerToPeer = true };
            _callsSecurity.Update(defaultP2PContacts);

            return _callsSecurity;
        }

        public void SaveCallsSecurity(TLCallsSecurity callsSecurity)
        {
            _callsSecurity = callsSecurity;

            if (callsSecurity == null)
            {
                FileUtils.Delete(_callsSecuritySyncRoot, Constants.CallsSecurityFileName);
            }
            else
            {
                TLUtils.SaveObjectToMTProtoFile(_callsSecuritySyncRoot, Constants.CallsSecurityFileName, _callsSecurity);
            }
        }
    }
}

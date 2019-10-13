// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#define DISABLE_INVISIBLEMODE
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Windows.Data.Xml.Dom;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Transport;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.ViewModels.Search;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Shell;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.Services.Location;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels.Auth;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Media;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class SettingsViewModel :
        ViewModelBase,
        Telegram.Api.Aggregator.IHandle<UploadableItem>,
        Telegram.Api.Aggregator.IHandle<UserNameChangedEventArgs>,
        Telegram.Api.Aggregator.IHandle<ProxyChangedEventArgs>
    {
        public string CloudStorageImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    return "/Images/Messages/chat.cloudstorage-white-WXGA.png";
                }

                return "/Images/Messages/chat.cloudstorage-WXGA.png.png";
            }
        }

        private TLUserBase _currentItem;

        public TLUserBase CurrentItem
        {
            get { return _currentItem; }
            set { SetField(ref _currentItem, value, () => CurrentItem); }
        }

        private IUpdatesService UpdateService
        {
            get { return IoC.Get<IUpdatesService>(); }
        }

        private IPushService PushService
        {
            get { return IoC.Get<IPushService>(); }
        }

        private IUploadFileManager UploadManager
        {
            get { return IoC.Get<IUploadFileManager>(); }
        }

        private string _stickersSubtitle;

        public string StickersSubtitle
        {
            get { return _stickersSubtitle; }
            set { SetField(ref _stickersSubtitle, value, () => StickersSubtitle); }
        }

        private string _proxySubtitle;

        public string ProxySubtitle
        {
            get { return _proxySubtitle; }
            set { SetField(ref _proxySubtitle, value, () => ProxySubtitle); }
        }

        private TLAllStickers29 _allStickers;

        private TLAllStickers29 _masks;

        private TLFeaturedStickers _featuredStickers;

        public ProfilePhotoViewerViewModel ProfilePhotoViewer { get; set; }

        public ITransportService TransportService { get; protected set; }

        public SettingsViewModel(ITransportService transportService, ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            eventAggregator.Subscribe(this);

            TransportService = transportService;

            SuppressUpdateStatus = true;

            //tombstoning
            if (stateService.CurrentContact == null)
            {
                ShellViewModel.Navigate(NavigationService);
                return;
            }

            UpdateProxySubtitle(TransportService.GetProxyConfig());

            CurrentItem = stateService.CurrentContact;
            stateService.CurrentContact = null;

            _allStickers = StateService.GetAllStickers() as TLAllStickers29;
            _masks = StateService.GetMasks() as TLAllStickers29;
            _featuredStickers = StateService.GetFeaturedStickers();
            if (_featuredStickers != null) UpdateSets(_featuredStickers, _featuredStickers.Unread);
            UpdateStickersSubtitle(_featuredStickers, _allStickers, _masks);

            StateService.GetNotifySettingsAsync(
                settings =>
                {
                    _saveIncomingPhotos = settings.SaveIncomingPhotos;
                    _invisibleMode = settings.InvisibleMode;
#if DISABLE_INVISIBLEMODE
                    _invisibleMode = false;
#endif

                    BeginOnUIThread(() =>
                    {
                        NotifyOfPropertyChange(() => SaveIncomingPhotos);
                        NotifyOfPropertyChange(() => InvisibleMode);
                    });
                });

            if (CurrentItem == null || CurrentItem.NotifySettings == null)
            {
                BeginOnThreadPool(() =>
                {
                    MTProtoService.GetFullUserAsync(new TLInputUserSelf(),
                        userFull =>
                        {
                            CurrentItem = userFull.User;

                            BeginOnUIThread(() =>
                            {
                                var user = CurrentItem as TLUser66;
                                if (user != null)
                                {
                                    user.NotifyOfPropertyChange(() => user.About);
                                }
                            });
                        });
                });
            }
        }

        public void ForwardInAnimationComplete()
        {
            UpdateStickersAsync(_featuredStickers);
        }

        private void UpdateProxySubtitle(TLProxyConfigBase proxyConfig)
        {
            var proxy = proxyConfig != null && proxyConfig.IsEnabled.Value && !proxyConfig.IsEmpty
                ? proxyConfig.GetProxy()
                : null;

            ProxySubtitle = proxy == null ? AppResources.Disabled : (proxy is TLSocks5Proxy ? AppResources.Socks5 : AppResources.MTProto);
        }

        private void UpdateStickersSubtitle(TLFeaturedStickers featuredStickers, TLAllStickers29 allStickers, TLAllStickers29 masks)
        {
            if (featuredStickers != null && featuredStickers.Unread.Count > 0)
            {
                var count = featuredStickers.Unread.Count;
                StickersSubtitle = Language.Declension(
                    count == 0 ? 1 : count,
                    AppResources.NewStickerPackNominativeSingular,
                    null,
                    AppResources.NewStickerPackGenitiveSingular,
                    AppResources.NewStickerPackGenitivePlural,
                    count < 2
                        ? string.Format("{1} {0}", AppResources.NewStickerPackNominativeSingular, 1).ToLowerInvariant()
                        : string.Format("{1} {0}", AppResources.NewStickerPackNominativePlural, Math.Abs(count))).ToLowerInvariant();
            }
            else if (allStickers != null || masks != null)
            {
                var parts = new List<string>();
                if (allStickers != null)
                {
                    var count = allStickers.Sets.Count;
                    var part = Language.Declension(
                        count == 0 ? 1 : count,
                        AppResources.StickerPackNominativeSingular,
                        null,
                        AppResources.StickerPackGenitiveSingular,
                        AppResources.StickerPackGenitivePlural,
                        count < 2
                            ? string.Format("{1} {0}", AppResources.StickerPackNominativeSingular, 1).ToLowerInvariant()
                            : string.Format("{1} {0}", AppResources.StickerPackNominativePlural, Math.Abs(count))).ToLowerInvariant();

                    parts.Add(part);

                }
                if (masks != null)
                {
                    var count = masks.Sets.Count;
                    var part = Language.Declension(
                        count == 0 ? 1 : count,
                        AppResources.MaskPackNominativePlural,
                        null,
                        AppResources.MaskPackGenitiveSingular,
                        AppResources.MaskPackGenitivePlural,
                        count < 2
                            ? string.Format("{1} {0}", AppResources.MaskPackNominativeSingular, 1).ToLowerInvariant()
                            : string.Format("{1} {0}", AppResources.MaskPackNominativePlural, Math.Abs(count))).ToLowerInvariant();

                    parts.Add(part);

                }
                StickersSubtitle = string.Join(", ", parts);
            }
        }

        private void UpdateStickersAsync(TLFeaturedStickers cachedStickers)
        {
            var hash = cachedStickers != null ? cachedStickers.HashValue : new TLInt(0);

            IsWorking = true;
            MTProtoService.GetFeaturedStickersAsync(true, hash,
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;

                    var featuredStickers = result as TLFeaturedStickers;
                    if (featuredStickers != null)
                    {
                        UpdateSets(featuredStickers, featuredStickers.Unread);
                        UpdateStickersSubtitle(featuredStickers, _allStickers, _masks);

                        StateService.SaveFeaturedStickersAsync(featuredStickers);
                    }
                }),
                error => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("messages.getFeaturedStickers error " + error);
                }));
        }

        private void UpdateSets(TLFeaturedStickers result, TLVector<TLLong> unread)
        {


            var unreadDict = new Dictionary<long, long>();
            foreach (var unreadId in unread)
            {
                unreadDict[unreadId.Value] = unreadId.Value;
            }

            for (var i = 0; i < result.Sets.Count; i++)
            {
                var set = result.Sets[i];
                if (unreadDict.ContainsKey(set.Id.Value))
                {
                    set.Unread = true;
                }
            }
        }

        private bool _saveIncomingPhotos;

        public bool SaveIncomingPhotos
        {
            get { return _saveIncomingPhotos; }
            set
            {
                SetField(ref _saveIncomingPhotos, value, () => SaveIncomingPhotos);
                StateService.GetNotifySettingsAsync(settings =>
                {
                    settings.SaveIncomingPhotos = value;
                    StateService.SaveNotifySettingsAsync(settings);
                });
            }
        }

        private bool _invisibleMode;

        public bool InvisibleMode
        {
            get { return _invisibleMode; }
            set
            {
                SetField(ref _invisibleMode, value, () => InvisibleMode);
                StateService.GetNotifySettingsAsync(settings =>
                {
                    settings.InvisibleMode = value;
                    StateService.SaveNotifySettingsAsync(settings);
                });
            }
        }

        private AskQuestionConfirmationViewModel _askQuestion;

        public AskQuestionConfirmationViewModel AskQuestion
        {
            get
            {
                return _askQuestion = _askQuestion ?? new AskQuestionConfirmationViewModel();
            }
        }



        protected override void OnActivate()
        {
            if (StateService.DCOption != null)
            {
                var option = StateService.DCOption;
                StateService.DCOption = null;
                Execute.ShowDebugMessage("New DCOption=" + option);
            }

            base.OnActivate();
        }

        public void OpenCloudStorage()
        {
            StateService.With = CurrentItem;
            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
        }

        public void OpenNotifications()
        {
            StateService.GetNotifySettingsAsync(
                s =>
                {
                    StateService.Settings = s;
                    NavigationService.UriFor<NotificationsViewModel>().Navigate();
                });
        }

        public static RichTextBox GetRichText(string richText)
        {
            var richTextBox = new RichTextBox();
            var paragraph = new Paragraph();
            var doc = new XmlDocument();
            doc.LoadXml("<section>" + richText + "</section>");
            if (doc.FirstChild != null)
            {
                foreach (var childNode in doc.FirstChild.ChildNodes)
                {
                    System.Diagnostics.Debug.WriteLine(childNode.InnerText);
                    var xmlText = childNode as XmlText;
                    if (xmlText != null)
                    {
                        if (!string.IsNullOrEmpty(childNode.InnerText))
                        {
                            paragraph.Inlines.Add(new Run { Text = childNode.InnerText });
                        }
                    }
                    var cdataNode = childNode as XmlCDataSection;
                    if (cdataNode != null)
                    {
                        var d = new XmlDocument();
                        try
                        {
                            d.LoadXml("<innerSection>" + cdataNode.InnerText + "</innerSection>");
                            if (d.FirstChild != null)
                            {
                                foreach (var c in d.FirstChild.ChildNodes)
                                {
                                    if (c.NodeName == "a")
                                    {
                                        var hyperlink = new Hyperlink { TargetName = "_blank" };
                                        foreach (var attribute in c.Attributes)
                                        {
                                            if (attribute.NodeName == "href")
                                            {
                                                var nodeInnerValue = attribute.NodeValue as string;
                                                if (!string.IsNullOrEmpty(nodeInnerValue))
                                                {
                                                    hyperlink.NavigateUri = new Uri(nodeInnerValue, UriKind.Absolute);
                                                }
                                            }
                                        }

                                        xmlText = c.FirstChild as XmlText;
                                        if (xmlText != null)
                                        {
                                            if (!string.IsNullOrEmpty(xmlText.InnerText))
                                            {
                                                hyperlink.Inlines.Add(new Run { Text = xmlText.InnerText, Foreground = (Brush)Application.Current.Resources["TelegramBadgeAccentBrush"] });
                                                paragraph.Inlines.Add(hyperlink);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Execute.ShowDebugMessage(ex.ToString());
                        }
                    }
                }
            }

            richTextBox.Blocks.Add(paragraph);

            return richTextBox;
        }

        public void Support()
        {
            var content = GetRichText(AppResources.AskAQuestionDescription);
            content.Padding = new Thickness(0.0);
            content.IsReadOnly = true;
            content.Margin = new Thickness(0.0, 40.0, 0.0, 40.0);

            ShellViewModel.ShowCustomMessageBox(null, null,
                AppResources.Ok.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.RightButton)
                    {
                        IsWorking = true;
                        MTProtoService.GetSupportAsync(support =>
                        {
                            IsWorking = false;
                            StateService.With = support.User;
                            BeginOnUIThread(() => NavigationService.UriFor<DialogDetailsViewModel>().Navigate());
                        },
                        error =>
                        {
                            IsWorking = false;
                        });
                    }
                },
                content);
        }

        public void OpenFAQ()
        {
            OpenUrl(Constants.TelegramFaq);
        }

        public void OpenPrivacyPolicy()
        {
            OpenUrl(Constants.TelegramPrivacyPolicy);
        }

        private static void OpenUrl(string url)
        {
            var task = new WebBrowserTask { URL = HttpUtility.UrlEncode(url) };
            task.Show();
        }

        public void OpenPhoto()
        {
            var user = CurrentItem;
            if (user != null)
            {
                var photo = user.Photo as TLUserProfilePhoto;
                if (photo != null)
                {
                    StateService.CurrentPhoto = photo;
                    StateService.CurrentContact = CurrentItem;

                    if (ProfilePhotoViewer == null)
                    {
                        ProfilePhotoViewer = new ProfilePhotoViewerViewModel(StateService, MTProtoService);
                        NotifyOfPropertyChange(() => ProfilePhotoViewer);
                    }

                    BeginOnUIThread(() => ProfilePhotoViewer.OpenViewer());
                }

                var photoEmpty = user.Photo as TLUserProfilePhotoEmpty;
                if (photoEmpty != null)
                {
                    EditCurrentUserActions.EditPhoto(result =>
                    {
                        var fileId = TLLong.Random();
                        IsWorking = true;
                        UploadManager.UploadFile(fileId, new TLUser66 { IsSelf = true }, result);
                    });
                }
            }
        }

        public void LogOut()
        {
            var result = MessageBox.Show(AppResources.LogOutConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;


            Telegram.Logs.Log.Write("SettingsViewModel.LogOut");
            PushService.UnregisterDeviceAsync(() =>
                MTProtoService.LogOutAsync(logOutResult =>
                {
                    ContactsHelper.DeleteContactsAsync(null);

                    Execute.BeginOnUIThread(() =>
                    {
                        foreach (var activeTile in ShellTile.ActiveTiles)
                        {
                            if (activeTile.NavigationUri.ToString().Contains("Action=SecondaryTile"))
                            {
                                activeTile.Delete();
                            }
                        }
                    });

                    //MTProtoService.LogOutTransportsAsync(
                    //    () =>
                    //    {

                    //    },
                    //    errors =>
                    //    {

                    //    });
                },
                error =>
                {
                    Execute.ShowDebugMessage("account.logOut error " + error);
                }));

            LogOutCommon(EventAggregator, MTProtoService, UpdateService, CacheService, StateService, PushService, NavigationService);
        }

        public void OpenSnapshots()
        {
            NavigationService.UriFor<SnapshotsViewModel>().Navigate();
        }

        public static void LogOutCommon(ITelegramEventAggregator eventAggregator, IMTProtoService mtProtoService, IUpdatesService updateService, ICacheService cacheService, IStateService stateService, IPushService pushService, INavigationService navigationService)
        {
            eventAggregator.Publish(Commands.LogOutCommand);

            SettingsHelper.SetValue(Constants.IsAuthorizedKey, false);
            SettingsHelper.RemoveValue(Constants.CurrentUserKey);
            mtProtoService.ClearQueue();
            updateService.ClearState();
            cacheService.ClearAsync();
            stateService.ResetPasscode();

            stateService.ClearAllStickersAsync();
            stateService.ClearFeaturedStickersAsync();
            stateService.ClearArchivedStickersAsync();

            EmojiControl emojiControl;
            if (EmojiControl.TryGetInstance(out emojiControl))
            {
                emojiControl.ClearStickersOnLogOut();
            }

            cacheService.ClearConfigImportAsync();
            SearchViewModel.DeleteRecentAsync();

            Bootstrapper.UpdateMainTile();

            DialogDetailsViewModel.DeleteInlineBots();
            FileUtils.Delete(new object(), Constants.InlineBotsNotificationFileName);
            FileUtils.Delete(new object(), Constants.WebPagePreviewsFileName);

            var liveLocationService = IoC.Get<ILiveLocationService>();
            liveLocationService.Clear();

            if (navigationService.CurrentSource == navigationService.UriFor<StartupViewModel>().BuildUri()
                || navigationService.CurrentSource == navigationService.UriFor<SignInViewModel>().BuildUri()
                || navigationService.CurrentSource == navigationService.UriFor<ConfirmViewModel>().BuildUri()
                || navigationService.CurrentSource == navigationService.UriFor<SignUpViewModel>().BuildUri())
            {
                return;
            }

            stateService.ClearNavigationStack = true;
            Telegram.Logs.Log.Write("StartupViewModel SettingsViewModel.LogOutCommon");
            navigationService.UriFor<StartupViewModel>().Navigate();
        }

        public void OpenPrivacySecurity()
        {
            NavigationService.UriFor<PrivacySecurityViewModel>().Navigate();
        }

        public void OpenLockScreenSettings()
        {
#if WP8
            Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));
#endif
        }

        public void OpenStickers()
        {
            NavigationService.UriFor<StickersViewModel>().Navigate();
        }

        public void OpenProxy()
        {
            NavigationService.UriFor<ProxyListViewModel>().Navigate();
        }

        public void OpenChatSettings()
        {
            NavigationService.UriFor<ChatSettingsViewModel>().Navigate();
        }

        public void EditProfile()
        {
            NavigationService.UriFor<EditCurrentUserViewModel>().Navigate();
        }

        public void EditProfilePhoto()
        {
            EditCurrentUserActions.EditPhoto(photo =>
            {
                var fileId = TLLong.Random();
                IsWorking = true;
                UploadManager.UploadFile(fileId, new TLUser66 { IsSelf = true }, photo);
            });
        }

        public void EditPhoneNumber()
        {
            var currentUser = CurrentItem;
            if (currentUser == null) return;

            StateService.CurrentContact = currentUser;
            NavigationService.UriFor<EditPhoneNumberViewModel>().Navigate();
        }

        public void EditUsername()
        {
            NavigationService.UriFor<EditUsernameViewModel>().Navigate();
        }

        public void EditBio()
        {
            StateService.CurrentContact = CurrentItem;
            NavigationService.UriFor<BioViewModel>().Navigate();
        }

        public void SendLogs()
        {
            var fileName = "log_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.fff", CultureInfo.InvariantCulture) + ".txt";

            Telegram.Logs.Log.CopyTo(fileName,
                result => BeginOnUIThread(() =>
                {
                    StateService.LogFileName = result;
                    NavigationService.UriFor<ChooseDialogViewModel>().Navigate();
                }));
        }

        public void SendCallLogs()
        {
            var fileName = "tgvoip.logFile.txt";

            BeginOnUIThread(() =>
            {
                StateService.LogFileName = fileName;
                NavigationService.UriFor<ChooseDialogViewModel>().Navigate();
            });
        }

        public void ClearLogs()
        {
            Telegram.Logs.Log.Clear(
                () => BeginOnUIThread(() =>
                {
                    MessageBox.Show("Complete");
                }));
        }

        public void Handle(UploadableItem item)
        {
            var userBase = item.Owner as TLUserBase;
            if (userBase != null && userBase.IsSelf)
            {
                Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    CurrentItem.NotifyOfPropertyChange(() => CurrentItem.Photo);
                });
            }
        }

        public void Handle(UserNameChangedEventArgs args)
        {
            var currentUser = CurrentItem;
            var userName = args.User as IUserName;

            if (currentUser != null
                && userName != null
                && args.User.Index == currentUser.Index)
            {
                CurrentItem = args.User;
                CurrentItem.NotifyOfPropertyChange(() => userName.UserName);
            }
        }

        public void Handle(ProxyChangedEventArgs args)
        {
            UpdateProxySubtitle(args.ProxyConfig);
        }
    }
}

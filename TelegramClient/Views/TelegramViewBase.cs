// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.System;
using Caliburn.Micro;
using Coding4Fun.Toolkit.Controls;
using Coding4Fun.Toolkit.Controls.Converters;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.EmojiPanel;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Controls;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Utils;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Auth;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Search;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Dialogs;
#if WP8
using System.Windows.Navigation;
#endif
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Telegram.Api.Transport;
using TelegramClient.ViewModels.Passport;
using EnterPasswordViewModel = TelegramClient.ViewModels.Passport.EnterPasswordViewModel;
using Execute = Telegram.Api.Helpers.Execute;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views
{
    public class TelegramViewBase : PhoneApplicationPage
    {
// fast resume
#if WP8
        private bool _isFastResume;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Reset)
            {
                _isFastResume = true;
            }

            if (e.NavigationMode == NavigationMode.Refresh
                && _isFastResume)
            {
                _isFastResume = false;

                if (e.Uri.OriginalString.StartsWith("/Protocol?encodedLaunchUri"))
                {
                    NavigateToTelegramUriAsync(e.Uri);
                }
                else if (e.Uri.OriginalString.StartsWith("/PeopleExtension?action=Show_Contact"))
                {
                    NavigateToContactFromPeopleHub(e.Uri);
                }
                else if (e.Uri.OriginalString.StartsWith("/Views/Additional/SettingsView.xaml?Action=DC_UPDATE"))
                {
                    UpdateDCOptions(e.Uri);
                }
            }

            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Uri.OriginalString.StartsWith("/Protocol?encodedLaunchUri"))
                {
                    NavigateToTelegramUriAsync(e.Uri);
                }
                else if (e.Uri.OriginalString.StartsWith("/PeopleExtension?action=Show_Contact"))
                {
                    NavigateToContactFromPeopleHub(e.Uri);
                }
                else if (e.Uri.OriginalString.StartsWith("/Views/Additional/SettingsView.xaml?Action=DC_UPDATE"))
                {
                    UpdateDCOptions(e.Uri);
                }
            }

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            var frame = Application.Current.RootVisual as TelegramTransitionFrame;
            if (frame != null)
            {
                if (frame.IsBlockingProgressOpen())
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (Preview != null && Preview.Visibility == Visibility.Visible)
            {
                StickerPanel_ManipulationCompleted(null, null);
            }

            if (_isFastResume
                && e.NavigationMode == NavigationMode.New
                && (e.Uri.OriginalString.EndsWith("ShellView.xaml")
                    || e.Uri.OriginalString.StartsWith("/Protocol?encodedLaunchUri")
                    || e.Uri.OriginalString.StartsWith("/PeopleExtension?action=Show_Contact")
                    || e.Uri.OriginalString.StartsWith("/Views/Additional/SettingsView.xaml?Action=DC_UPDATE")))
            {
                _isFastResume = false;
                e.Cancel = true;

                if (e.Uri.OriginalString.StartsWith("/Protocol?encodedLaunchUri"))
                {
                    NavigateToTelegramUriAsync(e.Uri);
                }
                else if (e.Uri.OriginalString.StartsWith("/PeopleExtension?action=Show_Contact"))
                {
                    NavigateToContactFromPeopleHub(e.Uri);
                }
                else if (e.Uri.OriginalString.StartsWith("/Views/Additional/SettingsView.xaml?Action=DC_UPDATE"))
                {
                    UpdateDCOptions(e.Uri);
                }

                return;
            }

            base.OnNavigatingFrom(e);
        }

        private void UpdateDCOptions(Uri uri)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var tempUri = HttpUtility.HtmlDecode(uri.OriginalString);
                var uriParams = TelegramUriMapper.ParseQueryString(tempUri);

                int id;
                var idParam = GetParam("dc", uriParams);
                var addrParam = GetParam("addr", uriParams);
                if (int.TryParse(idParam, out id) && !string.IsNullOrEmpty(addrParam))
                {
                    var addressParams = addrParam.Split(':');
                    if (addressParams.Length >= 2)
                    {
                        int port;
                        var host = addressParams[0];
                        if (string.IsNullOrEmpty(host) && int.TryParse(addressParams[1], out port))
                        {
                            IoC.Get<IMTProtoService>().CheckAndUpdateTransportInfoAsync(new TLInt(id), new TLString(host), new TLInt(port),
                                () =>
                                {
                                    //Execute.BeginOnUIThread(() => MessageBox.Show("App settings have been successfully updated.", AppResources.Info, MessageBoxButton.OK));
                                });
                        }
                    }
                }
            });
        }

        protected override void OnRemovedFromJournal(JournalEntryRemovedEventArgs e)
        {
            //Execute.ShowDebugMessage("OnRemovedFromJournal " + GetType());

            var viewModelBase = DataContext as ViewModelBase;
            if (viewModelBase != null)
            {
                Execute.BeginOnThreadPool(viewModelBase.Unsubscribe);
            }

            base.OnRemovedFromJournal(e);
        }

        private void NavigateToContactFromPeopleHub(Uri uri)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var tempUri = HttpUtility.UrlDecode(uri.ToString());
                try
                {
                    var uriParams = TelegramUriMapper.ParseQueryString(tempUri);

                    var userId = Convert.ToInt32(uriParams["contact_ids"]);
                    var cachedContact = IoC.Get<ICacheService>().GetUser(new TLInt(userId));

                    if (cachedContact != null)
                    {
                        Thread.Sleep(1000); // waiting for backwardin animations
                        NavigateToUser(cachedContact, string.Empty);
                    }
                }
                catch (Exception ex)
                {
                    Execute.ShowDebugMessage(tempUri + " ex\n" + ex);
                }
            });
        }

        public static bool IsFullHD()
        {
            return Application.Current.Host.Content.ScaleFactor == 225 || Environment.OSVersion.Version.Major >= 10;
        }

        public static void NavigateToTelegramUriAsync(Uri uri)
        {
            // /Protocol?encodedLaunchUri=tg%3A%2F%2F
            var start = "tg%3A";
            var uriString = uri.ToString().Replace("/Protocol?encodedLaunchUri=", string.Empty);
            if (uriString.StartsWith(start)
                && !uriString.StartsWith(start + "%2F%2F"))
            {
                uriString = uriString.Replace(start, start + "%2F%2F");
            }

            Execute.BeginOnThreadPool(() =>
            {
                var tempUri = HttpUtility.UrlDecode(uriString);

                Dictionary<string, string> uriParams = null;
                try
                {
                    uriParams = TelegramUriMapper.ParseQueryString(tempUri);
                }
                catch (Exception ex)
                {
                    Execute.ShowDebugMessage("Parse uri exception " + tempUri + ex);
                }
                if (uriParams != null)
                {
                    if (tempUri.StartsWith("tg://resolve") && tempUri.Contains("domain=telegrampassport")
                        || tempUri.StartsWith("tg://passport")
                        || tempUri.StartsWith("tg://secureid"))
                    {
                        // /Protocol?encodedLaunchUri=tg://resolve/?domain=telegrampassport&bot_id=<bot_id>&scope=<scope>&callback_url=<callback_url>&public_key=<public_key>
                        // /Protocol?encodedLaunchUri=tg://passport/?bot_id=<bot_id>&scope=<scope>&callback_url=<callback_url>&public_key=<public_key>
                        // /Protocol?encodedLaunchUri=tg://secureid/?bot_id=<bot_id>&scope=<scope>&callback_url=<callback_url>&public_key=<public_key>
                        var botId = GetParam("bot_id", uriParams);
                        var scope = HttpUtility.UrlDecode(GetParam("scope", uriParams));
                        var callbackUrl = HttpUtility.UrlDecode(GetParam("callback_url", uriParams));
                        var publicKey = HttpUtility.UrlDecode(GetParam("public_key", uriParams));
                        var payload = HttpUtility.UrlDecode(GetParam("payload", uriParams));
                        var nonce = HttpUtility.UrlDecode(GetParam("nonce", uriParams));
                        if (payload == "nonce" && !string.IsNullOrEmpty(nonce))
                        {
                            payload = nonce;
                        }

                        int botIdInt;
                        if (!int.TryParse(botId, out botIdInt))
                        {
                            botIdInt = -1;
                        }

                        var passportConfig = IoC.Get<IStateService>().GetPassportConfig();
                        NavigateToPassport(passportConfig, botIdInt, scope, callbackUrl, publicKey, payload, true);
                    }
                    else if (tempUri.StartsWith("tg://socks"))
                    {
                        // /Protocol?encodedLaunchUri=tg://socks/?server=<server>&port=<port>
                        // /Protocol?encodedLaunchUri=tg://socks/?server=<server>&port=<port>&user=<user>&pass=<pass>
                        var server = HttpUtility.UrlDecode(GetParam("server", uriParams));
                        var port = HttpUtility.UrlDecode(GetParam("port", uriParams));
                        var user = HttpUtility.UrlDecode(GetParam("user", uriParams));
                        var pass = HttpUtility.UrlDecode(GetParam("pass", uriParams));

                        int portInt;
                        if (int.TryParse(port, out portInt) && portInt >= 0)
                        {
                            NavigateToSocksProxy(server, portInt, user, pass);
                        }
                    }
                    else if (tempUri.StartsWith("tg://proxy"))
                    {
                        // /Protocol?encodedLaunchUri=tg://proxy/?server=<server>&port=<port>&secret=<secret>
                        var server = HttpUtility.UrlDecode(GetParam("server", uriParams));
                        var port = HttpUtility.UrlDecode(GetParam("port", uriParams));
                        var secret = HttpUtility.UrlDecode(GetParam("secret", uriParams));

                        int portInt;
                        if (int.TryParse(port, out portInt) && portInt >= 0 && !string.IsNullOrEmpty(secret))
                        {
                            NavigateToMTProtoProxy(server, portInt, secret);
                        }
                    }
                    else if (tempUri.StartsWith("tg://resolve"))
                    {
                        // /Protocol?encodedLaunchUri=tg://resolve/?domain=<username>&start=<access_token>
                        // /Protocol?encodedLaunchUri=tg://resolve/?domain=<username>&startgroup=<access_token>
                        // /Protocol?encodedLaunchUri=tg://resolve/?domain=<username>&post=<post_number>
                        // /Protocol?encodedLaunchUri=tg://resolve/?domain=<username>&game=<game>
                        var domain = GetParam("domain", uriParams);

                        PageKind pageKind;
                        var accessToken = GetAccessToken(uriParams, out pageKind);
                        var post = GetParam("post", uriParams);
                        var game = GetParam("game", uriParams);
                        var cachedContact = IoC.Get<ICacheService>().GetUsers().OfType<IUserName>().FirstOrDefault(x => string.Equals(x.UserName.ToString(), domain, StringComparison.OrdinalIgnoreCase)) as TLUserBase;

                        if (cachedContact != null)
                        {
                            Thread.Sleep(1000); // waiting for backwardin animations
                            if (!string.IsNullOrEmpty(game))
                            {
                                NavigateToGame(cachedContact, game);
                            }
                            else
                            {
                                NavigateToUser(cachedContact, accessToken, pageKind);
                            }
                        }
                        else
                        {
                            var mtProtoService = IoC.Get<IMTProtoService>();
                            NavigateToUsername(mtProtoService, domain, accessToken, post, game, pageKind);
                        }
                    }
                    else if (tempUri.StartsWith("tg://join"))
                    {
                        // /Protocol?encodedLaunchUri=tg://join/?invite=<group_access_token>
                        var link = GetParam("invite", uriParams);

                        var mtProtoService = IoC.Get<IMTProtoService>();
                        NavigateToInviteLink(mtProtoService, link);
                    }
                    else if (tempUri.StartsWith("tg://addstickers"))
                    {
                        // /Protocol?encodedLaunchUri=tg://addstickers/?set=<set_name>
                        var link = GetParam("set", uriParams);

                        var inputStickerSet = new TLInputStickerSetShortName { ShortName = new TLString(link) };

                        var mtProtoService = IoC.Get<IMTProtoService>();
                        var stateService = IoC.Get<IStateService>();
                        NavigateToStickers(mtProtoService, stateService, inputStickerSet);
                    }
                    else if (tempUri.StartsWith("tg://msg_url"))
                    {
                        // /Protocol?encodedLaunchUri=tg://msg_url/?url=<url_address>&text=<description>
                        var url = HttpUtility.UrlDecode(GetParam("url", uriParams));
                        var text = HttpUtility.UrlDecode(GetParam("text", uriParams));

                        NavigateToForwarding(url, text);
                    }
                    else if (tempUri.StartsWith("tg://"))
                    {
                        tempUri = tempUri.Replace("tg://", string.Empty);
                        var index = tempUri.IndexOf("?", StringComparison.Ordinal);
                        var undefinedPath = index >= 0 ? tempUri.Substring(0, index) : tempUri;

                        if (!string.IsNullOrEmpty(undefinedPath))
                        {
                            IoC.Get<IMTProtoService>().GetDeepLinkInfoAsync(
                                new TLString(undefinedPath),
                                result => Execute.BeginOnUIThread(() =>
                                {
                                    var deepLinkInfo = result as TLDeepLinkInfo;
                                    if (deepLinkInfo != null)
                                    {
                                        var richTextBox = new TelegramRichTextBox { Margin = new Thickness(0.0, 11.0, 0.0, 0.0), TextWrapping = TextWrapping.Wrap, Entities = deepLinkInfo.Entities, Text = deepLinkInfo.Message.ToString() };
                                        
                                        if (IsFullHD())
                                        {
                                            richTextBox.FontSize = 17.667;
                                        }

                                        ShellViewModel.ShowCustomMessageBox(null, AppResources.AppName,
                                            AppResources.Ok.ToLowerInvariant(),
                                            deepLinkInfo.UpdateApp ? AppResources.UpdateApp.ToLowerInvariant() : null,
                                            dismissed =>
                                            {
                                                if (dismissed == CustomMessageBoxResult.LeftButton)
                                                {
                                                    var task = new WebBrowserTask
                                                    {
                                                        Uri = PrivateBetaIdentityToVisibilityConverter.IsPrivateBeta
                                                            ? new Uri("https://www.microsoft.com/store/apps/9P0F9KC5TSTT", UriKind.Absolute)
                                                            : new Uri("https://www.microsoft.com/store/apps/9WZDNCRDZHS0", UriKind.Absolute)
                                                    };
                                                    task.Show();
                                                }
                                            },
                                            richTextBox);
                                    }
                                }));
                        }
                    }
                }
            });
        }
#endif

        public static string GetAccessToken(Dictionary<string, string> uriParams, out PageKind pageKind)
        {
            pageKind = PageKind.Dialog;
            var accessToken = string.Empty;
            if (uriParams.ContainsKey("start"))
            {
                accessToken = uriParams["start"];
            }
            else if (uriParams.ContainsKey("startgroup"))
            {
                pageKind = PageKind.Search;
                accessToken = uriParams["startgroup"];
            }

            return accessToken;
        }

        public static string GetGame(Dictionary<string, string> uriParams)
        {
            var post = string.Empty;
            if (uriParams.ContainsKey("game"))
            {
                post = uriParams["game"];
            }

            return post;
        }

        public static string GetPost(Dictionary<string, string> uriParams)
        {
            var post = string.Empty;
            if (uriParams.ContainsKey("post"))
            {
                post = uriParams["post"];
            }

            return post;
        }

        public static string GetPhone(Dictionary<string, string> uriParams)
        {
            var post = string.Empty;
            if (uriParams.ContainsKey("phone"))
            {
                post = uriParams["phone"];
            }

            return post;
        }

        public static string GetHash(Dictionary<string, string> uriParams)
        {
            var post = string.Empty;
            if (uriParams.ContainsKey("hash"))
            {
                post = uriParams["hash"];
            }

            return post;
        }

        public static string GetParam(string paramName, Dictionary<string, string> uriParams)
        {
            var result = string.Empty;
            if (uriParams.ContainsKey(paramName))
            {
                result = uriParams[paramName];
            }

            return result;
        }

        public static void NavigateToStickers(IMTProtoService mtProtoService, IStateService stateService, TLInputStickerSetBase inputStickerSet, System.Action stickerSetAdded = null, System.Action stickerSetRemoved = null)
        {
#if WP8
            if (mtProtoService != null)
            {
                stateService.GetAllStickersAsync(cachedStickers =>
                {
                    TLStickerSetBase set = null;
                    var allStickers = cachedStickers as TLAllStickers29;
                    if (allStickers != null)
                    {
                        set = allStickers.Sets.FirstOrDefault(x => x.Id.Value.ToString() == inputStickerSet.Name.ToString());
                    }

                    Execute.BeginOnUIThread(() =>
                    {
                        if (set != null)
                        {
                            set.Stickers = new TLVector<TLObject>();
                            for (var i = 0; i < allStickers.Documents.Count; i++)
                            {
                                var document22 = allStickers.Documents[i] as TLDocument22;
                                if (document22 != null)
                                {
                                    var documentAttributeSticker = document22.Attributes.FirstOrDefault(x => x is TLDocumentAttributeSticker29) as TLDocumentAttributeSticker29;
                                    if (documentAttributeSticker != null)
                                    {
                                        var stickerSetId = documentAttributeSticker.Stickerset as TLInputStickerSetId;
                                        if (stickerSetId != null && stickerSetId.Id.Value != set.Id.Value)
                                        {
                                            continue;
                                        }
                                        var stickerSetShortName = documentAttributeSticker.Stickerset as TLInputStickerSetShortName;
                                        if (stickerSetShortName != null
                                            && !TLString.Equals(stickerSetShortName.ShortName, set.ShortName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            continue;
                                        }
                                    }

                                    set.Stickers.Add(new TLStickerItem { Document = document22 });
                                }
                            }

                            ShowStickerSetMessageBox(true, true, set, prompt =>
                            {
                                if (prompt == PopUpResult.Ok)
                                {
                                    mtProtoService.UninstallStickerSetAsync(inputStickerSet,
                                        result => Execute.BeginOnUIThread(() =>
                                        {
                                            var shellViewModel = IoC.Get<ShellViewModel>();
                                            shellViewModel.RemoveStickerSet(set, inputStickerSet);

                                            mtProtoService.SetMessageOnTime(2.0, AppResources.StickersRemoved);

                                            stickerSetRemoved.SafeInvoke();
                                        }),
                                        error =>
                                            Execute.BeginOnUIThread(
                                                () => { Execute.ShowDebugMessage("messages.uninstallStickerSet error " + error); }));
                                }
                            });
                        }
                        else
                        {
                            var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                            if (frame != null) frame.OpenBlockingProgress();

                            mtProtoService.GetStickerSetAsync(inputStickerSet,
                                stickerSet => Execute.BeginOnUIThread(() =>
                                {
                                    if (frame != null) frame.CloseBlockingProgress();

                                    stickerSet.Set.Stickers = new TLVector<TLObject>();
                                    for (var i = 0; i < stickerSet.Documents.Count; i++)
                                    {
                                        var document22 = stickerSet.Documents[i] as TLDocument22;
                                        if (document22 != null)
                                        {
                                            stickerSet.Set.Stickers.Add(new TLStickerItem { Document = document22 });
                                        }
                                    }

                                    var stickerSet32 = stickerSet.Set as TLStickerSet32;
                                    if (stickerSet32 != null)
                                    {
                                        ShowStickerSetMessageBox(true, stickerSet32.Installed && !stickerSet32.Archived, stickerSet.Set, prompt =>
                                        {
                                            if (prompt == PopUpResult.Ok)
                                            {
                                                AddRemoveStickerSet(mtProtoService, stickerSet, stickerSetAdded, stickerSetRemoved);
                                            }
                                        });
                                    }
                                }),
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    if (frame != null) frame.CloseBlockingProgress();
                                    if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                                    {
                                        if (error.TypeEquals(ErrorType.STICKERSET_INVALID))
                                        {
                                            MessageBox.Show(AppResources.StickersNotFound, AppResources.Error, MessageBoxButton.OK);
                                        }
                                        else
                                        {
                                            Execute.ShowDebugMessage("messages.getStickerSet error " + error);
                                        }
                                    }
                                    else
                                    {
                                        Execute.ShowDebugMessage("messages.getStickerSet error " + error);
                                    }
                                }));
                        }
                    });
                });
            }
#endif
        }

        public static void AddRemoveStickerSet(IMTProtoService mtProtoService, TLMessagesStickerSet messagesStickerSet, System.Action stickerSetAdded = null, System.Action stickerSetRemoved = null)
        {
            var stickerSet32 = messagesStickerSet.Set as TLStickerSet32;
            if (stickerSet32 != null)
            {
                var inputStickerSet = new TLInputStickerSetId{ Id = stickerSet32.Id, AccessHash = stickerSet32.AccessHash };

                if (!stickerSet32.Installed || stickerSet32.Archived)
                {
                    mtProtoService.InstallStickerSetAsync(inputStickerSet, TLBool.False,
                        result => Execute.BeginOnUIThread(() =>
                        {
                            var resultArchive = result as TLStickerSetInstallResultArchive;
                            if (resultArchive != null)
                            {
                                ShowArchivedStickersMessageBox(resultArchive);
                            }

                            stickerSet32.Installed = true;
                            var stickerSet76 = stickerSet32 as TLStickerSet76;
                            if (stickerSet76 != null)
                            {
                                stickerSet76.InstalledDate = TLUtils.DateToUniversalTimeTLInt(IoC.Get<IMTProtoService>().ClientTicksDelta, DateTime.Now);
                            }

                            var shellViewModel = IoC.Get<ShellViewModel>();
                            shellViewModel.Handle(new TLUpdateNewStickerSet { Stickerset = messagesStickerSet });

                            mtProtoService.SetMessageOnTime(2.0, AppResources.NewStickersAdded);

                            stickerSetAdded.SafeInvoke();
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                            {
                                if (error.TypeEquals(ErrorType.STICKERSET_INVALID))
                                {
                                    MessageBox.Show(AppResources.StickersNotFound, AppResources.Error, MessageBoxButton.OK);
                                }
                                else
                                {
                                    Execute.ShowDebugMessage("messages.importChatInvite error " + error);
                                }
                            }
                            else
                            {
                                Execute.ShowDebugMessage("messages.importChatInvite error " + error);
                            }
                        }));
                }
                else
                {
                    mtProtoService.UninstallStickerSetAsync(inputStickerSet,
                        result => Execute.BeginOnUIThread(() =>
                        {
                            stickerSet32.Installed = false;
                            var stickerSet76 = stickerSet32 as TLStickerSet76;
                            if (stickerSet76 != null)
                            {
                                stickerSet76.InstalledDate = null;
                            }

                            var shellViewModel = IoC.Get<ShellViewModel>();
                            shellViewModel.RemoveStickerSet(stickerSet32, inputStickerSet);

                            mtProtoService.SetMessageOnTime(2.0, AppResources.StickersRemoved);

                            stickerSetRemoved.SafeInvoke();
                        }),
                        error => Execute.BeginOnUIThread(() => { Execute.ShowDebugMessage("messages.uninstallStickerSet error " + error); }));
                }
            }
        }

        public static void ShowAttachedStickersMessageBox(IStickers attachedStickers, Action<TLStickerSetBase> callback)
        {
            var stickerSets = new Dictionary<string, TLVector<TLStickerItem>>();
            for (var i = 0; i < attachedStickers.Documents.Count; i++)
            {
                var document22 = attachedStickers.Documents[i] as TLDocument22;
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

            for (var i = 0; i < attachedStickers.Sets.Count; i++)
            {
                var set = attachedStickers.Sets[i];

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

            var controls = new List<StickerSetControl>();
            var content = new Grid { Margin = new Thickness(-12.0, 24.0, 0.0, -12.0), Background = new SolidColorBrush(Colors.Transparent) };

            for (var i = 0; i < attachedStickers.Sets.Count && i < 5; i++)
            {
                content.RowDefinitions.Add(new RowDefinition());

                var stickerSetControl = new StickerSetControl
                {
                    DataContext = attachedStickers.Sets[i]
                };

                Grid.SetRow(stickerSetControl, i);
                content.Children.Add(stickerSetControl);
                controls.Add(stickerSetControl);
            }

            var messageBox = ShellViewModel.ShowCustomMessageBox(null, AppResources.AttachedStickers,
                AppResources.Ok.ToLowerInvariant(), null,
                dismissed =>
                {
                    
                },
                content);

            foreach (var stickerSetControl in controls)
            {
                stickerSetControl.Tap += (o, e) =>
                {
                    messageBox.Dismiss();

                    Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                    {
                        var stickerSetBase = stickerSetControl.DataContext as TLStickerSetBase;
                        callback.SafeInvoke(stickerSetBase);
                    });
                };
            }
        }

        public static void ShowArchivedStickersMessageBox(IStickers resultArchive)
        {
            var stickerSets = new Dictionary<string, TLVector<TLStickerItem>>();
            for (var i = 0; i < resultArchive.Documents.Count; i++)
            {
                var document22 = resultArchive.Documents[i] as TLDocument22;
                if (document22 != null)
                {
                    if (document22.StickerSet != null)
                    {
                        var setId = document22.StickerSet.Name;
                        TLVector<TLStickerItem> stickers;
                        if (stickerSets.TryGetValue(setId, out stickers))
                        {
                            stickers.Add(new TLStickerItem {Document = document22});
                        }
                        else
                        {
                            stickerSets[setId] = new TLVector<TLStickerItem> {new TLStickerItem {Document = document22}};
                        }
                    }
                }
            }

            for (var i = 0; i < resultArchive.Sets.Count; i++)
            {
                var set = resultArchive.Sets[i];

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

            var content = new Grid {Margin = new Thickness(-12.0, 24.0, 0.0, -12.0)};
            for (var i = 0; i < resultArchive.Sets.Count; i++)
            {
                content.RowDefinitions.Add(new RowDefinition());

                var stickerSetControl = new StickerSetControl
                {
                    DataContext = resultArchive.Sets[i]
                };
                Grid.SetRow(stickerSetControl, i);
                content.Children.Add(stickerSetControl);
            }

            ShellViewModel.ShowCustomMessageBox(AppResources.ArchivedStickersAbout, AppResources.ArchivedStickers,
                AppResources.Ok.ToLowerInvariant(), AppResources.Settings.ToLowerInvariant(),
                dismissed =>
                {
                    if (dismissed == CustomMessageBoxResult.LeftButton)
                    {
                        Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25),
                            () => IoC.Get<INavigationService>().UriFor<ArchivedStickersViewModel>().Navigate());
                    }
                },
                content);
        }

        protected static MessagePrompt _lastMessagePrompt;

        public static TLStickerSetBase _stickerSet;

        public static void ShowStickerSetMessageBox(bool sendEnabled, bool stickerSetExists, TLStickerSetBase stickerSet, Action<PopUpResult> callback)
        {
            if (stickerSet == null) return;

            var stickerSet32 = stickerSet as TLStickerSet32;

            _stickerSet = stickerSet;
            _fromItem = null;
            _storyboard = null;

            var panel = new Canvas { VerticalAlignment = VerticalAlignment.Top, UseOptimizedManipulationRouting = false };
            panel.ManipulationStarted += StcikerPanel_ManipulationStarted;
            panel.ManipulationDelta += StickerPanel_ManipulationDelta;
            panel.ManipulationCompleted += StickerPanel_ManipulationCompleted;
            panel.Loaded += (o, e) =>
            {
                //Touch.FrameReported += Touch_FrameReported;
            };
            panel.Unloaded += (o, e) =>
            {
                Touch.FrameReported -= Touch_FrameReported;
            };

            var scrollViewer = new ScrollViewer
            {
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Height = 400.0,
                Content = panel
            };

            var showEmoji = stickerSet32 != null && !stickerSet32.Masks;
            var sprites = CreateStickerSetSprites(showEmoji, sendEnabled, stickerSet);

            var messagePrompt = new MessagePrompt
            {
                Title = stickerSet.Title.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                Message = (string) new StickerSetToCountStringConverter().Convert(stickerSet, null, null, null),
                Body = new TextBlock
                {
                    Height = scrollViewer.Height,
                    Text = AppResources.Loading,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Style = (Style) Application.Current.Resources["PhoneTextGroupHeaderStyle"]
                },
                IsCancelVisible = stickerSetExists,
                IsAppBarVisible = true
            };

            messagePrompt.ActionPopUpButtons.Clear();
            var cancelButton = new Button { Width = 220.0 };
            cancelButton.Click += (sender, args) =>
            {
                messagePrompt.OnCompleted(new PopUpEventArgs<string, PopUpResult> { PopUpResult = PopUpResult.Cancelled });
            };
            cancelButton.Content = AppResources.Cancel.ToLowerInvariant();
            messagePrompt.ActionPopUpButtons.Add(cancelButton);

            var okButton = new Button { Width = 220.0, IsEnabled = stickerSet32 == null || (!stickerSet32.Official || stickerSet32.Masks) };
            okButton.Click += (sender, args) =>
            {
                messagePrompt.OnCompleted(new PopUpEventArgs<string, PopUpResult> { PopUpResult = PopUpResult.Ok });
            };
            okButton.Content = !stickerSetExists ? AppResources.Add.ToLowerInvariant() : AppResources.Remove.ToLowerInvariant();
            messagePrompt.ActionPopUpButtons.Add(okButton);
#if WP8
            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225 || Environment.OSVersion.Version.Major >= 10;
            if (isFullHD)
            {
                cancelButton.FontSize = 17.667;
                cancelButton.BorderThickness = new Thickness(2.0);
                cancelButton.Padding = new Thickness(10.0, 5.0, 10.0, 7.0);

                okButton.FontSize = 17.667;
                okButton.BorderThickness = new Thickness(2.0);
                okButton.Padding = new Thickness(10.0, 5.0, 10.0, 7.0);
            }
#endif

            messagePrompt.Opened += (o, args) =>
            {
                var stickerPreviewImage = new Image
                {
                    Width = 96.0,
                    Height = 96.0,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsHitTestVisible = false,
                };

                var stickerImage = new Image
                {
                    Width = 96.0,
                    Height = 96.0,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsHitTestVisible = false,
                    DataContext = null
                };

                var binding = new Binding
                {
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath("Document"),
                    Converter = new DefaultPhotoConverter()
                };

                stickerImage.SetBinding(Image.SourceProperty, binding);

                var stickerPreviewGrid = new Grid
                {
                    IsHitTestVisible = false,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new CompositeTransform { ScaleX = 2.6, ScaleY = 2.6 },
                };
                stickerPreviewGrid.Children.Add(stickerPreviewImage);
                stickerPreviewGrid.Children.Add(stickerImage);

                stickerPreviewGrid.Loaded += StickerPreviewGrid_OnLoaded;

                var debugText = new TextBlock{Visibility = Visibility.Collapsed};

                var stickerPreview = new Grid{ IsHitTestVisible = false, Visibility = Visibility.Collapsed };
                stickerPreview.Children.Add(new Border
                {
                    Opacity = 0.5,
                    Margin = new Thickness(-12.0, -54.0, -12.0, isFullHD? -86.0 : -96.0),
                    Background = (Brush) Application.Current.Resources["PhoneBackgroundBrush"],
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsHitTestVisible = false
                });
                stickerPreview.Children.Add(stickerPreviewGrid);
                stickerPreview.Children.Add(debugText);

                ViewportControl = scrollViewer;
                Canvas = panel;
                Image = stickerImage;
                PreviewImage = stickerPreviewImage;
                PreviewGrid = stickerPreviewGrid;
                Preview = stickerPreview;
                DebugText = debugText;

                var grid = new Grid();
                grid.Children.Add(scrollViewer);
                grid.Children.Add(stickerPreview);

                Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.1), () =>
                {
                    var top = 0.0;
                    messagePrompt.Body = grid;

                    const int firstSliceLength = 4;
                    foreach (var sprite in sprites.Take(firstSliceLength).Cast<FrameworkElement>())
                    {
                        Canvas.SetTop(sprite, top);
                        panel.Children.Add(sprite);
                        top += 120.0;
                    }
                    Execute.BeginOnUIThread(() =>
                    {
                        foreach (var sprite in sprites.Skip(firstSliceLength).Cast<FrameworkElement>())
                        {
                            Canvas.SetTop(sprite, top);
                            panel.Children.Add(sprite);
                            top += 120.0;
                        }
                        panel.Height = top;
                    });
                });
            };
            messagePrompt.Completed += (o, e) =>
            {
                _stickerSet = null;

                callback.SafeInvoke(e.PopUpResult);
            };
            _lastMessagePrompt = messagePrompt;
            messagePrompt.Show();
        }

        private static List<FrameworkElement> CreateStickerSetSprites(bool showEmoji, bool sendEnabled, TLStickerSetBase stickerSet)
        {
            if (stickerSet == null) return null;

            const int stickersPerRow = 4;
            var sprites = new List<FrameworkElement>();
            var stickers = new List<TLStickerItem>();
            for (var j = 1; j <= stickerSet.Stickers.Count; j++)
            {
                stickers.Add((TLStickerItem)stickerSet.Stickers[j - 1]);

                if (j % stickersPerRow == 0 || j == stickerSet.Stickers.Count)
                {
                    //var item = new StickerSpriteItem(stickersPerRow, stickers, 96.0, 438.0, true);
                    //item.StickerImage.MouseEnter += StickerPanel_MouseEnter;

                    var panelWidth = 438.0;

                    var panelMargin = new Thickness(4.0, 0.0, 4.0, 0.0);
                    var panelActualWidth = panelWidth - panelMargin.Left - panelMargin.Right;
                    //472, 438
                    var stackPanel = new Grid { Width = panelActualWidth, Margin = panelMargin, Background = new SolidColorBrush(Colors.Transparent) };

                    for (var i = 0; i < stickersPerRow; i++)
                    {
                        stackPanel.ColumnDefinitions.Add(new ColumnDefinition());
                    }

                    for (var i = 0; i < stickers.Count; i++)
                    {
                        var binding = new Binding
                        {
                            Mode = BindingMode.OneWay,
                            Path = new PropertyPath("Self"),
                            Converter = new DefaultPhotoConverter(),
                            ConverterParameter = 96.0
                        };

                        var stickerImage = new Image
                        {
                            Height = 96.0,
                            Margin = new Thickness(0, 12, 0, 12),
                            VerticalAlignment = VerticalAlignment.Top,
                            CacheMode = new BitmapCache()
                        };
                        stickerImage.MouseEnter += StickerPanel_MouseEnter;
                        if (sendEnabled)
                        {
                            stickerImage.Tap += Sticker_Tap;
                        }

                        stickerImage.SetBinding(Image.SourceProperty, binding);

                        var grid = new Grid();
                        grid.Children.Add(stickerImage);

                        if (showEmoji)
                        {
                            var document22 = stickers[i].Document as TLDocument22;
                            if (document22 != null)
                            {
                                var bytes = Encoding.BigEndianUnicode.GetBytes(document22.Emoticon);
                                var bytesStr = BrowserNavigationService.ConvertToHexString(bytes);

                                var emojiImage = new Image
                                {
                                    Height = 32,
                                    Width = 32,
                                    Margin = new Thickness(12, 12, 12, 12),
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    VerticalAlignment = VerticalAlignment.Bottom,
                                    Source = new BitmapImage(new Uri(string.Format("/Assets/Emoji/Separated/{0}.png", bytesStr), UriKind.RelativeOrAbsolute)),
                                    IsHitTestVisible = false
                                };
                                grid.Children.Add(emojiImage);
                            }
                        }

                        var listBoxItem = new ListBoxItem { Content = grid, DataContext = stickers[i] };
                        Microsoft.Phone.Controls.TiltEffect.SetIsTiltEnabled(listBoxItem, true);
                        grid.DataContext = stickers[i];
                        Grid.SetColumn(listBoxItem, i);
                        stackPanel.Children.Add(listBoxItem);
                    }

                    sprites.Add(stackPanel);
                    stickers.Clear();
                }
            }

            return sprites;
        }

        private static FrameworkElement _fromItem;
        private static Storyboard _storyboard;
        private static FrameworkElement _lastMouseEnter;
        private static ScrollViewer ViewportControl;
        private static Canvas Canvas;
        private static Grid Preview;
        private static Grid PreviewGrid;
        private static Image PreviewImage;
        private static Image Image;
        private static TextBlock DebugText;
        private static Storyboard _loadedStoryboard;

        public static readonly DependencyProperty PreviousTopProperty = DependencyProperty.RegisterAttached("PreviousTop", typeof (double?), typeof (TelegramViewBase), new PropertyMetadata(null));

        public static void SetPreviousTop(DependencyObject element, double? value)
        {
            element.SetValue(PreviousTopProperty, value);
        }

        public static double? GetPreviousTop(DependencyObject element)
        {
            return (double?) element.GetValue(PreviousTopProperty);
        }

        private static void Sticker_Tap(object sender, GestureEventArgs e)
        {
            return;

            if (Preview.Visibility == Visibility.Collapsed)
            {
                Preview.Visibility = Visibility.Visible;
            }

            DebugText.Text = sender.GetHashCode().ToString();

            //_lastMouseEnter = e.OriginalSource as FrameworkElement;

            var stickerImage = e.OriginalSource as Image;
            if (stickerImage != null)
            {
                PreviewImage.Source = stickerImage.Source;

                var stickerItem = stickerImage.DataContext as TLStickerItem;
                if (stickerItem != null)
                {
                    Image.DataContext = stickerItem;
                }
            }

            var duration = .5;
            var easingFunction = new ElasticEase { Oscillations = 1, Springiness = 10.0, EasingMode = EasingMode.EaseOut };
            var storyboard = new Storyboard();

            var doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0.0;
            doubleAnimation.To = 0.0;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            storyboard.Children.Add(doubleAnimation);

            var doubleAnimation2 = new DoubleAnimation();
            doubleAnimation2.From = 0.0; //position.Y;
            doubleAnimation2.To = 0.0;
            doubleAnimation2.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation2.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation2, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(doubleAnimation2);

            var doubleAnimation3 = new DoubleAnimation();
            doubleAnimation3.From = 1.0;
            doubleAnimation3.To = 1.0;
            doubleAnimation3.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation3.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation3, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation3, new PropertyPath("Opacity"));
            storyboard.Children.Add(doubleAnimation3);

            var doubleAnimation4 = new DoubleAnimation();
            doubleAnimation4.From = 2.4;
            doubleAnimation4.To = 2.6;
            doubleAnimation4.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation4.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation4, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation4, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));
            storyboard.Children.Add(doubleAnimation4);

            var doubleAnimation5 = new DoubleAnimation();
            doubleAnimation5.From = 2.4;
            doubleAnimation5.To = 2.6;
            doubleAnimation5.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation5.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation5, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation5, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));
            storyboard.Children.Add(doubleAnimation5);

            storyboard.Begin();

            _storyboard = storyboard;
        }

        private static void StickerPreviewGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            var fromItem = _fromItem;
            if (fromItem == null) return;

            var position = fromItem.TransformToVisual(Application.Current.RootVisual).Transform(new Point(fromItem.ActualWidth / 2.0, fromItem.ActualHeight / 2.0));
            DebugText.Text = position.ToString();

            var position2 = new Point(240.0, 400.0); //PreviewImage.TransformToVisual(Application.Current.RootVisual).Transform(new Point(PreviewImage.ActualWidth / 2.0, PreviewImage.ActualHeight / 2.0));//
            DebugText.Text += Environment.NewLine + position2;

            var duration = .75;
            IEasingFunction easingFunction = new ElasticEase { Oscillations = 1, Springiness = 10.0, EasingMode = EasingMode.EaseOut };
            var storyboard = new Storyboard();

            var doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = position.X - position2.X;
            doubleAnimation.To = 0.0;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            storyboard.Children.Add(doubleAnimation);

            var doubleAnimation2 = new DoubleAnimation();
            doubleAnimation2.From = position.Y - position2.Y; //position.Y;
            doubleAnimation2.To = 0.0;
            doubleAnimation2.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation2.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation2, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(doubleAnimation2);

            var doubleAnimation3 = new DoubleAnimation();
            doubleAnimation3.From = .5;
            doubleAnimation3.To = 1.0;
            doubleAnimation3.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation3.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation3, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation3, new PropertyPath("Opacity"));
            storyboard.Children.Add(doubleAnimation3);

            var doubleAnimation4 = new DoubleAnimation();
            doubleAnimation4.From = 1.0;
            doubleAnimation4.To = 2.6;
            doubleAnimation4.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation4.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation4, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation4, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));
            storyboard.Children.Add(doubleAnimation4);

            var doubleAnimation5 = new DoubleAnimation();
            doubleAnimation5.From = 1.0;
            doubleAnimation5.To = 2.6;
            doubleAnimation5.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation5.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation5, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation5, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));
            storyboard.Children.Add(doubleAnimation5);

            storyboard.Begin();

            if (_loadedStoryboard != null)
            {
                _loadedStoryboard.Begin();
                _loadedStoryboard = null;
            }

            _storyboard = storyboard;
        }

        private static void StickerPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Preview.Visibility == Visibility.Collapsed) return;

            DebugText.Text = sender.GetHashCode().ToString();

            var st1 = EmojiControl.GetScaleStoryboard(_lastMouseEnter ?? _fromItem, 1.0, 1.0);

            _lastMouseEnter = e.OriginalSource as FrameworkElement;

            var stickerImage = e.OriginalSource as Image;
            if (stickerImage != null)
            {
                PreviewImage.Source = stickerImage.Source;
                
                var stickerItem = stickerImage.DataContext as TLStickerItem;
                if (stickerItem != null)
                {
                    Image.DataContext = stickerItem;
                }
            }

            var duration = .5;
            var easingFunction = new ElasticEase { Oscillations = 1, Springiness = 10.0, EasingMode = EasingMode.EaseOut };
            var storyboard = new Storyboard();

            var doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0.0;
            doubleAnimation.To = 0.0;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            storyboard.Children.Add(doubleAnimation);

            var doubleAnimation2 = new DoubleAnimation();
            doubleAnimation2.From = 0.0; //position.Y;
            doubleAnimation2.To = 0.0;
            doubleAnimation2.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation2.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation2, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(doubleAnimation2);

            var doubleAnimation3 = new DoubleAnimation();
            doubleAnimation3.From = 1.0;
            doubleAnimation3.To = 1.0;
            doubleAnimation3.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation3.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation3, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation3, new PropertyPath("Opacity"));
            storyboard.Children.Add(doubleAnimation3);

            var doubleAnimation4 = new DoubleAnimation();
            doubleAnimation4.From = 2.4;
            doubleAnimation4.To = 2.6;
            doubleAnimation4.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation4.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation4, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation4, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));
            storyboard.Children.Add(doubleAnimation4);

            var doubleAnimation5 = new DoubleAnimation();
            doubleAnimation5.From = 2.4;
            doubleAnimation5.To = 2.6;
            doubleAnimation5.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation5.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation5, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation5, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));
            storyboard.Children.Add(doubleAnimation5);

            storyboard.Begin();

            _storyboard = storyboard;

            var st2 = EmojiControl.GetScaleStoryboard(_lastMouseEnter, 0.85, 1.0);
            if (st1 != null || st2 != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    if (st1 != null) st1.Begin();
                    if (st2 != null) st2.Begin();
                });
            }
        }

        private static DateTime? _startTime;

        private static ManipulationStartedEventArgs _manipulationStartedArgs;

        private static void StcikerPanel_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _fromItem = e.OriginalSource as FrameworkElement;
            _lastMouseEnter = null;

            _manipulationStartedArgs = e;
            _startTime = DateTime.Now;
            Touch.FrameReported += Touch_FrameReported;
        }

        private static void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            if (_manipulationStartedArgs == null)
            {
                Touch.FrameReported -= Touch_FrameReported;
                return;
            }

            var point = e.GetPrimaryTouchPoint(null);
            if (point.Action == TouchAction.Up)
            {
                Touch.FrameReported -= Touch_FrameReported;
                return;
            }

            var manipulationPoint = e.GetPrimaryTouchPoint(_manipulationStartedArgs.ManipulationContainer);
            var length = Math.Pow(manipulationPoint.Position.X - _manipulationStartedArgs.ManipulationOrigin.X, 2.0)
                + Math.Pow(manipulationPoint.Position.Y - _manipulationStartedArgs.ManipulationOrigin.Y, 2.0);
            if (length > 30.0 * 30.0)
            {
                Touch.FrameReported -= Touch_FrameReported;
                return;
            }

            if (_startTime.HasValue && _startTime.Value.AddSeconds(0.5) <= DateTime.Now)
            {
                Touch.FrameReported -= Touch_FrameReported;
                var offset = ViewportControl.VerticalOffset;
                foreach (var child in Canvas.Children)
                {
                    var top = Canvas.GetTop(child);
                    SetPreviousTop(child, top);
                    Canvas.SetTop(child, top - offset);
                }
                ViewportControl.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                _loadedStoryboard = EmojiControl.GetScaleStoryboard(_fromItem, 0.85, 1.0);

                Preview.Visibility = Visibility.Visible;
                var stickerImage = _fromItem as Image;
                if (stickerImage != null)
                {
                    PreviewImage.Source = stickerImage.Source;

                    var stickerItem = stickerImage.DataContext as TLStickerItem;
                    if (stickerItem != null)
                    {
                        Image.DataContext = stickerItem;
                    }
                }

                var grid = Preview;
                grid.Children.Remove(PreviewGrid);

                Execute.BeginOnUIThread(() =>
                {
                    PreviewGrid.RenderTransform = new CompositeTransform();
                    PreviewGrid.Opacity = 0.0;
                    grid.Children.Add(PreviewGrid);
                });
            }
        }

        private static void StickerPanel_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {

        }

        private static void StickerPanel_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var fromItem = _fromItem;

            _fromItem = null;
            if (_storyboard != null)
            {
                _storyboard.SkipToFill();
            }

            foreach (var child in Canvas.Children)
            {
                var previousTop = GetPreviousTop(child);
                if (previousTop.HasValue)
                {
                    Canvas.SetTop(child, previousTop.Value);
                }
            }
            ViewportControl.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Preview.Visibility = Visibility.Collapsed;

            var st = EmojiControl.GetScaleStoryboard(_lastMouseEnter ?? fromItem, 1.0, 1.0);
            if (st != null)
            {
                Execute.BeginOnUIThread(st.Begin);
            }
        }

        public static Path CreateXamlCancel(FrameworkElement control)
        {
            var path = XamlReader.Load("<Path \r\n\t\t\t\t\txmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"\r\n\t\t\t\t\tStretch=\"Uniform\" \r\n\t\t\t\t\tData=\"M15.047,0 L17.709,2.663 L11.5166,8.85499 L17.71,15.048 L15.049,17.709 L8.8553,11.5161 L2.662,17.709 L0,15.049 L6.19351,8.85467 L0.002036,2.66401 L2.66304,0.002015 L8.85463,6.19319 z\"\r\n\t\t\t\t\t/>") as Path;
            if (path != null)
                ApplyBinding(control, path, "ButtonHeight", HeightProperty, new NumberMultiplierConverter(), 0.25);
            return path;
        }

        public static void ApplyBinding(FrameworkElement source, FrameworkElement target, string propertyPath, DependencyProperty property, IValueConverter converter = null, object converterParameter = null)
        {
            if (source == null || target == null)
                return;
            target.SetBinding(property, new Binding()
            {
                Source = (object)source,
                Path = new PropertyPath(propertyPath, new object[0]),
                Converter = converter,
                ConverterParameter = converterParameter
            });
        }

        public static void NavigateToInviteLink(IMTProtoService mtProtoService, string link)
        {
            if (mtProtoService != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    var frame = Application.Current.RootVisual as TelegramTransitionFrame;
                    if (frame != null) frame.OpenBlockingProgress();

                    mtProtoService.CheckChatInviteAsync(new TLString(link),
                        chatInviteBase => Execute.BeginOnUIThread(() =>
                        {
                            if (frame != null) frame.CloseBlockingProgress();

                            var chatInviteAlready = chatInviteBase as TLChatInviteAlready;
                            if (chatInviteAlready != null)
                            {
                                var chat = chatInviteAlready.Chat;
                                NavigateToGroup(chat);
                                return;
                            }

                            var chatInvite = chatInviteBase as TLChatInvite;
                            if (chatInvite != null)
                            {
                                var chatInvite40 = chatInvite as TLChatInvite40;
                                if (chatInvite40 != null && chatInvite40.IsChannel && !chatInvite40.IsMegaGroup)
                                {
                                    var confirmationString = AppResources.JoinChannelConfirmation;
                                    var confirmation = MessageBox.Show(string.Format(confirmationString, chatInvite.Title), AppResources.Confirm, MessageBoxButton.OKCancel);
                                    if (confirmation == MessageBoxResult.OK)
                                    {
                                        ImportChatInviteAsync(mtProtoService, link);
                                    }
                                    return;
                                }

                                var content = new Grid();
                                content.RowDefinitions.Add(new RowDefinition());
                                content.RowDefinitions.Add(new RowDefinition());
                                content.RowDefinitions.Add(new RowDefinition());
                                content.ColumnDefinitions.Add(new ColumnDefinition{ Width = GridLength.Auto });
                                content.ColumnDefinitions.Add(new ColumnDefinition());
                                var chatInviteViewModel = new ChatInviteViewModel(IoC.Get<ITelegramEventAggregator>());

                                var chatInvite54 = chatInvite as TLChatInvite54;
                                if (chatInvite54 != null)
                                {
                                    var chat = new TLChat
                                    {
                                        Id = new TLInt(chatInvite54.Title.Value.GetHashCode()),
                                        Title = chatInvite54.Title
                                    };
                                    chatInvite54.Chat = chat;

                                    chatInviteViewModel.ChatInvite = chatInvite54;

                                    var chatInviteControl = new ChatInviteControl
                                    {
                                        HorizontalAlignment = HorizontalAlignment.Stretch
                                    };
                                    chatInviteControl.DataContext = chatInviteViewModel;

                                    content.Children.Add(chatInviteControl);
                                }

                                ShellViewModel.ShowCustomMessageBox(string.Empty, string.Empty, AppResources.Cancel.ToLowerInvariant(), AppResources.Join.ToLowerInvariant(),
                                    r =>
                                    {
                                        if (r == CustomMessageBoxResult.LeftButton)
                                        {
                                            ImportChatInviteAsync(mtProtoService, link);
                                        }
                                    },
                                    content);

                                return;
                            }
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            if (frame != null) frame.CloseBlockingProgress();
                            if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                            {
                                if (error.TypeEquals(ErrorType.INVITE_HASH_EMPTY)
                                    || error.TypeEquals(ErrorType.INVITE_HASH_INVALID)
                                    || error.TypeEquals(ErrorType.INVITE_HASH_EXPIRED))
                                {
                                    MessageBox.Show(AppResources.GroupNotExistsError, AppResources.Error,  MessageBoxButton.OK);
                                }
                                else
                                {
                                    Execute.ShowDebugMessage("messages.checkChatInvite error " + error);
                                }
                            }
                            else
                            {
                                Execute.ShowDebugMessage("messages.checkChatInvite error " + error);
                            }
                        }));
                });
            }
        }

        private static void ImportChatInviteAsync(IMTProtoService mtProtoService, string link)
        {
            mtProtoService.ImportChatInviteAsync(new TLString(link),
                result =>
                {
                    var updates = result as TLUpdates;
                    if (updates != null)
                    {
                        var chat = updates.Chats.FirstOrDefault();
                        if (chat != null)
                        {
                            var channel = chat as TLChannel;
                            if (channel != null)
                            {
                                mtProtoService.GetHistoryAsync(Stopwatch.StartNew(),
                                    new TLInputPeerChannel {ChatId = channel.Id, AccessHash = channel.AccessHash},
                                    new TLPeerChannel {Id = channel.Id}, false, new TLInt(0), new TLInt(0),
                                    new TLInt(0), new TLInt(Constants.MessagesSlice),
                                    result2 =>
                                    {
                                        var id = new TLVector<TLInt>();
                                        foreach (var message in result2.Messages)
                                        {
                                            id.Add(message.Id);
                                        }
                                        IoC.Get<ICacheService>().DeleteChannelMessages(channel.Id, id);
                                        IoC.Get<ICacheService>()
                                            .SyncPeerMessages(new TLPeerChannel {Id = channel.Id}, result2, true, false,
                                                result3 => { NavigateToGroup(chat); });
                                    },
                                    error2 => { Execute.ShowDebugMessage("messages.getHistory error " + error2); });
                            }
                            else
                            {
                                NavigateToGroup(chat);
                            }
                        }
                    }
                },
                error => Execute.BeginOnUIThread(() =>
                {
                    if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                    {
                        if (error.TypeEquals(ErrorType.INVITE_HASH_EMPTY)
                            || error.TypeEquals(ErrorType.INVITE_HASH_INVALID)
                            || error.TypeEquals(ErrorType.INVITE_HASH_EXPIRED))
                        {
                            MessageBox.Show(AppResources.GroupNotExistsError, AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (error.TypeEquals(ErrorType.USERS_TOO_MUCH))
                        {
                            MessageBox.Show(AppResources.UsersTooMuch, AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (error.TypeEquals(ErrorType.BOTS_TOO_MUCH))
                        {
                            MessageBox.Show(AppResources.BotsTooMuch, AppResources.Error, MessageBoxButton.OK);
                        }
                        else if (error.TypeEquals(ErrorType.USER_ALREADY_PARTICIPANT))
                        {
                            //Execute.BeginOnUIThread(() => MessageBox.Show(string.Format(AppResources.CantFindContactWithUsername, username), AppResources.Error, MessageBoxButton.OK));
                        }
                        else
                        {
                            Execute.ShowDebugMessage("messages.importChatInvite error " + error);
                        }
                    }
                    else
                    {
                        Execute.ShowDebugMessage("messages.importChatInvite error " + error);
                    }
                }));
        }

        private static void NavigateToGroup(TLChatBase chat)
        {
            if (chat == null) return;

            Execute.BeginOnUIThread(() =>
            {
                var navigationService = IoC.Get<INavigationService>();
                IoC.Get<IStateService>().With = chat;
                IoC.Get<IStateService>().RemoveBackEntries = true;
                navigationService.Navigate(new Uri("/Views/Dialogs/DialogDetailsView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
            });
        }

        public static void NavigateToConfirmPhone(IMTProtoService mtProtoService, string phone, string hash)
        {
            if (mtProtoService != null)
            {

                TelegramTransitionFrame frame = null;

                Execute.BeginOnUIThread(() =>
                {
                    frame = Application.Current.RootVisual as TelegramTransitionFrame;
                    if (frame != null) frame.OpenBlockingProgress();
                });


                mtProtoService.SendConfirmPhoneCodeAsync(new TLString(hash), null,
                    result => Execute.BeginOnUIThread(() =>
                    {
                        if (frame != null) frame.CloseBlockingProgress();

                        NavigateToConfirmPhone(result);

                        //var peerUser = result.Peer as TLPeerUser;
                        //if (peerUser != null)
                        //{
                        //    var userBase = result.Users.FirstOrDefault();
                        //    if (userBase != null)
                        //    {
                        //        var user = userBase as TLUser;
                        //        if (user != null && user.IsBot)
                        //        {
                        //            NavigateToUser(userBase, accessToken, PageKind.Dialog);
                        //        }
                        //        else
                        //        {
                        //            NavigateToUser(userBase, accessToken, pageKind);
                        //        }

                        //        return;
                        //    }
                        //}

                        //var peerChannel = result.Peer as TLPeerChannel;
                        //var peerChat = result.Peer as TLPeerChat;
                        //if (peerChannel != null || peerChat != null)
                        //{
                        //    var chat = result.Chats.FirstOrDefault();
                        //    if (chat != null)
                        //    {
                        //        NavigateToChat(chat, post);
                        //        return;
                        //    }
                        //}

                        //MessageBox.Show(string.Format(AppResources.CantFindContactWithUsername, username), AppResources.Error, MessageBoxButton.OK);
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        if (frame != null) frame.CloseBlockingProgress();

                        if (error.CodeEquals(ErrorCode.BAD_REQUEST)
                            && error.TypeEquals(ErrorType.USERNAME_NOT_OCCUPIED))
                        {
                            //MessageBox.Show(string.Format(AppResources.CantFindContactWithUsername, username), AppResources.Error, MessageBoxButton.OK);
                        }
                        else
                        {
                            Execute.ShowDebugMessage(string.Format("account.sendConfirmPhoneCode error {0}", error));
                        }
                    }));
            }
        }

        public static void NavigateToUsername(IMTProtoService mtProtoService, string username, string accessToken, string post, string game, PageKind pageKind = PageKind.Dialog)
        {
            if (mtProtoService != null)
            {
                var cachedUser = IoC.Get<ICacheService>().GetUser(username);
                if (cachedUser != null)
                {
                    if (!string.IsNullOrEmpty(game))
                    {
                        // telegram.me/bot_name?game=<game_name>
                        // t.me/bot_name?game=<game_name>
                        NavigateToGame(cachedUser, game);
                        return;
                    }

                    // pageKind=Search telegram.me/bot_name?startgroup=<access_token>
                    // pageKind=Search t.me/bot_name?startgroup=<access_token>
                    // pageKind=Dialog telegram.me/bot_name?start=<access_token>
                    // pageKind=Dialog t.me/bot_name?start=<access_token>
                    NavigateToUser(cachedUser, accessToken, pageKind);

                    return;
                }

                var cachedChannel = IoC.Get<ICacheService>().GetChannel(username);
                if (cachedChannel != null)
                {
                    // pageKind=Dialog telegram.me/chat_name?post=<post_number>
                    // pageKind=Dialog t.me/chat_name?post=<post_number>
                    NavigateToChat(cachedChannel, post);

                    return;
                }

                TelegramTransitionFrame frame = null;

                Execute.BeginOnUIThread(() =>
                {
                    frame = Application.Current.RootVisual as TelegramTransitionFrame;
                    if (frame != null) frame.OpenBlockingProgress();
                });


                mtProtoService.ResolveUsernameAsync(new TLString(username),
                    result => Execute.BeginOnUIThread(() => 
                    {
                        if (frame != null) frame.CloseBlockingProgress();

                        var peerUser = result.Peer as TLPeerUser;
                        if (peerUser != null)
                        {
                            var userBase = result.Users.FirstOrDefault();
                            if (userBase != null)
                            {
                                // telegram.me/bot_name?game=<game_name>
                                // t.me/bot_name?game=<game_name>
                                if (!string.IsNullOrEmpty(game))
                                {
                                    NavigateToGame(userBase, game);
                                    return;
                                }

                                // pageKind=Search telegram.me/bot_name?startgroup=<access_token>
                                // pageKind=Search t.me/bot_name?startgroup=<access_token>
                                // pageKind=Dialog telegram.me/bot_name?start=<access_token>
                                // pageKind=Dialog t.me/bot_name?start=<access_token>
                                NavigateToUser(userBase, accessToken, pageKind);

                                return;
                            }
                        }

                        var peerChannel = result.Peer as TLPeerChannel;
                        var peerChat = result.Peer as TLPeerChat;
                        if (peerChannel != null || peerChat != null)
                        {
                            var chat = result.Chats.FirstOrDefault();
                            if (chat != null)
                            {
                                // pageKind=Dialog telegram.me/chat_name?post=<post_number>
                                // pageKind=Dialog t.me/chat_name?post=<post_number>
                                NavigateToChat(chat, post);
                                return;
                            }
                        }

                        MessageBox.Show(string.Format(AppResources.CantFindContactWithUsername, username), AppResources.Error, MessageBoxButton.OK);
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        if (frame != null) frame.CloseBlockingProgress();

                        if (error.CodeEquals(ErrorCode.BAD_REQUEST)
                            && error.TypeEquals(ErrorType.USERNAME_NOT_OCCUPIED))
                        {
                            MessageBox.Show(string.Format(AppResources.CantFindContactWithUsername, username), AppResources.Error, MessageBoxButton.OK);
                        }
                        else
                        {
                            Execute.ShowDebugMessage(string.Format("contacts.resolveUsername {0} error {1}", username, error));
                        }
                    }));
            }
        }

        public static void NavigateToPassport(TLPassportConfig passportConfig, int botId, string scope, string callbackUrl, string publicKey, string payload, bool tryAgain = false)
        {
            Execute.BeginOnUIThread(() =>
            {
                IoC.Get<IMTProtoService>().GetAuthorizationFormAndPassportConfigAsync(
                    new TLInt(botId),
                    new TLString(scope),
                    new TLString(publicKey),
                    passportConfig != null ? passportConfig.Hash : new TLInt(0),
                    (result, resul2) =>
                    {
                        var newPassportConfig = resul2 as TLPassportConfig;
                        if (newPassportConfig != null)
                        {
                            IoC.Get<IStateService>().SavePassportConfig(newPassportConfig);
                            passportConfig = newPassportConfig;
                        }

                        result.Config = passportConfig;
                        result.BotId = new TLInt(botId);
                        result.Scope = new TLString(scope);
                        result.PublicKey = new TLString(publicKey);
                        result.CallbackUrl = new TLString(callbackUrl);
                        result.Payload = new TLString(payload);

                        IoC.Get<IMTProtoService>().GetPasswordAsync(result2 => Execute.BeginOnUIThread(() =>
                        {
                            var password = result2 as TLPassword;
                            if (password != null && password.HasPassword)
                            {
                                IoC.Get<IStateService>().AuthorizationForm = result;
                                IoC.Get<IStateService>().Password = result2;
                                IoC.Get<INavigationService>().UriFor<EnterPasswordViewModel>().WithParam(x => x.RandomParam, Guid.NewGuid().ToString()).Navigate();
                                return;
                            }

                            var noPassword = result2 as TLPassword;
                            if (noPassword != null && !password.HasPassword)
                            {
                                if (!TLString.IsNullOrEmpty(noPassword.EmailUnconfirmedPattern))
                                {
                                    IoC.Get<IStateService>().AuthorizationForm = result;
                                    IoC.Get<IStateService>().Password = result2;
                                    IoC.Get<INavigationService>().UriFor<PasswordViewModel>().Navigate();
                                }
                                else
                                {
                                    IoC.Get<IStateService>().AuthorizationForm = result;
                                    IoC.Get<IStateService>().Password = result2;
                                    IoC.Get<INavigationService>().UriFor<PasswordIntroViewModel>().Navigate();
                                    return;
                                }
                            }
                        }));
                    },
                    error => Execute.BeginOnUIThread(() =>
                    {
                        if (tryAgain && error.CodeEquals(ErrorCode.NOT_FOUND))
                        {
                            Execute.ShowDebugMessage(string.Format("accoung.getAuthorizationForm try_again={0} error={1}", tryAgain, error));
                            NavigateToPassport(passportConfig, botId, scope, callbackUrl, publicKey, payload);
                        }
                        else if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                        {
                            if (error.TypeEquals(ErrorType.APP_VERSION_OUTDATED))
                            {
                                ShellViewModel.ShowCustomMessageBox(
                                    AppResources.UpdateAppAlert,
                                    AppResources.AppName,
                                    AppResources.UpdateApp, AppResources.Cancel,
                                    dismissed =>
                                    {
                                        if (dismissed == CustomMessageBoxResult.RightButton)
                                        {
                                            var updateSerivce = IoC.Get<IWindowsPhoneStoreUpdateService>();
                                            updateSerivce.LaunchAppUpdateAsync();
                                        }
                                    });
                            }
                            else
                            {
                                ShellViewModel.ShowCustomMessageBox(
                                    AppResources.PassportFormError + Environment.NewLine + error.Message,
                                    AppResources.AppName,
                                    AppResources.Ok, null,
                                    dismissed => { });
                            }
                        }
                        else 
                        {
                            Execute.ShowDebugMessage(string.Format("accoung.getAuthorizationForm try_again={0} error={1}", tryAgain, error));
                        }
                    }));
            });
        }

        public static void NavigateToSocksProxy(string server, int port, string username, string password)
        {
            if (string.IsNullOrEmpty(server)) return;
            if (port < 0) return;

            Execute.BeginOnUIThread(() =>
            {
                var proxyString = string.Format("server: {0}\nport: {1}", server, port);
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    proxyString += string.Format("\nusername: {0}\npassword: {1}", username, password);
                }

                var message = string.Format("{0}\n\n{1}\n\n{2}", AppResources.EnableProxyConfigmation, proxyString, AppResources.ChangeProxyServerHint);
                var result = MessageBox.Show(message, AppResources.Proxy, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    var proxy = new TLSocks5Proxy
                    {
                        CustomFlags = new TLLong(0),
                        Server = new TLString(server),
                        Port = new TLInt(port),
                        Username = new TLString(username),
                        Password = new TLString(password)
                    };

                    ProxyListViewModel.ApplySettings(IoC.Get<ITransportService>().GetProxyConfig(), true, proxy);
                }
            });
        }

        public static void NavigateToMTProtoProxy(string server, int port, string secret)
        {
            if (string.IsNullOrEmpty(server)) return;
            if (port < 0) return;

            Execute.BeginOnUIThread(() =>
            {
                var proxyString = string.Format("server: {0}\nport: {1}", server, port);
                if (!string.IsNullOrEmpty(secret) && !string.IsNullOrEmpty(secret))
                {
                    proxyString += string.Format("\nsecret: {0}", secret);
                }

                var message = string.Format("{0}\n\n{1}\n\n{2}\n\n{3}", AppResources.EnableProxyConfigmation, proxyString, AppResources.ChangeProxyServerHint, AppResources.ProxySponsorWarning);
                var result = MessageBox.Show(message, AppResources.Proxy, MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    var proxy = new TLMTProtoProxy
                    {
                        CustomFlags = new TLLong(0),
                        Server = new TLString(server),
                        Port = new TLInt(port),
                        Secret = new TLString(secret)
                    };

                    ProxyListViewModel.ApplySettings(IoC.Get<ITransportService>().GetProxyConfig(), true, proxy);

                }
            });
        }

        public static void NavigateToHashtag(string hashtag)
        {
            if (string.IsNullOrEmpty(hashtag)) return;

            Execute.BeginOnUIThread(() =>
            {
                IoC.Get<IStateService>().Hashtag = hashtag;
                //IoC.Get<IStateService>().RemoveBackEntries = true;
                //var navigationService = IoC.Get<INavigationService>();
                //navigationService.Navigate(new Uri("/Views/Dialogs/DialogDetailsView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative)); // fix DialogDetailsView -> DialogDetailsView
                IoC.Get<INavigationService>().UriFor<SearchShellViewModel>().Navigate();
            });
        }

        public static void NavigateToUser(TLUserBase userBase, string accessToken, PageKind pageKind = PageKind.Dialog)
        {
            if (userBase == null) return;

            var rootVisual = Application.Current.RootVisual as PhoneApplicationFrame;
            if (rootVisual != null)
            {
                var dialogDetailsView = rootVisual.Content as IDialogDetailsView;
                if (dialogDetailsView != null)
                {
                    dialogDetailsView.CreateBitmapCache(null);
                }
            }

            Execute.BeginOnUIThread(() =>
            {
                
                var navigationService = IoC.Get<INavigationService>();

                var user = userBase as TLUser;
                if (user != null && user.IsBot)
                {
                    pageKind = PageKind.Dialog;
                }

                if (pageKind == PageKind.Profile)
                {
                    IoC.Get<IStateService>().CurrentContact = userBase;
                    //IoC.Get<IStateService>().RemoveBackEntries = true;
                    navigationService.Navigate(new Uri("/Views/Contacts/ContactView.xaml", UriKind.Relative));
                }
                else if (pageKind == PageKind.Search)
                {
                    if (user != null && user.IsBotGroupsBlocked)
                    {
                        MessageBox.Show(AppResources.AddBotToGroupsError, AppResources.Error, MessageBoxButton.OK);
                        return;
                    }

                    IoC.Get<IStateService>().With = userBase;
                    IoC.Get<IStateService>().RemoveBackEntries = true;
                    IoC.Get<IStateService>().AccessToken = accessToken;
                    IoC.Get<IStateService>().Bot = userBase;
                    navigationService.Navigate(new Uri("/Views/Dialogs/ChooseDialogView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
                }
                else
                {
                    IoC.Get<IStateService>().With = userBase;
                    IoC.Get<IStateService>().RemoveBackEntries = true;
                    IoC.Get<IStateService>().AccessToken = accessToken;
                    IoC.Get<IStateService>().Bot = userBase;
                    navigationService.Navigate(new Uri("/Views/Dialogs/DialogDetailsView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
                }
                 // fix DialogDetailsView -> DialogDetailsView
                //IoC.Get<INavigationService>().UriFor<DialogDetailsViewModel>().Navigate();
            });
        }

        public static void NavigateToChat(TLChatBase chatBase, string post)
        {
            if (chatBase == null) return;

            Execute.BeginOnUIThread(() =>
            {
                IoC.Get<IStateService>().With = chatBase;
                IoC.Get<IStateService>().Post = post;
                IoC.Get<IStateService>().RemoveBackEntries = true;
                IoC.Get<INavigationService>().Navigate(new Uri("/Views/Dialogs/DialogDetailsView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
            });
        }

        public static void NavigateToGame(TLUserBase user, string game)
        {
            Execute.BeginOnUIThread(() =>
            {
                IoC.Get<IStateService>().SharedContact = user;
                IoC.Get<IStateService>().GameString = game;
                IoC.Get<INavigationService>().UriFor<ChooseDialogViewModel>().Navigate();
            });
        }

        private static void NavigateToForwarding(string url, string urlText)
        {
            Execute.BeginOnUIThread(() =>
            {
                IoC.Get<IStateService>().Url = url;
                IoC.Get<IStateService>().UrlText = urlText;
                IoC.Get<INavigationService>().UriFor<ChooseDialogViewModel>().Navigate();
            });
        }

        private static void NavigateToShareTarget(string weblink)
        {
            if (string.IsNullOrEmpty(weblink)) return;

            Execute.BeginOnUIThread(() =>
            {
                Execute.ShowDebugMessage(weblink);
                var navigationService = IoC.Get<INavigationService>();
                navigationService.Navigate(new Uri("/Views/Dialogs/ChooseDialogView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
            });
        }

        public static void NavigateToConfirmPhone(TLSentCodeBase sentCode)
        {
            if (sentCode == null) return;

            Execute.BeginOnUIThread(() =>
            {
                IoC.Get<IStateService>().SentCode = sentCode;
                IoC.Get<IStateService>().PhoneCodeHash = sentCode.PhoneCodeHash;
                IoC.Get<IStateService>().PhoneRegistered = sentCode.PhoneRegistered;
                IoC.Get<IStateService>().SendCallTimeout = sentCode.SendCallTimeout;
                var sentCode50 = sentCode as TLSentCode50;
                if (sentCode50 != null)
                {
                    IoC.Get<IStateService>().Type = sentCode50.Type;
                    IoC.Get<IStateService>().NextType = sentCode50.NextType;
                }
                IoC.Get<INavigationService>().UriFor<CancelConfirmResetViewModel>().Navigate();
            });
        }
    }

    public enum PageKind
    {
        Dialog,
        Profile,
        Search
    }
}

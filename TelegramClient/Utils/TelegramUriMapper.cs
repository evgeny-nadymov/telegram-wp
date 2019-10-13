// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Caliburn.Micro;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.Utils
{
    public class TelegramUriMapper : UriMapperBase
    {
        public override Uri MapUri(Uri uri)
        {
#if WP81
            var op = ((App)Application.Current).ShareOperation;
            if (op != null)
            {
                ((App)Application.Current).ShareOperation = null;
                if (op.Data.Contains(StandardDataFormats.WebLink))
                {
                    IoC.Get<IStateService>().WebLink = op.Data.GetWebLinkAsync().GetResults();
                    //IoC.Get<INavigationService>().Navigate(new Uri("/Views/Dialogs/ChooseDialogView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
                }
                else if (op.Data.Contains(StandardDataFormats.StorageItems))
                {
                    IoC.Get<IStateService>().StorageItems = op.Data.GetStorageItemsAsync().GetResults();
                    //IoC.Get<INavigationService>().Navigate(new Uri("/Views/Dialogs/ChooseDialogView.xaml?rndParam=" + TLInt.Random(), UriKind.Relative));
                }
            }
#endif

            var tempUri = HttpUtility.UrlDecode(uri.ToString());          
            if (tempUri.Contains("msg_id") || tempUri.Contains("SecondaryTile"))
            {
                var uriParams = ParseQueryString(tempUri);

                if (tempUri.Contains("from_id"))
                {
                    IoC.Get<IStateService>().RemoveBackEntries = true;
                    IoC.Get<IStateService>().UserId = uriParams["from_id"];
                }
                else if (tempUri.Contains("encryptedchat_id") && tempUri.Contains("encrypteduser_id"))
                {
                    IoC.Get<IStateService>().ChatId = uriParams["encryptedchat_id"];
                    IoC.Get<IStateService>().UserId = uriParams["encrypteduser_id"];
                    return new Uri("/Views/Dialogs/SecretDialogDetailsView.xaml", UriKind.Relative);
                }
                else if (tempUri.Contains("chat_id"))
                {
                    IoC.Get<IStateService>().RemoveBackEntries = true;
                    IoC.Get<IStateService>().ChatId = uriParams["chat_id"];
                }
                else if (tempUri.Contains("channel_id"))
                {
                    IoC.Get<IStateService>().RemoveBackEntries = true;
                    IoC.Get<IStateService>().ChatId = uriParams["channel_id"];
                }
                else if (tempUri.Contains("broadcast_id"))
                {
                    IoC.Get<IStateService>().RemoveBackEntries = true;
                    IoC.Get<IStateService>().BroadcastId = uriParams["broadcast_id"];
                }
                else
                {
                    return uri;
                }

                return new Uri("/Views/Dialogs/DialogDetailsView.xaml", UriKind.Relative);
            }
            
            if (tempUri.StartsWith("/Protocol?encodedLaunchUri"))
            {
                return new Uri("/Views/ShellView.xaml", UriKind.Relative);
            }

            if (tempUri.StartsWith("/PeopleExtension?action"))
            {
                return new Uri("/Views/ShellView.xaml", UriKind.Relative);
            }

            if (tempUri.Contains("Action=ENCRYPTED_MESSAGE"))
            {
                IoC.Get<IStateService>().ClearNavigationStack = true;
                return uri;
            }

            if (tempUri.Contains("phone_call_id"))
            {
                var uriParams = ParseQueryString(tempUri);
                IoC.Get<IStateService>().PhoneCallId = uriParams["phone_call_id"];
                return uri;
            }
            // check ShellView.xaml.cs OnNavigatedTo
            //if (tempUri.Contains("FileId"))
            //{
                //var uriParams = ParseQueryString(tempUri);
                
                //IoC.Get<IStateService>().FileId = uriParams["FileId"];
                //IoC.Get<IStateService>().ClearNavigationStack = true;

                //return new Uri("/Views/ShellView.xaml", UriKind.Relative);
            //}


            // Otherwise perform normal launch.
            return uri;
        }

        public static Dictionary<string, string> ParseQueryString(string uri)
        {

            string substring = uri.Substring(((uri.LastIndexOf('?') == -1) ? 0 : uri.LastIndexOf('?') + 1));

            string[] pairs = substring.Split('&');

            var output = new Dictionary<string, string>();

            foreach (string piece in pairs)
            {
                string[] pair = piece.Split('=');
                if (pair.Length > 1)
                {
                    output.Add(pair[0], pair[1]);
                }
            }

            return output;

        }
    }
}

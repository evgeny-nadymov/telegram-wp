// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
#define INTERACTIVE_NOTIFICATIONS
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
#if WNS_PUSH_SERVICE
using Windows.UI.Notifications;
#endif
using Telegram.Api.Helpers;

namespace PhoneVoIPApp.Agents
{
    public static class PushUtils2
    {

        private static readonly Dictionary<string, string> _locKeys = new Dictionary<string, string>
        {
            {"PINNED_AUDIO", "pinned a voice message"},
            {"PINNED_CONTACT", "pinned a contact"},
            {"PINNED_DOC", "pinned a file"},
            {"PINNED_GAME", "pinned a game"},
            {"PINNED_GEO", "pinned a map"},
            {"PINNED_GIF", "pinned a GIF"},
            {"PINNED_INVOICE", "pinned an invoice"},
            {"PINNED_NOTEXT", "pinned a message"},
            {"PINNED_PHOTO", "pinned a photo"},
            {"PINNED_STICKER", "pinned a sticker"},
            {"PINNED_TEXT", "pinned \"{1}\""},
            {"PINNED_VIDEO", "pinned a video"},

            {"MESSAGE_FWDS", "forwarded you {1} messages"},
            {"MESSAGE_TEXT", "{1}"},
            {"MESSAGE_NOTEXT", "sent you a message"},
            {"MESSAGE_PHOTO", "sent you a photo"},
            {"MESSAGE_VIDEO", "sent you a video"},
            {"MESSAGE_DOC", "sent you a document"},
            {"MESSAGE_GIF", "sent you a GIF"},
            {"MESSAGE_AUDIO", "sent you a voice message"},
            {"MESSAGE_CONTACT", "shared a contact with you"},
            {"MESSAGE_GEO", "sent you a map"},
            {"MESSAGE_STICKER", "sent you a sticker"},
            {"MESSAGE_GAME", "invited you to play {1}"},
            {"MESSAGE_INVOICE", "sent you an invoice for {1}"},

            {"CHAT_MESSAGE_FWDS", "{0} forwarded {2} messages to the group"},
            {"CHAT_MESSAGE_TEXT", "{0}: {2}"},
            {"CHAT_MESSAGE_NOTEXT", "{0} sent a message to the group"},
            {"CHAT_MESSAGE_PHOTO", "{0} sent a photo to the group"},
            {"CHAT_MESSAGE_VIDEO", "{0} sent a video to the group"},
            {"CHAT_MESSAGE_DOC", "{0} sent a document to the group"},
            {"CHAT_MESSAGE_GIF", "{0} sent a GIF to the group"},
            {"CHAT_MESSAGE_AUDIO", "{0} sent a voice message to the group"},
            {"CHAT_MESSAGE_CONTACT", "{0} shared a contact in the group"},
            {"CHAT_MESSAGE_GEO", "{0} sent a map to the group"},
            {"CHAT_MESSAGE_STICKER", "{0} sent a sticker to the group"},
            {"CHAT_MESSAGE_GAME", "{0} invited the group to play {2}"},
            {"CHAT_MESSAGE_INVOICE", "{0} sent an invoice for {2}"},

            {"CHANNEL_MESSAGE_FWDS", "posted {1} forwarded messages"},
            {"CHANNEL_MESSAGE_TEXT", "{1}"},
            {"CHANNEL_MESSAGE_NOTEXT", "posted a message"},
            {"CHANNEL_MESSAGE_PHOTO", "posted a photo"},
            {"CHANNEL_MESSAGE_VIDEO", "posted a video"},
            {"CHANNEL_MESSAGE_DOC", "posted a document"},
            {"CHANNEL_MESSAGE_GIF", "posted a GIF"},
            {"CHANNEL_MESSAGE_AUDIO", "posted a voice message"},
            {"CHANNEL_MESSAGE_CONTACT", "posted a contact"},
            {"CHANNEL_MESSAGE_GEO", "posted a map"},
            {"CHANNEL_MESSAGE_STICKER", "posted a sticker"},
            {"CHANNEL_MESSAGE_GAME", "invited you to play {1}"},

            {"CHAT_CREATED", "{0} invited you to the group"},
            {"CHAT_TITLE_EDITED", "{0} edited the group's name"},
            {"CHAT_PHOTO_EDITED", "{0} edited the group's photo"},
            {"CHAT_ADD_MEMBER", "{0} invited {2} to the group"},
            {"CHAT_ADD_YOU", "{0} invited you to the group"},
            {"CHAT_DELETE_MEMBER", "{0} kicked {2} from the group"},
            {"CHAT_DELETE_YOU", "{0} kicked you from the group"},
            {"CHAT_LEFT", "{0} has left the group"},
            {"CHAT_RETURNED", "{0} has returned to the group"},
            {"GEOCHAT_CHECKIN", "{0} has checked-in"},
            {"CHAT_JOINED", "{0} has joined the group"},

            {"CONTACT_JOINED", "{0} joined the App!"},
            {"AUTH_UNKNOWN", "New login from unrecognized device {0}"},
            {"AUTH_REGION", "New login from unrecognized device {0}, location: {1}"},

            {"CONTACT_PHOTO", "updated profile photo"},

            {"ENCRYPTION_REQUEST", "You have a new message"},
            {"ENCRYPTION_ACCEPT", "You have a new message"},
            {"ENCRYPTED_MESSAGE", "You have a new message"},

            {"DC_UPDATE", "Open this notification to update app settings"},

            {"LOCKED_MESSAGE", "You have a new message"}
        };

        private static void AppendTile(XmlDocument toTile, XmlDocument fromTile)
        {
            var fromTileNode = toTile.ImportNode(fromTile.GetElementsByTagName("binding").Item(0), true);
            toTile.GetElementsByTagName("visual")[0].AppendChild(fromTileNode);
        }

        private static void UpdateTile(string caption, string message)
        {
#if WNS_PUSH_SERVICE
            var tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication("xcee0f789y8059y4881y8883y347265c01f93x");
            //tileUpdater.EnableNotificationQueue(false);
            tileUpdater.EnableNotificationQueue(true);
            tileUpdater.EnableNotificationQueueForSquare150x150(false);
            //tileUpdater.EnableNotificationQueueForWide310x150(true);

            var wideTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150IconWithBadgeAndText);
            SetImage(wideTileXml, "IconicSmall110.png");
            SetText(wideTileXml, caption, message);

            var squareTile150Xml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150IconWithBadge);
            SetImage(squareTile150Xml, "IconicTileMedium202.png");
            AppendTile(wideTileXml, squareTile150Xml);

            var squareTile71Xml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare71x71IconWithBadge);
            SetImage(squareTile71Xml, "IconicSmall110.png");
            AppendTile(wideTileXml, squareTile71Xml);

            try
            {
                tileUpdater.Update(new TileNotification(wideTileXml));
            }
            catch (Exception ex)
            {
                Telegram.Logs.Log.Write(ex.ToString());
            }
#endif
        }

        private static void UpdateBadge(int badgeNumber)
        {
#if WNS_PUSH_SERVICE
            var badgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication("xcee0f789y8059y4881y8883y347265c01f93x");
            if (badgeNumber == 0)
            {
                badgeUpdater.Clear();
                return;
            }

            var badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);

            var badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", badgeNumber.ToString());

            try
            {
                badgeUpdater.Update(new BadgeNotification(badgeXml));
            }
            catch (Exception ex)
            {
                Telegram.Logs.Log.Write(ex.ToString());
            }
#endif
        }

        private static bool IsMuted(Data data)
        {
            return data.mute == "1";
        }

        private static bool IsServiceNotification(Data data)
        {
            return data.loc_key == "DC_UPDATE";
        }

        private static void RemoveToastGroup(string groupname)
        {
#if WNS_PUSH_SERVICE
            ToastNotificationManager.History.RemoveGroup(groupname);
#endif
        }

        private static string GetCaption(Data data)
        {
            var locKey = data.loc_key;
            if (locKey == null)
            {
                return "locKey=null";
            }

            if (locKey.StartsWith("CHAT") || locKey.StartsWith("GEOCHAT"))
            {
                return data.loc_args[1];
            }

            if (locKey.StartsWith("MESSAGE"))
            {
                return data.loc_args[0];
            }

            if (locKey.StartsWith("CHANNEL"))
            {
                return data.loc_args[0];
            }

            if (locKey.StartsWith("PINNED"))
            {
                return data.loc_args[0];
            }

            if (locKey.StartsWith("AUTH")
                || locKey.StartsWith("CONTACT")
                || locKey.StartsWith("ENCRYPTED")
                || locKey.StartsWith("ENCRYPTION")
                || locKey.StartsWith("PHONE"))
            {
                return "Telegram";
            }

#if DEBUG
            return locKey;
#else
            return "Telegram";
#endif
        }

        private static string GetSound(Data data)
        {
            return data.sound;
        }

        private static string GetGroup(Data data)
        {
            return data.group;
        }

        private static string GetTag(Data data)
        {
            return data.tag;
        }

        private static string GetLaunch(Data data)
        {
            var locKey = data.loc_key;
            if (locKey == null) return null;

            var path = "/Views/ShellView.xaml";
            if (locKey == "DC_UPDATE")
            {
                path = "/Views/Additional/SettingsView.xaml";
            }

            var customParams = new List<string> { "Action=" + locKey };
            if (data.custom != null)
            {
                customParams.AddRange(data.custom.GetParams());
            }

            return string.Format("{0}?{1}", path, string.Join("&", customParams));
        }

        private static string GetMessage(Data data)
        {
            var locKey = data.loc_key;
            if (locKey == null)
            {
                Telegram.Logs.Log.Write("::PushNotificationsBackgroundTask locKey=null text=" + data.text);
                return string.Empty;
            }

            string locValue = "";
            if (_locKeys.TryGetValue(locKey, out locValue))
            {
                
            }
            //var resourceLoader = ResourceLoader.GetForViewIndependentUse("TelegramClient.Tasks/Resources");
            //locValue = resourceLoader.GetString(locKey);

            if (locValue != "")
            {
                return string.Format(locValue, data.loc_args).Replace("\r\n", "\n").Replace("\n", " ");
            }
            var builder = new StringBuilder();
            if (data.loc_args != null)
            {
                builder.AppendLine("loc_args");
                foreach (var locArg in data.loc_args)
                {
                    builder.AppendLine(locArg);
                }
            }
            Telegram.Logs.Log.Write(string.Format("::PushNotificationsBackgroundTask missing locKey={0} locArgs={1}", locKey, builder.ToString()));

            //if (locKey.StartsWith("CHAT") || locKey.StartsWith("GEOCHAT"))
            //{
            //    return data.text;
            //}

            //if (locKey.StartsWith("MESSAGE"))
            //{
            //    if (locKey == "MESSAGE_TEXT")
            //    {
            //        return data.loc_args[1];
            //    }

            //    return data.text; //add localization string here 
            //}

#if DEBUG
            return data.text;
#else
            return string.Empty;
#endif
        }

        public static void UpdateToastAndTiles(RootObject rootObject)
        {
            if (rootObject == null) return;
            if (rootObject.data == null) return;

            if (rootObject.data.loc_key == null)
            {
                var groupname = GetGroup(rootObject.data);
                RemoveToastGroup(groupname);
                return;
            }

            var caption = GetCaption(rootObject.data);
            var message = GetMessage(rootObject.data);
            var sound = GetSound(rootObject.data);
            var launch = GetLaunch(rootObject.data);
            var tag = GetTag(rootObject.data);
            var group = GetGroup(rootObject.data);

            if (!IsMuted(rootObject.data) && !Notifications.IsDisabled)
            {
                AddToast(rootObject, caption, message, sound, launch, tag, group);
            }
            if (!IsServiceNotification(rootObject.data))
            {
                UpdateTile(caption, message);
            }
            UpdateBadge(rootObject.data.badge);
        }

        private static void SetToastImage(XmlDocument document, string imageSource, bool isUserPlaceholder)
        {
            var imageElements = document.GetElementsByTagName("image");
            if (imageSource == null)
            {
                ((XmlElement)imageElements[0]).SetAttribute("src", isUserPlaceholder ? "ms-appx:///Images/W10M/user_placeholder.png" : "ms-appx:///Images/W10M/group_placeholder.png");
            }
            else
            {
                ((XmlElement)imageElements[0]).SetAttribute("src", "ms-appdata:///local/" + imageSource);
            }
        }

        private static void SetImage(XmlDocument document, string imageSource)
        {
            var imageElements = document.GetElementsByTagName("image");
            ((XmlElement)imageElements[0]).SetAttribute("src", imageSource);
        }

        private static void SetSound(XmlDocument document, string soundSource)
        {
            //return;

            if (!Regex.IsMatch(soundSource, @"^sound[1-6]$", RegexOptions.IgnoreCase))
            {
                return;
            }

            var toastNode = document.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");
            var audioElement = document.CreateElement("audio");
            audioElement.SetAttribute("src", "ms-appx:///Sounds/" + soundSource + ".wav");
            audioElement.SetAttribute("loop", "false");

            toastNode.AppendChild(audioElement);
        }

        private static void SetLaunch(RootObject rootObject, XmlDocument document, string launch)
        {
            if (string.IsNullOrEmpty(launch))
            {
                return;
            }
            if (rootObject != null
                && rootObject.data != null
                && rootObject.data.system != null
                && rootObject.data.system.StartsWith("10")) //10.0.10572.0 or less
            {
                try
                {
                    var currentVersion = new Version(rootObject.data.system);
                    var minVersion = new Version("10.0.10572.0");
                    if (currentVersion < minVersion)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Telegram.Logs.Log.Write(ex.ToString());
                }
            }
            //launch = "/Views/ShellView.xaml";
            var toastNode = document.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("launch", launch);
        }

        private static void SetText(XmlDocument document, string caption, string message)
        {
            var toastTextElements = document.GetElementsByTagName("text");
            toastTextElements[0].InnerText = caption ?? string.Empty;
            toastTextElements[1].InnerText = message ?? string.Empty;
        }

        private static string GetArguments(string peer, string peerId, bool needAccessHash, Custom custom)
        {
            string arguments = null;

            if (custom.mtpeer != null && custom.mtpeer.ah != null || !needAccessHash)
            {
                arguments = string.Format("{0}={1}", peer, peerId);

                if (custom.mtpeer != null && custom.mtpeer.ah != null)
                {
                    arguments += string.Format(" access_hash={0}", custom.mtpeer.ah);
                }

                if (custom.msg_id != null)
                {
                    arguments += string.Format(" msg_id={0}", custom.msg_id);
                }
            }

            return arguments;
        }

        private static void GetArgumentsAndImageSource(RootObject rootObject, out string arguments, out string imageSource)
        {
            arguments = null;
            imageSource = null;

            if (rootObject != null)
            {
                var data = rootObject.data;
                if (data != null)
                {
                    var custom = data.custom;
                    if (custom != null)
                    {
                        if (custom.from_id != null)
                        {
                            int fromId;
                            if (Int32.TryParse(custom.from_id, out fromId))
                            {
                                arguments = GetArguments("from_id", custom.from_id, true, custom);
                            }
                        }
                        else if (custom.chat_id != null)
                        {
                            int chatId;
                            if (Int32.TryParse(custom.chat_id, out chatId))
                            {
                                arguments = GetArguments("chat_id", custom.chat_id, false, custom);
                            }
                        }
                        else if (custom.channel_id != null)
                        {
                            int channelId;
                            if (Int32.TryParse(custom.channel_id, out channelId))
                            {
                                if (data.loc_key != null
                                    && data.loc_key.StartsWith("CHAT"))
                                {
                                    arguments = GetArguments("channel_id", custom.channel_id, true, custom);
                                }
                            }
                        }

                        imageSource = GetImageSource(custom);
                    }
                }
            }
        }

        private static void SetActions(XmlDocument document, string arguments)
        {
            if (arguments == null) return;

            //var resourceLoader = ResourceLoader.GetForViewIndependentUse("TelegramClient.Tasks/Resources");

            //"<actions>" +
            //"<input id=\"message\" type=\"text\" placeHolderContent=\"Type a reply\" />" +
            //"<action activationType=\"background\" content=\"Reply\" arguments=\"{0}\" hint-inputId=\"message\" imageUri=\"Assets/Icons/send.png\"/>" +
            //"</actions>"

            var toastNode = document.SelectSingleNode("/toast");
            var actionsElement = document.CreateElement("actions");

            var inputElement = document.CreateElement("input");
            inputElement.SetAttribute("id", "message");
            inputElement.SetAttribute("type", "text");
            inputElement.SetAttribute("placeHolderContent", "Type reply"); //resourceLoader.GetString("TypeReply"));
            actionsElement.AppendChild(inputElement);

            var replyAction = document.CreateElement("action");
            replyAction.SetAttribute("activationType", "background");
            replyAction.SetAttribute("content", "Reply"); //resourceLoader.GetString("Reply"));
            replyAction.SetAttribute("arguments", "action=reply " + arguments);
            replyAction.SetAttribute("hint-inputId", "message");
            replyAction.SetAttribute("imageUri", "Images/W10M/ic_send_2x.png");
            actionsElement.AppendChild(replyAction);

            var muteAction = document.CreateElement("action");
            muteAction.SetAttribute("activationType", "background");
            muteAction.SetAttribute("content", "Mute 1 hour"); //resourceLoader.GetString("Mute1Hour"));
            muteAction.SetAttribute("arguments", "action=mute " + arguments);
            actionsElement.AppendChild(muteAction);

            var disableAction = document.CreateElement("action");
            disableAction.SetAttribute("activationType", "background");
            disableAction.SetAttribute("content", "Disable"); //resourceLoader.GetString("Disable"));
            disableAction.SetAttribute("arguments", "action=disable " + arguments);
            actionsElement.AppendChild(disableAction);

            toastNode.AppendChild(actionsElement);
        }

        public static void AddToast(RootObject rootObject, string caption, string message, string sound, string launch, string tag, string group)
        {
#if WNS_PUSH_SERVICE

#if INTERACTIVE_NOTIFICATIONS

            var toastNotifier = ToastNotificationManager.CreateToastNotifier("xcee0f789y8059y4881y8883y347265c01f93x"); //("xcee0f789y8059y4881y8883y347265c01f93x");
            //toastNotifier.Setting = 
            Version version = Environment.OSVersion.Version;
            if (rootObject.data.system != null && rootObject.data.system != null)
            {
                Version.TryParse(rootObject.data.system, out version);
            }
            var toastXml = new XmlDocument();
            if (version != null && version.Major >= 10)
            {
                string arguments;
                string imageSource;
                GetArgumentsAndImageSource(rootObject, out arguments, out imageSource);

                var xml = 
                    "<toast>" +
                    "<visual>" +
                    "<binding template=\"ToastImageAndText02\">" +
                    "<text id=\"1\"></text>" +
                    "<text id=\"2\"></text>" +
                    "<image id=\"1\" placement=\"appLogoOverride\" src=\"\" hint-crop=\"circle\" />" +
                    "</binding>" +
                    "</visual>" +
                    "</toast>";

                toastXml.LoadXml(xml);
                SetToastImage(toastXml, imageSource, arguments != null && arguments.StartsWith("from_id"));
                SetActions(toastXml, arguments);
            }
            else
            {
                toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            }

            SetText(toastXml, caption, message);
            SetLaunch(rootObject, toastXml, launch);

            if (!string.IsNullOrEmpty(sound) 
                && !string.Equals(sound, "default", StringComparison.OrdinalIgnoreCase))
            {
                SetSound(toastXml, sound);
            }

            try
            {
                var toast = new ToastNotification(toastXml);
                if (tag != null) toast.Tag = tag;
                if (group != null) toast.Group = group;
                //RemoveToastGroup(group);
                toastNotifier.Show(toast);
            }
            catch (Exception ex)
            {
                Telegram.Logs.Log.Write(ex.ToString());
            }
#else
            var toastNotifier = ToastNotificationManager.CreateToastNotifier();

            var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            SetText(toastXml, caption, message);
            SetLaunch(toastXml, launch);

            if (!string.IsNullOrEmpty(sound)
                && !string.Equals(sound, "default", StringComparison.OrdinalIgnoreCase))
            {
                SetSound(toastXml, sound);
            }

            try
            {
                var toast = new ToastNotification(toastXml);
                if (tag != null) toast.Tag = tag;
                if (group != null) toast.Group = group;
                //RemoveToastGroup(group);
                toastNotifier.Show(toast);
            }
            catch (Exception ex)
            {
                Telegram.Logs.Log.Write(ex.ToString());
            }
#endif

#endif
        }

        public static T GetRootObject<T>(string payload) where T : class
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            T rootObject;
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(payload)))
            {
                rootObject = serializer.ReadObject(stream) as T;
            }

            return rootObject;
        }

        private static async Task<bool> IsFileExists(string fileName)
        {
            bool fileExists = true;
            Stream fileStream = null;
            StorageFile file = null;

            try
            {
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                fileStream = await file.OpenStreamForReadAsync();
                fileStream.Dispose();
            }
            catch (FileNotFoundException)
            {
                // If the file dosn't exits it throws an exception, make fileExists false in this case 
                fileExists = false;
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
            }

            return fileExists;
        }

        private static string GetImageSource(Custom custom)
        {
            string imageSource = null;
            if (custom.mtpeer != null)
            {
                var location = custom.mtpeer.ph;
                if (location != null)
                {
                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        location.volume_id,
                        location.local_id,
                        location.secret);

                    if (IsFileExists(fileName).Result)
                    {
                        imageSource = fileName;
                    }
                }
            }

            return imageSource;
        }

        public static string GetImageSource(MTPeer mtpeer)
        {
            string imageSource = null;
            if (mtpeer != null)
            {
                var location = mtpeer.ph;
                if (location != null)
                {
                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        location.volume_id,
                        location.local_id,
                        location.secret);

                    if (IsFileExists(fileName).Result)
                    {
                        imageSource = fileName;
                    }
                }
            }

            return imageSource;
        }
    }

    public sealed class Photo
    {
        public string volume_id { get; set; }
        public string local_id { get; set; }
        public string secret { get; set; }
        public int dc_id { get; set; }
    }

    public sealed class MTPeer
    {
        public string ah { get; set; }
        public Photo ph { get; set; }
    }

    public sealed class Custom
    {
        public string msg_id { get; set; }
        public string from_id { get; set; }
        public string chat_id { get; set; }
        public string channel_id { get; set; }
        public MTPeer mtpeer { get; set; }
        public string call_id { get; set; }
        public string call_ah { get; set; }

        public string group
        {
            get
            {
                if (chat_id != null) return "c" + chat_id;
                if (channel_id != null) return "c" + chat_id;
                if (from_id != null) return "u" + from_id;
                return null;
            }
        }

        public string tag { get { return msg_id; } }

        public IEnumerable<string> GetParams()
        {
            if (msg_id != null) yield return "msg_id=" + msg_id;
            if (from_id != null) yield return "from_id=" + from_id;
            if (chat_id != null) yield return "chat_id=" + chat_id;
            if (channel_id != null) yield return "channel_id=" + channel_id;
        }
    }

    public sealed class Data
    {
        public Custom custom { get; set; }
        public string sound { get; set; }
        public string mute { get; set; }
        public int badge { get; set; }
        public string loc_key { get; set; }
        public string[] loc_args { get; set; }
        public int random_id { get; set; }
        public int user_id { get; set; }
        public string text { get; set; }
        public string system { get; set; }

        public string group { get { return custom != null ? custom.group : null; } }
        public string tag { get { return custom != null ? custom.tag : null; } }
    }

    public sealed class RootObject
    {
        public int date { get; set; }
        public Data data { get; set; }
    }
}

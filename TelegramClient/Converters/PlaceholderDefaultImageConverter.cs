// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Telegram.Api;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.ViewModels;

namespace TelegramClient.Converters
{
    public class PlaceholderDefaultImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var user = value as TLUserBase;
            if (user != null && user.Index == 333000)
            {
#if WP81
                return new Uri("/ApplicationIcon106.png", UriKind.Relative);
#elif WP8
                return new Uri("/ApplicationIcon210.png", UriKind.Relative);
#endif

                return new Uri("/ApplicationIcon99.png", UriKind.Relative);
            }

            if (value is TLBroadcastChat)
            {
                return new Uri("/Images/Placeholder/placeholder.broadcast.png", UriKind.Relative);
            }

            return value is TLChatBase
                ? new Uri("/Images/Placeholder/placeholder.group.transparent-WXGA.png", UriKind.Relative)
                : new Uri("/Images/Placeholder/placeholder.user.transparent-WXGA.png", UriKind.Relative);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PlaceholderDefaultTextConverter : IValueConverter
    {
        public static string GetText(TLObject value)
        {
            if (value == null) return null;

            var word1 = string.Empty;
            var word2 = string.Empty;

            var user = value as TLUserBase;
            if (user != null)
            {
                word1 = user.FirstName != null ? user.FirstName.ToString() : string.Empty;
                word2 = user.LastName != null ? user.LastName.ToString() : string.Empty;

                if (word1.StartsWith("+") && string.IsNullOrEmpty(word2))
                {
                    word2 = word1.Substring(1).Trim();
                }
                else if (word2.StartsWith("+") && string.IsNullOrEmpty(word1))
                {
                    word1 = word2.Substring(1).Trim();
                }
            }

            var broadcast = value as TLBroadcastChat;
            if (broadcast != null)
            {
                var words = broadcast.FullName.Trim().Split(' ');

                if (words.Length > 0)
                {
                    if (words.Length == 1)
                    {
                        var si = StringInfo.GetTextElementEnumerator(broadcast.FullName ?? string.Empty);

                        word1 = si.MoveNext() ? si.GetTextElement() : string.Empty;
                        word2 = si.MoveNext() ? si.GetTextElement() : string.Empty;
                    }
                    else
                    {
                        word1 = words[0];
                        word2 = words[words.Length - 1];
                    }
                }
            }

            var chat = value as TLChatBase;
            if (chat != null)
            {
                var words = chat.FullName.Trim().Split(' ');

                if (words.Length > 0)
                {
                    if (words.Length == 1)
                    {
                        var si = StringInfo.GetTextElementEnumerator(chat.FullName ?? string.Empty);

                        word1 = si.MoveNext() ? si.GetTextElement() : string.Empty;
                        word2 = si.MoveNext() ? si.GetTextElement() : string.Empty;
                    }
                    else
                    {
                        word1 = words[0];
                        word2 = words[words.Length - 1];
                    }
                }
            }

            var si1 = StringInfo.GetTextElementEnumerator(word1 ?? string.Empty);
            var si2 = StringInfo.GetTextElementEnumerator(word2 ?? string.Empty);

            word1 = si1.MoveNext() ? si1.GetTextElement() : string.Empty;
            word2 = si2.MoveNext() ? si2.GetTextElement() : string.Empty;

            return string.Format("{0}{1}", word1, word2).Trim().ToUpperInvariant();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //System.Diagnostics.Debug.WriteLine("PlaceholderDefaultTextConverter elapsed=" + ShellViewModel.Timer.Elapsed);
            return GetText(value as TLObject);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class LinkDefaultTextConverter : IValueConverter
    {   
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var firstLetter = string.Empty;

            var message = value as TLMessage;
            if (message == null) return null;

            var links = message.Links;
            if (links != null && links.Count > 0)
            {
                firstLetter = GetFirstUrlLetter(links[0]);
            }
            else
            {
                var mediaWebPage = message.Media as TLMessageMediaWebPage;
                if (mediaWebPage != null)
                {
                    var webPage = mediaWebPage.WebPage as TLWebPage;
                    if (webPage != null)
                    {
                        if (!TLString.IsNullOrEmpty(webPage.DisplayUrl))
                        {
                            firstLetter = GetFirstUrlLetter(webPage.DisplayUrl.ToString());
                        }
                    }
                }
            }

            return firstLetter;
        }

        public static string GetFirstUrlLetter(string url)
        {
            url = url.Replace("http://", string.Empty);
            url = url.Replace("https://", string.Empty);
            url = url.Replace("www.", string.Empty);

            return GetFirstLetter(url);
        }

        public static string GetFirstLetter(string url)
        {
            var si = StringInfo.GetTextElementEnumerator(url);
            var word1 = si.MoveNext() ? si.GetTextElement() : string.Empty;

            return word1.Trim().ToUpperInvariant();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InlineResultDefaultTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            var firstLetter = string.Empty;

            var botInlineMediaResult = value as TLBotInlineMediaResult;
            if (botInlineMediaResult != null)
            {
                var title = botInlineMediaResult.Title != null ? botInlineMediaResult.Title.ToString() : null;
                if (!string.IsNullOrEmpty(title)) return LinkDefaultTextConverter.GetFirstLetter(title);

                var description = botInlineMediaResult.Description != null? botInlineMediaResult.Description.ToString() : null;
                if (!string.IsNullOrEmpty(description)) return LinkDefaultTextConverter.GetFirstLetter(description);

                return null;
            }

            var botInlineResult = value as TLBotInlineResult;
            if (botInlineResult != null)
            {
                var contentUrl = botInlineResult.ContentUrl != null ? botInlineResult.ContentUrl.ToString() : null;
                if (!string.IsNullOrEmpty(contentUrl)) return LinkDefaultTextConverter.GetFirstUrlLetter(contentUrl);
                
                var title = botInlineResult.Title != null ? botInlineResult.Title.ToString() : null;
                if (!string.IsNullOrEmpty(title)) return LinkDefaultTextConverter.GetFirstLetter(title);

                var description = botInlineResult.Description != null ? botInlineResult.Description.ToString() : null;
                if (!string.IsNullOrEmpty(description)) return LinkDefaultTextConverter.GetFirstLetter(description);
            }

            return firstLetter;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Globalization;
using System.Windows.Data;
using Caliburn.Micro;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.Converters
{
    public class DialogMessageFromConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dialog = value as TLDialog;

            if (dialog != null)
            {
                var dialog53 = dialog as TLDialog53;
                if (dialog53 != null)
                {
                    var draft = dialog53.Draft as TLDraftMessage;
                    if (draft != null && !TLString.IsNullOrEmpty(draft.Message))
                    {
                        var topMessage = dialog53.TopMessage;
                        if (topMessage != null && topMessage.DateIndex < draft.Date.Value)
                        {
                            return string.Empty; //AppResources.Draft + ": ";
                        }
                    }
                }

                var messageBase = dialog.TopMessage;
                if (messageBase != null && messageBase.ShowFrom)
                {
                    var messageCommon = messageBase as TLMessageCommon;
                    if (messageCommon != null)
                    {
                        var user = messageCommon.From as TLUserBase;
                        if (user != null)
                        {
                            var currentUserId = IoC.Get<IStateService>().CurrentUserId;
                            if (currentUserId == user.Index)
                            {
                                return AppResources.You + ": ";
                            }

                            var firstName = user.FirstName != null ? user.FirstName.ToString().Trim() : string.Empty;
                            var lastName = user.LastName != null ? user.LastName.ToString().Trim() : string.Empty;

                            if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                            {
                                return (user.Phone != null ? "+" + user.Phone : string.Empty) + ": ";
                            }

                            if (string.Equals(firstName, lastName, StringComparison.OrdinalIgnoreCase))
                            {
                                return firstName + ": ";
                            }

                            if (string.IsNullOrEmpty(firstName))
                            {
                                return lastName + ": ";
                            }

                            if (string.IsNullOrEmpty(lastName))
                            {
                                return firstName + ": ";
                            }

                            return firstName + ": ";
                        }

                        var peerChannel = messageCommon.ToId as TLPeerChannel;
                        if (peerChannel != null
                            && (messageCommon.FromId == null || messageCommon.FromId.Value == -1))
                        {
                            var channel = IoC.Get<ICacheService>().GetChat(peerChannel.Id) as TLChannel;

                            if (channel != null)
                            {
                                return channel.FullName + ": ";
                            }

                            var channelForbidden = IoC.Get<ICacheService>().GetChat(peerChannel.Id) as TLChannelForbidden;
                            if (channelForbidden != null)
                            {
                                return channelForbidden.FullName + ": ";
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

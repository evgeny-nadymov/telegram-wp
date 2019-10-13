// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using TelegramClient.Resources;

namespace TelegramClient.ViewModels.Contacts
{
    public class ShareContactDetailsViewModel : PropertyChangedBase
    {
        public Visibility ShareVisibility
        {
            get { return Mode == ShareContactDetailsMode.Share ? Visibility.Visible : Visibility.Collapsed; }
        }

        public string ActionString
        {
            get { return Mode == ShareContactDetailsMode.Share ? AppResources.ShareContact : AppResources.AddContact; }
        }

        public TLUserBase CurrentItem { get; set; }

        public ObservableCollection<TLObject> Items { get; set; }

        public ShareContactDetailsViewModel(TLUserBase contact)
        {
            Items = new ObservableCollection<TLObject>();
            SetContact(contact);
        }

        public void ForwardInAnimationBegin()
        {

        }

        public void ForwardInAnimationComplete()
        {
            
        }

        public Action<TLMessageMediaContact> Callback { get; set; }

        public ShareContactDetailsMode Mode { get; set; }

        public bool IsSharingEnabled { get { return Items.OfType<TLUserPhone>().Any(x => x.IsSelected); } }

        public void Send()
        {
            if (CurrentItem == null) return;

            var userPhone = Items.OfType<TLUserPhone>().FirstOrDefault(x => x.IsSelected);
            if (userPhone == null) return;

            var media = new TLMessageMediaContact82
            {
                UserId = CurrentItem.Id,
                FirstName = CurrentItem.FirstName,
                LastName = CurrentItem.LastName,
                PhoneNumber = userPhone.Number,
                VCard = ToVCard(CurrentItem)
            };

            Callback.SafeInvoke(media);
        }

        private TLString ToVCard(TLUserBase currentItem)
        {
            return TLString.Empty;
        }

        public void Call(TLUserPhone userPhone)
        {
            if (Mode != ShareContactDetailsMode.View) return;
            if (userPhone == null) return;

            var phoneNumber = userPhone.Number.ToString();
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                if (!phoneNumber.StartsWith("+"))
                {
                    phoneNumber = "+" + phoneNumber;
                }

                var task = new PhoneCallTask();
                task.DisplayName = CurrentItem.FullName2;
                task.PhoneNumber = phoneNumber;
                task.Show();
            }
        }

        public void SetContact(TLUserBase userBase)
        {
            CurrentItem = userBase;
            NotifyOfPropertyChange(() => CurrentItem);

            Items.Clear();
            var userNotRegistered = userBase as TLUserNotRegistered;
            if (userNotRegistered != null)
            {
                if (userNotRegistered.Phones != null)
                {
                    foreach (var phone in userNotRegistered.Phones)
                    {
                        Items.Add(phone);
                    }
                }
            }

            if (Items.Count == 0)
            {
                Items.Add(new TLUserPhone
                {
                    Kind = new TLInt(1),
                    Number = userBase.Phone,
                    Description = TLString.Empty
                }); 
            }


            for (var index = 0; index < Items.Count; index++)
            {
                var userPhone = Items[index] as TLUserPhone;
                if (userPhone != null)
                {
                    userPhone.IsSelected = index == 0;
                    userPhone.IsIconVisible = index == 0;
                }
            }
        }
    }
}

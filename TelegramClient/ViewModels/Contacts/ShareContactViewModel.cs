// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.Contacts;
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Models;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.Views.Contacts;
using TelegramClient.Views.Search;

namespace TelegramClient.ViewModels.Contacts
{
    public class ShareContactViewModel : ItemsViewModelBase<TLUserBase>
    {
        private ObservableCollection<AlphaKeyGroup<TLUserBase>> _contacts;

        public ObservableCollection<AlphaKeyGroup<TLUserBase>> Contacts
        {
            get { return _contacts; }
            set { SetField(ref _contacts, value, () => Contacts); }
        }

        private const int FirstSliceLength = 10;

        public TLMessageMediaContact PhoneContact { get; set; }

        public IList<TLUserBase> Source { get; set; }

        public ShareContactViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Source = new List<TLUserBase>();
            Items = new AlphaKeyGroup<TLUserBase>("@");
            _contacts = new ObservableCollection<AlphaKeyGroup<TLUserBase>> { (AlphaKeyGroup<TLUserBase>)Items };

            PhoneContact = StateService.PhoneContact;
            StateService.PhoneContact = null;

            if (PhoneContact != null)
            {

            }
            else
            {
                Status = AppResources.Loading;
                BeginOnThreadPool(() =>
                    CacheService.GetContactsAsync(
                    contacts =>
                    {
                        var currentUser = contacts.FirstOrDefault(x => x.Index == StateService.CurrentUserId);
                        if (currentUser == null)
                        {
                            currentUser = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
                            if (currentUser != null)
                            {
                                contacts.Add(currentUser);
                            }
                        }

                        foreach (var contact in contacts)
                        {
                            Source.Add(contact);
                        }

                        foreach (var contact in contacts)
                        {
                            LazyItems.Add(contact);
                        }

                        BeginOnUIThread(() =>
                        {
                            var importantCount = 0;
                            var count = 0;
                            for (var i = 0; i < LazyItems.Count && importantCount < FirstSliceLength; i++, count++)
                            {
                                Items.Add(LazyItems[i]);
                                importantCount++;
                            }
                            Status = Items.Count == 0 ? string.Format("{0}", AppResources.NoUsersHere) : string.Empty;

                            BeginOnUIThread(TimeSpan.FromSeconds(0.5), () =>
                            {
                                for (var i = count; i < LazyItems.Count; i++)
                                {
                                    Items.Add(LazyItems[i]);
                                }
                                LazyItems.Clear();

                                GetContactsAsync();
                            });
                        });
                    }));
            }
        }

        public Action<TLMessageMediaContact> ContinueAction { get; set; }

        protected override void OnActivate()
        {
            if (StateService.RemoveBackEntry)
            {
                StateService.RemoveBackEntry = false;
                NavigationService.RemoveBackEntry();
            }

            base.OnActivate();
        }

        #region Action
        public void UserAction(TLUserBase user)
        {
            if (user == null) return;

            var view = GetView() as ShareContactView;
            if (view != null)
            {
                view.OpenContactDetails(user, ShareContactDetailsMode.Share);
            }
        }

        public void Save()
        {
            if (PhoneContact == null) return;
            if (PhoneContact.User == null) return;

            var phoneNumber = PhoneContact.User.Phone.ToString();
            if (!string.IsNullOrEmpty(phoneNumber))
            {
                if (!PhoneContact.User.Phone.ToString().StartsWith("+"))
                {
                    phoneNumber = "+" + phoneNumber;
                }

                var task = new SaveContactTask();
                task.FirstName = PhoneContact.User.FirstName.ToString();
                task.LastName = PhoneContact.User.LastName.ToString();
                task.MobilePhone = phoneNumber;
                task.Show();
            }
        }

        public void UserActionContinue(TLMessageMediaContact media)
        {
            if (PhoneContact != null)
            {
                CloseEditor(true);
                Save();
            }
            else
            {
                CloseEditor(true);
                ContinueAction.SafeInvoke(media);
            }
        }

        public void ViewAction()
        {
            if (PhoneContact == null) return;
            if (PhoneContact.User == null) return;

            var view = GetView() as ShareContactView;
            if (view != null)
            {
                view.OpenContactDetails(PhoneContact.User, ShareContactDetailsMode.View);
            }
        }

        public async void GetContactsAsync()
        {
            var contactStore = await ContactManager.RequestStoreAsync();
            if (contactStore == null)
            {
                IsWorking = false;
                Status = Items.Count == 0 && LazyItems.Count == 0
                    ? AppResources.NoContactsHere
                    : string.Empty;

                return;
            }

            var contacts = await contactStore.FindContactsAsync();
            var notRegisteredContacts = new List<TLUserBase>();
            foreach (var contact in contacts)
            {
                var notRegisteredUser = ContactsViewModel.GetNotRegisteredUser(contact);
                if (notRegisteredUser != null)
                {
                    notRegisteredContacts.Add(notRegisteredUser);
                }
            }

            foreach (var notRegisteredContact in notRegisteredContacts)
            {
                Source.Add(notRegisteredContact);
            }

            var groups = AlphaKeyGroup<TLUserBase>.CreateGroups(
                notRegisteredContacts,
                Thread.CurrentThread.CurrentUICulture,
                x => x.FullName,
                false);

            var contactKeys = new Dictionary<string, string>();
            foreach (var contact in Contacts)
            {
                contactKeys[contact.Key] = contact.Key;
            }
            BeginOnThreadPool(() =>
            {
                foreach (var group in groups)
                {
                    var gr = new AlphaKeyGroup<TLUserBase>(group.Key);
                    foreach (var u in group.OrderBy(x => x.FullName))
                    {
                        gr.Add(u);
                    }

                    if (!contactKeys.ContainsKey(gr.Key))
                    {
                        BeginOnUIThread(() => Contacts.Add(gr));
                    }
                }

                BeginOnUIThread(() =>
                {
                    Status = Items.Count == 0 && LazyItems.Count == 0
                        ? AppResources.NoContactsHere
                        : string.Empty;
                });
            });
        }
        #endregion

        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            protected set
            {
                if (_isOpen != value)
                {
                    _isOpen = value;
                    NotifyOfPropertyChange(() => IsOpen);
                }
            }
        }

        public void OpenEditor()
        {
            IsOpen = true;
        }

        public void CloseEditor(bool force = false)
        {
            var view = GetView() as ShareContactView;
            if (view != null && !force)
            {
                var searchView = view.SearchPlaceholder.Content as SearchSharedContactsView;
                if (searchView != null)
                {
                    view.SearchPlaceholder.Content = null;
                    return;
                }
                if (view.MorePanel.Visibility == System.Windows.Visibility.Visible)
                {
                    view.AppBarPanel.Close();
                    return;
                }
            }

            IsOpen = false;
        }
    }

    public enum ShareContactDetailsMode
    {
        Share,
        View,
    }
}

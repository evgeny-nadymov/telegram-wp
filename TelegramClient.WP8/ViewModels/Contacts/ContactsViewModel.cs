using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Globalization;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.UserData;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Search;

namespace TelegramClient.ViewModels.Contacts
{
    public class ContactsViewModel : ItemsViewModelBase<TLUserBase>,
        IHandle<string>,
        IHandle<TLUserBase>,
        IHandle<TaskCompleted<SaveContactResult>>
    {
        private ObservableCollection<AlphaKeyGroup<TLUserBase>> _contacts;

        public ObservableCollection<AlphaKeyGroup<TLUserBase>> Contacts
        {
            get { return _contacts; }
            set { SetField(ref _contacts, value, () => Contacts); }
        }

        private volatile bool _isGettingContacts;

        public bool FirstRun { get; set; }

        public ContactsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, IEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Items = new AlphaKeyGroup<TLUserBase>("@");

            _contacts = new ObservableCollection<AlphaKeyGroup<TLUserBase>>();
            _contacts.Add((AlphaKeyGroup<TLUserBase>)Items);

            DisplayName = LowercaseConverter.Convert(AppResources.Contacts);
            Status = AppResources.Loading;

            EventAggregator.Subscribe(this);
        }

        private void UpdateItemsAsync()
        {
            _isGettingContacts = true;
            //IsWorking = true;
            var contactIds = string.Join(",", Items.Select(x => x.Index).OrderBy(x => x));
            var hash = MD5Core.GetHash(contactIds);
            var hashString = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            MTProtoService.GetContactsAsync(new TLString(hashString),
                result =>
                {
                    _isGettingContacts = false;

                    //IsWorking = false;
                    if (result is TLContactsNotModified)
                    {
                        return;
                    }

                    InsertContacts(((TLContacts)result).Users, false);
                },
                error =>
                {
                    _isGettingContacts = false;
                    //IsWorking = false;
                });
        }

        private bool _runOnce = true;

        protected override void OnActivate()
        {
            base.OnActivate();

            //if (FirstRun)
            //{
            //    SignInImport();
            //}
            //else
            {
                if (!_runOnce)
                {
                    BeginOnThreadPool(() =>
                    {
                        Thread.Sleep(300);
                        try
                        {
                            foreach (var item in Items)
                            {
                                item.NotifyOfPropertyChange("Status");
                            }

                        }
                        catch (Exception e)
                        {
                            TLUtils.WriteLine(e.ToString(), LogSeverity.Error);
                        }
                    });

                    return;
                }
                _runOnce = false;
                //BeginOnThreadPool(() =>
                {
#if WP7
                    Thread.Sleep(400);
#endif
                    var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                    if (isAuthorized)
                    {
                        Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.Loading : string.Empty;
                        var contacts = CacheService.GetContacts();
                        Status = string.Empty;
                        var orderedContacts = contacts.OrderBy(x => x.FullName).ToList();
                        var count = 0;

                        Items.Clear();
                        for (var i = 0; count < 10 && i < orderedContacts.Count; i++)
                        {
                            if (!(orderedContacts[i] is TLUserEmpty)
                                && orderedContacts[i].Index != StateService.CurrentUserId)
                            {
                                Items.Add(orderedContacts[i]);
                            }
                            count++;
                        }

                        LazyItems.Clear();
                        for (var i = count; i < orderedContacts.Count; i++)
                        {
                            if (!(orderedContacts[i] is TLUserEmpty) && orderedContacts[i].Index != StateService.CurrentUserId)
                            {
                                LazyItems.Add(orderedContacts[i]);
                            }
                        }


                        Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.Loading : string.Empty;
                        if (LazyItems.Count > 0)
                        {
                            PopulateItems(() =>
                            {
                                ImportContactsAsync();
                                UpdateItemsAsync();
                            });
                        }
                        else
                        {
                            ImportContactsAsync();
                            UpdateItemsAsync();
                        }
                    }
                }//);
            }
        }

        #region Commands

        public void AddContact()
        {
            EventAggregator.RequestTask<SaveContactTask>();
        }

        public void DeleteContact(TLUserBase user)
        {
            if (user == null) return;

            MTProtoService.DeleteContactAsync(
                user.ToInputUser(),
                link => Items.Remove(user),
                error =>
                {
                    
                });
        }

        public void UserAction(TLUserBase user)
        {
            OpenContactDetails(user);
        }

        public FrameworkElement OpenContactElement;

        public void SetOpenContactElement(object element)
        {
            OpenContactElement = element as FrameworkElement;
        }

        public void OpenContactDetails(TLUserBase user)
        {
            if (user == null || user is TLUserEmpty) return;

            if (user is TLUserNotRegistered)
            {
                //return;

                EventAggregator.RequestTask<SmsComposeTask>(t =>
                {
                    t.Body = AppResources.InviteFriendMessage;
                    t.To = user.Phone.ToString();
                });
                return;
            }

            StateService.DialogMessages = CacheService.GetHistory(user.Index);
            StateService.With = user;
            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
        }

        private Stopwatch _stopwatch;

        private void ImportContactsAsync(bool fullReplace = false)
        {
            var contacts = new Microsoft.Phone.UserData.Contacts();
            contacts.SearchCompleted += (e, args) => OnSearchCompleted(e, args, fullReplace);
            _stopwatch = Stopwatch.StartNew();
            contacts.SearchAsync(string.Empty, FilterKind.None, null);
        }

        private char _prevKey = '#';

        private void OnSearchCompleted(object sender, ContactsSearchEventArgs args, bool fullReplace)
        {

            TLUtils.WritePerformance("::Search contacts time: " + _stopwatch.Elapsed);
            _stopwatch = Stopwatch.StartNew();
            var contacts = args.Results;

            //var usersCache = Items.ToDictionary(x => x.Index);

            var contactsCache = new Dictionary<string, Contact>();
            var notRegisteredContacts = new List<TLUserNotRegistered>();
            foreach (var contact in contacts)
            {
                foreach (var phoneNumber in contact.PhoneNumbers)
                {
                    if (!contactsCache.ContainsKey(phoneNumber.PhoneNumber))
                    {
                        contactsCache.Add(phoneNumber.PhoneNumber, contact);
                    }
                }
                var completeName = contact.CompleteName;
                var firstName = completeName != null ? completeName.FirstName ?? "" : "";
                var lastName = completeName != null ? completeName.LastName ?? "" : "";


                if (string.IsNullOrEmpty(firstName) 
                    && string.IsNullOrEmpty(lastName))
                {
                    if (!string.IsNullOrEmpty(contact.DisplayName))
                    {
                        firstName = contact.DisplayName;
                    }
                    else
                    {
                        continue;
                    }
                }

                var clientId = contact.GetHashCode();
                var phone = contact.PhoneNumbers.FirstOrDefault();
                if (phone != null)
                    //&& !usersCache.ContainsKey(clientId))
                {
                    var notRegisteredUser = new TLUserNotRegistered
                    {
                        Id = new TLInt(-1),
                        Phone = new TLString(phone.PhoneNumber),
                        _firstName = new TLString(firstName),
                        _lastName = new TLString(lastName),
                        ClientId = new TLLong(clientId),
                        _photo = new TLPhotoEmpty(),
                        PhoneNumbers = contact.PhoneNumbers
                    };

                    if (lastName.Length > 0)
                    {
                        notRegisteredContacts.Add(notRegisteredUser);
                    }
                }
            }


            TLUtils.WritePerformance("::Get not registered phones time: " + _stopwatch.Elapsed);

            _stopwatch = Stopwatch.StartNew();

            var groups = AlphaKeyGroup<TLUserBase>.CreateGroups(
                notRegisteredContacts,
                Thread.CurrentThread.CurrentUICulture,
                x => x.FullName,
                false);


            TLUtils.WritePerformance("::Get groups time: " + _stopwatch.Elapsed);

            foreach (var @group in groups)
            {
                var gr = new AlphaKeyGroup<TLUserBase>(@group.Key);
                foreach (var u in @group.OrderBy(x => x.FullName))
                {
                    gr.Add(u);
                }

                BeginOnUIThread(() =>
                {
                    Contacts.Add(gr);
                });
            }

            var phones = contactsCache.Keys.Take(Constants.MaxImportingContactsCount).ToList();
            var importingContacts = new TLVector<TLInputContactBase>();
            foreach (var phone in phones)
            {
                var completeName = contactsCache[phone].CompleteName;

                var firstName = completeName != null ? completeName.FirstName ?? "" : "";
                var lastName = completeName != null ? completeName.LastName ?? "" : "";

                if (string.IsNullOrEmpty(firstName)
                    && string.IsNullOrEmpty(lastName))
                {
                    if (!string.IsNullOrEmpty(contactsCache[phone].DisplayName))
                    {
                        firstName = contactsCache[phone].DisplayName;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (firstName == "" && lastName == "") continue;


                var contact = new TLInputContact
                {
                    Phone = new TLString(phone),
                    FirstName = new TLString(firstName),
                    LastName = new TLString(lastName),
                    ClientId = new TLLong(contactsCache[phone].GetHashCode())
                };

                importingContacts.Add(contact);
            }

            _isLoading = true;
            var getResponse = false;
            BeginOnThreadPool(() =>
            {
                Thread.Sleep(1500);
                if (!getResponse)
                {
                    IsWorking = true;
                }
            });
            //IsWorking = true;
            MTProtoService.ImportContactsAsync(importingContacts, new TLBool(false),
                importedContacts =>
                {
                    getResponse = true;
                    _isLoading = true;
                    IsWorking = false;
                    Status = Items.Count == 0 && LazyItems.Count == 0 && importedContacts.Users.Count == 0
                        ? string.Format("{0}", AppResources.NoContactsHere)
                        : string.Empty;
                    InsertContacts(importedContacts.Users, fullReplace);
                },
                error =>
                {
                    getResponse = true;
                    _isLoading = true;
                    Status = string.Empty;
                    IsWorking = false;
                });
        }

        private bool _isLoading;

        public override void RefreshItems()
        {
            ImportContactsAsync(true);
        }

        public void InviteFriends()
        {
            EventAggregator.RequestTask<SmsComposeTask>(t => t.Body = AppResources.InviteFriendMessage);
        }

        private void InsertContacts(IEnumerable<TLUserBase> newUsers, bool fullReplace)
        {
            var itemsCache = Items.OfType<TLUserContact>().ToDictionary(x => x.Index);
            


            var users = newUsers.OrderByDescending(x => x.FullName);
            var addingUsers = new List<TLUserBase>();

            //if (fullReplace)
            //{
            //    Items.Clear();
            //    addingUsers.AddRange(users);
            //}
            //else
            {
                foreach (var user in users)
                {
                    if (!itemsCache.ContainsKey(user.Index) && !(user is TLUserEmpty) && user.Index != StateService.CurrentUserId)
                    {
                        addingUsers.Add(user);
                    }
                }
            }

            Status = addingUsers.Count != 0 || Items.Count != 0 || LazyItems.Count != 0 ? string.Empty : Status;
            foreach (var addingUser in addingUsers)
            {
                InsertContact(addingUser);
            }
        }

        private void InsertContact(TLUserBase user)
        {
            var comparer = Comparer<string>.Default;
            var position = 0;
            for (var i = 0; i < Items.Count; i++)
            {
                if (comparer.Compare(Items[i].FullName, user.FullName) == 0)
                {
                    position = -1;
                    break;
                }
                if (comparer.Compare(Items[i].FullName, user.FullName) > 0)
                {
                    position = i;
                    break;
                }
            }

            if (position != -1)
            {
                BeginOnUIThread(() => Items.Insert(position, user));
                //Thread.Sleep(20);
            }
        }

        #endregion

        public void Handle(string command)
        {
            if (string.Equals(command, Commands.LogOutCommand))
            {
                _runOnce = true;
                LazyItems.Clear();
                Items.Clear();
                Status = string.Empty;
                IsWorking = false;
            }
        }

        public void Handle(TaskCompleted<SaveContactResult> taskCompleted)
        {
            if (taskCompleted.Result.TaskResult == TaskResult.OK)
            {
                ImportContactsAsync();
            }
        }

        public void Search()
        {
            NavigationService.UriFor<SearchContactsViewModel>().Navigate();
        }

        public void Handle(TLUserBase user)
        {
            BeginOnUIThread(() =>
            {
                var item = Items.FirstOrDefault(x => x.Index == user.Index);
                if (item != null)
                {
                    if (!(user is TLUserContact))
                    {
                        Items.Remove(item);
                    }
                }
                else if (user is TLUserContact)
                {
                    InsertContact(user);
                }
            });
        }
    }

    public class AlphaKeyGroup<T> : ObservableCollection<T>
    {
        /// <summary>
        /// The delegate that is used to get the key information.
        /// </summary>
        /// <param name="item">An object of type T</param>
        /// <returns>The key value to use for this object</returns>
        public delegate string GetKeyDelegate(T item);

        /// <summary>
        /// The Key of this group.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Public constructor.
        /// </summary>
        /// <param name="key">The key for this group.</param>
        public AlphaKeyGroup(string key)
        {
            Key = key;
        }

        /// <summary>
        /// Create a list of AlphaGroup<T> with keys set by a SortedLocaleGrouping.
        /// </summary>
        /// <param name="slg">The </param>
        /// <returns>Theitems source for a LongListSelector</returns>
        private static List<AlphaKeyGroup<T>> CreateGroups(SortedLocaleGrouping slg)
        {
            List<AlphaKeyGroup<T>> list = new List<AlphaKeyGroup<T>>();

            foreach (string key in slg.GroupDisplayNames)
            {
                list.Add(new AlphaKeyGroup<T>(key));
            }

            return list;
        }

        /// <summary>
        /// Create a list of AlphaGroup<T> with keys set by a SortedLocaleGrouping.
        /// </summary>
        /// <param name="items">The items to place in the groups.</param>
        /// <param name="ci">The CultureInfo to group and sort by.</param>
        /// <param name="getKey">A delegate to get the key from an item.</param>
        /// <param name="sort">Will sort the data if true.</param>
        /// <returns>An items source for a LongListSelector</returns>
        public static List<AlphaKeyGroup<T>> CreateGroups(IEnumerable<T> items, CultureInfo ci, GetKeyDelegate getKey, bool sort)
        {
            SortedLocaleGrouping slg = new SortedLocaleGrouping(ci);
            List<AlphaKeyGroup<T>> list = CreateGroups(slg);

            foreach (T item in items)
            {
                int index = 0;
                if (slg.SupportsPhonetics)
                {
                    //check if your database has yomi string for item
                    //if it does not, then do you want to generate Yomi or ask the user for this item.
                    //index = slg.GetGroupIndex(getKey(Yomiof(item)));
                }
                else
                {
                    index = slg.GetGroupIndex(getKey(item));
                }
                if (index >= 0 && index < list.Count)
                {
                    list[index].Add(item);
                }
            }

            //if (sort)
            //{
            //    foreach (AlphaKeyGroup<T> group in list)
            //    {
            //        group.Sort((c0, c1) => { return ci.CompareInfo.Compare(getKey(c0), getKey(c1)); });
            //    }
            //}

            return list;
        }

    }
}

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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Windows.ApplicationModel.Contacts;
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Models;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.ViewModels.Search;
using Execute = Telegram.Api.Helpers.Execute;
using TaskResult = Microsoft.Phone.Tasks.TaskResult;

namespace TelegramClient.ViewModels.Contacts
{
    public class ContactsViewModel : ItemsViewModelBase<TLUserBase>,
        Telegram.Api.Aggregator.IHandle<string>,
        Telegram.Api.Aggregator.IHandle<TLUpdateContactLinkBase>,
        Telegram.Api.Aggregator.IHandle<TLUpdateUserStatus>,
        Telegram.Api.Aggregator.IHandle<InvokeImportContacts>,
        Telegram.Api.Aggregator.IHandle<InvokeDeleteContacts>,
        Telegram.Api.Aggregator.IHandle<TLUpdatePrivacy>,
        Telegram.Api.Aggregator.IHandle<TLUpdateUserName>,
        Telegram.Api.Aggregator.IHandle<TLUpdateUserPhoto>
    {
        private ObservableCollection<AlphaKeyGroup<TLUserBase>> _contacts;

        public ObservableCollection<AlphaKeyGroup<TLUserBase>> Contacts
        {
            get { return _contacts; }
            set { SetField(ref _contacts, value, () => Contacts); }
        }

        public bool FirstRun { get; set; }

        public TLUserBase Self { get; set; }

        private IFileManager _downloadFileManager;

        public IFileManager DownloadFileManager
        {
            get { return _downloadFileManager ?? (_downloadFileManager = IoC.Get<IFileManager>()); }
        }

        public ContactsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            App.Log("start ContactsViewModel.ctor ");

            Items = new AlphaKeyGroup<TLUserBase>("@");

            _contacts = new ObservableCollection<AlphaKeyGroup<TLUserBase>> { (AlphaKeyGroup<TLUserBase>)Items };

            DisplayName = LowercaseConverter.Convert(AppResources.Contacts);
            Status = AppResources.Loading;

            EventAggregator.Subscribe(this);

            App.Log("end ContactsViewModel.ctor");
        }

        private readonly object _savedCountSyncRoot = new object();

        public void GetContactsAsync(System.Action callback)
        {
            var savedCount = TLUtils.OpenObjectFromMTProtoFile<TLInt>(_savedCountSyncRoot, Constants.SavedCountFileName);
            var hash = TLUtils.GetContactsHash(savedCount, CacheService.GetContacts().Where(x => x.IsContact).OrderBy(x => x.Index).ToList());

            IsWorking = true;
            MTProtoService.GetContactsAsync(new TLInt(hash),
                result => Execute.BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage(result.ToString());

                    IsWorking = false;
                    var contacts = result as TLContacts71;
                    if (contacts != null)
                    {
                        TLUtils.SaveObjectToMTProtoFile(_savedCountSyncRoot, Constants.SavedCountFileName, contacts.SavedCount);
                        InsertContacts(contacts.Users);
                    }

                    var contactsNotModified = result as TLContactsNotModified;
                    if (contactsNotModified != null)
                    {

                    }

                    callback.SafeInvoke();
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("contacts.getContacts error: " + error);

                    callback.SafeInvoke();
                }));
        }

        private bool _runOnce = true;

        protected override void OnActivate()
        {
            base.OnActivate();

            if (!_runOnce)
            {
                UpdateStatusesAsync();

                return;
            }
            _runOnce = false;

            LoadCacheAsync();
        }

        private void LoadCache()
        {
            var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
            if (!isAuthorized)
            {
                return;
            }

            Status = string.Empty;

            var contacts = CacheService.GetContacts();
            var orderedContacts = contacts.OrderBy(x => x.FullName).ToList();
            var count = 0;


            var currentContact = CacheService.GetUser(new TLInt(StateService.CurrentUserId));
            if (currentContact != null)
            {
                Self = currentContact;
                NotifyOfPropertyChange(() => Self);
            }

            System.Diagnostics.Debug.WriteLine("LoadCache ordered_contacts={0} lazy_items={1}", orderedContacts.Count, LazyItems.Count);

            Items.Clear();
            LazyItems.Clear();
            for (var i = 0; i < orderedContacts.Count; i++)
            {
                if (!(orderedContacts[i] is TLUserEmpty)
                    && orderedContacts[i].Index != StateService.CurrentUserId)
                {
                    if (count < 10)
                    {
                        Items.Add(orderedContacts[i]);
                    }
                    else
                    {
                        LazyItems.Add(orderedContacts[i]);
                    }
                    count++;
                }
            }

            Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.Loading : string.Empty;

            if (LazyItems.Count > 0)
            {
                BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    foreach (var item in LazyItems)
                    {
                        Items.Add(item);
                    }
                    LazyItems.Clear();

                    Handle(new InvokeImportContacts());
                });
            }
            else
            {
                Handle(new InvokeImportContacts());
            }
        }

        private void LoadCacheAsync()
        {
#if WP8
            LoadCache();
#else
            BeginOnUIThread(TimeSpan.FromSeconds(0.4), LoadCache);
#endif
        }

        private DateTime? _lastUpdateStatusesTime;

        private void UpdateStatusesAsync()
        {
            BeginOnThreadPool(TimeSpan.FromSeconds(1.0), () =>
            {
                if (_lastUpdateStatusesTime.HasValue
                    && _lastUpdateStatusesTime.Value.AddSeconds(30.0) > DateTime.Now)
                {
                    return;
                }

                try
                {
                    for (var i = 0; i < Items.Count; i++)
                    {
                        Items[i].NotifyOfPropertyChange(() => Items[i].StatusCommon);
                    }

                    _lastUpdateStatusesTime = DateTime.Now;
                }
                catch (Exception e)
                {
                    Execute.ShowDebugMessage("UpdateStatuses ex " + e);
                }
            });
        }

        #region Commands

        public void Search()
        {
            StateService.NavigateToDialogDetails = true;
            NavigationService.UriFor<SearchViewModel>().Navigate();
        }

        public void AddContact()
        {
            var task = new SaveContactTask();
            task.Completed += (o, e) =>
            {
                if (e.TaskResult == TaskResult.OK)
                {
                    ImportContactsAsync();
                }
            };
            task.Show();
        }

        public void DeleteContact(TLUserBase user)
        {
            if (user == null) return;

            MTProtoService.DeleteContactAsync(
                user.ToInputUser(),
                link => BeginOnUIThread(() => Items.Remove(user)),
                error => Execute.ShowDebugMessage("contacts.deleteContact error: " + error));
        }

        public void UserAction(TLUserBase user)
        {
            if (user == null) return;

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
                var task = new SmsComposeTask();
                task.Body = AppResources.InviteFriendMessage;
                task.To = user.Phone != null ? user.Phone.ToString() : string.Empty;
                task.Show();

                return;
            }

            StateService.With = user;
            StateService.AnimateTitle = true;
            NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
        }

        private Stopwatch _stopwatch;
        private readonly object _importedPhonesRoot = new object();

        public async void ImportContactsAsync()
        {
            Telegram.Logs.Log.Write("Contacts start search 2");

            _stopwatch = Stopwatch.StartNew();
            var contactStore = await ContactManager.RequestStoreAsync();
            if (contactStore == null)
            {
                Telegram.Logs.Log.Write("ContactsStore is null");
                IsWorking = false;
                Status = Items.Count == 0 && LazyItems.Count == 0
                    ? AppResources.NoContactsHere
                    : string.Empty;

                return;
            }

            var contacts = await contactStore.FindContactsAsync();

            Telegram.Logs.Log.Write("Contacts search completed count=" + contacts.Count);

            TLUtils.WritePerformance("::Search contacts time: " + _stopwatch.Elapsed);
            _stopwatch = Stopwatch.StartNew();

            var phonesCache = new Dictionary<string, Contact>();
            var notRegisteredContacts = new List<TLUserBase>();

            foreach (var contact in contacts)
            {
                foreach (var phoneNumber in contact.Phones)
                {
                    phonesCache[phoneNumber.Number] = contact;
                }

                var notRegisteredUser = GetNotRegisteredUser(contact);
                if (notRegisteredUser != null)
                {
                    notRegisteredContacts.Add(notRegisteredUser);
                }
            }

            Telegram.Logs.Log.Write("Contacts skip empty count=" + notRegisteredContacts.Count);

            TLUtils.WritePerformance("::Get not registered phones time: " + _stopwatch.Elapsed);

            _stopwatch = Stopwatch.StartNew();

            var groups = AlphaKeyGroup<TLUserBase>.CreateGroups(
                notRegisteredContacts,
                Thread.CurrentThread.CurrentUICulture,
                x => x.FullName,
                false);

            TLUtils.WritePerformance("::Get groups time: " + _stopwatch.Elapsed);

            var contactKeys = new Dictionary<string, string>();
            foreach (var contact in Contacts)
            {
                contactKeys[contact.Key] = contact.Key;
            }
            BeginOnThreadPool(() =>
            {
                foreach (var @group in groups)
                {
                    var gr = new AlphaKeyGroup<TLUserBase>(@group.Key);
                    foreach (var u in @group.OrderBy(x => x.FullName))
                    {
                        gr.Add(u);
                    }

                    if (!contactKeys.ContainsKey(gr.Key))
                    {
                        BeginOnUIThread(() => Contacts.Add(gr));
                    }
                }
            });

            var importedPhonesCache = GetImportedPhones();

            Telegram.Logs.Log.Write("Contacts load imported count=" + importedPhonesCache.Count);

            var phones = phonesCache.Keys.Take(Constants.MaxImportingContactsCount).ToList();
            var importingContacts = new TLVector<TLInputContactBase>();
            var importingPhones = new List<string>();
            foreach (var phone in phones)
            {
                if (importedPhonesCache.ContainsKey(phone))
                {
                    continue;
                }

                var firstLastName = GetFirstLastName(phonesCache[phone]);

                var contact = new TLInputContact
                {
                    Phone = new TLString(phone),
                    FirstName = new TLString(firstLastName.Item1),
                    LastName = new TLString(firstLastName.Item2),
                    ClientId = new TLLong(phonesCache[phone].GetHashCode())
                };

                importingContacts.Add(contact);
                importingPhones.Add(phone);
            }

            Telegram.Logs.Log.Write("Contacts skip imported count=" + importingContacts.Count);

            if (importingContacts.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("contacts.importContacts id={0}", string.Join(",", importingContacts.Select(x => ((TLInputContact)x).Phone)));

                IsWorking = true;
                MTProtoService.ImportContactsAsync(importingContacts,
                    result => Execute.BeginOnUIThread(() =>
                    {
                        Telegram.Logs.Log.Write("Contacts contacts.importContacts result=" + result);

                        IsWorking = false;
                        Status = Items.Count == 0 && LazyItems.Count == 0 && result.Users.Count == 0
                            ? AppResources.NoContactsHere
                            : string.Empty;

                        var retryContactsCount = result.RetryContacts.Count;
                        if (retryContactsCount > 0)
                        {
                            Execute.ShowDebugMessage("contacts.importContacts retry_contacts_count=" + retryContactsCount);
                        }

                        InsertContacts(result.Users);

                        SaveImportedPhones(importedPhonesCache, importingPhones);
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        Telegram.Logs.Log.Write("Contacts contacts.importContacts error=" + error);

                        IsWorking = false;
                        Status = string.Empty;

                        Execute.ShowDebugMessage("contacts.importContacts error=" + error);
                    }));
            }
            else
            {
                Status = Items.Count == 0 && LazyItems.Count == 0
                    ? AppResources.NoContactsHere
                    : string.Empty;
            }
        }

        public static TLUserNotRegistered GetNotRegisteredUser(Contact contact)
        {
            var firstLastName = GetFirstLastName(contact);
            if (string.IsNullOrEmpty(firstLastName.Item1) && string.IsNullOrEmpty(firstLastName.Item2))
            {
                return null;
            }

            var phones = new TLVector<TLUserPhone>();
            foreach (var contactPhone in contact.Phones)
            {
                phones.Add(new TLUserPhone
                {
                    Kind = new TLInt((int)contactPhone.Kind),
                    Number = string.IsNullOrEmpty(contactPhone.Number) ? TLString.Empty : new TLString(contactPhone.Number),
                    Description = string.IsNullOrEmpty(contactPhone.Description) ? TLString.Empty : new TLString(contactPhone.Description),
                });
            }

            var phone = phones.FirstOrDefault(x => !TLString.IsNullOrEmpty(x.Number));
            if (phone != null)
            {
                return new TLUserNotRegistered
                {
                    Id = new TLInt(0),
                    Phone = phone.Number,
                    Phones = phones,
                    _firstName = new TLString(firstLastName.Item1),
                    _lastName = new TLString(firstLastName.Item2),
                    ClientId = new TLLong(contact.GetHashCode()),
                    _photo = new TLPhotoEmpty(),
                };
            }

            return null;
        }

        public static Tuple<string, string> GetFirstLastName(Contact contact)
        {
            var firstName = contact.FirstName ?? string.Empty;
            var lastName = contact.LastName ?? string.Empty;

            if (string.IsNullOrEmpty(firstName)
                && string.IsNullOrEmpty(lastName)
                && !string.IsNullOrEmpty(contact.DisplayName))
            {
                firstName = contact.DisplayName;
            }

            return new Tuple<string, string>(firstName, lastName);
        }

        private void SaveImportedPhones(Dictionary<string, string> importedPhonesCache, List<string> importingPhones)
        {
            foreach (var importingPhone in importingPhones)
            {
                importedPhonesCache[importingPhone] = importingPhone;
            }

            var importedPhones = new TLVector<TLString>(importedPhonesCache.Keys.Count);
            foreach (var importedPhone in importedPhonesCache.Keys)
            {
                importedPhones.Add(new TLString(importedPhone));
            }

            TLUtils.SaveObjectToMTProtoFile(_importedPhonesRoot, Constants.ImportedPhonesFileName, importedPhones);
        }

        private Dictionary<string, string> GetImportedPhones()
        {
            var importedPhones =
                TLUtils.OpenObjectFromMTProtoFile<TLVector<TLString>>(_importedPhonesRoot, Constants.ImportedPhonesFileName) ??
                new TLVector<TLString>();

            var importedPhonesCache = new Dictionary<string, string>();
            foreach (var importedPhone in importedPhones)
            {
                var phone = importedPhone.ToString();
                importedPhonesCache[phone] = phone;
            }

            return importedPhonesCache;
        }

        private void InsertContacts(IEnumerable<TLUserBase> newUsers)
        {
            var itemsCache = new Dictionary<int, TLUserBase>();

            for (int i = 0; i < Items.Count; i++)
            {
                var userBase = Items[i];
                if (userBase != null
                    && userBase.IsContact
                    && !itemsCache.ContainsKey(userBase.Index))
                {
                    itemsCache[userBase.Index] = userBase;
                }
            }

            var users = newUsers.OrderByDescending(x => x.FullName);
            var addingUsers = new List<TLUserBase>();

            foreach (var user in users)
            {
                if (!itemsCache.ContainsKey(user.Index) && !(user is TLUserEmpty) && user.Index != StateService.CurrentUserId)
                {
                    addingUsers.Add(user);
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
            var position = -1;
            for (var i = 0; i < Items.Count; i++)
            {
                if (Items[i].Index == user.Index)
                {
                    position = -2;
                    break;
                }

                if (comparer.Compare(Items[i].FullName, user.FullName) >= 0)
                {
                    position = i;
                    break;
                }
            }

            if (position == -1)
            {
                position = Items.Count;
            }

            System.Diagnostics.Debug.WriteLine("InsertContact id={0} position={1} full_name={2}", user.Id, position, user.FullName);

            if (position >= 0)
            {
                Items.Insert(position, user);
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
                FileUtils.Delete(_importedPhonesRoot, Constants.ImportedPhonesFileName);
                FileUtils.Delete(_savedCountSyncRoot, Constants.SavedCountFileName);
            }
        }

        public void Handle(TLUpdateContactLinkBase update)
        {
            BeginOnUIThread(() =>
            {
                if (LazyItems.Count > 0) return;

                var updateContactLink24 = update as TLUpdateContactLink24;
                if (updateContactLink24 == null) return;

                var isContact = updateContactLink24.MyLink is TLContactLink;
                var item = Items.FirstOrDefault(x => x.Index == updateContactLink24.UserId.Value);
                if (item != null)
                {
                    if (isContact)
                    {
                        InsertContact(item);
                    }
                    else
                    {
                        Items.Remove(item);
                    }
                }
                else if (isContact)
                {
                    var user = CacheService.GetUser(update.UserId);
                    if (user != null)
                    {
                        InsertContact(user);
                    }
                }
            });
        }

        public void Handle(TLUpdateUserStatus updateUserStatus)
        {
            BeginOnUIThread(() =>
            {
                if (LazyItems.Count > 0) return;

                var item = Items.FirstOrDefault(x => x.Index == updateUserStatus.UserId.Value);
                if (item != null)
                {
                    item.NotifyOfPropertyChange(() => item.Status);
                    item.NotifyOfPropertyChange(() => item.StatusCommon);
                }
            });
        }

        public void Handle(InvokeImportContacts message)
        {
            GetContactsAsync(ImportContactsAsync);
        }

        public void Handle(InvokeDeleteContacts message)
        {
            var id = new TLVector<TLInputUserBase>(CacheService.GetContacts().Where(x => x.IsContact).Select(x => x.ToInputUser()).ToList());

            MTProtoService.DeleteContactsAsync(id,
                result => Execute.BeginOnUIThread(() =>
                {
                    Handle(Commands.LogOutCommand);
                    Handle(new InvokeImportContacts());
                }),
                error => Execute.BeginOnUIThread(() =>
                {

                }));
        }

        public void Handle(TLUpdatePrivacy update)
        {
            if (update.Key is TLPrivacyKeyStatusTimestamp)
            {
                Execute.ShowDebugMessage("update privacy");
                MTProtoService.GetStatusesAsync(
                    statuses => Execute.BeginOnUIThread(() =>
                    {
                        try
                        {
                            for (var i = 0; i < Items.Count; i++)
                            {
                                Items[i].NotifyOfPropertyChange(() => Items[i].StatusCommon);
                            }

                            _lastUpdateStatusesTime = DateTime.Now;
                        }
                        catch (Exception e)
                        {
                            Execute.ShowDebugMessage("UpdateStatuses ex " + e);
                        }
                    }),
                    error =>
                    {
                        Execute.ShowDebugMessage("contacts.getStatuses error " + error);
                    });
            }
        }

        public void Handle(TLUpdateUserName update)
        {
            // threadpool
            var userId = update.UserId;
            var userBase = CacheService.GetUser(userId);
            if (userBase != null && userBase.IsContact)
            {
                ContactsHelper.UpdateContactAsync(DownloadFileManager, StateService, userBase);
            }
        }

        public void Handle(TLUpdateUserPhoto update)
        {
            //threadpool
            var userId = update.UserId;
            var userBase = CacheService.GetUser(userId);
            if (userBase != null && userBase.IsContact)
            {
                ContactsHelper.UpdateContactAsync(DownloadFileManager, StateService, userBase);
            }
        }
    }

    public class InvokeImportContacts { }

    public class InvokeDeleteContacts { }
}

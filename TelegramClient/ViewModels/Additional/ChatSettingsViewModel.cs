// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using Windows.Phone.PersonalInformation;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class ChatSettingsViewModel : ViewModelBase
    {
        private bool _locationServices;

        public bool LocationServices
        {
            get { return _locationServices; }
            set
            {
                SetField(ref _locationServices, value, () => LocationServices);
                StateService.GetNotifySettingsAsync(settings =>
                {
                    settings.LocationServices = value;
                    StateService.SaveNotifySettingsAsync(settings);
                });
            }
        }

        private bool _isPeopleHubEnabled = true;

        public bool IsPeopleHubEnabled
        {
            get { return _isPeopleHubEnabled; }
            set { SetField(ref _isPeopleHubEnabled, value, () => IsPeopleHubEnabled); }
        }

        private bool _peopleHub;

        public bool PeopleHub
        {
            get { return _peopleHub; }
            set
            {
                SetField(ref _peopleHub, value, () => PeopleHub);
                StateService.GetNotifySettingsAsync(settings =>
                {
                    settings.PeopleHub = value;
                    StateService.SaveNotifySettingsAsync(settings);
                });
            }
        }

        private readonly TLChatSettings _chatSettings;

        public bool AutoDownloadPhotoPrivateChats
        {
            get { return _chatSettings.AutoDownloadPhotoPrivateChats; }
            set { _chatSettings.AutoDownloadPhotoPrivateChats = value; }
        }

        public bool AutoDownloadPhotoGroups
        {
            get { return _chatSettings.AutoDownloadPhotoGroups; }
            set { _chatSettings.AutoDownloadPhotoGroups = value; }
        }

        public bool AutoDownloadAudioPrivateChats
        {
            get { return _chatSettings.AutoDownloadAudioPrivateChats; }
            set { _chatSettings.AutoDownloadAudioPrivateChats = value; }
        }

        public bool AutoDownloadAudioGroups
        {
            get { return _chatSettings.AutoDownloadAudioGroups; }
            set { _chatSettings.AutoDownloadAudioGroups = value; }
        }

        public bool AutoDownloadGifPrivateChats
        {
            get { return _chatSettings.AutoDownloadGifPrivateChats; }
            set { _chatSettings.AutoDownloadGifPrivateChats = value; }
        }

        public bool AutoDownloadGifGroups
        {
            get { return _chatSettings.AutoDownloadGifGroups; }
            set { _chatSettings.AutoDownloadGifGroups = value; }
        }

        public bool AutoPlayGif
        {
            get { return _chatSettings.AutoPlayGif; }
            set { _chatSettings.AutoPlayGif = value; }
        }

        private IFileManager DownloadManager
        {
            get { return IoC.Get<IFileManager>(); }
        }

        public ChatSettingsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _chatSettings = StateService.GetChatSettings();


            StateService.GetNotifySettingsAsync(
                settings =>
                {
                    _locationServices = settings.LocationServices;
                    _peopleHub = settings.PeopleHub;

                    BeginOnUIThread(() =>
                    {
                        NotifyOfPropertyChange(() => LocationServices);
                    });
                });

            PropertyChanged += OnPropertyChanged;
        }

        public static ContactsOperationToken PreviousToken;

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => PeopleHub))
            {
#if WP8
                if (PreviousToken != null)
                {
                    PreviousToken.IsCanceled = true;
                }

                if (PeopleHub)
                {
                    var contacts = CacheService.GetContacts().Where(x => x != null && x.IsContact).ToList();
                    var token = new ContactsOperationToken();

                    PreviousToken = token;
                    ContactsHelper.ImportContactsAsync(DownloadManager, token, contacts,
                        tuple =>
                        {
                            var importedCount = tuple.Item1;
                            var totalCount = tuple.Item2;

                            var isComplete = importedCount == totalCount;
                            if (isComplete)
                            {
                                PreviousToken = null;
                            }

                            var duration = isComplete ? 0.5 : 2.0;
                            MTProtoService.SetMessageOnTime(duration,
                                string.Format(AppResources.SyncContactsProgress, importedCount, totalCount));
                        },
                        () =>
                        {
                            MTProtoService.SetMessageOnTime(0.0, string.Empty);
                        });
                }
                else
                {
                    IsPeopleHubEnabled = false;
                    MTProtoService.SetMessageOnTime(25.0, AppResources.DeletingContacts);
                    ContactsHelper.DeleteContactsAsync(() =>
                    {
                        IsPeopleHubEnabled = true;
                        MTProtoService.SetMessageOnTime(0.0, string.Empty);
                    });
                }
#endif
            }
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            StateService.SaveChatSettings(_chatSettings);
        }

        public void OpenBackgrounds()
        {
            StateService.GetWallpapersAsync(result => BeginOnUIThread(() =>
            {
                StateService.Wallpapers = result;
                NavigationService.UriFor<ChooseBackgroundViewModel>().Navigate();
            }));
        }

        public void OpenCacheSettings()
        {
            NavigationService.UriFor<CacheViewModel>().Navigate();
        }

        public void OpenCameraSettings()
        {
            var settings = StateService.GetCameraSettings();
            NavigationService.UriFor<CameraViewModel>().Navigate();
        }

        public void OpenPhotoPickerSettings()
        {
            var settings = StateService.GetPhotoPickerSettings();
            NavigationService.UriFor<PhotoPickerViewModel>().Navigate();
        }
    }

    public class ContactsOperationToken
    {
        public volatile bool IsCanceled;
    }

    public static class ContactsHelper
    {
        private static readonly object _delayedContactsSyncRoot = new object();

        private static TLVector<TLInt> _delayedContacts;

        public static void GetDelayedContactsAsync(Action<TLVector<TLInt>> callback)
        {
            if (_delayedContacts != null)
            {
                callback.SafeInvoke(_delayedContacts);
            }

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _delayedContacts = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLInt>>(_delayedContactsSyncRoot, Constants.DelayedContactsFileName) ?? new TLVector<TLInt>();
                callback.SafeInvoke(_delayedContacts);
            });
        }

        public static void SaveDelayedContactsAsync(TLVector<TLInt> contacts)
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                TLUtils.SaveObjectToMTProtoFile(_delayedContactsSyncRoot, Constants.DelayedContactsFileName, contacts);
            });
        }

        public static void UpdateDelayedContactsAsync(ICacheService cacheService, IMTProtoService mtProtoService)
        {
#if WP8
            GetDelayedContactsAsync(contactIds =>
            {
                var contacts = new List<TLUserBase>();
                foreach (var contactId in contactIds)
                {
                    var userBase = cacheService.GetUser(contactId);
                    if (userBase != null && userBase.IsContact)
                    {
                        contacts.Add(userBase);
                    }
                }

                if (contacts.Count > 0)
                {
                    var token = new ContactsOperationToken();
                    var fileManager = IoC.Get<IFileManager>();
                    ChatSettingsViewModel.PreviousToken = token;
                    ImportContactsAsync(fileManager, token, contacts,
                        tuple =>
                        {
                            var importedCount = tuple.Item1;
                            var totalCount = tuple.Item2;

                            var isComplete = importedCount == totalCount;
                            if (isComplete)
                            {
                                ChatSettingsViewModel.PreviousToken = null;
                            }

                            var duration = isComplete ? 0.5 : 2.0;
                            mtProtoService.SetMessageOnTime(duration,
                                string.Format(AppResources.SyncContactsProgress, importedCount, totalCount));
                        },
                        () =>
                        {
                            mtProtoService.SetMessageOnTime(0.0, string.Empty);
                        });
                }
            });
#endif
        }

        public static void UpdateContactAsync(IFileManager fileManager, IStateService stateService, TLUserBase contact)
        {
#if WP8
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                stateService.GetNotifySettingsAsync(
                async settings =>
                {
                    if (settings.PeopleHub)
                    {
                        var store = await ContactStore.CreateOrOpenAsync();
                        var delayedContact = await UpdateContactInternalAsync(contact, fileManager, store, false);
                        if (delayedContact != null)
                        {
                            GetDelayedContactsAsync(contacts =>
                            {
                                contacts.Add(delayedContact.Id);
                                SaveDelayedContactsAsync(contacts);
                            });
                        }
                    }
                }));
#endif
        }

        public static void DeleteContactAsync(IStateService stateService, TLInt userId)
        {
#if WP8
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                stateService.GetNotifySettingsAsync(
                async settings =>
                {
                    if (settings.PeopleHub)
                    {
                        var store = await ContactStore.CreateOrOpenAsync();
                        var phoneContact = await store.FindContactByRemoteIdAsync(userId.ToString());
                        await store.DeleteContactAsync(phoneContact.Id);
                    }
                }));
#endif
        }

        public static void CreateContactAsync(IFileManager fileManager, IStateService stateService, TLUserBase contact)
        {
#if WP8
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                stateService.GetNotifySettingsAsync(
                async settings =>
                {
                    if (settings.PeopleHub)
                    {
                        var store = await ContactStore.CreateOrOpenAsync();
                        var delayedContact = await UpdateContactInternalAsync(contact, fileManager, store, true);
                        if (delayedContact != null)
                        {
                            GetDelayedContactsAsync(contacts =>
                            {
                                contacts.Add(delayedContact.Id);
                                SaveDelayedContactsAsync(contacts);
                            });
                        }
                    }
                }));
#endif
        }

        public static void DeleteContactsAsync(System.Action callback)
        {
#if WP8
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(
                async () =>
                {
                    var store = await ContactStore.CreateOrOpenAsync();
                    try
                    {
                        await store.DeleteAsync();
                        FileUtils.Delete(_delayedContactsSyncRoot, Constants.DelayedContactsFileName);
                    }
                    catch (Exception ex)
                    {
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("store.DeleteAsync ex " + ex);
                    }
                    finally
                    {
                        callback.SafeInvoke();
                    }
                });
#endif
        }

#if WP8
        public static async Task<TLUserBase> UpdateContactInternalAsync(TLUserBase contact, IFileManager fileManager, ContactStore store, bool updateOrCreate)
        {
            TLUserBase delayedContact = null;
            var remoteId = contact.Index.ToString(CultureInfo.InvariantCulture);
            var phoneContact = await store.FindContactByRemoteIdAsync(remoteId);

            if (updateOrCreate)
            {
                phoneContact = phoneContact ?? new StoredContact(store);
            }

            if (phoneContact == null)
            {
                return delayedContact;
            }

            phoneContact.RemoteId = remoteId;
            phoneContact.GivenName = contact.FirstName.ToString(); //FirstName
            phoneContact.FamilyName = contact.LastName.ToString(); //LastName

            var userProfilePhoto = contact.Photo as TLUserProfilePhoto;
            if (userProfilePhoto != null)
            {
                var location = userProfilePhoto.PhotoSmall as TLFileLocation;
                if (location != null)
                {
                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        location.VolumeId,
                        location.LocalId,
                        location.Secret);
                    using (var isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (isoStore.FileExists(fileName))
                        {
                            using (var file = isoStore.OpenFile(fileName, FileMode.Open, FileAccess.Read))
                            {
                                await phoneContact.SetDisplayPictureAsync(file.AsInputStream());
                            }
                        }
                        else
                        {
                            fileManager.DownloadFile(location, userProfilePhoto, new TLInt(0));
                            delayedContact = contact;
                        }
                    }
                }
            }

            var emptyPhoto = contact.Photo as TLPhotoEmpty;
            if (emptyPhoto != null)
            {
                try
                {
                    await phoneContact.SetDisplayPictureAsync(null);
                }
                catch (Exception ex)
                {

                }
            }

            var props = await phoneContact.GetPropertiesAsync();
            var mobilePhone = contact.Phone.ToString();
            if (mobilePhone.Length > 0)
            {
                props[KnownContactProperties.MobileTelephone] = mobilePhone.StartsWith("+")
                    ? mobilePhone
                    : "+" + mobilePhone;
            }

            var usernameContact = contact as IUserName;
            if (usernameContact != null)
            {
                var username = usernameContact.UserName.ToString();

                if (username.Length > 0)
                {
                    props[KnownContactProperties.Nickname] = username;
                }
            }

            await phoneContact.SaveAsync();

            return delayedContact;
        }
#endif

        public static void ImportContactsAsync(IFileManager fileManager, ContactsOperationToken token, IList<TLUserBase> contacts, Action<Telegram.Api.WindowsPhone.Tuple<int, int>> progressCallback, System.Action cancelCallback)
        {
#if WP8
            Execute.BeginOnThreadPool(async () =>
            {
                //var contacts = _cacheService.GetContacts();
                var totalCount = contacts.Count;
                if (totalCount == 0) return;


                var store = await ContactStore.CreateOrOpenAsync();
                var importedCount = 0;
                var delayedContacts = new TLVector<TLInt>();
                foreach (var contact in contacts)
                {

                    if (token.IsCanceled)
                    {
                        cancelCallback.SafeInvoke();
                        return;
                    }

                    try
                    {
                        var delayedContact = await UpdateContactInternalAsync(contact, fileManager, store, true);
                        if (delayedContact != null)
                        {
                            delayedContacts.Add(delayedContact.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        // continue import after failed contact
                    }
                    //Thread.Sleep(100);
                    importedCount++;
                    progressCallback.SafeInvoke(new Telegram.Api.WindowsPhone.Tuple<int, int>(importedCount, totalCount));
                    //var duration = importedCount == totalCount ? 0.5 : 2.0;
                    //_mtProtoService.SetMessageOnTime(duration, string.Format("Sync contacts ({0} of {1})...", importedCount, totalCount));
                }

                var result = new TLVector<TLInt>();
                foreach (var delayedContact in delayedContacts)
                {
                    result.Add(delayedContact);
                }
                SaveDelayedContactsAsync(result);
            });
#endif
        }
    }
}

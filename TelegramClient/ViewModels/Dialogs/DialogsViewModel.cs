 using System;
 using System.Collections.Generic;
 using System.Collections.ObjectModel;
 using System.Diagnostics;
 using System.Linq;
 using System.Windows;
using Caliburn.Micro;
 using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
 using Telegram.Api.Services.Cache.EventArgs;
 using Telegram.Api.Services.FileManager;
 using Telegram.Api.Services.Updates;
 using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
 using TelegramClient.ViewModels.Contacts;
using Execute = Telegram.Api.Helpers.Execute; 

namespace TelegramClient.ViewModels.Dialogs
{
    public partial class DialogsViewModel : ItemsViewModelBase<TLDialogBase>, 
        IHandle<TopMessageUpdatedEventArgs>, 
        IHandle<DialogAddedEventArgs>,
        IHandle<DialogRemovedEventArgs>,
        IHandle<DownloadableItem>,
        IHandle<UploadableItem>,
        IHandle<string>,
        IHandle<TLEncryptedChatBase>,
        IHandle<TLUpdateUserName>,
        IHandle<UpdateCompletedEventArgs>,
        IHandle<TLUpdateNotifySettings>
    {
        public bool FirstRun { get; set; }

        public DialogsViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, IEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Items = new ObservableCollection<TLDialogBase>();
            EventAggregator.Subscribe(this);

            DisplayName = (string)new LowercaseConverter().Convert(AppResources.Dialogs, null, null, null);


            Status = Items.Count == 0 && LazyItems.Count == 0 ? AppResources.Loading : string.Empty;
            //CacheService.DialogAdded += OnDialogAdded;
            //CacheService.TopMessageUpdated += OnTopMessageUpdated;

            BeginOnThreadPool(() =>
                {
                    var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                    if (isAuthorized)
                    {
                        var dialogs = CacheService.GetDialogs();

                        var dialogsCache = new Dictionary<int, TLDialogBase>();
                        var clearedDialogs = new List<TLDialogBase>();
                        foreach (var dialog in dialogs)
                        {
                            if (!dialogsCache.ContainsKey(dialog.Index))
                            {
                                clearedDialogs.Add(dialog);
                                dialogsCache[dialog.Index] = dialog;
                            }
                            else
                            {
                                var cachedDialog = dialogsCache[dialog.Index];
                                if (cachedDialog.Peer is TLPeerUser && dialog.Peer is TLPeerUser)
                                {
                                    CacheService.DeleteDialog(dialog);
                                    continue;
                                }
                                if (cachedDialog.Peer is TLPeerChat && dialog.Peer is TLPeerChat)
                                {
                                    CacheService.DeleteDialog(dialog);
                                    continue;
                                }
                            }
                        }


                        // load cache
                        Status = dialogs.Count == 0? AppResources.Loading : string.Empty;
                                
                        //BeginOnUIThread(() =>
                        //{
                        //    Items.Clear();
                        //    foreach (var d in clearedDialogs)
                        //    {
                        //        Items.Add(d);
                        //    }
                        //});
                        LazyItems.Clear();
                        //((BindableCollection<TLDialog>)Items).AddRange(dialogs);
                        //BeginOnUIThread(() =>
                        {
                            foreach (var dialog in clearedDialogs)
                            {
                                //Items.Add(dialog);
                                LazyItems.Add(dialog);
                            }
                        }
                        //);

                        if (LazyItems.Count == 0)
                        {
                            UpdateItemsAsync(0, 0, Telegram.Api.Constants.CachedDialogsCount);
                        }
                        else
                        {
                            BeginOnUIThread(() => PopulateItems(() => UpdateItemsAsync(0, 0, Telegram.Api.Constants.CachedDialogsCount)));
                        }
                        // update history
                                
                            
                    }
                });
        }

        //protected override void OnPopulateCompleted()
        //{
        //    UpdateItemsAsync(0, 0, Constants.DialogsSlice);

        //    base.OnPopulateCompleted();
        //}

        private void UpdateItemsAsync(int offset, int maxId, int count)
        {
            //IsWorking = true;

            MTProtoService.GetDialogsAsync(new TLInt(offset), new TLInt(maxId), new TLInt(count),
                result =>
                {
                    // сортируем, т.к. при синхронизации, если есть отправляющиеся сообщений, то TopMessage будет замещен на них
                    // и начальная сортировка сломается
                    var orderedDialogs = new TLVector<TLDialogBase>(result.Dialogs.Count);
                    foreach (var orderedDialog in result.Dialogs.OrderByDescending(x => x.GetDateIndex()))
                    {
                        orderedDialogs.Add(orderedDialog);
                    }
                    result.Dialogs = orderedDialogs;

                    //IsWorking = false;
                    var needUpdate = false;
                    var itemsCount = Items.Count;
                    for (var i = 0; i < result.Dialogs.Count; i++)
                    {
                        if (itemsCount - 1 < i || result.Dialogs[i] != Items[i])
                        {
                            needUpdate = true;
                            break;
                        }
                    }

                    // load updated cache
                    Status = Items.Count == 0 && result.Dialogs.Count == 0? string.Format("{0}", AppResources.NoDialogsHere) : string.Empty;
                    
                    if (needUpdate)
                    {
                        BeginOnUIThread(() =>
                        {
                            var encryptedDialogs = Items.OfType<TLEncryptedDialog>();
                            var startIndex = 0;
                            foreach (var encryptedDialog in encryptedDialogs)
                            {
                                for (var i = startIndex; i < result.Dialogs.Count; i++)
                                {
                                    if (encryptedDialog.GetDateIndex() > result.Dialogs[i].GetDateIndex())
                                    {
                                        result.Dialogs.Insert(i, encryptedDialog);
                                        startIndex = i;
                                        break;
                                    }
                                }
                            }

                            var broadcasts = Items.OfType<TLBroadcastDialog>();
                            startIndex = 0;
                            foreach (var broadcast in broadcasts)
                            {
                                for (var i = startIndex; i < result.Dialogs.Count; i++)
                                {
                                    if (broadcast.GetDateIndex() > result.Dialogs[i].GetDateIndex())
                                    {
                                        result.Dialogs.Insert(i, broadcast);
                                        startIndex = i;
                                        break;
                                    }
                                }
                            }

                            Items.Clear();
                            foreach (var dialog in result.Dialogs)
                            {
                                Items.Add(dialog);
                            }

                        });
                    }
                },
                error =>
                {
                    Status = string.Empty;
                    //IsWorking = false;
                });
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (FirstRun)
            {
                OnInitialize();
            }
        }

        protected override void OnInitialize()
        {
            BeginOnThreadPool(() =>
            {
                var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
                if (!FirstRun)
                {
                    return;
                }
                if (!isAuthorized)
                {
                    return;
                }

                FirstRun = false;

                Status = Items.Count == 0 && LazyItems.Count == 0? AppResources.Loading : string.Empty;
                MTProtoService.GetDialogsAsync(new TLInt(0), new TLInt(0), new TLInt(Constants.DialogsSlice),
                    dialogs =>
                    {
                        foreach (var dialog in dialogs.Dialogs)
                        {
                            LazyItems.Add(dialog);
                        }

                        Status = Items.Count == 0 && LazyItems.Count == 0? string.Format("{0}", AppResources.NoDialogsHere) : string.Empty;
                        if (LazyItems.Count > 0)
                        {
                            BeginOnUIThread(() => PopulateItems(() =>
                            {
                                EventAggregator.Publish(new InvokeImportContacts());
                            }));
                        }
                        else
                        {
                            EventAggregator.Publish(new InvokeImportContacts());
                        }
                    },
                    error =>
                    {
                        EventAggregator.Publish(new InvokeImportContacts());
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("messages.getHistory error " + error);
                        Status = string.Empty;
                    });
            });

            

            base.OnInitialize();
        }

        #region Actions

        public FrameworkElement OpenDialogElement;

        public void SetOpenDialogElement(object element)
        {
            OpenDialogElement = element as FrameworkElement;
        }

        public override void RefreshItems()
        {
            UpdateItemsAsync(0, 0, Constants.DialogsSlice);
        }

        #endregion

        public void Handle(TopMessageUpdatedEventArgs eventArgs)
        {
            eventArgs.Dialog.NotifyOfPropertyChange(() => eventArgs.Dialog.With);
            OnTopMessageUpdated(this, eventArgs);
        }

        public void Handle(DialogAddedEventArgs eventArgs)
        {
            OnDialogAdded(this, eventArgs);
        }

        private void OnTopMessageUpdated(object sender, TopMessageUpdatedEventArgs e)
        {
            BeginOnUIThread(() =>
            {
                var currentPosition = Items.IndexOf(e.Dialog);
                var newPosition = currentPosition;

                for (var i = 0; i < Items.Count; i++)
                {
                    if (// мигает диалог, если просто обновляется последнее сообщение, то номер становится на 1 больше
                        // и сначала удаляем, а потом вставляем на туже позицию
                        i != currentPosition
                        && Items[i].GetDateIndex() <= e.Dialog.GetDateIndex())
                    {
                        newPosition = i;
                        break;
                    }
                }

                if (currentPosition != newPosition)
                {
                    if (currentPosition < newPosition)
                    {
                        // т.к. будем сначала удалять диалог а потом вставлять, то
                        // curPos + 1 = newPos - это вставка на тоже место и не имеет смысла
                        if (currentPosition + 1 == newPosition)
                        {
                            Items[currentPosition].NotifyOfPropertyChange(() => Items[currentPosition].Self);
                            return;
                        }
                        Items.Remove(e.Dialog);
                        Items.Insert(newPosition - 1, e.Dialog);
                    }
                    else
                    {
                        Items.Remove(e.Dialog);
                        Items.Insert(newPosition, e.Dialog);
                    }
                }
            });
        }

        private void OnDialogAdded(object sender, DialogAddedEventArgs e)
        {
            var dialog = e.Dialog;
            if (dialog == null) return;

            BeginOnUIThread(() =>
            {
                var index = -1;
                for (var i = 0; i < Items.Count; i++)
                {
                    if (Items[i] == e.Dialog)
                    {
                        return;
                    }

                    if (Items[i].GetDateIndex() < dialog.GetDateIndex())
                    {
                        index = i;
                        break;
                    }
                }

                if (index == -1)
                {
                    Items.Add(dialog);
                }
                else
                {
                    Items.Insert(index, dialog);
                }
                Status = Items.Count == 0 || LazyItems.Count == 0 ? string.Empty : Status;
            });
        }

        public void Handle(DialogRemovedEventArgs args)
        {
            BeginOnUIThread(() =>
            {
                Items.Remove(args.Dialog);
                
            });
        }

        public void Handle(DownloadableItem item)
        {
            var photo = item.Owner as TLUserProfilePhoto;
            if (photo != null)
            {
                var user = CacheService.GetUser(photo);
                if (user != null)
                {
                    user.NotifyOfPropertyChange(() => user.Photo);
                }
                return;
            }

            var chatPhoto = item.Owner as TLChatPhoto;
            if (chatPhoto != null)
            {
                var chat = CacheService.GetChat(chatPhoto);
                if (chat != null)
                {
                    chat.NotifyOfPropertyChange(() => chat.Photo);
                }
                return;
            }
        }

        public void Handle(string command)
        {
            if (string.Equals(command, Commands.LogOutCommand))
            {
                LazyItems.Clear();
                BeginOnUIThread(() => Items.Clear());
                Status = string.Empty;
                IsWorking = false;
            }
        }

        public void Handle(TLUpdateUserName userName)
        {
            Execute.BeginOnUIThread(() =>
            {
                for (var i = 0; i < Items.Count; i++)
                {
                    if (Items[i].WithId == userName.UserId.Value
                        && Items[i].With is TLUserBase)
                    {
                        var user = (TLUserBase)Items[i].With;
                        user.FirstName = userName.FirstName;
                        user.LastName = userName.LastName;

                        var userWithUserName = user as IUserName;
                        if (userWithUserName != null)
                        {
                            userWithUserName.UserName = userName.UserName;
                        }

                        Items[i].NotifyOfPropertyChange(() => Items[i].With);
                        break;
                    }
                }
            });
        }

        public void Handle(UploadableItem item)
        {
            var userSelf = item.Owner as TLUserSelf;
            if (userSelf != null)
            {

                MTProtoService.UploadProfilePhotoAsync(
                    new TLInputFile
                    {
                        Id = item.FileId,
                        MD5Checksum = new TLString(MD5Core.GetHashString(item.Bytes).ToLowerInvariant()),
                        Name = new TLString(Guid.NewGuid() + ".jpg"),
                        Parts = new TLInt(item.Parts.Count)
                    },
                    new TLString(""),
                    new TLInputGeoPointEmpty(),
                    new TLInputPhotoCropAuto(),
                    photosPhoto =>
                    {
                        MTProtoService.GetFullUserAsync(new TLInputUserSelf(), userFull => { }, error => { });
                    },
                    error =>
                    {

                    });
                return;
            }


            var chat = item.Owner as TLChat;
            if (chat != null)
            {
                MTProtoService.EditChatPhotoAsync(
                    chat.Id,
                   new TLInputChatUploadedPhoto
                   {
                       File = new TLInputFile
                       {
                           Id = item.FileId,
                           MD5Checksum = new TLString(MD5Core.GetHashString(item.Bytes).ToLowerInvariant()),
                           Name = new TLString("chatPhoto.jpg"),
                           Parts = new TLInt(item.Parts.Count)
                       },
                       Crop = new TLInputPhotoCropAuto()
                   },
                   photosPhoto =>
                   {
                       //MTProtoService.GetFullChatAsync((chat).Id, userFull =>
                       //{
                       //    //NotifyOfPropertyChange(() => CurrentItem);
                       //},
                       //error => { });
                   },
                   error =>
                   {

                   });
            }
        }

        public void Handle(TLEncryptedChatBase chat)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                int index = -1;
                TLDialogBase dialog = null;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Peer is TLPeerEncryptedChat
                        && Items[i].Peer.Id.Value == chat.Id.Value)
                    {
                        index = i;
                        dialog = Items[i];
                        break;
                    }
                }

                if (index != -1 && dialog != null)
                {
                    dialog.NotifyOfPropertyChange(() => dialog.Self);
                }
            });
        }

        public void Handle(UpdateCompletedEventArgs args)
        {
            var dialogs = CacheService.GetDialogs();

            Execute.BeginOnUIThread(() =>
            {
                Items.Clear();
                foreach (var dialog in dialogs)
                {
                    Items.Add(dialog);
                }
            });
        }
    }
}

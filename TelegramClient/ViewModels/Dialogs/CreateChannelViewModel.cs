using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Chats;
using TelegramClient.ViewModels.Search;

namespace TelegramClient.ViewModels.Dialogs
{
    public class CreateChannelViewModel : CreateDialogViewModel
    {
        private string _about;

        public string About
        {
            get { return _about; }
            set { SetField(ref _about, value, () => About); }
        }

        private string _link;

        public string Link
        {
            get { return _link; }
            set { SetField(ref _link, value, () => Link); }
        }

        public TLPhotoBase Photo { get; set; }

        public CreateChannelViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            GroupedUsers = new ObservableCollection<TLUserBase>();
            //GroupedUsers = new ObservableCollection<AlphaKeyGroup<TLUserBase>>();

            BeginOnThreadPool(() =>
            {
                //Thread.Sleep(300);
                _source = _source ??
                    CacheService.GetContacts()
                    .Where(x => !(x is TLUserEmpty) && x.Index != StateService.CurrentUserId)
                    .OrderBy(x => x.FullName)
                    .ToList();

                Status = string.Empty;
                foreach (var contact in _source)
                {
                    contact._isSelected = false;
                    LazyItems.Add(contact);
                }

                if (_source.Count == 0)
                {
                    Status = AppResources.NoUsersHere;
                }

                BeginOnUIThread(PopulateItems);
                Thread.Sleep(500);
                BeginOnUIThread(() =>
                {
                    foreach (var item in _source)
                    {
                        GroupedUsers.Add(item);
                    }
                });
                //var groups = AlphaKeyGroup<TLUserBase>.CreateGroups(
                //    _source,
                //    Thread.CurrentThread.CurrentUICulture,
                //    x => x.FullName,
                //    false);

                //foreach (var @group in groups)
                //{
                //    var gr = new AlphaKeyGroup<TLUserBase>(@group.Key);
                //    foreach (var u in @group.OrderBy(x => x.FullName))
                //    {
                //        gr.Add(u);
                //    }

                //    BeginOnUIThread(() =>
                //    {
                //        GroupedUsers.Add(gr);
                //    });
                //}


            });
        }

        public void ChoosePhoto()
        {
            EditChatActions.EditPhoto(photo =>
            {
                var volumeId = TLLong.Random();
                var localId = TLInt.Random();
                var secret = TLLong.Random();

                var fileLocation = new TLFileLocation
                {
                    VolumeId = volumeId,
                    LocalId = localId,
                    Secret = secret,
                    DCId = new TLInt(0),
                    //Buffer = p.Bytes
                };

                var fileName = String.Format("{0}_{1}_{2}.jpg",
                    fileLocation.VolumeId,
                    fileLocation.LocalId,
                    fileLocation.Secret);

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (var fileStream = store.CreateFile(fileName))
                    {
                        fileStream.Write(photo, 0, photo.Length);
                    }
                }

                Photo = new TLChatPhoto
                {
                    PhotoSmall = new TLFileLocation
                    {
                        DCId = fileLocation.DCId,
                        VolumeId = fileLocation.VolumeId,
                        LocalId = fileLocation.LocalId,
                        Secret = fileLocation.Secret
                    },
                    PhotoBig = new TLFileLocation
                    {
                        DCId = fileLocation.DCId,
                        VolumeId = fileLocation.VolumeId,
                        LocalId = fileLocation.LocalId,
                        Secret = fileLocation.Secret
                    }
                };
                NotifyOfPropertyChange(() => Photo);
            });
        }

        public override void Create()
        {
            if (string.IsNullOrEmpty(Title))
            {
                MessageBox.Show(AppResources.PleaseEnterGroupSubject, AppResources.Error, MessageBoxButton.OK);
                return;
            }

            var participants = new TLVector<TLInputUserBase>();
            foreach (var item in SelectedUsers)
            {
                participants.Add(item.ToInputUser());
            }
            participants.Add(new TLInputUser { UserId = new TLInt(StateService.CurrentUserId), AccessHash = new TLLong(0) });

            if (participants.Count == 0)
            {
                MessageBox.Show(AppResources.PleaseChooseAtLeastOneParticipant, AppResources.Error, MessageBoxButton.OK);
                return;
            }

            var broadcastChat = new TLChannel49
            {
                Flags = new TLInt(0),
                Id = TLInt.Random(),
                AccessHash = new TLLong(0),
                Title = new TLString(Title),
                Photo = Photo?? new TLChatPhotoEmpty(),
                UserName = new TLString(Link),
                Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                Version = new TLInt(0),
                About = new TLString(About),
                ParticipantIds = new TLVector<TLInt> { Items = SelectedUsers.Select(x => x.Id).ToList() },
            };

            CacheService.SyncBroadcast(broadcastChat, result =>
            {
                var broadcastPeer = new TLPeerBroadcast { Id = broadcastChat.Id };
                var serviceMessage = new TLMessageService17
                {
                    FromId = new TLInt(StateService.CurrentUserId),
                    ToId = broadcastPeer,
                    Status = MessageStatus.Confirmed,
                    Out = TLBool.True,
                    Date = TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, DateTime.Now),
                    //IsAnimated = true,
                    RandomId = TLLong.Random(),
                    Action = new TLMessageActionChannelCreate
                    {
                        Title = broadcastChat.Title,
                    }
                };
                serviceMessage.SetUnread(TLBool.False);

                CacheService.SyncMessage(serviceMessage,
                    message =>
                    {
                        StateService.With = broadcastChat;
                        StateService.RemoveBackEntry = true;
                        NavigationService.UriFor<DialogDetailsViewModel>().Navigate();
                    });

            });
        }
    }
}

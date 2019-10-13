// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.ObjectModel;
using System.Windows;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Contacts;

namespace TelegramClient.ViewModels.Media
{
    public class SecretMediaViewModel : ItemsViewModelBase<TLDecryptedMessage>, Telegram.Api.Aggregator.IHandle<DownloadableItem>
    {
        private bool _isEmptyList;

        public bool IsEmptyList
        {
            get { return _isEmptyList; }
            set { SetField(ref _isEmptyList, value, () => IsEmptyList); }
        }

        public string EmptyListImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    return "/Images/Messages/media.white-WXGA.png";
                }

                return "/Images/Messages/media.black-WXGA.png";
            }
        }

        public SecretMediaViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) : 
            base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Items = new ObservableCollection<TLDecryptedMessage>();
        }

        public string DisplayName
        {
            get { return AppResources.Media.ToLowerInvariant(); }
        }

        public SecretContactViewModel Contact { get; set; }

        protected override void OnActivate()
        {
            var mediaItems = StateService.CurrentDecryptedMediaMessages;
            StateService.CurrentDecryptedMediaMessages = null;

            if (mediaItems != null)
            {
                foreach (var item in mediaItems)
                {
                    if (item.TTL == null || item.TTL.Value == 0)
                    {
                        Items.Add(item);
                    }
                }
            }

            if (Items.Count == 0)
            {
                IsEmptyList = true;
                Status = string.Empty;
            }

            base.OnActivate();
        }


        public void OpenMedia(TLDecryptedMessage message)
        {
            if (message == null) return;
            if (Contact == null) return;

            if (Contact.ImageViewer == null)
            {
                Contact.ImageViewer = new DecryptedImageViewerViewModel(StateService);
                Contact.NotifyOfPropertyChange(() => Contact.ImageViewer);
            }

            //var mediaPhoto = message.Media as TLMessageMediaPhoto;
            //if (mediaPhoto != null)
            {
                StateService.CurrentDecryptedMediaMessages = Items;
                StateService.CurrentDecryptedPhotoMessage = message;

                if (Contact.ImageViewer != null)
                {
                    Contact.ImageViewer.OpenViewer();
                }
                //NavigationService.UriFor<ImageViewerViewModel>().Navigate();
                return;
            }
        }

        public void Handle(DownloadableItem message)
        {
            
        }
    }
}

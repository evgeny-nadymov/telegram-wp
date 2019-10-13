// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO.IsolatedStorage;
using Caliburn.Micro;
using ImageTools;
using Telegram.Api.Services;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Media
{
    public class AnimatedImageViewerViewModel : TelegramPropertyChangedBase
    {

        public string ImageSource { get; protected set; }

        public TLMessage CurrentItem { get; protected set; }

        public IStateService StateService { get; protected set; }

        public AnimatedImageViewerViewModel(IStateService stateService)
        {
            StateService = stateService;
        }

        private bool _isOpen;

        public bool IsOpen { get { return _isOpen; } }

        public event EventHandler Open;

        protected virtual void RaiseOpen()
        {
            var handler = Open;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public void OpenViewer()
        {
            CurrentItem = StateService.CurrentPhotoMessage;
            _isOpen = CurrentItem != null;
            NotifyOfPropertyChange(() => IsOpen);

            StateService.CurrentPhotoMessage = null;
            StateService.ExtendedImage = null;

            if (CurrentItem != null)
            {
                var mediaDocument = CurrentItem.Media as TLMessageMediaDocument;
                if (mediaDocument != null)
                {
                    var document = mediaDocument.Document as TLDocument;
                    if (document != null)
                    {
                        if (string.Equals(document.MimeType.ToString(), "image/gif", StringComparison.OrdinalIgnoreCase))
                        {
                            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                            {
                                var fileName = document.GetFileName();
                                if (store.FileExists(fileName))
                                {
                                    ImageSource = fileName;
                                    NotifyOfPropertyChange(() => ImageSource);
                                }
                            }
                        }
                    }
                }
            }



            RaiseOpen();
        }

        public event EventHandler Close;

        protected virtual void RaiseClose()
        {
            var handler = Close;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }


        public void CloseViewer()
        {
            ImageSource = null;
            _isOpen = false;
            NotifyOfPropertyChange(() => IsOpen);
            NotifyOfPropertyChange(() => ImageSource);

            //RaiseClose();
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using Caliburn.Micro;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Media
{
    public class RecordedVideo
    {
        public string FileName { get; set; }

        public TLLong FileId { get; set; }

        public long Duration { get; set; }

        public List<UploadablePart> Parts { get; set; } 
    }

    public class VideoCaptureViewModel : Screen
    {
        private readonly INavigationService _navigationService;

        private readonly IStateService _stateService;

        public VideoCaptureViewModel(INavigationService navigationService, IStateService stateService)
        {
            _navigationService = navigationService;
            _stateService = stateService;
        }

        protected override void OnActivate()
        {
            if (_stateService.RemoveBackEntry)
            {
                _stateService.RemoveBackEntry = false;
                _navigationService.RemoveBackEntry();
            }
            base.OnActivate();
        }

        public string VideoIsoFile { get; set; }

        public long Duration { get; set; }

        public void Attach(string fileName, TLLong fileId, List<UploadablePart> parts)
        {
            if (!string.IsNullOrEmpty(VideoIsoFile))
            {
                _stateService.Duration = Duration;
                _stateService.VideoIsoFileName = VideoIsoFile;
                _stateService.RecordedVideo = new RecordedVideo
                {
                    FileName = fileName,
                    FileId = fileId,
                    Duration = Duration,
                    Parts = parts
                };
                _navigationService.GoBack();
            }
        }
    }
}

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
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Telegram.Api.Aggregator;
using TelegramClient.ViewModels.Contacts;
#if WP8
using Windows.Storage;
using Windows.System;
#endif
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;

namespace TelegramClient.ViewModels.Additional
{
    public class CacheViewModel : ViewModelBase
    {
        private string _status = AppResources.Calculating + "...";

        public string Status
        {
            get { return _status; }
            set { SetField(ref _status, value, () => Status); }
        }

        private string _localDatabaseStatus = AppResources.Calculating + "...";

        public string LocalDatabaseStatus
        {
            get { return _localDatabaseStatus; }
            set { SetField(ref _localDatabaseStatus, value, () => LocalDatabaseStatus); }
        }

        private TimerSpan _selectedSpan;

        public TimerSpan SelectedSpan
        {
            get { return _selectedSpan; }
            set
            {
                _selectedSpan = value;

                //if (_selectedSpan != null)
                //{
                //    if (_selectedSpan.Seconds == 0
                //        || _selectedSpan.Seconds == int.MaxValue)
                //    {
                //        MuteUntil = _selectedSpan.Seconds;
                //    }
                //    else
                //    {
                //        var now = DateTime.Now;
                //        var muteUntil = now.AddSeconds(_selectedSpan.Seconds);

                //        MuteUntil = muteUntil < now ? 0 : TLUtils.DateToUniversalTimeTLInt(MTProtoService.ClientTicksDelta, muteUntil).Value;
                //    }
                //}
            }
        }

        public IList<TimerSpan> Spans { get; protected set; } 


        readonly DispatcherTimer _timer = new DispatcherTimer();

        private volatile bool _isCalculating;

        public ClearCacheSettingsViewModel ClearCacheSettings { get; set; }

        private readonly ClearCacheSettings _settings = new ClearCacheSettings();

        public CacheViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            Spans = new List<TimerSpan>
            {
                new TimerSpan(AppResources.WeekNominativeSingular,  "1", (int)TimeSpan.FromDays(7.0).TotalSeconds, string.Format("{0} {1}", "1", AppResources.WeekNominativeSingular).ToLowerInvariant()),
                new TimerSpan(AppResources.MonthNominativeSingular,  "1", (int)TimeSpan.FromDays(30.0).TotalSeconds, string.Format("{0} {1}", "1", AppResources.MonthNominativeSingular).ToLowerInvariant()),
                new TimerSpan(AppResources.Forever, string.Empty, int.MaxValue, AppResources.Forever),
            };
            _selectedSpan = Spans[2];


            Files = new ObservableCollection<TelegramFileInfo>();

            _timer.Interval = TimeSpan.FromSeconds(30.0);
            _timer.Tick += OnTimerTick;

            CalculateCacheSizeAsync((size1, size2) => BeginOnUIThread(() =>
            {
                Status = FileSizeConverter.Convert(size1);
                LocalDatabaseStatus = FileSizeConverter.Convert(size2);
                _settings.NotifyOfPropertyChange(() => _settings.Self);
            }));

            PropertyChanged += (sender, e) =>
            {
                if (Property.NameEquals(e.PropertyName, () => IsWorking))
                {
                    NotifyOfPropertyChange(() => CanClearCache);
                }
            };
        }

        protected override void OnActivate()
        {
#if !DEBUG
            _timer.Start();
#endif

            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
#if !DEBUG
            _timer.Stop();
#endif

            base.OnDeactivate(close);
        }

        private void OnTimerTick(object sender, System.EventArgs e)
        {
            if (_isCalculating) return;

            CalculateCacheSizeAsync((result1, result2) => BeginOnUIThread(() =>
            {
                Status = FileSizeConverter.Convert(result1);
                LocalDatabaseStatus = FileSizeConverter.Convert(result2);
                _settings.NotifyOfPropertyChange(() => _settings.Self);
            }));
        }

        public ObservableCollection<TelegramFileInfo> Files { get; set; } 

        private void CalculateCacheSizeAsync(Action<long, long> callback)
        {
            BeginOnThreadPool(() =>
            {
                _isCalculating = true;

                var length1 = 0L;
                var length2 = 0L;
                var files = new List<TelegramFileInfo>();
                _settings.Clear();
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var fileNames = store.GetFileNames();

                    foreach (var fileName in fileNames)
                    {
                        if (fileName.StartsWith("staticmap"))
                        {
                            
                        }

                        try
                        {
                            var fileInfo = new TelegramFileInfo {Name = fileName};
                            if (store.FileExists(fileName))
                            {
                                using (var file = new IsolatedStorageFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, store))
                                {
                                    fileInfo.Length = file.Length;
                                    if (IsValidCacheFileName(fileName))
                                    {
                                        if (IsValidPhotoFileName(fileName))
                                        {
                                            _settings.PhotosLength += file.Length;
                                        }
                                        else if (IsValidMusicFileName(fileName))
                                        {
                                            _settings.MusicLength += file.Length;
                                        }
                                        else if (IsValidVideoFileName(fileName))
                                        {
                                            _settings.VideoLength += file.Length;
                                        }
                                        else if (IsValidVoiceMessageFileName(fileName))
                                        {
                                            _settings.VoiceMessagesLength += file.Length;
                                        }
                                        else if (IsValidDocumentFileName(fileName))
                                        {
                                            _settings.DocumentsLength += file.Length;
                                        }
                                        else if (IsValidOtherFileName(fileName))
                                        {
                                            _settings.OtherFilesLength += file.Length;
                                        }

                                        length1 += file.Length;
                                        fileInfo.IsValidCacheFileName = true;
                                    }
                                }
                            }
                            files.Add(fileInfo);
                        }
                        catch (Exception ex)
                        {
                            TLUtils.WriteException("CalculateCacheSizeAsync OpenFile: " + fileName, ex);
                        }
                    }

                    var directoryNames = store.GetDirectoryNames();
                    foreach (var fileName in directoryNames)
                    {
                        try
                        {
                            var fileInfo = new TelegramFileInfo { Name = fileName, Length = -1};
                            files.Add(fileInfo);
                        }
                        catch (Exception ex)
                        {
                            TLUtils.WriteException("CalculateCacheSizeAsync OpenFile: " + fileName, ex);
                        }
                    }

                    length2 = GetDatabaseLength(store, files);
                }

                _isCalculating = false; 

                callback.SafeInvoke(length1, length2);

                BeginOnUIThread(() =>
                {
                    Files.Clear();
                    foreach (var file in files)
                    {
                        Files.Add(file);
                    }
                });
            });
        }

        private long GetDatabaseLength(IsolatedStorageFile store, List<TelegramFileInfo> files)
        {
            long length2 = 0L;
            var databaseFileNames = GetDatabaseFileNames();
            foreach (var fileName in databaseFileNames)
            {
                try
                {
                    var fileInfo = new TelegramFileInfo {Name = fileName};
                    if (store.FileExists(fileName))
                    {
                        using (
                            var file = new IsolatedStorageFileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read,
                                store))
                        {
                            var lastAccessTime = store.GetLastAccessTime(fileName);
                            var lastWriteTime = store.GetLastWriteTime(fileName);
                            fileInfo.Length = file.Length;
                            //if (IsValidCacheFileName(fileName))
                            {
                                length2 += file.Length;
                                fileInfo.IsValidCacheFileName = true;
                            }
                        }
                    }
                    files.Add(fileInfo);
                }
                catch (Exception ex)
                {
                    TLUtils.WriteException("CalculateCacheSizeAsync OpenFile: " + fileName, ex);
                }
            }

            return length2;
        }

        private IEnumerable<string> GetDatabaseFileNames()
        {
            yield return InMemoryDatabase.BroadcastsMTProtoFileName;
            yield return InMemoryDatabase.ChatsMTProtoFileName;
            yield return InMemoryDatabase.DialogsMTProtoFileName;
            yield return InMemoryDatabase.EncryptedChatsMTProtoFileName;
            yield return InMemoryDatabase.UsersMTProtoFileName;

            yield return Telegram.Api.Constants.DifferenceFileName;
            yield return Telegram.Api.Constants.TempDifferenceFileName;
            yield return Telegram.Api.Constants.DifferenceTimeFileName;
        }

        public bool CanClearCache
        {
            get { return !IsWorking; }
        }

        private static bool IsValidCacheFileName(string fileName)
        {
            if (fileName == null)
            {
                return false;
            }

            if (fileName.EndsWith(".dat") || fileName.EndsWith(".temp"))
            {
                return false;
            }

            if (IsValidPhotoFileName(fileName)
                || IsValidVideoFileName(fileName)
                || IsValidMusicFileName(fileName)
                || IsValidVoiceMessageFileName(fileName)
                || IsValidDocumentFileName(fileName)
                || IsValidOtherFileName(fileName))
            {
                return true;
            }

            return false;
        }

        private static bool IsValidPhotoFileName(string fileName)
        {
            return fileName.EndsWith(".jpg") || fileName.EndsWith(".png");
        }

        private static bool IsValidMusicFileName(string fileName)
        {
            return fileName.EndsWith(".mp3");
        }

        private static bool IsValidVideoFileName(string fileName)
        {
            return fileName.StartsWith("video");
        }

        private static bool IsValidVoiceMessageFileName(string fileName)
        {
            return fileName.StartsWith("audio");
        }

        private static bool IsValidDocumentFileName(string fileName)
        {
            return fileName.StartsWith("document") 
                || fileName.EndsWith(".mp4");   // http gif
        }

        private static bool IsValidOtherFileName(string fileName)
        {
            return fileName.StartsWith("encrypted");
        }

        public void ClearCache()
        {
            if (_settings == null || _settings.TotalLength() == 0L) return;

            if (ClearCacheSettings == null)
            {
                ClearCacheSettings = new ClearCacheSettingsViewModel(_settings, ClearCacheInternal);
                NotifyOfPropertyChange(() => ClearCacheSettings);
            }

            _settings.SelectAll();
            _settings.NotifyOfPropertyChange(() => _settings.Self);
            BeginOnUIThread(() => ClearCacheSettings.Open());
        }

        public void ClearLocalDatabase()
        {
            var result = MessageBox.Show(AppResources.ClearLocalDatabaseConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                IsWorking = true;
                CacheService.CompressAsync(() =>
                {
                    long newLength;
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        var files = new List<TelegramFileInfo>();
                        newLength = GetDatabaseLength(store, files);
                            
                    }

                    BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        LocalDatabaseStatus = FileSizeConverter.Convert(newLength);

                        EventAggregator.Publish(new ClearLocalDatabaseEventArgs());
                    });
                });
            }
        }

        private void ClearCacheInternal()
        {
            if (_isCalculating) return;
            if (_settings == null) return;
            if (IsWorking) return;

            IsWorking = true;
            BeginOnThreadPool(() =>
            {

                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var fileNames = store.GetFileNames();
                    foreach (var fileName in fileNames)
                    {
                        if (IsValidCacheFileName(fileName))
                        {
                            if (_settings.Photos && IsValidPhotoFileName(fileName))
                            {
                                DeleteFile(store, fileName);
                            }
                            else if (_settings.Music && IsValidMusicFileName(fileName))
                            {
                                DeleteFile(store, fileName);
                            }
                            else if (_settings.Video && IsValidVideoFileName(fileName))
                            {
                                DeleteFile(store, fileName);
                            }
                            else if (_settings.VoiceMessages && IsValidVoiceMessageFileName(fileName))
                            {
                                DeleteFile(store, fileName);
                            }
                            else if (_settings.Documents && IsValidDocumentFileName(fileName))
                            {
                                DeleteFile(store, fileName);
                            }
                            else if (_settings.OtherFiles && IsValidOtherFileName(fileName))
                            {
                                DeleteFile(store, fileName);
                            }
                        }
                    }
                }

                if (_settings.Music 
                    || _settings.Video 
                    || _settings.VoiceMessages 
                    || _settings.Documents 
                    || _settings.OtherFiles)
                {
                    CacheService.ClearLocalFileNames();
                }


                EventAggregator.Publish(new ClearCacheEventArgs());
                var length = _settings.RecalculateLength();                
                Status = FileSizeConverter.Convert(length);
                IsWorking = false;
            });
        }

        private static void DeleteFile(IsolatedStorageFile store, string fileName)
        {
            try
            {
                store.DeleteFile(fileName);
            }
            catch (Exception ex)
            {
                TLUtils.WriteException(ex);
            }
        }

#if WP8
        public async void OpenFile(TelegramFileInfo fileInfo)
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();
            if (store.FileExists(fileInfo.Name))
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileInfo.Name);

                if (file != null)
                {
                    Launcher.LaunchFileAsync(file);
                    return;
                }
            }
        }
#endif
    }

    public class ClearCacheEventArgs
    {

    }

    public class ClearLocalDatabaseEventArgs
    {

    }

    public class TelegramFileInfo
    {
        public string Name { get; set; }

        public long Length { get; set; }

        public bool IsValidCacheFileName { get; set; }
    }

    public class ClearCacheSettings : TelegramPropertyChangedBase
    {
        public bool Photos { get; set; }

        public long PhotosLength { get; set; }

        public Visibility PhotosVisibility { get { return PhotosLength > 0? Visibility.Visible : Visibility.Collapsed; } }

        public bool Video { get; set; }

        public long VideoLength { get; set; }

        public Visibility VideoVisibility { get { return VideoLength > 0 ? Visibility.Visible : Visibility.Collapsed; } }

        public bool Documents { get; set; }

        public long DocumentsLength { get; set; }

        public Visibility DocumentsVisibility { get { return DocumentsLength > 0 ? Visibility.Visible : Visibility.Collapsed; } }

        public bool Music { get; set; }

        public long MusicLength { get; set; }

        public Visibility MusicVisibility { get { return MusicLength > 0 ? Visibility.Visible : Visibility.Collapsed; } }

        public bool VoiceMessages { get; set; }

        public long VoiceMessagesLength { get; set; }

        public Visibility VoiceMessagesVisibility { get { return VoiceMessagesLength > 0 ? Visibility.Visible : Visibility.Collapsed; } }

        public bool OtherFiles { get; set; }

        public long OtherFilesLength { get; set; }

        public Visibility OtherFilesVisibility { get { return OtherFilesLength > 0 ? Visibility.Visible : Visibility.Collapsed; } }

        public ClearCacheSettings Self { get { return this; } }

        public ClearCacheSettings()
        {
            Clear();
        }

        public long TotalLength()
        {
            return PhotosLength + VideoLength + DocumentsLength + MusicLength + VoiceMessagesLength + OtherFilesLength;
        }

        public long RecalculateLength()
        {
            if (Photos) PhotosLength = 0L;
            if (Video) VideoLength = 0L;
            if (Documents) DocumentsLength = 0L;
            if (Music) MusicLength = 0L;
            if (VoiceMessages) VoiceMessagesLength = 0L;
            if (OtherFiles) OtherFilesLength = 0L;

            var length = (!Photos ? PhotosLength : 0)
                + (!Video ? VideoLength : 0) 
                + (!Documents ? DocumentsLength : 0) 
                + (!Music ? MusicLength : 0) 
                + (!VoiceMessages ? VoiceMessagesLength : 0) 
                + (!OtherFiles ? OtherFilesLength : 0);

            return length;
        }

        public void SelectAll()
        {
            Photos = true;
            Video = true;
            Documents = true;
            Music = true;
            VoiceMessages = true;
            OtherFiles = true;
        }

        public void Clear()
        {
            SelectAll();

            PhotosLength = 0L;
            VideoLength = 0L;
            DocumentsLength = 0L;
            MusicLength = 0L;
            VoiceMessagesLength = 0L;
            OtherFilesLength = 0L;
        }
    }
}

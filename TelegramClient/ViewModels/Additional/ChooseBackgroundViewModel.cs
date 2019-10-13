// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TaskResult = Microsoft.Phone.Tasks.TaskResult;

namespace TelegramClient.ViewModels.Additional
{
    public class ChooseBackgroundViewModel : ViewModelBase, Telegram.Api.Aggregator.IHandle<DownloadableItem>
    {
        public ObservableCollection<BackgroundItem> Static { get; set; }

        private readonly BackgroundItem _emptyBackground;

        private readonly BackgroundItem _libraryBackground;

        public  BackgroundItem AnimatedBackground1 { get; protected set; }

        private static TLVector<TLWallPaperBase> _cachedWallpapers;

        public BackgroundItem GalleryItem { get { return _libraryBackground; } }

        public BackgroundItem EmptyItem { get { return _emptyBackground; } }

        public ChooseBackgroundViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Static = new ObservableCollection<BackgroundItem>();

            var currentBackground = StateService.CurrentBackground;

            if (currentBackground != null 
                && currentBackground.Name == Constants.EmptyBackgroundString)
            {
                _emptyBackground = currentBackground;
                _emptyBackground.IsSelected = true;
            }
            else
            {
                _emptyBackground = new BackgroundItem
                {
                    Name = Constants.EmptyBackgroundString,
                    SourceString = string.Empty,
                    IsSelected = currentBackground == null
                };
            }

            if (currentBackground != null
                && currentBackground.Name == Constants.LibraryBackgroundString)
            {
                _libraryBackground = currentBackground;
                _libraryBackground.IsSelected = true;
            }
            else
            {
                _libraryBackground = new BackgroundItem
                {
                    Name = Constants.LibraryBackgroundString,
                    SourceString = "/Images/Backgrounds/gallery_WXGA.png"
                };
            }

            _cachedWallpapers = StateService.Wallpapers;
            StateService.Wallpapers = null;

            _userCachedWallpapers = _cachedWallpapers != null;
            if (!_userCachedWallpapers)
            {
                IsWorking = true;
                MTProtoService.GetWallpapersAsync(
                    results =>
                    {
                        StateService.SaveWallpapersAsync(results);

                        Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                        {
                            IsWorking = false;
                            _cachedWallpapers = results;

                            if (_forwardInAnimationComplete)
                            {
                                OnGetWallpapers(results, 0, results.Count);
                            }
                            else
                            {
                                _wallpapers = results;
                                //OnGetWallpapers(results, 0, 4);
                            }
                        });
                    },
                    error =>
                    {
                        IsWorking = false;
                        Telegram.Api.Helpers.Execute.ShowDebugMessage("account.getWallpapers error " + error);
                    });
            }
            else
            {
                OnGetWallpapers(_cachedWallpapers, 0, 4);
            }
            

            eventAggregator.Subscribe(this);
        }

        private TLVector<TLWallPaperBase> _wallpapers;

        private bool _forwardInAnimationComplete;

        private readonly bool _userCachedWallpapers;

        public void OnForwardInAnimationComplete()
        {
            if (_userCachedWallpapers)
            {
                OnGetWallpapers(_cachedWallpapers, Static.Count, _cachedWallpapers.Count - Static.Count);
            }
            else
            {
                if (_wallpapers != null)
                {
                    OnGetWallpapers(_wallpapers, Static.Count, _wallpapers.Count - Static.Count);
                }
            }

            _forwardInAnimationComplete = true;
        }

        private void OnGetWallpapers(TLVector<TLWallPaperBase> results, int skip, int take)
        {

            var currentItem = StateService.CurrentBackground;
            foreach (var result in results.Skip(skip).Take(take))
            {
                var wallpaper = result as TLWallPaper;
                if (wallpaper == null) continue;

                var size = BackgroundImageConverter.GetPhotoSize(wallpaper.Sizes, 480.0);
                if (size != null)
                {
                    var location = size.Location as TLFileLocation;
                    if (location != null)
                    {
                        var fileName = String.Format("{0}_{1}_{2}.jpg",
                            location.VolumeId,
                            location.LocalId,
                            location.Secret);

                        BackgroundItem item;
                        var isSelected = currentItem != null && "telegram" + wallpaper.Id == currentItem.Name;
                        if (isSelected)
                        {
                            item = currentItem;
                            item.IsSelected = true;
                        }
                        else
                        {
                            item = new BackgroundItem
                            {
                                Name = "telegram" + wallpaper.Id,
                                Wallpaper = wallpaper,
                                IsoFileName = fileName
                            };
                        } 
                        Static.Add(item);
                    }
                }
            }
            
        }

#if DEBUG
        /// <summary>
        /// Add a finalizer to check for memory leaks
        /// </summary>
        //~ChooseBackgroundViewModel()
        //{
        //    TLUtils.WritePerformance("++ChooseBackgroundViewModel dstr");
        //}
#endif

        public void Choose(BackgroundItem item)
        {
            if (item == null)
            {
                return;
            }

            if (item == _libraryBackground)
            {
                var task = new PhotoChooserTask
                {
                    PixelHeight = 800, 
                    PixelWidth = 480, 
                    ShowCamera = true
                };
                task.Completed += (sender, result) =>
                {
                    if (result.TaskResult != TaskResult.OK)
                    {
                        return;
                    }

                    byte[] bytes;
                    var sourceStream = result.ChosenPhoto;
                    var fileName = Path.GetFileName(result.OriginalFileName);

                    if (string.IsNullOrEmpty(fileName))
                    {
                        return;
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        sourceStream.CopyTo(memoryStream);
                        bytes = memoryStream.ToArray();
                    }

                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (var file = store.OpenFile(fileName, FileMode.CreateNew))
                        {
                            file.Write(bytes, 0, bytes.Length);
                        }
                    }

                    _libraryBackground.IsoFileName = fileName;

                    ReplaceBackgroundItem(_libraryBackground);
                };
                task.Show();

                return;
            }

            if (item == AnimatedBackground1)
            {
                MessageBox.Show(AppResources.PleaseNoteThatAnimatedBackgroundConsumesMoreBatteryResources);
            }

            ReplaceBackgroundItem(item);
        }

        private void ReplaceBackgroundItem(BackgroundItem item)
        {
            var currentItem = StateService.CurrentBackground;
            if (currentItem != null)
            {
                currentItem.IsSelected = false;
                currentItem.NotifyOfPropertyChange("IsSelected");
            }
            else
            {
                _emptyBackground.IsSelected = false;
                _emptyBackground.NotifyOfPropertyChange("IsSelected");
            }

            item.IsSelected = true;
            item.NotifyOfPropertyChange("IsSelected");
            StateService.CurrentBackground = item;
        }

        public void Handle(DownloadableItem item)
        {
            if (item.Owner is TLWallPaper)
            {
                BeginOnUIThread(() =>
                {
                    foreach (var backgroundItem in Static)
                    {
                        if (backgroundItem.Wallpaper == item.Owner)
                        {
                            backgroundItem.NotifyOfPropertyChange("Self");
                            break;
                        }
                    }
                });
            }
        }
    }
    
    public class BackgroundItem : INotifyPropertyChanged
    {
        public string Name { get; set; }

        public string SourceString { get; set; }

        public string IsoFileName { get; set; }

        [IgnoreDataMember]
        public TLWallPaper Wallpaper { get; set; }

        public BackgroundItem Self { get { return this; } }

        public string ThemeSourceString
        {
            get
            {
                if (string.Equals(Name, "Library"))
                {
                    return SourceString;
                }

                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                var themeString = "Dark/";
                if (isLightTheme)
                {
                    themeString = "Light/";
                }

                return SourceString + themeString + Name;
            }
        }

        public bool IsSelected { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void NotifyOfPropertyChange(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

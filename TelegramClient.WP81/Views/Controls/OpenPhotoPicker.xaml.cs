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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Storage;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using Telegram.Logs;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Controls
{
    public partial class OpenPhotoPicker
    {
        public bool IsSingleItem { get; set; }

        private bool _isManipulating;
        
        private bool _hasManipulatingDelta;

        private readonly Dictionary<Picture, PhotoFile> _selectedPictures = new Dictionary<Picture, PhotoFile>(); 

        private PictureAlbumCollection _allAlbums;

        public event EventHandler<PickEventArgs> Pick;

        protected virtual void RaisePick(PickEventArgs e)
        {
            var handler = Pick;
            if (handler != null) handler(this, e);
        }

        public OpenPhotoPicker()
        {
            InitializeComponent();

            InitilalizeMediaLibrary();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                ((SolidColorBrush) Resources["BackgroundBrush"]).Color = ((SolidColorBrush) Resources["LightThemeBackgroundBrush"]).Color;
            }
            else
            {
                ((SolidColorBrush)Resources["BackgroundBrush"]).Color = ((SolidColorBrush)Resources["DarkThemeBackgroundBrush"]).Color;
            }

            Loaded += OnLoaded;
        }

        ~OpenPhotoPicker()
        {

        }

        private TelegramAppBarButton _selectButton;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            _selectButton = AppBar.Buttons[0] as TelegramAppBarButton;

            BeginOpenStoryboard();
        }

        private void InitilalizeMediaLibrary()
        {
            MediaLibrary ml = null;

            foreach (MediaSource source in MediaSource.GetAvailableMediaSources())
            {
                if (source.MediaSourceType == MediaSourceType.LocalDevice)
                {
                    ml = new MediaLibrary(source);
                    _allAlbums = ml.RootPictureAlbum.Albums;
                    _enumerator = _allAlbums.GetEnumerator();
                    if (_enumerator.MoveNext())
                    {
                        CurrentAlbum = _enumerator.Current;
                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            MakePhotoAlbum(_enumerator.Current);
                        });
                    }

                    Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                    {
                        if (Folders.ItemsSource == null)
                        {
                            _albums = ml.RootPictureAlbum.Albums.ToList();
                            Folders.ItemsSource = ml.RootPictureAlbum.Albums.ToList();
                            Folders.SelectedIndex = 0;
                        }
                    });
                }
            }
        }

        public static readonly DependencyProperty CurrentAlbumProperty = DependencyProperty.Register(
            "CurrentAlbum", typeof (PictureAlbum), typeof (OpenPhotoPicker), new PropertyMetadata(default(PictureAlbum)));

        public PictureAlbum CurrentAlbum
        {
            get { return (PictureAlbum) GetValue(CurrentAlbumProperty); }
            set { SetValue(CurrentAlbumProperty, value); }
        }

        private PictureAlbum _album;
        
        private void MakePhotoAlbum(PictureAlbum album)
        {
            PhotoRow currentRow = null;
            var stopwatch = Stopwatch.StartNew();
            var photoFiles = new List<PhotoFile>();
            var photoRow = new ObservableCollection<PhotoRow>();
            var secondSlice = new List<PhotoFile>();
            _album = album;
            if (_album.Pictures.Count > 0)
            {
                var cropedFiles = _album.Pictures.OrderByDescending(x => x.Date).ToList();
                var maxCount = 12;
                for (var i = 0; i < cropedFiles.Count; i++)
                {
                    var p = cropedFiles[i];
                    var photoFile = new PhotoFile { Picture = p, SuppressAnimation = true, IsSelected = _selectedPictures.ContainsKey(p) };

                    if (i < maxCount)
                    {
                        if (i % 3 == 0)
                        {
                            currentRow = new PhotoRow();
                            photoRow.Add(currentRow);
                        }
                        Stream thumbnail;
                        try
                        {
                            thumbnail = p.GetThumbnail();
                        }
                        catch (Exception e)
                        {
                            Log.Write(string.Format("OpenPhotoPicker File getThumbnail exception album={0} file={1}\n{2}", album.Name, p.Name, e));
                            cropedFiles.RemoveAt(i--);
                            continue;
                        }

                        photoFile.IsForeground = true;
                        photoFile.Thumbnail = new WeakReference<Stream>(thumbnail);
                        photoFiles.Add(photoFile);
                        currentRow.Add(photoFile);
                        photoFile.Row = currentRow;
                    }
                    else
                    {
                        secondSlice.Add(photoFile);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine(stopwatch.Elapsed);

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                CurrentAlbum = album;
                if (photoRow.Count < 4)
                {
                    Photos.VerticalAlignment = VerticalAlignment.Bottom;
                }
                else
                {
                    Photos.VerticalAlignment = VerticalAlignment.Stretch;
                }
                Photos.ItemsSource = photoRow;
                //LayoutRoot.Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));


                Photos.Visibility = Visibility.Visible;
                Photos.Opacity = 0.0;

                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    Photos.Opacity = 1.0;
                    var storyboard = new Storyboard();
                    var translateAnimaiton = new DoubleAnimationUsingKeyFrames();
                    translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = Photos.ActualHeight });
                    translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0.0, EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut } });
                    Storyboard.SetTarget(translateAnimaiton, Photos);
                    Storyboard.SetTargetProperty(translateAnimaiton, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                    storyboard.Children.Add(translateAnimaiton);

                    storyboard.Begin();

                    if (secondSlice.Count > 0)
                    {
                        storyboard.Completed += (o, e) =>
                        {
                            for (var i = 0; i < secondSlice.Count; i++)
                            {
                                if (i % 3 == 0)
                                {
                                    currentRow = new PhotoRow();
                                    photoRow.Add(currentRow);
                                }

                                photoFiles.Add(secondSlice[i]);
                                currentRow.Add(secondSlice[i]);
                                secondSlice[i].Row = currentRow;
                            }
                        };
                    }
                });
            });
        }

        private void PhotoControl_OnTap(object sender, GestureEventArgs e)
        {
            var control = sender as PhotoControl;
            if (control != null)
            {
                var file1 = control.File;
                if (file1 != null)
                {
                    ChangePictureSelection(file1, !file1.IsSelected);
                }
            }
        }

        private PhotoFile _previousFile;

        private void ChangePictureSelection(PhotoFile file1, bool isSelected)
        {
            var index = 0;
            // select middle item first of all
            if (!IsSingleItem
                && _hasManipulatingDelta
                && _previousFile != null
                && _previousFile.Row == file1.Row
                && file1.Row.File2.IsSelected != isSelected)
            {
                var row = file1.Row;
                if (
                    (row.File1 == _previousFile && row.File3 == file1)
                    || (row.File3 == _previousFile && row.File1 == file1))
                {
                    if (isSelected)
                    {
                        _selectedPictures[row.File2.Picture] = row.File2;

                        index = _selectedPictures.Count;
                    }
                    else
                    {
                        _selectedPictures.Remove(row.File2.Picture);

                        foreach (var photoFile in _selectedPictures.Values)
                        {
                            if (photoFile.Index > row.File2.Index)
                            {
                                photoFile.Index--;
                                photoFile.RaisePropertyChanged("Index");
                            }
                        }

                        index = 0;
                    }

                    row.File2.SuppressAnimation = false;
                    row.File2.IsSelected = isSelected;
                    row.File2.RaisePropertyChanged("IsSelected");
                    row.File2.Index = index;
                    row.File2.RaisePropertyChanged("Index");
                }
            }


            if (isSelected)
            {
                _selectedPictures[file1.Picture] = file1;

                index = _selectedPictures.Count;
            }
            else
            {
                _selectedPictures.Remove(file1.Picture);

                foreach (var photoFile in _selectedPictures.Values)
                {
                    if (photoFile.Index > file1.Index)
                    {
                        photoFile.Index--;
                        photoFile.RaisePropertyChanged("Index");
                    }
                }

                index = 0;
            }

            file1.SuppressAnimation = false;
            file1.IsSelected = isSelected;
            file1.RaisePropertyChanged("IsSelected");
            file1.Index = index;
            file1.RaisePropertyChanged("Index");

            _previousFile = file1;

            _selectButton.IsEnabled = _selectedPictures.Any();

            if (IsSingleItem)
            {
                ChooseButton_OnClick(null, null);
                LayoutRoot.IsHitTestVisible = false;
            }
        }

        private void Photos_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _hasManipulatingDelta = false;
        }

        private void Photos_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _hasManipulatingDelta = false;
        }

        private bool _isSelected;
        private IEnumerator<PictureAlbum> _enumerator;

        private void Photos_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (IsSingleItem) return;

            if (!_hasManipulatingDelta)
            {
                var grid = e.OriginalSource as Grid;

                if (grid != null)
                {
                    var control = grid.Parent as PhotoControl;
                    if (control != null)
                    {
                        var file1 = control.File;
                        if (file1 != null)
                        {
                            ChangePictureSelection(file1, !file1.IsSelected);
                            _isSelected = file1.IsSelected;
                        }
                    }
                }
            }

            _hasManipulatingDelta = true;
        }

        private void PhotoControl_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!_hasManipulatingDelta) return;

            var control = sender as PhotoControl;
            if (control != null)
            {
                var file1 = control.File;
                if (file1 != null && file1.IsSelected != _isSelected)
                {
                    ChangePictureSelection(file1, _isSelected);
                }
            }
        }

        public event EventHandler Close;

        protected virtual void RaiseClose()
        {
            EventHandler handler = Close;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            BeginCloseStoryboard(RaiseClose);
            //RaiseClose();
        }

        public async void StartPreview()
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            var deviceInfo = devices[0]; //grab first result
            DeviceInformation rearCamera = null;
            foreach (var device in devices)
            {
                if (device.Name.ToLowerInvariant().Contains("front"))
                {
                    DeviceInformation frontCamera;
                    deviceInfo = frontCamera = device;
                    var hasFrontCamera = true;
                }
                if (device.Name.ToLowerInvariant().Contains("back"))
                {
                    rearCamera = device;
                }
            }

            var mediaSettings = new MediaCaptureInitializationSettings
            {
                MediaCategory = MediaCategory.Communications,
                StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
                VideoDeviceId = rearCamera.Id
            };

            var mediaCaptureManager = new Windows.Media.Capture.MediaCapture();
            await mediaCaptureManager.InitializeAsync(mediaSettings);

            var previewSink = new Windows.Phone.Media.Capture.MediaCapturePreviewSink();

            // List of supported video preview formats to be used by the default preview format selector.
            var supportedVideoFormats = new List<string> { "nv12", "rgb32" };

            // Find the supported preview format
            var availableMediaStreamProperties =
                mediaCaptureManager.VideoDeviceController.GetAvailableMediaStreamProperties(
                Windows.Media.Capture.MediaStreamType.VideoPreview)
                    .OfType<Windows.Media.MediaProperties.VideoEncodingProperties>()
                    .Where(p => p != null
                        && !String.IsNullOrEmpty(p.Subtype)
                        && supportedVideoFormats.Contains(p.Subtype.ToLower()))
                    .ToList();
            var previewFormat = availableMediaStreamProperties.FirstOrDefault();
            foreach (var property in availableMediaStreamProperties)
            {
                if (previewFormat.Width < property.Width)
                {
                    previewFormat = property;
                }
            }
            //previewFormat.Width = 480;
            //previewFormat.Height = 480;
            // Start Preview stream
            await mediaCaptureManager.VideoDeviceController.SetMediaStreamPropertiesAsync(Windows.Media.Capture.MediaStreamType.VideoPreview, previewFormat);
            await mediaCaptureManager.StartPreviewToCustomSinkAsync(new Windows.Media.MediaProperties.MediaEncodingProfile { Video = previewFormat }, previewSink);

            var viewfinderBrush = new VideoBrush{ Stretch = Stretch.Uniform };
            // Set the source of the VideoBrush used for your preview
            Microsoft.Devices.CameraVideoBrushExtensions.SetSource(viewfinderBrush, previewSink);
            CameraPreview.Background = viewfinderBrush;


            mediaCaptureManager.SetPreviewRotation(VideoRotation.Clockwise90Degrees);
        }

        private async void ChooseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedPhoto = _selectedPictures.Values.OrderBy(x => x.Index).ToList();

            var picturesLibrary = KnownFolders.PicturesLibrary;
            if (picturesLibrary != null)
            {
                var folder = await picturesLibrary.GetFolderAsync(_album.Name);
                var folders = await picturesLibrary.GetFoldersAsync();

                if (folder != null)
                {
                    var getFileOperations = new List<Task<StorageFile>>();

                    foreach (var file in selectedPhoto)
                    {
                        getFileOperations.Add(Task.Run(async () =>
                        {
                            var f = new List<StorageFolder>();
                            foreach (var storageFolder in folders)
                            {
                                if (storageFolder.Name == file.Picture.Album.Name)
                                {
                                    f.Add(storageFolder);
                                    try
                                    {
                                        var returnFile = await storageFolder.GetFileAsync(file.Picture.Name);
                                        return returnFile;
                                    }
                                    catch (Exception ex)
                                    {
                                    }
                                }
                            }

                            string filePath = string.Empty;
                            try
                            {
                                filePath = file.Picture.GetPath();
                            }
                            catch (Exception ex)
                            {
                                
                            }

                            Log.Write(string.Format("OpenPhotoPicker File '{0}' is missing at folder '{1}'\nPath={2}\nFolders\n{3}", file.Picture.Name, file.Picture.Album, filePath, string.Join(Environment.NewLine, folders.Select(x => string.Format("Name='{0}' DisplayName='{1}' Path='{2}'", x.Name, x.DisplayName, x.Path)))));
                            
                            try
                            {
                                using (var imageStream = file.Picture.GetImage())
                                {
                                    var localFileName = string.Format("{0}_{1}_{2}", imageStream.Length, file.Picture.Album.Name, file.Picture.Name);
                                    try
                                    {
                                        var localFile = await ApplicationData.Current.LocalFolder.GetFileAsync(localFileName);
                                        return localFile;
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    var newFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(localFileName, CreationCollisionOption.ReplaceExisting);
                                    using (Stream outputStream = await newFile.OpenStreamForWriteAsync())
                                    {
                                        await imageStream.CopyToAsync(outputStream);
                                    }
                                    return newFile;
                                }
                            }
                            catch (Exception ex)
                            {
                                
                            }

                            try
                            {
                                using (var imageStream = file.Picture.GetThumbnail())
                                {
                                    var localFileName = string.Format("{0}_{1}_{2}", imageStream.Length, file.Picture.Album.Name, file.Picture.Name);
                                    try
                                    {
                                        var localFile = await ApplicationData.Current.LocalFolder.GetFileAsync(localFileName);
                                        return localFile;
                                    }
                                    catch (Exception ex)
                                    {

                                    }

                                    var newFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(localFileName, CreationCollisionOption.ReplaceExisting);
                                    using (Stream outputStream = await newFile.OpenStreamForWriteAsync())
                                    {
                                        await imageStream.CopyToAsync(outputStream);
                                    }
                                    return newFile;
                                }
                            }
                            catch (Exception ex)
                            {

                            }

                            return null;
                        }));
                    }
                    // wait for all operations in parallel
                    var result = await Task.WhenAll(getFileOperations);
                    var files = new List<StorageFile>(); 
                    var photoFiles = new List<PhotoFile>();

                    for (var i = 0; i < result.Length; i++)
                    {
                        if (result[i] != null)
                        {
                            files.Add(result[i]);
                            photoFiles.Add(selectedPhoto[i]);
                        }
                    }

                    RaisePick(new PickEventArgs{ Files = new ReadOnlyCollection<StorageFile>(files), PhotoFiles = new ReadOnlyCollection<PhotoFile>(photoFiles)} );
                    //MessageBox.Show("Files selected: " + files.Count(x => x != null));
                    System.Diagnostics.Debug.WriteLine(files.Count());
                }
            }
        }

        private Popup _popup;
        private List<PictureAlbum> _albums;

        private void ChangeButton_OnClick(object sender, RoutedEventArgs e)
        {

            return;

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                if (_enumerator.MoveNext())
                {
                    MakePhotoAlbum(_enumerator.Current);
                }
                else
                {
                    _enumerator.Reset();
                    if (_enumerator.MoveNext())
                    {
                        MakePhotoAlbum(_enumerator.Current);
                    }
                }
            });
        }

        private void BeginOpenStoryboard()
        {
            LayoutRoot.IsHitTestVisible = true;
            Bar.Visibility = Visibility.Collapsed;

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            var storyboard = new Storyboard();
            var translateAnimaiton = new DoubleAnimationUsingKeyFrames();
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0.0, EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut } });
            Storyboard.SetTarget(translateAnimaiton, Bar);
            Storyboard.SetTargetProperty(translateAnimaiton, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateAnimaiton);

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                Bar.Visibility = Visibility.Visible;
                storyboard.Begin();
            });
        }

        private void BeginCloseStoryboard(Action callback)
        {
            LayoutRoot.IsHitTestVisible = false;
            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, Photos);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateImageAniamtion);

            var translateBarAniamtion = new DoubleAnimationUsingKeyFrames();
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0 });
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateBarAniamtion, Bar);
            Storyboard.SetTargetProperty(translateBarAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateBarAniamtion);

            storyboard.Begin();
            if (callback != null)
            {
                storyboard.Completed += (o, e) => callback();
            }
        }

        private void Album_OnTap(object sender, GestureEventArgs e)
        {
            //ToggleFoldersVisibility();

            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement != null)
            {
                var album = frameworkElement.DataContext as PictureAlbum;
                if (album != null)
                {
                    Folders.SelectedItem = album;

                    Folders.UpdateLayout();
                    Folders.ScrollIntoView(album);


                    Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                    {
                        MakePhotoAlbum(album);
                    });
                }
            }
        }

        public bool TryClose()
        {
            BeginCloseStoryboard(RaiseClose);

            return true;
        }

        private void Folders_OnTap(object sender, GestureEventArgs e)
        {
            e.Handled = true;
        }

        private void Folders_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

    public class PhotoRow
    {
        public PhotoFile File1 { get; set; }
        public PhotoFile File2 { get; set; }
        public PhotoFile File3 { get; set; }

        public void Add(PhotoFile file)
        {
            if (File1 == null) File1 = file;
            else if (File2 == null) File2 = file;
            else if (File3 == null) File3 = file;
        }
    }

    public class PhotoFile : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public bool IsSelected { get; set; }

        public WeakReference<Stream> Thumbnail { get; set; }
        public Picture Picture { get; set; }
        public PhotoFile Self { get { return this; } }
        public bool IsForeground { get; set; }
        public PhotoRow Row { get; set; }

        public bool SuppressAnimation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PickEventArgs
    {
        public ReadOnlyCollection<StorageFile> Files { get; set; }

        public ReadOnlyCollection<PhotoFile> PhotoFiles { get; set; } 
    }

    public class PhotoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var photoFile = value as PhotoFile;
            if (photoFile != null)
            {
                var picture = photoFile.Picture;
                Stream thumbnail = null;

                if (photoFile.Thumbnail == null
                    || !photoFile.Thumbnail.TryGetTarget(out thumbnail))
                {
                    Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                    {
                        thumbnail = picture.GetThumbnail();
                        photoFile.Thumbnail = new WeakReference<Stream>(thumbnail);

                        Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                        {
                            photoFile.RaisePropertyChanged("Self");
                        });
                    });

                    return null;
                }

                var b = new BitmapImage();
                if (!photoFile.IsForeground)
                {
                    b.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                }

                b.SetSource(thumbnail);

                return b;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

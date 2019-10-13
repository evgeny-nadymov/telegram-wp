// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System;
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using TelegramClient.Converters;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Dialogs;

namespace TelegramClient.ViewModels.Media
{
    public class EditVideoViewModel : ViewAware
    {
        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyOfPropertyChange(propertyName);
            return true;
        }

        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyOfPropertyChange(selectorExpression);
            return true;
        }

        public TimerSpan SelectedQuality { get; set; }

        public Uri QualityButtonImageSource
        {
            get
            {
                if (SelectedQuality != null)
                {
                    if (SelectedQuality.Seconds == 0)
                    {
                        return new Uri("/Images/W10M/ic_quality_auto_2x.png", UriKind.Relative);
                    }
                    if (SelectedQuality.Seconds <= 240)
                    {
                        return new Uri("/Images/W10M/ic_quality240_2x.png", UriKind.Relative);
                    }
                    if (SelectedQuality.Seconds <= 360)
                    {
                        return new Uri("/Images/W10M/ic_quality360_2x.png", UriKind.Relative);
                    }
                    if (SelectedQuality.Seconds <= 480)
                    {
                        return new Uri("/Images/W10M/ic_quality480_2x.png", UriKind.Relative);
                    }
                    if (SelectedQuality.Seconds <= 720)
                    {
                        return new Uri("/Images/W10M/ic_quality720_2x.png", UriKind.Relative);
                    }
                    if (SelectedQuality.Seconds <= 1080)
                    {
                        return new Uri("/Images/W10M/ic_quality1080_2x.png", UriKind.Relative);
                    }
                }

                return new Uri("/Images/W10M/ic_quality_auto_2x.png", UriKind.Relative);
            }
        }

        public List<TimerSpan> QualityList { get; set; }

        private string _caption;

        public string Caption
        {
            get { return _caption; }
            set { SetField(ref _caption, value, () => Caption); }
        }

        public Visibility TimerButtonVisibility
        {
            get { return _with is TLUser ? Visibility.Visible : Visibility.Collapsed; }
        }

        public TimerSpan TimerSpan { get; set; }

        private TimeSpan? _trimLeft;

        public TimeSpan? TrimLeft
        {
            get { return _trimLeft; }
            set { SetField(ref _trimLeft, value, () => TrimLeft); }
        }

        private TimeSpan? _trimRight;

        public TimeSpan? TrimRight
        {
            get { return _trimRight; }
            set { SetField(ref _trimRight, value, () => TrimRight); }
        }

        private bool _compression = false;

        public bool Compression
        {
            get { return _compression; }
            set { SetField(ref _compression, value, () => Compression); }
        }

        private bool _mute = true;

        public bool Mute
        {
            get { return _mute; }
            set { SetField(ref _mute, value, () => Mute); }
        }

        public Visibility TimerVisibility { get; set; }

        public ulong Size { get; set; }

        public ulong EditedSize { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public VideoOrientation Orientation { get; set; }

        public TimeSpan Duration { get; set; }

        public string DurationString { get; set; }

        public StorageFile VideoFile { get; set; }

        public TLPhotoSizeBase ThumbPhoto { get; set; }

        private TLPhotoSizeBase _previewPhoto;

        public TLPhotoSizeBase PreviewPhoto
        {
            get { return _previewPhoto; }
            set { _previewPhoto = value; }
        }

        public string OriginalVideoParameters { get; protected set; }

        public string EditedVideoParameters { get; protected set; }

        private bool _isMuteEnabled;

        public bool IsMuteEnabled
        {
            get { return _isMuteEnabled; }
            set { SetField(ref _isMuteEnabled, value, () => IsMuteEnabled); }
        }

        private bool _isOpen;

        public bool IsOpen
        {
            get { return _isOpen; }
            set { SetField(ref _isOpen, value, () => IsOpen); }
        }

        private Tuple<BasicProperties, VideoProperties, MusicProperties> _properties;

        public readonly Action<CompressingVideoFile> _sendVideoAction;

        private readonly Func<string, IList<TLUserBase>> _getUsernameHints;

        public readonly TLObject _with;

        public EditVideoViewModel(Action<CompressingVideoFile> sendVideoAction, Func<string, IList<TLUserBase>> getUsernameHints, TLObject with)
        {
            _with = with;
            _getUsernameHints = getUsernameHints;
            _sendVideoAction = sendVideoAction;

            PropertyChanged += (sender, args) =>
            {
                if (Property.NameEquals(args.PropertyName, () => SelectedQuality))
                {
                    UpdateEditedVideoParameters();

                    NotifyOfPropertyChange(() => EditedVideoParameters);
                }
                else if (Property.NameEquals(args.PropertyName, () => IsMuteEnabled))
                {
                    UpdateEditedVideoParameters();

                    NotifyOfPropertyChange(() => EditedVideoParameters);
                }
                else if (Property.NameEquals(args.PropertyName, () => Caption))
                {
                    LoadMentionHints(Caption);
                }
            };
        }

        #region Mentions

        private IList<TLUserBase> _mentions;

        public void ContinueLoadMentionHints()
        {
            if (!string.IsNullOrEmpty(Caption))
            {
                var cachedResult = _getUsernameHints.Invoke(Caption);
                if (cachedResult.Count > MaxResults)
                {
                    CreateUsernameHints();

                    if (UsernameHints.Hints.Count == MaxResults)
                    {
                        var lastItem = UsernameHints.Hints.LastOrDefault();
                        if (lastItem != null)
                        {
                            var lastIndex = cachedResult.IndexOf(lastItem);
                            if (lastIndex >= 0)
                            {
                                for (var i = lastIndex + 1; i < cachedResult.Count; i++)
                                {
                                    UsernameHints.Hints.Add(cachedResult[i]);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void LoadMentionHints(string text)
        {
            if (_getUsernameHints != null)
            {
                var cachedResult = _getUsernameHints.Invoke(text);
                if (cachedResult.Count > 0)
                {
                    CreateUsernameHints();

                    ClearUsernameHints();
                    for (var i = 0; i < cachedResult.Count; i++)
                    {
                        if (UsernameHints.Hints.Count == MaxResults) break;

                        UsernameHints.Hints.Add(cachedResult[i]);
                    }
                }
                else
                {
                    ClearUsernameHints();
                }
            }
        }

        public void AddMention(TLUserBase userBase)
        {
            _mentions = _mentions ?? new List<TLUserBase>();
            _mentions.Add(userBase);
        }

        private const int MaxResults = 10;

        public UsernameHintsViewModel UsernameHints { get; protected set; }

        private void CreateUsernameHints()
        {
            if (UsernameHints == null)
            {
                UsernameHints = new UsernameHintsViewModel();
                NotifyOfPropertyChange(() => UsernameHints);
            }
        }

        private void ClearUsernameHints()
        {
            if (UsernameHints != null)
            {
                UsernameHints.Hints.Clear();
            }
        }
        #endregion

        public void SetVideoFile(StorageFile file)
        {
            _mentions = null;
            _encodingProfile = null;
            VideoFile = file;
            NotifyOfPropertyChange(() => VideoFile);
            PreviewPhoto = null;
            NotifyOfPropertyChange(() => PreviewPhoto);
            Caption = string.Empty;
            Compression = false;
            Mute = true;
            IsMuteEnabled = false;
            SelectedQuality = null;
            QualityList = null;
            TimerSpan = null;

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _properties = GetProperties(VideoFile).Result;

                var properties = _properties.Item1;
                var videoProperties = _properties.Item2;

                Size = properties.Size;
                Duration = videoProperties.Duration;
                Orientation = videoProperties.Orientation;
                Width = Orientation == VideoOrientation.Normal || Orientation == VideoOrientation.Rotate180 ? videoProperties.Width : videoProperties.Height;
                Height = Orientation == VideoOrientation.Normal || Orientation == VideoOrientation.Rotate180 ? videoProperties.Height : videoProperties.Width;

                DurationString = GetDurationString(Duration);
                var originalSizeString = FileSizeConverter.Convert((long)properties.Size);

                OriginalVideoParameters = string.Format("{0}x{1}, {2}, {3}", Width, Height, DurationString, originalSizeString);

                var minLength = Math.Min(Width, Height);
                SelectedQuality = GetSelectedQuality(minLength);
                QualityList = GetQualityList(minLength);

                UpdateEditedVideoParameters();

                var tuple = GetFilePreviewAndThumbAsync(VideoFile).Result;
                PreviewPhoto = tuple.Item1;
                ThumbPhoto = tuple.Item2;

                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    NotifyOfPropertyChange(() => SelectedQuality);
                    NotifyOfPropertyChange(() => QualityButtonImageSource);
                    NotifyOfPropertyChange(() => DurationString);
                    NotifyOfPropertyChange(() => OriginalVideoParameters);
                    if (QualityList.Count > 1)
                    {
                        Compression = true;
                    }

                    NotifyOfPropertyChange(() => EditedVideoParameters);
                    NotifyOfPropertyChange(() => PreviewPhoto);

                    var musicProperties = _properties.Item3;
                    if (musicProperties == null || musicProperties.Bitrate == 0)
                    {
                        IsMuteEnabled = true;
                        Mute = false;
                    }
                });
            });
        }

        private TimerSpan GetSelectedQuality(double minLength)
        {
            if (minLength > 360)
            {
                return new TimerSpan(string.Empty, "360", 360);
            }

            return new TimerSpan(string.Empty, AppResources.Auto, 0);
        }

        private List<TimerSpan> GetQualityList(double minLength)
        {
            var list = new List<TimerSpan> { new TimerSpan(string.Empty, AppResources.Auto, 0) };
            if (minLength > 240)
            {
                list.Add(new TimerSpan(string.Empty, "240", 240));
            }
            if (minLength > 360)
            {
                list.Add(new TimerSpan(string.Empty, "360", 360));
            }
            //if (minLength > 480)
            //{
            //    list.Add(new TimerSpan(string.Empty, "480", 480));
            //}
            if (minLength > 720)
            {
                list.Add(new TimerSpan(string.Empty, "720", 720));
            }
            if (minLength > 1080)
            {
                list.Add(new TimerSpan(string.Empty, "1080", 1080));
            }

            return list;
        }

        private async Task<Tuple<BasicProperties, VideoProperties, MusicProperties>> GetProperties(StorageFile videoFile)
        {
            var properties = await videoFile.GetBasicPropertiesAsync();
            var videoProperties = await videoFile.Properties.GetVideoPropertiesAsync();
            var musicProperties = await videoFile.Properties.GetMusicPropertiesAsync();

            return new Tuple<BasicProperties, VideoProperties, MusicProperties>(properties, videoProperties, musicProperties);
        }

        public void UpdateEditedVideoDuration()
        {
            var duration = GetEditedDuration(TrimLeft, TrimRight, Duration);
            if (IsMuteEnabled)    // gif
            {
                EditedSize = (ulong)(_editedBitrate * duration.TotalSeconds) / 8;
            }
            else if (SelectedQuality != null && SelectedQuality.Seconds > 0)    // compressed quality
            {
                EditedSize = (ulong)(_editedBitrate * duration.TotalSeconds) / 8;
            }
            else    // default quality
            {
                EditedSize = (ulong)(Size / Duration.TotalSeconds * duration.TotalSeconds);
            }

            var editedSizeString = FileSizeConverter.Convert((long)EditedSize);

            EditedVideoParameters = string.Format("{0}x{1}, {2}, ~{3}", _editedWidth, _editedHeight, GetDurationString(duration), editedSizeString);
            NotifyOfPropertyChange(() => EditedVideoParameters);
        }

        private void UpdateEditedVideoParameters()
        {
            var minLength = Math.Min(Width, Height);
            var scaleFactor = SelectedQuality != null && SelectedQuality.Seconds != 0 ? SelectedQuality.Seconds / minLength : 1.0;
            var duration = GetEditedDuration(TrimLeft, TrimRight, Duration);
            _editedBitrate = SetEditedBitrate();

            if (IsMuteEnabled)    // gif
            {
                EditedSize = (ulong)(_editedBitrate * duration.TotalSeconds) / 8;
            }
            else if (SelectedQuality != null && SelectedQuality.Seconds > 0)    // compressed quality
            {
                EditedSize = (ulong)(_editedBitrate * duration.TotalSeconds) / 8;
            }
            else    // default quality
            {
                EditedSize = (ulong)(Size / Duration.TotalSeconds * duration.TotalSeconds);
            }

            var editedSizeString = FileSizeConverter.Convert((long)EditedSize);
            _editedHeight = (uint)(Height * scaleFactor);
            _editedWidth = (uint)(Width * scaleFactor);
            if (_encodingProfile != null)
            {
                _encodingProfile.Video.Height = _properties.Item2.Orientation == VideoOrientation.Normal || _properties.Item2.Orientation == VideoOrientation.Rotate180? _editedHeight : _editedWidth;
                _encodingProfile.Video.Width = _properties.Item2.Orientation == VideoOrientation.Normal || _properties.Item2.Orientation == VideoOrientation.Rotate180 ? _editedWidth : _editedHeight;
            }

            EditedVideoParameters = string.Format("{0}x{1}, {2}, ~{3}", _editedWidth, _editedHeight, GetDurationString(duration), editedSizeString);
        }

        private MediaEncodingProfile _encodingProfile;
        private ulong _editedBitrate;
        private uint _editedHeight;
        private uint _editedWidth;

        private ulong SetEditedBitrate()
        {
            ulong bitrate = 0;
            if (IsMuteEnabled)
            {
                int targetVideoBitrate = 900000;
                _encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga);
                var minLength = Math.Min(Width, Height);
                var scaleFactor = SelectedQuality != null && SelectedQuality.Seconds != 0 ? SelectedQuality.Seconds / minLength : 1.0;
                var originalVideoBitrate = _properties != null && _properties.Item2 != null
                    ? _properties.Item2.Bitrate
                    : uint.MaxValue;
                _encodingProfile.Video.Bitrate = (uint)Math.Min(targetVideoBitrate, (int)(originalVideoBitrate * scaleFactor));
                _encodingProfile.Audio = null;
                bitrate += _encodingProfile.Video.Bitrate;
            }
            else if (SelectedQuality != null && SelectedQuality.Seconds > 0)
            {
                int targetVideoBitrate = 0;
                int targetAudioBitrate = 90000;
                switch (SelectedQuality.Seconds)
                {
                    case 240:
                        _encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Qvga);
                        targetVideoBitrate = 400000;
                        break;
                    case 360:
                        _encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga);
                        targetVideoBitrate = 900000;
                        break;
                    case 480:
                        _encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga);
                        targetVideoBitrate = 900000;
                        break;
                    case 720:
                        _encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p);
                        targetVideoBitrate = 1100000;
                        break;
                    default:
                        _encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);
                        targetVideoBitrate = 1600000;
                        break;
                }

                var minLength = Math.Min(Width, Height);
                var scaleFactor = SelectedQuality != null && SelectedQuality.Seconds != 0 ? SelectedQuality.Seconds / minLength : 1.0;
                var originalVideoBitrate = _properties != null && _properties.Item2 != null
                    ? _properties.Item2.Bitrate
                    : uint.MaxValue;
                _encodingProfile.Video.Bitrate = (uint)Math.Min(targetVideoBitrate, (int)(originalVideoBitrate * scaleFactor));
                bitrate += _encodingProfile.Video.Bitrate;
                if (!IsMuteEnabled)
                {
                    var originalAudioBitrate = _properties != null && _properties.Item3 != null
                    ? _properties.Item3.Bitrate
                    : uint.MaxValue;
                    _encodingProfile.Audio.ChannelCount = 1;
                    _encodingProfile.Audio.Bitrate = (uint)Math.Min(targetAudioBitrate, originalAudioBitrate);
                    bitrate += _encodingProfile.Audio.Bitrate;
                }
                else
                {
                    _encodingProfile.Audio = null;
                }
            }
            else
            {
                _encodingProfile = null;
                if (_properties != null && _properties.Item2 != null)
                {
                    bitrate += _properties.Item2.Bitrate;
                }
                if (_properties != null && _properties.Item3 != null && !IsMuteEnabled)
                {
                    bitrate += _properties.Item3.Bitrate;
                }
            }

            return bitrate;
        }



        public void OpenVideo()
        {
            Launcher.LaunchFileAsync(VideoFile);
        }

        private static async Task<Tuple<TLPhotoSizeBase, TLPhotoSizeBase>> GetFilePreviewAndThumbAsync(StorageFile file)
        {
            try
            {
                var preview = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 480, ThumbnailOptions.ResizeThumbnail);

                var thumbLocation = new TLFileLocation
                {
                    DCId = new TLInt(0),
                    VolumeId = TLLong.Random(),
                    LocalId = TLInt.Random(),
                    Secret = TLLong.Random(),
                };

                var thumbFileName = String.Format("{0}_{1}_{2}.jpg",
                    thumbLocation.VolumeId,
                    thumbLocation.LocalId,
                    thumbLocation.Secret);

                var previewLocation = new TLFileLocation
                {
                    DCId = new TLInt(0),
                    VolumeId = TLLong.Random(),
                    LocalId = TLInt.Random(),
                    Secret = TLLong.Random(),
                };

                var previewFileName = String.Format("{0}_{1}_{2}.jpg",
                    previewLocation.VolumeId,
                    previewLocation.LocalId,
                    previewLocation.Secret);

                var previewFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(previewFileName, CreationCollisionOption.ReplaceExisting);
                var previewBuffer = new Windows.Storage.Streams.Buffer(Convert.ToUInt32(preview.Size));
                var iBuf = await preview.ReadAsync(previewBuffer, previewBuffer.Capacity, InputStreamOptions.None);

                var filePreview = new TLPhotoSize
                {
                    W = new TLInt((int) preview.OriginalWidth),
                    H = new TLInt((int) preview.OriginalHeight),
                    Size = new TLInt((int) preview.Size),
                    Type = TLString.Empty,
                    Location = previewLocation,
                };

                Photo thumb;
                using (var previewStream = await previewFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await previewStream.WriteAsync(iBuf);

                    thumb = await ChooseAttachmentViewModel.ResizeJpeg(previewStream, 90, thumbFileName);
                }

                var thumbFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(thumbFileName, CreationCollisionOption.ReplaceExisting);
                var iBuf2 = thumb.Bytes.AsBuffer();
                using (var thumbStream = await thumbFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    await thumbStream.WriteAsync(iBuf2);
                }

                var fileThumb = new TLPhotoSize
                {
                    W = new TLInt(thumb.Width),
                    H = new TLInt(thumb.Height),
                    Size = new TLInt(thumb.Bytes.Length),
                    Type = new TLString("s"),
                    Location = thumbLocation,
                };

                return new Tuple<TLPhotoSizeBase, TLPhotoSizeBase>(filePreview, fileThumb);
            }
            catch (Exception ex)
            {
                Telegram.Api.Helpers.Execute.ShowDebugMessage("GetFilePreviewAndThumbAsync exception " + ex);
            }

            return new Tuple<TLPhotoSizeBase, TLPhotoSizeBase>(null, null);
        }

        private static TimeSpan GetEditedDuration(TimeSpan? leftTrim, TimeSpan? rightTrim, TimeSpan duration)
        {
            var start = leftTrim ?? TimeSpan.Zero;
            var end = rightTrim ?? duration;

            return end - start;
        }

        private static string GetDurationString(TimeSpan duration)
        {
            if (duration.Hours > 0)
            {
                return duration.ToString(@"h\:mm\:ss");
            }

            if (duration.TotalSeconds < 1.0)
            {
                return duration.ToString(@"m\:ss\.fff");
            }

            return duration.ToString(@"m\:ss");
        }

        public void Done()
        {
            CloseEditor();

            var videoFile = new CompressingVideoFile
            {
                EncodingProfile = _encodingProfile,
                TimerSpan = TimerSpan,
                Size = EditedSize,
                Duration = GetEditedDuration(TrimLeft, TrimRight, Duration).TotalSeconds,
                Width = Width,
                Height = Height,
                Orientation = Orientation,
                Source = VideoFile,
                ThumbPhoto = ThumbPhoto,
                TrimStartTime = TrimLeft ?? TimeSpan.Zero,
                TrimStopTime = TrimRight ?? TimeSpan.Zero,
                Caption = Caption,
                Mentions = _mentions
            };

            DeletePreviewAsync();

            Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () => _sendVideoAction.SafeInvoke(videoFile));
        }

        public void Cancel()
        {
            DeletePreviewAsync();
        }

        private void DeletePreviewAsync()
        {
            var preview = PreviewPhoto as TLPhotoSize;
            if (preview != null)
            {
                var fileLocation = preview.Location as TLFileLocation;
                if (fileLocation != null)
                {
                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        fileLocation.VolumeId,
                        fileLocation.LocalId,
                        fileLocation.Secret);

                    Telegram.Api.Helpers.Execute.BeginOnThreadPool(() => FileUtils.Delete(new object(), fileName));
                }
            }
        }

        public void OpenEditor()
        {
            IsOpen = true;
        }

        public void CloseEditor()
        {
            IsOpen = false;
        }
    }

    public class CompressingVideoFile
    {
        public MediaEncodingProfile EncodingProfile { get; set; }

        public TimeSpan TrimStartTime { get; set; }

        public TimeSpan TrimStopTime { get; set; }

        public TimerSpan TimerSpan { get; set; }

        public ulong Size { get; set; }

        public double Duration { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public VideoOrientation Orientation { get; set; }

        public StorageFile Source { get; set; }

        public TLPhotoSizeBase ThumbPhoto { get; set; }

        public string Caption { get; set; }

        public IList<TLUserBase> Mentions { get; set; }
    }
}

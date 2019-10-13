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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Caliburn.Micro;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using Telegram.Logs;
using TelegramClient.Helpers;
using TelegramClient.Services;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Dialogs;
using TelegramClient.Views.Media;
using PhotoFile = TelegramClient.Services.PhotoFile;

namespace TelegramClient.ViewModels.Media
{
    public class MultiImageEditorViewModel : ViewAware
    {
        private bool _isGrouped = true;

        public bool IsGrouped
        {
            get { return _isGrouped; }
            set
            {
                if (_isGrouped != value)
                {
                    _isGrouped = value;
                    NotifyOfPropertyChange(() => IsGrouped);
                }
            }
        }

        public Visibility TimerButtonVisibility
        {
            get { return _with is TLUser? Visibility.Visible : Visibility.Collapsed; }
        }

        public Visibility GroupButtonVisibility
        {
            get
            {
                var encryptedChatBase = _with as TLEncryptedChatBase;
                if (encryptedChatBase != null)
                {
                    var encryptedChat = _with as TLEncryptedChat20;
                    if (encryptedChat != null && encryptedChat.Layer.Value >= Constants.MinSecretChatWithGroupedMediaLayer)
                    {
                        return Visibility.Visible;
                    }

                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        private IMultiImageEditorView _view;

        public IMultiImageEditorView View
        {
            get
            {
                _view = _view ?? GetView() as IMultiImageEditorView;

                return _view;
            }
        }

        public bool IsSecretChat { get; set; }

        private bool _isCaptionEnabled = true;

        public bool IsCaptionEnabled
        {
            get { return _isCaptionEnabled; }
            set { _isCaptionEnabled = value; }
        }

        private PhotoFile _currentItem;

        public PhotoFile CurrentItem
        {
            get { return _currentItem; }
            set
            {
                if (_currentItem != value)
                {
                    SwitchSelection(value, _currentItem);
                    _currentItem = value;
                    NotifyOfPropertyChange(() => CurrentItem);
                    NotifyOfPropertyChange(() => Caption);
                }
            }
        }

        private void SwitchSelection(PhotoFile currentItem, PhotoFile previousItem)
        {
            if (currentItem != null)
            {
                currentItem.IsSelected = true;
            }

            if (previousItem != null)
            {
                previousItem.IsSelected = false;
            }
        }

        public ObservableCollection<PhotoFile> Items { get; set; }

        private readonly Action<IReadOnlyList<StorageFile>> _sendPhotosAction;

        private readonly Func<string, IList<TLUserBase>> _getUsernameHints;

        private readonly TLObject _with;

        public MultiImageEditorViewModel(Action<IReadOnlyList<StorageFile>> sendPhotosAction, Func<string, IList<TLUserBase>> getUsernameHints, TLObject with)
        {
            _with = with;
            _sendPhotosAction = sendPhotosAction;
            _getUsernameHints = getUsernameHints;

            Items = new ObservableCollection<PhotoFile>();
        }

        #region Mentions

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
            if (CurrentItem == null) return;

            CurrentItem.Mentions = CurrentItem.Mentions ?? new List<TLUserBase>();
            CurrentItem.Mentions.Add(userBase);
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

        public IReadOnlyCollection<StorageFile> Files { get; set; }

        public Action<IList<PhotoFile>> ContinueAction { get; set; }

        public string Caption
        {
            get
            {
                if (_currentItem == null) return null;

                var message = _currentItem.Object as TLMessage;
                if (message != null)
                {
                    var media = message.Media as TLMessageMediaPhoto28;
                    if (media != null)
                    {
                        return message.Message.ToString();
                    }
                }

                var decryptedMessage = _currentItem.Object as TLDecryptedMessage;
                if (decryptedMessage != null)
                {
                    var media = decryptedMessage.Media as TLDecryptedMessageMediaPhoto45;
                    if (media != null)
                    {
                        return media.Caption.ToString();
                    }
                }

                return null;
            }
            set
            {
                var message = _currentItem.Object as TLMessage;
                if (message != null)
                {
                    var media = message.Media as TLMessageMediaPhoto28;
                    if (media != null)
                    {
                        if (!string.Equals(message.Message.ToString(), value))
                        {
                            message.Message = new TLString(value);

                            LoadMentionHints(value);
                        }
                    }
                }

                var decryptedMessage = _currentItem.Object as TLDecryptedMessage;
                if (decryptedMessage != null)
                {
                    var media = decryptedMessage.Media as TLDecryptedMessageMediaPhoto45;
                    if (media != null)
                    {
                        if (!string.Equals(media.Caption.ToString(), value))
                        {
                            media.Caption = new TLString(value);

                            LoadMentionHints(value);
                        }
                    }
                }
            }
        }

        private bool _isOpen;

        public bool IsOpen { get { return _isOpen; } }

        private bool _isDoneEnabled;

        public bool IsDoneEnabled
        {
            get { return _isDoneEnabled; }
            set
            {
                if (_isDoneEnabled != value)
                {
                    _isDoneEnabled = value;
                    NotifyOfPropertyChange(() => IsDoneEnabled);
                }
            }
        }

        public bool IsDeleteEnabled
        {
            get { return Items.Count > 1; }
        }

        public Func<StorageFile, TLMessage25> GetPhotoMessage { get; set; }

        public Func<StorageFile, Telegram.Api.WindowsPhone.Tuple<TLDecryptedMessageBase, TLObject>> GetDecryptedPhotoMessage { get; set; } 

        public void Done()
        {
            _isOpen = false;
            NotifyOfPropertyChange(() => IsOpen);

            var logString = new StringBuilder();
            logString.AppendLine("photos");
            var messages = new List<PhotoFile>();
            var randomIndex = new Dictionary<long, long>();

            var groupedId = IsGrouped && Items.Count(x => !x.IsButton) > 1 ? TLLong.Random() : null;
            TLInt commonDate = null;
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (item.IsButton) continue;

                if (item.Message != null)
                {
                    if (item.Message.RandomIndex == 0)
                    {
                        logString.AppendLine(string.Format("random_id=0 msg={0} original_file_name={1}", item.Message,
                            item.File.Name));
                        continue;
                    }

                    if (i % Constants.MaxGroupedMediaCount == 0)
                    {
                        groupedId = IsGrouped && Items.Count(x => !x.IsButton) > 1 ? TLLong.Random() : null;
                    }

                    if (randomIndex.ContainsKey(item.Message.RandomIndex))
                    {
                        logString.AppendLine(string.Format("random_id exists msg={0} original_file_name={1}",
                            item.Message, item.File.Name));
                        continue;
                    }

                    var message73 = item.Message as TLMessage73;
                    if (message73 != null)
                    {
                        if (commonDate == null)
                        {
                            commonDate = message73.Date;
                        }
                        message73.Date = commonDate;
                        message73.GroupedId = groupedId;
                    }

                    randomIndex[item.Message.RandomIndex] = item.Message.RandomIndex;

                    var mediaPhoto = item.Message.Media as TLMessageMediaPhoto28;
                    var photo = mediaPhoto.Photo as TLPhoto28;
                    var size = photo.Sizes.First() as TLPhotoSize;
                    var fileLocation = size.Location;
                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        fileLocation.VolumeId,
                        fileLocation.LocalId,
                        fileLocation.Secret);

                    item.Message.Media.UploadingProgress = 0.001;

                    if (item.TimerSpan != null && item.TimerSpan.Seconds > 0)
                    {
                        var message25 = item.Message as TLMessage25;
                        if (message25 != null)
                        {
                            message25.NotListened = true;
                        }
                        var ttlMessageMedia = item.Message.Media as ITTLMessageMedia;
                        if (ttlMessageMedia != null)
                        {
                            ttlMessageMedia.TTLSeconds = new TLInt(item.TimerSpan.Seconds);
                        }
                    }

                    messages.Add(item);
                    logString.AppendLine(string.Format("msg={0} file_name={1}", item.Message, fileName));
                }
                else if (item.DecryptedTuple != null)
                {
                    if (item.DecryptedTuple.Item1.RandomIndex == 0)
                    {
                        logString.AppendLine(string.Format("random_id=0 msg={0} original_file_name={1}", item.DecryptedTuple.Item1, item.File.Name));
                        continue;
                    }

                    if (randomIndex.ContainsKey(item.DecryptedTuple.Item1.RandomIndex))
                    {
                        logString.AppendLine(string.Format("random_id exists msg={0} original_file_name={1}", item.DecryptedTuple.Item1, item.File.Name));
                        continue;
                    }

                    var decryptedMessage73 = item.DecryptedTuple.Item1 as TLDecryptedMessage73;
                    if (decryptedMessage73 != null)
                    {
                        if (commonDate == null)
                        {
                            commonDate = decryptedMessage73.Date;
                        }
                        decryptedMessage73.Date = commonDate;
                        decryptedMessage73.GroupedId = groupedId;
                    }

                    randomIndex[item.DecryptedTuple.Item1.RandomIndex] = item.DecryptedTuple.Item1.RandomIndex;

                    var mediaPhoto = ((TLDecryptedMessage) item.DecryptedTuple.Item1).Media as TLDecryptedMessageMediaPhoto;
                    var fileLocation = mediaPhoto.Photo as TLEncryptedFile;
                    var fileName = String.Format("{0}_{1}_{2}.jpg",
                        fileLocation.Id,
                        fileLocation.DCId,
                        fileLocation.AccessHash);

                    mediaPhoto.StorageFile = null;
                    mediaPhoto.UploadingProgress = 0.001;

                    messages.Add(item);
                    logString.AppendLine(string.Format("msg={0} file_name={1}", item.DecryptedTuple.Item1, fileName));
                }
                else
                {
                    logString.AppendLine(string.Format("empty msg original_file_name={0}", item.File.Name));
                }
            }

#if MULTIPLE_PHOTOS
            Log.Write(logString.ToString());
#endif

            ContinueAction.SafeInvoke(messages);
        }

        public void Cancel()
        {
            CloseEditor();
        }

        public void OpenEditor()
        {
            Items.Clear();
            //_items = new List<TLMessage> { CurrentItem };
            //IsGrouped = true;
            IsDoneEnabled = false;
            _isOpen = CurrentItem != null;
            NotifyOfPropertyChange(() => IsOpen);
            NotifyOfPropertyChange(() => IsDeleteEnabled);
        }

        public void CloseEditor()
        {
            if (View != null && View.IsExtendedImageEditorOpened)
            {
                View.CloseExtendedImageEditor();
                return;
            }

            _isOpen = false;
            NotifyOfPropertyChange(() => IsOpen);

            _currentItem = null;
        }

        public async void OpenAnimationComplete()
        {
            Items.Add(CurrentItem);
            Items.Add(new PhotoFile { IsButton = true });
            
            Log.Write("send photos count=" + Files.Count);

            var files = new List<StorageFile>(Files);
            files.RemoveAt(0);
            await AddFiles(files);
        }

        public async Task AddFiles(IList<StorageFile> files)
        {
            IsDoneEnabled = false;

            for (var i = 0; i < files.Count; i++)
            {
                var photoFile = new PhotoFile { File = files[i] };
                Items.Insert(Items.Count - 1, photoFile);
            }

            if (CurrentItem == null)
            {
                CurrentItem = Items.FirstOrDefault();
            }
            NotifyOfPropertyChange(() => IsDeleteEnabled);

            var maxCount = 9;
            var counter = 0;
            var firstSlice = new List<PhotoFile>();
            var secondSlice = new List<PhotoFile>();
            foreach (var item in Items)
            {
                if (item.IsButton)
                {
                    continue;
                }

                if (counter > maxCount)
                {
                    secondSlice.Add(item);
                }
                else
                {
                    firstSlice.Add(item);
                }
                counter++;
            }

            //await UpdateThumbnails(firstSlice);

            var count = Items.Count;
            if (count > 2)
            {
                var tasks = new List<Task>();
                for (var i = 0; i < count; i++)
                {
                    var localItem = Items[i];
                    if (localItem.Object != null)
                    {
                        continue;
                    }
                    if (localItem.IsButton)
                    {
                        continue;
                    }

                    var task = Task.Run(() =>
                    {
                        try
                        {
                            var file = localItem.File;
                            if (GetPhotoMessage != null)
                            {
                                var message = GetPhotoMessage(file);
                                localItem.Message = message;
                            }
                            else
                            {
                                var message = GetDecryptedPhotoMessage(file);
                                localItem.DecryptedTuple = message;
                            }

                            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                            {
                                localItem.NotifyOfPropertyChange(() => localItem.Self);
                            });
                        }
                        catch (Exception ex)
                        {
                            Log.Write(ex.ToString());
                        }
                    });
                    tasks.Add(task);
                }

                await Task.WhenAll(tasks);

                if (!IsOpen)
                {
                    return;
                }

                NotifyOfPropertyChange(() => CurrentItem);

                var date = CurrentItem.Date;
                foreach (var item in Items)
                {
                    if (item != null && item.Message != null)
                    {
                        item.Date = date;
                    }
                }

                IsDoneEnabled = true;

                //await UpdateThumbnails(secondSlice);
            }
            else
            {
                IsDoneEnabled = CurrentItem != null && CurrentItem.Object != null;
            }
        }

        private async Task UpdateThumbnails(IList<PhotoFile> items)
        {
            foreach (var item in items)
            {
                if (item.Thumbnail != null) continue;

                var thumbnail = await DialogDetailsViewModel.GetPhotoThumbnailAsync(item.File, ThumbnailMode.ListView, 99, ThumbnailOptions.None);
                item.Thumbnail = thumbnail;
                item.NotifyOfPropertyChange(() => item.Self);
            }
        }

        public async void PickPhoto()
        {
#if WP81
            var photoPickerSettings = IoC.Get<IStateService>().GetPhotoPickerSettings();
            if (photoPickerSettings != null && photoPickerSettings.External)
            {
                ((App)Application.Current).ChooseFileInfo = new ChooseFileInfo(DateTime.Now);
                var fileOpenPicker = new FileOpenPicker();
                fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
                fileOpenPicker.FileTypeFilter.Clear();
                fileOpenPicker.FileTypeFilter.Add(".bmp");
                fileOpenPicker.FileTypeFilter.Add(".png");
                fileOpenPicker.FileTypeFilter.Add(".jpeg");
                fileOpenPicker.FileTypeFilter.Add(".jpg");
                fileOpenPicker.ContinuationData.Add("From", IsSecretChat ? "SecretDialogDetailsView" : "DialogDetailsView");
                fileOpenPicker.ContinuationData.Add("Type", "Image");
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    var result = await fileOpenPicker.PickMultipleFilesAsync();
                    if (result.Count > 0)
                    {
                        Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                        {
                            _sendPhotosAction.SafeInvoke(result);
                        });
                    }
                }
                else
                {
                    fileOpenPicker.PickMultipleFilesAndContinue();
                }
            }
            else
            {
                ChooseAttachmentViewModel.OpenPhotoPicker(false, (r1, r2) => _sendPhotosAction(r1));
            }
#endif
        }

        public void SelectMessage(PhotoFile file)
        {
            CurrentItem = file;
        }

        public void Delete(PhotoFile file)
        {
            var index = Items.IndexOf(file);
            if (index == -1)
            {
                return;
            }
            Items.RemoveAt(index);
            if (CurrentItem == file)
            {
                if (Items.Count > 1)
                {
                    if (Items.Count > index + 1)
                    {
                        CurrentItem = Items[index];
                    }
                    else
                    {
                        CurrentItem = Items[index - 1];
                    }
                }
                else
                {
                    CurrentItem = null;
                }
            }

            IsDoneEnabled = Items.FirstOrDefault(x => !x.IsButton) != null;
            NotifyOfPropertyChange(() => IsDeleteEnabled);

            if (Items.Count == 1)
            {
                _isOpen = false;
                NotifyOfPropertyChange(() => IsOpen);
            }
        }
    }

    public class TimerSpanToBrushConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            "AccentBrush", typeof(Brush), typeof(TimerSpanToBrushConverter), new PropertyMetadata(default(Brush)));

        public Brush AccentBrush
        {
            get { return (Brush) GetValue(AccentBrushProperty); }
            set { SetValue(AccentBrushProperty, value); }
        }

        public static readonly DependencyProperty NormalBrushProperty = DependencyProperty.Register(
            "NormalBrush", typeof(Brush), typeof(TimerSpanToBrushConverter), new PropertyMetadata(default(Brush)));

        public Brush NormalBrush
        {
            get { return (Brush) GetValue(NormalBrushProperty); }
            set { SetValue(NormalBrushProperty, value); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var timerSpan = value as TimerSpan;
            if (timerSpan != null && timerSpan.Seconds > 0)
            {
                return AccentBrush;
            }

            return NormalBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

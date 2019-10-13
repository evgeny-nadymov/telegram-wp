// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using TelegramClient.Services;
using TelegramClient.Views.Controls;
using Execute = Telegram.Api.Helpers.Execute;
using TaskResult = Microsoft.Phone.Tasks.TaskResult;

namespace TelegramClient.ViewModels.Additional
{
    public class EditCurrentUserViewModel :
        ItemDetailsViewModelBase, Telegram.Api.Aggregator.IHandle<UploadableItem>
    {
        private string _firstName;

        public string FirstName
        {
            get { return _firstName; }
            set { SetField(ref _firstName, value, () => FirstName); }
        }

        private string _lastName;

        public string LastName
        {
            get { return _lastName; }
            set { SetField(ref _lastName, value, () => LastName); }
        }

        private readonly IUploadFileManager _uploadManager;

        public EditCurrentUserViewModel(IUploadFileManager uploadManager, ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) : 
            base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            EventAggregator.Subscribe(this);

            _uploadManager = uploadManager;

            CurrentItem = CacheService.GetUser(new TLInt(StateService.CurrentUserId));

            FirstName = ((TLUserBase)CurrentItem).FirstName.Value;
            LastName = ((TLUserBase)CurrentItem).LastName.Value;
        }

        //~EditCurrentUserViewModel()
        //{
            
        //}

        public void SetProfilePhoto()
        {
            EditCurrentUserActions.EditPhoto(photo =>
            {
                var fileId = TLLong.Random();
                IsWorking = true;
                _uploadManager.UploadFile(fileId, new TLUser66{ IsSelf = true }, photo);
            });
        }

        public void DeletePhoto()
        {
            MTProtoService.UpdateProfilePhotoAsync(new TLInputPhotoEmpty(), 
                result =>
                {
                    Execute.ShowDebugMessage("photos.updateProfilePhoto result " + result);
                },
                error =>
                {
                    Execute.ShowDebugMessage("photos.updateProfilePhoto error " + error);
                });
        }

        public void Done()
        {
            if (IsWorking) return;

            IsWorking = true;
            MTProtoService.UpdateProfileAsync(new TLString(FirstName), new TLString(LastName), null,
                statedMessage => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    var user = CurrentItem as TLUserBase;
                    if (user != null) user.NotifyOfPropertyChange(() => user.FullName2);

                    NavigationService.GoBack();
                }),
                error => BeginOnUIThread(() => 
                {
                    Execute.ShowDebugMessage("account.updateProfile error " + error);

                    IsWorking = false;
                    NavigationService.GoBack();
                }));
        }

        public void Cancel()
        {
            NavigationService.GoBack();
        }

        public void Handle(UploadableItem item)
        {
            var userBase = item.Owner as TLUserBase;
            if (userBase != null && userBase.IsSelf)
            {
                IsWorking = false;
            }
        }
    }

    public static class EditCurrentUserActions
    {
        private static CropControl _cropControl;

        public static void EditPhoto(Action<byte[]> callback)
        {
            var photoPickerSettings = IoC.Get<IStateService>().GetPhotoPickerSettings();
            if (photoPickerSettings != null && photoPickerSettings.External)
            {
                var photoChooserTask = new PhotoChooserTask { ShowCamera = true, PixelHeight = 800, PixelWidth = 800 };

                photoChooserTask.Completed += (o, e) =>
                {
                    if (e.TaskResult == TaskResult.OK)
                    {
                        byte[] bytes;
                        var sourceStream = e.ChosenPhoto;
                        using (var memoryStream = new MemoryStream())
                        {
                            sourceStream.CopyTo(memoryStream);
                            bytes = memoryStream.ToArray();
                        }

                        callback.SafeInvoke(bytes);
                    }
                };

                photoChooserTask.Show();
            }
            else
            {
                ChooseAttachmentViewModel.OpenPhotoPicker(true, (result1, result2) =>
                {
                    Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.4), () =>
                    {
                        var isVisible = false;
                        var frame = Application.Current.RootVisual as PhoneApplicationFrame;
                        PhoneApplicationPage page = null;
                        if (frame != null)
                        {
                            page = frame.Content as PhoneApplicationPage;
                            if (page != null)
                            {
                                var applicationBar = page.ApplicationBar;
                                if (applicationBar != null)
                                {
                                    isVisible = applicationBar.IsVisible;
                                    applicationBar.IsVisible = false;
                                }
                            }
                        }

                        if (page == null) return;

                        var popup = new Popup();
                        var cropControl = new CropControl
                        {
                            Width = page.ActualWidth,
                            Height = page.ActualHeight
                        };
                        _cropControl = cropControl;
                        page.SizeChanged += PageOnSizeChanged;

                        cropControl.Close += (sender, args) =>
                        {
                            _cropControl = null;
                            popup.IsOpen = false;
                            popup.Child = null;

                            frame = Application.Current.RootVisual as PhoneApplicationFrame;
                            if (frame != null)
                            {
                                page = frame.Content as PhoneApplicationPage;
                                if (page != null)
                                {
                                    page.SizeChanged -= PageOnSizeChanged;
                                    var applicationBar = page.ApplicationBar;
                                    if (applicationBar != null)
                                    {
                                        applicationBar.IsVisible = isVisible;
                                    }
                                }
                            }
                        };
                        cropControl.Crop += (sender, args) =>
                        {
                            callback.SafeInvoke(args.File);

                            cropControl.TryClose();
                        };
                        cropControl.SetFile(result1.FirstOrDefault(), result2.FirstOrDefault());

                        popup.Child = cropControl;
                        popup.IsOpen = true;
                    });
                });
            }
        }

        private static void PageOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_cropControl != null)
            {
                _cropControl.Width = e.NewSize.Width;
                _cropControl.Height = e.NewSize.Height;
            }
        }
    }
}

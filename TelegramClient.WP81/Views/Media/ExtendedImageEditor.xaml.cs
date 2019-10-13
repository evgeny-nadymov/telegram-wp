// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System.Threading;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using OpenCVComponent;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using Action = System.Action;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using Point = System.Windows.Point;
using ThreadPool = Windows.System.Threading.ThreadPool;

namespace TelegramClient.Views.Media
{
    public partial class ExtendedImageEditor : Telegram.Api.Aggregator.IHandle<DownloadableItem>
    {
        private ExtendedImageEditorMode _mode;

        public event EventHandler Cancel;

        protected virtual void RaiseCancel()
        {
            var handler = Cancel;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler<PreviewEventArgs> Done;

        protected virtual void RaiseDone(PreviewEventArgs args)
        {
            var handler = Done;
            if (handler != null) handler(this, args);
        }

        private OpenCVLib _opencv;

        private readonly ITelegramEventAggregator _eventAggregator;

        private TLAllStickers _masks;

        private TLAllStickers _stickers;

        public ExtendedImageEditor()
        {
            InitializeComponent();

            _eventAggregator = IoC.Get<ITelegramEventAggregator>();

            if (DesignerProperties.IsInDesignTool) return;
            _opencv = new OpenCVLib();

            Loaded += (o, e) =>
            {
                _eventAggregator.Subscribe(this);

                if (_lastStoryboard != null)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        if (_lastStoryboard == null) return;
                        _lastStoryboard.Begin();
                    });
                }
                //SetCanvasSize();
            };
            Unloaded += (o, e) =>
            {
                _eventAggregator.Unsubscribe(this);
            };

            Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
            {
                _masks = IoC.Get<IStateService>().GetMasks();
                _stickers = IoC.Get<IStateService>().GetAllStickers();
            });
        }

        private void UpdateEnabledButtons()
        {
            var isEnabled = PreviewCanvas.Children.Count > 1;

            ClearAllButton.IsEnabled = isEnabled;
            UndoButton.IsEnabled = isEnabled;
        }

        private bool _isManipulating;

        public bool IsStickerPanelOpened
        {
            get { return _stickersControl != null && _stickersControl.Visibility == Visibility.Visible; }
        }

        public void CloseStickerPanel()
        {
            if (_stickersControl == null) return;

            _stickersControl.Close();
        }

        private FacesImage _facesImage;

        private readonly List<PhotoFace> _photoFaces = new List<PhotoFace>();

        private PhotoFile _photoFile;

        private StorageFile _localFile;

        private TLMessage _message;

        private TLDecryptedMessage _decryptedMessage;

        private int _h;

        private int _w;

        public void Detect(PhotoFile photoFile, BitmapSource image)
        {
            FlipButton.Opacity = 0.0;

            _mode = ExtendedImageEditorMode.Paint;

            Preview.Source = null;
            PreviewCanvas.Children.Clear();
            PreviewCanvas.Children.Add(Preview);

            paintingSize = null;
            _w = 0;
            _h = 0;

            UpdateEnabledButtons();

            _photoFile = photoFile;
            if (photoFile == null) return;

            var message = _photoFile.Message;
            var decryptedTuple = _photoFile.DecryptedTuple;
            if (message != null)
            {
                var mediaPhoto = message.Media as TLMessageMediaPhoto;
                if (mediaPhoto == null) return;

                var file = mediaPhoto.File;
                if (file == null) return;

                _localFile = file;
                _message = message;
            }
            else if (decryptedTuple != null)
            {
                var decryptedMessage = decryptedTuple.Item1 as TLDecryptedMessage;
                if (decryptedMessage == null) return;

                var mediaPhoto = decryptedMessage.Media as TLDecryptedMessageMediaPhoto;
                if (mediaPhoto == null) return;

                var file = mediaPhoto.StorageFile;
                if (file == null) return;

                _localFile = file;
                _decryptedMessage = decryptedMessage;
            }
            else
            {
                return;
            }
            Preview.Source = image;

            SetCanvasSize();
            ThreadPool.RunAsync(async state =>
            {
                var cached = await GetFacesImage(photoFile, _localFile);

                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {

                    var bitmap = new WriteableBitmap(image);

                    var pixels = _facesImage.Image;
                    //#if DEBUG
                    //if (!cached)
                    //{
                    //    for (int x = 0; x < bitmap.Pixels.Length; x++)
                    //    {
                    //        bitmap.Pixels[x] = pixels[x];
                    //    }
                    //    Preview.Source = bitmap;
                    //}
                    //#endif

                    var targetSize = GetPaintingSize();
                    _photoFaces.Clear();
                    foreach (var face in _facesImage.Faces)
                    {
                        var photoFace = new PhotoFace(face, bitmap, targetSize, false);
                        _photoFaces.Add(photoFace);
                    }
                });
            },
            WorkItemPriority.Low,
            WorkItemOptions.None);
        }

        private void SetCanvasSize()
        {
            if (_w != 0 && _h != 0) return;

            var image = Preview.Source as BitmapImage;
            if (image == null) return;

            var canvasWidth = 480.0;
            var canvasHeight = 590.0;

            if (Application.Current.Host.Content.ScaleFactor == 225)
            {
                canvasHeight = 621.0;
            }

            var imageWidth = image.PixelWidth;
            var imageHeight = image.PixelHeight;

            if (imageWidth >= imageHeight)
            {
                _w = (int)canvasWidth;
                _h = (int)Math.Round(canvasWidth / imageWidth * imageHeight);
            }
            else
            {
                _w = (int)Math.Round(canvasHeight * imageWidth / imageHeight);
                _h = (int)canvasHeight;
            }

            Preview.Width = _w;
            Preview.Height = _h;

            PreviewCanvas.Opacity = 1.0;
            PreviewCanvas.Width = _w;
            PreviewCanvas.Height = _h;
            PreviewCanvas.Clip = new RectangleGeometry { Rect = new Rect(0, 0, _w, _h) };
        }

        private async Task<bool> GetFacesImage(PhotoFile photoFile, StorageFile file)
        {
            var cached = true;
            if (photoFile.FacesImage == null)
            {
                _opencv = new OpenCVLib();

                photoFile.FacesImage = await _opencv.ProcessImageAsync(file.Path);
                cached = false;
            }

            _facesImage = photoFile.FacesImage;
            return cached;
        }

        private void DrawPoint(Point? point, WriteableBitmap bitmap, Size targetSize, int width, uint color)
        {
            if (point.HasValue)
            {
                var x = (int)(point.Value.X * bitmap.PixelWidth / targetSize.Width);
                var y = (int)(point.Value.Y * bitmap.PixelHeight / targetSize.Height);
                DrawFilledRectangle(bitmap, x - width, y - width, x + width, y + width, (int)color);
            }
        }

        public static void DrawFilledRectangle(WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color)
        {
            // Use refs for faster access (really important!) speeds up a lot!
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
            int[] pixels = bmp.Pixels;

            // Check boundaries
            if (x1 < 0) { x1 = 0; }
            if (y1 < 0) { y1 = 0; }
            if (x2 < 0) { x2 = 0; }
            if (y2 < 0) { y2 = 0; }
            if (x1 >= w) { x1 = w - 1; }
            if (y1 >= h) { y1 = h - 1; }
            if (x2 >= w) { x2 = w; }
            if (y2 >= h) { y2 = h; }

            int i = y1 * w;
            for (int y = y1; y < y2; y++)
            {
                int i2 = i + x1;
                while (i2 < i + x2)
                {
                    pixels[i2++] = color;
                }
                i += w;
            }
        }

        private async void DoneButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (PreviewCanvas.Children.Count == 1)
            {
                RaiseCancel();
                Telegram.Api.Helpers.Execute.ShowDebugMessage("Cancel editing");
                return;
            }

            AddMessageStickers();

            ClearSelectedControl();

            //return;
            DoneButton.IsEnabled = false;
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                var bitmapImage = (BitmapSource)Preview.Source;
                var bitmapImageMax = Math.Max(bitmapImage.PixelHeight, bitmapImage.PixelWidth);
                var max = Math.Max(Preview.ActualHeight, Preview.ActualWidth);
                var min = Math.Min(Constants.DefaultImageSize, bitmapImageMax);
                var scale = min / max;
                var preview = new WriteableBitmap(PreviewCanvas, new ScaleTransform { ScaleX = scale, ScaleY = scale });

                TLPhotoSize photoSize = null;
                TLDecryptedMessageMediaPhoto decryptedMessageMediaPhoto = null;
                if (_message != null)
                {
                    var messageMediaPhoto = _message.Media as TLMessageMediaPhoto;
                    if (messageMediaPhoto != null)
                    {
                        var photo = messageMediaPhoto.Photo as TLPhoto;
                        if (photo != null)
                        {
                            photoSize = photo.Sizes.FirstOrDefault() as TLPhotoSize;
                            if (photoSize != null)
                            {
                                photoSize.W = new TLInt(preview.PixelWidth);
                                photoSize.H = new TLInt(preview.PixelHeight);
                            }
                        }
                    }
                }
                else if (_decryptedMessage != null)
                {
                    decryptedMessageMediaPhoto = _decryptedMessage.Media as TLDecryptedMessageMediaPhoto;
                    if (decryptedMessageMediaPhoto != null)
                    {
                        decryptedMessageMediaPhoto.W = new TLInt(preview.PixelWidth);
                        decryptedMessageMediaPhoto.H = new TLInt(preview.PixelHeight);
                    }
                }

                RaiseDone(new PreviewEventArgs { Preview = null });

                Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                {
                    if (_localFile == null) return;

                    var fileName = _localFile.Name;
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        using (var f = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            var quality = 95;
                            preview.SaveJpeg(f, preview.PixelWidth, preview.PixelHeight, 0, quality);
                            if (decryptedMessageMediaPhoto != null)
                            {
                                decryptedMessageMediaPhoto.Size = new TLInt((int)f.Length);
                            }
                            else if (photoSize != null)
                            {
                                photoSize.Size = new TLInt((int)f.Length);
                            }
                        }
                    }

                    Telegram.Api.Helpers.Execute.BeginOnUIThread(async () =>
                    {
                        DefaultPhotoConverter.InvalidateCacheItem(fileName);

                        DoneButton.IsEnabled = true;

                        _photoFile.File = _localFile;
                        _photoFile.NotifyOfPropertyChange(() => _photoFile.Object);

                        //if (_photoFile != null)
                        //{
                        //    var thumbnail = await DialogDetailsViewModel.GetPhotoThumbnailAsync(_localFile, ThumbnailMode.ListView, 99 * 2, ThumbnailOptions.None);
                        //    _photoFile.Thumbnail = thumbnail;
                        //    _photoFile.NotifyOfPropertyChange(() => _photoFile.Self);
                        //}
                    });
                });
            });
        }

        private void AddMessageStickers()
        {
            if (_photoFile != null)
            {
                var documents = new TLVector<TLDocumentBase>();
                foreach (var child in PreviewCanvas.Children)
                {
                    var sticker = child as Sticker;
                    if (sticker != null)
                    {
                        var document = sticker.StickerItem.Document as TLDocument54;
                        if (document != null)
                        {
                            documents.Add(document);
                        }
                    }
                }

                if (documents.Count > 0)
                {
                    var message48 = _photoFile.Message as TLMessage48;
                    if (message48 != null)
                    {
                        message48.Documents = documents;
                        var mediaPhoto = message48.Media as TLMessageMediaPhoto28;
                        if (mediaPhoto != null)
                        {
                            var photo = mediaPhoto.Photo as TLPhoto56;
                            if (photo != null)
                            {
                                photo.HasStickers = true;
                            }
                        }
                    }
                }
            }
        }

        private void Preview_OnImageOpened(object sender, RoutedEventArgs e)
        {
            PreviewCanvas.Opacity = 1.0;
        }

        private readonly Random _random = new Random();

        private StickersControl _stickersControl;

        private void Sticker_OnClick(object sender, RoutedEventArgs args)
        {
            _mode = ExtendedImageEditorMode.Sticker;

            if (_stickersControl == null)
            {
                _stickersControl = new StickersControl(
                    //#if DEBUG
                    //                    null, null
                    //#else
                    _masks, _stickers
                    //#endif
                    );
                _stickersControl.StickerSelected += (o, e) =>
                {
                    var stickerPosition = CalculateStickerPosition(e.Sticker.Document as TLDocument54);

#if DEBUG
                    if (stickerPosition == null)
                    {
                        var positionX = _random.Next(100, (int)Preview.ActualWidth - 100);
                        var positionY = _random.Next(100, (int)Preview.ActualHeight - 100);
                        stickerPosition = new StickerPosition(new Point(positionX, positionY), 1.0f, 0.0f);
                    }
#else
                    if (stickerPosition == null)
                    {
                        var positionX = _random.Next(100, (int)Preview.ActualWidth - 100);
                        var positionY = _random.Next(100, (int)Preview.ActualHeight - 100);
                        stickerPosition = new StickerPosition(new Point(positionX, positionY), 1.0f, 0.0f);
                    }
#endif
                    var stickerSize = BaseStickerSize();
                    var targetSize = GetPaintingSize();
                    var transform = new CompositeTransform
                    {
                        Rotation = stickerPosition.Angle,
                        ScaleX = stickerPosition.Scale / targetSize.Width * Preview.ActualWidth,
                        ScaleY = stickerPosition.Scale / targetSize.Width * Preview.ActualWidth,
                        TranslateX = stickerPosition.Position.X / targetSize.Width * Preview.ActualWidth,
                        TranslateY = stickerPosition.Position.Y / targetSize.Height * Preview.ActualHeight
                    };
                    AddSticker(e, stickerSize, transform);
                    _stickersControl.Close();
                };
                StickersPanel.Content = _stickersControl;
            }
            else
            {
                _stickersControl.Open();
            }
        }

        private StickerPosition CalculateStickerPosition(TLDocument54 document)
        {
            TLMaskCoords maskCoords = null;

            for (int a = 0; a < document.Attributes.Count; a++)
            {
                var attribute = document.Attributes[a] as TLDocumentAttributeSticker56;
                if (attribute != null)
                {
                    maskCoords = attribute.MaskCoords;
                    break;
                }
            }

            var defaultPosition = new StickerPosition(CenterPositionForEntity(), 0.75f, 0.0f);
            if (maskCoords == null || _photoFaces == null || _photoFaces.Count == 0)
            {
                return defaultPosition;
            }

            var anchor = maskCoords.N;

            var face = GetRandomFaceWithVacantAnchor(anchor.Value, document.Id.Value, maskCoords);
            if (face == null)
            {
                return null;
            }

            var referencePoint = face.GetPointForAnchor(anchor.Value);
            if (referencePoint.HasValue)
            {
                var referenceWidth = face.GetWidthForAnchor(anchor.Value);
                var angle = face.GetAngle();
                var baseSize = BaseStickerSize();

                var scale = (float)(referenceWidth / baseSize.Width * maskCoords.Zoom.Value * 1.25);

                var radAngle = MathHelper.ToRadians(angle);
                var xCompX = (float)(Math.Sin(Math.PI / 2.0f - radAngle) * referenceWidth * maskCoords.X.Value);
                var xCompY = (float)(Math.Cos(Math.PI / 2.0f - radAngle) * referenceWidth * maskCoords.X.Value);

                var yCompX = (float)(Math.Cos(Math.PI / 2.0f + radAngle) * referenceWidth * maskCoords.Y.Value);
                var yCompY = (float)(Math.Sin(Math.PI / 2.0f + radAngle) * referenceWidth * maskCoords.Y.Value);

                var x = (float)referencePoint.Value.X + xCompX + yCompX;
                var y = (float)referencePoint.Value.Y + xCompY + yCompY;

                return new StickerPosition(new Point(x, y), scale, angle);
            }

            return null;
        }

        private Point CenterPositionForEntity()
        {
            var paintingSize = GetPaintingSize();
            return new Point(paintingSize.Width / 2.0f, paintingSize.Height / 2.0f);
        }

        private Size? paintingSize;

        private Size GetPaintingSize()
        {
            if (paintingSize != null)
            {
                return paintingSize.Value;
            }

            var bitmapToEdit = Preview.Source as BitmapSource;
            if (bitmapToEdit == null) return new Size();

            float width = IsSidewardOrientation() ? bitmapToEdit.PixelHeight : bitmapToEdit.PixelWidth;
            float height = IsSidewardOrientation() ? bitmapToEdit.PixelWidth : bitmapToEdit.PixelHeight;

            var size = new Size(width, height);
            size.Width = 1280;
            size.Height = (float)Math.Floor(size.Width * height / width);
            if (size.Height > 1280)
            {
                size.Height = 1280;
                size.Width = (float)Math.Floor(size.Height * width / height);
            }
            paintingSize = size;
            return size;
        }

        private bool IsSidewardOrientation()
        {
            return false;
        }

        private Size BaseStickerSize()
        {
            var side = (float)Math.Floor(GetPaintingSize().Width * 0.5);
            return new Size(side, side);
        }

        private PhotoFace GetRandomFaceWithVacantAnchor(int anchor, long documentId, TLMaskCoords maskCoords)
        {
            if (anchor < 0 || anchor > 3 || _photoFaces == null || _photoFaces.Count == 0)
            {
                return null;
            }

            var random = new Random();
            int count = _photoFaces.Count;
            int randomIndex = random.Next(count);
            int remaining = count;

            PhotoFace selectedFace = null;
            for (int i = randomIndex; remaining > 0; i = (i + 1) % count, remaining--)
            {
                PhotoFace face = _photoFaces[i];
                if (!IsFaceAnchorOccupied(face, anchor, documentId, maskCoords))
                {
                    return face;
                }
            }

            return selectedFace;
        }

        private bool IsFaceAnchorOccupied(PhotoFace face, int anchor, long documentId, TLMaskCoords maskCoords)
        {
            var anchorPoint = face.GetPointForAnchor(anchor);
            if (anchorPoint == null)
            {
                return true;
            }

            var targetSize = GetPaintingSize();

            anchorPoint = new Point(
                anchorPoint.Value.X / targetSize.Width * Preview.ActualWidth,
                anchorPoint.Value.Y / targetSize.Height * Preview.ActualHeight);

            float minDistance = face.GetWidthForAnchor(0) * 1.1f;

            for (int index = 0; index < PreviewCanvas.Children.Count; index++)
            {
                var view = PreviewCanvas.Children[index];
                if (!(view is Sticker))
                {
                    continue;
                }

                Sticker stickerView = (Sticker)view;
                if (stickerView.GetAnchor() != anchor)
                {
                    continue;
                }

                Point location = stickerView.GetPosition();
                float distance = (float)Math.Sqrt(Math.Pow(location.X - anchorPoint.Value.X, 2.0) + Math.Pow(location.Y - anchorPoint.Value.Y, 2.0));
                if ((documentId == stickerView.StickerItem.Document.Id.Value || _photoFaces.Count > 1) && distance < minDistance)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddSticker(StickerSelectedEventArgs args, Size stickerSize, CompositeTransform transform, bool duplicate = false)
        {
            var sticker = new Sticker();
            sticker.IsHitTestVisible = false;
            sticker.CacheMode = new BitmapCache();
            sticker.Width = stickerSize.Width;
            sticker.Height = stickerSize.Height;

            var sourceBinding = new System.Windows.Data.Binding("Self")
            {
                Source = args.Sticker,
                Converter = new DefaultPhotoConverter()
            };
            sticker.SetBinding(Media.Sticker.SourceProperty, sourceBinding);

            var fullSourceBinding = new System.Windows.Data.Binding("Document")
            {
                Source = args.Sticker,
                Converter = new DefaultPhotoConverter()
            };
            sticker.SetBinding(Media.Sticker.FullSourceProperty, fullSourceBinding);

            sticker.RenderTransformOrigin = new Point(0.5, 0.5);
            DebugTransform(transform);

            sticker.RenderTransform = transform;

            sticker.StickerItem = args.Sticker;

            var deleteMenuItem = new MenuItem { Header = AppResources.Delete };
            deleteMenuItem.Click += (o, ee) =>
            {
                BeginOpacityAnimation(sticker, 0.0, () =>
                {
                    PreviewCanvas.Children.Remove(sticker);
                    UpdateEnabledButtons();
                });
                BeginOpacityAnimation(FlipButton, 0.0);
            };

            var duplicateMenuItem = new MenuItem { Header = AppResources.Duplicate };
            duplicateMenuItem.Click += (o, ee) =>
            {
                var selectedTextLabel = PreviewCanvas.Children.OfType<Sticker>().FirstOrDefault(x => x.IsSelected);
                if (selectedTextLabel == null) return;

                var selectedTransform = selectedTextLabel.RenderTransform as CompositeTransform;
                if (selectedTransform == null) return;

                var newCompositeTransfrom = new CompositeTransform
                {
                    Rotation = selectedTransform.Rotation,
                    ScaleX = selectedTransform.ScaleX,
                    ScaleY = selectedTransform.ScaleY,
                    TranslateX = selectedTransform.TranslateX,
                    TranslateY = selectedTransform.TranslateY
                };
                AddSticker(args, stickerSize, newCompositeTransfrom, true);
            };

            var contextMenu = new ContextMenu { Style = (Style)Application.Current.Resources["W10MContextMenuStyle"] };
            contextMenu.Items.Add(deleteMenuItem);
            contextMenu.Items.Add(duplicateMenuItem);

            ContextMenuService.SetContextMenu(sticker, contextMenu);
            PreviewCanvas.Children.Add(sticker);
            sticker.Opacity = 0.0;
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                if (!duplicate)
                {
                    transform.TranslateX -= sticker.ActualWidth / 2.0;
                    transform.TranslateY -= sticker.ActualHeight / 2.0;
                }
                else
                {
                    BeginTranslateAnimation(sticker, transform.TranslateX + 50.0, transform.TranslateY + 50.0);
                }
                sticker.Opacity = 1.0;
                BringToFront(sticker);

                UpdateEnabledButtons();
            });
        }

        private void DebugTransform(CompositeTransform transform)
        {
            return;
            DEBUG.Text = string.Format("TranslateX={0}\nTranslateY={1}\nRotation={2}\nScaleX={3}\nScaleY={4}\nPreview={5}x{6}",
                transform.TranslateX, transform.TranslateY, transform.Rotation, transform.ScaleX, transform.ScaleY, Preview.ActualWidth, Preview.ActualHeight);
        }

        private void Preview_OnTap(object sender, GestureEventArgs e)
        {
            ClearSelectedControl();
        }

        private void ClearSelectedControl()
        {
            foreach (var child in PreviewCanvas.Children)
            {
                var childSticker = child as SelectableUserControl;
                if (childSticker != null)
                {
                    childSticker.IsSelected = false;
                }
            }

            BeginOpacityAnimation(FlipButton, 0.0);
        }

        private void BeginOpacityAnimation(DependencyObject element, double toOpacity, System.Action callback = null)
        {
            var storyboard = new Storyboard();
            var opacityImageAniamtion = new DoubleAnimationUsingKeyFrames();
            opacityImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = toOpacity });
            Storyboard.SetTarget(opacityImageAniamtion, element);
            Storyboard.SetTargetProperty(opacityImageAniamtion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityImageAniamtion);
            storyboard.Begin();
            storyboard.Completed += (o, e) =>
            {
                callback.SafeInvoke();
            };
        }

        private void BeginTranslateAnimation(DependencyObject element, double translateXTo, double translateYTo)
        {
            var storyboard = new Storyboard();
            var duration = TimeSpan.FromSeconds(0.55);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 };

            var translateXAniamtion = new DoubleAnimationUsingKeyFrames();
            translateXAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = translateXTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateXAniamtion, element);
            Storyboard.SetTargetProperty(translateXAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            storyboard.Children.Add(translateXAniamtion);

            var translateYAniamtion = new DoubleAnimationUsingKeyFrames();
            translateYAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateYAniamtion, element);
            Storyboard.SetTargetProperty(translateYAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateYAniamtion);

            storyboard.Begin();
        }

        private void BringToFront(SelectableUserControl control)
        {
            if (control == null) return;

            var maxZIndex = 0;
            foreach (var child in PreviewCanvas.Children)
            {
                var childSticker = child as SelectableUserControl;
                if (childSticker != null)
                {
                    childSticker.IsSelected = false;
                }

                var zIndex = Canvas.GetZIndex(child);
                if (maxZIndex < zIndex)
                {
                    maxZIndex = zIndex;
                }
            }

            if (Canvas.GetZIndex(control) == 0
                || Canvas.GetZIndex(control) < maxZIndex)
            {
                Canvas.SetZIndex(control, maxZIndex + 1);
            }

            control.IsSelected = true;
            _selectedControl = control;

            var textLabel = control as TextLabel;
            if (textLabel != null)
            {
                var position = textLabel.ColorPosition;
                Picker.SetPosition(position);
            }

            var selectedSticker = control as Sticker;

            BeginOpacityAnimation(FlipButton, selectedSticker != null ? 1.0 : 0.0);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            RaiseCancel();
        }

        private void UndoButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (PreviewCanvas.Children.Count <= 1) return;

            var path = PreviewCanvas.Children[PreviewCanvas.Children.Count - 1] as System.Windows.Shapes.Path;
            if (path != null && path.Tag is Guid)
            {
                var tag = (Guid)path.Tag;
                if (tag != null)
                {
                    for (var i = PreviewCanvas.Children.Count - 2; i >= 1; i--)
                    {
                        path = PreviewCanvas.Children[i] as System.Windows.Shapes.Path;
                        if (path != null && path.Tag is Guid && (Guid)path.Tag == tag)
                        {
                            PreviewCanvas.Children.RemoveAt(i);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            PreviewCanvas.Children.RemoveAt(PreviewCanvas.Children.Count - 1);

            UpdateEnabledButtons();
        }

        private void ClearAllButton_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(AppResources.ClearPaintingConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK) return;

            while (PreviewCanvas.Children.Count > 1)
            {
                PreviewCanvas.Children.RemoveAt(PreviewCanvas.Children.Count - 1);
            }

            UpdateEnabledButtons();
        }

        private void FlipButton_OnClick(object sender, RoutedEventArgs e)
        {
            var sticker = PreviewCanvas.Children.OfType<Sticker>().FirstOrDefault(x => x.IsSelected);
            if (sticker == null) return;

            sticker.Flip();
        }

        private SelectableUserControl _selectedControl;

        private bool _isPinch;

        private void PreviewCanvas_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;

            if (InputGrid.Visibility == Visibility.Visible) return;

            if (_mode == ExtendedImageEditorMode.Paint)
            {
                StartPainting(e, false);
            }
            else
            {
                _selectedControl = GetNearestControl(e);
                if (_selectedControl != null)
                {
                    _mode = ExtendedImageEditorMode.Sticker;
                    BringToFront(_selectedControl);
                }
                else
                {
                    _mode = ExtendedImageEditorMode.Paint;
                    StartPainting(e, true);
                }
            }
        }

        private void PreviewCanvas_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;

            if (_mode == ExtendedImageEditorMode.Paint)
            {
                ContinuePainting(e);
            }
            else
            {
                if (_selectedControl == null) return;

                var compositeTransform = _selectedControl.RenderTransform as CompositeTransform;
                if (compositeTransform == null) return;

                var oldIsPinch = _isPinch;
                _isPinch = e.PinchManipulation != null;

                if (!oldIsPinch && !_isPinch)
                {
                    OnDragDelta(compositeTransform, e);
                }
                else if (!oldIsPinch && _isPinch)
                {
                    OnPinchStarted(compositeTransform, e);
                }
                else if (oldIsPinch && _isPinch)
                {
                    OnPinchDelta(compositeTransform, e);
                }
                else if (oldIsPinch && !_isPinch)
                {
                    OnPinchCompleted(compositeTransform, e);
                }
            }
        }

        private void OnDragDelta(CompositeTransform transform, ManipulationDeltaEventArgs e)
        {
            // HorizontalChange and VerticalChange from DragDeltaGestureEventArgs are now
            // DeltaManipulation.Translation.X and DeltaManipulation.Translation.Y.
            transform.TranslateX += e.DeltaManipulation.Translation.X;
            transform.TranslateY += e.DeltaManipulation.Translation.Y;

            DebugTransform(transform);
        }

        private double _initialAngle;

        private void OnPinchStarted(CompositeTransform transform, ManipulationDeltaEventArgs e)
        {
            _initialAngle = transform.Rotation;
        }

        private double GetAngle(PinchContactPoints points)
        {
            var directionVector = new Point(points.SecondaryContact.X - points.PrimaryContact.X, points.SecondaryContact.Y - points.PrimaryContact.Y);
            return GetAngle(directionVector.X, directionVector.Y);
        }

        private double GetAngle(double x, double y)
        {
            // Note that this function works in xaml coordinates, where positive y is down, and the
            // angle is computed clockwise from the x-axis. 
            var angle = Math.Atan2(y, x);

            // Atan2() returns values between pi and -pi.  We want a value between
            // 0 and 2 pi.  In order to compensate for this, we'll add 2 pi to the angle
            // if it's less than 0, and then multiply by 180 / pi to get the angle
            // in degrees rather than radians, which are the expected units in XAML.
            if (angle < 0)
            {
                angle += 2 * Math.PI;
            }

            return angle * 180 / Math.PI;
        }

        private void OnPinchDelta(CompositeTransform transform, ManipulationDeltaEventArgs e)
        {
            // Rather than providing the rotation, the event args now just provide
            // the raw points of contact for the pinch manipulation.
            // However, calculating the rotation from these two points is fairly trivial;
            // the utility method used here illustrates how that's done.
            double angleDelta = GetAngle(e.PinchManipulation.Current) - GetAngle(e.PinchManipulation.Original);

            transform.Rotation = _initialAngle + angleDelta;

            // DistanceRatio from PinchGestureEventArgs is now replaced by
            // PinchManipulation.DeltaScale and PinchManipulation.CumulativeScale,
            // which expose the scale from the pinch directly.
            transform.ScaleX *= e.PinchManipulation.DeltaScale;
            transform.ScaleY *= e.PinchManipulation.DeltaScale;

            var selectedTextLable = _selectedControl as TextLabel;
            if (selectedTextLable != null)
            {
                if (transform.ScaleX > 4.0) transform.ScaleX = 4.0;
                if (transform.ScaleY > 4.0) transform.ScaleY = 4.0;
            }

            DebugTransform(transform);
        }

        private void OnPinchCompleted(CompositeTransform transform, ManipulationDeltaEventArgs e)
        {

        }

        private void PreviewCanvas_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (_lastPath != null) _lastPath.CacheMode = null;
            e.Handled = true;

            UpdateEnabledButtons();
            _mode = ExtendedImageEditorMode.Sticker;
        }

        private SelectableUserControl GetNearestControl(ManipulationStartedEventArgs e)
        {
            var globalPoint = e.ManipulationContainer.TransformToVisual(Application.Current.RootVisual).Transform(e.ManipulationOrigin);
            var selectedControl = FindNearestControlInHostCoordinates(globalPoint);

            return selectedControl;
        }

        private SelectableUserControl FindNearestControlInHostCoordinates(Point globalPoint)
        {
            var minDistance = double.MaxValue;
            SelectableUserControl selectedSticker = null;
            foreach (var child in PreviewCanvas.Children)
            {
                var sticker = child as SelectableUserControl;
                if (sticker != null)
                {
                    var center = new Point(sticker.ActualWidth / 2.0, sticker.ActualHeight / 2.0);
                    var globalStickerCenter = sticker.TransformToVisual(Application.Current.RootVisual).Transform(center);
                    var stickerWidth = sticker.ActualWidth * ((CompositeTransform)sticker.RenderTransform).ScaleX;
                    var stickerHeight = sticker.ActualHeight * ((CompositeTransform)sticker.RenderTransform).ScaleY;

                    if (globalStickerCenter.X - stickerWidth / 2.0 < globalPoint.X
                        && globalPoint.X < globalStickerCenter.X + stickerWidth / 2.0
                        && globalStickerCenter.Y - stickerHeight / 2.0 < globalPoint.Y
                        && globalPoint.Y < globalStickerCenter.Y + stickerHeight / 2.0)
                    {
                        var distance = Math.Sqrt(Math.Pow(globalStickerCenter.X - globalPoint.X, 2.0) + Math.Pow(globalStickerCenter.Y - globalPoint.Y, 2.0));
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            selectedSticker = sticker;
                        }
                    }
                }
            }

            return selectedSticker;
        }

        private void PreviewCanvas_OnTap(object sender, GestureEventArgs e)
        {
            if (InputGrid.Visibility == Visibility.Visible) return;
            if (_mode == ExtendedImageEditorMode.Paint) return;

            var globalPoint = e.GetPosition(Application.Current.RootVisual);
            var selectedSticker = FindNearestControlInHostCoordinates(globalPoint);

            BringToFront(selectedSticker);
        }

        private void PreviewCanvas_OnHold(object sender, GestureEventArgs e)
        {
            var globalPoint = e.GetPosition(Application.Current.RootVisual);
            var selectedSticker = FindNearestControlInHostCoordinates(globalPoint);

            if (selectedSticker == null) return;

            BringToFront(selectedSticker);

            var contextMenu = ContextMenuService.GetContextMenu(selectedSticker);
            if (contextMenu != null) contextMenu.IsOpen = true;
        }

        private void Text_OnClick(object sender, RoutedEventArgs e)
        {
            _mode = ExtendedImageEditorMode.Text;

            ClearSelectedControl();
            Input.Text = string.Empty;
            InputGrid.Visibility = Visibility.Visible;
            Canvas.SetLeft(Input, (PreviewCanvas.ActualWidth - 300) / 2.0);
            Canvas.SetTop(Input, (PreviewCanvas.ActualHeight - 100) / 2.0);
            Input.Focus();
        }

        private void Input_OnLostFocus(object sender, RoutedEventArgs e)
        {
            AddOrEditTextLabel(Input.Text);
        }

        private void AddOrEditTextLabel(string text)
        {
            var selectedText = PreviewCanvas.Children.FirstOrDefault(x => x is TextLabel && ((TextLabel)x).IsSelected) as TextLabel;
            if (selectedText != null)
            {
                if (string.IsNullOrEmpty(text))
                {
                    PreviewCanvas.Children.Remove(selectedText);
                    InputGrid.Visibility = Visibility.Collapsed;
                }
                else
                {
                    selectedText.Text = text;
                    InputGrid.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                var transform = new CompositeTransform
                {
                    Rotation = 0,
                    ScaleX = 1.0,
                    ScaleY = 1.0,
                    TranslateX = PreviewCanvas.ActualWidth / 2.0,
                    TranslateY = PreviewCanvas.ActualHeight / 2.0
                };
                AddTextLabel(text, transform, 1.0, new SolidColorBrush(Colors.White));
            }
        }

        private void AddTextLabel(string text, CompositeTransform transform, double colorPosition, SolidColorBrush foreground, bool duplicate = false)
        {
            if (string.IsNullOrEmpty(text))
            {
                InputGrid.Visibility = Visibility.Collapsed;
                return;
            }

            var textLabel = new TextLabel();
            textLabel.MaxWidth = PreviewCanvas.ActualWidth;
            textLabel.Text = text;
            textLabel.IsHitTestVisible = false;
            textLabel.CacheMode = new BitmapCache { RenderAtScale = 1.5 };
            textLabel.RenderTransformOrigin = new Point(0.5, 0.5);
            textLabel.ColorPosition = colorPosition;
            textLabel.SetForeground(foreground);
            DebugTransform(transform);

            textLabel.RenderTransform = transform;

            var editMenuItem = new MenuItem { Header = AppResources.Edit };
            editMenuItem.Click += (o, ee) => { EditTextLable(textLabel); };

            var deleteMenuItem = new MenuItem { Header = AppResources.Delete };
            deleteMenuItem.Click += (o, ee) =>
            {
                BeginOpacityAnimation(textLabel, 0.0, () =>
                {
                    PreviewCanvas.Children.Remove(textLabel);

                    UpdateEnabledButtons();
                });
            };

            var duplicateMenuItem = new MenuItem { Header = AppResources.Duplicate };
            duplicateMenuItem.Click += (o, ee) =>
            {
                var selectedTextLabel = PreviewCanvas.Children.OfType<TextLabel>().FirstOrDefault(x => x.IsSelected);
                if (selectedTextLabel == null) return;

                var selectedTransform = selectedTextLabel.RenderTransform as CompositeTransform;
                if (selectedTransform == null) return;

                var newCompositeTransfrom = new CompositeTransform
                {
                    Rotation = selectedTransform.Rotation,
                    ScaleX = selectedTransform.ScaleX,
                    ScaleY = selectedTransform.ScaleY,
                    TranslateX = selectedTransform.TranslateX,  // + 50.0,
                    TranslateY = selectedTransform.TranslateY   // + 50.0
                };

                AddTextLabel(selectedTextLabel.Text, newCompositeTransfrom, selectedTextLabel.ColorPosition, selectedTextLabel.GetForeground(), true);

            };

            var contextMenu = new ContextMenu { Style = (Style)Application.Current.Resources["W10MContextMenuStyle"] };
            contextMenu.Items.Add(deleteMenuItem);
            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(duplicateMenuItem);

            ContextMenuService.SetContextMenu(textLabel, contextMenu);

            InputGrid.Visibility = Visibility.Collapsed;

            PreviewCanvas.Children.Add(textLabel);
            textLabel.Opacity = 0.0;
            Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
            {
                if (!duplicate)
                {
                    transform.TranslateX -= textLabel.ActualWidth / 2.0;
                    transform.TranslateY -= textLabel.ActualHeight / 2.0;
                }
                else
                {
                    BeginTranslateAnimation(textLabel, transform.TranslateX + 50.0, transform.TranslateY + 50.0);
                }
                textLabel.Opacity = 1.0;
                BringToFront(textLabel);

                UpdateEnabledButtons();
            });
        }

        private void EditTextLable(TextLabel textLabel)
        {
            Input.Text = textLabel.Text;
            InputGrid.Visibility = Visibility.Visible;
            Input.SelectionStart = Input.Text.Length;
            Input.SelectionLength = 0;
            Input.Focus();
        }

        private void ColorPicker_OnSelectedColorChanged(object sender, ColorEventArgs e)
        {
            var textLabel = _selectedControl as TextLabel;
            if (textLabel == null) return;

            textLabel.SetForeground(new SolidColorBrush(e.Color));
            textLabel.ColorPosition = e.Position;
        }

        private void PaintButton_OnClick(object sender, RoutedEventArgs e)
        {
            _mode = ExtendedImageEditorMode.Paint;
            ClearSelectedControl();
        }

        #region Painting

        private Point _startPoint;

        private Point _pathStartPoint;

        private List<Point> _points;

        private System.Windows.Shapes.Path _lastPath;

        private int _pointsCount;

        private Brush _brush;

        private Guid _tag;

        private int _strokeThickness;

        private void StartPainting(ManipulationStartedEventArgs e, bool clearSelectedControl)
        {
            _tag = Guid.NewGuid();

            if (_lastPath != null) _lastPath.CacheMode = null;
            _lastPath = null;
            _startPoint = e.ManipulationOrigin;
            _pointsCount = 0;
            _points = new List<Point>();

            _strokeThickness = (int)(10.0 / 0.35 * Picker.CurrentScale);
            _brush = new SolidColorBrush(Picker.SelectedColor);
            _pathStartPoint = e.ManipulationOrigin;

            if (clearSelectedControl)
            {
                ClearSelectedControl();
            }
            else
            {
                var segments = new PathSegmentCollection { new LineSegment { Point = new Point(_pathStartPoint.X, _pathStartPoint.Y) } };
                _lastPath = GetPath(_strokeThickness, _brush, _pathStartPoint, segments);

                PreviewCanvas.Children.Add(_lastPath);
            }
        }

        private void ContinuePainting(ManipulationDeltaEventArgs e)
        {
            _pointsCount++;

            if (_pointsCount == 100)
            {
                _pointsCount = 0;
                _points = _points.Skip(_points.Count - 20).Take(20).ToList();
                if (_lastPath != null) _lastPath.CacheMode = null;
                _lastPath = null;
                _pathStartPoint = _points[0];
            }

            var point = new Point(_startPoint.X + e.CumulativeManipulation.Translation.X,
                _startPoint.Y + e.CumulativeManipulation.Translation.Y);
            _points.Add(point);

            if (_lastPath != null) PreviewCanvas.Children.Remove(_lastPath);

            var segments = GetSegments(_points);
            _lastPath = GetPath(_strokeThickness, _brush, _pathStartPoint, segments);

            PreviewCanvas.Children.Add(_lastPath);
        }

        private System.Windows.Shapes.Path GetPath(double strokeThickness, Brush brush, Point startPoint, PathSegmentCollection segments)
        {
            var path = new System.Windows.Shapes.Path
            {
                Tag = _tag,
                CacheMode = new BitmapCache(),
                StrokeThickness = strokeThickness,
                Stroke = brush,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                StrokeDashCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                Data = new PathGeometry
                {
                    Figures =
                        new PathFigureCollection
                        {
                            new PathFigure {StartPoint = startPoint, Segments = segments, IsClosed = false}
                        }
                }
            };

            return path;
        }

        public static PathSegmentCollection GetSegments(IReadOnlyList<Point> controlPoints)
        {
            if (controlPoints.Count == 0)
            {
                return null;
            }

            var collection = new PathSegmentCollection();

            double beginPointX = controlPoints[0].X;
            double beginPointY = controlPoints[0].Y;

            if (controlPoints.Count <= 3)
            {
                var lineSegment = new LineSegment
                {
                    Point = new Point(beginPointX + 1, beginPointY)
                };

                collection.Add(lineSegment);
            }
            else
            {
                for (int i = 1; i < controlPoints.Count; i++)
                {
                    double prePointX = controlPoints[i - 1].X;
                    double prePointY = controlPoints[i - 1].Y;
                    double currentPointX = controlPoints[i].X;
                    double currentPointY = controlPoints[i].Y;

                    double s = Math.Sqrt(Math.Pow(currentPointX - prePointX, 2) + Math.Pow(currentPointY - prePointY, 2));

                    if (s < 2)
                    {
                        var lineSegment = new LineSegment
                        {
                            Point = new Point(currentPointX, currentPointY)
                        };

                        collection.Add(lineSegment);
                    }
                    else
                    {
                        var startPoint = new Point(prePointX, prePointY);
                        var stopPoint = i == controlPoints.Count - 1
                            ? new Point((currentPointX), (currentPointY))
                            : new Point((prePointX + currentPointX) / 2.0f, (prePointY + currentPointY) / 2.0f);

                        var bezierSegment = new PolyQuadraticBezierSegment
                        {
                            Points = new PointCollection
                            {
                                startPoint,
                                stopPoint
                            }
                        };

                        collection.Add(bezierSegment);
                    }
                }
            }

            return collection;
        }
        #endregion


        public void BeginCloseStoryboard(Action completeAction)
        {
            if (_lastStoryboard != null)
            {
                _lastStoryboard.Pause();
            }

            var duration = TimeSpan.FromSeconds(0.4);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, LayoutRoot);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            storyboard.Completed += (o, args) =>
            {
                completeAction.SafeInvoke();
            };
            Deployment.Current.Dispatcher.BeginInvoke(() => storyboard.Begin());
        }

        private Storyboard _lastStoryboard;

        public void BeginOpenStoryboard(Action completeAction)
        {
            DoneButton.IsEnabled = true;

            var transparentBlack = Colors.Black;
            transparentBlack.A = 0;

            LayoutRoot.RenderTransform = new CompositeTransform();

            var duration = TimeSpan.FromSeconds(0.4);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            ((CompositeTransform)LayoutRoot.RenderTransform).TranslateY = translateYTo;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = 0.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, LayoutRoot);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            storyboard.Completed += (sender, args) =>
            {
                _lastStoryboard = null;
                completeAction.SafeInvoke();
            };

            _lastStoryboard = storyboard;

            if (ActualWidth == 0.0 && ActualHeight == 0.0)
            {

            }
            else
            {
                Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
            }
        }

        public void Handle(DownloadableItem item)
        {
            var document = item.Owner as TLDocument54;
            if (document != null)
            {
                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    var sprites = PreviewCanvas.Children;
                    if (sprites != null)
                    {
                        for (var i = 0; i < sprites.Count; i++)
                        {
                            var stickerSprite = sprites[i] as Sticker;
                            if (stickerSprite != null && stickerSprite.StickerItem.Document == document)
                            {
                                stickerSprite.StickerItem.NotifyOfPropertyChange(() => stickerSprite.StickerItem.Document);
                                break;
                            }
                        }
                    }
                });
            }
        }

        public void UpdateRecentButtonsVisibility()
        {
            var stickersControl = StickersPanel.Content as StickersControl;
            if (stickersControl != null)
            {
                stickersControl.UpdateRecentButtonsVisibility();
            }
        }

        public void ClosePreview()
        {
            if (_stickersControl != null)
            {
                _stickersControl.ClosePreview();
            }
        }
    }

    public class PreviewEventArgs : System.EventArgs
    {
        public WriteableBitmap Preview { get; set; }
    }

    internal enum ExtendedImageEditorMode
    {
        Sticker,
        Paint,
        Text
    }
}

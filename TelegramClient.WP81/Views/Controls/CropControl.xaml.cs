// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Microsoft.Phone.Controls;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Dialogs;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Controls
{
    public partial class CropControl
    {
        public event EventHandler Close;

        protected virtual void RaiseClose()
        {
            var handler = Close;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler<CropEventArgs> Crop;

        protected virtual void RaiseCrop(CropEventArgs e)
        {
            var handler = Crop;
            if (handler != null) handler(this, e);
        }

        public CropControl()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        ~CropControl()
        {
            
        }

        public bool TryClose()
        {
            BeginCloseStoryboard(RaiseClose);

            return true;
        }

        private void BeginCloseStoryboard(Action callback)
        {
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                var page = frame.Content as PhoneApplicationPage;
                if (page != null)
                {
                    page.IsHitTestVisible = true;
                }
            }

            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, LayoutRoot);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateImageAniamtion);

            var opacityImageAniamtion = new ObjectAnimationUsingKeyFrames();
            opacityImageAniamtion.KeyFrames.Add(new DiscreteObjectKeyFrame{ KeyTime = TimeSpan.FromSeconds(0.25), Value = 0 });
            Storyboard.SetTarget(opacityImageAniamtion, ImageBorder);
            Storyboard.SetTargetProperty(opacityImageAniamtion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityImageAniamtion);

            var translateBarAniamtion = new DoubleAnimationUsingKeyFrames();
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0 });
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateBarAniamtion, Bar);
            Storyboard.SetTargetProperty(translateBarAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateBarAniamtion);

            if (callback != null)
            {
                storyboard.Completed += (o, e) => callback();
            }

            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        private StorageFile _result;
        private uint _width;
        private uint _height;
        private Stopwatch _stopwatch;


        public async void SetFile(StorageFile result, PhotoFile file)
        {
            if (result == null) return;

            _result = result;
            var properties = await result.Properties.GetImagePropertiesAsync();
            if (properties != null
                && properties.Width > 0
                && properties.Height > 0)
            {
                _width = properties.Width;
                _height = properties.Height;
                if (_width == 0 || _height == 0) return;

                var minDimension = Math.Min(_width, _height);
                ImageBorder.Width = _width*400.0/minDimension;
                ImageBorder.Height = _height*400.0/minDimension;
                ImageBorder.Margin = new Thickness((400.0 - ImageBorder.Width)/2.0, (400.0 - ImageBorder.Height)/2.0,
                    (400.0 - ImageBorder.Width)/2.0, (400.0 - ImageBorder.Height)/2.0);

                _stopwatch = Stopwatch.StartNew();
                var stream = await result.GetThumbnailAsync(ThumbnailMode.SingleItem, 800);

                System.Diagnostics.Debug.WriteLine("Picture.OpenReadAsync=" + _stopwatch.Elapsed);

                var thumbnailSource = new BitmapImage();
                //thumbnailSource.DecodePixelHeight = (int) ImageBorder.Height * 3;
                //thumbnailSource.DecodePixelWidth = (int) ImageBorder.Width * 3;
                thumbnailSource.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                thumbnailSource.SetSource(stream.AsStream());
                Image.Source = thumbnailSource;
            }
            else
            {
                var sourceStream = await result.OpenReadAsync();
                var decoder = await BitmapDecoder.CreateAsync(sourceStream);

                _width = decoder.OrientedPixelWidth;
                _height = decoder.OrientedPixelHeight;
                if (_width == 0 || _height == 0) return;

                var minDimension = Math.Min(_width, _height);
                ImageBorder.Width = _width * 400.0 / minDimension;
                ImageBorder.Height = _height * 400.0 / minDimension;
                ImageBorder.Margin = new Thickness((400.0 - ImageBorder.Width) / 2.0, (400.0 - ImageBorder.Height) / 2.0,
                    (400.0 - ImageBorder.Width) / 2.0, (400.0 - ImageBorder.Height) / 2.0);

                _stopwatch = Stopwatch.StartNew();
                var resizedJpeg = await DialogDetailsViewModel.ResizeJpeg(sourceStream, 800, string.Empty, string.Empty);
                var stream = new MemoryStream(resizedJpeg.Bytes);

                System.Diagnostics.Debug.WriteLine("Picture.OpenReadAsync=" + _stopwatch.Elapsed);

                var thumbnailSource = new BitmapImage();
                //thumbnailSource.DecodePixelHeight = (int) ImageBorder.Height * 3;
                //thumbnailSource.DecodePixelWidth = (int) ImageBorder.Width * 3;
                thumbnailSource.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                thumbnailSource.SetSource(stream);
                Image.Source = thumbnailSource;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoaded;

            Rect.Rect = new Rect(0.0, 0.0, 480, 800);
            Ellipse.Center = new Point(480 / 2.0, 800 / 2.0);

            BeginOpenStoryboard();
        }

        private void BeginOpenStoryboard()
        {
            var frame = Application.Current.RootVisual as PhoneApplicationFrame;
            if (frame != null)
            {
                var page = frame.Content as PhoneApplicationPage;
                if (page != null)
                {
                    page.IsHitTestVisible = false;
                }
            }

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYFrom = rootFrameHeight;

            var storyboard = new Storyboard();

            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYFrom });
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut } });
            Storyboard.SetTarget(translateImageAniamtion, LayoutRoot);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateImageAniamtion);


            var translateAnimaiton = new DoubleAnimationUsingKeyFrames();
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYFrom });
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0.0, EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut } });
            Storyboard.SetTarget(translateAnimaiton, Bar);
            Storyboard.SetTargetProperty(translateAnimaiton, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateAnimaiton);

            LayoutRoot.Visibility = Visibility.Collapsed;
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                LayoutRoot.Visibility = Visibility.Visible;
                storyboard.Begin();
            });
        }

        private async void DoneButton_OnClick(object sender, GestureEventArgs e)
        {
            var transform = CropCircle.TransformToVisual(ImageBorder);

            var point = transform.Transform(new Point(0, 0));

            var scaleFactor = _width / (ImageBorder.Width * PanAndZoom.CurrentScaleX);
            var originalCropPointX = Math.Floor(point.X * PanAndZoom.CurrentScaleX * scaleFactor);
            var originalCropPointY = Math.Floor(point.Y * PanAndZoom.CurrentScaleY * scaleFactor);
            var originalCropSize = Math.Floor(400.0 * scaleFactor);

            var finalMaxSize = 800.0;
            var scale = Math.Min(finalMaxSize / originalCropSize, 1.0);

            var cropedImage = await GetCroppedImageAsync(_result, new Point(originalCropPointX, originalCropPointY), new Size(originalCropSize, originalCropSize), scale);


            RaiseCrop(new CropEventArgs{ File = cropedImage });
        }

        /// <summary>
        /// Get cropped image
        /// </summary>
        /// <param name="originalImgFile"> Original image </param>
        /// <param name="startPoint"> Crop start point at original image coords </param>
        /// <param name="cropSize"> Crop size at original image coords </param>
        /// <param name="scale"> Scaling for final crop image respect to original size </param>
        /// <returns></returns>
        public static async Task<byte[]> GetCroppedImageAsync(StorageFile originalImgFile, Point startPoint, Size cropSize, double scale)
        {
            if (double.IsNaN(scale) || double.IsInfinity(scale))
            {
                scale = 1.0;
            }

            // Convert start point and size to integer. 
            uint startPointX = (uint)Math.Floor(startPoint.X * scale);
            uint startPointY = (uint)Math.Floor(startPoint.Y * scale);
            uint height = (uint)Math.Floor(cropSize.Height * scale);
            uint width = (uint)Math.Floor(cropSize.Width * scale);


            using (IRandomAccessStream stream = await originalImgFile.OpenReadAsync())
            {
                // Create a decoder from the stream. With the decoder, we can get  
                // the properties of the image. 
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // The scaledSize of original image. 
                uint orientedScaledWidth = (uint)Math.Floor(decoder.OrientedPixelWidth * scale);
                uint orientedScaledHeight = (uint)Math.Floor(decoder.OrientedPixelHeight * scale);

                uint scaledWidth = (uint)Math.Floor(decoder.PixelWidth * scale);
                uint scaledHeight = (uint)Math.Floor(decoder.PixelHeight * scale);

                // Refine the start point and the size.  
                if (startPointX + width > orientedScaledWidth)
                {
                    startPointX = orientedScaledWidth - width;
                }

                if (startPointY + height > orientedScaledHeight)
                {
                    startPointY = orientedScaledHeight - height;
                }

                // Create cropping BitmapTransform and define the bounds. 
                BitmapTransform transform = new BitmapTransform();
                BitmapBounds bounds = new BitmapBounds();
                bounds.X = startPointX;
                bounds.Y = startPointY;
                bounds.Height = height;
                bounds.Width = width;
                transform.Bounds = bounds;
                transform.InterpolationMode = BitmapInterpolationMode.Fant;

                transform.ScaledWidth = scaledWidth;
                transform.ScaledHeight = scaledHeight;

                // Get the cropped pixels within the bounds of transform. 
                PixelDataProvider pix = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.RespectExifOrientation,
                    ColorManagementMode.ColorManageToSRgb);

                using (var destinationStream = new InMemoryRandomAccessStream())
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, destinationStream);
                    encoder.SetPixelData(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode, width, height, decoder.DpiX, decoder.DpiY, pix.DetachPixelData());
                    await encoder.FlushAsync();

                    var reader = new DataReader(destinationStream.GetInputStreamAt(0));
                    var bytes = new byte[destinationStream.Size];
                    await reader.LoadAsync((uint)destinationStream.Size);
                    reader.ReadBytes(bytes);

                    return bytes;
                }
            }
        }

        private void ImageBorder_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            
        }

        private void Image_OnImageOpened(object sender, RoutedEventArgs e)
        {
            LoadingLabel.Visibility = Visibility.Collapsed;
        }
    }

    public class CropEventArgs
    {
        public byte[] File { get; set; }
    }
}

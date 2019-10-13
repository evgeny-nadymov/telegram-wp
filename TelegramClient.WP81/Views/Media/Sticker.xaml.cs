// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Telegram.Api.TL;
using TelegramClient.Views.Dialogs;

namespace TelegramClient.Views.Media
{
    public partial class Sticker
    {
        protected override void OnIsSelectedChanged(object sender, bool isSelected)
        {
            Border.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
            //LeftRotatePoint.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
            //RightRotatePoint.Visibility = isSelected ? Visibility.Visible : Visibility.Collapsed;
        }

        public static readonly DependencyProperty FullSourceProperty = DependencyProperty.Register(
            "FullSource", typeof(ImageSource), typeof(Sticker), new PropertyMetadata(default(ImageSource), OnFullSourceChanged));

        private static void OnFullSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sticker = d as Sticker;
            if (sticker != null)
            {
                sticker.FullImage.Source = e.NewValue as ImageSource;
                if (sticker.FullImage.Source != null)
                {
                    sticker.Image.Source = null;
                }
            }
        }

        public ImageSource FullSource
        {
            get { return (ImageSource) GetValue(FullSourceProperty); }
            set { SetValue(FullSourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(Sticker), new PropertyMetadata(default(ImageSource), OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sticker = d as Sticker;
            if (sticker != null)
            {
                if (sticker.FullImage.Source == null)
                {
                    sticker.Image.Source = e.NewValue as ImageSource;
                }
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public Sticker()
        {
            InitializeComponent();

            Border.Visibility = Visibility.Collapsed;
            LeftRotatePoint.Visibility = Visibility.Collapsed;
            RightRotatePoint.Visibility = Visibility.Collapsed;
        }

        private ContextMenu _lastContextMenu;

        private void RemoveContextMenu()
        {
            _lastContextMenu = ContextMenuService.GetContextMenu(this);

            ContextMenuService.SetContextMenu(this, null);
        }

        private void RestoreContextMenu()
        {
            ContextMenuService.SetContextMenu(this, _lastContextMenu);

            _lastContextMenu = null;
        }

        private Point _manipulationOrigin;

        private void RightRotatePoint_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;

            _manipulationOrigin = e.ManipulationOrigin;

            RemoveContextMenu();
        }

        private void RightRotatePoint_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            DebugManipulationDelta(e);

            e.Handled = true;

            //var pointA = new Point(ActualWidth / 2.0, ActualHeight / 2.0);
            //var pointB = new Point(0.0, ActualHeight / 2.0);
            //var pointC = new Point(pointB.X + e.DeltaManipulation.Translation.X, pointB.Y + e.DeltaManipulation.Translation.Y);

            //var AB = Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2.0) + Math.Pow(pointA.Y - pointB.Y, 2.0));
            //var BC = Math.Sqrt(Math.Pow(pointB.X - pointC.X, 2.0) + Math.Pow(pointB.Y - pointC.Y, 2.0));
            //var AC = Math.Sqrt(Math.Pow(pointA.X - pointC.X, 2.0) + Math.Pow(pointA.Y - pointC.Y, 2.0));

            //var cosA = (AB * AB + AC * AC - BC * BC) / (2.0 * AB * AC);
            //var a = Math.Acos(cosA) * 180.0 / Math.PI;
            //if (pointC.Y < pointB.Y)
            //{
            //    a = -a;
            //}

            //RotateTransform.CenterX = ActualWidth / 2.0;
            //RotateTransform.CenterY = ActualHeight / 2.0;
            //RotateTransform.Angle += a;

        }

        private void DebugManipulationDelta(ManipulationDeltaEventArgs e)
        {
            DEBUG.Text = string.Format("pinch{0}\ncumulative\nX={1}\nY={2}\ndelta\nX={3}\nY={4}\n{5}\n{6}",
                e.PinchManipulation,
                e.CumulativeManipulation.Translation.X,
                e.CumulativeManipulation.Translation.Y,
                e.DeltaManipulation.Translation.X,
                e.DeltaManipulation.Translation.Y,
                e.CumulativeManipulation.Scale,
                e.DeltaManipulation.Scale);
        }

        private void LeftRotatePoint_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            e.Handled = true;

            RemoveContextMenu();
        }

        private void LeftRotatePoint_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            DebugManipulationDelta(e);

            e.Handled = true;

            //var pointA = new Point(ActualWidth / 2.0, ActualHeight / 2.0);
            //var pointB = new Point(0.0, ActualHeight / 2.0);
            //var pointC = new Point(pointB.X + e.DeltaManipulation.Translation.X, pointB.Y + e.DeltaManipulation.Translation.Y);

            //var AB = Math.Sqrt(Math.Pow(pointA.X - pointB.X, 2.0) + Math.Pow(pointA.Y - pointB.Y, 2.0));
            //var BC = Math.Sqrt(Math.Pow(pointB.X - pointC.X, 2.0) + Math.Pow(pointB.Y - pointC.Y, 2.0));
            //var AC = Math.Sqrt(Math.Pow(pointA.X - pointC.X, 2.0) + Math.Pow(pointA.Y - pointC.Y, 2.0));

            //var cosA = (AB * AB + AC * AC - BC * BC) / (2.0 * AB * AC);
            //var a = Math.Acos(cosA) * 180.0 / Math.PI;
            //if (pointC.Y > pointB.Y)
            //{
            //    a = -a;
            //}

            //RotateTransform.CenterX = ActualWidth/2.0;
            //RotateTransform.CenterY = ActualHeight/2.0;
            //RotateTransform.Angle += a;
        }

        public void Flip()
        {
            var scaleTransform = ImageScaleTransform;
            if (scaleTransform == null) return;

            scaleTransform.CenterX = Image.ActualWidth / 2.0;
            scaleTransform.CenterY = Image.ActualHeight / 2.0;

            var storyboard = new Storyboard();

            var toScaleY = scaleTransform.ScaleX > 0.0 ? -1.0 : 1.0;
            var scaleAnimation = new DoubleAnimationUsingKeyFrames();
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = toScaleY, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
            Storyboard.SetTarget(scaleAnimation, scaleTransform);
            Storyboard.SetTargetProperty(scaleAnimation, new PropertyPath("ScaleX"));
            storyboard.Children.Add(scaleAnimation);

            storyboard.Begin();
        }

        private void LeftRotatePoint_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            RestoreContextMenu();
        }

        private void RightRotatePoint_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            RestoreContextMenu();
        }

        public TLStickerItem StickerItem { get; set; }

        public int GetAnchor()
        {
            if (StickerItem != null)
            {
                var document = StickerItem.Document as TLDocument54;

                if (document != null)
                {
                    foreach (var attribute in document.Attributes)
                    {
                        var stickerAttribute = attribute as TLDocumentAttributeSticker56;
                        if (stickerAttribute != null)
                        {
                            var maskCoords = stickerAttribute.MaskCoords;
                            if (maskCoords != null)
                            {
                                return maskCoords.N.Value;
                            }
                        }
                    }
                }
            }

            return -1;
        }

        public Point GetPosition()
        {
            var compositeTransform = RenderTransform as CompositeTransform;
            if (compositeTransform != null)
            {
                return new Point(compositeTransform.TranslateX, compositeTransform.TranslateY);
            }

            return new Point(0.0, 0.0);
        }
    }
}

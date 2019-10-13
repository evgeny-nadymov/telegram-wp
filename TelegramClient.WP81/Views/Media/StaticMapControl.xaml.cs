// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TelegramClient.Converters;

namespace TelegramClient.Views.Media
{
    public partial class StaticMapControl
    {
        public event EventHandler Failed;

        protected virtual void RaiseFailed()
        {
            var handler = Failed;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler Opened;

        protected virtual void RaiseOpened()
        {
            var handler = Opened;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        private object _prevDataContext;

        public static readonly DependencyProperty ImageUriProperty = DependencyProperty.Register(
            "ImageUri", typeof(string), typeof(StaticMapControl), new PropertyMetadata(default(string), OnUriChanged));

        private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mapControl = d as StaticMapControl;
            if (mapControl != null)
            {
                if (mapControl._prevDataContext == null)
                {
                    var bitmapImage = new BitmapImage(new Uri(e.NewValue as string));
                    mapControl.Map2.Source = bitmapImage;
                    mapControl._prevDataContext = mapControl.DataContext;
                }
                else if (mapControl._prevDataContext == mapControl.DataContext)
                {
                    var wrapper = new BitmapImageWrapper((string) e.NewValue, new BitmapImage(new Uri((string) e.NewValue)));
                    wrapper.Subscribe(
                        bi =>
                        {
                            var source = mapControl.Map1.Source;
                            mapControl.Map1.Source = mapControl.Map2.Source;
                            mapControl.Map2.Source = source;

                            StartFadeInAnimation(mapControl.Map2);

                            mapControl.RaiseOpened();
                        },
                        bi =>
                        {
                            mapControl.RaiseFailed();
                        });

                    mapControl.Map1.Source = wrapper.Image;
                }
                else
                {
                    mapControl._prevDataContext = mapControl.DataContext;

                    var wrapper = new BitmapImageWrapper((string) e.NewValue, new BitmapImage(new Uri((string) e.NewValue)));
                    wrapper.Subscribe(
                        bi =>
                        {
                            var source = mapControl.Map1.Source;
                            mapControl.Map1.Source = mapControl.Map2.Source;
                            mapControl.Map2.Source = source;

                            mapControl.RaiseOpened();
                        },
                        bi =>
                        {
                            mapControl.RaiseFailed();
                        });
                    mapControl.Map1.Source = wrapper.Image;
                }
            }
        }

        public string ImageUri
        {
            get { return (string)GetValue(ImageUriProperty); }
            set { SetValue(ImageUriProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof(ImageSource), typeof(StaticMapControl), new PropertyMetadata(default(ImageSource), OnImageSourceChanged));

        private static void StartFadeInAnimation(UIElement obj)
        {
            var storyboard = new Storyboard();
            var opacity1Animation = new DoubleAnimationUsingKeyFrames();
            opacity1Animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            opacity1Animation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 1.0 });
            Storyboard.SetTarget(opacity1Animation, obj);
            Storyboard.SetTargetProperty(opacity1Animation, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacity1Animation);
            storyboard.Begin();
        }

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mapControl = d as StaticMapControl;
            if (mapControl != null)
            {
                var newSource = e.NewValue as ImageSource;

                if (mapControl._prevDataContext == null)
                {
                    mapControl._prevDataContext = mapControl.DataContext;

                    mapControl.Map1.Source = null;
                    mapControl.Map2.Source = newSource;

                    StartFadeInAnimation(mapControl.Map2);
                }
                else if (mapControl._prevDataContext == mapControl.DataContext)
                {
                    mapControl.Map1.Source = mapControl.Map2.Source;
                    mapControl.Map2.Source = newSource;

                    StartFadeInAnimation(mapControl.Map2);

                    mapControl.RaiseOpened();
                }
                else
                {
                    mapControl._prevDataContext = mapControl.DataContext;

                    mapControl.Map1.Source = null;
                    mapControl.Map2.Source = newSource;

                    mapControl.Map2.Opacity = 1.0;

                    mapControl.RaiseOpened();
                }
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public StaticMapControl()
        {
            InitializeComponent();
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Windows;
using System.Windows.Media;

namespace TelegramClient.Helpers
{
    public class Clip
    {
        public static bool GetToBounds(DependencyObject depObj)
        {
            return (bool)depObj.GetValue(ToBoundsProperty);
        }

        public static void SetToBounds(DependencyObject depObj, bool clipToBounds)
        {
            depObj.SetValue(ToBoundsProperty, clipToBounds);
        }

        public static readonly DependencyProperty ToBoundsProperty =
            DependencyProperty.RegisterAttached("ToBounds", typeof(bool),
            typeof(Clip), new PropertyMetadata(false, OnToBoundsPropertyChanged));


        private static void OnToBoundsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fe = d as FrameworkElement;
            if (fe != null)
            {
                ClipToBounds(fe);

                // whenever the element which this property is attached to is loaded
                // or re-sizes, we need to update its clipping geometry
                fe.Loaded += FrameworkElement_Loaded;
                fe.SizeChanged += FrameworkElement_SizeChanged;

            }
        }

        private static void ClipToBounds(FrameworkElement fe)
        {
            if (GetToBounds(fe))
            {
                fe.Clip = new RectangleGeometry
                {
                    Rect = new Rect(0, 0, fe.ActualWidth, fe.ActualHeight)
                };
            }
            else
            {
                fe.Clip = null;
            }
        }

        static void FrameworkElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ClipToBounds(sender as FrameworkElement);
        }

        static void FrameworkElement_Loaded(object sender, RoutedEventArgs e)
        {
            ClipToBounds(sender as FrameworkElement);
        }
    }
}

// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Telegram.Controls.Extensions;

namespace TelegramClient.Animation.Navigation
{
    public class ContinuumAnimator : AnimatorHelperBase
    {
        public FrameworkElement LayoutRoot { get; set; }
        private Popup _popup;

        public override void Begin(Action completionAction)
        {
            Storyboard.Stop();

            PrepareElement(LayoutRoot);

            //if (this is ContinuumForwardOutAnimator)
            //{
            //    WriteableBitmap bitmap = new WriteableBitmap(RootElement, null);
            //    bitmap.Invalidate();
            //    var image = new Image() { Source = bitmap, Stretch = System.Windows.Media.Stretch.None };

            //    var rootVisual = Application.Current.RootVisual as PhoneApplicationFrame;
            //    _popup = new Popup();
            //    var popupChild = new Canvas()
            //    {
            //        Width = rootVisual.ActualWidth,
            //        Height = rootVisual.ActualHeight
            //    };

            //    var transfrom = RootElement.TransformToVisual(rootVisual);
            //    var origin = transfrom.Transform(new Point(0, 0));
            //    popupChild.Children.Add(image);
            //    PrepareElement(image);
            //    Canvas.SetLeft(image, origin.X);
            //    Canvas.SetTop(image, origin.Y);

            //    _popup.Child = popupChild;
            //    RootElement.Opacity = 0;
            //    _popup.IsOpen = true;

            //    Storyboard.Completed += new EventHandler(OnContinuumBackwardOutStoryboardCompleted);
            //    base.SetTargets(new Dictionary<string, FrameworkElement>()
            //    {
            //        { "LayoutRoot", LayoutRoot },
            //        { "ContinuumElement", image }
            //    });
            //}
            //else
            {
                PrepareElement(LayoutRoot);
                PrepareElement(RootElement);
                SetTargets(new Dictionary<string, FrameworkElement>
                               {
                                   {"LayoutRoot", LayoutRoot},
                                   {"ContinuumElement", RootElement}
                               });
            }
            base.Begin(completionAction);
        }

        void OnContinuumBackwardOutStoryboardCompleted(object sender, System.EventArgs e)
        {
            Storyboard.Completed -= new EventHandler(OnContinuumBackwardOutStoryboardCompleted);
            _popup.IsOpen = false;
            _popup.Child = null;
            _popup = null;
        }

        private bool PrepareElement(UIElement element)
        {
            element.GetTransform<CompositeTransform>(TransformCreationMode.CreateOrAddAndIgnoreMatrix);

            return true;
        }
    }

    public class ContinuumForwardInAnimator : ContinuumAnimator
    {
        public ContinuumForwardInAnimator()
            : base()
        {
            Storyboard = XamlReader.Load(Storyboards.ContinuumForwardInStoryboard) as Storyboard;
        }
    }

    public class ContinuumBackwardOutAnimator : ContinuumAnimator
    {
        public ContinuumBackwardOutAnimator()
            : base()
        {
            Storyboard = XamlReader.Load(Storyboards.ContinuumBackwardOutStoryboard) as Storyboard;
        }
    }

    public class ContinuumBackwardInAnimator : ContinuumAnimator
    {
        public ContinuumBackwardInAnimator()
            : base()
        {
            Storyboard = XamlReader.Load(Storyboards.ContinuumBackwardInStoryboard) as Storyboard;
        }
    }

    public class ContinuumForwardOutAnimator : ContinuumAnimator
    {
        public ContinuumForwardOutAnimator()
            : base()
        {
            Storyboard = XamlReader.Load(Storyboards.ContinuumForwardOutStoryboard) as Storyboard;
        }
    }

}

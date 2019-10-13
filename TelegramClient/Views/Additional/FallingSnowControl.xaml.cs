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
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TelegramClient.Views.Additional
{
    public partial class FallingSnowControl : UserControl
    {
        public FallingSnowControl()
        {
            InitializeComponent(); InitializeComponent();

            Loaded += (o, args) =>
            {
                //if (IsEnabled)
                {
                    RunBackgroundAnimation();
                }
            };
            Unloaded += (o, args) =>
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Thread.Sleep(200);
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        foreach (var e in _ellipses)
                        {
                            var sb = e.Resources["sb"] as Storyboard;
                            if (sb != null)
                            {
                                sb.Pause();
                            }
                        }

                    });

                });

            };
        }

        private bool _initialized;

        private void RunBackgroundAnimation()
        {
            if (_initialized)
            {
                foreach (var e in _ellipses)
                {
                    var sb = e.Resources["sb"] as Storyboard;
                    if (sb != null)
                    {
                        sb.Resume();
                    }
                }

                return;
            }

            _initialized = true;

            for (var i = 0; i < 30; i++)
            {
                var ellipse = CreateEllipse();
                LayoutRoot.Children.Add(ellipse);
                RandomiseAndBegin(ellipse);
            }
        }

        private void ContinueBackgroundAnimation()
        {
            foreach (var e in _ellipses)
            {

                var sb = e.Resources["sb"] as Storyboard;
                if (sb.GetCurrentState() == ClockState.Stopped)
                {
                    sb.Begin();
                }
            }
        }

        private IList<Ellipse> _ellipses = new List<Ellipse>();

        private Ellipse CreateEllipse()
        {
            var ellipse = new Ellipse();
            _ellipses.Add(ellipse);
            ellipse.CacheMode = new BitmapCache();

            ellipse.Width = 8;
            ellipse.Height = 8;
            var transform = new CompositeTransform();
            ellipse.RenderTransformOrigin = new Point(0.5, 0.5);
            ellipse.RenderTransform = transform;
            ellipse.Fill = new SolidColorBrush(Colors.White);

            DoubleAnimation xAnim = new DoubleAnimation();
            Storyboard.SetTarget(xAnim, ellipse);
            Storyboard.SetTargetProperty(xAnim,
              new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));

            DoubleAnimation yAnim = new DoubleAnimation();
            Storyboard.SetTarget(yAnim, ellipse);
            Storyboard.SetTargetProperty(yAnim,
              new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));

            Storyboard sb = new Storyboard();
            sb.Children.Add(xAnim);
            sb.Children.Add(yAnim);
            ellipse.Resources.Add("sb", sb);

            sb.Completed += (s, e) =>
            {
                sb.Stop();
                RandomiseAndBegin(ellipse);
            };

            return (ellipse);
        }
        void RandomiseAndBegin(Ellipse e)
        {
            double scale = Utility.Randomise(0.2, 1.0);
            CompositeTransform transform = e.RenderTransform as CompositeTransform;
            transform.ScaleX = transform.ScaleY = scale;

            //BlurEffect effect = (BlurEffect)e.Effect;
            //effect.Radius = Utility.Randomise(20.0, 30.0);

            //var color = new Color();
            //color.R = (byte)Utility.Randomise(0, 255);
            //color.G = (byte)Utility.Randomise(0, 255);
            //color.B = (byte)Utility.Randomise(0, 255);

            //e.Fill = new SolidColorBrush(color);

            e.Opacity = Utility.Randomise(0.2, 0.5);

            Duration duration = new Duration(new TimeSpan(0, 0, (int)Utility.Randomise(9, 30)));

            var sb = e.Resources["sb"] as Storyboard;
            sb.Children[0].Duration = duration;
            sb.Children[1].Duration = duration;

            DoubleAnimation xAnim = (DoubleAnimation)sb.Children[0];
            xAnim.From =
              Utility.Randomise(0, this.LayoutRoot.ActualWidth - (scale * e.Width));
            xAnim.To = xAnim.From;
              //Utility.Randomise(0, this.LayoutRoot.ActualWidth - (scale * e.Width));

            DoubleAnimation yAnim = (DoubleAnimation)sb.Children[1];
            yAnim.To =
              this.LayoutRoot.ActualHeight - ((e.Height - (scale * e.Height)) / 2);
            yAnim.From =
              0 - e.Height - ((scale * e.Height) / 2);

            sb.Begin();

        }
    }
}

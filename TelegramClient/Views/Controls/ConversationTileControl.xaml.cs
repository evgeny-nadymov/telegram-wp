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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Telegram.Api.TL;
using TelegramClient.Extensions;

namespace TelegramClient.Views
{
    public partial class ConversationTileControl
    {
        public static readonly DependencyProperty SuppressFavedProperty = DependencyProperty.Register(
            "SuppressFaved", typeof(bool), typeof(ConversationTileControl), new PropertyMetadata(default(bool)));

        public bool SuppressFaved
        {
            get { return (bool) GetValue(SuppressFavedProperty); }
            set { SetValue(SuppressFavedProperty, value); }
        }

        ~ConversationTileControl()
        {

        }

        public static readonly DependencyProperty ObjectProperty = DependencyProperty.Register(
            "Object", typeof(TLObject), typeof(ConversationTileControl), new PropertyMetadata(default(TLObject), OnObjectChanged));

        private static void OnObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var conversationTileControl = d as ConversationTileControl;
            if (conversationTileControl != null)
            {
                var user = e.NewValue as TLUser;
                var channels = e.NewValue as TLVector<TLChatBase>;
                if (channels != null)
                {
                    conversationTileControl.GoToFeedTemplate(channels);
                }
                else if (user != null && user.IsSelf && !conversationTileControl.SuppressFaved)
                {
                    conversationTileControl.GoToFavedTemplate();
                }
                else
                {
                    conversationTileControl.GoToNormalTemplate();
                }
            }
        }

        private Grid _feedGrid;

        private DispatcherTimer _timer;

        private void GoToFeedTemplate(IList<TLChatBase> channels)
        {
            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(5.0);
                _timer.Tick += (sender, args) =>
                {
                    GoToFeedTemplate(Object as IList<TLChatBase>);
                };
                _timer.Start();
            }

            //if (_feedGrid == null)
            {
                var previousGrid = _feedGrid;

                _feedGrid = new Grid { Width = Size > 0.0 ? Size : 62.0, Height = Size > 0.0 ? Size : 62.0 };

                var random = new Random();
                var randomChannels = channels.OrderBy(x => random.Next()).Take(4).ToList();
                var textConverter = (IValueConverter) Application.Current.Resources["PlaceholderDefaultTextConverter"];
                var fillConverter = (IValueConverter) Application.Current.Resources["PlaceholderBackgroundConverter"];
                var photoConverter = (IValueConverter) Application.Current.Resources["DefaultPhotoConverter"];


                if (randomChannels.Count > 0)
                {
                    var channel = randomChannels[0];

                    var tile = new ConversationTileControl
                    {
                        Margin = new Thickness(2.0),
                        Size = 68.0,
                        Fill = (Brush)fillConverter.Convert(channel.Index, null, null, null),
                        Text = (string)textConverter.Convert(channel, null, null, null),
                        RenderTransform = new CompositeTransform { TranslateX = 0.0, TranslateY = 0.0, ScaleY = 0.5, ScaleX = 0.5 }
                    };

                    var binding = new Binding("Photo")
                    {
                        Converter = photoConverter,
                        Source = channel
                    };
                    tile.SetBinding(SourceProperty, binding);

                    _feedGrid.Children.Add(tile);
                }

                if (randomChannels.Count > 1)
                {
                    var channel = randomChannels[1];

                    var tile = new ConversationTileControl
                    {
                        Margin = new Thickness(2.0),
                        Size = 68.0,
                        Fill = (Brush)fillConverter.Convert(channel.Index, null, null, null),
                        Text = (string)textConverter.Convert(channel, null, null, null),
                        RenderTransform = new CompositeTransform { TranslateX = 36.0, TranslateY = 0.0, ScaleY = 0.5, ScaleX = 0.5 }
                    };

                    var binding = new Binding("Photo")
                    {
                        Converter = photoConverter,
                        Source = channel
                    };
                    tile.SetBinding(SourceProperty, binding);

                    _feedGrid.Children.Add(tile);
                }

                if (randomChannels.Count > 2)
                {
                    var channel = randomChannels[2];

                    var tile = new ConversationTileControl
                    {
                        Margin = new Thickness(2.0),
                        Size = 68.0,
                        Fill = (Brush)fillConverter.Convert(channel.Index, null, null, null),
                        Text = (string)textConverter.Convert(channel, null, null, null),
                        RenderTransform = new CompositeTransform { TranslateX = 0.0, TranslateY = 36.0, ScaleY = 0.5, ScaleX = 0.5 }
                    };

                    var binding = new Binding("Photo")
                    {
                        Converter = photoConverter,
                        Source = channel
                    };
                    tile.SetBinding(SourceProperty, binding);

                    _feedGrid.Children.Add(tile);
                }

                if (randomChannels.Count > 3)
                {
                    var channel = randomChannels[3];

                    var tile = new ConversationTileControl
                    {
                        Margin = new Thickness(2.0),
                        Size = 68.0,
                        Fill = (Brush)fillConverter.Convert(channel.Index, null, null, null),
                        Text = (string)textConverter.Convert(channel, null, null, null),
                        RenderTransform = new CompositeTransform { TranslateX = 36.0, TranslateY = 36.0, ScaleY = 0.5, ScaleX = 0.5 }
                    };

                    var binding = new Binding("Photo")
                    {
                        Converter = photoConverter,
                        Source = channel
                    };
                    tile.SetBinding(SourceProperty, binding);

                    _feedGrid.Children.Add(tile);
                }

                var storyboard = new Storyboard();
                var opacityInAnimation = new DoubleAnimation{ From = 0.0, To = 1.0, Duration = TimeSpan.FromSeconds(0.5) };
                Storyboard.SetTargetProperty(opacityInAnimation, new PropertyPath("Opacity"));
                Storyboard.SetTarget(opacityInAnimation, _feedGrid);
                storyboard.Children.Add(opacityInAnimation);
                storyboard.Begin();
                storyboard.Completed += (sender, args) =>
                {
                    if (previousGrid != null) Root.Children.Remove(previousGrid);
                };

                Root.Children.Add(_feedGrid);
            }

            Root.Children.Remove(TileGrid);
            if (_feedGrid != null) _feedGrid.Children.Remove(TileGrid);
            //_feedGrid.Children.Add(TileGrid);

            //TileGrid
            TileGrid.RenderTransform = new CompositeTransform { TranslateX = 2, TranslateY = 2, ScaleY = 0.472, ScaleX = 0.472 };
            TileGrid.Visibility = Visibility.Visible;
            _feedGrid.Visibility = Visibility.Visible;
            if (_favedGrid != null) _favedGrid.Visibility = Visibility.Collapsed;
            
        }

        private Grid _favedGrid;

        private void GoToFavedTemplate()
        {
            if (_favedGrid == null)
            {
                _favedGrid = new Grid { Width = Size > 0.0 ? Size : 62.0, Height = Size > 0.0 ? Size : 62.0 };
                var ellipse = new Ellipse
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Fill = (Brush)Application.Current.Resources["TelegramBadgeAccentBrush"]
                };
                var icon = new Image
                {
                    Source = new BitmapImage(new Uri("/Images/Placeholder/ic_bookmark.png", UriKind.Relative)),
                    Margin = new Thickness(12.0)
                };

                _favedGrid.Children.Add(ellipse);
                _favedGrid.Children.Add(icon);

                Root.Children.Add(_favedGrid);
            }

            Root.Children.Remove(TileGrid);
            if (_feedGrid != null) _feedGrid.Children.Remove(TileGrid);
            Root.Children.Add(TileGrid);

            TileGrid.RenderTransform = null;
            TileGrid.Visibility = Visibility.Collapsed;
            _favedGrid.Visibility = Visibility.Visible;
            if (_feedGrid != null) _feedGrid.Visibility = Visibility.Collapsed;
        }

        private void GoToNormalTemplate()
        {
            Root.Children.Remove(TileGrid);
            if (_feedGrid != null) _feedGrid.Children.Remove(TileGrid);
            Root.Children.Add(TileGrid);

            TileGrid.RenderTransform = null;
            TileGrid.Visibility = Visibility;
            if (_favedGrid != null) _favedGrid.Visibility = Visibility.Collapsed;
            if (_feedGrid != null) _feedGrid.Visibility = Visibility.Collapsed;
        }

        public TLObject Object
        {
            get { return (TLObject) GetValue(ObjectProperty); }
            set { SetValue(ObjectProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
            "Size", typeof (double), typeof (ConversationTileControl), new PropertyMetadata(default(double), OnSizeChanged));

        private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var conversationTileControl = d as ConversationTileControl;
            if (conversationTileControl != null)
            {
                var size = (double) e.NewValue;

                conversationTileControl.Ellipse.Width = size;
                conversationTileControl.Ellipse.Height = size;
                conversationTileControl.Image.Width = size;
                conversationTileControl.Image.Height = size;
                if (conversationTileControl._favedGrid != null)
                {
                    conversationTileControl._favedGrid.Width = size;
                    conversationTileControl._favedGrid.Height = size;
                }

                conversationTileControl.ImageClipGeometry.Center = new Point(size / 2.0, size / 2.0);
                conversationTileControl.ImageClipGeometry.RadiusX = size / 2.0;
                conversationTileControl.ImageClipGeometry.RadiusY = size / 2.0;
            }
        }

        public double Size
        {
            get { return (double) GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill", typeof (Brush), typeof (ConversationTileControl), new PropertyMetadata(default(Brush), OnFillChanged));

        private static void OnFillChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var conversationTileControl = d as ConversationTileControl;
            if (conversationTileControl != null)
            {
                var brush = (Brush) e.NewValue;

                conversationTileControl.Ellipse.Fill = brush;
            }
        }

        public Brush Fill
        {
            get { return (Brush) GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof (string), typeof (ConversationTileControl), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var conversationTileControl = d as ConversationTileControl;
            if (conversationTileControl != null)
            {
                var text = (string) e.NewValue;

                conversationTileControl.Label.Text = text;
            }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register(
            "LabelFontSize", typeof (int), typeof (ConversationTileControl), new PropertyMetadata(default(int), OnLabelFontSizeChanged));

        private static void OnLabelFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var conversationTileControl = d as ConversationTileControl;
            if (conversationTileControl != null)
            {
                conversationTileControl.Label.FontSize = (int) e.NewValue;
            }
        }

        public int LabelFontSize
        {
            get { return (int) GetValue(LabelFontSizeProperty); }
            set { SetValue(LabelFontSizeProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            "Source", typeof (ImageSource), typeof (ConversationTileControl), new PropertyMetadata(default(ImageSource), OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var conversationTileControl = d as ConversationTileControl;
            if (conversationTileControl != null)
            {
                var imageSource = e.NewValue as ImageSource;

                conversationTileControl.Image.Source = imageSource;
                conversationTileControl.Image.Visibility = imageSource == null
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                conversationTileControl.Ellipse.Visibility = imageSource != null
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                conversationTileControl.Label.Visibility = imageSource != null
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
        }

        public ImageSource Source
        {
            get { return (ImageSource) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public ConversationTileControl()
        {
            InitializeComponent();
        }
    }
}

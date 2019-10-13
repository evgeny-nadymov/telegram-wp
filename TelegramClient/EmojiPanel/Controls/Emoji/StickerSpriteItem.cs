// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Telegram.Api.TL;
using Telegram.Controls;
using Telegram.Controls.VirtualizedView;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Views.Controls;
using TelegramClient.Views.Dialogs;

namespace Telegram.EmojiPanel.Controls.Emoji
{
    class SearchSpriteItem : VListItemBase
    {
        public override double FixedHeight
        {
            get { return 62.0; }
            set { }
        }

        public string Text { get { return _searchBox.Text; } }

        private WatermarkedTextBox _searchBox;

        public SearchSpriteItem(double panelWidth)
        {
            var inputScope = new InputScope();
            var searchNameValue = new InputScopeName {NameValue = InputScopeNameValue.Search};
            inputScope.Names.Add(searchNameValue);
            _searchBox = new WatermarkedTextBox { FontSize = 17.778, Watermark = AppResources.Search, InputScope = inputScope, Style = (Style)Application.Current.Resources["W10MWatermarkedTextBoxStyle"] };
            _searchBox.GotFocus += (sender, args) =>
            {
                RaiseOpenFullScreen();
            };
            _searchBox.LostFocus += (sender, args) =>
            {
                //RaiseCloseFullScreen();
            };
            _searchBox.KeyUp += (sender, args) =>
            {
                var text = _searchBox.Text;
                Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
                {
                    if (!string.Equals(text, _searchBox.Text)) return;

                    RaiseSearchText(new SearchTextEventArgs{ Text = text});
                });
            };

            Children.Add(_searchBox);

            View.Width = panelWidth;
        }

        public event EventHandler OpenFullScreen;

        protected virtual void RaiseOpenFullScreen()
        {
            var handler = OpenFullScreen;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler CloseFullScreen;

        protected virtual void RaiseCloseFullScreen()
        {
            var handler = CloseFullScreen;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler<SearchTextEventArgs> SearchText;

        protected virtual void RaiseSearchText(SearchTextEventArgs e)
        {
            var handler = SearchText;
            if (handler != null) handler(this, e);
        }

        public void Focus()
        {
            _searchBox.Focus();
        }

        public void Clear()
        {
            _searchBox.Text = string.Empty;
        }
    }

    class FeaturedStickerSpriteItem : StickerSpriteItem
    {
        public override double FixedHeight
        {
            get { return 200.0; }
            set { }
        }

        private readonly TLStickerSetBase _stickerSet;

        public TLStickerSetBase StickerSet { get { return _stickerSet; } }

        public event EventHandler<StickerSetAddedEventArgs> StickerSetAdded;

        protected virtual void RaiseStickerSetAdded()
        {
            var handler = StickerSetAdded;
            if (handler != null) handler(this, new StickerSetAddedEventArgs{ Set = _stickerSet });
        }

        public event EventHandler<StickerSetOpenedEventArgs> StickerSetOpened;

        protected virtual void RaiseStickerSetOpened()
        {
            var handler = StickerSetOpened;
            if (handler != null) handler(this, new StickerSetOpenedEventArgs { Set = _stickerSet });
        }

        public FeaturedStickerSpriteItem(TLStickerSetBase stickerSetBase, int columns, IList<TLStickerItem> stickers, double stickerHeight, double panelWidth, MouseEventHandler onStickerMouseEnter, bool showEmoji = false)
        {
            _stickerSet = stickerSetBase;

            FeaturedStickerSetControl captionPanel = null;
            var stickerSet32 = stickerSetBase as TLStickerSet32;
            if (stickerSet32 != null)
            {
                captionPanel = new FeaturedStickerSetControl();
                captionPanel.DataContext = stickerSetBase;
                captionPanel.Added += (o, e) => RaiseStickerSetAdded();
                captionPanel.Opened += (o, e) => RaiseStickerSetOpened();
            }

            Stickers = stickers;

            _stickerHeight = stickerHeight;

            var panelMargin = new Thickness(4.0, 0.0, 4.0, 0.0);
            var panelActualWidth = panelWidth - panelMargin.Left - panelMargin.Right;
            //472, 438

            var stackPanel = new Grid { Width = panelActualWidth, Margin = panelMargin, Background = new SolidColorBrush(Colors.Transparent) };

            for (var i = 0; i < columns; i++)
            {
                stackPanel.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (var i = 0; i < stickers.Count; i++)
            {
                var binding = new Binding
                {
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath("Self"),
                    Converter = new DefaultPhotoConverter(),
                    ConverterParameter = StickerHeight
                };

                var stickerImage = new Image
                {
                    Height = StickerHeight,
                    Margin = new Thickness(0, 12, 0, 12),
                    VerticalAlignment = VerticalAlignment.Top,
                    CacheMode = new BitmapCache()
                };
                if (onStickerMouseEnter != null)
                {
                    stickerImage.MouseEnter += onStickerMouseEnter;
                }
                stickerImage.SetBinding(Image.SourceProperty, binding);
                StickerImage = stickerImage;

                var grid = new Grid();
                grid.Children.Add(stickerImage);

                if (showEmoji)
                {
                    var document22 = stickers[i].Document as TLDocument22;
                    if (document22 != null)
                    {
                        var bytes = Encoding.BigEndianUnicode.GetBytes(document22.Emoticon);
                        var bytesStr = BrowserNavigationService.ConvertToHexString(bytes);

                        var emojiImage = new Image
                        {
                            Height = 32,
                            Width = 32,
                            Margin = new Thickness(12, 12, 12, 12),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Source = new BitmapImage(new Uri(string.Format("/Assets/Emoji/Separated/{0}.png", bytesStr), UriKind.RelativeOrAbsolute))
                        };
                        grid.Children.Add(emojiImage);
                    }
                }

                var listBoxItem = new ListBoxItem { Content = grid, DataContext = stickers[i] };
                Microsoft.Phone.Controls.TiltEffect.SetIsTiltEnabled(listBoxItem, true);
                listBoxItem.Tap += FeaturedSticker_OnTap;
                //grid.DataContext = stickers[i];
                Grid.SetColumn(listBoxItem, i);
                stackPanel.Children.Add(listBoxItem);
            }

            if (captionPanel != null)
            {
                var panel = new StackPanel();
                panel.Children.Add(captionPanel);
                panel.Children.Add(stackPanel);

                Children.Add(panel);
            }
            else
            {
                Children.Add(stackPanel);
            }

            View.Width = panelWidth;
        }

        protected void FeaturedSticker_OnTap(object sender, GestureEventArgs e)
        {
            var sticker = ((FrameworkElement)sender).DataContext as TLStickerItem;
            if (sticker == null) return;

            RaiseStickerSelected(new StickerSelectedEventArgs { Sticker = sticker, Set = StickerSet });
        }
    }

    class StickerSpriteItem : VListItemBase
    {
//#if DEBUG
//        ~StickerSpriteItem()
//        {
//            Api.Helpers.Execute.BeginOnUIThread(() => MessageBox.Show("Dispose"));
//        }
//#endif

        private double _fixedHeight = 120.0;

        public override double FixedHeight
        {
            get { return _fixedHeight; }
            set { _fixedHeight = value; }
        }

        protected double _stickerHeight = 96.0;

        public double StickerHeight
        {
            get { return _stickerHeight; }
        }

        public IList<TLStickerItem> Stickers { get; set; } 

        public Image StickerImage { get; protected set; }

        protected StickerSpriteItem()
        {
            
        }

        public StickerSpriteItem(int columns, IList<TLStickerItem> stickers, double stickerHeight, double panelWidth, MouseEventHandler onStickerMouseEnter, bool showEmoji = false)
        {
            Stickers = stickers;

            _stickerHeight = stickerHeight;

            var panelMargin = new Thickness(4.0, 0.0, 4.0, 0.0);
            var panelActualWidth = panelWidth - panelMargin.Left - panelMargin.Right;
            //472, 438
            var stackPanel = new Grid{ Width = panelActualWidth, Margin = panelMargin, Background = new SolidColorBrush(Colors.Transparent) };

            for (var i = 0; i < columns; i++)
            {
                stackPanel.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (var i = 0; i < stickers.Count; i++)
            {
                var binding = new Binding
                {
                    Mode = BindingMode.OneWay,
                    Path = new PropertyPath("Self"),
                    Converter = new DefaultPhotoConverter(),
                    ConverterParameter = StickerHeight
                };

                var stickerImage = new Image
                {
                    Height = StickerHeight,
                    Margin = new Thickness(0, 12, 0, 12),
                    VerticalAlignment = VerticalAlignment.Top,
                    CacheMode = new BitmapCache()
                };
                if (onStickerMouseEnter != null)
                {
                    stickerImage.MouseEnter += onStickerMouseEnter;
                }
                stickerImage.SetBinding(Image.SourceProperty, binding);
                StickerImage = stickerImage;

                var grid = new Grid();
                grid.Children.Add(stickerImage);

                if (showEmoji)
                {
                    var document22 = stickers[i].Document as TLDocument22;
                    if (document22 != null)
                    {
                        var bytes = Encoding.BigEndianUnicode.GetBytes(document22.Emoticon);
                        var bytesStr = BrowserNavigationService.ConvertToHexString(bytes);

                        var emojiImage = new Image
                        {
                            Height = 32,
                            Width = 32,
                            Margin = new Thickness(12, 12, 12, 12),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom,
                            Source = new BitmapImage(new Uri(string.Format("/Assets/Emoji/Separated/{0}.png", bytesStr), UriKind.RelativeOrAbsolute))
                        };
                        grid.Children.Add(emojiImage);
                    }
                }

                var listBoxItem = new ListBoxItem {Content = grid, DataContext = stickers[i]};
                Microsoft.Phone.Controls.TiltEffect.SetIsTiltEnabled(listBoxItem, true);
                listBoxItem.Tap += Sticker_OnTap;
                //grid.DataContext = stickers[i];
                Grid.SetColumn(listBoxItem, i);
                stackPanel.Children.Add(listBoxItem);
            }

            Children.Add(stackPanel);

            View.Width = panelWidth;
        }

        public event EventHandler<StickerSelectedEventArgs> StickerSelected;

        protected virtual void RaiseStickerSelected(StickerSelectedEventArgs e)
        {
            var handler = StickerSelected;
            if (handler != null) handler(this, e);
        }

        protected void Sticker_OnTap(object sender, GestureEventArgs e)
        {
            var sticker = ((FrameworkElement) sender).DataContext as TLStickerItem;
            if (sticker == null) return;

            RaiseStickerSelected(new StickerSelectedEventArgs { Sticker = sticker });
        }
    }

    class StickerHeaderSpriteItem : VListItemBase
    {
        public override double FixedHeight
        {
            get { return 23.65 + 6.0 + 6.0; }
            set { }
        }

        public StickerHeaderSpriteItem(TLStickerSetBase stickerSetBase, double panelWidth)
        {
            var label = new TextBlock { FontSize = 17.778, Margin = new Thickness(12.0, 6.0, 12.0, 6.0), Text = stickerSetBase.Title.ToString(), Style = (Style) Application.Current.Resources["PhoneTextSubtleStyle"] };
            Children.Add(label);

            View.Width = panelWidth;
            View.IsHitTestVisible = false;
        }
    }

    class StickerFooterSpriteItem : VListItemBase
    {
        public override double FixedHeight { get; set; }

        public StickerFooterSpriteItem(double panelWidth)
        {
            View.Width = panelWidth;
            View.IsHitTestVisible = false;
        }
    }

    public class StickerSelectedEventArgs : EventArgs
    {
        public TLStickerItem Sticker { get; set; }

        public TLStickerSetBase Set { get; set; }
    }

    public class StickerSetAddedEventArgs : EventArgs
    {
        public TLStickerSetBase Set { get; set; }
    }

    public class StickerSetOpenedEventArgs : EventArgs
    {
        public TLStickerSetBase Set { get; set; }
    }

    public class SearchTextEventArgs : EventArgs
    {
        public string Text { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Telegram.Controls.VirtualizedView;

namespace EmojiPanel.Controls.Emoji
{
    public class EmojiSpriteItem : VListItemBase
    {
        public int CategoryIndex;
        public int SpriteOffset;
        public int Rows;

        public EventHandler<EmojiDataItem> EmojiSelected = delegate { };

        public EmojiSpriteItem(int categoryIndex, int spriteOffset)
        {
            CategoryIndex = categoryIndex;
            SpriteOffset = spriteOffset;
            Rows = EmojiData.SpriteRowsCountByCategory[categoryIndex][spriteOffset];

            var emojiInCategory = EmojiData.CodesByCategory[categoryIndex];
            ulong[] emojis = null;
            emojis = spriteOffset != 0 ? 
                emojiInCategory.Skip(spriteOffset*EmojiData.ItemsInSprite).Take(EmojiData.ItemsInSprite).ToArray() : 
                emojiInCategory.Take(EmojiData.ItemsInSprite).ToArray();

            View.Width = SpriteWidth + 8;
            var decodePixelWidth = SpriteWidth;
            switch (Application.Current.Host.Content.ScaleFactor)
            {
                case 100:
                    break;
                case 150:
                    decodePixelWidth = 711;
                    break;
                case 160:
                    decodePixelWidth = 758;
                    break;
            }

            var image = new Image
            {
                Width = SpriteWidth,
                Source = new BitmapImage
                {
                    DecodePixelWidth = decodePixelWidth,
                    DecodePixelType = DecodePixelType.Physical
                    //UriSource = spriteUri
                },
                Margin = new Thickness(4, 1, 4, 1),
                VerticalAlignment = VerticalAlignment.Top
            };
            Children.Add(image);

            View.MouseLeftButtonDown += ViewOnMouseLeftButtonDown;
            View.LostMouseCapture += ViewOnLostMouseCapture;
            View.MouseLeftButtonUp += ViewOnLostMouseCapture;
            View.MouseLeave += ViewOnLostMouseCapture;
            View.Tap += ViewOnTap;

            CreateBorders();
        }

        public EmojiSpriteItem(Uri spriteUri, int categoryIndex, int spriteOffset)
        {
            CategoryIndex = categoryIndex;
            SpriteOffset = spriteOffset;
            Rows = EmojiData.SpriteRowsCountByCategory[categoryIndex][spriteOffset];

            View.Width = SpriteWidth + 8;
            var decodePixelWidth = SpriteWidth;
            switch (Application.Current.Host.Content.ScaleFactor)
            {
                case 100:
                    break;
                case 150:
                    decodePixelWidth = 711;
                    break;
                case 160:
                    decodePixelWidth = 758;
                    break;
            }

            var image = new Image
            {
                Width = SpriteWidth,
                Source = new BitmapImage
                {
                    DecodePixelWidth = decodePixelWidth,
                    DecodePixelType = DecodePixelType.Physical,
                    UriSource = spriteUri
                },
                Margin = new Thickness(4, 1, 4, 1),
                VerticalAlignment = VerticalAlignment.Top
            };
            Children.Add(image);

            View.MouseLeftButtonDown += ViewOnMouseLeftButtonDown;
            View.LostMouseCapture += ViewOnLostMouseCapture;
            View.MouseLeftButtonUp += ViewOnLostMouseCapture;
            View.MouseLeave += ViewOnLostMouseCapture;
            View.Tap += ViewOnTap;

            CreateBorders();
        }

        private static void ViewOnLostMouseCapture(object sender, MouseEventArgs mouseEventArgs)
        {
            ClearCurrentHighlight();
        }

        public static void ClearCurrentHighlight()
        {
            if (_currentHighlight == null) return;

            var parent = _currentHighlight.Parent as Grid;
            if (parent != null)
                parent.Children.Remove(_currentHighlight);

            _currentHighlight = null;
        }

        private void ViewOnMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            var point = args.GetPosition(View);
            var column = (int) Math.Ceiling(point.X / ColumnWidth);
            var row = (int) Math.Ceiling(point.Y / RowHeight);

            if (column <= 0 || row <= 0) return;
            if (Rows < MaxRowsInSprite && row == Rows)
            {
                if (EmojiData.ItemsInRow - EmojiData.SpriteMissingCellsByCategory[CategoryIndex] < column)
                    return;
            }

            var emojiHoverBackground = new Rectangle
            {
                Width = ColumnWidth - 2, //width without 2px border
                Height = RowHeight,
                Fill = (Brush) Application.Current.Resources["PhoneAccentBrush"],
                Margin = new Thickness((column - 1) * 79 + 4, (row - 1) * 70 + 2, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };
            View.Children.Insert(0, emojiHoverBackground);

            ClearCurrentHighlight();
            _currentHighlight = emojiHoverBackground;
        }

        private void ViewOnTap(object sender, GestureEventArgs args)
        {
            var point = args.GetPosition(View);
            var column = (int) Math.Ceiling(point.X / 79);
            var row = (int) Math.Ceiling(point.Y / 70);

            if (column <= 0 || row <= 0) return;

            //Debug.WriteLine("{0}-{1}", column, row);

            var itemIndex = (row - 1) * EmojiData.ItemsInRow + (column - 1);

            var emoji = EmojiDataItem.GetByIndex(CategoryIndex, SpriteOffset, itemIndex);
            if (emoji != null)
                EmojiSelected(null, emoji);
        }

        private static Rectangle _currentHighlight;

        private void CreateBorders()
        {
            for (int i = 0; i < Rows + 1; i++)
            {
                var line = new Rectangle
                {
                    Width = SpriteWidth + 4,
                    Height = 2,
                    Fill = (Brush) Application.Current.Resources["PhoneChromeBrush"],
                    Margin = new Thickness(0, i * RowHeight, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                Children.Add(line);
            }

            for (int i = 0; i < 5; i++)
            {
                var line = new Rectangle
                {
                    Width = 2,
                    Height = RowHeight * Rows,
                    Fill = (Brush) Application.Current.Resources["PhoneChromeBrush"],
                    Margin = new Thickness((i + 1) * ColumnWidth + 2, 0, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                Children.Add(line);
            }

            if (Rows < MaxRowsInSprite)
            {
                var missingRows = EmojiData.SpriteMissingCellsByCategory[CategoryIndex];
                var startIndex = EmojiData.ItemsInRow - missingRows;

                var width = missingRows * ColumnWidth;
                var horizontalOffset = startIndex * ColumnWidth + 4;

                var rect = new Rectangle
                {
                    Fill = (Brush) Application.Current.Resources["PhoneChromeBrush"],
                    Width = width,
                    Height = RowHeight,
                    Margin = new Thickness(horizontalOffset, (Rows - 1) * RowHeight, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                Children.Add(rect);
            }
        }

        public const int SpriteWidth = 472;
        public const int SpriteHeight = 420;
        public const int ColumnWidth = 79;
        public const int RowHeight = 70; // 105 in pixel logic

        public const int MaxRowsInSprite = 6;

        public override double FixedHeight
        {
            get { return RowHeight * Rows; }
            set { }
        }
    }
}

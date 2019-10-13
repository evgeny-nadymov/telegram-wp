using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using EmojiPanel.Controls.Utilites;
using Microsoft.Phone.Controls;
using Telegram.Controls.VirtualizedView;

namespace EmojiPanel.Controls.Emoji
{
    public partial class EmojiControl
    {
        private List<VListItemBase> _category1Sprites;
        private List<VListItemBase> _category2Sprites;
        private List<VListItemBase> _category3Sprites;
        private List<VListItemBase> _category4Sprites;
        private List<VListItemBase> _category5Sprites;

        public EventHandler<bool> IsOpenedChanged = delegate { };

        public TextBox TextBoxTarget { get; set; }

        private const int AlbumOrientationHeight = 328;
        private const int PortraitOrientationHeight = 408;

        private bool _isOpen;
        private bool _isPortrait = true;
        private bool _isTextBoxTargetFocused;
        private bool _isBlocked; // Block IsOpen during animation
        private int _currentCategory;
        private bool _wasRendered;
        private readonly TranslateTransform _frameTransform;
        private static EmojiControl _instance;

        public static EmojiControl GetInstance()
        {
            return _instance ?? (_instance = new EmojiControl());
        }

        public static readonly DependencyProperty RootFrameTransformProperty = DependencyProperty.Register(
                "RootFrameTransform",
                typeof(double),
                typeof(EmojiControl),
                new PropertyMetadata(OnRootFrameTransformChanged));

        public EmojiControl()
        {
            InitializeComponent();

            var frame = (Frame)Application.Current.RootVisual;
            _frameTransform = ((TranslateTransform)((TransformGroup)frame.RenderTransform).Children[0]);
            var binding = new Binding("Y")
            {
                Source = _frameTransform
            };
            SetBinding(RootFrameTransformProperty, binding);

            VirtPanel.InitializeWithScrollViewer(CSV);
            VirtPanel.ScrollPositionChanged += VirtPanelOnScrollPositionChanged;
            SizeChanged += OnSizeChanged;
            OnSizeChanged(null, null);

            LoadButtons();
            CurrentCategory = 0;


        }

        public void BindTextBox(TextBox textBox)
        {
            TextBoxTarget = textBox;
            textBox.GotFocus += TextBoxOnGotFocus;
            textBox.LostFocus += TextBoxOnLostFocus;
        }

        public void UnbindTextBox()
        {
            TextBoxTarget.GotFocus -= TextBoxOnGotFocus;
            TextBoxTarget.LostFocus -= TextBoxOnLostFocus;
            TextBoxTarget = null;
        }

        public bool IsOpen
        {
            get
            {
                return !_isTextBoxTargetFocused && _isOpen;
            }
            set
            {
                // Dont hide EmojiControl when keyboard is shown (or to be shown)
                if (!_isTextBoxTargetFocused && _isOpen == value || _isBlocked) return;

                if (value) Open();
                else Hide();
                IsOpenedChanged(null, value);
            }
        }

        private void Open()
        {
            _isOpen = true;

            VisualStateManager.GoToState(TextBoxTarget, "Focused", false);

            var frame = (PhoneApplicationFrame)Application.Current.RootVisual;
            EmojiContainer.Visibility = Visibility.Visible;
            frame.BackKeyPress += OnBackKeyPress;

            if (!(EmojiContainer.RenderTransform is TranslateTransform))
                EmojiContainer.RenderTransform = new TranslateTransform();
            var transform = (TranslateTransform)EmojiContainer.RenderTransform;

            var offset = _isPortrait ? PortraitOrientationHeight : AlbumOrientationHeight;
            EmojiContainer.Height = offset;

            var from = 0;

            if (_frameTransform.Y < 0) // Keyboard is in view
            {
                from = (int)_frameTransform.Y;
                //_frameTransform.Y = -offset;
                //transform.Y = offset;// -72;
            }
            transform.Y = offset;// -72

            if (from == offset) return;

            frame.IsHitTestVisible = false;
            _isBlocked = true;

            var storyboard = new Storyboard();
            var doubleTransformFrame = new DoubleAnimation
            {
                From = from,
                To = -offset,
                Duration = TimeSpan.FromMilliseconds(440),
                EasingFunction = new ExponentialEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 6
                }
            };
            storyboard.Children.Add(doubleTransformFrame);
            Storyboard.SetTarget(doubleTransformFrame, _frameTransform);
            Storyboard.SetTargetProperty(doubleTransformFrame, new PropertyPath("Y"));

            EmojiContainer.Dispatcher.BeginInvoke(async () =>
            {
                storyboard.Begin();

                if (_frameTransform.Y < 0) // Keyboard is in view
                {
                    Focus();
                    TextBoxTarget.Dispatcher.BeginInvoke(() // no effect without dispatcher
                        => VisualStateManager.GoToState(TextBoxTarget, "Focused", false));
                }

                if (_wasRendered) return;
                await Task.Delay(50);
                LoadCategory(0);
            });

            storyboard.Completed += (sender, args) =>
            {
                frame.IsHitTestVisible = true;
                _isBlocked = false;
            };
        }

        private void Hide()
        {
            _isOpen = false;

            var frame = (PhoneApplicationFrame)Application.Current.RootVisual;
            frame.BackKeyPress -= OnBackKeyPress;

            if (_isTextBoxTargetFocused)
            {
                _frameTransform.Y = 0;

                EmojiContainer.Visibility = Visibility.Collapsed;

                return;
            }

            VisualStateManager.GoToState(TextBoxTarget, "Unfocused", false);

            frame.IsHitTestVisible = false;
            _isBlocked = true;

            var transform = (TranslateTransform)EmojiContainer.RenderTransform;

            var storyboard = new Storyboard();
            var doubleTransformFrame = new DoubleAnimation
            {
                From = -transform.Y,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(440),
                EasingFunction = new ExponentialEase
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 6
                }
            };
            storyboard.Children.Add(doubleTransformFrame);
            Storyboard.SetTarget(doubleTransformFrame, _frameTransform);
            Storyboard.SetTargetProperty(doubleTransformFrame, new PropertyPath("Y"));
            storyboard.Begin();

            storyboard.Completed += (sender, args) =>
            {
                EmojiContainer.Visibility = Visibility.Collapsed;

                frame.IsHitTestVisible = true;
                _isBlocked = false;
                transform.Y = 0;
            };

        }

        #region _isTextBoxTargetFocused listeners
        private void TextBoxOnGotFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            _isTextBoxTargetFocused = true;
        }
        private void TextBoxOnLostFocus(object sender, RoutedEventArgs routedEventArgs)
        {
            _isTextBoxTargetFocused = false;
        }
        #endregion

        /// <summary>
        /// Hide instance on pressing hardware Back button. Fires only when instance is opened.
        /// </summary>
        private void OnBackKeyPress(object sender, CancelEventArgs cancelEventArgs)
        {
            IsOpen = false;
            cancelEventArgs.Cancel = true;
        }

        /// <summary>
        /// Clear current highlight on scroll
        /// </summary>
        private static void VirtPanelOnScrollPositionChanged(object sender, MyVirtualizingPanel.ScrollPositionChangedEventAgrs scrollPositionChangedEventAgrs)
        {
            EmojiSpriteItem.ClearCurrentHighlight();
        }

        /// <summary>
        /// Changes tabs in UI and _currentCategory property
        /// </summary>
        public int CurrentCategory
        {
            get { return _currentCategory; }
            set
            {
                var previousCategory = GetCategoryButtonByIndex(_currentCategory);
                var nextCategory = GetCategoryButtonByIndex(value);

                if (previousCategory != null)
                    previousCategory.Background = new SolidColorBrush(Color.FromArgb(255, 71, 71, 71));

                nextCategory.Background = (Brush)Application.Current.Resources["PhoneAccentBrush"];
                _currentCategory = value;
            }
        }

        public async void LoadCategory(int index)
        {
            VirtPanel.ClearItems();

            if (_currentCategory == RecentsCategoryIndex)
                UnloadRecents();

            if (index == RecentsCategoryIndex)
            {
                LoadRecents();
                return;
            }

            List<VListItemBase> sprites = null;

            switch (index)
            {
                case 0:
                    sprites = _category1Sprites;
                    break;
                case 1:
                    sprites = _category2Sprites;
                    break;
                case 2:
                    sprites = _category3Sprites;
                    break;
                case 3:
                    sprites = _category4Sprites;
                    break;
                case 4:
                    sprites = _category5Sprites;
                    break;
            }

            if (sprites == null)
            {
                sprites = new List<VListItemBase>();

                for (var i = 0; i < EmojiData.SpritesByCategory[index].Length; i++)
                {
                    //var item = new EmojiSpriteItem(index, i);
                    var item = new EmojiSpriteItem(EmojiData.SpritesByCategory[index][i], index, i);
                    item.EmojiSelected += OnEmojiSelected;
                    sprites.Add(item);
                }

                switch (index)
                {
                    case 0:
                        _category1Sprites = sprites;
                        break;
                    case 1:
                        _category2Sprites = sprites;
                        break;
                    case 2:
                        _category3Sprites = sprites;
                        break;
                    case 3:
                        _category4Sprites = sprites;
                        break;
                    case 4:
                        _category5Sprites = sprites;
                        break;
                }
            }

            CurrentCategory = index;

            VirtPanel.AddItems(new List<VListItemBase> { sprites[0] });
            CreateButtonsBackgrounds(index);

            if (!_wasRendered)
            {
                // Display LoadingProgressBar only once
                LoadingProgressBar.Visibility = Visibility.Collapsed;
                _wasRendered = true;
            }

            // Delayed rendering of the rest parts - speeds up initial load
            await Task.Delay(100);
            if (_currentCategory != index)
                return;

            var listList = sprites.ToList();
            listList.RemoveAt(0);
            VirtPanel.AddItems(listList);
        }

        public static void OnRootFrameTransformChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            ((EmojiControl)source).OnRootFrameTransformChanged();
        }

        public void OnRootFrameTransformChanged()
        {
            if (!_isOpen) return;

            var offset = _isPortrait ? -PortraitOrientationHeight : -AlbumOrientationHeight;
            _frameTransform.Y = offset;
        }

        #region Recents
        public void LoadRecents()
        {
            CurrentCategory = RecentsCategoryIndex;

            var recents = EmojiData.Recents;
            //recents = recents.ToList();
        }

        public void UnloadRecents()
        {

        }


        #endregion Recents

        private void OnEmojiSelected(object sender, EmojiDataItem emojiDataItem)
        {
            TextBoxTarget.Dispatcher.BeginInvoke(() =>
            {
                var selectionStart = TextBoxTarget.SelectionStart;
                TextBoxTarget.Text = TextBoxTarget.Text.Insert(selectionStart, emojiDataItem.String);
                TextBoxTarget.Select(selectionStart + emojiDataItem.String.Length, 0);
            });

            if (_currentCategory == RecentsCategoryIndex) return;

            var that = emojiDataItem;
            ThreadPool.QueueUserWorkItem(state => EmojiData.AddToRecents(that));
        }


        /// <summary>
        /// Emoji control backspace button logic
        /// </summary>
        private void BackspaceButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var text = TextBoxTarget.Text;
            var selectionStart = TextBoxTarget.SelectionStart;

            if (text.Length <= 0) return;
            if (selectionStart == 0) return;

            int toSubstring;

            if (text.Length > 1)
            {
                var prevSymbol = text[selectionStart - 2];
                var prevBytes = BitConverter.GetBytes(prevSymbol);

                var curSymbol = text[selectionStart - 1];
                var curBytes = BitConverter.GetBytes(curSymbol);

                if (prevBytes[1] == 0xD8 && (prevBytes[0] == 0x3D || prevBytes[0] == 0x3C))
                    toSubstring = 2;
                else if (curBytes[1] == 0x20 && curBytes[0] == 0xE3)
                    toSubstring = 2;
                else
                    toSubstring = 1;
            }
            else
            {
                toSubstring = 1;
            }

            TextBoxTarget.Text = text.Remove(selectionStart - toSubstring, toSubstring);
            TextBoxTarget.SelectionStart = selectionStart - toSubstring;
        }

        #region User Interface

        private readonly Button _abcButton = new Button { ClickMode = ClickMode.Release };
        private readonly Button _recentsButton = new Button { ClickMode = ClickMode.Press };
        private readonly Button _cat0Button = new Button { ClickMode = ClickMode.Press };
        private readonly Button _cat1Button = new Button { ClickMode = ClickMode.Press };
        private readonly Button _cat2Button = new Button { ClickMode = ClickMode.Press };
        private readonly Button _cat3Button = new Button { ClickMode = ClickMode.Press };
        private readonly Button _cat4Button = new Button { ClickMode = ClickMode.Press };
        private readonly RepeatButton _backspaceButton = new RepeatButton { ClickMode = ClickMode.Release, Interval = 100 };
        public const int RecentsCategoryIndex = 5;

        private Button GetCategoryButtonByIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return _cat0Button;
                case 1:
                    return _cat1Button;
                case 2:
                    return _cat2Button;
                case 3:
                    return _cat3Button;
                case 4:
                    return _cat4Button;
                case RecentsCategoryIndex:
                    return _recentsButton;
                default:
                    return null;
            }
        }
        public void LoadButtons()
        {
            var buttonStyle = (Style)Resources["CategoryButtonStyle"];
            _abcButton.Style = buttonStyle;
            _recentsButton.Style = buttonStyle;
            _cat0Button.Style = buttonStyle;
            _cat1Button.Style = buttonStyle;
            _cat2Button.Style = buttonStyle;
            _cat3Button.Style = buttonStyle;
            _cat4Button.Style = buttonStyle;
            _backspaceButton.Style = (Style)Resources["RepeatButtonStyle"];

            _abcButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri("emoji.abc")),
                Width = 34,
                Height = 32
            };
            _recentsButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri("emoji.recent")),
                Width = 34,
                Height = 32
            };
            _cat0Button.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri("emoji.category.1")),
                Width = 34,
                Height = 32
            };
            _cat1Button.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri("emoji.category.2")),
                Width = 34,
                Height = 32
            };
            _cat2Button.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri("emoji.category.3")),
                Width = 34,
                Height = 32
            };
            _cat3Button.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri("emoji.category.4")),
                Width = 34,
                Height = 32
            };
            _cat4Button.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri("emoji.category.5")),
                Width = 34,
                Height = 32
            };
            _backspaceButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri("emoji.backspace")),
                Width = 34,
                Height = 32
            };

            Grid.SetColumn(_abcButton, 0);
            Grid.SetColumn(_recentsButton, 1);
            Grid.SetColumn(_cat0Button, 2);
            Grid.SetColumn(_cat1Button, 3);
            Grid.SetColumn(_cat2Button, 4);
            Grid.SetColumn(_cat3Button, 5);
            Grid.SetColumn(_cat4Button, 6);
            Grid.SetColumn(_backspaceButton, 7);

            ButtonsGrid.Children.Add(_abcButton);
            ButtonsGrid.Children.Add(_recentsButton);
            ButtonsGrid.Children.Add(_cat0Button);
            ButtonsGrid.Children.Add(_cat1Button);
            ButtonsGrid.Children.Add(_cat2Button);
            ButtonsGrid.Children.Add(_cat3Button);
            ButtonsGrid.Children.Add(_cat4Button);
            ButtonsGrid.Children.Add(_backspaceButton);

            _abcButton.Click += AbcButtonOnClick;
            _cat0Button.Click += CategoryButtonClick;
            _cat1Button.Click += CategoryButtonClick;
            _cat2Button.Click += CategoryButtonClick;
            _cat3Button.Click += CategoryButtonClick;
            _cat4Button.Click += CategoryButtonClick;
            _recentsButton.Click += CategoryButtonClick;
            _backspaceButton.Click += BackspaceButtonOnClick;
        }

        private void AbcButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            TextBoxTarget.Focus();
        }

        private void CategoryButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (sender == _cat0Button)
                LoadCategory(0);
            else if (sender == _cat1Button)
                LoadCategory(1);
            else if (sender == _cat2Button)
                LoadCategory(2);
            else if (sender == _cat3Button)
                LoadCategory(3);
            else if (sender == _cat4Button)
                LoadCategory(4);
            else if (sender == _recentsButton)
                LoadCategory(RecentsCategoryIndex);
        }

        private void CreateButtonsBackgrounds(int categoryIndex)
        {
            var sprites = EmojiData.SpriteRowsCountByCategory[categoryIndex];

            for (var i = 0; i < sprites.Length; i++)
            {
                var rowsCount = sprites[i];

                var block = new Rectangle
                {
                    Width = EmojiSpriteItem.SpriteWidth,
                    Height = EmojiSpriteItem.RowHeight * rowsCount,
                    Fill = new SolidColorBrush(Color.FromArgb(255, 71, 71, 71)),
                    Margin = new Thickness(4, 0, 4, 0)
                };
                Canvas.SetTop(block, (EmojiSpriteItem.SpriteHeight) * i);
                VirtPanel.Children.Insert(0, block);
            }
        }

        private void InitializeOrientation(Orientation orientation)
        {
            switch (orientation)
            {
                case Orientation.Vertical:
                    ButtonsGrid.Height = 78;
                    ButtonsGrid.Margin = new Thickness(0, 6, 0, 0);
                    EmojiContainer.Height = PortraitOrientationHeight;
                    _frameTransform.Y = -PortraitOrientationHeight;
                    break;

                case Orientation.Horizontal:
                    ButtonsGrid.Height = 58;
                    ButtonsGrid.Margin = new Thickness(0, 6, 0, 3);
                    EmojiContainer.Height = AlbumOrientationHeight;
                    _frameTransform.Y = -AlbumOrientationHeight;
                    break;
            }
        }

        #endregion User Interface


        /// <summary>
        /// Orientation change handler
        /// </summary>
        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var currentOrientation = ((PhoneApplicationFrame)Application.Current.RootVisual).Orientation;
            var isPortrait = currentOrientation == PageOrientation.PortraitUp ||
                             currentOrientation == PageOrientation.PortraitDown ||
                             currentOrientation == PageOrientation.Portrait;

            if (_isPortrait == isPortrait && _wasRendered) return;

            _isPortrait = isPortrait;
            InitializeOrientation(isPortrait ? Orientation.Vertical : Orientation.Horizontal);
        }
    }
}

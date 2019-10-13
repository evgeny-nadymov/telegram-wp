// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Caliburn.Micro;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Shell;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Controls;
using Telegram.Controls.Extensions;
using Telegram.Controls.VirtualizedView;
using Telegram.EmojiPanel.Controls.Utilites;
using TelegramClient.Converters;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels;
using TelegramClient.Views;
using TelegramClient.Views.Dialogs;
using Binding = System.Windows.Data.Binding;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using Execute = Telegram.Api.Helpers.Execute;
using UnreadCounter = Telegram.Controls.UnreadCounter;

namespace Telegram.EmojiPanel.Controls.Emoji
{
    public partial class EmojiControl
    {
        public static readonly DependencyProperty IsStickersPanelVisibleProperty = DependencyProperty.Register(
            "IsStickersPanelVisible", typeof (bool), typeof (EmojiControl), new PropertyMetadata(OnIsStickersPanelVisible));

        private static void OnIsStickersPanelVisible(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var emojiControl = d as EmojiControl;
            if (emojiControl != null)
            {
                emojiControl.UpdateButtons((bool)e.NewValue);
            }
        }

        public bool IsStickersPanelVisible
        {
            get { return (bool) GetValue(IsStickersPanelVisibleProperty); }
            set { SetValue(IsStickersPanelVisibleProperty, value); }
        }

        private List<VListItemBase> _category1Sprites;
        private List<VListItemBase> _category2Sprites;
        private List<VListItemBase> _category3Sprites;
        private List<VListItemBase> _category4Sprites;
        private List<VListItemBase> _category5Sprites;

        private List<VListItemBase> _recentStickersSprites;
        private List<VListItemBase> _favedStickersSprites;
        private List<VListItemBase> _groupStickersSprites;
        private List<List<VListItemBase>> _stickersSprites;

        public event EventHandler<IsOpenedEventArgs> IsOpenedChanged;

        private void RaiseIsOpenedChanged(bool isOpened)
        {
            var eventHandler = IsOpenedChanged;

            if (eventHandler != null)
            {
                eventHandler(this, new IsOpenedEventArgs { IsOpened = isOpened });
            }
        }

        public event EventHandler SettingsButtonClick;

        protected virtual void RaiseSettingsButtonClick()
        {
            var handler = SettingsButtonClick;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        public event EventHandler<StickerSetAddedEventArgs> StickerSetAdded;

        protected virtual void RaiseStickerSetAdded(StickerSetAddedEventArgs args)
        {
            var handler = StickerSetAdded;
            if (handler != null) handler(this, args);
        }

        public TextBox TextBoxTarget { get; set; }

        private const int FirstStickerSliceCount = 3;

        private const int FeaturedStickersSliceCount = 10;

        private const int AlbumOrientationHeight = 328;

        public const int PortraitOrientationHeight100 = 408;

        public const int PortraitOrientationHeight100WP10 = (int) 341.3333;

        public const int PortraitOrientationHeight112 = 408;

        public const int PortraitOrientationHeight112Software = 400;

        public const int PortraitOrientationHeight150 = 408;

        public const int PortraitOrientationHeight150Software = 400;

        public const int PortraitOrientationHeight160 = 408;

        public const int PortraitOrientationHeight160WP10 = (int) 341.6667;

        public const int PortraitOrientationHeight225 = 332;

        public static int PortraitOrientationHeight
        {
            get
            {
#if WP8
                var deviceName = DeviceStatus.DeviceName;
                var appBar = new ApplicationBar();
                switch (Application.Current.Host.Content.ScaleFactor)
                {
                    case 100:   //Lumia 820             WVGA    480x800
                        if (!string.IsNullOrEmpty(deviceName))
                        {
                            deviceName = deviceName.Replace("-", string.Empty).ToLowerInvariant();

                            //Lumia 640
                            if (deviceName.StartsWith("rm1067")         // 640XL 720x1280 scale=100 wp10
                                )    
                            {
                                return PortraitOrientationHeight100WP10;
                            }
                        }

                        return PortraitOrientationHeight100;
                        break;
                    case 112:   //Lumia 535             qHD     540x960
                        // Software buttons //Lumia 535
                        if (appBar.DefaultSize == 67.0)
                        {
                            return PortraitOrientationHeight112Software;
                        }
                        
                        return PortraitOrientationHeight112;
                        break;
                    case 150:   //HTC 8X, 730, 830      720p    720x1280
                        //Software buttons  //Lumia 730
                        if (appBar.DefaultSize == 67.0)
                        {
                            return PortraitOrientationHeight150Software;
                        }

                        return PortraitOrientationHeight150;
                        break;
                    case 160:   //Lumia 925, 1020       WXGA    768x1280
                        if (!string.IsNullOrEmpty(deviceName))
                        {
                            deviceName = deviceName.Replace("-", string.Empty).ToLowerInvariant();

                            //Lumia 950, 950XL    5,2 5,7 inch   QHD     2560x1440
                            if (deviceName.StartsWith("rm1116")         // 950XL dual sim
                                || deviceName.StartsWith("rm1085")      // 950XL single sim
                                || deviceName.StartsWith("rm1118")      // 950 dual sim
                                || deviceName.StartsWith("rm1104"))     // 950 single sim
                            {
                                return PortraitOrientationHeight160WP10;
                            }
                        }


                        return PortraitOrientationHeight160;
                        break;
                    case 225:   // Lumia 1520, 930      1020p   1080x1920  
                        
                        if (!string.IsNullOrEmpty(deviceName))
                        {
                            deviceName = deviceName.Replace("-", string.Empty).ToLowerInvariant();

                            //Lumia 1520    6 inch 1020p
                            if (deviceName.StartsWith("rm937")
                                || deviceName.StartsWith("rm938")
                                || deviceName.StartsWith("rm939")
                                || deviceName.StartsWith("rm940"))
                            {
                                return PortraitOrientationHeight225;
                            }
                        }

                        //Lumia 930 other 1020p
                        return PortraitOrientationHeight100;
                        break;
                }
#endif

                return PortraitOrientationHeight100;
            }
        }

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

        public static bool TryGetInstance(out EmojiControl instance)
        {
            instance = _instance;

            return instance != null;
        }

        public static bool HasInstance
        {
            get { return _instance != null; }
        }

        public static readonly DependencyProperty RootFrameTransformProperty = DependencyProperty.Register(
                "RootFrameTransform",
                typeof(double),
                typeof(EmojiControl),
                new PropertyMetadata(OnRootFrameTransformChanged));

        public EmojiControl()
        {
            InitializeComponent();

            //var frame = (Frame)Application.Current.RootVisual;
            //_frameTransform = ((TranslateTransform)((TransformGroup)frame.RenderTransform).Children[0]);
            //var binding = new Binding("Y")
            //{
            //    Source = _frameTransform
            //};
            //SetBinding(RootFrameTransformProperty, binding);

            Preview.Margin = new Thickness(0.0, -(800.0 - PortraitOrientationHeight), 0.0, 0.0);


            VirtPanel.InitializeWithScrollViewer(CSV);
            VirtPanel.ScrollPositionChanged += VirtPanel_OnScrollPositionChanged;
            VirtPanel.ScrollStateChanged += VirtPanel_OnScrollStateChanged;

            FeaturedStickersVirtPanel.InitializeWithScrollViewer(FeaturedStickersCSV);
            FeaturedStickersVirtPanel.ScrollPositionChanged += FeaturedStickersVirtPanel_OnScrollPositionChanged;
            FeaturedStickersVirtPanel.ScrollStateChanged += FeaturedStickersVirtPanel_OnScrollStateChanged;
            //SizeChanged += OnSizeChanged;
            OnSizeChanged(null, null);

            LoadButtons();
            LoadCachedFeaturedStickersAsync();
            CurrentCategory = 0;
        }

        private void VirtPanel_OnScrollStateChanged(object sender, ScrollingStateChangedEventArgs e)
        {
            if (_searchSprite == null || string.IsNullOrEmpty(_searchSprite.Text)) return;

            if (!e.NewValue)
            {
                var unreadStickerSets = new List<TLStickerSetBase>();

                var height = CSV.VerticalOffset + CSV.ViewportHeight;
                var lastInViewDisplacement = double.PositiveInfinity;
                MyListItemBase lastInViewItem = null;
                foreach (var child in VirtPanel.Children)
                {
                    var canvasTop = Canvas.GetTop(child);
                    var displacement = Math.Abs(height - canvasTop);
                    if (displacement <= lastInViewDisplacement && canvasTop < height)
                    {
                        var listItem = child as MyListItemBase;
                        if (listItem != null)
                        {
                            lastInViewItem = listItem;
                            lastInViewDisplacement = displacement;
                        }
                    }
                }

                if (lastInViewItem != null)
                {
                    foreach (var item in VirtPanel.VirtItems)
                    {
                        var listItem = item.View;
                        if (listItem != null)
                        {
                            var spriteItem = listItem.VirtSource as FeaturedStickerSpriteItem;
                            if (spriteItem != null && spriteItem.StickerSet.Unread)
                            {
                                unreadStickerSets.Add(spriteItem.StickerSet);
                            }
                        }

                        if (item.View == lastInViewItem)
                        {
                            break;
                        }
                    }
                }

                if (unreadStickerSets.Count > 0)
                {
                    var id = new TLVector<TLLong>();
                    foreach (var unreadStickerSet in unreadStickerSets)
                    {
                        id.Add(unreadStickerSet.Id);
                    }

                    var mtProtoService = IoC.Get<IMTProtoService>();
                    mtProtoService.ReadFeaturedStickersAsync(id,
                        result =>
                        {
                            foreach (var unreadStickerSet in unreadStickerSets)
                            {
                                unreadStickerSet.Unread = false;
                            }

                            if (_featuredStickers != null)
                            {
                                for (var i = 0; i < id.Count; i++)
                                {
                                    for (var j = 0; j < _featuredStickers.Unread.Count; j++)
                                    {
                                        if (_featuredStickers.Unread[j].Value == id[i].Value)
                                        {
                                            _featuredStickers.Unread.RemoveAt(j);
                                            break;
                                        }
                                    }
                                }

                                var cacheService = IoC.Get<IStateService>();
                                cacheService.SaveFeaturedStickersAsync(_featuredStickers);
                            }

                            Execute.BeginOnUIThread(() =>
                            {
                                foreach (var unreadStickerSet in unreadStickerSets)
                                {
                                    unreadStickerSet.NotifyOfPropertyChange(() => unreadStickerSet.Unread);
                                }
                                if (_featuredStickers != null && _featuredStickersCounter != null)
                                {
                                    _featuredStickersCounter.Counter = _featuredStickers.Unread.Count;
                                }
                            });
                        },
                        error => Execute.BeginOnUIThread(() =>
                        {
                            Execute.ShowDebugMessage("messages.readFeaturedStickers error=" + error);
                        }));
                }

            }
        }

        private void FeaturedStickersVirtPanel_OnScrollStateChanged(object sender, ScrollingStateChangedEventArgs e)
        {
            if (CurrentCategory != FeaturedStickersCategoryIndex) return;

            if (!e.NewValue)
            {
                var unreadStickerSets = new List<TLStickerSetBase>();

                var height = FeaturedStickersCSV.VerticalOffset + FeaturedStickersCSV.ViewportHeight;
                var lastInViewDisplacement = double.PositiveInfinity;
                MyListItemBase lastInViewItem = null;
                foreach (var child in FeaturedStickersVirtPanel.Children)
                {
                    var canvasTop = Canvas.GetTop(child);
                    var displacement = Math.Abs(height - canvasTop);
                    if (displacement <= lastInViewDisplacement && canvasTop < height)
                    {
                        var listItem = child as MyListItemBase;
                        if (listItem != null)
                        {
                            lastInViewItem = listItem;
                            lastInViewDisplacement = displacement;
                        }
                    }
                }

                if (lastInViewItem != null)
                {
                    foreach (var item in FeaturedStickersVirtPanel.VirtItems)
                    {
                        var listItem = item.View;
                        if (listItem != null)
                        {
                            var spriteItem = listItem.VirtSource as FeaturedStickerSpriteItem;
                            if (spriteItem != null && spriteItem.StickerSet.Unread)
                            {
                                unreadStickerSets.Add(spriteItem.StickerSet);
                            }
                        }

                        if (item.View == lastInViewItem)
                        {
                            break;
                        }
                    }
                }

                if (unreadStickerSets.Count > 0)
                {
                    var id = new TLVector<TLLong>();
                    foreach (var unreadStickerSet in unreadStickerSets)
                    {
                        id.Add(unreadStickerSet.Id);
                    }

                    //Execute.ShowDebugMessage("message.readFeaturedStickers id=" + string.Join(", ", unreadStickerSets.Select(x => x.Id.Value)));

                    var mtProtoService = IoC.Get<IMTProtoService>();
                    mtProtoService.ReadFeaturedStickersAsync(id,
                        result =>
                        {
                            foreach (var unreadStickerSet in unreadStickerSets)
                            {
                                unreadStickerSet.Unread = false;
                            }

                            if (_featuredStickers != null)
                            {
                                for (var i = 0; i < id.Count; i++)
                                {
                                    for (var j = 0; j < _featuredStickers.Unread.Count; j++)
                                    {
                                        if (_featuredStickers.Unread[j].Value == id[i].Value)
                                        {
                                            _featuredStickers.Unread.RemoveAt(j);
                                            break;
                                        }
                                    }
                                }

                                var cacheService = IoC.Get<IStateService>();
                                cacheService.SaveFeaturedStickersAsync(_featuredStickers);
                            }

                            Execute.BeginOnUIThread(() =>
                            {
                                foreach (var unreadStickerSet in unreadStickerSets)
                                {
                                    unreadStickerSet.NotifyOfPropertyChange(() => unreadStickerSet.Unread);
                                }
                                if (_featuredStickers != null && _featuredStickersCounter != null)
                                {
                                    _featuredStickersCounter.Counter = _featuredStickers.Unread.Count;
                                }
                            });
                        },
                        error => Execute.BeginOnUIThread(() =>
                        {
                            Execute.ShowDebugMessage("messages.readFeaturedStickers error=" + error);  
                        }));
                }
                
            }
        }

        public void BindTextBox(TextBox textBox, bool isStickersPanelVisible = false)
        {
            TextBoxTarget = textBox;
            UpdateButtons(isStickersPanelVisible);
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

                if (value)
                {
                    Open();
                }
                else
                {
                    Hide();
                }


                RaiseIsOpenedChanged(value);
            }
        }

        public void SetHeight(double height)
        {
            //return;
            //EmojiContainer.MaxHeight = height;
        }

        private void Open()
        {
            _isOpen = true;

            if (TextBoxTarget != null)
            {
                TextBoxTarget.Dispatcher.BeginInvoke(() => VisualStateManager.GoToState(TextBoxTarget, "Focused", false));
            }

            //var frame = (PhoneApplicationFrame)Application.Current.RootVisual;
            EmojiContainer.Visibility = Visibility.Visible;
            ButtonsGrid.Visibility = Visibility.Visible;
            StickersGrid.Visibility = Visibility.Collapsed;
            Deployment.Current.Dispatcher.BeginInvoke(() => LoadCategory(0));

            //frame.BackKeyPress += OnBackKeyPress;

            //if (!(EmojiContainer.RenderTransform is TranslateTransform))
            //    EmojiContainer.RenderTransform = new TranslateTransform();
            //var transform = (TranslateTransform)EmojiContainer.RenderTransform;

            var offset = _isPortrait ? PortraitOrientationHeight : AlbumOrientationHeight;
            SetHeight(offset);

            //var from = 0;

            //if (_frameTransform.Y < 0) // Keyboard is in view
            //{
            //    from = (int)_frameTransform.Y;
            //    //_frameTransform.Y = -offset;
            //    //transform.Y = offset;// -72;
            //}
            //transform.Y = offset;// -72

            //if (from == offset) return;

            //frame.IsHitTestVisible = false;
            //_isBlocked = true;

            //var storyboard = new Storyboard();
            //var doubleTransformFrame = new DoubleAnimation
            //{
            //    From = from,
            //    To = -offset,
            //    Duration = TimeSpan.FromMilliseconds(440),
            //    EasingFunction = new ExponentialEase
            //    {
            //        EasingMode = EasingMode.EaseOut,
            //        Exponent = 6
            //    }
            //};
            //storyboard.Children.Add(doubleTransformFrame);
            //Storyboard.SetTarget(doubleTransformFrame, _frameTransform);
            //Storyboard.SetTargetProperty(doubleTransformFrame, new PropertyPath("Y"));

            //EmojiContainer.Dispatcher.BeginInvoke(async () =>
            //{
            //    storyboard.Begin();

            //    if (_frameTransform.Y < 0) // Keyboard is in view
            //    {
            //        Focus();
            //        TextBoxTarget.Dispatcher.BeginInvoke(() // no effect without dispatcher
            //            => VisualStateManager.GoToState(TextBoxTarget, "Focused", false));
            //    }

            //    if (_wasRendered) return;
            //    await Task.Delay(50);
            //    LoadCategory(0);
            //});

            //storyboard.Completed += (sender, args) =>
            //{
            //    frame.IsHitTestVisible = true;
            //    _isBlocked = false;
            //};
        }

        private void Hide()
        {
            _isOpen = false;

            VisualStateManager.GoToState(TextBoxTarget, "Unfocused", false);
            EmojiContainer.Visibility = Visibility.Collapsed;


            //var frame = (PhoneApplicationFrame)Application.Current.RootVisual;
            //frame.BackKeyPress -= OnBackKeyPress;

            //if (_isTextBoxTargetFocused)
            //{
            //    _frameTransform.Y = 0;

            //    EmojiContainer.Visibility = Visibility.Collapsed;

            //    return;
            //}

            //VisualStateManager.GoToState(TextBoxTarget, "Unfocused", false);

            //frame.IsHitTestVisible = false;
            //_isBlocked = true;

            //var transform = (TranslateTransform)EmojiContainer.RenderTransform;

            //var storyboard = new Storyboard();
            //var doubleTransformFrame = new DoubleAnimation
            //{
            //    From = -transform.Y,
            //    To = 0,
            //    Duration = TimeSpan.FromMilliseconds(440),
            //    EasingFunction = new ExponentialEase
            //    {
            //        EasingMode = EasingMode.EaseOut,
            //        Exponent = 6
            //    }
            //};
            //storyboard.Children.Add(doubleTransformFrame);
            //Storyboard.SetTarget(doubleTransformFrame, _frameTransform);
            //Storyboard.SetTargetProperty(doubleTransformFrame, new PropertyPath("Y"));
            //storyboard.Begin();

            //storyboard.Completed += (sender, args) =>
            //{
            //    EmojiContainer.Visibility = Visibility.Collapsed;

            //    frame.IsHitTestVisible = true;
            //    _isBlocked = false;
            //    transform.Y = 0;
            //};

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

        private void VirtPanel_OnScrollPositionChanged(object sender, MyVirtualizingPanel.ScrollPositionChangedEventAgrs args)
        {
            //System.Diagnostics.Debug.WriteLine("scroll_height=" + scrollPositionChangedEventAgrs.ScrollHeight + " current_position=" + scrollPositionChangedEventAgrs.CurrentPosition);

            EmojiSpriteItem.ClearCurrentHighlight();
        }

        private void FeaturedStickersVirtPanel_OnScrollPositionChanged(object sender, MyVirtualizingPanel.ScrollPositionChangedEventAgrs args)
        {
            if (CurrentCategory != FeaturedStickersCategoryIndex) return;

            if (args.CurrentPosition + 2000.0 >= args.ScrollHeight)
            {
                LoadNextFeaturedStickersSlice();
            }
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
                    previousCategory.Background = ButtonBackground;

                nextCategory.Background = (Brush)Application.Current.Resources["TelegramBadgeAccentBrush"];
                _currentCategory = value;


                CSV.Visibility = value != FeaturedStickersCategoryIndex
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                FeaturedStickersCSV.Visibility = value == FeaturedStickersCategoryIndex
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public void RemoveStickerSet(TLInputStickerSetBase stickerSet)
        {
            var setId = stickerSet.Name;
            if (_featuredStickers != null)
            {
                foreach (var featuredStickerSet in _featuredStickers.Sets)
                {
                    var stickerSet32 = featuredStickerSet as TLStickerSet32;
                    if (stickerSet32 != null && string.Equals(stickerSet32.Id.Value.ToString(), stickerSet.Name))
                    {
                        stickerSet32.Installed = false;

                        var stickerSet76 = stickerSet32 as TLStickerSet76;
                        if (stickerSet76 != null)
                        {
                            stickerSet76.InstalledDate = null;
                        }

                        stickerSet32.NotifyOfPropertyChange(() => stickerSet32.Installed);
                    }
                }
            }
            
            if (_stickerSets.ContainsKey(setId))
            {
                _stickerSets.Remove(setId);

                var stateService = IoC.Get<IStateService>();
                stateService.GetAllStickersAsync(cachedStickers =>
                {
                    var allStickers43 = cachedStickers as TLAllStickers43;
                    if (allStickers43 != null && allStickers43.RecentStickers != null)
                    {
                        List<TLDocument22> recentStickers;
                        if (_stickerSets.TryGetValue(@"tlg/recentlyUsed", out recentStickers))
                        {
                            var recentStickersCache = new Dictionary<long, long>();
                            for (var i = 0; i < recentStickers.Count; i++)
                            {
                                if (recentStickers[i].StickerSet.Name == setId)
                                {
                                    recentStickersCache[recentStickers[i].Id.Value] = recentStickers[i].Id.Value;
                                    recentStickers.RemoveAt(i--);

                                    _reloadStickerSprites = true;
                                }
                            }

                            for (var i = 0; i < allStickers43.RecentStickers.Documents.Count; i++)
                            {
                                var recentSticker = allStickers43.RecentStickers.Documents[i];
                                if (recentStickersCache.ContainsKey(recentSticker.Id.Value))
                                {
                                    allStickers43.RecentStickers.Documents.RemoveAt(i--);
                                }
                            }
                        }
                    }

                    stateService.SaveAllStickersAsync(cachedStickers);

                    UpdateStickersPanel(CurrentCategory);
                    //UpdateStickersPanel(StickerCategoryIndex);
                });
            }

            //UpdateAllStickersAsync();
        }

        public void AddStickerSet(TLMessagesStickerSet stickerSet)
        {
            var stateService = IoC.Get<IStateService>();
            stateService.GetAllStickersAsync(cachedStickers =>
            {
                CreateSetsAndUpdatePanel(_currentCategory, cachedStickers);
            });
            //var setId = stickerSet.Set.Id.ToString();
            //if (_stickerSets.Count > 0
            //    && !_stickerSets.ContainsKey(setId))
            //{
            //    var stickers = new List<TLDocument22>();
            //    foreach (var document in stickerSet.Documents)
            //    {
            //        var document22 = document as TLDocument22;
            //        if (document22 != null)
            //        {
            //            stickers.Add(document22);
            //        }
            //    }

            //    _stickerSets[setId] = stickers;

            //    UpdateStickersPanel(_currentCategory);
            //}

            //UpdateAllStickersAsync();
        }

        public void ReorderStickerSets()
        {
            var stateService = IoC.Get<IStateService>();
            stateService.GetAllStickersAsync(cachedStickers =>
            {
                CreateSetsAndUpdatePanel(_currentCategory, cachedStickers);
            });
        }

        private void UpdateStickersPanel(int index)
        {
            var hasUnreadFeaturedStickers = _featuredStickers != null && _featuredStickers.Unread.Count > 0;

            StickersPanel.Children.Clear();
            StickersPanel.Children.Add(_emojiButton);
            if (hasUnreadFeaturedStickers)
            {
                if (_featuredStickers != null && _featuredStickersCounter != null)
                {
                    _featuredStickersCounter.Counter = _featuredStickers.Unread.Count;
                }
                StickersPanel.Children.Add(_featuredStickersGrid);
            }
            StickersPanel.Children.Add(_favedStickersButton);
            StickersPanel.Children.Add(_recentStickersButton);
            StickersPanel.Children.Add(_groupStickersButton);

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            var buttonStyleResourceKey = isLightTheme ? "CategoryButtonLightThemeStyle" : "CategoryButtonDarkThemeStyle";
            var buttonStyle = (Style)Resources[buttonStyleResourceKey];

            string previousKey = null;
            if (index > RecentStickersCategoryIndex)
            {
                var previousCategory = GetCategoryButtonByIndex(_currentCategory);
                if (previousCategory != null)
                {
                    previousKey = previousCategory.DataContext as string;
                }
            }

            var secondSlice = new List<Button>();
            _stickerSetButtons.Clear();
            var allStickers = _allStickers as TLAllStickers29;
            if (allStickers != null)
            {
                var firstSliceCount = 3;
                var count = 0;
                foreach (var stickerSet in allStickers.Sets)
                {
                    var key = stickerSet.Id.Value.ToString(CultureInfo.InvariantCulture);
                    List<TLDocument22> stickers;
                    if (_stickerSets.TryGetValue(key, out stickers))
                    {
                        var image = new Image { Width = 53.0, Height = 53.0, DataContext = new TLStickerItem { Document = stickers.FirstOrDefault() } };
                        var binding = new Binding("Self")
                        {
                            Converter = new DefaultPhotoConverter(),
                            ConverterParameter = 64.0
                        };
                        image.SetBinding(Image.SourceProperty, binding);

                        var button = new Button
                        {
                            Width = 78.0,
                            Height = 78.0,
                            ClickMode = ClickMode.Release,
                            Style = buttonStyle,
                            Content = image,
                            DataContext = key
                        };
                        button.Click += StickerSetButtonOnClick;

                        if (count < firstSliceCount)
                        {
                            StickersPanel.Children.Add(button);
                            count++;
                        }
                        else
                        {
                            secondSlice.Add(button);
                            count++;
                        }

                        _stickerSetButtons.Add(button);
                    }
                }
            }

            var selectFavedStickers = false;
            var selectRecentlyUsedStickers = false;
            var selectFirstStickerSet = false;
            if (index > RecentStickersCategoryIndex && index > FeaturedStickersCategoryIndex)
            {
                var nextIndex = GetStickerSetButtonByKey(previousKey);
                if (nextIndex != -1)
                {
                    CurrentCategory = nextIndex;
                }
                else if (_favedStickersButton.Visibility == Visibility.Visible)
                {
                    selectFavedStickers = true;
                }
                else
                {
                    selectRecentlyUsedStickers = true;
                }
            }
            else if (index == StickerCategoryIndex)
            {
                var allStickers43 = allStickers as TLAllStickers43;
                if (allStickers43 != null && (allStickers43.RecentStickers == null || allStickers43.RecentStickers.Documents.Count == 0))
                {
                    if (index == StickerCategoryIndex && _stickerSetButtons.Count > 0)
                    {
                        selectFirstStickerSet = true;
                    }
                }

                if (!selectFirstStickerSet)
                {
                    if (_favedStickersButton.Visibility == Visibility.Visible)
                    {
                        selectFavedStickers = true;
                    }
                    else
                    {
                        selectRecentlyUsedStickers = true;
                    }
                }
            }

            if (selectFirstStickerSet)
            {
                Execute.BeginOnUIThread(() =>
                {
                    StickerSetButtonOnClick(_stickerSetButtons[0], null);

                    AddLastStickerButtons(secondSlice, hasUnreadFeaturedStickers);
                });
            }
            else if (selectFavedStickers)
            {
                Execute.BeginOnUIThread(() =>
                {
                    FavedStickersButtonOnClick(null, null);

                    AddLastStickerButtons(secondSlice, hasUnreadFeaturedStickers);
                });
            }
            else if (selectRecentlyUsedStickers)
            {
                Execute.BeginOnUIThread(() =>
                {
                    RecentStickersButtonOnClick(null, null);

                    AddLastStickerButtons(secondSlice, hasUnreadFeaturedStickers);
                });
            }
            else
            {
                Execute.BeginOnUIThread(() =>
                {
                    AddLastStickerButtons(secondSlice, hasUnreadFeaturedStickers);
                });
            }
        }

        private void AddLastStickerButtons(List<Button> secondSlice, bool hasUnreadFeaturedStickers)
        {
            if (secondSlice.Count > 0)
            {
                Execute.BeginOnUIThread(() =>
                {
                    foreach (var button in secondSlice)
                    {
                        StickersPanel.Children.Add(button);
                    }

                    if (!hasUnreadFeaturedStickers)
                    {
                        StickersPanel.Children.Add(_featuredStickersGrid);
                    }
                    StickersPanel.Children.Add(_settingsButton);
                });
            }
            else
            {
                if (!hasUnreadFeaturedStickers)
                {
                    StickersPanel.Children.Add(_featuredStickersGrid);
                }
                StickersPanel.Children.Add(_settingsButton);
            }
        }

        private void StickerSetButtonOnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var key = button.DataContext as string;
            if (key == null) return;

            List<TLDocument22> stickers;
            if (_stickerSets.TryGetValue(key, out stickers))
            {
                LoadStickerSet(key, stickers);
            }
        }

        private void LoadCachedFeaturedStickersAsync()
        {
            var stopwatch = Stopwatch.StartNew();
            Execute.BeginOnThreadPool(() =>
            {
                var stateService = IoC.Get<IStateService>();
                var mtProtoService = IoC.Get<IMTProtoService>();

                stateService.GetFeaturedStickersAsync(cachedStickers =>
                {
                    System.Diagnostics.Debug.WriteLine("LoadCachedFeaturedStickersAsync 1 elapsed=" + stopwatch.Elapsed);
                    UpdateFeaturedSets(cachedStickers);

                    System.Diagnostics.Debug.WriteLine("LoadCachedFeaturedStickersAsync 2 elapsed=" + stopwatch.Elapsed);
                    Execute.BeginOnUIThread(() =>
                    {
                        _featuredStickers = cachedStickers;
                        UpdateFeaturedStickersPosition();
                    });
                    var hash = cachedStickers != null ? cachedStickers.HashValue : new TLInt(0);
                    //Execute.ShowDebugMessage("messages.getFeaturedStickers hash=" + hash);
                    mtProtoService.GetFeaturedStickersAsync(true, hash,
                        result => Execute.BeginOnUIThread(() =>
                        {
                            var featuredStickers = result as TLFeaturedStickers;
                            if (featuredStickers != null)
                            {
                                UpdateFeaturedSets(featuredStickers);
                                //Execute.ShowDebugMessage(string.Format("messages.getFeaturedStickers hash={0} result={1}", featuredStickers.Hash, featuredStickers));
                                _featuredStickers = featuredStickers;
                                stateService.SaveFeaturedStickersAsync(featuredStickers);
                            }
                            else
                            {
                                //Execute.ShowDebugMessage("messages.getFeaturedStickers result=" + result);
                            }
                        }),
                        error => Execute.BeginOnUIThread(() =>
                        {
                            //Execute.ShowDebugMessage("messages.getFeaturedStickers error " + error);
                        }));
                });
            });
        }

        private int _featuredStickersOffset;

        private void LoadNextFeaturedStickersSlice()
        {
            if (_featuredStickers == null) return;
            var sets = _featuredStickers.SetsCovered;
            if (sets == null) return;

            var setId = @"tlg/featured";
            var sprites = new List<VListItemBase>();
            List<VListItemBase> cachedSprites;
            if (_stickerSetSprites.TryGetValue(setId, out cachedSprites))
            {
                if (FeaturedStickersVirtPanel.VirtItems.Count < cachedSprites.Count)
                {
                    for (var i = FeaturedStickersVirtPanel.VirtItems.Count; i < FeaturedStickersVirtPanel.VirtItems.Count + FeaturedStickersSliceCount && i < cachedSprites.Count; i++)
                    {
                        sprites.Add(cachedSprites[i]);
                    }
                    FeaturedStickersVirtPanel.AddItems(sprites);
                    return;
                }
            }

            for (var i = _featuredStickersOffset; i < sets.Count && i < _featuredStickersOffset + FeaturedStickersSliceCount; i++)
            {
                var sprite = GetCoveredStickerSprite(sets[i]);
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            if (sprites.Count == 0) return;

            if (_stickerSetSprites.TryGetValue(setId, out cachedSprites))
            {
                foreach (var sprite in sprites)
                {
                    cachedSprites.Add(sprite);
                }
            }

            _featuredStickersOffset += sprites.Count;

            FeaturedStickersVirtPanel.AddItems(sprites);
        }

        private VListItemBase GetCoveredStickerSprite(TLStickerSetCoveredBase set)
        {
            var stickerPerRow = 5;

            var stickers = new List<TLStickerItem>();

            var stickerSetCovered = set as TLStickerSetCovered;
            if (stickerSetCovered != null)
            {
                if (stickerSetCovered.StickerSet.Stickers != null
                    && stickerSetCovered.StickerSet.Stickers.Count > 0)
                {
                    for (var j = 0; j < stickerSetCovered.StickerSet.Stickers.Count && stickers.Count < 5; j++)
                    {
                        var stickerItem = stickerSetCovered.StickerSet.Stickers[j] as TLStickerItem;
                        if (stickerItem != null)
                        {
                            stickers.Add(stickerItem);
                        }
                    }
                }
                else
                {
                    stickers.Add(new TLStickerItem { Document = stickerSetCovered.Cover });
                }
                var item = new FeaturedStickerSpriteItem(stickerSetCovered.StickerSet, stickerPerRow, new List<TLStickerItem>(stickers), 90.0, 472.0, StickerPanel_MouseEnter);
                item.StickerSetAdded += OnStickerSetAdded;
                item.StickerSetOpened += OnStickerSetOpened;
                item.StickerSelected += OnFeaturedStickerSelected;

                return item;
            }

            var stickerSetMultiCovered = set as TLStickerSetMultiCovered;
            if (stickerSetMultiCovered != null)
            {
                for (var j = 0; j < stickerSetMultiCovered.Covers.Count && stickers.Count < 5; j++)
                {
                    stickers.Add(new TLStickerItem { Document = stickerSetMultiCovered.Covers[j] });
                }
                var item = new FeaturedStickerSpriteItem(stickerSetMultiCovered.StickerSet, stickerPerRow, new List<TLStickerItem>(stickers), 90.0, 472.0, StickerPanel_MouseEnter);
                item.StickerSetAdded += OnStickerSetAdded;
                item.StickerSetOpened += OnStickerSetOpened;
                item.StickerSelected += OnFeaturedStickerSelected;

                return item;
            }

            return null;
        }

        private void LoadFeaturedStickerSets(IList<TLStickerSetCoveredBase> sets)
        {
            var readFeaturedSets = false;

            var setId = @"tlg/featured";
            List<VListItemBase> sprites;
            var created = false;
            if (!_stickerSetSprites.TryGetValue(setId, out sprites))
            {
                created = true;
                sprites = new List<VListItemBase>();
                for (var i = 0; i < sets.Count && i < FeaturedStickersSliceCount; i++)
                {
                    if (sets[i].StickerSet.Unread)
                    {
                        readFeaturedSets = true;
                    }

                    var sprite = GetCoveredStickerSprite(sets[i]);
                    if (sprite != null)
                    {
                        sprites.Add(sprite);
                    }
                }

                _stickerSetSprites[setId] = sprites;
            }

            _featuredStickersOffset = sprites.Count;

            CurrentSprites = sprites;

            if (created)
            {
                var firstSlice = 3;
                FeaturedStickersVirtPanel.ClearItems();
                FeaturedStickersVirtPanel.AddItems(sprites.Take(firstSlice));
                //LoadingProgressBar.Visibility = Visibility.Visible;
                Execute.BeginOnUIThread(() =>
                {
                    //LoadingProgressBar.Visibility = Visibility.Collapsed;
                    FeaturedStickersVirtPanel.AddItems(sprites.Skip(firstSlice).Take(FeaturedStickersSliceCount - firstSlice));

                    _runOnce = true;
                });
            }

            CurrentCategory = FeaturedStickersCategoryIndex;

            if (readFeaturedSets)
            {
                Execute.BeginOnUIThread(() =>
                {
                    FeaturedStickersVirtPanel_OnScrollStateChanged(this, new ScrollingStateChangedEventArgs(false, false));
                });
            }
        }

        private void OnStickerSetOpened(object sender, StickerSetOpenedEventArgs e)
        {
            if (e.Set == null) return;
            var set32 = e.Set as TLStickerSet32;
            if (set32 == null) return;

            Execute.BeginOnUIThread(() =>
                TelegramViewBase.ShowStickerSetMessageBox(false, set32.Installed && !set32.Archived, set32, prompt =>
                {
                    if (prompt == PopUpResult.Ok)
                    {
                        OnStickerSetAdded(sender, new StickerSetAddedEventArgs{ Set = set32 });
                    }
                }));
        }

        private void OnStickerSetAdded(object sender, StickerSetAddedEventArgs e)
        {
            if (e.Set == null) return;
            var set32 = e.Set as TLStickerSet32;
            if (set32 == null) return;

            var inputStickerSet = new TLInputStickerSetId { Id = e.Set.Id, AccessHash = e.Set.AccessHash };

            var mtProtoService = IoC.Get<IMTProtoService>();
            mtProtoService.GetStickerSetAsync(inputStickerSet,
                stickerSet => Execute.BeginOnUIThread(() =>
                {
                    var stickerSet32 = stickerSet.Set as TLStickerSet32;
                    if (stickerSet32 == null) return;

                    //if (stickerSet32.Installed && !stickerSet32.Archived)
                    //{
                    //    set32.Installed = true;
                    //    set32.Archived = false;
                        
                    //    set32.NotifyOfPropertyChange(() => set32.Installed);

                    //    return;
                    //}

                    stickerSet.Set = set32;

                    stickerSet.Set.Stickers = new TLVector<TLObject>();
                    for (var i = 0; i < stickerSet.Documents.Count; i++)
                    {
                        var document22 = stickerSet.Documents[i] as TLDocument22;
                        if (document22 != null)
                        {
                            stickerSet.Set.Stickers.Add(new TLStickerItem { Document = document22 });
                        }
                    }
                    if (stickerSet32.Installed && !stickerSet32.Archived)
                    {
                        mtProtoService.UninstallStickerSetAsync(inputStickerSet,
                            result => Execute.BeginOnUIThread(() =>
                            {
                                set32.Installed = false;
                                var set76 = set32 as TLStickerSet76;
                                if (set76 != null)
                                {
                                    set76.InstalledDate = null;
                                }

                                set32.NotifyOfPropertyChange(() => stickerSet32.Installed);

                                var shellViewModel = IoC.Get<ShellViewModel>();
                                shellViewModel.RemoveStickerSet(set32, inputStickerSet);

                                mtProtoService.SetMessageOnTime(2.0, AppResources.StickersRemoved);
                            }),
                            error => Execute.BeginOnUIThread(() =>
                            {
                                Execute.ShowDebugMessage("messages.uninstallStickerSet error " + error);
                            }));
                    }
                    else
                    {
                        mtProtoService.InstallStickerSetAsync(inputStickerSet, TLBool.False,
                            result => Execute.BeginOnUIThread(() =>
                            {
                                var resultArchive = result as TLStickerSetInstallResultArchive;
                                if (resultArchive != null)
                                {
                                    TelegramViewBase.ShowArchivedStickersMessageBox(resultArchive);
                                }

                                set32.Installed = true;
                                var set76 = set32 as TLStickerSet76;
                                if (set76 != null)
                                {
                                    set76.InstalledDate = TLUtils.DateToUniversalTimeTLInt(IoC.Get<IMTProtoService>().ClientTicksDelta, DateTime.Now);
                                }

                                set32.NotifyOfPropertyChange(() => stickerSet32.Installed);

                                var shellViewModel = IoC.Get<ShellViewModel>();
                                shellViewModel.Handle(new TLUpdateNewStickerSet { Stickerset = stickerSet });

                                mtProtoService.SetMessageOnTime(2.0, AppResources.NewStickersAdded);
                            }),
                            error => Execute.BeginOnUIThread(() =>
                            {
                                if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                                {
                                    if (error.TypeEquals(ErrorType.STICKERSET_INVALID))
                                    {
                                        MessageBox.Show(AppResources.StickersNotFound, AppResources.Error,
                                            MessageBoxButton.OK);
                                    }
                                    else
                                    {
                                        Execute.ShowDebugMessage("messages.importChatInvite error " + error);
                                    }
                                }
                                else
                                {
                                    Execute.ShowDebugMessage("messages.importChatInvite error " + error);
                                }
                            }));
                    }
                }),
                error => Execute.BeginOnUIThread(() =>
                {
                    if (error.CodeEquals(ErrorCode.BAD_REQUEST))
                    {
                        if (error.TypeEquals(ErrorType.STICKERSET_INVALID))
                        {
                            MessageBox.Show(AppResources.StickersNotFound, AppResources.Error, MessageBoxButton.OK);
                        }
                        else
                        {
                            Execute.ShowDebugMessage("messages.getStickerSet error " + error);
                        }
                    }
                    else
                    {
                        Execute.ShowDebugMessage("messages.getStickerSet error " + error);
                    }
                }));
        }

        private SearchSpriteItem _searchSprite;

        private void LoadStickerSet(string key, List<TLDocument22> stickerSet)
        {
            var recentlyUsedKey = @"tlg/recentlyUsed";
            var favedKey = @"tlg/faved";
            var groupKey = @"tlg/group";
            var stickerPerRow = 5;

            var setId = key;
            List<VListItemBase> sprites;
            if (!_stickerSetSprites.TryGetValue(setId, out sprites)
                || (key == recentlyUsedKey && _reloadStickerSprites)
                || (key == favedKey && _reloadStickerSprites))
            {
                _reloadStickerSprites = false;
                sprites = new List<VListItemBase>();
                var stickers = new List<TLStickerItem>();
                for (var i = 1; i <= stickerSet.Count; i++)
                {
                    stickers.Add(new TLStickerItem { Document = stickerSet[i - 1] });

                    if (i % stickerPerRow == 0 || i == stickerSet.Count)
                    {
                        var item = new StickerSpriteItem(stickerPerRow, new List<TLStickerItem>(stickers), 90.0, 472.0, StickerPanel_MouseEnter);
                        item.StickerSelected += OnStickerSelected;
                        sprites.Add(item);
                        stickers.Clear();
                    }
                }

                _stickerSetSprites[setId] = sprites;
            }

            if (key == recentlyUsedKey)
            {
                _recentStickersSprites = sprites;
            }
            if (key == favedKey)
            {
                _favedStickersSprites = sprites;
            }

            CurrentSprites = sprites;

            _searchSprite = new SearchSpriteItem(478.0);
            _searchSprite.SearchText += OnSearchText;
            _searchSprite.OpenFullScreen += OnOpenFullScreen;
            _searchSprite.CloseFullScreen += OnCloseFullScreen;


            var totalHeight = sprites.Sum(x => x.FixedHeight);
            if (sprites.Count > 0
                && CSV.ViewportHeight > totalHeight)
            {
                var footer = new StickerFooterSpriteItem(478.0);
                footer.View.Height = (CSV.ViewportHeight - totalHeight);
                footer.FixedHeight = footer.View.Height;

                sprites.Add(footer);
            }

            var firstSlice = new List<VListItemBase> { _searchSprite };
            var secondSlice = new List<VListItemBase>();
            for (var i = 0; i < sprites.Count; i++)
            {
                if (i < FirstStickerSliceCount)
                {
                    firstSlice.Add(sprites[i]);
                }
                else
                {
                    secondSlice.Add(sprites[i]);
                }
            }

            VirtPanel.ClearItems();
            VirtPanel.AddItems(firstSlice);
            CSV.UpdateLayout();
            CSV.ScrollToVerticalOffset(_searchSprite.FixedHeight);
            Execute.BeginOnUIThread(() =>
            {
                VirtPanel.AddItems(secondSlice);
            });

            if (key == favedKey)
            {
                CurrentCategory = FavedStickersCategoryIndex;
            }
            else if (key == groupKey)
            {
                CurrentCategory = GroupStickersCategoryIndex;
            }
            else if (key == recentlyUsedKey)
            {
                CurrentCategory = RecentStickersCategoryIndex;
            }
            else
            {
                var index = 0;
                foreach (var button in _stickerSetButtons)
                {
                    var stickerSetKey = button.DataContext as string;
                    if (stickerSetKey == key) break;
                    index++;
                }

                CurrentCategory = RecentStickersCategoryIndex + index + 1;
            }
        }

        public event EventHandler OpenFullScreen;

        private void OnOpenFullScreen(object sender, EventArgs e)
        {
            var handler = OpenFullScreen;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        public event EventHandler CloseFullScreen;

        private void OnCloseFullScreen(object sender, EventArgs e)
        {
            var handler = CloseFullScreen;
            if (handler != null)
            {
                handler.Invoke(sender, e);
            }
        }

        private bool _isSearchOpened;

        public void OpenSearch()
        {
            _isSearchOpened = true;

            var firstSlice = new List<VListItemBase>{ _searchSprite }; 

            VirtPanel.ClearItems();
            VirtPanel.AddItems(firstSlice);
            CSV.UpdateLayout();
            CSV.ScrollToVerticalOffset(0.0);
            _searchSprite.Focus();
            StickersGrid.Visibility = Visibility.Collapsed;
            NoStickersPlaceholder.Visibility = Visibility.Collapsed;
            CSV.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        public void CloseSearch()
        {
            _isSearchOpened = false;

            var sprites = CurrentSprites;

            _searchSprite = new SearchSpriteItem(478.0);
            _searchSprite.SearchText += OnSearchText;
            _searchSprite.OpenFullScreen += OnOpenFullScreen;
            _searchSprite.CloseFullScreen += OnCloseFullScreen;

            var firstSlice = new List<VListItemBase> { _searchSprite };
            var secondSlice = new List<VListItemBase>();
            for (var i = 0; i < sprites.Count; i++)
            {
                if (i < FirstStickerSliceCount)
                {
                    firstSlice.Add(sprites[i]);
                }
                else
                {
                    secondSlice.Add(sprites[i]);
                }
            }

            VirtPanel.ClearItems();
            VirtPanel.AddItems(firstSlice);
            CSV.UpdateLayout();
            CSV.ScrollToVerticalOffset(0.0);
            StickersGrid.Visibility = Visibility.Visible;
            NoStickersPlaceholder.Visibility = Visibility.Collapsed;
            CSV.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Execute.BeginOnUIThread(() =>
            {
                VirtPanel.AddItems(secondSlice);
            });
        }

        private void OnSearchText(object sender, SearchTextEventArgs e)
        {
            var sprites = new List<VListItemBase>();
            var dict = new Dictionary<long, long>();

            if (!string.IsNullOrEmpty(e.Text))
            {
                var stickerPerRow = 4;
                var allStickers = _allStickers as TLAllStickers43;
                if (allStickers != null)
                {
                    var sets = allStickers.Sets;
                    if (sets != null)
                    {
                        for (var i = 0; i < sets.Count; i++)
                        {
                            var set = sets[i] as TLStickerSet32;
                            if (set != null
                                && !dict.ContainsKey(set.Id.Value)
                                && (set.Title.ToString().IndexOf(e.Text, StringComparison.OrdinalIgnoreCase) != -1
                                    || set.ShortName.ToString().IndexOf(e.Text, StringComparison.OrdinalIgnoreCase) != -1))
                            {
                                dict[set.Id.Value] = set.Id.Value;
                                var setId = set.Id.ToString();
                                List<VListItemBase> items;
                                if (!_stickerSetSprites.TryGetValue(setId, out items))
                                {
                                    List<TLDocument22> documents;
                                    if (_stickerSets.TryGetValue(setId, out documents))
                                    {
                                        items = new List<VListItemBase>();
                                        var stickers = new List<TLStickerItem>();
                                        for (var j = 1; j <= documents.Count; j++)
                                        {
                                            stickers.Add(new TLStickerItem { Document = documents[j - 1] });

                                            if (j % stickerPerRow == 0 || j == documents.Count)
                                            {
                                                var item = new StickerSpriteItem(stickerPerRow, new List<TLStickerItem>(stickers), 90.0, 472.0, StickerPanel_MouseEnter);
                                                item.StickerSelected += OnStickerSelected;
                                                items.Add(item);
                                                stickers.Clear();
                                            }
                                        }

                                        _stickerSetSprites[setId] = items;
                                    }
                                }

                                var header = new StickerHeaderSpriteItem(set, 478.0);
                                sprites.Add(header);
                                sprites.AddRange(items.Where(x => !(x is StickerFooterSpriteItem)));
                            }
                        }
                    }
                }

                if (_featuredStickers != null)
                {
                    var sets = _featuredStickers.SetsCovered;
                    if (sets != null)
                    {
                        for (var i = 0; i < sets.Count; i++)
                        {
                            var set = sets[i].StickerSet;
                            if (set != null
                                && !dict.ContainsKey(set.Id.Value)
                                && (set.Title.ToString().IndexOf(e.Text, StringComparison.OrdinalIgnoreCase) != -1
                                    || set.ShortName.ToString().IndexOf(e.Text, StringComparison.OrdinalIgnoreCase) != -1))
                            {
                                dict[set.Id.Value] = set.Id.Value;
                                var sprite = GetCoveredStickerSprite(sets[i]);
                                if (sprite != null)
                                {
                                    sprites.Add(sprite);
                                }
                            }
                        }
                    }
                }
            }

            SearchSprites = sprites;

            VirtPanel.ClearItems();
            if (sprites.Count > 0)
            {
                NoStickersPlaceholder.Visibility = Visibility.Collapsed;
                CSV.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                var searchItem = _searchSprite;
                var firstSlice = new List<VListItemBase> {_searchSprite};
                var secondSlice = new List<VListItemBase>();
                for (var i = 0; i < sprites.Count; i++)
                {
                    if (i < FirstStickerSliceCount)
                    {
                        firstSlice.Add(sprites[i]);
                    }
                    else
                    {
                        secondSlice.Add(sprites[i]);
                    }
                }

                VirtPanel.AddItems(firstSlice);
                searchItem.Focus();
                Execute.BeginOnUIThread(() =>
                {
                    VirtPanel.AddItems(secondSlice);

                    SearchStickerSetsAsync(e.Text, dict);
                });
            }
            else
            {
                var searchItem = _searchSprite;
                var firstSlice = new List<VListItemBase> { _searchSprite };
                VirtPanel.AddItems(firstSlice);
                searchItem.Focus();

                SearchStickerSetsAsync(e.Text, dict);
            }
        }

        private void SearchStickerSetsAsync(string text, Dictionary<long, long> setsDict)
        {
            if (string.IsNullOrEmpty(text)) return;

            IoC.Get<IMTProtoService>().SearchStickerSetsAsync(true, true, new TLString(text), new TLInt(0),
                result => Execute.BeginOnUIThread(() =>
                {
                    var foundStickerSets = result as TLFoundStickerSets;
                    if (foundStickerSets != null
                        && _searchSprite != null 
                        && string.Equals(_searchSprite.Text, text))
                    {
                        UpdateFoundSets(foundStickerSets);

                        var sets = foundStickerSets.SetsCovered;
                        if (sets != null)
                        {
                            var sprites = new List<VListItemBase>();
                            for (var i = 0; i < sets.Count; i++)
                            {
                                var set = sets[i].StickerSet;
                                if (set != null && !setsDict.ContainsKey(set.Id.Value))
                                {
                                    setsDict[set.Id.Value] = set.Id.Value;
                                    var sprite = GetCoveredStickerSprite(sets[i]);
                                    if (sprite != null)
                                    {
                                        sprites.Add(sprite);
                                    }
                                }
                            }

                            SearchSprites.AddRange(sprites);
                            if (sprites.Count > 0)
                            {
                                NoStickersPlaceholder.Visibility = Visibility.Collapsed;
                                CSV.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                                var firstSlice = new List<VListItemBase>();
                                var secondSlice = new List<VListItemBase>();
                                for (var i = 0; i < sprites.Count; i++)
                                {
                                    if (i < FirstStickerSliceCount)
                                    {
                                        firstSlice.Add(sprites[i]);
                                    }
                                    else
                                    {
                                        secondSlice.Add(sprites[i]);
                                    }
                                }

                                VirtPanel.AddItems(firstSlice);
                                Execute.BeginOnUIThread(() =>
                                {
                                    VirtPanel.AddItems(secondSlice);
                                });
                            }
                            else
                            {
                                NoStickersPlaceholder.Visibility = VirtPanel.VirtItems.Count == 1 ? Visibility.Visible : Visibility.Collapsed;
                                CSV.VerticalScrollBarVisibility = VirtPanel.VirtItems.Count == 1 ? ScrollBarVisibility.Hidden : ScrollBarVisibility.Auto;
                            }
                        }
                    }
                }),
                error =>
                {

                });
        }

        public List<VListItemBase> CurrentSprites;

        public List<VListItemBase> SearchSprites;

        private TLFeaturedStickers _featuredStickers;
        private TLFavedStickers _favedStickers;
        private readonly Dictionary<string, List<TLDocument22>> _stickerSets = new Dictionary<string, List<TLDocument22>>();
        private readonly Dictionary<string, List<VListItemBase>> _stickerSetSprites = new Dictionary<string, List<VListItemBase>>();
        private TLAllStickers _allStickers;

        public void LoadCategory(int index)
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
                case StickerCategoryIndex:
                    {
                        var allStickers43 = _allStickers as TLAllStickers43;
                        if (allStickers43 != null)
                        {
                            if (allStickers43.FavedStickers != null && allStickers43.FavedStickers.Documents.Count > 0)
                            {
                                sprites = _favedStickersSprites;
                                index = FavedStickersCategoryIndex;
                            }
                            else if (allStickers43.RecentStickers != null && allStickers43.RecentStickers.Documents.Count > 0)
                            {
                                sprites = _recentStickersSprites;
                                index = RecentStickersCategoryIndex;
                            }
                        }

                        Button button = null;
                        if (sprites == null && _stickerSetButtons.Count > 0)
                        {
                            button = _stickerSetButtons[0];
                            var key = button.DataContext as string;

                            List<VListItemBase> localSprites;
                            if (key != null
                                && _stickerSetSprites.TryGetValue(key, out localSprites))
                            {
                                sprites = localSprites;
                                index = RecentStickersCategoryIndex + 1;
                            }
                        }

                        UpdateFeaturedStickersPosition();

                        if (sprites != null)
                        {
                            if (index == FavedStickersCategoryIndex)
                            {
                                FavedStickersButtonOnClick(null, null);
                            }
                            else if (index == RecentStickersCategoryIndex)
                            {
                                RecentStickersButtonOnClick(null, null);
                            }
                            else if (button != null)
                            {
                                StickerSetButtonOnClick(button, null);
                            }
                        }
                    }
                    break;
            }

            var firstSlice = new List<VListItemBase>();
            var secondSlice = new List<VListItemBase>();
            if (sprites == null)
            {
                sprites = new List<VListItemBase>();

                if (index == StickerCategoryIndex)
                {
                    CurrentCategory = index;

                    var stateService = IoC.Get<IStateService>();
                    var mtProtoService = IoC.Get<IMTProtoService>();

                    LoadingProgressBar.Visibility = Visibility.Visible;
                    stateService.GetAllStickersAsync(cachedStickers =>
                    {
                        if (_reloadStickerSprites)
                        {
                            Execute.BeginOnUIThread(() =>
                            {
                                _reloadStickerSprites = false;
                                LoadingProgressBar.Visibility = Visibility.Collapsed;
                                CreateSetsAndUpdatePanel(index, cachedStickers);
                            });
                            return;
                        }

                        var cachedStickers43 = cachedStickers as TLAllStickers43;
                        if (cachedStickers43 != null
                            && cachedStickers43.RecentStickers != null
                            && cachedStickers43.FavedStickers != null)
                        {
                            LoadingProgressBar.Visibility = Visibility.Collapsed;
                            CreateSetsAndUpdatePanel(index, cachedStickers);
                        }
                        else
                        {
                            var hash = cachedStickers != null ? cachedStickers.Hash : TLString.Empty;
                            mtProtoService.GetAllStickersAsync(hash,
                                result => Execute.BeginOnUIThread(() =>
                                {
                                    var allStickers = result as TLAllStickers43;
                                    if (allStickers != null)
                                    {
                                        var cachedStickers29 = cachedStickers as TLAllStickers29;
                                        if (cachedStickers29 != null)
                                        {
                                            allStickers.ShowStickersTab = cachedStickers29.ShowStickersTab;
                                            allStickers.RecentlyUsed = cachedStickers29.RecentlyUsed;
                                            allStickers.Date = TLUtils.DateToUniversalTimeTLInt(0, DateTime.Now);
                                        }

                                        if (cachedStickers43 != null)
                                        {
                                            allStickers.RecentStickers = cachedStickers43.RecentStickers;
                                            allStickers.FavedStickers = cachedStickers43.FavedStickers;
                                        }

                                        cachedStickers = allStickers;
                                        stateService.SaveAllStickersAsync(cachedStickers);
                                    }

                                    var recentStickersHash = new TLInt(0);
                                    var allStickers43 = cachedStickers as TLAllStickers43;
                                    if (allStickers43 != null)
                                    {
                                        var recentStickers = allStickers43.RecentStickers;
                                        if (recentStickers != null)
                                        {
                                            recentStickersHash = recentStickers.Hash;
                                        }
                                    }

                                    mtProtoService.GetRecentStickersAsync(false, recentStickersHash,
                                        result2 => Execute.BeginOnUIThread(() =>
                                        {
                                            var recentStickers = result2 as TLRecentStickers;
                                            if (allStickers43 != null && recentStickers != null)
                                            {
                                                allStickers43.RecentStickers = recentStickers;
                                                stateService.SaveAllStickersAsync(cachedStickers);
                                            }

                                            var favedStickersHash = new TLInt(0);
                                            if (allStickers43 != null)
                                            {
                                                var favedStickers = allStickers43.FavedStickers;
                                                if (favedStickers != null)
                                                {
                                                    favedStickersHash = favedStickers.Hash;
                                                }
                                            }
                                            mtProtoService.GetFavedStickersAsync(favedStickersHash,
                                                result3 => Execute.BeginOnUIThread(() =>
                                                {
                                                    var favedStickers = result3 as TLFavedStickers;
                                                    if (allStickers43 != null && favedStickers != null)
                                                    {
                                                        allStickers43.FavedStickers = favedStickers;
                                                        stateService.SaveAllStickersAsync(cachedStickers);
                                                    }

                                                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                                                    CreateSetsAndUpdatePanel(index, cachedStickers);
                                                }),
                                                error3 => Execute.BeginOnUIThread(() =>
                                                {
                                                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                                                    CreateSetsAndUpdatePanel(index, cachedStickers);

                                                    Execute.ShowDebugMessage("messages.getFavedStickers error " + error3);
                                                }));
                                        }),
                                        error => Execute.BeginOnUIThread(() =>
                                        {
                                            LoadingProgressBar.Visibility = Visibility.Collapsed;
                                            CreateSetsAndUpdatePanel(index, cachedStickers);

                                            Execute.ShowDebugMessage("messages.getRecentStickers error " + error);
                                        }));
                                }),
                                error => Execute.BeginOnUIThread(() =>
                                {
                                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                                    CreateSetsAndUpdatePanel(index, cachedStickers);

                                    Execute.ShowDebugMessage("messages.getAllStickers error " + error);
                                }));
                        }
                    });

                    return;
                }

                for (var i = 0; i < EmojiData.SpritesByCategory[index].Length; i++)
                {
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

            // only emoji category here
            if (index < StickerCategoryIndex)
            {
                CurrentCategory = index;
                var firstSliceCount = 1;
                for (var i = 0; i < sprites.Count; i++)
                {
                    if (i < firstSliceCount)
                    {
                        firstSlice.Add(sprites[i]);
                    }
                    else
                    {
                        secondSlice.Add(sprites[i]);
                    }
                }

                VirtPanel.AddItems(firstSlice); 
                CreateButtonsBackgrounds(index);
                if (!_wasRendered)
                {
                    // Display LoadingProgressBar only once
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                    _wasRendered = true;
                }
                Execute.BeginOnUIThread(() =>
                {
                    if (_currentCategory != index)
                        return;

                    VirtPanel.AddItems(secondSlice);
                });
            }
        }

        private void UpdateFeaturedStickersPosition()
        {
            if (_featuredStickers == null) return;

            var hasUnreadFeaturedStickers = _featuredStickers.Unread.Count > 0;
            var index = StickersPanel.Children.IndexOf(_featuredStickersGrid);

            if (hasUnreadFeaturedStickers)
            {
                if (_featuredStickers != null && _featuredStickersCounter != null)
                {
                    _featuredStickersCounter.Counter = _featuredStickers.Unread.Count;
                }
                if (index == StickersPanel.Children.Count - 1 - 1)
                {
                    StickersPanel.Children.RemoveAt(index);
                    StickersPanel.Children.Insert(1, _featuredStickersGrid);
                }
            }
            else
            {
                if (index == 1)
                {
                    StickersPanel.Children.RemoveAt(index);
                    StickersPanel.Children.Insert(StickersPanel.Children.Count - 1, _featuredStickersGrid);
                }
            }
        }

        private static void UpdateFoundSets(IStickers featuredStickers)
        {
            if (featuredStickers == null) return;

            var stickerSets = new Dictionary<string, TLVector<TLStickerItem>>();

            for (var i = 0; i < featuredStickers.Documents.Count; i++)
            {
                var document22 = featuredStickers.Documents[i] as TLDocument22;
                if (document22 != null)
                {
                    if (document22.StickerSet != null)
                    {
                        var setId = document22.StickerSet.Name;
                        TLVector<TLStickerItem> stickers;
                        if (stickerSets.TryGetValue(setId, out stickers))
                        {
                            stickers.Add(new TLStickerItem { Document = document22 });
                        }
                        else
                        {
                            stickerSets[setId] = new TLVector<TLStickerItem> { new TLStickerItem { Document = document22 } };
                        }
                    }
                }
            }
            for (var i = 0; i < featuredStickers.Sets.Count; i++)
            {
                var set = featuredStickers.Sets[i];

                var setName = set.Id.ToString();
                TLVector<TLStickerItem> stickers;
                if (stickerSets.TryGetValue(setName, out stickers))
                {
                    var objects = new TLVector<TLObject>();
                    foreach (var sticker in stickers)
                    {
                        objects.Add(sticker);
                    }

                    set.Stickers = objects;
                }
            }
        }

        private static void UpdateFeaturedSets(TLFeaturedStickers featuredStickers)
        {
            if (featuredStickers == null) return;

            var stickerSets = new Dictionary<string, TLVector<TLStickerItem>>();
            var unreadDict = new Dictionary<long, long>();
            foreach (var unreadId in featuredStickers.Unread)
            {
                unreadDict[unreadId.Value] = unreadId.Value;
            }

            for (var i = 0; i < featuredStickers.Documents.Count; i++)
            {
                var document22 = featuredStickers.Documents[i] as TLDocument22;
                if (document22 != null)
                {
                    if (document22.StickerSet != null)
                    {
                        var setId = document22.StickerSet.Name;
                        TLVector<TLStickerItem> stickers;
                        if (stickerSets.TryGetValue(setId, out stickers))
                        {
                            stickers.Add(new TLStickerItem {Document = document22});
                        }
                        else
                        {
                            stickerSets[setId] = new TLVector<TLStickerItem> {new TLStickerItem {Document = document22}};
                        }
                    }
                }
            }
            for (var i = 0; i < featuredStickers.Sets.Count; i++)
            {
                var set = featuredStickers.Sets[i];
                if (unreadDict.ContainsKey(set.Id.Value))
                {
                    set.Unread = true;
                }

                var setName = set.Id.ToString();
                TLVector<TLStickerItem> stickers;
                if (stickerSets.TryGetValue(setName, out stickers))
                {
                    var objects = new TLVector<TLObject>();
                    foreach (var sticker in stickers)
                    {
                        objects.Add(sticker);
                    }

                    set.Stickers = objects;
                }
            }
        }

        private void CreateSetsAndUpdatePanel(int index, TLAllStickers allStickers)
        {
            _allStickers = allStickers;
            
            CreateSets(allStickers);

            UpdateStickersPanel(index);
        }

        private void CreateSets(TLAllStickers allStickers)
        {
            _stickerSets.Clear();
            var recentlyUsedKey = @"tlg/recentlyUsed";
            var favedKey = @"tlg/faved";
            var groupKey = @"tlg/group";
            _stickerSets[recentlyUsedKey] = new List<TLDocument22>();
            _stickerSets[favedKey] = new List<TLDocument22>();
            _stickerSets[groupKey] = new List<TLDocument22>();

            if (_messagesStickerSet != null)
            {
                for (var i = 0; i < _messagesStickerSet.Documents.Count; i++)
                {
                    var document22 = _messagesStickerSet.Documents[i] as TLDocument22;
                    if (document22 != null)
                    {
                        List<TLDocument22> stickers;
                        if (_stickerSets.TryGetValue(groupKey, out stickers))
                        {
                            stickers.Add(document22);
                        }
                        else
                        {
                            _stickerSets[groupKey] = new List<TLDocument22> { document22 };
                        }
                    }
                }
            }

            if (allStickers == null) return;

            for (var i = 0; i < allStickers.Documents.Count; i++)
            {
                var document22 = allStickers.Documents[i] as TLDocument22;
                if (document22 != null)
                {
                    if (document22.StickerSet != null)
                    {
                        var setId = document22.StickerSet.Name;
                        List<TLDocument22> stickers;
                        if (_stickerSets.TryGetValue(setId, out stickers))
                        {
                            stickers.Add(document22);
                        }
                        else
                        {
                            _stickerSets[setId] = new List<TLDocument22> {document22};
                        }
                    }
                }
            }

            var allStickers43 = allStickers as TLAllStickers43;
            if (allStickers43 != null)
            {
                var favedStickersCache = new Dictionary<long, long>();
                if (allStickers43.FavedStickers != null && allStickers43.FavedStickers.Documents.Count > 0)
                {
                    _favedStickersButton.Visibility = Visibility.Visible;
                    var config = IoC.Get<ICacheService>().GetConfig() as TLConfig71;
                    var stickersFavedLimit = config != null ? config.StickersFavedLimit.Value : 5;

                    for (var i = 0; i < allStickers43.FavedStickers.Documents.Count && i < stickersFavedLimit; i++)
                    {
                        var document22 = allStickers43.FavedStickers.Documents[i] as TLDocument22;
                        if (document22 != null)
                        {
                            favedStickersCache[document22.Index] = document22.Index;

                            List<TLDocument22> stickers;
                            if (_stickerSets.TryGetValue(favedKey, out stickers))
                            {
                                stickers.Add(document22);
                            }
                            else
                            {
                                _stickerSets[favedKey] = new List<TLDocument22> { document22 };
                            }
                        }
                    }
                }
                else
                {
                    _favedStickersButton.Visibility = Visibility.Collapsed;
                }

                if (allStickers43.RecentStickers != null)
                {
                    for (var i = 0; i < allStickers43.RecentStickers.Documents.Count; i++)
                    {
                        var document22 = allStickers43.RecentStickers.Documents[i] as TLDocument22;
                        if (document22 != null && !favedStickersCache.ContainsKey(document22.Index))
                        {
                            List<TLDocument22> stickers;
                            if (_stickerSets.TryGetValue(recentlyUsedKey, out stickers))
                            {
                                stickers.Add(document22);
                            }
                            else
                            {
                                _stickerSets[recentlyUsedKey] = new List<TLDocument22> {document22};
                            }
                        }
                    }
                }
            }
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

        public static readonly DependencyProperty RecentItemsProperty = DependencyProperty.Register(
            "RecentItems", typeof (IList<EmojiDataItem>), typeof (EmojiControl), new PropertyMetadata(default(IList<EmojiDataItem>)));

        public IList<EmojiDataItem> RecentItems
        {
            get { return (IList<EmojiDataItem>) GetValue(RecentItemsProperty); }
            set { SetValue(RecentItemsProperty, value); }
        }

        public void LoadRecents()
        {
            CurrentCategory = RecentsCategoryIndex;

            if (EmojiData.Recents == null)
            {
                EmojiData.LoadRecents();
            }

            RecentItems = new ObservableCollection<EmojiDataItem>(EmojiData.Recents ?? new List<EmojiDataItem>());

            CSV.IsHitTestVisible = false;
            Recents.Visibility = Visibility.Visible;
        }

        public void UnloadRecents()
        {
            CSV.IsHitTestVisible = true;
            Recents.Visibility = Visibility.Collapsed;
        }

        #endregion Recents

        private void OnEmojiSelected(object sender, EmojiSelectedEventArgs args)
        {
            TextBoxTarget.Dispatcher.BeginInvoke(() =>
            {
                var selectionStart = TextBoxTarget.SelectionStart;
                TextBoxTarget.Text = TextBoxTarget.Text.Insert(selectionStart, args.DataItem.String);
                TextBoxTarget.Select(selectionStart + args.DataItem.String.Length, 0);
            });

            if (_currentCategory == RecentsCategoryIndex) return;

            var that = args.DataItem;
            ThreadPool.QueueUserWorkItem(state => EmojiData.AddToRecents(that));
        }

        public event EventHandler<StickerSelectedEventArgs> StickerSelected;

        protected virtual void RaiseStickerSelected(StickerSelectedEventArgs e)
        {
            var handler = StickerSelected;
            if (handler != null) handler(this, e);
        }

        private void OnStickerSelected(object sender, StickerSelectedEventArgs args)
        {
            UpdateRecentStickers(args);

            RaiseStickerSelected(args);
        }

        private void OnFeaturedStickerSelected(object sender, StickerSelectedEventArgs args)
        {
            OnStickerSetOpened(sender, new StickerSetOpenedEventArgs{ Set = args.Set });
        }

        private bool _reloadStickerSprites;

        public void UpdateRecentStickers(StickerSelectedEventArgs args)
        {
            if (args == null) return;
            var stickerId = args.Sticker.Document.Id;

            Execute.BeginOnThreadPool(() =>
            {
                var stateService = IoC.Get<IStateService>();
                stateService.GetAllStickersAsync(cachedStickers =>
                {
                    var allStickers = cachedStickers as TLAllStickers43;
                    if (allStickers != null)
                    {
                        var recentStickers = allStickers.RecentStickers.Documents;
                        if (recentStickers != null)
                        {
                            var isAdded = false;
                            for (var i = 0; i < recentStickers.Count; i++)
                            {
                                var recentlyUsedSticker = recentStickers[i];

                                if (recentStickers[i].Id.Value == stickerId.Value)
                                {
                                    if (i != 0)
                                    {
                                        recentStickers.RemoveAt(i);
                                        recentStickers.Insert(0, recentlyUsedSticker);

                                        var recentStickers76 = allStickers.RecentStickers as TLRecentStickers76;
                                        if (recentStickers76 != null)
                                        {
                                            recentStickers76.Dates.RemoveAt(i);
                                            recentStickers76.Dates.Insert(0, TLUtils.DateToUniversalTimeTLInt(IoC.Get<IMTProtoService>().ClientTicksDelta, DateTime.Now));
                                        }

                                        _reloadStickerSprites = true;
                                    }
                                    isAdded = true;
                                    break;
                                }
                            }

                            if (!isAdded)
                            {
                                recentStickers.Insert(0, args.Sticker.Document);

                                var recentStickers76 = allStickers.RecentStickers as TLRecentStickers76;
                                if (recentStickers76 != null)
                                {
                                    recentStickers76.Dates.Insert(0, TLUtils.DateToUniversalTimeTLInt(IoC.Get<IMTProtoService>().ClientTicksDelta, DateTime.Now));
                                }

                                _reloadStickerSprites = true;
                            }

                            if (_stickerSets != null)
                            {
                                var recentlyUsedKey = @"tlg/recentlyUsed";
                                _stickerSets[recentlyUsedKey] = new List<TLDocument22>();
                                var favedStickersCache = new Dictionary<long, long>();
                                if (allStickers.FavedStickers != null && allStickers.FavedStickers.Documents.Count > 0)
                                {
                                    var config = IoC.Get<ICacheService>().GetConfig() as TLConfig71;
                                    var stickersFavedLimit = config != null ? config.StickersFavedLimit.Value : 5;
                                    for (var i = 0; i < allStickers.FavedStickers.Documents.Count && i < stickersFavedLimit; i++)
                                    {
                                        var document22 = allStickers.FavedStickers.Documents[i] as TLDocument22;
                                        if (document22 != null)
                                        {
                                            favedStickersCache[document22.Index] = document22.Index;
                                        }
                                    }
                                }

                                if (allStickers.RecentStickers != null)
                                {
                                    for (var i = 0; i < allStickers.RecentStickers.Documents.Count; i++)
                                    {
                                        var document22 = allStickers.RecentStickers.Documents[i] as TLDocument22;
                                        if (document22 != null && !favedStickersCache.ContainsKey(document22.Index))
                                        {
                                            List<TLDocument22> stickers;
                                            if (_stickerSets.TryGetValue(recentlyUsedKey, out stickers))
                                            {
                                                stickers.Add(document22);
                                            }
                                            else
                                            {
                                                _stickerSets[recentlyUsedKey] = new List<TLDocument22> { document22 };
                                            }
                                        }
                                    }
                                }
                            }

                            stateService.SaveAllStickersAsync(cachedStickers);
                        }
                    }
                });
            });
        }

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
        private readonly Button _stickerButton = new Button { ClickMode = ClickMode.Press };
        private readonly RepeatButton _backspaceButton = new RepeatButton { ClickMode = ClickMode.Release, Interval = 100 };

        private readonly Button _emojiButton = new Button { ClickMode = ClickMode.Press };
        private readonly Button _favedStickersButton = new Button { ClickMode = ClickMode.Press };
        private readonly Button _recentStickersButton = new Button { ClickMode = ClickMode.Press };
        private readonly Button _groupStickersButton = new Button { ClickMode = ClickMode.Release };
        private readonly List<Button> _stickerSetButtons = new List<Button>();
        private readonly Button _featuredStickersButton = new Button { ClickMode = ClickMode.Press };
        private Grid _featuredStickersGrid;
        private UnreadCounter _featuredStickersCounter;
        private readonly Button _settingsButton = new Button { ClickMode = ClickMode.Release };

        public const int RecentsCategoryIndex = 5;
        public const int StickerCategoryIndex = 6;
        public const int EmojiCategoryIndex = 7;
        public const int FeaturedStickersCategoryIndex = 8;
        public const int FavedStickersCategoryIndex = 9;
        public const int GroupStickersCategoryIndex = 10;
        public const int RecentStickersCategoryIndex = 11;

        private int GetStickerSetButtonByKey(string key)
        {
            if (key == null)
            {
                return -1;
            }

            for (var i = 0; i < _stickerSetButtons.Count; i++)
            {
                if (string.Equals(key, _stickerSetButtons[i].DataContext.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return RecentStickersCategoryIndex + i + 1;
                }
            }

            return -1;
        }

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
                case StickerCategoryIndex:
                    return _stickerButton;
                case EmojiCategoryIndex:
                    return _emojiButton;
                case FeaturedStickersCategoryIndex:
                    return _featuredStickersButton;
                case FavedStickersCategoryIndex:
                    return _favedStickersButton;
                case GroupStickersCategoryIndex:
                    return _groupStickersButton;
                case RecentStickersCategoryIndex:
                    return _recentStickersButton;
                default:
                    {
                        if (index > RecentStickersCategoryIndex)
                        {
                            index -= RecentStickersCategoryIndex;
                            if (index > 0
                                && _stickerSetButtons.Count >= index)
                            {
                                return _stickerSetButtons[index - 1];
                            }
                        }

                        return null;
                    }
            }
        }

        public Brush ButtonBackground
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                return isLightTheme
                    ? new SolidColorBrush(Colors.White)
                    : new SolidColorBrush(Color.FromArgb(255, 71, 71, 71));
            }
        }

        private void UpdateButtons(bool isStickersPanelVisible)
        {
            IsStickersPanelVisible = isStickersPanelVisible;

            ButtonsGrid.Children.Clear();
            ButtonsGrid.ColumnDefinitions.Clear();

            var columnsCount = IsStickersPanelVisible ? 9 : 8;
            for (var i = 0; i < columnsCount; i++)
            {
                ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            ButtonsGrid.Children.Add(_abcButton);
            ButtonsGrid.Children.Add(_recentsButton);
            ButtonsGrid.Children.Add(_cat0Button);
            ButtonsGrid.Children.Add(_cat1Button);
            ButtonsGrid.Children.Add(_cat2Button);
            ButtonsGrid.Children.Add(_cat3Button);
            ButtonsGrid.Children.Add(_cat4Button);
            if (IsStickersPanelVisible)
            {
                ButtonsGrid.Children.Add(_stickerButton);
            }
            ButtonsGrid.Children.Add(_backspaceButton);
        }

        public void LoadButtons()
        {
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            var buttonStyleResourceKey = isLightTheme ? "CategoryButtonLightThemeStyle" : "CategoryButtonDarkThemeStyle";
            var buttonStyle = (Style)Resources[buttonStyleResourceKey];

            _abcButton.Style = buttonStyle;
            _recentsButton.Style = buttonStyle;
            _cat0Button.Style = buttonStyle;
            _cat1Button.Style = buttonStyle;
            _cat2Button.Style = buttonStyle;
            _cat3Button.Style = buttonStyle;
            _cat4Button.Style = buttonStyle;
            _stickerButton.Style = buttonStyle;

            _emojiButton.Style = buttonStyle;
            _favedStickersButton.Style = buttonStyle;
            _groupStickersButton.Style = buttonStyle;
            _recentStickersButton.Style = buttonStyle;
            _featuredStickersButton.Style = buttonStyle;
            _settingsButton.Style = buttonStyle;

            var repeatButtonStyleResourceKey = isLightTheme ? "RepeatButtonLightThemeStyle" : "RepeatButtonDarkThemeStyle";
            _backspaceButton.Style = (Style)Resources[repeatButtonStyleResourceKey];

            var prefix = isLightTheme ? "light." : string.Empty;
            _abcButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.abc")),
                Width = 34,
                Height = 32
            };
            _recentsButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.recent")),
                Width = 34,
                Height = 32
            };
            _cat0Button.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.category.1")),
                Width = 34,
                Height = 32
            };
            _cat1Button.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.category.2")),
                Width = 34,
                Height = 32
            };
            _cat2Button.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.category.3")),
                Width = 34,
                Height = 32
            };
            _cat3Button.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.category.4")),
                Width = 34,
                Height = 32
            };
            _cat4Button.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.category.5")),
                Width = 34,
                Height = 32
            };
            _stickerButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.sticker")),
                Width = 34,
                Height = 32
            };
            _backspaceButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.backspace")),
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
            Grid.SetColumn(_stickerButton, 7);
            Grid.SetColumn(_backspaceButton, 8);

            _emojiButton.Width = 47.0;
            _emojiButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.category.1")),
                Width = 34,
                Height = 32
            };
            _featuredStickersButton.Width = 78.0;
            _featuredStickersButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.featured")),
                Width = 34,
                Height = 32
            };
            _favedStickersButton.Width = 78.0;
            _favedStickersButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.faved")),
                Width = 34,
                Height = 32
            };
            _groupStickersButton.Width = 78.0;
            _groupStickersButton.Content = new ConversationTileControl
            {
                Size = 45,
                LabelFontSize = 19
            };
            _recentStickersButton.Width = 78.0;
            _recentStickersButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.recent")),
                Width = 34,
                Height = 32
            };
            _settingsButton.Width = 78.0;
            _settingsButton.Content = new Image
            {
                Source = new BitmapImage(Helpers.GetAssetUri(prefix + "emoji.settings")),
                Width = 34,
                Height = 32
            };

            //Grid.SetColumn(_emojiButton, 0);
            //Grid.SetColumn(_recentStickersButton, 1);

            _abcButton.Click += AbcButtonOnClick;
            _cat0Button.Click += CategoryButtonClick;
            _cat1Button.Click += CategoryButtonClick;
            _cat2Button.Click += CategoryButtonClick;
            _cat3Button.Click += CategoryButtonClick;
            _cat4Button.Click += CategoryButtonClick;
            _stickerButton.Click += StickerButtonOnClick;
            _recentsButton.Click += CategoryButtonClick;
            _backspaceButton.Click += BackspaceButtonOnClick;

            _emojiButton.Click += EmojiButtonOnClick;
            _favedStickersButton.Click += FavedStickersButtonOnClick;
            _groupStickersButton.Click += GroupStickersButtonOnClick;
            _recentStickersButton.Click += RecentStickersButtonOnClick;
            _featuredStickersButton.Click += FeaturedStickersButtonOnClick;
            _settingsButton.Click += SettingsButtonOnClick;

            StickersPanel.Children.Clear();
            StickersPanel.Children.Add(_emojiButton);

            var featuredStickersCounter = new UnreadCounter
            {
                BorderBackground = (Brush) new OverlayAccentBrushConverter{ AccentColor = ((SolidColorBrush)Application.Current.Resources["TelegramBadgeAccentBrush"]).Color }.Convert(null, null, null, null),
                Counter = 0,
                Margin = new Thickness(6, 6, 9, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                IsHitTestVisible = false
            };
            var featuredStickersGrid = new Grid();
            featuredStickersGrid.Children.Add(_featuredStickersButton);
            featuredStickersGrid.Children.Add(featuredStickersCounter);
            _featuredStickersGrid = featuredStickersGrid;
            _featuredStickersCounter = featuredStickersCounter;
            StickersPanel.Children.Add(_featuredStickersGrid);
            StickersPanel.Children.Add(_favedStickersButton);
            StickersPanel.Children.Add(_groupStickersButton);
            StickersPanel.Children.Add(_recentStickersButton);

            StickersPanel.Children.Add(_settingsButton);

            UpdateButtons(IsStickersPanelVisible);
        }

        private void AbcButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            TextBoxTarget.Focus();
        }

        private void SettingsButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            RaiseSettingsButtonClick();
        }

        private void FeaturedStickersButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_featuredStickers != null)
            {
                LoadFeaturedStickerSets(_featuredStickers.SetsCovered);
            }
            else
            {
                FeaturedStickersVirtPanel.ClearItems();
                CurrentCategory = FeaturedStickersCategoryIndex;
            }
        }

        private void GroupStickersButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var key = @"tlg/group";
            List<TLDocument22> stickerSet;
            if (_stickerSets != null && _stickerSets.TryGetValue(@"tlg/group", out stickerSet))
            {
                LoadStickerSet(key, stickerSet);
            }
            else
            {
                VirtPanel.ClearItems();
                _groupStickersSprites = new List<VListItemBase>();
                CurrentCategory = GroupStickersCategoryIndex;
            }
        }

        private void FavedStickersButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var key = @"tlg/faved";
            List<TLDocument22> stickerSet;
            if (_stickerSets != null && _stickerSets.TryGetValue(@"tlg/faved", out stickerSet))
            {
                LoadStickerSet(key, stickerSet);
            }
            else
            {
                VirtPanel.ClearItems();
                _favedStickersSprites = new List<VListItemBase>();
                CurrentCategory = FavedStickersCategoryIndex;
            }
        }

        private void RecentStickersButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var key = @"tlg/recentlyUsed";
            List<TLDocument22> stickerSet;
            if (_stickerSets != null && _stickerSets.TryGetValue(@"tlg/recentlyUsed", out stickerSet))
            {
                LoadStickerSet(key, stickerSet);
            }
            else
            {
                VirtPanel.ClearItems();
                _recentStickersSprites = new List<VListItemBase>();
                CurrentCategory = RecentStickersCategoryIndex;
            }
        }

        private void StickerButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ButtonsGrid.Visibility = Visibility.Collapsed;
            StickersGrid.Visibility = Visibility.Visible;
            StickersScrollViewer.ScrollToHorizontalOffset(0.0);

            LoadCategory(StickerCategoryIndex);
        }

        private void EmojiButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            ButtonsGrid.Visibility = Visibility.Visible;
            StickersGrid.Visibility = Visibility.Collapsed;

            if (EmojiData.Recents == null)
            {
                EmojiData.LoadRecents();
            }

            if (EmojiData.Recents == null || EmojiData.Recents.Count == 0)
            {
                LoadCategory(0);
            }
            else
            {
                LoadCategory(RecentsCategoryIndex);
            }
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
            else if (sender == _favedStickersButton)
                LoadCategory(FavedStickersCategoryIndex);
            else if (sender == _recentsButton)
                LoadCategory(RecentsCategoryIndex);
            else if (sender == _stickerButton)
                LoadCategory(StickerCategoryIndex);
        }

        private void CreateButtonsBackgrounds(int categoryIndex)
        {
            var sprites = EmojiData.SpriteRowsCountByCategory[categoryIndex];
            var buttonBackgroundColor = ButtonBackground;
            for (var i = 0; i < sprites.Length; i++)
            {
                var rowsCount = sprites[i];

                var block = new Rectangle
                {
                    Width = EmojiSpriteItem.SpriteWidth,
                    Height = EmojiSpriteItem.RowHeight * rowsCount,
                    Fill = buttonBackgroundColor,
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
                    SetHeight(PortraitOrientationHeight);
                    //_frameTransform.Y = -PortraitOrientationHeight;
                    break;

                case Orientation.Horizontal:
                    ButtonsGrid.Height = 58;
                    ButtonsGrid.Margin = new Thickness(0, 6, 0, 3);
                    SetHeight(AlbumOrientationHeight);
                    //_frameTransform.Y = -AlbumOrientationHeight;
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

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            ((Border)sender).Background = (Brush)Application.Current.Resources["TelegramBadgeAccentBrush"];
        }

        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ((Border) sender).Background = ButtonBackground;
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ((Border) sender).Background = ButtonBackground;
        }

        private void EmojiButton_OnTap(object sender, GestureEventArgs e)
        {
            var button = (FrameworkElement)sender;
            var emojiItem = (EmojiDataItem)button.DataContext;

            OnEmojiSelected(sender, new EmojiSelectedEventArgs{DataItem = emojiItem});

            ////RaiseEmojiAdded(new EmojiAddedEventArgs { Emoji = emojiItem.String });

            //if (_currentCategory != RecentsCategoryIndex)
            //{
            //    var prevItem = RecentItems.FirstOrDefault(x => x.Code == emojiItem.Code);
            //    if (prevItem != null)
            //    {
            //        RecentItems.Remove(prevItem);
            //        RecentItems.Insert(0, prevItem);
            //    }
            //    else
            //    {
            //        RecentItems.Insert(0, emojiItem);
            //        RecentItems = RecentItems.Take(30).ToList();
            //    }
            //}
        }

        public void ReloadStickerSprites()
        {
            if (_reloadStickerSprites)
            {
                _recentStickersSprites = null;
            }
        }

        public void ClearRecentStickers()
        {
            _recentStickersSprites = null;
        }

        public void OpenStickerSprites()
        {
            if (_reloadStickerSprites)
            {
                if (_currentCategory == StickerCategoryIndex)
                LoadCategory(_currentCategory);
            }
        }

        public void ResetFavedStickers()
        {
            var allStickers43 = IoC.Get<IStateService>().GetAllStickers() as TLAllStickers43;
            if (allStickers43 != null && allStickers43.FavedStickers != null)
            {
                var config = IoC.Get<ICacheService>().GetConfig() as TLConfig71;
                var stickersFavedLimit = config != null ? config.StickersFavedLimit.Value : 5;

                var favedKey = @"tlg/faved";
                _stickerSets[favedKey] = new List<TLDocument22>();

                if (allStickers43.FavedStickers.Documents.Count > 0)
                {
                    _favedStickersButton.Visibility = Visibility.Visible;

                    var favedStickersCache = new Dictionary<long, long>();
                    for (var i = 0; i < allStickers43.FavedStickers.Documents.Count && i < stickersFavedLimit; i++)
                    {
                        var document22 = allStickers43.FavedStickers.Documents[i] as TLDocument22;
                        if (document22 != null)
                        {
                            favedStickersCache[document22.Index] = document22.Index;

                            List<TLDocument22> stickers;
                            if (_stickerSets.TryGetValue(favedKey, out stickers))
                            {
                                stickers.Add(document22);
                            }
                            else
                            {
                                _stickerSets[favedKey] = new List<TLDocument22> {document22};
                            }
                        }
                    }
                    _stickerSetSprites.Remove(favedKey);
                    _favedStickersSprites = null;

                    if (allStickers43.RecentStickers != null)
                    {
                        var recentKey = @"tlg/recentlyUsed";
                        _stickerSets[recentKey] = new List<TLDocument22>();
                        for (var i = 0; i < allStickers43.RecentStickers.Documents.Count; i++)
                        {
                            var document22 = allStickers43.RecentStickers.Documents[i] as TLDocument22;
                            if (document22 != null && !favedStickersCache.ContainsKey(document22.Index))
                            {
                                List<TLDocument22> stickers;
                                if (_stickerSets.TryGetValue(recentKey, out stickers))
                                {
                                    stickers.Add(document22);
                                }
                                else
                                {
                                    _stickerSets[recentKey] = new List<TLDocument22> { document22 };
                                }
                            }
                        }
                        _stickerSetSprites.Remove(recentKey);
                        _recentStickersSprites = null;
                    }

                    if (CurrentCategory == FavedStickersCategoryIndex)
                    {
                        FavedStickersButtonOnClick(null, null);
                    }
                }
                else
                {
                    _favedStickersButton.Visibility = Visibility.Collapsed;
                    if (CurrentCategory == FavedStickersCategoryIndex)
                    {
                        RecentStickersButtonOnClick(null, null);
                    }
                }
            }
        }

        public void ResetStickerSets()
        {
            var stateService = IoC.Get<IStateService>();
            stateService.GetAllStickersAsync(cachedStickers =>
            {
                Execute.BeginOnUIThread(() =>
                {
                    CreateSetsAndUpdatePanel(_currentCategory, cachedStickers);
                    _recentStickersSprites = null;
                });
            });
        }

        public void ClearStickersOnLogOut()
        {
            CreateSetsAndUpdatePanel(_currentCategory, null);
            _recentStickersSprites = null;
            _stickerSets.Clear();
            _stickerSetSprites.Clear();
        }

        #region Stickers preview

        private static DateTime? _startTime;
        private static FrameworkElement _fromItem;
        private static Storyboard _storyboard;
        private static FrameworkElement _lastMouseEnter;
        //private static ScrollViewer ViewportControl;
        //private static Canvas Canvas;
        //private static Grid Preview;
        //private static Grid PreviewGrid;
        //private static Image PreviewImage;
        //private static Image Image;
        //private static TextBlock DebugText;

        private void StickerPreviewGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            var fromItem = _fromItem;
            if (fromItem == null) return;

            var position = fromItem.TransformToVisual(Application.Current.RootVisual).Transform(new Point(fromItem.ActualWidth / 2.0, fromItem.ActualHeight / 2.0));
            //DebugText.Text = position.ToString();

            var position2 = new Point(240.0, 400.0); //PreviewImage.TransformToVisual(Application.Current.RootVisual).Transform(new Point(PreviewImage.ActualWidth / 2.0, PreviewImage.ActualHeight / 2.0));//
            //DebugText.Text += Environment.NewLine + position2;

            var duration = .75;
            IEasingFunction easingFunction = new ElasticEase { Oscillations = 1, Springiness = 10.0, EasingMode = EasingMode.EaseOut };
            var storyboard = new Storyboard();

            var doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = position.X - position2.X;
            doubleAnimation.To = 0.0;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            storyboard.Children.Add(doubleAnimation);

            var doubleAnimation2 = new DoubleAnimation();
            doubleAnimation2.From = position.Y - position2.Y; //position.Y;
            doubleAnimation2.To = 0.0;
            doubleAnimation2.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation2.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation2, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(doubleAnimation2);

            var doubleAnimation3 = new DoubleAnimation();
            doubleAnimation3.From = .5;
            doubleAnimation3.To = 1.0;
            doubleAnimation3.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation3.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation3, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation3, new PropertyPath("Opacity"));
            storyboard.Children.Add(doubleAnimation3);

            var doubleAnimation4 = new DoubleAnimation();
            doubleAnimation4.From = 1.0;
            doubleAnimation4.To = 2.6;
            doubleAnimation4.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation4.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation4, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation4, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));
            storyboard.Children.Add(doubleAnimation4);

            var doubleAnimation5 = new DoubleAnimation();
            doubleAnimation5.From = 1.0;
            doubleAnimation5.To = 2.6;
            doubleAnimation5.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation5.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation5, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation5, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));
            storyboard.Children.Add(doubleAnimation5);

            storyboard.Begin();

            if (_loadedStoryboard != null)
            {
                _loadedStoryboard.Begin();
                _loadedStoryboard = null;
            }

            _storyboard = storyboard;
        }

        private DispatcherTimer _timer;

        private void StickerPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Preview.Visibility == Visibility.Collapsed) return;

            var st1 = GetScaleStoryboard(_lastMouseEnter ?? _fromItem, 1.0, 1.0);

            _lastMouseEnter = e.OriginalSource as FrameworkElement;

            RestartMenuTimer();

            var stickerImage = e.OriginalSource as Image;
            if (stickerImage != null)
            {
                PreviewImage.Source = stickerImage.Source;

                var stickerItem = stickerImage.DataContext as TLStickerItem;
                if (stickerItem != null)
                {
                    Image.DataContext = stickerItem;
                }
            }

            var duration = .5;
            var easingFunction = new ElasticEase { Oscillations = 1, Springiness = 10.0, EasingMode = EasingMode.EaseOut };

            var storyboard = new Storyboard();

            var doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 0.0;
            doubleAnimation.To = 0.0;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            storyboard.Children.Add(doubleAnimation);

            var doubleAnimation2 = new DoubleAnimation();
            doubleAnimation2.From = 0.0; //position.Y;
            doubleAnimation2.To = 0.0;
            doubleAnimation2.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation2.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation2, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(doubleAnimation2);

            var doubleAnimation3 = new DoubleAnimation();
            doubleAnimation3.From = 1.0;
            doubleAnimation3.To = 1.0;
            doubleAnimation3.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation3.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation3, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation3, new PropertyPath("Opacity"));
            storyboard.Children.Add(doubleAnimation3);

            var doubleAnimation4 = new DoubleAnimation();
            doubleAnimation4.From = 2.4;
            doubleAnimation4.To = 2.6;
            doubleAnimation4.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation4.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation4, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation4, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));
            storyboard.Children.Add(doubleAnimation4);

            var doubleAnimation5 = new DoubleAnimation();
            doubleAnimation5.From = 2.4;
            doubleAnimation5.To = 2.6;
            doubleAnimation5.Duration = new Duration(TimeSpan.FromSeconds(duration));
            doubleAnimation5.EasingFunction = easingFunction;
            Storyboard.SetTarget(doubleAnimation5, PreviewGrid);
            Storyboard.SetTargetProperty(doubleAnimation5, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));
            storyboard.Children.Add(doubleAnimation5);

            storyboard.Begin();

            _storyboard = storyboard;

            var st2 = GetScaleStoryboard(_lastMouseEnter, 0.85, 1.0);
            if (st1 != null || st2 != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    if (st1 != null) st1.Begin();
                    if (st2 != null) st2.Begin();
                });
            }
        }

        private void RestartMenuTimer()
        {
            if (!_isSearchOpened
                && CurrentCategory != RecentStickersCategoryIndex 
                && CurrentCategory != FavedStickersCategoryIndex 
                && CurrentCategory != GroupStickersCategoryIndex
                && CurrentCategory != FeaturedStickersCategoryIndex)
            {
                return;
            }

            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(1.2);
                _timer.Tick += Timer_OnTick;
            }
            _timer.Stop();
            _timer.Start();
        }

        private void Timer_OnTick(object sender, EventArgs e)
        {
            _timer.Stop();

            var item = _lastMouseEnter ?? _fromItem;
            if (item == null) return;

            var sticker = item.DataContext as TLStickerItem;
            if (sticker == null) return;

            var document22 = sticker.Document as TLDocument22;
            if (document22 == null) return;

            var stickerSet = document22.StickerSet;
            if (stickerSet == null) return;

            var stickerSetName = stickerSet.Name;
            if (string.IsNullOrEmpty(stickerSetName)) return;

            if (!_isSearchOpened
                && CurrentCategory != RecentStickersCategoryIndex 
                && CurrentCategory != FavedStickersCategoryIndex 
                && CurrentCategory != GroupStickersCategoryIndex
                && CurrentCategory != FeaturedStickersCategoryIndex) return;

            if (Preview == null || Preview.Visibility == Visibility.Collapsed) return;

            var stickerPreviewMenu = StickerPreviewMenuPlaceholder.Content as StickerPreviewMenu;
            if (stickerPreviewMenu == null)
            {
                stickerPreviewMenu = new StickerPreviewMenu(Preview);

                stickerPreviewMenu.Send += (o, args) =>
                {
                    StickerPanel_ManipulationCompleted(null, null);

                    OnStickerSelected(this, args);
                };
                
                stickerPreviewMenu.OpenPack += (o, args) =>
                {
                    StickerPanel_ManipulationCompleted(null, null);

                    if (args.Button == null || _isSearchOpened)
                    {
                        var mtProtoService = IoC.Get<IMTProtoService>();
                        mtProtoService.GetStickerSetAsync(stickerPreviewMenu.InputStickerSet, 
                            result => Execute.BeginOnUIThread(() =>                        
                            {
                                var stickerSet32 = result.Set as TLStickerSet32;
                                if (stickerSet32 != null)
                                {
                                    stickerSet32.Stickers = new TLVector<TLObject>();
                                    for (var i = 0; i < result.Documents.Count; i++)
                                    {
                                        var document = result.Documents[i] as TLDocument22;
                                        if (document != null)
                                        {
                                            stickerSet32.Stickers.Add(new TLStickerItem { Document = document });
                                        }
                                    }

                                    TelegramViewBase.ShowStickerSetMessageBox(false, stickerSet32.Installed, stickerSet32, prompt =>
                                    {
                                        if (prompt == PopUpResult.Ok)
                                        {
                                            OnStickerSetAdded(sender, new StickerSetAddedEventArgs { Set = stickerSet32 });
                                        }
                                    });
                                }
                            }));
                    }
                    else
                    {
                        StickerSetButtonOnClick(args.Button, null);

                        StickersScrollViewer.ScrollIntoView(args.Button, (StickersScrollViewer.ActualWidth - args.Button.ActualWidth) / 2.0, 0.0, TimeSpan.Zero);
                    }
                };

                stickerPreviewMenu.ChangeFaved += (o, args) =>
                {
                    StickerPanel_ManipulationCompleted(null, null);

                    var document = args.Sticker.Document as TLDocument;
                    if (document != null)
                    {
                        var allStickers = IoC.Get<IStateService>().GetAllStickers() as TLAllStickers43;
                        if (allStickers != null)
                        {
                            var favedStickers = allStickers.FavedStickers;
                            if (favedStickers != null)
                            {
                                var unfave = favedStickers.Documents.FirstOrDefault(x => x.Index == args.Sticker.Document.Index) != null;

                                IoC.Get<IMTProtoService>().FaveStickerAsync(new TLInputDocument{ Id = document.Id, AccessHash = document.AccessHash }, new TLBool(unfave), 
                                    result => Execute.BeginOnUIThread(() =>
                                    {
                                        if (unfave)
                                        {
                                            favedStickers.RemoveSticker(args.Sticker.Document);
                                        }
                                        else
                                        {
                                            favedStickers.AddSticker(args.Sticker.Document);
                                        }

                                        allStickers.FavedStickers = favedStickers;
                                        IoC.Get<IStateService>().SaveAllStickersAsync(allStickers);

                                        if (unfave)
                                        {
                                            IoC.Get<IMTProtoService>().GetFavedStickersAsync(allStickers.FavedStickers.Hash,
                                                result2 =>
                                                {
                                                    var favedStickers2 = result2 as TLFavedStickers;
                                                    if (favedStickers2 != null)
                                                    {
                                                        allStickers.FavedStickers = favedStickers;
                                                        IoC.Get<IStateService>().SaveAllStickersAsync(allStickers);

                                                        Execute.BeginOnUIThread(() =>
                                                        {
                                                            ResetFavedStickers();
                                                        });
                                                    }
                                                },
                                                error2 =>
                                                {

                                                });
                                        }


                                        ResetFavedStickers();
                                    }),
                                    error =>
                                    {
                                        
                                    });
                            }
                        }
                    }
                };
                
                stickerPreviewMenu.Closed += (o, args) =>
                {
                    StickerPanel_ManipulationCompleted(null, null);
                };

                StickerPreviewMenuPlaceholder.Content = stickerPreviewMenu;
            }

            stickerPreviewMenu.SetStickerItem(sticker);
            stickerPreviewMenu.InputStickerSet = stickerSet;
            var stickerSetButton = _stickerSetButtons.FirstOrDefault(x => (string)x.DataContext == stickerSetName);
            //if (stickerSetButton == null) return;
            stickerPreviewMenu.SetButton(stickerSetButton);

            var stickers = IoC.Get<IStateService>().GetAllStickers() as TLAllStickers43;
            if (stickers != null)
            {
                var favedStickers = stickers.FavedStickers;
                if (favedStickers != null)
                {
                    var exists = favedStickers.Documents.FirstOrDefault(x => x.Index == sticker.Document.Index) != null;
                    stickerPreviewMenu.SwitchFavedStickerLabel.Text = exists ? AppResources.DeleteFromFavorites.ToLowerInvariant() : AppResources.AddToFavorites.ToLowerInvariant();
                }
            }

            VibrateController.Default.Start(TimeSpan.FromSeconds(0.01));
            stickerPreviewMenu.Open();
        }

        public static Storyboard GetScaleStoryboard(FrameworkElement element, double scale, double duration)
        {
            if (element == null) return null;

            var easingFunction = new ElasticEase { Oscillations = 1, Springiness = 10.0, EasingMode = EasingMode.EaseOut };

            var previousAnimation = element.Tag as Storyboard;
            if (previousAnimation != null)
            {
                previousAnimation.Pause();
                element.Tag = null;
            }

            if (!(element.RenderTransform is CompositeTransform))
            {
                element.RenderTransformOrigin = new Point(0.5, 0.5);
                element.RenderTransform = new CompositeTransform();
            }

            var scaleStoryboard = new Storyboard();
            var scaleXAnimation = new DoubleAnimation();
            scaleXAnimation.To = scale;
            scaleXAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            scaleXAnimation.EasingFunction = easingFunction;
            Storyboard.SetTarget(scaleXAnimation, element);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleY)"));
            scaleStoryboard.Children.Add(scaleXAnimation);

            var scaleYAnimation = new DoubleAnimation();
            scaleYAnimation.To = scale;
            scaleYAnimation.Duration = new Duration(TimeSpan.FromSeconds(duration));
            scaleYAnimation.EasingFunction = easingFunction;
            Storyboard.SetTarget(scaleYAnimation, element);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.ScaleX)"));
            scaleStoryboard.Children.Add(scaleYAnimation);
            element.Tag = scaleStoryboard;
            scaleStoryboard.Completed += (o, e) =>
            {
                element.Tag = null;
            };

            return scaleStoryboard;
            //Deployment.Current.Dispatcher.BeginInvoke(
            //    () =>
            //    {
            //        scaleStoryboard.Begin();
            //}
            //);
        }

        private bool _runOnce = true;

        private void FeaturedStickersPanel_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (CurrentCategory != FeaturedStickersCategoryIndex) return;

            if (_runOnce)
            {
                _runOnce = false;
                LoadNextFeaturedStickersSlice();
            }

            StartTouchFrameReporting(e);
        }

        private ManipulationStartedEventArgs _manipulationStartedArgs;

        private void StickerPanel_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (!_isSearchOpened && CurrentCategory < FavedStickersCategoryIndex) return;
            if (!_isSearchOpened && CurrentCategory == RecentStickersCategoryIndex + _stickerSetButtons.Count + 1) return;

            StartTouchFrameReporting(e);
        }

        private void StartTouchFrameReporting(ManipulationStartedEventArgs e)
        {
            _startTime = DateTime.Now;

            _fromItem = e.OriginalSource as FrameworkElement;
            _lastMouseEnter = null;

            _manipulationStartedArgs = e;

            Touch.FrameReported += Touch_FrameReported;
        }

        private Storyboard _loadedStoryboard;

        private void Touch_FrameReported(object sender, TouchFrameEventArgs e)
        {
            if (_manipulationStartedArgs == null)
            {
                Touch.FrameReported -= Touch_FrameReported;
                return;
            }

            var point = e.GetPrimaryTouchPoint(null);
            if (point.Action == TouchAction.Up)
            {
                Touch.FrameReported -= Touch_FrameReported;
                return;
            }

            var manipulationPoint = e.GetPrimaryTouchPoint(_manipulationStartedArgs.ManipulationContainer);
            var length = Math.Pow(manipulationPoint.Position.X - _manipulationStartedArgs.ManipulationOrigin.X, 2.0)
                + Math.Pow(manipulationPoint.Position.Y - _manipulationStartedArgs.ManipulationOrigin.Y, 2.0);
            if (length > 30.0 * 30.0)
            {
                Touch.FrameReported -= Touch_FrameReported;
                return;
            }

            if (_startTime.HasValue && _startTime.Value.AddSeconds(0.5) <= DateTime.Now)
            {
                Touch.FrameReported -= Touch_FrameReported;
                VirtPanel.DisableVerticalScrolling();

                _loadedStoryboard = GetScaleStoryboard(_fromItem, 0.85, 1.0);

                Preview.Visibility = Visibility.Visible;
                var stickerImage = _fromItem as Image;
                if (stickerImage != null)
                {
                    PreviewImage.Source = stickerImage.Source;

                    var stickerItem = stickerImage.DataContext as TLStickerItem;
                    if (stickerItem != null)
                    {
                        Image.DataContext = stickerItem;
                    }
                }

                var grid = Preview;
                grid.Children.Remove(PreviewGrid);

                Execute.BeginOnUIThread(() =>
                {
                    PreviewGrid.RenderTransform = new CompositeTransform();
                    PreviewGrid.Opacity = 0.0;
                    grid.Children.Add(PreviewGrid);

                    RestartMenuTimer();
                });
            }
        }

        private void StickerPanel_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            _startTime = null;
        }

        private void StickerPanel_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var st = GetScaleStoryboard(_lastMouseEnter ?? _fromItem, 1.0, 1.0);
            if (st != null)
            {
                Execute.BeginOnUIThread(st.Begin);
            }

            var stickerPreviewMenu = StickerPreviewMenuPlaceholder.Content as StickerPreviewMenu;
            if (stickerPreviewMenu != null && stickerPreviewMenu.IsOpened)
            {
                return;
            }

            _startTime = null;
            _fromItem = null;
            _lastMouseEnter = null;
            if (_storyboard != null)
            {
                _storyboard.SkipToFill();
            }
            if (_timer != null)
            {
                _timer.Stop();
            }

            VirtPanel.EnableVerticalScrolling();
            Preview.Visibility = Visibility.Collapsed;
        }
        #endregion

        public void ClosePreview()
        {
            Touch.FrameReported -= Touch_FrameReported;

            StickerPanel_ManipulationCompleted(null, null);
        }

        public bool IsPreviewMenuOpened
        {
            get
            {
                var stickerPreviewMenu = StickerPreviewMenuPlaceholder.Content as StickerPreviewMenu;
                if (stickerPreviewMenu != null && stickerPreviewMenu.IsOpened)
                {
                    return true;
                }

                return false;
            }
        }

        public void ClosePreviewMenu()
        {
            var stickerPreviewMenu = StickerPreviewMenuPlaceholder.Content as StickerPreviewMenu;
            if (stickerPreviewMenu != null && stickerPreviewMenu.IsOpened)
            {
                stickerPreviewMenu.Close();
            }
        }

        private TLMessagesStickerSet _messagesStickerSet;

        private TLStickerSetBase _stickerSet;

        public void SetGroupStickers(TLChannel68 group, TLStickerSetBase stickerSet)
        {
            _stickerSet = stickerSet;

            _groupStickersButton.Visibility = Visibility.Collapsed;
            _messagesStickerSet = null;

            if (_stickerSet != null)
            {
                _groupStickersButton.Visibility = Visibility.Visible;
                var tileControl = _groupStickersButton.Content as ConversationTileControl;
                if (tileControl != null)
                {
                    var textBinding = new Binding
                    {
                        Source = group,
                        Converter = new PlaceholderDefaultTextConverter()
                    };

                    tileControl.SetBinding(ConversationTileControl.TextProperty, textBinding);

                    var fillBinding = new Binding("Index")
                    {
                        Source = group,
                        Converter = new IdToPlaceholderBackgroundConverter()
                    };

                    tileControl.SetBinding(ConversationTileControl.FillProperty, fillBinding);

                    var sourceBinding = new Binding("Photo")
                    {
                        Source = group,
                        Converter = new DefaultPhotoConverter()
                    };

                    tileControl.SetBinding(ConversationTileControl.SourceProperty, sourceBinding);
                }

                IoC.Get<IMTProtoService>()
                    .GetStickerSetAsync(new TLInputStickerSetShortName { ShortName = stickerSet.ShortName },
                        result =>
                        {
                            _messagesStickerSet = result;

                            var groupKey = @"tlg/group";
                            _stickerSets[groupKey] = new List<TLDocument22>();

                            for (var i = 0; i < _messagesStickerSet.Documents.Count; i++)
                            {
                                var document22 = _messagesStickerSet.Documents[i] as TLDocument22;
                                if (document22 != null)
                                {
                                    List<TLDocument22> stickers;
                                    if (_stickerSets.TryGetValue(groupKey, out stickers))
                                    {
                                        stickers.Add(document22);
                                    }
                                    else
                                    {
                                        _stickerSets[groupKey] = new List<TLDocument22> { document22 };
                                    }
                                }
                            }

                            _stickerSetSprites.Remove(groupKey);

                        },
                        error =>
                        {

                        });
            }
        }
    }

    public class IsOpenedEventArgs : EventArgs
    {
        public bool IsOpened { get; set; }
    }
}

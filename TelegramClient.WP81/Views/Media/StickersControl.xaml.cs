// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Caliburn.Micro;
using Telegram.Api.Services;
using Telegram.Api.TL;
using Telegram.Controls.VirtualizedView;
using Telegram.EmojiPanel.Controls.Emoji;
using TelegramClient.Converters;
using TelegramClient.Services;
using TelegramClient.Views.Dialogs;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.Views.Media
{
    public partial class StickersControl
    {
        private const string RecentlyUsedStickersKey = "@tlg/recentlyUsed";

        private const string RecentlyUsedMasksKey = "@tlg/recentlyUsedMasks";

        public event EventHandler<StickerSelectedEventArgs> StickerSelected;

        protected virtual void RaiseStickerSelected(StickerSelectedEventArgs e)
        {
            var handler = StickerSelected;
            if (handler != null) handler(this, e);
        }

        private TLAllStickers _masks;

        private TLAllStickers _stickers;

        public StickersControl(TLAllStickers masks, TLAllStickers stickers)
        {
            InitializeComponent();

            LayoutRoot.Opacity = 0.0;

            VirtPanel.InitializeWithScrollViewer(CSV);

            Loaded += OnLoaded;

            _masks = masks;
            _stickers = stickers;

            if (_masks == null)
            {
                LoadMasksAsync();
            }

            if (_stickers == null)
            {
                LoadStickersAsync();
            }

            LoadButtons();
            CreateSetsAndUpdatePanel();
        }

        private void LoadStickersAsync()
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            var mtProtoService = IoC.Get<IMTProtoService>();
            mtProtoService.GetAllStickersAsync(TLString.Empty,
                result =>
                {
                    var allStickers = result as TLAllStickers43;
                    if (allStickers != null)
                    {
                        var stateService = IoC.Get<IStateService>();
                        stateService.SaveAllStickersAsync(allStickers);

                        Execute.BeginOnUIThread(() =>
                        {
                            LoadingProgressBar.Visibility = Visibility.Collapsed;

                            _stickers = result as TLAllStickers;
                            CreateSets(_stickers);
                            UpdateButtons(_stickers, StickersPanel, _recentStickersButton, _stickersButtons);
                            if (StickersScrollViewer.Visibility == Visibility.Visible)
                            {
                                LoadSprites(_stickers, _stickersButtons);
                            }
                        });
                    }
                },
                error => Execute.BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage("messages.getAllStickers error " + error);
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                }));
        }

        private void LoadMasksAsync()
        {
            LoadingProgressBar.Visibility = Visibility.Visible;
            var mtProtoService = IoC.Get<IMTProtoService>();
            mtProtoService.GetMaskStickersAsync(TLString.Empty,
                result =>
                {
                    var allStickers = result as TLAllStickers43;

                    if (allStickers != null)
                    {
                        var stateService = IoC.Get<IStateService>();
                        stateService.SaveMasksAsync(allStickers);

                        Execute.BeginOnUIThread(() =>
                        {
                            LoadingProgressBar.Visibility = Visibility.Collapsed;
                            _masks = allStickers;
                            CreateSets(_masks);
                            UpdateButtons(_masks, MasksPanel, _recentMasksButton, _masksButtons);
                            if (MasksScrollViewer.Visibility == Visibility.Visible)
                            {
                                LoadSprites(_masks, _masksButtons);
                            }
                        });
                    }
                },
                error => Execute.BeginOnUIThread(() =>
                {
                    Execute.ShowDebugMessage("messages.getMaskStickers error " + error);
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                }));
        }

        private readonly Button _recentMasksButton = new Button();

        private readonly List<Button> _masksButtons = new List<Button>();

        private readonly Button _recentStickersButton = new Button(); 

        private readonly List<Button> _stickersButtons = new List<Button>();

        private void LoadButtons()
        {
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            var buttonStyleResourceKey = isLightTheme ? "CategoryButtonLightThemeStyle" : "CategoryButtonDarkThemeStyle";
            var buttonStyle = (Style)Resources[buttonStyleResourceKey];

            var prefix = isLightTheme ? "light." : string.Empty;

            _recentStickersButton.Style = buttonStyle;
            _recentStickersButton.Width = 78.0;
            _recentStickersButton.Content = new Image
            {
                Source = new BitmapImage(Telegram.EmojiPanel.Controls.Utilites.Helpers.GetAssetUri(prefix + "emoji.recent")),
                Width = 34,
                Height = 32
            };
            _recentStickersButton.Click += RecentStickersButtonOnClick;

            _recentMasksButton.Style = buttonStyle;
            _recentMasksButton.Width = 78.0;
            _recentMasksButton.Content = new Image
            {
                Source = new BitmapImage(Telegram.EmojiPanel.Controls.Utilites.Helpers.GetAssetUri(prefix + "emoji.recent")),
                Width = 34,
                Height = 32
            };
            _recentMasksButton.Click += RecentMasksButtonOnClick;
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

        private Button _previousButton;

        private void RecentStickersButtonOnClick(object sender, RoutedEventArgs e)
        {
            SetButtonsBackground(sender);

            List<TLDocument22> stickerSet;
            if (_stickerSets != null && _stickerSets.TryGetValue(RecentlyUsedStickersKey, out stickerSet))
            {
                LoadStickerSet(RecentlyUsedStickersKey, stickerSet);
            }
            else
            {
                VirtPanel.ClearItems();
                _recentStickersSprites = new List<VListItemBase>();
            }
        }

        private void RecentMasksButtonOnClick(object sender, RoutedEventArgs e)
        {
            SetButtonsBackground(sender);

            List<TLDocument22> stickerSet;
            if (_stickerSets != null && _stickerSets.TryGetValue(RecentlyUsedMasksKey, out stickerSet))
            {
                LoadStickerSet(RecentlyUsedMasksKey, stickerSet);
            }
            else
            {
                VirtPanel.ClearItems();
                _recentStickersSprites = new List<VListItemBase>();
            }
        }
        private void SetButtonsBackground(object sender)
        {
            if (_previousButton != null)
            {
                _previousButton.Tag = null;
                _previousButton.Background = ButtonBackground;
            }

            _previousButton = sender as Button;
            if (_previousButton != null)
            {
                _previousButton.Tag = true;
                _previousButton.Background = (Brush) Application.Current.Resources["PhoneAccentBrush"];
            }
        }

        private void CreateSetsAndUpdatePanel()
        {
            _stickerSets.Clear();

            CreateSets(_masks);
            CreateSets(_stickers);

            UpdateButtons(_masks, MasksPanel, _recentMasksButton, _masksButtons);
            UpdateButtons(_stickers, StickersPanel, _recentStickersButton, _stickersButtons);
        }

        private readonly Dictionary<string, List<TLDocument22>> _stickerSets = new Dictionary<string, List<TLDocument22>>();

        private void CreateSets(TLAllStickers allStickers)
        {
            if (allStickers == null) return;

            var recentlyUsedSetId = allStickers == _masks? RecentlyUsedMasksKey : RecentlyUsedStickersKey;
            _stickerSets[recentlyUsedSetId] = new List<TLDocument22>();

            var documentsCache = new Dictionary<long, TLDocument22>();
            for (var i = 0; i < allStickers.Documents.Count; i++)
            {
                var document22 = allStickers.Documents[i] as TLDocument22;
                if (document22 != null)
                {
                    documentsCache[document22.Id.Value] = document22;

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
                            _stickerSets[setId] = new List<TLDocument22> { document22 };
                        }
                    }
                }
            }

            var allStickers43 = allStickers as TLAllStickers43;
            if (allStickers43 != null)
            {
                if (allStickers43.RecentlyUsed != null)
                {
                    var documentsDict = new Dictionary<long, TLDocumentBase>();
                    foreach (var document in allStickers43.Documents)
                    {
                        documentsDict[document.Id.Value] = document;
                    }

                    for (var i = 0; i < allStickers43.RecentlyUsed.Count; i++)
                    {
                        TLDocumentBase document;
                        var recentlyUsedDocument = allStickers43.RecentlyUsed[i];
                        if (documentsDict.TryGetValue(recentlyUsedDocument.Id.Value, out document))
                        {
                            var document22 = document as TLDocument22;
                            if (document22 != null)
                            {
                                documentsCache[document22.Id.Value] = document22;

                                List<TLDocument22> stickers;
                                if (_stickerSets.TryGetValue(recentlyUsedSetId, out stickers))
                                {
                                    stickers.Add(document22);
                                }
                                else
                                {
                                    _stickerSets[recentlyUsedSetId] = new List<TLDocument22> { document22 };
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateButtons(TLAllStickers allStickersBase, Panel panel, Button recentStickersButton, List<Button> allButtons)
        {
            panel.Children.Clear();

            if (allStickersBase == null)
            {
                return;
            }
            var allStickers43 = allStickersBase as TLAllStickers43;
            if (allStickers43 != null)
            {
                recentStickersButton.Visibility = allStickers43.RecentlyUsed == null || allStickers43.RecentlyUsed.Count == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }
            panel.Children.Add(recentStickersButton);

            var isLightTheme = (Visibility) Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            var buttonStyleResourceKey = isLightTheme ? "CategoryButtonLightThemeStyle" : "CategoryButtonDarkThemeStyle";
            var buttonStyle = (Style) Resources[buttonStyleResourceKey];

            allButtons.Clear();
            var allStickers = allStickersBase as TLAllStickers29;
            if (allStickers != null)
            {
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
                            Style = buttonStyle,
                            Content = image,
                            DataContext = key
                        };
                        button.Click += StickerSetButtonOnClick;
                        button.Tap += StickerSetButtonOnTap;
                        panel.Children.Add(button);
                        
                        allButtons.Add(button);
                    }
                }
            }
        }

        private void StickerSetButtonOnTap(object sender, GestureEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Tap");
        }

        private void StickerSetButtonOnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Click");

            SetButtonsBackground(sender);

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

        private bool _reloadRecentStickers;

        private bool _reloadRecentMasks;

        public List<VListItemBase> CurrentSprites;

        private readonly Dictionary<string, List<VListItemBase>> _stickerSetSprites = new Dictionary<string, List<VListItemBase>>();

        private List<VListItemBase> _recentStickersSprites;

        private List<VListItemBase> _recentMasksSprites;

        private const int FirstStickerSliceCount = 6;

        private void LoadStickerSet(string key, List<TLDocument22> stickerSet)
        {
            var stickerPerRow = 4;

            var setId = key;
            List<VListItemBase> sprites;
            if (!_stickerSetSprites.TryGetValue(setId, out sprites)
                || (key == RecentlyUsedMasksKey && _reloadRecentMasks)
                || (key == RecentlyUsedStickersKey && _reloadRecentStickers))
            {
                if (key == RecentlyUsedStickersKey && _reloadRecentStickers) _reloadRecentStickers = false;
                if (key == RecentlyUsedMasksKey && _reloadRecentMasks) _reloadRecentMasks = false;

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

            if (key == RecentlyUsedMasksKey)
            {
                _recentMasksSprites = sprites;
            }
            else if (key == RecentlyUsedStickersKey)
            {
                _recentStickersSprites = sprites;
            }

            CurrentSprites = sprites;
            AddSprites(sprites);
        }

        public void UpdateRecentAsync(List<TLDocument22> stickers, List<TLDocument22> masks)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var stateService = IoC.Get<IStateService>();

                var cachedMasks = _masks as TLAllStickers43;
                if (cachedMasks != null
                    && masks != null
                    && masks.Count > 0)
                {
                    cachedMasks.RecentlyUsed = cachedMasks.RecentlyUsed ?? new TLVector<TLRecentlyUsedSticker>();

                    foreach (var mask in masks)
                    {
                        var updated = false;
                        for (var i = 0; i < cachedMasks.RecentlyUsed.Count; i++)
                        {
                            var recentlyUsedSticker = cachedMasks.RecentlyUsed[i];

                            if (recentlyUsedSticker.Id.Value == mask.Id.Value)
                            {
                                recentlyUsedSticker.Count.Value++;
                                cachedMasks.RecentlyUsed.RemoveAt(i);
                                cachedMasks.RecentlyUsed.Insert(0, recentlyUsedSticker);
                                updated = true;
                                break;
                            }
                        }

                        if (!updated)
                        {
                            cachedMasks.RecentlyUsed.Insert(0, new TLRecentlyUsedSticker { Id = mask.Id, Count = new TLLong(1) });
                        }
                    }

                    List<TLDocument22> set;
                    if (_stickerSets.TryGetValue(RecentlyUsedMasksKey, out set))
                    {
                        for (var i = 0; i < masks.Count; i++)
                        {
                            for (var j = 0; j < set.Count; j++)
                            {
                                if (set[j].Id.Value == masks[i].Id.Value)
                                {
                                    set.RemoveAt(j);
                                    break;
                                }
                            }
                            set.Insert(0, masks[i]);
                        }
                    }
                    else
                    {
                        _stickerSets[RecentlyUsedMasksKey] = masks;
                    }

                    stateService.SaveMasksAsync(cachedMasks);

                    _reloadRecentMasks = true;
                }

                var cachedStickers = _stickers as TLAllStickers43;
                if (cachedStickers != null
                    && stickers != null
                    && stickers.Count > 0)
                {
                    cachedStickers.RecentlyUsed = cachedStickers.RecentlyUsed ?? new TLVector<TLRecentlyUsedSticker>();

                    foreach (var sticker in stickers)
                    {
                        var updated = false;
                        for (var i = 0; i < cachedStickers.RecentlyUsed.Count; i++)
                        {
                            var recentlyUsedSticker = cachedStickers.RecentlyUsed[i];

                            if (recentlyUsedSticker.Id.Value == sticker.Id.Value)
                            {
                                recentlyUsedSticker.Count.Value++;
                                cachedStickers.RecentlyUsed.RemoveAt(i);
                                cachedStickers.RecentlyUsed.Insert(0, recentlyUsedSticker);
                                updated = true;
                                break;
                            }
                        }

                        if (!updated)
                        {
                            cachedStickers.RecentlyUsed.Insert(0, new TLRecentlyUsedSticker { Id = sticker.Id, Count = new TLLong(1) });
                        }
                    }

                    List<TLDocument22> set;
                    if (_stickerSets.TryGetValue(RecentlyUsedStickersKey, out set))
                    {
                        for (var i = 0; i < stickers.Count; i++)
                        {
                            for (var j = 0; j < set.Count; j++)
                            {
                                if (set[j].Id.Value == stickers[i].Id.Value)
                                {
                                    set.RemoveAt(j);
                                    break;
                                }
                            }
                            set.Insert(0, stickers[i]);
                        }
                    }
                    else
                    {
                        _stickerSets[RecentlyUsedStickersKey] = stickers;
                    }

                    stateService.SaveAllStickersAsync(cachedStickers);

                    _reloadRecentStickers = true;
                }
            });
        }

        private void AddSprites(List<VListItemBase> sprites)
        {
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

            VirtPanel.ClearItems();
            VirtPanel.AddItems(firstSlice);
            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.1), () => { VirtPanel.AddItems(secondSlice); });
        }

        private void OnStickerSelected(object sender, StickerSelectedEventArgs e)
        {
            RaiseStickerSelected(e);
        }

        private bool _once = true;

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (!_once) return;

            _once = false;
            Deployment.Current.Dispatcher.BeginInvoke(Open);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void Close()
        {
            var storyboard = new Storyboard();

            var translateAnimation = new DoubleAnimationUsingKeyFrames();
            translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 0.0 });
            translateAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 150.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 6.0 } });
            Storyboard.SetTarget(translateAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateAnimation);

            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.00), Value = 1.0 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 1.0 });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
            Storyboard.SetTarget(opacityAnimation, LayoutRoot);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(opacityAnimation);

            storyboard.Begin();
            storyboard.Completed += (sender, args) =>
            {
                Visibility = Visibility.Collapsed;
            };
        }

        public void Open()
        {
            var storyboard = new Storyboard();

            var continuumLayoutRootY = new DoubleAnimationUsingKeyFrames();
            continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 150.0 });
            continuumLayoutRootY.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 3.0 } });
            Storyboard.SetTarget(continuumLayoutRootY, LayoutRoot);
            Storyboard.SetTargetProperty(continuumLayoutRootY, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(continuumLayoutRootY);

            var continuumLayoutRootOpacity = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.25),
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 6.0 }
            };
            Storyboard.SetTarget(continuumLayoutRootOpacity, LayoutRoot);
            Storyboard.SetTargetProperty(continuumLayoutRootOpacity, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(continuumLayoutRootOpacity);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                Visibility = Visibility.Visible;
                LayoutRoot.Opacity = 1.0;
                storyboard.Completed += (o, e) =>
                {
                    LoadSprites(_masks, _masksButtons);
                };
                storyboard.Begin();
            });
        }

        private void LoadSprites(TLAllStickers allStickers, List<Button> buttons)
        {
            if (VirtPanel.Children.Count > 0) return;

            var selectRecentlyUsedStickers = false;
            var selectFirstStickerSet = false;
            var allStickers43 = allStickers as TLAllStickers43;
            if (allStickers43 != null && (allStickers43.RecentlyUsed == null || allStickers43.RecentlyUsed.Count == 0))
            {
                if (buttons.Count > 0)
                {
                    selectFirstStickerSet = true;
                }
            }

            if (!selectFirstStickerSet)
            {
                selectRecentlyUsedStickers = true;
            }

            if (selectFirstStickerSet)
            {
                Execute.BeginOnUIThread(() =>
                {
                    StickerSetButtonOnClick(buttons[0], null);
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                    
                    return;
                });
            }

            if (selectRecentlyUsedStickers)
            {
                Execute.BeginOnUIThread(() =>
                {
                    if (allStickers == _masks)
                    {
                        RecentMasksButtonOnClick(_recentMasksButton, null);
                    }
                    else if (allStickers == _stickers)
                    {
                        RecentStickersButtonOnClick(_recentStickersButton, null);
                    }
                    LoadingProgressBar.Visibility = Visibility.Collapsed;

                    return;
                });
            }
        }

        private List<VListItemBase> _previousSprites;

        private Button _previousCategoryButton;

        private void Masks_OnClick(object sender, RoutedEventArgs e)
        {
            if (MasksScrollViewer.Visibility == Visibility.Visible) return;

            Execute.BeginOnUIThread(() =>
            {
                var currentSprites = CurrentSprites;
                var currentButton = StickersPanel.Children.OfType<Button>().FirstOrDefault(x => x.Tag is bool);
                if (_previousSprites != null)
                {
                    CurrentSprites = _previousSprites;
                    AddSprites(_previousSprites);
                    SetButtonsBackground(_previousCategoryButton);
                }
                else
                {
                    VirtPanel.Children.Clear();
                    LoadSprites(_masks, _masksButtons);
                }

                MasksScrollViewer.Visibility = Visibility.Visible;
                StickersScrollViewer.Visibility = Visibility.Collapsed;

                _previousSprites = currentSprites;
                _previousCategoryButton = currentButton;
            });
        }

        private void Stickers_OnClick(object sender, RoutedEventArgs e)
        {
            if (StickersScrollViewer.Visibility == Visibility.Visible) return;

            Execute.BeginOnUIThread(() =>
            {
                var currentSprites = CurrentSprites;
                var currentButton = MasksPanel.Children.OfType<Button>().FirstOrDefault(x => x.Tag is bool);
                if (_previousSprites != null)
                {
                    CurrentSprites = _previousSprites;
                    AddSprites(_previousSprites);
                    SetButtonsBackground(_previousCategoryButton);
                }
                else
                {
                    VirtPanel.Children.Clear();
                    LoadSprites(_stickers, _stickersButtons);
                }

                MasksScrollViewer.Visibility = Visibility.Collapsed;
                StickersScrollViewer.Visibility = Visibility.Visible;

                _previousSprites = currentSprites;
                _previousCategoryButton = currentButton;
            });
        }

        public void UpdateRecentButtonsVisibility()
        {
            var masks43 = _masks as TLAllStickers43;
            _recentMasksButton.Visibility = masks43 == null || masks43.RecentlyUsed == null || masks43.RecentlyUsed.Count == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            var stickers43 = _stickers as TLAllStickers43;
            _recentStickersButton.Visibility = stickers43 == null || stickers43.RecentlyUsed == null || stickers43.RecentlyUsed.Count == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }

        private static DateTime? _startTime;
        private static FrameworkElement _fromItem;
        private static Storyboard _storyboard;
        private static FrameworkElement _lastMouseEnter;
        private static Storyboard _loadedStoryboard;
        private static ManipulationStartedEventArgs _manipulationStartedArgs;

        private void StickerPanel_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _startTime = DateTime.Now;
            var startTime = _startTime;

            _fromItem = e.OriginalSource as FrameworkElement;
            _lastMouseEnter = null;
            _manipulationStartedArgs = e;

            Touch.FrameReported += Touch_FrameReported;
        }

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

                _loadedStoryboard = EmojiControl.GetScaleStoryboard(_fromItem, 0.85, 1.0);

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
                });
            }
        }

        private void StickerPanel_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            _startTime = null;
        }

        private void StickerPanel_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            var fromItem = _fromItem;

            _startTime = null;
            _fromItem = null;
            
            if (_storyboard != null)
            {
                _storyboard.SkipToFill();
            }

            VirtPanel.EnableVerticalScrolling();
            Preview.Visibility = Visibility.Collapsed;

            var st = EmojiControl.GetScaleStoryboard(_lastMouseEnter ?? fromItem, 1.0, 1.0);
            if (st != null)
            {
                Execute.BeginOnUIThread(st.Begin);
            }
        }

        private void StickerPanel_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Preview.Visibility == Visibility.Collapsed) return;

            //DebugText.Text = sender.GetHashCode().ToString();

            var st1 = EmojiControl.GetScaleStoryboard(_lastMouseEnter ?? _fromItem, 1.0, 1.0);

            _lastMouseEnter = e.OriginalSource as FrameworkElement;

            var stickerImage = e.OriginalSource as Image;
            if (stickerImage != null)
            {
                PreviewImage.Source = stickerImage.Source;

                var stickerItem = stickerImage.DataContext as TLStickerItem;
                if (stickerItem != null)
                {
                    Image.DataContext = stickerItem;
                }
                PreviewGrid.DataContext = Image.DataContext;
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

            var st2 = EmojiControl.GetScaleStoryboard(_lastMouseEnter, 0.85, 1.0);
            if (st1 != null || st2 != null)
            {
                Execute.BeginOnUIThread(() =>
                {
                    if (st1 != null) st1.Begin();
                    if (st2 != null) st2.Begin();
                });
            }
        }

        private void StickerPreviewGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            var fromItem = _fromItem;
            if (fromItem == null) return;

            var position = fromItem.TransformToVisual(Application.Current.RootVisual).Transform(new Point(fromItem.ActualWidth / 2.0, fromItem.ActualHeight / 2.0));

            var position2 = new Point(240.0, 400.0);

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

        public void ClosePreview()
        {
            Touch.FrameReported -= Touch_FrameReported;

            StickerPanel_ManipulationCompleted(null, null);
        }
    }
}

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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Windows.UI.ViewManagement;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Extensions;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using Telegram.Controls;
using Telegram.Controls.Utils;
using TelegramClient.Services;
using TelegramClient.ViewModels.Dialogs;
using Action = System.Action;
using Execute = Telegram.Api.Helpers.Execute;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Controls
{
    public partial class ShareMessagePicker
    {
        public static readonly DependencyProperty CommentProperty = DependencyProperty.Register(
            "Comment", typeof(string), typeof(ShareMessagePicker), new PropertyMetadata(default(string)));

        public string Comment
        {
            get { return (string) GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(ShareMessagePicker), new PropertyMetadata(default(string), OnTextChanged));

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = d as ShareMessagePicker;
            if (picker != null)
            {
                picker.Search();
            }
        }

        public string Text
        {
            get { return (string) GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private readonly Dictionary<int, DialogItem> _selectedPictures = new Dictionary<int, DialogItem>();

        public TLDialogBase CurrentDialog { get; set; }

        public List<DialogItem> DialogsSource { get; protected set; }

        public ShareMessagePicker()
        {
            InitializeComponent();

            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                ((SolidColorBrush)Resources["BackgroundBrush"]).Color = ((SolidColorBrush)Resources["LightThemeBackgroundBrush"]).Color;
            }
            else
            {
                ((SolidColorBrush)Resources["BackgroundBrush"]).Color = ((SolidColorBrush)Resources["DarkThemeBackgroundBrush"]).Color;
            }

            Rows = new BindableCollection<DialogRow>();
            DialogsSource = new List<DialogItem>();

            InitilalizeDialogs();

            Bar.Visibility = Visibility.Collapsed;

            Loaded += OnLoadedOnce;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            InputPane.GetForCurrentView().Showing += InputPane_Showing;
            InputPane.GetForCurrentView().Hiding += InputPane_Hiding;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            KeyboardPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            var keyboardHeight = 480.0 / args.OccludedRect.Width * args.OccludedRect.Height - AppBar.ActualHeight + 18.0;

            KeyboardPlaceholder.Visibility = Visibility.Visible;
            KeyboardPlaceholder.Height = keyboardHeight;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            InputPane.GetForCurrentView().Showing -= InputPane_Showing;
            InputPane.GetForCurrentView().Hiding -= InputPane_Hiding;
        }

        //~ShareMessagePicker()
        //{
            
        //}

        private void InitilalizeDialogs()
        {
            Execute.BeginOnThreadPool(() =>
            {
                LoadDialogs();
            });
        }

        private static bool Skip(TLDialogBase d)
        {
            if (d is TLEncryptedDialog)
            {
                return true;
            }

            var user = d.With as TLUser;
            if (user != null && user.IsSelf)
            {
                return true;
            }

            if (user != null && user.IsDeleted)
            {
                return true;
            }

            var channel = d.With as TLChannel;
            if (channel != null && channel.Left.Value)
            {
                return true;
            }

            var chat = d.With as TLChat41;
            if (chat != null && chat.IsMigrated)
            {
                return true;
            }

            if (channel != null && !channel.IsMegaGroup && !(channel.IsEditor || channel.Creator))
            {
                return true;
            }

            var chatForbidden = d.With as TLChatForbidden;
            if (chatForbidden != null)
            {
                return true;
            }

            return false;
        }

        public BindableCollection<DialogRow> Rows { get; set; }

        private IList<TLDialogBase> _dialogs;

        private void LoadDialogs()
        {
            TLDialogBase currentUserDialog = null;
            _dialogs = IoC.Get<ICacheService>().GetDialogs();

            var clearedDialogs = new List<TLDialogBase>();
            foreach (var d in _dialogs)
            {
                if (Skip(d))
                {
                    var user = d.With as TLUser;
                    if (user != null && user.IsSelf)
                    {
                        currentUserDialog = d;
                    }

                    continue;
                }

                clearedDialogs.Add(d);
            }

            if (currentUserDialog != null)
            {
                clearedDialogs.Insert(0, currentUserDialog);
            }
            else
            {
                var currentUser = IoC.Get<ICacheService>().GetUser(new TLInt(IoC.Get<IStateService>().CurrentUserId));

                currentUserDialog = new TLDialog{ Peer = new TLPeerUser{ Id = currentUser.Id }, With = currentUser };
                clearedDialogs.Insert(0, currentUserDialog);
            }

            _dialogsSource = clearedDialogs;

            DialogRow currentRow = null;
            var rows = new Group<DialogRow>("group");
            Rows = rows;
            var groups = new ObservableCollection<Group<DialogRow>>{ rows };
            var secondSlice = new List<DialogRow>();
            if (clearedDialogs.Count > 0)
            {
                var maxFirstSliceCount = 12;
                var maxSecondSliceCount = 24;
                for (var i = 0; i < clearedDialogs.Count; i++)
                {
                    if (i % 4 == 0)
                    {
                        currentRow = new DialogRow();
                        if (i < maxFirstSliceCount)
                        {
                            rows.Add(currentRow);
                        }
                        else if (i < maxSecondSliceCount)
                        {
                            secondSlice.Add(currentRow);
                        }
                    }

                    var d = new DialogItem { Dialog = clearedDialogs[i], Row = currentRow };

                    DialogsSource.Add(d);

                    currentRow.Add(d);
                }
            }

            var lastDialog = _dialogs.LastOrDefault(x => x is TLDialog) as TLDialog;
            UpdateDialogsAsync(lastDialog);

            Execute.BeginOnUIThread(() =>
            {
                SetListVerticalAlignment(Rows.Count);
                Dialogs.ItemsSource = groups;
                Dialogs.Visibility = Visibility.Visible;
                Dialogs.Opacity = 0.0;

                Execute.BeginOnUIThread(() =>
                {
                    Dialogs.Opacity = 1.0;
                    var storyboard = new Storyboard();
                    var translateAnimaiton = new DoubleAnimationUsingKeyFrames();
                    translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = Dialogs.ActualHeight });
                    translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0.0, EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut } });
                    Storyboard.SetTarget(translateAnimaiton, Dialogs);
                    Storyboard.SetTargetProperty(translateAnimaiton, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                    storyboard.Children.Add(translateAnimaiton);

                    storyboard.Begin();

                    if (secondSlice.Count > 0)
                    {
                        storyboard.Completed += (o, e) =>
                        {
                            foreach (var item in secondSlice)
                            {
                                Rows.Add(item);
                            }
                            _secondSliceLoaded = true;

                            LoadNextSlice();
                        };
                    }
                    else
                    {
                        _secondSliceLoaded = true;

                        LoadNextSlice();
                    }
                });
            });
        }

        private bool _secondSliceLoaded;

        private void LoadLastSlice()
        {
            if (_lastSlice != null && _lastSlice.Count > 0)
            {
                var lastSlice = _lastSlice;
                _lastSlice = null;

                LoadNextRows(lastSlice);
            }
        }

        private void LoadNextRows(List<DialogRow> rows)
        {
            for (var i = 0; i < 5; i++)
            {
                var row = rows.FirstOrDefault();
                if (row != null)
                {
                    rows.RemoveAt(0);

                    Rows.Add(row);
                }
                else
                {
                    return;
                }
            }

            if (rows.Count > 0)
            {
                Execute.BeginOnUIThread(() =>
                {
                    LoadNextRows(rows);
                });
            }
        }

        public void LoadNextSlice()
        {
            if (!string.IsNullOrEmpty(Text)) return;
            if (!_secondSliceLoaded) return;

            var maxSliceCount = int.MaxValue;
            var index = 0;

            var lastRow = Rows.LastOrDefault();
            if (lastRow != null)
            {
                var lastDialogItem = lastRow.GetLast();
                if (lastDialogItem != null)
                {
                    index = DialogsSource.IndexOf(lastDialogItem);
                }
            }

            var currentRow = lastRow;
            if (currentRow != null && currentRow.HasEmptyItem())
            {
                for (index = index + 1; index < DialogsSource.Count; index++)
                {
                    if (!currentRow.HasEmptyItem()) break;

                    var d = DialogsSource[index];

                    d.Row = currentRow;

                    currentRow.Add(d, true);
                }
            }
            else
            {
                index++;
            }

            _lastSlice = new List<DialogRow>();
            for (var count = 0; index < DialogsSource.Count && count < maxSliceCount; index++, count++)
            {
                if (currentRow == null || !currentRow.HasEmptyItem())
                {
                    currentRow = new DialogRow();
                    _lastSlice.Add(currentRow);
                }

                var d = DialogsSource[index];
                d.Row = currentRow;

                currentRow.Add(d);
            }

            if (_lastSlice.Count > 0
                && Rows.Count < 10)
            {
                for (var i = 0; i < 10 - Rows.Count; i++)
                {
                    var row = _lastSlice.FirstOrDefault();
                    if (row != null)
                    {
                        _lastSlice.RemoveAt(0);
                        Rows.Add(row);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        private bool _lastSliceLoaded;

        private void UpdateDialogsAsync(TLDialog lastDialog)
        {
            if (_lastSliceLoaded) return;

            var offsetDate = 0;
            var offsetId = 0;
            TLInputPeerBase offsetPeer = new TLInputPeerEmpty();
            if (lastDialog != null)
            {
                var lastMessage = lastDialog.TopMessage as TLMessageCommon;
                if (lastMessage != null)
                {
                    offsetDate = lastMessage.DateIndex;
                    offsetId = lastMessage.Index;
                    if (lastMessage.ToId is TLPeerUser)
                    {
                        offsetPeer = !lastMessage.Out.Value
                            ? DialogDetailsViewModel.PeerToInputPeer(new TLPeerUser { Id = lastMessage.FromId })
                            : DialogDetailsViewModel.PeerToInputPeer(lastMessage.ToId);
                    }
                    else
                    {
                        offsetPeer = DialogDetailsViewModel.PeerToInputPeer(lastMessage.ToId);
                    }
                }
            }

            var stopwatch = Stopwatch.StartNew();
            IoC.Get<IMTProtoService>().GetDialogsAsync(stopwatch,
                new TLInt(offsetDate), 
                new TLInt(offsetId), 
                offsetPeer, 
                new TLInt(int.MaxValue),
                new TLInt(0),
                result => Execute.BeginOnUIThread(() =>
                {
                    _lastSliceLoaded = true;

                    var dialogs = result.Dialogs;
                    var clearedDialogs = new List<TLDialogBase>();
                    foreach (var d in dialogs)
                    {
                        if (Skip(d))
                        {
                            continue;
                        }

                        clearedDialogs.Add(d);
                    }

                    foreach (var clearedDialog in clearedDialogs)
                    {
                        var d = new DialogItem { Dialog = clearedDialog };

                        DialogsSource.Add(d);
                    }

                    LoadNextSlice();
                }),
                error => Execute.BeginOnUIThread(() =>
                {

                }));
        }

        private void SetListVerticalAlignment(int rowsCount)
        {
            Dialogs.VerticalAlignment = rowsCount < 3 ? VerticalAlignment.Bottom : VerticalAlignment.Stretch;
        }

        private TelegramAppBarButton _selectButton;

        private void OnLoadedOnce(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoadedOnce;

            _selectButton = AppBar.Buttons[0] as TelegramAppBarButton;

            BeginOpenStoryboard();
        }

        private void BeginOpenCommentGridStoryboard()
        {
            if (CommentGrid.Visibility == Visibility.Visible) return;

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            var storyboard = new Storyboard();
            var translateAnimaiton = new DoubleAnimationUsingKeyFrames();
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0.0, EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut } });
            Storyboard.SetTarget(translateAnimaiton, CommentGrid);
            Storyboard.SetTargetProperty(translateAnimaiton, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateAnimaiton);

            Execute.BeginOnUIThread(() =>
            {
                CommentGrid.Visibility = Visibility.Visible;
                storyboard.Begin();
            });
        }

        private void BeginCloseCommentGridStoryboard()
        {
            if (CommentGrid.Visibility == Visibility.Collapsed) return;

            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            var translateCommentGridAniamtion = new DoubleAnimationUsingKeyFrames();
            translateCommentGridAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0 });
            translateCommentGridAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateCommentGridAniamtion, CommentGrid);
            Storyboard.SetTargetProperty(translateCommentGridAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateCommentGridAniamtion);

            storyboard.Begin();
            storyboard.Completed += (sender, args) =>
            {
                CommentGrid.Visibility = Visibility.Collapsed;
            };
        }

        private void BeginOpenStoryboard()
        {
            Bar.Visibility = Visibility.Collapsed;

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            var storyboard = new Storyboard();
            var translateAnimaiton = new DoubleAnimationUsingKeyFrames();
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateAnimaiton.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0.0, EasingFunction = new ExponentialEase { Exponent = 5.0, EasingMode = EasingMode.EaseOut } });
            Storyboard.SetTarget(translateAnimaiton, Bar);
            Storyboard.SetTargetProperty(translateAnimaiton, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateAnimaiton);

            Execute.BeginOnUIThread(() =>
            {
                Bar.Visibility = Visibility.Visible;
                storyboard.Begin();
            });
        }

        private void BeginCloseStoryboard(Action callback)
        {
            //LayoutRoot.Background = (Brush)Resources["BackgroundTransparentBrush"];

            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, Dialogs);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateImageAniamtion);

            var translateBarAniamtion = new DoubleAnimationUsingKeyFrames();
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0 });
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateBarAniamtion, Bar);
            Storyboard.SetTargetProperty(translateBarAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateBarAniamtion);

            var translateCommentGridAniamtion = new DoubleAnimationUsingKeyFrames();
            translateCommentGridAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0 });
            translateCommentGridAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateCommentGridAniamtion, CommentGrid);
            Storyboard.SetTargetProperty(translateCommentGridAniamtion, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyboard.Children.Add(translateCommentGridAniamtion);

            storyboard.Begin();
            if (callback != null)
            {
                storyboard.Completed += (o, e) => callback();
            }
        }

        public bool TryClose()
        {
            BeginCloseStoryboard(RaiseClose);

            return true;
        }

        public bool ForceClose()
        {
            RaiseClose();

            return true;
        }

        public event EventHandler Close;

        protected virtual void RaiseClose()
        {
            var handler = Close;
            if (handler != null) handler(this, System.EventArgs.Empty);
        }

        public event EventHandler<PickDialogEventArgs> Pick;

        protected virtual void RaisePick(PickDialogEventArgs e)
        {
            var handler = Pick;
            if (handler != null) handler(this, e);
        }

        private void DialogControl_OnTap(object sender, GestureEventArgs e)
        {
            var control = sender as DialogControl;
            if (control != null)
            {
                var dialogItem = control.DataContext as DialogItem;
                if (dialogItem != null)
                {
                    ChangeDialogSelection(dialogItem, !dialogItem.IsSelected);
                }
            }
        }

        private DialogItem _previousDialog;

        private void ChangeDialogSelection(DialogItem dialogItem, bool isSelected)
        {
            if (isSelected)
            {
                _selectedPictures[dialogItem.Dialog.Index] = dialogItem;
            }
            else
            {
                _selectedPictures.Remove(dialogItem.Dialog.Index);
            }

            dialogItem.SuppressAnimation = false;
            dialogItem.IsSelected = isSelected;
            dialogItem.RaisePropertyChanged("IsSelected");

            if (_hasManipulatingDelta
                && _previousDialog != null
                && _previousDialog.Row == dialogItem.Row)
            {
                var row = dialogItem.Row;
                var between = row.Between(_previousDialog, dialogItem).ToList();
                if (between.Count > 0)
                {
                    if (isSelected)
                    {
                        foreach (var item in between)
                        {
                            _selectedPictures[item.Dialog.Index] = item;
                        }
                    }
                    else
                    {
                        foreach (var item in between)
                        {
                            _selectedPictures.Remove(item.Dialog.Index);
                        }
                    }

                    foreach (var item in between)
                    {
                        item.SuppressAnimation = false;
                        item.IsSelected = isSelected;
                        item.RaisePropertyChanged("IsSelected");
                    }
                }
            }

            _previousDialog = dialogItem;

            _selectButton.IsEnabled = _selectedPictures.Any();

            if (_selectedPictures.Any())
            {
                BeginOpenCommentGridStoryboard();
            }
            else
            {
                BeginCloseCommentGridStoryboard();
            }
            //if (IsSingleItem)
            //{
            //    ChooseButton_OnClick(null, null);
            //}
        }

        private bool _hasManipulatingDelta;

        private void Photos_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _hasManipulatingDelta = false;
        }

        private void Photos_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _hasManipulatingDelta = false;
        }

        private bool _isSelected;

        private void Dialogs_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            //Debug.Text = string.Format("{0}\n{1}\n{2}", Dialogs.Viewport.Viewport, Dialogs.Viewport.Bounds, e.CumulativeManipulation.Translation);

            if (!_hasManipulatingDelta)
            {
                var grid = e.OriginalSource as Grid;

                if (grid != null)
                {
                    var control = grid.Parent as DialogControl;
                    if (control != null)
                    {
                        var file1 = control.DataContext as DialogItem;
                        if (file1 != null)
                        {
                            ChangeDialogSelection(file1, !file1.IsSelected);
                            _isSelected = file1.IsSelected;
                        }
                    }
                }
            }

            _hasManipulatingDelta = true;
        }

        private void DialogControl_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!_hasManipulatingDelta) return;

            var control = sender as DialogControl;
            if (control != null)
            {
                var file1 = control.DataContext as DialogItem;
                if (file1 != null)
                {
                    ChangeDialogSelection(file1, _isSelected);
                }
            }
        }

        private void ChooseButton_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedDialogs = _selectedPictures.Values.Select(x => x.Dialog).ToList();
            if (selectedDialogs.Count == 0) return;

            RaisePick(new PickDialogEventArgs { Comment = Comment, Dialogs = new ReadOnlyCollection<TLDialogBase>(selectedDialogs) });

            Execute.BeginOnUIThread(() =>
            {
                TryClose();
            });
        }

        public static readonly DependencyProperty LinkProperty = DependencyProperty.Register(
            "Link", typeof(string), typeof(ShareMessagePicker), new PropertyMetadata(default(string), OnLinkChanged));

        private static void OnLinkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ShareMessagePicker;
            if (control != null)
            {
                control.CopyLinkMenuItem.Visibility = !string.IsNullOrEmpty(e.NewValue as string)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public string Link
        {
            get { return (string) GetValue(LinkProperty); }
            set { SetValue(LinkProperty, value); }
        }

        private void CopyLink_OnTap(object sender, GestureEventArgs e)
        {
            AppBar.Close();
            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                if (string.IsNullOrEmpty(Link)) return;

                Clipboard.SetText(Link);
            });
        }

        private void Dialogs_OnViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            if (Dialogs.VerticalAlignment == VerticalAlignment.Stretch
                && Dialogs.Viewport.Viewport.Height + Dialogs.Viewport.Viewport.Y >= Dialogs.Viewport.Bounds.Height + Dialogs.Viewport.Bounds.Y - 0.1   // scrolled to top
                && Dialogs.Viewport.Viewport.Y - Dialogs.Viewport.Bounds.Y >= 300.0 - 0.1)      // header not in view
            {
                Dialogs.Background = (Brush)Resources["BackgroundBrush"];
                //LayoutRoot.Background = (Brush) Resources["BackgroundBrush"];
            }
            else
            {
                Dialogs.Background = null;
                //LayoutRoot.Background = (Brush) Resources["TransparentBackgroundBrush"];
            }

            var watermarkTextBox = FocusManager.GetFocusedElement() as WatermarkedTextBox;
            if (watermarkTextBox != null && string.Equals(watermarkTextBox.Tag, "Search"))
            {
                Dialogs.Background = (Brush)Resources["BackgroundBrush"];
                //LayoutRoot.Background = (Brush)Resources["BackgroundBrush"];
            }

            if (Dialogs.VerticalAlignment == VerticalAlignment.Stretch
                && Dialogs.Viewport.Viewport.Height + Dialogs.Viewport.Viewport.Y >= Dialogs.Viewport.Bounds.Height + Dialogs.Viewport.Bounds.Y - 400.0)
            {
                LoadLastSlice();
            }

            Debug.Text = Dialogs.Viewport.Viewport + Environment.NewLine + Dialogs.Viewport.Bounds;
        }

        private VerticalAlignment _previousAlignment;

        private List<DialogRow> _previousRows;

        private List<TLDialogBase> _dialogsSource;

        private SearchDialogRequest _lastRequest;

        private readonly LRUCache<string, SearchDialogRequest> _searchResultsCache = new LRUCache<string, SearchDialogRequest>(Constants.MaxCacheCapacity);

        private List<DialogRow> _lastSlice;

        private void Search()
        {
            _previousRows = _previousRows ?? new List<DialogRow>(Rows);

            var text = Text;

            var trimmedText = Text.Trim();
            if (string.IsNullOrEmpty(trimmedText))
            {
                Execute.BeginOnUIThread(() =>
                {
                    if (!string.Equals(trimmedText, Text.Trim(), StringComparison.OrdinalIgnoreCase)) return;

                    Rows.Clear();
                    var firstSliceCount = 5;
                    var count = 0;
                    var secondSlice = new List<DialogRow>();
                    foreach (var item in _previousRows)
                    {
                        if (count < firstSliceCount)
                        {
                            Rows.Add(item);
                            count++;
                        }
                        else
                        {
                            secondSlice.Add(item);
                        }
                    }
                    if (secondSlice.Count > 0)
                    {
                        Execute.BeginOnUIThread(() =>
                        {
                            if (!string.Equals(trimmedText, Text.Trim(), StringComparison.OrdinalIgnoreCase)) return;

                            foreach (var item in secondSlice)
                            {
                                Rows.Add(item);
                            }
                        });
                    }
                });

                return;
            }

            //Search(Text);
            //return;

            Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.25), () =>
            {
                if (!string.Equals(text, Text, StringComparison.OrdinalIgnoreCase)) return;

                Search(Text);
            });
        }

        private void Search(string text)
        {
            if (!string.Equals(text, Text, StringComparison.OrdinalIgnoreCase)) return;

            if (_lastRequest != null)
            {
                _lastRequest.Cancel();
            }

            var trimmedText = Text.Trim();
            if (string.IsNullOrEmpty(trimmedText))
            {
                Execute.BeginOnUIThread(() =>
                {
                    if (!string.Equals(trimmedText, Text.Trim(), StringComparison.OrdinalIgnoreCase)) return;

                    Rows.Clear();
                    var firstSliceCount = 5;
                    var count = 0;
                    var secondSlice = new List<DialogRow>();
                    foreach (var item in _previousRows)
                    {
                        if (count < firstSliceCount)
                        {
                            Rows.Add(item);
                            count++;
                        }
                        else
                        {
                            secondSlice.Add(item);
                        }
                    }
                    if (secondSlice.Count > 0)
                    {
                        Execute.BeginOnUIThread(() =>
                        {
                            if (!string.Equals(trimmedText, Text.Trim(), StringComparison.OrdinalIgnoreCase)) return;

                            foreach (var item in secondSlice)
                            {
                                Rows.Add(item);
                            }
                        });
                    }
                });

                return;
            }

            var nextRequest = GetNextRequest(text, DialogsSource);

            nextRequest.ProcessAsync(results =>
            {
                if (nextRequest.IsCanceled) return;
                if (!string.Equals(Text, nextRequest.Text, StringComparison.OrdinalIgnoreCase)) return;

                Rows.Clear();
                if (results.Count > 0)
                {
                    DialogRow currentRow = null;
                    var secondSlice = new List<DialogItem>();
                    var maxCount = 16;
                    for (var i = 0; i < results.Count; i++)
                    {
                        var d = results[i];

                        if (i < maxCount)
                        {
                            if (i % 4 == 0)
                            {
                                currentRow = new DialogRow();
                                Rows.Add(currentRow);
                            }

                            currentRow.Add(d);
                            d.Row = currentRow;
                        }
                        else
                        {
                            secondSlice.Add(d);
                        }
                    }

                    if (secondSlice.Count > 0)
                    {
                        Execute.BeginOnUIThread(() =>
                        {
                            if (nextRequest.IsCanceled) return;
                            if (!string.Equals(Text, nextRequest.Text, StringComparison.OrdinalIgnoreCase)) return;

                            for (var i = 0; i < secondSlice.Count; i++)
                            {
                                if (i % 4 == 0)
                                {
                                    currentRow = new DialogRow();
                                    Rows.Add(currentRow);
                                }

                                currentRow.Add(secondSlice[i]);
                                secondSlice[i].Row = currentRow;
                            }
                        });
                    }
                }
            });

            _searchResultsCache[nextRequest.Text] = nextRequest;
            _lastRequest = nextRequest;
        }

        private SearchDialogRequest GetNextRequest(string text, List<DialogItem> dialogsSource)
        {
            SearchDialogRequest nextRequest;
            if (!_searchResultsCache.TryGetValue(text, out nextRequest))
            {
                foreach (var dialogItem in dialogsSource)
                {
                    var dialog = dialogItem.Dialog;

                    var chat = dialog.With as TLChatBase;
                    var user = dialog.With as TLUserBase;
                    if (chat != null && chat.FullNameWords == null)
                    {
                        chat.FullNameWords = chat.FullName.Split(' ');
                    }
                    else if (user != null && user.FullNameWords == null)
                    {
                        user.FullNameWords = user.FullName.Split(' ');
                    }
                }

                nextRequest = new SearchDialogRequest(text, dialogsSource);
            }
            return nextRequest;
        }

        private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var header = Dialogs.ListHeader as Grid;
            if (header != null)
            {
                header.Visibility = Visibility.Collapsed;
            }

            Dialogs.VerticalAlignment = VerticalAlignment.Stretch;
            Dialogs.Background = (Brush)Resources["BackgroundBrush"];

            //LayoutRoot.Background = (Brush)Resources["BackgroundBrush"];
        }

        private void UIElement_OnLostFocus(object sender, RoutedEventArgs e)
        {
            var header = Dialogs.ListHeader as Grid;
            if (header != null)
            {
                header.Visibility = Visibility.Visible;
            }

            SetListVerticalAlignment(Rows.Count);
            Dialogs_OnViewportChanged(null, null);
        }

        private void Dialogs_OnCloseToEnd(object sender, System.EventArgs e)
        {
            //LoadNextSlice();
        }

        private void ListHeader_OnTap(object sender, GestureEventArgs e)
        {
            TryClose();
        }
    }
    
    public class Group<T> : BindableCollection<T>
    {
        public string GroupName { get; private set; }

        public Group(string groupName)
        {
            GroupName = groupName;
        }
    }

    public class DialogRow : PropertyChangedBase
    {
        public DialogItem Dialog1 { get; set; }
        public DialogItem Dialog2 { get; set; }
        public DialogItem Dialog3 { get; set; }
        public DialogItem Dialog4 { get; set; }

        public bool HasEmptyItem()
        {
            return Dialog4 == null;
        }

        public void Add(DialogItem dialog, bool notify = false)
        {
            if (Dialog1 == null)
            {
                Dialog1 = dialog;
                if (notify) NotifyOfPropertyChange(() => Dialog1);
            }
            else if (Dialog2 == null)
            {
                Dialog2 = dialog;
                if (notify) NotifyOfPropertyChange(() => Dialog2);
            }
            else if (Dialog3 == null)
            {
                Dialog3 = dialog;
                if (notify) NotifyOfPropertyChange(() => Dialog3);
            }
            else if (Dialog4 == null)
            {
                Dialog4 = dialog;
                if (notify) NotifyOfPropertyChange(() => Dialog4);
            }
        }

        public IEnumerable<DialogItem> Between(DialogItem item1, DialogItem item2)
        {
            if (item1 == Dialog1)
            {
                if (item2 == Dialog3)
                {
                    yield return Dialog2;
                }
                else if (item2 == Dialog4)
                {
                    yield return Dialog2;
                    yield return Dialog3;
                }
            }
            else if (item1 == Dialog2)
            {
                if (item2 == Dialog4)
                {
                    yield return Dialog3;
                }
            }
            else if (item1 == Dialog3)
            {
                if (item2 == Dialog1)
                {
                    yield return Dialog2;
                }
            }
            else if (item1 == Dialog4)
            {
                if (item2 == Dialog2)
                {
                    yield return Dialog3;
                }
                else if (item2 == Dialog1)
                {
                    yield return Dialog2;
                    yield return Dialog3;
                }
            }
        }

        public DialogItem GetLast()
        {
            if (Dialog4 != null) return Dialog4;
            if (Dialog3 != null) return Dialog3;
            if (Dialog2 != null) return Dialog2;

            return Dialog1;
        }
    }

    public class DialogItem : INotifyPropertyChanged
    {
        public bool IsSelected { get; set; }
        public TLDialogBase Dialog { get; set; }
        public DialogItem Self { get { return this; } }
        public DialogRow Row { get; set; }

        public bool SuppressAnimation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PickDialogEventArgs : System.EventArgs
    {
        public string Comment { get; set; }

        public IList<TLDialogBase> Dialogs { get; set; }
    }

    public class SearchDialogRequest
    {
        public bool IsCanceled;

        public string TransliterateText { get; private set; }

        public string Text { get; private set; }

        public IList<DialogItem> DialogsSource { get; private set; }

        public IList<DialogItem> Results { get; private set; }

        public int Offset { get; set; }

        public int Limit { get { return 20; } }

        public bool IsLastSliceLoaded { get; set; }

        public SearchDialogRequest(string text, IList<DialogItem> dialogsSource)
        {
            Text = text;
            TransliterateText = Language.Transliterate(text);
            DialogsSource = dialogsSource;
        }

        private static bool IsUserValid(TLUserBase contact, string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            return contact.FirstName.ToString().StartsWith(text, StringComparison.OrdinalIgnoreCase)
                || contact.LastName.ToString().StartsWith(text, StringComparison.OrdinalIgnoreCase)
                || contact.FullName.StartsWith(text, StringComparison.OrdinalIgnoreCase)
                || (contact.FullNameWords != null && contact.FullNameWords.Any(x => x.StartsWith(text, StringComparison.OrdinalIgnoreCase)));
        }

        private static bool IsChatValid(TLChatBase chat, string text, bool useFastSearch)
        {
            if (string.IsNullOrEmpty(text)) return false;

            if (!useFastSearch)
            {
                var fullName = chat.FullName;

                var i = fullName.IndexOf(text, StringComparison.OrdinalIgnoreCase);
                if (i != -1)
                {
                    while (i < fullName.Length && i != -1)
                    {
                        if (i == 0 || (i > 0 && fullName[i - 1] == ' '))
                        {
                            return true;
                        }
                        if (fullName.Length > i + 1)
                        {
                            i = fullName.IndexOf(text, i + 1, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                if (chat.FullNameWords != null
                    && chat.FullNameWords.Any(x => x.StartsWith(text, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsUsernameValid(IUserName userNameContact, string text)
        {
            if (text.Length >= Constants.UsernameMinLength)
            {
                if (userNameContact != null)
                {
                    var userName = userNameContact.UserName != null ? userNameContact.UserName.ToString() : string.Empty;
                    if (userName.StartsWith(text.TrimStart('@'), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void ProcessAsync(Action<IList<DialogItem>> callback)
        {
            if (Results != null)
            {
                IsCanceled = false;
                callback.SafeInvoke(Results);
                return;
            }

            var dialogsSource = DialogsSource;

            Execute.BeginOnThreadPool(() =>
            {
                var useFastSearch = !Text.Contains(" ");

                var results = new List<DialogItem>(dialogsSource.Count);
                foreach (var dialogItem in dialogsSource)
                {
                    var dialog = dialogItem.Dialog;

                    var user = dialog.With as TLUser;
                    var chat = dialog.With as TLChatBase;
                    if (user != null)
                    {
                        if (IsUserValid(user, Text)
                            || IsUserValid(user, TransliterateText)
                            || IsUsernameValid(user, Text))
                        {
                            results.Add(dialogItem);
                        }
                    }
                    else if (chat != null)
                    {
                        if (IsChatValid(chat, Text, useFastSearch)
                            || IsChatValid(chat, TransliterateText, useFastSearch)
                            || IsUsernameValid(chat as IUserName, Text))
                        {
                            var channelForbidden = chat as TLChannelForbidden;
                            if (channelForbidden != null)
                            {
                                continue;
                            }

                            var chat41 = chat as TLChat41;
                            if (chat41 != null && chat41.IsMigrated)
                            {
                                continue;
                            }

                            results.Add(dialogItem);
                        }
                    }
                }

                Results = results;

                Execute.BeginOnUIThread(() => callback.SafeInvoke(Results));
            });
        }

        public void Cancel()
        {
            IsCanceled = true;
        }
    }
}

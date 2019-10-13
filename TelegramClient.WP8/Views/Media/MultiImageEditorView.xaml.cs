// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.UI.ViewManagement;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Telegram.Api.Extensions;
using Telegram.Api.TL;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Contacts;
using TelegramClient.ViewModels.Media;
using TelegramClient.Views.Additional;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;
using PhotoFile = TelegramClient.Services.PhotoFile;

namespace TelegramClient.Views.Media
{
    public partial class MultiImageEditorView : IMultiImageEditorView
    {
        private readonly AppBarButton _doneButton = new AppBarButton
        {
            Text = AppResources.Done,
            IconUri = new Uri("/Images/ApplicationBar/appbar.check.png", UriKind.Relative)
        };

        private readonly AppBarButton _cancelButton = new AppBarButton
        {
            Text = AppResources.Cancel,
            IconUri = new Uri("/Images/ApplicationBar/appbar.cancel.rest.png", UriKind.Relative)
        };

        public MultiImageEditorViewModel ViewModel
        {
            get { return DataContext as MultiImageEditorViewModel; }
        }

        public MultiImageEditorView()
        {
            InitializeComponent();

            Visibility = Visibility.Collapsed;

            BuildLocalizedAppBar();
            
            var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;
            if (isLightTheme)
            {
                TopBorder.Fill = (Brush)Resources["InputBorderBrushLight"];
            }
            else
            {
                TopBorder.Fill = (Brush)Resources["InputBorderBrushDark"];
            }

            Loaded += OnLoadedOnce;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            InputPane.GetForCurrentView().Showing += InputPane_Showing;
            InputPane.GetForCurrentView().Hiding += InputPane_Hiding;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            InputPane.GetForCurrentView().Showing -= InputPane_Showing;
            InputPane.GetForCurrentView().Hiding -= InputPane_Hiding;
        }

        private void InputPane_Hiding(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            CaptionWatermark.Visibility = string.IsNullOrEmpty(Caption.Text) ? Visibility.Visible : Visibility.Collapsed;
            KeyboardPlaceholder.Height = 0.0;
            KeyboardPlaceholder.Visibility = Visibility.Collapsed;
            ImagesGrid.Margin = new Thickness(0.0);

            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };
            var storyboard = new Storyboard();
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.20), Value = 0.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, ImagesGrid);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        private void InputPane_Showing(InputPane sender, InputPaneVisibilityEventArgs args)
        {
            var keyboardHeight = 480.0 / args.OccludedRect.Width * args.OccludedRect.Height - AppBar.ActualHeight + 18.0;

            var height = GetKeyboardHeightDifference(keyboardHeight);
            CaptionWatermark.Visibility = Visibility.Collapsed;
            KeyboardPlaceholder.Height = keyboardHeight;
            KeyboardPlaceholder.Visibility = Visibility.Visible;
            ImagesGrid.Margin = new Thickness(0.0, 0.0, 0.0, -height);

            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 };
            var storyboard = new Storyboard();
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = -height / 2.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, ImagesGrid);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        private void OnLoadedOnce(object sender, RoutedEventArgs e)
        {
            Loaded -= OnLoadedOnce;

            Telegram.Api.Helpers.Execute.BeginOnUIThread(() => ViewModel.OpenEditor());
        }

        private bool _isOpening;

        private void OpenExtendedImageEditorInternal()
        {
            _isOpening = true;
            ExtendedImageEditorPlaceholder.Visibility = Visibility.Visible;
            _extendedImageEditor.UpdateRecentButtonsVisibility();
            _extendedImageEditor.BeginOpenStoryboard(() =>
            {
                _isOpening = false;
                BackgroundBorder.Visibility = Visibility.Collapsed;
                ImagesGrid.Visibility = Visibility.Collapsed;
                Bar.Visibility = Visibility.Collapsed;
            });
        }

        private void CloseExtendedImageEditorIntarnal(bool confirm = true)
        {
            var result = MessageBoxResult.OK;
            if (confirm && _extendedImageEditor.PreviewCanvas.Children.Count > 1)
            {
                result = MessageBox.Show(AppResources.DiscardChangesConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            }
            if (result != MessageBoxResult.OK) return;

            BackgroundBorder.Visibility = Visibility.Visible;
            ImagesGrid.Visibility = Visibility.Visible;
            Bar.Visibility = Visibility.Visible;
            _extendedImageEditor.BeginCloseStoryboard(() =>
            {
                ExtendedImageEditorPlaceholder.Visibility = Visibility.Collapsed;
            });
            
            Focus();
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsOpen))
            {
                if (ViewModel.IsOpen)
                {
                    BeginOpenStoryboard();
                }
                else
                {
                    BeginCloseStoryboard();
                }
            }
            else if (Property.NameEquals(e.PropertyName, () => ViewModel.IsDoneEnabled))
            {
                _doneButton.IsEnabled = ViewModel.IsDoneEnabled;
            }
        }

        private void BuildLocalizedAppBar()
        {
            return;

            if (ApplicationBar != null)
            {
                return;
            }

            ApplicationBar = new ApplicationBar { Opacity = 0.9999, IsVisible = false };
            //ApplicationBar.BackgroundColor = Colors.Black;
            //ApplicationBar.ForegroundColor = Colors.White;
            ApplicationBar.StateChanged += (o, e) =>
            {
                ApplicationBar.Opacity = e.IsMenuVisible ? 0.9999 : 0.0;
            };

            _doneButton.Click += (sender, args) => AppBarAction(ViewModel.Done);
            _doneButton.IsEnabled = false;
            _cancelButton.Click += (sender, args) => AppBarAction(ViewModel.Cancel);

            ApplicationBar.Buttons.Add(_doneButton);
            ApplicationBar.Buttons.Add(_cancelButton);
        }

        private void AppBarAction(System.Action action)
        {
            if (FocusManager.GetFocusedElement() == Caption)
            {
                Items.Focus();
                Telegram.Api.Helpers.Execute.BeginOnUIThread(TimeSpan.FromSeconds(0.20), action.SafeInvoke);
            }
            else
            {
                action.SafeInvoke();
            }
        }

        private void BeginCloseStoryboard()
        {
            SystemTray.IsVisible = true;
            //ApplicationBar.IsVisible = false;

            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, ImagesGrid);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            var opacityImageAniamtion = new DoubleAnimationUsingKeyFrames();
            opacityImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0 });
            Storyboard.SetTarget(opacityImageAniamtion, BackgroundBorder);
            Storyboard.SetTargetProperty(opacityImageAniamtion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityImageAniamtion);

            var translateBarAniamtion = new DoubleAnimationUsingKeyFrames();
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = 0.0 });
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = translateYTo, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateBarAniamtion, Bar);
            Storyboard.SetTargetProperty(translateBarAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateBarAniamtion);

            storyboard.Begin();
            storyboard.Completed += (o, args) =>
            {
                Visibility = Visibility.Collapsed;
            };
        }

        private void BeginOpenStoryboard()
        {
            SystemTray.IsVisible = false;
            //ApplicationBar.IsVisible = true;

            var transparentBlack = Colors.Black;
            transparentBlack.A = 0;

            CaptionWatermark.Visibility = Visibility.Visible;
            Visibility = Visibility.Visible;
            ImagesGrid.Opacity = 1.0;
            ImagesGrid.RenderTransform = new CompositeTransform();
            BackgroundBorder.Opacity = 1.0;


            var duration = TimeSpan.FromSeconds(0.25);
            var easingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 };

            var storyboard = new Storyboard();

            var rootFrameHeight = ((PhoneApplicationFrame)Application.Current.RootVisual).ActualHeight;
            var translateYTo = rootFrameHeight;

            ((CompositeTransform)ImagesGrid.RenderTransform).TranslateY = translateYTo;
            var translateImageAniamtion = new DoubleAnimationUsingKeyFrames();
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = translateYTo });
            translateImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = duration, Value = 0.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateImageAniamtion, ImagesGrid);
            Storyboard.SetTargetProperty(translateImageAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateImageAniamtion);

            var translateBarAniamtion = new DoubleAnimationUsingKeyFrames();
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.15), Value = translateYTo });
            translateBarAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.4), Value = 0.0, EasingFunction = easingFunction });
            Storyboard.SetTarget(translateBarAniamtion, Bar);
            Storyboard.SetTargetProperty(translateBarAniamtion, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(translateBarAniamtion);

            storyboard.Completed += (sender, args) => ViewModel.OpenAnimationComplete();
            Deployment.Current.Dispatcher.BeginInvoke(storyboard.Begin);
        }

        private void Caption_OnGotFocus(object sender, RoutedEventArgs e)
        {
            return;
            
        }

        private double GetKeyboardHeightDifference(double keyboardHeight)
        {
            var heightDifference = keyboardHeight - Items.ActualHeight;

            return heightDifference;
        }

        private void Caption_OnLostFocus(object sender, RoutedEventArgs e)
        {
            return;
        }

        private void Caption_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CaptionWatermark.Visibility = string.IsNullOrEmpty(Caption.Text) && FocusManager.GetFocusedElement() != Caption ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Caption_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ViewModel.Items.Count == 1)
                {
                    ViewModel.Done();
                }
                else
                {
                    Items.Focus();
                }
            }
        }

        private void Image_OnImageOpened(object sender, RoutedEventArgs e)
        {
            var image = (Image) sender;
            image.Opacity = 0.0;
            var storyboard = new Storyboard();

            var opacityImageAniamtion = new DoubleAnimationUsingKeyFrames();
            opacityImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.5), Value = 1.0 });
            Storyboard.SetTarget(opacityImageAniamtion, image);
            Storyboard.SetTargetProperty(opacityImageAniamtion, new PropertyPath("Opacity"));
            storyboard.Children.Add(opacityImageAniamtion);

            storyboard.Begin();
        }

        private static List<Telegram.Api.WindowsPhone.Tuple<PhotoFile, Image>> _imagesCache = new List<Telegram.Api.WindowsPhone.Tuple<PhotoFile, Image>>();

        private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            var image = (Image) sender;
            var photoFile = image.DataContext as PhotoFile;
            _imagesCache.Add(new Telegram.Api.WindowsPhone.Tuple<PhotoFile, Image>(photoFile, image));
        }

        public static void ImageOpened(PhotoFile photoFile)
        {
            var tuple = _imagesCache.LastOrDefault(x => x.Item1 == photoFile);
            _imagesCache.Remove(tuple);
            if (tuple != null)
            {
                var image = tuple.Item2;
                image.Opacity = 0.0;
                var storyboard = new Storyboard();

                var opacityImageAniamtion = new DoubleAnimationUsingKeyFrames();
                opacityImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.5), Value = 1.0 });
                Storyboard.SetTarget(opacityImageAniamtion, image);
                Storyboard.SetTargetProperty(opacityImageAniamtion, new PropertyPath("Opacity"));
                storyboard.Children.Add(opacityImageAniamtion);

                storyboard.Begin();
            }
        }

        private void ContextMenu_OnLoaded(object sender, RoutedEventArgs e)
        {
            var menu = (ContextMenu) sender;
            menu.Visibility = ViewModel.Items.FirstOrDefault(x => x.Message != null) != null ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            DeleteCurrentItem();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            DeleteCurrentItem();
        }

        private ExtendedImageEditor _extendedImageEditor;

        private void Edit_OnClick(object sender, RoutedEventArgs args)
        {
            var photoFile = ViewModel.CurrentItem;
            if (photoFile != null)
            {
                if (photoFile.IsButton) return;
                if (photoFile.File == null) return;

                var previewBitmap = Preview.Source as BitmapSource;
                if (previewBitmap == null) return;

                if (_extendedImageEditor == null)
                {
                    _extendedImageEditor = new ExtendedImageEditor();
                    _extendedImageEditor.Done += (o, e) =>
                    {
                        CloseExtendedImageEditorIntarnal(false);
                    };
                    _extendedImageEditor.Cancel += (o, e) =>
                    {
                        CloseExtendedImageEditorIntarnal();
                    };
                    ExtendedImageEditorPlaceholder.Content = _extendedImageEditor;
                }
                
                OpenExtendedImageEditorInternal();
                _extendedImageEditor.Detect(photoFile, previewBitmap);
            }
        }

        private void DeleteCurrentItem()
        {
            var photoFile = ViewModel.CurrentItem;
            if (photoFile != null)
            {
                if (photoFile.IsButton) return;
                
                var index = ViewModel.Items.IndexOf(photoFile);
                if (index == -1) return;

                var container = Items.ContainerFromItem(photoFile);
                if (container != null)
                {
                    var storyboard = new Storyboard();

                    var opacityImageAniamtion = new DoubleAnimationUsingKeyFrames();
                    opacityImageAniamtion.KeyFrames.Add(new EasingDoubleKeyFrame
                    {
                        KeyTime = TimeSpan.FromSeconds(0.25),
                        Value = 0.0
                    });
                    Storyboard.SetTarget(opacityImageAniamtion, container);
                    Storyboard.SetTargetProperty(opacityImageAniamtion, new PropertyPath("Opacity"));
                    storyboard.Children.Add(opacityImageAniamtion);

                    storyboard.Begin();
                    storyboard.Completed += (o, args) => ViewModel.Delete(photoFile);
                }
            }
        }

        public bool IsExtendedImageEditorOpened
        {
            get { return _extendedImageEditor != null && ExtendedImageEditorPlaceholder.Visibility == Visibility.Visible; }
        }

        public void CloseExtendedImageEditor()
        {
            if (_extendedImageEditor.IsStickerPanelOpened)
            {
                _extendedImageEditor.CloseStickerPanel();
            }
            else if (_isOpening)
            {
                CloseExtendedImageEditorIntarnal();
                return;
            }
            else if (ExtendedImageEditorPlaceholder.Visibility == Visibility.Visible)
            {
                CloseExtendedImageEditorIntarnal();
                return;
            }
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Done();

            if (_extendedImageEditor != null)
            {
                var stickersControl = _extendedImageEditor.StickersPanel.Content as StickersControl;
                if (stickersControl != null)
                {
                    var viewModel = ViewModel;
                    Telegram.Api.Helpers.Execute.BeginOnThreadPool(() =>
                    {
                        var stickers = new List<TLDocument22>();
                        var masks = new List<TLDocument22>();

                        foreach (var item in viewModel.Items)
                        {
                            var message = item.Message as TLMessage48;
                            if (message != null && message.Documents != null)
                            {
                                foreach (var document in message.Documents)
                                {
                                    var document54 = document as TLDocument54;
                                    if (document54 != null)
                                    {
                                        var attributeSticker = document54.Attributes.FirstOrDefault(x => x is TLDocumentAttributeSticker56) as TLDocumentAttributeSticker56;
                                        if (attributeSticker != null)
                                        {
                                            if (attributeSticker.Mask)
                                            {
                                                masks.Add(document54);
                                            }
                                            else
                                            {
                                                stickers.Add(document54);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        stickersControl.UpdateRecentAsync(stickers, masks);
                    });
                }
            }
        }

        public void ClosePreview()
        {
            if (_extendedImageEditor != null)
            {
                _extendedImageEditor.ClosePreview();
            }
        }

        private void Timer_OnClick(object sender, GestureEventArgs e)
        {
            var stateService = IoC.Get<IStateService>();
            stateService.SelectedTimerSpan = ViewModel.CurrentItem.TimerSpan;
            var chooseTTLViewModel = IoC.Get<ChooseTTLViewModel>();
            chooseTTLViewModel.Subtitle = AppResources.SelfDestructTimerPhotoSubtitle;

            var chooseTTLView = new ChooseTTLView { Height = 330.0, DataContext = chooseTTLViewModel, Margin = new Thickness(0.0, -34.0, 0.0, -6.0) };
            ShellViewModel.ShowCustomMessageBox(null, null, AppResources.Done.ToLowerInvariant(), AppResources.Cancel.ToLowerInvariant(),
                result =>
                {
                    if (result == CustomMessageBoxResult.RightButton)
                    {
                        var selector = chooseTTLView.Selector;

                        ViewModel.CurrentItem.TimerSpan = ((TimerSpan)selector.DataSource.SelectedItem);
                    }
                },
                chooseTTLView);
        }

        private void AddButton_OnTap(object sender, GestureEventArgs e)
        {
            ViewModel.PickPhoto();
        }

        private void GroupedIcon_OnTap(object sender, GestureEventArgs e)
        {
            ShowHint();
        }

        private void ShowHint()
        {
            if (InputMessageHintPlaceholder.Content == null)
            {
                var control = new InputMessageHint(true);
                control.FontSize = new ScaledText().DefaultFontSize;
                control.Closed += OnInputMessageHintClosed;

                InputMessageHintPlaceholder.Content = control;
            }

            var inputMessageHint = InputMessageHintPlaceholder.Content as InputMessageHint;
            if (inputMessageHint != null)
            {
                inputMessageHint.Hint = ViewModel.IsGrouped ? AppResources.GroupMediaDescription : AppResources.UngroupMediaDescription;
            }
        }

        private void OnInputMessageHintClosed(object sender, System.EventArgs e)
        {
            var control = sender as InputMessageHint;
            if (control != null)
            {
                control.Closed -= OnInputMessageHintClosed;
            }

            InputMessageHintPlaceholder.Content = null;
        }

        private void UsernameHint_OnTap(object sender, GestureEventArgs e)
        {
            Caption.Focus();

            var frameworkElement = e.OriginalSource as FrameworkElement;
            if (frameworkElement != null)
            {
                var user = frameworkElement.DataContext as IUserName;
                if (user != null)
                {
                    var userBase = user as TLUserBase;

                    var index = 0;
                    for (var i = Caption.Text.Length - 1; i >= 0; i--)
                    {
                        if (Caption.Text[i] == '@')
                        {
                            index = i;
                            break;
                        }
                    }

                    if (TLString.IsNullOrEmpty(user.UserName))
                    {
                        if (userBase != null)
                        {
                            ViewModel.AddMention(userBase);

                            Caption.Text = string.Format("{0}({1})", Caption.Text.Substring(0, index + 1), userBase.FullName);
                            Caption.SelectionStart = Caption.Text.Length - userBase.FullName.Length - 1;
                            Caption.SelectionLength = userBase.FullName.Length;
                        }
                    }
                    else
                    {
                        Caption.Text = string.Format("{0}{1} ", Caption.Text.Substring(0, index + 1), user.UserName);
                        Caption.SelectionStart = Caption.Text.Length;
                        Caption.SelectionLength = 0;
                    }
                }
            }
        }

        private void UsernameHints_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ViewModel.ContinueLoadMentionHints();
        }
    }

    public interface IMultiImageEditorView
    {
        bool IsExtendedImageEditorOpened { get; }

        void CloseExtendedImageEditor();
    }
}

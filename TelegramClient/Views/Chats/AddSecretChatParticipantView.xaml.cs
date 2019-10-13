// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using TelegramClient.Animation.Navigation;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Chats;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Chats
{
    public partial class AddSecretChatParticipantView
    {
        public AddSecretChatParticipantViewModel ViewModel
        {
            get { return DataContext as AddSecretChatParticipantViewModel; }
        }

        private readonly ApplicationBarIconButton _searchButton = new ApplicationBarIconButton
        {
            Text = AppResources.Search,
            IconUri = new Uri("/Images/ApplicationBar/appbar.feature.search.rest.png", UriKind.Relative)
        };

        public AddSecretChatParticipantView()
        {
            InitializeComponent();

            _searchButton.Click += (sender, args) => ViewModel.Search();

            Loaded += (o, e) =>
            {
                BuildLocalizedAppBar();
                ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            };

            Unloaded += (o, e) =>
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            };
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsWorking))
            {
                if (!ViewModel.IsWorking)
                {
                    RestoreTapedItem();
                }
            }
        }

        private void RestoreTapedItem()
        {
            if (_tapedItem != null)
            {
                if (_lastStoryboard != null)
                {
                    _lastStoryboard.Stop();
                }

                _tapedItem.Opacity = 1.0;
                var compositeTransform = _tapedItem.RenderTransform as CompositeTransform;
                if (compositeTransform != null)
                {
                    compositeTransform.TranslateX = 0.0;
                    compositeTransform.TranslateY = 0.0;
                }
            }
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            RestoreTapedItem();

            if (ViewModel.Contact != null)
            {
                e.Cancel = true;
                ViewModel.CancelSecretChat();
                return;
            }

            base.OnBackKeyPress(e);
        }

        private bool _initialized;

        private void BuildLocalizedAppBar()
        {
            if (_initialized) return;

            _initialized = true;

            ApplicationBar = new ApplicationBar();
            ApplicationBar.Buttons.Add(_searchButton);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (_tapedItem != null)
            {
                _tapedItem.Opacity = 1.0;
                ((CompositeTransform)_tapedItem.RenderTransform).TranslateX = 0.0;
                ((CompositeTransform)_tapedItem.RenderTransform).TranslateY = 0.0;
            }
            base.OnNavigatedTo(e);
        }

        private FrameworkElement _tapedItem;
        private Storyboard _lastStoryboard;

        private void MainItemGrid_OnTap(object sender, GestureEventArgs e)
        {
            if (ViewModel.IsWorking) return;

            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null) return;

            var user = frameworkElement.DataContext as TLUserBase;
            if (user == null) return;

            ViewModel.UserAction(user);

            var tapedItem = frameworkElement;

            _tapedItem = tapedItem;
            
            VisualTreeExtensions.FindVisualChildWithPredicate<TextBlock>(tapedItem, AnimatedBasePage.GetIsAnimationTarget);

            if (!(_tapedItem.RenderTransform is CompositeTransform))
            {
                _tapedItem.RenderTransform = new CompositeTransform();
            }

            FrameworkElement tapedItemContainer = _tapedItem.FindParentOfType<ListBoxItem>();

            if (tapedItemContainer != null)
            {
                tapedItemContainer = tapedItemContainer.FindParentOfType<ContentPresenter>();
            }

            var storyboard = new Storyboard();

            var timeline = new DoubleAnimationUsingKeyFrames();
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            timeline.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 73.0 });
            Storyboard.SetTarget(timeline, tapedItem);
            Storyboard.SetTargetProperty(timeline, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateY)"));
            storyboard.Children.Add(timeline);

            var timeline2 = new DoubleAnimationUsingKeyFrames();
            timeline2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 0.0 });
            timeline2.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 425.0, EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn, Exponent = 5.0 } });
            Storyboard.SetTarget(timeline2, tapedItem);
            Storyboard.SetTargetProperty(timeline2, new PropertyPath("(UIElement.RenderTransform).(CompositeTransform.TranslateX)"));
            storyboard.Children.Add(timeline2);

            var timeline3 = new DoubleAnimationUsingKeyFrames();
            timeline3.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 1.0 });
            timeline3.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.2), Value = 1.0 });
            timeline3.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
            Storyboard.SetTarget(timeline3, tapedItem);
            Storyboard.SetTargetProperty(timeline3, new PropertyPath("(UIElement.Opacity)"));
            storyboard.Children.Add(timeline3);

            if (tapedItemContainer != null)
            {
                var timeline4 = new ObjectAnimationUsingKeyFrames();
                timeline4.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.0), Value = 999.0 });
                timeline4.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = 0.0 });
                Storyboard.SetTarget(timeline4, tapedItemContainer);
                Storyboard.SetTargetProperty(timeline4, new PropertyPath("(Canvas.ZIndex)"));
                storyboard.Children.Add(timeline4);
            }
            _lastStoryboard = storyboard;
            storyboard.Begin();
        }
    }
}
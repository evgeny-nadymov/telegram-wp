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
using System.Windows.Input;
using System.Windows.Media.Animation;
using Caliburn.Micro;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.Services;
using TelegramClient.ViewModels.Calls;

namespace TelegramClient.Views.Calls
{
    public partial class CallView
    {
        public CallViewModel ViewModel
        {
            get { return DataContext as CallViewModel; }
        }

        private bool _initState;

        public CallView()
        {
            InitializeComponent();

            LayoutRoot.Background = ShellView.CaptionBrush;

            Loaded += (sender, args) =>
            {
                if (!_initState)
                {
                    _initState = true;
                    GoToState(ViewModel.ViewState);
                }
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;

                Telegram.Api.Helpers.Execute.BeginOnUIThread(() =>
                {
                    ViewModel.Signal = IoC.Get<IVoIPService>().GetSignalBarsCount();
                });
            };
            Unloaded += (sender, args) =>
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            };
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (Property.NameEquals(args.PropertyName, () => ViewModel.ViewState))
            {
                GoToState(ViewModel.ViewState);
            }
        }

        public void GoToState(CallViewState state)
        {
            System.Diagnostics.Debug.WriteLine("GoToState state=" + state);
            BottomCommand.IsEnabled = true;
            IgnoreButton.IsEnabled = true;
            VisualStateManager.GoToState(this, state.ToString(), false);
            switch (state)
            {
                    case CallViewState.Call:
                    BottomCommandLabel.Text = AppResources.EndCall;
                    IgnoreButtonLabel.Text = AppResources.Ignore;
                    break;
                    case CallViewState.CallConnecting:
                    BottomCommandLabel.Text = AppResources.EndCall;
                    IgnoreButtonLabel.Text = AppResources.Ignore;
                    break;
                    case CallViewState.IncomingCall:
                    BottomCommandLabel.Text = AppResources.Answer;
                    IgnoreButtonLabel.Text = AppResources.Ignore;
                    break;
                    case CallViewState.IncomingCallBusy:
                    BottomCommandLabel.Text = AppResources.Answer;
                    IgnoreButtonLabel.Text = AppResources.Ignore;
                    break;
                    case CallViewState.OutgoingCall:
                    BottomCommandLabel.Text = AppResources.Cancel;
                    IgnoreButtonLabel.Text = AppResources.Ignore;
                    break;
                    case CallViewState.OutgoingCallBusy:
                    BottomCommandLabel.Text = AppResources.Call;
                    IgnoreButtonLabel.Text = AppResources.Cancel;
                    break;
            }
        }

        private void EmojisPanel_OnTap(object sender, GestureEventArgs e)
        {
            var expanded = EmojisTransform.TranslateX == 0.0;
            var scaleX = expanded ? 2.5 : 1.0;
            var scaleY = expanded ? 2.5 : 1.0;
            var labelOpacity = expanded ? 1.0 : 0.0;

            var pointFrom = EmojisPanel.TransformToVisual(this).Transform(new Point(EmojisPanel.ActualWidth / 2.0, EmojisPanel.ActualHeight / 2.0));
            var pointTo = EmojiKeyLabel.TransformToVisual(this).Transform(new Point(EmojiKeyLabel.ActualWidth / 2.0, 0.0 - EmojisPanel.ActualHeight * scaleY));//new Point(480.0/2.0, 800.0/2.0);

            var storyboard = new Storyboard();

            var translateX = expanded ? pointTo.X - pointFrom.X : 0.0;
            var translateXAnimation = new DoubleAnimationUsingKeyFrames();
            translateXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = translateX });
            Storyboard.SetTargetProperty(translateXAnimation, new PropertyPath("TranslateX"));
            storyboard.Children.Add(translateXAnimation);

            var translateY = expanded ? pointTo.Y - pointFrom.Y : 0.0;
            var translateYAnimation = new DoubleAnimationUsingKeyFrames();
            translateYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = translateY });
            Storyboard.SetTargetProperty(translateYAnimation, new PropertyPath("TranslateY"));
            storyboard.Children.Add(translateYAnimation);

            var scaleXAnimation = new DoubleAnimationUsingKeyFrames();
            scaleXAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = scaleX });
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("ScaleX"));
            storyboard.Children.Add(scaleXAnimation);

            var scaleYAnimation = new DoubleAnimationUsingKeyFrames();
            scaleYAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = scaleY });
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("ScaleY"));
            storyboard.Children.Add(scaleYAnimation);

            var labelOpacityAnimation = new DoubleAnimationUsingKeyFrames();
            labelOpacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = labelOpacity });
            Storyboard.SetTargetProperty(labelOpacityAnimation, new PropertyPath("Opacity"));
            Storyboard.SetTarget(labelOpacityAnimation, EmojiKeyLabel);
            storyboard.Children.Add(labelOpacityAnimation);

            var backgroundOpacityAnimation = new DoubleAnimationUsingKeyFrames();
            backgroundOpacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame { KeyTime = TimeSpan.FromSeconds(0.25), Value = labelOpacity });
            Storyboard.SetTargetProperty(backgroundOpacityAnimation, new PropertyPath("Opacity"));
            Storyboard.SetTarget(backgroundOpacityAnimation, PhotoBorder);
            storyboard.Children.Add(backgroundOpacityAnimation);

            Storyboard.SetTarget(storyboard, EmojisTransform);

            storyboard.Begin();
        }

        private void Speaker_OnChecked(object sender, RoutedEventArgs e)
        {
            ViewModel.SwitchSpeaker(true);
        }

        private void Speaker_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.SwitchSpeaker(false);
        }

        private void Mute_OnChecked(object sender, RoutedEventArgs e)
        {
            ViewModel.Mute(true);
        }

        private void Mute_OnUnchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.Mute(false);
        }
    }

    public enum CallViewState
    {
        Call,
        CallConnecting,
        OutgoingCall,
        OutgoingCallBusy,
        IncomingCall,
        IncomingCallBusy
    }
}
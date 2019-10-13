// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Telegram.Api.Helpers;
using Telegram.Api.TL;
using Telegram.Controls.Extensions;
using TelegramClient.Helpers;
using TelegramClient.Resources;
using TelegramClient.ViewModels.Additional;
using TelegramClient.ViewModels.Search;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace TelegramClient.Views.Additional
{
    public interface IChooseAttachmentView
    {
        void HideInlineBots();
        void ShowHint(string hint);
    }

    public partial class ChooseAttachmentView : IChooseAttachmentView
    {
        public ChooseAttachmentViewModel ViewModel
        {
            get { return DataContext as ChooseAttachmentViewModel; }
        }

        public ChooseAttachmentView()
        {
            InitializeComponent();

            LayoutRoot.Visibility = Visibility.Collapsed;
            Hint.Visibility = Visibility.Collapsed;

            Loaded += (o, e) => ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            Unloaded += (o, e) => ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Property.NameEquals(e.PropertyName, () => ViewModel.IsOpen))
            {
                if (ViewModel.IsOpen)
                {
                    OpenContactItem.Visibility = ViewModel.OpenContactVisibility;
                    OpenStoryboard.Begin();
                }
                else
                {
                    if (ViewModel.InlineBots.Count > 0 && ContentPanelTransform.Y == 140.0)
                    {
                        InlineBotsControl.Opacity = 0.0;
                    }
                    CloseStoryboard.Begin();
                }
            }
        }

        private void LayoutRoot_OnTap(object sender, GestureEventArgs e)
        {
            ((ChooseAttachmentViewModel)DataContext).Close();
        }

        private static DateTime? _lastOpenTime;

        private void OpenStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            _lastOpenTime = DateTime.Now;

            InlineBotsControl.Opacity = 1.0;
            var isLoading = ViewModel.LoadInlineBots(ShowHint);
            if (!isLoading)
            {
                ShowHint();
            }
        }

        private void ShowHint()
        {
            Hint.Opacity = ViewModel.InlineBots.Count > 0 ? 0.4 : 0.0;
            Hint.Visibility = ViewModel.InlineBots.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (ViewModel.InlineBots.Count > 0)
            {
                _isSuppressInlineBots = IsSuppressInlineBotsHint();

                if (_isSuppressInlineBots == true) return;

                if (InputMessageHintPlaceholder.Content == null)
                {
                    var control = new InputMessageHint(true);
                    control.Closed += OnInputMessageHintClosed;

                    InputMessageHintPlaceholder.Content = control;
                }

                var inputMessageHint = InputMessageHintPlaceholder.Content as InputMessageHint;
                if (inputMessageHint != null)
                {
                    inputMessageHint.Hint = AppResources.OpenInlineBotsHint;
                }
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

        public void HideInlineBots()
        {
            if (ViewModel.InlineBots.Count == 0) return;

            ContentPanelTransform.Y = 140.0;
        }

        public void ShowHint(string hint)
        {

        }

        private bool _manipulating;

        private void ContentPanel_OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            if (ViewModel.InlineBots.Count == 0) return;

            _manipulating = true;
        }

        private readonly object _suppressInlineBotsHintSyncRoot = new object();

        private static bool? _isSuppressInlineBots;

        private bool IsSuppressInlineBotsHint()
        {
            try
            {
                bool result;
                lock (_suppressInlineBotsHintSyncRoot)
                {
                    if (_isSuppressInlineBots.HasValue)
                    {
                        result = _isSuppressInlineBots.Value;
                    }
                    else
                    {
                        result = TLUtils.OpenObjectFromMTProtoFile<TLBool>(_suppressInlineBotsHintSyncRoot, Constants.SuppressInlineBotsHintFileName) != null;

                        _isSuppressInlineBots = result;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }

            return false;
        }

        private void SuppressInlineBotsHint()
        {
            try
            {
                lock (_suppressInlineBotsHintSyncRoot)
                {
                    if (_isSuppressInlineBots == true)
                    {
                        return;
                    }

                    TLUtils.SaveObjectToMTProtoFile(_suppressInlineBotsHintSyncRoot, Constants.SuppressInlineBotsHintFileName, TLBool.True);

                    _isSuppressInlineBots = true;
                }
            }
            catch (Exception ex)
            {
                Execute.ShowDebugMessage(ex.ToString());
            }
        }

        private void ContentPanel_OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (!_manipulating) return;

            _manipulating = false;
            //Debug.WriteLine(e.FinalVelocities.LinearVelocity.Y + " " + e.IsInertial + " " + e.TotalManipulation.Translation.Y);

            if (e.FinalVelocities.LinearVelocity.Y < -500.0)
            {
                if (ContentPanelTransform.Y == 0.0)
                {
                    SuppressInlineBotsHint();
                    return;
                }

                var transformTo = 0.0;
                var defaultDuration = 0.5;
                var maxExpansionTransform = 140.0;
                var velocityDuration = maxExpansionTransform / Math.Abs(e.FinalVelocities.LinearVelocity.Y);
                var duration = Math.Abs(ContentPanelTransform.Y - transformTo) / maxExpansionTransform * defaultDuration;
                var minDuration = Math.Min(duration, velocityDuration);
                if (minDuration < 0.1) minDuration = 0.1;

                var storyboard = new Storyboard();
                var transformAnimaion = new DoubleAnimation { To = transformTo, Duration = TimeSpan.FromSeconds(minDuration), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } };
                Storyboard.SetTarget(transformAnimaion, ContentPanelTransform);
                Storyboard.SetTargetProperty(transformAnimaion, new PropertyPath("Y"));
                storyboard.Children.Add(transformAnimaion);
                var opacityAnimaion = new DoubleAnimation { To = 0.0, Duration = TimeSpan.FromSeconds(minDuration) };
                Storyboard.SetTarget(opacityAnimaion, Hint);
                Storyboard.SetTargetProperty(opacityAnimaion, new PropertyPath("Opacity"));
                storyboard.Children.Add(opacityAnimaion);
                storyboard.Begin();
                storyboard.Completed += (o, args) => SuppressInlineBotsHint();
                return;
            }
            else if (e.FinalVelocities.LinearVelocity.Y > 500.0)
            {
                if (ContentPanelTransform.Y == 140.0) return;

                var transformTo = 140.0;
                var defaultDuration = 0.5;
                var maxExpansionTransform = 140.0;
                var velocityDuration = maxExpansionTransform / Math.Abs(e.FinalVelocities.LinearVelocity.Y);
                var duration = Math.Abs(ContentPanelTransform.Y - transformTo) / maxExpansionTransform * defaultDuration;
                var minDuration = Math.Min(duration, velocityDuration);
                if (minDuration < 0.1) minDuration = 0.1;

                var storyboard = new Storyboard();
                var transformAnimaion = new DoubleAnimation { To = transformTo, Duration = TimeSpan.FromSeconds(minDuration), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } };
                Storyboard.SetTarget(transformAnimaion, ContentPanelTransform);
                Storyboard.SetTargetProperty(transformAnimaion, new PropertyPath("Y"));
                storyboard.Children.Add(transformAnimaion);
                var opacityAnimaion = new DoubleAnimation { To = .4, Duration = TimeSpan.FromSeconds(minDuration) };
                Storyboard.SetTarget(opacityAnimaion, Hint);
                Storyboard.SetTargetProperty(opacityAnimaion, new PropertyPath("Opacity"));
                storyboard.Children.Add(opacityAnimaion);
                storyboard.Begin();
                return;
            }

            if (ContentPanelTransform.Y < 70.0)
            {
                if (ContentPanelTransform.Y == 0.0)
                {
                    SuppressInlineBotsHint();
                    return;
                }

                var transformTo = 0.0;
                var defaultDuration = 0.5;
                var maxExpansionTransform = 140.0;
                var duration = Math.Abs(ContentPanelTransform.Y - transformTo) / maxExpansionTransform * defaultDuration;
                if (duration < 0.1) duration = 0.1;

                var storyboard = new Storyboard();
                var transformAnimaion = new DoubleAnimation { To = transformTo, Duration = TimeSpan.FromSeconds(duration), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } };
                Storyboard.SetTarget(transformAnimaion, ContentPanelTransform);
                Storyboard.SetTargetProperty(transformAnimaion, new PropertyPath("Y"));
                storyboard.Children.Add(transformAnimaion);
                var opacityAnimaion = new DoubleAnimation { To = 0.0, Duration = TimeSpan.FromSeconds(duration) };
                Storyboard.SetTarget(opacityAnimaion, Hint);
                Storyboard.SetTargetProperty(opacityAnimaion, new PropertyPath("Opacity"));
                storyboard.Children.Add(opacityAnimaion);
                storyboard.Begin();
                storyboard.Completed += (o, args) => SuppressInlineBotsHint();
            }
            else
            {
                if (ContentPanelTransform.Y == 140.0) return;

                var transformTo = 140.0;
                var defaultDuration = 0.5;
                var maxExpansionTransform = 140.0;
                var duration = Math.Abs(ContentPanelTransform.Y - transformTo) / maxExpansionTransform * defaultDuration;
                if (duration < 0.1) duration = 0.1;

                var storyboard = new Storyboard();
                var transformAnimaion = new DoubleAnimation { To = transformTo, Duration = TimeSpan.FromSeconds(duration), EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5.0 } };
                Storyboard.SetTarget(transformAnimaion, ContentPanelTransform);
                Storyboard.SetTargetProperty(transformAnimaion, new PropertyPath("Y"));
                storyboard.Children.Add(transformAnimaion);
                var opacityAnimaion = new DoubleAnimation { To = .4, Duration = TimeSpan.FromSeconds(duration) };
                Storyboard.SetTarget(opacityAnimaion, Hint);
                Storyboard.SetTargetProperty(opacityAnimaion, new PropertyPath("Opacity"));
                storyboard.Children.Add(opacityAnimaion);
                storyboard.Begin();
            }
        }

        private void ContentPanel_OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (!_manipulating) return;

            var nextTransform = ContentPanelTransform.Y + e.DeltaManipulation.Translation.Y;

            if (nextTransform < 0.0)
            {
                nextTransform = ContentPanelTransform.Y + 0.05 * e.DeltaManipulation.Translation.Y;
            }
            if (nextTransform > 140.0)
            {
                nextTransform = 140.0;
            }

            ContentPanelTransform.Y = nextTransform;

            var currentValue = ContentPanelTransform.Y;
            var maxValue = 140.0;
            var minValue = 0.0;

            if (currentValue > maxValue) currentValue = maxValue;
            if (currentValue < minValue) currentValue = minValue;

            var opacity = 0.4 * currentValue / (maxValue - minValue);
            Hint.Opacity = opacity;
        }

        private ScrollViewer _scrollViewer;

        private void CloseStoryboard_OnCompleted(object sender, System.EventArgs e)
        {
            if (InlineBotsControl.Items.Count > 0)
            {
                _scrollViewer = _scrollViewer ?? this.FindChildOfType<ScrollViewer>();

                if (_scrollViewer != null)
                {
                    _scrollViewer.ScrollToHorizontalOffset(0.0);
                }
            }

            Hint.Visibility = Visibility.Collapsed;
            InlineBotsControl.Opacity = 0.0;
            HideInlineBots();
        }
    }
}
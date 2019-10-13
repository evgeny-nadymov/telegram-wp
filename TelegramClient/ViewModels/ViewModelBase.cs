// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Caliburn.Micro;
using Microsoft.Phone.Controls;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels
{
    public abstract class ViewModelBase : Screen
    {

        private Visibility _visibility;

        public Visibility Visibility
        {
            get { return _visibility; }
            set { SetField(ref _visibility, value, () => Visibility); }
        }

        protected bool SetField<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyOfPropertyChange(propertyName);
            return true;
        }

        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyOfPropertyChange(selectorExpression);
            return true;
        }

        private bool _isLoadingError;

        public bool IsLoadingError
        {
            get { return _isLoadingError; }
            set { SetField(ref _isLoadingError, value, () => IsLoadingError); }
        }

        private bool _isWorking;

        public bool IsWorking
        {
            get { return _isWorking; }
            set { SetField(ref _isWorking, value, () => IsWorking); }
        }
        
        public IMTProtoService MTProtoService { get; private set; }

        protected readonly INavigationService NavigationService;

        public IStateService StateService { get; private set; }

        protected readonly ITelegramEventAggregator EventAggregator;

        protected readonly ICommonErrorHandler ErrorHandler;

        protected readonly ICacheService CacheService;

        private static DateTime _lastStatusTime;

        protected ViewModelBase(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
        {
            CacheService = cacheService;
            ErrorHandler = errorHandler;
            StateService = stateService;
            MTProtoService = mtProtoService;
            NavigationService = navigationService;
            EventAggregator = eventAggregator;
        }

        protected bool SuppressUpdateStatus { get;  set; }

        protected override void OnActivate()
        {
            var offline = ((App) Application.Current).Offline;
            if (offline)
            {
                ((App) Application.Current).Offline = false;

                _lastStatusTime = DateTime.Now;

                BeginOnThreadPool(() =>
                    StateService.GetNotifySettingsAsync(
                        settings =>
                        {
                            var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);

                            if (isAuthorized && !settings.InvisibleMode)
                            {
                                MTProtoService.UpdateStatusAsync(TLBool.False,
                                    result =>
                                    {
                                        
                                    });
                            }
                        }));
            }
            else
            {
                if (SuppressUpdateStatus) return;

                if ((DateTime.Now - _lastStatusTime).TotalSeconds < 20.0)
                {
                    return;
                }
                _lastStatusTime = DateTime.Now;

                BeginOnThreadPool(() =>
                    StateService.GetNotifySettingsAsync(
                        settings =>
                        {
                            var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);

                            if (isAuthorized && !settings.InvisibleMode)
                            {
                                MTProtoService.RaiseSendStatus(new SendStatusEventArgs(TLBool.False));
                            }
                        }));
            }

            base.OnActivate();
        }

        public void BeginOnUIThread(System.Action action)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(action);
        }

        public void BeginOnUIThread(TimeSpan delay, System.Action action)
        {
            Telegram.Api.Helpers.Execute.BeginOnUIThread(delay, action);
        }

        public void BeginOnThreadPool(System.Action action)
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(action);
        }

        public void BeginOnThreadPool(TimeSpan delay, System.Action action)
        {
            Telegram.Api.Helpers.Execute.BeginOnThreadPool(delay, action);
        }

        public void Subscribe()
        {
            EventAggregator.Subscribe(this);
        }

        public void Unsubscribe()
        {
            EventAggregator.Unsubscribe(this);
        }

        public void Report(TLInputPeerBase inputPeer, TLVector<TLInt> id = null)
        {
            var spamRadioButton = new RadioButton { Content = AppResources.Spam, IsChecked = true, Margin = new Thickness(0.0, 0.0, 12.0, -12.0), Background = new SolidColorBrush(Colors.Transparent), GroupName = "Report" };
            var violenceRadioButton = new RadioButton { Content = AppResources.Violence, Margin = new Thickness(0.0, -12.0, 12.0, -12.0), Background = new SolidColorBrush(Colors.Transparent), GroupName = "Report" };
            var pornographyRadioButton = new RadioButton { Content = AppResources.Pornography, Margin = new Thickness(0.0, -12.0, 12.0, -12.0), Background = new SolidColorBrush(Colors.Transparent), GroupName = "Report" };
            var copyrightRadioButton = new RadioButton { Content = AppResources.Copyright, Margin = new Thickness(0.0, -12.0, 12.0, -12.0), Background = new SolidColorBrush(Colors.Transparent), GroupName = "Report" };
            var otherRadioButton = new RadioButton { Content = AppResources.Other, Margin = new Thickness(0.0, -12.0, 12.0, -12.0), Background = new SolidColorBrush(Colors.Transparent), GroupName = "Report" };

            TiltEffect.SetIsTiltEnabled(spamRadioButton, true);
            TiltEffect.SetIsTiltEnabled(violenceRadioButton, true);
            TiltEffect.SetIsTiltEnabled(pornographyRadioButton, true);
            TiltEffect.SetIsTiltEnabled(otherRadioButton, true);

            var reportContent = new StackPanel();
            reportContent.Children.Add(spamRadioButton);
            reportContent.Children.Add(violenceRadioButton);
            reportContent.Children.Add(pornographyRadioButton);
            reportContent.Children.Add(copyrightRadioButton);
            reportContent.Children.Add(otherRadioButton);

            var confirmation = new CustomMessageBox
            {
                Caption = AppResources.Report,
                Message = string.Empty,
                Content = reportContent,
                LeftButtonContent = AppResources.Cancel.ToLowerInvariant(),
                RightButtonContent = AppResources.Ok.ToLowerInvariant(),
                IsLeftButtonEnabled = true,
                IsRightButtonEnabled = true
            };

#if WP8
            var isFullHD = Application.Current.Host.Content.ScaleFactor == 225 || Environment.OSVersion.Version.Major >= 10;
            if (isFullHD)
            {
                spamRadioButton.FontSize = 17.667;
                violenceRadioButton.FontSize = 17.667;
                pornographyRadioButton.FontSize = 17.667;
                copyrightRadioButton.FontSize = 17.667;
                otherRadioButton.FontSize = 17.667;
                confirmation.Style = (Style)Application.Current.Resources["CustomMessageBoxFullHDStyle"];
            }
#endif

            confirmation.Dismissed += (sender, args) =>
            {
                if (args.Result == CustomMessageBoxResult.RightButton)
                {
                    TLInputReportReasonBase reason = null;
                    if (spamRadioButton.IsChecked == true)
                    {
                        reason = new TLInputReportReasonSpam();
                    }
                    else if (violenceRadioButton.IsChecked == true)
                    {
                        reason = new TLInputReportReasonViolence();
                    }
                    else if (pornographyRadioButton.IsChecked == true)
                    {
                        reason = new TLInputReportReasonPornography();
                    }
                    else if (copyrightRadioButton.IsChecked == true)
                    {
                        reason = new TLInputReportReasonCopyright();
                    }
                    else if (otherRadioButton.IsChecked == true)
                    {
                        reason = new TLInputReportReasonOther { Text = TLString.Empty };
                    }
                    GetReasonAndReportAsync(inputPeer, id, reason);
                }
            };
            confirmation.Show();
        }

        private void GetReasonAndReportAsync(TLInputPeerBase inputPeer, TLVector<TLInt> id, TLInputReportReasonBase reason)
        {
            if (reason is TLInputReportReasonOther)
            {
                var text = new TextBox { Margin = new Thickness(0.0, 0.0, 12.0, 0.0) };
                TiltEffect.SetIsTiltEnabled(text, true);

                var reportContent = new StackPanel();
                reportContent.Children.Add(new TextBlock { Text = AppResources.Description, Margin = new Thickness(12.0, 6.0, 12.0, -5.0), Style = (Style)Application.Current.Resources["PhoneTextSubtleStyle"] });
                reportContent.Children.Add(text);

                var confirmation = new CustomMessageBox
                {
                    Caption = AppResources.Report,
                    Message = string.Empty,
                    Content = reportContent,
                    RightButtonContent = AppResources.Cancel.ToLowerInvariant(),
                    LeftButtonContent = AppResources.Ok.ToLowerInvariant(),
                    IsLeftButtonEnabled = true,
                    IsRightButtonEnabled = true
                };

                text.Loaded += (o, e) =>
                {
                    confirmation.IsLeftButtonEnabled = false;
                    text.Focus();
                };
                text.TextChanged += (o, e) =>
                {
                    confirmation.IsLeftButtonEnabled = !string.IsNullOrEmpty(text.Text);
                };
#if WP8
                var isFullHD = Application.Current.Host.Content.ScaleFactor == 225;
                if (isFullHD || Environment.OSVersion.Version.Major >= 10)
                {
                    text.FontSize = 17.667;
                    confirmation.Style = (Style)Application.Current.Resources["CustomMessageBoxFullHDStyle"];
                }
#endif

                confirmation.Dismissed += (sender, args) =>
                {
                    switch (args.Result)
                    {
                        case CustomMessageBoxResult.RightButton:
                            break;
                        case CustomMessageBoxResult.LeftButton:
                            reason = new TLInputReportReasonOther { Text = new TLString(text.Text) };
                            ReportAsync(inputPeer, id, reason);
                            break;
                        case CustomMessageBoxResult.None:
                            // Do something.
                            break;
                        default:
                            break;
                    }
                };
                confirmation.Show();
            }
            else
            {
                ReportAsync(inputPeer, id, reason);
            }
        }

        private void ReportAsync(TLInputPeerBase inputPeer, TLVector<TLInt> id, TLInputReportReasonBase reason)
        {
            if (inputPeer == null) return;
            if (reason == null) return;

            if (id == null)
            {
                IsWorking = true;
                MTProtoService.ReportPeerAsync(inputPeer, reason,
                    result => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        MessageBox.Show(AppResources.ReportSpamNotification, AppResources.AppName, MessageBoxButton.OK);
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                    }));
            }
            else
            {
                IsWorking = true;
                MTProtoService.ReportAsync(inputPeer, id, reason,
                    result => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                        MessageBox.Show(AppResources.ReportSpamNotification, AppResources.AppName, MessageBoxButton.OK);
                    }),
                    error => Execute.BeginOnUIThread(() =>
                    {
                        IsWorking = false;
                    }));
            }
        }
    }
}

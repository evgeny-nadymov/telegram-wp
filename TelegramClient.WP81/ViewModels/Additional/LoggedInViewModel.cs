// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Resources;
using TelegramClient.Services;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class LoggedInViewModel : ItemsViewModelBase<TLWebAuthorization>
    {
        public bool IsEmptyList { get; protected set; }

        public string EmptyListImageSource
        {
            get
            {
                var isLightTheme = (Visibility)Application.Current.Resources["PhoneLightThemeVisibility"] == Visibility.Visible;

                if (isLightTheme)
                {
                    return "/Images/Settings/noapps.white-WXGA.png";
                }

                return "/Images/Settings/noapps.black-WXGA.png";
            }
        }

        private readonly DispatcherTimer _getAuthorizationsTimer = new DispatcherTimer();

        private void StartTimer()
        {
            _getAuthorizationsTimer.Start();
        }

        private void StopTimer()
        {
            _getAuthorizationsTimer.Stop();
        }

        public LoggedInViewModel(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator)
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            _getAuthorizationsTimer.Tick += OnGetAuthorizations;
            _getAuthorizationsTimer.Interval = TimeSpan.FromSeconds(10.0);

            EventAggregator.Subscribe(this);

            Status = AppResources.Loading;

            UpdateSessionsAsync();
        }

        private void OnGetAuthorizations(object sender, System.EventArgs e)
        {
            UpdateSessionsAsync();
        }

        protected override void OnActivate()
        {
            StartTimer();

            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            StopTimer();

            base.OnDeactivate(close);
        }

        private Dictionary<long, TLWebAuthorization> _authorizationsCache = new Dictionary<long, TLWebAuthorization>();

        private bool _firstRun = true;

        private void UpdateSessionsAsync()
        {
            BeginOnThreadPool(() =>
            {
                IsWorking = true;
                MTProtoService.GetWebAuthorizationsAsync(
                    result => Execute.BeginOnUIThread(() =>
                    {
                        Status = string.Empty;
                        IsEmptyList = result.Authorizations.Count == 0;
                        NotifyOfPropertyChange(() => IsEmptyList);
                        IsWorking = false;

                        if (_firstRun)
                        {
                            _firstRun = false;
                            Items.Clear();
                            var firstChunkSize = 4;
                            var count = 0;
                            var delayedItems = new List<TLWebAuthorization>();
                            for (var i = 0; i < result.Authorizations.Count; i++)
                            {
                                var authorization = result.Authorizations[i];
                                _authorizationsCache[authorization.Hash.Value] = authorization;
                                if (count < firstChunkSize)
                                {
                                    Items.Add(authorization);
                                    count++;
                                }
                                else
                                {
                                    delayedItems.Add(authorization);
                                }
                            }

                            BeginOnUIThread(TimeSpan.FromSeconds(0.5), () =>
                            {
                                foreach (var authorization in delayedItems)
                                {
                                    Items.Add(authorization);
                                }
                            });
                        }
                        else
                        {
                            var newAuthorizationsCache = new Dictionary<long, TLWebAuthorization>();
                            var itemsToAdd = new List<TLWebAuthorization>();
                            for (var i = 0; i < result.Authorizations.Count; i++)
                            {
                                var authorization = result.Authorizations[i];
                                TLWebAuthorization cachedAuthorization;
                                if (!_authorizationsCache.TryGetValue(authorization.Hash.Value, out cachedAuthorization))
                                {
                                    itemsToAdd.Add(authorization);
                                    newAuthorizationsCache[authorization.Hash.Value] = authorization;
                                }
                                else
                                {
                                    cachedAuthorization.Update(authorization);
                                    cachedAuthorization.NotifyOfPropertyChange(() => cachedAuthorization.Domain);
                                    cachedAuthorization.NotifyOfPropertyChange(() => cachedAuthorization.Browser);
                                    cachedAuthorization.NotifyOfPropertyChange(() => cachedAuthorization.DateActive);
                                    cachedAuthorization.NotifyOfPropertyChange(() => cachedAuthorization.Ip);
                                    cachedAuthorization.NotifyOfPropertyChange(() => cachedAuthorization.Region);

                                    newAuthorizationsCache[authorization.Hash.Value] = cachedAuthorization;
                                }
                            }

                            for (var i = 0; i < Items.Count; i++)
                            {
                                if (!newAuthorizationsCache.ContainsKey(Items[i].Hash.Value))
                                {
                                    Items.RemoveAt(i--);
                                }
                            }

                            for (var i = 0; i < itemsToAdd.Count; i++)
                            {
                                Items.Insert(0, itemsToAdd[i]);
                            }

                            _authorizationsCache = newAuthorizationsCache;
                        }

                    }),
                    error =>
                    {
                        Status = string.Empty;
                        IsEmptyList = Items.Count == 0;
                        NotifyOfPropertyChange(() => IsEmptyList);
                        IsWorking = false;
                        Execute.ShowDebugMessage("account.getWebAuthorizations error " + error);
                    });
            });
        }

        public void Terminate(TLWebAuthorization authorization)
        {
            if (authorization == null) return;

            IsWorking = true;
            MTProtoService.ResetWebAuthorizationAsync(
                authorization.Hash,
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Items.Remove(authorization);
                    IsEmptyList = Items.Count == 0;
                    _authorizationsCache.Remove(authorization.Hash.Value);
                    NotifyOfPropertyChange(() => IsEmptyList);
                }),
                error =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("account.resetWebAuthotization error " + error);
                });
        }

        public void TerminateWebAuthorizations()
        {
            var confirmation = MessageBox.Show(AppResources.LogOutAllApplicationsConfirmation, AppResources.Confirm, MessageBoxButton.OKCancel);
            if (confirmation != MessageBoxResult.OK)
            {
                return;
            }

            IsWorking = true;
            MTProtoService.ResetWebAuthorizationsAsync(
                result => BeginOnUIThread(() =>
                {
                    IsWorking = false;
                    Items.Clear();
                    IsEmptyList = Items.Count == 0;
                    _authorizationsCache.Clear();
                    NotifyOfPropertyChange(() => IsEmptyList);
                }),
                error =>
                {
                    IsWorking = false;
                    Execute.ShowDebugMessage("account.resetWebAuthotizations error " + error);
                });
        }

        public void Handle(TLUpdateNewAuthorization update)
        {
            UpdateSessionsAsync();
        }
    }
}

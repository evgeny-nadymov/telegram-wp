// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.TL;
using TelegramClient.Services;

namespace TelegramClient.ViewModels
{
    public abstract class ItemsViewModelBase : ViewModelBase
    {
        private string _status;

        public string Status
        {
            get { return _status; }
            set { SetField(ref _status, value, () => Status); }
        }

        private bool _isScrolling;

        public bool IsScrolling
        {
            get { return _isScrolling; }
            set { SetField(ref _isScrolling, value, () => IsScrolling); }
        }

        private bool _isLastSliceLoaded;

        protected bool IsLastSliceLoaded
        {
            get { return _isLastSliceLoaded; }
            set { _isLastSliceLoaded = value; }
        }

        protected ItemsViewModelBase(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {

        }

        protected virtual void OnPopulateCompleted()
        {
        
        }

        protected virtual void OnPopulateItemCompleted(object item)
        {

        }

        public virtual void RefreshItems()
        {

        }
    }

    public abstract class ItemsViewModelBase<T> : ItemsViewModelBase
    {


        #region Private fields

        private T _selectedItem;

        protected readonly ObservableCollection<T> LazyItems = new ObservableCollection<T>();
        #endregion

        #region Protected fields

        #endregion

        #region Public fields

        public ObservableCollection<T> Items { get; protected set; }

        public T SelectedItem
        {
            get { return _selectedItem; }
            set { SetField(ref _selectedItem, value, () => SelectedItem); }
        }
        #endregion

        private bool _populateToBegin;

        #region Constructor
        protected ItemsViewModelBase(ICacheService cacheService, ICommonErrorHandler errorHandler, IStateService stateService, INavigationService navigationService, IMTProtoService mtProtoService, ITelegramEventAggregator eventAggregator) 
            : base(cacheService, errorHandler, stateService, navigationService, mtProtoService, eventAggregator)
        {
            Items = new BindableCollection<T>();
            _timer.Tick += (s, e) =>
            {
                /*if (IsScrolling)
                {
                    _timer.Stop();
                    return;
                }*/

                if (LazyItems.Count <= 0)
                {

                    _populateToBegin = false;
                    _timer.Stop();
                    //OnPopulateCompleted();
                    return;
                }

                var addedItem = LazyItems.FirstOrDefault();
                if (addedItem != null)
                {
                    if (_populateToBegin)
                    {
                        Items.Insert(0, addedItem);
                    }
                    else
                    {
                        Items.Add(addedItem);
                    }
                    OnPopulateItemCompleted(addedItem);
                    LazyItems.RemoveAt(0);
                }
                addedItem = LazyItems.FirstOrDefault();
                if (addedItem != null)
                {
                    if (_populateToBegin)
                    {
                        Items.Insert(0, addedItem);
                    }
                    else
                    {
                        Items.Add(addedItem);
                    }
                    OnPopulateItemCompleted(addedItem);
                    LazyItems.RemoveAt(0);
                }
                
                if (LazyItems.Count <= 0)
                {
                    _populateToBegin = false;
                    _timer.Stop();
                    OnPopulateCompleted();
                }
            };
        }
        #endregion

        private readonly DispatcherTimer _timer = new DispatcherTimer { Interval = TimeSpan.FromTicks(20) };

        protected void PopulateItems()
        {
            _timer.Start();       
        }

        protected void PopulateItems(System.Action callback)
        {
            _populateCallback = callback;
            _timer.Start();
        }

        private System.Action _populateCallback;

        protected override void OnPopulateCompleted()
        {
            if (_populateCallback != null)
            {
                var action = _populateCallback;
                _populateCallback = null;
                action();
            }

            base.OnPopulateCompleted();
        }

        protected void PopulateItemsToBegin()
        {
            _populateToBegin = true;
            _timer.Start();
        }
    }
}

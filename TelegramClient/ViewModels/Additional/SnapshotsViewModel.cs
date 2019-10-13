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
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using Caliburn.Micro;
using Telegram.Api.Aggregator;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Updates;
using Telegram.Api.TL;
using Execute = Telegram.Api.Helpers.Execute;

namespace TelegramClient.ViewModels.Additional
{
    public class SnapshotsViewModel : TelegramPropertyChangedBase
    {
        public TLState State { get; set; }

        public TLVector<TLDifferenceBase> Difference { get; set; } 

        public IList<SnapshotItem> Items { get; set; }

        private readonly ICacheService _cacheService;

        private readonly IUpdatesService _updateService;

        private readonly ITelegramEventAggregator _eventAggregator;

        public SnapshotsViewModel(ICacheService cacheService, IUpdatesService updateService, ITelegramEventAggregator eventAggregator)
        {
            Items = new ObservableCollection<SnapshotItem>();

            _cacheService = cacheService;
            _updateService = updateService;
            _eventAggregator = eventAggregator;

            Execute.BeginOnThreadPool(() =>
            {
                try
                {
                    var currentState = _updateService.GetState();
                    State = currentState;
                    NotifyOfPropertyChange(() => State);

                    var tempDifference = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLDifferenceBase>>(new object(), Telegram.Api.Constants.DifferenceFileName);
                    Difference = tempDifference;
                    NotifyOfPropertyChange(() => Difference);

                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (store.DirectoryExists(Constants.SnapshotsDirectoryName))
                        {
                            foreach (var directory in store.GetDirectoryNames(Constants.SnapshotsDirectoryName + "/*"))
                            {
                                var item = new SnapshotItem {Name = directory};

                                var stateFullFileName = Path.Combine(Path.Combine(Constants.SnapshotsDirectoryName, directory), Telegram.Api.Constants.StateFileName);
                                if (store.FileExists(stateFullFileName))
                                {
                                    var state = TLUtils.OpenObjectFromMTProtoFile<TLState>(new object(), stateFullFileName);
                                    item.State = state;
                                }

                                var differenceFullFileName = Path.Combine(Path.Combine(Constants.SnapshotsDirectoryName, directory), Telegram.Api.Constants.DifferenceFileName);
                                if (store.FileExists(differenceFullFileName))
                                {
                                    long length;
                                    var difference = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLDifferenceBase>>(new object(), differenceFullFileName, out length);
                                    item.Difference = difference;
                                    item.DifferenceLength = length;
                                }

                                Execute.BeginOnUIThread(() => Items.Add(item));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            });
        }

        public void Create()
        {
            Execute.BeginOnThreadPool(() =>
            {
                var newSnapshotDirectoryName = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss.fff", CultureInfo.InvariantCulture);
                var newSnapshotDirectoryFullName = Path.Combine(Constants.SnapshotsDirectoryName, newSnapshotDirectoryName);
                var stateFullFileName = Path.Combine(newSnapshotDirectoryFullName, Telegram.Api.Constants.StateFileName);
                try
                {
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.DirectoryExists(Constants.SnapshotsDirectoryName))
                        {
                            store.CreateDirectory(Constants.SnapshotsDirectoryName);
                        }
                        store.CreateDirectory(newSnapshotDirectoryFullName);
                    }

                    _updateService.SaveStateSnapshot(newSnapshotDirectoryFullName);
                    _cacheService.SaveSnapshot(newSnapshotDirectoryFullName);

                    var state = TLUtils.OpenObjectFromMTProtoFile<TLState>(new object(), stateFullFileName);

                    Execute.ShowDebugMessage("Snapshot has been successfully created");
                    Execute.BeginOnUIThread(() => Items.Add(new SnapshotItem{Name = newSnapshotDirectoryName, State = state}));
                }
                catch (Exception ex)
                {
                    DeleteDirectory(newSnapshotDirectoryFullName);
                }
            });
        }

        public void CreateTemp()
        {
            Execute.BeginOnThreadPool(() =>
            {
                var newSnapshotDirectoryName = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss.fff", CultureInfo.InvariantCulture);
                var newSnapshotDirectoryFullName = Path.Combine(Constants.SnapshotsDirectoryName, newSnapshotDirectoryName);
                var stateFullFileName = Path.Combine(newSnapshotDirectoryFullName, Telegram.Api.Constants.StateFileName);
                var differenceFullFileName = Path.Combine(newSnapshotDirectoryFullName, Telegram.Api.Constants.DifferenceFileName);
                try
                {
                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (!store.DirectoryExists(Constants.SnapshotsDirectoryName))
                        {
                            store.CreateDirectory(Constants.SnapshotsDirectoryName);
                        }
                        store.CreateDirectory(newSnapshotDirectoryFullName);
                    }

                    _updateService.SaveStateSnapshot(newSnapshotDirectoryFullName);
                    _cacheService.SaveTempSnapshot(newSnapshotDirectoryFullName);

                    var state = TLUtils.OpenObjectFromMTProtoFile<TLState>(new object(), stateFullFileName);

                    long length;
                    var difference = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLDifferenceBase>>(new object(), differenceFullFileName, out length);

                    Execute.ShowDebugMessage("Snapshot has been successfully created");
                    Execute.BeginOnUIThread(() => Items.Add(new SnapshotItem { Name = newSnapshotDirectoryName, State = state, Difference = difference, DifferenceLength = length }));
                }
                catch (Exception ex)
                {
                    DeleteDirectory(newSnapshotDirectoryFullName);
                }
            });
        }

        public void Apply(SnapshotItem item)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var snapshotDirectoryName = item.Name;
                var snapshotDirectoryFullName = Path.Combine(Constants.SnapshotsDirectoryName, snapshotDirectoryName);
                var stateFullFileName = Path.Combine(snapshotDirectoryFullName, Telegram.Api.Constants.StateFileName);
                try
                {
                    _updateService.LoadStateSnapshot(snapshotDirectoryFullName);
                    _cacheService.LoadSnapshot(snapshotDirectoryFullName);

                    var currentState = _updateService.GetState();
                    State = currentState;
                    NotifyOfPropertyChange(() => State);

                    Difference = TLUtils.OpenObjectFromMTProtoFile<TLVector<TLDifferenceBase>>(new object(), Telegram.Api.Constants.DifferenceFileName);
                    NotifyOfPropertyChange(() => Difference);

                    _eventAggregator.Publish(new UpdateCompletedEventArgs());

                    Execute.ShowDebugMessage("Snapshot has been successfully applied");
                }
                catch (Exception ex)
                {
                    
                }
            });
        }

        public void Delete(SnapshotItem item)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var deletingSnapshotDirectoryName = item.Name;
                var deletingSnapshotDirectoryFullName = Path.Combine(Constants.SnapshotsDirectoryName, deletingSnapshotDirectoryName);

                DeleteDirectory(deletingSnapshotDirectoryFullName);

                Execute.BeginOnUIThread(() => Items.Remove(item));
            });
        }

        private static void DeleteDirectory(string directory)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.DirectoryExists(directory))
                {
                    foreach (var fileName in store.GetFileNames(directory + "/*"))
                    {
                        store.DeleteFile(Path.Combine(directory, fileName));
                    }
                    store.DeleteDirectory(directory);
                }
            }
        }
    }

    public class SnapshotItem
    {
        public string Name { get; set; }

        public TLState State { get; set; }

        public TLVector<TLDifferenceBase> Difference { get; set; }

        public long DifferenceLength { get; set; }

        public string DifferenceString
        {
            get
            {
                if (Difference == null) return null;

                return string.Format("{0} {1} bytes", Difference.Count, DifferenceLength);
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

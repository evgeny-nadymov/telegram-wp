// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Threading;
using Telegram.Api.Aggregator;
using Telegram.Api.Extensions;
using Telegram.Api.Helpers;
using Telegram.Api.Services.FileManager;
using Telegram.Api.TL;

namespace TelegramClient.Services
{
    public interface IHttpDocumentFileManager
    {
        void DownloadFileAsync(string sourceUri, string destFileName, TLObject owner, Action<DownloadableItem> callback, Action<DownloadableItem> faultCallback = null);
        void CancelAsync(TLObject owner);
    }

    public class HttpDocumentFileManager : IHttpDocumentFileManager
    {
        private readonly List<Worker> _workers = new List<Worker>(Telegram.Api.Constants.WorkersNumber);

        private readonly object _itemsSyncRoot = new object();

        private readonly List<DownloadableItem> _items = new List<DownloadableItem>();

        private readonly ITelegramEventAggregator _eventAggregator;

        public HttpDocumentFileManager(ITelegramEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            for (var i = 0; i < 1; i++)
            {
                var worker = new Worker(OnDownloading, "httpDocumentDownloader" + i);
                _workers.Add(worker);
            }
        }

        private void OnDownloading(object state)
        {
            DownloadablePart part = null;
            lock (_itemsSyncRoot)
            {
                for (var i = 0; i < _items.Count; i++)
                {
                    var item = _items[i];
                    if (item.Canceled)
                    {
                        _items.RemoveAt(i--);
                    }
                }

                foreach (var item in _items)
                {
                    part = item.Parts.FirstOrDefault(x => x.Status == PartStatus.Ready);
                    if (part != null)
                    {
                        part.Status = PartStatus.Processing;
                        break;
                    }
                }
            }

            if (part == null)
            {
                var currentWorker = (Worker)state;
                currentWorker.Stop();
                return;
            }


            var fileName = part.ParentItem.DestFileName;
            var manualResetEvent = new ManualResetEvent(false);
            long? length = null;

            var webClient = new WebClient();
            webClient.OpenReadAsync(new Uri(part.ParentItem.SourceUri, UriKind.Absolute));
            webClient.OpenReadCompleted += (sender, args) =>
            {
                if (args.Cancelled)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(part.ParentItem.Timeout));
                    part.ParentItem.IncreaseTimeout();
                    part.Status = PartStatus.Ready;
                    manualResetEvent.Set();

                    return;
                }

                if (args.Error != null)
                {
                    var webException = args.Error as WebException;
                    if (webException != null)
                    {
                        var response = webException.Response as HttpWebResponse;
                        if (response != null)
                        {
                            if (response.StatusCode == HttpStatusCode.Forbidden 
                                || response.StatusCode == HttpStatusCode.NotFound)
                            {
                                part.HttpErrorsCount++;
                                if (part.HttpErrorsCount >= 5)
                                {
                                    lock (_itemsSyncRoot)
                                    {
                                        part.ParentItem.Canceled = true;
                                    }
                                }
                                else
                                {
                                    Thread.Sleep(TimeSpan.FromSeconds(part.ParentItem.Timeout));
                                    part.ParentItem.IncreaseTimeout();
                                    part.Status = PartStatus.Ready;

                                }
                            }
                            else
                            {
                                lock (_itemsSyncRoot)
                                {
                                    part.ParentItem.Canceled = true;
                                }
                            }
                        }
                    }

                    manualResetEvent.Set();
                    return;
                }

                try
                {
                    length = args.Result.Length;
                    using (var stream = args.Result)
                    {
                        using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            using (var file = store.OpenFile(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                            {
                                if (store.FileExists(fileName))
                                {
                                    //if (file.Position > file.Position)
                                    //{
                                    //    stream.Seek(file.Position, SeekOrigin.Begin);
                                    //}
                                    //else
                                    {
                                        file.Seek(0, SeekOrigin.Begin);
                                        stream.Seek(0, SeekOrigin.Begin);
                                    }
                                }

                                const int BUFFER_SIZE = 128 * 1024;
                                var buf = new byte[BUFFER_SIZE];

                                int bytesRead;
                                while ((bytesRead = stream.Read(buf, 0, BUFFER_SIZE)) > 0)
                                {
                                    file.Seek(0, SeekOrigin.End);
                                    file.Write(buf, 0, bytesRead);

                                    lock (_itemsSyncRoot)
                                    {
                                        if (part.ParentItem.Canceled)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    manualResetEvent.Set();
                }
                catch (Exception ex)
                {
                    manualResetEvent.Set();
                }
            };

            manualResetEvent.WaitOne();

            // indicate progress
            // indicate complete
            bool isComplete;
            bool isCanceled;
            lock (_itemsSyncRoot)
            {
                long? fileLength = null;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(fileName))
                    {
                        using (var file = store.OpenFile(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            fileLength = file.Length;
                        }
                    }
                }

                isCanceled = part.ParentItem.Canceled;
                isComplete = length.HasValue && fileLength.HasValue && fileLength == length;

                if (isComplete)
                {
                    part.Status = PartStatus.Processed;
                    _items.Remove(part.ParentItem);
                }
            }

            if (!isCanceled)
            {
                if (isComplete)
                {
                    part.ParentItem.IsoFileName = fileName;
                    if (part.ParentItem.Callback != null)
                    {
                        Execute.BeginOnThreadPool(() =>
                        {
                            part.ParentItem.Callback(part.ParentItem);
                            if (part.ParentItem.Callbacks != null)
                            {
                                foreach (var callback in part.ParentItem.Callbacks)
                                {
                                    callback.SafeInvoke(part.ParentItem);
                                }
                            }
                        });
                    }
                }
            }
            else
            {
                if (part.ParentItem.FaultCallback != null)
                {
                    Execute.BeginOnThreadPool(() =>
                    {
                        part.ParentItem.FaultCallback(part.ParentItem);
                        if (part.ParentItem.FaultCallbacks != null)
                        {
                            foreach (var callback in part.ParentItem.FaultCallbacks)
                            {
                                callback.SafeInvoke(part.ParentItem);
                            }
                        }
                    });
                }
            }
        }

        public void DownloadFileAsync(string sourceUri, string destFileName, TLObject owner, Action<DownloadableItem> callback, Action<DownloadableItem> faultCallback = null)
        {
            Execute.BeginOnThreadPool(() =>
            {
                var downloadableItem = GetDownloadableItem(sourceUri, destFileName, owner, callback, faultCallback);

                lock (_itemsSyncRoot)
                {
                    var addFile = true;
                    foreach (var item in _items)
                    {
                        if (item.SourceUri == sourceUri)
                        {
                            //if (item.Owner == owner)
                            //{
                            //    addFile = false;
                            //    break;
                            //}

                            if (callback != null)
                            {
                                if (item.Callbacks == null)
                                {
                                    item.Callbacks = new List<Action<DownloadableItem>>();
                                }
                                item.Callbacks.Add(callback);
                            }

                            if (faultCallback != null)
                            {
                                if (item.FaultCallbacks == null)
                                {
                                    item.FaultCallbacks = new List<Action<DownloadableItem>>();
                                }
                                item.FaultCallbacks.Add(faultCallback);
                            }

                            addFile = false;
                            break;
                        }
                    }

                    if (addFile)
                    {
                        _items.Add(downloadableItem);
                    }
                }

                StartAwaitingWorkers();
            });
        }

        private DownloadableItem GetDownloadableItem(string sourceUri, string destFileName, TLObject owner, Action<DownloadableItem> callback, Action<DownloadableItem> faultCallback)
        {
            var item = new DownloadableItem
            {
                SourceUri = sourceUri,
                DestFileName = destFileName,
                Owner = owner,
                Callback = callback,
                FaultCallback = faultCallback,
                Timeout = 4.0
            };
            item.Parts = GetItemParts(item);

            return item;
        }

        private void StartAwaitingWorkers()
        {
            var awaitingWorkers = _workers.Where(x => x.IsWaiting);

            foreach (var awaitingWorker in awaitingWorkers)
            {
                awaitingWorker.Start();
            }
        }

        private List<DownloadablePart> GetItemParts(DownloadableItem item)
        {
            return new List<DownloadablePart> { new DownloadablePart(item, new TLInt(0), new TLInt(0), 0) };
        }

        public void CancelAsync(TLObject owner)
        {
            Execute.BeginOnThreadPool(() =>
            {
                lock (_itemsSyncRoot)
                {
                    var items = _items.Where(x => x.Owner == owner);

                    foreach (var item in items)
                    {
                        item.Canceled = true;
                    }
                }
            });
        }
    }
}

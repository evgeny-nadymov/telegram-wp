// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Linq;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.TL;

namespace Telegram.Api.Services.FileManager
{
    public class AudioFileManager : FileManagerBase, IAudioFileManager
    {
        public AudioFileManager(ITelegramEventAggregator eventAggregator, IMTProtoService mtProtoService) : base(eventAggregator, mtProtoService)
        {
            for (var i = 0; i < Constants.AudioDownloadersCount; i++)
            {
                var worker = new Worker(OnDownloading, "audioDownloader" + i);
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
                        try
                        {
                            //_eventAggregator.Publish(new UploadingCanceledEventArgs(item));
                        }
                        catch (Exception e)
                        {
                            TLUtils.WriteException(e);
                        }
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

            var partName = part.ParentItem.InputLocation.GetPartFileName(part.Number, "audio");
            var isLastPart = part.Number + 1 == part.ParentItem.Parts.Count;
            var partLength = FileUtils.GetLocalFileLength(partName);
            var partExists = partLength > 0;
            var isCorrectPartLength = isLastPart || partLength == part.Limit.Value;

            if (!partExists || !isCorrectPartLength)
            {
                bool canceled;
                ProcessFilePart(part, part.ParentItem.DCId, part.ParentItem.InputLocation, out canceled);
                if (canceled)
                {
                    lock (_itemsSyncRoot)
                    {
                        part.ParentItem.Canceled = true;
                        part.Status = PartStatus.Processed;
                        _items.Remove(part.ParentItem);
                    }

                    return;
                }

                //part.File = GetFile(part.ParentItem.DCId, (TLInputFileLocationBase) part.ParentItem.InputAudioLocation, part.Offset, part.Limit, out error, out canceled);
                //while (part.File == null)
                //{
                //    part.File = GetFile(part.ParentItem.DCId, (TLInputFileLocationBase) part.ParentItem.InputAudioLocation, part.Offset, part.Limit, out error, out canceled);
                //}
            }

            // indicate progress
            // indicate complete
            bool isComplete;
            bool isCanceled;
            var progress = 0.0;
            lock (_itemsSyncRoot)
            {
                part.Status = PartStatus.Processed;

                FileUtils.CheckMissingPart(_itemsSyncRoot, part, partName);

                isCanceled = part.ParentItem.Canceled;

                isComplete = part.ParentItem.Parts.All(x => x.Status == PartStatus.Processed);
                if (!isComplete)
                {
                    var downloadedCount = part.ParentItem.Parts.Count(x => x.Status == PartStatus.Processed);
                    var count = part.ParentItem.Parts.Count;
                    progress = downloadedCount / (double)count;
                }
                else
                {
                    _items.Remove(part.ParentItem);
                }
            }

            if (!isCanceled)
            {
                if (isComplete)
                {
                    var fileName = part.ParentItem.InputLocation.GetFileName("audio", ".mp3");
                    var getPartName = new Func<DownloadablePart, string>(x => x.ParentItem.InputLocation.GetPartFileName(x.Number, "audio"));

                    FileUtils.MergePartsToFile(getPartName, part.ParentItem.Parts, fileName);

                    part.ParentItem.IsoFileName = fileName;
                    if (part.ParentItem.Callback != null)
                    {
                        Execute.BeginOnThreadPool(() => part.ParentItem.Callback(part.ParentItem));
                    }
                    else
                    {
                        Execute.BeginOnThreadPool(() => _eventAggregator.Publish(part.ParentItem));
                    }
                }
                else
                {
                    //Execute.BeginOnThreadPool(() => _eventAggregator.Publish(new ProgressChangedEventArgs(part.ParentItem, progress)));
                }
            }
        }

        public void DownloadFile(TLInt dcId, TLInputFileLocationBase fileLocation, TLObject owner, TLInt fileSize, Action<DownloadableItem> callback)
        {
            var downloadableItem = GetDownloadableItem(dcId, fileLocation, owner, fileSize, callback);

            var downloadedCount = downloadableItem.Parts.Count(x => x.Status == PartStatus.Processed);
            var count = downloadableItem.Parts.Count;
            var isComplete = downloadedCount == count;

            if (isComplete)
            {
                var fileName = downloadableItem.InputLocation.GetFileName("audio", ".mp3");
                var getPartName = new Func<DownloadablePart, string>(x => x.ParentItem.InputLocation.GetPartFileName(x.Number, "audio"));

                FileUtils.MergePartsToFile(getPartName, downloadableItem.Parts, fileName);

                downloadableItem.IsoFileName = fileName;
                _eventAggregator.Publish(downloadableItem);
            }
            else
            {
                lock (_itemsSyncRoot)
                {
                    bool addFile = true;
                    foreach (var item in _items)
                    {
                        if (item.InputLocation.LocationEquals(fileLocation)
                            && item.Owner == owner)
                        {
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
            }
        }

        private DownloadableItem GetDownloadableItem(TLInt dcId, TLInputFileLocationBase location, TLObject owner, TLInt fileSize, Action<DownloadableItem> callback)
        {
            var item = new DownloadableItem
            {
                Owner = owner,
                DCId = dcId,
                InputLocation = location,
                Callback = callback,
            };
            item.Parts = GetItemParts(fileSize, item);

            return item;
        }
    }
}

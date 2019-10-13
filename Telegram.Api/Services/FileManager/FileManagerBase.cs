// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telegram.Api.Aggregator;
using Telegram.Api.Helpers;
using Telegram.Api.TL;

namespace Telegram.Api.Services.FileManager
{
    public abstract class FileManagerBase
    {
        private readonly object _randomRoot = new object();

        private readonly Random _random = new Random();

        protected readonly object _itemsSyncRoot = new object();

        protected readonly List<Worker> _workers = new List<Worker>(Constants.WorkersNumber);

        protected readonly List<DownloadableItem> _items = new List<DownloadableItem>();

        protected readonly IMTProtoService _mtProtoService;

        protected readonly ITelegramEventAggregator _eventAggregator;

        protected FileManagerBase(ITelegramEventAggregator eventAggregator, IMTProtoService mtProtoService)
        {
            _mtProtoService = mtProtoService;
            _eventAggregator = eventAggregator;
        }

        protected void ProcessFilePart(DownloadablePart part, TLInt dcId, TLInputFileLocationBase location, out bool canceled)
        {
            do
            {
                TLRPCError error;

                TLFileBase result;
                if (part.ParentItem.CdnRedirect != null)
                {
                    TLCdnFileReuploadNeeded cdnFileReuploadNeeded;
                    bool tokenInvalid;
                    result = GetCdnFile(part.ParentItem.CdnRedirect, part.Offset, part.Limit, out cdnFileReuploadNeeded, out error, out canceled, out tokenInvalid);
                    if (cdnFileReuploadNeeded != null)
                    {
                        ReuploadFile(part.ParentItem.CdnRedirect, dcId, cdnFileReuploadNeeded.RequestToken, out error, out canceled, out tokenInvalid);
                    }

                    if (tokenInvalid)
                    {
                        lock (_itemsSyncRoot)
                        {
                            part.ParentItem.CdnRedirect = null;
                        }
                        continue;
                    }
                }
                else
                {
                    result = GetFile(dcId, location, part.Offset, part.Limit, out error, out canceled);
                    var fileCdnRedirect = result as TLFileCdnRedirect;
                    if (fileCdnRedirect != null)
                    {
                        lock (_itemsSyncRoot)
                        {
                            part.ParentItem.CdnRedirect = fileCdnRedirect;
                        }
                        continue;
                    }
                }

                part.File = result as TLFile;

                if (canceled)
                {
                    return;
                }
            } while (part.File == null);
        }

        protected TLFileBase GetFile(TLInt dcId, TLInputFileLocationBase location, TLInt offset, TLInt limit, out TLRPCError er, out bool isCanceled)
        {
            var manualResetEvent = new ManualResetEvent(false);
            TLFileBase result = null;
            TLRPCError outError = null;
            var outIsCanceled = false;
            _mtProtoService.GetFileAsync(dcId, location, offset, limit,
                file =>
                {
                    result = file;
                    manualResetEvent.Set();
                },
                error =>
                {
                    outError = error;

                    if (error.CodeEquals(ErrorCode.INTERNAL)
                        || (error.CodeEquals(ErrorCode.BAD_REQUEST) && (error.TypeEquals(ErrorType.LOCATION_INVALID) || error.TypeEquals(ErrorType.VOLUME_LOC_NOT_FOUND)))
                        || (error.CodeEquals(ErrorCode.NOT_FOUND) && error.Message != null && error.Message.ToString().StartsWith("Incorrect dhGen")))
                    {
                        outIsCanceled = true;

                        manualResetEvent.Set();
                        return;
                    }

                    int delay;
                    lock (_randomRoot)
                    {
                        delay = _random.Next(1000, 3000);
                    }

                    Execute.BeginOnThreadPool(TimeSpan.FromMilliseconds(delay), () => manualResetEvent.Set());
                });

            manualResetEvent.WaitOne();
            er = outError;
            isCanceled = outIsCanceled;

            return result;
        }

        protected TLVector<TLFileHash> ReuploadFile(TLFileCdnRedirect redirect, TLInt dcId, TLString requestToken, out TLRPCError er, out bool isCanceled, out bool isTokenInvalid)
        {
            var manualResetEvent = new ManualResetEvent(false);
            TLVector<TLFileHash> result = null;
            TLRPCError outError = null;
            var outIsCanceled = false;
            var outIsTokenInvalid = false;

            _mtProtoService.ReuploadCdnFileAsync(dcId, redirect.FileToken, requestToken,
                callback =>
                {
                    result = callback;

                    manualResetEvent.Set();
                },
                error =>
                {
                    outError = error;

                    if (error.CodeEquals(ErrorCode.INTERNAL)
                        || (error.CodeEquals(ErrorCode.BAD_REQUEST) && (error.TypeEquals(ErrorType.LOCATION_INVALID) || error.TypeEquals(ErrorType.VOLUME_LOC_NOT_FOUND)))
                        || (error.CodeEquals(ErrorCode.NOT_FOUND) && error.Message != null && error.Message.ToString().StartsWith("Incorrect dhGen")))
                    {
                        outIsCanceled = true;

                        manualResetEvent.Set();
                        return;
                    }
                    if (error.CodeEquals(ErrorCode.BAD_REQUEST) && (error.TypeEquals(ErrorType.FILE_TOKEN_INVALID) || error.TypeEquals(ErrorType.REQUEST_TOKEN_INVALID)))
                    {
                        outIsTokenInvalid = true;

                        manualResetEvent.Set();
                        return;
                    }

                    int delay;
                    lock (_randomRoot)
                    {
                        delay = _random.Next(1000, 3000);
                    }

                    Execute.BeginOnThreadPool(TimeSpan.FromMilliseconds(delay), () => manualResetEvent.Set());
                });

            manualResetEvent.WaitOne();
            er = outError;
            isCanceled = outIsCanceled;
            isTokenInvalid = outIsTokenInvalid;

            return result;
        }

        protected byte[] GetIV(byte[] ivec, TLInt offset)
        {
            var iv = new byte[ivec.Length];
            Buffer.BlockCopy(ivec, 0, iv, 0, ivec.Length);

            Array.Reverse(iv);
            var bi = new System.Numerics.BigInteger(TLUtils.Combine(iv, new byte[] { 0x00 }));
            bi = (bi + offset.Value/16);
            var biArray = bi.ToByteArray();
            var b = new byte[16];
            Buffer.BlockCopy(biArray, 0, b, 0, Math.Min(b.Length, biArray.Length));

            Array.Reverse(b);

            return b;
        }

        protected TLFileBase GetCdnFile(TLFileCdnRedirect redirect, TLInt offset, TLInt limit, out TLCdnFileReuploadNeeded reuploadNeeded, out TLRPCError er, out bool isCanceled, out bool isTokenInvalid)
        {
            var manualResetEvent = new ManualResetEvent(false);
            TLFileBase result = null;
            TLCdnFileReuploadNeeded outReuploadNeeded = null;
            TLRPCError outError = null;
            var outIsCanceled = false;
            var outIsTokenInvalid = false;

            _mtProtoService.GetCdnFileAsync(redirect.DCId, redirect.FileToken, offset, limit,
                cdnFileBase =>
                {
                    var cdnFile = cdnFileBase as TLCdnFile;
                    if (cdnFile != null)
                    {
                        var iv = GetIV(redirect.EncryptionIV.Data, offset);
                        var counter = offset.Value / 16;
                        iv[15] = (byte)(counter & 0xFF);
                        iv[14] = (byte)((counter >> 8) & 0xFF);
                        iv[13] = (byte)((counter >> 16) & 0xFF);
                        iv[12] = (byte)((counter >> 24) & 0xFF);

                        var key = redirect.EncryptionKey.Data;

                        var ecount_buf = new byte[0];
                        var num = 0u;
                        var bytes = Utils.AES_ctr128_encrypt(cdnFile.Bytes.Data, key, ref iv, ref ecount_buf, ref num);

                        result = new TLFile { Bytes = TLString.FromBigEndianData(bytes) };
                    }

                    var cdnFileReuploadNeeded = cdnFileBase as TLCdnFileReuploadNeeded;
                    if (cdnFileReuploadNeeded != null)
                    {
                        outReuploadNeeded = cdnFileReuploadNeeded;
                    }

                    manualResetEvent.Set();
                },
                error =>
                {
                    outError = error;

                    if (error.CodeEquals(ErrorCode.INTERNAL)
                        || (error.CodeEquals(ErrorCode.BAD_REQUEST) && (error.TypeEquals(ErrorType.LOCATION_INVALID) || error.TypeEquals(ErrorType.VOLUME_LOC_NOT_FOUND)))
                        || (error.CodeEquals(ErrorCode.NOT_FOUND) && error.Message != null && error.Message.ToString().StartsWith("Incorrect dhGen")))
                    {
                        outIsCanceled = true;

                        manualResetEvent.Set();
                        return;
                    }
                    if (error.CodeEquals(ErrorCode.BAD_REQUEST) && error.TypeEquals(ErrorType.FILE_TOKEN_INVALID))
                    {
                        outIsTokenInvalid = true;

                        manualResetEvent.Set();
                        return;
                    }

                    int delay;
                    lock (_randomRoot)
                    {
                        delay = _random.Next(1000, 3000);
                    }

                    Execute.BeginOnThreadPool(TimeSpan.FromMilliseconds(delay), () => manualResetEvent.Set());
                });

            manualResetEvent.WaitOne();
            reuploadNeeded = outReuploadNeeded;
            er = outError;
            isCanceled = outIsCanceled;
            isTokenInvalid = outIsTokenInvalid;

            return result;
        }

        protected virtual List<DownloadablePart> GetItemParts(TLInt size, DownloadableItem item)
        {
            var chunkSize = size.Value > 1024 * 1024? Constants.DownloadedBigChunkSize : Constants.DownloadedChunkSize;
            var parts = new List<DownloadablePart>();
            var partsCount = size.Value / chunkSize + ((size.Value % chunkSize > 0 || size.Value == 0) ? 1 : 0);
            for (var i = 0; i < partsCount; i++)
            {
                var part = new DownloadablePart(item, new TLInt(i * chunkSize), size.Value == 0 ? new TLInt(1024 * 1024) : new TLInt(chunkSize), i);
                parts.Add(part);
            }

            return parts;
        }

        public void CancelDownloadFile(TLObject owner)
        {
            lock (_itemsSyncRoot)
            {
                var items = _items.Where(x => x.Owner == owner);

                foreach (var item in items)
                {
                    item.Canceled = true;
                }
            }
        }

        public void CancelDownloadFileAsync(TLObject owner)
        {
            Execute.BeginOnThreadPool(() => CancelDownloadFile(owner));
        }

        protected void StartAwaitingWorkers()
        {
            var awaitingWorkers = _workers.Where(x => x.IsWaiting);

            foreach (var awaitingWorker in awaitingWorkers)
            {
                awaitingWorker.Start();
            }
        }
    }
}

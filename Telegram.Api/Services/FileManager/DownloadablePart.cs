// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Telegram.Api.TL;

namespace Telegram.Api.Services.FileManager
{
    public enum PartStatus
    {
        Ready,
        Processing,
        Processed,
    }

    public class DownloadablePart
    {
        public int Number { get; protected set; }

        public DownloadableItem ParentItem { get; protected set; }

        public TLInt Offset { get; protected set; }

        public TLInt Limit { get; protected set; }

        public PartStatus Status { get; set; }

        public TLFile File { get; set; }

        public int HttpErrorsCount { get; set; }

        public bool NotifyProgress { get; set; }

        public DownloadablePart(DownloadableItem item, TLInt offset, TLInt limit, int number)
        {
            ParentItem = item;
            Offset = offset;
            Limit = limit;
            Number = number;
            NotifyProgress = true;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Number, Offset, Limit);
        }
    }
}
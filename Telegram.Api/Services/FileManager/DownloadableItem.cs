// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.Collections.Generic;
using Telegram.Api.TL;

namespace Telegram.Api.Services.FileManager
{
    public class DownloadableItem
    {
        public TLInt DCId { get; set; }

        public TLString FileName { get; set; }

        public TLObject Owner { get; set; }

        public System.Action<DownloadableItem> Callback { get; set; }

        public IList<System.Action<DownloadableItem>> Callbacks { get; set; }

        public TLInputFileLocationBase InputLocation { get; set; }

        public List<DownloadablePart> Parts { get; set; }

        public string IsoFileName { get; set; }

        public bool Canceled { get; set; }

        public bool SuppressMerge { get; set; }

        public TLFileCdnRedirect CdnRedirect { get; set; }

        #region Http

        public string SourceUri { get; set; }

        public string DestFileName { get; set; }

        public System.Action<DownloadableItem> FaultCallback { get; set; }

        public IList<System.Action<DownloadableItem>> FaultCallbacks { get; set; }

        public double Timeout { get; set; }

        public void IncreaseTimeout()
        {
            Timeout = Timeout * 2.0;
            if (Timeout == 0.0)
            {
                Timeout = 4.0;
            }
            if (Timeout >= 32.0)
            {
                Timeout = 4.0;
            }
        }
        #endregion
    }
}
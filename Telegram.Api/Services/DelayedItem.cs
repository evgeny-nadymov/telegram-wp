// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Text;
using Telegram.Api.TL;

namespace Telegram.Api.Services
{
    class DelayedItem
    {
        public string Caption { get; set; }
        public DateTime SendTime { get; set; }
        //public DateTime? SendBeforeTime { get; set; }
        public TLObject Object { get; set; }
        public Action<TLObject> Callback { get; set; }
        public Action<TLRPCError> FaultCallback { get; set; }
        public Action<int> AttemptFailed { get; set; }
        public int? MaxAttempt { get; set; }
        public int CurrentAttempt { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("DelayedItem");
            sb.AppendLine("Caption " + Caption);
            sb.AppendLine("MaxAttempt " + MaxAttempt);
            sb.AppendLine("CurrentAttempt " + CurrentAttempt);

            return sb.ToString();
        }
    }
}

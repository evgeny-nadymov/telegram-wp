// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using Telegram.Api.TL;

namespace Telegram.Api.Helpers
{
    public static class Notifications
    {
        private static readonly object _notificatonsSyncRoot = new object();

        public static bool IsDisabled
        {
            get
            {
                var result = TLUtils.OpenObjectFromMTProtoFile<TLBool>(_notificatonsSyncRoot, Constants.DisableNotificationsFileName);

                return result != null;
            }
        }

        public static void Disable()
        {
            TLUtils.SaveObjectToMTProtoFile(_notificatonsSyncRoot, Constants.DisableNotificationsFileName, TLBool.True);
        }

        public static void Enable()
        {
            FileUtils.Delete(_notificatonsSyncRoot, Constants.DisableNotificationsFileName);
        }
    }
}

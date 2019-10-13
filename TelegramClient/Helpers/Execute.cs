using System;
using System.Threading;
using System.Windows;
using Telegram.Api.Extensions;
using Telegram.Api.TL;

namespace TelegramClient.Helpers
{
    public static class Execute
    {
        public static void BeginOnThreadPool(Action action)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    action.SafeInvoke();
                }
                catch (Exception ex)
                {
                    TLUtils.WriteException(ex);
                }
            });
        }

        public static void BeginOnUIThread(Action action)
        {
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                try
                {
                    action.SafeInvoke();
                }
                catch (Exception ex)
                {
                    TLUtils.WriteException(ex);
                }
            });
        }

        public static void ShowDebugMessage(string message)
        {
#if DEBUG
            BeginOnUIThread(() => MessageBox.Show(message));
#endif
        }
    }
}

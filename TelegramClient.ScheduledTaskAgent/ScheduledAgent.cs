using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;

namespace TelegramClient.ScheduledTaskAgent
{
    public class ScheduledAgent : Microsoft.Phone.Scheduler.ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
//            var manualResetEvent = new ManualResetEvent(false);
//            var eventAggregator = new EventAggregator();
//            var cacheService = new InMemoryCacheService(eventAggregator);
//            var updatesService = new UpdatesService(cacheService, eventAggregator);
//            //TODO: Add code to perform your task in background
//            if (ShellTile.ActiveTiles.FirstOrDefault() == null) return;

//            ShellTileData tileData;
//#if WP8
//            tileData = new StandardTileData { Count = 99 };
//            //tileDate = new IconicTileData { Count = 99, WideContent1 = "", WideContent2 = "", WideContent3 = "" };
//#else
//            tileData = new StandardTileData { BackTitle = DateTime.Now.ToString("dd-MMM HH:mm") };
//#endif
//            try
//            {
//                var tile = ShellTile.ActiveTiles.FirstOrDefault();
//                if (tile != null)
//                { 
//                    tile.Update(tileData);
//                }
//            }
//            catch (Exception e)
//            {

//            }



//            try
//            {
//                //var isAuthorized = SettingsHelper.GetValue<bool>(Constants.IsAuthorizedKey);
//                //if (!isAuthorized) return;

//                var mtProtoService = new MTProtoService(updatesService, cacheService, new TransportService());
//                //var stateService = IoC.Get<IStateService>();

//                //TLUtils.WritePerformance(">>OnActivate GetDifferenceAsync");
//                updatesService.GetCurrentUserId = () => mtProtoService.CurrentUserId;
//                updatesService.GetStateAsync = mtProtoService.GetStateAsync;
//                updatesService.GetDHConfigAsync = mtProtoService.GetDHConfigAsync;
//                updatesService.GetDifferenceAsync = mtProtoService.GetDifferenceAsync;
//                updatesService.AcceptEncryptionAsync = mtProtoService.AcceptEncryptionAsync;
//                //stateService.SuppressNotifications = true;

//                var timer = Stopwatch.StartNew();
//                updatesService.LoadStateAndUpdate(false, () =>
//                {
//                    //TLUtils.WritePerformance("::GetDifference time: " + timer.Elapsed);
//                    //stateService.SuppressNotifications = false;
//                });
//            }
//            catch (Exception e)
//            {
//                //TLUtils.WriteException(e);
//            }

//            manualResetEvent.WaitOne(20000);
            NotifyComplete();
        }
    }
}
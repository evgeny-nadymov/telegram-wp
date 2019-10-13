using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
//using Caliburn.Micro;
using Mangopollo.Tiles;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Telegram.Api;
using Telegram.Api.Helpers;
using Telegram.Api.Services;
using Telegram.Api.Services.Cache;
using Telegram.Api.Services.Updates;
using Telegram.Api.Transport;

namespace Telegram.Client.TileUpdated
{
    public class ScheduledAgent : ScheduledTaskAgent
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
            /*var manualResetEvent = new ManualResetEvent(false);
            var eventAggregator = new EventAggregator();
            var cacheService = new InMemoryCacheService(eventAggregator);
            var updatesService = new UpdatesService(cacheService, eventAggregator);
            //TODO: Add code to perform your task in background
            var mtProtoService = new MTProtoService(updatesService, cacheService, new TransportService());
            mtProtoService.GetStateAsync(
                state =>
                {

                    var unreadCount = state.UnreadCount.Value;

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        //var shellTileData = Mangopollo.Utils.CanUseLiveTiles
                        //    ? (ShellTileData)new IconicTileData { Count = unreadCount }
                        //    : new StandardTileData {Count = unreadCount};

                        var shellTileData = new StandardTileData { Count = unreadCount };

                        var tile = ShellTile.ActiveTiles.FirstOrDefault();
                        if (tile != null)
                        {
                            tile.Update(shellTileData);
                        }

                        //if (previousUnreadCount < unreadCount)
                        //{
                        //    //_previousUnreadCount = unreadCount;
                        //    var toast = new ShellToast
                        //    {
                        //        Content = string.Format("Previous - {0}, current - {1}", previousUnreadCount, unreadCount),
                        //        Title = "Telegram",
                        //        NavigationUri = new System.Uri("/Views/ShellView.xaml", System.UriKind.Relative)
                        //    };

                        //    settings["UnreadCountKey"] = unreadCount;
                        //    toast.Show();
                        //}
                        

                        manualResetEvent.Set();
                    });

                },
                error =>
                {
                    manualResetEvent.Set();
                });

            manualResetEvent.WaitOne(20000);*/
            NotifyComplete();
        }
    }
}
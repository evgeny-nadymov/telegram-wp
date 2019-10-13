// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System.IO.IsolatedStorage;
using System.Windows;
using Microsoft.Phone.Tasks;
using TelegramClient.Resources;

namespace TelegramClient.Analytics
{
    public class ReviewRequester
    {
        public static void IncreaseLaunchCount()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains("LaunchCount"))
            {
                var launchCount = (int)settings["LaunchCount"];
                settings["LaunchCount"] = ++launchCount;
            }
            else
            {
                settings["LaunchCount"] = 1;
            }
        }

        public static void Request()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;

            if (settings.Contains("LaunchCount"))
            {
                var launchCount = (int)settings["LaunchCount"];
                var isReviewed = settings.Contains("IsReviewed");

                if (!isReviewed)
                {
                    if (launchCount == 3
                        || launchCount == 5)
                    {
                        //var tracker = new AnalyticsTracker();
                        
                        //var result = MessageBox.Show(AppResources.ReviewRequestMessage, AppResources.ReviewRequestTitle, MessageBoxButton.OKCancel);
                        //tracker.Track("Review", "Requested");

                        //if (result == MessageBoxResult.OK)
                        //{
                        //    settings.Add("IsReviewed", true);
                        //    tracker.Track("Review", "Accepted");

                        //    var task = new MarketplaceReviewTask();
                        //    task.Show();
                        //}
                    }
                }

            }
        }
    }
}

using System;
using System.Linq;
using Microsoft.Phone.Scheduler;
using System.Windows;
using System.Threading;
using Microsoft.Phone.Shell;
using FaceFeed.Model;
using FaceFeed.LiveTile;
using System.Collections.Generic;

namespace FaceFeed.Tasks
{
    public class NotificationTask : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public NotificationTask()
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
            if (FBDataAccess.Current.IsLoggedIn())
            {
                // wait handler for the end of task processing
                var lcklist = new List<ManualResetEvent>();

                // toast notifications
                lcklist.Add(NotifyLatestNotifications(task.LastScheduledTime));

                // update live tiles
                var tileMapping = new Dictionary<string, ShellTile>(); // user id, tile
                for (int i = 0; i < ShellTile.ActiveTiles.Count(); i++)
                {
                    var tile = ShellTile.ActiveTiles.ElementAt(i);
                    var id = i == 0 ? "me" : tile.NavigationUri.GetQueryString("id");
                    tileMapping.Add(id, tile);
                }
                foreach (var tilemap in tileMapping)
                {
                    lcklist.Add(UpdateUserLiveTile(tilemap.Key, tilemap.Value));
                }

                // wait for the handler
                foreach (var lck in lcklist)
                {
                    lck.WaitOne();
                }
            }

            // Call NotifyComplete to let the system know the agent is done working.
            NotifyComplete();
        }

        private ManualResetEvent UpdateUserLiveTile(string p, ShellTile shellTile)
        {
            var lck = new ManualResetEvent(false);

            FBEntity.FindByID<User>(p, user =>
            {
                if (user != null)
                {
                    user.GetStatuses(statuses =>
                    {
                        var first = statuses.FirstOrDefault();
                        if (first != null)
                        {
                            LiveTileHelper.UpdateUserTileStatus(shellTile, user, first);
                        }

                        lck.Set();
                    });
                }
                else
                {
                    lck.Set();
                }
            });

            return lck;
        }

        bool IS_TEST_MODE = System.Diagnostics.Debugger.IsAttached;
        ManualResetEvent NotifyLatestNotifications(DateTime lastRun)
        {
            var lck = new ManualResetEvent(false);

            // get current user
            Me.GetCurrent(me =>
            {
                // if logged in
                if (me != null)
                {
                    // get notification
                    using (new DataAccessDisableCacheContext())
                    {
                        me.GetNotifications(notifications =>
                        {
                            // update main tile count
                            var count = IS_TEST_MODE ? DateTime.Now.Millisecond % 20 : notifications.Where(a => a.Unread).Count();
                            LiveTileHelper.UpdateMainTileCount(count);

                            var items = notifications.Where(a => a.Date > lastRun); // max 3, dont kill user

                            // make sure in test mode there is at least one thing to notify!
                            if (IS_TEST_MODE)
                            {
                                if (items.Where(a => a.Unread).None())
                                {
                                    items.First().Unread = true;
                                }
                            }

                            // notify every new item
                            foreach (var item in items.Where(a => a.Unread).Take(3))
                            {
                                ShowToast(item);
                            }

                            lck.Set();
                        });
                    }
                }
                else
                {
                    lck.Set();
                }
            });

            return lck;
        }

        private void ShowToast(Notification notification)
        {
            if (Settings.Current.ToastNotificationDisabled) return;

            var url = FBNavigationHelper.TranslateFacebookLink(notification.Link).Or("/Pages/Home.xaml");

            var toast = new ShellToast
            {
                Title = "FaceFeed",
                Content = notification.Title,
                NavigationUri = new Uri(url, UriKind.Relative),
            };

            toast.Show();
        }
    }

}

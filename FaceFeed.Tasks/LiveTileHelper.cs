using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Media.Imaging;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
using FaceFeed.Model;

namespace FaceFeed.LiveTile
{
    static class __ext
    {
        public static string ToTimeDifferenceString(this DateTime date)
        {
            var diff = DateTime.Now - date;

            if (diff.TotalSeconds < 0) diff = TimeSpan.Zero;

            if (diff.TotalMinutes < 1) return "{0} seconds ago".FormatWith((int)diff.TotalSeconds);
            if (diff.TotalHours < 1) return "{0} minutes ago".FormatWith((int)diff.TotalMinutes);
            if (diff.TotalDays < 1) return "{0} hours ago".FormatWith((int)diff.TotalHours);
            if (diff.TotalDays < 7) return "on {0}".FormatWith(date.DayOfWeek).ToString();
            if (date.Year >= DateTime.Now.Year) return "{0:MMMMM dd}".FormatWith(date);

            return "{0:MMMMM dd yyyy}".FormatWith(date).ToString();
        }

    }
    public static class LiveTileHelper
    {
        public static void UpdateMainTileCount(int count)
        {
            Uri backtileimageuri = null;
            var tilelock = new ManualResetEvent(false);
            ThreadHelper.RunOnUI(() =>
            {
                new Templates.TileFrontMainTemplate(count).SaveAsImage("Main.FrontTile", fn =>
                {
                    backtileimageuri = fn;
                    tilelock.Set();
                });
            });
            tilelock.WaitOne();

            ShellTile.ActiveTiles.First().Update(new StandardTileData
            {
                BackgroundImage = backtileimageuri
            });
        }
        public static void UpdateUserTileStatus(ShellTile shellTile, User user, FeedItem first)
        {
            shellTile.Update(new StandardTileData
            {
                BackBackgroundImage = GenerateUserTileBackBackgroundImage(user, first)
            });
        }
        public static Uri GenerateUserTileBackBackgroundImage(User user, FeedItem first)
        {
            var date = "";
            var story = "";
            if (first != null)
            {
                date = first.Date.ToTimeDifferenceString();
                story = first.Story;
            }

            Uri backtileimageuri = null;
            var tilelock = new ManualResetEvent(false);
            ThreadHelper.RunOnUI(() =>
            {
                new Templates.TileBackTemplate
                {
                    Date = date,
                    Body = story,
                    User = user.Name
                }.SaveAsImage("User.{0}.BackTile".FormatWith(user.Id), fn =>
                {
                    backtileimageuri = fn;
                    tilelock.Set();
                });
            });
            tilelock.WaitOne();

            return backtileimageuri;
        }
    }
}

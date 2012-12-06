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
using FaceFeed.Model;
using Microsoft.Phone.Controls;
using System.Threading;

namespace FaceFeed
{
    public static class App_BaseBehavious
    {
        public static void LoadComments(FBEntity Item, ListBox lstComments, Button btnCommentsMore, bool nextpage)
        {
            string lastid = null;
            var offset = 0;
            if (nextpage)
            {
                var current = lstComments.ItemsSource as VirtualizedCollection<Comment>;
                if (current != null)
                {
                    lastid = current.Last().Id;
                    offset = current.Count;
                }
            }

            LoadingManager.Start();
            Item.GetComments(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    var loadmoreenabled = nextpage ? list.Any() : list.Count >= FBDataAccess.LIST_LIMIT_SIZE;

                    if (lastid.HasValue())
                    {
                        var current = lstComments.ItemsSource as VirtualizedCollection<Comment>;
                        if (current != null)
                        {
                            list = new VirtualizedCollection<Comment>(current.Concat(list).Distinct(a => a.Id));
                        }
                    }

                    lstComments.ItemsSource = list;
                    btnCommentsMore.Visibility = loadmoreenabled.ToVisibility();
                    btnCommentsMore.IsEnabled = true;
                });
                LoadingManager.Stop();
            }, lastid, offset);
        }
    }
}

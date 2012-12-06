using System;
using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Threading;
using FaceFeed.Model;
using System.Windows.Navigation;
using FaceFeed.Modules;
using System.Collections;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using System.WindowsPhone;
using System.Windows.Media.Imaging;
using System.IO;

namespace Microsoft.Phone.Controls
{
    public static class ShellHelper
    {
        public static string SaveShellImage(byte[] imagedata, string itemid)
        {
            var imagefilename = "";
            if (imagedata != null)
            {
                var source = (BitmapImage)new ByteArrayToImageSourceConverter().Convert(imagedata, null, null, null);
                if (source != null)
                {
                    var bmp = new WriteableBitmap(source);

                    var imagefolder = "/Shared/ShellContent/";
                    imagefilename = imagefolder + "User.Image.{0}.PNG".FormatWith(itemid);

                    if (IOHelper.Directory.Exists(imagefolder) == false)
                    {
                        IOHelper.Directory.CreateDirectory(imagefolder);
                    }
                    IOHelper.File.CreateFile(imagefilename, str =>
                    {
                        bmp.SaveJpeg(str, bmp.PixelWidth, bmp.PixelHeight, 0, 100);
                    });
                }
            }

            return imagefilename;
        }
        public static ShellTile FindShellTile(string url)
        {
            return ShellTile.ActiveTiles.FirstOrDefault(a => a.NavigationUri.OriginalString == url);
        }
    
    }

    public static class NavigationHelper
    {
        public static bool ProcessFacebookLink(this PhoneApplicationPage page, string link)
        {
            var url = FBNavigationHelper.TranslateFacebookLink(link);
            if (url.HasValue())
            {
                page.NavigationService.NavigateTo(url);
                return true;
            }

            return false;
        }

        public static void NavigateToItem<T>(this PhoneApplicationPage page, T item, string idcontainer = null) where T : FBEntity
        {
            if (item != null)
            {
                NavigateToItem<T>(page, item.Id, item, idcontainer);
            }
        }
        public static void NavigateToItem<T>(this PhoneApplicationPage page, string id, FBEntity instance = null, string idcontainer = null) where T : FBEntity
        {
            var type = instance == null ? typeof(T) : instance.GetType();
            var typename = type.Name;
            if (typename == "Notification")
            {
                var notif = instance as FaceFeed.Model.Notification;
                if (notif != null && page.ProcessFacebookLink(notif.Link))
                {
                    return;
                }
            }

            if (FBNavigationHelper.TypePageMapping.ContainsKey(typename))
            {
                var pagename = FBNavigationHelper.TypePageMapping[typename];
                page.NavigateToSubsection(pagename, id, idcontainer);
            }
            else
            {
                if (instance is FromUserItem)
                {
                    NavigateToItem<User>(page, (instance as FromUserItem).From_Id, idcontainer: idcontainer);
                }
            }
        }
        static void NavigateToSubsection(this PhoneApplicationPage page, string pagename, string id, string idcontainer = null) { page.NavigationService.NavigateTo(FBNavigationHelper.GetSubsectionUrl(pagename, id, idcontainer)); }

        public static void HandleChange(this ListBox list, bool sendContainer = false)
        {
            if (list.SelectedIndex == -1) return;

            var selectedItem = list.SelectedItem as FBEntity;

            var container = "";
            if (sendContainer)
            {
                container = IDContainerManager.AddContainer((list.ItemsSource as IEnumerable).Cast<FBEntity>().Select(a => a.Id).ToList());
            }

            // mark notification as read
            if (selectedItem is FaceFeed.Model.Notification)
            {
                var notif = selectedItem as FaceFeed.Model.Notification;
                notif.MarkAsRead();
            }

            // go to relevant page
            list.FindRootPage().NavigateToItem(selectedItem, container);

            list.SelectedIndex = -1;
        }
    }

    public class PivotManager
    {
        Pivot PivotControl { get; set; }
        Action<string> LoadAction { get; set; }
        Action<string> UnloadAction { get; set; }
        int LastCalledFor { get; set; }

        public PivotManager(Pivot control, Action<string> loadAction, Action<string> unloadAction = null)
        {
            LastCalledFor = -1;
            PivotControl = control;
            LoadAction = loadAction;
            UnloadAction = unloadAction;

            control.SelectionChanged += (sender, args) => OnChanged(control.SelectedIndex);
            OnChanged(control.SelectedIndex);
        }

        void OnChanged(int ind)
        {
            if (LastCalledFor == ind) return;

            // cancel all pending tasks
            FBDataAccess.Current.CancelAllPendingTasks();
            FBDataAccess.Current.CancelAllPendingImageDownloaders();
            LoadingManager.StopAll();

            var oldIndex = LastCalledFor;

            LastCalledFor = ind;

            if (LoadAction != null && ind >= 0)
            {
                LoadAction.Invoke((PivotControl.Items[ind] as PivotItem).Header.ToString());
            }
            if (UnloadAction != null && oldIndex >= 0)
            {
                UnloadAction.Invoke((PivotControl.Items[oldIndex] as PivotItem).Header.ToString());
            }
        }
        public void Trigger(int ix)
        {
            LastCalledFor = -1;
            OnChanged(ix);
        }

        public void Refresh()
        {
            using (new DataAccessDisableCacheContext())
            {
                Trigger(PivotControl.SelectedIndex);
            }
        }
    }

    public static class LoadingManager
    {
        static LoadingPanel ProgressBar;
        static int LoadingCount;

        public static void Register(LoadingPanel control)
        {
            lock (typeof(LoadingManager))
            {
                ProgressBar = control;
                LoadingCount = 0;
            }
        }
        public static void UnRegister()
        {
            lock (typeof(LoadingManager))
            {
                ProgressBar = null;
                LoadingCount = 0;
            }
        }

        public static void Start()
        {
            lock (typeof(LoadingManager))
            {
                if (ProgressBar == null) return;

                if (ProgressBar == null) return;
                ThreadHelper.RunOnUI(() =>
                {
                    ProgressBar.Start();
                });

                Interlocked.Increment(ref LoadingCount);
            }
        }
        public static void Stop()
        {
            lock (typeof(LoadingManager))
            {
                if (ProgressBar == null) return;

                Interlocked.Decrement(ref LoadingCount);
                if (LoadingCount <= 0)
                {
                    LoadingCount = 0;

                    ThreadHelper.RunOnUI(() =>
                    {
                        ProgressBar.Stop();
                    });
                }
            }
        }
        internal static void StopAll()
        {
            lock (typeof(LoadingManager))
            {
                LoadingCount = 0;
                if (ProgressBar == null) return;

                ThreadHelper.RunOnUI(() =>
                {
                    ProgressBar.Stop();
                });
            }
        }

        internal static void ShowMessage(string p)
        {
            if (ProgressBar != null)
            {
                ThreadHelper.RunOnUI(() =>
                {
                    MessageBox.Show(p);
                    Stop();
                });
            }
        }

        internal static void GenericError(Exception ex)
        {
            // ignore ad-related errors
            if (ex.StackTrace.Contains("Microsoft.Advertising"))
            {
                return;
            }

            ThreadHelper.RunOnUI(() =>
            {
                if (MessageBox.Show(@"A generic error occured in the application!
Do you want to report this?", "Error", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    var message = "";
                    var pex = ex;
                    while (pex != null)
                    {
                        message += ex.Message + Environment.NewLine;
                        pex = pex.InnerException;
                    }

                    new EmailComposeTask
                    {
                        Subject = "FaceFeed - Error",
                        To = "persianwindowsphone@gmail.com",
                        Body = @"Message:
{0}

StackTrace:
{1}

version: 1.4.0.0".FormatWith(message, ex.StackTrace)
                    }.Show();
                }
            });

        }
    }

    public static class IDContainerManager
    {
        static Dictionary<string, List<string>> Dictionary = new Dictionary<string, List<string>>();

        public static string AddContainer(List<string> list)
        {
            var id = Guid.NewGuid().ToString();
            lock (Dictionary)
            {
                Dictionary.Add(id, list);
            }

            return id;
        }
        public static List<string> GetContainer(string id)
        {
            lock (Dictionary)
            {
                return Dictionary.TryGet(id) ?? new List<string>();
            }
        }
        public static void RemoveContainer(string id)
        {
            if (id.HasValue() == false) return;

            lock (Dictionary)
            {
                Dictionary.Remove(id);
            }
        }
    }


}

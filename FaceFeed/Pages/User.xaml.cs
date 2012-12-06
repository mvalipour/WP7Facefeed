using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using FaceFeed.Model;
using System.Threading;
using Microsoft.Phone.Shell;
using System.WindowsPhone;
using System.Windows.Media.Imaging;
using System.IO;
using FaceFeed.LiveTile;

namespace FaceFeed.Pages
{
    public partial class User_Page : PhoneApplicationPage
    {
        public User_Page()
        {
            InitializeComponent();
        }

        public User Item { get; set; }
        void LoadItem()
        {
            LoadingManager.Start();

            var id = NavigationContext.QueryString["id"];
            User.GetUser(id, u =>
            {
                Item = u;

                ThreadHelper.RunOnUI(() =>
                {
                    OnItemLoaded();
                });

                LoadingManager.Stop();
            });
        }

        public PivotManager PivotManager { get; set; }
        void OnItemLoaded()
        {
            if (Item == null)
            {
                MessageBox.Show("Could not load this item.");
                NavigationService.GoBack();
                return;
            }

            pivot.Title = pageMenu.Title = Item.Name;

            PivotManager = new PivotManager(pivot, k => LoadPivotItem(k), k => UnloadPivotItem(k));
        }
        void LoadPivotItem(string k)
        {
            if (Item == null) return;

            if (k == "feed") LoadFeeds();
            else if (k == "albums") LoadPhotoAlbums();
            else if (k == "pages") LoadPages();
            else if (k == "groups") LoadGroups();
            else if (k == "notes") LoadNotes();
            else if (k == "photos") LoadPhotos();
            else if (k == "mutual") LoadMutualFriends();
            else if (k == "profile") LoadProfile();
        }
        void UnloadPivotItem(string k)
        {
            if (k == "feed") lstFeeds.ItemsSource = null;
            else if (k == "albums") lstAlbums.ItemsSource = null;
            else if (k == "pages") lstPages.ItemsSource = null;
            else if (k == "notes") lstNotes.ItemsSource = null;
            else if (k == "photos") lstPhotos.ItemsSource = null;
            else if (k == "mutual") lstMutuals.ItemsSource = null;
        }

        private void LoadPages()
        {
            LoadingManager.Start();

            Item.GetPages(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstPages.ItemsSource = list;

                    LoadingManager.Stop();
                });
            });
        }
        private void LoadGroups()
        {
            LoadingManager.Start();

            Item.GetGroups(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstGroups.ItemsSource = list;
                    LoadingManager.Stop();
                });
            });
        }
        private void LoadNotes()
        {
            LoadingManager.Start();

            Item.GetNotes(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstNotes.ItemsSource = list;

                    LoadingManager.Stop();
                });
            });
        }
        private void LoadProfile()
        {
            if (profileContainer.DataContext != null) return;

            Item.GetLargeImage(data =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    profileContainer.DataContext = data;
                });
            });

            Item.GetStatuses(items =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    statusContainer.DataContext = items.FirstOrDefault();
                });
            });
        }
        private void LoadMutualFriends()
        {
            LoadingManager.Start();
            Me.GetCurrent(me =>
            {
                me.GetMutualFriends(Item, list =>
                {
                    ThreadHelper.RunOnUI(() =>
                    {
                        lstMutuals.ItemsSource = list;
                    });

                    LoadingManager.Stop();
                });
            });
        }
        private void LoadPhotos()
        {
            LoadingManager.Start();
            Item.GetPhotos(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstPhotos.ItemsSource = list;
                });

                LoadingManager.Stop();
            });
        }
        private void LoadPhotoAlbums()
        {
            LoadingManager.Start();
            Item.GetPhotoAlbums(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstAlbums.ItemsSource = list;
                });

                LoadingManager.Stop();
            });
        }
        private void LoadFeeds()
        {
            LoadingManager.Start();
            Item.GetWallFeed(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstFeeds.ItemsSource = list;
                });

                LoadingManager.Stop();
            });
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // if not logged in anymore 
            if (FBDataAccess.Current.IsLoggedIn() == false) { App.MyApp.NavigateToLogin(); return; }

            LoadingManager.Register(progressbar);
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                pageMenu.Setup(pivot);

                LoadItem();
            }

            if (FindShellTile() != null)
            {
                ApplicationBar.FindButton("pin").IconUri = new Uri("/Images/Icons/unpin.PNG", UriKind.Relative);
            }
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            LoadingManager.UnRegister();
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);

            if (pageMenu.Visibility == System.Windows.Visibility.Visible)
            {
                pageMenu.Toggle();
                e.Cancel = true;
            }
        }

        private void lstFeeds_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(true); }
        private void lstMutuals_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstPhotos_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(true); }
        private void lstPages_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstNotes_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstGroups_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }

        private void btnPost_Click(object sender, RoutedEventArgs e)
        {
            if (txtWallMessage.Text.HasValue() == false)
            {
                return;
            }

            LoadingManager.Start();

            txtWallMessage.IsEnabled = false;
            pivot.Focus();

            Item.PostOnWall(txtWallMessage.Text, () =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    txtWallMessage.Text = "";
                    txtWallMessage.IsEnabled = true;

                    LoadingManager.Stop();
                });
            });
        }
        private void btnStatus_Click(object sender, RoutedEventArgs e)
        {
            if (statusContainer.DataContext != null)
            {
                this.NavigateToItem(statusContainer.DataContext as FBEntity);
            }
        }
        private void btnMenu_Click(object sender, EventArgs e) { pageMenu.Toggle(); }
        private void btnSync_Click(object sender, EventArgs e) { if (PivotManager != null) PivotManager.Refresh(); }
        private void btnHome_Click(object sender, RoutedEventArgs e) { NavigationService.NavigateTo("/Pages/Home.xaml"); }

        private void btnPin_Click(object sender, EventArgs e)
        {
            if (Item == null) return;

            var tile = FindShellTile();
            if (tile != null)
            {
                ApplicationBar.FindButton("pin").IconUri = new Uri("/Images/Icons/pin.PNG", UriKind.Relative);

                tile.Delete();

                return;
            }

            try
            {
                LoadingManager.Start();
                GetTileData(data =>
                {
                    ThreadHelper.RunOnUI(() =>
                    {
                        ShellTile.Create(new Uri(GetUrl(), UriKind.Relative), data);
                        LoadingManager.Stop();
                    });
                });
            }
            catch (ThreadHelperException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GetTileData(Action<ShellTileData> callback)
        {
            var imagefilename = ShellHelper.SaveShellImage(profileContainer.DataContext as byte[], Item.Id);

            var res = new StandardTileData
            {
                Title = Item.Name,
            };

            if (imagefilename.HasValue())
            {
                res.BackgroundImage = new Uri("isostore:" + imagefilename, UriKind.Absolute);
            }

            var status = statusContainer.DataContext as FeedItem;

            ThreadHelper.RunUniqueAsync(() =>
            {
                res.BackBackgroundImage = LiveTileHelper.GenerateUserTileBackBackgroundImage(Item, status);

                callback.Invoke(res);
            });
        }
        private ShellTile FindShellTile() { return ShellHelper.FindShellTile(GetUrl()); }
        private string GetUrl()
        {
            return "/Pages/User.xaml?id={0}".FormatWith(NavigationContext.GetQueryString("id"));
        }
    }
}
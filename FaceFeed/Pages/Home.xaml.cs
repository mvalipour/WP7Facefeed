
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

namespace FaceFeed.Pages
{
    public partial class Home_Page : PhoneApplicationPage
    {
        public Home_Page()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // if not logged in anymore 
            if (FBDataAccess.Current.IsLoggedIn() == false) { App.MyApp.NavigateToLogin(); return; }

            if (App.IsWhatsNewShown == false)
            {
                NavigationService.NavigateTo("/Pages/WhatsNew.xaml");
                return;
            }

            if (App.IsAlertShown == false)
            {
                ThreadHelper.RunUniqueAsync(() =>
                {
                    Thread.Sleep(2000);
                    ThreadHelper.RunOnUI(() =>
                    {
                        App.ShowAlert();
                        App.IsAlertShown = true;
                    });
                });
            }

            // clear back entry
            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }

            LoadingManager.Register(progressbar);
            if (e.NavigationMode != System.Windows.Navigation.NavigationMode.Back)
            {
                pageMenu.Setup(pivot);

                LoadItem();
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
                return;
            }

            if (MessageBox.Show("Are you sure you want to exit?", "", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
        }

        public Me Item { get; set; }
        private void LoadItem()
        {
            LoadingManager.Start();

            Me.GetCurrent(me =>
            {
                LoadingManager.Stop();

                if (me == null)
                {
                    App.MyApp.NavigateToLogin();
                }
                else
                {
                    Item = me;

                    ThreadHelper.RunOnUI(() =>
                    {
                        LoadMeData();
                    });
                }
            });
        }

        public PivotManager PivotManager { get; set; }
        private void LoadMeData()
        {
            pivot.Title = pageMenu.Title = Item.Name;

            PivotManager = new PivotManager(pivot, k => LoadPivotItem(k), k => UnloadPivotItem(k));
        }

        void LoadPivotItem(string k)
        {
            if (Item == null) return;

            if (k == "notifications") LoadNotifications();
            else if (k == "pages") LoadPages();
            else if (k == "groups") LoadGroups();
            else if (k == "notes") LoadNotes();
            else if (k == "friends") LoadFriends();
            else if (k == "feed") LoadFeeds();
            else if (k == "wall") LoadWallPosts();
            else if (k == "albums") LoadPhotoAlbums();
            else if (k == "photos") LoadPhotos();
            else if (k == "profile") LoadProfile();
        }
        void UnloadPivotItem(string k)
        {
            if (k == "notifications") lstNotifications.ItemsSource = null;
            else if (k == "pages") lstPages.ItemsSource = null;
            else if (k == "notes") lstNotes.ItemsSource = null;
            else if (k == "friends") lstFriends.ItemsSource = null;
            else if (k == "feed") lstFeeds.ItemsSource = null;
            else if (k == "wall") lstWallPosts.ItemsSource = null;
            else if (k == "albums") lstAlbums.ItemsSource = null;
            else if (k == "photos") lstPhotos.ItemsSource = null;
            else if (k == "profile") { }
        }

        bool lstPages_Loading = false;
        private void LoadPages(string f = null)
        {
            if (lstPages_Loading) return;

            lstPages_Loading = true;
            LoadingManager.Start();

            Item.GetPages(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    if (f.HasValue())
                    {
                        list = new VirtualizedCollection<Model.Page>(list.Where(a => a.Matches(f)));
                    }

                    lstPages.ItemsSource = list;
                    lstPages_Loading = false;
                    LoadingManager.Stop();

                    lstFriends.Focus();
                });
            });
        }
        bool lstGroups_Loading = false;
        void LoadGroups(string f = null)
        {
            if (lstGroups_Loading) return;

            lstGroups_Loading = true;
            LoadingManager.Start();

            Item.GetGroups(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    if (f.HasValue())
                    {
                        list = new VirtualizedCollection<Model.Group>(list.Where(a => a.Matches(f)));
                    }

                    lstGroups.ItemsSource = list;
                    lstGroups_Loading = false;
                    LoadingManager.Stop();

                    lstFriends.Focus();
                });
            });
        }
        void LoadNotes()
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
        void LoadProfile()
        {
            if (profileContainer.DataContext != null) return;

            Item.GetLargeImage(data =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    profileContainer.DataContext = new
                    {
                        ImageData = data,
                    };
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
        void LoadNotifications()
        {
            LoadingManager.Start();

            Item.GetNotifications(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstNotifications.ItemsSource = list;

                    LoadingManager.Stop();
                });
            });
        }
        void LoadPhotos()
        {
            LoadingManager.Start();

            Item.GetPhotos(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstPhotos.ItemsSource = list;

                    LoadingManager.Stop();
                });
            });
        }
        void LoadPhotoAlbums()
        {
            LoadingManager.Start();

            Item.GetPhotoAlbums(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstAlbums.ItemsSource = list;
                    LoadingManager.Stop();
                });
            });
        }
        void LoadFeeds(bool nextpage = false)
        {
            DateTime? until = null;
            if (nextpage)
            {
                var current = lstFeeds.ItemsSource as VirtualizedCollection<FeedItem>;
                if (current != null)
                {
                    until = current.Last().Date;
                }
            }

            LoadingManager.Start();
            Item.GetHomeFeed(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    var loadmoreenabled = nextpage ? list.Any() : list.Count >= FBDataAccess.LIST_LIMIT_SIZE;

                    if (until.HasValue)
                    {
                        var current = lstFeeds.ItemsSource as VirtualizedCollection<FeedItem>;
                        if (current != null)
                        {
                            list = new VirtualizedCollection<FeedItem>(current.Concat(list).Distinct(a => a.Id));
                        }
                    }

                    lstFeeds.ItemsSource = list;
                    btnFeedLoadMore.Visibility = loadmoreenabled.ToVisibility();
                    btnFeedLoadMore.IsEnabled = true;

                    LoadingManager.Stop();
                });
            }, until);
        }
        void LoadWallPosts(bool nextpage = false)
        {
            DateTime? until = null;
            if (nextpage)
            {
                var current = lstWallPosts.ItemsSource as VirtualizedCollection<FeedItem>;
                if (current != null)
                {
                    until = current.Last().Date;
                }
            }

            LoadingManager.Start();
            Item.GetWallFeed(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    var loadmoreenabled = nextpage ? list.Any() : list.Count >= FBDataAccess.LIST_LIMIT_SIZE;

                    if (until.HasValue)
                    {
                        var current = lstWallPosts.ItemsSource as VirtualizedCollection<FeedItem>;
                        if (current != null)
                        {
                            list = new VirtualizedCollection<FeedItem>(current.Concat(list).Distinct(a => a.Id));
                        }
                    }

                    lstWallPosts.ItemsSource = list;
                    btnWallPostsMore.Visibility = loadmoreenabled.ToVisibility();
                    btnWallPostsMore.IsEnabled = true;

                    LoadingManager.Stop();
                });
            }, until);
        }
        bool lstFriends_Loading = false;
        private void LoadFriends(string f = null)
        {
            if (lstFriends_Loading) return;

            lstFriends_Loading = true;
            LoadingManager.Start();
            Item.GetFriends(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    if (f.HasValue())
                    {
                        list = new VirtualizedCollection<User>(list.Where(a => a.Matches(f)));
                    }

                    lstFriends.ItemsSource = list;
                    lstFriends_Loading = false;
                    LoadingManager.Stop();

                    lstFriends.Focus();
                });
            });
        }

        private void lstFriends_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstFeeds_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(true); }
        private void lstAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstPhotos_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(true); }
        private void lstNotifications_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstPages_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstNotes_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstGroups_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstWallPosts_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
 
        private void btnPost_Click(object sender, RoutedEventArgs e)
        {
            if (Item == null)
            {
                return;
            }

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
        private void btnClearCache_Click(object sender, EventArgs e)
        {
            LoadingManager.Start();

            try
            {
                FBDataAccess.ClearCache(() =>
                {
                    ThreadHelper.RunOnUI(() =>
                    {
                        LoadingManager.Stop();
                    });
                });
            }
            catch (ThreadHelperException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void btnLogOut_Click(object sender, EventArgs e)
        {
            LoadingManager.Start();

            FBDataAccess.Current.LogOut(() =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    LoadingManager.Stop();

                    App.MyApp.NavigateToLogin();
                });
            });
        }
        private void btnSync_Click(object sender, EventArgs e) { if (PivotManager != null) PivotManager.Refresh(); }
        private void btnSettings_Click(object sender, EventArgs e)
        {
            NavigationService.NavigateTo("/Pages/Settings.xaml");
        }
        private void btnAbout_Click(object sender, EventArgs e)
        {
            NavigationService.NavigateTo("/Pages/About.xaml");
        }

        private void txtSearchFriends_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var filter = txtSearchFriends.Text.ToLower();
                LoadFriends(filter);
            }
        }
        private void txtSearchPages_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var filter = txtSearchPages.Text.ToLower();
                LoadPages(filter);
            }
        }
        private void txtSearchGroups_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var filter = txtSearchGroups.Text.ToLower();
                LoadGroups(filter);
            }
        }

        private void btnFeedLoadMore_Click(object sender, RoutedEventArgs e)
        {
            btnFeedLoadMore.IsEnabled = false;
            LoadFeeds(true);
        }
        private void btnWallPostsMore_Click(object sender, RoutedEventArgs e)
        {
            btnWallPostsMore.IsEnabled = false;
            LoadWallPosts(true);
        }

    }
}
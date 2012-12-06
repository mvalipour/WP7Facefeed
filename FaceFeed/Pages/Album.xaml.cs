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
    public partial class Album_Page : PhoneApplicationPage
    {
        public Album_Page()
        {
            InitializeComponent();
        }

        public PhotoAlbum Item { get; set; }
        void LoadItem()
        {
            LoadingManager.Start();

            var id = NavigationContext.QueryString["id"];
            FBEntity.FindByID<PhotoAlbum>(id, u =>
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

            PivotManager = new PivotManager(pivot, k => LoadPivotItem(k));
        }
        void LoadPivotItem(string k)
        {
            if (Item == null) return;

            if (k == "comments") LoadComments();
            else if (k == "likes") LoadLikes();
            else if (k == "photos") LoadPhotos();
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
        private void LoadLikes()
        {
            LoadingManager.Start();
            Item.GetLikes(list =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstLikes.ItemsSource = list;
                });
                LoadingManager.Stop();
            });
        }
        void LoadComments(bool nextpage = false) { App_BaseBehavious.LoadComments(Item, lstComments, btnCommentsMore, nextpage); }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

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
            }
        }

        private void lstComments_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(true); }
        private void lstLikes_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }
        private void lstPhotos_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(true); }

        private void txtComment_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtComment.Text.HasValue() == false)
                {
                    return;
                }
                
                LoadingManager.Start();

                txtComment.IsEnabled = false;
                pivot.Focus();

                Item.PostComment(txtComment.Text, () =>
                {
                    ThreadHelper.RunOnUI(() =>
                    {
                        lstComments.ItemsSource = null;
                        using (new DataAccessDisableCacheContext())
                        {
                            LoadComments();
                        }

                        txtComment.Text = "";
                        txtComment.IsEnabled = true;

                        LoadingManager.Stop();
                    });
                });
            }
        }
        private void btnLike_Click(object sender, RoutedEventArgs e)
        {
            LoadingManager.Start();

            btnLike.IsEnabled = false;

            Item.PostLike(() =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    lstLikes.ItemsSource = null;
                    using (new DataAccessDisableCacheContext())
                    {
                        LoadLikes();
                    }

                    btnLike.IsEnabled = true;

                    LoadingManager.Stop();
                });
            });
        }
        private void btnSync_Click(object sender, EventArgs e) { if (PivotManager != null) PivotManager.Refresh(); }
        private void btnMenu_Click(object sender, EventArgs e) { pageMenu.Toggle(); }
        private void btnHome_Click(object sender, RoutedEventArgs e) { NavigationService.NavigateTo("/Pages/Home.xaml"); }
        private void btnCommentsMore_Click(object sender, RoutedEventArgs e)
        {
            btnCommentsMore.IsEnabled = false;
            LoadComments(true);
        }
    }
}
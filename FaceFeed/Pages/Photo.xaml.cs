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
using System.Windows.Media.Imaging;
using System.IO;

namespace FaceFeed.Pages
{
    public partial class Photo_Page : PhoneApplicationPage
    {
        public Photo_Page()
        {
            InitializeComponent();
        }

        List<string> IDList { get; set; }
        int CurrentIDIndex { get; set; }

        public Photo Item { get; set; }
        void LoadItem()
        {
            // store
            State["CurrentIDIndex"] = CurrentIDIndex;

            var btnNext = ApplicationBar.FindButton("next");
            var btnPrev = ApplicationBar.FindButton("prev");
            btnNext.IsEnabled = CurrentIDIndex < IDList.Count - 1;
            btnPrev.IsEnabled = CurrentIDIndex > 0;

            OnBeforeLoading();

            LoadingManager.Start();

            var id = IDList[CurrentIDIndex];
            FBEntity.FindByID<Photo>(id, u =>
            {
                Item = u;

                ThreadHelper.RunOnUI(() =>
                {
                    OnItemLoaded();
                });

                LoadingManager.Stop();
            });
        }
        void OnBeforeLoading()
        {
            itemScroller.ScrollToVerticalOffset(0);
            itemContainer.Visibility = false.ToVisibility();

            pivot.SelectedIndex = 0;
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

            itemContainer.DataContext = Item;
            itemContainer.Visibility = true.ToVisibility();

            PivotManager = new PivotManager(pivot, k => LoadPivotItem(k));
        }
        void LoadPivotItem(string k)
        {
            if (Item == null) return;

            if (k == "comments") LoadComments();
            else if (k == "likes") LoadLikes();
            else if (k == "photo") LoadPhoto();
        }

        private void LoadPhoto()
        {
            LoadingManager.Start();
            Item.GetImage(data =>
            {
                ThreadHelper.RunOnUI(() =>
                {
                    imgPhoto.Source = (ImageSource)new System.WindowsPhone.ByteArrayToImageSourceConverter().Convert(data, null, null, null);
                    LoadingManager.Stop();
                });
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

                var id = NavigationContext.QueryString["id"];
                var idcontainer = NavigationContext.QueryString.TryGet("container");
                if (idcontainer.HasValue())
                {
                    IDList = IDContainerManager.GetContainer(idcontainer);
                    CurrentIDIndex = State.ContainsKey("CurrentIDIndex") ? Convert.ToInt32(State["CurrentIDIndex"]) : IDList.IndexOf(id);
                }
                else
                {
                    IDList = new[] { id }.ToList();
                }

                LoadItem();
            }
        }
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            LoadingManager.UnRegister();

            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.Back)
            {
                IDContainerManager.RemoveContainer(NavigationContext.QueryString.TryGet("container"));
            }
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
        private void lstTaggedUsers_SelectionChanged(object sender, SelectionChangedEventArgs e) { (sender as ListBox).HandleChange(); }

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
        private void btnFrom_Click(object sender, RoutedEventArgs e)
        {
            this.NavigateToItem<User>(Item.From_Id);
        }
        private void btnHome_Click(object sender, RoutedEventArgs e) { NavigationService.NavigateTo("/Pages/Home.xaml"); }
        private void btnPrevious_Click(object sender, EventArgs e)
        {
            CurrentIDIndex--;
            if (CurrentIDIndex < 0) CurrentIDIndex = IDList.Count - 1;
            LoadItem();
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            CurrentIDIndex++;
            if (CurrentIDIndex >= IDList.Count) CurrentIDIndex = 0;
            LoadItem();
        }
        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            if (Item == null) return;
            var source = imgPhoto.Source as BitmapImage;
            if (source == null) return;

            var bmp = new WriteableBitmap(source);
            var library = new Microsoft.Xna.Framework.Media.MediaLibrary();
            var memstr = new MemoryStream();
            bmp.SaveJpeg(memstr, source.PixelWidth, source.PixelHeight, 0, 100);
            memstr.Position = 0;

            LoadingManager.Start();
            ThreadHelper.RunUniqueAsync(() =>
            {
                var succ = false;
                var error = "";
                try
                {
                    library.SavePicture("Image", memstr);

                    succ = true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }

                ThreadHelper.RunOnUI(() =>
                {
                    if (succ)
                    {
                        MessageBox.Show("The image is successfully saved to your phone.", "Done!", MessageBoxButton.OK);
                    }
                    else
                    {
                        MessageBox.Show("There was an error saving image to your phone.", "Failed!", MessageBoxButton.OK);
                    }

                    LoadingManager.Stop();
                });
            });
        }
        private void btnCommentsMore_Click(object sender, RoutedEventArgs e)
        {
            btnCommentsMore.IsEnabled = false;
            LoadComments(true);
        }

    }
}
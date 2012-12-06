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

namespace FaceFeed.Pages
{
    public partial class Login : PhoneApplicationPage
    {
        public Login()
        {
            InitializeComponent();

            browser.Navigated += new EventHandler<System.Windows.Navigation.NavigationEventArgs>(browser_Navigated);
            browser.Navigating += new EventHandler<NavigatingEventArgs>(browser_Navigating);
        }

        void browser_Navigating(object sender, NavigatingEventArgs e)
        {
            LoadingManager.Start();
        }

        void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (FBDataAccess.Current.Authorize(e.Uri.OriginalString))
            {
                NavigationService.NavigateTo("/Pages/Home.xaml");
            }

            LoadingManager.Stop();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoadingManager.Register(progressbar);

            LoadOAuth();
        }

        private void LoadOAuth()
        {
            browser.Navigate(new Uri(FBDataAccess.GetOAuthUrl()));
        }
    }
}
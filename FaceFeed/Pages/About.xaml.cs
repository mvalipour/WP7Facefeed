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
using Microsoft.Phone.Tasks;

namespace FaceFeed.Pages
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            new EmailComposeTask
            {
                Subject = "Face Feed",
                To = "persianwindowsphone@gmail.com"
            }.Show();
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            new MarketplaceReviewTask
            {

            }.Show();
        }
        private void btnMore_Click(object sender, RoutedEventArgs e)
        {
            new MarketplaceSearchTask
            {
                ContentType = MarketplaceContentType.Applications,
                SearchTerms = "ManoRey"
            }.Show();
        }

        private void btnTerms_Click(object sender, RoutedEventArgs e)
        {
            new WebBrowserTask
            {
                Uri = new Uri("http://www.facebook.com/legal/terms")
            }.Show();
        }

        private void HyperlinkButton_Click_2(object sender, RoutedEventArgs e)
        {
            new WebBrowserTask
            {
                URL = "http://www.facebook.com/pages/Persian-Apps-for-Windows-Phone/141190782633836"
            }.Show();
        }

    }
}
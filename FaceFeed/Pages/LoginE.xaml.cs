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
using Microsoft.Phone.Tasks;

namespace FaceFeed.Pages
{
    public partial class LoginE : PhoneApplicationPage
    {
        public LoginE()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            while (NavigationService.CanGoBack)
            {
                NavigationService.RemoveBackEntry();
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            new EmailComposeTask
            {
                Subject = "Persian Facebook for Windows Phone",
                Body = @"
لینک زیر را بر روی کامپیوتر خود باز کنید و مراحل آن را ادامه دهید و تکمیل کنید.

سپس بعد از مشاهده ی پیغام
success
آدرس نوار ابزار مرورگر خود را کپی کنید و در قسمت مربوطه در این نرم افزار  قرار دهید.

لینک اصلی:
{0}

لینک کوتاه شده:
http://goo.gl/PDnyY

برای اطلاعات بیشتر به گروه ما در فیسبوک بپیوندید:
http://www.facebook.com/pages/Persian-Apps-for-Windows-Phone/141190782633836
".FormatWith(FBDataAccess.GetOAuthUrl())
            }.Show();
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FBDataAccess.Current.Authorize(txtUrl.Text))
                {
                    NavigationService.NavigateTo("/Pages/Home.xaml");
                    return;
                }
            }
            catch (Exception ex) { }

            MessageBox.Show("آدرس وارد شده صحیح نمی باشد. لطفا مراحل را با دقت دنبال کنید.");
        }

        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            new WebBrowserTask
            {
                Uri = new Uri(FBDataAccess.GetOAuthUrl())
            }.Show();
        }

        private void btnFacebook_Click(object sender, RoutedEventArgs e)
        {
            new WebBrowserTask
            {
                URL = "http://www.facebook.com/pages/Persian-Apps-for-Windows-Phone/141190782633836"
            }.Show();
        }
    }
}
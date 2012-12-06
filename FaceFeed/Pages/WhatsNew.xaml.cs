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

namespace FaceFeed.Pages
{
    public partial class WhatsNew : PhoneApplicationPage
    {
        public WhatsNew()
        {
            InitializeComponent();
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            App.IsWhatsNewShown = true;
            NavigationService.NavigateTo("/Pages/Home.xaml");
        }
    }
}
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
    public partial class Settings : PhoneApplicationPage
    {
        public Settings()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            PopulateLists();
        }

        void PopulateLists()
        {
            chkNotification.IsChecked = !FaceFeed.Model.Settings.Current.ToastNotificationDisabled;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            FaceFeed.Model.Settings.Current.ToastNotificationDisabled = !(chkNotification.IsChecked ?? false);
            
            FaceFeed.Model.Settings.Current.Persist();

            NavigationService.GoBack();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }
    }

    public class ListItemObject
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ListItemObject(string item)
        {
            Name = Value = item;
        }
        public ListItemObject(string item, string value)
        {
            Name = item;
            Value = value;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
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

namespace FaceFeed.Modules
{
    public partial class PageMenu : UserControl
    {
        public PageMenu()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return lblMenuTitle.Text; }
            set { lblMenuTitle.Text = value; }
        }

        Pivot Pivot { get; set; }

        public void Setup(Pivot p)
        {
            Pivot = p;
            Title = "{0}".FormatWith(Pivot.Title);
        }

        public void Toggle()
        {
            if (Pivot == null) return;

            if (this.Visibility == System.Windows.Visibility.Visible)
            {
                this.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            if (lstMenu.ItemsSource == null)
            {
                lstMenu.ItemsSource = from item in Pivot.Items.OfType<PivotItem>()
                                      select item.Header.ToString();
            }

            this.Visibility = System.Windows.Visibility.Visible;
        }

        private void lstMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstMenu.SelectedIndex == -1) return;

            var item = lstMenu.SelectedItem as string;

            var index = (lstMenu.ItemsSource as IEnumerable<string>).ToList().IndexOf(item);

            Pivot.SelectedIndex = index;
            this.Visibility = System.Windows.Visibility.Collapsed;

            lstMenu.SelectedIndex = -1;
        }

    }
}

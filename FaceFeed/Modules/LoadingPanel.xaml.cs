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

namespace FaceFeed.Modules
{
    public partial class LoadingPanel : UserControl
    {
        public LoadingPanel()
        {
            InitializeComponent();

        }

        public void Start(string text = "Loading...")
        {
            progressbar.IsIndeterminate = true;
            progressbar.IsEnabled = true;
            Visibility = System.Windows.Visibility.Visible;

            //lblLabel.Text = text;
        }
        public void Stop()
        {
            progressbar.IsIndeterminate = false;
            progressbar.IsEnabled = false;
            Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}

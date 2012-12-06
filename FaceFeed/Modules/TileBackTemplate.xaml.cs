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

namespace PersianFacebook.Modules
{
    public partial class TileBackTemplate : UserControl
    {
        public TileBackTemplate()
        {
            InitializeComponent();
        }

        public string Date
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                lblItemDate.Text = value;
            }
        }
        public string Body
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                lblItem.Text = value;
            }
        }
        public string User
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                lblUser.Text = value;
            }
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {

        }
    }
}

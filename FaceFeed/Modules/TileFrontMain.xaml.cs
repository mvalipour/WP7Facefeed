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
using System.Windows.Media.Imaging;

namespace PersianFacebook.Modules
{
    public partial class TileFrontMain : UserControl
    {
        public TileFrontMain()
        {
            InitializeComponent();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
            set
            {
                imgImage.Source = new BitmapImage(new Uri("/images/tile.front" + (value == 0 ? "" : ".c") + ".png", UriKind.Relative));
                txtCount.Text = value == 0 ? "" : value.ToString();
            }
        }
    }
}

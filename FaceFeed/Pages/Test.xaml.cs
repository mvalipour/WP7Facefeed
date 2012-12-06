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
using System.IO;
using FaceFeed.Model;
using System.Windows.Media.Imaging;
using System.Threading;

namespace FaceFeed.Pages
{
    public partial class Test : PhoneApplicationPage
    {
        public Test()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

                //var canvas = new Modules.TileBackTemplate();
                //
                //canvas.Measure(new Size(173, 173));
                //canvas.Arrange(new Rect(0, 0, 173, 173));
                //
                //var bmp = new WriteableBitmap(173, 173);
                //bmp.Render(canvas, null);
                //bmp.Invalidate();
                //
                //    imgImage.Source = bmp;
        }
    }
}
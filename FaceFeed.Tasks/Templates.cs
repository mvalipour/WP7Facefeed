using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Threading;
using System.IO;

namespace FaceFeed.LiveTile
{
    public class Templates
    {
        public class TileBackTemplate : TileTemplate
        {
            TextBlock lblItemDate, lblItem, lblUser;

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

            public TileBackTemplate()
            {
                var grid = new Grid
                {
                    Width = Width,
                    Height = Height
                };

                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

                grid.Children.Add(CreateDateBar());
                grid.Children.Add(CreateItemBar());
                grid.Children.Add(CreateUserBar());

                //
                // bg image
                var source = new BitmapImage();
                var lck = new ManualResetEvent(true);
                source.ImageOpened += (a, b) =>
                {
                    lck.Set();
                };
                source.UriSource = new Uri("/images/tile.bg.png", UriKind.Relative);
                lck.WaitOne();

                Children.Add(new Image { Source = source });
                Children.Add(grid);
            }

            private UIElement CreateDateBar()
            {
                lblItemDate = new TextBlock
                {
                    TextAlignment = TextAlignment.Right,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 14,
                    Margin = new Thickness(6, 3, 6, 0),
                    Opacity = 0.75
                };

                return lblItemDate;
            }
            private UIElement CreateItemBar()
            {
                lblItem = new TextBlock
                {
                    LineHeight = 18,
                    LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 16
                };

                var grid = new Grid
                {
                    Margin = new Thickness(6, 6, 6, 0)
                };
                Grid.SetRow(grid, 1);

                grid.Children.Add(lblItem);

                //
                return grid;
            }
            private UIElement CreateUserBar()
            {
                lblUser = new TextBlock
                {
                    Margin = new Thickness(6, 3, 6, 3),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 20
                };

                var grid = new Grid
                {
                    Background = new SolidColorBrush(Colors.Black) { Opacity = 0.25 }
                };
                Grid.SetRow(grid, 2);

                grid.Children.Add(lblUser);

                //
                return grid;
            }
        }

        public class TileFrontMainTemplate : TileTemplate
        {
            public TileFrontMainTemplate(int count)
            {
                var grid = new Grid
                {
                    Width = Width,
                    Height = Height
                };

                var txtItem = new TextBlock
                {
                    Margin = new Thickness(0, 0, 0, 50),
                    Text = count == 0 ? "" : count.ToString(),
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    FontSize = 80,
                    Foreground = new SolidColorBrush(Colors.White)
                };

                grid.Children.Add(txtItem);


                // bg image
                var source = new BitmapImage();
                var lck = new ManualResetEvent(true);
                source.ImageOpened += (a, b) =>
                {
                    lck.Set();
                };
                source.UriSource = new Uri("/images/tile.front" + (count == 0 ? "" : ".c") + ".png", UriKind.Relative);
                lck.WaitOne();

                Children.Add(new Image { Source = source });
                Children.Add(grid);
            }
        }
    }
}

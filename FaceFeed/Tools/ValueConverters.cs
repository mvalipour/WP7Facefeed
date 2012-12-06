using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Globalization;
using FaceFeed.Model;

namespace System.WindowsPhone
{
    public class FontDetector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (RTLDetector.IsRTL("{0}".FormatWith(value))) return new FontSource(Application.GetResourceStream(new Uri("/Fonts/Nazanin.zip")).Stream);

            return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ItemEmptinessToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NotificationUnreadBackgroundConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as Notification;

            var change = true;
            if (item == null) change = false;
            if (item.Unread == false) change= false;

            return FaceFeed.App.Current.Resources[change ? "PhoneAccentBrush" : "AppForegroundFadeBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NotificationUnreadWeightConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as Notification;
            if (item == null) return null;
            if (item.Unread == false) return null;

            return FontWeights.Bold;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class RTLDetector : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return IsRTL("{0}".FormatWith(value)) ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static bool IsRTL(string str)
        {
            var rtl = false;

            foreach (var ch in str)
            {
                if (ch >= 1548 && ch <= 1790)
                {
                    rtl = true;
                    break;
                }
            }

            return rtl;
        }
    }
    public class PageMenuImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BitmapImage(new Uri("/images/menu/{0}.png".FormatWith(value), UriKind.Relative));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class StringEmptinessToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "{0}".FormatWith(value).HasValue() ? Visibility.Visible : Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class DateTimeDifferenceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime)) return null;

            var date = (DateTime)value;
            var diff = DateTime.Now - date;

            if (diff.TotalSeconds < 0) diff = TimeSpan.Zero;

            if (diff.TotalMinutes < 1) return "{0} seconds ago".FormatWith((int)diff.TotalSeconds);
            if (diff.TotalHours < 1) return "{0} minutes ago".FormatWith((int)diff.TotalMinutes);
            if (diff.TotalDays < 1) return "{0} hours ago".FormatWith((int)diff.TotalHours);
            if (diff.TotalDays < 7) return "on {0}".FormatWith(date.DayOfWeek).ToString();
            if (date.Year >= DateTime.Now.Year) return "{0:MMMMM dd}".FormatWith(date);

            return "{0:MMMMM dd yyyy}".FormatWith(date).ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ByteArrayToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is byte[])) return null;

            var bytes = (byte[])value;

            try
            {
                return GetSource(bytes);
            }
            catch (Exception ex)
            {
                // WTF! it's gif!

                if (parameter == null)
                {
                    var defaultBytes = System.Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAIAAACRXR/mAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAIhSURBVFhH5ZltT8JAEIT7/3+XQRMFRcQXggawBkuQiJUKiOAkTZqK0O7MtdJEwgdS7srTubndvcWrnfcr+PYqyAQk76jeq+D732A1b4cP/mQyjeaLz9OWry1FMWqdNAfd/ss0nG9SL1zRmDDLFavdDcLZIk2TfIZsB8A6a/nRx3InUHKx0xtrZKJacE82UPLtaPIukClYwTg0MsXDICpLRmM9BW8Uk0bGYWHDC0zxFMpnHJYmVYw1i4il5LC2IhOl3Hq9sTuMw4rmOREhG7QsrMVyRSm0NRilgZGMU8sVq1EOFrJvFdXKzTaH8da+rGyU0GgsuoJwCRCrr3VZWGw2TKsIX5aCVW/7jjvxuhsYyYgA4SIVm7AJLORao7X3DcODFa8WimBHLHt1T6h1fDFwxLq8ey5eLdzREct+PiPUApaLvezGosOpi2A4SxpXUMHSAj1VAypY2E2Cw3D2t0ulYNUafQHrqmPdgzE9Z/l4Dh6dIqOStI7FGl9oRihqAYsyPuUqJ7WoevDvsKgKx37gSR5AXETK8mg5sYIpWGzORmuudCzkEEqqeDBLRquFHxCw0FOhBKOxtKYN1a5RojxCtqAWptiLLQILm/xx+KoBJbPQRzXC5S8itjdu5wiUno4MkVs9Z2HhWEdFcwodATnjxLEDC2EJ6yV7iILDYBTTv+PtDyykerZoYSH2jcdWvbkfbScfXHLsXRXChyXCHxGA+wa6mMyUga9ScwAAAABJRU5ErkJggg==");
                    return Convert(defaultBytes, targetType, true, culture);
                }

                return null;
            }
        }

        public static object GetSource(byte[] bytes)
        {
            using (var memstr = new System.IO.MemoryStream(bytes))
            {
                var source = new BitmapImage();
                source.SetSource(memstr);
                return source;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new byte[0];
        }
    }
}

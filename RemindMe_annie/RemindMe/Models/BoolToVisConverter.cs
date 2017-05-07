using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace RemindMe.Models
{
    public class BoolToVisConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Visibility Vis = (Visibility)value;
            return Vis == Visibility.Collapsed ? true : false;
        }
    }
}

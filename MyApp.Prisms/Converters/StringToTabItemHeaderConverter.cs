using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MyApp.Prisms.Converters
{
    internal class StringToTabItemHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return value;
            }

            if(value is TabItem tabItem)
            {
                return tabItem.Header?.ToString();
            }

            throw new InvalidOperationException();
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TcpSocket.Converters
{
    public class DigitalToCornerRadiusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                return new CornerRadius((double) value / 2);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
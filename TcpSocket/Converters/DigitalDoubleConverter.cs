using System;
using System.Globalization;
using System.Windows.Data;

namespace TcpSocket.Converters
{
    public class DigitalDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double mul = 1;
                
                double.TryParse((string) parameter, out mul);

                return (double) value * mul;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
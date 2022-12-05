using System;
using System.Globalization;
using System.Windows.Data;

namespace TcpSocket.Converters
{
    public class DigitalSubConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double)
            {
                double sub = 1;
                
                double.TryParse((string) parameter, out sub);

                return (double) value - sub;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
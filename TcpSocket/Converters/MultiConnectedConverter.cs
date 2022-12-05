using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace TcpSocket.Converters
{
    public class MultiConnectedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return !values.Any(val=>(bool)val);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

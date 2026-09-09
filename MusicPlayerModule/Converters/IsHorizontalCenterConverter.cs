using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicPlayerModule.Converters
{
    internal class IsHorizontalCenterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is HorizontalAlignment horizontalAlignment)
            {
                return horizontalAlignment == HorizontalAlignment.Center || horizontalAlignment == HorizontalAlignment.Stretch;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

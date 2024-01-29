using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MusicPlayerModule.Converters
{
    internal class VerticalDesktopLyricMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2)
            {
                if (values[0] is bool isShow && isShow && values[1] is bool isVertical && isVertical)
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Globalization;
using System.Windows.Data;

namespace MusicPlayerModule.Converters
{
    /// <summary>
    /// 歌词可见部分高度
    /// </summary>
    internal class LyricHeightMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length > 1)
            {
                if (values[0] is double left && values[1] is double right)
                {
                    if (left > 0 && right > 0)
                    {
                        return Math.Abs(left - right) - 33;
                    }

                    return left;
                }
            }

            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

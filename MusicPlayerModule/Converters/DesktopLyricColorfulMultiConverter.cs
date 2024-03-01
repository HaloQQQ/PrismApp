using System.Globalization;
using System.Windows.Data;

namespace MusicPlayerModule.Converters
{
    internal class DesktopLyricColorfulMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 5)
            {
                return 0d;
            }

            if (values[0] is not bool || values[1] is not int || values[2] is not bool || values[3] is not double)
            {
                return 0d;
            }

            var fontSize = double.Parse(values[4].ToString());

            if (values[0] is bool isPlayed && isPlayed)
            {
                var totalWords = int.Parse(values[1].ToString());
                return totalWords * fontSize;
            }

            var isPlayingLine = bool.Parse(values[2].ToString());
            var wordProgress = double.Parse(values[3].ToString());

            return wordProgress * fontSize * (isPlayingLine ? 1 : 0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

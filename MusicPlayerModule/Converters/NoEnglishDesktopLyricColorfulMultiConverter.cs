using System.Globalization;
using System.Windows.Data;

namespace MusicPlayerModule.Converters
{
    /// <summary>
    /// 非英文垂直桌面歌词
    /// </summary>
    internal class NoEnglishDesktopLyricColorfulMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 5)
            {
                return 0d;
            }

            if (values[0] is not bool || values[1] is not bool || values[2] is not double)
            {
                return 0d;
            }

            var wordsLength = int.Parse(values[3].ToString());
            var fontSize = double.Parse(values[4].ToString());

            var wordsHeight = wordsLength * fontSize;

            if (values[0] is bool isPlayed && isPlayed)
            {
                return wordsHeight;
            }

            var isPlayingLine = bool.Parse(values[1].ToString());
            if (!isPlayingLine)
            {
                return 0d;
            }

            var wordProgress = double.Parse(values[2].ToString());

            return wordProgress * wordsHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

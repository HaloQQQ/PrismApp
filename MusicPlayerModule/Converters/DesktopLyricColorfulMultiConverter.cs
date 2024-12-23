using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;

namespace MusicPlayerModule.Converters
{
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8604 // 引用类型参数可能为 null。
    /// <summary>
    /// 桌面歌词
    /// </summary>
    internal class DesktopLyricColorfulMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 4)
            {
                return 0d;
            }

            if (values[0] is not bool || values[1] is not bool || values[2] is not double)
            {
                return 0d;
            }

            var text = values[3] as TextBlock;
            FormattedText formattedText = new FormattedText(
                text.Text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface(text.FontFamily.ToString()),
                      text.FontSize,
                      Brushes.Black,
                      VisualTreeHelper.GetDpi(text).PixelsPerDip
                    );

            var lineWidth = formattedText.WidthIncludingTrailingWhitespace;

            if (values[0] is bool isPlayed && isPlayed)
            {
                return lineWidth;
            }

            var isPlayingLine = bool.Parse(values[1].ToString());
            if (!isPlayingLine)
            {
                return 0d;
            }

            var wordProgress = double.Parse(values[2].ToString());

            return wordProgress * lineWidth;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

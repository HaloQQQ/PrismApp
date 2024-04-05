using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MusicPlayerModule.Converters
{
    /// <summary>
    /// 多行歌词进度转换宽度
    /// </summary>
    internal class LyricWidth_MultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isPlayingLine = bool.Parse(values[0].ToString());
            if (!isPlayingLine)
            {
                return 0d;
            }

            double lineProgress = double.Parse(values[1].ToString());
            double num = lineProgress;

            TextBlock text = values[2] as TextBlock;
            FormattedText formattedText = new FormattedText(
                        text.Text,
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        new Typeface(text.FontFamily.ToString()),
                        text.FontSize,
                        Brushes.Black,
                        VisualTreeHelper.GetDpi(text).PixelsPerDip
                        );

            return num * formattedText.WidthIncludingTrailingWhitespace;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

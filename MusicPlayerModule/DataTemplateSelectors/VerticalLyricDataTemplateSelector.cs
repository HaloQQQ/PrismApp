using MusicPlayerModule.Utils;
using System.Windows;
using System.Windows.Controls;

namespace MusicPlayerModule.DataTemplateSelectors
{
    internal class VerticalLyricDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate VerticalDesktopLyricWithoutEnglish { get; set; }
        public DataTemplate VerticalDesktopLyricWithEnglish { get; set; }


        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var lyricLine = item as KRCLyricsLine;

            bool isEnglish = false;

            if (lyricLine != null)
            {
                isEnglish = lyricLine.IsEnglish;
            }

            return isEnglish ? VerticalDesktopLyricWithEnglish : VerticalDesktopLyricWithoutEnglish;
        }
    }
}

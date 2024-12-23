using MusicPlayerModule.Utils;
using System.Windows;
using System.Windows.Controls;

namespace MusicPlayerModule.DataTemplateSelectors
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
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

using IceTea.Pure.Utils;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MyApp.Prisms.Models
{
    internal class ThemeModel
    {
        private ThemeModel(ThemeType name, string colorStr, string themeFileUri)
        {
            Name = name;
            ColorStr = colorStr.AssertArgumentNotNull(nameof(ColorStr));
            ThemeFileUri = new ResourceDictionary()
            {
                Source = new Uri(themeFileUri.AssertArgumentNotNull(nameof(themeFileUri)), UriKind.RelativeOrAbsolute)
            };
        }

        public ThemeType Name { get; private set; }
        public string ColorStr { get; private set; }
        public ResourceDictionary ThemeFileUri { get; private set; }

        public static IEnumerable<ThemeModel> Create()
        {
            yield return new ThemeModel(ThemeType.Light, "#66FFFFFF", "pack://application:,,,/IceTea.Wpf.Core;component/Resources/LightTheme.xaml");
            yield return new ThemeModel(ThemeType.Dark, "#66000000", "pack://application:,,,/IceTea.Wpf.Core;component/Resources/DarkTheme.xaml");
            yield return new ThemeModel(ThemeType.Spring, "#FF6EE7A8", "pack://application:,,,/MyApp.Prisms;component/Resources/SpringTheme.xaml");
            yield return new ThemeModel(ThemeType.Summer, "#FF00CED1", "pack://application:,,,/MyApp.Prisms;component/Resources/SummerTheme.xaml");
            yield return new ThemeModel(ThemeType.Autumn, "#FFDAA520", "pack://application:,,,/MyApp.Prisms;component/Resources/AutumnTheme.xaml");
            yield return new ThemeModel(ThemeType.Winter, "#FF87CEEB", "pack://application:,,,/MyApp.Prisms;component/Resources/WinterTheme.xaml");
        }
    }

    internal enum ThemeType
    {
        Light,
        Dark,
        Spring,
        Summer,
        Autumn,
        Winter,
    }
}

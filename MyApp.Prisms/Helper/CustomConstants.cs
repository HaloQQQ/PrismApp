using System.Windows;
using System;
using System.Windows.Media;
using IceTea.Atom.Utils.HotKey.GlobalHotKey;

namespace MyApp.Prisms.Helper
{
    internal static class CustomConstants
    {
        internal const string SEND_MSG = "SendMsg";

        internal const string LOG = "Log";

        internal const string NOTE_PAD = "notepad";

        internal const string NOTE_PAD_EXE = "notepad.exe";

        internal const string TaskBar_Resource_Key = "Taskbar";

        internal const string Container_Background = "Container.Background";

        internal const string MAX = "Max";
        internal const string NORMAL = "Normal";
        internal const string MIN = "Min";
        internal const string CLOSE = "Close";

        internal const string ONLY_ONE_PROCESS = "OnlyOneProcess";
        internal const string AUTO_START = "AutoStart";
        internal const string BACKGROUND_SWITCH = "BackgroundSwitch";

        internal const string IsVideoPlayer = "IsVideoPlayer";
        internal const string IsMusicPlayer = "IsMusicPlayer";

        internal const string DefaultThemeURI = "DefaultThemeURI";
        internal const string BkgrdUri = "BkgrdUri";

         

        internal const string Software_Log_Dir = "软件启停记录";

        internal static readonly Brush SendBrush = Brushes.LightSkyBlue;
        internal static readonly Brush RecvBrush = Brushes.LightGreen;

        internal static readonly ResourceDictionary Light = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/IceTea.Wpf.Core;component/Resources/LightTheme.xaml", UriKind.RelativeOrAbsolute)
        };

        internal static readonly ResourceDictionary Dark = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/IceTea.Wpf.Core;component/Resources/DarkTheme.xaml", UriKind.RelativeOrAbsolute)
        };

        internal static readonly string[] WindowCornerRadius = new string[] { "WindowCornerRadius" };

        internal static readonly string[] MailAccounts = new string[] { "MailAccounts" };

        internal static readonly string[] ConfigGlobalHotkeys = new string[] { "HotKeys", "Global" };
        internal static class GlobalHotKeysConst
        {
            internal const string Pause = "暂停";
            internal const string Prev = "上一个";
            internal const string Next = "下一个";
            internal const string UpScreenBright = "提高屏幕亮度";
            internal const string DownScreenBright = "降低屏幕亮度";
        }

        internal static GlobalHotKeyModel[] GlobalHotKeys = new GlobalHotKeyModel[] {
            new GlobalHotKeyModel(GlobalHotKeysConst.Pause, CustomModifierKeys.Alt, CustomKeys.S),
            new GlobalHotKeyModel(GlobalHotKeysConst.Prev, CustomModifierKeys.Alt, CustomKeys.Left),
            new GlobalHotKeyModel(GlobalHotKeysConst.Next, CustomModifierKeys.Alt, CustomKeys.Right),
            new GlobalHotKeyModel(GlobalHotKeysConst.UpScreenBright, CustomModifierKeys.Alt, CustomKeys.F3),
            new GlobalHotKeyModel(GlobalHotKeysConst.DownScreenBright, CustomModifierKeys.Alt, CustomKeys.F2),
        };
    }
}
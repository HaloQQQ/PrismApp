using System.Windows;
using System;
using System.Windows.Media;
using IceTea.Atom.Utils.HotKey.Global.Contracts;

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

            internal const string Ahead = "快进";
            internal const string Delay = "快退";

            internal const string IncreaseVolume = "增大音量";
            internal const string DecreaseVolume = "降低音量";

            internal const string UpScreenBright = "提高屏幕亮度";
            internal const string DownScreenBright = "降低屏幕亮度";

            internal const string MusicLyricDesktop = "歌词桌面栏";
        }

        internal class GlobalHotKeyInfo
        {
            public GlobalHotKeyInfo(string name, CustomModifierKeys customModifierKeys, CustomKeys customKeys, bool isUsable = true)
            {
                Name = name;
                CustomModifierKeys = customModifierKeys;
                CustomKeys = customKeys;
                IsUsable = isUsable;
            }

            public string Name { get; }
            public CustomModifierKeys CustomModifierKeys { get; }
            public CustomKeys CustomKeys { get; }
            public bool IsUsable { get; }
        }

        internal static GlobalHotKeyInfo[] GlobalHotKeys = {
            new GlobalHotKeyInfo(GlobalHotKeysConst.Pause, CustomModifierKeys.Alt, CustomKeys.S),

            new GlobalHotKeyInfo(GlobalHotKeysConst.Prev, CustomModifierKeys.Alt, CustomKeys.Left),
            new GlobalHotKeyInfo(GlobalHotKeysConst.Next, CustomModifierKeys.Alt, CustomKeys.Right),

            new GlobalHotKeyInfo(GlobalHotKeysConst.Delay, CustomModifierKeys.Control|CustomModifierKeys.Shift, CustomKeys.Left),
            new GlobalHotKeyInfo(GlobalHotKeysConst.Ahead, CustomModifierKeys.Control|CustomModifierKeys.Shift, CustomKeys.Right),

            new GlobalHotKeyInfo(GlobalHotKeysConst.IncreaseVolume, CustomModifierKeys.Control|CustomModifierKeys.Shift, CustomKeys.Up),
            new GlobalHotKeyInfo(GlobalHotKeysConst.DecreaseVolume, CustomModifierKeys.Control|CustomModifierKeys.Shift, CustomKeys.Down),

            new GlobalHotKeyInfo(GlobalHotKeysConst.UpScreenBright, CustomModifierKeys.Alt, CustomKeys.F3),
            new GlobalHotKeyInfo(GlobalHotKeysConst.DownScreenBright, CustomModifierKeys.Alt, CustomKeys.F2),

            new GlobalHotKeyInfo(GlobalHotKeysConst.MusicLyricDesktop, CustomModifierKeys.Alt, CustomKeys.C)
        };
    }
}
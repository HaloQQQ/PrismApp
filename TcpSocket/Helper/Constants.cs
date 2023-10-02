using System.Windows;
using System;
using System.Windows.Media;

namespace TcpSocket.Helper
{
    internal static class Constants
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

        internal const string Hotkeys = "Hotkeys";


        internal const string Software_Log_Dir = "软件启停记录";

        internal const string Image_Dir = "E:\\图片";

        internal static readonly Brush SendBrush = Brushes.LightSkyBlue;
        internal static readonly Brush RecvBrush = Brushes.LightGreen;

        internal static readonly ResourceDictionary Light = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/WpfStyleResources;component/Resources/LightTheme.xaml", UriKind.RelativeOrAbsolute)
        };

        internal static readonly ResourceDictionary Dark = new ResourceDictionary()
        {
            Source = new Uri("pack://application:,,,/WpfStyleResources;component/Resources/DarkTheme.xaml", UriKind.RelativeOrAbsolute)
        };

        internal static class HotKeys
        {
            internal const string Pause = "暂停";
            internal const string Prev = "上一个";
            internal const string Next = "下一个";
            internal const string AppFull = "程序全覆盖";
            internal const string ClearAB = "清空AB点";
        }
    }
}
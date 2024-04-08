using IceTea.General.Utils.AppHotKey;
using MusicPlayerModule.Models.Common;
using System.Windows.Input;

namespace MusicPlayerModule.Common
{
    internal static class CustomStatics
    {
        internal static string LastMusicDir;

        internal static string LyricDir;

        internal static string LastVideoDir;

        public static List<MediaPlayOrderModel> MediaPlayOrderList { get; private set; } = new()
        {
            new MediaPlayOrderModel("\ue871","顺序播放", MediaPlayOrderModel.EnumOrderType.Order),
            new MediaPlayOrderModel("\ue66c","循环播放", MediaPlayOrderModel.EnumOrderType.Loop),
            new MediaPlayOrderModel("\ue66b","随机播放", MediaPlayOrderModel.EnumOrderType.Random),
            new MediaPlayOrderModel("\ue66d","单曲循环", MediaPlayOrderModel.EnumOrderType.SingleCycle),
            new MediaPlayOrderModel("\ue621","单曲播放", MediaPlayOrderModel.EnumOrderType.SingleOnce)
        };

        #region Music
        internal const string MUSIC = "Music";

        internal static readonly string[] MusicPlayOrder_ConfigKey = new[] { MUSIC, "MusicPlayOrder" };
        internal static readonly string[] LastMusicDir_ConfigKey = new[] { MUSIC, nameof(LastMusicDir) };
        internal static readonly string[] LyricDir_ConfigKey = new[] { MUSIC, nameof(LyricDir) };

        internal static readonly string[] Vertical_DesktopLyric_WindowLeftTop_ConfigKey = new string[] { MUSIC, "Vertical_DesktopLyric_WindowLeftTop" };
        internal static readonly string[] Horizontal_DesktopLyric_WindowLeftTop_ConfigKey = new string[] { MUSIC, "Horizontal_DesktopLyric_WindowLeftTop" };

        internal static readonly string[] IsDesktopLyricShow_ConfigKey = new[] { MUSIC, "IsDesktopLyricShow" };
        internal static readonly string[] IsVertical_ConfigKey = new[] { MUSIC, "IsVertical" };
        internal static readonly string[] IsSingleLine_ConfigKey = new[] { MUSIC, "IsSingleLine" };

        internal static readonly string[] CurrentLyricForeground_ConfigKey = new[] { MUSIC, "CurrentLyricForeground" };
        internal static readonly string[] CurrentLyricFontSize_ConfigKey = new[] { MUSIC, "CurrentLyricFontSize" };

        internal static readonly string[] LinearGradientLyricColor_ConfigKey = new[] { MUSIC, "LinearGradientLyricColor" };

        internal static readonly string[] AppMusicHotkeys = new string[] { "HotKeys", "App", MUSIC };
        internal static readonly string[] AppVideoHotkeys = new string[] { "HotKeys", "App", "Video" };
        #endregion

        #region Video
        internal const string VIDEO = "Video";
        internal const string VideoABPoints = "VideoABPoints";

        internal static readonly string[] VideoPlayOrder_ConfigKey = new[] { VIDEO, "VideoPlayOrder" };
        internal static readonly string[] VideoStretch_ConfigKey = new[] { VIDEO, "VideoStretch" };

        internal static readonly string[] LastVideoDir_ConfigKey = new[] { VIDEO, nameof(LastVideoDir) };


        #endregion

        private static class AppMediaHotKeys
        {
            internal const string ResetPointAB = "重置AB点";
            internal const string SetPointA = "设置A点";
            internal const string SetPointB = "设置B点";

            internal const string DecreaseVolume = "降低音量";
            internal const string IncreaseVolume = "提高音量";

            internal const string PlayPlaying = "播放/暂停";
            internal const string CleanPlaying = "清空播放队列";
            internal const string StopPlayMusic = "停止";
            internal const string PlayingListPanel = "播放列表";

            internal const string OpenFolder = "打开文件夹";

            internal const string MoveToHome = "返回开头";
            internal const string MoveToEnd = "跳至结尾";

            internal const string Rewind = "快退";
            internal const string FastForward = "快进";

            internal const string Prev = "上一个";
            internal const string Next = "下一个";
        }

        private static class AppMusicHotKeys
        {
            internal const string DesktopLyric = "桌面歌词";
            internal const string PlayAllMusic = "播放所有音乐";

            internal const string LyricPanel = "歌词封面";

            internal const string Find = "搜索";
        }

        internal static AppHotKeyModel[] MusicHotKeys = new AppHotKeyModel[] {
            new AppHotKeyModel(AppMediaHotKeys.ResetPointAB, ModifierKeys.Control, Key.Delete),
            new AppHotKeyModel(AppMediaHotKeys.SetPointA, ModifierKeys.Control, Key.D1),
            new AppHotKeyModel(AppMediaHotKeys.SetPointB, ModifierKeys.Control, Key.D2),

            new AppHotKeyModel(AppMediaHotKeys.MoveToHome, ModifierKeys.None, Key.Home),
            new AppHotKeyModel(AppMediaHotKeys.MoveToEnd, ModifierKeys.None, Key.End),
            new AppHotKeyModel(AppMediaHotKeys.Rewind, ModifierKeys.None, Key.Left),
            new AppHotKeyModel(AppMediaHotKeys.FastForward, ModifierKeys.None, Key.Right),
            new AppHotKeyModel(AppMediaHotKeys.Prev, ModifierKeys.None, Key.PageUp),
            new AppHotKeyModel(AppMediaHotKeys.Next, ModifierKeys.None, Key.PageDown),

            new AppHotKeyModel(AppMusicHotKeys.DesktopLyric, ModifierKeys.Alt, Key.C),
            new AppHotKeyModel(AppMediaHotKeys.PlayingListPanel, ModifierKeys.Alt, Key.X),
            new AppHotKeyModel(AppMusicHotKeys.LyricPanel, ModifierKeys.None, Key.Escape),

            new AppHotKeyModel(AppMusicHotKeys.Find, ModifierKeys.Control, Key.F),

            new AppHotKeyModel(AppMediaHotKeys.DecreaseVolume, ModifierKeys.Control, Key.Down),
            new AppHotKeyModel(AppMediaHotKeys.IncreaseVolume, ModifierKeys.Control, Key.Up),

            new AppHotKeyModel(AppMediaHotKeys.OpenFolder, ModifierKeys.Control, Key.O),
            new AppHotKeyModel(AppMusicHotKeys.PlayAllMusic, ModifierKeys.Alt, Key.P),

            new AppHotKeyModel(AppMediaHotKeys.PlayPlaying, ModifierKeys.Alt, Key.F),
            new AppHotKeyModel(AppMediaHotKeys.CleanPlaying, ModifierKeys.Alt, Key.D),
            new AppHotKeyModel(AppMediaHotKeys.StopPlayMusic, ModifierKeys.Alt, Key.W)
        };

        internal static AppHotKeyModel[] VideoHotKeys = new AppHotKeyModel[] {
            new AppHotKeyModel(AppMediaHotKeys.ResetPointAB, ModifierKeys.Control, Key.Delete),
            new AppHotKeyModel(AppMediaHotKeys.SetPointA, ModifierKeys.Control, Key.D1),
            new AppHotKeyModel(AppMediaHotKeys.SetPointB, ModifierKeys.Control, Key.D2),

            new AppHotKeyModel(AppMediaHotKeys.MoveToHome, ModifierKeys.None, Key.Home),
            new AppHotKeyModel(AppMediaHotKeys.MoveToEnd, ModifierKeys.None, Key.End),
            new AppHotKeyModel(AppMediaHotKeys.Rewind, ModifierKeys.None, Key.Left),
            new AppHotKeyModel(AppMediaHotKeys.FastForward, ModifierKeys.None, Key.Right),
            new AppHotKeyModel(AppMediaHotKeys.Prev, ModifierKeys.None, Key.PageUp),
            new AppHotKeyModel(AppMediaHotKeys.Next, ModifierKeys.None, Key.PageDown),

            new AppHotKeyModel(AppMediaHotKeys.DecreaseVolume, ModifierKeys.Control, Key.Down),
            new AppHotKeyModel(AppMediaHotKeys.IncreaseVolume, ModifierKeys.Control, Key.Up),

            new AppHotKeyModel(AppMediaHotKeys.OpenFolder, ModifierKeys.Control, Key.O),

            new AppHotKeyModel(AppMediaHotKeys.CleanPlaying, ModifierKeys.Alt, Key.D),
            new AppHotKeyModel(AppMediaHotKeys.StopPlayMusic, ModifierKeys.Alt, Key.W),

            new AppHotKeyModel(AppMediaHotKeys.PlayPlaying, ModifierKeys.None, Key.Space),

            new AppHotKeyModel(AppMediaHotKeys.PlayingListPanel, ModifierKeys.Alt, Key.X),
        };
    }
}

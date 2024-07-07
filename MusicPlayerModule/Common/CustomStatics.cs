using IceTea.General.Utils.HotKey.App;
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

        internal static AppHotKey[] MusicHotKeys = new AppHotKey[] {
            new AppHotKey(AppMediaHotKeys.ResetPointAB, Key.Delete, ModifierKeys.Control),
            new AppHotKey(AppMediaHotKeys.SetPointA, Key.D1, ModifierKeys.Control),
            new AppHotKey(AppMediaHotKeys.SetPointB, Key.D2, ModifierKeys.Control),

            new AppHotKey(AppMediaHotKeys.MoveToHome, Key.Home, ModifierKeys.None),
            new AppHotKey(AppMediaHotKeys.MoveToEnd, Key.End, ModifierKeys.None),
            new AppHotKey(AppMediaHotKeys.Rewind, Key.Left, ModifierKeys.None),
            new AppHotKey(AppMediaHotKeys.FastForward, Key.Right, ModifierKeys.None),
            new AppHotKey(AppMediaHotKeys.Prev, Key.PageUp, ModifierKeys.None),
            new AppHotKey(AppMediaHotKeys.Next, Key.PageDown, ModifierKeys.None),

            new AppHotKey(AppMusicHotKeys.DesktopLyric, Key.C, ModifierKeys.Alt),
            new AppHotKey(AppMediaHotKeys.PlayingListPanel, Key.X, ModifierKeys.Alt),
            new AppHotKey(AppMusicHotKeys.LyricPanel, Key.Escape, ModifierKeys.None),

            new AppHotKey(AppMusicHotKeys.Find, Key.F, ModifierKeys.Control),

            new AppHotKey(AppMediaHotKeys.DecreaseVolume, Key.Down, ModifierKeys.Control),
            new AppHotKey(AppMediaHotKeys.IncreaseVolume, Key.Up, ModifierKeys.Control),

            new AppHotKey(AppMediaHotKeys.OpenFolder, Key.O, ModifierKeys.Control),
            new AppHotKey(AppMusicHotKeys.PlayAllMusic, Key.P, ModifierKeys.Alt),

            new AppHotKey(AppMediaHotKeys.PlayPlaying, Key.F, ModifierKeys.Alt),
            new AppHotKey(AppMediaHotKeys.CleanPlaying, Key.D, ModifierKeys.Alt),
            new AppHotKey(AppMediaHotKeys.StopPlayMusic, Key.W, ModifierKeys.Alt)
        };

        internal static AppHotKey[] VideoHotKeys = new AppHotKey[] {
            new AppHotKey(AppMediaHotKeys.ResetPointAB, Key.Delete, ModifierKeys.Control),
            new AppHotKey(AppMediaHotKeys.SetPointA, Key.D1, ModifierKeys.Control),
            new AppHotKey(AppMediaHotKeys.SetPointB, Key.D2, ModifierKeys.Control),

            new AppHotKey(AppMediaHotKeys.MoveToHome, Key.Home, ModifierKeys.None),
            new AppHotKey(AppMediaHotKeys.MoveToEnd, Key.End, ModifierKeys.None),
            new AppHotKey(AppMediaHotKeys.Rewind, Key.Left, ModifierKeys.None),
            new AppHotKey(AppMediaHotKeys.FastForward, Key.Right, ModifierKeys.None),
            new AppHotKey(AppMediaHotKeys.Prev, Key.PageUp, ModifierKeys.None),
            new AppHotKey(AppMediaHotKeys.Next, Key.PageDown, ModifierKeys.None),

            new AppHotKey(AppMediaHotKeys.DecreaseVolume, Key.Down, ModifierKeys.Control),
            new AppHotKey(AppMediaHotKeys.IncreaseVolume, Key.Up, ModifierKeys.Control),

            new AppHotKey(AppMediaHotKeys.OpenFolder, Key.O, ModifierKeys.Control),

            new AppHotKey(AppMediaHotKeys.CleanPlaying, Key.D, ModifierKeys.Alt),
            new AppHotKey(AppMediaHotKeys.StopPlayMusic, Key.W, ModifierKeys.Alt),

            new AppHotKey(AppMediaHotKeys.PlayPlaying, Key.Space, ModifierKeys.None),

            new AppHotKey(AppMediaHotKeys.PlayingListPanel, Key.X, ModifierKeys.Alt),
        };
    }
}

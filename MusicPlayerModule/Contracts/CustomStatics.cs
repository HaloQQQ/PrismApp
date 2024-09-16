namespace MusicPlayerModule.Contracts
{
    public static class CustomStatics
    {
        public enum EnumSettings
        {
            Music,
            Lyric,
            Video
        }

        public static List<MediaPlayOrderModel> MediaPlayOrderList { get; private set; } = new()
        {
            new MediaPlayOrderModel("\ue871","顺序播放", MediaPlayOrderModel.EnumOrderType.Order),
            new MediaPlayOrderModel("\ue66c","循环播放", MediaPlayOrderModel.EnumOrderType.Loop),
            new MediaPlayOrderModel("\ue66b","随机播放", MediaPlayOrderModel.EnumOrderType.Random),
            new MediaPlayOrderModel("\ue66d","单曲循环", MediaPlayOrderModel.EnumOrderType.SingleCycle),
            new MediaPlayOrderModel("\ue621","单曲播放", MediaPlayOrderModel.EnumOrderType.SingleOnce)
        };

        #region Music
        public static string MUSIC = EnumSettings.Music.ToString();
        public static string LYRIC = EnumSettings.Lyric.ToString();

        //internal static readonly string[] MusicPlayOrder_ConfigKey = new[] { MUSIC, "MusicPlayOrder" };
        public static readonly string[] LastMusicDir_ConfigKey = new[] { MUSIC, "LastMusicDir" };
        public static readonly string[] LastLyricDir_ConfigKey = new[] { MUSIC, "LyricDir" };

        internal static readonly string[] Vertical_DesktopLyric_WindowLeftTop_ConfigKey = new string[] { MUSIC, "Vertical_DesktopLyric_WindowLeftTop" };
        internal static readonly string[] Horizontal_DesktopLyric_WindowLeftTop_ConfigKey = new string[] { MUSIC, "Horizontal_DesktopLyric_WindowLeftTop" };

        internal static readonly string[] IsDesktopLyricShow_ConfigKey = new[] { MUSIC, "IsDesktopLyricShow" };
        internal static readonly string[] IsVertical_ConfigKey = new[] { MUSIC, "IsVertical" };
        internal static readonly string[] IsSingleLine_ConfigKey = new[] { MUSIC, "IsSingleLine" };

        internal static readonly string[] CurrentLyricForeground_ConfigKey = new[] { MUSIC, "CurrentLyricForeground" };
        internal static readonly string[] CurrentLyricFontSize_ConfigKey = new[] { MUSIC, "CurrentLyricFontSize" };

        internal static readonly string[] CurrentLyricFontFamily_ConfigKey = new[] { MUSIC, "CurrentLyricFontFamily" };

        internal static readonly string[] LinearGradientLyricColor_ConfigKey = new[] { MUSIC, "LinearGradientLyricColor" };
        #endregion

        #region Video
        public static string VIDEO = EnumSettings.Video.ToString();
        //internal const string VideoABPoints = "VideoABPoints";

        //internal static readonly string[] VideoPlayOrder_ConfigKey = new[] { VIDEO, "VideoPlayOrder" };
        internal static readonly string[] VideoStretch_ConfigKey = new[] { VIDEO, "VideoStretch" };

        public static readonly string[] LastVideoDir_ConfigKey = new[] { VIDEO, "LastVideoDir" };
        #endregion
    }
}

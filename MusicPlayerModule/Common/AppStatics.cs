using MusicPlayerModule.Models.Common;
using System.Collections.Generic;

namespace MusicPlayerModule.Common
{
    internal static class AppStatics
    {
        internal static string LastMusicDir;

        internal static string LastVideoDir;

        public static List<MediaPlayOrderModel> MediaPlayOrderList { get; private set; } = new()
        {
            new MediaPlayOrderModel("\ue871","顺序播放", MediaPlayOrderModel.EnumOrderType.Order),
            new MediaPlayOrderModel("\ue66c","循环播放", MediaPlayOrderModel.EnumOrderType.Loop),
            new MediaPlayOrderModel("\ue66b","随机播放", MediaPlayOrderModel.EnumOrderType.Random),
            new MediaPlayOrderModel("\ue66d","单曲循环", MediaPlayOrderModel.EnumOrderType.SingleCycle),
            new MediaPlayOrderModel("\ue621","单曲播放", MediaPlayOrderModel.EnumOrderType.SingleOnce)
        };
    }
}

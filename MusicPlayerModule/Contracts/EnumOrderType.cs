using System.ComponentModel;

namespace MusicPlayerModule.Contracts
{
    internal enum EnumOrderType
    {
        [Description("顺序播放")]
        Order,
        [Description("循环播放")]
        Loop,
        [Description("随机播放")]
        Random,
        [Description("单曲循环")]
        SingleCycle,
        [Description("单曲播放")]
        SingleOnce
    }
}

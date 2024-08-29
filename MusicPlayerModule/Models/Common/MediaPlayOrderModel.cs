using System.ComponentModel;

namespace MusicPlayerModule.Models.Common
{
    public class MediaPlayOrderModel
    {
        public MediaPlayOrderModel(string iconString, string description, EnumOrderType orderType)
        {
            IconString = iconString;
            OrderType = orderType;
            Description = description;
        }

        public string IconString { get; }
        public string Description { get; }
        public EnumOrderType OrderType { get; }

        public enum EnumOrderType
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
}

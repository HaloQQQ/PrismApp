using MusicPlayerModule.ViewModels;

namespace MusicPlayerModule.MsgEvents.Video.Dtos
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal class VideoModelAndGuid
    {
        public VideoModelAndGuid(Guid guid)
        {
            Guid = guid;
        }

        public PlayingVideoViewModel Video { get; set; }
        public Guid Guid { get; private set; }
    }
}

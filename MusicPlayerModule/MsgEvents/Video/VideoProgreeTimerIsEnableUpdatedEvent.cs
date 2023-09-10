using MusicPlayerModule.MsgEvents.Video.Dtos;
using Prism.Events;

namespace MusicPlayerModule.MsgEvents.Video
{
    /// <summary>
    /// 播放状态改变时启动或停止获取视频进度的Timer
    /// </summary>
    internal class VideoProgreeTimerIsEnableUpdatedEvent : PubSubEvent<BoolAndGuid>
    {
    }
}

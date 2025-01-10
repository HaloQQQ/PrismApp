using Prism.Events;

namespace MusicPlayerModule.MsgEvents.Media
{
    /// <summary>
    /// 播放状态改变时启动或停止获取歌曲进度的Timer
    /// </summary>
    internal class MediaProgressTimerIsEnableUpdatedEvent : PubSubEvent<bool> { }
}

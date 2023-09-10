using Prism.Events;

namespace MusicPlayerModule.MsgEvents
{
    /// <summary>
    /// 更新滚动条到对应歌词
    /// </summary>
    internal class UpdateScrollBarToTargetLyricEvent : PubSubEvent<int>
    {
    }
}

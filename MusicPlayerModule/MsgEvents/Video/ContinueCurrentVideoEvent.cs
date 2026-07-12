using Prism.Events;

namespace MusicPlayerModule.MsgEvents.Video
{
    /// <summary>
    /// 只通知自己
    /// </summary>
    public class ContinueCurrentVideoEvent : PubSubEvent<Guid> { }
}

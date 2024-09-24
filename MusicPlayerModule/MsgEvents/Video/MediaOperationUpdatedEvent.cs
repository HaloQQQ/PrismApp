using Prism.Events;

namespace MusicPlayerModule.MsgEvents.Video
{
    /// <summary>
    /// 用于媒体操作更新通知
    /// </summary>
    internal class MediaOperationUpdatedEvent : PubSubEvent<Guid>
    {
    }
}

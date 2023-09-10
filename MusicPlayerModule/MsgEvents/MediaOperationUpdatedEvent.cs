using Prism.Events;
using System;

namespace MusicPlayerModule.MsgEvents
{
    /// <summary>
    /// 用于媒体操作更新通知
    /// </summary>
    internal class MediaOperationUpdatedEvent : PubSubEvent<Guid>
    {
    }
}

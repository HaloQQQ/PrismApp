using Prism.Events;
using System;

namespace MusicPlayerModule.MsgEvents.Video
{
    internal class ContinueCurrentVideo : PubSubEvent<Guid>
    {
    }
}

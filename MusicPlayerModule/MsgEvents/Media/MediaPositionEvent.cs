using Prism.Events;

namespace MusicPlayerModule.MsgEvents
{
    internal class MediaPositionEvent : PubSubEvent<TimeSpan> { }
}

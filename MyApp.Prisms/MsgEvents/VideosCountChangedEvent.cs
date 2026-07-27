using MyApp.Prisms.Contracts;
using Prism.Events;

namespace MyApp.Prisms.MsgEvents
{
    internal class VideosCountChangedEvent : PubSubEvent<EnumVideosCount>
    {
    }
}

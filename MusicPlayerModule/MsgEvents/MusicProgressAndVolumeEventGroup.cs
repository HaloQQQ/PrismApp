using Prism.Events;

namespace MusicPlayerModule.MsgEvents
{
    public class AheadEvent : PubSubEvent { }

    public class DelayEvent : PubSubEvent { }

    public class IncreaseVolumeEvent : PubSubEvent { }

    public class DecreaseVolumeEvent : PubSubEvent { }
}

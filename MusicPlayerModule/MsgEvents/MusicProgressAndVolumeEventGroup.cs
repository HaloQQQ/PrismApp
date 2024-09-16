using Prism.Events;

namespace MusicPlayerModule.MsgEvents
{
    public class FastForwardEvent : PubSubEvent { }

    public class RewindEvent : PubSubEvent { }

    public class IncreaseVolumeEvent : PubSubEvent { }

    public class DecreaseVolumeEvent : PubSubEvent { }
}

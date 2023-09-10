using Prism.Events;

namespace MusicPlayerModule.MsgEvents
{
    public class PauseCurrentMusicEvent : PubSubEvent { }

    public class ContinueCurrentMusicEvent : PubSubEvent { }

    public class ToggeleCurrentMusicEvent : PubSubEvent { }

    public class PrevMusicEvent : PubSubEvent { }

    public class NextMusicEvent : PubSubEvent { }
}

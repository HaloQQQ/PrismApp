using MusicPlayerModule.Models;
using Prism.Events;

namespace MusicPlayerModule.MsgEvents
{
    internal class BatchAddToPlayingEvent : PubSubEvent<IEnumerable<FavoriteMusicViewModel>>
    {
    }
}

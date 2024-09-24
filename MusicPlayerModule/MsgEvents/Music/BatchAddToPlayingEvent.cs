using MusicPlayerModule.Models;
using Prism.Events;

namespace MusicPlayerModule.MsgEvents.Music
{
    internal class BatchAddToPlayingEvent : PubSubEvent<IEnumerable<FavoriteMusicViewModel>>
    {
    }
}

using MusicPlayerModule.ViewModels;
using Prism.Events;
using System.Collections.Generic;

namespace MusicPlayerModule.MsgEvents
{
    internal class BatchAddToPlayingEvent : PubSubEvent<IEnumerable<FavoriteMusicViewModel>>
    {
    }
}

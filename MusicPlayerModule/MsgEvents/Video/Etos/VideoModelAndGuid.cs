using MusicPlayerModule.ViewModels;

namespace MusicPlayerModule.MsgEvents.Video.Dtos
{
    internal class VideoModelAndGuid
    {
        public VideoModelAndGuid(Guid guid)
        {
            Guid = guid;
        }

        public PlayingVideoViewModel Video { get; set; }
        public Guid Guid { get; private set; }
    }
}

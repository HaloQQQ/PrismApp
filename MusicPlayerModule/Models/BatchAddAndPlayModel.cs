using IceTea.Atom.Utils;

namespace MusicPlayerModule.Models
{
    internal class BatchAddAndPlayModel
    {
        public BatchAddAndPlayModel(FavoriteMusicViewModel? targetToPlay, IEnumerable<FavoriteMusicViewModel> collection)
        {
            TargetToPlay = targetToPlay;
            Collection = collection.AssertNotEmpty(nameof(Collection));
        }

        public FavoriteMusicViewModel TargetToPlay { get; set; }
        public IEnumerable<FavoriteMusicViewModel> Collection { get; private set; }
    }
}

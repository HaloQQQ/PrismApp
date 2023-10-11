using MusicPlayerModule.Models;
using Prism.Mvvm;

namespace MusicPlayerModule.ViewModels
{
    internal class FavoriteMusicViewModel : BindableBase, IDisposable
    {
        private static readonly FavoriteMusicViewModel _empty = new FavoriteMusicViewModel(null);
        public static FavoriteMusicViewModel Empty => _empty;

        public FavoriteMusicViewModel(MusicModel music)
        {
            Music = music;
        }

        private bool _isDeleting;

        public bool IsDeleting
        {
            get { return _isDeleting; }
            set
            {
                if (SetProperty<bool>(ref _isDeleting, value))
                {
                    DeleteStatusChanged?.Invoke(value);
                }
            }
        }

        public static event Action<bool> DeleteStatusChanged;

        public MusicModel Music { get; private set; }

        public void Dispose()
        {
            this.Music.Dispose();
            this.Music = null;
        }
    }
}
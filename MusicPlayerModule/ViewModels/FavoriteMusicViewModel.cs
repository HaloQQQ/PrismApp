using IceTea.Atom.Utils;
using MusicPlayerModule.Models;
using Prism.Mvvm;

namespace MusicPlayerModule.ViewModels
{
    internal class FavoriteMusicViewModel : BindableBase, IDisposable
    {
        public FavoriteMusicViewModel(MusicModel music)
        {
            Music = music.AssertNotNull(nameof(music));
        }

        private int _index;

        public int Index
        {
            get { return _index; }
            set { this._index = value; RaisePropertyChanged(nameof(IndexString)); }
        }

        public string IndexString => this._index.ToString("000");

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
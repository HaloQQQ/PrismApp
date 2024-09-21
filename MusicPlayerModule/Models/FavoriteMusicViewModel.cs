using IceTea.Atom.Utils;
using MusicPlayerModule.Contracts;
using System.Diagnostics;

namespace MusicPlayerModule.Models
{
    [DebuggerDisplay("Index={Index}, IsDeleting={IsDeleting},MusicName={Music.Name}")]
    internal class FavoriteMusicViewModel : ChildrenBase, IDisposable
    {
        public FavoriteMusicViewModel(MusicModel music)
        {
            Music = music.AssertNotNull(nameof(music));
        }

        private int _index;

        public int Index
        {
            get { return _index; }
            set { _index = value; RaisePropertyChanged(nameof(IndexString)); }
        }

        public string IndexString => _index.ToString("000");

        private bool _isDeleting;

        public bool IsDeleting
        {
            get { return _isDeleting; }
            set
            {
                if (SetProperty(ref _isDeleting, value))
                {
                    DeleteStatusChanged?.Invoke(value);
                }
            }
        }

        internal static event Action<bool> DeleteStatusChanged;

        public MusicModel Music { get; private set; }

        public void Dispose()
        {
            Music.Dispose();
            Music = null;
        }
    }
}
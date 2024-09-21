using IceTea.Atom.Utils;
using MusicPlayerModule.Contracts;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace MusicPlayerModule.Models
{
    enum MusicClassifyType
    {
        Album,
        Singer,
        Dir
    }

    [DebuggerDisplay("ClassifyType={ClassifyType}, ClassifyKey={ClassifyKey}, IsSelected={IsSelected}")]
    internal class MusicWithClassifyModel : ChildrenBase
    {
        public MusicClassifyType ClassifyType { get; }

        public MusicWithClassifyModel(string classifyKey, ObservableCollection<FavoriteMusicViewModel> displayByClassifyKeyFavorites, MusicClassifyType musicClassifyType)
        {
            ClassifyKey = classifyKey.AssertNotNull(nameof(classifyKey));
            DisplayByClassifyKeyFavorites = displayByClassifyKeyFavorites.AssertNotNull(nameof(displayByClassifyKeyFavorites));

            DisplayByClassifyKeyFavorites.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (displayByClassifyKeyFavorites.Count == 0)
                    {
                        this.RemoveFromAll();
                    }
                }
            };

            ClassifyType = musicClassifyType;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                if (SetProperty<bool>(ref _isSelected, value) && value)
                {
                    SelectedEvent?.Invoke(this);
                }
            }
        }

        internal static event Action<MusicWithClassifyModel> SelectedEvent;

        public string ClassifyKey { get; private set; }

        public ObservableCollection<FavoriteMusicViewModel> DisplayByClassifyKeyFavorites { get; private set; }
    }
}

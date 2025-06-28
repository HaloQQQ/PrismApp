using IceTea.Atom.BaseModels;
using IceTea.Atom.Utils;
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
                        this.Dispose();
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

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        internal static event Action<MusicWithClassifyModel> SelectedEvent;

        public string ClassifyKey { get; }

        public ObservableCollection<FavoriteMusicViewModel> DisplayByClassifyKeyFavorites { get; }

        public bool Contains(FavoriteMusicViewModel item)
        {
            return this.DisplayByClassifyKeyFavorites.Any(i => i.Equals(item));
        }

        public bool HasUnselected()
        {
            return this.DisplayByClassifyKeyFavorites.Any(_ => !_.IsDeleting);
        }
    }
}

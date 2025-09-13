using IceTea.Pure.BaseModels;
using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using IceTea.Wpf.Core.Utils;
using MusicPlayerModule.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
namespace MusicPlayerModule.Models;

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

    public string ClassifyKey { get; }

    public MusicWithClassifyModel(string classifyKey, ObservableCollection<FavoriteMusicViewModel> displayByClassifyKeyFavorites, MusicClassifyType musicClassifyType)
    {
        ClassifyKey = classifyKey.AssertNotNull(nameof(classifyKey));
        ClassifyFavorites = displayByClassifyKeyFavorites.AssertNotNull(nameof(displayByClassifyKeyFavorites));

        ClassifyFavorites.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) =>
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

    public ObservableCollection<FavoriteMusicViewModel> ClassifyFavorites { get; private set; }

    public bool HasUnselected => this.ClassifyFavorites.Any(_ => !_.IsDeleting);

    public static bool TryCreateDirItemFromPath(Collection<MusicWithClassifyModel> list, string classifyKey)
    {
        classifyKey.AssertNotNull(nameof(classifyKey));
        list.AssertNotEmpty(nameof(list));

        if (!list.Any(item => item.ClassifyKey == classifyKey))
        {
            MusicClassifyType musicClassifyType = list.First().ClassifyType;

            var current = new MusicWithClassifyModel(classifyKey,
                                new ObservableCollection<FavoriteMusicViewModel>(),
                                musicClassifyType);
            current.IsSelected = true;

            current.TryAddTo(list);

            return true;
        }

        return false;
    }

    public bool MoveMusicsTo(Collection<MusicWithClassifyModel> dirs, string defaultDir)
    {
        var targetDir = WpfCoreUtils.OpenFolderDialog(defaultDir);

        if (targetDir.IsNullOrBlank())
        {
            return false;
        }

        TryCreateDirItemFromPath(dirs, targetDir);

        var originDir = this.ClassifyKey;
        var originColls = this.ClassifyFavorites;

        var targetColls = dirs.First(item => item.ClassifyKey == targetDir)
                                .ClassifyFavorites;

        var newColls = originColls.SkipWhile(item => targetColls.Any(m => m.Music.Name == item.Music.Name))
                            .ToList();

        if (newColls.Count > 0)
        {
            foreach (var favorite in newColls)
            {
                favorite.TryAddTo(targetColls);

                favorite.Music.MoveFileTo(targetDir);

                favorite.TryRemoveFrom(originColls);
            }

            originDir.DeleteDirIfEmptyOr(_ => _.IsDirectoryExists() && _.IsDirectoryEmpty());

            return true;
        }

        return false;
    }

    protected override void DisposeCore()
    {
        if (this.ClassifyFavorites.Count > 0)
        {
            this.ClassifyFavorites.Clear();
        }
        this.ClassifyFavorites = null;

        base.DisposeCore();
    }
}

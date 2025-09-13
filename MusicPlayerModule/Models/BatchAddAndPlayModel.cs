using IceTea.Pure.BaseModels;
using IceTea.Pure.Utils;
using MusicPlayerModule.ViewModels;

namespace MusicPlayerModule.Models;

#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
internal class BatchAddAndPlayModel : DisposableBase
{
    public BatchAddAndPlayModel(FavoriteMusicViewModel? targetToPlay, IEnumerable<FavoriteMusicViewModel> collection)
    {
        TargetToPlay = targetToPlay;
        Collection = collection.AssertNotEmpty(nameof(Collection));
    }

    public FavoriteMusicViewModel TargetToPlay { get; private set; }
    public IEnumerable<FavoriteMusicViewModel> Collection { get; private set; }

    protected override void DisposeCore()
    {
        this.TargetToPlay = null;
        this.Collection = null;

        base.DisposeCore();
    }
}

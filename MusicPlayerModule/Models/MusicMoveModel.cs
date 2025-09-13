using IceTea.Pure.BaseModels;
using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;

namespace MusicPlayerModule.Models;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
internal class MusicMoveModel : DisposableBase
{
    public MusicModel Music { get; set; }

    public string MoveToDir { get; set; }

    public bool MoveTo(IEnumerable<MusicWithClassifyModel> classifyDirs)
    {
        var originDir = this.Music.AssertNotNull(nameof(Music)).FileDir;
        var targetDir = this.MoveToDir.AssertNotNull(nameof(MoveToDir));

        AppUtils.AssertDataValidation(!originDir.EqualsIgnoreCase(targetDir), "源目录和目标目录不允许相同");

        var originCollection = classifyDirs.First(item => item.ClassifyKey == originDir)
                                    .ClassifyFavorites;
        var targetCollection = classifyDirs.First(item => item.ClassifyKey == targetDir)
                                    .ClassifyFavorites;

        var item = originCollection.FirstOrDefault(item => item.Music == this.Music);

        item.AssertNotNull($"源目录不存在【{this.Music.Name}】");

#pragma warning disable CS8602 // 解引用可能出现空引用。
        AppUtils.AssertOperationValidation(item.TryAddTo(targetCollection),
            $"{targetDir}已存在【{this.Music.Name}】");

        this.Music.MoveFileTo(this.MoveToDir);

        item.TryRemoveFrom(originCollection);

        originDir.DeleteDirIfEmptyOr(_ => _.IsDirectoryExists() && _.IsDirectoryEmpty());

        return true;
    }

    protected override void DisposeCore()
    {
        this.Music = null;
        this.MoveToDir = null;

        base.DisposeCore();
    }
}

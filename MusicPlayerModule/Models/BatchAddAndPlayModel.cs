using IceTea.Atom.Utils;

namespace MusicPlayerModule.Models
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
    internal class BatchAddAndPlayModel
    {
        public BatchAddAndPlayModel(FavoriteMusicViewModel? targetToPlay, IEnumerable<FavoriteMusicViewModel> collection)
        {
            TargetToPlay = targetToPlay;
            Collection = collection.AssertNotEmpty(nameof(Collection));
        }

        public FavoriteMusicViewModel TargetToPlay { get; }
        public IEnumerable<FavoriteMusicViewModel> Collection { get; }
    }
}

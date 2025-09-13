using IceTea.Pure.BaseModels;
using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using MusicPlayerModule.Models;
using System.Diagnostics;

namespace MusicPlayerModule.ViewModels;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
#pragma warning disable CS8602
[DebuggerDisplay("Index={Index}, IsDeleting={IsDeleting},MusicName={Music.Name}")]
internal class FavoriteMusicViewModel : ChildrenBase, IEquatable<FavoriteMusicViewModel>, IEquatable<PlayingMusicViewModel>
{
    public FavoriteMusicViewModel(MusicModel music)
    {
        Music = music.AssertNotNull(nameof(music));
    }

    private int _index;

    public int Index
    {
        get => _index;
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

    protected override void DisposeCore()
    {
        base.DisposeCore();

        Music.Dispose();
        Music = null;
    }

    public bool Equals(FavoriteMusicViewModel? other)
    {
        return other.IsNotNullAnd(_ => _.Music.Equals(Music));
    }

    public bool Equals(PlayingMusicViewModel? other)
    {
        return other.IsNotNullAnd(_ => _.Music.Equals(Music));
    }
}

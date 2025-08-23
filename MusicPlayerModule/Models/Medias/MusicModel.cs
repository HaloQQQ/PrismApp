using MusicPlayerModule.Utils;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TagLib;
using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using System.Text.RegularExpressions;
using IceTea.Pure.Contracts;
using System.Diagnostics;

namespace MusicPlayerModule.Models;
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8602
[DebuggerDisplay("Name={Name}")]
internal class MusicModel : MediaBaseModel, IEquatable<MusicModel>
{
    public bool IsEnglishTitle { get; }
    public bool IsEnglishSinger { get; }

    public MusicModel(string filePath) : base(filePath)
    {
        var file = TagLib.File.Create(filePath);   // 打开音频文件

        if (file.Tag.Performers.Length > 0)
        {
            Singer = file.Tag.Performers[0];   // 歌手名
        }
        else
        {
            Singer = Performer;
        }

        if (!file.Tag.Title.IsNullOrBlank())
        {
            Name = file.Tag.Title;
        }

        IsEnglishTitle = Regex.IsMatch(Name, RegexConstants.ContainsEnglishPattern);
        IsEnglishSinger = Regex.IsMatch(Singer, RegexConstants.ContainsEnglishPattern);

        Album = file.Tag.Album ?? "空专辑";             // 专辑名称
        Year = (int)file.Tag.Year;             // 年份
        TrackNum = (int)file.Tag.Track;        // 曲目号
        Genre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : "空流派";   // 流派

        // 获取时长（单位为毫秒）
        int duration = (int)file.Properties.Duration.TotalMilliseconds;
        // 将毫秒数转换为TimeSpan类型
        TimeSpan time = TimeSpan.FromMilliseconds(duration);
        TotalMills = (int)time.TotalMilliseconds;
        // 转换为分钟:秒数格式
        Duration = time.FormatTimeSpan();
    }

    private bool _noCover;
    private WeakReference<ImageSource>? _weakReference;

    public bool IsLoadingLyric { get; internal set; }
    public bool IsPureMusic { get; internal set; }

    private string _filePath;
    public override string FilePath
    {
        get => this._filePath;
        protected set
        {
            if (SetProperty<string>(ref _filePath, value))
            {
                RaisePropertyChanged(nameof(FileDir));
            }
        }
    }

    public ImageSource? ImageSource
    {
        get
        {
            if (_noCover)
            {
                return null;
            }

            if (_weakReference == null || !_weakReference.TryGetTarget(out var source))
            {
                var file = TagLib.File.Create(FilePath);   // 打开音频文件

                var pictures = file.Tag.Pictures;

                if (_noCover = (pictures.Length == 0))
                {
                    return null;
                }

                IPicture picture = pictures[0];
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();

                bi.StreamSource = new MemoryStream(picture.Data.Data);

                bi.CacheOption = BitmapCacheOption.OnLoad;

                bi.EndInit();

                if (_weakReference == null)
                {
                    _weakReference = new WeakReference<ImageSource>(bi);
                }
                else
                {
                    _weakReference.SetTarget(bi);
                }

                return bi;
            }

            return source;
        }
    }

    /// <summary>
    /// 曲目号
    /// </summary>
    public int TrackNum { get; }
    /// <summary>
    /// 年份
    /// </summary>
    public int Year { get; }
    /// <summary>
    /// 专辑
    /// </summary>
    public string Album { get; }
    /// <summary>
    /// 流派
    /// </summary>
    public string Genre { get; }

    public string Singer { get; }

    private volatile WeakReference<KRCLyrics>? _krcLyrics;

    public KRCLyrics? Lyric
    {
        get => _krcLyrics != null && _krcLyrics.TryGetTarget(out var kRCLyrics) ? kRCLyrics : null;

        internal set
        {
            value.AssertNotNull(nameof(Lyric));

            if (_krcLyrics == null)
            {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                _krcLyrics = new WeakReference<KRCLyrics>(value);
            }
            else
            {
                _krcLyrics.SetTarget(value);
            }

            RaisePropertyChanged();
        }
    }

    public bool MoveTo(string targetDir)
    {
        if (targetDir.IsDirectoryExists() && FilePath.IsFileExists())
        {
            var file = FilePath.GetFileName();

            var targetPath = Path.Combine(targetDir, file);

            System.IO.File.Move(FilePath, targetPath);

            FilePath = targetPath;

            return true;
        }

        return false;
    }

    protected override void DisposeCore()
    {
        base.DisposeCore();

        _weakReference = null;

        _krcLyrics = null;
    }

    public bool Equals(MusicModel? other)
    {
        return other.IsNotNullAnd(_ => _.FilePath == this.FilePath);
    }
}
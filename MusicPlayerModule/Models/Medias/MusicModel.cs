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
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
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
    private WeakReference<ImageSource>? _imageRef;

    public bool IsPureMusic { get; private set; }

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

            if (_imageRef == null || !_imageRef.TryGetTarget(out var source))
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

                if (_imageRef == null)
                {
                    _imageRef = new WeakReference<ImageSource>(bi);
                }
                else
                {
                    _imageRef.SetTarget(bi);
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

    #region Lyric
    private bool _isLoadingLyric;
    public bool IsLoadingLyric
    {
        get => _isLoadingLyric;
        private set => SetProperty<bool>(ref _isLoadingLyric, value);
    }

    private volatile WeakReference<KRCLyrics>? _krcLyricsRef;

    public KRCLyrics? Lyric
    {
        get
        {
            if (this.IsPureMusic)
            {
                return null;
            }

            KRCLyrics result = default;

            _krcLyricsRef.IsNotNullAnd(_ => _.TryGetTarget(out result));

            return result;
        }

        private set
        {
            value.AssertNotNull(nameof(Lyric));

            if (_krcLyricsRef == null)
            {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                _krcLyricsRef = new WeakReference<KRCLyrics>(value);
            }
            else
            {
                _krcLyricsRef.SetTarget(value);
            }

            RaisePropertyChanged();
        }
    }

    public async Task TryLoadLyricAsync(string lyricDir)
    {
        if (this.IsPureMusic || this.IsLoadingLyric || this.Lyric != null)
        {
            return;
        }

        IEnumerable<string> paths = await KRCLyrics.TryGetLyricPathsAsync(lyricDir).ConfigureAwait(false);

        string? lyricFilePath = paths.FirstOrDefault(path => path.ContainsIgnoreCase(this.Name) &&
                                                        (
                                                            path.ContainsIgnoreCase(this.Performer)
                                                            || path.ContainsIgnoreCase(this.Singer)
                                                        )
                                                    );

        if (!(this.IsPureMusic = lyricFilePath == null))
        {
            this.IsLoadingLyric = true;

            await Task.Delay(10).ConfigureAwait(false);

            this.Lyric = KRCLyrics.LoadFromFile(lyricFilePath);

            this.IsLoadingLyric = false;
        }
    }

    public int GetCurrentLineIndex(int currentMills)
    {
        int currentIndex = 0;

        if (this.IsPureMusic)
        {
            return currentIndex;
        }

        var lyric = this.Lyric;
        var lines = lyric.Lines;

        // 注意当前歌词的结束时间要与下一句歌词的开始时间比较
        while (currentIndex < lines.Count - 1 &&
               currentMills >= lines[currentIndex + 1].LineStart.TotalMilliseconds)
        {
            // 更新当前歌词的索引
            currentIndex++;
        }

        var currentLine = lines[currentIndex];
        if (!currentLine.IsPlayingLine)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                if (i != currentIndex)
                {
                    lines[i].IsPlayingLine = false;
                }

                lines[i].IsPlayed = i < currentIndex;
            }
        }

        return currentIndex;
    }

    public double GetWordProgress(int currentMills, int currentLineIndex)
    {
        if (this.IsPureMusic)
        {
            return 0d;
        }

        var lyric = this.Lyric;

        var currentLine = lyric.Lines[currentLineIndex];

        if (currentMills < currentLine.LineStart.TotalMilliseconds)
        {
            return 0d;
        }

        int charCount = 0;
        KRCLyricsWord tempChar;
        for (int i = 0; i < currentLine.Chars.Count; i++)
        {
            tempChar = currentLine.Chars[i];
            double value = currentMills - tempChar.CharStart.Add(currentLine.LineStart).Add(tempChar.CharDuring).TotalMilliseconds;

            if (value <= 0)
            {
                return (charCount + (tempChar.CharDuring.TotalMilliseconds + value) /
                            tempChar.CharDuring.TotalMilliseconds * tempChar.Word.Length
                        ) / currentLine.Words.Length;
            }

            charCount += tempChar.Word.Length;
        }

        return 1d;
    }

    public bool TryResetLyric()
    {
        if (!this.IsPureMusic)
        {
            var lyric = this.Lyric;
            if (lyric != null)
            {
                foreach (var item in lyric.Lines)
                {
                    item.IsPlayed = false;

                    if (item.IsPlayingLine)
                    {
                        item.IsPlayingLine = false;
                    }
                }
            }

            return true;
        }

        return false;
    }
    #endregion

    public bool MoveFileTo(string targetDir)
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

        _imageRef = null;

        _krcLyricsRef = null;
    }

    public bool Equals(MusicModel? other)
    {
        return other.IsNotNullAnd(_ => _.FilePath == this.FilePath);
    }
}
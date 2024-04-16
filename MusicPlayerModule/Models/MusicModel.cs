using MusicPlayerModule.Utils;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Prism.Mvvm;
using TagLib;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using System.Text.RegularExpressions;

namespace MusicPlayerModule.Models;

internal class MusicModel : BindableBase, IDisposable
{
    public bool IsEnglishTitle { get; private set; }
    public bool IsEnglishSinger { get; private set; }

    public MusicModel(string filePath)
    {
        this.FilePath = filePath;

        var arr = filePath.GetFileNameWithoutExtension().Split(" - ");
        if (arr.Length > 1)
        {
            this.Singer = arr[0];
            this.Name = arr[1];
        }

        var size = new FileInfo(filePath).Length / 1024.0 / 1024;
        this.Size = size.ToString("0.00");

        var file = TagLib.File.Create(filePath);   // 打开音频文件

        if (this.Name == null || this.Singer == null)
        {
            this.Singer = file.Tag.Performers.Length > 0 ? file.Tag.Performers[0] : null;   // 歌手名
            this.Name = file.Tag.Title;             // 歌曲标题
        }

        this.IsEnglishTitle = Regex.IsMatch(Name, "[a-zA-Z]");
        this.IsEnglishSinger = Regex.IsMatch(Singer, "[a-zA-Z]");

        this.Album = file.Tag.Album;             // 专辑名称
        this.Year = (int)file.Tag.Year;             // 年份
        this.TrackNum = (int)file.Tag.Track;        // 曲目号
        this.Genre = file.Tag.Genres.Length > 0 ? file.Tag.Genres[0] : null;   // 流派

        // 获取时长（单位为毫秒）
        int duration = (int)file.Properties.Duration.TotalMilliseconds;
        // 将毫秒数转换为TimeSpan类型
        TimeSpan time = TimeSpan.FromMilliseconds(duration);
        this.TotalMills = (int)time.TotalMilliseconds;
        // 转换为分钟:秒数格式
        this.Duration = time.FormatTimeSpan();

        // 获取封面
        IPicture[] pictures = file.Tag.Pictures;
        if (pictures.Length > 0)
        {
            IPicture picture = pictures[0];
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = new MemoryStream(picture.Data.Data);
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();

            this.ImageSource = bi;
        }
    }

    public ImageSource ImageSource { get; private set; }

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
    public string Name { get; }
    /// <summary>
    /// MB
    /// </summary>
    public string Size { get; }

    public string Duration { get; }

    public int TotalMills { get; }

    public bool IsLoadingLyric { get; internal set; }
    public bool IsPureMusic { get; internal set; }

    private volatile WeakReference<KRCLyrics> _krcLyrics;

    public KRCLyrics? Lyric
    {
        get
        {
            if (this._krcLyrics != null && this._krcLyrics.TryGetTarget(out KRCLyrics kRCLyrics))
            {
                return kRCLyrics;
            }

            return null;
        }

        internal set
        {
            value.AssertNotNull(nameof(Lyric));

            if (_krcLyrics == null)
            {
                _krcLyrics = new WeakReference<KRCLyrics>(value);
            }
            else
            {
                _krcLyrics.SetTarget(value);
            }

            RaisePropertyChanged();
        }
    }

    private string _filePath;
    public string FilePath
    {
        get => this._filePath;
        private set => SetProperty<string>(ref _filePath, value);
    }
    public string FileDir => this.FilePath.GetParentPath();

    public bool MoveTo(string targetDir)
    {
        if (Directory.Exists(targetDir) && System.IO.File.Exists(this.FilePath))
        {
            var file = FilePath.GetFileName();

            var targetPath = Path.Combine(targetDir, file);

            System.IO.File.Move(this.FilePath, targetPath);

            this.FilePath = targetPath;

            return true;
        }

        return false;
    }

    public void Dispose()
    {
        this.ImageSource = null;
    }
}
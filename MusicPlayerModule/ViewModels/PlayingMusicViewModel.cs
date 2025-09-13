using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using MusicPlayerModule.Models;
using MusicPlayerModule.Utils;
using MusicPlayerModule.ViewModels.Base;
using PrismAppBasicLib.Models;

namespace MusicPlayerModule.ViewModels;

#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
#pragma warning disable CS8602

internal class PlayingMusicViewModel : PlayingMediaBaseViewModel, IEquatable<PlayingMusicViewModel>, IEquatable<FavoriteMusicViewModel>
{
    public PlayingMusicViewModel(MusicModel music, SettingModel lyricSetting)
        : base(music)
    {
        Music = music.AssertNotNull(nameof(MusicModel));
        this._lyricSetting = lyricSetting.AssertNotNull(nameof(lyricSetting));

        this.TotalMills = music.TotalMills;
    }

    private SettingModel _lyricSetting;
    public MusicModel Music { get; private set; }

    #region 当前歌曲进度相关

    /// <summary>
    /// 当前歌词的字进度
    /// </summary>
    public double WordProgress { get; private set; }

    public override int CurrentMills
    {
        get => this._currentMills;
        set
        {
            if (_currentMills != value)
            {
                _currentMills = Math.Max(value, 0);
                _currentMills = Math.Min(value, this.TotalMills);

                // 从B点返回A点
                if (this.PointBMills != 0 && Math.Abs(this._currentMills - this.PointBMills) < 500)
                {
                    SetProperty<int>(ref _currentMills, this.PointAMills);
                }
                else
                {
                    RaisePropertyChanged();
                }

                RaisePropertyChanged(nameof(base.CurrentTime));

                this.UpdateLyricSelect();
            }
        }
    }

    private bool UpdateLyricSelect()
    {
        var currentMills = this._currentMills;
        if (currentMills + 100 > this.TotalMills)
        {
            ToNextMusic?.Invoke(this);
            return false;
        }

        if (this.Music.IsPureMusic)
        {
            return false;
        }

        var lyric = this.Music.Lyric;

        if (lyric == null)
        {
            // 不要await不然会卡死界面
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            this.Music.TryLoadLyricAsync(_lyricSetting.Value);

            return false;
        }

        var lines = lyric.Lines;

        // 注意当前歌词的结束时间要与下一句歌词的开始时间比较
        var currentIndex = this.Music.GetCurrentLineIndex(currentMills);

        if (this.OneLine == null && this.AnotherLine == null && currentIndex == 0)
        {
            this.OneLine = lines[0];
            this.OneLine.IsPlayingLine = true;
            this.AnotherLine = lines[1];
        }

        var currentLine = lines[currentIndex];
        this.CurrentLine = currentLine;

        if (currentLine != this.OneLine && currentLine != this.AnotherLine)
        {
            SetLine(currentLine, currentIndex);
        }

        if (!currentLine.IsPlayingLine)
        {
            var nextIndex = currentIndex + 1;
            if (nextIndex < lines.Count)
            {
                SetLine(lines[nextIndex], nextIndex);
            }

            currentLine.IsPlayingLine = true;
        }

        this.WordProgress = this.Music.GetWordProgress(currentMills, currentIndex);
        // 通知歌词字的进度
        RaisePropertyChanged(nameof(WordProgress));

        return true;

        void SetLine(KRCLyricsLine line, int index)
        {
            // 索引偶数
            if ((index & 1) == 0)
            {
                this.OneLine = line;
            }
            else // 奇数
            {
                this.AnotherLine = line;
            }
        }
    }

    private KRCLyricsLine _currentLine;

    public KRCLyricsLine CurrentLine
    {
        get => this._currentLine;
        private set => SetProperty<KRCLyricsLine>(ref _currentLine, value);
    }

    private KRCLyricsLine _oneLine;

    public KRCLyricsLine OneLine
    {
        get => this._oneLine;
        private set => SetProperty<KRCLyricsLine>(ref this._oneLine, value);
    }

    private KRCLyricsLine _anotherLine;

    public KRCLyricsLine AnotherLine
    {
        get => this._anotherLine;
        private set => SetProperty<KRCLyricsLine>(ref this._anotherLine, value);
    }

    internal static event Action<PlayingMusicViewModel> ToNextMusic;

    #endregion

    public override void Reset()
    {
        base.Reset();

        this._currentMills = 0;
        this.CurrentLine = null;
        this.OneLine = this.AnotherLine = null;

        this.Music.TryResetLyric();
    }

    protected override void DisposeCore()
    {
        base.DisposeCore();

        this._lyricSetting = null;

        this.CurrentLine = null;

        this.AnotherLine = null;

        this.OneLine = null;

        this.Music = null;

        ToNextMusic = null;
    }

    public bool Equals(PlayingMusicViewModel? other)
    {
        return other.IsNotNullAnd(_ => _.Music.Equals(this.Music));
    }

    public bool Equals(FavoriteMusicViewModel? other)
    {
        return other.IsNotNullAnd(_ => _.Music.Equals(this.Music));
    }
}
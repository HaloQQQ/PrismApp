using IceTea.Atom.Utils;
using MusicPlayerModule.Common;
using MusicPlayerModule.Models;
using MusicPlayerModule.Utils;
using MusicPlayerModule.ViewModels.Base;

namespace MusicPlayerModule.ViewModels
{
    internal class PlayingMusicViewModel : MediaBaseViewModel
    {
        public PlayingMusicViewModel(MusicModel music)
        {
            Music = music.AssertNotNull(nameof(MusicModel));

            base.IsLongTimeMedia = music.TotalMills > 1000 * 60 * 60;
        }

        public MusicModel Music { get; private set; }

        private bool _isPlayingMusic;

        /// <summary>
        /// 当前音乐是否为正在播放的
        /// </summary>
        public bool IsPlayingMusic
        {
            get { return _isPlayingMusic; }
            internal set { SetProperty<bool>(ref _isPlayingMusic, value); }
        }

        #region 当前歌曲进度相关

        private int _currentLineIndex = 0;

        /// <summary>
        /// 当前歌词的字进度
        /// </summary>
        public double WordProgress
        {
            get
            {
                var lyric = this.Music.Lyric;

                if (lyric == null)
                {
                    return 0;
                }

                var line = lyric.Lines[this._currentLineIndex];

                if (this._currentMills < line.LineStart.TotalMilliseconds)
                {
                    return 0;
                }

                KRCLyricsChar tempChar;
                for (int i = 0; i < line.Chars.Count; i++)
                {
                    tempChar = line.Chars[i];
                    double value = this._currentMills - tempChar.CharStart.Add(line.LineStart).Add(tempChar.CharDuring)
        .TotalMilliseconds;
                    if (value <= 0)
                    {
                        return i + (tempChar.CharDuring.TotalMilliseconds + value) /
                            tempChar.CharDuring.TotalMilliseconds;
                    }
                }

                return line.Chars.Count;
            }
        }

        public override int CurrentMills
        {
            get => this._currentMills;
            set
            {
                if (_currentMills != value)
                {
                    _currentMills = Math.Max(value, 0);
                    _currentMills = Math.Min(value, Music.TotalMills);

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

                    if (this.UpdateLyricSelect())
                    {
                        // 通知歌词字的进度
                        RaisePropertyChanged(nameof(WordProgress));
                    }
                }
            }
        }

        private bool UpdateLyricSelect()
        {
            if (this._currentMills + 100 > this.Music.TotalMills)
            {
                ToNextMusic?.Invoke(this);
                return false;
            }

            if (this.Music.IsPureMusic)
            {
                return false;
            }

            LoadLyricToMusicModel.LoadAsync(CustomStatics.LastMusicDir, this.Music);

            var lyric = this.Music.Lyric;

            if (lyric == null)
            {
                return false;
            }

            var lines = lyric.Lines;

            // 注意当前歌词的结束时间要与下一句歌词的开始时间比较
            var currentIndex = this.GetCurrentLineIndex(lyric.Lines);

            if (currentIndex < 0)
            {
                return false;
            }

            if (this.OneLine == null && this.AnotherLine == null && currentIndex == 0)
            {
                this.OneLine = lines[0];
                this.OneLine.IsPlayingLine = true;
                this.AnotherLine = lines[1];
            }

            this._currentLineIndex = currentIndex;

            for (int i = 0; i < lines.Count; i++)
            {
                if (i != currentIndex)
                {
                    lines[i].IsPlayingLine = false;
                }
            }

            var currentLine = lines[currentIndex];
            this.CurrentLine = currentLine;

            if (currentLine != this.OneLine && currentLine != this.AnotherLine)
            {
                this.SetLine(currentLine, currentIndex);
            }

            if (!currentLine.IsPlayingLine)
            {
                var nextIndex = currentIndex + 1;
                if (nextIndex < lines.Count)
                {
                    this.SetLine(lines[nextIndex], nextIndex);
                }

                currentLine.IsPlayingLine = true;
            }

            return true;
        }

        private int GetCurrentLineIndex(IList<KRCLyricsLine> lines)
        {
            int currentIndex = -1;

            // 注意当前歌词的结束时间要与下一句歌词的开始时间比较
            while (currentIndex < lines.Count - 1 &&
                   this._currentMills >= lines[currentIndex + 1].LineStart.TotalMilliseconds)
            {
                // 更新当前歌词的索引
                currentIndex++;
            }

            return currentIndex;
        }

        private void SetLine(KRCLyricsLine line, int index)
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

        private KRCLyricsLine _currentLine;

        public KRCLyricsLine CurrentLine
        {
            get => this._currentLine;
            set => SetProperty<KRCLyricsLine>(ref _currentLine, value);
        }

        private KRCLyricsLine _oneLine;

        public KRCLyricsLine OneLine
        {
            get => this._oneLine;
            set => SetProperty<KRCLyricsLine>(ref this._oneLine, value);
        }

        private KRCLyricsLine _anotherLine;

        public KRCLyricsLine AnotherLine
        {
            get => this._anotherLine;
            set => SetProperty<KRCLyricsLine>(ref this._anotherLine, value);
        }

        internal static event Action<PlayingMusicViewModel> ToNextMusic;

        #endregion

        public override void Reset()
        {
            base.Reset();

            this._currentMills = 0;
            this._currentLineIndex = 0;
            this.OneLine = this.AnotherLine = null;

            if (!this.Music.IsPureMusic)
            {
                var lyric = this.Music.Lyric;
                if (lyric != null)
                {
                    foreach (var item in lyric.Lines.Where(l => l.IsPlayingLine))
                    {
                        item.IsPlayingLine = false;
                    }
                }
            }
        }

        protected override void SetPointToTotalMills()
        {
            this.PointBMills = this.Music.TotalMills;
        }

        public override void Dispose()
        {
            this.Music.Dispose();

            this.Music = null;
        }
    }
}
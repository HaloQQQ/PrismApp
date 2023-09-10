using MusicPlayerModule.Models;
using MusicPlayerModule.Utils;
using MusicPlayerModule.ViewModels.Base;
using System;
using System.Windows.Controls;

namespace MusicPlayerModule.ViewModels
{
    internal class PlayingMusicViewModel : MediaBaseViewModel
    {
        public PlayingMusicViewModel(MusicModel music)
        {
            Music = music;

            base.IsLongTimeMedia = music.TotalMills > 1000 * 60 * 60;
        }

        public MusicModel Music { get; private set; }

        private bool _isPlayingMusic;

        public bool IsPlayingMusic
        {
            get { return _isPlayingMusic; }
            set { SetProperty<bool>(ref _isPlayingMusic, value); }
        }

        #region 当前歌曲进度相关
        private int _currentLineIndex = 0;
        private int GetCurrentLineIndex()
        {
            var currentIndex = -1;

            if (this.Music.Lyric == null)
            {
                return currentIndex;
            }

            var lyrics = this.Music.Lyric.Lines;

            // 注意当前歌词的结束时间要与下一句歌词的开始时间比较
            while (currentIndex < lyrics.Count - 1 &&
                   this._currentMills >= lyrics[currentIndex + 1].LineStart.TotalMilliseconds)
            {
                // 更新当前歌词的索引
                currentIndex++;
            }

            return currentIndex;
        }

        /// <summary>
        /// 当前歌词的字进度
        /// </summary>
        public double WordProgress
        {
            get
            {
                if (this.Music.Lyric == null)
                {
                    return 0;
                }

                var line = this.Music.Lyric.Lines[this._currentLineIndex];

                KRCLyricsChar tempChar = null;
                double value = 0;
                for (int i = 0; i < line.Chars.Count; i++)
                {
                    tempChar = line.Chars[i];
                    value = this._currentMills - tempChar.CharStart.Add(line.LineStart).Add(tempChar.CharDuring).TotalMilliseconds;
                    if (value <= 0)
                    {
                        return i + (tempChar.CharDuring.TotalMilliseconds + value) / tempChar.CharDuring.TotalMilliseconds;
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
                    _currentMills = value;

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
                this.OneLine = this.AnotherLine = null;
                ToNextMusic?.Invoke(this);
                return false;
            }

            if (this.Music.Lyric == null || this.Music.Lyric.Lines.Count == 0)
            {
                return false;
            }

            var lines = this.Music.Lyric.Lines;

            // 注意当前歌词的结束时间要与下一句歌词的开始时间比较
            var currentIndex = this.GetCurrentLineIndex();

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

                // 更新显示
                ScrollBarMoveToLyric?.Invoke(currentIndex);
            }

            return true;
        }

        private void SetLine(KRCLyricsLine line, int index)
        {
            // 索引偶数
            if ((index & 1) == 0)
            {
                this.OneLine = line;
            }
            else   // 奇数
            {
                this.AnotherLine = line;
            }
        }

        private KRCLyricsLine _oneLine;
        public KRCLyricsLine OneLine { get => this._oneLine; set => SetProperty<KRCLyricsLine>(ref this._oneLine, value); }

        private KRCLyricsLine _anotherLine;
        public KRCLyricsLine AnotherLine { get => this._anotherLine; set => SetProperty<KRCLyricsLine>(ref this._anotherLine, value); }

        internal static event Action<int> ScrollBarMoveToLyric;

        internal static event Action<PlayingMusicViewModel> ToNextMusic;
        #endregion

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

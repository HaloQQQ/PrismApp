using IceTea.Atom.BaseModels;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using MusicPlayerModule.Models;

namespace MusicPlayerModule.ViewModels.Base
{
    internal abstract class MediaBaseViewModel : ChildrenBase
    {
        private int _index;
        public int Index
        {
            get { return _index; }
            set
            {
                if (SetProperty(ref _index, value))
                {
                    RaisePropertyChanged(nameof(IndexString));
                }
            }
        }

        public string IndexString => this._index.ToString("000");

        public string MediaName { get; }
        public string FilePath { get; }

        protected MediaBaseViewModel(MediaBaseModel mediaBase)
        {
            mediaBase.AssertNotNull(nameof(MediaBaseModel));

            MediaName = mediaBase.Name;
            FilePath = mediaBase.FilePath;
        }

        public virtual int MillsStep => 1000;

        public void Rewind() => this.CurrentMills = Math.Max(this.CurrentMills - this.MillsStep, 0);
        public void FastForward() => this.CurrentMills = Math.Min(this.CurrentMills + this.MillsStep, TotalMills);

        private bool _isPlayingMedia;
        public bool IsPlayingMedia
        {
            get { return _isPlayingMedia; }
            internal set { SetProperty<bool>(ref _isPlayingMedia, value); }
        }

        private int _totalMills;
        public int TotalMills
        {
            get => _totalMills;
            set { SetProperty(ref _totalMills, value); }
        }

        #region 当前时间更新
        public double ProgressPercent => this.TotalMills == 0 ? 0 : Math.Round((this.CurrentMills * 1.0) / this.TotalMills * 100, 1);

        protected int _currentMills;
        public virtual int CurrentMills { get => _currentMills; set { throw new NotImplementedException(); } }

        public string CurrentTime => this.GetFormatTime(this._currentMills);
        #endregion

        #region 媒体AB点
        public bool LoadedABPoint { get; set; }

        private int _pointAMills;
        public int PointAMills
        {
            get { return _pointAMills; }
            private set
            {
                if (SetProperty<int>(ref _pointAMills, value))
                {
                    RaisePropertyChanged(nameof(this.PointATime));
                }
            }
        }

        public string PointATime => this.GetFormatTime(this._pointAMills);

        public void SetPointA(int mills)
        {
            this.PointAMills = mills;

            if (this._pointBMills != 0 && this._pointBMills < this._pointAMills)
            {
                this.PointBMills = this.TotalMills;
            }
        }

        public void GoToPointA()
        {
            if (PointAMills != 0)
            {
                this.CurrentMills = this.PointAMills;
            }
        }

        private int _pointBMills;
        public int PointBMills
        {
            get { return _pointBMills; }
            protected set
            {
                if (SetProperty<int>(ref _pointBMills, value))
                {
                    RaisePropertyChanged(nameof(this.PointBTime));
                }
            }
        }

        public string PointBTime => this.GetFormatTime(this._pointBMills);

        public void SetPointB(int mills)
        {
            if (mills > 0)
            {
                this.PointBMills = mills;

                if (this._pointAMills > this._pointBMills)
                {
                    this.PointAMills = 0;
                }
            }
        }

        public bool ShouleSaveABPoint()
        {
            return this.PointAMills != 0 || this.PointBMills != 0;
        }

        public virtual void ResetABPoint()
        {
            this.PointAMills = 0;
            this.PointBMills = 0;
        }
        #endregion

        private string GetFormatTime(int mills)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(mills);
            return time.FormatTimeSpan(this.TotalMills > 1000 * 60 * 60);
        }

        /// <summary>
        /// 加载时重置数据
        /// </summary>
        public virtual void Reset()
        {
            //this.ResetABPoint();
        }
    }
}

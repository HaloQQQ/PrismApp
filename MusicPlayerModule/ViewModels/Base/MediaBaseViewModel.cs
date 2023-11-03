using IceTea.Atom.Extensions;
using Prism.Mvvm;

namespace MusicPlayerModule.ViewModels.Base
{
    internal abstract class MediaBaseViewModel : BindableBase, IDisposable
    {
        private int _index;

        public int Index
        {
            get { return _index; }
            set { this._index = value; RaisePropertyChanged(nameof(IndexString)); }
        }

        public string IndexString => this._index.ToString("000");

        public bool IsLongTimeMedia { get; protected set; }

        #region 当前时间更新
        public virtual double ProgressPercent { get; }

        protected int _currentMills;

        public virtual int CurrentMills { get; set; }

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
                this.SetPointToTotalMills();
            }
        }

        protected abstract void SetPointToTotalMills();

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
            this.PointBMills = mills;

            if (this._pointAMills > this._pointBMills)
            {
                this.PointAMills = 0;
            }
        }

        public virtual void ResetABPoint()
        {
            this.PointAMills = 0;
            this.PointBMills = 0;
        }

        #endregion

        protected string GetFormatTime(int mills)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(mills);
            return time.FormatTimeSpan(this.IsLongTimeMedia);
        }

        public abstract void Dispose();

        public virtual void Reset()
        {
            this.ResetABPoint();
        }
    }
}

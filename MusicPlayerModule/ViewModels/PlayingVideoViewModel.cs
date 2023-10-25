using IceTea.Atom.Utils;
using MusicPlayerModule.Models;
using MusicPlayerModule.MsgEvents.Video.Dtos;
using MusicPlayerModule.ViewModels.Base;

namespace MusicPlayerModule.ViewModels
{
    internal class PlayingVideoViewModel : MediaBaseViewModel
    {
        private VideoModelAndGuid _dto;
        public PlayingVideoViewModel(VideoModelAndGuid dto, VideoModel video)
        {
            Video = video.AssertNotNull(nameof(VideoModelAndGuid));
            _dto = dto.AssertNotNull(nameof(VideoModel));
        }

        public VideoModel Video { get; private set; }

        public void SetVideoTotalMills(int totalMills)
        {
            this.Video.SetTotalMills(totalMills);

            base.IsLongTimeMedia = totalMills > 1000 * 60 * 60;
        }

        private bool _isPlayingVideo;

        public bool IsPlayingVideo
        {
            get { return _isPlayingVideo; }
            set { SetProperty<bool>(ref _isPlayingVideo, value); }
        }

        #region 关键点
        protected override void SetPointToTotalMills()
        {
            this.PointBMills = this.Video.TotalMills;
        }
        #endregion

        public override double ProgressPercent => this.Video.TotalMills == 0 ? 0 : Math.Round((this.CurrentMills * 1.0) / this.Video.TotalMills * 100, 1);

        public override int CurrentMills
        {
            get => this._currentMills;
            set
            {
                if (this.Video.TotalMills == 0)
                {
                    return;
                }

                if (_currentMills != value)
                {
                    _currentMills = value;

                    // 从B点返回A点
                    if (this.PointBMills != 0 && Math.Abs(this._currentMills - this.PointBMills) < 700)
                    {
                        SetProperty<int>(ref _currentMills, this.PointAMills);
                    }
                    else
                    {
                        RaisePropertyChanged();
                    }

                    if (this._currentMills + 500 > this.Video.TotalMills)
                    {
                        this._dto.Video = this;
                        ToNextVideo?.Invoke(this._dto);
                        return;
                    }

                    RaisePropertyChanged(nameof(base.CurrentTime));
                    RaisePropertyChanged(nameof(this.ProgressPercent));
                }
            }
        }

        internal static event Action<VideoModelAndGuid> ToNextVideo;

        public override void Dispose()
        {
            this.Video.Dispose();

            this.Video = null;
        }
    }
}

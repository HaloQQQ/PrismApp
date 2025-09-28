using IceTea.Pure.Utils;
using MusicPlayerModule.Models;
using MusicPlayerModule.MsgEvents.Video.Dtos;
using MusicPlayerModule.ViewModels.Base;

#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
namespace MusicPlayerModule.ViewModels;

internal class PlayingVideoViewModel : PlayingMediaBaseViewModel
{
    private VideoModelAndGuid _dto;
    public PlayingVideoViewModel(VideoModelAndGuid dto, VideoModel video)
        : base(video)
    {
        Video = video.AssertNotNull(nameof(MediaBaseModel));
        _dto = dto.AssertNotNull(nameof(VideoModelAndGuid));
    }

    public VideoModel Video { get; private set; }

    internal void SetVideoTotalMills(int totalMills)
    {
        this.Video.SetTotalMills(totalMills);

        this.TotalMills = totalMills;
    }

    public override int MillsStep => 5000;

    public override int CurrentMills
    {
        get => this._currentMills;
        set
        {
            if (this.TotalMills == 0)
            {
                return;
            }

            if (_currentMills != value)
            {
                _currentMills = Math.Max(value, 0);
                _currentMills = Math.Min(value, this.TotalMills);

                // 从B点返回A点
                if (this.PointBMills != 0 && Math.Abs(this._currentMills - this.PointBMills) < 700)
                {
                    SetProperty<int>(ref _currentMills, this.PointAMills);
                }
                else
                {
                    RaisePropertyChanged();
                }

                if (this._currentMills + 500 > this.TotalMills)
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

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal static event Action<VideoModelAndGuid> ToNextVideo;

    protected override void DisposeCore()
    {
        base.DisposeCore();

        this._dto?.Dispose();
        this._dto = null;

        this.Video?.Dispose();
        this.Video = null;
    }
}

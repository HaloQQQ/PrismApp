using IceTea.Pure.Extensions;

namespace MusicPlayerModule.Models;

internal class VideoModel : MediaBaseModel
{
    public VideoModel(string filePath) : base(filePath)
    {
    }

    internal void SetTotalMills(int totalMills)
    {
        // 将毫秒数转换为TimeSpan类型
        TimeSpan time = TimeSpan.FromMilliseconds(totalMills);
        TotalMills = (int)time.TotalMilliseconds;
        // 转换为分钟:秒数格式
        Duration = time.FormatTimeSpan();
    }

    public string Actor => Performer;
}
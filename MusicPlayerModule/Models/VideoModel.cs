using Prism.Mvvm;
using System.IO;
using IceTea.Wpf.Core.Helper;

namespace MusicPlayerModule.Models;

internal class VideoModel : BindableBase, IDisposable
{
    public VideoModel(string filePath)
    {
        this.FilePath = filePath;

        var arr = Path.GetFileNameWithoutExtension(filePath).Split(" - ");

        if (arr.Length > 1)
        {
            this.Actor = arr[0];
            this.Name = arr[1];
        }
        else
        {
            this.Name = arr[0];
        }

        var size = new FileInfo(filePath).Length / 1024.0 / 1024;
        this.Size = size.ToString("0.00");
    }

    internal void SetTotalMills(int totalMills)
    {
        // 将毫秒数转换为TimeSpan类型
        TimeSpan time = TimeSpan.FromMilliseconds(totalMills);
        this.TotalMills = (int)time.TotalMilliseconds;
        // 转换为分钟:秒数格式
        this.Duration = time.FormatTimeSpan();
    }

    public string Actor { get; }
    public string Name { get; }
    /// <summary>
    /// MB
    /// </summary>
    public string Size { get; }

    private string _duration = "00:00";
    public string Duration { get => this._duration; private set { SetProperty<string>(ref _duration, value); } }

    private int _totalMills;
    public int TotalMills { get => this._totalMills; private set { SetProperty<int>(ref _totalMills, value); } }

    public string FilePath { get; private set; }
    public string FileDir => Directory.GetParent(this.FilePath).FullName;

    public void Dispose()
    {
    }
}
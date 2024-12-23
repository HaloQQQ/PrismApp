using IceTea.Atom.BaseModels;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using System.IO;

namespace MusicPlayerModule.Models
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal class MediaBaseModel : BaseNotifyModel, IDisposable
    {
        public MediaBaseModel(string filePath)
        {
            FilePath = filePath.AssertNotNull(nameof(filePath));

            var arr = Path.GetFileNameWithoutExtension(filePath).Split(" - ");

            if (arr.Length > 1)
            {
                Performer = arr[0];
                Name = arr[1];
            }
            else
            {
                Name = arr[0];
            }

            var size = new FileInfo(filePath).Length / 1024.0 / 1024;
            Size = size.ToString("0.00");
        }

        public string Performer { get; protected set; }
        public string Name { get; protected set; }
        /// <summary>
        /// MB
        /// </summary>
        public string Size { get; }

        private string _duration = "00:00";
        public string Duration { get => _duration; protected set { SetProperty(ref _duration, value); } }

        private int _totalMills;
        public int TotalMills { get => _totalMills; protected set { SetProperty(ref _totalMills, value); } }

        public virtual string FilePath { get; protected set; }
        public string FileDir => FilePath.GetParentPath();

        public void Dispose()
        {
        }
    }
}

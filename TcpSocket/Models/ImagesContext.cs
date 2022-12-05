using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Helper.AbstractModel;
using TcpSocket.Helper;

namespace TcpSocket.Models
{
    public class MyImage
    {
        public string URI { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string Name { get; set; } = null!;
        public double Size { get; set; }

        public MyImage()
        {
        }

        public MyImage(string path)
        {
            this.URI = path;
            this.FileType = path.Split('.').LastOrDefault("(*^▽^*)");
            this.Name = Path.GetFileNameWithoutExtension(path);
            this.Size = ((double) (File.ReadAllBytes(path).Length) / 1024);
        }
    }

    public class MyImageBlock : MyImage
    {
        public MyImageBlock(string path) : base(path)
        {
        }
    }

    public class ImagesContext : BaseNotifyModel
    {
        public string ImageDir { get; set; }
        public ObservableCollection<MyImage> List { get; set; } = new ObservableCollection<MyImage>();
        public ObservableCollection<MyImage> Data { get; set; } = new ObservableCollection<MyImage>();
        public ObservableCollection<MyImageBlock> Block { get; set; } = new ObservableCollection<MyImageBlock>();

        private bool _showInList;

        public bool ShowInList
        {
            get => this._showInList;
            set
            {
                this._showInList = value;
                CallModel();
            }
        }


        private void GetFiles(string directoryPath, List<string> filePaths)
        {
            foreach (string d in Directory.GetFileSystemEntries(directoryPath))
            {
                if (File.Exists(d))
                {
                    System.Drawing.Image img = System.Drawing.Image.FromFile(d);
                    if (img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) || img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                    {
                        filePaths.Add(d);
                    }
                }
                else
                {
                    GetFiles(d, filePaths);
                }
            }
        }

        private IEnumerable<string> GetImageUris()
        {
            List<string> list = new List<string>();

            var config = ConfigurationHelper.Instance;

            if (!Directory.Exists(this.ImageDir = config.GetImageDir()))
            {
                this.ImageDir = Constants.Image_Dir;
            }

            this.GetFiles(this.ImageDir, list);

            return list;
        }

        private ImagesContext()
        {
            this.Data.Add(new MyImage());

            Task.Run(async () =>
            {
                var coll = this.GetImageUris();
                foreach (var item in coll)
                {
                    var image = new MyImage(item);

                    Helper.Helper.Invoke(() =>
                    {
                        this.List.Add(image);
                        this.Data.Add(image);

                        this.Block.Add(new MyImageBlock(item));
                    });

                    await Task.Delay(100);
                }
            });
        }

        public static ImagesContext CreateInstance()
        {
            return new ImagesContext();
        }
    }
}
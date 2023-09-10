using Helper.AbstractModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TcpSocket.Helper;

namespace TcpSocket.ViewModels
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
            URI = path;
            FileType = path.Split('.').LastOrDefault("(*^▽^*)");
            Name = Path.GetFileNameWithoutExtension(path);
            Size = (double)File.ReadAllBytes(path).Length / 1024;
        }
    }

    public class MyImageBlock : MyImage
    {
        public MyImageBlock(string path) : base(path)
        {
        }
    }

    public class ImageDisplayViewModel : BaseNotifyModel, IDisposable
    {
        public string ImageDir { get; set; }
        public ObservableCollection<MyImage> List { get; set; } = new ObservableCollection<MyImage>();
        public ObservableCollection<MyImage> Data { get; set; } = new ObservableCollection<MyImage>();
        public ObservableCollection<MyImageBlock> Block { get; set; } = new ObservableCollection<MyImageBlock>();

        private bool _showInList;

        public bool ShowInList
        {
            get => _showInList;
            set
            {
                _showInList = value;
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

            if (!Directory.Exists(ImageDir))
            {
                ImageDir = Constants.Image_Dir;
            }

            GetFiles(ImageDir, list);

            return list;
        }

        public ImageDisplayViewModel(WpfStyleResources.Interfaces.IConfigManager config)
        {
            this.Data.Add(new MyImage());

            this.ImageDir = config.ReadConfigNode("ImageDir");//config.GetImageDir();
            //config.SetConfig += (config, jsonObject) => 
            //    config.SetImageDir(jsonObject, this.ImageDir);

            config.SetConfig += config =>
                config.WriteConfigNode(this.ImageDir, "ImageDir");


            Task.Run(async () =>
            {
                var coll = GetImageUris();
                foreach (var item in coll)
                {
                    if (_disposed)
                    {
                        break;
                    }

                    var image = new MyImage(item);

                    Helper.Helper.Invoke(() =>
                    {
                        List.Add(image);
                        Data.Add(image);

                        Block.Add(new MyImageBlock(item));
                    });

                    await Task.Delay(100);
                }
            });
        }

        private bool _disposed;
        public void Dispose()
        {
            _disposed = true;
            Block.Clear();
            Block = null;
            List.Clear();
            List = null;
            Data.Clear();
            Data = null;
        }
    }
}
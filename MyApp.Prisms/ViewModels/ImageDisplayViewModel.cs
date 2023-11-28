using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IceTea.Atom.Extensions;
using IceTea.Atom.BaseModels;
using IceTea.Wpf.Core.Utils;
using IceTea.Atom.Contracts;
using System.Windows.Input;
using Prism.Commands;

namespace MyApp.Prisms.ViewModels
{
    public class MyImage : BaseNotifyModel
    {
        public bool InList { get; set; }

        private bool _selected;

        public bool Selected
        {
            get => this._selected;
            set => SetProperty<bool>(ref _selected, value);
        }


        public string URI { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Size { get; set; }

        public MyImage()
        {
        }

        public MyImage(string path)
        {
            URI = path?.GetFullPath();
            FileType = path.GetFileType();
            Name = path.GetFileNameWithoutExtension();
            Size = ((double)new FileInfo(path).Length / 1024).ToString("0.00");
        }
    }

    internal class ImageDisplayViewModel : BaseNotifyModel, IDisposable
    {
        public ObservableCollection<MyImage> Data { get; private set; } = new ObservableCollection<MyImage>();

        public IEnumerable<MyImage> ActualData => Data.SkipWhile(item => item.Name == null);

        private bool _showInList;

        public bool ShowInList
        {
            get => _showInList;
            set => SetProperty<bool>(ref _showInList, value);
        }

        private IEnumerable<string> GetImageUris(string imageDir)
        {
            List<string> list = imageDir.GetFiles(file =>
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(file);
                return img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) || img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png);
            });

            return list;
        }

        public ImageDisplayViewModel(IConfigManager config)
        {
            this.Data.CollectionChanged += (sender, e) => CallModel(nameof(this.ActualData));

            this.RefreshData(config);

            this.RefreshCommand = new DelegateCommand(() => this.RefreshData(config));
        }

        private void RefreshData(IConfigManager config)
        {
            this.IsLoading = true;

            this.Data.Clear();
            this.Data.Add(new MyImage());
            Task.Run(async () =>
            {
                var dir = config.ReadConfigNode(nameof(SettingsViewModel.ImageDir));
                var coll = GetImageUris(dir);
                foreach (var item in coll)
                {
                    if (_disposed)
                    {
                        break;
                    }

                    var image = new MyImage(item);

                    CommonUtils.BeginInvoke(() =>
                    {
                        Data.Add(image);
                    });

                    await Task.Delay(20);
                }
            }).ContinueWith(task => this.IsLoading = false);
        }

        public ICommand RefreshCommand { get; private set; }

        private bool _isLoading;

        public bool IsLoading
        {
            get => this._isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }

        private bool _disposed;
        public void Dispose()
        {
            _disposed = true;
            Data.Clear();
            Data = null;
        }
    }
}
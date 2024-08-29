using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IceTea.Atom.Extensions;
using IceTea.Atom.BaseModels;
using IceTea.Atom.Contracts;
using System.Windows.Input;
using Prism.Commands;
using System.Threading;
using IceTea.Wpf.Atom.Utils;
using MyApp.Prisms.Helper;

namespace MyApp.Prisms.ViewModels
{
    internal class MyImage : BaseNotifyModel
    {
        public bool InList { get; set; }

        private bool _selected;

        public bool Selected
        {
            get => this._selected;
            internal set => SetProperty<bool>(ref _selected, value);
        }


        public string URI { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Size { get; set; }

        internal MyImage()
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
            IList<string> list = imageDir.GetFiles(file =>
            {
                System.Drawing.Image img = System.Drawing.Image.FromFile(file);
                return img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) || img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png);
            });

            return list;
        }

        public ImageDisplayViewModel(IConfigManager config)
        {
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
                var dir = CustomConstants.LastImageDir ??= config.ReadConfigNode(nameof(SettingsViewModel.ImageDir));
                var coll = GetImageUris(dir);
                foreach (var item in coll)
                {
                    if (_disposed)
                    {
                        break;
                    }

                    var image = new MyImage(item);

                    // A.跨线程同步
                    //_synchronizationContext.Post(_ => Data.Add(image), null);

                    // B.跨线程同步
                    CommonAtomUtils.BeginInvoke(() =>
                    {
                        Data.Add(image);
                    });

                    Thread.Sleep(20);
                }
            }).ContinueWith(task => CallModel(nameof(this.ActualData))).ContinueWith(task => this.IsLoading = false);
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
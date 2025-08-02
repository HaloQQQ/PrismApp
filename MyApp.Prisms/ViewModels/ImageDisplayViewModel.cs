using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using IceTea.Atom.Extensions;
using IceTea.Atom.BaseModels;
using IceTea.Atom.Contracts;
using System.Windows.Input;
using Prism.Commands;
using IceTea.Wpf.Atom.Utils;
using MyApp.Prisms.Helper;
using IceTea.Atom.Utils;
using System.Windows.Media.Imaging;
using Prism.Events;
using PrismAppBasicLib.Models;

namespace MyApp.Prisms.ViewModels
{
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
    internal class MyImage : NotifyBase
    {
        public bool InList { get; set; }

        private bool _selected;

        public bool Selected
        {
            get => this._selected;
            internal set => SetProperty<bool>(ref _selected, value);
        }

        private BitmapSource? _source;
        public BitmapSource? Source
        {
            get => _source;
            private set => SetProperty(ref _source, value);
        }

        public bool IsEmpty { get; }

        public string URI { get; set; } = null!;
        public string FileType { get; set; } = null!;
        public string Name { get; set; } = null!;
        /// <summary>
        /// KB
        /// </summary>
        public string Size { get; set; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
        internal MyImage()
        {
            this.IsEmpty = true;
        }

        public MyImage(string path)
        {
            path.AssertNotNull(nameof(path));

            URI = path.GetFullPath();
            FileType = path.GetFileType();
            Name = path.GetFileNameWithoutExtension();
            Size = (new FileInfo(path).Length / 1024d).ToString("0.00");
        }
    }

    internal class ImageDisplayViewModel : NotifyBase, IDisposable
    {
        public ObservableCollection<MyImage> Data { get; private set; } = new ObservableCollection<MyImage>();

        private Random _random = new Random();

        public int ImagesCount => Math.Max(this.Data.Count - 1, 0);

        public void RaisePropertyChangedEvent(string propName)
        {
            this.RaisePropertyChanged(propName);
        }

        internal string GetRandomImage()
        {
            return this.Data[_random.Next(0, ImagesCount)].URI;
        }

        internal void SelectImage(string selectedImage)
        {
            foreach (var item in this.Data)
            {
                item.Selected = item.URI == selectedImage;
            }
        }

        private bool _showInList;

        public bool ShowInList
        {
            get => _showInList;
            set => SetProperty<bool>(ref _showInList, value);
        }

        public ImageDisplayViewModel(IConfigManager config, ISettingManager<SettingModel> settingManager, IEventAggregator eventAggregator)
        {
            settingManager.TryAdd(CustomConstants.IMAGE, () => new SettingModel(string.Empty, config.ReadConfigNode<string>(CustomConstants.LastImageDir_ConfigKey), null));

            this.ShowInList = config.ReadConfigNode<bool>(nameof(ShowInList).FillToArray());

            config.SetConfig += _ => _.WriteConfigNode(this.ShowInList, nameof(ShowInList).FillToArray());

            RefreshData(config);

            this.RefreshCommand = new DelegateCommand(() => RefreshData(config), () => !this.IsLoading).ObservesProperty(() => this.IsLoading);

            void RefreshData(IConfigManager config)
            {
                this.IsLoading = true;

                this.Data.Clear();
                this.Data.Add(new MyImage());

                Task.Run(async () =>
                {
                    var dir = settingManager[CustomConstants.IMAGE].Value;

                    if (!dir.IsDirectoryExists())
                    {
                        return;
                    }

                    var coll = GetImageUris(dir);
                    foreach (var item in coll)
                    {
                        if (this.IsDisposed)
                        {
                            break;
                        }

                        var image = new MyImage(item);

                        // A.跨线程同步
                        //SynchronizationContext.Current.Post(_ => Data.Add(image), null);

                        // B.跨线程同步
                        WpfAtomUtils.BeginInvoke(() =>
                        {
                            Data.Add(image);

                            RaisePropertyChanged(nameof(ImagesCount));
                        });

                        await Task.Delay(20);
                    }

                    IEnumerable<string> GetImageUris(string imageDir)
                        => imageDir.GetFiles(true, file =>
                            {
                                System.Drawing.Image img = System.Drawing.Image.FromFile(file);
                                return img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg) || img.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png);
                            });
                }).ContinueWith(task => this.IsLoading = false)
                .ContinueWith(task => this.RaisePropertyChanged(nameof(this.ImagesCount)));
            }
        }

        public ICommand RefreshCommand { get; }

        private bool _isLoading;

        public bool IsLoading
        {
            get => this._isLoading;
            private set => SetProperty<bool>(ref _isLoading, value);
        }

        protected override void DisposeCore()
        {
            Data.Clear();
            Data = null;

            base.DisposeCore();
        }
    }
}
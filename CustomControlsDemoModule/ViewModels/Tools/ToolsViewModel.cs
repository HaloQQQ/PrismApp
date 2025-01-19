using IceTea.Atom.BaseModels;
using IceTea.Atom.Contracts;
using IceTea.Atom.Utils;
using IceTea.Wpf.Atom.Utils;
using Prism.Commands;
using System.Windows.Input;
using IceTea.Atom.Extensions;
using Prism.Events;
using PrismAppBasicLib.Contracts;
using System.Collections.Generic;
using System.ServiceProcess;
using IceTea.Core.Utils.OS;
using System.Linq;
using System.Collections.ObjectModel;
using System;
using IceTea.Wpf.Atom.Contracts.FileFilters;
using CustomControlsDemoModule.Models;
using System.Windows.Media.Imaging;
using System.Drawing;
using IceTea.Core.Extensions;
using IceTea.Desktop.Extensions;
using static IceTea.Core.Extensions.ImageCoreExtensions;
using System.Threading.Tasks;

namespace CustomControlsDemoModule.ViewModels
{
    internal class ToolsViewModel : BaseNotifyModel
    {
        public ToolsViewModel(IEventAggregator eventAggregator)
        {
            this.InitCommands(eventAggregator);
        }

        private void InitCommands(IEventAggregator eventAggregator)
        {
            this.RestartComputerCommand = new DelegateCommand<string>(
                duration => AppUtils.RestartPC(GetMills(duration))
            );

            this.ShutdownComputerCommand = new DelegateCommand<string>(
                duration => AppUtils.ShutdownPC(GetMills(duration))
            );

            this.CancelShutdownComputerCommand = new DelegateCommand(() => AppUtils.CancalShutdownPC());

            uint GetMills(string duration)
            {
                uint time = 0;
                switch (duration)
                {
                    case "立即":
                        break;
                    case "10秒":
                        time = 10;
                        break;
                    case "30秒":
                        time = 30;
                        break;
                    case "1分钟":
                        time = 60;
                        break;
                    case "3分钟":
                        time = 180;
                        break;
                    case "5分钟":
                        time = 300;
                        break;
                    case "10分钟":
                        time = 600;
                        break;
                    case "30分钟":
                        time = 1800;
                        break;
                    case "1小时":
                        time = 3600;
                        break;
                    case "3小时":
                        time = 10800;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return time;
            }


            this.SelectFileCommand = new DelegateCommand(() =>
            {
                var dialog = CommonAtomUtils.OpenFileDialog(AppStatics.DeskTop, new ExeFilter());

                if (dialog != null)
                {
                    this.FilePath = dialog.FileName;
                }
            });

            this.AutoStartCommand = new DelegateCommand(() =>
            {
                var isSucceed = AppUtils.AutoStartWithShortcut(this.IsOn, this.IsOnlyForMe, this.FilePath);

                var message = (IsOnlyForMe ? "私有" : "全局") + "开机启动快捷方式" + (IsOn ? "创建" : "移除") + (isSucceed ? "成功" : "失败");

                CommonUtil.PublishMessage(eventAggregator, message);
            }, () => FilePath.IsFileExists())
            .ObservesProperty(() => FilePath);



            this.FentchServiceCommand = new DelegateCommand(() =>
            {
                this._services.Clear();

                this._services.AddIfItemsNotWhileOrNotContains(ServiceUtil.GetAllNormalServiceList().Select(p => p.Value));

                this.Services.Clear();

                this.Services.AddIfItemsNotWhileOrNotContains(this._services.Select(sc => sc.ServiceName).OrderBy(_ => _));
            });

            this.SC_StartServiceCommand = new DelegateCommand<string>(
                    AppUtils.StartService_sc,
                    Services.Contains
                ).ObservesProperty(() => CurrentServiceName);

            this.SC_StopServiceCommand = new DelegateCommand<string>(
                    AppUtils.StopService_sc,
                    Services.Contains
                )
                .ObservesProperty(() => CurrentServiceName);

            this.Net_StartServiceCommand = new DelegateCommand<string>(
                    AppUtils.StartService_net,
                    Services.Contains
                ).ObservesProperty(() => CurrentServiceName);

            this.Net_StopServiceCommand = new DelegateCommand<string>(
                    AppUtils.StopService_net,
                    Services.Contains
                )
                .ObservesProperty(() => CurrentServiceName);


            this.SelectExeFileCommand = new DelegateCommand(() =>
            {
                var dialog = CommonAtomUtils.OpenFileDialog(AppStatics.DeskTop, new ExeFilter());

                if (dialog != null)
                {
                    this.ServicExeFilePath = dialog.FileName;
                }
            });

            this.InstallServiceCommand = new DelegateCommand<string>(
                    exeFilePath => AppUtils.InstallService(exeFilePath, this.ServiceName),
                    _ => ServicExeFilePath.IsFileExists()
                ).ObservesProperty(() => this.ServicExeFilePath);

            this.UnInstallServiceCommand = new DelegateCommand<string>(
                    exeFilePath => AppUtils.UnInstallService(exeFilePath, this.ServiceName),
                    _ => ServicExeFilePath.IsFileExists()
                ).ObservesProperty(() => this.ServicExeFilePath);

            // 图片处理
            this.SelectPictureCommand = new DelegateCommand(() =>
            {
                var dialog = CommonAtomUtils.OpenFileDialog(AppStatics.DeskTop, new PictureFilter());

                if (dialog != null)
                {
                    this.ImageFilePath = dialog.FileName;

                    this.Pictures.Clear();

                    this.Pictures.Add(new Picture("原图", ImageFilePath, new BitmapImage(new Uri(ImageFilePath))));
                }
            });

            this.SolvePictureCommand = new DelegateCommand(() =>
            {
                var filePath = this.ImageFilePath;

                this.IsLoading = true;

                var list = new List<Task>();

                var taskA = Task.Run(() =>
                {
                    CommonAtomUtils.BeginInvoke(() =>
                    {
                        var i = new Bitmap(filePath);
                        var waterMarkSource = ImageCoreExtensions.AddWaterMark(i, new PointF(20, 30), "Are you Ok?", null, null).GetImageSource();
                        this.Pictures.Add(new Picture("水印", filePath, waterMarkSource));

                        var relativeVerticalSource = ImageCoreExtensions.ArrangeImages(new ImageLocation[]
                        {
                            new ImageLocation(i),
                            new ImageLocation(i)
                        }, ImageAlignment.Relative_Vertical).GetImageSource();
                        this.Pictures.Add(new Picture("垂直排列", filePath, relativeVerticalSource));

                        var relativeHorizontalSource = ImageCoreExtensions.ArrangeImages(new ImageLocation[]
                        {
                            new ImageLocation(i),
                            new ImageLocation(i)
                        }, ImageAlignment.Relative_Horizontal).GetImageSource();
                        this.Pictures.Add(new Picture("水平排列", filePath, relativeHorizontalSource));

                        var absoluteSource = ImageCoreExtensions.ArrangeImages(new ImageLocation[]
                        {
                            new ImageLocation(i),
                            new ImageLocation(i, new PointF(50,50))
                        }, ImageAlignment.Absolute).GetImageSource();
                        this.Pictures.Add(new Picture("堆叠", filePath, absoluteSource));
                    });

                });
                list.Add(taskA);

                var taskB = Task.Run(() =>
                {
                    CommonAtomUtils.BeginInvoke(() =>
                    {
                        var i = new Bitmap(filePath);
                        var clipSource = i.CaptureImage(new Rectangle(44, 44, 500, 500)).GetImageSource();
                        this.Pictures.Add(new Picture("裁剪", filePath, clipSource));

                        i = new Bitmap(filePath);
                        var updateLightnessSource = i.UpdateLightness(-100).GetImageSource();
                        this.Pictures.Add(new Picture("调节亮度", filePath, updateLightnessSource));

                        i = new Bitmap(filePath);
                        var reverseColorSource = i.ReverseColor().GetImageSource();
                        this.Pictures.Add(new Picture("反色处理", filePath, reverseColorSource));

                        i = new Bitmap(filePath);
                        var embossSource = i.Emboss().GetImageSource();
                        this.Pictures.Add(new Picture("浮雕处理", filePath, embossSource));
                    });
                });
                list.Add(taskB);

                var taskC = Task.Run(() =>
                {
                    CommonAtomUtils.BeginInvoke(() =>
                    {
                        var i = new Bitmap(filePath);
                        var resizeSource = i.Resize(200, 200).GetImageSource();
                        this.Pictures.Add(new Picture("缩放处理", filePath, resizeSource));

                        i = new Bitmap(filePath);
                        var filterSource = i.Filter().GetImageSource();
                        this.Pictures.Add(new Picture("滤色处理", filePath, resizeSource));

                        i = new Bitmap(filePath);
                        var flipHorizontalSource = i.FlipHorizontal().GetImageSource();
                        this.Pictures.Add(new Picture("左右翻转处理", filePath, flipHorizontalSource));

                        i = new Bitmap(filePath);
                        var flipVerticalSource = i.FlipVertical().GetImageSource();
                        this.Pictures.Add(new Picture("上下翻转处理", filePath, flipVerticalSource));
                    });
                });
                list.Add(taskC);

                var taskD = Task.Run(() =>
                {
                    CommonAtomUtils.BeginInvoke(() =>
                    {
                        var i = new Bitmap(filePath);
                        var compressSource = i.Compress(300, 300).GetImageSource();
                        this.Pictures.Add(new Picture("压缩处理", filePath, compressSource));

                        i = new Bitmap(filePath);
                        var toGraySource = i.ToGray().GetImageSource();
                        this.Pictures.Add(new Picture("灰度处理", filePath, toGraySource));

                        i = new Bitmap(filePath);
                        var toDarkLightSource = i.ToDarkLight().GetImageSource();
                        this.Pictures.Add(new Picture("黑白处理", filePath, toDarkLightSource));
                    });
                });
                list.Add(taskD);

                Task.WhenAll(list.ToArray()).ContinueWith(_ => this.IsLoading = false);
            }, () => ImageFilePath.IsFileExists())
                .ObservesProperty(() => ImageFilePath);
        }

        #region Props
        #region 开机启动
        private bool _isOnlyForMe;
        public bool IsOnlyForMe
        {
            get => _isOnlyForMe;
            set => SetProperty<bool>(ref _isOnlyForMe, value);
        }

        private bool _isOn;
        public bool IsOn
        {
            get => _isOn;
            set => SetProperty<bool>(ref _isOn, value);
        }

        private string _filePath;
        public string FilePath
        {
            get => _filePath;
            private set => SetProperty<string>(ref _filePath, value);
        }
        #endregion

        #region 启动关闭服务
        private string _curentServiceName;
        public string CurrentServiceName
        {
            get => _curentServiceName;
            set => SetProperty<string>(ref _curentServiceName, value);
        }

        public IList<string> Services { get; } = new ObservableCollection<string>();

        private IList<ServiceController> _services = new List<ServiceController>();
        #endregion

        #region 安装卸载服务
        private string _servicExeFilePath;
        public string ServicExeFilePath
        {
            get => _servicExeFilePath;
            private set => SetProperty<string>(ref _servicExeFilePath, value);
        }

        private string _serviceName;
        public string ServiceName
        {
            get => _serviceName;
            set => SetProperty<string>(ref _serviceName, value);
        }
        #endregion

        #region 图片处理
        public bool IsLoading { get; private set; }

        private string _imageFilePath;
        public string ImageFilePath
        {
            get => _imageFilePath;
            private set => SetProperty<string>(ref _imageFilePath, value);
        }

        public ObservableCollection<Picture> Pictures { get; } = new ObservableCollection<Picture>();
        #endregion
        #endregion

        #region Commands
        #region 重启、关机
        public ICommand RestartComputerCommand { get; private set; }
        public ICommand ShutdownComputerCommand { get; private set; }
        public ICommand CancelShutdownComputerCommand { get; private set; }
        #endregion

        #region 开机启动
        public ICommand SelectFileCommand { get; private set; }

        public ICommand AutoStartCommand { get; private set; }
        #endregion

        #region 启动关闭服务
        public ICommand FentchServiceCommand { get; private set; }

        public ICommand SC_StartServiceCommand { get; private set; }

        public ICommand SC_StopServiceCommand { get; private set; }

        public ICommand Net_StartServiceCommand { get; private set; }

        public ICommand Net_StopServiceCommand { get; private set; }
        #endregion

        #region 安装卸载服务
        public ICommand InstallServiceCommand { get; private set; }
        public ICommand UnInstallServiceCommand { get; private set; }

        public ICommand SelectExeFileCommand { get; private set; }
        #endregion

        #region 图片处理
        public ICommand SelectPictureCommand { get; private set; }

        public ICommand SolvePictureCommand { get; private set; }
        #endregion
        #endregion
    }
}

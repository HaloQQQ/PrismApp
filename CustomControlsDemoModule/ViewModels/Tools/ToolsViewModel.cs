using IceTea.Atom.BaseModels;
using IceTea.Atom.Contracts;
using IceTea.Atom.Utils;
using IceTea.Wpf.Atom.Contracts.MediaInfo;
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
            this.SelectFileCommand = new DelegateCommand(() =>
            {
                var dialog = CommonAtomUtils.OpenFileDialog(AppStatics.DeskTop, new AnyMedia());

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

            this.StartServiceCommand = new DelegateCommand<string>(
                    serviceName =>
                    {
                        //this._services.First(s => s.ServiceName == serviceName).Start();

                        AppUtils.ExecuteCmd($"net start {serviceName}");
                    },
                    Services.Contains
                ).ObservesProperty(() => CurrentServiceName);

            this.StopServiceCommand = new DelegateCommand<string>(
                    serviceName =>
                    {
                        //this._services.First(s => s.ServiceName == serviceName).Stop();

                        AppUtils.ExecuteCmd($"net stop {serviceName}");
                    },
                    Services.Contains
                )
                .ObservesProperty(() => CurrentServiceName);
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
            set => SetProperty<string>(ref _filePath, value);
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
        #endregion

        #region Commands
        #region 开机启动
        public ICommand SelectFileCommand { get; private set; }

        public ICommand AutoStartCommand { get; private set; }
        #endregion

        #region 启动关闭服务
        public ICommand FentchServiceCommand { get; private set; }

        public ICommand StartServiceCommand { get; private set; }

        public ICommand StopServiceCommand { get; private set; }
        #endregion
        #endregion
    }
}

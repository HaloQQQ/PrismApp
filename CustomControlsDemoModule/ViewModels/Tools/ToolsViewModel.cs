using IceTea.Atom.BaseModels;
using IceTea.Atom.Contracts;
using IceTea.Atom.Utils;
using IceTea.Wpf.Atom.Contracts.MediaInfo;
using IceTea.Wpf.Atom.Utils;
using Prism.Commands;
using System.Windows.Input;
using IceTea.Atom.Extensions;
using Prism.Events;
using PrismAppBasicLib.MsgEvents;

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

                eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage(message));
            }, () => FilePath.IsFileExists())
            .ObservesProperty(() => FilePath);
        }

        #region Props
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

        #region Commands
        public ICommand SelectFileCommand { get; private set; }

        public ICommand AutoStartCommand { get; private set; }
        #endregion
    }
}

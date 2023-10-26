using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyApp.Prisms.Helper;
using IceTea.Atom.Utils;
using IceTea.Atom.Utils.HotKey.GlobalHotKey;
using IceTea.Atom.Extensions;
using IceTea.General.Utils.AppHotKey;
using IceTea.General.Utils;
using MusicPlayerModule.MsgEvents;
using IceTea.Wpf.Core.Contracts;
using IceTea.Wpf.Core.Utils;
using IceTea.Atom.Contracts;

namespace MyApp.Prisms.ViewModels
{
    internal class SettingsViewModel : BindableBase
    {
        public SettingsViewModel(
                IConfigManager config,
                IAppHotKeyManager appHotKeyManager,
                IContainerProvider containerProvider,
                IEventAggregator eventAggregator
            )
        {
            this._config = config.AssertNotNull(nameof(IConfigManager));
            this.AppHotKeyManager = appHotKeyManager.AssertNotNull(nameof(IAppHotKeyManager));

            this.ImageDir = config.ReadConfigNode(nameof(this.ImageDir));
            this.LastMusicDir = config.ReadConfigNode("Music", nameof(this.LastMusicDir));
            this.LastVideoDir = config.ReadConfigNode("Video", nameof(this.LastVideoDir));

            this.InitHotkeys(config);

            this.InitCommands(containerProvider, eventAggregator);
        }

        public IAppHotKeyManager AppHotKeyManager { get; private set; }

        private IConfigManager _config;

        private void InitCommands(IContainerProvider containerProvider, IEventAggregator eventAggregator)
        {
            this.FindImageDirCommand = new DelegateCommand(() =>
            {
                var str = CommonUtils.OpenFolderDialog(this.ImageDir);
                if (!string.IsNullOrEmpty(str))
                {
                    this.ImageDir = str;
                }

                this.IsEditingSetting = true;
            });

            this.FindMusicDirCommand = new DelegateCommand(() =>
            {
                var str = CommonUtils.OpenFolderDialog(this.LastMusicDir);
                if (!string.IsNullOrEmpty(str))
                {
                    this.LastMusicDir = str;
                }

                this.IsEditingSetting = true;
            });

            this.FindVideoDirCommand = new DelegateCommand(() =>
            {
                var str = CommonUtils.OpenFolderDialog(this.LastVideoDir);
                if (!string.IsNullOrEmpty(str))
                {
                    this.LastVideoDir = str;
                }

                this.IsEditingSetting = true;
            });

            this.CancelCommand = new DelegateCommand(() =>
            {
                if (this.IsEditingSetting)
                {
                    this.IsEditingSetting = false;
                }

                foreach (var item in this.GlobalHotKeys)
                {
                    item.GoBack();
                }
            });

            this.SubmitCommand = new DelegateCommand(() =>
            {
                var resultStr = containerProvider.Resolve<GlobalHotKeyManager>().RegisterHotKeys(this.GlobalHotKeys);

                resultStr = resultStr.IsNullOrEmpty() ? "注册快捷键无错误" : resultStr;

                this.IsEditingSetting = false;

                eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage(resultStr, 4));
            });

            this.ResetGlobalHotKeysCommand = new DelegateCommand(() =>
            {
                foreach (var item in this.GlobalHotKeys)
                {
                    item.Reset();
                }

                this.SubmitCommand.Execute(null);
            });

            this.ResetGroupHotKeysCommand = new DelegateCommand<HotKeyGroup>(groupHotKey =>
            {
                if (!groupHotKey.KeyBindings.IsNullOrEmpty())
                {
                    foreach (var keyBinding in groupHotKey.KeyBindings)
                    {
                        keyBinding.Reset();
                    }
                }
            });
        }

        #region GlobalHotKeys
        public ObservableCollection<GlobalHotKeyModel> GlobalHotKeys { get; private set; }

        private void InitHotkeys(IConfigManager config)
        {
            this.GlobalHotKeys = HotKeyUtils.Provide(config, CustomConstants.ConfigGlobalHotkeys, CustomConstants.GlobalHotKeys);
        }
        #endregion

        #region Commands
        public ICommand FindImageDirCommand { get; private set; }
        public ICommand FindMusicDirCommand { get; private set; }
        public ICommand FindVideoDirCommand { get; private set; }

        public ICommand ResetGlobalHotKeysCommand { get; private set; }

        public ICommand ResetGroupHotKeysCommand { get; private set; }

        /// <summary>
        /// 还原未提交的修改
        /// </summary>
        public ICommand CancelCommand { get; private set; }
        /// <summary>
        /// 提交注册全局快捷键
        /// </summary>
        public ICommand SubmitCommand { get; private set; }
        #endregion

        #region Props
        private bool _isEditingSetting;

        public bool IsEditingSetting
        {
            get => this._isEditingSetting;
            set
            {
                if (SetProperty<bool>(ref _isEditingSetting, value) && !value)
                {
                    this.CancelCommand.Execute(null);
                }
            }
        }

        private string _imageDir;

        public string ImageDir
        {
            get => this._imageDir;
            set
            {
                if (SetProperty<string>(ref _imageDir, value))
                {
                    _config.WriteConfigNode(this.ImageDir, nameof(this.ImageDir));
                }
            }
        }

        private string _lastMusicDir;

        public string LastMusicDir
        {
            get => this._lastMusicDir;
            set
            {
                if (SetProperty<string>(ref _lastMusicDir, value))
                {
                    _config.WriteConfigNode(this.LastMusicDir, "Music", nameof(this.LastMusicDir));
                }
            }
        }

        private string _lastVideoDir;

        public string LastVideoDir
        {
            get => this._lastVideoDir;
            set
            {
                if (SetProperty<string>(ref _lastVideoDir, value))
                {
                    _config.WriteConfigNode(this.LastVideoDir, "Video", nameof(this.LastVideoDir));
                }
            }
        }
        #endregion
    }
}

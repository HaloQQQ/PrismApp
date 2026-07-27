using CustomControlsDemoModule.Events;
using CustomControlsDemoModule.Views;
using IceTea.Atom.Utils;
using IceTea.Pure.BaseModels;
using IceTea.Pure.Businesses.Config;
using IceTea.Pure.Businesses.HotKey.Global;
using IceTea.Pure.Businesses.Setting;
using IceTea.Pure.Contracts;
using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using IceTea.Wpf.Atom.Businesses.HotKey.App;
using MusicPlayerModule.Contracts;
using MyApp.Prisms.Contracts;
using MyApp.Prisms.MsgEvents;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using PrismAppBasicLib.Contracts;
using PrismAppBasicLib.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MyApp.Prisms.ViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal class SettingsViewModel : NotifyBase
    {
        public SettingsViewModel(
                IConfigManager configManager,
                ISettingManager settingManager,
                ISettingManager<SettingModel> settingModels,
                IAppConfigFileHotKeyManager appCfgHotkeyManager,
                IGlobalConfigFileHotKeyManager globalCfgHotkeyManager,
                IEventAggregator eventAggregator,
                IDialogService dialogService
            )
        {
            this.AppConfigFileHotKeyManager = appCfgHotkeyManager.AssertNotNull(nameof(IAppConfigFileHotKeyManager));
            this.GlobaConfigFilelHotKeyManager = globalCfgHotkeyManager.AssertArgumentNotNull(nameof(IGlobalConfigFileHotKeyManager));

            this.SettingModels = settingModels.AssertNotNull(nameof(ISettingManager<SettingModel>));

            this._eventAggregator = eventAggregator;

            this._settingManager = settingManager.AssertNotNull(nameof(settingManager));

            this.LoadConfig(configManager, settingModels);

            this.LoadMailAccounts(configManager, settingManager);

            this.LoadVideosCount(configManager, settingManager);

            this.InitCommands(eventAggregator, settingManager, configManager, dialogService);
        }

        private readonly IEventAggregator _eventAggregator;

        private readonly ISettingManager _settingManager;

        private void LoadVideosCount(IConfigManager configManager, ISettingManager settingManager)
        {
            IList<string> historyList = configManager.ReadConfigNode<List<string>>(CustomStatics.HistoryList_ConfigKey);

            int count = 8;
            if (!historyList.IsNullOrEmpty())
            {
                count = historyList.Count;
            }

            count = Enum.GetValues(typeof(EnumVideosCount))
               .Cast<int>()
               .Where(v => v <= count)
               .Max();

            _videosCount = (EnumVideosCount)count;

            settingManager.AddOrUpdate(CustomConstants.SettingKeys.VideosCount, count.ToString());
        }

        public ISettingManager<SettingModel> SettingModels { get; }

        public IAppConfigFileHotKeyManager AppConfigFileHotKeyManager { get; }

        public IGlobalConfigFileHotKeyManager GlobaConfigFilelHotKeyManager { get; }

        private void InitCommands(IEventAggregator eventAggregator, ISettingManager settingManager, IConfigManager configManager, IDialogService dialogService)
        {
            this.CleanConfigWhenExitAppCommand = new DelegateCommand(() =>
            {
                configManager.PostSetConfig -= CleanAll;
                configManager.PostSetConfig += CleanAll;

                void CleanAll(IConfigManager config)
                {
                    config.CleanAll();
                }
            });

            this.AddMailAccountCommand = new DelegateCommand(() =>
            {
                if (!RegexConstants.EmailPattern.IsMatch(this.CurrentMailPair.Key))
                {
                    CommonUtil.PublishMessage(eventAggregator, "要添加的邮箱不符合邮箱规则");
                    return;
                }

                this.MailAccounts.Add(new Pair(this.CurrentMailPair.Key, this.CurrentMailPair.Value));
                settingManager.AddOrUpdate(this.CurrentMailPair.Key, () => this.CurrentMailPair.Value);
                this.CurrentMailPair.Clear();
            },
            () => !this.CurrentMailPair.Key.IsNullOrBlank() && !this.CurrentMailPair.Value.IsNullOrBlank()
            ).ObservesProperty(() => this.CurrentMailPair.Key)
             .ObservesProperty(() => this.CurrentMailPair.Value);

            this.RemoveMailAccountCommand = new DelegateCommand<Pair>(pair =>
            {
                this.MailAccounts.Remove(pair);
                settingManager.Remove(pair.Key);
            });

            this.CancelCommand = new DelegateCommand<IGlobalConfigFileHotKeyGroup>(globalHotKeyGroup =>
            {
                this.IsEditingSetting = false;

                globalHotKeyGroup.GoBack();
            });

            this.SubmitCommand = new DelegateCommand<IGlobalConfigFileHotKeyGroup>(globalHotKeyGroup =>
            {
                this.IsEditingSetting = false;

                var failedItems = globalHotKeyGroup.Submit();
                var message = failedItems.Any() ? $"{string.Join(Environment.NewLine, failedItems.Select(i => i.ToString()))}{Environment.NewLine}提交失败" : "提交成功";

                CommonUtil.PublishMessage(eventAggregator, message, 4);
            });

            this.ResetGlobalHotKeyGroupCommand = new DelegateCommand<IGlobalConfigFileHotKeyGroup>(globalHotKeyGroup =>
            {
                this.IsEditingSetting = false;

                var failedItems = globalHotKeyGroup.Reset();
                var message = failedItems.Any() ? $"{string.Join(Environment.NewLine, failedItems.Select(i => i.ToString()))}{Environment.NewLine}重置失败" : "重置成功";

                CommonUtil.PublishMessage(eventAggregator, message, 4);
            });

            this.ResetAppHotKeyGroupCommand = new DelegateCommand<IAppConfigFileHotKeyGroup>(appHotKeyGroup =>
            {
                this.IsEditingSetting = false;

                var failedItems = appHotKeyGroup.Reset();
                var message = failedItems.Any() ? $"{string.Join(Environment.NewLine, failedItems.Select(i => i.ToString()))}{Environment.NewLine}重置失败" : "重置成功";

                CommonUtil.PublishMessage(eventAggregator, message, 4);
            });

            this.ShowDialogCommand = new DelegateCommand<string>(dialogService.ShowDialog);

            eventAggregator.GetEvent<ColorPickerEvent>().Subscribe(ToggleColorPicker);

            this.ColorPickerCommand = new DelegateCommand(ToggleColorPicker, () => !IsColorPicker).ObservesProperty(() => IsColorPicker);

            void ToggleColorPicker()
            {
                this.IsColorPicker = !IsColorPicker;

                if (IsColorPicker)
                {
                    dialogService.Show(nameof(FetchBackColorView), null, null, nameof(FetchBackColor));
                }
            }

            this.RestartComputerCommand = new DelegateCommand(() =>
            {
                AppUtils.RestartPC(1);

                Application.Current.MainWindow.Close();
            });

            this.ShutdownComputerCommand = new DelegateCommand(() =>
            {
                AppUtils.ShutdownPC(1);

                Application.Current.MainWindow.Close();
            });

            this.ComputerSleepCommand = new DelegateCommand(() => AppUtils.SleepPC());
        }

        #region Emails
        public Pair CurrentMailPair { get; } = new();

        public ObservableCollection<Pair> MailAccounts { get; } = new();

        private void LoadMailAccounts(IConfigManager configManager, ISettingManager settingManager)
        {
            var accounts = configManager.ReadConfigNode<IEnumerable<Pair>>(CustomConstants.MailAccounts);

            if (accounts != null)
            {
                this.MailAccounts.AddRange(accounts);

                accounts.ForEach(item => settingManager.AddOrUpdate(item.Key, item.Value));
            }

            configManager.SetConfig += config =>
            {
                config.WriteConfigNode(MailAccounts, CustomConstants.MailAccounts);
            };
        }
        #endregion

        #region Commands
        public ICommand ShowDialogCommand { get; private set; }

        public ICommand ColorPickerCommand { get; private set; }

        public ICommand CleanConfigWhenExitAppCommand { get; private set; }

        public ICommand AddMailAccountCommand { get; private set; }
        public ICommand RemoveMailAccountCommand { get; private set; }

        public ICommand FindImageDirCommand { get; private set; }
        public ICommand FindMusicDirCommand { get; private set; }
        public ICommand FindVideoDirCommand { get; private set; }

        public ICommand ResetGlobalHotKeyGroupCommand { get; private set; }

        public ICommand ResetAppHotKeyGroupCommand { get; private set; }


        public ICommand RestartComputerCommand { get; private set; }

        public ICommand ShutdownComputerCommand { get; private set; }

        public ICommand ComputerSleepCommand { get; private set; }

        /// <summary>
        /// 还原未提交的修改
        /// </summary>
        public ICommand CancelCommand { get; private set; }
        /// <summary>
        /// 提交注册全局快捷键
        /// </summary>
        public ICommand SubmitCommand { get; private set; }
        #endregion

        #region 读取和保存配置
        private void LoadConfig(IConfigManager configManager, ISettingManager<SettingModel> settingModels)
        {
            this.InitSetting(configManager, settingModels, CustomConstants.IMAGE, "图片默认目录", CustomConstants.LastImageDir_ConfigKey);

            this.InitSetting(configManager, settingModels, CustomStatics.MUSIC, "音乐默认目录", CustomStatics.LastMusicDir_ConfigKey);

            this.InitSetting(configManager, settingModels, CustomStatics.LYRIC, "歌词默认目录", CustomStatics.LastLyricDir_ConfigKey);

            this.InitSetting(configManager, settingModels, CustomStatics.VIDEO, "视频默认目录", CustomStatics.LastVideoDir_ConfigKey);

            this.LoadWindowCornerRadius(configManager);

            IsVideosAutoLoad = configManager.ReadConfigNode<bool>(CustomConstants.ConfigNodes.IsVideosAutoLoad);

            configManager.SetConfig += config => config.WriteConfigNode<bool>(this.IsVideosAutoLoad, CustomConstants.ConfigNodes.IsVideosAutoLoad);
        }

        private void InitSetting(IConfigManager configManager, ISettingManager<SettingModel> settingModels, string key, string description, params string[] configNode)
        {
            settingModels.AddOrUpdate(key, new SettingModel(description, configManager.ReadConfigNode<string>(configNode), () => this.IsEditingSetting = true));

            configManager.SetConfig += config =>
            {
                config.WriteConfigNode(settingModels[key].Value, configNode);
            };
        }

        private void LoadWindowCornerRadius(IConfigManager configManager)
        {
            var windowCornerRadius = configManager.ReadConfigNode<string>(CustomConstants.WindowCornerRadius);

            if (!windowCornerRadius.IsNullOrBlank())
            {
                this.CornerRadius = new CornerRadius(double.Parse(windowCornerRadius));
            }

            configManager.SetConfig += config =>
            {
                config.WriteConfigNode(this.CornerRadius.TopLeft, CustomConstants.WindowCornerRadius);
            };
        }
        #endregion

        #region Props
        private bool _isLightSysTheme;
        public bool IsLightSysTheme
        {
            get => _isLightSysTheme;
            set
            {
                if (SetProperty<bool>(ref _isLightSysTheme, value))
                {
                    if (RegistryUtils.SwitchLightTheme(_isLightSysTheme))
                    {
                        ContainerLocator.Current.Resolve<IEventAggregator>()
                            .GetEvent<SwitchThemeEvent>().Publish(_isLightSysTheme);
                    }
                }
            }
        }

        private bool _isColorPicker;
        public bool IsColorPicker
        {
            get => _isColorPicker;
            private set => SetProperty(ref _isColorPicker, value);
        }

        /// <summary>
        /// 窗口圆角
        /// </summary>
        private CornerRadius _cornerRadius;

        public CornerRadius CornerRadius
        {
            get => this._cornerRadius;
            set => SetProperty<CornerRadius>(ref _cornerRadius, value);
        }

        private EnumVideosCount _videosCount;
        public EnumVideosCount VideosCount
        {
            get => _videosCount;
            set
            {
                if (SetProperty(ref _videosCount, value))
                {
                    this.IsEditingSetting = false;

                    // 持久化新的视频格数量，确保 VideoPlayerViewModel 在 SetConfig 时读到最新值
                    _settingManager.AddOrUpdate(CustomConstants.SettingKeys.VideosCount, ((int)value).ToString());

                    _eventAggregator.GetEvent<VideosCountChangedEvent>()
                                    .Publish(_videosCount);
                }
            }
        }

        private bool _isEditingSetting;

        public bool IsEditingSetting
        {
            get => this._isEditingSetting;
            set
            {
                if (SetProperty<bool>(ref _isEditingSetting, value))
                {
                    if (value)
                    {
                        this.GlobaConfigFilelHotKeyManager.GoBack();
                    }
                }
            }
        }

        private bool _isVideosAutoLoad;
        public bool IsVideosAutoLoad
        {
            get => _isVideosAutoLoad;
            set => SetProperty<bool>(ref _isVideosAutoLoad, value);
        }

        #endregion
    }

    internal class Pair : NotifyBase
    {
        public Pair()
        {
        }

        public Pair(string key, string value)
        {
            Key = key;
            Value = value;
        }

        private string _key;
        public string Key
        {
            get => this._key;
            set => SetProperty<string>(ref _key, value);
        }

        private string _value;
        public string Value
        {
            get => this._value;
            set => SetProperty<string>(ref _value, value);
        }

        public void Clear()
        {
            this.Key = string.Empty;
            this.Value = string.Empty;
        }
    }
}

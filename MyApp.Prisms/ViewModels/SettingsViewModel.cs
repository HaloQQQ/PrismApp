using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyApp.Prisms.Helper;
using IceTea.Atom.Utils;
using IceTea.Atom.Extensions;
using IceTea.Wpf.Core.Utils;
using IceTea.Atom.Contracts;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using PrismAppBasicLib.MsgEvents;
using System.Windows;
using IceTea.General.Utils.HotKey.App;
using IceTea.Atom.Utils.HotKey.Global;
using System;

namespace MyApp.Prisms.ViewModels
{
    internal class SettingsViewModel : BindableBase
    {
        public SettingsViewModel(
                IConfigManager config,
                ISettingManager settingManager,
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

            this.LoadMailAccounts(config, settingManager);
            this.LoadWindowCornerRadius(config);

            this.InitCommands(containerProvider, eventAggregator, settingManager);
        }

        public IAppHotKeyManager AppHotKeyManager { get; private set; }

        private IGlobalHotKeyManager _globalHotKeyManager;
        public IGlobalHotKeyManager GlobalHotKeyManager
        {
            get => _globalHotKeyManager;
            set => SetProperty<IGlobalHotKeyManager>(ref _globalHotKeyManager, value.AssertNotNull(nameof(IGlobalHotKeyManager)));
        }

        private IConfigManager _config;

        private void CleanAll(IConfigManager configManager)
        {
            configManager.CleanAll();
        }

        private void InitCommands(IContainerProvider containerProvider, IEventAggregator eventAggregator, ISettingManager settingManager)
        {
            this.CleanConfigWhenExitAppCommand = new DelegateCommand(() =>
            {
                _config.PostSetConfig -= CleanAll;
                _config.PostSetConfig += CleanAll;
            });

            this.AddMailAccountCommand = new DelegateCommand(() =>
            {
                if (!Regex.IsMatch(this.CurrentMailPair.Key, "^[A-Za-z0-9\\u4e00-\\u9fa5]+@[a-zA-Z0-9_-]+(\\.[a-zA-Z0-9_-]+)+$"))
                {
                    eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage("要添加的邮箱不符合邮箱规则"));
                    return;
                }

                this.MailAccounts.Add(new Pair(this.CurrentMailPair.Key, this.CurrentMailPair.Value));
                settingManager.AddOrUpdate(this.CurrentMailPair.Key, this.CurrentMailPair.Value);
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

                eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage(message, 4));
            });

            this.ResetGlobalHotKeyGroupCommand = new DelegateCommand<IGlobalConfigFileHotKeyGroup>(globalHotKeyGroup =>
            {
                this.IsEditingSetting = false;

                var failedItems = globalHotKeyGroup.Reset();
                var message = failedItems.Any() ? $"{string.Join(Environment.NewLine, failedItems.Select(i => i.ToString()))}{Environment.NewLine}重置失败" : "重置成功";

                eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage(message, 4));
            });

            this.ResetAppHotKeyGroupCommand = new DelegateCommand<IAppConfigFileHotKeyGroup>(appHotKeyGroup =>
            {
                this.IsEditingSetting = false;

                var failedItems = appHotKeyGroup.Reset();
                var message = failedItems.Any() ? $"{string.Join(Environment.NewLine, failedItems.Select(i => i.ToString()))}{Environment.NewLine}重置失败" : "重置成功";

                eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage(message, 4));
            });
        }

        #region Emails
        public Pair CurrentMailPair { get; } = new();

        public ObservableCollection<Pair> MailAccounts { get; } = new();

        private void LoadMailAccounts(IConfigManager configManager, ISettingManager settingManager)
        {
            var accounts = configManager.ReadConfigNode(CustomConstants.MailAccounts);

            if (!accounts.IsNullOrBlank())
            {
                var dictionary = accounts.DeserializeObject<IEnumerable<Pair>>();

                this.MailAccounts.AddRange(dictionary);

                settingManager.AddRange(dictionary.Select<Pair, KeyValuePair<string, string>>(pair => new KeyValuePair<string, string>(pair.Key, pair.Value)));
            }

            configManager.SetConfig += config =>
            {
                config.WriteConfigNode(MailAccounts.SerializeObject(), CustomConstants.MailAccounts);
            };
        }
        #endregion

        #region Commands

        public ICommand CleanConfigWhenExitAppCommand { get; private set; }

        public ICommand AddMailAccountCommand { get; private set; }
        public ICommand RemoveMailAccountCommand { get; private set; }

        public ICommand FindImageDirCommand { get; private set; }
        public ICommand FindMusicDirCommand { get; private set; }
        public ICommand FindVideoDirCommand { get; private set; }

        public ICommand ResetGlobalHotKeyGroupCommand { get; private set; }

        public ICommand ResetAppHotKeyGroupCommand { get; private set; }

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
        /// <summary>
        /// 窗口圆角
        /// </summary>
        private CornerRadius _cornerRadius;

        public CornerRadius CornerRadius
        {
            get => this._cornerRadius;
            set => SetProperty<CornerRadius>(ref _cornerRadius, value);
        }

        private void LoadWindowCornerRadius(IConfigManager configManager)
        {
            var windowCornerRadius = configManager.ReadConfigNode(CustomConstants.WindowCornerRadius);

            if (!windowCornerRadius.IsNullOrBlank())
            {
                this.CornerRadius = new CornerRadius(double.Parse(windowCornerRadius));
            }

            configManager.SetConfig += config =>
            {
                config.WriteConfigNode(this.CornerRadius.TopLeft, CustomConstants.WindowCornerRadius);
            };
        }

        private bool _isEditingSetting;

        public bool IsEditingSetting
        {
            get => this._isEditingSetting;
            set
            {
                SetProperty<bool>(ref _isEditingSetting, value);
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

    internal class Pair : BindableBase
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

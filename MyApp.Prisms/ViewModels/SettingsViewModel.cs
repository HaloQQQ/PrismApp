using Prism.Commands;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MyApp.Prisms.Helper;
using IceTea.Atom.Utils;
using IceTea.Atom.Extensions;
using IceTea.Atom.Contracts;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using PrismAppBasicLib.MsgEvents;
using System.Windows;
using System;
using IceTea.Atom.Utils.HotKey.Global.Contracts;
using CustomControlsDemoModule.Views;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using MusicPlayerModule.Models;
using MusicPlayerModule.Contracts;
using IceTea.Atom.BaseModels;

namespace MyApp.Prisms.ViewModels
{
    internal class SettingsViewModel : BaseNotifyModel
    {
        public SettingsViewModel(
                IConfigManager configManager,
                ISettingManager settingManager,
                ISettingManager<SettingModel> settingModels,
                IAppConfigFileHotKeyManager appConfigFileHotKeyManager,
                IEventAggregator eventAggregator
            )
        {
            this.AppConfigFileHotKeyManager = appConfigFileHotKeyManager.AssertNotNull(nameof(IAppConfigFileHotKeyManager));

            this.SettingModels = settingModels.AssertNotNull(nameof(ISettingManager<SettingModel>));

            this.LoadConfig(configManager, settingModels);

            this.LoadMailAccounts(configManager, settingManager);

            this.InitCommands(eventAggregator, settingManager, configManager);
        }

        public ISettingManager<SettingModel> SettingModels { get; }

        public IAppConfigFileHotKeyManager AppConfigFileHotKeyManager { get; }

        private IGlobalConfigFileHotKeyManager _globalConfigFileHotKeyManager;
        public IGlobalConfigFileHotKeyManager GlobaConfigFilelHotKeyManager
        {
            get => _globalConfigFileHotKeyManager;
            internal set => SetProperty(ref _globalConfigFileHotKeyManager, value);
        }

        private void InitCommands(IEventAggregator eventAggregator, ISettingManager settingManager, IConfigManager configManager)
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
                if (!Regex.IsMatch(this.CurrentMailPair.Key, RegexConstants.EmailPattern))
                {
                    eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage("要添加的邮箱不符合邮箱规则"));
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

            this._2048Command = new DelegateCommand(() =>
            {
                new _2048Window().ShowDialog();
            });

            this.FiveChessCommand = new DelegateCommand(() =>
            {
                new FiveChessWindow().ShowDialog();
            });

            this.ChineseChessCommand = new DelegateCommand(() =>
            {
                new ChineseChessWindow().ShowDialog();
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

                dictionary.ForEach(item => settingManager.AddOrUpdate(item.Key, item.Value));
            }

            configManager.SetConfig += config =>
            {
                config.WriteConfigNode(MailAccounts.SerializeObject(), CustomConstants.MailAccounts);
            };
        }
        #endregion

        #region Commands

        public ICommand _2048Command { get; private set; }
        public ICommand FiveChessCommand { get; private set; }
        public ICommand ChineseChessCommand { get; private set; }



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

        #region 读取和保存配置
        private void LoadConfig(IConfigManager configManager, ISettingManager<SettingModel> settingModels)
        {
            this.InitSetting(configManager, settingModels, CustomConstants.IMAGE, "图片默认目录", CustomConstants.LastImageDir_ConfigKey);

            this.InitSetting(configManager, settingModels, CustomStatics.MUSIC, "音乐默认目录", CustomStatics.LastMusicDir_ConfigKey);

            this.InitSetting(configManager, settingModels, CustomStatics.LYRIC, "歌词默认目录", CustomStatics.LastLyricDir_ConfigKey);

            this.InitSetting(configManager, settingModels, CustomStatics.VIDEO, "视频默认目录", CustomStatics.LastVideoDir_ConfigKey);

            this.LoadWindowCornerRadius(configManager);
        }

        private void InitSetting(IConfigManager configManager, ISettingManager<SettingModel> settingModels, string key, string description, params string[] configNode)
        {
            settingModels.AddOrUpdate(key, new SettingModel(description, configManager.ReadConfigNode(configNode), () => this.IsEditingSetting = true));

            configManager.SetConfig += config =>
            {
                config.WriteConfigNode(settingModels[key].Value, configNode);
            };
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
        #endregion
    }

    internal class Pair : BaseNotifyModel
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

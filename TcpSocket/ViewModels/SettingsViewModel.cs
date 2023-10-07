using Helper.Utils;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Input;
using TcpSocket.Helper;
using TcpSocket.Models;
using TcpSocket.MsgEvents;
using WpfStyleResources.Helper;

namespace TcpSocket.ViewModels
{
    internal class SettingsViewModel : BindableBase
    {
        public SettingsViewModel(
            IConfigManager config,
            IContainerProvider containerProvider,
            IEventAggregator eventAggregator
            )
        {
            this._config = config;
            this.ImageDir = config.ReadConfigNode(nameof(this.ImageDir));
            this.LastMusicDir = config.ReadConfigNode("Music", nameof(this.LastMusicDir));
            this.LastVideoDir = config.ReadConfigNode("Video", nameof(this.LastVideoDir));

            this.InitHotkeys(config);

            this.InitCommands(containerProvider, eventAggregator);
        }

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
                this.IsEditingSetting = false;

                foreach (var item in this.HotKeys)
                {
                    item.GoBack();
                }
            });

            this.SubmitCommand = new DelegateCommand(() =>
            {
                var resultStr = containerProvider.Resolve<HotKeyHelper>().RegisterHotKeys(this.HotKeys);

                resultStr = string.IsNullOrEmpty(resultStr) ? "注册快捷键无错误" : resultStr;

                this.IsEditingSetting = false;

                eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage(resultStr, 4));
            });
        }

        public ICommand FindImageDirCommand { get; private set; }
        public ICommand FindMusicDirCommand { get; private set; }
        public ICommand FindVideoDirCommand { get; private set; }

        /// <summary>
        /// 还原未提交的修改
        /// </summary>
        public ICommand CancelCommand { get; private set; }
        /// <summary>
        /// 提交注册快捷键
        /// </summary>
        public ICommand SubmitCommand { get; private set; }

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


        public List<HotKeyModel> HotKeys { get; private set; } = new List<HotKeyModel>();

        private void InitHotkeys(IConfigManager config)
        {
            var str = config.ReadConfigNode(Constants.Hotkeys);
            if (!string.IsNullOrEmpty(str))
            {
                var hotkeyDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
                if (hotkeyDic != null && hotkeyDic.Count > 0)
                {
                    foreach (var item in hotkeyDic.Values)
                    {
                        this.HotKeys.Add(HotKeyModel.Parse(item));
                    }
                }
            }
            else
            {
                this.HotKeys.AddRange(new List<HotKeyModel>() {
                    new HotKeyModel(Constants.HotKeys.Pause, false, false, true, Keys.S),
                    new HotKeyModel(Constants.HotKeys.Prev, false, false, true, Keys.Left),
                    new HotKeyModel(Constants.HotKeys.Next, false, false, true, Keys.Right)
                });
            }

            config.SetConfig += config =>
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();

                foreach (var item in this.HotKeys)
                {
                    dic.Add(item.Name, item.ToString());
                }

                config.WriteConfigNode(JsonConvert.SerializeObject(dic), Constants.Hotkeys);
            };
        }

        private string _imageDir;

        public string ImageDir
        {
            get => this._imageDir;
            set
            {
                if(SetProperty<string>(ref _imageDir, value))
                {
                    _config.WriteConfigNode(this.ImageDir.EnsureEndsWith("/"), nameof(this.ImageDir));
                }
            }
        }

        private string _lastMusicDir;

        public string LastMusicDir
        {
            get => this._lastMusicDir;
            set
            {
                if(SetProperty<string>(ref _lastMusicDir, value))
                {
                    _config.WriteConfigNode(this.LastMusicDir.EnsureEndsWith("/"), "Music", nameof(this.LastMusicDir));
                }
            }
        }

        private string _lastVideoDir;

        public string LastVideoDir
        {
            get => this._lastVideoDir;
            set
            {
                if(SetProperty<string>(ref _lastVideoDir, value))
                {
                    _config.WriteConfigNode(this.LastVideoDir.EnsureEndsWith("/"), "Video", nameof(this.LastVideoDir));
                }
            }
        }
    }
}

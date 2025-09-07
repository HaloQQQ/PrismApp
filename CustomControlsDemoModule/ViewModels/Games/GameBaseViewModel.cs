using IceTea.Pure.BaseModels;
using IceTea.Pure.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace CustomControlsDemoModule.ViewModels
{
    internal abstract class GameBaseViewModel<T> : NotifyBase, IDialogAware where T : NotifyBase
    {
        protected virtual string GameName { get; }

        public GameBaseViewModel(IAppConfigFileHotKeyManager appConfigFileHotKeyManager, IConfigManager configManager, IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            this.LoadConfig(configManager);

            this.Datas = new ObservableCollection<T>();
            this.InitDatas();

            this.InitHotKeys(appConfigFileHotKeyManager);

            StartPauseCommand = new DelegateCommand(() =>
            {
                IsUsable = !IsUsable;
            }, () => !IsGameOver).ObservesProperty(() => IsGameOver);

            RePlayCommand = new DelegateCommand(RePlay_CommandExecute);

            this.Begin_Wav();
        }

        #region Logicals
        protected abstract bool CheckGameOver();

        protected void InitHotKeys(IAppConfigFileHotKeyManager appConfigFileHotKeyManager)
        {
            var groupName = GameName;

            appConfigFileHotKeyManager.TryAdd(groupName, new[] { "HotKeys", "App", groupName });

            appConfigFileHotKeyManager.TryRegisterItem(groupName, new AppHotKey("重玩", Key.R, ModifierKeys.Alt));
            appConfigFileHotKeyManager.TryRegisterItem(groupName, new AppHotKey("播放/暂停", Key.Space, ModifierKeys.None));

            this.InitHotKeysCore(appConfigFileHotKeyManager);

            KeyGestureDic = appConfigFileHotKeyManager.First(g => g.GroupName == groupName).ToDictionary(hotKey => hotKey.Name);
        }

        /// <summary>
        /// 注册快捷键
        /// </summary>
        /// <param name="appConfigFileHotKeyManager"></param>
        protected virtual void InitHotKeysCore(IAppConfigFileHotKeyManager appConfigFileHotKeyManager)
        {
        }

        protected abstract void InitDatas();

        protected virtual void LoadConfig(IConfigManager configManager) { }

        protected virtual void RePlay_CommandExecute()
        {
            this.IsGameOver = false;

            this.Restart_Wav();
        }
        #endregion

        #region Commons
        private MediaPlayer _player = new MediaPlayer();
        protected void PlayMedia(string mediaName)
        {
            _player.Open(new Uri(Path.Combine(AppStatics.ExeDirectory, "Resources/Medias", mediaName), UriKind.RelativeOrAbsolute));
            _player.Play();
        }

        protected void Begin_Wav() => this.PlayMedia("begin.wav");
        protected void Move_Wav() => this.PlayMedia("move.wav");
        protected void Over_Wav() => this.PlayMedia("over.wav");
        protected void Restart_Wav() => this.PlayMedia("restart.wav");


        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            this.Dispose();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
        #endregion

        #region Fields
        protected readonly IEventAggregator _eventAggregator;
        #endregion

        #region Props
        public Dictionary<string, IHotKey<Key, ModifierKeys>> KeyGestureDic { get; private set; }

        public IList<T> Datas { get; private set; }

        private bool _isUsable = true;
        public bool IsUsable
        {
            get => _isUsable;
            private set
            {
                if (SetProperty(ref _isUsable, value))
                {
                    this.OnUsableChanged(value);
                }
            }
        }

        protected virtual void OnUsableChanged(bool newValue)
        {
        }

        private bool _isGameOver;

#pragma warning disable CS0067 
        public event Action<IDialogResult> RequestClose;

        public bool IsGameOver
        {
            get => _isGameOver;
            protected set
            {
                if (SetProperty(ref _isGameOver, value) && value)
                {
                    this.OnGameOver();
                }

                IsUsable = !_isGameOver;
            }
        }

        protected virtual void OnGameOver()
        {
        }

        public string Title => this.GameName;
        #endregion

        #region Commands
        public ICommand RePlayCommand { get; protected set; }
        public ICommand StartPauseCommand { get; }
        #endregion
    }
}

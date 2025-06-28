using IceTea.Atom.BaseModels;
using IceTea.Atom.Contracts;
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
        }
        #endregion

        #region Commons
        private MediaPlayer _player = new MediaPlayer();
        protected void PlayMedia(string mediaName)
        {
            _player.Open(new Uri(Path.Combine(AppStatics.ExeDirectory, "Resources/medias" , mediaName), UriKind.RelativeOrAbsolute));
            _player.Play();
        }

        public bool CanCloseDialog()
        {             
            return true;
        }

        public void OnDialogClosed()
        {
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
            private set { SetProperty(ref _isUsable, value); }
        }

        private bool _isGameOver;

#pragma warning disable CS0067 
        public event Action<IDialogResult> RequestClose;

        public bool IsGameOver
        {
            get => _isGameOver;
            protected set
            {
                SetProperty(ref _isGameOver, value);
                IsUsable = !_isGameOver;
            }
        }

        public string Title => this.GameName;
        #endregion

        #region Commands
        public ICommand RePlayCommand { get; protected set; }
        public ICommand StartPauseCommand { get; }
        #endregion
    }
}

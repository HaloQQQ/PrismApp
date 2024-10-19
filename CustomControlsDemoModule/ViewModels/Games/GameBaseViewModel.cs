using IceTea.Atom.BaseModels;
using IceTea.Atom.Contracts;
using IceTea.Atom.Utils.HotKey.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using Prism.Commands;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace CustomControlsDemoModule.ViewModels
{
    internal abstract class GameBaseViewModel<T> : BaseNotifyModel where T : BaseNotifyModel
    {
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
        protected abstract void InitHotKeys(IAppConfigFileHotKeyManager appHotKeyManager);

        protected abstract void InitDatas();

        protected virtual void LoadConfig(IConfigManager configManager) { }

        protected virtual void RePlay_CommandExecute()
        {
            this.IsGameOver = false;
        }
        #endregion

        #region Fields
        protected readonly IEventAggregator _eventAggregator;
        #endregion

        #region Props
        public Dictionary<string, IHotKey<Key, ModifierKeys>> KeyGestureDic { get; protected set; }

        public IList<T> Datas { get; private set; }

        private bool _isUsable = true;
        public bool IsUsable
        {
            get => _isUsable;
            private set { SetProperty(ref _isUsable, value); }
        }

        private bool _isGameOver;
        public bool IsGameOver
        {
            get => _isGameOver;
            protected set
            {
                SetProperty(ref _isGameOver, value);
                IsUsable = !_isGameOver;
            }
        }
        #endregion

        #region Commands
        public ICommand RePlayCommand { get; protected set; }
        public ICommand StartPauseCommand { get; }
        #endregion
    }
}

using Prism.Mvvm;
using System.Windows.Input;
using Prism.Commands;
using System.Collections.ObjectModel;
using Prism.Events;
using IceTea.Atom.Utils;
using IceTea.Atom.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using IceTea.Atom.Utils.HotKey.Contracts;
using MusicPlayerModule.Models.Common;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using MusicPlayerModule.Models;


namespace MusicPlayerModule.ViewModels.Base
{
    internal abstract class MediaPlayerViewModel : BindableBase, IDisposable
    {
        protected readonly IEventAggregator _eventAggregator;
        protected readonly ISettingManager<SettingModel> _settingManager;

        protected MediaPlayerViewModel(IEventAggregator eventAggregator, IConfigManager config, IAppConfigFileHotKeyManager appConfigFileHotKeyManager, ISettingManager<SettingModel> settingManager)
        {
            this._eventAggregator = eventAggregator.AssertNotNull(nameof(IEventAggregator));
            this._settingManager = settingManager;

            this.LoadConfig(config);

            this.InitCommands();

            this.InitHotkeys(appConfigFileHotKeyManager);
        }

        public Dictionary<string, IHotKey<Key, ModifierKeys>> KeyGestureDic { get; protected set; }

        protected void InitHotkeys(IAppConfigFileHotKeyManager appConfigFileHotKeyManager)
        {
            var groupName = this.MediaType;
            appConfigFileHotKeyManager.TryAdd(groupName, this.MediaSettingNode);
            foreach (var item in this.MediaHotKeys)
            {
                appConfigFileHotKeyManager.TryRegisterItem(groupName, item);
            }

            this.KeyGestureDic = appConfigFileHotKeyManager.First(g => g.GroupName == groupName).ToDictionary(hotKey => hotKey.Name);
        }

        protected abstract void LoadConfig(IConfigManager configManager);

        protected virtual void InitCommands()
        {
            this.PointACommand =
                new DelegateCommand(
                    () => this.CurrentMedia?.SetPointA(this.CurrentMedia.CurrentMills),
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.PointBCommand =
                new DelegateCommand(
                    () => this.CurrentMedia?.SetPointB(this.CurrentMedia.CurrentMills),
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.ToPointACommand =
                new DelegateCommand(
                    () => this.CurrentMedia?.GoToPointA(),
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.ResetPointABCommand =
                new DelegateCommand(
                    () => this.CurrentMedia?.ResetABPoint(),
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.DelayCommand =
                new DelegateCommand(
                    this.Rewind,
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.PrevCommand =
                new DelegateCommand<MediaBaseViewModel>(
                    this.PrevMedia,
                    currentVideo => this.CurrentMedia != null && this.DisplayPlaying.Count > 0)
                    .ObservesProperty(() => this.CurrentMedia)
                    .ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.NextCommand =
                new DelegateCommand<MediaBaseViewModel>(
                    this.NextMedia,
                    currentVideo => this.CurrentMedia != null && this.DisplayPlaying.Count > 0)
                    .ObservesProperty(() => this.CurrentMedia)
                    .ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.AheadCommand =
                new DelegateCommand(
                    this.FastForward,
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.StopPlayCommand =
                new DelegateCommand(() =>
                    {
                        this.CurrentMedia = null;
                        this.Running = false;
                    },
                    () => !this.Running && this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia)
                    .ObservesProperty(() => this.Running);

            this.CleanPlayingCommand = new DelegateCommand(
                    this.CleanPlaying,
                    () => !this.Running && this.DisplayPlaying.Count > 0)
                    .ObservesProperty(() => this.Running)
                    .ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.MoveToHomeCommand = new DelegateCommand(
                    () => this.CurrentMedia.CurrentMills = 0,
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.MoveToEndCommand = new DelegateCommand(
                    () => this.CurrentMedia.CurrentMills =
                                this.CurrentMedia.TotalMills > 5000
                                    ? (this.CurrentMedia.TotalMills - 5000) : this.CurrentMedia.TotalMills,
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.PlayPlayingCommand = new DelegateCommand<MediaBaseViewModel>(
                    this.PlayPlaying,
                    _ => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);
        }

        #region Methods
        protected void SetAndPlay(MediaBaseViewModel? item)
        {
            this.CurrentMedia = item;

            if (this.Running = item != null)
            {
                this.CurrentMedia.Reset();

                this.RaiseResetPlayerAndPlayMediaEvent(this._eventAggregator);
            }
            else
            {
                this.RaiseResetMediaEvent(this._eventAggregator);
            }
        }

        protected virtual void Rewind()
        {
            this.CurrentMedia?.Rewind();
        }

        protected void PrevMedia(MediaBaseViewModel currentMedia)
        {
            if (currentMedia != null && this.DisplayPlaying.Count > 0)
            {
                if (this.CurrentPlayOrder != null && this.CurrentPlayOrder.OrderType == MediaPlayOrderModel.EnumOrderType.Random)
                {
                    this.SetAndPlay(this.DisplayPlaying[this._random.Next(this.DisplayPlaying.Count)]);
                    return;
                }

                if (currentMedia.Index > 1)
                {
                    this.SetAndPlay(this.DisplayPlaying[currentMedia.Index - 2]);
                }
                else
                {
                    this.SetAndPlay(this.DisplayPlaying[this.DisplayPlaying.Count - 1]);
                }
            }
        }

        protected abstract void RaiseResetMediaEvent(IEventAggregator eventAggregator);

        protected abstract void RaiseResetPlayerAndPlayMediaEvent(IEventAggregator eventAggregator);

        protected void NextMedia(MediaBaseViewModel currentMedia)
        {
            if (currentMedia != null && this.DisplayPlaying.Count > 0)
            {
                if (this.CurrentPlayOrder != null)
                {
                    switch (this.CurrentPlayOrder.OrderType)
                    {
                        case MediaPlayOrderModel.EnumOrderType.Order:
                            if (currentMedia.Index == this.DisplayPlaying.Count)
                            {
                                this.SetAndPlay(null);

                                return;
                            }
                            break;
                        case MediaPlayOrderModel.EnumOrderType.SingleOnce:
                            this.SetAndPlay(null);
                            return;

                        case MediaPlayOrderModel.EnumOrderType.SingleCycle:
                            this.RaiseResetPlayerAndPlayMediaEvent(this._eventAggregator);
                            return;
                        case MediaPlayOrderModel.EnumOrderType.Loop:
                            break;
                        case MediaPlayOrderModel.EnumOrderType.Random:
                            this.SetAndPlay(this.DisplayPlaying[this._random.Next(this.DisplayPlaying.Count)]);
                            return;
                        default:
                            throw new IndexOutOfRangeException();
                    }
                }

                if (currentMedia.Index < this.DisplayPlaying.Count)
                {
                    this.SetAndPlay(this.DisplayPlaying[currentMedia.Index]);
                }
                else
                {
                    this.SetAndPlay(this.DisplayPlaying[0]);
                }
            }
        }

        protected virtual void FastForward()
        {
            this.CurrentMedia?.FastForward();
        }

        protected virtual void CleanPlaying()
        {
            this.DisplayPlaying.Clear();
            this.CurrentMedia = null;
            this.Running = false;
        }

        protected abstract void PlayPlaying(MediaBaseViewModel currentMedia);
        #endregion

        #region Fields
        protected Random _random = new Random();
        #endregion

        #region Props
        #region HotKeys
        protected abstract string MediaType { get; }
        protected abstract string[] MediaSettingNode { get; }

        private static class MediaHotKeyConsts
        {
            internal const string ResetPointAB = "重置AB点";
            internal const string SetPointA = "设置A点";
            internal const string SetPointB = "设置B点";
            internal const string ToPointA = "去A点";

            internal const string DecreaseVolume = "降低音量";
            internal const string IncreaseVolume = "提高音量";

            internal const string PlayPlaying = "播放/暂停";
            internal const string CleanPlaying = "清空播放队列";
            internal const string StopPlayMedia = "停止";
            internal const string PlayingListPanel = "播放列表";

            internal const string OpenFolder = "打开文件夹";

            internal const string MoveToHome = "返回开头";
            internal const string MoveToEnd = "跳至结尾";

            internal const string Rewind = "快退";
            internal const string FastForward = "快进";

            internal const string Prev = "上一个";
            internal const string Next = "下一个";
        }

        protected virtual IEnumerable<AppHotKey> MediaHotKeys =>
        [
            new AppHotKey(MediaHotKeyConsts.ResetPointAB, Key.Delete, ModifierKeys.Control),
            new AppHotKey(MediaHotKeyConsts.SetPointA, Key.D1, ModifierKeys.Control),
            new AppHotKey(MediaHotKeyConsts.SetPointB, Key.D2, ModifierKeys.Control),
            new AppHotKey(MediaHotKeyConsts.ToPointA, Key.D3, ModifierKeys.Control),

            new AppHotKey(MediaHotKeyConsts.MoveToHome, Key.Home, ModifierKeys.None),
            new AppHotKey(MediaHotKeyConsts.MoveToEnd, Key.End, ModifierKeys.None),
            new AppHotKey(MediaHotKeyConsts.Rewind, Key.Left, ModifierKeys.None),
            new AppHotKey(MediaHotKeyConsts.FastForward, Key.Right, ModifierKeys.None),
            new AppHotKey(MediaHotKeyConsts.Prev, Key.PageUp, ModifierKeys.None),
            new AppHotKey(MediaHotKeyConsts.Next, Key.PageDown, ModifierKeys.None),

            new AppHotKey(MediaHotKeyConsts.DecreaseVolume, Key.Down, ModifierKeys.Control),
            new AppHotKey(MediaHotKeyConsts.IncreaseVolume, Key.Up, ModifierKeys.Control),

            new AppHotKey(MediaHotKeyConsts.OpenFolder, Key.O, ModifierKeys.Control),

            new AppHotKey(MediaHotKeyConsts.PlayPlaying, Key.Space, ModifierKeys.None),
            new AppHotKey(MediaHotKeyConsts.CleanPlaying, Key.D, ModifierKeys.Alt),
            new AppHotKey(MediaHotKeyConsts.StopPlayMedia, Key.W, ModifierKeys.Alt),

            new AppHotKey(MediaHotKeyConsts.PlayingListPanel, Key.X, ModifierKeys.Alt)
        ];
        #endregion

        public ObservableCollection<MediaBaseViewModel> DisplayPlaying { get; private set; } = new();

        protected MediaBaseViewModel _currentMedia;

        public virtual MediaBaseViewModel CurrentMedia
        {
            get { return _currentMedia; }
            set { throw new NotImplementedException(); }
        }

        private MediaPlayOrderModel _mediaPlayOrder;

        public MediaPlayOrderModel CurrentPlayOrder
        {
            get { return _mediaPlayOrder; }
            set { _mediaPlayOrder = value; IsEditingPlayOrder = false; }
        }

        private bool _isEditingPlayOrder;

        public bool IsEditingPlayOrder
        {
            get => this._isEditingPlayOrder;
            set => SetProperty<bool>(ref _isEditingPlayOrder, value);
        }

        protected bool _running;

        public virtual bool Running
        {
            get { return _running; }
            set { throw new NotImplementedException(); }
        }
        #endregion

        #region Commnads
        public ICommand MoveToHomeCommand { get; private set; }
        public ICommand MoveToEndCommand { get; private set; }

        public ICommand PointACommand { get; private set; }
        public ICommand PointBCommand { get; private set; }

        public ICommand ToPointACommand { get; private set; }

        public ICommand ResetPointABCommand { get; private set; }

        public ICommand StopPlayCommand { get; private set; }

        /// <summary>
        /// 清空播放队列
        /// </summary>
        public ICommand CleanPlayingCommand { get; protected set; }

        /// <summary>
        /// 播放或暂停
        /// </summary>
        public ICommand PlayPlayingCommand { get; protected set; }

        public ICommand PrevCommand { get; protected set; }
        public ICommand NextCommand { get; protected set; }

        /// <summary>
        /// 歌曲进度后退,此地只为让前端按钮在该禁用时禁用
        /// </summary>
        public ICommand DelayCommand { get; protected set; }

        /// <summary>
        /// 歌曲进度提前,此地只为让前端按钮在该禁用时禁用
        /// </summary>
        public ICommand AheadCommand { get; protected set; }
        #endregion

        public void Dispose()
        {
            this.PrevCommand = null;
            this.NextCommand = null;

            this.DelayCommand = null;
            this.AheadCommand = null;

            this.CleanPlayingCommand = null;

            this.PointACommand = null;
            this.PointBCommand = null;
            this.ResetPointABCommand = null;
            this.ToPointACommand = null;

            this.MoveToHomeCommand = null;
            this.MoveToEndCommand = null;

            this.StopPlayCommand = null;

            this.PlayPlayingCommand = null;

            this.DisplayPlaying.Clear();
            this.DisplayPlaying = null;
        }
    }
}

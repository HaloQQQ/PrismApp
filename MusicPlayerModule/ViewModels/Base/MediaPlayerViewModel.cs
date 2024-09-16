using Prism.Mvvm;
using System.Windows.Input;
using Prism.Commands;
using System.Collections.ObjectModel;
using Prism.Events;
using IceTea.Atom.Utils;
using IceTea.Atom.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using IceTea.Atom.Utils.HotKey.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using MusicPlayerModule.Models;
using MusicPlayerModule.MsgEvents;
using System.Diagnostics;
using System.IO;
using IceTea.Atom.Extensions;
using PrismAppBasicLib.MsgEvents;
using MusicPlayerModule.Contracts;

namespace MusicPlayerModule.ViewModels.Base
{
    internal abstract class MediaPlayerViewModel : BindableBase, IDisposable
    {
        protected readonly IEventAggregator _eventAggregator;
        protected readonly ISettingManager<SettingModel> _settingManager;

        protected readonly IConfigManager _configManager;

        protected MediaPlayerViewModel(IEventAggregator eventAggregator, IConfigManager configManager, IAppConfigFileHotKeyManager appConfigFileHotKeyManager, ISettingManager<SettingModel> settingManager)
        {
            this._eventAggregator = eventAggregator.AssertNotNull(nameof(IEventAggregator));
            this._configManager = configManager.AssertArgumentNotNull(nameof(IConfigManager));
            this._settingManager = settingManager;

            this._settingManager.TryAdd(CustomStatics.MUSIC, () => new SettingModel(string.Empty, configManager.ReadConfigNode(CustomStatics.LastMusicDir_ConfigKey), () => { }));
            this._settingManager.TryAdd(CustomStatics.LYRIC, () => new SettingModel(string.Empty, configManager.ReadConfigNode(CustomStatics.LastLyricDir_ConfigKey), () => { }));
            this._settingManager.TryAdd(CustomStatics.VIDEO, () => new SettingModel(string.Empty, configManager.ReadConfigNode(CustomStatics.LastVideoDir_ConfigKey), () => { }));

            this.LoadConfig(configManager);

            this.InitCommands();

            this.InitHotkeys(appConfigFileHotKeyManager);

            this.SubscribeEvents(eventAggregator);
        }

        public Dictionary<string, IHotKey<Key, ModifierKeys>> KeyGestureDic { get; protected set; }

        protected void PublishMessage(string msg, int seconds = 3)
        {
            _eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage(msg, seconds));
        }

        protected void InitHotkeys(IAppConfigFileHotKeyManager appConfigFileHotKeyManager)
        {
            var groupName = this.MediaType;
            appConfigFileHotKeyManager.TryAdd(groupName, this.MediaHotKey_ConfigKey);

            foreach (var item in this.MediaHotKeys)
            {
                appConfigFileHotKeyManager.TryRegisterItem(groupName, item);
            }

            this.KeyGestureDic = appConfigFileHotKeyManager.First(g => g.GroupName == groupName).ToDictionary(hotKey => hotKey.Name);
        }

        protected virtual void LoadConfig(IConfigManager configManager)
        {
            var playOrder = configManager.ReadConfigNode(this.MediaPlayOrder_ConfigKey);
            this.CurrentPlayOrder =
                CustomStatics.MediaPlayOrderList.FirstOrDefault(item => item.Description == playOrder) ??
                CustomStatics.MediaPlayOrderList.First();

            configManager.SetConfig += config =>
            {
                config.WriteConfigNode(this.CurrentPlayOrder.Description, this.MediaPlayOrder_ConfigKey);
            };

            configManager.PostSetConfig += config =>
            {
                foreach (var item in this.DisplayPlaying.Where(m => m.PointAMills != 0 || m.PointBMills != 0))
                {
                    ICollection<string> mediaNode = new List<string>(MediaABPoints_ConfigKey)
                    {
                        item.MediaName
                    };

                    ICollection<string> pointANode = new List<string>(mediaNode)
                    {
                        nameof(MediaBaseViewModel.PointAMills)
                    };

                    ICollection<string> pointBNode = new List<string>(mediaNode)
                    {
                        nameof(MediaBaseViewModel.PointBMills)
                    };

                    config.WriteConfigNode(item.MediaName, mediaNode.ToArray());
                    config.WriteConfigNode(item.PointAMills, pointANode.ToArray());
                    config.WriteConfigNode(item.PointBMills, pointBNode.ToArray());
                }
            };
        }

        protected virtual void InitCommands()
        {
            this.OpenInExploreCommand = new DelegateCommand<string>(mediaDir =>
            {
                if (Directory.Exists(mediaDir))
                {
                    Process.Start("explorer", mediaDir);
                }
            });

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

            this.AllToPointACommand = new DelegateCommand(
                    () => _eventAggregator.GetEvent<GoBackMediaPointAEvent>().Publish(),
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.ResetPointABCommand =
                new DelegateCommand(
                    () => this.CurrentMedia?.ResetABPoint(),
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.RewindCommand =
                new DelegateCommand(
                    Rewind_CommandExecute,
                    () => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.PrevCommand =
                new DelegateCommand<MediaBaseViewModel>(
                    PrevMedia_CommandExecute,
                    currentVideo => this.CurrentMedia != null && this.DisplayPlaying.Count > 0)
                    .ObservesProperty(() => this.CurrentMedia)
                    .ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.NextCommand =
                new DelegateCommand<MediaBaseViewModel>(
                    NextMedia_CommandExecute,
                    currentVideo => this.CurrentMedia != null && this.DisplayPlaying.Count > 0)
                    .ObservesProperty(() => this.CurrentMedia)
                    .ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.FastForwardCommand =
                new DelegateCommand(
                    FastForward_CommandExecute,
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
                    CleanPlaying_CommandExecute,
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
                    PlayPlaying_CommandExecute,
                    _ => this.CurrentMedia != null)
                    .ObservesProperty(() => this.CurrentMedia);

            this.AddFilesCommand = new DelegateCommand(AddMediaFromFileDialog_CommandExecute);

            this.AddFolderCommand = new DelegateCommand(AddMediaFromFolderDialog_CommandExecute);

            this.DeletePlayingCommand = new DelegateCommand<MediaBaseViewModel>(
                    DeletePlaying_CommandExecute,
                    _ => this.DisplayPlaying.Count > 0
                )
                .ObservesProperty(() => this.DisplayPlaying.Count);

            this.IncreaseVolumeCommand = new DelegateCommand(
                () => _eventAggregator.GetEvent<IncreaseVolumeEvent>().Publish(), 
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

            this.DecreaseVolumeCommand = new DelegateCommand(
                () => _eventAggregator.GetEvent<DecreaseVolumeEvent>().Publish(),
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);
        }

        #region CommandExecute
        protected virtual void CleanPlaying_CommandExecute()
        {
            this.DisplayPlaying.Clear();
            this.CurrentMedia = null;
            this.Running = false;
        }

        protected virtual void Rewind_CommandExecute()
        {
            this.CurrentMedia?.Rewind();
        }

        protected virtual void FastForward_CommandExecute()
        {
            this.CurrentMedia?.FastForward();
        }

        protected void PrevMedia_CommandExecute(MediaBaseViewModel currentMedia)
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

        protected void NextMedia_CommandExecute(MediaBaseViewModel currentMedia)
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

        protected virtual void DeletePlaying_CommandExecute(MediaBaseViewModel media)
        {
            if (media != null && media == this.CurrentMedia)
            {
                if (this.DisplayPlaying.Count > 1)
                {
                    this.NextMedia_CommandExecute(media);
                }
                else
                {
                    this.CurrentMedia = null;
                    this.Running = false;
                }
            }

            this.DisplayPlaying.Remove(media);

            this.RefreshPlayingIndex();
        }

        protected abstract void AddMediaFromFileDialog_CommandExecute();
        protected abstract void AddMediaFromFolderDialog_CommandExecute();

        protected abstract void PlayPlaying_CommandExecute(MediaBaseViewModel currentMedia);
        #endregion

        #region Methods
        protected abstract void RaiseContinueMediaEvent();

        protected abstract void RaisePauseMediaEvent();

        protected abstract void RaiseResetMediaEvent(IEventAggregator eventAggregator);

        protected abstract void RaiseResetPlayerAndPlayMediaEvent(IEventAggregator eventAggregator);

        protected virtual void SubscribeEvents(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<GoBackMediaPointAEvent>().Subscribe(() => this.CurrentMedia?.GoToPointA());
        }

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

        protected void RefreshPlayingIndex()
        {
            var index = 1;
            foreach (var item in this.DisplayPlaying)
            {
                item.Index = index++;
            }
        }
        #endregion

        #region Fields
        private bool _isLoading;

        protected Random _random = new Random();
        #endregion

        #region Props
        #region ConfigKeys
        protected abstract string[] MediaABPoints_ConfigKey { get; }

        protected abstract string[] MediaPlayOrder_ConfigKey { get; }
        #endregion

        #region HotKeys
        protected abstract string MediaType { get; }
        protected abstract string[] MediaHotKey_ConfigKey { get; }

        private static class MediaHotKeyConsts
        {
            internal const string ResetPointAB = "重置AB点";
            internal const string SetPointA = "设置A点";
            internal const string SetPointB = "设置B点";
            internal const string ToPointA = "去A点";
            internal const string AllToPointA = "全体返回A点";

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
            new AppHotKey(MediaHotKeyConsts.ToPointA, Key.D3, ModifierKeys.Alt),
            new AppHotKey(MediaHotKeyConsts.AllToPointA, Key.D1, ModifierKeys.Alt),

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

        public bool IsLoading
        {
            get => this._isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }

        public ObservableCollection<MediaBaseViewModel> DisplayPlaying { get; private set; } = new();

        protected virtual void AllMediaModelNotPlaying()
        {
            foreach (var item in this.DisplayPlaying.Where(m => m.IsPlayingMedia))
            {
                item.IsPlayingMedia = false;
            }
        }

        protected MediaBaseViewModel _currentMedia;
        public virtual MediaBaseViewModel CurrentMedia
        {
            get { return _currentMedia; }
            set
            {
                if (SetProperty(ref _currentMedia, value) && value != null)
                {
                    this.AllMediaModelNotPlaying();

                    _currentMedia.IsPlayingMedia = true;

                    if (!_currentMedia.LoadedABPoint)
                    {
                        ICollection<string> pointANode = new List<string>(MediaABPoints_ConfigKey)
                        {
                            _currentMedia.MediaName,
                            nameof(MediaBaseViewModel.PointAMills)
                        };

                        if (int.TryParse(
                                this._configManager.ReadConfigNode(pointANode.ToArray()),
                                out int mills))
                        {
                            _currentMedia.CurrentMills = mills;
                            _currentMedia.SetPointA(mills);
                        }

                        ICollection<string> pointBNode = new List<string>(MediaABPoints_ConfigKey)
                        {
                            _currentMedia.MediaName,
                            nameof(MediaBaseViewModel.PointBMills)
                        };

                        if (int.TryParse(
                                this._configManager.ReadConfigNode(pointBNode.ToArray()),
                                out mills))
                        {
                            _currentMedia.SetPointB(mills);
                        }

                        _currentMedia.LoadedABPoint = true;
                    }
                }
            }
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
        public ICommand OpenInExploreCommand { get; private set; }

        /// <summary>
        /// 删除一条Playing
        /// </summary>
        public ICommand DeletePlayingCommand { get; private set; }

        /// <summary>
        /// 从本地添加媒体文件到列表
        /// </summary>
        public ICommand AddFilesCommand { get; private set; }

        /// <summary>
        /// 从文件夹添加媒体文件到列表
        /// </summary>
        public ICommand AddFolderCommand { get; private set; }

        public ICommand MoveToHomeCommand { get; private set; }
        public ICommand MoveToEndCommand { get; private set; }

        public ICommand PointACommand { get; private set; }
        public ICommand PointBCommand { get; private set; }

        public ICommand ToPointACommand { get; private set; }
        public ICommand AllToPointACommand { get; private set; }

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

        public ICommand IncreaseVolumeCommand { get; private set; }
        public ICommand DecreaseVolumeCommand { get; private set; }

        /// <summary>
        /// 歌曲进度后退,此地只为让前端按钮在该禁用时禁用
        /// </summary>
        public ICommand RewindCommand { get; protected set; }

        /// <summary>
        /// 歌曲进度提前,此地只为让前端按钮在该禁用时禁用
        /// </summary>
        public ICommand FastForwardCommand { get; protected set; }
        #endregion

        public void Dispose()
        {
            this.OpenInExploreCommand = null;

            this.AddFilesCommand = null;
            this.AddFolderCommand = null;

            this.PrevCommand = null;
            this.NextCommand = null;

            this.RewindCommand = null;
            this.FastForwardCommand = null;

            this.CleanPlayingCommand = null;

            this.PointACommand = null;
            this.PointBCommand = null;
            this.ResetPointABCommand = null;
            this.ToPointACommand = null;

            this.MoveToHomeCommand = null;
            this.MoveToEndCommand = null;

            this.StopPlayCommand = null;

            this.PlayPlayingCommand = null;

            foreach (var item in this.DisplayPlaying)
            {
                item.Dispose();
            }
            this.DisplayPlaying.Clear();
            this.DisplayPlaying = null;
        }
    }
}

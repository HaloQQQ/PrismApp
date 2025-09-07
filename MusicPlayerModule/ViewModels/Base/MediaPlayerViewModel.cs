using System.Windows.Input;
using System.Collections.ObjectModel;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.Contracts;
using PrismAppBasicLib.Models;
using PrismAppBasicLib.Contracts;
using MusicPlayerModule.MsgEvents.Media;
using IceTea.Pure.BaseModels;
using Prism.Events;
using IceTea.Pure.Contracts;
using IceTea.Pure.Utils;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using Prism.Commands;
using IceTea.Pure.Extensions;
using IceTea.Wpf.Atom.Utils.HotKey.App;

namespace MusicPlayerModule.ViewModels.Base;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8604 // 引用类型参数可能为 null。
internal abstract class MediaPlayerViewModel : NotifyBase
{
    protected readonly IEventAggregator _eventAggregator;
    protected readonly ISettingManager<SettingModel> _settingManager;

    protected readonly IConfigManager _configManager;

    protected MediaPlayerViewModel(IEventAggregator eventAggregator, IConfigManager configManager, IAppConfigFileHotKeyManager appConfigFileHotKeyManager, ISettingManager<SettingModel> settingManager)
    {
        this._eventAggregator = eventAggregator.AssertNotNull(nameof(IEventAggregator));
        this._configManager = configManager.AssertArgumentNotNull(nameof(IConfigManager));
        this._settingManager = settingManager.AssertArgumentNotNull(nameof(ISettingManager<SettingModel>));

        this._settingManager.TryAdd(CustomStatics.MUSIC, () => new SettingModel(string.Empty, configManager.ReadConfigNode<string>(CustomStatics.LastMusicDir_ConfigKey), null));
        this._settingManager.TryAdd(CustomStatics.LYRIC, () => new SettingModel(string.Empty, configManager.ReadConfigNode<string>(CustomStatics.LastLyricDir_ConfigKey), null));
        this._settingManager.TryAdd(CustomStatics.VIDEO, () => new SettingModel(string.Empty, configManager.ReadConfigNode<string>(CustomStatics.LastVideoDir_ConfigKey), null));

        this.LoadConfig(configManager);

        this.InitCommands();

        this.InitHotkeys(appConfigFileHotKeyManager);

        this.SubscribeEvents(eventAggregator);
    }

    public Dictionary<string, IHotKey<Key, ModifierKeys>> KeyGestureDic { get; private set; }

    private void InitHotkeys(IAppConfigFileHotKeyManager appConfigFileHotKeyManager)
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
        var playOrder = configManager.ReadConfigNode<string>(this.MediaPlayOrder_ConfigKey);
        this.CurrentPlayOrder =
            CustomStatics.MediaPlayOrderList.FirstOrDefault(
                    item => item.Description == playOrder,
                    CustomStatics.MediaPlayOrderList.First()
                );

        configManager.SetConfig += config =>
        {
            config.WriteConfigNode(this.CurrentPlayOrder.Description, this.MediaPlayOrder_ConfigKey);
        };

        configManager.PostSetConfig += config =>
        {
            foreach (var item in this.DisplayPlaying.Where(m => m.ShouleSaveABPoint()))
            {
                IList<string> mediaNode = new List<string>(MediaABPoints_ConfigKey)
                {
                    item.MediaName
                };

                IList<string> pointANode = new List<string>(mediaNode)
                {
                    nameof(MediaPlayerBaseViewModel.PointAMills)
                };

                IList<string> pointBNode = new List<string>(mediaNode)
                {
                    nameof(MediaPlayerBaseViewModel.PointBMills)
                };

                config.WriteConfigNode(item.MediaName, mediaNode);
                config.WriteConfigNode(item.PointAMills, pointANode);
                config.WriteConfigNode(item.PointBMills, pointBNode);
            }
        };
    }

    protected virtual void InitCommandsExtend() { }

    protected void InitCommands()
    {
        this.OpenInExploreCommand = new DelegateCommand<string>(mediaDir =>
        {
            if (mediaDir.IsDirectoryExists())
            {
                AppUtils.OpenExplorer(mediaDir);
            }
        });

        this.PointACommand = new DelegateCommand(
                () => this.CurrentMedia?.SetPointA(this.CurrentMedia.CurrentMills),
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

        this.PointBCommand = new DelegateCommand(
                () => this.CurrentMedia?.SetPointB(this.CurrentMedia.CurrentMills),
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

        this.ToPointACommand = new DelegateCommand(
                () => this.CurrentMedia?.GoToPointA(),
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

        this.AllToPointACommand = new DelegateCommand(
                () => _eventAggregator.GetEvent<GoBackMediaPointAEvent>().Publish(),
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

        this.ResetPointABCommand = new DelegateCommand(
                () => this.CurrentMedia?.ResetABPoint(),
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

        this.RewindCommand = new DelegateCommand(
                Rewind_CommandExecute,
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

        this.PrevCommand = new DelegateCommand<MediaPlayerBaseViewModel>(
                PrevMedia_CommandExecute,
                currentVideo => this.CurrentMedia != null && this.DisplayPlaying.Count > 0)
                .ObservesProperty(() => this.CurrentMedia)
                .ObservesProperty<int>(() => this.DisplayPlaying.Count);

        this.NextCommand = new DelegateCommand<MediaPlayerBaseViewModel>(
                NextMedia_CommandExecute,
                currentVideo => this.CurrentMedia != null && this.DisplayPlaying.Count > 0)
                .ObservesProperty(() => this.CurrentMedia)
                .ObservesProperty<int>(() => this.DisplayPlaying.Count);

        this.FastForwardCommand = new DelegateCommand(
                FastForward_CommandExecute,
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

        this.StopPlayCommand = new DelegateCommand(
                () => this.CurrentMedia = null,
                () => !this.Running && this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia)
                .ObservesProperty(() => this.Running);

        this.CleanPlayingCommand = new DelegateCommand(
                CleanPlaying_CommandExecute,
                () => !this.Running && this.DisplayPlaying.Count > 0)
                .ObservesProperty(() => this.Running)
                .ObservesProperty<int>(() => this.DisplayPlaying.Count);

        this.AllMoveToHomeEventCommand = new DelegateCommand(() => this._eventAggregator.GetEvent<MoveToHomeEvent>().Publish());

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

        this.TogglePlayCommand = new DelegateCommand<MediaPlayerBaseViewModel>(
                PlayInPlaying_CommandExecute,
                _ => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

        this.TogglePlayCurrentCommand = new DelegateCommand<MediaPlayerBaseViewModel>(
                PlayInPlaying_CommandExecute);

        this.AddFilesCommand = new DelegateCommand(AddMediaFromFileDialog_CommandExecute);

        this.AddFolderCommand = new DelegateCommand(AddMediaFromFolderDialog_CommandExecute);

        this.DeletePlayingCommand = new DelegateCommand<MediaPlayerBaseViewModel>(
                DeleteFromPlaying_CommandExecute,
                _ => this.DisplayPlaying.Count > 0)
                .ObservesProperty(() => this.DisplayPlaying.Count);

        this.IncreaseVolumeCommand = new DelegateCommand(
            () => _eventAggregator.GetEvent<IncreaseVolumeEvent>().Publish(),
            () => this.CurrentMedia != null)
            .ObservesProperty(() => this.CurrentMedia);

        this.DecreaseVolumeCommand = new DelegateCommand(
            () => _eventAggregator.GetEvent<DecreaseVolumeEvent>().Publish(),
            () => this.CurrentMedia != null)
            .ObservesProperty(() => this.CurrentMedia);

        this.InitCommandsExtend();
    }

    #region CommandExecute
    protected virtual void CleanPlaying_CommandExecute()
    {
        for (int i = this.DisplayPlaying.Count - 1; i >= 0; i--)
        {
            this.DisplayPlaying[i].Dispose();
        }

        this.DisplayPlaying.Clear();
        this.CurrentMedia = null;
    }

    protected virtual void Rewind_CommandExecute()
    {
        this.CurrentMedia?.Rewind();
    }

    protected virtual void FastForward_CommandExecute()
    {
        this.CurrentMedia?.FastForward();
    }

    protected void PrevMedia_CommandExecute(MediaPlayerBaseViewModel? currentMedia)
    {
        if (currentMedia != null && this.DisplayPlaying.Count > 0)
        {
            if (this.CurrentPlayOrder != null)
            {
                switch (this.CurrentPlayOrder.OrderType)
                {
                    case MediaPlayOrderModel.EnumOrderType.Order:
                        if (currentMedia.Index <= 1)
                        {
                            currentMedia = null;
                        }
                        else
                        {
                            currentMedia = this.DisplayPlaying[currentMedia.Index - 2];
                        }
                        break;
                    case MediaPlayOrderModel.EnumOrderType.SingleOnce:
                        currentMedia = null;
                        break;
                    case MediaPlayOrderModel.EnumOrderType.SingleCycle:
                        currentMedia = this.CurrentMedia;
                        break;
                    case MediaPlayOrderModel.EnumOrderType.Loop:
                        currentMedia = currentMedia.Index > 1
                            ? this.DisplayPlaying[currentMedia.Index - 2] : this.DisplayPlaying.Last();
                        break;
                    case MediaPlayOrderModel.EnumOrderType.Random:
                        currentMedia = this.DisplayPlaying[this._random.Next(this.DisplayPlaying.Count)];
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }

            this.SetAndPlay(currentMedia);
        }
    }

    protected void NextMedia_CommandExecute(MediaPlayerBaseViewModel? currentMedia)
    {
        if (currentMedia != null && this.DisplayPlaying.Count > 0)
        {
            if (this.CurrentPlayOrder != null)
            {
                switch (this.CurrentPlayOrder.OrderType)
                {
                    case MediaPlayOrderModel.EnumOrderType.Order:
                        if (currentMedia.Index >= this.DisplayPlaying.Count)
                        {
                            currentMedia = null;
                        }
                        else
                        {
                            currentMedia = this.DisplayPlaying[currentMedia.Index];
                        }
                        break;
                    case MediaPlayOrderModel.EnumOrderType.SingleOnce:
                        currentMedia = null;
                        break;
                    case MediaPlayOrderModel.EnumOrderType.SingleCycle:
                        currentMedia = this.CurrentMedia;
                        break;
                    case MediaPlayOrderModel.EnumOrderType.Loop:
                        currentMedia = currentMedia.Index < this.DisplayPlaying.Count
                            ? this.DisplayPlaying[currentMedia.Index] : this.DisplayPlaying.First();
                        break;
                    case MediaPlayOrderModel.EnumOrderType.Random:
                        currentMedia = this.DisplayPlaying[this._random.Next(this.DisplayPlaying.Count)];
                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }

            this.SetAndPlay(currentMedia);
        }
    }

    /// <summary>
    /// 删除播放列表中的当前Media
    /// </summary>
    /// <param name="media"></param>
    protected void DeleteFromPlaying_CommandExecute(MediaPlayerBaseViewModel media)
    {
        if (media.IsNotNullAnd(m => m.IsPlayingMedia))
        {
            CommonUtil.PublishMessage(_eventAggregator, "当前音乐正在播放，不允许删除");

            return;
        }

        if (media.IsNotNullAnd(_ => _ == this.CurrentMedia))
        {
            if (this.DisplayPlaying.Count > 1)
            {
                this.NextMedia_CommandExecute(media);
            }
            else
            {
                this.CurrentMedia = null;
            }
        }

        media.Dispose();

        this.TryRefreshPlayingIndex();
    }

    protected abstract void AddMediaFromFileDialog_CommandExecute();
    protected abstract void AddMediaFromFolderDialog_CommandExecute();

    protected virtual void PlayInPlaying_CommandExecute(MediaPlayerBaseViewModel currentMedia)
    {
        if (currentMedia == this.CurrentMedia)
        {
            if (this.Running = !this.Running)
            {
                this.RaiseContinueMediaEvent();
            }
            else
            {
                this.RaisePauseMediaEvent();
            }
        }
        else
        {
            this.SetAndPlay(currentMedia);
        }
    }
    #endregion

    #region Methods
    protected virtual void RaiseContinueMediaEvent()
    {
        _eventAggregator.GetEvent<ContinueCurrentMediaEvent>().Publish();
    }

    protected virtual void RaisePauseMediaEvent()
    {
        _eventAggregator.GetEvent<PauseCurrentMediaEvent>().Publish();
    }

    protected virtual void RaiseResetMediaEvent()
    {
        _eventAggregator.GetEvent<ResetMediaPlayerEvent>().Publish();
    }

    protected virtual void RaiseResetPlayerAndPlayMediaEvent()
    {
        _eventAggregator.GetEvent<ResetPlayerAndPlayMediaEvent>().Publish();
    }

    protected virtual void SubscribeEvents(IEventAggregator eventAggregator)
    {
        eventAggregator.GetEvent<MoveToHomeEvent>().Subscribe(() =>
        {
            if (CurrentMedia != null && Running)
            {
                this.CurrentMedia.CurrentMills = 0;
            }
        });

        eventAggregator.GetEvent<GoBackMediaPointAEvent>().Subscribe(() => this.CurrentMedia?.GoToPointA());

        eventAggregator.GetEvent<FastForwardMediaEvent>().Subscribe(() => this.CurrentMedia?.FastForward());
        eventAggregator.GetEvent<RewindMediaEvent>().Subscribe(() => this.CurrentMedia?.Rewind());

        eventAggregator.GetEvent<PrevMediaEvent>().Subscribe(() => this.PrevMedia_CommandExecute(this.CurrentMedia));
        eventAggregator.GetEvent<NextMediaEvent>().Subscribe(() => this.NextMedia_CommandExecute(this.CurrentMedia));

        eventAggregator.GetEvent<ToggeleCurrentMediaEvent>().Subscribe(() =>
        {
            if (this.CurrentMedia != null)
            {
                if (this.Running = !this.Running)
                {
                    this.RaiseContinueMediaEvent();
                }
                else
                {
                    this.RaisePauseMediaEvent();
                }
            }
        });
    }

    protected void SetAndPlay(MediaPlayerBaseViewModel? item)
    {
        this.CurrentMedia = item;

        if (this.Running = item != null)
        {
            this.CurrentMedia.Reset();

            this.RaiseResetPlayerAndPlayMediaEvent();
        }
        else
        {
            this.RaiseResetMediaEvent();
        }
    }

    protected virtual bool AllowRefreshPlayingIndex => true;

    protected void TryRefreshPlayingIndex()
    {
        if (this.AllowRefreshPlayingIndex)
        {
            var index = 1;
            foreach (var item in this.DisplayPlaying)
            {
                item.Index = index++;
            }
        }
    }

    protected virtual void AllMediaModelNotPlaying()
    {
        foreach (var item in this.DisplayPlaying.Where(m => m.IsPlayingMedia))
        {
            item.IsPlayingMedia = false;
        }
    }
    #endregion

    #region Fields
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

        internal const string AllMoveToHome = "全体返回开头";
        internal const string MoveToHome = "返回开头";
        internal const string MoveToEnd = "跳至结尾";

        internal const string Rewind = "快退";
        internal const string FastForward = "快进";

        internal const string Prev = "上一个";
        internal const string Next = "下一个";
    }

    protected virtual IEnumerable<AppHotKey> MediaHotKeys =>
        new AppHotKey[]
        {
            new AppHotKey(MediaHotKeyConsts.ResetPointAB, Key.Delete, ModifierKeys.Control),
            new AppHotKey(MediaHotKeyConsts.SetPointA, Key.D1, ModifierKeys.Control),
            new AppHotKey(MediaHotKeyConsts.SetPointB, Key.D2, ModifierKeys.Control),
            new AppHotKey(MediaHotKeyConsts.ToPointA, Key.D3, ModifierKeys.Alt),
            new AppHotKey(MediaHotKeyConsts.AllToPointA, Key.D1, ModifierKeys.Alt),

            new AppHotKey(MediaHotKeyConsts.AllMoveToHome, Key.Home, ModifierKeys.Control),
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
        };
    #endregion

    public ObservableCollection<MediaPlayerBaseViewModel> DisplayPlaying { get; } = new();

    protected MediaPlayerBaseViewModel _currentMedia;
    public virtual MediaPlayerBaseViewModel CurrentMedia
    {
        get { return _currentMedia; }
        set
        {
            if (SetProperty(ref _currentMedia, value))
            {
                if (value == null)
                {
                    this.Running = false;

                    return;
                }

                this.AllMediaModelNotPlaying();

                _currentMedia.IsPlayingMedia = true;

                if (!_currentMedia.LoadedABPoint)
                {
                    IList<string> pointANode = new List<string>(MediaABPoints_ConfigKey)
                    {
                        _currentMedia.MediaName,
                        nameof(MediaPlayerBaseViewModel.PointAMills)
                    };

                    if (int.TryParse(
                            this._configManager.ReadConfigNode<string>(pointANode),
                            out int mills))
                    {
                        _currentMedia.CurrentMills = mills;
                        _currentMedia.SetPointA(mills);
                    }

                    IList<string> pointBNode = new List<string>(MediaABPoints_ConfigKey)
                    {
                        _currentMedia.MediaName,
                        nameof(MediaPlayerBaseViewModel.PointBMills)
                    };

                    if (int.TryParse(
                            this._configManager.ReadConfigNode<string>(pointBNode),
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
        set
        {
            if (SetProperty<bool>(ref _running, value))
            {
                this._eventAggregator.GetEvent<MediaProgressTimerIsEnableUpdatedEvent>().Publish(value);
            }
        }
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

    public ICommand AllMoveToHomeEventCommand { get; private set; }
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
    public ICommand CleanPlayingCommand { get; private set; }

    /// <summary>
    /// 播放或暂停
    /// </summary>
    public ICommand TogglePlayCommand { get; private set; }

    /// <summary>
    /// 启停播放列表音乐
    /// </summary>
    public ICommand TogglePlayCurrentCommand { get; private set; }

    public ICommand PrevCommand { get; private set; }
    public ICommand NextCommand { get; private set; }

    public ICommand IncreaseVolumeCommand { get; private set; }
    public ICommand DecreaseVolumeCommand { get; private set; }

    /// <summary>
    /// 歌曲进度后退,此地只为让前端按钮在该禁用时禁用
    /// </summary>
    public ICommand RewindCommand { get; private set; }

    /// <summary>
    /// 歌曲进度提前,此地只为让前端按钮在该禁用时禁用
    /// </summary>
    public ICommand FastForwardCommand { get; private set; }
    #endregion

    protected override void DisposeCore()
    {
        base.DisposeCore();

        this.CleanPlaying_CommandExecute();
    }
}

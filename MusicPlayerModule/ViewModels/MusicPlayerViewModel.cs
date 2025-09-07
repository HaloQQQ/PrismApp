using MusicPlayerModule.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MusicPlayerModule.ViewModels.Base;
using MusicPlayerModule.Contracts;
using MusicPlayerModule.MsgEvents.Music;
using PrismAppBasicLib.Models;
using PrismAppBasicLib.Contracts;
using Prism.Events;
using IceTea.Pure.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using Prism.Commands;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using IceTea.Pure.Extensions;

namespace MusicPlayerModule.ViewModels;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
internal class MusicPlayerViewModel : MediaPlayerViewModel
{
    protected override string MediaType => "音乐";

    protected override string[] MediaHotKey_ConfigKey => new string[] { "HotKeys", "App", "Music" };

    protected override string[] MediaPlayOrder_ConfigKey => new string[] { CustomStatics.EnumSettings.Music.ToString(), "MusicPlayOrder" };

    protected override string[] MediaABPoints_ConfigKey => new string[] { CustomStatics.EnumSettings.Music.ToString(), "MusicABPoints" };

    private SettingModel LyricSetting => this._settingManager[CustomStatics.LYRIC];

    protected override void AllMediaModelNotPlaying()
    {
        foreach (var item in this.Playing.Where(m => m.IsPlayingMedia))
        {
            item.IsPlayingMedia = false;
        }
    }

    #region AddMusicToPlaying

    /// <summary>
    ///  默认播放列表第一个
    /// </summary>
    /// <param name="favoriteMusicViewModel"></param>
    private void PlayCurrentItems(BatchAddAndPlayModel? aggregate = null)
    {
        if (aggregate == null)
        {
            aggregate = new BatchAddAndPlayModel(
                this.DistributeMusicViewModel.DisplayFavorites.First(),
                this.DistributeMusicViewModel.DisplayFavorites);
        }

        this.AddItemsToPlaying(aggregate);
    }

    private PlayingMusicViewModel AddOneToPlaying(FavoriteMusicViewModel music)
    {
        var item = new PlayingMusicViewModel(music.Music, this.LyricSetting);

        item.TryAddTo(this.Playing);
        item.TryAddTo(this.DisplayPlaying);

        item.Index = this.Playing.Count;

        return item;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="aggregate">要添加到播放队列的数据</param>
    /// <param name="replace">true:清空并填充 false:追加</param>
    private void AddItemsToPlaying(BatchAddAndPlayModel aggregate, bool replace = true)
    {
        PlayingMusicViewModel? result = null;

        if (this.DistributeMusicViewModel.DisplayFavorites.Count > 0)
        {
            bool isFirstNotReplace = !replace && this.Playing.Count == 0;
            if (replace)
            {
                for (int i = this.Playing.Count - 1; i >= 0; i--)
                {
                    this.Playing[i].DisConnect();
                }
            }

            foreach (var item in aggregate.Collection)
            {
                if (!this.Playing.Any(m => m.Equals(item)))
                {
                    PlayingMusicViewModel temp = new PlayingMusicViewModel(item.Music, this.LyricSetting);
                    if (result == null)
                    {
                        result = temp;
                    }

                    temp.TryAddTo(this.Playing);
                    temp.TryAddTo(this.DisplayPlaying);

                    if (replace && temp.Equals(aggregate.TargetToPlay))
                    {
                        result = temp;
                    }
                }
            }

            if (result != null)
            {
                this.TryRefreshPlayingIndex();

                if (isFirstNotReplace || aggregate.TargetToPlay != null)
                {
                    this.SetAndPlay(result);
                }
            }
        }
    }

    #endregion

    #region General Props
    public DesktopLyricViewModel DesktopLyric { get; }

    public DistributeMusicViewModel DistributeMusicViewModel { get; }

    public Collection<PlayingMusicViewModel> Playing { get; } = new();

    protected override bool AllowRefreshPlayingIndex => PlayingListFilteKeyWords.IsNullOrBlank();

    /// <summary>
    /// 播放队列筛选条件
    /// </summary>
    public string PlayingListFilteKeyWords
    {
        get { return _playingListFilteKeyWords; }
        set
        {
            if (SetProperty<string>(ref _playingListFilteKeyWords, value))
            {
                if (this.Playing.Count == 0)
                {
                    return;
                }

                this.DisplayPlaying.Clear();

                if (!_playingListFilteKeyWords.IsNullOrBlank())
                {
                    this.DisplayPlaying.AddRange(
                        this.Playing.Where(item =>
                            item.MediaName.ContainsIgnoreCase(_playingListFilteKeyWords)
                            || item.Music.Singer.ContainsIgnoreCase(_playingListFilteKeyWords)
                        )
                    );
                }
                else
                {
                    this.DisplayPlaying.AddRange(this.Playing);

                    this.TryRefreshPlayingIndex();
                }
            }
        }
    }
    #endregion

    #region Fields
    private string _playingListFilteKeyWords;
    #endregion

    #region Command
    /// <summary>
    /// 桌面歌词显示/隐藏
    /// </summary>
    public ICommand ToggleDesktopLyricCommand { get; private set; }

    /// <summary>
    /// Singer、Album、Dir分类添加到播放队列
    /// </summary>
    public ICommand AddToPlayingCommand { get; private set; }

    /// <summary>
    /// 播放当前选中的Singer、Album、Dir分类添
    /// </summary>
    public ICommand PlayCurrentCategoryCommand { get; private set; }

    /// <summary>
    /// 播放当前列表
    /// </summary>
    public ICommand PlayCurrentItemsCommand { get; private set; }

    /// <summary>
    /// 播放全部
    /// </summary>
    public ICommand PlayAllCommand { get; private set; }

    /// <summary>
    /// 播放当前
    /// </summary>
    public ICommand PlayFavoriteCommand { get; private set; }

    /// <summary>
    /// 添加到下一首播放
    /// </summary>
    public ICommand AddNextCommand { get; private set; }

    /// <summary>
    /// 下载当前
    /// </summary>
    public ICommand DownLoadCommand { get; private set; }
    #endregion

    public MusicPlayerViewModel(IEventAggregator eventAggregator, IConfigManager config, IAppConfigFileHotKeyManager appConfigFileHotKeyManager, ISettingManager<SettingModel> settingManager)
        : base(eventAggregator, config, appConfigFileHotKeyManager, settingManager)
    {
        this.DistributeMusicViewModel = new DistributeMusicViewModel(eventAggregator, settingManager);

        this.PlayAllCommand = new DelegateCommand(
            () => PlayCurrentItems(),
            () => this.DistributeMusicViewModel.DisplayFavorites.Count > 0 && !this.DistributeMusicViewModel.IsLoading)
            .ObservesProperty(() => this.DistributeMusicViewModel.DisplayFavorites.Count)
            .ObservesProperty(() => this.DistributeMusicViewModel.IsLoading);

        this.DesktopLyric = new DesktopLyricViewModel(config);
    }

    protected override IEnumerable<AppHotKey> MediaHotKeys => base.MediaHotKeys.Concat(
        new AppHotKey[]
        {
            new AppHotKey("播放所有音乐", Key.P, ModifierKeys.Alt),

            new AppHotKey("桌面歌词", Key.C, ModifierKeys.Alt),
            new AppHotKey("歌词封面", Key.Escape, ModifierKeys.None),

            new AppHotKey("搜索", Key.F, ModifierKeys.Control),
            new AppHotKey("允许批量删除", Key.E, ModifierKeys.Control)
        });

    #region CommandExecute
    #region MusicFile
    protected override async void AddMediaFromFileDialog_CommandExecute()
    {
        await this.DistributeMusicViewModel.AddMediaFromFileDialogAsync(_settingManager);
    }

    protected override async void AddMediaFromFolderDialog_CommandExecute()
    {
        await this.DistributeMusicViewModel.AddMediaFromFolderDialogAsync(_settingManager);
    }
    #endregion

    protected override void CleanPlaying_CommandExecute()
    {
        base.CleanPlaying_CommandExecute();

        for (int i = this.Playing.Count - 1; i >= 0; i--)
        {
            this.Playing[i].Dispose();
        }

        this.Playing.Clear();
    }
    #endregion

    #region overrides
    protected override void SubscribeEvents(IEventAggregator eventAggregator)
    {
        base.SubscribeEvents(eventAggregator);

        PlayingMusicViewModel.ToNextMusic += NextMedia_CommandExecute;

        eventAggregator.GetEvent<ToggleDesktopLyricEvent>().Subscribe(() =>
        {
            this.DesktopLyric.ToggleDesktopLyric();
        });

        eventAggregator.GetEvent<ToggleBatchSeleteEvent>().Subscribe(() => this.DistributeMusicViewModel.CanBatchSelect = !this.DistributeMusicViewModel.CanBatchSelect);
    }

    protected override void InitCommandsExtend()
    {
        this.ToggleDesktopLyricCommand = new DelegateCommand(
            () => this.DesktopLyric.ToggleDesktopLyric(),
            () => this.CurrentMedia != null)
            .ObservesProperty(() => this.CurrentMedia);

        this.AddToPlayingCommand = new DelegateCommand<MusicWithClassifyModel>(category =>
        {
            if (category.IsNullOr(_ => _.DisplayByClassifyKeyFavorites.IsNullOrEmpty()))
            {
                CommonUtil.PublishMessage(_eventAggregator, $"传入的分类{category?.ClassifyKey}为空");
                return;
            }

            this.AddItemsToPlaying(new BatchAddAndPlayModel(null, category.DisplayByClassifyKeyFavorites), false);
        });

        this.PlayCurrentCategoryCommand = new DelegateCommand<MusicWithClassifyModel>(category =>
        {
            if (category.IsNullOr(_ => _.DisplayByClassifyKeyFavorites.IsNullOrEmpty()))
            {
                CommonUtil.PublishMessage(_eventAggregator, $"传入的分类{category?.ClassifyKey}为空");
                return;
            }

            this.PlayCurrentItems(new BatchAddAndPlayModel(category.DisplayByClassifyKeyFavorites.First(),
                category.DisplayByClassifyKeyFavorites));
        });

        this.PlayCurrentItemsCommand = new DelegateCommand<BatchAddAndPlayModel>(model =>
        {
            if (model.IsNullOr(_ => _.Collection.IsNullOrEmpty()))
            {
                CommonUtil.PublishMessage(_eventAggregator, "传入的音乐集合为空");
                return;
            }

            this.PlayCurrentItems(model);
        });

        this.PlayFavoriteCommand = new DelegateCommand<FavoriteMusicViewModel>(music =>
        {
            if (music == null)
            {
                CommonUtil.PublishMessage(_eventAggregator, "传入的音乐为空");
                return;
            }

            var item = this.Playing.FirstOrDefault(playing => playing.Equals(music));
            if (item == null)
            {
                item = this.AddOneToPlaying(music);
            }

            this.SetAndPlay(item);
        });

        this.AddNextCommand = new DelegateCommand<FavoriteMusicViewModel>(music =>
        {
            if (music == null)
            {
                CommonUtil.PublishMessage(_eventAggregator, "传入的下一首待播放音乐为空");
                return;
            }

            if (this.Playing.Count == 0)
            {
                this.SetAndPlay(this.AddOneToPlaying(music));
                return;
            }

            if (this.Playing.Any(p => p.Equals(music)))
            {
                CommonUtil.PublishMessage(_eventAggregator, "同名歌曲已存在");
                return;
            }

            if (this.CurrentMedia.Index < this.Playing.Count)
            {
                var temp = new PlayingMusicViewModel(music.Music, this.LyricSetting);

                this.Playing.Insert(this.CurrentMedia.Index, temp);
                this.DisplayPlaying.Insert(this.CurrentMedia.Index, temp);

                this.TryRefreshPlayingIndex();
            }
            else
            {
                this.AddOneToPlaying(music);
            }
        });

        this.DownLoadCommand = new DelegateCommand<MusicModel>(music => { }, _ => false);
    }
    #endregion

    ~MusicPlayerViewModel()
    {
        this.Dispose();
    }

    protected override void DisposeCore()
    {
        this.DistributeMusicViewModel.Dispose();

        PlayingMusicViewModel.ToNextMusic -= NextMedia_CommandExecute;

        base.DisposeCore();
    }
}
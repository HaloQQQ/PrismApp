using MusicPlayerModule.Models;
using Prism.Commands;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Windows.Input;
using IceTea.Atom.Extensions;
using IceTea.Atom.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using MusicPlayerModule.ViewModels.Base;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using MusicPlayerModule.Contracts;
using MusicPlayerModule.MsgEvents.Music;
using PrismAppBasicLib.Models;
using PrismAppBasicLib.Contracts;

namespace MusicPlayerModule.ViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
    internal class MusicPlayerViewModel : MediaPlayerViewModel, IDisposable
    {
        protected override string MediaType => "音乐";
        protected override string[] MediaHotKey_ConfigKey => new string[] { "HotKeys", "App", "Music" };

        protected override string[] MediaPlayOrder_ConfigKey => new string[] { CustomStatics.EnumSettings.Music.ToString(), "MusicPlayOrder" };

        protected override string[] MediaABPoints_ConfigKey => new string[] { CustomStatics.EnumSettings.Music.ToString(), "MusicABPoints" };

        private SettingModel LyricSetting => this._settingManager[CustomStatics.LYRIC];

        public DesktopLyricViewModel DesktopLyric { get; }

        public override bool Running
        {
            get => _running;
            set
            {
                if (SetProperty<bool>(ref _running, value))
                {
                    this._eventAggregator.GetEvent<MusicProgressTimerIsEnableUpdatedEvent>().Publish(value);
                }
            }
        }

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

        private PlayingMusicViewModel AddOneToPlayingList(FavoriteMusicViewModel music)
        {
            var item = new PlayingMusicViewModel(music.Music, this.LyricSetting);
            this.Playing.Add(item);
            this.DisplayPlaying.Add(item);

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
                    this.Playing.Clear();
                    this.DisplayPlaying.Clear();
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

                        this.Playing.Add(temp);
                        this.DisplayPlaying.Add(temp);

                        if (replace && temp.Equals(aggregate.TargetToPlay))
                        {
                            result = temp;
                        }
                    }
                }

                if (result != null)
                {
                    this.RefreshPlayingIndex();

                    if (isFirstNotReplace || aggregate.TargetToPlay != null)
                    {
                        this.SetAndPlay(result);
                    }
                }
            }
        }

        #endregion

        #region General Props
        public DistributeMusicViewModel DistributeMusicViewModel { get; }

        public Collection<PlayingMusicViewModel> Playing { get; } = new();

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
                    }
                }
            }
        }
        #endregion

        #region Fields
        private string _playingListFilteKeyWords;
        #endregion

        #region Command
        public ICommand DesktopLyricPanelCommand { get; private set; }

        /// <summary>
        /// Singer、Album、Dir分类添加到播放队列
        /// </summary>
        public ICommand AddToPlayingCommand { get; private set; }

        /// <summary>
        /// 播放当前分类下的歌曲
        /// </summary>
        public ICommand PlayCurrentCategoryCommand { get; private set; }

        /// <summary>
        /// 添加当前选中Favorite列表到播放列表并播放当前音乐
        /// </summary>
        public ICommand PlayAndAddCurrentFavoritesCommand { get; private set; }

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

            this.DesktopLyric = new DesktopLyricViewModel(config);
        }

        protected override IEnumerable<AppHotKey> MediaHotKeys => base.MediaHotKeys.Concat(
            new AppHotKey[]
            {
                new AppHotKey("播放所有音乐", Key.P, ModifierKeys.Alt),

                new AppHotKey("桌面歌词", Key.C, ModifierKeys.Alt),
                new AppHotKey("歌词封面", Key.Escape, ModifierKeys.None),

                new AppHotKey("搜索", Key.F, ModifierKeys.Control)
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

        protected override void DeletePlaying_CommandExecute(MediaBaseViewModel media)
        {
            base.DeletePlaying_CommandExecute(media);

#pragma warning disable CS8604 // 引用类型参数可能为 null。
            this.Playing.Remove(media as PlayingMusicViewModel);
        }

        protected override void CleanPlaying_CommandExecute()
        {
            base.CleanPlaying_CommandExecute();

            this.Playing.Clear();
        }

        protected override void PlayPlaying_CommandExecute(MediaBaseViewModel currentMedia)
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

        #region overrides
        protected override void SubscribeEvents(IEventAggregator eventAggregator)
        {
            base.SubscribeEvents(eventAggregator);

            PlayingMusicViewModel.ToNextMusic += NextMedia_CommandExecute;

            eventAggregator.GetEvent<ToggleDesktopLyricEvent>().Subscribe(() =>
            {
                this.DesktopLyric.IsDesktopLyricShow = !this.DesktopLyric.IsDesktopLyricShow;
            });
        }

        protected override void InitCommands()
        {
            base.InitCommands();

            this.DesktopLyricPanelCommand = new DelegateCommand(
                () => this.DesktopLyric.IsDesktopLyricShow = !this.DesktopLyric.IsDesktopLyricShow,
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

            this.PlayAndAddCurrentFavoritesCommand = new DelegateCommand<BatchAddAndPlayModel>(model =>
            {
                if (model.IsNullOr(_ => _.Collection.IsNullOrEmpty()))
                {
                    CommonUtil.PublishMessage(_eventAggregator, "传入的音乐集合为空");
                    return;
                }

                this.PlayCurrentItems(model);
            });

            this.PlayAllCommand = new DelegateCommand(() => PlayCurrentItems());

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
                    item = this.AddOneToPlayingList(music);
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
                    this.SetAndPlay(this.AddOneToPlayingList(music));
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

                    this.RefreshPlayingIndex();
                }
                else
                {
                    this.AddOneToPlayingList(music);
                }
            });

            this.DownLoadCommand = new DelegateCommand<MusicModel>(music => { }, _ => false);
        }
        #endregion

        void IDisposable.Dispose()
        {
            foreach (var item in this.Playing)
            {
                item.Dispose();
            }

            this.Playing.Clear();

            this.DistributeMusicViewModel.Dispose();

            base.Dispose();
        }
    }
}
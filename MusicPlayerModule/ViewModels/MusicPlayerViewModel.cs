using MusicPlayerModule.Models;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.Utils;
using Prism.Commands;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using IceTea.Wpf.Core.Utils;
using IceTea.Atom.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using IceTea.Wpf.Atom.Utils;
using IceTea.Wpf.Atom.Contracts.MediaInfo;
using MusicPlayerModule.ViewModels.Base;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using MusicPlayerModule.Contracts;

namespace MusicPlayerModule.ViewModels
{
    internal class MusicPlayerViewModel : MediaPlayerViewModel, IDisposable
    {
        protected override string MediaType => "音乐";
        protected override string[] MediaHotKey_ConfigKey => ["HotKeys", "App", "Music"];

        protected override string[] MediaPlayOrder_ConfigKey => [CustomStatics.EnumSettings.Music.ToString(), "MusicPlayOrder"];

        protected override string[] MediaABPoints_ConfigKey => [CustomStatics.EnumSettings.Music.ToString(), "MusicABPoints"];

        private SettingModel MusicSetting => this._settingManager[CustomStatics.MUSIC];
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

        public int SelectedCount => this.DisplayFavorites.Count(item => item.IsDeleting);

        protected override void AllMediaModelNotPlaying()
        {
            foreach (var item in this.Playing.Where(m => m.IsPlayingMedia))
            {
                item.IsPlayingMedia = false;
            }
        }

        /// <summary>
        /// 收藏队列筛选条件  Name or Singer
        /// </summary>
        public string FavoriteListFilteKeyWords
        {
            get { return _favoriteListFilteKeyWords; }
            set
            {
                if (SetProperty<string>(ref _favoriteListFilteKeyWords, value))
                {
                    if (this.Favorites.Count == 0)
                    {
                        return;
                    }

                    this.DisplayFavorites.Clear();

                    if (!_favoriteListFilteKeyWords.IsNullOrBlank())
                    {
                        this.DisplayFavorites.AddRange(
                            this.Favorites.Where(item =>
                                    item.Music.Name.ContainsIgnoreCase(_favoriteListFilteKeyWords)
                                )
                                .Union(
                                    this.Favorites.Where(item =>
                                        item.Music.Singer.ContainsIgnoreCase(_favoriteListFilteKeyWords)
                                    )
                                )
                        );
                    }
                    else
                    {
                        this.DisplayFavorites.AddRange(this.Favorites);
                    }
                }
            }
        }

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
                                )
                                .Union(
                                    this.Playing.Where(item =>
                                        item.Music.Singer.ContainsIgnoreCase(_playingListFilteKeyWords)
                                    )
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

        #region AddMusicToPlaying

        /// <summary>
        ///  默认播放列表第一个
        /// </summary>
        /// <param name="favoriteMusicViewModel"></param>
        private void BatchAddToPlay(BatchAddAndPlayModel aggregate = null)
        {
            if (aggregate == null)
            {
                aggregate = new BatchAddAndPlayModel(null, this.DisplayFavorites);
                aggregate.TargetToPlay = this.DisplayFavorites.First();
            }

            this.AddAllToPlaying(aggregate);
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
        private void AddAllToPlaying(BatchAddAndPlayModel aggregate, bool replace = true)
        {
            PlayingMusicViewModel result = null;

            if (this.DisplayFavorites.Count > 0)
            {
                bool isFirstNotReplace = !replace && this.Playing.Count == 0;
                if (replace)
                {
                    this.Playing.Clear();
                    this.DisplayPlaying.Clear();
                }

                foreach (var item in aggregate.Collection)
                {
                    if (!this.Playing.Any(m => m.Music == item.Music))
                    {
                        PlayingMusicViewModel temp = new PlayingMusicViewModel(item.Music, this.LyricSetting);
                        if (result == null)
                        {
                            result = temp;
                        }

                        this.Playing.Add(temp);
                        this.DisplayPlaying.Add(temp);

                        if (replace && temp.Music == aggregate.TargetToPlay?.Music)
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
        public DistributeMusicViewModel DistributeMusicViewModel { get; private set; }

        public Collection<FavoriteMusicViewModel> Favorites { get; private set; } = new();
        public ObservableCollection<FavoriteMusicViewModel> DisplayFavorites { get; private set; } = new();

        public Collection<PlayingMusicViewModel> Playing { get; private set; } = new();

        /// <summary>
        /// 是否显示批量删除按钮
        /// </summary>
        public bool CanBatchSelect
        {
            get { return _canBatchSelect; }
            set { SetProperty<bool>(ref _canBatchSelect, value); }
        }

        public bool SelectFavoriteAll
        {
            get { return _selectFavoriteAll; }
            set
            {
                if (SetProperty<bool>(ref _selectFavoriteAll, value))
                {
                    this.DisplayFavorites.ForEach(i => i.IsDeleting = value);
                }
            }
        }

        /// <summary>
        /// 当前列表是按歌曲分组显示
        /// </summary>
        public bool IsInSong
        {
            get => _isInSong;
            set
            {
                if (SetProperty(ref _isInSong, value) && !value)
                {
                    if (!this.FavoriteListFilteKeyWords.IsNullOrBlank())
                    {
                        this.FavoriteListFilteKeyWords = string.Empty;
                    }
                }
            }
        }
        #endregion

        #region Fields
        private bool _isInSong = true;

        private bool _canBatchSelect;

        private bool _selectFavoriteAll;

        private string _playingListFilteKeyWords;
        private string _favoriteListFilteKeyWords;
        #endregion

        #region Command
        public ICommand DesktopLyricPanelCommand { get; private set; }

        /// <summary>
        /// 播放当前分类下的歌曲
        /// </summary>
        public ICommand PlayCurrentCategoryCommand { get; set; }

        /// <summary>
        /// 添加当前选中Favorite列表到播放列表并播放当前音乐
        /// </summary>
        public ICommand PlayAndAddCurrentFavoritesCommand { get; private set; }

        /// <summary>
        /// 播放全部
        /// </summary>
        public ICommand PlayAllCommand { get; set; }

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

        /// <summary>
        /// 批量删除收藏列表
        /// </summary>
        public ICommand BatchDeleteCommand { get; private set; }

        /// <summary>
        /// 删除一条Favorite
        /// </summary>
        public ICommand DeleteFavoriteCommand { get; private set; }

        /// <summary>
        /// 用于启用或禁用使用该命令的控件
        /// </summary>
        public ICommand SelectAllCommand { get; private set; }

        #endregion

        public MusicPlayerViewModel(IEventAggregator eventAggregator, IConfigManager config, IAppConfigFileHotKeyManager appConfigFileHotKeyManager, ISettingManager<SettingModel> settingManager)
            : base(eventAggregator, config, appConfigFileHotKeyManager, settingManager)
        {
            this.DistributeMusicViewModel = new DistributeMusicViewModel(this.Favorites, eventAggregator, settingManager);

            this.DesktopLyric = new DesktopLyricViewModel(config);
        }

        protected override IEnumerable<AppHotKey> MediaHotKeys => base.MediaHotKeys.Concat(
        [
            new AppHotKey("播放所有音乐", Key.P, ModifierKeys.Alt),

            new AppHotKey("桌面歌词", Key.C, ModifierKeys.Alt),
            new AppHotKey("歌词封面", Key.Escape, ModifierKeys.None),

            new AppHotKey("搜索", Key.F, ModifierKeys.Control)
        ]);

        #region CommandExecute
        #region MusicFile

        protected override async void AddMediaFromFileDialog_CommandExecute()
        {
            OpenFileDialog openFileDialog =
                CommonAtomUtils.OpenFileDialog(this.MusicSetting.Value, new MusicMedia());

            if (openFileDialog != null)
            {
                if (await this.TryLoadMusicAsync(openFileDialog.FileNames))
                {
                    var musicDir = openFileDialog.FileName.GetParentPath();

                    this.MusicSetting.Value = musicDir;

                    var lyricDir = await LoadLyricToMusicModel.TryGetLyricDir(musicDir);
                    if (!lyricDir.IsNullOrBlank())
                    {
                        this.LyricSetting.Value = lyricDir;
                    }
                }
                else
                {
                    PublishMessage($"选中文件中不存在新的mp3音乐文件");
                }
            }
        }

        protected override async void AddMediaFromFolderDialog_CommandExecute()
        {
            var selectedPath = CommonCoreUtils.OpenFolderDialog(this.MusicSetting.Value);
            if (!selectedPath.IsNullOrBlank())
            {
                var list = selectedPath.GetFiles(str => str.EndsWithIgnoreCase(".mp3"));

                if (await this.TryLoadMusicAsync(list))
                {
                    this.MusicSetting.Value = selectedPath;

                    var lyricDir = await LoadLyricToMusicModel.TryGetLyricDir(selectedPath);
                    if (!lyricDir.IsNullOrBlank())
                    {
                        this.LyricSetting.Value = lyricDir;
                    }
                }
                else
                {
                    PublishMessage($"【{selectedPath}】中找不到新的mp3音乐文件");
                }
            }
        }

        private async Task<bool> TryLoadMusicAsync(IEnumerable<string> filePaths)
        {
            if (filePaths.IsNullOrEmpty())
            {
                return false;
            }

            List<string> list = TryGetNewFiles(filePaths).ToList();

            if (list.Count == 0)
            {
                return false;
            }

            this.IsLoading = true;

            await this.MultiThreadBatchLoadMusic(list).ConfigureAwait(false);

            this.IsLoading = false;

            return true;

            IEnumerable<string> TryGetNewFiles(IEnumerable<string> filePaths)
            {
                foreach (var filePath in filePaths)
                {
                    if (!this.Favorites.Any(item => item.Music.FilePath == filePath || item.Music.FilePath.GetFileNameWithoutExtension() == filePath.GetFileNameWithoutExtension()))
                    {
                        yield return filePath;
                    }
                }
            }
        }

        private void SingleThreadLoadMusic(IList<string> filePathList)
        {
            AppUtils.AssertNotEmpty(filePathList, nameof(filePathList));

            foreach (var file in filePathList)
            {
                var children = new FavoriteMusicViewModel(new MusicModel(file));
                children.AddTo(DisplayFavorites);
                children.AddTo(Favorites);

                DistributeMusicViewModel.AddNewMusic(children);
            }

            this.RefreshFavoriteIndex();
        }

        private async Task MultiThreadBatchLoadMusic(IList<string> filePathList)
        {
            AppUtils.AssertNotEmpty(filePathList, nameof(filePathList));

            var taskList = new List<Task>();

            var step = 40;
            for (int i = 0; i < filePathList.Count / step + 1; i++)
            {
                var index = i;
                taskList.Add(
                    Task.Run(() =>
                    {
                        foreach (var item in filePathList
                                                 .Skip(index * step)
                                                 .Take(step)
                                     )
                        {
                            CommonAtomUtils.Invoke(() =>
                            {
                                var children = new FavoriteMusicViewModel(new MusicModel(item));
                                children.AddTo(DisplayFavorites);
                                children.AddTo(Favorites);

                                DistributeMusicViewModel.AddNewMusic(children);
                            });
                        }
                    }
                    )
                );
            }

            await Task.WhenAll(taskList);

            this.RefreshFavoriteIndex();
        }

        private void RefreshFavoriteIndex()
        {
            for (int i = 0; i < Favorites.Count; i++)
            {
                Favorites[i].Index = i + 1;
            }
        }
        #endregion

        protected override void DeletePlaying_CommandExecute(MediaBaseViewModel media)
        {
            base.DeletePlaying_CommandExecute(media);

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

            FavoriteMusicViewModel.DeleteStatusChanged += newValue =>
            {
                this.RaisePropertyChanged(nameof(SelectedCount));

                if (newValue != SelectFavoriteAll)
                {
                    if (newValue && DisplayFavorites.Any(m => !m.IsDeleting))
                    {
                        return;
                    }

                    _selectFavoriteAll = newValue;

                    RaisePropertyChanged(nameof(SelectFavoriteAll));
                }
            };

            PlayingMusicViewModel.ToNextMusic += NextMedia_CommandExecute;

            eventAggregator.GetEvent<BatchAddToPlayingEvent>().Subscribe(coll => this.AddAllToPlaying(new BatchAddAndPlayModel(null, coll), false));
        }

        protected override void InitCommands()
        {
            base.InitCommands();

            this.DesktopLyricPanelCommand = new DelegateCommand(
                () => _eventAggregator.GetEvent<ToggleDesktopLyricEvent>().Publish(),
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

            this.PlayCurrentCategoryCommand = new DelegateCommand<MusicWithClassifyModel>(category =>
            {
                if (category == null || category.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
                {
                    PublishMessage($"传入的分类{category?.ClassifyKey}为空");
                    return;
                }

                this.BatchAddToPlay(new BatchAddAndPlayModel(category.DisplayByClassifyKeyFavorites.First(),
                    category.DisplayByClassifyKeyFavorites));
            }, _ => this.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.PlayAndAddCurrentFavoritesCommand = new DelegateCommand<BatchAddAndPlayModel>(model =>
            {
                if (model == null || model.Collection.IsNullOrEmpty())
                {
                    PublishMessage($"传入的音乐集合为空");
                    return;
                }

                this.BatchAddToPlay(model);
            }, _ => this.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.PlayAllCommand = new DelegateCommand(
                () => this.BatchAddToPlay(),
                () => this.DisplayFavorites.Count > 0
                ).ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.PlayFavoriteCommand = new DelegateCommand<FavoriteMusicViewModel>(music =>
            {
                if (music == null)
                {
                    PublishMessage("传入的音乐为空");
                    return;
                }

                var item = this.Playing.FirstOrDefault(playing => playing.Music == music.Music);
                if (item == null)
                {
                    item = this.AddOneToPlayingList(music);
                }

                this.SetAndPlay(item);
            }, _ => this.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.AddNextCommand = new DelegateCommand<FavoriteMusicViewModel>(music =>
            {
                if (music == null)
                {
                    PublishMessage("传入的下一首待播放音乐为空");
                    return;
                }

                if (this.Playing.Count == 0)
                {
                    this.SetAndPlay(this.AddOneToPlayingList(music));
                    return;
                }

                var temp = new PlayingMusicViewModel(music.Music, this.LyricSetting);
                if (this.CurrentMedia.Index < this.Playing.Count)
                {
                    this.Playing.Insert(this.CurrentMedia.Index, temp);
                    this.DisplayPlaying.Insert(this.CurrentMedia.Index, temp);

                    this.RefreshPlayingIndex();
                }
                else
                {
                    this.AddOneToPlayingList(music);
                }
            }, _ => this.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.DownLoadCommand = new DelegateCommand<MusicModel>(music => { });

            this.BatchDeleteCommand = new DelegateCommand(() =>
            {
                if (IsInSong)
                {
                    var items = DisplayFavorites.Where(item => item.IsDeleting).ToArray();

                    for (int i = items.Length - 1; i >= 0; i--)
                    {
                        items[i].RemoveFromAll();
                    }

                    this.RefreshFavoriteIndex();
                }
                else if (this.DistributeMusicViewModel.IsInAlbum)
                {
                    var items = DistributeMusicViewModel.DisplayMusicAlbumFavorites
                                                    .FirstOrDefault(i => i.IsSelected)?.DisplayByClassifyKeyFavorites
                                                     .Where(m => m.IsDeleting).ToArray();
                    if (items != null)
                    {
                        for (int i = items.Length - 1; i >= 0; i--)
                        {
                            items[i].RemoveFromAll();
                        }
                    }
                }
                else if (DistributeMusicViewModel.IsInSinger)
                {
                    var items = DistributeMusicViewModel.DisplayMusicSingerFavorites
                                                    .FirstOrDefault(i => i.IsSelected)?.DisplayByClassifyKeyFavorites
                                                     .Where(m => m.IsDeleting).ToArray();

                    if (items != null)
                    {
                        for (int i = items.Length - 1; i >= 0; i--)
                        {
                            items[i].RemoveFromAll();
                        }
                    }
                }
                else if (DistributeMusicViewModel.IsInDir)
                {
                    var items = DistributeMusicViewModel.MusicDirFavorites
                                                    .FirstOrDefault(i => i.IsSelected)?.DisplayByClassifyKeyFavorites
                                                     .Where(m => m.IsDeleting).ToArray();

                    if (items != null)
                    {
                        for (int i = items.Length - 1; i >= 0; i--)
                        {
                            items[i].RemoveFromAll();
                        }
                    }
                }

                this.DistributeMusicViewModel.OnlyRefreshClassifySelectAllStatus();

                this.SelectFavoriteAll = this.DisplayFavorites.Count > 0 && !this.DisplayFavorites.Any(m => !m.IsDeleting);

                if (this.DisplayFavorites.Count == 0)
                {
                    CanBatchSelect = false;
                }

                this.RaisePropertyChanged(nameof(SelectedCount));
            }, () => this.DisplayFavorites.Any(item => item.IsDeleting))
                .ObservesProperty(() => this.SelectedCount);

            this.DeleteFavoriteCommand = new DelegateCommand<FavoriteMusicViewModel>(music =>
            {
                if (music == null)
                {
                    PublishMessage("传入的列表中待删除音乐为空");
                    return;
                }

                music.RemoveFromAll();

                this.RefreshFavoriteIndex();

                this.DistributeMusicViewModel.OnlyRefreshClassifySelectAllStatus();

                this.SelectFavoriteAll = this.DisplayFavorites.Count > 0 && !this.DisplayFavorites.Any(m => !m.IsDeleting);

                if (this.DisplayFavorites.Count == 0)
                {
                    CanBatchSelect = false;
                }

                if (music.IsDeleting)
                {
                    this.RaisePropertyChanged(nameof(SelectedCount));
                }
            }, _ => this.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.SelectAllCommand = new DelegateCommand(() => { }, () => this.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DisplayFavorites.Count);
        }
        #endregion

        public void Dispose()
        {
            foreach (var item in this.Favorites)
            {
                item.Dispose();
            }

            this.DisplayFavorites.Clear();
            this.DisplayFavorites = null;

            foreach (var item in this.Favorites)
            {
                item.Dispose();
            }

            this.Favorites.Clear();
            this.Favorites = null;

            foreach (var item in this.Playing)
            {
                item.Dispose();
            }

            this.Playing.Clear();
            this.Playing = null;

            this.DistributeMusicViewModel.Dispose();

            this.PlayAllCommand = null;
            this.PlayFavoriteCommand = null;
            this.AddNextCommand = null;
            this.DownLoadCommand = null;
            this.BatchDeleteCommand = null;
            this.SelectAllCommand = null;

            base.Dispose();
        }
    }
}
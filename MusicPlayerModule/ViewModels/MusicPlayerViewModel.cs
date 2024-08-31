using MusicPlayerModule.Common;
using MusicPlayerModule.Models;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.Utils;
using Prism.Commands;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Win32;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using IceTea.Wpf.Core.Utils;
using IceTea.Atom.Contracts;
using PrismAppBasicLib.MsgEvents;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using IceTea.Wpf.Atom.Utils;
using IceTea.Wpf.Atom.Contracts.MediaInfo;
using MusicPlayerModule.ViewModels.Base;
using IceTea.Wpf.Atom.Utils.HotKey.App;

namespace MusicPlayerModule.ViewModels
{
    internal class MusicPlayerViewModel : MediaPlayerViewModel, IDisposable
    {
        protected override string MediaType => "音乐";
        protected override string[] MediaSettingNode => ["HotKeys", "App", "Music"];

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

        public bool IsLoading
        {
            get => this._isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }

        public int SelectedCount => this.DisplayFavorites.Count(item => item.IsDeleting);

        public override MediaBaseViewModel CurrentMedia
        {
            get => _currentMedia;
            set
            {
                if (SetProperty<MediaBaseViewModel>(ref _currentMedia, value) && value != null)
                {
                    PlayingMusicViewModel musicViewModel = value as PlayingMusicViewModel;

                    LoadLyricToMusicModel.LoadLyricAsync(this._settingManager[CustomStatics.EnumSettings.Lyric.ToString()].Value, musicViewModel.Music);

                    foreach (var item in this.Playing.Where(m => m.IsPlayingMedia))
                    {
                        item.IsPlayingMedia = false;
                    }

                    _currentMedia.IsPlayingMedia = true;
                }
            }
        }

        /// <summary>
        /// 收藏队列筛选条件
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

                    if (!string.IsNullOrEmpty(_favoriteListFilteKeyWords))
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

                    if (!string.IsNullOrEmpty(_playingListFilteKeyWords))
                    {
                        this.DisplayPlaying.AddRange(
                            this.Playing.Where(item =>
                                    item.Music.Name.ContainsIgnoreCase(_playingListFilteKeyWords)
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

        private void PublishMessage(string msg, int seconds = 3)
        {
            _eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage(msg, seconds));
        }

        private void SubscribeEvents(IEventAggregator eventAggregator)
        {
            // 手动通知,比封装属性然后触发PropertyChanged事件快一些
            FavoriteMusicViewModel.DeleteStatusChanged += isDeleteing => this.CheckSelectAllStatus();

            FavoriteMusicViewModel.DeleteStatusChanged += isDeleteing => this.NotifyBatchDeleteCanExecute();

            FavoriteMusicViewModel.DeleteStatusChanged +=
                isDeleting => this.RaisePropertyChanged(nameof(SelectedCount));

            PlayingMusicViewModel.ToNextMusic += NextMedia;

            MusicWithClassifyModel.SelectStatusChanged +=
                selected => this.DistributeMusicViewModel.CheckClassifySelectAllStatus();

            eventAggregator.GetEvent<ToggeleCurrentMusicEvent>().Subscribe(() =>
            {
                if (this.CurrentMedia != null)
                {
                    if (this.Running = !this.Running)
                    {
                        this._eventAggregator.GetEvent<ContinueCurrentMusicEvent>().Publish();
                    }
                    else
                    {
                        this._eventAggregator.GetEvent<PauseCurrentMusicEvent>().Publish();
                    }
                }
            });

            eventAggregator.GetEvent<PrevMusicEvent>().Subscribe(() => this.PrevMedia(this.CurrentMedia));
            eventAggregator.GetEvent<NextMusicEvent>().Subscribe(() => this.NextMedia(this.CurrentMedia));

            eventAggregator.GetEvent<AheadEvent>().Subscribe(() =>
            {
                if (this.CurrentMedia != null)
                {
                    this.CurrentMedia.FastForward();
                }
            });
            eventAggregator.GetEvent<DelayEvent>().Subscribe(() =>
            {
                if (this.CurrentMedia != null)
                {
                    this.CurrentMedia.Rewind();
                }
            });

            eventAggregator.GetEvent<BatchAddToPlayingEvent>().Subscribe(coll => this.AddAllToPlaying(new BatchAddAndPlayModel(null, coll), false));
        }

        private void TryClearFavoriteListFilteKeyWords()
        {
            if (!this.FavoriteListFilteKeyWords.IsNullOrEmpty())
            {
                this.FavoriteListFilteKeyWords = null;
            }
        }

        /// <summary>
        /// Favirotes列表移除或者勾选删除时检查全选状态
        /// </summary>
        private void CheckSelectAllStatus()
        {
            this.SelectFavoriteAll =
                this.DisplayFavorites.Count > 0 && this.SelectedCount == this.DisplayFavorites.Count;
            this.DistributeMusicViewModel.CheckClassifySelectAllStatus();
        }

        private void RefreshFavoriteIndex()
        {
            if (this.Favorites.Count == 0)
            {
                return;
            }

            var t = 1;
            this.Favorites.ForEach(m => m.Index = t++);
        }

        private void RefreshPlayingIndex()
        {
            var index = 1;
            foreach (var item in this.Playing)
            {
                item.Index = index++;
            }
        }

        #region MusicFile

        private async void AddMusicFromFileDialog()
        {
            OpenFileDialog openFileDialog =
                CommonAtomUtils.OpenFileDialog(this._settingManager[CustomStatics.EnumSettings.Music.ToString()].Value, new MusicMedia());

            if (openFileDialog != null)
            {
                await this.LoadMusicAsync(openFileDialog.FileNames);

                var musicDir = openFileDialog.FileName.GetParentPath();
                this.TryRefreshLastMusicDir(musicDir);

                this._settingManager[CustomStatics.EnumSettings.Lyric.ToString()].Value = LoadLyricToMusicModel.TryGetLyricDir(musicDir);
            }
        }

        private async void AddMusicFromFolderDialog()
        {
            var selectedPath = CommonCoreUtils.OpenFolderDialog(this._settingManager[CustomStatics.EnumSettings.Music.ToString()].Value);
            if (!selectedPath.IsNullOrEmpty())
            {
                this.TryRefreshLastMusicDir(selectedPath);

                var list = selectedPath.GetFiles(str => str.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase));

                await this.LoadMusicAsync(list);

                this._settingManager[CustomStatics.EnumSettings.Lyric.ToString()].Value = LoadLyricToMusicModel.TryGetLyricDir(selectedPath);
            }
        }

        private void TryRefreshLastMusicDir(string dir)
        {
            AppUtils.AssertDataValidation(dir.IsDirectoryPath(), $"{dir}必须为存在的目录");

            this._settingManager[CustomStatics.EnumSettings.Lyric.ToString()].Value = dir;
        }

        private IEnumerable<string> GetNewFiles(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                if (!this.Favorites.Any(item => item.Music.FilePath.GetFileNameWithoutExtension() == filePath.GetFileNameWithoutExtension() || item.Music.FilePath == filePath))
                {
                    yield return filePath;
                }
            }
        }

        private async Task LoadMusicAsync(IEnumerable<string> filePaths)
        {
            if (filePaths.IsNullOrEmpty())
            {
                return;
            }

            List<string> list = this.GetNewFiles(filePaths).ToList();

            if (list.Count == 0)
            {
                return;
            }

            this.IsLoading = true;

            await this.MultiThreadBatchLoadMusic(list).ConfigureAwait(false);

            this.IsLoading = false;
        }

        private void SingleThreadLoadMusic(List<string> filePathList)
        {
            AppUtils.AssertNotEmpty(filePathList, nameof(filePathList));

            foreach (var file in filePathList)
            {
                this.DisplayFavorites.Add(new FavoriteMusicViewModel(new MusicModel(file)));
            }

            this.Favorites.AddRange(this.DisplayFavorites);
        }

        private async Task MultiThreadBatchLoadMusic(List<string> filePathList)
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
                                    var musicModel = new FavoriteMusicViewModel(new MusicModel(item));
                                    this.DisplayFavorites.Add(musicModel);
                                    this.DistributeMusicViewModel.AddNewMusic(musicModel);

                                    this.Favorites.Add(musicModel);
                                });
                            }
                        }
                    )
                );
            }

            await Task.WhenAll(taskList);

            this.RefreshFavoriteIndex();
        }

        #endregion

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
            var item = new PlayingMusicViewModel(music.Music, _settingManager[CustomStatics.EnumSettings.Music.ToString()]);
            this.Playing.Add(item);
            this.DisplayPlaying.Add(item);

            item.Index = this.Playing.Count;

            return item;
        }

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
                    if (this.Playing.FirstOrDefault(m => m.Music == item.Music) == null)
                    {
                        PlayingMusicViewModel temp = new PlayingMusicViewModel(item.Music, _settingManager[CustomStatics.EnumSettings.Music.ToString()]);
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

        #region NotifyCanExecute

        /// <summary>
        /// 检查是否允许批量删除
        /// </summary>
        private void NotifyBatchDeleteCanExecute()
        {
            ((DelegateCommandBase)this.BatchDeleteCommand).RaiseCanExecuteChanged();
        }

        #endregion

        #region General Props

        public DistributeMusicViewModel DistributeMusicViewModel { get; private set; }

        public Collection<FavoriteMusicViewModel> Favorites { get; private set; } = new();
        public ObservableCollection<FavoriteMusicViewModel> DisplayFavorites { get; private set; } = new();
        public Collection<PlayingMusicViewModel> Playing { get; private set; } = new();

        public bool IsBatchSelect
        {
            get { return _isBatchSelect; }
            set { SetProperty<bool>(ref _isBatchSelect, value); }
        }

        public bool SelectFavoriteAll
        {
            get { return _selectFavoriteAll; }
            set { SetProperty<bool>(ref _selectFavoriteAll, value); }
        }

        #endregion

        #region Fields
        private bool _isLoading;
        private bool _isBatchSelect;

        private bool _selectFavoriteAll;

        private string _playingListFilteKeyWords;
        private string _favoriteListFilteKeyWords;
        #endregion

        #region Command
        public ICommand DesktopLyricPanelCommand { get; private set; }

        public ICommand OpenInExploreCommand { get; private set; }

        /// <summary>
        /// 从本地添加音乐到列表
        /// </summary>
        public ICommand AddFilesCommand { get; private set; }

        /// <summary>
        /// 从文件夹添加音乐到列表
        /// </summary>
        public ICommand AddFolderCommand { get; private set; }

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
        /// 删除一条Playing
        /// </summary>
        public ICommand DeletePlayingCommand { get; private set; }

        /// <summary>
        /// 收藏列表全选或全不选
        /// </summary>
        public ICommand SelectAllCommand { get; private set; }

        #endregion

        public MusicPlayerViewModel(IEventAggregator eventAggregator, IConfigManager config, IAppConfigFileHotKeyManager appConfigFileHotKeyManager, ISettingManager<SettingModel> settingManager)
            : base(eventAggregator, config, appConfigFileHotKeyManager, settingManager)
        {
            this.SubscribeEvents(eventAggregator);

            this.DistributeMusicViewModel = new DistributeMusicViewModel(this.Favorites, eventAggregator, settingManager);
            this.DistributeMusicViewModel.ClearFavoriteListFilteKeyWords += TryClearFavoriteListFilteKeyWords;

            this.DesktopLyric = new DesktopLyricViewModel(config);
        }

        protected override IEnumerable<AppHotKey> MediaHotKeys => base.MediaHotKeys.Concat(
        [
            new AppHotKey("播放所有音乐", Key.P, ModifierKeys.Alt),

            new AppHotKey("桌面歌词", Key.C, ModifierKeys.Alt),
            new AppHotKey("歌词封面", Key.Escape, ModifierKeys.None),

            new AppHotKey("搜索", Key.F, ModifierKeys.Control)
        ]);

        #region overrides
        protected override void InitCommands()
        {
            base.InitCommands();

            this.DesktopLyricPanelCommand = new DelegateCommand(() => this._eventAggregator.GetEvent<ToggleLyricDesktopEvent>().Publish(),
                () => this.CurrentMedia != null)
                .ObservesProperty(() => this.CurrentMedia);

            this.OpenInExploreCommand = new DelegateCommand<string>(musicDir =>
            {
                if (musicDir.IsNullOrEmpty())
                {
                    PublishMessage($"未传入目录");
                    return;
                }

                Process.Start("explorer", musicDir);
            });

            this.AddFilesCommand = new DelegateCommand(AddMusicFromFileDialog);

            this.AddFolderCommand = new DelegateCommand(AddMusicFromFolderDialog);

            this.PlayCurrentCategoryCommand = new DelegateCommand<MusicWithClassifyModel>(category =>
            {
                if (category == null || category.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
                {
                    PublishMessage($"传入的分类{category?.ClassifyKey}为空");
                    return;
                }

                this.BatchAddToPlay(new BatchAddAndPlayModel(category.DisplayByClassifyKeyFavorites.First(),
                    category.DisplayByClassifyKeyFavorites));
            }
                , category => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.PlayAndAddCurrentFavoritesCommand = new DelegateCommand<BatchAddAndPlayModel>(model =>
            {
                if (model == null || model.Collection.IsNullOrEmpty())
                {
                    PublishMessage($"传入的音乐集合为空");
                    return;
                }
                this.BatchAddToPlay(model);
            }, _ => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.PlayAllCommand =
                new DelegateCommand(() => this.BatchAddToPlay(), () => this.DisplayFavorites.Count > 0)
                    .ObservesProperty<int>(() => this.DisplayFavorites.Count);

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
            }, _ => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);

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

                var temp = new PlayingMusicViewModel(music.Music, _settingManager[CustomStatics.EnumSettings.Music.ToString()]);
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
            }, _ => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.DownLoadCommand = new DelegateCommand<MusicModel>(music => { });

            this.BatchDeleteCommand = new DelegateCommand(() =>
            {
                this.DisplayFavorites.RemoveAll(items => items.IsDeleting);
                this.Favorites.RemoveAll(items => items.IsDeleting);

                this.RefreshFavoriteIndex();

                this.DistributeMusicViewModel.DeleteAllMarkedMusic();

                this.CheckSelectAllStatus();
                this.IsBatchSelect = this.DisplayFavorites.Count > 0;
                this.NotifyBatchDeleteCanExecute();
                this.RaisePropertyChanged(nameof(SelectedCount));
            }, () => this.DisplayFavorites.Any(item => item.IsDeleting));

            this.DeleteFavoriteCommand = new DelegateCommand<FavoriteMusicViewModel>(music =>
            {
                if (music == null)
                {
                    PublishMessage("传入的列表中待删除音乐为空");
                    return;
                }

                this.DisplayFavorites.Remove(music);
                this.Favorites.Remove(music);

                this.RefreshFavoriteIndex();

                this.DistributeMusicViewModel.DeleteSingleMusic(music);

                this.CheckSelectAllStatus();
                if (this.DisplayFavorites.Count == 0)
                {
                    this.IsBatchSelect = false;
                }

                this.NotifyBatchDeleteCanExecute();
                this.RaisePropertyChanged(nameof(SelectedCount));
            }, _ => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.DeletePlayingCommand = new DelegateCommand<PlayingMusicViewModel>(music =>
            {
                if (music == null)
                {
                    PublishMessage("传入的播放队列中待删除音乐为空");
                    return;
                }

                if (music == this.CurrentMedia)
                {
                    if (this.DisplayPlaying.Count > 1)
                    {
                        this.NextMedia(music);
                    }
                    else
                    {
                        this.CurrentMedia = null;
                        this.Running = false;
                    }
                }

                this.DisplayPlaying.Remove(music);
                this.Playing.Remove(music);

                this.RefreshPlayingIndex();
            }, _ => this.DisplayPlaying.Count > 0).ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.SelectAllCommand = new DelegateCommand<bool?>(isChecked =>
            {
                if (isChecked == null)
                {
                    PublishMessage("传入的全选状态为空");
                    return;
                }

                foreach (var item in this.DisplayFavorites)
                {
                    item.IsDeleting = (bool)isChecked;
                }
            }, isChecked => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);
        }

        protected override void LoadConfig(IConfigManager configManager)
        {
            var musicPlayOrder = configManager.ReadConfigNode(CustomStatics.MusicPlayOrder_ConfigKey);

            if (!musicPlayOrder.IsNullOrBlank())
            {
                this.CurrentPlayOrder =
                    CustomStatics.MediaPlayOrderList.FirstOrDefault(item => item.Description == musicPlayOrder) ??
                    CustomStatics.MediaPlayOrderList.First();
            }

            //CustomStatics.LastMusicDir = configManager.ReadConfigNode(CustomStatics.LastMusicDir_ConfigKey);

            //CustomStatics.LyricDir = configManager.ReadConfigNode(CustomStatics.LyricDir_ConfigKey);

            configManager.SetConfig += config =>
            {
                config.WriteConfigNode(this.CurrentPlayOrder.Description, CustomStatics.MusicPlayOrder_ConfigKey);

                //config.WriteConfigNode(CustomStatics.LastMusicDir, CustomStatics.LastMusicDir_ConfigKey);

                //config.WriteConfigNode(CustomStatics.LyricDir, CustomStatics.LyricDir_ConfigKey);
            };
        }

        protected override void RaiseResetMediaEvent(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<ResetMusicPlayerEvent>().Publish();
        }

        protected override void RaiseResetPlayerAndPlayMediaEvent(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<ResetPlayerAndPlayMusicEvent>().Publish();
        }

        protected override void CleanPlaying()
        {
            base.CleanPlaying();

            this.Playing.Clear();
        }

        protected override void PlayPlaying(MediaBaseViewModel currentMedia)
        {
            if (currentMedia == this.CurrentMedia)
            {
                if (this.Running = !this.Running)
                {
                    this._eventAggregator.GetEvent<ContinueCurrentMusicEvent>().Publish();
                }
                else
                {
                    this._eventAggregator.GetEvent<PauseCurrentMusicEvent>().Publish();
                }
            }
            else
            {
                this.SetAndPlay(currentMedia);
            }
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

            this.AddFilesCommand = null;
            this.PlayPlayingCommand = null;

            this.PlayAllCommand = null;
            this.PlayFavoriteCommand = null;
            this.AddNextCommand = null;
            this.DownLoadCommand = null;
            this.BatchDeleteCommand = null;
            this.SelectAllCommand = null;

            this.AddFilesCommand = null;
            this.AddFolderCommand = null;

            this.OpenInExploreCommand = null;

            base.Dispose();
        }
    }
}
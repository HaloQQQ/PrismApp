using IceTea.Core.Extensions;
using MusicPlayerModule.Common;
using MusicPlayerModule.Models;
using MusicPlayerModule.Models.Common;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using IceTea.Wpf.Core.Helper;
using IceTea.Wpf.Core.Helper.MediaInfo;

namespace MusicPlayerModule.ViewModels
{
    internal class MusicPlayerViewModel : BindableBase, IDisposable
    {
        public bool Running
        {
            get { return _running; }
            set
            {
                if (SetProperty<bool>(ref _running, value))
                {
                    this._eventAggregator.GetEvent<MusicProgreeTimerIsEnableUpdatedEvent>().Publish(value);
                }
            }
        }

        public bool IsLoading
        {
            get => this._isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }

        public int SelectedCount => this.DisplayFavorites.Count(item => item.IsDeleting);

        public PlayingMusicViewModel CurrentMusic
        {
            get { return _currentMusic; }
            set
            {
                if (SetProperty<PlayingMusicViewModel>(ref _currentMusic, value))
                {
                    if (this._currentMusic != null && !this._currentMusic.LoadedLyric)
                    {
                        LoadLyricToMusicModel.LoadAsync(AppStatics.LastMusicDir, this._currentMusic?.Music);
                    }

                    foreach (var item in this.Playing)
                    {
                        item.IsPlayingMusic = false;
                    }

                    if (this._currentMusic != null)
                    {
                        _currentMusic.IsPlayingMusic = true;
                    }
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
                                    item.Music.Name.Contains(_favoriteListFilteKeyWords)
                                )
                                .Union(
                                    this.Favorites.Where(item =>
                                        item.Music.Singer.Contains(_favoriteListFilteKeyWords)
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
                                    item.Music.Name.Contains(_playingListFilteKeyWords)
                                )
                                .Union(
                                    this.Playing.Where(item =>
                                        item.Music.Singer.Contains(_playingListFilteKeyWords)
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

        private void InitCommands()
        {
            this.OpenInExploreCommand = new DelegateCommand<string>(musicDir =>
            {
                if (musicDir == null)
                {
                    return;
                }

                Process.Start("explorer", musicDir);
            });

            this.PointACommand =
                new DelegateCommand(() => { this.CurrentMusic.SetPointA(this.CurrentMusic.CurrentMills); },
                    () => this.CurrentMusic != null).ObservesProperty(() => this.CurrentMusic);

            this.PointBCommand =
                new DelegateCommand(() => { this.CurrentMusic.SetPointB(this.CurrentMusic.CurrentMills); },
                    () => this.CurrentMusic != null).ObservesProperty(() => this.CurrentMusic);

            this.ResetPointABCommand =
                new DelegateCommand(() => { this.CurrentMusic?.ResetABPoint(); }, () => this.CurrentMusic != null)
                    .ObservesProperty(() => this.CurrentMusic);

            this.StopPlayMusicCommand = new DelegateCommand(() =>
                {
                    this.CurrentMusic = null;
                    this.Running = false;
                }, () => !this.Running && this.CurrentMusic != null).ObservesProperty(() => this.CurrentMusic)
                .ObservesProperty(() => this.Running);

            this.AddFilesCommand = new DelegateCommand(AddMusicFromFileDialog);

            this.AddFolderCommand = new DelegateCommand(AddMusicFromFolderDialog);

            this.DelayCommand =
                new DelegateCommand(() => { }, () => this.CurrentMusic != null).ObservesProperty<PlayingMusicViewModel>(
                    () => this.CurrentMusic);

            this.PrevCommand = new DelegateCommand<PlayingMusicViewModel>(
                    currentMusic => this.PrevMusic(currentMusic),
                    currentMusic => this.CurrentMusic != null && this.DisplayPlaying.Count > 0
                )
                .ObservesProperty<PlayingMusicViewModel>(() => this.CurrentMusic)
                .ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.NextCommand = new DelegateCommand<PlayingMusicViewModel>(
                    currentMusic => this.NextMusic(currentMusic),
                    currentMusic => this.CurrentMusic != null && this.DisplayPlaying.Count > 0
                ).ObservesProperty<PlayingMusicViewModel>(() => this.CurrentMusic)
                .ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.AheadCommand =
                new DelegateCommand(() => { }, () => this.CurrentMusic != null).ObservesProperty<PlayingMusicViewModel>(
                    () => this.CurrentMusic);

            this.PlayCurrentCategoryCommand = new DelegateCommand<MusicWithClassifyModel>(category =>
                {
                    if (category != null && category.DisplayByClassifyKeyFavorites.Count > 0)
                    {
                        this.BatchAddToPlay(new BatchAddAndPlayModel(category.DisplayByClassifyKeyFavorites.First(),
                            category.DisplayByClassifyKeyFavorites));
                    }
                }
                , category => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.PlayAndAddCurrentFavoritesCommand = new DelegateCommand<BatchAddAndPlayModel>(model =>
            {
                if (model != null)
                {
                    this.BatchAddToPlay(model);
                }
            }, _ => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.PlayAllCommand =
                new DelegateCommand(() => this.BatchAddToPlay(), () => this.DisplayFavorites.Count > 0)
                    .ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.PlayFavoriteCommand = new DelegateCommand<FavoriteMusicViewModel>(music =>
            {
                if (music == null)
                {
                    return;
                }

                var item = this.Playing.FirstOrDefault(playing => playing.Music == music.Music);
                if (item == null)
                {
                    item = this.AddOneToPlayingList(music);
                }

                this.SetAndPlay(item);
            }, _ => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.PlayPlayingCommand = new DelegateCommand<PlayingMusicViewModel>(music =>
            {
                if (music == null)
                {
                    return;
                }

                if (music == this.CurrentMusic)
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
                    this.SetAndPlay(music);
                }
            }, _ => this.CurrentMusic != null).ObservesProperty(() => this.CurrentMusic);

            this.AddNextCommand = new DelegateCommand<FavoriteMusicViewModel>(music =>
            {
                if (music == null)
                {
                    return;
                }

                if (this.Playing.Count == 0)
                {
                    this.SetAndPlay(this.AddOneToPlayingList(music));
                    return;
                }

                var temp = new PlayingMusicViewModel(music.Music);
                if (this.CurrentMusic.Index < this.Playing.Count)
                {
                    this.Playing.Insert(this.CurrentMusic.Index, temp);
                    this.DisplayPlaying.Insert(this.CurrentMusic.Index, temp);

                    this.RefreshPlayingIndex();
                }
                else
                {
                    this.AddOneToPlayingList(music);
                }
            }, _ => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.DownLoadCommand = new DelegateCommand<MusicModel>(music => { });

            this.CleanPlayingCommand = new DelegateCommand(() =>
                {
                    this.Playing.Clear();
                    this.DisplayPlaying.Clear();
                    this.CurrentMusic = null;
                    this.Running = false;
                }, () => !this.Running && this.DisplayPlaying.Count > 0).ObservesProperty(() => this.Running)
                .ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.BatchDeleteCommand = new DelegateCommand(() =>
            {
                for (int i = this.DisplayFavorites.Count - 1; i >= 0; i--)
                {
                    if (this.DisplayFavorites[i].IsDeleting)
                    {
                        this.DisplayFavorites.RemoveAt(i);
                        this.Favorites.RemoveAt(i);
                    }
                }

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
                    return;
                }

                this.DisplayFavorites.Remove(music);
                this.Favorites.Remove(music);

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
                    return;
                }

                if (music == this.CurrentMusic)
                {
                    if (this.DisplayPlaying.Count > 1)
                    {
                        this.NextMusic(music);
                    }
                    else
                    {
                        this.CurrentMusic = null;
                        this.Running = false;
                    }
                }

                this.DisplayPlaying.Remove(music);
                this.Playing.Remove(music);

                this.RefreshPlayingIndex();
            }, _ => this.DisplayPlaying.Count > 0).ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.SelectAllCommand = new DelegateCommand<bool?>(isChecked =>
            {
                if (isChecked != null)
                {
                    foreach (var item in this.DisplayFavorites)
                    {
                        item.IsDeleting = (bool)isChecked;
                    }
                }
            }, isChecked => this.DisplayFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayFavorites.Count);
        }

        private void SubscribeEvents(IEventAggregator eventAggregator)
        {
            // 手动通知,比封装属性然后触发PropertyChanged事件快一些
            FavoriteMusicViewModel.DeleteStatusChanged += isDeleteing => this.CheckSelectAllStatus();

            FavoriteMusicViewModel.DeleteStatusChanged += isDeleteing => this.NotifyBatchDeleteCanExecute();

            FavoriteMusicViewModel.DeleteStatusChanged +=
                isDeleting => this.RaisePropertyChanged(nameof(SelectedCount));

            PlayingMusicViewModel.ToNextMusic += NextMusic;

            PlayingMusicViewModel.ScrollBarMoveToLyric += cureentLineIndex =>
                eventAggregator.GetEvent<UpdateScrollBarToTargetLyricEvent>().Publish(cureentLineIndex);

            MusicWithClassifyModel.SelectStatusChanged +=
                selected => this.DistributeMusicViewModel.CheckClassifySelectAllStatus();

            eventAggregator.GetEvent<ToggeleCurrentMusicEvent>().Subscribe(() =>
            {
                if (this.CurrentMusic != null)
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

            eventAggregator.GetEvent<PrevMusicEvent>().Subscribe(() => this.PrevMusic(this.CurrentMusic));
            eventAggregator.GetEvent<NextMusicEvent>().Subscribe(() => this.NextMusic(this.CurrentMusic));

            eventAggregator.GetEvent<BatchAddToPlayingEvent>().Subscribe(coll => this.AddAllToPlaying(new BatchAddAndPlayModel(null, coll), false));
        }

        private void PrevMusic(PlayingMusicViewModel currentMusic)
        {
            if (currentMusic != null && this.DisplayPlaying.Count > 0)
            {
                if (this.CurrentPlayOrder != null && this.CurrentPlayOrder.OrderType == MediaPlayOrderModel.EnumOrderType.Random)
                {
                    this.SetAndPlay(this.DisplayPlaying[this._random.Next(this.DisplayPlaying.Count)]);
                    return;
                }

                if (currentMusic.Index > 1)
                {
                    this.SetAndPlay(this.Playing[currentMusic.Index - 2]);
                }
                else
                {
                    this.SetAndPlay(this.Playing[this.Playing.Count - 1]);
                }
            }
        }

        private void NextMusic(PlayingMusicViewModel currentMusic)
        {
            if (currentMusic != null && this.DisplayPlaying.Count > 0)
            {
                if (this.CurrentPlayOrder != null)
                {
                    switch (this.CurrentPlayOrder.OrderType)
                    {
                        case MediaPlayOrderModel.EnumOrderType.Order:
                            if (currentMusic.Index == this.Playing.Count)
                            {
                                this._eventAggregator.GetEvent<ResetPlayerEvent>().Publish();
                                this.SetAndPlay(null);
                                return;
                            }

                            break;
                        case MediaPlayOrderModel.EnumOrderType.Loop:
                            break;
                        case MediaPlayOrderModel.EnumOrderType.Random:
                            this.SetAndPlay(this.DisplayPlaying[this._random.Next(this.DisplayPlaying.Count)]);
                            return;
                        case MediaPlayOrderModel.EnumOrderType.SingleCycle:
                            this._eventAggregator.GetEvent<ResetPlayerAndPlayMusicEvent>().Publish();
                            return;
                        case MediaPlayOrderModel.EnumOrderType.SingleOnce:
                            this._eventAggregator.GetEvent<ResetPlayerEvent>().Publish();
                            this.SetAndPlay(null);
                            return;
                        default:
                            throw new IndexOutOfRangeException();
                    }
                }

                if (currentMusic.Index < this.Playing.Count)
                {
                    this.SetAndPlay(this.Playing[currentMusic.Index]);
                }
                else
                {
                    this.SetAndPlay(this.Playing.Count > 0 ? this.Playing[0] : null);
                }
            }
        }

        private void SetAndPlay(PlayingMusicViewModel item)
        {
            this.CurrentMusic = item;

            if (this.Running = item != null)
            {
                this.CurrentMusic.ResetABPoint();

                this._eventAggregator.GetEvent<ResetPlayerAndPlayMusicEvent>().Publish();
                this._eventAggregator.GetEvent<UpdateScrollBarToTargetLyricEvent>().Publish(0);
            }
        }

        private void TryClearFavoriteListFilteKeyWords()
        {
            if (!string.IsNullOrEmpty(this.FavoriteListFilteKeyWords))
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
            System.Windows.Forms.OpenFileDialog openFileDialog =
                CommonUtils.OpenFileDialog(AppStatics.LastMusicDir, new MusicMedia());

            if (openFileDialog != null)
            {
                await this.LoadMusicAsync(openFileDialog.FileNames, openFileDialog.InitialDirectory);
            }
        }

        private async void AddMusicFromFolderDialog()
        {
            var selectedPath = CommonUtils.OpenFolderDialog(AppStatics.LastMusicDir);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                selectedPath = selectedPath.EnsureEndsWith("/");
                if (!selectedPath.Equals(AppStatics.LastMusicDir, StringComparison.CurrentCultureIgnoreCase))
                {
                    _config.WriteConfigNode(selectedPath, new[] { "Music", nameof(AppStatics.LastMusicDir) });

                    AppStatics.LastMusicDir = selectedPath;
                }

                var list = selectedPath.GetFiles(str => str.EndsWith(".mp3"));

                await this.LoadMusicAsync(list, selectedPath);
            }
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

        private async Task LoadMusicAsync(IEnumerable<string> filePaths, string directory)
        {
            if (filePaths == null || !filePaths.Any())
            {
                return;
            }

            List<string> list = this.GetNewFiles(filePaths).ToList();

            if (list.Count == 0)
            {
                return;
            }

            this.IsLoading = true;

            //await Task.Delay(1);

            await this.MultiThreadBatchLoadMusic(list, directory);

            this.IsLoading = false;
        }

        private async Task SingleThreadLoadMusic(List<string> filePathList, string directory)
        {
            Debug.Assert(filePathList != null && filePathList.Count > 0);

            foreach (var file in filePathList)
            {
                this.DisplayFavorites.Add(new FavoriteMusicViewModel(new MusicModel(file)));
            }

            this.Favorites.AddRange(this.DisplayFavorites);
        }

        private async Task MultiThreadBatchLoadMusic(List<string> filePathList, string directory)
        {
            Debug.Assert(filePathList != null && filePathList.Count > 0);

            var taskList = new List<Task>();

            var newMusicList = new List<FavoriteMusicViewModel>();

            var step = 40;
            for (int i = 0; i < filePathList.Count / step + 1; i++)
            {
                var index = i;
                taskList.Add(
                    Task.Run(() =>
                        {
                            foreach (var item in filePathList
                                         .Skip(index * step)
                                         .Take(step))
                            {
                                CommonUtils.Invoke(() =>
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
            var item = new PlayingMusicViewModel(music.Music);
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
                        PlayingMusicViewModel temp = new PlayingMusicViewModel(item.Music);
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
        public ObservableCollection<PlayingMusicViewModel> DisplayPlaying { get; private set; } = new();

        public bool IsBatchSelect
        {
            get { return _isBatchSelect; }
            set { SetProperty<bool>(ref _isBatchSelect, value); }
        }

        private bool _isEditingPlayOrder;

        public bool IsEditingPlayOrder
        {
            get => this._isEditingPlayOrder;
            set => SetProperty<bool>(ref _isEditingPlayOrder, value);
        }

        private MediaPlayOrderModel _mediaPlayOrder;

        public MediaPlayOrderModel CurrentPlayOrder
        {
            get { return _mediaPlayOrder; }
            set { _mediaPlayOrder = value; IsEditingPlayOrder = false; }
        }

        public bool SelectFavoriteAll
        {
            get { return _selectFavoriteAll; }
            set { SetProperty<bool>(ref _selectFavoriteAll, value); }
        }

        #endregion

        #region Fields

        private bool _isLoading;
        private bool _running;
        private bool _isBatchSelect;
        private PlayingMusicViewModel _currentMusic;

        private bool _selectFavoriteAll;

        private string _playingListFilteKeyWords;
        private string _favoriteListFilteKeyWords;

        private readonly IEventAggregator _eventAggregator;

        private Random _random = new Random();

        #endregion

        #region Command

        public ICommand OpenInExploreCommand { get; private set; }

        public ICommand PointACommand { get; private set; }
        public ICommand PointBCommand { get; private set; }

        public ICommand ResetPointABCommand { get; private set; }

        public ICommand StopPlayMusicCommand { get; private set; }

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
        /// 播放或暂停
        /// </summary>
        public ICommand PlayPlayingCommand { get; set; }

        public ICommand PrevCommand { get; private set; }
        public ICommand NextCommand { get; private set; }

        /// <summary>
        /// 添加到下一首播放
        /// </summary>
        public ICommand AddNextCommand { get; private set; }

        /// <summary>
        /// 下载当前
        /// </summary>
        public ICommand DownLoadCommand { get; private set; }


        /// <summary>
        /// 歌曲进度后退,此地只为让前端按钮在该禁用时禁用
        /// </summary>
        public ICommand DelayCommand { get; private set; }

        /// <summary>
        /// 歌曲进度提前,此地只为让前端按钮在该禁用时禁用
        /// </summary>
        public ICommand AheadCommand { get; private set; }

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

        /// <summary>
        /// 清空播放队列
        /// </summary>
        public ICommand CleanPlayingCommand { get; private set; }

        #endregion

        private IConfigManager _config;
        public MusicPlayerViewModel(IEventAggregator eventAggregator, IConfigManager config)
        {
            this._config = config;
            this._eventAggregator = eventAggregator;

            this.LoadConfig(config);

            this.InitCommands();

            this.SubscribeEvents(eventAggregator);

            this.DistributeMusicViewModel = new DistributeMusicViewModel(this.Favorites, eventAggregator);
            this.DistributeMusicViewModel.ClearFavoriteListFilteKeyWords += TryClearFavoriteListFilteKeyWords;
        }

        private void LoadConfig(IConfigManager config)
        {
            var baseNode = "Music";
            var musicKey = "MusicPlayOrder";

            var musicPlayOrder = config.ReadConfigNode(new[] { baseNode, musicKey });

            if (!string.IsNullOrEmpty(musicPlayOrder))
            {
                this.CurrentPlayOrder =
                    AppStatics.MediaPlayOrderList.FirstOrDefault(item => item.Description == musicPlayOrder) ??
                    AppStatics.MediaPlayOrderList.First();
            }

            AppStatics.LastMusicDir = config.ReadConfigNode(new[] { baseNode, nameof(AppStatics.LastMusicDir) }) ??
                                      "G:/KuGou/";

            config.SetConfig += config =>
            {
                config.WriteConfigNode(this.CurrentPlayOrder.Description, new[] { baseNode, musicKey });
            };
        }

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

            this.DisplayPlaying.Clear();
            this.DisplayPlaying = null;
            foreach (var item in this.Playing)
            {
                item.Dispose();
            }

            this.Playing.Clear();
            this.Playing = null;

            this.DistributeMusicViewModel.Dispose();

            this.AddFilesCommand = null;
            this.PlayPlayingCommand = null;
            this.PrevCommand = null;
            this.NextCommand = null;

            this.PlayAllCommand = null;
            this.PlayFavoriteCommand = null;
            this.AddNextCommand = null;
            this.DownLoadCommand = null;
            this.BatchDeleteCommand = null;
            this.SelectAllCommand = null;
            this.CleanPlayingCommand = null;

            this.AddFilesCommand = null;
            this.AddFolderCommand = null;

            this.DelayCommand = null;
            this.AheadCommand = null;

            this.OpenInExploreCommand = null;
            this.PointACommand = null;
            this.PointBCommand = null;
            this.StopPlayMusicCommand = null;
        }
    }
}
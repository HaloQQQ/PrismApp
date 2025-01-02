using MusicPlayerModule.Models;
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

        protected override void AllMediaModelNotPlaying()
        {
            foreach (var item in this.Playing.Where(m => m.IsPlayingMedia))
            {
                item.IsPlayingMedia = false;
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
        private void BatchAddToPlay(BatchAddAndPlayModel? aggregate = null)
        {
            if (aggregate == null)
            {
                aggregate = new BatchAddAndPlayModel(null, this.DistributeMusicViewModel.DisplayFavorites);
                aggregate.TargetToPlay = this.DistributeMusicViewModel.DisplayFavorites.First();
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

        public Collection<PlayingMusicViewModel> Playing { get; private set; } = new();

        #endregion

        #region Fields
        private string _playingListFilteKeyWords;
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
        /// 用于启用或禁用使用该命令的控件
        /// </summary>
        public ICommand SelectAllCommand { get; private set; }

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
                    CommonUtil.PublishMessage(_eventAggregator, $"选中文件中不存在新的mp3音乐文件");
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
                    CommonUtil.PublishMessage(_eventAggregator, $"【{selectedPath}】中找不到新的mp3音乐文件");
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
                    if (!this.DistributeMusicViewModel.DisplayFavorites.Any(item => item.Music.FilePath == filePath || item.Music.FilePath.GetFileNameWithoutExtension() == filePath.GetFileNameWithoutExtension()))
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
                children.AddTo(DistributeMusicViewModel.DisplayFavorites);
                children.AddTo(DistributeMusicViewModel.Favorites);

                DistributeMusicViewModel.AddNewMusic(children);
            }

            this.DistributeMusicViewModel.RefreshFavoriteIndex();
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
                                children.AddTo(DistributeMusicViewModel.DisplayFavorites);
                                children.AddTo(DistributeMusicViewModel.Favorites);

                                DistributeMusicViewModel.AddNewMusic(children);
                            });
                        }
                    }
                    )
                );
            }

            await Task.WhenAll(taskList);

            this.DistributeMusicViewModel.RefreshFavoriteIndex();
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

            eventAggregator.GetEvent<BatchAddToPlayingEvent>().Subscribe(coll => this.AddAllToPlaying(new BatchAddAndPlayModel(null, coll), false));

            eventAggregator.GetEvent<ToggleDesktopLyricEvent>().Subscribe(() =>
            {
                this.DesktopLyric.IsDesktopLyricShow = !this.DesktopLyric.IsDesktopLyricShow;
            });
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
                    CommonUtil.PublishMessage(_eventAggregator, $"传入的分类{category?.ClassifyKey}为空");
                    return;
                }

                this.BatchAddToPlay(new BatchAddAndPlayModel(category.DisplayByClassifyKeyFavorites.First(),
                    category.DisplayByClassifyKeyFavorites));
            }, _ => this.DistributeMusicViewModel.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DistributeMusicViewModel.DisplayFavorites.Count);

            this.PlayAndAddCurrentFavoritesCommand = new DelegateCommand<BatchAddAndPlayModel>(model =>
            {
                if (model == null || model.Collection.IsNullOrEmpty())
                {
                    CommonUtil.PublishMessage(_eventAggregator, $"传入的音乐集合为空");
                    return;
                }

                this.BatchAddToPlay(model);
            }, _ => this.DistributeMusicViewModel.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DistributeMusicViewModel.DisplayFavorites.Count);

            this.PlayAllCommand = new DelegateCommand(
                () => this.BatchAddToPlay(),
                () => this.DistributeMusicViewModel.DisplayFavorites.Count > 0
                ).ObservesProperty<int>(() => this.DistributeMusicViewModel.DisplayFavorites.Count);

            this.PlayFavoriteCommand = new DelegateCommand<FavoriteMusicViewModel>(music =>
            {
                if (music == null)
                {
                    CommonUtil.PublishMessage(_eventAggregator, "传入的音乐为空");
                    return;
                }

                var item = this.Playing.FirstOrDefault(playing => playing.Music == music.Music);
                if (item == null)
                {
                    item = this.AddOneToPlayingList(music);
                }

                this.SetAndPlay(item);
            }, _ => this.DistributeMusicViewModel.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DistributeMusicViewModel.DisplayFavorites.Count);

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

                if (this.Playing.Any(p => p.Music == music.Music))
                {
                    CommonUtil.PublishMessage(_eventAggregator, "同名歌曲已存在");
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
            }, _ => this.DistributeMusicViewModel.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DistributeMusicViewModel.DisplayFavorites.Count);

            this.DownLoadCommand = new DelegateCommand<MusicModel>(music => { }, _ => false);

            this.SelectAllCommand = new DelegateCommand(() => { }, () => this.DistributeMusicViewModel.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DistributeMusicViewModel.DisplayFavorites.Count);
        }
        #endregion

        void IDisposable.Dispose()
        {
            foreach (var item in this.Playing)
            {
                item.Dispose();
            }

            this.Playing.Clear();
            this.Playing = null;

            this.DistributeMusicViewModel.Dispose();
            this.DistributeMusicViewModel = null;

            base.Dispose();
        }
    }
}
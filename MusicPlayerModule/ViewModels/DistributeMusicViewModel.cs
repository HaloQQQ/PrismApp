using MusicPlayerModule.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MusicPlayerModule.Contracts;
using System.Collections.Specialized;
using PrismAppBasicLib.Models;
using PrismAppBasicLib.Contracts;
using Microsoft.Win32;
using MusicPlayerModule.Utils;
using IceTea.Pure.BaseModels;
using IceTea.Pure.Utils;
using IceTea.Pure.Extensions;
using Prism.Events;
using IceTea.Pure.Contracts;
using Prism.Commands;
using IceTea.Wpf.Core.Utils;
using IceTea.Wpf.Atom.Utils;
using IceTea.Wpf.Atom.Contracts.FileFilters;

namespace MusicPlayerModule.ViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
    /// <summary>
    /// 按照歌名、专辑、歌手、自定义分类
    /// </summary>
    internal class DistributeMusicViewModel : NotifyBase
    {
        public int SelectedCount => this.DisplayFavorites.Count(item => item.IsDeleting);

        /// <summary>
        /// 是否显示批量删除按钮
        /// </summary>
        public bool CanBatchSelect
        {
            get => _canBatchSelect;
            set => SetProperty<bool>(ref _canBatchSelect, value);
        }

        private IEventAggregator _eventAggregator;

        public DistributeMusicViewModel(IEventAggregator eventAggregator, ISettingManager<SettingModel> settingManager)
        {
            this._eventAggregator = eventAggregator.AssertNotNull(nameof(IEventAggregator));

            MusicWithClassifyModel.SelectedEvent += OnlyRefreshClassifySelectAllStatus;

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

#pragma warning disable CS8602 // 解引用可能出现空引用。
            FavoriteMusicViewModel.DeleteStatusChanged += newValue =>
            {
                if (newValue != SelectAlbumFavoriteAll)
                {
                    if (newValue)
                    {
                        var c = DisplayAlbumClassify.FirstOrDefault(c => c.IsSelected);

                        if (c.IsNullOr(_ => _.HasUnselected))
                        {
                            return;
                        }
                    }

                    _selectAlbumFavoriteAll = newValue;

                    RaisePropertyChanged(nameof(SelectAlbumFavoriteAll));
                }
            };

            FavoriteMusicViewModel.DeleteStatusChanged += newValue =>
            {
                if (newValue != SelectSingerFavoriteAll)
                {
                    if (newValue)
                    {
                        var c = DisplaySingerClassify.FirstOrDefault(c => c.IsSelected);

                        if (c.IsNullOr(_ => _.HasUnselected))
                        {
                            return;
                        }
                    }

                    _selectSingerFavoriteAll = newValue;

                    RaisePropertyChanged(nameof(SelectSingerFavoriteAll));
                }
            };

            FavoriteMusicViewModel.DeleteStatusChanged += newValue =>
            {
                if (newValue != SelectDirFavoriteAll)
                {
                    if (newValue)
                    {
                        var c = DisplayDirClassify.FirstOrDefault(c => c.IsSelected);

                        if (c.IsNullOr(_ => _.HasUnselected))
                        {
                            return;
                        }
                    }

                    _selectDirFavoriteAll = newValue;

                    RaisePropertyChanged(nameof(SelectDirFavoriteAll));
                }
            };

            this.DisplayDirClassify.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (e.OldItems != null)
                    {
                        foreach (MusicWithClassifyModel item in e.OldItems)
                        {
                            this.MusicDirs.RemoveAll(i => i.DirPath == item.ClassifyKey);
                        }
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    if (e.NewItems != null)
                    {
                        foreach (MusicWithClassifyModel item in e.NewItems)
                        {
                            this.MusicDirs.AddIfNotAnyWhile(
                                model => item.ClassifyKey == model.DirPath,
                                () => new MusicDirModel(item.ClassifyKey)
                            );
                        }
                    }
                }
            };

            this.InitCommands(settingManager);
        }

        private void InitCommands(ISettingManager<SettingModel> settingManager)
        {
            this.DeleteFavoriteCommand = new DelegateCommand<FavoriteMusicViewModel>(music =>
            {
                if (music == null)
                {
                    CommonUtil.PublishMessage(_eventAggregator, "传入的列表中待删除音乐为空");
                    return;
                }

                music.Dispose();

                this.TryRefreshFavoriteIndex();

                this.OnlyRefreshClassifySelectAllStatus();

                this.SelectFavoriteAll = this.DisplayFavorites.Count > 0 && !this.DisplayFavorites.Any(m => !m.IsDeleting);

                this.RaisePropertyChanged(nameof(SelectedCount));
            }, _ => this.DisplayFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DisplayFavorites.Count);

            this.BatchDeleteCommand = new DelegateCommand(() =>
            {
                FavoriteMusicViewModel[]? items = default;

                if (this.IsInSong)
                {
                    items = DisplayFavorites.Where(item => item.IsDeleting).ToArray();
                }
                else if (this.IsInAlbum)
                {
                    items = DisplayAlbumClassify
                                    .FirstOrDefault(i => i.IsSelected)?.ClassifyFavorites
                                        .Where(m => m.IsDeleting).ToArray();
                }
                else if (this.IsInSinger)
                {
                    items = DisplaySingerClassify
                                    .FirstOrDefault(i => i.IsSelected)?.ClassifyFavorites
                                        .Where(m => m.IsDeleting).ToArray();
                }
                else if (this.IsInDir)
                {
                    items = DisplayDirClassify
                                    .FirstOrDefault(i => i.IsSelected)?.ClassifyFavorites
                                        .Where(m => m.IsDeleting).ToArray();
                }

                items.AssertNotNull(nameof(items));

                for (int i = items.Length - 1; i >= 0; i--)
                {
                    items[i].Dispose();
                }

                this.OnlyRefreshClassifySelectAllStatus();

                this.SelectFavoriteAll = this.DisplayFavorites.Count > 0 && !this.DisplayFavorites.Any(m => !m.IsDeleting);

                this.TryRefreshFavoriteIndex();

                this.RaisePropertyChanged(nameof(SelectedCount));
            }, () => this.DisplayFavorites.Any(item => item.IsDeleting))
                .ObservesProperty(() => this.SelectedCount);

            this.BatchMoveMusicDirCommand = new DelegateCommand<string>(originDir =>
            {
                if (originDir.IsNullOrBlank())
                {
                    CommonUtil.PublishMessage(_eventAggregator, "未传入源目录");
                    return;
                }

                var dirItem = DisplayDirClassify.First(item => item.ClassifyKey == originDir);
                if (dirItem.MoveMusicsTo(DisplayDirClassify, settingManager[CustomStatics.MUSIC].Value))
                {
                    CommonUtil.PublishMessage(_eventAggregator, $"移动成功");
                }
                else
                {
                    CommonUtil.PublishMessage(_eventAggregator, "目标目录已包含源目录所有内容，不允许重复");
                }
            });

            this.AddMusicDirCommand = new DelegateCommand(() =>
            {
                var path = settingManager[CustomStatics.MUSIC].Value;

                var selectedPath = WpfCoreUtils.OpenFolderDialog(path);

                if (!selectedPath.IsNullOrBlank())
                {
                    if (MusicWithClassifyModel.TryCreateDirItemFromPath(this.DisplayDirClassify, selectedPath))
                    {
                        CommonUtil.PublishMessage(_eventAggregator, $"{selectedPath}分类创建成功");
                    }
                    else
                    {
                        CommonUtil.PublishMessage(_eventAggregator, $"{selectedPath}分类之前已存在");
                    }
                }
            });

            this.RemoveMusicDirCommand = new DelegateCommand<string>(dir =>
            {
                if (CommonUtil.TryDeleteEmptyFolder(_eventAggregator, dir))
                {
                    this.DisplayDirClassify.RemoveAll(c => c.ClassifyKey.EqualsIgnoreCase(dir) && c.ClassifyFavorites.Count == 0);
                }
            });

            this.RenameMusicDirCommand = new DelegateCommand<MusicWithClassifyModel>(item =>
            {
                if (item == null)
                {
                    CommonUtil.PublishMessage(_eventAggregator, $"未选中{item?.ClassifyKey}分类");
                    return;
                }

                if (item.MoveMusicsTo(this.DisplayDirClassify, settingManager[CustomStatics.MUSIC].Value))
                {
                    CommonUtil.PublishMessage(_eventAggregator, "目录重命名成功");
                }
                else
                {
                    CommonUtil.PublishMessage(_eventAggregator, $"{item.ClassifyKey}分类之前已存在");
                }
            });

            this.DistributeToCommand = new DelegateCommand<MusicMoveModel>(moveModel =>
            {
                if (moveModel.IsNullOr(m => m.Music == null))
                {
                    return;
                }

                try
                {
                    using (moveModel)
                    {
                        if (moveModel.MoveTo(this.DisplayDirClassify))
                        {
                            CommonUtil.PublishMessage(_eventAggregator, $"【{moveModel.Music.Name}】移动成功");
                        }
                    }
                }
                catch (Exception ex)
                {
                    CommonUtil.PublishMessage(_eventAggregator, ex.Message);
                }
            });
        }

        internal void AddNewMusic(FavoriteMusicViewModel musicModel)
        {
            TryAddNewMusic(DisplayDirClassify, null, musicModel.Music.FileDir, MusicClassifyType.Dir);

            TryAddNewMusic(SingerClassify, DisplaySingerClassify, musicModel.Music.Singer, MusicClassifyType.Singer);

            TryAddNewMusic(AlbumClassify, DisplayAlbumClassify, musicModel.Music.Album, MusicClassifyType.Album);

            void TryAddNewMusic(Collection<MusicWithClassifyModel> collection, ObservableCollection<MusicWithClassifyModel> displayCollection, string classifyKey, MusicClassifyType classifyType)
            {
                var classifyModel = collection.FirstOrDefault(item => item.ClassifyKey == classifyKey);

                if (classifyModel == null)
                {
                    classifyModel = new MusicWithClassifyModel(
                                classifyKey,
                                new ObservableCollection<FavoriteMusicViewModel>(),
                                classifyType
                            );

                    classifyModel.TryAddTo(collection);

                    if (displayCollection != null)
                    {
                        classifyModel.TryAddTo(displayCollection);
                    }
                }

                musicModel.TryAddTo(classifyModel.ClassifyFavorites);
            }
        }

        private void OnlyRefreshClassifySelectAllStatus(MusicWithClassifyModel current = null)
        {
            var newValue = IsCollectionSelectedAll(this.DisplayAlbumClassify);
            if (newValue != SelectAlbumFavoriteAll)
            {
                if (newValue && (current ?? DisplayAlbumClassify.First(item => item.IsSelected)).HasUnselected)
                {
                    return;
                }

                this._selectAlbumFavoriteAll = newValue;
                RaisePropertyChanged(nameof(SelectAlbumFavoriteAll));
            }

            newValue = IsCollectionSelectedAll(this.DisplaySingerClassify);
            if (newValue != SelectSingerFavoriteAll)
            {
                if (newValue && (current ?? DisplaySingerClassify.First(item => item.IsSelected)).HasUnselected)
                {
                    return;
                }

                this._selectSingerFavoriteAll = newValue;
                RaisePropertyChanged(nameof(SelectSingerFavoriteAll));
            }

            newValue = IsCollectionSelectedAll(this.DisplayDirClassify);
            if (newValue != SelectDirFavoriteAll)
            {
                if (newValue && (current ?? DisplayDirClassify.First(item => item.IsSelected)).HasUnselected)
                {
                    return;
                }

                this._selectDirFavoriteAll = newValue;
                RaisePropertyChanged(nameof(SelectDirFavoriteAll));
            }

            bool IsCollectionSelectedAll(ICollection<MusicWithClassifyModel> collection)
                => collection.Count > 0 &&
                        collection.Any(item => item.IsSelected) &&
                        !(current ?? collection.First(item => item.IsSelected)).HasUnselected;
        }

        private void TryRefreshFavoriteIndex()
        {
            if (this.AllowRefreshFavoriteIndex)
            {
                for (int i = 0; i < DisplayFavorites.Count; i++)
                {
                    DisplayFavorites[i].Index = i + 1;
                }
            }
        }

        #region Logicals
        internal async Task AddMediaFromFolderDialogAsync(ISettingManager<SettingModel> settingManger)
        {
            var musicSetting = settingManger[CustomStatics.MUSIC];
            var lyricSetting = settingManger[CustomStatics.LYRIC];

            var selectedFolder = WpfCoreUtils.OpenFolderDialog(musicSetting.Value);
            if (!selectedFolder.IsNullOrBlank())
            {
                var list = selectedFolder.GetFiles(true, str => str.EndsWithIgnoreCase(".mp3"));

                if (await this.TryLoadMusicAsync(list))
                {
                    musicSetting.Value = selectedFolder;

                    var lyricDir = await KRCLyrics.TryGetRealDir(selectedFolder);
                    if (!lyricDir.IsNullOrBlank())
                    {
                        lyricSetting.Value = lyricDir;
                    }
                }
                else
                {
                    CommonUtil.PublishMessage(_eventAggregator, $"【{selectedFolder}】中找不到新的mp3音乐文件");
                }
            }
        }

        internal async Task AddMediaFromFileDialogAsync(ISettingManager<SettingModel> settingManger)
        {
            var musicSetting = settingManger[CustomStatics.MUSIC];
            var lyricSetting = settingManger[CustomStatics.LYRIC];

            OpenFileDialog openFileDialog =
                WpfAtomUtils.OpenFileDialog(musicSetting.Value, new MusicFilter());

            if (openFileDialog != null)
            {
                if (await this.TryLoadMusicAsync(openFileDialog.FileNames))
                {
                    var musicDir = openFileDialog.FileName.GetParentPath();

                    musicSetting.Value = musicDir;

                    var lyricDir = await KRCLyrics.TryGetRealDir(musicDir);
                    if (!lyricDir.IsNullOrBlank())
                    {
                        lyricSetting.Value = lyricDir;
                    }
                }
                else
                {
                    CommonUtil.PublishMessage(_eventAggregator, $"选中文件中不存在新的mp3音乐文件");
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

            this.TryRefreshFavoriteIndex();

            this.IsLoading = false;

            return true;

            IEnumerable<string> TryGetNewFiles(IEnumerable<string> filePaths)
            {
                foreach (var filePath in filePaths)
                {
                    if (!this.DisplayFavorites.Any(item => item.Music.FilePath == filePath))
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
                children.TryAddTo(DisplayFavorites);
                children.TryAddTo(Favorites);

                AddNewMusic(children);
            }
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
                            WpfAtomUtils.Invoke(() =>
                            {
                                var children = new FavoriteMusicViewModel(new MusicModel(item));
                                children.TryAddTo(DisplayFavorites);
                                children.TryAddTo(Favorites);

                                AddNewMusic(children);
                            });
                        }
                    }
                    )
                );
            }

            await Task.WhenAll(taskList);
        }
        #endregion

        #region Fields
        private bool _canBatchSelect;
        private bool _selectFavoriteAll;

        private bool _isLoading;

        private bool _isInSong = true;
        private bool _isInAlbum;
        private bool _isInSinger;
        private bool _isInDir;

        private bool _selectAlbumFavoriteAll;
        private bool _selectSingerFavoriteAll;
        private bool _selectDirFavoriteAll;

        private string _favoriteListFilteKeyWords;
        private string _albumMusicFilteKeyWords;
        private string _singerMusicFilteKeyWords;
        #endregion

        #region GenericProps
        private Collection<FavoriteMusicViewModel> Favorites { get; } = new();
        public ObservableCollection<FavoriteMusicViewModel> DisplayFavorites { get; } = new();

        private Collection<MusicWithClassifyModel> AlbumClassify { get; } = new();
        public ObservableCollection<MusicWithClassifyModel> DisplayAlbumClassify { get; private set; } = new();

        private Collection<MusicWithClassifyModel> SingerClassify { get; } = new();
        public ObservableCollection<MusicWithClassifyModel> DisplaySingerClassify { get; private set; } = new();

        public ObservableCollection<MusicWithClassifyModel> DisplayDirClassify { get; private set; } = new();
        public ObservableCollection<MusicDirModel> MusicDirs { get; private set; } = new();

        public bool IsLoading
        {
            get => this._isLoading;
            private set => SetProperty<bool>(ref _isLoading, value);
        }

        #region 按歌名分类
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

        public bool SelectFavoriteAll
        {
            get => _selectFavoriteAll;
            set
            {
                if (SetProperty<bool>(ref _selectFavoriteAll, value))
                {
                    this.DisplayFavorites.ForEach(i => i.IsDeleting = value);
                }
            }
        }

        private bool AllowRefreshFavoriteIndex => FavoriteListFilteKeyWords.IsNullOrBlank();

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
                                    || item.Music.Singer.ContainsIgnoreCase(_favoriteListFilteKeyWords)
                                )
                        );
                    }
                    else
                    {
                        this.DisplayFavorites.AddRange(this.Favorites);

                        this.TryRefreshFavoriteIndex();
                    }
                }
            }
        }
        #endregion

        #region 按专辑分类
        public bool IsInAlbum
        {
            get => _isInAlbum;
            set
            {
                if (SetProperty(ref _isInAlbum, value) && !value)
                {
                    if (!this.AlbumMusicFilteKeyWords.IsNullOrBlank())
                    {
                        this.AlbumMusicFilteKeyWords = string.Empty;
                    }
                }
            }
        }

        public bool SelectAlbumFavoriteAll
        {
            get => _selectAlbumFavoriteAll;
            set
            {
                if (SetProperty<bool>(ref _selectAlbumFavoriteAll, value))
                {
                    var current = this.DisplayAlbumClassify.FirstOrDefault(item => item.IsSelected);

                    current?.ClassifyFavorites.ForEach(m => m.IsDeleting = value);
                }
            }
        }

        /// <summary>
        /// 按照专辑分类后的关键词筛选
        /// </summary>
        public string AlbumMusicFilteKeyWords
        {
            get => this._albumMusicFilteKeyWords;
            set
            {
                if (SetProperty<string>(ref _albumMusicFilteKeyWords, value))
                {
                    if (this.AlbumClassify.Count == 0)
                    {
                        return;
                    }

                    this.DisplayAlbumClassify.Clear();

                    if (!_albumMusicFilteKeyWords.IsNullOrBlank())
                    {
                        this.DisplayAlbumClassify.AddRange(this.AlbumClassify.Where(item =>
                            item.ClassifyKey.ContainsIgnoreCase(this._albumMusicFilteKeyWords)));
                    }
                    else
                    {
                        this.DisplayAlbumClassify.AddRange(this.AlbumClassify);
                    }
                }
            }
        }
        #endregion

        #region 按歌手分类
        public bool IsInSinger
        {
            get => _isInSinger;
            set
            {
                if (SetProperty(ref _isInSinger, value) && !value)
                {
                    if (!this.SingerMusicFilteKeyWords.IsNullOrBlank())
                    {
                        this.SingerMusicFilteKeyWords = string.Empty;
                    }
                }
            }
        }

        public bool SelectSingerFavoriteAll
        {
            get => _selectSingerFavoriteAll;
            set
            {
                if (SetProperty<bool>(ref _selectSingerFavoriteAll, value))
                {
                    var current = this.DisplaySingerClassify.FirstOrDefault(item => item.IsSelected);

                    current?.ClassifyFavorites.ForEach(m => m.IsDeleting = value);
                }
            }
        }

        /// <summary>
        /// 按照歌手分类后的关键词筛选
        /// </summary>
        public string SingerMusicFilteKeyWords
        {
            get => this._singerMusicFilteKeyWords;
            set
            {
                if (SetProperty<string>(ref _singerMusicFilteKeyWords, value))
                {
                    if (this.SingerClassify.Count == 0)
                    {
                        return;
                    }

                    this.DisplaySingerClassify.Clear();

                    if (!_singerMusicFilteKeyWords.IsNullOrBlank())
                    {
                        this.DisplaySingerClassify.AddRange(this.SingerClassify.Where(item =>
                            item.ClassifyKey.ContainsIgnoreCase(this._singerMusicFilteKeyWords)));
                    }
                    else
                    {
                        this.DisplaySingerClassify.AddRange(this.SingerClassify);
                    }
                }
            }
        }
        #endregion

        #region 按文件夹分类
        public bool IsInDir
        {
            get => _isInDir;
            set => SetProperty(ref _isInDir, value);
        }

        public bool SelectDirFavoriteAll
        {
            get => _selectDirFavoriteAll;
            set
            {
                if (SetProperty<bool>(ref _selectDirFavoriteAll, value))
                {
                    var current = this.DisplayDirClassify.FirstOrDefault(item => item.IsSelected);

                    current?.ClassifyFavorites.ForEach(m => m.IsDeleting = value);
                }
            }
        }
        #endregion
        #endregion

        #region Commands
        /// <summary>
        /// 删除一条Favorite
        /// </summary>
        public ICommand DeleteFavoriteCommand { get; private set; }

        /// <summary>
        /// 批量删除收藏列表
        /// </summary>
        public ICommand BatchDeleteCommand { get; private set; }

        public ICommand BatchMoveMusicDirCommand { get; private set; }
        public ICommand AddMusicDirCommand { get; private set; }
        public ICommand RemoveMusicDirCommand { get; private set; }
        public ICommand RenameMusicDirCommand { get; private set; }

        /// <summary>
        /// 添加到..分类
        /// </summary>
        public ICommand DistributeToCommand { get; private set; }

        public ICommand CurrentDirSelectAllCommand { get; private set; }
        public ICommand CurrentSingerSelectAllCommand { get; private set; }
        public ICommand CurrentAlbumSelectAllCommand { get; private set; }
        #endregion

        protected override void DisposeCore()
        {
            foreach (var item in this.Favorites)
            {
                item.Dispose();
            }

            this.DisplayFavorites.Clear();
            this.Favorites.Clear();


            this.DisplayAlbumClassify.Clear();

            foreach (var item in AlbumClassify)
            {
                item.Dispose();
            }
            this.AlbumClassify.Clear();


            this.DisplaySingerClassify.Clear();

            foreach (var item in SingerClassify)
            {
                item.Dispose();
            }
            this.SingerClassify.Clear();


            foreach (var item in DisplayDirClassify)
            {
                item.Dispose();
            }
            this.DisplayDirClassify.Clear();

            this.MusicDirs.Clear();
            this.MusicDirs = null;

            this._eventAggregator = null;

            MusicWithClassifyModel.SelectedEvent -= OnlyRefreshClassifySelectAllStatus;

            base.DisposeCore();
        }
    }
}
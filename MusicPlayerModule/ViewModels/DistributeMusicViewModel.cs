using MusicPlayerModule.Models;
using Prism.Commands;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Windows.Input;
using IceTea.Atom.Extensions;
using System.IO;
using IceTea.Atom.Utils;
using IceTea.Atom.Contracts;
using IceTea.Wpf.Core.Utils;
using MusicPlayerModule.Contracts;
using System.Collections.Specialized;
using IceTea.Atom.BaseModels;
using MusicPlayerModule.MsgEvents.Music;
using PrismAppBasicLib.Models;
using PrismAppBasicLib.Contracts;

namespace MusicPlayerModule.ViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
    /// <summary>
    /// 按照歌名、专辑、歌手、自定义分类
    /// </summary>
    internal class DistributeMusicViewModel : BaseNotifyModel, IDisposable
    {
        public int SelectedCount => this.DisplayFavorites.Count(item => item.IsDeleting);

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
        /// 是否显示批量删除按钮
        /// </summary>
        public bool CanBatchSelect
        {
            get { return _canBatchSelect; }
            set { SetProperty<bool>(ref _canBatchSelect, value); }
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
        #endregion

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
                    if (this.MusicAlbumFavorites.Count == 0)
                    {
                        return;
                    }

                    this.DisplayMusicAlbumFavorites.Clear();

                    if (!_albumMusicFilteKeyWords.IsNullOrBlank())
                    {
                        this.DisplayMusicAlbumFavorites.AddRange(this.MusicAlbumFavorites.Where(item =>
                            item.ClassifyKey.ContainsIgnoreCase(this._albumMusicFilteKeyWords)));
                    }
                    else
                    {
                        this.DisplayMusicAlbumFavorites.AddRange(this.MusicAlbumFavorites);
                    }
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
                    if (this.MusicSingerFavorites.Count == 0)
                    {
                        return;
                    }

                    this.DisplayMusicSingerFavorites.Clear();

                    if (!_singerMusicFilteKeyWords.IsNullOrBlank())
                    {
                        this.DisplayMusicSingerFavorites.AddRange(this.MusicSingerFavorites.Where(item =>
                            item.ClassifyKey.ContainsIgnoreCase(this._singerMusicFilteKeyWords)));
                    }
                    else
                    {
                        this.DisplayMusicSingerFavorites.AddRange(this.MusicSingerFavorites);
                    }
                }
            }
        }

        /// <summary>
        /// 为新目录创建对象
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool TryAddNewDirItemFromPath(string path)
        {
            if (!this.MusicDirs.Any(item => item.DirPath == path))
            {
                var current = new MusicWithClassifyModel(path, new ObservableCollection<FavoriteMusicViewModel>(),
                    MusicClassifyType.Dir);
                current.IsSelected = true;

                current.AddTo(MusicDirFavorites);

                return true;
            }

            return false;
        }

        private bool MoveMusicsTo(MusicWithClassifyModel originClassify, string targetDir)
        {
            TryAddNewDirItemFromPath(targetDir);

            var originDir = originClassify.ClassifyKey;
            var originColls = originClassify.DisplayByClassifyKeyFavorites;

            var targetColls = this.MusicDirFavorites.First(item => item.ClassifyKey == targetDir)
                                    .DisplayByClassifyKeyFavorites;

            var newColls = originColls.SkipWhile(item => targetColls.Any(m => m.Music.Name == item.Music.Name))
                                .ToList();

            if (newColls.Any())
            {
                foreach (var favorite in newColls)
                {
                    favorite.AddTo(targetColls);

                    favorite.Music.MoveTo(targetDir);

                    favorite.RemoveFrom(originColls);
                }

                this.TryRemoveMusicWithClassifyModel(originDir);

                return true;
            }
            else
            {
                CommonUtil.PublishMessage(_eventAggregator, "目标目录已包含源目录所有内容，不允许重复");

                return false;
            }
        }

        /// <summary>
        /// 删除空文件夹
        /// </summary>
        /// <param name="originDir"></param>
        private void TryRemoveMusicWithClassifyModel(string originDir)
        {
            if (Directory.Exists(originDir))
            {
                if (originDir.DeleteIfEmptyOr())
                {
                    CommonUtil.PublishMessage(_eventAggregator, $"【{originDir}】删除成功");
                }
                else
                {
                    CommonUtil.PublishMessage(_eventAggregator, "目录不为空，不允许删除");
                }
            }
        }

        private readonly IEventAggregator _eventAggregator;

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

            FavoriteMusicViewModel.DeleteStatusChanged += newValue =>
            {
                if (newValue != SelectAlbumFavoriteAll)
                {
                    if (newValue)
                    {
                        var c = DisplayMusicAlbumFavorites.FirstOrDefault(c => c.IsSelected);

                        if (c == null || c.DisplayByClassifyKeyFavorites.Any(m => !m.IsDeleting))
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
                        var c = DisplayMusicSingerFavorites.FirstOrDefault(c => c.IsSelected);

                        if (c == null || c.DisplayByClassifyKeyFavorites.Any(m => !m.IsDeleting))
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
                        var c = MusicDirFavorites.FirstOrDefault(c => c.IsSelected);

                        if (c == null || c.DisplayByClassifyKeyFavorites.Any(m => !m.IsDeleting))
                        {
                            return;
                        }
                    }

                    _selectDirFavoriteAll = newValue;

                    RaisePropertyChanged(nameof(SelectDirFavoriteAll));
                }
            };

            this.MusicDirFavorites.CollectionChanged += (object? sender, NotifyCollectionChangedEventArgs e) =>
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

                music.RemoveFromAll();

                this.RefreshFavoriteIndex();

                this.OnlyRefreshClassifySelectAllStatus();

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

            this.BatchDeleteCommand = new DelegateCommand(() =>
            {
                if (this.IsInSong)
                {
                    var items = DisplayFavorites.Where(item => item.IsDeleting).ToArray();

                    for (int i = items.Length - 1; i >= 0; i--)
                    {
                        items[i].RemoveFromAll();
                    }

                    this.RefreshFavoriteIndex();
                }
                else if (this.IsInAlbum)
                {
                    var items = DisplayMusicAlbumFavorites
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
                else if (this.IsInSinger)
                {
                    var items = DisplayMusicSingerFavorites
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
                else if (IsInDir)
                {
                    var items = MusicDirFavorites.FirstOrDefault(i => i.IsSelected)?.DisplayByClassifyKeyFavorites
                                                     .Where(m => m.IsDeleting).ToArray();

                    if (items != null)
                    {
                        for (int i = items.Length - 1; i >= 0; i--)
                        {
                            items[i].RemoveFromAll();
                        }
                    }
                }

                this.OnlyRefreshClassifySelectAllStatus();

                this.SelectFavoriteAll = this.DisplayFavorites.Count > 0 && !this.DisplayFavorites.Any(m => !m.IsDeleting);

                if (this.DisplayFavorites.Count == 0)
                {
                    CanBatchSelect = false;
                }

                this.RaisePropertyChanged(nameof(SelectedCount));
            }, () => this.DisplayFavorites.Any(item => item.IsDeleting))
                .ObservesProperty(() => this.SelectedCount);

            this.AddToPlayingCommand = new DelegateCommand<MusicWithClassifyModel>(item =>
            {
                if (item == null || item.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
                {
                    CommonUtil.PublishMessage(_eventAggregator, $"音乐集合为空或不存在{item?.ClassifyKey}分类");
                    return;
                }

                _eventAggregator.GetEvent<BatchAddToPlayingEvent>().Publish(item.DisplayByClassifyKeyFavorites);
            });

            this.BatchMoveMusicDirCommand = new DelegateCommand<string>(originDir =>
            {
                if (originDir.IsNullOrBlank())
                {
                    CommonUtil.PublishMessage(_eventAggregator, "未传入源目录");
                    return;
                }

                var path = settingManager[CustomStatics.MUSIC].Value;

                var selectedPath = CommonCoreUtils.OpenFolderDialog(path);

                if (!selectedPath.IsNullOrBlank())
                {
                    this.TryAddNewDirItemFromPath(selectedPath);

                    if (this.MoveMusicsTo(MusicDirFavorites.First(item => item.ClassifyKey == originDir), selectedPath))
                    {
                        CommonUtil.PublishMessage(_eventAggregator, $"移动成功");
                    }
                }
            });

            this.AddMusicDirCommand = new DelegateCommand(() =>
            {
                var path = settingManager[CustomStatics.MUSIC].Value;

                var selectedPath = CommonCoreUtils.OpenFolderDialog(path);

                if (!selectedPath.IsNullOrBlank())
                {
                    if (this.TryAddNewDirItemFromPath(selectedPath))
                    {
                        CommonUtil.PublishMessage(_eventAggregator, $"{selectedPath}分类创建成功");
                    }
                    else
                    {
                        CommonUtil.PublishMessage(_eventAggregator, $"{selectedPath}分类之前已存在");
                    }
                }
            });

            this.RemoveMusicDirCommand = new DelegateCommand<string>(TryRemoveMusicWithClassifyModel);

            this.RenameMusicDirCommand = new DelegateCommand<MusicWithClassifyModel>(item =>
            {
                if (item == null)
                {
                    CommonUtil.PublishMessage(_eventAggregator, $"未选中{item?.ClassifyKey}分类");
                    return;
                }

                var path = settingManager[CustomStatics.MUSIC].Value;
                var selectedPath = CommonCoreUtils.OpenFolderDialog(path);

                if (!selectedPath.IsNullOrBlank())
                {
                    if (this.TryAddNewDirItemFromPath(selectedPath))
                    {
                        if (this.MoveMusicsTo(item, selectedPath))
                        {
                            CommonUtil.PublishMessage(_eventAggregator, $"目录重命名成功");
                        }
                    }
                    else
                    {
                        CommonUtil.PublishMessage(_eventAggregator, $"{item.ClassifyKey}分类之前已存在");
                    }
                }
            });

            this.DistributeByDirectoryCommand = new DelegateCommand(() =>
            {
                TryLoadItemsToCollection(null, MusicDirFavorites, item => item.Music.FileDir ?? "未知目录", MusicClassifyType.Dir);
            });

            this.DistributeBySingerCommand = new DelegateCommand(() =>
            {
                TryLoadItemsToCollection(DisplayMusicSingerFavorites, MusicSingerFavorites, item => item.Music.Singer ?? "未知歌手", MusicClassifyType.Singer);
            });

            this.DistributeByAlbumCommand = new DelegateCommand(() =>
            {
                TryLoadItemsToCollection(DisplayMusicAlbumFavorites, MusicAlbumFavorites, item => item.Music.Album ?? "未知专辑", MusicClassifyType.Album);
            });

            void TryLoadItemsToCollection(Collection<MusicWithClassifyModel> display, Collection<MusicWithClassifyModel> classifys, Func<FavoriteMusicViewModel, string> keySelector, MusicClassifyType musicClassifyType)
            {
                if (classifys.IsNullOrEmpty())
                {
                    IEnumerable<IGrouping<string, FavoriteMusicViewModel>> groups = Favorites.GroupBy<FavoriteMusicViewModel, string>(keySelector);

                    foreach (var group in groups)
                    {
                        var classify = new MusicWithClassifyModel(
                                    group.Key,
                                    new ObservableCollection<FavoriteMusicViewModel>(),
                                    musicClassifyType
                                );

                        foreach (var item in group)
                        {
                            item.AddTo(classify.DisplayByClassifyKeyFavorites);
                        }

                        if (display != null)
                        {
                            classify.AddTo(display);
                        }

                        classify.AddTo(classifys);
                    }

                    classifys.First().IsSelected = true;
                }
            }


            this.DistributeToCommand = new DelegateCommand<MusicMoveModel>(moveModel =>
            {
                if (moveModel.IsNullOr(m => m.Music == null))
                {
                    return;
                }

                var originDir = moveModel.Music.FileDir;
                var targetDir = moveModel.MoveToDir;

                if (originDir.EqualsIgnoreCase(targetDir))
                {
                    CommonUtil.PublishMessage(_eventAggregator, $"源目录和目标目录不允许相同");
                    return;
                }

                if (this.MusicDirFavorites.IsNullOrEmpty())
                {
                    this.DistributeByDirectoryCommand.Execute(null);
                }

                if (moveModel.Music.MoveTo(moveModel.MoveToDir))
                {
                    var originItem = this.MusicDirFavorites.First(item => item.ClassifyKey == originDir);

                    var originCollection = originItem.DisplayByClassifyKeyFavorites;
                    var targetCollection = this.MusicDirFavorites.First(item => item.ClassifyKey == targetDir)
                        .DisplayByClassifyKeyFavorites;

                    var item = originCollection.FirstOrDefault(item => item.Music == moveModel.Music);
                    if (item != null)
                    {
                        item.RemoveFrom(originCollection);

                        if (originCollection.IsNullOrEmpty())
                        {
                            this.TryRemoveMusicWithClassifyModel(originDir);
                        }

                        item.AddTo(targetCollection);

                        CommonUtil.PublishMessage(_eventAggregator, $"【{moveModel.Music.Name}】移动成功");
                    }
                    else
                    {
                        CommonUtil.PublishMessage(_eventAggregator, $"源目录不存在【{moveModel.Music.Name}】");
                    }
                }
            });
        }

        internal void AddNewMusic(FavoriteMusicViewModel musicModel)
        {
            TryAddNewMusic(MusicDirFavorites, item => item.ClassifyKey == musicModel.Music.FileDir, musicModel.Music.FileDir, MusicClassifyType.Dir);

            TryAddNewMusic(MusicSingerFavorites, item => item.ClassifyKey == musicModel.Music.Singer, musicModel.Music.Singer, MusicClassifyType.Singer);

            TryAddNewMusic(MusicAlbumFavorites, item => item.ClassifyKey == musicModel.Music.Album, musicModel.Music.Album, MusicClassifyType.Album);

            void TryAddNewMusic(Collection<MusicWithClassifyModel> collection, Func<MusicWithClassifyModel, bool> selector, string classifyKey, MusicClassifyType classifyType)
            {
                if (collection.Count > 0)
                {
                    var classifyModel =
                        collection.FirstOrDefault(selector);


                    if (classifyModel == null)
                    {
                        classifyModel = new MusicWithClassifyModel(classifyKey,
                                                                new ObservableCollection<FavoriteMusicViewModel>(),
                                                                classifyType
                                                        );
                    }

                    musicModel.AddTo(classifyModel.DisplayByClassifyKeyFavorites);

                    classifyModel.AddTo(MusicDirFavorites);
                }
            }
        }

        internal void OnlyRefreshClassifySelectAllStatus(MusicWithClassifyModel current = null)
        {
            var newValue = IsCollectionSelectedAll(this.DisplayMusicAlbumFavorites);
            if (newValue != SelectAlbumFavoriteAll)
            {
                if (newValue && (current ??= DisplayMusicAlbumFavorites.First(item => item.IsSelected)).DisplayByClassifyKeyFavorites.Any(m => !m.IsDeleting))
                {
                    return;
                }

                this._selectAlbumFavoriteAll = newValue;
                RaisePropertyChanged(nameof(SelectAlbumFavoriteAll));
            }

            newValue = IsCollectionSelectedAll(this.DisplayMusicSingerFavorites);
            if (newValue != SelectSingerFavoriteAll)
            {
                if (newValue && (current ??= DisplayMusicSingerFavorites.First(item => item.IsSelected)).DisplayByClassifyKeyFavorites.Any(m => !m.IsDeleting))
                {
                    return;
                }

                this._selectSingerFavoriteAll = newValue;
                RaisePropertyChanged(nameof(SelectSingerFavoriteAll));
            }

            newValue = IsCollectionSelectedAll(this.MusicDirFavorites);
            if (newValue != SelectDirFavoriteAll)
            {
                if (newValue && (current ??= MusicDirFavorites.First(item => item.IsSelected)).DisplayByClassifyKeyFavorites.Any(m => !m.IsDeleting))
                {
                    return;
                }

                this._selectDirFavoriteAll = newValue;
                RaisePropertyChanged(nameof(SelectDirFavoriteAll));
            }

            bool IsCollectionSelectedAll(ICollection<MusicWithClassifyModel> collection)
            {
                return collection.Count > 0 &&
                        collection.Any(item => item.IsSelected) &&
                        !(current ??= collection.First(item => item.IsSelected))
                            .DisplayByClassifyKeyFavorites.Any(item => !item.IsDeleting);
            }
        }

        internal void RefreshFavoriteIndex()
        {
            for (int i = 0; i < DisplayFavorites.Count; i++)
            {
                DisplayFavorites[i].Index = i + 1;
            }
        }

        #region Fields
        private bool _canBatchSelect;
        private bool _selectFavoriteAll;

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
        public Collection<FavoriteMusicViewModel> Favorites { get; private set; } = new();
        public ObservableCollection<FavoriteMusicViewModel> DisplayFavorites { get; private set; } = new();

        public Collection<MusicWithClassifyModel> MusicAlbumFavorites { get; private set; } = new();
        public ObservableCollection<MusicWithClassifyModel> DisplayMusicAlbumFavorites { get; private set; } = new();

        public Collection<MusicWithClassifyModel> MusicSingerFavorites { get; private set; } = new();
        public ObservableCollection<MusicWithClassifyModel> DisplayMusicSingerFavorites { get; private set; } = new();

        public ObservableCollection<MusicWithClassifyModel> MusicDirFavorites { get; private set; } = new();

        public ObservableCollection<MusicDirModel> MusicDirs { get; private set; } = new();

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
            get { return _selectAlbumFavoriteAll; }
            set
            {
                if (SetProperty<bool>(ref _selectAlbumFavoriteAll, value))
                {
                    var current = this.DisplayMusicAlbumFavorites.FirstOrDefault(item => item.IsSelected);

                    current?.DisplayByClassifyKeyFavorites.ForEach(m => m.IsDeleting = value);
                }
            }
        }

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
            get { return _selectSingerFavoriteAll; }
            set
            {
                if (SetProperty<bool>(ref _selectSingerFavoriteAll, value))
                {
                    var current = this.DisplayMusicSingerFavorites.FirstOrDefault(item => item.IsSelected);

                    current?.DisplayByClassifyKeyFavorites.ForEach(m => m.IsDeleting = value);
                }
            }
        }

        public bool IsInDir
        {
            get => _isInDir;
            set => SetProperty(ref _isInDir, value);
        }

        public bool SelectDirFavoriteAll
        {
            get { return _selectDirFavoriteAll; }
            set
            {
                if (SetProperty<bool>(ref _selectDirFavoriteAll, value))
                {
                    var current = this.MusicDirFavorites.FirstOrDefault(item => item.IsSelected);

                    current?.DisplayByClassifyKeyFavorites.ForEach(m => m.IsDeleting = value);
                }
            }
        }

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

        public ICommand AddToPlayingCommand { get; private set; }

        public ICommand BatchMoveMusicDirCommand { get; private set; }
        public ICommand AddMusicDirCommand { get; private set; }
        public ICommand RemoveMusicDirCommand { get; private set; }
        public ICommand RenameMusicDirCommand { get; private set; }

        /// <summary>
        /// 按照来源目录分类收藏列表
        /// </summary>
        public ICommand DistributeByDirectoryCommand { get; private set; }

        /// <summary>
        /// 按照歌手分类收藏列表
        /// </summary>
        public ICommand DistributeBySingerCommand { get; private set; }

        /// <summary>
        /// 按照专辑分类收藏列表
        /// </summary>
        public ICommand DistributeByAlbumCommand { get; private set; }

        /// <summary>
        /// 添加到..分类
        /// </summary>
        public ICommand DistributeToCommand { get; private set; }

        public ICommand CurrentDirSelectAllCommand { get; private set; }
        public ICommand CurrentSingerSelectAllCommand { get; private set; }
        public ICommand CurrentAlbumSelectAllCommand { get; private set; }

        #endregion

        public void Dispose()
        {
            foreach (var item in this.Favorites)
            {
                item.Dispose();
            }

            this.DisplayFavorites.Clear();
            this.DisplayFavorites = null;

            this.Favorites.Clear();
            this.Favorites = null;


            this.DisplayMusicAlbumFavorites.Clear();
            this.DisplayMusicAlbumFavorites = null;

            this.MusicAlbumFavorites.Clear();
            this.MusicAlbumFavorites = null;

            this.DisplayMusicSingerFavorites.Clear();
            this.DisplayMusicSingerFavorites = null;

            this.MusicSingerFavorites.Clear();
            this.MusicSingerFavorites = null;

            this.MusicDirFavorites.Clear();
            this.MusicDirFavorites = null;
        }
    }
}
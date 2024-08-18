using MusicPlayerModule.Common;
using MusicPlayerModule.Models;
using MusicPlayerModule.MsgEvents;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using IceTea.Atom.Extensions;
using System.IO;
using IceTea.Atom.Utils;
using PrismAppBasicLib.MsgEvents;
using IceTea.Atom.Contracts;
using IceTea.Wpf.Core.Utils;

namespace MusicPlayerModule.ViewModels
{
    internal class DistributeMusicViewModel : BindableBase, IDisposable
    {
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

                    if (!string.IsNullOrEmpty(_albumMusicFilteKeyWords))
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

                    if (!string.IsNullOrEmpty(_singerMusicFilteKeyWords))
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

        private bool TryAddNewDirItemFromPath(string path)
        {
            if (!this.MusicDirs.Any(item => item.DirPath == path))
            {
                var current = new MusicWithClassifyModel(path, new ObservableCollection<FavoriteMusicViewModel>(),
                    MusicClassifyType.Dir);
                current.IsSelected = true;

                this.MusicDirFavorites.Add(current);
                this.MusicDirs.Add(new MusicDirModel(path));

                PublishMessage($"新的分类【{current.ClassifyKey}】添加成功");

                return true;
            }

            PublishMessage($"【{path.GetCurrentDirName()}】分类已存在");

            return false;
        }

        private void MoveMusicsTo(MusicWithClassifyModel originClassify, string targetDir)
        {
            var originDir = originClassify.ClassifyKey;
            var originColls = originClassify.DisplayByClassifyKeyFavorites;

            var targetColls = this.MusicDirFavorites.First(item => item.ClassifyKey == targetDir)
                                    .DisplayByClassifyKeyFavorites;

            var newColls = originColls.SkipWhile(item => targetColls.Any(m => m.Music.Name == item.Music.Name))
                                .ToList();

            if (newColls.Any())
            {
                targetColls.AddRange(newColls);

                foreach (var favorite in newColls)
                {
                    favorite.Music.MoveTo(targetDir);

                    originColls.Remove(favorite);
                }

                this.TryRemoveMusicWithClassifyModel(originDir);

                PublishMessage($"文件移动成功");
            }
            else
            {
                PublishMessage("目标目录已包含源目录所有内容，不允许重复");
            }
        }

        private void MoveMusicsTo(string originDir, string targetDir)
        {
            MoveMusicsTo(this.MusicDirFavorites.First(item => item.ClassifyKey == originDir), targetDir);
        }

        /// <summary>
        /// 不允许删除音乐，所以只有空目录分类才会被移除
        /// </summary>
        /// <param name="originDir"></param>
        private void TryRemoveMusicWithClassifyModel(string originDir)
        {
            if (originDir.IsNullOrEmpty())
            {
                PublishMessage("未传入待删除目录");
                return;
            }

            var current = this.MusicDirFavorites.First(item => item.ClassifyKey == originDir);

            if (current.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
            {
                this.MusicDirFavorites.Remove(current);

                this.MusicDirs.RemoveAll(item => item.DirPath == originDir);

                if (Directory.Exists(originDir))
                {
                    originDir.DeleteIfEmptyOr();
                }

                PublishMessage($"【{current.ClassifyKey}】删除成功");
            }
            else
            {
                PublishMessage("目录不为空，不允许删除");
            }
        }

        internal event Action ClearFavoriteListFilteKeyWords;

        private readonly Collection<FavoriteMusicViewModel> _collection;
        private readonly IEventAggregator _eventAggregator;

        public DistributeMusicViewModel(Collection<FavoriteMusicViewModel> collection, IEventAggregator eventAggregator)
        {
            this._collection = collection.AssertNotNull(nameof(Collection<FavoriteMusicViewModel>));
            this._eventAggregator = eventAggregator.AssertNotNull(nameof(IEventAggregator));

            this.AddToPlayingCommand = new DelegateCommand<MusicWithClassifyModel>(item =>
            {
                if (item == null || item.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
                {
                    PublishMessage($"音乐集合为空或不存在{item?.ClassifyKey}分类");
                    return;
                }

                eventAggregator.GetEvent<BatchAddToPlayingEvent>().Publish(item.DisplayByClassifyKeyFavorites);
            });

            this.BatchMoveMusicDirCommand = new DelegateCommand<string>(originDir =>
            {
                if (originDir.IsNullOrEmpty())
                {
                    PublishMessage("未传入源目录");
                    return;
                }

                var selectedPath = CommonCoreUtils.OpenFolderDialog(CustomStatics.LastMusicDir);

                if (!selectedPath.IsNullOrEmpty())
                {
                    if (originDir.EqualsIgnoreCase(selectedPath))
                    {
                        PublishMessage("源目录和目标目录相同");
                        return;
                    }

                    this.TryAddNewDirItemFromPath(selectedPath);

                    this.MoveMusicsTo(originDir, selectedPath);
                }
            });

            this.AddMusicDirCommand = new DelegateCommand(() =>
            {
                var selectedPath = CommonCoreUtils.OpenFolderDialog(CustomStatics.LastMusicDir);

                if (!selectedPath.IsNullOrEmpty())
                {
                    this.TryAddNewDirItemFromPath(selectedPath);
                }
            });

            this.RemoveMusicDirCommand = new DelegateCommand<string>(dir =>
            {
                this.TryRemoveMusicWithClassifyModel(dir);
            });

            this.RenameMusicDirCommand = new DelegateCommand<MusicWithClassifyModel>(item =>
            {
                if (item == null || item.DisplayByClassifyKeyFavorites.Count == 0)
                {
                    PublishMessage($"未选中{item?.ClassifyKey}分类");
                    return;
                }

                var selectedPath = CommonCoreUtils.OpenFolderDialog(CustomStatics.LastMusicDir);

                if (!selectedPath.IsNullOrEmpty())
                {
                    if (!Directory.Exists(selectedPath))
                    {
                        PublishMessage($"目录【{selectedPath}】不存在");
                        return;
                    }

                    if (this.MusicDirFavorites.Any(music => music.ClassifyKey == selectedPath))
                    {
                        PublishMessage($"目录【{selectedPath}】已存在，请换个名字");
                        return;
                    }

                    this.TryAddNewDirItemFromPath(selectedPath);

                    this.MoveMusicsTo(item, selectedPath);
                }
            });

            this.DistributeBySongCommand = new DelegateCommand(() =>
            {
                if (this._collection.IsNullOrEmpty())
                {
                    return;
                }

                if (!this.AlbumMusicFilteKeyWords.IsNullOrEmpty())
                {
                    this.AlbumMusicFilteKeyWords = null;
                }

                if (!this.SingerMusicFilteKeyWords.IsNullOrEmpty())
                {
                    this.SingerMusicFilteKeyWords = null;
                }
            });

            this.DistributeByDirectoryCommand = new DelegateCommand(() =>
            {
                if (this._collection.IsNullOrEmpty())
                {
                    return;
                }

                this.ClearFavoriteListFilteKeyWords?.Invoke();

                if (!this.AlbumMusicFilteKeyWords.IsNullOrEmpty())
                {
                    this.AlbumMusicFilteKeyWords = null;
                }

                if (!this.SingerMusicFilteKeyWords.IsNullOrEmpty())
                {
                    this.SingerMusicFilteKeyWords = null;
                }

                if (this.MusicDirFavorites.IsNullOrEmpty())
                {
                    this.MusicDirFavorites.AddRange(
                        this._collection
                        .GroupBy<FavoriteMusicViewModel, string>(item => item.Music.FileDir ?? "未知目录")
                        .Select<IGrouping<string, FavoriteMusicViewModel>, MusicWithClassifyModel>(group =>
                            new MusicWithClassifyModel(
                                group.Key,
                                new ObservableCollection<FavoriteMusicViewModel>(group),
                                MusicClassifyType.Dir
                            )
                        )
                    );
                }
            });

            this.DistributeBySingerCommand = new DelegateCommand(() =>
            {
                if (this._collection.IsNullOrEmpty())
                {
                    return;
                }

                this.ClearFavoriteListFilteKeyWords?.Invoke();

                if (!this.AlbumMusicFilteKeyWords.IsNullOrEmpty())
                {
                    this.AlbumMusicFilteKeyWords = null;
                }

                if (this.MusicSingerFavorites.IsNullOrEmpty())
                {
                    this.MusicSingerFavorites.AddRange(this._collection
                        .GroupBy<FavoriteMusicViewModel, string>(item => item.Music.Singer ?? "未知歌手")
                        .Select<IGrouping<string, FavoriteMusicViewModel>, MusicWithClassifyModel>(group =>
                            new MusicWithClassifyModel(
                                group.Key,
                                new ObservableCollection<FavoriteMusicViewModel>(group),
                                MusicClassifyType.Singer
                            )
                        )
                    );

                    this.DisplayMusicSingerFavorites.AddRange(this.MusicSingerFavorites);

                    this.DisplayMusicSingerFavorites.First().IsSelected = true;
                }
            });

            this.DistributeByAlbumCommand = new DelegateCommand(() =>
            {
                if (this._collection.IsNullOrEmpty())
                {
                    return;
                }

                this.ClearFavoriteListFilteKeyWords?.Invoke();

                if (!this.SingerMusicFilteKeyWords.IsNullOrEmpty())
                {
                    this.SingerMusicFilteKeyWords = null;
                }

                if (this.MusicAlbumFavorites.IsNullOrEmpty())
                {
                    this.MusicAlbumFavorites.AddRange(this._collection
                        .GroupBy<FavoriteMusicViewModel, string>(item => item.Music.Album ?? "未知专辑")
                        .Select<IGrouping<string, FavoriteMusicViewModel>, MusicWithClassifyModel>(group =>
                            new MusicWithClassifyModel(
                                group.Key,
                                new ObservableCollection<FavoriteMusicViewModel>(group),
                                MusicClassifyType.Album
                            )
                        )
                    );

                    this.DisplayMusicAlbumFavorites.AddRange(this.MusicAlbumFavorites);

                    this.DisplayMusicAlbumFavorites.First().IsSelected = true;
                }
            });

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
                    PublishMessage($"源目录和目标目录不允许相同");
                    return;
                }

                if (this.MusicDirFavorites.IsNullOrEmpty())
                {
                    this.DistributeByDirectoryCommand.Execute(null);
                }

                if (moveModel.Music.MoveTo(moveModel.MoveToDir))
                {
                    var originItem = this.MusicDirFavorites.First(item => item.ClassifyKey == originDir);

                    var orginCollection = originItem.DisplayByClassifyKeyFavorites;
                    var targetCollection = this.MusicDirFavorites.First(item => item.ClassifyKey == targetDir)
                        .DisplayByClassifyKeyFavorites;

                    var item = orginCollection.FirstOrDefault(item => item.Music == moveModel.Music);
                    if (item != null)
                    {
                        orginCollection.Remove(item);

                        if (orginCollection.IsNullOrEmpty())
                        {
                            this.TryRemoveMusicWithClassifyModel(originDir);
                        }

                        targetCollection.Add(item);

                        PublishMessage($"【{moveModel.Music.Name}】移动成功");
                    }
                    else
                    {
                        PublishMessage($"源目录不存在【{moveModel.Music.Name}】");
                    }
                }
            });

            this.CurrentAlbumSelectAllCommand = new DelegateCommand<bool?>(isChecked =>
                {
                    if (isChecked != null)
                    {
                        var current = this.MusicAlbumFavorites.FirstOrDefault(item => item.IsSelected);

                        if (current != null)
                        {
                            foreach (var item in current.DisplayByClassifyKeyFavorites)
                            {
                                item.IsDeleting = (bool)isChecked;
                            }
                        }
                    }
                }, isChecked => this.DisplayMusicAlbumFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DisplayMusicAlbumFavorites.Count);

            this.CurrentSingerSelectAllCommand = new DelegateCommand<bool?>(isChecked =>
                {
                    if (isChecked != null)
                    {
                        var current = this.MusicSingerFavorites.FirstOrDefault(item => item.IsSelected);

                        if (current != null)
                        {
                            foreach (var item in current.DisplayByClassifyKeyFavorites)
                            {
                                item.IsDeleting = (bool)isChecked;
                            }
                        }
                    }
                }, isChecked => this.MusicSingerFavorites.Count > 0)
                .ObservesProperty<int>(() => this.DisplayMusicSingerFavorites.Count);

            this.CurrentDirSelectAllCommand = new DelegateCommand<bool?>(isChecked =>
            {
                if (isChecked != null)
                {
                    var current = this.MusicDirFavorites.FirstOrDefault(item => item.IsSelected);

                    if (current != null)
                    {
                        foreach (var item in current.DisplayByClassifyKeyFavorites)
                        {
                            item.IsDeleting = (bool)isChecked;
                        }
                    }
                }
            }, isChecked => this.MusicDirFavorites.Count > 0)
                .ObservesProperty<int>(() => this.MusicDirFavorites.Count);
        }

        private void PublishMessage(string msg, int seconds = 3)
        {
            _eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage(msg, seconds));
        }

        internal void CheckClassifySelectAllStatus()
        {
            this.SelectAlbumFavoriteAll = this.MusicAlbumFavorites.Count > 0 &&
                                          this.MusicAlbumFavorites.Any(item => item.IsSelected) &&
                                          !this.MusicAlbumFavorites.First(item => item.IsSelected)
                                               .DisplayByClassifyKeyFavorites.Any(item => !item.IsDeleting);

            this.SelectSingerFavoriteAll = this.MusicSingerFavorites.Count > 0 &&
                                           this.MusicSingerFavorites.Any(item => item.IsSelected) &&
                                           !this.MusicSingerFavorites.First(item => item.IsSelected)
                                               .DisplayByClassifyKeyFavorites.Any(item => !item.IsDeleting);

            this.SelectDirFavoriteAll = this.MusicDirFavorites.Count > 0 &&
                                        this.MusicDirFavorites.Any(item => item.IsSelected) &&
                                        !this.MusicDirFavorites.First(item => item.IsSelected)
                                               .DisplayByClassifyKeyFavorites.Any(item => !item.IsDeleting);
        }

        internal void AddNewMusic(FavoriteMusicViewModel musicModel)
        {
            this.MusicDirs.AddIfNotAnyWhile(
                model => musicModel.Music.FileDir == model.DirPath,
                () => new MusicDirModel(musicModel.Music.FileDir)
            );

            if (this.MusicDirFavorites.Count > 0)
            {
                var currentDirModel =
                    this.MusicDirFavorites.FirstOrDefault(item => item.ClassifyKey == musicModel.Music.FileDir);
                if (currentDirModel != null)
                {
                    currentDirModel.DisplayByClassifyKeyFavorites.AddIfItemNotWhileOrNotExists(musicModel);
                }
                else
                {
                    this.MusicDirFavorites.Add(new MusicWithClassifyModel(
                                                        musicModel.Music.FileDir,
                                                        new ObservableCollection<FavoriteMusicViewModel>(new[] { musicModel }),
                                                        MusicClassifyType.Dir
                                                    )
                                                );
                }
            }

            if (this.MusicSingerFavorites.Count > 0)
            {
                var currentSingerModel =
                    this.MusicSingerFavorites.FirstOrDefault(item => item.ClassifyKey == musicModel.Music.Singer);
                if (currentSingerModel != null)
                {
                    currentSingerModel.DisplayByClassifyKeyFavorites.Add(musicModel);
                }
                else
                {
                    this.MusicSingerFavorites.Add(new MusicWithClassifyModel(
                                                        musicModel.Music.Singer,
                                                        new ObservableCollection<FavoriteMusicViewModel>(new[] { musicModel }),
                                                        MusicClassifyType.Singer
                                                    )
                                                );
                }
            }

            if (this.MusicAlbumFavorites.Count > 0)
            {
                var currentAlbumModel =
                    this.MusicAlbumFavorites.FirstOrDefault(item => item.ClassifyKey == musicModel.Music.Album);
                if (currentAlbumModel != null)
                {
                    currentAlbumModel.DisplayByClassifyKeyFavorites.Add(musicModel);
                }
                else
                {
                    this.MusicAlbumFavorites.Add(new MusicWithClassifyModel(
                                                        musicModel.Music.Album,
                                                        new ObservableCollection<FavoriteMusicViewModel>(new[] { musicModel }),
                                                        MusicClassifyType.Album
                                                    )
                                                );
                }
            }
        }

        internal void DeleteAllMarkedMusic()
        {
            for (int i = this.DisplayMusicAlbumFavorites.Count - 1; i >= 0; i--)
            {
                var current = this.DisplayMusicAlbumFavorites[i];

                current.DisplayByClassifyKeyFavorites.RemoveAll(item => item.IsDeleting);

                if (current.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
                {
                    this.DisplayMusicAlbumFavorites.Remove(current);
                    this.MusicAlbumFavorites.Remove(current);
                }
            }

            for (int i = this.DisplayMusicSingerFavorites.Count - 1; i >= 0; i--)
            {
                var current = this.DisplayMusicSingerFavorites[i];
                current.DisplayByClassifyKeyFavorites.RemoveAll(item => item.IsDeleting);

                if (current.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
                {
                    this.DisplayMusicSingerFavorites.Remove(current);
                    this.MusicSingerFavorites.Remove(current);
                }
            }

            for (int i = this.MusicDirFavorites.Count - 1; i >= 0; i--)
            {
                var current = this.MusicDirFavorites[i];
                current.DisplayByClassifyKeyFavorites.RemoveAll(item => item.IsDeleting);

                if (current.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
                {
                    this.MusicDirs.RemoveAll(item => item.DirPath == current.ClassifyKey);
                    this.MusicDirFavorites.Remove(current);
                }
            }
        }

        internal void DeleteSingleMusic(FavoriteMusicViewModel music)
        {
            var current = this.MusicAlbumFavorites.FirstOrDefault(m => m.ClassifyKey == music.Music.Album);
            if (current.IsNotNullAnd(albumColl => albumColl.DisplayByClassifyKeyFavorites.Remove(music)))
            {
                if (current.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
                {
                    this.MusicAlbumFavorites.Remove(current);
                    this.DisplayMusicAlbumFavorites.Remove(current);
                }
            }

            current = this.MusicSingerFavorites.FirstOrDefault(m => m.ClassifyKey == music.Music.Singer);
            if (current.IsNotNullAnd(singerColl => singerColl.DisplayByClassifyKeyFavorites.Remove(music)))
            {
                if (current.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
                {
                    this.MusicSingerFavorites.Remove(current);
                    this.DisplayMusicSingerFavorites.Remove(current);
                }
            }

            current = this.MusicDirFavorites.FirstOrDefault(m => m.ClassifyKey == music.Music.FileDir);
            if (current.IsNotNullAnd(dirColl => dirColl.DisplayByClassifyKeyFavorites.Remove(music)))
            {
                if (current.DisplayByClassifyKeyFavorites.IsNullOrEmpty())
                {
                    this.MusicDirs.RemoveAll(item => item.DirPath == current.ClassifyKey);
                    this.MusicDirFavorites.Remove(current);
                }
            }
        }

        #region Fields

        private bool _selectAlbumFavoriteAll;
        private bool _selectSingerFavoriteAll;
        private bool _selectDirFavoriteAll;

        private string _albumMusicFilteKeyWords;
        private string _singerMusicFilteKeyWords;

        #endregion

        #region GenericProps

        public Collection<MusicWithClassifyModel> MusicAlbumFavorites { get; private set; } = new();
        public ObservableCollection<MusicWithClassifyModel> DisplayMusicAlbumFavorites { get; private set; } = new();
        public Collection<MusicWithClassifyModel> MusicSingerFavorites { get; private set; } = new();
        public ObservableCollection<MusicWithClassifyModel> DisplayMusicSingerFavorites { get; private set; } = new();

        public ObservableCollection<MusicWithClassifyModel> MusicDirFavorites { get; private set; } = new();

        public ObservableCollection<MusicDirModel> MusicDirs { get; private set; } = new();

        public bool SelectAlbumFavoriteAll
        {
            get { return _selectAlbumFavoriteAll; }
            set { SetProperty<bool>(ref _selectAlbumFavoriteAll, value); }
        }

        public bool SelectSingerFavoriteAll
        {
            get { return _selectSingerFavoriteAll; }
            set { SetProperty<bool>(ref _selectSingerFavoriteAll, value); }
        }

        public bool SelectDirFavoriteAll
        {
            get { return _selectDirFavoriteAll; }
            set { SetProperty<bool>(ref _selectDirFavoriteAll, value); }
        }

        #endregion

        #region Commands

        public ICommand AddToPlayingCommand { get; private set; }

        public ICommand BatchMoveMusicDirCommand { get; private set; }
        public ICommand AddMusicDirCommand { get; private set; }
        public ICommand RemoveMusicDirCommand { get; private set; }
        public ICommand RenameMusicDirCommand { get; private set; }

        /// <summary>
        /// 单曲
        /// </summary>
        public ICommand DistributeBySongCommand { get; private set; }

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


            this.CurrentAlbumSelectAllCommand = null;
            this.CurrentSingerSelectAllCommand = null;
            this.CurrentDirSelectAllCommand = null;


            this.DistributeByAlbumCommand = null;
            this.DistributeBySingerCommand = null;
            this.DistributeBySongCommand = null;
            this.DistributeByDirectoryCommand = null;

            this.DistributeToCommand = null;
        }
    }
}
using MusicPlayerModule.Common;
using MusicPlayerModule.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using WpfStyleResources.Helper;

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
                        this.DisplayMusicAlbumFavorites.AddRange(this.MusicAlbumFavorites.Where(item => item.ClassifyKey.Contains(this._albumMusicFilteKeyWords)));
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
                        this.DisplayMusicSingerFavorites.AddRange(this.MusicSingerFavorites.Where(item => item.ClassifyKey.Contains(this._singerMusicFilteKeyWords)));
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
                var current = new MusicWithClassifyModel(path, new ObservableCollection<FavoriteMusicViewModel>());
                current.IsSelected = true;

                this.MusicDirFavorites.Add(current);
                this.MusicDirs.Add(new MusicDirModel(path));

                return true;
            }

            return false;
        }

        private void MoveMusicsTo(string originDir, string targetDir)
        {
            var colls = this.MusicDirFavorites.First(item => item.ClassifyKey == originDir).DisplayByClassifyKeyFavorites;
            this.MusicDirFavorites.First(item => item.ClassifyKey == targetDir).DisplayByClassifyKeyFavorites.AddRange(colls);

            foreach (var favorite in colls)
            {
                favorite.Music.MoveTo(targetDir);
            }

            colls.Clear();

            this.TryRemoveMusicWithClassifyModel(originDir);
        }

        /// <summary>
        /// 不允许删除音乐，所以只有空目录分类才会被移除
        /// </summary>
        /// <param name="originDir"></param>
        private void TryRemoveMusicWithClassifyModel(string originDir)
        {
            var current = this.MusicDirFavorites.First(item => item.ClassifyKey == originDir);

            if (current.DisplayByClassifyKeyFavorites.Count == 0)
            {
                this.MusicDirFavorites.Remove(current);

                this.MusicDirs.Remove(this.MusicDirs.First(item => item.DirPath == originDir));

                if (!Directory.EnumerateFileSystemEntries(originDir).Any())
                {
                    Directory.Delete(originDir);
                }
            }
        }

        internal event Action ClearFavoriteListFilteKeyWords;

        private readonly Collection<FavoriteMusicViewModel> _collection;

        public DistributeMusicViewModel(Collection<FavoriteMusicViewModel> collection)
        {
            this._collection = collection;

            this.BatchMoveMusicDirCommand = new DelegateCommand<string>(originDir =>
            {
                var selectedPath = CommonUtils.OpenFolderDialog(AppStatics.LastMusicDir);

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    this.TryAddNewDirItemFromPath(selectedPath);

                    this.MoveMusicsTo(originDir, selectedPath);
                }
            });

            this.AddMusicDirCommand = new DelegateCommand(() =>
            {
                var selectedPath = CommonUtils.OpenFolderDialog(AppStatics.LastMusicDir);

                if (!string.IsNullOrEmpty(selectedPath))
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
                    return;
                }

                var selectedPath = CommonUtils.OpenFolderDialog(AppStatics.LastMusicDir);

                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (!this.MusicDirFavorites.Any(music => music.ClassifyKey == selectedPath))
                    {
                        this.TryAddNewDirItemFromPath(selectedPath);

                        this.MoveMusicsTo(item.ClassifyKey, selectedPath);
                    }
                }
            });

            this.DistributeBySongCommand = new DelegateCommand(() =>
            {
                if (this._collection.Count == 0)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(this.AlbumMusicFilteKeyWords))
                {
                    this.AlbumMusicFilteKeyWords = null;
                }

                if (!string.IsNullOrEmpty(this.SingerMusicFilteKeyWords))
                {
                    this.SingerMusicFilteKeyWords = null;
                }
            });

            this.DistributeByDirectoryCommand = new DelegateCommand(() =>
            {
                if (this._collection.Count == 0)
                {
                    return;
                }

                this.ClearFavoriteListFilteKeyWords?.Invoke();

                if (!string.IsNullOrEmpty(this.AlbumMusicFilteKeyWords))
                {
                    this.AlbumMusicFilteKeyWords = null;
                }

                if (!string.IsNullOrEmpty(this.SingerMusicFilteKeyWords))
                {
                    this.SingerMusicFilteKeyWords = null;
                }

                if (this.MusicDirFavorites.Count == 0)
                {
                    this.MusicDirFavorites.AddRange(this._collection
                                                                .GroupBy<FavoriteMusicViewModel, string>(item => item.Music.FileDir ?? "未知目录")
                                                                .Select<IGrouping<string, FavoriteMusicViewModel>, MusicWithClassifyModel>(group =>
                                                                        new MusicWithClassifyModel(
                                                                            group.Key,
                                                                            new ObservableCollection<FavoriteMusicViewModel>().AddRange(group)
                                                                        )
                                                                )
                                                            );
                }
            });

            this.DistributeBySingerCommand = new DelegateCommand(() =>
            {
                if (this._collection.Count == 0)
                {
                    return;
                }

                this.ClearFavoriteListFilteKeyWords?.Invoke();

                if (!string.IsNullOrEmpty(this.AlbumMusicFilteKeyWords))
                {
                    this.AlbumMusicFilteKeyWords = null;
                }

                if (this.MusicSingerFavorites.Count == 0)
                {
                    this.MusicSingerFavorites.AddRange(this._collection
                                                                .GroupBy<FavoriteMusicViewModel, string>(item => item.Music.Singer ?? "未知歌手")
                                                                .Select<IGrouping<string, FavoriteMusicViewModel>, MusicWithClassifyModel>(group =>
                                                                        new MusicWithClassifyModel(
                                                                            group.Key,
                                                                            new ObservableCollection<FavoriteMusicViewModel>().AddRange(group)
                                                                        )
                                                                )
                                                            );

                    this.DisplayMusicSingerFavorites.AddRange(this.MusicSingerFavorites);

                    this.DisplayMusicSingerFavorites.First().IsSelected = true;
                }
            });

            this.DistributeByAlbumCommand = new DelegateCommand(() =>
            {
                if (this._collection.Count == 0)
                {
                    return;
                }

                this.ClearFavoriteListFilteKeyWords?.Invoke();

                if (!string.IsNullOrEmpty(this.SingerMusicFilteKeyWords))
                {
                    this.SingerMusicFilteKeyWords = null;
                }

                if (this.MusicAlbumFavorites.Count == 0)
                {
                    this.MusicAlbumFavorites.AddRange(this._collection
                                                                .GroupBy<FavoriteMusicViewModel, string>(item => item.Music.Album ?? "未知专辑")
                                                                .Select<IGrouping<string, FavoriteMusicViewModel>, MusicWithClassifyModel>(group =>
                                                                        new MusicWithClassifyModel(
                                                                            group.Key,
                                                                            new ObservableCollection<FavoriteMusicViewModel>().AddRange(group)
                                                                        )
                                                                )
                                                            );

                    this.DisplayMusicAlbumFavorites.AddRange(this.MusicAlbumFavorites);

                    this.DisplayMusicAlbumFavorites.First().IsSelected = true;
                }
            });

            this.DistributeToCommand = new DelegateCommand<MusicMoveModel>(moveModel =>
            {
                var orginPath = moveModel.Music.FilePath;
                var originDir = moveModel.Music.FileDir;
                var targetDir = moveModel.MoveToDir;

                if (originDir.Equals(targetDir, StringComparison.CurrentCultureIgnoreCase))
                {
                    return;
                }

                if (this.MusicDirFavorites.Count == 0)
                {
                    this.DistributeByDirectoryCommand.Execute(null);
                }

                if (moveModel.Music.MoveTo(moveModel.MoveToDir))
                {
                    var originItem = this.MusicDirFavorites.First(item => item.ClassifyKey == originDir);

                    var orginCollection = originItem.DisplayByClassifyKeyFavorites;
                    var targetCollection = this.MusicDirFavorites.First(item => item.ClassifyKey == targetDir).DisplayByClassifyKeyFavorites;

                    var item = orginCollection.FirstOrDefault(item => item.Music == moveModel.Music);
                    if (item != null)
                    {
                        orginCollection.Remove(item);

                        if (orginCollection.Count == 0)
                        {
                            this.TryRemoveMusicWithClassifyModel(originDir);
                        }
                    }

                    targetCollection.Add(item);
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
            }, isChecked => this.DisplayMusicAlbumFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayMusicAlbumFavorites.Count);

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
            }, isChecked => this.MusicSingerFavorites.Count > 0).ObservesProperty<int>(() => this.DisplayMusicSingerFavorites.Count);

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
            }, isChecked => this.MusicDirFavorites.Count > 0).ObservesProperty<int>(() => this.MusicDirFavorites.Count);
        }

        internal void CheckClassifySelectAllStatus()
        {
            this.SelectAlbumFavoriteAll = this.MusicAlbumFavorites.Count > 0 && this.MusicAlbumFavorites.Any(item => item.IsSelected) && !this.MusicAlbumFavorites.First(item => item.IsSelected).DisplayByClassifyKeyFavorites.Any(item => !item.IsDeleting);

            this.SelectSingerFavoriteAll = this.MusicSingerFavorites.Count > 0 && this.MusicSingerFavorites.Any(item => item.IsSelected) && !this.MusicSingerFavorites.First(item => item.IsSelected).DisplayByClassifyKeyFavorites.Any(item => !item.IsDeleting);

            this.SelectDirFavoriteAll = this.MusicDirFavorites.Count > 0 && this.MusicDirFavorites.Any(item => item.IsSelected) && !this.MusicDirFavorites.First(item => item.IsSelected).DisplayByClassifyKeyFavorites.Any(item => !item.IsDeleting);
        }

        internal void AddNewMusic(FavoriteMusicViewModel musicModel)
        {
            if (!this.MusicDirs.Any(item => item.DirPath == musicModel.Music.FileDir))
            {
                this.MusicDirs.Add(new MusicDirModel(musicModel.Music.FileDir));
            }

            if (this.MusicDirFavorites.Count > 0)
            {
                var currentDirModel = this.MusicDirFavorites.FirstOrDefault(item => item.ClassifyKey == musicModel.Music.FileDir);
                if (currentDirModel != null)
                {
                    currentDirModel.DisplayByClassifyKeyFavorites.Add(musicModel);
                }
                else
                {
                    this.MusicDirFavorites.Add(new MusicWithClassifyModel(musicModel.Music.FileDir, new ObservableCollection<FavoriteMusicViewModel>(new[] { musicModel })));
                    this.MusicDirs.Add(new MusicDirModel(musicModel.Music.FileDir));
                }
            }

            if (this.MusicSingerFavorites.Count > 0)
            {
                var currentSingerModel = this.MusicSingerFavorites.FirstOrDefault(item => item.ClassifyKey == musicModel.Music.Singer);
                if (currentSingerModel != null)
                {
                    currentSingerModel.DisplayByClassifyKeyFavorites.Add(musicModel);
                }
                else
                {
                    this.MusicSingerFavorites.Add(new MusicWithClassifyModel(musicModel.Music.Singer, new ObservableCollection<FavoriteMusicViewModel>(new[] { musicModel })));
                }
            }

            if (this.MusicAlbumFavorites.Count > 0)
            {
                var currentAlbumModel = this.MusicAlbumFavorites.FirstOrDefault(item => item.ClassifyKey == musicModel.Music.Album);
                if (currentAlbumModel != null)
                {
                    currentAlbumModel.DisplayByClassifyKeyFavorites.Add(musicModel);
                }
                else
                {
                    this.MusicAlbumFavorites.Add(new MusicWithClassifyModel(musicModel.Music.Album, new ObservableCollection<FavoriteMusicViewModel>(new[] { musicModel })));
                }
            }
        }

        internal void DeleteAllMarkedMusic()
        {
            for (int i = this.DisplayMusicAlbumFavorites.Count - 1; i >= 0; i--)
            {
                var current = this.DisplayMusicAlbumFavorites[i];
                for (int j = current.DisplayByClassifyKeyFavorites.Count - 1; j >= 0; j--)
                {
                    if (current.DisplayByClassifyKeyFavorites[j].IsDeleting)
                    {
                        current.DisplayByClassifyKeyFavorites.RemoveAt(j);
                    }
                }

                if (current.DisplayByClassifyKeyFavorites.Count == 0)
                {
                    this.DisplayMusicAlbumFavorites.Remove(current);
                    this.MusicAlbumFavorites.Remove(current);
                }
            }

            for (int i = this.DisplayMusicSingerFavorites.Count - 1; i >= 0; i--)
            {
                var current = this.DisplayMusicSingerFavorites[i];
                for (int j = current.DisplayByClassifyKeyFavorites.Count - 1; j >= 0; j--)
                {
                    if (current.DisplayByClassifyKeyFavorites[j].IsDeleting)
                    {
                        current.DisplayByClassifyKeyFavorites.RemoveAt(j);
                    }
                }

                if (current.DisplayByClassifyKeyFavorites.Count == 0)
                {
                    this.DisplayMusicSingerFavorites.Remove(current);
                    this.MusicSingerFavorites.Remove(current);
                }
            }

            for (int i = this.MusicDirFavorites.Count - 1; i >= 0; i--)
            {
                var current = this.MusicDirFavorites[i];

                for (int j = current.DisplayByClassifyKeyFavorites.Count - 1; j >= 0; j--)
                {
                    if (current.DisplayByClassifyKeyFavorites[j].IsDeleting)
                    {
                        current.DisplayByClassifyKeyFavorites.RemoveAt(j);
                    }
                }

                if (current.DisplayByClassifyKeyFavorites.Count == 0)
                {
                    this.MusicDirs.Remove(this.MusicDirs.First(item => item.DirPath == current.ClassifyKey));
                    this.MusicDirFavorites.Remove(current);
                }
            }
        }

        internal void DeleteSingleMusic(FavoriteMusicViewModel music)
        {
            for (int i = 0; i < this.MusicAlbumFavorites.Count; i++)
            {
                if (this.MusicAlbumFavorites[i].DisplayByClassifyKeyFavorites.Remove(music))
                {
                    this.DisplayMusicAlbumFavorites[i].DisplayByClassifyKeyFavorites.Remove(music);
                    if (this.MusicAlbumFavorites[i].DisplayByClassifyKeyFavorites.Count == 0)
                    {
                        this.MusicAlbumFavorites.RemoveAt(i);
                        this.DisplayMusicAlbumFavorites.RemoveAt(i);
                    }
                    break;
                }
            }

            for (int i = 0; i < this.MusicSingerFavorites.Count; i++)
            {
                if (this.MusicSingerFavorites[i].DisplayByClassifyKeyFavorites.Remove(music))
                {
                    this.DisplayMusicSingerFavorites[i].DisplayByClassifyKeyFavorites.Remove(music);
                    if (this.MusicSingerFavorites[i].DisplayByClassifyKeyFavorites.Count == 0)
                    {
                        this.MusicSingerFavorites.RemoveAt(i);
                        this.DisplayMusicSingerFavorites.RemoveAt(i);
                    }
                    break;
                }
            }

            for (int i = 0; i < this.MusicDirFavorites.Count; i++)
            {
                if (this.MusicDirFavorites[i].DisplayByClassifyKeyFavorites.Remove(music))
                {
                    if (this.MusicDirFavorites[i].DisplayByClassifyKeyFavorites.Count == 0)
                    {
                        this.MusicDirs.Remove(this.MusicDirs.First(item => item.DirPath == this.MusicDirFavorites[i].ClassifyKey));
                        this.MusicDirFavorites.RemoveAt(i);
                    }
                    break;
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

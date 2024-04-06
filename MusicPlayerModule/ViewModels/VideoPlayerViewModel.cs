using MusicPlayerModule.Common;
using MusicPlayerModule.Models;
using MusicPlayerModule.Models.Common;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.MsgEvents.Video;
using MusicPlayerModule.MsgEvents.Video.Dtos;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using IceTea.General.Utils.AppHotKey;
using IceTea.General.Utils;
using IceTea.Wpf.Core.Contracts.MediaInfo;
using IceTea.Wpf.Core.Utils;
using IceTea.Atom.Contracts;

namespace MusicPlayerModule.ViewModels
{
    internal class VideoPlayerViewModel : BindableBase
    {
        private bool _running;

        public bool Running
        {
            get { return _running; }
            set
            {
                if (SetProperty<bool>(ref _running, value))
                {
                    this._eventAggregator.GetEvent<VideoProgreeTimerIsEnableUpdatedEvent>()
                        .Publish(new BoolAndGuid(value, this.Identity));
                }
            }
        }

        private bool _isEditingStretch;

        public bool IsEditingStretch
        {
            get => this._isEditingStretch;
            set => SetProperty<bool>(ref _isEditingStretch, value);
        }

        private Stretch _stretch;

        public Stretch Stretch
        {
            get => this._stretch;
            set { SetProperty<Stretch>(ref _stretch, value); IsEditingStretch = false; }
        }

        private PlayingVideoViewModel _currentVideo;

        public PlayingVideoViewModel CurrentVideo
        {
            get { return _currentVideo; }
            set
            {
                if (SetProperty<PlayingVideoViewModel>(ref _currentVideo, value))
                {
                    foreach (var item in this.DisplayPlaying)
                    {
                        item.IsPlayingVideo = false;
                    }

                    if (this._currentVideo != null)
                    {
                        _currentVideo.IsPlayingVideo = true;

                        if (!_currentVideo.LoadedABPoint)
                        {
                            if (int.TryParse(
                                    this._config.ReadConfigNode(new[]
                                    {
                                        CustomStatics.VIDEO, CustomStatics.VideoABPoints, _currentVideo.Video.Name,
                                        nameof(_currentVideo.PointAMills)
                                    }), out int mills))
                            {
                                _currentVideo.CurrentMills = mills;
                                _currentVideo.SetPointA(mills);
                            }

                            if (int.TryParse(
                                    this._config.ReadConfigNode(new[]
                                    {
                                        CustomStatics.VIDEO, CustomStatics.VideoABPoints, _currentVideo.Video.Name,
                                        nameof(_currentVideo.PointBMills)
                                    }), out mills))
                            {
                                _currentVideo.SetPointB(mills);
                            }

                            _currentVideo.LoadedABPoint = true;
                        }
                    }
                }
            }
        }

        public MediaOperationViewModel MediaOperationViewModel { get; } = new MediaOperationViewModel();

        private void RefreshMediaOperation(OperationType operationType)
        {
            this.MediaOperationViewModel.OperationType = operationType;

            this._eventAggregator.GetEvent<MediaOperationUpdatedEvent>().Publish(this.Identity);
        }

        public Guid Identity { get; private set; } = Guid.NewGuid();

        private void InitCommands()
        {
            this.OpenInExploreCommand = new DelegateCommand<string>(videoDir =>
            {
                if (videoDir == null)
                {
                    return;
                }

                Process.Start("explorer", videoDir);
            });

            this.PointACommand =
                new DelegateCommand(() => { this.CurrentVideo?.SetPointA(this.CurrentVideo.CurrentMills); }, () => this.CurrentVideo != null)
                    .ObservesProperty(() => this.CurrentVideo);

            this.PointBCommand =
                new DelegateCommand(() => { this.CurrentVideo?.SetPointB(this.CurrentVideo.CurrentMills); }, () => this.CurrentVideo != null)
                    .ObservesProperty(() => this.CurrentVideo);

            this.ResetPointABCommand =
                new DelegateCommand(() => { this.CurrentVideo?.ResetABPoint(); }, () => this.CurrentVideo != null)
                    .ObservesProperty(() => this.CurrentVideo);

            this.DeletePlayingCommand = new DelegateCommand<PlayingVideoViewModel>(video =>
            {
                if (video == null)
                {
                    return;
                }

                if (video == this.CurrentVideo)
                {
                    if (this.DisplayPlaying.Count > 1)
                    {
                        this.NextVideo(video);
                    }
                    else
                    {
                        this.CurrentVideo = null;
                        this.Running = false;
                    }
                }

                this.DisplayPlaying.Remove(video);

                this.RefreshPlayingIndex();
            });

            this.StopPlayVideoCommand = new DelegateCommand(() =>
                {
                    this.CurrentVideo = null;
                    this.Running = false;
                }, () => !this.Running && this.CurrentVideo != null).ObservesProperty(() => this.CurrentVideo)
                .ObservesProperty(() => this.Running);

            this.AddFilesCommand = new DelegateCommand(AddVideoFromFileDialog);

            this.AddFolderCommand = new DelegateCommand(AddVideoFromFolderDialog);

            this.DelayCommand =
                new DelegateCommand(() => { this.CurrentVideo.Rewind(); this.RefreshMediaOperation(OperationType.Rewind); },
                    () => this.CurrentVideo != null).ObservesProperty<PlayingVideoViewModel>(() => this.CurrentVideo);

            this.PrevCommand = new DelegateCommand<PlayingVideoViewModel>(
                    currentVideo =>
                    {
                        if (currentVideo != null)
                        {
                            if (currentVideo.Index > 1)
                            {
                                this.SetAndPlay(this.DisplayPlaying[currentVideo.Index - 2]);
                            }
                            else
                            {
                                this.SetAndPlay(this.DisplayPlaying[this.DisplayPlaying.Count - 1]);
                            }
                        }
                    },
                    currentVideo => this.CurrentVideo != null && this.DisplayPlaying.Count > 0)
                .ObservesProperty<PlayingVideoViewModel>(() => this.CurrentVideo)
                .ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.NextCommand = new DelegateCommand<PlayingVideoViewModel>(
                    currentVideo => this.NextVideo(currentVideo),
                    currentVideo => this.CurrentVideo != null && this.DisplayPlaying.Count > 0
                ).ObservesProperty<PlayingVideoViewModel>(() => this.CurrentVideo)
                .ObservesProperty<int>(() => this.DisplayPlaying.Count);

            this.AheadCommand =
                new DelegateCommand(() => { this.CurrentVideo.FastForward(); this.RefreshMediaOperation(OperationType.FastForward); },
                    () => this.CurrentVideo != null).ObservesProperty<PlayingVideoViewModel>(() => this.CurrentVideo);

            this.PlayPlayingCommand = new DelegateCommand<PlayingVideoViewModel>(video =>
            {
                if (video == null)
                {
                    return;
                }

                if (video == this.CurrentVideo)
                {
                    if (this.Running = !this.Running)
                    {
                        this._eventAggregator.GetEvent<ContinueCurrentVideo>().Publish(this.Identity);
                    }
                    else
                    {
                        this._eventAggregator.GetEvent<PauseCurrentVideo>().Publish(this.Identity);
                        this.RefreshMediaOperation(OperationType.Pause);
                    }
                }
                else
                {
                    this.SetAndPlay(video);
                }
            }, _ => this.CurrentVideo != null).ObservesProperty(() => this.CurrentVideo);

            this.CleanPlayingCommand = new DelegateCommand(() =>
                {
                    this.DisplayPlaying.Clear();
                    this.CurrentVideo = null;
                    this.Running = false;
                }, () => !this.Running && this.DisplayPlaying.Count > 0).ObservesProperty(() => this.Running)
                .ObservesProperty<int>(() => this.DisplayPlaying.Count);
        }

        private void RefreshPlayingIndex()
        {
            var index = 1;
            foreach (var item in this.DisplayPlaying)
            {
                item.Index = index++;
            }
        }

        private void SubscribeEvents()
        {
            PlayingVideoViewModel.ToNextVideo += dto =>
            {
                if (dto.Guid == this.Identity)
                {
                    if (this.CurrentPlayOrder != null)
                    {
                        switch (this.CurrentPlayOrder.OrderType)
                        {
                            case MediaPlayOrderModel.EnumOrderType.Order:
                                if (dto.Video.Index == this.DisplayPlaying.Count)
                                {
                                    this._eventAggregator.GetEvent<ResetVideoPlayerEvent>().Publish(this.Identity);
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
                                this.SetAndPlay(this.CurrentVideo);
                                return;
                            case MediaPlayOrderModel.EnumOrderType.SingleOnce:
                                this._eventAggregator.GetEvent<ResetVideoPlayerEvent>().Publish(this.Identity);
                                this.SetAndPlay(null);
                                return;
                            default:
                                throw new IndexOutOfRangeException();
                        }

                        this.NextVideo(dto.Video);
                    }
                }
            };
        }

        private void NextVideo(PlayingVideoViewModel currentVideo)
        {
            if (currentVideo != null)
            {
                if (currentVideo.Index < this.DisplayPlaying.Count)
                {
                    this.SetAndPlay(this.DisplayPlaying[currentVideo.Index]);
                }
                else
                {
                    this.SetAndPlay(this.DisplayPlaying.Count > 0 ? this.DisplayPlaying[0] : null);
                }
            }
        }

        private void SetAndPlay(PlayingVideoViewModel item)
        {
            this.CurrentVideo = item;

            if (this.Running = item != null)
            {
                // this.CurrentVideo.ResetABPoint();
                this._eventAggregator.GetEvent<ResetPlayerAndPlayVideoEvent>().Publish(this.Identity);
            }
        }

        private void AddVideoFromFileDialog()
        {
            OpenFileDialog openFileDialog =
                CommonUtils.OpenFileDialog(CustomStatics.LastVideoDir, new VideoMedia());

            if (openFileDialog != null)
            {
                this.LoadVideo(openFileDialog.FileNames);

                this.TryRefreshLastVideoDir(openFileDialog.FileName.GetParentPath());
            }
        }

        private void AddVideoFromFolderDialog()
        {
            var selectedPath = CommonUtils.OpenFolderDialog(CustomStatics.LastVideoDir);
            if (!selectedPath.IsNullOrEmpty())
            {
                this.TryRefreshLastVideoDir(selectedPath);

                var list = selectedPath.GetFiles(str => str.EndsWith(".mp4"));

                this.LoadVideo(list);
            }
        }

        private void TryRefreshLastVideoDir(string dir)
        {
            AppUtils.AssertDataValidation(dir.IsDirectoryPath(), $"{dir}必须为存在的目录");

            if (!dir.EqualsIgnoreCase(CustomStatics.LastVideoDir))
            {
                _config.WriteConfigNode(dir, CustomStatics.LastVideoDir_ConfigKey);

                CustomStatics.LastVideoDir = dir;
            }
        }

        private IEnumerable<string> GetNewFiles(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                if (!this.DisplayPlaying.Any(item => item.Video.FilePath == filePath))
                {
                    yield return filePath;
                }
            }
        }

        private void LoadVideo(IEnumerable<string> filePaths)
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

            foreach (var filePath in list)
            {
                this.DisplayPlaying.Add(new PlayingVideoViewModel(this._dto, new VideoModel(filePath)));
            }

            this.RefreshPlayingIndex();

            if (this.CurrentVideo is null)
            {
                this.SetAndPlay(this.DisplayPlaying.FirstOrDefault());
            }
        }

        private VideoModelAndGuid _dto;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigManager _config;

        public VideoPlayerViewModel(IEventAggregator eventAggregator, IConfigManager config, IAppHotKeyManager appHotKeyManager)
        {
            this._eventAggregator = eventAggregator.AssertNotNull(nameof(IEventAggregator));
            this._config = config.AssertNotNull(nameof(IConfigManager));

            this._dto = new VideoModelAndGuid(this.Identity);

            this.LoadConfig(config);

            this.InitCommands();
            this.InitHotkeys(config, appHotKeyManager);

            this.SubscribeEvents();
        }

        #region 应用内快捷键
        public Dictionary<string, AppHotKeyModel> KeyGestureDic { get; private set; }

        private void InitHotkeys(IConfigManager config, IAppHotKeyManager appHotKeyManager)
        {
            this.KeyGestureDic = HotKeyUtils.Provide(config, appHotKeyManager, new AppHotKeyGroup("视频", CustomStatics.AppVideoHotkeys, CustomStatics.VideoHotKeys));
        }
        #endregion

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

        private void LoadConfig(IConfigManager config)
        {
            

            var playOrder = config.ReadConfigNode(CustomStatics.VideoPlayOrder_ConfigKey);
            if (!string.IsNullOrEmpty(playOrder))
            {
                this.CurrentPlayOrder =
                    CustomStatics.MediaPlayOrderList.FirstOrDefault(item => item.Description == playOrder) ??
                    CustomStatics.MediaPlayOrderList.First();
            }

            var stretch = config.ReadConfigNode(CustomStatics.VideoStretch_ConfigKey);
            if (!string.IsNullOrEmpty(stretch) && Enum.TryParse<Stretch>(stretch, true, out Stretch result))
            {
                this.Stretch = result;
            }

            CustomStatics.LastVideoDir = config.ReadConfigNode(CustomStatics.LastVideoDir_ConfigKey);

            config.SetConfig += config =>
            {
                config.WriteConfigNode(this.CurrentPlayOrder.Description, CustomStatics.VideoPlayOrder_ConfigKey);
                config.WriteConfigNode(this.Stretch, CustomStatics.VideoStretch_ConfigKey);
            };

            config.PostSetConfig += config =>
            {
                foreach (var item in this.DisplayPlaying)
                {
                    config.WriteConfigNode(item.Video.Name, new[] { CustomStatics.VIDEO, CustomStatics.VideoABPoints, item.Video.Name });
                    config.WriteConfigNode(item.PointAMills,
                        new[] { CustomStatics.VIDEO, CustomStatics.VideoABPoints, item.Video.Name, nameof(item.PointAMills) });
                    config.WriteConfigNode(item.PointBMills,
                        new[] { CustomStatics.VIDEO, CustomStatics.VideoABPoints, item.Video.Name, nameof(item.PointBMills) });
                }
            };
        }

        public ObservableCollection<PlayingVideoViewModel> DisplayPlaying { get; private set; } = new();
        private Random _random = new Random();

        #region Commands

        public ICommand OpenInExploreCommand { get; private set; }

        public ICommand PointACommand { get; private set; }
        public ICommand PointBCommand { get; private set; }

        public ICommand ResetPointABCommand { get; private set; }

        public ICommand DeletePlayingCommand { get; private set; }

        public ICommand StopPlayVideoCommand { get; private set; }

        /// <summary>
        /// 从本地添加音乐到列表
        /// </summary>
        public ICommand AddFilesCommand { get; private set; }

        /// <summary>
        /// 从文件夹添加音乐到列表
        /// </summary>
        public ICommand AddFolderCommand { get; private set; }

        /// <summary>
        /// 播放或暂停
        /// </summary>
        public ICommand PlayPlayingCommand { get; set; }

        public ICommand PrevCommand { get; private set; }
        public ICommand NextCommand { get; private set; }

        /// <summary>
        /// 歌曲进度后退,此地只为让前端按钮在该禁用时禁用
        /// </summary>
        public ICommand DelayCommand { get; private set; }

        /// <summary>
        /// 歌曲进度提前,此地只为让前端按钮在该禁用时禁用
        /// </summary>
        public ICommand AheadCommand { get; private set; }

        /// <summary>
        /// 清空播放队列
        /// </summary>
        public ICommand CleanPlayingCommand { get; private set; }

        #endregion

        public void Dispose()
        {
            foreach (var item in this.DisplayPlaying)
            {
                item.Dispose();
            }

            this.DisplayPlaying.Clear();
            this.DisplayPlaying = null;


            this.PlayPlayingCommand = null;
            this.PrevCommand = null;
            this.NextCommand = null;

            this.CleanPlayingCommand = null;

            this.AddFilesCommand = null;
            this.AddFolderCommand = null;

            this.DelayCommand = null;
            this.AheadCommand = null;

            this.PointACommand = null;
            this.PointBCommand = null;
            this.ResetPointABCommand = null;
        }
    }
}
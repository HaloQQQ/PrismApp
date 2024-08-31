using MusicPlayerModule.Common;
using MusicPlayerModule.Models;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.MsgEvents.Video;
using MusicPlayerModule.MsgEvents.Video.Dtos;
using Prism.Commands;
using Prism.Events;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using IceTea.Atom.Extensions;
using IceTea.Atom.Contracts;
using IceTea.Wpf.Atom.Contracts.MediaInfo;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using IceTea.Wpf.Atom.Utils;
using IceTea.Wpf.Core.Utils;
using IceTea.Atom.Utils;
using MusicPlayerModule.ViewModels.Base;

namespace MusicPlayerModule.ViewModels
{
    internal class VideoPlayerViewModel : MediaPlayerViewModel
    {
        protected override string MediaType => "视频";
        protected override string[] MediaSettingNode => ["HotKeys", "App", "Video"];

        public override bool Running
        {
            get => _running;
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

        public override MediaBaseViewModel CurrentMedia
        {
            get => _currentMedia;
            set
            {
                if (SetProperty<MediaBaseViewModel>(ref _currentMedia, value) && value != null)
                {
                    foreach (var item in this.DisplayPlaying.Where(m => m.IsPlayingMedia))
                    {
                        item.IsPlayingMedia = false;
                    }

                    _currentMedia.IsPlayingMedia = true;

                    if (!_currentMedia.LoadedABPoint)
                    {
                        if (int.TryParse(
                                this._config.ReadConfigNode(new[]
                                {
                                        CustomStatics.VIDEO, CustomStatics.VideoABPoints, _currentMedia.Name,
                                        nameof(_currentMedia.PointAMills)
                                }), out int mills))
                        {
                            _currentMedia.CurrentMills = mills;
                            _currentMedia.SetPointA(mills);
                        }

                        if (int.TryParse(
                                this._config.ReadConfigNode(new[]
                                {
                                        CustomStatics.VIDEO, CustomStatics.VideoABPoints, _currentMedia.Name,
                                        nameof(_currentMedia.PointBMills)
                                }), out mills))
                        {
                            _currentMedia.SetPointB(mills);
                        }

                        _currentMedia.LoadedABPoint = true;
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
                    this.NextMedia(dto.Video);
                }
            };
        }

        private void AddVideoFromFileDialog()
        {
            var path = this._settingManager[CustomStatics.EnumSettings.Video.ToString()].Value;

            OpenFileDialog openFileDialog =
                CommonAtomUtils.OpenFileDialog(path, new VideoMedia());

            if (openFileDialog != null)
            {
                this.LoadVideo(openFileDialog.FileNames);

                this.TryRefreshLastVideoDir(openFileDialog.FileName.GetParentPath());
            }
        }

        private void AddVideoFromFolderDialog()
        {
            var path = this._settingManager[CustomStatics.EnumSettings.Video.ToString()].Value;

            var selectedPath = CommonCoreUtils.OpenFolderDialog(path);
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

            this._settingManager[CustomStatics.EnumSettings.Video.ToString()].Value = dir;
        }

        private IEnumerable<string> GetNewFiles(IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                if (!this.DisplayPlaying.Any(item => item.FilePath == filePath))
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

            if (this.CurrentMedia is null)
            {
                this.SetAndPlay(this.DisplayPlaying.FirstOrDefault());
            }
        }

        private VideoModelAndGuid _dto;
        private readonly IConfigManager _config;

        public VideoPlayerViewModel(IEventAggregator eventAggregator, IConfigManager config, IAppConfigFileHotKeyManager appConfigFileHotKeyManager, ISettingManager<SettingModel> settingMnager)
            : base(eventAggregator, config, appConfigFileHotKeyManager, settingMnager)
        {
            this._config = config.AssertNotNull(nameof(IConfigManager));

            this._dto = new VideoModelAndGuid(this.Identity);

            this.SubscribeEvents();
        }

        #region overrides
        protected override void InitCommands()
        {
            base.InitCommands();

            this.OpenInExploreCommand = new DelegateCommand<string>(videoDir =>
            {
                if (videoDir == null)
                {
                    return;
                }

                Process.Start("explorer", videoDir);
            });

            this.DeletePlayingCommand = new DelegateCommand<PlayingVideoViewModel>(video =>
            {
                if (video == null)
                {
                    return;
                }

                if (video == this.CurrentMedia)
                {
                    if (this.DisplayPlaying.Count > 1)
                    {
                        this.NextMedia(video);
                    }
                    else
                    {
                        this.CurrentMedia = null;
                        this.Running = false;
                    }
                }

                this.DisplayPlaying.Remove(video);

                this.RefreshPlayingIndex();
            });

            this.AddFilesCommand = new DelegateCommand(AddVideoFromFileDialog);

            this.AddFolderCommand = new DelegateCommand(AddVideoFromFolderDialog);
        }

        protected override void LoadConfig(IConfigManager config)
        {
            var playOrder = config.ReadConfigNode(CustomStatics.VideoPlayOrder_ConfigKey);
            this.CurrentPlayOrder =
                CustomStatics.MediaPlayOrderList.FirstOrDefault(item => item.Description == playOrder) ??
                CustomStatics.MediaPlayOrderList.First();

            var stretch = config.ReadConfigNode(CustomStatics.VideoStretch_ConfigKey);
            if (!string.IsNullOrEmpty(stretch) && Enum.TryParse<Stretch>(stretch, true, out Stretch result))
            {
                this.Stretch = result;
            }

            config.SetConfig += config =>
            {
                config.WriteConfigNode(this.CurrentPlayOrder.Description, CustomStatics.VideoPlayOrder_ConfigKey);
                config.WriteConfigNode(this.Stretch, CustomStatics.VideoStretch_ConfigKey);
            };

            config.PostSetConfig += config =>
            {
                foreach (var item in this.DisplayPlaying)
                {
                    config.WriteConfigNode(item.Name, new[] { CustomStatics.VIDEO, CustomStatics.VideoABPoints, item.Name });
                    config.WriteConfigNode(item.PointAMills,
                        new[] { CustomStatics.VIDEO, CustomStatics.VideoABPoints, item.Name, nameof(item.PointAMills) });
                    config.WriteConfigNode(item.PointBMills,
                        new[] { CustomStatics.VIDEO, CustomStatics.VideoABPoints, item.Name, nameof(item.PointBMills) });
                }
            };
        }

        protected override void RaiseResetMediaEvent(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<ResetVideoPlayerEvent>().Publish(this.Identity);
        }

        protected override void RaiseResetPlayerAndPlayMediaEvent(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<ResetPlayerAndPlayVideoEvent>().Publish(this.Identity);
        }

        protected override void PlayPlaying(MediaBaseViewModel currentMedia)
        {
            if (currentMedia == this.CurrentMedia)
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
                this.SetAndPlay(currentMedia);
            }
        }

        protected override void Rewind()
        {
            base.Rewind();

            this.RefreshMediaOperation(OperationType.Rewind);
        }

        protected override void FastForward()
        {
            base.FastForward();

            this.RefreshMediaOperation(OperationType.FastForward);
        }
        #endregion

        #region Commands

        public ICommand OpenInExploreCommand { get; private set; }

        public ICommand DeletePlayingCommand { get; private set; }

        /// <summary>
        /// 从本地添加音乐到列表
        /// </summary>
        public ICommand AddFilesCommand { get; private set; }

        /// <summary>
        /// 从文件夹添加音乐到列表
        /// </summary>
        public ICommand AddFolderCommand { get; private set; }
        #endregion

        public void Dispose()
        {
            foreach (var item in this.DisplayPlaying)
            {
                item.Dispose();
            }

            this.OpenInExploreCommand = null;

            this.PlayPlayingCommand = null;

            this.AddFilesCommand = null;
            this.AddFolderCommand = null;

            base.Dispose();
        }
    }
}
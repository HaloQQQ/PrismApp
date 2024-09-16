using MusicPlayerModule.Models;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.MsgEvents.Video;
using MusicPlayerModule.MsgEvents.Video.Dtos;
using Prism.Events;
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
using MusicPlayerModule.Contracts;

namespace MusicPlayerModule.ViewModels
{
    internal class VideoPlayerViewModel : MediaPlayerViewModel
    {
        protected override string MediaType => "视频";
        protected override string[] MediaHotKey_ConfigKey => ["HotKeys", "App", "Video"];

        protected override string[] MediaPlayOrder_ConfigKey => [CustomStatics.EnumSettings.Video.ToString(), "VideoPlayOrder"];

        protected override string[] MediaABPoints_ConfigKey => [CustomStatics.EnumSettings.Video.ToString(), "VideoABPoints"];

        private SettingModel VideoSetting => this._settingManager[CustomStatics.VIDEO];

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


        public MediaOperationViewModel MediaOperationViewModel { get; } = new MediaOperationViewModel();

        private void RefreshMediaOperation(OperationType operationType)
        {
            this.MediaOperationViewModel.OperationType = operationType;

            this._eventAggregator.GetEvent<MediaOperationUpdatedEvent>().Publish(this.Identity);
        }

        public Guid Identity { get; private set; } = Guid.NewGuid();

        private void TryRefreshLastVideoDir(string dir)
        {
            AppUtils.AssertDataValidation(dir.IsDirectoryPath(), $"{dir}必须为存在的目录");

            this.VideoSetting.Value = dir;
        }

        private void LoadVideo(IEnumerable<string> filePaths)
        {
            if (filePaths.IsNullOrEmpty())
            {
                return;
            }

            List<string> list = TryGetNewFiles(filePaths).ToList();

            if (list.Count == 0)
            {
                return;
            }

            foreach (var filePath in list)
            {
                this.DisplayPlaying.Add(new PlayingVideoViewModel(this._dto, new VideoModel(filePath)));
            }

            this.RefreshPlayingIndex();

            if (this.CurrentMedia == null)
            {
                this.SetAndPlay(this.DisplayPlaying.FirstOrDefault());
            }

            IEnumerable<string> TryGetNewFiles(IEnumerable<string> filePaths)
            {
                foreach (var filePath in filePaths)
                {
                    if (!this.DisplayPlaying.Any(item => item.FilePath == filePath))
                    {
                        yield return filePath;
                    }
                }
            }
        }

        private VideoModelAndGuid _dto;

        public VideoPlayerViewModel(IEventAggregator eventAggregator, IConfigManager config, IAppConfigFileHotKeyManager appConfigFileHotKeyManager, ISettingManager<SettingModel> settingMnager)
            : base(eventAggregator, config, appConfigFileHotKeyManager, settingMnager)
        {
            this._dto = new VideoModelAndGuid(this.Identity);
        }

        #region overrides
        protected override void AddMediaFromFileDialog_CommandExecute()
        {
            var path = this.VideoSetting.Value;

            OpenFileDialog openFileDialog =
                CommonAtomUtils.OpenFileDialog(path, new VideoMedia());

            if (openFileDialog != null)
            {
                this.LoadVideo(openFileDialog.FileNames);

                this.TryRefreshLastVideoDir(openFileDialog.FileName.GetParentPath());
            }
        }

        protected override void AddMediaFromFolderDialog_CommandExecute()
        {
            var path = this.VideoSetting.Value;

            var selectedPath = CommonCoreUtils.OpenFolderDialog(path);
            if (!selectedPath.IsNullOrEmpty())
            {
                this.TryRefreshLastVideoDir(selectedPath);

                var list = selectedPath.GetFiles(str => str.EndsWith(".mp4"));

                this.LoadVideo(list);
            }
        }

        protected override void SubscribeEvents(IEventAggregator eventAggregator)
        {
            base.SubscribeEvents(eventAggregator);

            PlayingVideoViewModel.ToNextVideo += dto =>
            {
                if (dto.Guid == this.Identity)
                {
                    this.NextMedia_CommandExecute(dto.Video);
                }
            };
        }

        protected override void LoadConfig(IConfigManager config)
        {
            base.LoadConfig(config);

            var stretch = config.ReadConfigNode(CustomStatics.VideoStretch_ConfigKey);
            if (Enum.TryParse<Stretch>(stretch, true, out Stretch result))
            {
                this.Stretch = result;
            }

            config.SetConfig += config =>
            {
                config.WriteConfigNode(this.Stretch, CustomStatics.VideoStretch_ConfigKey);
            };
        }

        protected override void RaiseContinueMediaEvent()
        {
            _eventAggregator.GetEvent<ContinueCurrentVideo>().Publish(this.Identity);
        }

        protected override void RaisePauseMediaEvent()
        {
            _eventAggregator.GetEvent<PauseCurrentVideo>().Publish(this.Identity);
        }

        protected override void RaiseResetMediaEvent(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<ResetVideoPlayerEvent>().Publish(this.Identity);
        }

        protected override void RaiseResetPlayerAndPlayMediaEvent(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<ResetPlayerAndPlayVideoEvent>().Publish(this.Identity);
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
                    this.RefreshMediaOperation(OperationType.Pause);
                }
            }
            else
            {
                this.SetAndPlay(currentMedia);
            }
        }

        protected override void Rewind_CommandExecute()
        {
            base.Rewind_CommandExecute();

            this.RefreshMediaOperation(OperationType.Rewind);
        }

        protected override void FastForward_CommandExecute()
        {
            base.FastForward_CommandExecute();

            this.RefreshMediaOperation(OperationType.FastForward);
        }
        #endregion
    }
}
using MusicPlayerModule.Models;
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
using MusicPlayerModule.ViewModels.Base;
using MusicPlayerModule.Contracts;
using PrismAppBasicLib.Models;

namespace MusicPlayerModule.ViewModels
{
    internal class VideoPlayerViewModel : MediaPlayerViewModel
    {
        protected override string MediaType => "视频";
        protected override string[] MediaHotKey_ConfigKey => new string[] { "HotKeys", "App", "Video" };

        protected override string[] MediaPlayOrder_ConfigKey => new string[] { CustomStatics.EnumSettings.Video.ToString(), "VideoPlayOrder" };

        protected override string[] MediaABPoints_ConfigKey => new string[] { CustomStatics.EnumSettings.Video.ToString(), "VideoABPoints" };

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

        public Guid Identity { get; } = Guid.NewGuid();

        private bool TryLoadVideo(IEnumerable<string> filePaths)
        {
            if (filePaths.IsNullOrEmpty())
            {
                return false;
            }

            IEnumerable<string> list = TryGetNewFiles(filePaths);

            if (!list.Any())
            {
                return false;
            }

            foreach (var filePath in list)
            {
                new PlayingVideoViewModel(this._dto, new VideoModel(filePath)).TryAddTo(this.DisplayPlaying);
            }

            this.RefreshPlayingIndex();

            if (this.CurrentMedia == null)
            {
                this.SetAndPlay(this.DisplayPlaying.FirstOrDefault());
            }

            return true;

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
            OpenFileDialog openFileDialog =
                CommonAtomUtils.OpenFileDialog(this.VideoSetting.Value, new VideoMedia());

            if (openFileDialog != null)
            {
                this.TryLoadVideo(openFileDialog.FileNames);

                this.VideoSetting.Value = openFileDialog.FileName.GetParentPath();
            }
        }

        protected override void AddMediaFromFolderDialog_CommandExecute()
        {
            var selectedPath = CommonCoreUtils.OpenFolderDialog(this.VideoSetting.Value);

            if (!selectedPath.IsNullOrBlank())
            {
                var list = selectedPath.GetFiles(str => str.EndsWithIgnoreCase(".mp4"));

                this.TryLoadVideo(list);

                this.VideoSetting.Value = selectedPath;
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

        protected override void LoadConfig(IConfigManager configManager)
        {
            base.LoadConfig(configManager);

            var stretch = configManager.ReadConfigNode(CustomStatics.VideoStretch_ConfigKey);
            if (Enum.TryParse<Stretch>(stretch, true, out Stretch result))
            {
                this.Stretch = result;
            }

            configManager.SetConfig += config =>
            {
                config.WriteConfigNode(this.Stretch, CustomStatics.VideoStretch_ConfigKey);
            };
        }

        protected override void RaiseContinueMediaEvent()
        {
            _eventAggregator.GetEvent<ContinueCurrentVideoEvent>().Publish(this.Identity);
        }

        protected override void RaisePauseMediaEvent()
        {
            _eventAggregator.GetEvent<PauseCurrentVideoEvent>().Publish(this.Identity);
        }

        protected override void RaiseResetMediaEvent()
        {
            _eventAggregator.GetEvent<ResetVideoPlayerEvent>().Publish(this.Identity);
        }

        protected override void RaiseResetPlayerAndPlayMediaEvent()
        {
            _eventAggregator.GetEvent<ResetPlayerAndPlayVideoEvent>().Publish(this.Identity);
        }

        protected override void PlayInPlaying_CommandExecute(MediaBaseViewModel currentMedia)
        {
            base.PlayInPlaying_CommandExecute(currentMedia);

            if (currentMedia == this.CurrentMedia)
            {
                if (!this.Running)
                {
                    this.RefreshMediaOperation(OperationType.Pause);
                }
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
using MusicPlayerModule.Contracts;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.MsgEvents.Video;
using MusicPlayerModule.MsgEvents.Video.Dtos;
using MusicPlayerModule.ViewModels;
using Prism.Events;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace MusicPlayerModule.Views
{
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8629 // 可为 null 的值类型可为 null。
    /// <summary>
    /// VideoPlayerView.xaml 的交互逻辑
    /// </summary>
    public partial class VideoPlayerView : UserControl
    {
        private DispatcherTimer _progressTime;
        private readonly IEventAggregator _eventAggregator;

        private VideoPlayerViewModel _videoPlayerViewModel;

        public VideoPlayerView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            this._eventAggregator = eventAggregator;
            this._videoPlayerViewModel = this.DataContext as VideoPlayerViewModel;

            this._dto = new VideoModelAndGuid(this._videoPlayerViewModel.Identity);

            this._progressTime = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            this._progressTime.Tick += (sender, e) =>
            {
                this.videoSlider.Value = this.mediaPlayer.Position.TotalMilliseconds;

                this.RefreshCursor();
            };

            this._eventAggregator.GetEvent<VideoProgreeTimerIsEnableUpdatedEvent>().Subscribe(isTimerEnable =>
            {
                if (_videoPlayerViewModel.Identity == isTimerEnable.Guid)
                {
                    this._progressTime.IsEnabled = isTimerEnable.Value;

                    if (!isTimerEnable.Value)
                    {
                        while (this.Cursor == Cursors.None)
                        {
                            this.Cursor = Cursors.Arrow;
                        }
                    }
                }
            });
            this._eventAggregator.GetEvent<ResetPlayerAndPlayVideoEvent>().Subscribe(guid =>
            {
                if (this._videoPlayerViewModel.Identity == guid)
                {
                    Commons.ResetMediaPlayer(this.videoSlider, this.mediaPlayer);
                    this.mediaPlayer.Play();
                }
            });
            this._eventAggregator.GetEvent<ResetVideoPlayerEvent>().Subscribe(guid =>
            {
                if (this._videoPlayerViewModel.Identity == guid)
                {
                    Commons.ResetMediaPlayer(this.videoSlider, this.mediaPlayer);
                }
            });

            this._eventAggregator.GetEvent<ContinueCurrentVideoEvent>().Subscribe(guid =>
            {
                if (this._videoPlayerViewModel.Identity == guid)
                {
                    this.mediaPlayer.Play();
                }
            });
            this._eventAggregator.GetEvent<PauseCurrentVideoEvent>().Subscribe(guid =>
            {
                if (this._videoPlayerViewModel.Identity == guid)
                {
                    this.mediaPlayer.Pause();
                }
            });

            this._eventAggregator.GetEvent<MediaOperationUpdatedEvent>().Subscribe(guid =>
            {
                if (this._dto.Guid == guid)
                {
                    ShowOperationAnimation();
                }

                /// <summary>
                /// 视频暂停操作隐藏动画
                /// </summary>
                void ShowOperationAnimation()
                {
                    var storyBoard = new Storyboard();

                    var keyframes = new ObjectAnimationUsingKeyFrames();

                    var show = new DiscreteObjectKeyFrame();
                    show.KeyTime = TimeSpan.FromSeconds(0);
                    show.Value = Visibility.Visible;
                    keyframes.KeyFrames.Add(show);

                    var keyFrame = new DiscreteObjectKeyFrame();
                    keyFrame.KeyTime = TimeSpan.FromSeconds(2);
                    keyFrame.Value = Visibility.Collapsed;
                    keyframes.KeyFrames.Add(keyFrame);

                    Storyboard.SetTarget(keyframes, this.OperationResultBox);
                    Storyboard.SetTargetProperty(keyframes, new PropertyPath("Visibility"));

                    storyBoard.Children.Add(keyframes);

                    storyBoard.Begin();
                }
            });

            this._eventAggregator.GetEvent<IncreaseVolumeEvent>().Subscribe(() =>
            {
                if (this.mediaPlayer.IsLoaded)
                {
                    Commons.IncreaseVolume(mediaPlayer);
                }
            });
            this._eventAggregator.GetEvent<DecreaseVolumeEvent>().Subscribe(() =>
            {
                if (this.mediaPlayer.IsLoaded)
                {
                    Commons.DecreaseVolume(mediaPlayer);
                }
            });
        }

        private Point lastMousePosition;  // 上次鼠标位置
        private void RefreshCursor()
        {
            Point currentMousePosition = Mouse.GetPosition(this);  // 获取当前鼠标位置

            var width = this.RenderSize.Width;
            var height = this.RenderSize.Height;

            if (currentMousePosition.X >= 0 && currentMousePosition.X < width
                && currentMousePosition.Y >= 0 && currentMousePosition.Y < height
                && currentMousePosition != lastMousePosition)
            {
                this.Cursor = Cursors.Arrow;  // 显示鼠标光标

                lastMousePosition = currentMousePosition;  // 更新上次鼠标位置
            }
            else
            {
                this.Cursor = Cursors.None;  // 隐藏鼠标光标
            }
        }

        private VideoModelAndGuid _dto;

        /// <summary>
        /// 播放列表弹窗出现时更新列表滚动条位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayingListPopup_UpdateDataGridScrollBar(object sender, EventArgs e)
        {
            if (this.PlayingDataGrid.SelectedItem != null)
            {
                this.PlayingDataGrid.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
                    this.PlayingDataGrid.ScrollIntoView(this.PlayingDataGrid.SelectedItem)
                );
            }
        }

        private bool _hasSubscribedWindowStateChangedEvent;
        /// <summary>
        /// 获取时常、窗口最小化时静音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (this.DataContext is VideoPlayerViewModel videoPlayerViewModel)
            {
                if (videoPlayerViewModel.CurrentMedia is PlayingVideoViewModel video && video.TotalMills == 0)
                {
                    video.SetVideoTotalMills((int)this.mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds);
                }
            }

            var window = Window.GetWindow(this);

            if (window != null)
            {
                if (!this._hasSubscribedWindowStateChangedEvent)
                {
                    window.StateChanged += (sender, e) =>
                    {
                        if (window.WindowState == WindowState.Minimized)
                        {
                            this.mediaPlayer.Pause();
                            this._videoPlayerViewModel.Running = false;
                        }
                        else
                        {
                            this.mediaPlayer.Play();
                            this._videoPlayerViewModel.Running = true;
                        }
                    };

                    this._hasSubscribedWindowStateChangedEvent = true;
                }
            }
        }

        private void UserControl_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Open)
            {
                e.Handled = true;

                this.PlayingListButton.IsChecked = !this.PlayingListButton.IsChecked;
            }
        }

        /// <summary>
        /// 进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void musicSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            e.Handled = true;

            var newSpan = TimeSpan.FromMilliseconds(e.NewValue);
            if (Math.Abs(this.mediaPlayer.Position.Subtract(newSpan).TotalMilliseconds) > 50)
            {
                this.mediaPlayer.Position = newSpan;
            }
        }

        /// <summary>
        /// 播放速度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VideoSpeedRatioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            e.Handled = true;

            double newValue = Math.Round(e.NewValue, 2); // 保留2位小数
            this.VideoSpeedRatioSlider.Value = newValue;

            this.mediaPlayer.SpeedRatio = newValue;
        }

        #region 音量调节
        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.VolumePopup.IsOpen)
            {
                int baseValue = e.Delta < 0 ? -1 : 1;

                var value = this.mediaPlayer.Volume + 0.05 * baseValue;

                if (value > 1)
                {
                    value = 1;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                this.mediaPlayer.Volume = value;

                e.Handled = true;
            }
        }

        private double _lastVolume;
        private void VolumeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (this.mediaPlayer.IsMuted)
            {
                this._lastVolume = this.mediaPlayer.Volume;
                this.mediaPlayer.Volume = 0;
            }
            else
            {
                this.mediaPlayer.Volume = this._lastVolume;
            }
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            e.Handled = true;

            if (e.NewValue == 0)
            {
                this.mediaPlayer.IsMuted = true;
            }
            else
            {
                if (this.mediaPlayer.IsMuted)
                {
                    this.mediaPlayer.IsMuted = false;
                }
            }
        }
        #endregion
    }
}

using MusicPlayerModule.MsgEvents.Music;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.ViewModels;
using Prism.Events;
using System.Windows;
using System.Windows.Controls;
using Prism.Ioc;
using System.Windows.Input;
using System.Windows.Threading;
using MusicPlayerModule.Contracts;
using IceTea.Atom.Contracts;
using IceTea.Wpf.Atom.Extensions;

namespace MusicPlayerModule.Views
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    /// <summary>
    /// MusicFooterView.xaml 的交互逻辑
    /// </summary>
    public partial class MusicFooterView : UserControl
    {
        private DispatcherTimer _lyricTime;

        private Window _horizentalDesktopLyricWindow;
        private Window _verticalDesktopLyricWindow;

        public MusicFooterView()
        {
            InitializeComponent();

            this.Init();
        }

        private void Init()
        {
            var eventAggregator = ContainerLocator.Current.Resolve<IEventAggregator>();

            this._lyricTime = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
            this._lyricTime.Tick += (sender, e) =>
                this.musicSlider.Value = this.mediaPlayer.Position.TotalMilliseconds;

            eventAggregator.GetEvent<MusicProgressTimerIsEnableUpdatedEvent>().Subscribe(isTimerEnable => this._lyricTime.IsEnabled = isTimerEnable);
            eventAggregator.GetEvent<ResetPlayerAndPlayMediaEvent>().Subscribe(() =>
            {
                Commons.ResetMediaPlayer(this.musicSlider, this.mediaPlayer);
                this.mediaPlayer.Play();

                var mainWindow = Window.GetWindow(this);
                if ((_horizentalDesktopLyricWindow == null || _horizentalDesktopLyricWindow.DataContext == null) && mainWindow != null)
                {
                    var configManager = ContainerLocator.Current.Resolve<IConfigManager>();

                    _horizentalDesktopLyricWindow = new HorizontalMusicLyricDesktopWindow(configManager);
                    mainWindow.Closing += (sender, e) => _horizentalDesktopLyricWindow.Close();
                }

                if ((_verticalDesktopLyricWindow == null || _verticalDesktopLyricWindow.DataContext == null) && mainWindow != null)
                {
                    var configManager = ContainerLocator.Current.Resolve<IConfigManager>();

                    _verticalDesktopLyricWindow = new VerticalMusicLyricDesktopWindow(configManager);
                    mainWindow.Closing += (sender, e) => _verticalDesktopLyricWindow.Close();
                }
            });
            eventAggregator.GetEvent<ResetMediaPlayerEvent>().Subscribe(() => Commons.ResetMediaPlayer(this.musicSlider, this.mediaPlayer));

            eventAggregator.GetEvent<ContinueCurrentMediaEvent>().Subscribe(() => this.mediaPlayer.Play());
            eventAggregator.GetEvent<PauseCurrentMediaEvent>().Subscribe(() => this.mediaPlayer.Pause());

            eventAggregator.GetEvent<IncreaseVolumeEvent>().Subscribe(() =>
            {
                if (this.mediaPlayer.IsLoaded)
                {
                    Commons.IncreaseVolume(mediaPlayer);
                }
            });
            eventAggregator.GetEvent<DecreaseVolumeEvent>().Subscribe(() =>
            {
                if (this.mediaPlayer.IsLoaded)
                {
                    Commons.DecreaseVolume(mediaPlayer);
                }
            });

            eventAggregator.GetEvent<MediaPositionEvent>().Subscribe(timeSpan => this.mediaPlayer.Position = timeSpan);

            eventAggregator.GetEvent<PlayListPanelOpenEvent>().Subscribe(() =>
            {
                this.PlayingListButton.IsChecked = !this.PlayingListButton.IsChecked;

                if (this.PlayingListButton.IsChecked.HasValue && this.PlayingListButton.IsChecked.Value)
                {
                    this.AdaptPlayingListPanelSize();
                }
            });
        }

        private void UserControl_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var handled = true;

            if (e.Command == ApplicationCommands.Find)
            {
                switch (e.Parameter?.ToString())
                {
                    case "Playing":
                        this.QueryButton.IsChecked = true;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            else if (e.Command == ApplicationCommands.Close)
            {
                switch (e.Parameter?.ToString())
                {
                    case "StopPlayingListFilte":
                        // 播放队列筛选输入框消失
                        this.PlayingKeyWordsTxt.Text = string.Empty;
                        this.QueryButton.IsChecked = false;

                        break;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            else
            {
                handled = false;
            }

            e.Handled = handled;
        }

        /// <summary>
        /// 播放在播放列表中的某个音乐
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridRow_Playing_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.Source is DataGridRow row && row.DataContext is PlayingMusicViewModel playing)
            {
                var dataGrid = row.GetVisualAncestor<DataGrid>();

                if (dataGrid.DataContext is MusicPlayerViewModel musicPlayerViewModel)
                {
                    musicPlayerViewModel.PlayPlayingCommand.Execute(playing);
                }
            }
        }

        private void musicSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            e.Handled = true;
            var newSpan = TimeSpan.FromMilliseconds(e.NewValue);
            if (Math.Abs(this.mediaPlayer.Position.Subtract(newSpan).TotalMilliseconds) > 50)
            {
                this.mediaPlayer.Position = newSpan;
            }
        }

        private void MusicSpeedRatioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            e.Handled = true;

            double newValue = Math.Round(e.NewValue, 1); // 保留1位小数
            this.MusicSpeedRatioSlider.Value = newValue;

            this.mediaPlayer.SpeedRatio = newValue;
        }

        /// <summary>
        /// 播放列表搜索某个歌曲时搜索框获取焦点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayingListFilterPanel_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool b && b)
            {
                this.PlayingKeyWordsTxt.Focus();
            }
        }

        private void PlayingListButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            this.AdaptPlayingListPanelSize();
        }


        private void AdaptPlayingListPanelSize()
        {
            // 根据屏幕高度设置播放列表弹窗高度
            var window = Window.GetWindow(this);

            if (window.WindowState == WindowState.Maximized)
            {
                this.PlayingListPopup.Height = 664;
            }
            else
            {
                this.PlayingListPopup.Height = 522;
            }
        }

        //音量调节
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

                if (this.mediaPlayer.Volume != value)
                {
                    this.mediaPlayer.Volume = value;
                }

                e.Handled = true;
            }
        }

        #region 音量调整
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

        /// <summary>
        /// Popup打开时加载DataGrid滚动条
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

        private void PlayingListPopup_Closed(object sender, EventArgs e)
        {
            this.PlayingKeyWordsTxt.Text = string.Empty;
            this.QueryButton.IsChecked = false;
        }

        /// <summary>
        /// 拦截Popup的点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmptyHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}

using MusicPlayerModule.Models;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.Utils;
using MusicPlayerModule.ViewModels;
using Prism.Events;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using IceTea.Wpf.Atom.Extensions;
using IceTea.Atom.Contracts;

namespace MusicPlayerModule.Views
{
    public partial class MusicPlayer : UserControl
    {
        private DispatcherTimer _lyricTime;
        private readonly IEventAggregator _eventAggregator;

        private MusicPlayerViewModel _musicPlayerViewModel;

        public MusicPlayer(IEventAggregator eventAggregator, IConfigManager configManager)
        {
            InitializeComponent();

            this._musicPlayerViewModel = this.DataContext as MusicPlayerViewModel;

            this._eventAggregator = eventAggregator;

            this._lyricTime = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
            this._lyricTime.Tick += (sender, e) =>
                this.musicSlider.Value = this.mediaPlayer.Position.TotalMilliseconds;

            this._eventAggregator.GetEvent<MusicProgressTimerIsEnableUpdatedEvent>().Subscribe(isTimerEnable => this._lyricTime.IsEnabled = isTimerEnable);
            this._eventAggregator.GetEvent<ResetPlayerAndPlayMusicEvent>().Subscribe(() =>
            {
                this.ResetMediaPlayer();
                this.mediaPlayer.Play();

                var mainWindow = Window.GetWindow(this);
                if ((_horizentalDesktopLyricWindow == null || _horizentalDesktopLyricWindow.DataContext == null) && mainWindow != null)
                {
                    _horizentalDesktopLyricWindow = new HorizontalMusicLyricDesktopWindow(configManager);
                    mainWindow.Closing += (sender, e) => _horizentalDesktopLyricWindow.Close();
                }

                if ((_verticalDesktopLyricWindow == null || _verticalDesktopLyricWindow.DataContext == null) && mainWindow != null)
                {
                    _verticalDesktopLyricWindow = new VerticalMusicLyricDesktopWindow(configManager);
                    mainWindow.Closing += (sender, e) => _verticalDesktopLyricWindow.Close();
                }
            });
            this._eventAggregator.GetEvent<ResetMusicPlayerEvent>().Subscribe(() => this.ResetMediaPlayer());

            this._eventAggregator.GetEvent<ContinueCurrentMusicEvent>().Subscribe(() => this.mediaPlayer.Play());
            this._eventAggregator.GetEvent<PauseCurrentMusicEvent>().Subscribe(() => this.mediaPlayer.Pause());

            this._eventAggregator.GetEvent<ToggleLyricDesktopEvent>().Subscribe(() =>
            {
                _musicPlayerViewModel.DesktopLyric.IsDesktopLyricShow = !_musicPlayerViewModel.DesktopLyric.IsDesktopLyricShow;
            });

            this._eventAggregator.GetEvent<IncreaseVolumeEvent>().Subscribe(() =>
            {
                if (_musicPlayerViewModel.CurrentMedia != null)
                {
                    this.IncreaseVolume();
                }
            });
            this._eventAggregator.GetEvent<DecreaseVolumeEvent>().Subscribe(() =>
            {
                if (_musicPlayerViewModel.CurrentMedia != null)
                {
                    this.DecreaseVolume();
                }
            });
        }

        private Window _horizentalDesktopLyricWindow;
        private Window _verticalDesktopLyricWindow;

        private void ResetMediaPlayer()
        {
            this.musicSlider.Value = 0;
            this.mediaPlayer.Stop();
            this.mediaPlayer.Position = TimeSpan.Zero;
        }

        /// <summary>
        /// 添加所有到播放列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataGridRow_Favorites_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (e.OriginalSource is Run || (e.OriginalSource is TextBlock txt && 16 == txt.ActualHeight))
            {
                return;
            }

            if (sender is DataGridRow row && row.DataContext is FavoriteMusicViewModel music)
            {
                var dataGrid = row.GetVisualAncestor<DataGrid>();

                var items = dataGrid.Items.OfType<FavoriteMusicViewModel>();

                if (items.Any())
                {
                    this._musicPlayerViewModel.PlayAndAddCurrentFavoritesCommand.Execute(new BatchAddAndPlayModel(music, items));
                }
            }
        }

        /// <summary>
        /// 播放分类中的选中音乐集合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MusicDistribute_ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (sender is ListBoxItem listBoxItem && listBoxItem.DataContext is MusicWithClassifyModel model)
            {
                this._musicPlayerViewModel.PlayCurrentCategoryCommand.Execute(model);
            }
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
                this._musicPlayerViewModel.PlayPlayingCommand.Execute(playing);
            }
        }

        /// <summary>
        /// 歌词封面显示和隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LyricPageSlide(object sender, RoutedEventArgs e)
        {
            var storyBoard = new Storyboard();

            var y = (this.LyricPage.RenderTransform as TranslateTransform).Y;

            var to = y == 0 ? SystemParameters.PrimaryScreenHeight : 0;

            var translateAnimation = new DoubleAnimation(y, to, new Duration(TimeSpan.FromMilliseconds(500)));

            Storyboard.SetTarget(translateAnimation, this.LyricPage);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

            storyBoard.Children.Add(translateAnimation);

            storyBoard.Begin();

            e.Handled = true;
        }

        #region 歌词刷新
        private void LyricList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            if (LyricList.Items.Count == 0)
            {
                return;
            }

            var index = this.LyricList.SelectedIndex;

            // 查找 ListBox 中的 ScrollViewer 元素
            DependencyObject child = this.LyricList.GetVisualChildObject<ScrollViewer>();

            // 如果找到了 ScrollViewer，将其滚动到选中项的位置
            if (child is ScrollViewer scrollViewer)
            {
                var value = index - 3;

                scrollViewer.ScrollToVerticalOffset(value * scrollViewer.ExtentHeight / LyricList.Items.Count);
            }
        }
        #endregion

        /// <summary>
        /// 设置歌曲位置为当前歌词开始
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.Source is ListBoxItem item)
            {
                if (item.DataContext is KRCLyricsLine line)
                {
                    this.mediaPlayer.Position = line.LineStart;
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

        private void IncreaseVolume()
        {
            var value = this.mediaPlayer.Volume + 0.05;

            if (value > 1)
            {
                value = 1;
            }

            this.mediaPlayer.Volume = value;
        }

        private void DecreaseVolume()
        {
            var value = this.mediaPlayer.Volume - 0.05;

            if (value < 0)
            {
                value = 0;
            }

            this.mediaPlayer.Volume = value;
        }

        private void UserControl_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Find)
            {
                this.FavoritesKeyWordsTxt.Focus();

                e.Handled = true;
            }
            else if (e.Command == MediaCommands.IncreaseVolume)
            {
                IncreaseVolume();

                e.Handled = true;
            }
            else if (e.Command == MediaCommands.DecreaseVolume)
            {
                DecreaseVolume();

                e.Handled = true;
            }
            else if (e.Command == MediaCommands.TogglePlayPause)
            {
                this.SwitchLyric(e);
            }
            else if (e.Command == ApplicationCommands.Open)
            {
                e.Handled = true;

                this.PlayingListButton.IsChecked = !this.PlayingListButton.IsChecked;

                if ((bool)this.PlayingListButton.IsChecked)
                {
                    this.AdaptPlayingListPanelSize();
                }
            }
            else if (e.Command == ApplicationCommands.Close)
            {
                e.Handled = true;

                if (e.Parameter != null)
                {
                    switch (e.Parameter.ToString())
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
            }
            else if (e.Command == NavigationCommands.GoToPage)
            {
                this.LyricPageSlide(null, e);
            }
        }

        private void MusicSpeedRatioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            e.Handled = true;

            double newValue = Math.Round(e.NewValue, 1); // 保留1位小数
            this.MusicSpeedRatioSlider.Value = newValue;

            this.mediaPlayer.SpeedRatio = newValue;
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

        /// <summary>
        /// 单击多行歌词，切换到双行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchLyricMode(object sender, MouseButtonEventArgs e)
        {
            this.SwitchLyric(e);
        }

        /// <summary>
        /// 歌词多行和双行切换
        /// </summary>
        /// <param name="e"></param>
        private void SwitchLyric(RoutedEventArgs e)
        {
            if (this.LyricList.Visibility == Visibility.Visible)
            {
                this.LyricList.Visibility = Visibility.Collapsed;
                this.LyricSimple.Visibility = Visibility.Visible;
            }
            else
            {
                this.LyricList.Visibility = Visibility.Visible;
                this.LyricSimple.Visibility = Visibility.Collapsed;
            }

            e.Handled = true;
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

        /// <summary>
        /// 拦截Popup的点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EmptyHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void PlayingList_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Find)
            {
                this.QueryButton.IsChecked = true;

                e.Handled = true;
            }
        }

        private void DropIn_LinearGradientForegroundColor(object sender, DragEventArgs e)
        {
            var data = e.Data;

            if (data.GetDataPresent(typeof(SolidColorBrush)))
            {
                var color = data.GetData(typeof(SolidColorBrush)) as Brush;

                this.LinearGradientStartColor.Background = color;
            }
        }

        private void DragLeaveFrom(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
            {
                var item = (ListBoxItem)sender;
                DragDrop.DoDragDrop(item, item.Background, DragDropEffects.Copy);
            }
        }

        private void DropIn_ForegroundColor(object sender, DragEventArgs e)
        {
            var data = e.Data;

            if (data.GetDataPresent(typeof(SolidColorBrush)))
            {
                var color = data.GetData(typeof(SolidColorBrush)) as Brush;

                this.ForegroundColor.Background = color;
            }
        }
    }
}
using MusicPlayerModule.Models;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.Utils;
using MusicPlayerModule.ViewModels;
using Prism.Events;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfStyleResources.Extensions;

namespace MusicPlayerModule.Views
{
    public partial class MusicPlayer : UserControl
    {
        private DispatcherTimer _lyricTime;
        private readonly IEventAggregator _eventAggregator;

        private MusicPlayerViewModel _musicPlayerViewModel;

        public MusicPlayer(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            this._musicPlayerViewModel = this.DataContext as MusicPlayerViewModel;

            this._eventAggregator = eventAggregator;

            this._lyricTime = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
            this._lyricTime.Tick += (sender, e) => this.musicSlider.Value = this.mediaPlayer.Position.TotalMilliseconds;

            this._eventAggregator.GetEvent<MusicProgreeTimerIsEnableUpdatedEvent>().Subscribe(isTimerEnable => this._lyricTime.IsEnabled = isTimerEnable);
            this._eventAggregator.GetEvent<UpdateScrollBarToTargetLyricEvent>().Subscribe(cureentLineIndex => this.UpdateLyricPosition(cureentLineIndex));
            this._eventAggregator.GetEvent<ResetPlayerAndPlayMusicEvent>().Subscribe(() =>
            {
                this.ResetMediaPlayer();
                this.mediaPlayer.Play();
            });
            this._eventAggregator.GetEvent<ResetPlayerEvent>().Subscribe(() => this.ResetMediaPlayer());

            this._eventAggregator.GetEvent<ContinueCurrentMusicEvent>().Subscribe(() => this.mediaPlayer.Play());
            this._eventAggregator.GetEvent<PauseCurrentMusicEvent>().Subscribe(() => this.mediaPlayer.Pause());
        }

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

        #region  音量调节弹窗显示和隐藏
        private void ToggleButton_MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (!this.VolumeControl.IsMouseIn())
            {
                this.VolumeControl.Visibility = Visibility.Collapsed;
            }
        }

        private void ToggleButton_MouseEnter(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            this.VolumeControl.Visibility = Visibility.Visible;
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            this.VolumeControl.Visibility = Visibility.Collapsed;
        }
        #endregion

        /// <summary>
        /// 歌词封面显示和隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LyricPageSlide(object sender, RoutedEventArgs e)
        {
            var allow = e.Source is Grid || (e.Source is TextBlock txt && "SliderTxt".Equals(txt.Name));

            if (!allow)
            {
                return;
            }

            var storyBoard = new Storyboard();

            var y = (this.LyricPage.RenderTransform as TranslateTransform).Y;

            var to = y == 0 ? System.Windows.SystemParameters.PrimaryScreenHeight : 0;

            var translateAnimation = new DoubleAnimation(y, to, new Duration(TimeSpan.FromMilliseconds(500)));

            Storyboard.SetTarget(translateAnimation, this.LyricPage);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

            storyBoard.Children.Add(translateAnimation);

            storyBoard.Begin();

            e.Handled = true;
        }

        #region 歌词刷新
        private void UpdateLyricPosition(int index)
        {
            // 查找 ListBox 中的 ScrollViewer 元素
            DependencyObject child = this.LyricList.GetVisualChildObject<ScrollViewer>();

            // 如果找到了 ScrollViewer，将其滚动到选中项的位置
            if (child is ScrollViewer scrollViewer)
            {
                var value = index > 3 ? index - 3 : index;

                if (value == 0 || LyricList.Items.Count == 0)
                {
                    scrollViewer.ScrollToVerticalOffset(0);
                }
                else
                {
                    scrollViewer.ScrollToVerticalOffset(value * scrollViewer.ExtentHeight / LyricList.Items.Count);
                }
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

        /// <summary>
        /// 根据屏幕高度设置播放列表弹窗高度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playingList_PreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

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
        private void volumeToggleButton_Click(object sender, RoutedEventArgs e)
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
        private void DataGrid_UpdateScrollBar(object sender, EventArgs e)
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

        private void DesktopLyricPanel_Visible(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            this.DesktopLyricPanel.Visibility = Visibility.Visible;
        }

        private void DesktopLyricPanel_Hidden(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            this.DesktopLyricPanel.Visibility = Visibility.Hidden;
        }

        private void MusicGoBack(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var value = this.musicSlider.Value - 1000;

            if (value < 0)
            {
                value = 0;
            }

            this.musicSlider.Value = value;
        }

        private void MusicGoForward(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var value = this.musicSlider.Value + 1000;

            if (value > this.musicSlider.Maximum)
            {
                value = this.musicSlider.Maximum;
            }

            this.musicSlider.Value = value;
        }

        private void UserControl_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == MediaCommands.Rewind)
            {
                this.MusicGoBack(null, e);
            }
            else if (e.Command == MediaCommands.FastForward)
            {
                this.MusicGoForward(null, e);
            }
            else if (e.Command == MediaCommands.IncreaseVolume)
            {
                var value = this.mediaPlayer.Volume + 0.05;

                if (value > 1)
                {
                    value = 1;
                }

                this.mediaPlayer.Volume = value;

                e.Handled = true;
            }
            else if (e.Command == MediaCommands.DecreaseVolume)
            {
                var value = this.mediaPlayer.Volume - 0.05;

                if (value < 0)
                {
                    value = 0;
                }

                this.mediaPlayer.Volume = value;

                e.Handled = true;
            }
            else if (e.Command == ComponentCommands.MoveToHome)
            {
                this.mediaPlayer.Position = TimeSpan.Zero;

                e.Handled = true;
            }
            else if (e.Command == ComponentCommands.MoveToEnd)
            {
                if (this.mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds > 5)
                {
                    this.mediaPlayer.Position = this.mediaPlayer.NaturalDuration.TimeSpan.Subtract(TimeSpan.FromSeconds(5));
                }

                e.Handled = true;
            }
            else if (e.Command == MediaCommands.TogglePlayPause)
            {
                this.SwitchLyric(e);
            }
            else if (e.Command == ApplicationCommands.Close)
            {
                e.Handled = true;

                if (e.Parameter != null)
                {
                    switch (e.Parameter.ToString())
                    {
                        case "CloseDesktopLyricPanel":
                            this.DesktopLyricToggleButton.IsChecked = !this.DesktopLyricToggleButton.IsChecked;

                            break;
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
            if (this.VolumeControl.IsVisible)
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

        private void FavoritesKeyWordsTxtBox_LostFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            var element = this.FavoritesKeyWordsTxt;
            if (element.Text?.Length == 0 && element.Width > 70)
            {
                var storyboard = new Storyboard();

                var widthAnimation = new DoubleAnimation();
                widthAnimation.To = 70;
                widthAnimation.Duration = TimeSpan.FromSeconds(0.2);

                Storyboard.SetTarget(widthAnimation, element);
                Storyboard.SetTargetProperty(widthAnimation, new PropertyPath("Width"));

                storyboard.Children.Add(widthAnimation);

                var visibilityAnimation = new ObjectAnimationUsingKeyFrames();
                var keyFrame = new DiscreteObjectKeyFrame();
                keyFrame.KeyTime = TimeSpan.FromSeconds(0.2);
                keyFrame.Value = Visibility.Visible;

                var target = element.GetVisualChildObject<FrameworkElement>(element => element.Name == "Txt");

                Storyboard.SetTarget(visibilityAnimation, target);
                Storyboard.SetTargetProperty(visibilityAnimation, new PropertyPath("Visibility"));

                storyboard.Children.Add(visibilityAnimation);

                storyboard.Begin();
            }
        }
    }
}
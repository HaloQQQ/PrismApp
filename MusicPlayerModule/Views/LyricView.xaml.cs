using IceTea.Wpf.Atom.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using MusicPlayerModule.Utils;
using Prism.Events;
using Prism.Ioc;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.MsgEvents.Music;

namespace MusicPlayerModule.Views
{
    /// <summary>
    /// LyricView.xaml 的交互逻辑
    /// </summary>
    public partial class LyricView : UserControl
    {
        public LyricView()
        {
            InitializeComponent();

            ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<LyricPageSlideEvent>().Subscribe(() => this.LyricPageSlide(null, null));
        }

        /// <summary>
        /// 歌词封面显示和隐藏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void LyricPageSlide(object? sender, RoutedEventArgs? e)
        {
            var storyBoard = new Storyboard();

            var y = (this.RenderTransform as TranslateTransform).Y;

            var to = y == 0 ? SystemParameters.PrimaryScreenHeight : 0;

            var translateAnimation = new DoubleAnimation(y, to, new Duration(TimeSpan.FromMilliseconds(500)));

            Storyboard.SetTarget(translateAnimation, this);
            Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

            storyBoard.Children.Add(translateAnimation);

            storyBoard.Begin();

            if (e != null)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// 当前歌词行调整到垂直居中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<MediaPositionEvent>().Publish(line.LineStart);
                }
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

        private void UserControl_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var handled = false;
            if (e.Command == MediaCommands.TogglePlayPause)
            {
                this.SwitchLyric(e);

                handled = true;
            }
            else if (e.Command == NavigationCommands.GoToPage)
            {
                this.LyricPageSlide(null, e);

                handled = true;
            }

            e.Handled = handled;
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

        #region 歌词颜色调整
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
        #endregion
    }
}

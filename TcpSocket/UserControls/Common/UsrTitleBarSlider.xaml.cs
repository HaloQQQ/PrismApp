using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TcpSocket.Helper;
using Point = System.Windows.Point;

namespace TcpSocket.UserControls.Common
{
    public partial class UsrTitleBarSlider : UserControl
    {
        public UsrTitleBarSlider()
        {
            InitializeComponent();
        }

        #region WindowControl

        /// <summary>
        /// 窗口最小化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minimize_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            Window window = Window.GetWindow(this);

            window.Hide();
        }

        /// <summary>
        /// 窗口最大化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Maximize_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            Window window = Window.GetWindow(this);

            window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Close_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            Window.GetWindow(this).Close();
        }

        /// <summary>
        /// 双击标题栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleBarDoubleClick_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.Source is Grid && e.ClickCount == 2)
            {
                this.Maximize_OnMouseLeftButtonUp(sender, e);
            }
        }

        /// <summary>
        /// 界面移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Move_OnMouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Helper.Helper.IsInPopup(e.Source as FrameworkElement))
                {
                    return;
                }
                
                Window window = Window.GetWindow(this);

                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;

                    Point point = e.MouseDevice.GetPosition(window);
                    window.Left = point.X - point.X * (this.ActualWidth / SystemParameters.WorkArea.Width);
                    window.Top = point.Y - point.Y * (this.ActualHeight / SystemParameters.MaximizedPrimaryScreenHeight);
                }
                else
                {
                    window.DragMove();
                }
            }
        }

        /// <summary>
        /// 锁屏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LockScreen_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            Helper.Helper.LockWorkStation();
        }

        /// <summary>
        /// 界面置顶
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Topmost_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            Window window = Window.GetWindow(this);

            window.Topmost = !window.Topmost;
        }

        /// <summary>
        /// 标题栏上滑下拉动画
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SlideTitleBar_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var textBlock = e.Source as TextBlock;

            Debug.Assert(textBlock != null, nameof(textBlock) + " != null");

            if (this.Margin.Top == 0)
            {
                // 收起动画
                this.SlideTitleBar(textBlock, -40, 40);
            }
            else
            {
                // 下拉动画
                this.SlideTitleBar(textBlock, 0, 0);
            }
        }

        private void SlideTitleBar(TextBlock textBlock, double containerYTo, double textYTo)
        {
            var storyBoard = new Storyboard();

            var animation = new ThicknessAnimation(this.Margin,
                new Thickness(0, containerYTo, 0, 0),
                new Duration(TimeSpan.FromSeconds(1)));

            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
            storyBoard.Children.Add(animation);


            // 文字
            Debug.Assert((textBlock.RenderTransform as TranslateTransform) != null, "标题栏下拉按钮的TranslateTransform不允许未定义");
            var y = ((textBlock.RenderTransform as TranslateTransform)!).Y;
            var translateAnimation = new DoubleAnimation(y, textYTo,
                new Duration(TimeSpan.FromSeconds(1)));

            Storyboard.SetTarget(translateAnimation, textBlock);
            Storyboard.SetTargetProperty(translateAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyBoard.Children.Add(translateAnimation);

            storyBoard.Begin();
        }

        #endregion

        #region 更换主题、背景

        public event Action<object, MouseButtonEventArgs> SwitchTheme = null!;

        private void SwitchTheme_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            this.SwitchTheme(sender, e);
        }

        public event Action<object, MouseButtonEventArgs> SwitchBackground = null!;

        private void SwitchBackSliderMoveOut_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var softwareContext = Statics.DataContext.SoftwareContext;
            softwareContext.PopupResult.BackgroundImageSlider = !softwareContext.PopupResult.BackgroundImageSlider;

            this.SwitchBackground(sender, e);
        }

        #endregion

        private void OpenMainMenu_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var block = e.Source as TextBlock;

            Debug.Assert(block != null, nameof(block) + " != null");
            Debug.Assert(block.ContextMenu != null, "block.ContextMenu != null");
            block.ContextMenu.PlacementTarget = block;
            block.ContextMenu.IsOpen = true;
        }

        private async void KeyBoard_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            string? str1 = e.Parameter?.ToString();

            if (e.Command == ApplicationCommands.Save)
            {
                if (Helper.Helper.Equals(str1, Constants.ONLY_ONE_PROCESS))
                {
                    var softwareContext = Statics.DataContext.SoftwareContext;
                    softwareContext.OnlyOneProcess = !softwareContext.OnlyOneProcess;

                    Helper.Helper.ShowBalloonTip("设置软件唯一启动", (softwareContext.OnlyOneProcess ? "开启" : "关闭") + "软件唯一存在");
                }
                else if (Helper.Helper.Equals(str1, Constants.AUTO_START))
                {
                    var softwareContext = Statics.DataContext.SoftwareContext;
                    softwareContext.AutoStart = !softwareContext.AutoStart;

                    Helper.Helper.ShowBalloonTip("设置软件开机自启动", (softwareContext.AutoStart ? "自" : "不自") + "启动");
                }
                else if (Helper.Helper.Equals(str1, Constants.BACKGROUND_SWITCH))
                {
                    var softwareContext = Statics.DataContext.SoftwareContext;
                    softwareContext.BackgroundSwitch = !softwareContext.BackgroundSwitch;

                    Helper.Helper.ShowBalloonTip("设置背景轮播", softwareContext.BackgroundSwitch ? "开启" : "关闭");
                }
            }
        }
    }
}
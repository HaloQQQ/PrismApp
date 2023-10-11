using Prism.Events;
using Prism.Ioc;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MyApp.Prisms.MsgEvents;
using MyApp.Prisms.ViewModels;
using IceTea.Wpf.Core.Extensions;
using Point = System.Windows.Point;

namespace MyApp.Prisms.Views
{
    public partial class WindowTitleBarView : UserControl
    {
        private SoftwareViewModel _softwareViewModel;

        public WindowTitleBarView()
        {
            InitializeComponent();

            this._softwareViewModel = this.DataContext as SoftwareViewModel;
        }

        #region WindowControl

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
                    window.Top = point.Y -
                                 point.Y * (this.ActualHeight / SystemParameters.MaximizedPrimaryScreenHeight);
                }
                else
                {
                    window.DragMove();
                }
            }
        }

        private void SlideTitleBar(UIElement element, double containerYTo, double textYTo)
        {
            var storyBoard = new Storyboard();

            var animation = new ThicknessAnimation(this.Margin,
                new Thickness(0, containerYTo, 0, 0),
                new Duration(TimeSpan.FromSeconds(1)));

            Storyboard.SetTarget(animation, this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
            storyBoard.Children.Add(animation);


            Debug.Assert((element.RenderTransform as TranslateTransform) != null, "标题栏下拉按钮的TranslateTransform不允许未定义");
            var y = ((element.RenderTransform as TranslateTransform)!).Y;
            var translateAnimation = new DoubleAnimation(y, textYTo,
                new Duration(TimeSpan.FromSeconds(1)));

            Storyboard.SetTarget(translateAnimation, element);
            Storyboard.SetTargetProperty(translateAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            storyBoard.Children.Add(translateAnimation);

            storyBoard.Begin();
        }

        #endregion

        #region 更换主题、背景

        private void SwitchBackSliderMoveOut_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var eventAggregator = ContainerLocator.Current.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<BackgroundImageSelectorShowEvent>().Publish();
        }

        #endregion

        private void Slider_CheckedStatusChanged(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is UIElement element)
            {
                if (this.Margin.Top == 0)
                {
                    // 标题栏收起动画
                    this.SlideTitleBar(element, -42, 42);
                }
                else
                {
                    // 标题栏下拉动画
                    this.SlideTitleBar(element, 0, 0);
                }
            }

            e.Handled = true;
        }

        #region 消息框

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (_softwareViewModel.DialogMessage != null)
            {
                _softwareViewModel.DialogMessage.StopHide = false;
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (_softwareViewModel.DialogMessage != null)
            {
                _softwareViewModel.DialogMessage.StopHide = true;
            }
        }

        #endregion

        #region 屏幕亮度调节
        private double _lastBright;
        private void BrightToggleButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (this._softwareViewModel.IsLeastBright)
            {
                this._lastBright = this.brightSlider.Value;
                this.brightSlider.Value = 0;
            }
            else
            {
                this.brightSlider.Value = this._lastBright;
            }
        }

        private void ToggleButton_MouseEnter(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            this.brightControl.Visibility = Visibility.Visible;
        }

        private void ToggleButton_MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (!this.brightControl.IsMouseIn())
            {
                this.brightControl.Visibility = Visibility.Collapsed;
            }
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            this.brightControl.Visibility = Visibility.Collapsed;
        }

        private void Button_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.brightControl.IsVisible)
            {
                int baseValue = e.Delta < 0 ? -1 : 1;

                var value = this.brightSlider.Value + 5 * baseValue;

                if (value > 100)
                {
                    value = 100;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                if (this.brightSlider.Value != value)
                {
                    this.brightSlider.Value = value;
                }

                e.Handled = true;
            }
        }
        #endregion
    }
}
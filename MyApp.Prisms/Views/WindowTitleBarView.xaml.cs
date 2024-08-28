using Prism.Events;
using Prism.Ioc;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.MsgEvents;
using MyApp.Prisms.ViewModels;
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
                Window window = Window.GetWindow(this);

                if (window == null)
                {
                    return;
                }

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

        #endregion

        #region 更换主题、背景

        private void SwitchBackSliderMoveOut_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var eventAggregator = ContainerLocator.Current.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<BackgroundImageSelectorShowEvent>().Publish();
        }

        #endregion

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

        private void Button_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.brightPopup.IsOpen)
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

        private void BrightPopup_Opened(object sender, EventArgs e)
        {
            this._softwareViewModel.RefreshBrightness();
        }
        #endregion
    }
}
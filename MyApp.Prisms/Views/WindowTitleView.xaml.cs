using MyApp.Prisms.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyApp.Prisms.Views
{
#pragma warning disable CS8602 // 解引用可能出现空引用。
    /// <summary>
    /// WindowTitleView.xaml 的交互逻辑
    /// </summary>
    public partial class WindowTitleView : UserControl
    {
        private SoftwareViewModel? _softwareViewModel;

        public WindowTitleView()
        {
            InitializeComponent();

            this._softwareViewModel = this.DataContext as SoftwareViewModel;
        }

        #region 消息框

        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (_softwareViewModel.DialogMessage != null)
            {
                _softwareViewModel.DialogMessage.IsEnable = true;
            }
        }

        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (_softwareViewModel.DialogMessage != null)
            {
                _softwareViewModel.DialogMessage.IsEnable = false;
            }
        }

        #endregion

        #region 屏幕亮度调节
        private double _lastBright;

        private void BrightToggleButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (this.BrightButton.IsChecked == true)
            {
                this._lastBright = this._softwareViewModel.CurrentBright;
                this._softwareViewModel.CurrentBright = 0;
            }
            else
            {
                this._softwareViewModel.CurrentBright = this._lastBright;
            }
        }

        private void Bright_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (this.BrightPopup.IsOpen)
            {
                int baseValue = e.Delta < 0 ? -1 : 1;

                var value = this._softwareViewModel.CurrentBright + 5 * baseValue;

                if (value > 100)
                {
                    value = 100;
                }
                else if (value < 0)
                {
                    value = 0;
                }

                if (this._softwareViewModel.CurrentBright != value)
                {
                    this._softwareViewModel.CurrentBright = value;
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

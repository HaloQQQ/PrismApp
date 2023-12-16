using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace MusicPlayerModule.Views
{
    /// <summary>
    /// MusicLyricDesktopWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MusicLyricDesktopWindow : Window
    {
        public MusicLyricDesktopWindow()
        {
            InitializeComponent();

            this.SetBinding(Window.VisibilityProperty, new Binding("IsDesktopLyricShow")
            {
                Converter = new BooleanToVisibilityConverter()
            });
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

        private void DragWindow_MouseDonw(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}

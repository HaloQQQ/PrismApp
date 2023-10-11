using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyApp.Prisms.Views
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void UIElement_OnKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (sender is FrameworkElement element && element.DataContext is IceTea.NetCore.Utils.HotKeyModel model)
            {
                model.Fill(e);
            }
        }
    }
}
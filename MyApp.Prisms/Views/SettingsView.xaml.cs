using IceTea.Atom.Utils.HotKey.Contracts;
using IceTea.Atom.Utils.HotKey.Global;
using IceTea.General.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyApp.Prisms.Views
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void GlobalHotKeyTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (sender is FrameworkElement element && element.DataContext is IHotKey<CustomKeys, CustomModifierKeys> model)
            {
                model.Fill(e);
            }
        }

        private void AppHotKeyTextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            if (sender is FrameworkElement element && element.DataContext is IHotKey<Key, ModifierKeys> model)
            {
                model.Fill(e);
            }
        }
    }
}
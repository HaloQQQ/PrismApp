using IceTea.Wpf.Atom.Extensions;
using System.Windows;
using System.Windows.Controls;

namespace MusicPlayerModule.Views
{
    /// <summary>
    /// MusicListView.xaml 的交互逻辑
    /// </summary>
    public partial class MusicListView : UserControl
    {
        public MusicListView()
        {
            InitializeComponent();
        }

        private void PreventDataGridDbClick_Handler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as DependencyObject;

            if (originalSource.GetVisualAncestor<DataGridRow>() is null)
            {
                e.Handled = true;
            }
        }
    }
}

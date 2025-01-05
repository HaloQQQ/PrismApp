using IceTea.Wpf.Atom.Extensions;
using MusicPlayerModule.Models;
using MusicPlayerModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

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

        /// <summary>
        /// 播放当前列表
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

            if (sender is DataGridRow row && row.DataContext is FavoriteMusicViewModel music)
            {
                var dataGrid = row.GetVisualAncestor<DataGrid>();

                var items = dataGrid.Items.OfType<FavoriteMusicViewModel>();

                if (items.Any())
                {
                    if (dataGrid.DataContext is MusicPlayerViewModel musicPlayerViewModel)
                    {
                        musicPlayerViewModel.PlayCurrentItemsCommand.Execute(new BatchAddAndPlayModel(music, items));
                    }
                }
            }
        }

        /// <summary>
        /// 播放当前选中的分类
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MusicDistribute_ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (sender is ListBoxItem listBoxItem && listBoxItem.DataContext is MusicWithClassifyModel model)
            {
                var listBox = listBoxItem.GetVisualAncestor<ListBox>();

                if (listBox != null && listBox.DataContext is MusicPlayerViewModel musicPlayerViewModel)
                {
                    musicPlayerViewModel.PlayCurrentCategoryCommand.Execute(model);
                }
            }
        }
    }
}

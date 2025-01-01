using IceTea.Wpf.Atom.Extensions;
using MusicPlayerModule.Models;
using MusicPlayerModule.ViewModels;
using System.Windows.Controls;
using System.Windows.Documents;
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
        /// 添加所有到播放列表
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

            if (e.OriginalSource is Run || (e.OriginalSource is TextBlock txt && 16 == txt.ActualHeight))
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
                        musicPlayerViewModel.PlayAndAddCurrentFavoritesCommand.Execute(new BatchAddAndPlayModel(music, items));
                    }
                }
            }
        }

        /// <summary>
        /// 播放分类中的选中音乐集合
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

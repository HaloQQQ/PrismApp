using MusicPlayerModule.MsgEvents.Music;
using Prism.Events;
using System.Windows.Controls;
using Prism.Ioc;
using MusicPlayerModule.ViewModels;

namespace MusicPlayerModule.Views
{
    /// <summary>
    /// OperationButtonsView.xaml 的交互逻辑
    /// </summary>
    public partial class OperationButtonsView : UserControl
    {
        public OperationButtonsView()
        {
            InitializeComponent();

            ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<FocusFavoriteKeyWordTextBoxEvent>().Subscribe(() =>
            {
                if (this.DataContext is MusicPlayerViewModel musicPlayerViewModel)
                {
                    musicPlayerViewModel.DistributeMusicViewModel.CanBatchSelect = false;
                }

                this.FavoritesKeyWordsTxt.Focus();
            });
        }
    }
}

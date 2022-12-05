using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TcpSocket.Models.ProcessService;

namespace TcpSocket.UserControls.Function
{
    public partial class UsrProcessService : UserControl
    {
        public UsrProcessService()
        {
            InitializeComponent();
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            DataGrid dataGrid = e.Source as DataGrid;
            if (dataGrid != null)
            {
                if (dataGrid.DataContext is ObservableCollection<ProcessContext> processContexts)
                {
                    var value = (bool) e.Parameter;
                    foreach (var processContext in processContexts)
                    {
                        processContext.IsChecked = value;
                    }
                }
            }
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (this.DataContext is ProcessServiceContext context)
            {
                context.ProcessListContext.IsSelectedAll = (bool)(e.Source as CheckBox).IsChecked;
            }
        }
    }
}
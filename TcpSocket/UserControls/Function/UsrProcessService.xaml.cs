using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TcpSocket.Helper;
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

            if (e.Command == ApplicationCommands.Open)
            {
                if (e.Parameter.ToString() == "Service")
                {
                    if (e.OriginalSource is FrameworkElement element && element.DataContext is ServiceContext service)
                    {
                        service.ServiceController.Start();
                        Statics.DataContext.ProcessServiceContext.RefreshServices();
                    }
                }
            }
            else if (e.Command == ApplicationCommands.Close)
            {
                if (e.Parameter.ToString() == "Process")
                {
                    if (e.OriginalSource is FrameworkElement element && element.DataContext is ProcessContext process)
                    {
                        Process.GetProcessById(process.Id).Kill();
                        Statics.DataContext.ProcessServiceContext.RefreshProcesses();
                    }
                }
                else if (e.Parameter.ToString() == "Service")
                {
                    if (e.OriginalSource is FrameworkElement element && element.DataContext is ServiceContext service)
                    {
                        service.ServiceController.Stop();
                        Statics.DataContext.ProcessServiceContext.RefreshServices();
                    }
                }
            }
            else if (e.Command == NavigationCommands.Refresh)
            {
                if (e.Source is DataGrid dataGrid)
                {
                    if (dataGrid.Name == "DataGridProcess")
                    {
                        Statics.DataContext.ProcessServiceContext.RefreshProcesses();
                    }
                    else if (dataGrid.Name == "DataGridService")
                    {
                        Statics.DataContext.ProcessServiceContext.RefreshServices();
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
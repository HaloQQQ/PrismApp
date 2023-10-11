using System.Diagnostics;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.Models;
using MyApp.Prisms.ViewModels;

namespace MyApp.Prisms.Views
{
    public partial class ProcessServiceView : UserControl
    {
        private ProcessServiceViewModel _context;
        public ProcessServiceView()
        {
            InitializeComponent();

            this._context = this.DataContext as ProcessServiceViewModel;
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
                        if (service.ServiceController.Status != ServiceControllerStatus.Running)
                        {
                            service.ServiceController.Start();
                            _context.RefreshServices();
                        }
                    }
                }
            }
            else if (e.Command == ApplicationCommands.Close)
            {
                if (e.Parameter.ToString() == "Process")
                {
                    if (e.OriginalSource is FrameworkElement element && element.DataContext is ProcessContext process)
                    {
                        var current = Process.GetProcessById(process.Id);
                        if (!current.HasExited)
                        {
                            current.Kill();

                            _context.RefreshProcesses();
                        }
                    }
                }
                else if (e.Parameter.ToString() == "Service")
                {
                    if (e.OriginalSource is FrameworkElement element && element.DataContext is ServiceContext service)
                    {
                        if (service.ServiceController.CanStop)
                        {
                            service.ServiceController.Stop();
                            _context.RefreshServices();
                        }
                    }
                }
            }
        }
    }
}
using System.Diagnostics;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.Models;
using MyApp.Prisms.ViewModels;

namespace MyApp.Prisms.Views
{
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
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
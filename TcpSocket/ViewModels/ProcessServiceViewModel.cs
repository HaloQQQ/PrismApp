using Helper.AbstractModel;
using Helper.ProcessServices;
using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Windows.Input;
using TcpSocket.Models;

namespace TcpSocket.ViewModels
{
    public class ProcessServiceViewModel : BaseNotifyModel, IDisposable
    {
        public ObservableCollection<ProcessContext> ProcessList { get; private set; } = new ObservableCollection<ProcessContext>();

        public void RefreshProcesses()
        {
            Task.Run(async () =>
            {
                try
                {

                    ProcessContext processContext;
                    if (ProcessList.Count > 0)
                    {
                        Helper.Helper.Invoke(() => ProcessList.Clear());
                    }

                    foreach (var process in ProcessUtil.GetAllProcessList().OrderBy(p => p.Key))
                    {
                        if (_disposed)
                        {
                            break;
                        }

                        processContext = new ProcessContext(process.Value);
                        Helper.Helper.Invoke(() => ProcessList.Add(processContext));

                        await Task.Delay(50);
                    }
                }
                catch (Exception ex)
                {
                }
            });
        }

        public ObservableCollection<ServiceContext> ServiceList { get; private set; } = new ObservableCollection<ServiceContext>();

        public void RefreshServices()
        {
            Task.Run(async () =>
            {
                try
                {
                    ServiceContext serviceContext;
                    if (ServiceList.Count > 0)
                    {
                        Helper.Helper.Invoke(() => ServiceList.Clear());
                    }

                    foreach (var service in ServiceUtil.GetAllNormalServiceList().OrderBy(s => s.Key))
                    {
                        if (_disposed)
                        {
                            break;
                        }

                        serviceContext = new ServiceContext(service.Value);
                        Helper.Helper.Invoke(() => { ServiceList.Add(serviceContext); });

                        await Task.Delay(50);
                    }
                }
                catch (Exception)
                {
                }
            });
        }

        private bool _disposed;
        public void Dispose()
        {
            _disposed = true;

            ProcessList.Clear();
            ProcessList = null;
            foreach (var item in this.ServiceList)
            {
                item.Dispose();
            }
            ServiceList.Clear();
            ServiceList = null;
        }

        public ProcessServiceViewModel()
        {
            RefreshProcesses();

            RefreshServices();

            this.SelectAllCommand = new DelegateCommand<bool?>(isChecked =>
            {
                foreach (var item in ProcessList)
                {
                    item.IsChecked = (bool)isChecked;
                }
            });

            this.StartServiceCommand = new DelegateCommand(() =>
            {
                var service = this.CurrentService;
                if (service != null)
                {
                    if (service.ServiceController.Status != ServiceControllerStatus.Running)
                    {
                        service.ServiceController.Start();
                        this.RefreshServices();
                    }
                }
            });

            this.StopServiceCommand = new DelegateCommand(() =>
            {
                var service = this.CurrentService;
                if (service != null)
                {
                    if (service.ServiceController.CanStop)
                    {
                        service.ServiceController.Stop();
                        this.RefreshServices();
                    }
                }
            });

            this.ReloadServiceCommand = new DelegateCommand(() => this.RefreshServices());

            this.StopProcessCommand = new DelegateCommand(() =>
            {
                var process = this.CurrentProcess;
                if (process != null)
                {
                    var current = Process.GetProcessById(process.Id);
                    if (!current.HasExited)
                    {
                        current.Kill();

                        this.RefreshProcesses();
                    }
                }
            });

            this.ReloaProcessCommand = new DelegateCommand(() => this.RefreshProcesses());
        }

        public ProcessContext CurrentProcess { get; set; }
        public ServiceContext CurrentService { get; set; }

        public ICommand SelectAllCommand { get; private set; }

        public ICommand StartServiceCommand { get; private set; }

        public ICommand StopServiceCommand { get; private set; }

        public ICommand ReloadServiceCommand { get; private set; }

        public ICommand StopProcessCommand { get; private set; }

        public ICommand ReloaProcessCommand { get; private set; }
    }
}
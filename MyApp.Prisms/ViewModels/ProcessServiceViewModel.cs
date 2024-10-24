using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MyApp.Prisms.Models;
using IceTea.Core.Utils.OS;
using System.ServiceProcess;
using IceTea.Atom.Extensions;
using IceTea.Wpf.Atom.Utils;
using IceTea.Atom.BaseModels;

namespace MyApp.Prisms.ViewModels
{
    internal class ProcessServiceViewModel : BaseNotifyModel, IDisposable
    {
        public ObservableCollection<ProcessContext> ProcessList { get; private set; } = new ObservableCollection<ProcessContext>();

        public void RefreshProcesses()
        {
            Task.Run(async () =>
            {
                try
                {
                    if (ProcessList.Count > 0)
                    {
                        CommonAtomUtils.BeginInvokeAtOnce(() => ProcessList.Clear());
                    }

                    foreach (var process in ProcessUtil.GetAllProcessList().OrderBy(p => p.Key))
                    {
                        if (_disposed)
                        {
                            break;
                        }

                        ProcessContext processContext = new ProcessContext(process.Value);
                        CommonAtomUtils.BeginInvoke(() => ProcessList.Add(processContext));

                        await Task.Delay(50);
                    }
                }
                catch (Exception)
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
                    if (ServiceList.Count > 0)
                    {
                        CommonAtomUtils.BeginInvokeAtOnce(() => ServiceList.Clear());
                    }

                    foreach (var service in ServiceUtil.GetAllNormalServiceList().OrderBy(s => s.Key))
                    {
                        if (_disposed)
                        {
                            break;
                        }

                        ServiceContext serviceContext = new ServiceContext(service.Value);
                        CommonAtomUtils.BeginInvoke(() => { ServiceList.Add(serviceContext); });

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

            ProcessContext.SelectStatusChanged += status =>
            {
                if (status != IsSelectedAllProcesses)
                {
                    if (status && this.ProcessList.Any(p => !p.IsChecked))
                    {
                        return;
                    }

                    _isSelectedAllProcesses = status;

                    RaisePropertyChanged(nameof(IsSelectedAllProcesses));
                }
            };

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

        private bool _isSelectedAllProcesses;
        public bool IsSelectedAllProcesses
        {
            get => _isSelectedAllProcesses;
            set
            {
                if (SetProperty(ref _isSelectedAllProcesses, value))
                {
                    this.ProcessList.ForEach(i => i.IsChecked = value);
                }
            }
        }

        public ProcessContext CurrentProcess { get; set; }
        public ServiceContext CurrentService { get; set; }

        public ICommand StartServiceCommand { get; private set; }

        public ICommand StopServiceCommand { get; private set; }

        public ICommand ReloadServiceCommand { get; private set; }

        public ICommand StopProcessCommand { get; private set; }

        public ICommand ReloaProcessCommand { get; private set; }
    }
}
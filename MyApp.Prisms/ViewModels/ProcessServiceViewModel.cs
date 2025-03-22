using Prism.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MyApp.Prisms.Models;
using System.ServiceProcess;
using IceTea.Atom.Extensions;
using IceTea.Wpf.Atom.Utils;
using IceTea.Atom.BaseModels;
using MyApp.Prisms.Helper;
using IceTea.Core.Utils.OS;

namespace MyApp.Prisms.ViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
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
                        WpfAtomUtils.BeginInvokeAtOnce(() => ProcessList.Clear());
                    }

                    foreach (var process in ProcessUtil.GetAllProcessList().OrderBy(p => p.Key))
                    {
                        if (_disposed)
                        {
                            break;
                        }

                        ProcessContext processContext = new ProcessContext(process.Value);
                        WpfAtomUtils.BeginInvoke(() => ProcessList.Add(processContext));

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
                        WpfAtomUtils.BeginInvokeAtOnce(() => ServiceList.Clear());
                    }

                    foreach (var service in ServiceUtil.GetAllNormalServiceList().OrderBy(s => s.Key))
                    {
                        if (_disposed)
                        {
                            break;
                        }

                        ServiceContext serviceContext = new ServiceContext(service.Value);
                        WpfAtomUtils.BeginInvoke(() => ServiceList.Add(serviceContext));

                        await Task.Delay(50);
                    }
                }
                catch (Exception ex)
                {
                    Helper.Helper.Log(CustomConstants.LogType.Exception_Log_Dir, $"{nameof(ProcessServiceViewModel)}.Service loading error!{ex.Message}");
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
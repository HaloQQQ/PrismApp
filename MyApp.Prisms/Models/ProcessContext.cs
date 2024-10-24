using IceTea.Atom.BaseModels;
using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace MyApp.Prisms.Models
{
    public class ProcessContext : BaseNotifyModel
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (SetProperty(ref _isChecked, value))
                {
                    SelectStatusChanged?.Invoke(value);
                }
            }
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public static event Action<bool> SelectStatusChanged;

        public ProcessContext(Process process)
        {
            Id = process.Id;
            Name = process.ProcessName;
        }
    }

    public class ServiceContext : IDisposable
    {
        public string ServiceName { get;  }
        public string DisplayName { get; }
        public string ServiceControllerStatus { get;  }
        public string ServiceType { get; }

        public string ServiceStartMode { get; }

        public ServiceController ServiceController { get; private set; }

        public ServiceContext(ServiceController service)
        {
            ServiceName = service.ServiceName;
            DisplayName = service.DisplayName;
            ServiceType = service.ServiceType.ToString();
            ServiceStartMode = service.StartType.ToString();
            ServiceControllerStatus = service.Status.ToString();

            ServiceController = service;
        }

        public void Dispose()
        {
            this.ServiceController.Dispose();
            this.ServiceController = null;
        }
    }
}
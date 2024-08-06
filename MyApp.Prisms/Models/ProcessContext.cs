using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace MyApp.Prisms.Models
{
    public class ProcessContext : BindableBase
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
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public string ServiceControllerStatus { get; set; }
        public string ServiceType { get; set; }

        public string ServiceStartMode { get; set; }

        public ServiceController ServiceController { get; set; }

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
            this.ServiceController = null;
        }
    }
}
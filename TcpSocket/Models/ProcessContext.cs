using Helper.NetCore.AbstractModel;
using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace TcpSocket.Models
{
    public class ProcessContext : BaseNotifyModel
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                CallModel();
            }
        }

        public int Id { get; set; }
        public string Name { get; set; }

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
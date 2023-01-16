using System;
using System.Diagnostics;
using System.ServiceProcess;
using Helper.AbstractModel;

namespace TcpSocket.Models.ProcessService
{
    public class ProcessContext : BaseNotifyModel, ISelectNotify
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get => this._isChecked;
            set
            {
                this._isChecked = value;
                
                this.SelecteChanged?.Invoke(value);
            }
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public ProcessContext(Process process)
        {
            this.Id = process.Id;
            this.Name = process.ProcessName;
        }

        public void Select(bool value)
        {
            this._isChecked = value;
                
            CallModel(nameof(this.IsChecked));
        }

        public event Action<bool>? SelecteChanged;
    }

    public class ServiceContext
    {
        public string ServiceName { get; set; }
        public string DisplayName { get; set; }
        public string ServiceControllerStatus { get; set; }
        public string ServiceType { get; set; }

        public string ServiceStartMode { get; set; }

        public ServiceController ServiceController { get; set; }

        public ServiceContext(ServiceController service)
        {
            this.ServiceName = service.ServiceName;
            this.DisplayName = service.DisplayName;
            this.ServiceType = service.ServiceType.ToString();
            this.ServiceStartMode = service.StartType.ToString();
            this.ServiceControllerStatus = service.Status.ToString();

            this.ServiceController = service;
        }
    }
}
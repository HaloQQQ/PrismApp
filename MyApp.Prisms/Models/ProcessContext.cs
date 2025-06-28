using IceTea.Atom.BaseModels;
using System;
using System.Diagnostics;
using System.ServiceProcess;

namespace MyApp.Prisms.Models
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    public class ProcessContext : NotifyBase
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
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
            this.ServiceController = null;
        }
    }
}
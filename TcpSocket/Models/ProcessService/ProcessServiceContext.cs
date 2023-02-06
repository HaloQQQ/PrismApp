using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Helper.AbstractModel;
using Helper.ProcessServices;

namespace TcpSocket.Models.ProcessService
{
    public class ProcessServiceContext : BaseNotifyModel
    {
        public CanSelectListModel<ProcessContext> ProcessListContext { get; set; }

        public void RefreshProcesses()
        {
            Task.Run(async () =>
            {
                ProcessContext processContext;
                if (this.ProcessListContext.AllCount > 0)
                {
                    this.ProcessListContext.Clear();
                }

                foreach (var process in ProcessUtil.GetAllProcessList())
                {
                    processContext = new ProcessContext(process.Value);
                    Helper.Helper.Invoke(() => this.ProcessListContext.Add(processContext));

                    await Task.Delay(100);
                }
            });
        }

        public ObservableCollection<ServiceContext> ServiceList { get; set; }

        public void RefreshServices()
        {
            Task.Run(async () =>
            {
                ServiceContext serviceContext;
                if (this.ServiceList.Count > 0)
                {
                    this.ServiceList.Clear();
                }

                foreach (var service in ServiceUtil.GetAllNormalServiceList())
                {
                    serviceContext = new ServiceContext(service.Value);
                    Helper.Helper.Invoke(() => { this.ServiceList.Add(serviceContext); });

                    await Task.Delay(50);
                }
            });
        }

        public ProcessServiceContext()
        {
            this.ProcessListContext = new CanSelectListModel<ProcessContext>();
            this.ServiceList = new ObservableCollection<ServiceContext>();

            this.RefreshProcesses();

            this.RefreshServices();
        }
    }
}
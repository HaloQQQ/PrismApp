using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Helper.AbstractModel;
using Helper.ProcessServices;

namespace TcpSocket.Models.ProcessService
{
    public class ProcessServiceContext : BaseNotifyModel
    {
        public CanSelectListModel<ProcessContext> ProcessListContext { get; set; }

        public ObservableCollection<ServiceContext> ServiceList { get; set; }

        public ProcessServiceContext()
        {
            this.ProcessListContext = new CanSelectListModel<ProcessContext>();
            this.ServiceList = new ObservableCollection<ServiceContext>();
            
            Task.Run(async () =>
            {
                ProcessContext processContext;
                foreach (var process in ProcessUtil.GetAllProcessList().Values)
                {
                    processContext = new ProcessContext(process);
                    Helper.Helper.Invoke(() => { this.ProcessListContext.Add(processContext); });

                    await Task.Delay(100);
                }
            });

            Task.Run(async () =>
            {

                ServiceContext serviceContext = null;
                foreach (var service in ServiceUtil.GetAllNormalServiceList().Values)
                {
                    serviceContext = new ServiceContext(service);
                    Helper.Helper.Invoke(() => { this.ServiceList.Add(serviceContext); });

                    await Task.Delay(50);
                }
            });
        }
    }
}
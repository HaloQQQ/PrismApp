using WpfStyleResources.Interfaces;

namespace TcpSocket.ViewModels
{
    public class AnotherTcpServerViewModel : TcpServerViewModel
    {
        public AnotherTcpServerViewModel(IConfigManager config) : base(config)
        {
            this.IP = "127.0.0.1";
            this.Port = "50001";
            this.Name = "程序服务器";

            this.IsLogging = config.IsTrue(new string[] { "IsLogging", this.Name });
        }
    }
}

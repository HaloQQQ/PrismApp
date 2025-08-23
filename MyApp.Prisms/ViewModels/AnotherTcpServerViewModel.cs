using IceTea.Pure.Contracts;

namespace MyApp.Prisms.ViewModels
{
    internal class AnotherTcpServerViewModel : TcpServerViewModel
    {
        public AnotherTcpServerViewModel(IConfigManager config, string name = "程序服务器") : base(config, name)
        {
            this.Port = "50001";
        }
    }
}

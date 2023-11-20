using IceTea.Atom.Contracts;

namespace MyApp.Prisms.ViewModels
{
    internal class AnotherTcpServerViewModel : TcpServerViewModel
    {
        public AnotherTcpServerViewModel(IConfigManager config) : base(config)
        {
            this.Port = "50001";
            this.Name = "程序服务器";

            this.IsLogging = config.IsTrue(new string[] { "IsLogging", this.Name });
        }
    }
}

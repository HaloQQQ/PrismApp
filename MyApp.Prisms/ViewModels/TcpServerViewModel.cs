using IceTea.Atom.Contracts;
using MyApp.Prisms.ViewModels.BaseViewModels;

namespace MyApp.Prisms.ViewModels
{
    internal class TcpServerViewModel : BaseSocketViewModel
    {
        public TcpServerViewModel(IConfigManager config) : base(config)
        {
            this.IP = "127.0.0.1";
            this.Port = "50000";
            this.Name = "设备服务器";

            this.IsLogging = config.IsTrue(new string[] { "IsLogging", this.Name });
        }

        public ushort MaxClientCount { get; set; } = 10;

        public bool CanReConnect { get; private set; }
    }
}

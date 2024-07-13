using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Tcp;
using IceTea.SocketStandard.Tcp.Contracts;
using MyApp.Prisms.ViewModels.BaseViewModels;
using System.Text;

namespace MyApp.Prisms.ViewModels
{
    internal class TcpClientViewModel : BaseSocketViewModel
    {
        public TcpClientViewModel(IConfigManager config) : base(config, "Tcp客户端")
        {
        }

        private ITcpClient _tcpClient;

        protected override bool InitSocket()
        {
            if (this._tcpClient.IsNotNullAnd(client => client.IsTryConnecting))
            {
                return false;
            }

            this.Socket = this._tcpClient = new NewTcpClient(Encoding.UTF8, this.Ip, this._port, this.Name);
            this._tcpClient.TryReConnect = TryReConnect;
            this._tcpClient.ReConnectPeriodMilliseconds = 3000;

            return true;
        }

        private bool _tryReConnect;

        public bool TryReConnect
        {
            get => this._tryReConnect;
            set
            {
                if (SetProperty<bool>(ref _tryReConnect, value))
                {
                    if (this.Socket.IsNotNullAnd())
                    {
                        this._tcpClient.TryReConnect = value;
                    }
                }
            }
        }
    }
}

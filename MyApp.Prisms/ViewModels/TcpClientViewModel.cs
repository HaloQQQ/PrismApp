using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Tcp;
using IceTea.SocketStandard.Tcp.Contracts;
using MyApp.Prisms.ViewModels.BaseViewModels;
using System.Text;

namespace MyApp.Prisms.ViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal class TcpClientViewModel : SocketViewModelBase
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

            this.Socket = this._tcpClient = new NewTcpClient(Encoding.UTF8, this.TryReConnect, this.Ip, this._port, this.Name);
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

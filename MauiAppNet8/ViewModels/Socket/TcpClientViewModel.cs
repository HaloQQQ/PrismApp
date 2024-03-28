using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Base;
using IceTea.SocketStandard.Tcp;
using System.Text;

namespace MauiAppNet8.ViewModels.Socket
{
    internal class TcpClientViewModel : BaseSocketViewModel
    {
        private ITcpClient _tcpClient;

        protected override bool InitSocket()
        {
            if (this._tcpClient.IsNotNullAnd(client => client.IsTryConnecting))
            {
                return false;
            }

            this.Socket = this._tcpClient = new NewTcpClient(Encoding.UTF8, this.Ip ?? this.DefaultIp, this._port);

            this._tcpClient.TryReConnect = TryReConnect;
            this._tcpClient.ReConnectPeriodMilliseconds = 3000;

            this.Socket.Started += socket => this.Message += $"{socket}已连接到服务器..".AppendLineOr();

            this.Socket.StartFailed += remoteSocket => this.Message += $"连接{remoteSocket}失败..".AppendLineOr();

            this.Socket.ExceptionOccurred += (socketName, exception) =>
                this.Message += exception.Message.AppendLineOr();

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

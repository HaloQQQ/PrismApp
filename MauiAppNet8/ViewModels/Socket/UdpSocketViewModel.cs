using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Udp;
using IceTea.SocketStandard.Udp.Contracts;
using System.Text;

namespace MauiAppNet8.ViewModels.Socket
{
    internal class UdpSocketViewModel : BaseSocketViewModel
    {
        private IUdpSocket _udpSocket;

        protected override bool InitSocket()
        {
            ushort remotePort = this.DefaultRemotePort;
            if (!this.RemotePort.IsNullOrBlank() && !ushort.TryParse(this.RemotePort, out remotePort))
            {
                Shell.Current.DisplayAlert("验证错误", "远程端口无效", "知道了");
                return false;
            }

            this.Socket = this._udpSocket = new NewUdpSocket(Encoding.UTF8, false, this.Ip ?? this.DefaultIp, this._port, this.RemoteIp ?? this.DefaultIp, remotePort);

            this._udpSocket.UnreachableDisconnect = this.UnreachableDisConnect;

            Socket.Started += ip => this.Message += $"{ip}已启动".AppendLineOr();
            Socket.StartFailed += ip => this.Message += $"{ip}连接失败".AppendLineOr();


            Socket.ReceivedMessage += (from, to, bytes) =>
            {
                string[] arr = from.ToString()!.Split(":");
                this.RemoteIp = arr[0];
                this.RemotePort = arr[1];
            };

            this.Socket.ExceptionOccurred += (socketName, exception) =>
                this.Message += exception.Message.AppendLineOr();

            return true;
        }

        private string _remoteIp;

        public string RemoteIp
        {
            get => _remoteIp;
            set => SetProperty(ref _remoteIp, value);
        }

        public ushort DefaultRemotePort { get; } = 50001;

        private string _remotePort;

        public string RemotePort
        {
            get => _remotePort;
            set => SetProperty(ref _remotePort, value);
        }

        private bool _unreachableDisConnect;

        public bool UnreachableDisConnect
        {
            get => this._unreachableDisConnect;
            set
            {
                if (SetProperty<bool>(ref _unreachableDisConnect, value))
                {
                    if (this.Socket.IsNotNullAnd())
                    {
                        this._udpSocket.UnreachableDisconnect = value;
                    }
                }
            }
        }
    }
}

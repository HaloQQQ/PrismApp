using IceTea.SocketStandard.Udp;
using MyApp.Prisms.ViewModels.BaseViewModels;
using System.Text;
using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Base;

namespace MyApp.Prisms.ViewModels
{
    internal class UdpSocketViewModel : BaseSocketViewModel
    {
        public UdpSocketViewModel(IConfigManager config) : base(config)
        {
            Name = "UDP客户端";

            this.IsLogging = config.IsTrue("IsLogging:" + this.Name);

            this.RemoteIp = this.DefaultIp;
        }

        private IUdpSocket _udpSocket;

        protected override bool InitSocket()
        {
            this.Socket = this._udpSocket = new NewUdpSocket(Encoding.UTF8, false, this.Ip ?? this.DefaultIp, this._port, this.RemoteIp ?? this.DefaultIp, this._remotePort, this.Name);

            this._udpSocket.UnreachableDisconnect = this.UnreachableDisConnect;

            this.Socket.ReceivedMessage += (from, to, bytes) =>
            {
                string[] arr = from.ToString()!.Split(":");
                this.RemoteIp = arr[0];
                this.RemotePort = arr[1];
            };

            return true;
        }

        private string _remoteIp;

        public string RemoteIp
        {
            get => _remoteIp;
            set => SetProperty(ref _remoteIp, value);
        }

        protected ushort _remotePort = 50001;

        public string RemotePort
        {
            get => _remotePort.ToString();
            set => SetProperty(ref _remotePort, ushort.Parse(value));
        }

        private bool _unreachableDisConnect;

        public bool UnreachableDisConnect
        {
            get => this._unreachableDisConnect;
            set
            {
                if(SetProperty<bool>(ref _unreachableDisConnect, value))
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

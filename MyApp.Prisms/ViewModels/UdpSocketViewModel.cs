using IceTea.SocketStandard.Udp;
using MyApp.Prisms.ViewModels.BaseViewModels;
using System.Text;
using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Base;
using Prism.Events;
using Prism.Ioc;
using PrismAppBasicLib.MsgEvents;
using IceTea.Atom.GeneralModels;

namespace MyApp.Prisms.ViewModels
{
    internal class UdpSocketViewModel : BaseSocketViewModel
    {
        public UdpSocketViewModel(IConfigManager config) : base(config)
        {
            Name = "UDP客户端";

            this.IsLogging = config.IsTrue("IsLogging:" + this.Name);

            this.RemoteIp = AppStatics.Ip;
        }

        private IUdpSocket _udpSocket;

        protected override bool InitSocket()
        {
            if (this.RemoteIp.IsNullOrBlank())
            {
                ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<DialogMessageEvent>().Publish(new DialogMessage("远程Ip无效"));
                return false;
            }

            if (!ushort.TryParse(this.RemotePort, out ushort remotePort))
            {
                ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<DialogMessageEvent>().Publish(new DialogMessage("远程端口无效"));
                return false;
            }

            this.Socket = this._udpSocket = new NewUdpSocket(Encoding.UTF8, false, this.Ip, this._port, this.RemoteIp, remotePort, this.Name);

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

        private string _remotePort = "50001";

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

using IceTea.SocketStandard.Udp;
using MyApp.Prisms.ViewModels.BaseViewModels;
using IceTea.Pure.Contracts;
using IceTea.Pure.Extensions;
using Prism.Events;
using Prism.Ioc;
using IceTea.SocketStandard.Udp.Contracts;
using PrismAppBasicLib.Contracts;

namespace MyApp.Prisms.ViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal class UdpSocketViewModel : SocketViewModelBase
    {
        public UdpSocketViewModel(IConfigManager config) : base(config, "UDP客户端")
        {
            this.RemoteIp = AppStatics.Ip;
        }

        private IUdpSocket _udpSocket;
        protected override bool InitSocket()
        {
            if (this.RemoteIp.IsNullOrBlank())
            {
                CommonUtil.PublishMessage(ContainerLocator.Current.Resolve<IEventAggregator>(), "远程Ip无效");
                return false;
            }

            if (!ushort.TryParse(this.RemotePort, out ushort remotePort))
            {
                CommonUtil.PublishMessage(ContainerLocator.Current.Resolve<IEventAggregator>(), "远程端口无效");
                return false;
            }

            this.Socket = this._udpSocket = new NewUdpSocket(this.UnreachableDisconnect, this.Ip, this._port, this.RemoteIp, remotePort, this.Name);

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

        private bool _unreachableDisconnect;

        public bool UnreachableDisconnect
        {
            get => this._unreachableDisconnect;
            set
            {
                if (SetProperty<bool>(ref _unreachableDisconnect, value))
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

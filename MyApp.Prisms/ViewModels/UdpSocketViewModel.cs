using IceTea.Pure.Contracts;
using IceTea.Pure.Businesses.Config;
using IceTea.Pure.Extensions;
using IceTea.SocketStandard.Udp;
using MyApp.Prisms.ViewModels.BaseViewModels;
using Prism.Events;
using Prism.Ioc;
using PrismAppBasicLib.Contracts;
using System.ComponentModel.DataAnnotations;

namespace MyApp.Prisms.ViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal class UdpSocketViewModel : SocketViewModelBase
    {
        public UdpSocketViewModel(IConfigManager config) : base(config, "UDP客户端")
        {
            this.RemoteIp = AppStatics.Ip.ToString();

            this.ConnectCommand
                .ObservesProperty(() => this.RemoteIp)
                .ObservesProperty(() => this.RemotePort);
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
                string[] arr = from.ToString()!.Split(':');
                this.RemoteIp = arr[0];
                this.RemotePort = arr[1];
            };

            return true;
        }

        private string _remoteIp;
        [RegularExpression(RegexConstants.IPv4PatternStr, ErrorMessage = "必须是IP格式")]
        public string RemoteIp
        {
            get => _remoteIp;
            set
            {
                if (SetProperty<string>(ref _remoteIp, value))
                {
                    ValidateNotifyDataError();
                }
            }
        }

        private int _remotePort = 50001;
        [RegularExpression(RegexConstants.PortPatternStr, ErrorMessage = "必须是端口格式")]
        public string RemotePort
        {
            get => _remotePort.ToString();
            set
            {
                var portStr = value;
                if (_remotePort.ToString() != portStr)
                {
                    if (ushort.TryParse(portStr, out var port))
                    {
                        _remotePort = port;
                    }
                    else
                    {
                        RaisePropertyChanged();
                        ValidateNotifyDataError();
                    }
                }
            }
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

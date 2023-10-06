using System;
using TcpSocket.ViewModels.BaseViewModels;
using WpfStyleResources.Helper;

namespace TcpSocket.ViewModels
{
    public class UDPViewModel : BaseSocketViewModel
    {
        public UDPViewModel(IConfigManager config) : base(config)
        {
            IP = "127.0.0.1";
            Port = "50000";

            TargetIP = "127.0.0.1";
            TargetPort = "50001";

            Name = "UDP客户端";

            this.IsLogging = config.IsTrue("IsLogging:" + this.Name);
        }

        private string _targetIP;

        public string TargetIP
        {
            get => this._targetIP;
            set => SetProperty<string>(ref _targetIP, value);
        }

        private string _targetPort;

        public string TargetPort
        {
            get => this._targetPort;
            set => SetProperty<string>(ref _targetPort, value);
        }

        private bool _unreachableDisconnect;

        public bool UnreachableDisconnect
        {
            get => this._unreachableDisconnect;
            set
            {
                this._unreachableDisconnect = value;

                this.UnreachableDisconnectChanged?.Invoke(this._unreachableDisconnect);
            }
        }

        public event Action<bool> UnreachableDisconnectChanged;
    }
}
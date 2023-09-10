using System;
using TcpSocket.ViewModels.BaseViewModels;

namespace TcpSocket.ViewModels
{
    public class TcpClientViewModel : BaseSocketViewModel
    {
        public TcpClientViewModel(WpfStyleResources.Interfaces.IConfigManager config) : base(config)
        {
            IP = "127.0.0.1";
            Port = "50000";
            MaxMessageLength = 256;
            Name = "Tcp客户端";

            this.IsLogging = config.IsTrue("IsLogging:" + this.Name);
        }

        private bool _canReConnect;

        public bool CanReConnect
        {
            get => this._canReConnect;

            set
            {
                SetProperty<bool>(ref _canReConnect, value);

                this.CanReConnectChanged?.Invoke(this._canReConnect);
            }
        }

        public event Action<bool> CanReConnectChanged;
    }
}

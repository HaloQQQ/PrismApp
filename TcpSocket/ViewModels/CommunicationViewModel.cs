using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using TcpSocket.Views;

namespace TcpSocket.ViewModels
{
    internal class CommunicationViewModel : BindableBase
    {
        public CommunicationViewModel(UDPViewModel udpContext,
            TcpClientViewModel applicationClientContext,
            TcpServerViewModel machineServerContext,
            AnotherTcpServerViewModel applicationServerContext)
        {
            IsTcp = true;
            IsTcpServer = true;

            this.UdpContext = udpContext;

            this.MachineServerContext = machineServerContext;
            this.ApplicationServerContext = applicationServerContext;
            this.ApplicationClientContext = applicationClientContext;
        }

        public UDPViewModel UdpContext { get; private set; }

        #region TCP
        public TcpServerViewModel MachineServerContext { get; private set; }
        public AnotherTcpServerViewModel ApplicationServerContext { get; private set; }
        public TcpClientViewModel ApplicationClientContext { get; private set; }

        public ISet<TcpServerView> Servers = new HashSet<TcpServerView>();

        public void AddServer(TcpServerView server)
        {
            this.Servers.Add(server);

            server.SetMediator(this);
        }

        /// <summary>
        /// 服务器之间为客户端转发消息
        /// </summary>
        /// <param name="server"></param>
        /// <param name="data"></param>
        public void TransmitFrom(TcpServerView server, byte[] data)
        {
            foreach (var item in this.Servers.Where(i => i != server))
            {
                item.TransmitMessage(data);
            }
        }

        #endregion

        private bool _isTcp;

        public bool IsTcp
        {
            get => this._isTcp;
            set => SetProperty<bool>(ref _isTcp, value);
        }

        private bool _isServer;

        public bool IsTcpServer
        {
            get => this._isServer;
            set => SetProperty<bool>(ref _isServer, value);
        }

        private bool _isHex;

        public bool IsHex
        {
            get => this._isHex;
            set
            {
                if (SetProperty<bool>(ref _isHex, value))
                {
                    this.UdpContext.IsHex = _isHex;
                    this.MachineServerContext.IsHex = _isHex;
                    this.ApplicationServerContext.IsHex = _isHex;
                    this.ApplicationClientContext.IsHex = _isHex;
                }
            }
        }

        public ushort MaxClientCount
        {
            set
            {
                this.ApplicationServerContext.MaxClientCount = value;
                this.MachineServerContext.MaxClientCount = value;
            }
        }
    }
}

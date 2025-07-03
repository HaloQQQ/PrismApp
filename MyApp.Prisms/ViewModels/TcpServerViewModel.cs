using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Tcp;
using IceTea.SocketStandard.Tcp.Contracts;
using IceTea.Wpf.Atom.Utils;
using MyApp.Prisms.ViewModels.BaseViewModels;
using Prism.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace MyApp.Prisms.ViewModels
{
    internal class TcpServerViewModel : SocketViewModelBase
    {
        public TcpServerViewModel(IConfigManager config, string name = "设备服务器") : base(config, name)
        {
            this.SendCommand = new DelegateCommand(
                    () => this.Socket.SendAsync(this.SendMessage),
                    () => this.Socket.IsNotNullAnd(server => server.IsConnected)
                            && !this.SendMessage.IsNullOrBlank()
                            && this.ConnList.Count > 0
                )
                .ObservesProperty(() => this.SendMessage);

            this.RemoveCommand = new DelegateCommand<string>(socketName =>
            {
                if (this.Socket is ITcpServer server)
                {
                    server.DestoryClientHandler(socketName);
                }
            });
        }

        public ICommand RemoveCommand { get; private set; }

        private IList<string> _connList = new ObservableCollection<string>();

        public IList<string> ConnList
        {
            get => this._connList;
            private set => SetProperty<IList<string>>(ref _connList, value);
        }

        private ushort _maxClientsCount;

        public ushort MaxClientsCount
        {
            get => this._maxClientsCount;
            set => SetProperty<ushort>(ref _maxClientsCount, value);
        }

        protected override bool InitSocket()
        {
            ITcpServer tcpServer = new NewTcpServer(Encoding.UTF8, this.Ip, this._port, this.Name,
                messageMaxLength: 256, maxClientsCount: MaxClientsCount);

            tcpServer.SocketCommunicateWithClientCreated += socket =>
            {
                if (socket.RemoteEndPoint != null)
                {
                    WpfAtomUtils.BeginInvokeAtOnce(() =>
                        this.ConnList.Add(socket.RemoteEndPoint.ToString()!)
                    );
                }
            };

            tcpServer.ExceptionOccurred += (socketName, exception) =>
            {
                WpfAtomUtils.BeginInvokeAtOnce(() =>
                {
                    if (this.ConnList.Contains(socketName))
                    {
                        this.ConnList.Remove(socketName);
                    }
                }
                );
            };

            this.Socket = tcpServer;

            return true;
        }
    }
}

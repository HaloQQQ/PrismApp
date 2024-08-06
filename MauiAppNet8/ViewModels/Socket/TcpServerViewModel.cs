using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Tcp;
using IceTea.SocketStandard.Tcp.Contracts;
using Prism.Commands;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace MauiAppNet8.ViewModels.Socket
{
    [QueryProperty("test", "test")]
    internal class TcpServerViewModel : BaseSocketViewModel
    {
        public TcpServerViewModel()
        {
            this.RemoveCommand = new DelegateCommand<string>(socketName =>
            {
                if (this.Socket is ITcpServer server)
                {
                    server.DestoryClientHandler(socketName);
                }
            });

            this.SendCommand = new DelegateCommand(
                    () => this.Socket.SendAsync(this.SendMessage),
                    () => this.Socket.IsNotNullAnd(server => server.IsConnected)
                            && !this.SendMessage.IsNullOrBlank()
                            && this.Clients.Count > 0
                )
                .ObservesProperty(() => this.SendMessage);
        }

        protected override bool InitSocket()
        {
            ITcpServer tcpServer = new NewTcpServer(Encoding.UTF8, this.Ip ?? this.DefaultIp, this._port);

            this.Socket = tcpServer;

            tcpServer.SocketCommunicateWithClientCreated += socket =>
            {
                this.Message += $"连接{socket.LocalEndPoint}=>{socket.RemoteEndPoint}建立!".AppendLineOr();

                if (socket.RemoteEndPoint != null)
                {
                    this.Clients.Add(socket.RemoteEndPoint.ToString());
                }
            };

            this.Socket.Started += socket => this.Message += $"开始监听{socket}..".AppendLineOr();

            this.Socket.ExceptionOccurred += (socketName, exception) =>
            {
                this.Message += exception.Message.AppendLineOr();

                App.Current.Dispatcher.DispatchAsync(() =>
                {
                    if (this.Clients.Contains(socketName))
                    {
                        this.Clients.Remove(socketName);
                    }
                }
                );
            };

            return true;
        }

        #region Commands
        public ICommand RemoveCommand { get; private set; }
        #endregion

        #region Props
        private string _test;

        public string test
        {
            get { return _test; }
            set { _test = value; }
        }

        private ObservableCollection<string> _clients = new();

        public ObservableCollection<string> Clients
        {
            get => _clients;
            set => SetProperty(ref _clients, value);
        }
        #endregion
    }
}

using IceTea.Atom.BaseModels;
using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Base;
using System.Windows.Input;
using Prism.Commands;
using System.Text;
using IceTea.Atom.Utils;
using System.Collections.Generic;
using System.Linq;

namespace MyApp.Prisms.ViewModels.BaseViewModels
{
    internal abstract class BaseSocketViewModel : BaseNotifyModel
    {
        private ISocket _socket;

        public ISocket Socket
        {
            get => this._socket;
            protected set => SetProperty<ISocket>(ref _socket, value);
        }

        public BaseSocketViewModel(IConfigManager config)
        {
            config.SetConfig += config =>
                config.WriteConfigNode<bool>(this.IsLogging, new string[] { "IsLogging", this.Name });

            this.ConnectCommand = new DelegateCommand(this.Connect);

            this.SendCommand = new DelegateCommand(
                    () => this.Socket.SendAsync(this.SendMessage),
                    () => this.Socket.IsNotNullAnd(server => server.IsConnected) && !this.SendMessage.IsNullOrBlank()
                )
                .ObservesProperty(() => this.SendMessage);

            this.Ip = this.DefaultIp;
        }

        public string DefaultIp { get; } = AppStatics.Ip;
        public ushort DefaultPort { get; } = 50000;

        #region Fields
        private string _ip;
        protected ushort _port = 50000;
        private string _sendMessage;

        public string Name { get; set; } = "Socket";

        /// <summary>
        /// 可接受的最长数据长度
        /// </summary>
        public uint MaxMessageLength { get; set; } = 256;

        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public bool IsHex { get; set; }

        private bool _isLogging;
        #endregion

        #region Props
        public bool IsLogging
        {
            get => _isLogging;
            set => SetProperty<bool>(ref _isLogging, value);
        }

        public string Ip
        {
            get => _ip;
            set => SetProperty(ref _ip, value);
        }

        public string Port
        {
            get => _port.ToString();
            set => SetProperty(ref _port, ushort.Parse(value));
        }

        public string SendMessage
        {
            get => this._sendMessage;
            set => SetProperty<string>(ref _sendMessage, value);
        }
        #endregion

        #region Commands
        public ICommand ConnectCommand { get; protected set; }
        public ICommand SendCommand { get; protected set; }
        #endregion

        protected abstract bool InitSocket();

        protected void Connect()
        {
            if (this.Socket.IsNotNullAnd(server => server.IsConnected))
            {
                this.Socket.Close();
                return;
            }

            if (this._port == 0)
            {
                this._port = this.DefaultPort;
            }

            if (this.InitSocket())
            {
                this.Socket.Start();
            }
        }

        public static IEnumerable<string> Ips { get; } =
            AppUtils.GetIpAddressColl().Select(address => address.ToString());
    }
}

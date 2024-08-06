using IceTea.Atom.BaseModels;
using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using System.Net;
using System.Windows.Input;
using Prism.Commands;
using IceTea.SocketStandard.Contracts;

namespace MauiAppNet8.ViewModels.Socket
{
    internal enum CommunicationType : byte
    {
        Recv,
        Send
    }

    internal abstract class BaseSocketViewModel : BaseNotifyModel
    {
        private ISocket _socket;

        public ISocket Socket
        {
            get => this._socket;
            protected set => SetProperty<ISocket>(ref _socket, value);
        }

        public BaseSocketViewModel()
        {
            this.ConnectCommand = new DelegateCommand(this.Connect);

            this.SendCommand = new DelegateCommand(
                    () => this.Socket.SendAsync(this.SendMessage),
                    () => this.Socket.IsNotNullAnd(server => server.IsConnected) && !this.SendMessage.IsNullOrBlank()
                )
                .ObservesProperty(() => this.SendMessage);

            this.CleanMessageCommand = new DelegateCommand(
                    () => this.Message = string.Empty,
                    () => !string.IsNullOrEmpty(this.Message)
                )
                .ObservesProperty(() => this.Message);
        }

        public string DefaultIp { get; } = AppStatics.Ip;
        public ushort DefaultPort { get; } = 50000;

        public string Ip { get; set; }

        protected ushort _port;

        public string Port { get; set; }

        private string _message;

        public string Message
        {
            get => this._message;
            set => SetProperty<string>(ref _message, value);
        }

        private string _sendMessage;

        public string SendMessage
        {
            get => this._sendMessage;
            set => SetProperty<string>(ref _sendMessage, value);
        }

        public ICommand ConnectCommand { get; private set; }
        public ICommand SendCommand { get; protected set; }
        public ICommand CleanMessageCommand { get; private set; }

        protected abstract bool InitSocket();

        protected void Connect()
        {
            this._port = this.DefaultPort;

            if (!this.Port.IsNullOrBlank() && !ushort.TryParse(this.Port, out this._port))
            {
                Shell.Current.DisplayAlert("验证错误", "端口无效", "知道了");
                return;
            }

            if (this.Socket.IsNotNullAnd(server => server.IsConnected))
            {
                this.Socket.Close();
                return;
            }

            if (this.InitSocket())
            {
                this.Socket.ReceivedMessage += (from, to, bytes) =>
                {
                    string message = this.Socket.GetString(bytes).TrimWhiteSpace();

                    //if (message.IsHexString())
                    //{
                    //    message = message.GetStringFromHex();
                    //}

                    this.Message += this.WrapMsg(CommunicationType.Recv, from, to, message);
                };

                this.Socket.SentMessage += (from, to, bytes) =>
                {
                    this.Message += this.WrapMsg(CommunicationType.Send, from, to, this.Socket.GetString(bytes));

                    this.SendMessage = string.Empty;
                };

                this.Socket.Start();
            }
        }


        protected virtual string WrapMsg(string msg) => $"{DateTime.Now.FormatTime()} {msg}{Environment.NewLine}";

        protected virtual string WrapMsg(CommunicationType type, EndPoint from, EndPoint to, string msg) => $"[{DateTime.Now.FormatTime()}]# {type} {from}=>{to}>{Environment.NewLine}{msg}{Environment.NewLine}";
    }
}

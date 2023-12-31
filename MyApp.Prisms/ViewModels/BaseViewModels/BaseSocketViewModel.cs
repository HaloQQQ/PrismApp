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
using DryIoc;
using Prism.Ioc;
using Prism.Events;
using PrismAppBasicLib.MsgEvents;
using IceTea.Atom.GeneralModels;

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

        public BaseSocketViewModel(IConfigManager config, string name)
        {
            this.Name = name;
            this.IsLogging = config.IsTrue(new string[] { "IsLogging", this.Name });

            config.SetConfig += config =>
                config.WriteConfigNode<bool>(this.IsLogging, new string[] { "IsLogging", this.Name });

            this.ConnectCommand = new DelegateCommand(this.Connect);

            this.SendCommand = new DelegateCommand(
                    () => this.Socket.SendAsync(this.SendMessage),
                    () => this.Socket.IsNotNullAnd(server => server.IsConnected) && !this.SendMessage.IsNullOrBlank()
                )
                .ObservesProperty(() => this.SendMessage);

            this.Ip = AppStatics.Ip;
        }

        #region Fields
        private string _sendMessage;

        public string Name { get; protected set; } = "Socket";

        private bool _isLogging;

        protected ushort _port;

        /// <summary>
        /// 可接受的最长数据长度
        /// </summary>
        public uint MaxMessageLength { get; set; } = 256;

        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public bool IsHex { get; set; }
        #endregion

        #region Props

        public bool IsLogging
        {
            get => this._isLogging;
            set => SetProperty<bool>(ref _isLogging, value);
        }

        public string Ip { get; set; }

        public string Port { get; set; } = "50000";

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
            if (this.Ip.IsNullOrBlank())
            {
                ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<DialogMessageEvent>().Publish(new DialogMessage("Ip无效"));
                return;
            }

            if (!ushort.TryParse(this.Port, out this._port))
            {
                ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<DialogMessageEvent>().Publish(new DialogMessage("端口无效"));
                return;
            }

            if (this.Socket.IsNotNullAnd(server => server.IsConnected))
            {
                this.Socket.Close();
                return;
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

using IceTea.Pure.BaseModels;
using IceTea.Pure.Contracts;
using IceTea.Pure.Extensions;
using IceTea.SocketStandard.Contracts;
using System.Windows.Input;
using Prism.Commands;
using System.Text;
using IceTea.Pure.Utils;
using System.Collections.Generic;
using System.Linq;
using DryIoc;
using Prism.Ioc;
using Prism.Events;
using PrismAppBasicLib.Contracts;

namespace MyApp.Prisms.ViewModels.BaseViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal abstract class SocketViewModelBase : NotifyBase
    {
        private ISocket _socket;

        public ISocket Socket
        {
            get => this._socket;
            protected set => SetProperty<ISocket>(ref _socket, value);
        }

        public SocketViewModelBase(IConfigManager config, string name)
        {
            this.Name = name;

            var configKey = new string[] { "IsLogging", this.Name };
            this.IsLogging = config.IsTrue(configKey);

            config.SetConfig += config =>
                config.WriteConfigNode<bool>(this.IsLogging, configKey);

            this.ConnectCommand = new DelegateCommand(this.Connect);

            this.SendCommand = new DelegateCommand(
                    () => this.Socket.SendAsync(this.SendMessage),
                    () => this.Socket.IsNotNullAnd(server => server.IsConnected) && !this.SendMessage.IsNullOrBlank()
                )
                .ObservesProperty(() => this.SendMessage)
                .ObservesProperty(() => this.Socket.IsConnected);

#pragma warning disable CA1416 // 验证平台兼容性
            this.OpenLogCommand = new DelegateCommand(() => CommonUtil.OpenLog(this.Name));

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

        public ICommand OpenLogCommand { get; private set; }
        #endregion

        protected abstract bool InitSocket();

        protected void Connect()
        {
            if (this.Ip.IsNullOrBlank())
            {
                CommonUtil.PublishMessage(ContainerLocator.Current.Resolve<IEventAggregator>(), "Ip无效");
                return;
            }

            if (!ushort.TryParse(this.Port, out this._port))
            {
                CommonUtil.PublishMessage(ContainerLocator.Current.Resolve<IEventAggregator>(), "端口无效");
                return;
            }

            if (this.Socket.IsNotNullAnd(server => server.IsConnected))
            {
                this.Socket.CloseAsync();
                return;
            }

            if (this.InitSocket())
            {
                this.Socket.StartAsync();
            }
        }

        public static IEnumerable<string> Ips { get; } =
            AppUtils.GetIpAddressColl().Select(address => address.ToString());
    }
}

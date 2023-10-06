using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Helper.Utils;
using WpfStyleResources.Helper;

namespace TcpSocket.ViewModels.BaseViewModels
{
    public class BaseSocketViewModel : BindableBase
    {
        public BaseSocketViewModel(IConfigManager config)
        {
            //config.SetConfig += (config, jsonObject) => 
            //    config.SetLogging(jsonObject, this.Name, this.IsLogging);

            config.SetConfig += config =>
                config.WriteConfigNode<bool>(this.IsLogging, new string[] { "IsLogging", this.Name });
        }

        public string Name { get; set; } = "Socket";

        /// <summary>
        /// 可接受的最长数据长度
        /// </summary>
        public uint MaxMessageLength { get; set; } = 256;

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        private bool _isLogging;

        public bool IsLogging
        {
            get => _isLogging;
            set => SetProperty<bool>(ref _isLogging, value);
        }

        public string IpEndPoint => IP + ":" + Port;

        private bool _isConnected;

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty<bool>(ref _isConnected, value);
        }

        private IList<string> _connList = new ObservableCollection<string>();

        public IList<string> ConnList
        {
            get => this._connList;
            set => SetProperty<IList<string>>(ref _connList, value);
        }

        private string _sendMsg;

        public string SendMsg
        {
            get => _sendMsg;
            set => SetProperty<string>(ref _sendMsg, value);
        }

        private string _iP;

        public string IP
        {
            get => _iP;
            set => SetProperty<string>(ref _iP, value);
        }

        private string _port;

        public string Port
        {
            get => _port;
            set => SetProperty<string>(ref _port, value);
        }

        private bool _connecting;

        public bool Connecting
        {
            get => _connecting;
            set => SetProperty<bool>(ref _connecting, value);
        }

        public bool IsHex { get; set; }

        public static IEnumerable<string> Ips { get; } =
            AppUtils.GetIpAddressColl().Select(address => address.ToString());
    }
}
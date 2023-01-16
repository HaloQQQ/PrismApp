using System.Text;
using Helper.AbstractModel;

namespace TcpSocket.Models
{
    public class BaseSocketContext : BaseNotifyModel
    {
        public string Name { get; set; } = "Socket";

        /// <summary>
        /// 可接受的最长数据长度
        /// </summary>
        public uint MaxMessageLength { get; set; } = 256;

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        private bool _isLogging;

        public bool IsLogging
        {
            get => this._isLogging;
            set
            {
                this._isLogging = value;
                CallModel();
            }
        }

        public string IpEndPoint => this.IP + ":" + this.Port;

        private bool _isConnected;

        public bool IsConnected
        {
            get => this._isConnected;
            set
            {
                this._isConnected = value;
                CallModel();
            }
        }


        private string _sendMsg;

        public string SendMsg
        {
            get => this._sendMsg;
            set
            {
                this._sendMsg = value;
                CallModel();
            }
        }

        private string _iP;

        public string IP
        {
            get => this._iP;
            set
            {
                this._iP = value;
                CallModel();
            }
        }

        private string _port;

        public string Port
        {
            get => this._port;
            set
            {
                this._port = value;
                CallModel();
            }
        }

        private volatile bool _connecting;

        public bool Connecting
        {
            get => this._connecting;
            set
            {
                this._connecting = value;
                CallModel();
            }
        }
    }
}
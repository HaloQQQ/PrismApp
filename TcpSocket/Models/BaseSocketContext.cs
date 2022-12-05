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
        
        private bool isConnected;
        public bool IsConnected
        {
            get => this.isConnected;
            set
            {
                this.isConnected = value;

                CallModel();
            }
        }
        

        private string sendMsg = null!;

        public string SendMsg
        {
            get => this.sendMsg;
            set
            {
                this.sendMsg = value;
                CallModel();
            }
        }

        private string iP = null!;

        public string IP
        {
            get => this.iP;
            set
            {
                this.iP = value;
                CallModel();
            }
        }

        private string port = null!;

        public string Port
        {
            get => this.port;
            set
            {
                this.port = value;
                CallModel();
            }
        }
    }
}
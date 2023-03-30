using SocketHelper.Tcp;
using System.Net;
using System.Threading.Tasks;
using TcpSocket.Helper;
using TcpSocket.Models;

namespace TcpSocket.UserControls.Function.Communication
{
    public class UsrCtrlTcpClient : UsrCtrlTcpSocket
    {
        public UsrCtrlTcpClient(TcpSocketContext tcpSocketContext) : base(tcpSocketContext)
        {
        }

        protected override void InitTcpSocket(ushort port)
        {
            if (string.IsNullOrEmpty(base._tcpSocketContext.Name))
            {
                base._tcpSocketContext.Name = "客户端";
            }

            base._tcpSocket = new NewTcpClient(base._tcpSocketContext.Encoding, base._tcpSocketContext.IP, port,
                base._tcpSocketContext.Name, base._tcpSocketContext.MaxMessageLength);

            (base._tcpSocket as NewTcpClient).TryConnecting = base._tcpSocketContext.CanReConnect;
            (base._tcpSocket as NewTcpClient).ReConnectPeriodMilliseconds = 2000;

            // 防止重复订阅
            this._tcpSocketContext.CanReConnectChanged -= this.ReConnectChanged_Handler;

            this._tcpSocketContext.CanReConnectChanged += this.ReConnectChanged_Handler;

            base._tcpSocket.Started += socket =>
            {
                this.rhTxt.Info(this._tcpSocketContext, $"{socket}连接成功");

                Helper.Helper.Invoke(() =>
                {
                    base._tcpSocketContext.ConnList.Add(
                        new IPEndPoint(IPAddress.Parse(this._tcpSocketContext.IP), port).ToString()
                    );
                });
            };

            base._tcpSocket.StartFailed += socket =>
            {
                base._tcpSocketContext.Connecting = base._tcpSocketContext.CanReConnect;

                this.rhTxt.Info(this._tcpSocketContext, $"连接{socket}失败..");
            };
        }

        private void ReConnectChanged_Handler(bool currentStatus)
        {
            (base._tcpSocket as NewTcpClient).TryConnecting = currentStatus;

            if (!currentStatus)
            {
                base._tcpSocketContext.Connecting = false;
            }
        }
    }
}
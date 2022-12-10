using SocketHelper.Tcp;
using System.Net;
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

            base._tcpSocket = new NewTcpClient(base._tcpSocketContext.Encoding, base._tcpSocketContext.IP, port, base._tcpSocketContext.Name, base._tcpSocketContext.MaxMessageLength);

            base._tcpSocket.Started += socket =>
            {
                this.rhTxt.Info( this._tcpSocketContext, "连接成功");

                base._tcpSocketContext.ConnList.Add(
                    new IPEndPoint(IPAddress.Parse(this._tcpSocketContext.IP), port).ToString()
                    );
            };
        }
    }
}

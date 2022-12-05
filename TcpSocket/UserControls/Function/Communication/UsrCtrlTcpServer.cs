using SocketHelper.Tcp;
using System;
using Helper.Utils;
using TcpSocket.Models;

namespace TcpSocket.UserControls.Function.Communication
{
    public class UsrCtrlTcpServer : UsrCtrlTcpSocket
    {
        private MediatorContext _mediatorContext = null!;

        public void SetMediator(MediatorContext mediatorContext)
        {
            this._mediatorContext = mediatorContext;
        }

        public UsrCtrlTcpServer(TcpSocketContext tcpSocketContext) : base(tcpSocketContext)
        {
        }

        protected override void InitTcpSocket(ushort port)
        {
            if (string.IsNullOrEmpty(base._tcpSocketContext.Name))
            {
                base._tcpSocketContext.Name = "服务器";
            }

            base._tcpSocket = new NewTcpServer(base._tcpSocketContext.Encoding, base._tcpSocketContext.IP, port,
                base._tcpSocketContext.Name, base._tcpSocketContext.MaxMessageLength,
                base._tcpSocketContext.MaxClientCount);

            var server = (NewTcpServer) base._tcpSocket;

            server.SocketCommunicateWithClientCreated += socket =>
            {
                this.AppendMsg(GetMessage(socket.RemoteEndPoint!, socket.LocalEndPoint!, "连接建立!"));

                if (socket.RemoteEndPoint != null)
                {
                    base.Dispatcher.Invoke(() =>
                        base._tcpSocketContext.ConnList.Add(socket.RemoteEndPoint.ToString()!)
                    );
                }
            };

            server.Started += socket =>
            {
                base.AppendMsg(base.GetMessage(string.Empty,
                    $"开始监听{base._tcpSocketContext.IP}:{base._tcpSocketContext.Port}.."));
            };

            server.ReceivedMessage += (from, to, data) => { this._mediatorContext.TransmitFrom(this, data); };
        }

        public void TransmitMessage(byte[] data)
        {
            try
            {
                AppUtils.Assert(base._tcpSocket != null, "Socket未初始化!");

                var server = base._tcpSocket as NewTcpServer;

                if (!server!.IsConnected)
                {
                    return;
                }

                AppUtils.Assert(server.Clients.Count > 0,
                    $"【{base._tcpSocketContext.Name}】不存在客户端连接!请连接后重试!");

                var msg = this.ResolveMsg(base._tcpSocket!.GetString(data).Trim('\0').Trim());

                base._tcpSocket.SendAsync(msg);

                base.AppendMsg($"转发数据：【{msg}】");
            }
            catch (Exception ex)
            {
                base.AppendMsg(ex.Message);
            }
        }
    }
}
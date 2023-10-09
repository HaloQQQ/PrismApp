using Helper.NetCore.Extensions;
using Helper.NetCore.Utils;
using SocketHelper.Tcp;
using System;
using TcpSocket.Helper;
using TcpSocket.ViewModels;
using TcpSocket.Views.BaseViews;
using WpfStyleResources.Helper;

namespace TcpSocket.Views
{
    internal class TcpServerView : BaseTcpSocketView
    {
        private CommunicationViewModel _mediatorContext = null!;

        public void SetMediator(CommunicationViewModel mediatorContext)
        {
            _mediatorContext = mediatorContext;
        }

        public TcpServerView()
        {
            base.ResolveMsg += str =>
            {
                if (base._tcpSocketContext.IsHex)
                {
                    if (str.IsHexString())
                    {
                        return str.GetStringFromHex();
                    }

                    return str.GetHexFromString();
                }

                return str;
            };
        }

        protected override void CloseSocket()
        {
            base.CloseSocket();

            (base._tcpSocketContext as TcpServerViewModel).ConnList.Clear();
        }

        protected override void InitTcpSocket(ushort port)
        {
            if (string.IsNullOrEmpty(_tcpSocketContext.Name))
            {
                _tcpSocketContext.Name = "服务器";
            }

            var tcpServerViewModel = base._tcpSocketContext as TcpServerViewModel;

            _tcpSocket = new NewTcpServer(_tcpSocketContext.Encoding, _tcpSocketContext.IP, port,
                _tcpSocketContext.Name, _tcpSocketContext.MaxMessageLength,
                tcpServerViewModel.MaxClientCount);

            var server = (NewTcpServer)_tcpSocket;

            server.SocketCommunicateWithClientCreated += socket =>
            {
                rhTxt.Info(_tcpSocketContext, $"连接{socket.LocalEndPoint}=>{socket.RemoteEndPoint}建立!");

                if (socket.RemoteEndPoint != null)
                {
                    CommonUtils.Invoke(() =>
                        tcpServerViewModel.ConnList.Add(socket.RemoteEndPoint.ToString()!)
                    );
                }
            };

            server.Started += socket =>
            {
                rhTxt.Info(_tcpSocketContext,
                    $"开始监听{socket}..");
            };

            server.ReceivedMessage += (from, to, data) => _mediatorContext.TransmitFrom(this, data);

            //server.DestoryClient += client =>
            //{
            //    rhTxt.Info(_tcpSocketContext, $"客户端{client}已销毁");
            //};

            this._tcpSocket.ExceptionOccurred += (socketName, exception) =>
            {
                CommonUtils.Invoke(() =>
                {
                    if (tcpServerViewModel.ConnList.Contains(socketName))
                    {
                        tcpServerViewModel.ConnList.Remove(socketName);
                    }

                    this._tcpSocketContext.Connecting = false;
                }
                );
            };

            base._tcpSocket.ConnectStatusChanged += status =>
            {
                if (this._tcpSocketContext.IsConnected = status)
                {
                    this._tcpSocketContext.Connecting = false;
                }
            };
        }

        public void TransmitMessage(byte[] data)
        {
            try
            {
                AppUtils.Assert(_tcpSocket != null, "Socket未初始化!");

                var server = _tcpSocket as NewTcpServer;

                if (!server!.IsConnected)
                {
                    return;
                }

                AppUtils.Assert(server.Clients.Count > 0,
                    $"【{_tcpSocketContext.Name}】不存在客户端连接!请连接后重试!");

                var msg = ResolveMsg(_tcpSocket!.GetString(data).Trim('\0').Trim());

                _tcpSocket.SendAsync(msg);

                rhTxt.Info(_tcpSocketContext, $"转发数据：【{msg}】");
            }
            catch (Exception ex)
            {
                rhTxt.Info(_tcpSocketContext, ex.Message);
            }
        }
    }
}
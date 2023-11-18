using IceTea.SocketStandard.Tcp;
using System;
using MyApp.Prisms.Helper;
using MyApp.Prisms.ViewModels;
using MyApp.Prisms.Views.BaseViews;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using IceTea.Wpf.Core.Utils;
using IceTea.SocketStandard.Base;

namespace MyApp.Prisms.Views
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

        private ITcpServer _tcpServer;

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

            _tcpServer = new NewTcpServer(_tcpSocketContext.Encoding, _tcpSocketContext.IP, port,
                _tcpSocketContext.Name, _tcpSocketContext.MaxMessageLength,
                tcpServerViewModel.MaxClientCount);

            this._tcpSocket = _tcpServer;

            _tcpServer.SocketCommunicateWithClientCreated += socket =>
            {
                rhTxt.Info(_tcpSocketContext, $"连接{socket.LocalEndPoint}=>{socket.RemoteEndPoint}建立!");

                if (socket.RemoteEndPoint != null)
                {
                    CommonUtils.BeginInvokeAtOnce(() =>
                        tcpServerViewModel.ConnList.Add(socket.RemoteEndPoint.ToString()!)
                    );
                }
            };

            _tcpServer.Started += socket =>
            {
                rhTxt.Info(_tcpSocketContext,
                    $"开始监听{socket}..");
            };

            _tcpServer.ReceivedMessage += (from, to, data) => _mediatorContext.TransmitFrom(this, data);

            this._tcpSocket.ExceptionOccurred += (socketName, exception) =>
            {
                CommonUtils.BeginInvokeAtOnce(() =>
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
                AppUtils.Assert(_tcpServer != null, "Socket未初始化!");

                if (!_tcpServer.IsConnected)
                {
                    return;
                }

                AppUtils.Assert(_tcpServer.CurrentCount > 0,
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
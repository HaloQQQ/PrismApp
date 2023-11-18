using IceTea.SocketStandard.Tcp;
using MyApp.Prisms.Helper;
using MyApp.Prisms.ViewModels;
using MyApp.Prisms.Views.BaseViews;
using IceTea.Atom.Extensions;
using IceTea.Wpf.Core.Utils;
using IceTea.SocketStandard.Base;

namespace MyApp.Prisms.Views
{
    public class TcpClientView : BaseTcpSocketView
    {
        public TcpClientView()
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

        private ITcpClient _tcpClient;

        protected override void InitTcpSocket(ushort port)
        {
            if (string.IsNullOrEmpty(base._tcpSocketContext.Name))
            {
                base._tcpSocketContext.Name = "客户端";
            }

            this._tcpClient = new NewTcpClient(base._tcpSocketContext.Encoding, base._tcpSocketContext.IP, port,
                base._tcpSocketContext.Name, base._tcpSocketContext.MaxMessageLength);

            base._tcpSocket = this._tcpClient;

            var tcpClientViewModel = base._tcpSocketContext as TcpClientViewModel;
            this._tcpClient.TryReConnect = tcpClientViewModel.CanReConnect;
            this._tcpClient.ReConnectPeriodMilliseconds = 2000;

            // 防止重复订阅
            tcpClientViewModel.CanReConnectChanged -= this.ReConnectChanged_Handler;

            tcpClientViewModel.CanReConnectChanged += this.ReConnectChanged_Handler;

            base._tcpSocket.Started += socket =>
            {
                this.rhTxt.Info(this._tcpSocketContext, $"{socket}连接成功");

                CommonUtils.BeginInvoke(() =>
                {
                    base._tcpSocketContext.ConnList.Add($"{base._tcpSocketContext.IP}:{port}");
                });
            };

            base._tcpSocket.StartFailed += socket =>
            {
                base._tcpSocketContext.Connecting = tcpClientViewModel.CanReConnect;

                this.rhTxt.Info(this._tcpSocketContext, $"连接{socket}失败..");
            };

            base._tcpSocket.ExceptionOccurred += (socketName, exception) =>
            {
                CommonUtils.BeginInvoke(() =>
                {
                    if (base._tcpSocketContext.ConnList.Contains(socketName))
                    {
                        base._tcpSocketContext.ConnList.Remove(socketName);
                    }

                    if (!tcpClientViewModel.CanReConnect)
                    {
                        this._tcpSocketContext.Connecting = false;
                    }
                });
            };

            base._tcpSocket.ConnectStatusChanged += status =>
            {
                if ((this._tcpSocketContext.IsConnected = status) && !tcpClientViewModel.CanReConnect)
                {
                    this._tcpSocketContext.Connecting = false;
                }
            };
        }

        private void ReConnectChanged_Handler(bool currentStatus)
        {
            this._tcpClient.TryReConnect = currentStatus;

            if (!currentStatus)
            {
                base._tcpSocketContext.Connecting = false;
            }
        }
    }
}
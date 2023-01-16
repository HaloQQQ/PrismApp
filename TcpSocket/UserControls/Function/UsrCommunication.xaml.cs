using System;
using System.Windows.Controls;
using System.Windows.Data;
using Helper.Extensions;
using TcpSocket.Converters;
using TcpSocket.Helper;
using TcpSocket.UserControls.Function.Communication;

namespace TcpSocket.UserControls.Function
{
    public partial class UsrCommunication : UserControl
    {
        public UsrCommunication()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // Tcp Server模式操作界面
            var machine = new UsrCtrlTcpServer(Statics.DataContext.MachineServerContext);
            this.grdServer.Children.Add(machine);
            Statics.DataContext.AddServer(machine);
            Grid.SetColumn(machine, 0);
            this.InitHexMsg(machine);

            var application = new UsrCtrlTcpServer(Statics.DataContext.ApplicationServerContext);
            this.grdServer.Children.Add(application);
            Statics.DataContext.AddServer(application);
            Grid.SetColumn(application, 1);
            this.InitHexMsg(application);

            // Tcp Client模式操作界面
            var client = new UsrCtrlTcpClient(Statics.DataContext.ApplicationClientContext);
            this.grdClient.Children.Add(client);
            this.InitHexMsg(client);

            // UDP操作界面
            var udp = new UsrCtrlUDP(Statics.DataContext.UdpContext);
            this.grdUDP.Children.Add(udp);
            this.InitHexMsg(udp);
        }

        private void InitHexMsg(UsrCtrlTcpSocket usrCtrlTcpSocket)
        {
            usrCtrlTcpSocket.ResolveMsg += str =>
            {
                if (Statics.DataContext.IsHex)
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

        private void InitHexMsg(UsrCtrlUDP usrCtrlUdp)
        {
            usrCtrlUdp.ResolveMsg += str =>
            {
                if (Statics.DataContext.IsHex)
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
    }
}